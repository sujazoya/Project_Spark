using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel98Process
    {
        private static readonly string[] FaultNames = { "A · POLYMER ESR", "B · OPEN RETURN VIA", "C · KELVIN SENSE" };
        private const float SampleInterval = .04f;
        private const int CapturesPerRun = 3;

        private int activeFault = -1, repaired, phase, transitions, samples;
        private bool faultWitnessed, reworked, armed, interrupted, resetAuthorized, lastHigh;
        private bool calibrated, capturing;
        private float time, bootAt, nextPulse, nextSolve, dischargingUntil, lastRise = float.NegativeInfinity, phaseAge, sampleAge;
        private float baseline, minimum, maximum, peakCurrent, maximumEsr, peakReturn, kelvinMismatch, endVoltage;
        private float temperature = 25f;
        private string evidence = "Scope idle";
        private string benchState = "BENCH ISOLATED";

        public bool Busy => capturing;
        public bool Calibrated => calibrated;
        public bool FaultWitnessed => faultWitnessed;
        public int Repaired => repaired;
        public string Evidence => evidence;
        public float LoadCurrent => Mathf.Abs(load.Current);
        public float LoadVoltage => Measure(actualRail, loadReturn);
        public float GroundOffset => Voltage(loadReturn);
        public float SenseVoltage => Measure(sense, loadReturn);
        public bool SenseValid => HasWire(routes[senseRoute]) && Mathf.Abs(SenseVoltage - LoadVoltage) < profile.maximumKelvinError;
        public bool Fresh => linked && transitions >= 2 && time - lastRise < .58f;
        private bool GoodRails => Voltage(standby) > 11f && Voltage(mainRail) is > 4.80f and < 5.20f &&
            Voltage(monitorRail) is > 3.12f and < 3.48f && LoadVoltage is > 1.25f and < 1.96f;
        private bool SuppliesReady => linked && powered && TrialTopology && GoodRails;
        private bool FaultTrial => stage == 4 && activeFault >= 0 && !reworked;
        private bool Withheld => interrupted && !resetAuthorized;

        private float Measure(SparkTerminal positive, SparkTerminal negative)
        {
            var result = manager.ElectricalSolver.LatestSolveResult;
            return result != null && result.TryGetVoltage(positive, negative, out float voltage) ? voltage : 0f;
        }

        public bool TogglePower()
        {
            if (powered)
            {
                IsolatePower();
                message = "Power isolated. Stored charge discharges through the bleed circuit.";
                return true;
            }
            if (stage < 2 || !TrialTopology || !linked)
                return Reject("Finish assembly, routing and W075 before applying the bench supply.");
            powered = true;
            bootAt = time;
            SetContact(bench, true);
            Solve();
            message = "12 V input connected. Wait for the regulated rails and fresh U8 pulses.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            dischargingUntil = time + 2f;
            InvalidateEvidence();
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                SetContact(workload, false);
                SetContact(coreEnable, false);
                SetContact(monitorEnable, false);
                SetContact(mainEnable, false);
                SetContact(bench, false);
            }
            Solve();
        }

        private void InvalidateEvidence()
        {
            if (capturing) load.SetResistance(profile.idleOhms);
            calibrated = false;
            capturing = false;
            resetAuthorized = false;
            transitions = 0;
            lastHigh = false;
            lastRise = float.NegativeInfinity;
            SetContact(heartbeat, false);
        }

        private void Solve()
        {
            // A transient sample must advance the native capacitor history even when the graph is unchanged.
            var solver = manager.ElectricalSolver;
            solver.MarkDirty();
            solver.SolveNow();
        }

        private void TickBench(float delta)
        {
            time += delta;
            if (!powered)
            {
                benchState = "BENCH ISOLATED";
                if (time < dischargingUntil && time >= nextSolve)
                {
                    nextSolve = time + .10f;
                    Solve();
                }
                return;
            }
            float elapsed = time - bootAt;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                bool inputReady = Voltage(standby) > 11f;
                SetContact(mainEnable, elapsed > .10f && inputReady);
                SetContact(monitorEnable, elapsed > .24f && inputReady && Voltage(mainRail) > 4.8f);
                SetContact(coreEnable, elapsed > .38f && inputReady && Voltage(monitorRail) > 3.12f);
            }
            if (!capturing && time >= nextSolve)
            {
                nextSolve = time + SampleInterval;
                Solve();
            }
            bool ready = SuppliesReady;
            if (ready && time >= nextPulse)
            {
                nextPulse = time + .17f;
                SetContact(heartbeat, !heartbeat.IsClosed);
                Solve();
            }
            else if (!ready)
            {
                SetContact(heartbeat, false);
                transitions = 0;
                lastRise = float.NegativeInfinity;
            }
            bool high = ready && Voltage(jumper.to) > 2.65f;
            if (high && !lastHigh) { transitions++; lastRise = time; }
            lastHigh = high;
            SetContact(workload, ready && Fresh && !Withheld);
            if (stage == 5 && !Inhibited && !interrupted && calibrated) armed = true;
            benchState = !linked ? "W075 OPEN · U9 LATCHED" : Withheld ? "U9 LATCHED · LOAD ISOLATED" :
                capturing ? "SCOPE CAPTURE IN PROGRESS" : Fresh ? calibrated ? "CALIBRATED · READY" : "RAILS READY · CALIBRATION DUE" : "QUALIFYING RAILS";
            temperature = Mathf.MoveTowards(temperature,
                Mathf.Min(92f, 25f + LoadCurrent * 1.6f + Mathf.Abs(bulkEsr.Power) * 24f), delta * 9f);
            if (!capturing) return;
            if (!ready || !Fresh || Inhibited)
            {
                capturing = false;
                load.SetResistance(profile.idleOhms);
                message = "Capture aborted because the load permit dropped.";
                return;
            }
            AdvanceCapture(delta);
        }

        public bool Calibrate()
        {
            if (stage < 2 || !SuppliesReady || !Fresh || !Construction || !SenseValid || capturing ||
                (!Withheld && LoadCurrent is < .40f or > .61f) || Mathf.Abs(LoadVoltage - 1.8f) > profile.maximumIdleError)
                return Reject("Wait for healthy rails, a fresh W075 signal and agreeing Kelvin/current readings.");
            calibrated = true;
            if (stage == 2) Award();
            message = Withheld ? "Calibration is fresh. Authorize U9 reset before running new profiles." :
                "Probe comparison passed. Run rising, falling and repetitive load steps.";
            return true;
        }

        public bool RunProfiles()
        {
            if (capturing || stage is < 3 or > 5 || !SuppliesReady || !Fresh || Inhibited || (!calibrated && !FaultTrial) ||
                faultWitnessed && FaultTrial)
                return Reject("Use a fresh calibration, or run the active fault trial for diagnosis.");
            baseline = LoadVoltage;
            minimum = maximum = baseline;
            peakCurrent = maximumEsr = peakReturn = kelvinMismatch = 0f;
            phase = samples = 0;
            sampleAge = phaseAge = 0f;
            scope.Clear();
            capturing = true;
            evidence = "Capturing the 0.5 A baseline…";
            return true;
        }

        private void AdvanceCapture(float delta)
        {
            phaseAge += delta;
            sampleAge += delta;
            if (sampleAge >= SampleInterval)
            {
                sampleAge %= SampleInterval;
                Solve();
                float v = LoadVoltage, current = LoadCurrent;
                minimum = Mathf.Min(minimum, v);
                maximum = Mathf.Max(maximum, v);
                peakCurrent = Mathf.Max(peakCurrent, current);
                peakReturn = Mathf.Max(peakReturn, Mathf.Abs(GroundOffset));
                kelvinMismatch = Mathf.Max(kelvinMismatch, Mathf.Abs(v - SenseVoltage));
                maximumEsr = Mathf.Max(maximumEsr, Mathf.Abs(bulkEsr.Voltage));
                endVoltage = v;
                samples++;
                scope.Add(v, current);
            }
            bool high = phase % 2 == 1;
            if (phaseAge < (high ? profile.pulseDuration : profile.restDuration)) return;
            phaseAge = 0f;
            phase++;
            if (phase > CapturesPerRun * 2) { CompleteCapture(); return; }
            high = phase % 2 == 1;
            load.SetResistance(high ? profile.peakOhms : profile.idleOhms);
            float draw = high ? 6.4f : 1.8f;
            inputDraw.SetResistance(144f / draw);
            evidence = high ? $"Pulse {(phase + 1) / 2}/{CapturesPerRun} · peak demand" : "Falling edge · recovery sample";
        }

        private void CompleteCapture()
        {
            capturing = false;
            float droop = Mathf.Max(0, baseline - minimum);
            float overshoot = Mathf.Max(0, maximum - baseline);
            evidence = $"Min {minimum:F3} V · droop {droop * 1000:F0} mV · overshoot {overshoot * 1000:F0} mV\n" +
                $"Peak {peakCurrent:F2} A · return {peakReturn * 1000:F0} mV · ESR {maximumEsr * 1000:F0} mV";
            if (FaultTrial)
            {
                faultWitnessed = activeFault switch
                {
                    0 => maximumEsr > .041f,
                    1 => peakReturn > .13f,
                    2 => kelvinMismatch > .30f,
                    _ => false
                };
                if (faultWitnessed) QueueCheckpoint();
                message = faultWitnessed ? $"{FaultNames[activeFault]} measured. Isolate power, locate the affected contact and repair." :
                    "Fault evidence was inconclusive. Repeat the capture.";
                return;
            }
            bool healthy = samples >= 24 && minimum >= profile.minimumLoadVolts && maximum <= profile.maximumLoadVolts &&
                peakCurrent >= profile.minimumPeakAmps && peakReturn < profile.maximumHealthyGroundOffset &&
                kelvinMismatch < profile.maximumKelvinError && Mathf.Abs(endVoltage - baseline) < .055f && calibrated;
            if (!healthy) { message = "The scope did not qualify the load profile. Review its voltage and current limits."; return; }
            if (stage == 3) Award();
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                faultWitnessed = reworked = false;
                if (repaired == 3) Award();
                QueueCheckpoint();
            }
            else if (stage == 5 && interrupted && resetAuthorized && Fresh && !Inhibited) Award();
            message = stage switch
            {
                4 => "Profiles qualified. Start the next fault trial.",
                5 => "Profiles qualified. Open W075 while U9 is enabling the load.",
                6 => "Transient Shield certified. Fresh scope and W075 recovery evidence accepted.",
                _ => "Load step capture qualified."
            };
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !calibrated || capturing || Inhibited)
                return Reject("Finish a calibrated profile and the current repair before starting a new fault trial.");
            activeFault = repaired;
            faultWitnessed = reworked = false;
            switch (activeFault)
            {
                case 0:
                    bulkEsr.SetResistance(profile.wornEsr);
                    break;
                case 1:
                    SetContact(routes[groundRoute].barrel, false);
                    ReadGraph();
                    break;
                case 2:
                    if (!RemoveWire(routes[senseRoute]))
                    {
                        activeFault = -1;
                        return Reject("The native Kelvin copper path could not be opened.");
                    }
                    break;
            }
            QueueCheckpoint();
            message = $"{FaultNames[activeFault]} trial active. Wait for U8 pulses, then capture the symptom.";
            return true;
        }

        public bool LocateFault()
        {
            if (!FaultTrial || !faultWitnessed) return Reject("Measure the fault before selecting a repair target.");
            if (activeFault == 0) { SelectFootprint(capacitorPart); FocusPart(); }
            else { SelectRoute(activeFault == 1 ? groundRoute : senseRoute); PadView(); }
            return true;
        }

        public bool Repair()
        {
            if (!FaultTrial || powered || !faultWitnessed) return Reject("Capture the fault and isolate bench power before repair.");
            if (activeFault == 0 && footprint != capacitorPart ||
                activeFault != 0 && selectedRoute != (activeFault == 1 ? groundRoute : senseRoute))
                return Reject("Select the diagnosed capacitor, via or Kelvin trace first.");
            if (activeFault == 0) bulkEsr.SetResistance(profile.goodEsr);
            if (activeFault == 1) SetContact(routes[groundRoute].barrel, true);
            if (activeFault == 2 && !CreateWire(routes[senseRoute])) return Reject("Could not restore the Kelvin edge.");
            reworked = true;
            ReadGraph();
            QueueCheckpoint();
            BoardView();
            message = "Repair applied. Power up, recalibrate and qualify a fresh load profile.";
            return true;
        }

        public bool OpenW075()
        {
            if (stage != 5 || !powered || !armed || !linked || Inhibited || capturing)
                return Reject("Qualify the board and wait for live protected load before opening W075.");
            return RemoveWire(jumper);
        }

        public bool RestoreW075()
        {
            SelectRoute(routes.Length);
            return ConnectPads();
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || !calibrated || !Fresh || !Construction || !powered)
                return Reject("Reconnect W075, collect fresh calibration and wait for U8 pulses.");
            resetAuthorized = true;
            message = "U9 reset authorized. Run all three profiles to recertify.";
            return true;
        }
    }
}
