using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ProjectSpark.Gameplay.SiliconWorkshop
{
    public sealed partial class SparkL129Workshop
    {
        private static SparkInteractionContext Context => default;
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

        private bool RailOK(int index) => !supplies[index].IsCurrentLimited &&
            Live(rails[index], Nominal[index], RailMinimumAmps[index]);

        public void ChooseRail(int index)
        {
            if (index < 0 || index >= rails.Length) return;
            pickedRail = index;
            RefreshPanel();
        }

        public bool ApplyStandby()
        {
            if (Stage is < 1 or > 5 || IsBusy || HasForeignWire()) return Reject("Finish service and clear foreign wiring before connecting the bench.");
            if (standby) return Reject("Standby is live. Probe the 5V test point under load.");
            supplies[0].SetOutput(true, Context);
            standby = supplies[0].IsOutputActive;
            checkedRails = 0;
            RefreshElectrical();
            return standby ? Note("Standby energized. Probe its loaded 5V test point before requesting main power.") :
                Reject("Standby supply rejected the command.");
        }

        public bool PowerOn()
        {
            if (!standby || running || !RailOK(0) || (checkedRails & 1) == 0 ||
                faultLatched || IsBusy || Stage > 5 || HasForeignWire())
                return Reject("Verify standby and clear the protection latch before requesting main power.");
            StartMain();
            return Note("Main power requested. Check EPS, core, memory and I/O at the loaded test points.");
        }

        private void StartMain()
        {
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                for (int i = 1; i < supplies.Length; i++) supplies[i].SetOutput(true, Context);
                Switch(vrm, activeFault != 0 || candidateRepair);
                Switch(dimms[0].contact, (installed & 4) != 0);
                Switch(dimms[1].contact, (installed & 8) != 0 && (activeFault != 1 || candidateRepair));
                Switch(storage.contact, (installed & 16) != 0);
                Switch(fan.contact, (installed & 2) != 0 && (!faultLatched || fanRestored));
            }
            running = supplies.Skip(1).All(item => item.IsOutputActive);
            checkedRails &= 1;
            checkedPorts = 0;
            postPassed = false;
            SetPost(PostSignal.CPU);
            RefreshElectrical();
        }

        public bool Isolate()
        {
            AbortTrial();
            ShutdownMain(false);
            return Note("All bench outputs are disabled. Repair is permitted only here.");
        }

        private void ShutdownMain(bool keepStandby)
        {
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                for (int i = keepStandby ? 1 : 0; i < supplies.Length; i++) supplies[i].SetOutput(false, Context);
                Switch(boost.contact, false);
                foreach (var port in ports) Switch(port.contact, false);
                Switch(fan.contact, false);
            }
            running = false;
            standby = keepStandby && supplies[0].IsOutputActive;
            checkedRails = checkedPorts = 0;
            postPassed = false;
            freshReboot = false;
            SetPost(PostSignal.Off);
            RefreshElectrical();
        }

        public bool ProbeRail()
        {
            if (!standby || IsBusy || pickedRail > 0 && !running)
                return Reject("Enable the selected rail before measuring it.");
            RefreshElectrical();
            float volts = Volts(rails[pickedRail]);
            detail = float.IsFinite(volts)
                ? $"{RailNames[pickedRail]}  {volts:F3} V  /  {Amps(rails[pickedRail]) * 1000f:F1} mA"
                : $"{RailNames[pickedRail]}  no connected measurement";
            if (!RailOK(pickedRail))
            {
                checkedRails &= ~(1 << pickedRail);
                return Reject("Loaded rail is outside its training window. Compare its upstream input and downstream output.");
            }
            checkedRails |= 1 << pickedRail;
            if (Stage == 1 && checkedRails == 31)
            {
                Award();
                return Note("All five loaded rails qualified. Run POST to check CPU and both DDR5 channels.");
            }
            return Note("Loaded rail meets its tolerance and current window.");
        }

        public bool StartPost()
        {
            if (Stage is < 2 or > 5 || !running || IsBusy)
                return Reject("Bring up main power before running POST.");
            if (checkedRails != 31 && activeFault < 0)
                return Reject("Probe all five loaded rails before a fresh POST.");
            postPassed = false;
            checkedPorts = 0;
            trial = StartCoroutine(PostSequence());
            return Note("Starting CPU → DRAM → VGA → BOOT sequence.");
        }

        private IEnumerator PostSequence()
        {
            SetPost(PostSignal.CPU);
            yield return new WaitForSecondsRealtime(.28f);
            if (!RailOK(1) || !RailOK(2) || (installed & 3) != 3)
            {
                EndTest("POST halted at CPU. Compare EPS and loaded VCORE.");
                yield break;
            }

            SetPost(PostSignal.DRAM);
            yield return new WaitForSecondsRealtime(.28f);
            if (!RailOK(3) || !TestMemory(0) || !TestMemory(1))
            {
                EndTest("POST halted at DRAM. Compare the two trained channels.");
                yield break;
            }

            SetPost(PostSignal.VGA);
            yield return new WaitForSecondsRealtime(.28f);
            if (!TestPortLoad(4))
            {
                EndTest("POST halted at VGA. The display fixture could not draw its expected load.");
                yield break;
            }

            SetPost(PostSignal.BOOT);
            yield return new WaitForSecondsRealtime(.28f);
            if (ReadStorage() != 12)
            {
                EndTest("POST halted at BOOT. Storage enumeration or block integrity failed.");
                yield break;
            }

            postPassed = true;
            SetPost(PostSignal.Ready);
            if (Stage == 2) Award();
            EndTest("POST complete. Both DDR5 channels and the boot drive passed fresh patterns.");
        }

        private bool TestMemory(int index)
        {
            if (!Live(dimms[index], 1.10f, .013f)) return false;
            var memory = new uint[32];
            for (int order = 0; order < 2; order++)
            {
                for (int cell = 0; cell < memory.Length; cell++) memory[cell] = MemoryWord(cell, order);
                for (int cell = memory.Length - 1; cell >= 0; cell--)
                    if (memory[cell] != MemoryWord(cell, order)) return false;
            }
            return true;
        }

        private static uint MemoryWord(int cell, int order) =>
            (order == 0 ? 0xAAAAAAAAu : 0x55555555u) ^ unchecked(0x9E3779B9u * (uint)(cell + 1));

        private int ReadStorage()
        {
            int blocks = 0;
            bool intermittent = activeFault == 2 && !candidateRepair;
            var block = new byte[32];
            for (int number = 0; number < 12; number++)
            {
                if (intermittent)
                {
                    Switch(storage.contact, number % 4 != 2 && number % 4 != 3);
                    RefreshElectrical();
                }
                if (!Live(storage, 3.3f, .016f)) continue;
                for (int offset = 0; offset < block.Length; offset++) block[offset] = (byte)((17 * number + 53 * offset) & 255);
                uint written = Checksum(block);
                var readBack = (byte[])block.Clone();
                if (Checksum(readBack) == written) blocks++;
            }
            return blocks;
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

        public bool RunInterface()
        {
            if (Stage is < 3 or > 5 || !running || !postPassed || IsBusy)
                return Reject("A fresh POST is required before exercising interfaces.");
            if (pickedMode != CorrectPortModes[pickedPort])
                return Reject($"{PortModes[pickedMode]} is incompatible with {PortNames[pickedPort]}. Select a suitable fixture mode.");
            int samples = TestInterface(pickedPort);
            detail = $"{PortNames[pickedPort]} · {PortModes[pickedMode]} · {samples}/12 verified";
            if (samples != 12)
            {
                checkedPorts &= ~(1 << pickedPort);
                return Reject("The loaded interface or its bounded data check failed.");
            }
            checkedPorts |= 1 << pickedPort;
            RecordRestart();
            if (checkedPorts == 255 && Stage == 3)
            {
                Award();
                if (repaired == 3) Award();
                return Note(repaired == 3 ? "Electrical qualification restored. Begin sustained thermal qualification." :
                    "Eight interfaces passed. Begin the three independent hardware fault runs.");
            }
            return Note("Fixture and response verified. Continue with the remaining interfaces.");
        }

        private int TestInterface(int index)
        {
            if (!TestPortLoad(index)) return 0;
            return index switch
            {
                2 => ReadStorage(),
                3 => TestStorageFixture(index),
                _ => Loopback(index)
            };
        }

        private static int TestStorageFixture(int index)
        {
            var media = new byte[12][];
            var writeCrc = new uint[media.Length];
            for (int block = 0; block < media.Length; block++)
            {
                var payload = new byte[32];
                for (int offset = 0; offset < payload.Length; offset++)
                    payload[offset] = (byte)(index + block * 17 + offset * 5);
                writeCrc[block] = Checksum(payload);
                media[block] = payload;
            }

            int verified = 0;
            for (int block = media.Length - 1; block >= 0; block--)
            {
                byte[] response = media[block];
                if (response[0] == index + block * 17 && Checksum(response) == writeCrc[block]) verified++;
            }
            return verified;
        }

        private int Loopback(int index)
        {
            int verified = 0;
            for (int frame = 0; frame < 12; frame++)
            {
                float fraction = frame / 11f;
                uint stimulus = unchecked((uint)(frame + 1) * 0x9E3779B9u);
                byte[] packet = { (byte)index, (byte)frame, (byte)(stimulus >> 8), (byte)stimulus };
                bool valid = index switch
                {
                    0 or 1 => TestLanes(stimulus, index == 0 ? 16 : 1),
                    4 => TestDigitalVideo(stimulus),
                    5 => Mathf.Abs(fraction * .7f - .7f * Mathf.Round(fraction * 255f) / 255f) < .002f,
                    6 => Checksum(EchoPacket(packet, frame)) == Checksum(packet),
                    _ => ReceiveFrame(packet, frame)
                };
                if (!valid) continue;
                verified++;
            }
            return verified;
        }

        private static bool TestLanes(uint pattern, int laneCount)
        {
            var lanes = new uint[laneCount];
            for (int bit = 0; bit < 32; bit++)
                lanes[bit % laneCount] |= (pattern >> bit & 1u) << (bit / laneCount);
            uint received = 0;
            for (int bit = 31; bit >= 0; bit--)
                received = (received << 1) | (lanes[bit % laneCount] >> (bit / laneCount) & 1u);
            return received == pattern;
        }

        private static bool TestDigitalVideo(uint rgb)
        {
            byte red = (byte)(rgb >> 16), green = (byte)(rgb >> 8), blue = (byte)rgb;
            uint decoded = (uint)(red * 65536 + green * 256 + blue);
            return decoded == (rgb & 0xFFFFFFu);
        }

        private static byte[] EchoPacket(byte[] packet, int sequence)
        {
            var response = new byte[packet.Length];
            for (int offset = 0; offset < response.Length; offset++) response[offset] = packet[offset];
            return response[1] == sequence ? response : Array.Empty<byte>();
        }

        private static bool ReceiveFrame(byte[] packet, int sequence)
        {
            byte[] wire = packet.Concat(BitConverter.GetBytes(Checksum(packet))).ToArray();
            var payload = new byte[packet.Length];
            Buffer.BlockCopy(wire, 0, payload, 0, payload.Length);
            return payload[1] == sequence && Checksum(payload) == BitConverter.ToUInt32(wire, payload.Length);
        }

        private bool TestPortLoad(int index)
        {
            Channel channel = ports[index];
            Switch(channel.contact, true);
            RefreshElectrical();
            bool pass = !supplies[4].IsCurrentLimited && Live(channel, 3.30f, .006f);
            Switch(channel.contact, false);
            RefreshElectrical();
            return pass;
        }

        public bool InjectFault()
        {
            if (Stage != 4 || activeFault >= 0 || repaired >= 3 || !running || !postPassed || checkedPorts != 255 || IsBusy)
                return Reject("Finish the clean baseline before opening a fault ticket.");
            activeFault = repaired;
            identified = selectedFault = -1;
            faultProbed = candidateRepair = false;
            switch (activeFault)
            {
                case 0: Switch(vrm, false); break;
                case 1: Switch(dimms[1].contact, false); break;
            }
            checkedPorts = 0;
            checkedRails &= 1;
            postPassed = false;
            detail = "";
            RefreshElectrical();
            return Note($"Ticket {repaired + 1}/3 active. Read POST, then choose an independent diagnostic.");
        }

        public bool Diagnose(int index)
        {
            if (Stage != 4 || activeFault < 0 || candidateRepair || !running || IsBusy)
                return Reject("Run a powered fault ticket before collecting its evidence.");
            RefreshElectrical();
            switch (index)
            {
                case 0:
                    faultProbed = post == PostSignal.CPU && RailOK(1) && !RailOK(2);
                    detail = $"EPS {Volts(rails[1]):F2} V  /  VCORE {Volts(rails[2]):F2} V";
                    break;
                case 1:
                    bool first = TestMemory(0), second = TestMemory(1);
                    faultProbed = post == PostSignal.DRAM && RailOK(3) && first && !second;
                    detail = $"Channel 1 {(first ? "trained" : "failed")}  /  Channel 2 {(second ? "trained" : "failed")}";
                    break;
                case 2:
                    int blocks = ReadStorage();
                    faultProbed = post == PostSignal.BOOT && blocks is > 0 and < 12;
                    detail = $"M.2  {blocks}/12 readable blocks · intermittent link";
                    break;
                default: return false;
            }
            identified = faultProbed ? index : -1;
            return faultProbed ? Note("Signature captured. Isolate the bench, select the physical repair site and choose a spare.") :
                Reject("This probe did not isolate the root cause. Compare measurements at the stalled POST stage.");
        }

        public void SetRepairSite(int index)
        {
            if (index < 0 || index >= faultSites.Length) return;
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

        public bool Repair()
        {
            if (Stage != 4 || activeFault < 0 || !faultProbed || identified != activeFault ||
                candidateRepair || standby || running || supplies.Any(source => source.IsOutputActive) || IsBusy)
                return Reject("Diagnose the fault and physically isolate all power before replacing a part.");
            if (selectedFault != activeFault || pickedSpare != activeFault)
                return Reject("The selected site or spare does not match the measured fault.");
            candidateRepair = true;
            Switch(vrm, true);
            Switch(dimms[1].contact, true);
            Switch(storage.contact, true);
            return Note("Replacement fitted. Repeat standby, loaded rails, POST and all eight interfaces before signing the repair.");
        }

        public bool VerifyRepair()
        {
            if (Stage != 4 || !candidateRepair || !running || IsBusy || !postPassed || checkedRails != 31 || checkedPorts != 255 ||
                !Enumerable.Range(0, 5).All(RailOK))
                return Reject("A repair needs fresh passing rails, POST and all eight interface tests.");
            repaired++;
            activeFault = identified = selectedFault = -1;
            candidateRepair = faultProbed = false;
            if (repaired == 3) Award();
            SaveCheckpoint();
            return Note(repaired == 3 ? "All three repairs verified. Begin sustained thermal qualification." :
                $"Repair verified ({repaired}/3). Open the next fault ticket.");
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


