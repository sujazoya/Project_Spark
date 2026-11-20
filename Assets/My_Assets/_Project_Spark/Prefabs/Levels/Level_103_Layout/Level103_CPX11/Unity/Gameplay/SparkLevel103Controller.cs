using System;
using ProjectSpark.VoltageDominion;
using UnityEngine;

namespace ProjectSpark.Gameplay.PowerCore
{
    public sealed partial class SparkLevel103Process
    {
        private readonly SparkDominionVrm model = new(1.0, 400000, .0025);
        private readonly float[] trace = new float[192];
        private int activeFault = -1, repaired, runNumber, requiredRun, intervalCount, traceCount;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset, lastHigh, localPulse;
        private bool calibrated, testing, qualified, protectedRun, startupRun;
        private float time, powerAt, nextPulse, lastRise = float.NegativeInfinity;
        private double accumulator, commandedLoad = 2, beginRun, nextTrace, minimum, maximum, peakHeat, ripple, spread, steadyError, minEfficiency;
        private double peakBias, peakThermalDelta, minimumPhase3;
        private int completedProfiles;
        private string evidence = "No calibrated capture";
        private string benchState = "BENCH ISOLATED";

        public SparkDominionVrm Model => model;
        public bool Calibrated => calibrated;
        public bool ReadyForCalibration => RailsReady && model.ConnectedPhases == 15 && model.PowerGood &&
            model.PhaseTemperature(3) - model.PhaseTemperature(0) < 15 && (activeFault < 0 || reworked);
        public bool Testing => testing;
        public bool Qualifies => qualified;
        public bool FaultWitnessed => faultWitnessed;
        public int RunNumber => runNumber;
        public int Repaired => repaired;
        public int ProfileCount { get; private set; }
        public double PeakPhase2Bias => peakBias;
        public double PeakPhase4Rise => peakThermalDelta;
        public double MinimumPhase3 => minimumPhase3;
        public string Evidence => evidence;
        public bool Fresh => linked && intervalCount >= 2 && time - lastRise < .54f;
        private bool FaultLive(int index) => activeFault == index && !reworked;
        private bool RailsReady => powered && TrialTopology && logicGate.IsConducting && auxGate.IsConducting &&
            Voltage(standby) is > 11.5f and < 12.5f && Voltage(aux33) is > 3.05f and < 3.5f &&
            Voltage(aux18) is > 1.66f and < 1.95f && Voltage(monitorSupply) > 3f;

        private void Solve() => manager.ElectricalSolver.SolveNow();

        public bool TogglePower()
        {
            if (powered)
            {
                IsolatePower();
                message = "Bench isolated. Wait for capacitor discharge before rework.";
                return true;
            }
            if (stage < 2 || !TrialTopology || !linked || !model.ResetProtection())
                return Reject("Complete the board and W117, then wait for a discharged, cool rail.");
            powerAt = time;
            powered = true;
            SetContact(bench, true);
            ConfigureFaultDevices();
            Solve();
            benchState = "INPUT PROTECTED · AUXILIARY STARTUP";
            message = "Wait for 3.3 V, 1.8 V and stable VCORE before calibrating.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            calibrated = false;
            InvalidateDiagnostics();
            ClearHeartbeat();
            SetContact(auxGate, false);
            SetContact(logicGate, false);
            SetContact(bench, false);
            Solve();
            benchState = "BENCH ISOLATED";
        }

        private void InvalidateBoot()
        {
            calibrated = false;
            InvalidateDiagnostics();
            ClearHeartbeat();
        }

        private void InvalidateDiagnostics()
        {
            testing = qualified = false;
            ProfileCount = 0;
            evidence = "No fresh calibrated capture";
        }

        private void ConfigureFaultDevices()
        {
            model.Phase2SenseOffset = FaultLive(0) ? .95 : 0;
            model.Phase4ExtraLoss = FaultLive(2) ? 2.0 : 0;
            var via = routes[phase3Route].barrel;
            if (FaultLive(1)) SetContact(via, false);
            else if (activeFault == 1 && reworked) SetContact(via, true);
        }

