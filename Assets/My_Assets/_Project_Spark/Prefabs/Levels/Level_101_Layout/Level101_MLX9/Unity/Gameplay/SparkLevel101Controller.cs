using System;
using UnityEngine;
using TrainingChannel = ProjectSpark.Gameplay.SignalPhantom.SparkLevel102Channel;

namespace ProjectSpark.Gameplay.HiddenLayers
{
    public sealed partial class SparkLevel101Process
    {
        [Serializable]
        public sealed class Lane
        {
            public int positive, negative;
            public SparkTerminal receiveP, receiveN;
            public SparkSwitch pHigh, pLow, nHigh, nLow;
            public SparkResistor termination;
        }

        private readonly TrainingChannel.Capture[] captures = new TrainingChannel.Capture[2];
        private bool running, calibrated, trained, measuring, qualified, booted, memoryVerified;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset, lastHigh, localPulse;
        private int activeFault = -1, repaired, trainingEpoch, trainingStep, requiredEpoch, intervalCount, sampleCount, runNumber, memoryStep;
        private readonly SparkLevel101Memory memory = new();
        private SparkLevel101Memory.Image image;
        private float memoryAt;
        private string memoryEvidence = "FLASH EMPTY · RAM PENDING";
        private int displayLane;
        private bool showEye;
        private float time, powerAt, nextSolve, nextPulse, lastRise = float.NegativeInfinity, phaseAt;
        private string benchState = "BENCH ISOLATED";
        private string evidence = "No calibrated capture";

        public bool Running => running && booted;
        public bool ControllerReady => running;
        public bool HasImage => memory.HasImage;
        public bool MemoryVerified => memoryVerified;
        public bool Programming => memory.Busy;
        public float FlashDrive { get; private set; }
        public float FlashReceive { get; private set; }
        public float ReturnOhms { get; private set; }
        public bool Calibrated => calibrated;
        public bool Trained => trained;
        public bool Measuring => measuring;
        public bool Qualifies => qualified;
        public bool FaultWitnessed => faultWitnessed;
        public int Repaired => repaired;
        public int Epoch => trainingEpoch;
        public int Samples => sampleCount;
        public int RunNumber => runNumber;
        public string Evidence => evidence;
        public bool Fresh => linked && intervalCount >= 2 && time - lastRise < .54f;
        public float MeasuredOhms(int lane) => captures[lane]?.terminationOhms ?? 0f;
        public float MeasuredSkew(int lane) => captures[lane]?.skewPs ?? 0f;
        public int ObservedErrors(int lane) => captures[lane]?.errors ?? -1;

        private bool FaultLive(int index) => activeFault == index && !reworked;
        private bool RailsReady => powered && TrialTopology && regulator.IsConducting && transceiverRegulator.IsConducting &&
            Voltage(standby) > 4.7f && Voltage(vdd) is > 3.05f and < 3.5f && Voltage(laneRail) is > 1.66f and < 1.95f &&
            Voltage(monitorSupply) > 3.0f;

        private void Solve()
        {
            var solver = manager.ElectricalSolver;
            solver.MarkDirty();
            solver.SolveNow();
        }

        public bool TogglePower()
        {
            if (powered)
            {
                IsolatePower();
                message = "Bench isolated. Select the measured repair target.";
                return true;
            }
            if (stage < 2 || !TrialTopology || !linked)
                return Reject("Complete the board, copper and W101 before powering up.");
            powered = true;
            powerAt = time;
            SetContact(bench, true);
            ConfigureFaultDevices();
            benchState = "5 V INPUT · RAIL STARTUP";
            message = "Wait for the regulated rails, then program or boot the diagnostic image.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            InvalidateBoot();
            SetContact(transceiverRegulator, false);
            SetContact(regulator, false);
            SetContact(bench, false);
            benchState = "BENCH ISOLATED";
            Solve();
        }

        private void InvalidateBoot()
        {
            running = booted = memoryVerified = calibrated = trained = false;
            trainingStep = memoryStep = 0;
            memory.Abort();
            SetContact(flashClock, false);
            memoryEvidence = memory.HasImage ? "FLASH IMAGE HELD · RAM PENDING" : "FLASH EMPTY · RAM PENDING";
            InvalidateDiagnostics();
            ClearHeartbeat();
            SetContact(clockDrive, false);
            SetContact(clockDischarge, true);
            SetContact(resetRelease, false);
            foreach (var lane in lanes) Drive(lane, 0);
        }

        private void InvalidateDiagnostics()
        {
            qualified = measuring = false;
            Array.Clear(captures, 0, captures.Length);
            evidence = "No fresh calibrated capture";
        }

