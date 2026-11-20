using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.SignalPhantom
{
    public sealed partial class SparkLevel102Process
    {
        [Serializable]
        public sealed class Lane
        {
            public int positive, negative;
            public SparkTerminal receiveP, receiveN;
            public SparkSwitch pHigh, pLow, nHigh, nLow;
            public SparkResistor termination;
            public float exitLengthMm;
        }

        public LineRenderer stubHighlight;
        private readonly SparkLevel102Channel.Capture[] captures = new SparkLevel102Channel.Capture[2];
        private bool running, calibrated, trained, measuring, qualified, aggressor = true;
        private bool faultWitnessed, couplingIsolated, reworked, armed, interrupted, safetyReset, lastHigh, localPulse;
        private int activeFault = -1, repaired, trainingEpoch, trainingStep, requiredEpoch, intervalCount, sampleCount, runNumber;
        private int displayLane;
        private bool showEye;
        private float time, powerAt, nextSolve, nextPulse, lastRise = float.NegativeInfinity, phaseAt;
        private float spacingMm = 1.35f, stubMm = .12f;
        private string benchState = "BENCH ISOLATED";
        private string evidence = "No calibrated capture";

        public bool Running => running;
        public bool Calibrated => calibrated;
        public bool Trained => trained;
        public bool Measuring => measuring;
        public bool Qualifies => qualified;
        public bool FaultWitnessed => faultWitnessed;
        public bool CouplingIsolated => couplingIsolated;
        public int Repaired => repaired;
        public int Epoch => trainingEpoch;
        public int Samples => sampleCount;
        public int RunNumber => runNumber;
        public string Evidence => evidence;
        public float SpacingMm => spacingMm;
        public float StubMm => stubMm;
        public bool Fresh => linked && intervalCount >= 2 && time - lastRise < .54f;
        public float MeasuredOhms(int lane) => captures[lane]?.terminationOhms ?? 0f;
        public float MeasuredCoupling(int lane) => captures[lane]?.coupling ?? 0f;
        public float MeasuredStub(int lane) => captures[lane]?.stubReflection ?? 0f;
        public int ObservedErrors(int lane) => captures[lane]?.errors ?? -1;

        private bool FaultLive(int index) => activeFault == index && !reworked;
        private bool RailsReady => powered && Construction && regulator.IsConducting && transceiverRegulator.IsConducting &&
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
            if (stage < 2 || !Construction || !linked)
                return Reject("Complete the board, copper and W109 before powering up.");
            powered = true;
            powerAt = time;
            SetContact(bench, true);
            ConfigureFaultDevices();
            benchState = "5 V INPUT · RAIL STARTUP";
            message = "Wait for both regulated rails, then calibrate the probes.";
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
            running = calibrated = trained = false;
            trainingStep = 0;
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
            if (!running || !RailsReady || measuring || trainingStep != 0)
                return Reject("Stable 5 V, 3.3 V and 1.8 V rails and released reset are required.");
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
                benchState = "HSX-10 CONTROLLER RUNNING";
                message = "Rails, reference clock and reset verified. Calibrate the probes.";
            }
            else if (!RailsReady || Voltage(clockSense) < 2.55f || Voltage(resetSense) < 2.55f)
            {
                InvalidateBoot();
                benchState = "CONTROLLER RESET";
                message = "Power or reference lost. Wait for stable rails and recalibrate.";
                return;
            }

            sampleCount++;
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
            if (!running || !calibrated || !RailsReady || measuring || trainingStep != 0)
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
            if (stage == 2) Award();
            message = "Both native paths verified in both polarities. Run a full link test.";
        }

        public bool RunDiagnostics()
        {
            if (!running || !calibrated || !trained || !RailsReady || measuring || !CheckPolarity(-1))
                return Reject("Calibrate and train both differential pairs before a fresh capture.");
            InvalidateDiagnostics();
            measuring = true;
            phaseAt = time;
            message = "Sampling both 1 Gb/s training lanes and generating TDR and PRBS captures.";
            return true;
        }

        private static float RouteLength(Route route) =>
            (Mathf.Abs(route.entry.x - route.destination.x) + Mathf.Abs(route.entry.z - route.destination.z)) * 1000f;

        private void AcquireCaptures()
        {
            measuring = false;
            if (!CheckPolarity(-1) || !RailsReady)
            {
                InvalidateDiagnostics();
                message = "Capture invalidated by electrical conditions.";
                return;
            }

            for (int i = 0; i < lanes.Length; i++)
            {
                var lane = lanes[i];
                float tap = RouteLength(routes[lane.positive]);
                float negative = RouteLength(routes[lane.negative]);
                float parallel = Mathf.Abs(routes[lane.positive].entry.z - routes[lane.positive].destination.z) * 1000f;
                captures[i] = SparkLevel102Channel.Measure(tap + lane.exitLengthMm, negative, lane.termination.Resistance,
                    i == 0 ? spacingMm : 1.55f, parallel, tap, i == 1 ? stubMm : 0f, aggressor, i);
            }

            runNumber++;
            evidence = $"Capture {runNumber} · training epoch {trainingEpoch} · modeled 1.0 Gb/s";
            var a = captures[0];
            var b = captures[1];
            if (FaultLive(0) && a.terminationOhms > 110f)
            {
                faultWitnessed = true;
                displayLane = 0;
                showEye = false;
                message = $"F1 · R1 termination inferred at {a.terminationOhms:F0} Ω. Locate R1 and isolate power.";
            }
            else if (FaultLive(1) && a.coupling >= .15f)
            {
                faultWitnessed = true;
                displayLane = 0;
                showEye = true;
                message = "F2 · Lane A has excessive coupling. Pause the aggressor and repeat the capture.";
            }
            else if (FaultLive(1) && faultWitnessed && !aggressor && a.coupling < .03f)
            {
                couplingIsolated = true;
                message = "Lane A improves with the aggressor paused. Locate W041 and isolate power.";
            }
            else if (FaultLive(2) && b.stubReflection >= .16f)
            {
                faultWitnessed = true;
                displayLane = 1;
                showEye = false;
                message = $"F3 · V19 reflects at {b.stubReturnNs:F2} ns. The plated span still conducts. Locate and isolate.";
            }
            if (faultWitnessed) QueueCheckpoint();

            qualified = aggressor && a.Passing && b.Passing;
            if (!qualified)
            {
                if (!faultWitnessed) message = aggressor ? "Capture outside the acceptance window. Inspect the two lanes." :
                    "Aggressor paused. Enable it for a qualifying capture.";
                return;
            }
            if (stage == 3) Award();
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                reworked = faultWitnessed = couplingIsolated = false;
                if (repaired == 3) Award();
                QueueCheckpoint();
            }

            if (stage == 4) message = "Both lanes pass. Start the next signal fault trial.";
            else if (stage == 5 && !interrupted) message = "Wait for fresh W109 feedback, then open W109 under power.";
            else if (stage == 5) message = "Both lanes pass. The supervisor is checking the fresh authorization.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !qualified || !running)
                return Reject("Complete a passing two-lane capture before starting a fault trial.");
            activeFault = repaired;
            reworked = faultWitnessed = couplingIsolated = false;
            aggressor = true;
            ConfigureFaultDevices();
            InvalidateDiagnostics();
            message = "Fault applied. Train state is retained. Run a fresh link capture.";
            QueueCheckpoint();
            return true;
        }

        public void ToggleAggressor()
        {
            if (!running || measuring) return;
            aggressor = !aggressor;
            InvalidateDiagnostics();
            message = aggressor ? "Aggressor enabled. Run a fresh acceptance capture." :
                "Aggressor paused. Run a comparison capture.";
        }

        private void ConfigureFaultDevices()
        {
            lanes[0].termination.SetResistance(FaultLive(0) ? 150f : 100f);
            spacingMm = FaultLive(1) ? .18f : 1.35f;
            stubMm = FaultLive(2) ? 1.38f : .12f;
            var aggressorTrace = routes[spacingRoute];
            var victim = routes[lanes[0].positive];
            var parallelX = victim.destination.x + spacingMm * .001f;
            var y = aggressorTrace.entry.y;
            aggressorTrace.copper.positionCount = 4;
            aggressorTrace.copper.SetPositions(new[]
            {
                aggressorTrace.entry,
                new Vector3(parallelX, y, victim.entry.z + .005f),
                new Vector3(parallelX, y, victim.destination.z - .005f),
                aggressorTrace.destination
            });
            var tap = routes[stubRoute].destination;
            stubHighlight.SetPosition(0, tap);
            stubHighlight.SetPosition(1, tap - Vector3.up * (stubMm * .001f));
            stubHighlight.SetPosition(2, tap - Vector3.up * (stubMm * .001f));
        }

        public bool LocateFault()
        {
            if (!faultWitnessed || activeFault < 0) return Reject("Capture a fault before selecting the repair target.");
            if (activeFault == 0) { SelectFootprint(terminationPart); FocusPart(); }
            else { SelectRoute(activeFault == 1 ? spacingRoute : stubRoute); PadView(); }
            return true;
        }

        public bool Repair()
        {
            if (activeFault < 0 || !faultWitnessed || reworked || powered)
                return Reject("Observe the fault and isolate power before rework.");
            if (activeFault == 1 && !couplingIsolated)
                return Reject("Confirm coupling with a comparison capture while the aggressor is paused.");
            if ((activeFault == 0 && footprint != terminationPart) ||
                (activeFault == 1 && selectedRoute != spacingRoute) || (activeFault == 2 && selectedRoute != stubRoute))
                return Reject("Select the matching R1, W041 or V19 repair target.");
            reworked = true;
            aggressor = true;
            ConfigureFaultDevices();
            InvalidateDiagnostics();
            QueueCheckpoint();
            BoardView();
            message = "Rework applied. Power up, recalibrate and verify both lanes.";
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
                message = "Signal Phantom certified. New capture and W109 authorization verified.";
        }

        public bool OpenW109()
        {
            if (stage != 5 || !powered || !armed || !linked || Inhibited || measuring)
                return Reject("Open W109 only after all three repairs and live transmission authorization.");
            return RemoveWire(jumper);
        }

        public bool RestoreW109()
        {
            SelectRoute(routes.Length);
            return ConnectPads();
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !running || !calibrated || !Fresh || !Construction)
                return Reject("Reconnect W109, wait for fresh edges and calibrate before resetting U11.");
            safetyReset = true;
            requiredEpoch = trainingEpoch + 1;
            trained = false;
            InvalidateDiagnostics();
            message = "U11 reset authorized. Retrain and run a new full two-lane capture.";
            return true;
        }
    }
}