        private void TickController(float delta)
        {
            time += delta;
            if (powered && time - powerAt > .16f && Voltage(standby) > 11.5f)
            {
                if (!logicGate.IsClosed) { SetContact(logicGate, true); Solve(); }
                if (Voltage(aux33) > 3.05f && !auxGate.IsClosed) { SetContact(auxGate, true); Solve(); }
            }
            bool stableRails = RailsReady;
            int phaseMask = 0;
            if (stableRails)
                for (int i = 0; i < drivers.Length; i++)
                    if (Voltage(drivers[i]) > 2.65f) phaseMask |= 1 << i;
            model.ConnectedPhases = phaseMask;
            double inputVoltage = Voltage(standby);
            accumulator += Math.Min(delta, .054f);
            while (accumulator >= SparkDominionVrm.StepSeconds)
            {
                accumulator -= SparkDominionVrm.StepSeconds;
                double age = model.Time - beginRun;
                var amps = testing && !startupRun ? ProfileLoad(age) : 2;
                double limit = 450 * SparkDominionVrm.StepSeconds;
                commandedLoad += Math.Clamp(amps - commandedLoad, -limit, limit);
                model.Step(inputVoltage, stableRails && phaseMask != 0, commandedLoad);
                if (testing) Sample(age);
            }

            if (powered)
                benchState = model.Trip != SparkDominionVrm.Protection.None ? $"PROTECTION · {model.Trip.ToString().ToUpperInvariant()}" :
                    !stableRails ? "INPUT PROTECTED · AUXILIARY STARTUP" :
                    model.PowerGood ? "FOUR-PHASE VCORE REGULATING" : "MEASURING VCORE AND PHASE RESPONSE";
            UpdateSupervisor();
        }

        private static int ProfileLoad(double age)
        {
            if (age < .30) return 2;
            if (age < .85) return 10;
            if (age < 1.55) return 20;
            if (age < 1.90) return 2;
            return age < 2.18 ? 20 : 2;
        }

        public bool Calibrate()
        {
            if (!ReadyForCalibration || testing)
                return Reject("Wait for all four phase paths and a stable VCORE before calibration.");
            calibrated = true;
            InvalidateDiagnostics();
            message = stage == 2 ? "Probes calibrated. Capture the 2 A commissioning run." :
                "Probes calibrated. Run the 2, 10, 20 A and transient profiles.";
            return true;
        }

        public bool RunProfiles()
        {
            bool faultTrial = activeFault >= 0 && !reworked;
            if (testing || stage < 2 || !RailsReady || (!faultTrial && (!calibrated || !model.PowerGood)))
                return Reject("Settle all rails and calibrate before running a fresh capture.");
            if (stage == 5 && safetyReset && Inhibited)
                return Reject("Wait for U11 to enable the protected CPU diagnostic load.");
            testing = true;
            qualified = false;
            startupRun = stage == 2;
            protectedRun = stage == 5 && safetyReset && !Inhibited;
            beginRun = model.Time;
            nextTrace = 0;
            traceCount = completedProfiles = ProfileCount = 0;
            minimum = 2;
            maximum = peakHeat = ripple = spread = steadyError = peakBias = peakThermalDelta = 0;
            minimumPhase3 = 16;
            minEfficiency = 1;
            message = startupRun ? "Capturing low-load regulation and independent phase readings." :
                protectedRun ? "Capturing the protected CPU load after U11 reset." :
                    "Capturing idle, nominal, peak and transient demand.";
            return true;
        }

        private void Sample(double age)
        {
            if (protectedRun && Inhibited)
            {
                InvalidateDiagnostics();
                message = "Protected run canceled because the safety permit was withdrawn.";
                return;
            }
            minimum = Math.Min(minimum, model.Voltage);
            maximum = Math.Max(maximum, model.Voltage);
            ripple = Math.Max(ripple, model.Ripple);
            spread = Math.Max(spread, model.CurrentSpread);
            peakHeat = Math.Max(peakHeat, model.Temperature);
            peakBias = Math.Max(peakBias, Math.Abs(model.ControllerCurrent(1) - model.PhaseCurrent(1)));
            peakThermalDelta = Math.Max(peakThermalDelta, model.PhaseTemperature(3) - model.PhaseTemperature(0));
            if (age >= .18) minimumPhase3 = Math.Min(minimumPhase3, model.PhaseCurrent(2));
            if (age >= nextTrace && traceCount < trace.Length)
            {
                trace[traceCount++] = (float)model.Voltage;
                nextTrace += startupRun ? .006 : .016;
            }

            int window = startupRun ? age >= .28 ? 1 : 0 :
                age is >= .22 and < .30 ? 1 : age is >= .73 and < .85 ? 2 :
                age is >= 1.43 and < 1.55 ? 4 : age >= 2.42 ? 8 : 0;
            if (window != 0)
            {
                completedProfiles |= window;
                steadyError = Math.Max(steadyError, Math.Abs(model.Voltage - model.TargetVoltage));
                minEfficiency = Math.Min(minEfficiency, model.Efficiency);
            }
            if (age >= (startupRun ? .55 : 2.55)) FinishCapture();
        }