        public bool Calibrate()
        {
            if (!Running || !memoryVerified || !RailsReady || measuring || trainingStep != 0)
                return Reject("Verify flash and RAM before calibrating the two lane probes.");
            calibrated = true;
            trained = false;
            InvalidateDiagnostics();
            message = "Probe reference calibrated. Train both differential lanes.";
            return true;
        }

        private void TickController(float delta)
        {
            time += delta;
            if (!powered) return;
            if (time >= nextSolve)
            {
                nextSolve = time + .055f;
                Solve();
            }

            if (!running)
            {
                if (time - powerAt > .18f && Voltage(standby) > 4.7f) SetContact(regulator, true);
                if (regulator.IsConducting && Voltage(vdd) > 3.05f) SetContact(transceiverRegulator, true);
                if (!RailsReady) return;
                SetContact(clockDischarge, false);
                SetContact(clockDrive, true);
                if (Voltage(clockSense) < 2.70f) return;
                SetContact(resetRelease, true);
                if (Voltage(resetSense) < 2.65f) return;
                running = true;
                benchState = "RAILS READY · BOOT PENDING";
                message = "Rails, reference clock and reset verified. Program the image or boot and test memory.";
            }
            else if (!RailsReady || Voltage(clockSense) < 2.55f || Voltage(resetSense) < 2.55f)
            {
                InvalidateBoot();
                benchState = "CONTROLLER RESET";
                message = "Power or reference lost. Wait for stable rails and recalibrate.";
                return;
            }

            if (memory.Tick(delta))
            {
                benchState = "FLASH VERIFIED · BOOT READY";
                memoryEvidence = "IMAGE PROGRAMMED · START BOOT";
                message = "MLX-9 diagnostic image programmed and read back. Boot and test RAM.";
                QueueCheckpoint();
            }
            if (memoryStep != 0 && time - memoryAt >= .14f) AdvanceMemory();
            if (booted) sampleCount++;
            if (trainingStep != 0 && time - phaseAt > .15f) AdvanceTraining();
            if (measuring && time - phaseAt > .32f) AcquireCaptures();
            UpdateSupervisor();
        }

        private static void Drive(Lane lane, int polarity)
        {
            SetContact(lane.pHigh, false);
            SetContact(lane.pLow, false);
            SetContact(lane.nHigh, false);
            SetContact(lane.nLow, false);
            if (polarity == 0) return;
            SetContact(polarity > 0 ? lane.pHigh : lane.nHigh, true);
            SetContact(polarity > 0 ? lane.nLow : lane.pLow, true);
        }

        public bool Train()
        {
            if (!Running || !memoryVerified || !calibrated || !RailsReady || measuring || trainingStep != 0)
                return Reject("Calibrate both lanes before training.");
            trained = false;
            InvalidateDiagnostics();
            foreach (var lane in lanes) Drive(lane, 1);
            trainingStep = 1;
            phaseAt = time;
            message = "Training both lanes. Checking P and N at both receivers.";
            return true;
        }

        private bool CheckPolarity(int expected)
        {
            foreach (var lane in lanes)
            {
                float p = Voltage(lane.receiveP), n = Voltage(lane.receiveN);
                if (expected * (p - n) < 1.50f || Mathf.Min(p, n) > .25f || Mathf.Max(p, n) > 1.95f) return false;
            }
            return true;
        }

        private void AdvanceTraining()
        {
            if (!CheckPolarity(trainingStep == 1 ? 1 : -1))
            {
                trainingStep = 0;
                message = "Receiver polarity or common-mode check failed. Inspect both pairs.";
                return;
            }
            if (trainingStep == 1)
            {
                foreach (var lane in lanes) Drive(lane, -1);
                trainingStep = 2;
                phaseAt = time;
                return;
            }
            trainingStep = 0;
            trained = true;
            trainingEpoch++;
            message = "Both native paths verified in both polarities. Run a full link test.";
        }

        public void SelectImage() => image = (SparkLevel101Memory.Image)(((int)image + 1) % 3);

        public bool Program()
        {
            if (!running || !RailsReady || memory.Busy || memoryStep != 0 || measuring || trainingStep != 0)
                return Reject("Programming requires stable rails and idle instruments.");
            if (!memory.Begin(image, out string failure)) return Reject(failure);
            booted = calibrated = trained = memoryVerified = false;
            benchState = "PROGRAMMING FLASH";
            InvalidateDiagnostics();
            message = "Writing the curated MLX-9 flash image.";
            return true;
        }

        public bool BootAndTestMemory()
        {
            if (!running || !RailsReady || !memory.HasImage || memory.Busy || memoryStep != 0 || measuring || trainingStep != 0)
                return Reject("Program a verified MLX-9 image and wait for power, clock and reset.");
            memoryVerified = trained = calibrated = false;
            InvalidateDiagnostics();
            memoryStep = 1;
            memoryAt = time;
            SetContact(flashClock, true);
            message = "Checking the SPI clock at U1 and U2 before flash CRC and RAM readback.";
            return true;
        }

