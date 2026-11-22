using System;
using System.Collections;
using System.Linq;
using ProjectSpark.Measurement;
using UnityEngine;

namespace ProjectSpark.Gameplay.RecoveryLab
{
    public sealed partial class SparkL130Workshop
    {
        private static readonly int[] RailSupplies = { 0, 1, 1, 2, 3, 4 };
        private static readonly byte[] LabRom = CreateLabRom();
        private static readonly uint ApprovedChecksum = Checksum(LabRom);

        private static void Switch(SparkSwitch contact, bool closed) =>
            contact.SetState(closed ? SparkSwitchState.Closed : SparkSwitchState.Open);

        public float Volts(Channel channel)
        {
            var result = manager.ElectricalSolver.LatestSolveResult;
            return result != null && result.TryGetVoltage(channel.pin, ground, out float value) ? value : float.NaN;
        }

        public float Amps(Channel channel) => Mathf.Abs(channel.load.Current);

        private bool Live(Channel channel, float expected, float minimumCurrent)
        {
            float value = Volts(channel);
            return float.IsFinite(value) && Mathf.Abs(value - expected) <= expected * .055f &&
                Amps(channel) >= minimumCurrent && Amps(channel) < minimumCurrent * 4f;
        }

        private bool RailOK(int index) => !supplies[RailSupplies[index]].IsCurrentLimited &&
            Live(rails[index], Nominal[index], RailMinimumAmps[index]);

        public void ChooseRail(int index)
        {
            if (index < 0 || index >= rails.Length) return;
            pickedRail = index;
            RefreshPanel();
        }

        public bool ApplyStandby()
        {
            if (Stage is < 1 or > 5 || standby || IsBusy || HasForeignWire())
                return Reject("The accepted intake and a clear bench are required before applying standby.");
            supplies[0].SetOutput(true, default);
            standby = supplies[0].IsOutputActive;
            checkedRails = sampledRails = 0;
            RefreshElectrical();
            return standby ? Note("Standby energized. Measure 5V under load before requesting main power.") :
                Reject("Standby supply rejected the command.");
        }

