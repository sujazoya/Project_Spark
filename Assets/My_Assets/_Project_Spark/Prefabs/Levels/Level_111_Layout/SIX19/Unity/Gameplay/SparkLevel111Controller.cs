using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.SignalCrucible
{
    public sealed partial class SparkLevel111Process
    {
        [Serializable]
        public sealed class Lane
        {
            public SparkSwitch[] drive = new SparkSwitch[4];
            public SparkTerminal[] sense = new SparkTerminal[4];
            public float[] lengthMm = new float[4];
        }

        public Lane[] lanes;
        public SparkTerminal standby, aux, retimerRail, clockP, clockN, returnSense, heatLoad, equalizerBias, monitorSupply, protectedFeed;
        public SparkSwitch bench, auxGate, retimerGate, clockDriveP, clockDriveN, clockNoiseP, clockNoiseN, loadGate, thermalShunt, heartbeat, workload;
        public float returnDistanceMm;

        private readonly SparkLevel111SignalModel.Snapshot[] windows = new SparkLevel111SignalModel.Snapshot[4];
        private readonly SparkLevel111Pattern pattern = new();
        private readonly float[] swings = new float[4], equalization = new float[4];
        private float time, powerAt, clockAt, commissionAt, patternAt, reworkUntil, heartbeatAt, lastRise = -1f;
        private float temperature = 31f, peakTemperature, clockJitter, tdrLoss, tdrDistance;
        private bool clockPhase, clockObserved, commissioned, testing, faultWitnessed, reworked, hotBiasObserved;
        private bool heartbeatDrive, highFeedback, armed, interrupted, safetyReset;
        private int epoch, requiredEpoch, captures, clockMask, commissioning, sampleEpoch = -1, tdrEpoch = -1, eyeEpoch = -1;
        private int calibrationEpoch = -1, patternEpoch = -1, feedbackIntervals, repaired, activeFault = -1, repairTarget = -1;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool RailsReady => powered && Voltage(standby) > 4.7f && Voltage(aux) > 3.05f && Voltage(retimerRail) > 1.68f;
        public bool ClockObserved => clockObserved && clockMask == 3;
        public bool ClockLocked => ClockObserved && clockJitter < 18f;
        public bool Commissioned => commissioned;
        public bool Commissioning => commissioning != 0;
        public bool Testing => testing;
        public bool FaultWitnessed => faultWitnessed;
        public bool Reworking => reworkUntil > time;
        public bool Qualifies => RailsReady && ClockLocked && commissioned && tdrEpoch == epoch && eyeEpoch == epoch &&
            tdrLoss >= 14f && calibrationEpoch == epoch && patternEpoch == epoch && peakTemperature >= 53f &&
            pattern.Errors == 0 && Array.TrueForAll(windows, lane => lane.Healthy);
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .53f;
        public int Epoch => epoch;
        public int Captures => captures;
        public int Bits => pattern.Bits;
        public int Errors => pattern.Errors;
        public int FirstErrorLane => pattern.FirstErrorLane;
        public int Repaired => repaired;
        public float ClockJitter => clockJitter;
        public float Temperature => temperature;
        public float PeakTemperature => peakTemperature;
        public float TdrLoss => tdrLoss;
        public float TdrDistance => tdrDistance;
        public float EyeMv(int lane) => windows[lane].EyeMv;
        public bool HealthyLane(int lane) => windows[lane].Healthy;
        public string Evidence => evidence;

        private bool FaultLive(int fault) => activeFault == fault && !reworked;
        private void Solve() => manager.ElectricalSolver.SolveNow();

        public bool TogglePower()
        {
            if (powered)
            {
                IsolatePower();
                message = "Bench isolated. Apply the measured repair only to its original board target.";
                return true;
            }
            if (stage < 2 || Reworking || !TrialTopology || !linked && !interrupted)
                return Reject("Complete the route graph and W185, and finish any isolated rework, before powering the bench.");
            powered = true;
            powerAt = time;
            SetContact(bench, true);
            InvalidateClock();
            Solve();
            benchState = "POWER SEQUENCE";
            message = "Protected 5 V applied. Wait for 3.3 V and 1.8 V, then verify the scope’s two REFCLK states.";
            QueueCheckpoint();
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            SetContact(bench, false);
            SetContact(auxGate, false);
            SetContact(retimerGate, false);
            InvalidateClock();
            Solve();
            benchState = "BENCH ISOLATED";
            QueueCheckpoint();
        }

        private void InvalidateClock()
        {
            clockMask = 0;
            clockAt = time;
            clockJitter = 0;
            clockObserved = clockPhase = false;
            SetContact(clockDriveP, false);
            SetContact(clockDriveN, false);
            InvalidateDiagnostics();
        }

        private void InvalidateDiagnostics()
        {
            commissioned = testing = false;
            commissioning = 0;
            sampleEpoch = tdrEpoch = eyeEpoch = calibrationEpoch = patternEpoch = -1;
            Array.Clear(equalization, 0, equalization.Length);
            Array.Clear(windows, 0, windows.Length);
            SetContact(loadGate, false);
            ClearHeartbeat();
        }

        public bool VerifyClock()
        {
            if (!RailsReady || clockMask != 3) return Reject("Wait for both native REFCLK polarities and both powered rails.");
            clockObserved = true;
            evidence = $"U5 → U4 · two complementary states · {clockJitter:F1} ps modeled jitter";
            benchState = ClockLocked ? "REFERENCE LOCKED" : "REFERENCE OUT OF SPEC";
            message = ClockLocked ? "Reference qualified. Commission the four lanes at low rate, then 10 Gb/s."
                : "Phase noise exceeds 18 ps. Inspect the reference conditioner before commissioning.";
            return true;
        }

        public bool Commission()
        {
            if (!RailsReady || !ClockLocked || Commissioning || testing)
                return Reject("Verify the reference clock with both rails up before commissioning.");
            InvalidateDiagnostics();
            pattern.Reset();
            hotBiasObserved = false;
            epoch++;
            commissioning = 1;
            commissionAt = time + .18f;
            benchState = "STARTING · 2.5 Gb/s";
            message = "Sampling both differential states on all TX and RX legs.";
            return true;
        }

        public bool ProbeTdr()
        {
            if (!RailsReady || Commissioning || testing) return Reject("Power the bench and pause the pattern before probing reflections.");
            CaptureSwing();
            Measure();
            int worst = 0;
            for (int lane = 1; lane < windows.Length; lane++)
                if (windows[lane].ReturnLoss < windows[worst].ReturnLoss) worst = lane;
            tdrLoss = windows[worst].ReturnLoss;
            tdrDistance = windows[worst].EchoPs * SparkLevel111SignalModel.PropagationMmPerPs * .5f;
            tdrEpoch = epoch;
            plotMode = PlotMode.Tdr;
            evidence = $"Lane {worst} · {tdrLoss:F1} dB return loss · {windows[worst].EchoPs:F0} ps round trip · {tdrDistance:F1} mm";
            message = tdrLoss < 14f ? "The reflection exceeds the return-loss limit. Inspect the return path on its entry layer."
                : "TDR acquired. Inspect the eye and calibrate equalization before the pattern test.";
            return true;
        }

        public bool ProbeEye()
        {
            if (!RailsReady || Commissioning || testing) return Reject("Use the powered bench with the pattern paused to measure the eye.");
            CaptureSwing();
            Measure();
            eyeEpoch = epoch;
            plotMode = PlotMode.Eye;
            evidence = $"Lane 2 · {windows[2].EyeMv:F0} mV eye · {windows[2].EyePs:F1} ps wide · {windows[2].JitterPs:F1} ps jitter";
            message = "Eye snapshots acquired. Calibrate the equalizer after checking the channel.";
            return true;
        }

        public bool CalibrateEqualizer()
        {
            if (!RailsReady || !ClockLocked || eyeEpoch != epoch || testing || Commissioning)
                return Reject("Acquire a fresh eye snapshot and verify the reference before calibrating.");
            for (int lane = 0; lane < lanes.Length; lane++)
                equalization[lane] = Mathf.Clamp(.28f + (560f - windows[lane].EyeMv) / 400f, .15f, .75f);
            Measure();
            calibrationEpoch = epoch;
            plotMode = PlotMode.Eye;
            message = "Equalizer taps set. Run the four-lane hot-load pattern test.";
            return true;
        }

        public bool RunPattern()
        {
            if (!RailsReady || !ClockLocked || sampleEpoch != epoch || tdrEpoch != epoch || eyeEpoch != epoch ||
                calibrationEpoch != epoch || testing || Commissioning || !commissioned && !FaultLive(0))
                return Reject("Commission the lanes, acquire fresh TDR and eye samples, then calibrate before the pattern.");
            pattern.Reset();
            testing = true;
            patternEpoch = -1;
            peakTemperature = temperature;
            hotBiasObserved = false;
            patternAt = time;
            ClearHeartbeat();
            SetContact(loadGate, true);
            Solve();
            benchState = "LIVE · HOT-LOAD PATTERN";
            message = "Comparing PRBS31 words on four lanes while the retimer warms.";
            return true;
        }

        private void TickController(float dt)
        {
            time += dt;
            bool hotLoad = powered && Voltage(heatLoad) > 1.5f;
            temperature = Mathf.MoveTowards(temperature, hotLoad ? 64f : 31f, dt * (hotLoad ? 32f : 18f));
            if (reworkUntil > 0 && !Reworking)
            {
                reworkUntil = 0;
                reworked = true;
                ConfigureFaultDevices();
                Solve();
                message = "Thermal path restored. Power up and run a new hot-load qualification.";
                QueueCheckpoint();
            }
            if (!powered) return;
            bool shunt = FaultLive(2) && temperature >= 53f;
            if (thermalShunt.IsClosed != shunt)
            {
                SetContact(thermalShunt, shunt);
                Solve();
            }
            if (!auxGate.IsClosed && time - powerAt >= .14f && Voltage(standby) > 4.7f)
            {
                SetContact(auxGate, true);
                Solve();
            }
            if (!retimerGate.IsClosed && time - powerAt >= .29f && Voltage(aux) > 3f)
            {
                SetContact(retimerGate, true);
                Solve();
                benchState = "CLOCK CHECK PENDING";
            }
            if (RailsReady && time >= clockAt) TickReference();
            if (Commissioning && time >= commissionAt) TickCommission();
            if (testing && time >= patternAt) TickPattern();
            if (stage == 2 && commissioned && Award())
                message = "Four lanes qualified at 10 Gb/s. Capture TDR and eye, calibrate, then run the hot-load pattern.";
            if (stage == 3 && Qualifies && Award())
                message = "Signal integrity passed. Start the first controlled fault trial.";
            if (stage == 4 && reworked && Qualifies)
            {
                repaired++;
                activeFault = repairTarget = -1;
                faultWitnessed = reworked = false;
                QueueCheckpoint();
                if (repaired == 3)
                {
                    if (Award()) message = "All three repairs verified. Wait for the W185 heartbeat, then open the jumper.";
                }
                else message = "Repair verified with fresh measurements. Start the next fault trial.";
            }
            TickSupervisor();
        }

        private void TickReference()
        {
            clockAt = time + .115f;
            clockPhase = !clockPhase;
            SetContact(clockDriveP, clockPhase);
            SetContact(clockDriveN, !clockPhase);
            Solve();
            float swing = (Voltage(clockP) - Voltage(clockN)) * (clockPhase ? 1f : -1f);
            clockMask = swing > 1.75f ? clockMask | (clockPhase ? 1 : 2) : 0;
            clockJitter = 4.1f + Mathf.Max(0f, 3.27f - swing) * 25f;
            if (commissioned && !ClockLocked)
            {
                InvalidateDiagnostics();
                benchState = "REFERENCE OUT OF SPEC";
                message = "Reference lost. The current lane measurements are invalid.";
            }
        }

        private void TickCommission()
        {
            if (!ClockLocked)
            {
                InvalidateDiagnostics();
                return;
            }
            CaptureSwing();
            Measure();
            if (commissioning == 1)
            {
                commissioning = 2;
                commissionAt = time + .24f;
                benchState = "QUALIFYING · 10 Gb/s";
                return;
            }
            commissioning = 0;
            commissioned = Array.TrueForAll(swings, swing => swing >= 1.55f) &&
                Array.TrueForAll(windows, lane => lane.Healthy);
            benchState = commissioned ? "4 LANES · 10 Gb/s" : "CHANNEL DEGRADED";
            message = commissioned ? "Commissioned. Capture a fresh TDR and eye before calibration and hot qualification."
                : "A channel failed the high-rate envelope. Use TDR and the eye to locate it.";
        }

        private void DriveLanes(bool positive)
        {
            foreach (var lane in lanes)
            for (int leg = 0; leg < lane.drive.Length; leg++)
                SetContact(lane.drive[leg], leg % 2 == (positive ? 0 : 1));
            Solve();
        }

        private float MeasureSwing(Lane lane, bool positive)
        {
            float sign = positive ? 1f : -1f;
            float tx = (Voltage(lane.sense[0]) - Voltage(lane.sense[1])) * sign;
            float rx = (Voltage(lane.sense[2]) - Voltage(lane.sense[3])) * sign;
            return Mathf.Min(tx, rx);
        }

        private void CaptureSwing()
        {
            DriveLanes(true);
            for (int i = 0; i < lanes.Length; i++) swings[i] = MeasureSwing(lanes[i], true);
            DriveLanes(false);
            for (int i = 0; i < lanes.Length; i++) swings[i] = Mathf.Min(swings[i], MeasureSwing(lanes[i], false));
            sampleEpoch = epoch;
        }

        private void Measure()
        {
            float bias = Voltage(equalizerBias);
            bool returnOpen = Voltage(returnSense) > 1f;
            for (int i = 0; i < lanes.Length; i++)
            {
                var length = lanes[i].lengthMm;
                float longest = Mathf.Max(Mathf.Max(length[0], length[1]), Mathf.Max(length[2], length[3]));
                float skew = Mathf.Max(Mathf.Abs(length[0] - length[1]), Mathf.Abs(length[2] - length[3]));
                windows[i] = SparkLevel111SignalModel.Measure(i, swings[i], longest, skew,
                    clockJitter, bias, equalization[i], returnOpen && i == 2, returnDistanceMm);
            }
        }

        private void TickPattern()
        {
            patternAt = time + .13f;
            bool positive = pattern.Bursts % 2 == 0;
            DriveLanes(positive);
            float bias = Voltage(equalizerBias);
            hotBiasObserved |= bias < 1.25f && temperature >= 53f;
            peakTemperature = Mathf.Max(peakTemperature, temperature);
            for (int i = 0; i < lanes.Length; i++) swings[i] = MeasureSwing(lanes[i], positive);
            Measure();
            pattern.Compare(windows);
            if (pattern.Bursts < SparkLevel111Pattern.BurstCount) return;
            testing = false;
            SetContact(loadGate, false);
            Solve();
            captures++;
            patternEpoch = epoch;
            float projection = 0;
            foreach (var lane in windows) projection += lane.ProjectedErrors * .25f;
            evidence = $"Run {captures} · epoch {epoch} · {pattern.Errors}/{pattern.Bits:N0} observed errors\nModel projection: {projection:G3} errors per 10⁹ bits";
            benchState = pattern.Errors == 0 ? "PATTERN VERIFIED" : "PATTERN ERRORS";
            message = pattern.Errors == 0 ? "Four-lane pattern passed at hot load."
                : $"First mismatch on lane {pattern.FirstErrorLane}. Check the measured cause before rework.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Qualifies)
                return Reject("Pass the current four-lane test before starting the next fault trial.");
            activeFault = repaired;
            repairTarget = -1;
            reworked = faultWitnessed = hotBiasObserved = false;
            pattern.Reset();
            evidence = "";
            InvalidateClock();
            ConfigureFaultDevices();
            ReadGraph();
            Solve();
            benchState = "DIAGNOSTIC TRIAL";
            message = "Verify REFCLK. If it passes, commission, capture TDR and eye, calibrate, then run a hot-load pattern.";
            QueueCheckpoint();
            return true;
        }

        private void ConfigureFaultDevices()
        {
            if (activeFault == 0) SetContact(routes[returnRoute].barrel, !FaultLive(0));
            SetContact(clockNoiseP, FaultLive(1));
            SetContact(clockNoiseN, FaultLive(1));
            SetContact(thermalShunt, false);
        }

        private int ExpectedPart => activeFault == 1 ? clockPart : activeFault == 2 ? retimerPart : -1;

        public bool LocateFault()
        {
            bool measured = powered && (activeFault switch
            {
                0 => tdrEpoch == epoch && tdrLoss < 14f && eyeEpoch == epoch && patternEpoch == epoch && pattern.FirstErrorLane == 2,
                1 => ClockObserved && clockJitter >= 18f && clockNoiseP.IsConducting && clockNoiseN.IsConducting,
                2 => patternEpoch == epoch && pattern.Errors > 0 && hotBiasObserved && peakTemperature >= 53f,
                _ => false
            });
            if (!measured || reworked) return Reject("Measure the fault before selecting the repair.");
            faultWitnessed = true;
            repairTarget = ExpectedPart;
            if (activeFault == 0)
            {
                SelectRoute(returnRoute);
                PadView();
                plotMode = PlotMode.Tdr;
                message = $"The lane 2 return echo locates V37 at {tdrDistance:F1} mm. Isolate power and replate on L03.";
            }
            else
            {
                footprint = repairTarget;
                FocusPart();
                message = activeFault == 1 ? "U5 produces excess phase noise. Isolate power and restore the conditioner."
                    : "U8 loses equalizer bias under heat. Isolate power and rework the thermal path.";
            }
            QueueCheckpoint();
            return true;
        }

        public bool Repair()
        {
            if (stage != 4 || powered || !faultWitnessed || reworked || Reworking ||
                repairTarget != ExpectedPart || activeFault == 0 && (Selected != routes[returnRoute] || layer != Selected.layer))
                return Reject("Isolate the bench and select the diagnosed via on its entry layer or the diagnosed component.");
            if (activeFault == 2)
            {
                reworkUntil = time + 1.15f;
                benchState = "ISOLATED THERMAL REWORK";
                message = "Rebuilding the retimer thermal contact. Bench power remains isolated until the cycle finishes.";
                return true;
            }
            reworked = true;
            ConfigureFaultDevices();
            ReadGraph();
            Solve();
            BoardView();
            message = "Hardware path restored. Start with a fresh clock check and finish with a hot-load pattern.";
            QueueCheckpoint();
            return true;
        }

        public bool OpenW185()
        {
            if (stage != 5 || !armed || !Fresh || interrupted || !linked)
                return Reject("Wait for valid cadence and an enabled U19 workload permit.");
            requiredEpoch = epoch + 1;
            if (!RemoveWire(jumper)) return Reject("Could not open the native W185 wire.");
            Solve();
            return true;
        }

        public bool RestoreW185()
        {
            if (stage != 5 || !interrupted || linked) return Reject("First record the supervised physical W185 opening.");
            SelectRoute(routes.Length);
            if (!CreateWire(jumper)) return Reject("W185’s native terminals rejected the connection.");
            ReadGraph();
            Solve();
            message = "W185 restored. Acquire new four-lane commissioning and instrument evidence before resetting U19.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || epoch < requiredEpoch || !Fresh || !Inhibited ||
                Voltage(monitorSupply) < 3f) return Reject("Reconnect W185 and verify new measurements, pattern data and a fresh heartbeat.");
            safetyReset = true;
            requiredEpoch = epoch + 1;
            InvalidateDiagnostics();
            Solve();
            message = "U19 reset accepted. Repeat commissioning and all measurements for a new authorization.";
            return true;
        }

        private void ClearHeartbeat()
        {
            feedbackIntervals = 0;
            lastRise = -1f;
            highFeedback = heartbeatDrive = false;
            heartbeatAt = time;
            SetContact(heartbeat, false);
            SetContact(workload, false);
        }

        private void TickSupervisor()
        {
            if (Qualifies && time >= heartbeatAt)
            {
                heartbeatDrive = !heartbeatDrive;
                heartbeatAt = time + .15f;
                SetContact(heartbeat, heartbeatDrive);
                Solve();
                bool high = Voltage(jumper.to) > 2.8f;
                if (high && !highFeedback)
                {
                    float period = time - lastRise;
                    feedbackIntervals = lastRise >= 0 && period is >= .23f and <= .46f ? feedbackIntervals + 1 : 0;
                    lastRise = time;
                }
                highFeedback = high;
            }
            bool permit = Fresh && Voltage(monitorSupply) > 3f && (!interrupted || safetyReset && epoch >= requiredEpoch);
            if (workload.IsClosed != permit)
            {
                SetContact(workload, permit);
                Solve();
            }
            if (!permit || Voltage(protectedFeed) <= 3f || stage != 5) return;
            if (!interrupted) armed = true;
            else if (safetyReset && Award())
            {
                benchState = "SIGNAL INTEGRITY CERTIFIED";
                message = "Fresh four-lane evidence and W185 restored U19’s independent workload permit.";
            }
        }
    }
}