        private void FinishCapture()
        {
            testing = false;
            runNumber++;
            ProfileCount = ((completedProfiles & 1) != 0 ? 1 : 0) + ((completedProfiles & 2) != 0 ? 1 : 0) +
                ((completedProfiles & 4) != 0 ? 1 : 0) + ((completedProfiles & 8) != 0 ? 1 : 0);
            evidence = $"Run {runNumber} · {minimum:F3}–{maximum:F3} V · {ripple * 1000:F2} mVpp\n" +
                $"Phase spread {spread:F2} A · P4 rise {peakThermalDelta:F0} °C\n" +
                $"Peak {peakHeat:F0} °C · efficiency {minEfficiency:P0} · profiles {ProfileCount}/{(startupRun ? 1 : 4)}";
            if (FaultLive(0) && peakBias > .5)
            {
                faultWitnessed = true;
                message = $"F1 · Phase 2 senses {peakBias:F2} A above the independent probe. Locate R2.";
            }
            else if (FaultLive(1) && (model.ConnectedPhases & 4) == 0 && minimumPhase3 < .08)
            {
                faultWitnessed = true;
                message = "F2 · V017 open. Phase 3 contributes zero current. Locate and isolate.";
            }
            else if (FaultLive(2) && peakThermalDelta > 18)
            {
                faultWitnessed = true;
                message = $"F3 · Phase 4 is {peakThermalDelta:F0} °C hotter than Phase 1. Locate Q7.";
            }
            if (faultWitnessed) QueueCheckpoint();

            qualified = calibrated && model.ConnectedPhases == 15 && model.Trip == SparkDominionVrm.Protection.None &&
                completedProfiles == (startupRun ? 1 : 15) && minimum > .68 && maximum < 1.24 &&
                steadyError < .04 && ripple < .025 && spread < .30 && peakHeat < 74 &&
                peakBias < .15 && peakThermalDelta < 18 && minEfficiency > .72;
            if (!qualified)
            {
                if (!faultWitnessed) message = "The capture exceeded a voltage, phase balance or thermal limit.";
                return;
            }
            if (stage is 2 or 3) Award();
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                reworked = faultWitnessed = false;
                if (repaired == 3) Award();
                QueueCheckpoint();
            }
            if (stage == 3) message = "Four phases commissioned. Run the full CPU workload set.";
            else if (stage == 4) message = "The workload set qualified. Start the next power fault trial.";
            else if (stage == 5 && !interrupted) message = "Wait for fresh U11 authorization, then open W117 under power.";
            else if (stage == 5 && !safetyReset) message = "Fresh pre-reset evidence qualified. Reset U11, then take a new CPU capture.";
            if (stage == 5 && interrupted && safetyReset && protectedRun && runNumber >= requiredRun &&
                Fresh && !Inhibited && Voltage(protectedFeed) > 3f && Award())
                message = "The Power Core certified. W117 recovery and fresh protected load tests verified.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !qualified || !RailsReady)
                return Reject("Qualify a complete workload capture before inducing another fault.");
            activeFault = repaired;
            reworked = faultWitnessed = false;
            ConfigureFaultDevices();
            if (activeFault == 1) ReadGraph();
            calibrated = false;
            InvalidateDiagnostics();
            message = "Fault applied under diagnostic load. Run all profiles to isolate the failure.";
            QueueCheckpoint();
            return true;
        }

        public bool LocateFault()
        {
            if (!faultWitnessed || activeFault < 0) return Reject("Capture the failure before selecting a repair target.");
            if (activeFault == 1) { SelectRoute(phase3Route); PadView(); }
            else { SelectFootprint(activeFault == 0 ? sensePart : thermalPart); FocusPart(); }
            return true;
        }

        public bool Repair()
        {
            if (activeFault < 0 || !faultWitnessed || reworked || powered || model.Voltage > .08)
                return Reject("Capture the failure and discharge the isolated VCORE before rework.");
            if (activeFault == 1 ? selectedRoute != phase3Route : footprint != (activeFault == 0 ? sensePart : thermalPart))
                return Reject("Select the correct component or layer span before rework.");
            reworked = true;
            ConfigureFaultDevices();
            ReadGraph();
            InvalidateDiagnostics();
            QueueCheckpoint();
            BoardView();
            message = "Rework applied. Cool the board, power up, calibrate and run all profiles.";
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
            bool trustworthy = RailsReady && calibrated && model.ConnectedPhases == 15 &&
                model.Trip == SparkDominionVrm.Protection.None && model.Voltage is > .68 and < 1.24;
            if (!trustworthy)
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
            SetContact(workload, stage >= 3 && Fresh && (!interrupted || safetyReset));
            if (stage == 5 && !interrupted && qualified && !Inhibited) armed = true;
        }

        public bool OpenW117()
        {
            if (stage != 5 || !powered || !armed || !linked || Inhibited || testing)
                return Reject("Open W117 only with all repairs certified and the protected load enabled.");
            return RemoveWire(jumper);
        }

        public bool RestoreW117()
        {
            SelectRoute(routes.Length);
            return ConnectPads();
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !qualified || !calibrated || !Fresh || !model.PowerGood || !Construction)
                return Reject("Restore W117 and capture a fresh qualification before resetting U11.");
            safetyReset = true;
            requiredRun = runNumber + 1;
            InvalidateDiagnostics();
            message = "U11 reset accepted. Run all profiles with the protected CPU authorized.";
            return true;
        }
    }
}