        public bool PowerOn()
        {
            if (!standby || running || (checkedRails & 1) == 0 || !RailOK(0) || faultLatched ||
                IsBusy || Stage > 5 || HasForeignWire())
                return Reject("Measure loaded standby and clear any protection latch before requesting main power.");
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                supplies[1].SetOutput(true, default);
                supplies[4].SetOutput(true, default);
                Switch(rails[2].contact, activeFault != 0 || candidateRepair);
            }
            RefreshElectrical();
            bool upstreamAccepted = RailOK(1) && RailOK(2);
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                supplies[2].SetOutput(upstreamAccepted, default);
                supplies[3].SetOutput(upstreamAccepted, default);
                Switch(fanMotor.contact, upstreamAccepted);
                Switch(fanTach.contact, upstreamAccepted && (activeFault != 2 || candidateRepair));
                Switch(romPower.contact, upstreamAccepted);
                Switch(romData.contact, upstreamAccepted && (activeFault != 1 || candidateRepair));
                Switch(ports[4].contact, upstreamAccepted);
            }
            running = supplies[1].IsOutputActive && supplies[4].IsOutputActive;
            checkedRails &= 1;
            sampledRails &= 1;
            checkedPorts = 0;
            postPassed = thermalPassed = false;
            boot++;
            RefreshElectrical();
            SetPost(PostSignal.CPU);
            return Note(upstreamAccepted ? "Main request accepted. Probe all six points then run POST." :
                "Main requested. Protected core remains off until the EPS socket has a valid feed.");
        }

        public bool Isolate()
        {
            AbortTrial();
            ShutdownMain(false);
            return Note("All motherboard supplies are off. Captured case evidence remains available for isolated repair.");
        }

        private void ShutdownMain(bool keepStandby)
        {
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                for (int i = keepStandby ? 1 : 0; i < supplies.Length; i++) supplies[i].SetOutput(false, default);
                Switch(boost.contact, false);
                Switch(fanMotor.contact, false);
                Switch(fanTach.contact, false);
                Switch(romPower.contact, false);
                Switch(romData.contact, false);
                foreach (var port in ports) Switch(port.contact, false);
            }
            running = false;
            standby = keepStandby && supplies[0].IsOutputActive;
            checkedRails = sampledRails = checkedPorts = 0;
            postPassed = thermalPassed = false;
            SetPost(PostSignal.Off);
            RefreshElectrical();
        }

        public bool ProbeRail()
        {
            if (!standby || pickedRail > 0 && !running || IsBusy)
                return Reject("Enable the selected test point before sampling voltage.");
            SparkTerminal pin = rails[pickedRail].pin;
            measurement.TryMeasureTerminal(pin, SparkMeasurementType.Voltage, out var probe);
            measurement.TryMeasureTerminal(ground, SparkMeasurementType.Voltage, out var reference);
            float differential = probe.Value - reference.Value;
            float solved = Volts(rails[pickedRail]);
            if (!probe.Valid || !reference.Valid || !float.IsFinite(solved) || Mathf.Abs(solved - differential) > .06f)
            {
                detail = $"{RailNames[pickedRail]} · probe {(probe.Valid ? probe.Value.ToString("F3") : "open")} · return " +
                    $"{(reference.Valid ? reference.Value.ToString("F3") : "open")} · solver {solved:F3} V";
                return Reject("The existing measurement system did not return a valid referenced reading.");
            }
            sampledRails |= 1 << pickedRail;
            detail = $"{RailNames[pickedRail]}  {differential:F3} V  /  {Amps(rails[pickedRail]) * 1000f:F1} mA";
            if (!RailOK(pickedRail))
            {
                checkedRails &= ~(1 << pickedRail);
                return Reject("Reading captured outside its loaded window. Compare upstream and downstream evidence.");
            }
            checkedRails |= 1 << pickedRail;
            return Note("Loaded reading accepted and attached to this power-on session.");
        }

        public bool StartPost()
        {
            if (Stage is < 1 or > 5 || !running || IsBusy ||
                (checkedRails != AllRails && !(Stage == 1 && activeFault == 0 && !candidateRepair)))
                return Reject("Collect the loaded power evidence before attempting POST.");
            checkedPorts = 0;
            postPassed = thermalPassed = false;
            trial = StartCoroutine(PostSequence());
            return Note("Running CPU, lab ROM, DDR5, video and storage initialization.");
        }

        private IEnumerator PostSequence()
        {
            SetPost(PostSignal.CPU);
            yield return new WaitForSecondsRealtime(.26f);
            if (!RailOK(2) || !RailOK(3) || (installed & 3) != 3)
            {
                EndTest("CPU initialization halted. Compare standby and the two sides of the EPS connection.");
                yield break;
            }

            SetPost(PostSignal.Firmware);
            yield return new WaitForSecondsRealtime(.26f);
            if (!ReadLabRom(out _, out _))
            {
                EndTest("Initialization halted at the lab ROM read, before memory training. Compare both reads against the approved image.");
                yield break;
            }

            SetPost(PostSignal.DRAM);
            yield return new WaitForSecondsRealtime(.26f);
            if (!CheckDevice(0) || !CheckDevice(1))
            {
                EndTest("POST halted at DRAM. Verify the two independently loaded channels.");
                yield break;
            }

            SetPost(PostSignal.VGA);
            yield return new WaitForSecondsRealtime(.26f);
            if (!CheckDevice(2))
            {
                EndTest("POST halted at VGA. The primary graphics link failed.");
                yield break;
            }

            SetPost(PostSignal.BOOT);
            yield return new WaitForSecondsRealtime(.26f);
            if (!CheckDevice(4))
            {
                EndTest("POST halted at BOOT. Storage enumeration or integrity failed.");
                yield break;
            }

            postPassed = true;
            SetPost(PostSignal.Ready);
            EndTest("POST ready. Complete the independent device checks or sign the active repair.");
        }

        private static byte[] CreateLabRom()
        {
            var image = new byte[64];
            uint state = 0x6101307Du;
            for (int i = 0; i < image.Length; i++)
            {
                state = unchecked(state * 1664525u + 1013904223u);
                image[i] = (byte)(state >> 24);
            }
            return image;
        }

        private bool ReadLabRom(out uint first, out uint second)
        {
            first = second = 0;
            if (!Live(romPower, 3.3f, .012f)) return false;
            bool dataReady = Live(romData, 3.3f, .008f);
            var readback = (byte[])LabRom.Clone();
            if (!dataReady) readback[17] ^= 0x40;
            first = Checksum(readback);
            if (!dataReady)
            {
                readback[17] ^= 0x40;
                readback[39] ^= 0x08;
            }
            second = Checksum(readback);
            return dataReady && first == ApprovedChecksum && second == ApprovedChecksum;
        }

        private static uint Checksum(byte[] data)
        {
            uint crc = uint.MaxValue;
            foreach (byte value in data)
            {
                crc ^= value;
                for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xEDB88320u : 0);
            }
            return ~crc;
        }

        public void ChoosePort(int index)
        {
            if (index < 0 || index >= ports.Length) return;
            pickedPort = index;
            RefreshPanel();
        }

        public bool ProbeDevice()
        {
            if (Stage is < 3 or > 5 || !running || !postPassed || IsBusy || boot < minimumBoot)
                return Reject("This device proof requires a fresh measured POST after a full isolated restart.");
            if (!CheckDevice(pickedPort))
            {
                checkedPorts &= ~(1 << pickedPort);
                return Reject("Device failed its independent native load or data check.");
            }
            checkedPorts |= 1 << pickedPort;
            detail = pickedPort switch
            {
                0 or 1 => $"{PortNames[pickedPort]} · 64 / 64 pattern reads",
                4 => "NVMe board socket · 12 / 12 block CRCs verified",
                _ => $"{PortNames[pickedPort]} · 12 / 12 lane patterns"
            };
            return Note("Device evidence credited to the current boot.");
        }

        private bool CheckDevice(int index)
        {
            var channel = ports[index];
            if (index != 4) Switch(channel.contact, true);
            RefreshElectrical();
            bool loaded = Live(channel, index < 2 ? 1.10f : 3.3f, index < 2 ? .013f : .008f);
            if (index != 4) Switch(channel.contact, false);
            if (!loaded) return false;
            return index switch { < 2 => CheckMemory(), 4 => CheckStorage(), _ => CheckLanes(index == 2 ? 16 : 1) };
        }

        private static bool CheckMemory()
        {
            var cells = new uint[32];
            for (int seedIndex = 0; seedIndex < 2; seedIndex++)
            {
                uint seed = seedIndex == 0 ? 0xAAAAAAAAu : 0x55555555u;
                for (int cell = 0; cell < cells.Length; cell++)
                    cells[cell] = seed ^ unchecked(0x9E3779B9u * (uint)(cell + 1));
                for (int cell = cells.Length - 1; cell >= 0; cell--)
                    if ((cells[cell] ^ seed) != unchecked(0x9E3779B9u * (uint)(cell + 1))) return false;
            }
            return true;
        }

        private static bool CheckLanes(int width)
        {
            var lanes = new uint[width];
            for (int sample = 0; sample < 12; sample++)
            {
                uint pattern = unchecked(0x9E3779B9u * (uint)(sample + 1));
                Array.Clear(lanes, 0, lanes.Length);
                for (int bit = 0; bit < 32; bit++) lanes[bit % width] |= (pattern >> bit & 1u) << (bit / width);
                uint received = 0;
                for (int bit = 31; bit >= 0; bit--) received = (received << 1) | (lanes[bit % width] >> (bit / width) & 1u);
                if (received != pattern) return false;
            }
            return true;
        }

        private static bool CheckStorage()
        {
            var blocks = new byte[12][];
            var expected = new uint[blocks.Length];
            for (int block = 0; block < blocks.Length; block++)
            {
                var data = new byte[32];
                for (int offset = 0; offset < data.Length; offset++) data[offset] = (byte)(block * 17 + offset * 5);
                expected[block] = Checksum(data);
                blocks[block] = data;
            }
            for (int block = blocks.Length - 1; block >= 0; block--)
                if (blocks[block][0] != block * 17 || Checksum(blocks[block]) != expected[block]) return false;
            return true;
        }

        public bool SignDevices()
        {
            if (Stage != 3 || !running || !postPassed || checkedRails != AllRails || checkedPorts != AllDevices || boot < minimumBoot)
                return Reject("Verify both memory channels, both PCIe links and NVMe after the required restart.");
            if (!Award()) return false;
            Record("O4", "DDR5 / PCIe / NVMe", "Five independent current-boot device proofs", true);
            return Note("Device qualification accepted. Open the cooling ticket from this clean baseline.");
        }

        public bool OpenCase()
        {
            if (Stage is not (2 or 4) || activeFault >= 0 || IsBusy || !running || checkedRails != AllRails ||
                Stage == 4 && (!postPassed || checkedPorts != AllDevices))
                return Reject("Collect the clean baseline before opening the next case.");
            activeFault = Stage == 2 ? 1 : 2;
            revision++;
            beforeRepair = null;
            inspected = candidateRepair = thermalPassed = false;
            if (activeFault == 1)
            {
                Switch(romData.contact, false);
                postPassed = false;
                SetPost(PostSignal.CPU);
            }
            else
            {
                Switch(fanTach.contact, false);
                checkedPorts = 0;
                postPassed = false;
            }
            RefreshElectrical();
            return Note(activeFault == 1
                ? "A new no-POST ticket is active. Run initialization and analyze the selected suspect."
                : "Repeat POST and device checks with this fault active. Compare motor motion and tach before stressing the CPU.");
        }

        public bool AnalyzeSelected()
        {
            if (activeFault < 0 || candidateRepair || !running || IsBusy || targetIndex != FaultTargets[activeFault])
                return Reject("Choose the suspect indicated by the current symptom and gather live case evidence.");
            bool failed;
            string criterion;
            switch (activeFault)
            {
                case 0:
                    failed = post == PostSignal.CPU && (sampledRails & 7) == 7 && RailOK(0) && RailOK(1) &&
                        !RailOK(2) && !RailOK(3);
                    criterion = $"Standby {Volts(rails[0]):F2} V · EPS source {Volts(rails[1]):F2} V · socket {Volts(rails[2]):F2} V · core {Volts(rails[3]):F2} V";
                    break;
                case 1:
                    bool valid = ReadLabRom(out uint first, out uint second);
                    failed = post == PostSignal.Firmware && Live(romPower, 3.3f, .012f) && !valid &&
                        first != 0 && second != 0 && first != second;
                    criterion = $"ROM reads {first:X8} / {second:X8} · lab reference {ApprovedChecksum:X8}";
                    break;
                default:
                    failed = postPassed && MotorRpm >= 900 && TachRpm == 0;
                    criterion = $"Motor {MotorRpm} RPM · tach {TachRpm} RPM";
                    break;
            }
            detail = criterion;
            if (!failed) return Reject("Evidence is incomplete. Check the live symptom and its independent measurement.");
            beforeRepair = Record($"F{activeFault + 1}", FaultNames[activeFault], criterion, false);
            return Note(activeFault == 2 ? "Motor power is present but tach is absent. Run the bounded guard challenge." :
                "Before-repair evidence logged. Isolate, inspect the target and choose the matching recovery action.");
        }

        public void ChooseRepair(int method)
        {
            if (method < 0 || method >= RepairMethods.Length) return;
            pickedRepair = method;
            RefreshPanel();
        }

        public bool InspectRepair()
        {
            if (!BeforeIsCurrent() || targetIndex != FaultTargets[activeFault] || standby || running || IsBusy ||
                candidateRepair || activeFault == 2 && !guardObserved)
                return Reject("First record the fault and isolate all power. Fan repair also requires a verified guard trip.");
            inspected = true;
            zoom = true;
            return Note("The selected original footprint is inspected. Choose the recovery action next.");
        }

        public bool Repair()
        {
            if (!BeforeIsCurrent() || !inspected || standby || running || candidateRepair || IsBusy ||
                supplies.Any(source => source.IsOutputActive))
                return Reject("Ground the repair in current fault evidence and isolate the motherboard before fitting anything.");
            if (targetIndex != FaultTargets[activeFault] || pickedRepair != activeFault)
                return Reject("The selected site or action does not match the diagnosed fault.");
            candidateRepair = true;
            revision++;
            if (activeFault == 2)
            {
                fanOriginal.SetActive(false);
                fanReplacement.SetActive(true);
            }
            detail = RepairMethods[activeFault];
            return Note(activeFault == 2 ? "Tach spare fitted. Clear the guard with all supplies isolated before retesting." :
                "Physical remedy recorded. Collect fresh loaded readings in a new power-on session.");
        }

        public bool VerifyRepair()
        {
            if (activeFault < 0 || !candidateRepair || !beforeRepair.HasValue || beforeRepair.Value.Revision + 1 != revision ||
                beforeRepair.Value.Topology != manager.CircuitSystem.TopologyVersion || boot <= beforeRepair.Value.Boot ||
                !running || IsBusy || checkedRails != AllRails || !Enumerable.Range(0, rails.Length).All(RailOK))
                return Reject("The repair needs fresh loaded measurements from a later power-on session.");
            if (activeFault == 1 && (!postPassed || !ReadLabRom(out _, out _)))
                return Reject("Complete clean boot initialization and a matching repeat ROM read.");
            if (activeFault == 2 && (!guardObserved || faultLatched || !thermalPassed || !postPassed ||
                checkedPorts != AllDevices || TachRpm < 900))
                return Reject("Repeat POST, devices, and the full restrained thermal run after tach replacement.");
            int completedCase = activeFault;
            string criterion = completedCase switch
            {
                0 => $"Seated EPS {Volts(rails[2]):F2} V · protected core {Volts(rails[3]):F2} V",
                1 => $"Lab ROM reads matched {ApprovedChecksum:X8} · cold POST ready",
                _ => $"Tach {TachRpm} RPM · guarded test and {ThermalSamples}/{ThermalFrames} healthy samples"
            };
            Record($"F{completedCase + 1}", FaultNames[completedCase], criterion, true);
            repairedMask |= 1 << completedCase;
            activeFault = -1;
            candidateRepair = inspected = false;
            beforeRepair = null;
            if (!Award()) return false;
            if (completedCase != 0) minimumBoot = boot + 1;
            detail = criterion;
            return Note(completedCase switch
            {
                0 => "EPS recovery verified. Open a new ticket from this clean power baseline.",
                1 => "Firmware recovery verified. Fully isolate and restart before independent memory and link qualification.",
                _ => "Feedback restored. Fully isolate for the final audit and collect new power, POST, device and thermal evidence."
            });
        }

        private bool BeforeIsCurrent() => activeFault >= 0 && beforeRepair.HasValue &&
            beforeRepair.Value.Revision == revision && beforeRepair.Value.Topology == manager.CircuitSystem.TopologyVersion;

        private Evidence Record(string caseId, string target, string criterion, bool passed)
        {
            var entry = new Evidence(caseId, target, criterion, revision, manager.CircuitSystem.TopologyVersion, boot, passed);
            if (journal.Count >= 24) journal.RemoveAt(0);
            journal.Add(entry);
            return entry;
        }

        private bool HasForeignWire()
        {
            connections.Clear();
            manager.CircuitSystem.CopyConnections(connections);
            return connections.Exists(edge => edge.Kind == SparkConnectionKind.Wire &&
                ((edge.A && edge.A.transform.IsChildOf(transform)) || (edge.B && edge.B.transform.IsChildOf(transform))));
        }

        private void RefreshElectrical() => manager.ElectricalSolver.SolveNow();

        private void EndTest(string result)
        {
            trial = null;
            Note(result);
        }

        private void AbortTrial()
        {
            if (trial == null) return;
            StopCoroutine(trial);
            trial = null;
        }
    }
}
