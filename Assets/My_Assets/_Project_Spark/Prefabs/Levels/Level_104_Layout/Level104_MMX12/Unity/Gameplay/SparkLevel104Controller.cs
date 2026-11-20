using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.MemoryMatrix
{
    public sealed partial class SparkLevel104Process
    {
        [Serializable]
        public sealed class Lane
        {
            public int strobeRoute, dataRoute;
            public SparkTerminal strobe, data;
            public SparkSwitch strobeDrive, dataDrive;
        }

        private readonly SparkLevel104Memory memory = new();
        private readonly SparkLevel104Memory.Window[] windows = new SparkLevel104Memory.Window[4];
        private readonly bool[] high = new bool[4];
        private bool initialized, trained, testing, qualified, faultWitnessed, reworked, reflow;
        private bool ckVerified, interrupted, safetyReset, armed, localPulse, lastHigh;
        private int activeFault = -1, repaired, initStep, trainStep, burst, intervalCount, epoch, requiredEpoch, captureNumber;
        private float time, powerAt, phaseAt, burstAt, reflowAt, nextPulse, lastRise = float.NegativeInfinity;
        private float heat = 25f, maxHeat = 25f, reflowHeat = 25f;
        private string evidence = "No fresh DDR4 training";
        private string benchState = "BENCH ISOLATED";

        public bool Initialized => initialized;
        public bool CkVerified => ckVerified;
        public bool Trained => trained;
        public bool Training => trainStep != 0;
        public bool Testing => testing;
        public bool Qualifies => qualified;
        public bool FaultWitnessed => faultWitnessed;
        public bool Reflowing => reflow;
        public int Epoch => epoch;
        public int CaptureNumber => captureNumber;
        public int Repaired => repaired;
        public int Bits => memory.Bits;
        public int Errors => memory.Errors;
        public float Temperature => heat;
        public float Hottest => maxHeat;
        public float Lane2StrobeHigh { get; private set; }
        public float CaLevel { get; private set; }
        public float CaReflection => Mathf.Abs((termination.Resistance - 47f) / (termination.Resistance + 47f));
        public int HotBgaOpens { get; private set; }
        public string FirstFailure => memory.FirstFailure;
        public string Evidence => evidence;
        public float ReadMargin(int lane) => windows[lane].ReadMargin;
        public float WriteMargin(int lane) => windows[lane].WriteMargin;
        public bool Fresh => linked && intervalCount >= 2 && time - lastRise < .54f;
        private bool FaultLive(int fault) => activeFault == fault && !reworked;
        public bool SupplyReady => powered && TrialTopology && logicGate.IsConducting && vppGate.IsConducting &&
            vddGate.IsConducting && vttGate.IsConducting && Voltage(standby) > 4.7f &&
            Voltage(aux) is > 3.05f and < 3.50f && Voltage(vpp) is > 2.37f and < 2.63f &&
            Voltage(vdd) is > 1.14f and < 1.26f && Voltage(vtt) is > .56f and < .64f &&
            Voltage(vref) is > .56f and < .64f && Voltage(monitorSupply) > 3f;

        private void Solve() => manager.ElectricalSolver.SolveNow();

        public bool TogglePower()
        {
            if (powered)
            {
                IsolatePower();
                message = "Bench isolated. Select the measured repair target.";
                return true;
            }
            if (stage < 2 || !TrialTopology || !linked || reflow)
                return Reject("Complete W125 and the board. Wait for any BGA rework to finish before energizing.");
            powered = true;
            powerAt = time;
            SetContact(bench, true);
            ConfigureFaultDevices();
            Solve();
            benchState = "POWER SEQUENCE · VPP BEFORE VDD";
            message = "Wait for the DDR4 supply sequence before initializing.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            InvalidateBoot();
            SetContact(vttGate, false);
            SetContact(vddGate, false);
            SetContact(vppGate, false);
            SetContact(logicGate, false);
            SetContact(bench, false);
            Solve();
            benchState = "BENCH ISOLATED";
        }

        private void InvalidateBoot()
        {
            initialized = ckVerified = false;
            initStep = 0;
            InvalidateDiagnostics();
            memory.Reset();
            ClearHeartbeat();
            SetContact(ckPDrive, false);
            SetContact(ckNDrive, false);
            SetContact(commandDrive, false);
        }

        private void InvalidateDiagnostics()
        {
            testing = qualified = trained = false;
            trainStep = 0;
            memory.ResetTest();
            Array.Clear(windows, 0, windows.Length);
            DriveLanes(false, false);
            SetContact(bgaBall, true);
            evidence = "No fresh DDR4 training";
        }

        private void ConfigureFaultDevices()
        {
            termination.SetResistance(FaultLive(1) ? 220f : 47f);
            var via = routes[strobeRoute].barrel;
            if (FaultLive(0)) SetContact(via, false);
            else if (activeFault == 0 && reworked) SetContact(via, true);
            SetContact(bgaBall, true);
        }

        private void TickController(float delta)
        {
            time += delta;
            heat = Mathf.MoveTowards(heat, testing ? 52f : powered ? 31f : 25f, delta * (testing ? 32f : 19f));
            if (reflow)
            {
                float age = time - reflowAt;
                reflowHeat = age <= .46f ? Mathf.Lerp(25f, 205f, age / .46f) :
                    Mathf.Lerp(205f, 42f, Mathf.Clamp01((age - .46f) / .64f));
                if (age >= 1.10f)
                {
                    reflow = false;
                    CompleteRepair();
                }
                return;
            }
            if (!powered) return;

            float sincePower = time - powerAt;
            if (sincePower >= .14f && Voltage(standby) > 4.7f && !logicGate.IsClosed) Enable(logicGate);
            if (sincePower >= .27f && Voltage(aux) > 3.05f && !vppGate.IsClosed) Enable(vppGate);
            if (sincePower >= .40f && Voltage(vpp) > 2.37f && !vddGate.IsClosed) Enable(vddGate);
            if (sincePower >= .54f && Voltage(vdd) > 1.14f && !vttGate.IsClosed) Enable(vttGate);
            if (!SupplyReady)
            {
                if (initialized || initStep != 0)
                {
                    InvalidateBoot();
                    benchState = "RAIL LOST · DDR4 RESET";
                    message = "DDR4 reset after loss of a qualified supply.";
                }
                return;
            }
            if (!initialized && initStep == 0) benchState = "RAILS VALID · INITIALIZE DDR4";
            if (initStep != 0 && time - phaseAt > .12f) AdvanceInit();
            if (trainStep != 0 && time - phaseAt > .12f) AdvanceTraining();
            if (testing && time - burstAt >= .024f) AdvanceData();
            UpdateSupervisor();
        }

        private void Enable(SparkSwitch gate)
        {
            SetContact(gate, true);
            Solve();
        }

        public bool InitializeMemory()
        {
            if (!SupplyReady || initStep != 0 || trainStep != 0 || testing)
                return Reject("Wait for stable VPP, VDD, VTT, VREF and controller power.");
            InvalidateBoot();
            SetContact(commandDrive, true);
            SetContact(ckPDrive, true);
            Solve();
            initStep = 1;
            phaseAt = time;
            benchState = "RESET RELEASE · NATIVE CK / CA";
            message = "Checking both CK polarities and live command termination.";
            return true;
        }

        private void AdvanceInit()
        {
            if (initStep == 1)
            {
                CaLevel = Voltage(commandSense);
                if (Voltage(ckP) < .9f || Voltage(ckN) > .20f || CaLevel < .87f) { FailInit(); return; }
                SetContact(ckPDrive, false);
                SetContact(ckNDrive, true);
                Solve();
                initStep = 2;
                phaseAt = time;
                return;
            }

            if (Voltage(ckN) < .9f || Voltage(ckP) > .20f || CaReflection > .30f) { FailInit(); return; }
            initStep = 0;
            ckVerified = initialized = true;
            memory.Initialize();
            benchState = "U2 + U3 INITIALIZED";
            if (stage == 2) Award();
            message = "Both devices initialized. Train all four byte lanes.";
        }

        private void FailInit()
        {
            InvalidateBoot();
            message = "CK or CA failed. Check pads and RN1 before initialization.";
        }

        private void DriveLanes(bool strobe, bool data)
        {
            foreach (var lane in lanes)
            {
                SetContact(lane.strobeDrive, strobe);
                SetContact(lane.dataDrive, data);
            }
        }

        public bool Train()
        {
            if (!SupplyReady || !initialized || !memory.ModeReady || trainStep != 0 || testing)
                return Reject("Initialize both DDR4 devices before training.");
            InvalidateDiagnostics();
            SetContact(commandDrive, true);
            using (manager.ElectricalSolver.BeginConfigurationBatch()) DriveLanes(true, true);
            Solve();
            phaseAt = time;
            trainStep = 1;
            message = "Measuring high and low witnesses before scanning per-lane read/write taps.";
            return true;
        }

        private void AdvanceTraining()
        {
            if (trainStep == 1)
            {
                for (int i = 0; i < lanes.Length; i++)
                    high[i] = Voltage(lanes[i].strobe) > .88f && Voltage(lanes[i].data) > .88f;
                Lane2StrobeHigh = Voltage(lanes[2].strobe);
                CaLevel = Voltage(commandSense);
                using (manager.ElectricalSolver.BeginConfigurationBatch()) DriveLanes(false, false);
                Solve();
                phaseAt = time;
                trainStep = 2;
                return;
            }
            trainStep = 0;
            trained = true;
            for (int i = 0; i < lanes.Length; i++)
            {
                var lane = lanes[i];
                bool responsive = high[i] && Voltage(lane.strobe) < .20f && Voltage(lane.data) < .20f;
                windows[i] = SparkLevel104Memory.Train(Length(routes[lane.strobeRoute]), Length(routes[lane.dataRoute]),
                    Voltage(vref), termination.Resistance, responsive, i);
                trained &= windows[i].Pass;
            }
            if (!trained)
            {
                if (FaultLive(0) && Lane2StrobeHigh < .20f)
                {
                    faultWitnessed = true;
                    message = "F1 · Byte 2 has no DQS at U3. Locate V19 and isolate before via rework.";
                }
                else if (FaultLive(1) && CaReflection > .30f && CaLevel > 1.12f)
                {
                    faultWitnessed = true;
                    message = "F2 · RN1 mismatch raised CA reflection and narrowed the timing windows.";
                }
                else message = "One or more byte lanes fell outside the training window.";
                evidence = $"CA {CaLevel:F2} V · reflection {CaReflection:P0} · byte 2 strobe {Lane2StrobeHigh:F2} V";
                if (faultWitnessed) QueueCheckpoint();
                return;
            }
            epoch++;
            evidence = $"Epoch {epoch} · four read and write windows trained";
            message = "All four byte lanes trained. Run the four-pattern DDR4 data test.";
        }

        private static float Length(Route route)
        {
            float result = 0;
            for (int i = 1; i < route.copper.positionCount; i++)
                result += Vector3.Distance(route.copper.GetPosition(i - 1), route.copper.GetPosition(i));
            return result * 1000f;
        }

        public bool RunData()
        {
            if (!SupplyReady || !initialized || !trained || testing || trainStep != 0)
                return Reject("Resolve electrical conditions and train all four byte lanes first.");
            memory.ResetTest();
            HotBgaOpens = 0;
            maxHeat = heat;
            testing = true;
            qualified = false;
            burst = 0;
            burstAt = time;
            SetContact(lanes[3].dataDrive, true);
            Solve();
            evidence = "Acquiring four patterns and 64 read/write bursts";
            message = "Testing address, walking one, walking zero and seeded data patterns.";
            return true;
        }

        private void AdvanceData()
        {
            burstAt = time;
            maxHeat = Mathf.Max(maxHeat, heat);
            bool open = FaultLive(2) && heat >= 44f && burst % 12 < 5;
            if (bgaBall.IsClosed == open)
            {
                SetContact(bgaBall, !open);
                Solve();
            }
            bool responsive = Voltage(lanes[3].data) > .88f;
            if (!responsive) HotBgaOpens++;
            memory.Burst(burst++, responsive);
            if (burst < SparkLevel104Memory.Bursts) return;

            testing = false;
            SetContact(bgaBall, true);
            SetContact(lanes[3].dataDrive, false);
            captureNumber++;
            evidence = $"Run {captureNumber} · epoch {epoch} · {memory.Errors}/{memory.Bits} observed bit errors";
            if (FaultLive(2) && HotBgaOpens > 0 && memory.Errors > 0 && maxHeat >= 44f)
            {
                faultWitnessed = true;
                message = $"F3 · U3 DQ0 opened during heated bursts. {memory.FirstFailure}.";
                QueueCheckpoint();
            }
            qualified = trained && memory.Bits == SparkLevel104Memory.TestBits && memory.Errors == 0;
            if (!qualified)
            {
                if (!faultWitnessed) message = "The finite memory test failed. Check the failing lane.";
                return;
            }
            if (stage == 3) Award();
            else if (stage == 4 && reworked && activeFault >= 0)
            {
                repaired++;
                activeFault = -1;
                faultWitnessed = reworked = false;
                if (repaired == 3) Award();
                QueueCheckpoint();
            }
            if (stage == 4) message = "All patterns passed. Start the next DDR4 fault trial.";
            else if (stage == 5 && !interrupted) message = "Wait for fresh W125 authorization, then open the jumper under power.";
            else if (stage == 5 && !safetyReset) message = "Fresh training and data verified. Wait for W125 pulses and reset U9.";
            else if (stage == 5) message = "New post-reset capture passed. Waiting for fresh U9 authorization.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !qualified || !SupplyReady)
                return Reject("Finish a healthy four-lane data test before starting another fault.");
            activeFault = repaired;
            reworked = faultWitnessed = false;
            ConfigureFaultDevices();
            if (activeFault == 0) ReadGraph();
            InvalidateDiagnostics();
            message = activeFault == 2 ? "Train at low activity, then run sustained data bursts to warm U3." :
                "Acquire new training evidence to locate the inserted fault.";
            QueueCheckpoint();
            return true;
        }

        public bool LocateFault()
        {
            if (!faultWitnessed || activeFault < 0) return Reject("Capture the fault before opening its repair view.");
            if (activeFault == 0) { SelectRoute(strobeRoute); PadView(); }
            else { SelectFootprint(activeFault == 1 ? terminationPart : bgaPart); FocusPart(); }
            return true;
        }

        public bool Repair()
        {
            if (activeFault < 0 || !faultWitnessed || reworked || powered || reflow)
                return Reject("Identify the failure and isolate power before rework.");
            if (activeFault == 0 ? selectedRoute != strobeRoute : footprint != (activeFault == 1 ? terminationPart : bgaPart))
                return Reject("Select the diagnosed via, RN1 or U3 contact.");
            if (activeFault == 2)
            {
                reflow = true;
                reflowAt = time;
                message = "U3 isolated rework in progress. Wait for the thermal cycle and cooldown.";
            }
            else CompleteRepair();
            return true;
        }

        private void CompleteRepair()
        {
            reworked = true;
            ConfigureFaultDevices();
            ReadGraph();
            InvalidateDiagnostics();
            QueueCheckpoint();
            BoardView();
            message = "Rework applied. Power up, reinitialize, retrain and run fresh data tests.";
        }

        private void ClearHeartbeat()
        {
            intervalCount = 0;
            lastHigh = localPulse = false;
            lastRise = float.NegativeInfinity;
            SetContact(heartbeat, false);
            SetContact(workload, false);
        }

        private void UpdateSupervisor()
        {
            if (!initialized || !qualified || !SupplyReady)
            {
                ClearHeartbeat();
                return;
            }
            if (time >= nextPulse)
            {
                nextPulse = time + .16f;
                localPulse = !localPulse;
                SetContact(heartbeat, localPulse);
                Solve();
            }
            bool high = linked && Voltage(jumper.to) > 2.6f;
            if (high && !lastHigh)
            {
                float gap = time - lastRise;
                intervalCount = gap is >= .24f and <= .46f ? intervalCount + 1 : 0;
                lastRise = time;
            }
            lastHigh = high;
            SetContact(workload, stage >= 4 && Fresh && (!interrupted || safetyReset && epoch >= requiredEpoch));
            if (stage == 5 && !interrupted && !Inhibited) armed = true;
            if (stage == 5 && interrupted && safetyReset && epoch >= requiredEpoch &&
                Fresh && !Inhibited && Voltage(protectedFeed) > 3f && Award())
                message = "Memory Matrix certified. Fresh training, data and W125 authorization verified.";
        }

        public bool OpenW125()
        {
            if (stage != 5 || !powered || !armed || !linked || Inhibited || testing)
                return Reject("Open W125 with all repairs verified and U9 authorization live.");
            return RemoveWire(jumper);
        }

        public bool RestoreW125()
        {
            SelectRoute(routes.Length);
            return ConnectPads();
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !initialized || !qualified || !Fresh || !Construction)
                return Reject("Reconnect W125 and acquire fresh training, data and pulse cadence before resetting U9.");
            safetyReset = true;
            requiredEpoch = epoch + 1;
            InvalidateDiagnostics();
            message = "U9 reset accepted. Retrain all four lanes and run a new complete data test.";
            return true;
        }
    }
}
