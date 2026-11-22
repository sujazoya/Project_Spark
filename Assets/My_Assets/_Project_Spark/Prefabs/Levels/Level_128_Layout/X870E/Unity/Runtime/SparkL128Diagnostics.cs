using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ProjectSpark.Gameplay.AorusWorkshop
{
    public sealed partial class SparkL128Workshop
    {
        private const int Upstream = (1 << 1) | (1 << 5);
        private static void Switch(SparkSwitch contact, bool closed) =>
            contact.SetState(closed ? SparkSwitchState.Closed : SparkSwitchState.Open);

        public float Volts(Channel channel)
        {
            var result = manager.ElectricalSolver.LatestSolveResult;
            return result != null && result.TryGetVoltage(channel.pin, ground, out float value) ? value : float.NaN;
        }

        public float Amps(Channel channel) => Mathf.Abs(channel.load.Current);

        private bool Live(Channel channel, float voltage, float minimumCurrent)
        {
            float value = Volts(channel);
            return float.IsFinite(value) && Mathf.Abs(value - voltage) <= voltage * .055f &&
                Amps(channel) >= minimumCurrent && Amps(channel) < minimumCurrent * 4f;
        }

        private bool RailOK(int index) =>
            (index == 6 || !supplies[index].IsCurrentLimited) && Live(rails[index], Nominal[index], RailMinimumAmps[index]);

        public void ChooseRail(int index)
        {
            if (index < 0 || index >= rails.Length) return;
            pickedRail = index;
            RefreshPanel();
        }

        public bool ApplyStandby()
        {
            if (Stage is < 1 or > 5 || standby || IsBusy || HasForeignWire() ||
                driveArmor.Any(cover => !cover.activeSelf) || !vrmArmor.activeSelf)
                return Reject("Finish isolated work and refit the heat spreaders before applying standby.");
            supplies[0].SetOutput(true, default);
            standby = supplies[0].IsOutputActive;
            checkedRails = 0;
            RefreshElectrical();
            return standby ? Note("Standby energized. Measure its loaded test point before enabling both EPS inputs.") :
                Reject("Standby rejected the command.");
        }

        public bool PowerOn()
        {
            if (!standby || running || !RailOK(0) || (checkedRails & 1) == 0 ||
                faultLatched || IsBusy || Stage > 5 || HasForeignWire())
                return Reject("Verify loaded standby and clear the protection latch before requesting EPS power.");
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                supplies[1].SetOutput(true, default);
                supplies[4].SetOutput(true, default);
                supplies[5].SetOutput(true, default);
                Switch(fan.contact, true);
            }
            running = supplies[1].IsOutputActive && supplies[4].IsOutputActive && supplies[5].IsOutputActive;
            checkedRails &= 1;
            checkedPorts = 0;
            postPassed = false;
            RefreshElectrical();
            return running ? Note("Measure EPS A and EPS B. Then release power-good and qualify the regulated rails.") :
                Reject("Both EPS inputs are required.");
        }

        public bool EnableRegulators()
        {
            if (!running || regulators || !standby || IsBusy || faultLatched ||
                (checkedRails & Upstream) != Upstream || !RailOK(1) || !RailOK(5))
                return Reject("Measure both loaded EPS inputs before enabling power-good.");
            using (manager.ElectricalSolver.BeginConfigurationBatch()) Switch(rails[6].contact, true);
            RefreshElectrical();
            if (!RailOK(6)) return Reject("The native power-good path failed its sense window.");
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                supplies[2].SetOutput(true, default);
                supplies[3].SetOutput(true, default);
                Switch(vrm, activeFault != 0 || candidateRepair);
                Switch(dimms[0].contact, activeFault != 1 || candidateRepair);
                Switch(dimms[1].contact, true);
                for (int slot = 0; slot < storageSlots.Length; slot++)
                    Switch(storageSlots[slot].contact, (driveMask & (1 << slot)) != 0);
            }
            regulators = supplies[2].IsOutputActive && supplies[3].IsOutputActive;
            RefreshElectrical();
            return regulators ? Note("Power-good is present. Probe core, DDR5, I/O and power-good under load.") :
                Reject("The regulated outputs were rejected.");
        }

        public bool Isolate()
        {
            AbortTrial();
            ShutdownMain(false);
            return Note("Motherboard rails are isolated. The recorded diagnostic can guide safe service.");
        }

        private void ShutdownMain(bool keepStandby)
        {
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                for (int i = keepStandby ? 1 : 0; i < supplies.Length; i++) supplies[i].SetOutput(false, default);
                Switch(rails[6].contact, false);
                Switch(boost.contact, false);
                Switch(upperGpuLanes.contact, false);
                foreach (var port in ports) Switch(port.contact, false);
                Switch(fan.contact, false);
            }
            running = regulators = false;
            standby = keepStandby && supplies[0].IsOutputActive;
            checkedRails = checkedPorts = 0;
            postPassed = freshReboot = false;
            SetPost(PostSignal.Off);
            RefreshElectrical();
        }

        public bool ProbeRail()
        {
            if (!standby || IsBusy || pickedRail > 0 && !running)
                return Reject("Enable the selected test point before collecting a loaded reading.");
            RefreshElectrical();
            float value = Volts(rails[pickedRail]);
            detail = float.IsFinite(value)
                ? $"{RailNames[pickedRail]}  {value:F3} V  /  {Amps(rails[pickedRail]) * 1000f:F1} mA"
                : $"{RailNames[pickedRail]}  no connected measurement";
            if (!RailOK(pickedRail))
            {
                checkedRails &= ~(1 << pickedRail);
                return Reject("This point failed its loaded training window. Check upstream EPS and power-good.");
            }
            checkedRails |= 1 << pickedRail;
            if (Stage == 1 && checkedRails == AllRails)
            {
                Award();
                return Note("All seven test points qualified. Run POST to train A2/B2 and verify video and boot storage.");
            }
            return Note("This point meets the loaded training window.");
        }

        public bool StartPost()
        {
            if (Stage is < 2 or > 5 || !running || !regulators || IsBusy)
                return Reject("Enable both EPS inputs and power-good before running POST.");
            if (checkedRails != AllRails && activeFault < 0)
                return Reject("Collect all seven fresh rail readings before POST.");
            postPassed = false;
            checkedPorts = 0;
            trial = StartCoroutine(PostSequence());
            return Note("Starting CPU → DRAM → VGA → BOOT sequence.");
        }

        private IEnumerator PostSequence()
        {
            SetPost(PostSignal.CPU);
            yield return new WaitForSecondsRealtime(.27f);
            if (!RailOK(1) || !RailOK(5) || !RailOK(6) || !RailOK(2) || (installed & 3) != 3)
            {
                EndTest("POST halted at CPU. Compare both EPS feeds and power-good against VCORE.");
                yield break;
            }

            SetPost(PostSignal.DRAM);
            yield return new WaitForSecondsRealtime(.27f);
            if (!RailOK(3) || !TestMemory(0) || !TestMemory(1))
            {
                EndTest("POST halted at DRAM. Compare trained patterns on A2 and B2.");
                yield break;
            }

            SetPost(PostSignal.VGA);
            yield return new WaitForSecondsRealtime(.27f);
            if (!TestGpu(out int lanes) || lanes < 8)
            {
                EndTest("POST halted at VGA. Check the GPU fixture and the primary lane bank.");
                yield break;
            }

            SetPost(PostSignal.BOOT);
            yield return new WaitForSecondsRealtime(.27f);
            if (ReadStorage(0) != 12 || (driveMask & 2) != 0 && ReadStorage(1) != 12)
            {
                EndTest("POST halted at BOOT. Compare storage stability on the boot socket and expansion sockets.");
                yield break;
            }

            postPassed = true;
            SetPost(PostSignal.Ready);
            if (Stage == 2) Award();
            EndTest("POST complete. Both DDR5 channels trained, graphics linked and boot storage verified.");
        }

        private bool TestMemory(int index)
        {
            var memory = new uint[32];
            for (int order = 0; order < 2; order++)
            {
                bool intermittent = index == 0 && activeFault == 1 && !candidateRepair;
                if (intermittent)
                {
                    Switch(dimms[index].contact, order == 0);
                    RefreshElectrical();
                }
                if (!Live(dimms[index], 1.10f, .013f)) return false;
                uint seed = order == 0 ? 0xAAAAAAAAu : 0x55555555u;
                for (int cell = 0; cell < memory.Length; cell++)
                    memory[cell] = seed ^ unchecked(0x9E3779B9u * (uint)(cell + 1));
                for (int cell = memory.Length - 1; cell >= 0; cell--)
                    if ((memory[cell] ^ seed) != unchecked(0x9E3779B9u * (uint)(cell + 1))) return false;
            }
            return true;
        }

        private int ReadStorage(int slot)
        {
            int blocks = 0;
            bool intermittent = slot == 1 && activeFault == 2 && !candidateRepair;
            var writing = new byte[32];
            var media = new byte[12, writing.Length];
            var expected = new uint[12];
            var written = new bool[12];
            for (int number = 0; number < written.Length; number++)
            {
                if (intermittent)
                {
                    Switch(storageSlots[slot].contact, number % 4 < 2);
                    RefreshElectrical();
                }
                if (!Live(storageSlots[slot], 3.3f, .016f)) continue;
                for (int offset = 0; offset < writing.Length; offset++)
                {
                    byte data = (byte)((slot * 19 + number * 17 + offset * 53) & 255);
                    writing[offset] = data;
                    media[number, offset] = data;
                }
                expected[number] = Crc(writing);
                written[number] = true;
            }
            for (int number = written.Length - 1; number >= 0; number--)
            {
                if (!written[number]) continue;
                for (int offset = 0; offset < writing.Length; offset++) writing[offset] = media[number, offset];
                if (Crc(writing) == expected[number]) blocks++;
            }
            return blocks;
        }

        private static uint Crc(byte[] data)
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
            if (selection && selection.SelectedObject != interfaceSites[index].selectable)
                selection.Select(interfaceSites[index].selectable);
            RefreshPanel();
        }

        public void ChooseMode(int index)
        {
            if (index < 0 || index >= PortModes.Length) return;
            pickedMode = index;
            RefreshPanel();
        }

        public bool LiftDriveArmor()
        {
            int socket = Stage == 0 && serviceIndex == 4 ? 0 : Stage == 3 && pickedPort >= 2 ? pickedPort - 1 : -1;
            if (socket < 0 || standby || running || IsBusy)
                return Reject("Select the correct M.2 socket, then isolate all board power before lifting its heat spreader.");
            driveArmor[socket].SetActive(!driveArmor[socket].activeSelf);
            return Note(driveArmor[socket].activeSelf ? "Heat spreader refitted." :
                "Heat spreader lifted. Fit or remove the drive on this isolated socket.");
        }

        public bool SwapLaneDrive()
        {
            int slot = pickedPort - 1;
            if (Stage != 3 || slot < 1 || slot >= driveModels.Length || standby || running || IsBusy || driveArmor[slot].activeSelf)
                return Reject("Select M.2 B, C or D and lift its heat spreader with all board outputs disabled.");
            driveMask ^= 1 << slot;
            bool fitted = (driveMask & (1 << slot)) != 0;
            driveModels[slot].SetActive(fitted);
            driveArmor[slot].SetActive(true);
            checkedPorts = checkedRails = 0;
            postPassed = false;
            gpuWidth = 0;
            return Note($"{PortNames[pickedPort]} {(fitted ? "fitted" : "removed")}. Heat spreader secured. Requalify this topology.");
        }

        public bool RunInterface()
        {
            if (Stage is < 3 or > 5 || !running || !postPassed || IsBusy)
                return Reject("A fresh POST is required after each drive configuration.");
            int samples;
            if (pickedPort == 0)
            {
                if (!TestGpu(out gpuWidth)) return Reject("The native primary GPU lane bank failed.");
                detail = $"GPU trained ×{gpuWidth} · {((driveMask & 6) != 0 ? "CPU M.2 B/C sharing active" : "unshared GPU lane bank")}";
                if (gpuWidth != (pickedMode == 0 ? 16 : 8)) return Reject("Measured width differs from your prediction. Consider which sockets share CPU lanes.");
                samples = 12;
            }
            else
            {
                if ((driveMask & (1 << (pickedPort - 1))) == 0) return Reject("This socket is empty. Isolate power and install a training drive first.");
                samples = TestPortLoad(pickedPort) ? ReadStorage(pickedPort - 1) : 0;
                detail = $"{PortNames[pickedPort]} · {samples}/12 blocks verified";
            }
            if (samples != 12)
            {
                checkedPorts &= ~(1 << pickedPort);
                return Reject("The storage fixture or read/write check failed.");
            }

            checkedPorts |= 1 << pickedPort;
            RecordRestart();
            if (Stage == 5 && guardObserved && checkedPorts == AllLinks)
                return Note(coldBoots == 1 ? "Cold boot 1/2 verified. Isolate the whole bench before the second startup." :
                    "Cold boot 2/2 verified. Run the final thermal soak and cooldown.");
            if (Stage == 3 && (checkedPorts & (1 | driveMask << 1)) == (1 | driveMask << 1))
            {
                laneScenarios |= driveMask switch { 1 => 1, 9 => 2, 15 => 4, _ => 0 };
                if (laneScenarios == 7 && checkedPorts == AllLinks && driveMask == 15)
                {
                    Award();
                    if (repaired == 3) Award();
                    return Note(repaired == 3 ? "Bus matrix requalified. Begin the protected thermal trial." :
                        "Lane sharing and all four M.2 links verified. Open the first hardware fault ticket.");
                }
                return Note((laneScenarios & 1) == 0 ? "Prove GPU ×16 with just the M.2 A boot drive." :
                    (laneScenarios & 2) == 0 ? "Isolate and fit only M.2 D alongside A to prove chipset storage leaves GPU ×16." :
                    "Isolate and fit B and C. Verify GPU ×8 and all four drives on the fully populated topology.");
            }
            return Note("Fixture and response verified. Continue with the other links in this topology.");
        }

        private bool TestGpu(out int width)
        {
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                Switch(ports[0].contact, true);
                Switch(upperGpuLanes.contact, (driveMask & 6) == 0);
            }
            RefreshElectrical();
            bool lower = !supplies[4].IsCurrentLimited && Live(ports[0], 3.3f, .006f);
            bool upper = Live(upperGpuLanes, 3.3f, .006f);
            width = lower ? upper ? 16 : 8 : 0;
            bool success = lower;
            for (int sample = 0; success && sample < 12; sample++)
            {
                uint pattern = unchecked(0x9E3779B9u * (uint)(sample + 1));
                var lanes = new uint[width];
                for (int bit = 0; bit < 32; bit++) lanes[bit % width] |= (pattern >> bit & 1u) << (bit / width);
                uint received = 0;
                for (int bit = 31; bit >= 0; bit--) received = (received << 1) | (lanes[bit % width] >> (bit / width) & 1u);
                success = received == pattern;
            }
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                Switch(ports[0].contact, false);
                Switch(upperGpuLanes.contact, false);
            }
            return success;
        }

        private bool TestPortLoad(int index)
        {
            Channel channel = ports[index];
            Switch(channel.contact, true);
            RefreshElectrical();
            bool pass = !supplies[4].IsCurrentLimited && Live(channel, 3.30f, .006f);
            Switch(channel.contact, false);
            return pass;
        }

        public bool InjectFault()
        {
            if (Stage != 4 || activeFault >= 0 || repaired >= 3 || !running || !postPassed || checkedPorts != AllLinks || IsBusy)
                return Reject("Finish the full lane baseline before injecting a hardware fault.");
            activeFault = repaired;
            identified = selectedFault = -1;
            faultProbed = candidateRepair = inspectedFault = false;
            if (activeFault == 0) Switch(vrm, false);
            checkedPorts = 0;
            checkedRails &= 1;
            postPassed = false;
            detail = "";
            RefreshElectrical();
            return Note($"Ticket {repaired + 1}/3 active. Run POST and choose an independent measurement for the stalled phase.");
        }

        public bool Diagnose(int index)
        {
            if (Stage != 4 || activeFault < 0 || candidateRepair || !running || IsBusy)
                return Reject("Run a powered fault ticket before collecting evidence.");
            RefreshElectrical();
            switch (index)
            {
                case 0:
                    faultProbed = post == PostSignal.CPU && RailOK(1) && RailOK(5) && RailOK(6) && !RailOK(2);
                    detail = $"EPS A/B {Volts(rails[1]):F2}/{Volts(rails[5]):F2} V · PG {Volts(rails[6]):F2} V · core {Volts(rails[2]):F2} V";
                    break;
                case 1:
                    bool primary = TestMemory(0), secondary = TestMemory(1);
                    faultProbed = post == PostSignal.DRAM && RailOK(3) && !primary && secondary;
                    detail = $"A2 {(primary ? "trained" : "failed")} · B2 {(secondary ? "trained" : "failed")}";
                    break;
                case 2:
                    int boot = ReadStorage(0), expansion = ReadStorage(1);
                    faultProbed = post == PostSignal.BOOT && boot == 12 && expansion is > 0 and < 12;
                    detail = $"M.2 A {boot}/12 · M.2 B {expansion}/12 readable blocks";
                    break;
                default: return false;
            }
            identified = faultProbed ? index : -1;
            return faultProbed ? Note("Signature captured. Isolate, select the physical site and inspect it before fitting the spare.") :
                Reject("This probe did not isolate the cause. Compare the independent result against the stalled POST phase.");
        }

        public void SetRepairSite(int index)
        {
            if (index < 0 || index >= faultSites.Length) return;
            if (selectedFault != index && inspectedFault) CloseAccess();
            selectedFault = index;
            if (selection && selection.SelectedObject != faultSites[index].selectable)
                selection.Select(faultSites[index].selectable);
            RefreshPanel();
        }

        public void ChooseSpare(int index)
        {
            if (index < 0 || index >= SpareNames.Length) return;
            pickedSpare = index;
            RefreshPanel();
        }

        public bool InspectRepair()
        {
            if (Stage != 4 || selectedFault < 0 || activeFault < 0 || !faultProbed || standby || running ||
                candidateRepair || IsBusy)
                return Reject("Select an isolated repair site after collecting the fault evidence.");
            if (selectedFault == 0) vrmArmor.SetActive(false);
            if (selectedFault == 2) driveArmor[1].SetActive(false);
            inspectedFault = true;
            zoom = true;
            return Note("Physical access opened. Match the selected footprint and spare to the diagnosed problem.");
        }

        private void CloseAccess()
        {
            vrmArmor.SetActive(true);
            driveArmor[1].SetActive(true);
            inspectedFault = false;
        }

        public bool Repair()
        {
            if (Stage != 4 || activeFault < 0 || !faultProbed || identified != activeFault || !inspectedFault ||
                candidateRepair || standby || running || supplies.Any(source => source.IsOutputActive) || IsBusy)
                return Reject("Diagnose and inspect the fault with every motherboard supply isolated before replacement.");
            if (selectedFault != activeFault || pickedSpare != activeFault)
                return Reject("The selected site or spare does not match the measured fault.");
            candidateRepair = true;
            Switch(vrm, true);
            Switch(dimms[0].contact, true);
            Switch(storageSlots[1].contact, true);
            CloseAccess();
            return Note("Replacement fitted and armor refitted. Verify seven fresh readings, POST and all five lanes/drive checks.");
        }

        public bool VerifyRepair()
        {
            if (Stage != 4 || !candidateRepair || !running || IsBusy || !postPassed || checkedRails != AllRails ||
                checkedPorts != AllLinks || !Enumerable.Range(0, rails.Length).All(RailOK))
                return Reject("Fresh rails, POST and five link results are required to certify the repair.");
            repaired++;
            activeFault = identified = selectedFault = -1;
            candidateRepair = faultProbed = false;
            if (repaired == 3) Award();
            SaveCheckpoint();
            return Note(repaired == 3 ? "All three repairs verified. Begin the thermal protection challenge." :
                $"Repair verified ({repaired}/3). Open the next hardware ticket.");
        }

        private bool HasForeignWire()
        {
            connections.Clear();
            manager.CircuitSystem.CopyConnections(connections);
            return connections.Exists(edge => edge.Kind == SparkConnectionKind.Wire &&
                ((edge.A && edge.A.transform.IsChildOf(transform)) || (edge.B && edge.B.transform.IsChildOf(transform))));
        }

        private void RefreshElectrical() => manager.ElectricalSolver.SolveNow();

        private void EndTest(string text)
        {
            trial = null;
            Note(text);
        }

        private void AbortTrial()
        {
            if (trial == null) return;
            StopCoroutine(trial);
            trial = null;
        }
    }
}
