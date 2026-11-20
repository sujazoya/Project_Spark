using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel106Process
    {
        private enum Post { Idle, Ramp, Sec, Pei, Dxe, Bds, Handoff, Halt }
        private static readonly string[] FaultNames = { "F1 · CPU POWER-GOOD", "F2 · SPI FLASH", "F3 · eMMC CONTACT" };
        private static readonly string[] RepairNames = { "Replace timing resistor", "Clear solder bridge", "Reseat storage" };
        private const float RailStableDelay = .38f;

        private Post post;
        private int lanes, repaired, activeFault = -1;
        private bool faultWitnessed, reworked, armed, interrupted, resetAuthorized, lastHigh;
        private int transitions;
        private float time, bootAge, phaseAge, stableSince = -1f, nextPulse, lastRise = float.NegativeInfinity;
        private string postCode = "00 · Awaiting power";

        public bool Busy => post is >= Post.Ramp and <= Post.Bds;
        public bool BootPassed => post == Post.Handoff;
        public int TrainedLanes => lanes;
        public int Repaired => repaired;
        public bool FaultWitnessed => faultWitnessed;
        public string PostCode => postCode;
        public bool Fresh => linked && transitions >= 2 && time - lastRise < .55f;

        public bool TogglePower()
        {
            if (powered)
            {
                IsolatePower();
                message = "Bench isolated. You can work on the board.";
                return true;
            }
            if (stage < 2 || !Construction || !linked)
                return Reject("Complete assembly, copper, vias and W141 before enabling the bench.");
            powered = true;
            SetContact(bench, true);
            Solve();
            message = "12 V standby is live. Run POST to request the CPU and memory rails.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            SetContact(bench, false);
            SetContact(coreEnable, false);
            SetContact(ddrEnable, false);
            SetContact(resetSignal, false);
            SetContact(flashBridge, false);
            SetContact(workload, false);
            InvalidateBoot();
            postCode = "00 · Bench isolated";
            Solve();
        }

        public bool StartPost()
        {
            if (stage < 2 || !powered || !Construction || !linked || Busy || Voltage(standby) < 11f)
                return Reject("POST needs a stable standby rail, complete copper and a linked W141.");
            InvalidateBoot();
            SetContact(workload, false);
            SetContact(resetSignal, false);
            SetContact(coreEnable, true);
            SetContact(ddrEnable, true);
            SetContact(flashBridge, FaultLive(1));
            SetContact(storageContact, !FaultLive(2));
            Solve();
            MoveTo(Post.Ramp, "10 · Qualifying VCORE and DDR4 VDD");
            message = "POST is sampling rails, reset timing and subsystem contacts.";
            return true;
        }

        private bool FaultLive(int index) => activeFault == index && !reworked;
        private bool RailsReady => Voltage(standby) > 11f && InRange(Voltage(core), 1f, .06f) &&
            InRange(Voltage(ddr), 1.2f, .07f);
        private bool ResetReleased => Voltage(resetSense) > 2.7f;
        private static bool InRange(float value, float nominal, float tolerance) => Mathf.Abs(value - nominal) <= tolerance;
        private void Solve() => manager.ElectricalSolver.SolveNow();

        private void InvalidateBoot()
        {
            post = Post.Idle;
            lanes = 0;
            resetAuthorized = false;
            ClearHeartbeat();
            postCode = "00 · Fresh POST required";
        }

        private void AdvanceBoot(float delta)
        {
            time += delta;
            if (!powered || !Construction || !Busy)
            {
                if (BootPassed && (!RailsReady || !ResetReleased || Voltage(flashSense) < 2.7f ||
                    Voltage(linkSense) < 2.7f || Voltage(storageSense) < 2.7f))
                    Halt("EF · Boot evidence lost", -1);
                return;
            }

            bootAge += delta;
            phaseAge += delta;
            if (post == Post.Ramp)
            {
                if (!RailsReady) stableSince = -1f;
                else if (stableSince < 0f) stableSince = bootAge;
                bool stable = stableSince >= 0f && bootAge - stableSince >= RailStableDelay;
                bool release = FaultLive(0) ? bootAge >= .13f : stable;
                if (release && !resetSignal.IsClosed)
                {
                    SetContact(resetSignal, true);
                    Solve();
                }
                if (ResetReleased && !stable) { Halt("E1 · Reset rose before the stable-rail window", 0); return; }
                if (stable && ResetReleased) MoveTo(Post.Sec, "20 · SEC / CPU reset released");
                else if (bootAge > 1.6f) Halt("E0 · Rail qualification timed out", -1);
                return;
            }

            if (!RailsReady || !ResetReleased) { Halt("E0 · Rail or reset lost during POST", -1); return; }
            if (phaseAge < .19f) return;
            phaseAge = 0f;
            switch (post)
            {
                case Post.Sec:
                    if (!ReadImage(firmware, firmwareChecksum, Voltage(flashSense) > 2.7f))
                        Halt("E2 · SPI firmware checksum mismatch", 1);
                    else MoveTo(Post.Pei, "30 · PEI / training DDR4");
                    break;
                case Post.Pei:
                    if (!TrainLane(lanes)) { Halt($"E4 · DDR4 lane {lanes + 1} failed", -1); break; }
                    lanes++;
                    if (lanes == 4)
                    {
                        if (stage == 2) Award();
                        MoveTo(Post.Dxe, "40 · DXE / enumerating PCIe");
                    }
                    else postCode = $"30 · PEI / DDR4 lanes {lanes}/4";
                    break;
                case Post.Dxe:
                    if (Voltage(linkSense) < 2.7f) Halt("E5 · PCIe diagnostic endpoint absent", -1);
                    else MoveTo(Post.Bds, "50 · BDS / verifying eMMC boot image");
                    break;
                case Post.Bds:
                    if (Voltage(storageSense) < 2.7f || !ReadImage(bootImage, storageChecksum, true))
                        Halt("E3 · eMMC image read failed", 2);
                    else CompletePost();
                    break;
            }
        }

        // Small game-owned fixtures, with independent expected checksums and deterministic contact errors.
        private static readonly byte[] firmware = { 0x53, 0x46, 0x58, 0x14, 0x01, 0xC3 };
        private static readonly byte[] bootImage = { 0x42, 0x4F, 0x4F, 0x54, 0x14, 0x06 };
        private const int firmwareChecksum = 0xD9CC, storageChecksum = 0x0CDC;

        private static bool ReadImage(byte[] image, int expected, bool healthy)
        {
            int checksum = 0xFFFF;
            for (int i = 0; i < image.Length; i++)
            {
                int value = image[i] ^ (!healthy && i == 2 ? 0x20 : 0);
                checksum ^= value << 8;
                for (int bit = 0; bit < 8; bit++)
                    checksum = ((checksum << 1) ^ ((checksum & 0x8000) != 0 ? 0x1021 : 0)) & 0xFFFF;
            }
            return checksum == expected;
        }

        private bool TrainLane(int lane)
        {
            float deviation = Mathf.Abs(Voltage(ddr) - 1.20f);
            float timingMargin = .075f - lane * .004f - deviation;
            if (timingMargin <= 0) return false;
            for (int bit = 0; bit < 8; bit++)
            {
                byte written = (byte)(1 << bit);
                byte read = timingMargin < .01f && bit == lane ? (byte)0 : written;
                if (read != written) return false;
            }
            return true;
        }

        private void MoveTo(Post value, string code)
        {
            post = value;
            phaseAge = 0f;
            if (value == Post.Ramp) { bootAge = 0f; stableSince = -1f; }
            postCode = code;
        }

        private void Halt(string code, int fault)
        {
            MoveTo(Post.Halt, code);
            if (fault == activeFault && fault >= 0 && !reworked)
            {
                faultWitnessed = true;
                QueueCheckpoint();
                message = $"{FaultNames[fault]} reproduced. Isolate the bench and locate the affected part.";
            }
            else message = "POST halted. Inspect the live rail and contact readings.";
        }

        private void CompletePost()
        {
            MoveTo(Post.Handoff, "A0 · OS handoff / SFX-14 READY");
            if (stage == 3) Award();
            if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                reworked = faultWitnessed = false;
                if (repaired == 3) Award();
                QueueCheckpoint();
            }
            message = stage switch
            {
                4 when activeFault < 0 => "POST passed. Run the next fault trial.",
                5 when !interrupted => "Wait for a fresh U14 heartbeat, then open W141 under power.",
                5 => "POST passed after reconnect. Wait for U15 to see fresh pulses, then reset safety.",
                _ => "Firmware verified. Four DDR4 lanes, PCIe x4 and the eMMC image passed."
            };
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= FaultNames.Length || !BootPassed)
                return Reject("Finish a healthy boot or the current repair before starting a fault trial.");
            activeFault = repaired;
            faultWitnessed = reworked = false;
            QueueCheckpoint();
            return StartPost();
        }

        public bool LocateFault()
        {
            if (activeFault < 0 || !faultWitnessed) return Reject("Reproduce the fault in POST first.");
            SelectFootprint(repairParts[activeFault]);
            FocusPart();
            return true;
        }

        public bool Repair()
        {
            if (activeFault < 0 || !faultWitnessed || reworked || powered || footprint != repairParts[activeFault])
                return Reject("Isolate power and select the diagnosed footprint before repairing.");
            reworked = true;
            SetContact(flashBridge, false);
            SetContact(storageContact, true);
            QueueCheckpoint();
            message = "Contact serviced. Restore power and run a fresh full POST to prove the repair.";
            return true;
        }

        private void ClearHeartbeat()
        {
            transitions = 0;
            lastHigh = false;
            lastRise = float.NegativeInfinity;
            SetContact(heartbeat, false);
        }

        private void UpdateSafety()
        {
            bool monitorReady = stage >= 5 && powered && Construction && BootPassed && repaired == 3;
            if (monitorReady && time >= nextPulse)
            {
                nextPulse = time + .16f;
                SetContact(heartbeat, !heartbeat.IsClosed);
                Solve();
            }
            if (!monitorReady || !linked) ClearHeartbeat();
            bool high = monitorReady && linked && Voltage(jumper.to) > 2.7f;
            if (high && !lastHigh) { transitions++; lastRise = time; }
            lastHigh = high;
            bool permit = monitorReady && linked && Fresh && (!interrupted || resetAuthorized);
            SetContact(workload, permit);
            if (permit && !interrupted) armed = true;
        }

        public bool OpenW141()
        {
            if (stage < 5 || !powered || !armed || !linked || Inhibited)
                return Reject("Wait for fresh U14 heartbeat and an energized protected output before opening W141.");
            return RemoveWire(jumper);
        }

        public bool RestoreW141()
        {
            SelectRoute(routes.Length);
            return ConnectPads();
        }

        public bool ResetSafety()
        {
            if (stage < 5 || !interrupted || !BootPassed || !Fresh || !Construction || !powered)
                return Reject("Reconnect, pass a fresh POST and wait for new U14 pulses before resetting safety.");
            resetAuthorized = true;
            message = "U15 reset authorized. Verifying the protected output.";
            return true;
        }
    }
}