        private void AdvanceMemory()
        {
            FlashDrive = Voltage(flashTx);
            FlashReceive = Voltage(flashRx);
            if (memoryStep == 1 && FlashDrive > 2.65f && FlashReceive > 2.65f)
            {
                SetContact(flashClock, false);
                memoryStep = 2;
                memoryAt = time;
                return;
            }
            bool clockValid = memoryStep == 2 && FlashDrive < .25f && FlashReceive < .25f;
            memoryStep = 0;
            SetContact(flashClock, false);
            if (!clockValid || !memory.BootAndVerify(out string proof))
            {
                memoryEvidence = "MEMORY CHECK FAILED";
                message = $"Clock or readback failed · drive {FlashDrive:F2} V · receive {FlashReceive:F2} V.";
                if (FaultLive(0) && FlashDrive > 2.65f && FlashReceive < .25f)
                {
                    faultWitnessed = true;
                    QueueCheckpoint();
                }
                return;
            }
            booted = memoryVerified = true;
            benchState = "MLX-9 MCU RUNNING";
            memoryEvidence = proof;
            if (stage == 2) Award();
            message = "MCU boot, SPI flash CRC and RAM verified. Calibrate and train the link.";
            QueueCheckpoint();
        }

        public bool RunDiagnostics()
        {
            if (!Running || !calibrated || !trained || !RailsReady || measuring || memoryStep != 0 ||
                (!memoryVerified && !FaultLive(0)) || !CheckPolarity(-1))
                return Reject("Boot and verify memory, then calibrate and train both differential pairs.");
            InvalidateDiagnostics();
            measuring = true;
            phaseAt = time;
            if (FaultLive(0)) SetContact(flashClock, true);
            message = "Measuring return impedance, both 1 Gb/s lanes and finite PRBS captures.";
            return true;
        }

        private static float RouteLength(Route route)
        {
            float length = 0f;
            for (int i = 1; i < route.copper.positionCount; i++)
                length += Vector3.Distance(route.copper.GetPosition(i - 1), route.copper.GetPosition(i));
            return length * 1000f;
        }

        private void AcquireCaptures()
        {
            measuring = false;
            if (!CheckPolarity(-1) || !RailsReady)
            {
                InvalidateDiagnostics();
                SetContact(flashClock, false);
                message = "Capture invalidated by electrical conditions.";
                return;
            }
            runNumber++;
            evidence = $"Capture {runNumber} · training epoch {trainingEpoch} · modeled 1.0 Gb/s";
            if (FaultLive(0))
            {
                FlashDrive = Voltage(flashTx);
                FlashReceive = Voltage(flashRx);
                SetContact(flashClock, false);
                if (FlashDrive > 2.65f && FlashReceive < .25f)
                {
                    faultWitnessed = true;
                    message = $"F1 · V09 open. U1 drives {FlashDrive:F2} V and U2 receives {FlashReceive:F2} V.";
                    QueueCheckpoint();
                }
                else message = "Unable to verify the SPI clock discontinuity.";
                return;
            }

            float returnVoltage = Voltage(returnProbe);
            ReturnOhms = 180f * returnVoltage / Mathf.Max(.01f, Voltage(vdd) - returnVoltage);
            for (int i = 0; i < lanes.Length; i++)
            {
                var lane = lanes[i];
                float positive = RouteLength(routes[lane.positive]);
                float negative = RouteLength(routes[lane.negative]);
                float paired = Mathf.Min(positive, negative);
                captures[i] = TrainingChannel.Measure(positive, negative, lane.termination.Resistance,
                    1.50f, paired, positive, 0f, true, i);
                AddReturnReflection(captures[i], ReturnOhms, paired * .62f);
            }
            var a = captures[0];
            var b = captures[1];
            if (FaultLive(1) && ReturnOhms > 2f)
            {
                faultWitnessed = true;
                displayLane = 0;
                showEye = false;
                message = $"F2 · L5 return is {ReturnOhms:F1} Ω with visible ringing. Locate W073 and isolate.";
            }
            else if (FaultLive(2) && a.skewPs > 15f)
            {
                faultWitnessed = true;
                displayLane = 0;
                showEye = true;
                message = $"F3 · Lane A has {a.skewPs:F0} ps skew with intact continuity. Locate W053 and isolate.";
            }
            if (faultWitnessed) QueueCheckpoint();

            qualified = memoryVerified && ReturnOhms <= 2f && a.Passing && b.Passing;
            if (!qualified)
            {
                if (!faultWitnessed) message = "Capture is outside the return or differential acceptance window.";
                return;
            }
            if (stage == 3) Award();
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                reworked = faultWitnessed = false;
                if (repaired == 3) Award();
                QueueCheckpoint();
            }
            if (stage == 4) message = "Both lanes qualify. Start the next internal fault trial.";
            else if (stage == 5 && !interrupted) message = "Wait for fresh W101 feedback, then open W101 under power.";
            else if (stage == 5) message = "Both lanes qualify. The supervisor is checking the authorization.";
        }

        private static void AddReturnReflection(TrainingChannel.Capture capture, float returnOhms, float distanceMm)
        {
            float arrival = 2f * distanceMm / TrainingChannel.VelocityMmPerNs;
            float gamma = returnOhms / (100f + returnOhms);
            for (int i = 0; i < capture.tdr.Length; i++)
            {
                float time = i * capture.tdrSpanNs / (capture.tdr.Length - 1) - arrival;
                if (time >= 0f) capture.tdr[i] += gamma * Mathf.Exp(-time * 7.5f) * Mathf.Cos(time * 31f);
            }
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !qualified || !Running)
                return Reject("Complete a passing return and two-lane capture before starting a fault trial.");
            activeFault = repaired;
            reworked = faultWitnessed = false;
            ConfigureFaultDevices();
            if (activeFault == 0)
            {
                memoryVerified = false;
                memoryEvidence = "SPI PATH NEEDS RECHECK";
                ReadGraph();
            }
            InvalidateDiagnostics();
            message = "Fault applied. The MCU stays online. Run a fresh link test to isolate the fault.";
            QueueCheckpoint();
            return true;
        }

        private void ConfigureFaultDevices()
        {
            SetContact(routes[memoryRoute].barrel, !FaultLive(0) && (routes[memoryRoute].barrel.IsClosed || stage >= 4));
            SetContact(groundStitch, !FaultLive(1));
            var route = routes[skewRoute];
            if (FaultLive(2))
            {
                float x1 = Mathf.Lerp(route.entry.x, route.destination.x, .30f);
                float x2 = Mathf.Lerp(route.entry.x, route.destination.x, .65f);
                float y = route.entry.y, z = route.entry.z;
                route.copper.positionCount = 6;
                route.copper.SetPositions(new[]
                {
                    route.entry, new Vector3(x1, y, z), new Vector3(x1, y, z - .009f),
                    new Vector3(x2, y, z - .009f), new Vector3(x2, y, z), route.destination
                });
            }
            else
            {
                route.copper.positionCount = 3;
                route.copper.SetPositions(new[] { route.entry,
                    new Vector3(route.destination.x, route.entry.y, route.entry.z), route.destination });
            }
        }

        public bool LocateFault()
        {
            if (!faultWitnessed || activeFault < 0) return Reject("Capture the fault before selecting a repair target.");
            SelectRoute(activeFault == 0 ? memoryRoute : activeFault == 1 ? returnRoute : skewRoute);
            PadView();
            return true;
        }

        public bool Repair()
        {
            if (activeFault < 0 || !faultWitnessed || reworked || powered)
                return Reject("Capture the fault and isolate power before internal rework.");
            int expected = activeFault == 0 ? memoryRoute : activeFault == 1 ? returnRoute : skewRoute;
            if (selectedRoute != expected) return Reject("Select the measured via, stitch or lane segment.");
            reworked = true;
            ConfigureFaultDevices();
            ReadGraph();
            InvalidateDiagnostics();
            QueueCheckpoint();
            BoardView();
            message = "Rework applied. Verify memory, recalibrate and run a fresh two-lane capture.";
            return true;
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
            if (!Running)
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
            SetContact(workload, stage >= 5 && Fresh && qualified && (!interrupted || safetyReset && trainingEpoch >= requiredEpoch));
            if (stage == 5 && !interrupted && !Inhibited) armed = true;
            if (stage == 5 && interrupted && safetyReset && trainingEpoch >= requiredEpoch && qualified &&
                Fresh && !Inhibited && Voltage(protectedFeed) > 3f && Award())
                message = "The Hidden Layers certified. New capture and W101 authorization verified.";
        }

        public bool OpenW101()
        {
            if (stage != 5 || !powered || !armed || !linked || Inhibited || measuring)
                return Reject("Open W101 only after all three repairs and live transmission authorization.");
            return RemoveWire(jumper);
        }

        public bool RestoreW101()
        {
            SelectRoute(routes.Length);
            return ConnectPads();
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !Running || !calibrated || !qualified || !Fresh || !Construction)
                return Reject("Reconnect W101, acquire fresh edges and a qualified capture before resetting U8.");
            safetyReset = true;
            requiredEpoch = trainingEpoch + 1;
            trained = false;
            InvalidateDiagnostics();
            message = "U8 reset authorized. Retrain and run a new full two-lane capture.";
            return true;
        }
    }
}
