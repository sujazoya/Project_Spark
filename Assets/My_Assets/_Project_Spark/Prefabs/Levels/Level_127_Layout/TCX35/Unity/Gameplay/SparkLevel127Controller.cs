using UnityEngine;
using Thermal = ProjectSpark.Gameplay.ThermalAegis.SparkLevel127Thermal;

namespace ProjectSpark.Gameplay.ThermalAegis
{
    public sealed partial class SparkLevel127Process
    {
        public SparkTerminal rail, monitorSupply, telemetryPin, heaterSupply, heaterBackup, heaterPin, referencePin;
        public SparkTerminal baseHeatPin, pulsePin, radiatorSupply, radiatorBackup, testRadiatorPin, protectedFeed;
        public SparkTerminal[] logicPins, sensorPins, pumpPins, radiatorPins;
        public SparkSwitch bench, monitorGate, telemetryGate, heaterPower, heaterBackupGate, heaterGate, referenceGate;
        public SparkSwitch baseHeatGate, pulseGate, radiatorPower, radiatorBackupPower, testRadiatorGate, heartbeat, workload;
        public SparkSwitch pumpAContact, outletContact;
        public SparkSwitch[] logicGates, sensorGates, pumpCommands;
        public SparkResistor referenceLoad, baseHeat, pulseHeat, heaterLoad, testRadiatorLoad;
        public SparkResistor[] pumpLoads, radiatorLoads;
        public Renderer[] sensorMarkers;
        public GameObject assemblyArt;
        public int pumpPart, sensePart, outletPart;

        private const float ThermalWattsPerBenchWatt = 900f;
        public enum ThermalFailure { None, PumpOpen, SensorReference, StaleOutlet, Endurance }
        private readonly Thermal thermal = new();
        private Thermal.Replay replay, protectedReplay;
        private float time, nextSample, nextHeartbeat, lastRise = -1f, watchdogUntil;
        private float measuredA, measuredB, measuredHeater;
        private bool driverHeartbeat, highFeedback, checkedInputs, watchdogObserved;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset, showSensors, faultEnergized, protectedRequest;
        private int epoch, qualifiedEpoch = -1, measurementEpoch = -1, calibrationEpoch = -1, runEpoch = -1, minimumEpoch, feedbackIntervals;
        private int activeFault = -1, repaired, repairTarget = -1, spare, verifiedPermits;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool RailsReady => powered && Live(rail) && Live(monitorSupply) && Live(telemetryPin) &&
            Live(heaterSupply) && Live(heaterBackup) && Live(logicPins[0]) && Live(logicPins[1]);
        public bool Measured => measurementEpoch == epoch;
        public bool Calibrated => calibrationEpoch == epoch;
        public bool Running => replay != null && !replay.Complete;
        public bool ProtectedBusy => protectedReplay != null;
        public bool Qualifies => qualifiedEpoch == epoch && Measured && Calibrated && checkedInputs &&
            RailsReady && SensorCount == 12 && ReferenceGain is > .96f and < 1.04f &&
            Thermal.Flow(PumpCurrent(0), PumpCurrent(1)) > .30f;
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .49f;
        public bool WatchdogActive => time < watchdogUntil;
        public bool WatchdogObserved => watchdogObserved;
        public int SensorCount { get; private set; }
        public float ReferenceGain => Current(referenceLoad) / .05f;
        public float PumpCurrent(int channel) => Current(pumpLoads[channel]);
        public float MeasuredPumpA => measuredA;
        public float MeasuredPumpB => measuredB;
        public float HeaterWatts => BenchWatts(heaterPin, heaterLoad);
        public float RadiatorCurrent => Current(radiatorLoads[0]) + Current(radiatorLoads[1]);
        public int Accepted => replay?.Good ?? 0;
        public int Sent => replay?.Sent ?? 0;
        public int HeaterSamples => replay?.HeaterSamples ?? 0;
        public int BackupSamples => replay?.BackupSamples ?? 0;
        public int RestoredSamples => replay?.RecoveredSamples ?? 0;
        public int PumpLoss => replay?.PumpLoss ?? 0;
        public int DriftSamples => replay?.Drift ?? 0;
        public int StaleSamples => replay?.Stale ?? 0;
        public float PeakTemperature => replay?.Peak ?? 0f;
        public float MinimumTemperature => replay?.Minimum ?? 0f;
        public float FinalTemperature => replay?.Core ?? 0f;
        public float BalanceError => replay?.Residual ?? 0f;
        public float MaxDisagreement => replay?.MaxDisagreement ?? 0f;
        public float PeakSensorAge => replay?.PeakAge ?? 0f;
        public int Repaired => repaired;
        public int Epoch => epoch;
        public int ProtectedDelivered { get; private set; }
        public int ProtectedBackupGates { get; private set; }
        public int ProtectedHeatSamples { get; private set; }
        public int UndeliveredAtTrip { get; private set; }
        public ThermalFailure Failure { get; private set; }
        public string Evidence => evidence;

        private int ExpectedPart => activeFault switch { 0 => pumpPart, 1 => sensePart, 2 => outletPart, _ => -1 };
        private bool FaultLive(int index) => activeFault == index && !reworked && faultEnergized;
        private bool Live(SparkTerminal pin) => Voltage(pin) > 4.55f;
        private static float Current(SparkResistor load) => Mathf.Abs(load.Current);
        private float BenchWatts(SparkTerminal pin, SparkResistor load) =>
            Mathf.Max(0f, Voltage(pin)) * Current(load) * ThermalWattsPerBenchWatt;

        public bool TogglePower()
        {
            if (stage < 2 || stage >= 6 || !Construction || !linked && !interrupted)
                return Reject("Complete native continuity and W465 before enabling the thermal bench.");
            if (powered) { IsolatePower(); return true; }
            powered = true;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                foreach (var contact in new[] { bench, monitorGate, telemetryGate, heaterPower, heaterBackupGate,
                    referenceGate, baseHeatGate, radiatorPower, radiatorBackupPower })
                    SetContact(contact, true);
                foreach (var gate in logicGates) SetContact(gate, true);
                foreach (var gate in sensorGates) SetContact(gate, true);
                SetContact(pumpAContact, !FaultLive(0));
                referenceLoad.SetResistance(FaultLive(1) ? 68f : 100f);
                SetContact(outletContact, !FaultLive(2));
            }
            ApplyPump(false);
            ReadSensors();
            message = "Measure both pump currents, survival heat and the sensor reference.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                foreach (var contact in new[] { bench, monitorGate, telemetryGate, heaterPower, heaterBackupGate, heaterGate,
                    referenceGate, baseHeatGate, pulseGate, radiatorPower, radiatorBackupPower, testRadiatorGate, heartbeat, workload })
                    SetContact(contact, false);
                foreach (var gate in logicGates) SetContact(gate, false);
                foreach (var gate in sensorGates) SetContact(gate, false);
                foreach (var gate in pumpCommands) SetContact(gate, false);
                InvalidateDiagnostics();
            }
            ReadSensors();
            message = "Bench isolated. Select the diagnosed footprint and its matching spare.";
        }

        private void ApplyPump(bool backup)
        {
            bool fallback = !backup && !pumpAContact.IsConducting;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                SetContact(pumpCommands[0], powered && !backup);
                SetContact(pumpCommands[1], powered && (backup || fallback));
            }
            if (!backup && powered && PumpCurrent(0) < .035f)
                SetContact(pumpCommands[1], true);
        }

        private void ReadSensors()
        {
            SensorCount = 0;
            if (!powered) return;
            foreach (var pin in sensorPins)
                if (Live(pin)) SensorCount++;
        }

        public bool MeasureBench()
        {
            if (!RailsReady || Running || ProtectedBusy)
                return Reject("Power the bench and stop the trial before measuring.");
            ApplyPump(false);
            measuredA = PumpCurrent(0);
            ApplyPump(true);
            measuredB = PumpCurrent(1);
            ApplyPump(false);
            SetContact(heaterGate, true);
            measuredHeater = HeaterWatts;
            SetContact(heaterGate, false);
            ReadSensors();
            measurementEpoch = epoch;
            evidence = $"Loop A {measuredA * 1000f:F1} mA · loop B {measuredB * 1000f:F1} mA · heater {measuredHeater:F1} W\n" +
                $"{SensorCount}/12 energized sensors · reference gain {ReferenceGain:F3}";
            bool valid = measuredA is > .045f and < .055f && measuredB is > .036f and < .044f &&
                measuredHeater > 38f && SensorCount == 12 && ReferenceGain is > .96f and < 1.04f;
            message = valid ? "Pump currents and sensor excitation verified. Commission the thermal system." :
                "A reading is out of range. Use the endurance trial to capture the fault.";
            return valid;
        }

        public bool Calibrate()
        {
            if (!RailsReady || !Measured || Running || ProtectedBusy || measuredA < .045f || measuredB < .036f ||
                measuredHeater < 38f || SensorCount != 12 || ReferenceGain is < .96f or > 1.04f ||
                !Live(radiatorSupply) || !Live(radiatorBackup))
                return Reject("Measure both pumps, survival heat, the sensor array and independent references first.");
            calibrationEpoch = epoch;
            qualifiedEpoch = -1;
            checkedInputs = false;
            if (stage == 2) Award();
            evidence = "Two-second thermal samples · 15–65 °C operating band · backup flow 65% of nominal.";
            message = "Test stale-sensor and no-flow guards, then run thermal endurance.";
            return true;
        }

        public bool CheckGuardRejection()
        {
            if (!Calibrated || !Measured || Running || ProtectedBusy)
                return Reject("Commission the thermal system before challenging protection.");
            var check = new Thermal.Replay { Core = 40f };
            bool positive = Thermal.RequestRadiator(check, 12, 1f, 1.1f);
            checkedInputs = positive && !Thermal.RequestRadiator(check, 11, 1f, 1.1f) &&
                !Thermal.RequestRadiator(check, 12, 1.47f, 1.1f) && !Thermal.RequestRadiator(check, 12, 1f, 0f);
            thermal.Advance(check, new Thermal.Inputs(float.NaN, .04f, 90f, 0f, 0f, 1f, 12));
            checkedInputs &= check.Invalid && !Thermal.RequestRadiator(check, 12, 1f, 1.1f);
            if (!checkedInputs) return Reject("The thermal fixture failed an input guard.");
            evidence = "Stale data, reference disagreement, lost flow and invalid data hold both radiators.";
            message = "Protection guards passed. Start thermal endurance.";
            return true;
        }

        public bool RunThermal()
        {
            if (stage is < 3 or > 5 || !RailsReady || !linked || !Calibrated || !Measured || !checkedInputs ||
                Running || ProtectedBusy || WatchdogActive)
                return Reject("Measure, commission and exercise the protection guards before running endurance.");
            if (activeFault >= 0 && !reworked) EnergizeFault();
            replay = new Thermal.Replay();
            qualifiedEpoch = runEpoch = -1;
            Failure = ThermalFailure.None;
            nextSample = time;
            scope?.ResetTrace();
            message = "Sampling cold start, heat pulse, loop-B takeover and primary recovery.";
            return true;
        }

        private void TickController(float dt)
        {
            time += dt;
            ReadSensors();
            if (stage is >= 2 and <= 6) UpdateSupervisor();
            if (time < nextSample || !Running && !ProtectedBusy) return;
            bool protect = ProtectedBusy;
            var run = protect ? protectedReplay : replay;
            if (protect && !Fresh)
            {
                AbortProtected();
                message = "Trusted readiness expired. Requalify before enabling the radiators.";
                return;
            }

            var phase = Thermal.PhaseAt(run.Sent);
            ApplyPump(phase == Thermal.Phase.Backup);
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                SetContact(pulseGate, phase is Thermal.Phase.Heat or Thermal.Phase.Backup);
                SetContact(heaterGate, Thermal.RequestHeater(run));
                if (FaultLive(2)) SetContact(outletContact, !Thermal.OutletDrop(run.Sent));
            }
            ReadSensors();
            float pumpA = PumpCurrent(0), pumpB = PumpCurrent(1);
            bool request = Thermal.RequestRadiator(run, SensorCount, ReferenceGain, Thermal.Flow(pumpA, pumpB));
            if (protect)
            {
                protectedRequest = request;
                SetContact(workload, Fresh && protectedRequest);
            }
            else SetContact(testRadiatorGate, request);

            float heat = BenchWatts(baseHeatPin, baseHeat) + BenchWatts(pulsePin, pulseHeat);
            float radiator = protect ? RadiatorCurrent : Current(testRadiatorLoad);
            float survival = HeaterWatts;
            thermal.Advance(run, new Thermal.Inputs(pumpA, pumpB, heat, survival, radiator, ReferenceGain, SensorCount));

            nextSample = time + (protect ? !interrupted && phase == Thermal.Phase.Backup ? .15f : .051f : .028f);
            var sample = run.Last;
            scope?.Record(Mathf.Clamp01(sample.Core / 65f), Mathf.Clamp01(sample.Coolant / 65f),
                Mathf.Clamp01(sample.Flow / 1.45f), Mathf.Clamp01(survival / 90f));
            if (!protect)
            {
                if (run.Complete) FinishThermal(run);
                return;
            }

            bool result = request ? !Inhibited && Live(radiatorPins[0]) && Live(radiatorPins[1]) && radiator > .021f :
                Inhibited && !Live(radiatorPins[0]) && !Live(radiatorPins[1]) && radiator < .001f;
            if (result)
            {
                verifiedPermits++;
                if (phase == Thermal.Phase.Backup && request) ProtectedBackupGates++;
                if (survival > 35f) ProtectedHeatSamples++;
            }
            if (!interrupted && phase == Thermal.Phase.Backup && ProtectedBackupGates == 1)
                message = "Loop B and both radiators are active. Open W465 to cut the radiator outputs.";
            ProtectedDelivered = run.Sent;
            if (run.Complete) FinishProtected(run);
        }

        private void FinishThermal(Thermal.Replay report)
        {
            runEpoch = epoch;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                SetContact(testRadiatorGate, false);
                SetContact(pulseGate, false);
                SetContact(heaterGate, false);
                if (FaultLive(2)) SetContact(outletContact, false);
            }
            ReadSensors();
            Failure = report.PumpLoss > 0 ? ThermalFailure.PumpOpen : report.Drift > 0 ? ThermalFailure.SensorReference :
                report.Stale > 0 ? ThermalFailure.StaleOutlet : report.Qualified ? ThermalFailure.None : ThermalFailure.Endurance;
            evidence = $"{report.Minimum:F1}–{report.Peak:F1} °C · final {report.Core:F1} °C\n" +
                $"{report.BackupSamples} backup samples · {report.HeaterSamples} heated · balance {report.Residual:F3} J";
            if (!report.Qualified)
            {
                message = Failure switch
                {
                    ThermalFailure.PumpOpen => "Loop A command is live but its pump current is absent. Probe Q1.",
                    ThermalFailure.SensorReference => "Loop B disagrees with the independent reference. Probe R1.",
                    ThermalFailure.StaleOutlet => "Outlet measurements aged out. Probe TH1.",
                    _ => "Thermal endurance failed. Review flow and the temperature trace."
                };
                return;
            }

            qualifiedEpoch = epoch;
            if (stage == 3 && Award()) message = "Thermal endurance qualified. Begin the three repair trials.";
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                faultWitnessed = reworked = false;
                QueueCheckpoint();
                message = repaired == 3 && Award() ? "Repairs verified. Start a protected trial and open W465 during loop B." :
                    "Repair verified by new measurements. Begin the next hardware fault.";
            }
            else if (stage == 4) message = "Thermal evidence refreshed. Begin the next hardware fault.";
            else if (stage == 5) message = interrupted ? "Thermal control requalified. Wait for fresh feedback and reset U3." :
                "Thermal control requalified. Wait for trusted feedback before the protected trial.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Qualifies || Running)
                return Reject("Finish a healthy endurance trial before staging a fault.");
            activeFault = repaired;
            faultWitnessed = reworked = false;
            repairTarget = -1;
            spare = 0;
            qualifiedEpoch = runEpoch = -1;
            EnergizeFault();
            message = "Fault staged. Run endurance to capture its measured signature.";
            QueueCheckpoint();
            return true;
        }

        private void EnergizeFault()
        {
            faultEnergized = true;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                SetContact(pumpAContact, !FaultLive(0));
                referenceLoad.SetResistance(FaultLive(1) ? 68f : 100f);
                SetContact(outletContact, !FaultLive(2));
            }
            ApplyPump(false);
        }

        public bool ProbeFault()
        {
            if (!powered || runEpoch != epoch || activeFault < 0 || reworked)
                return Reject("Run the trial with the fault present before probing.");
            bool measured = activeFault switch
            {
                0 => Failure == ThermalFailure.PumpOpen && PumpLoss > 0 && !pumpAContact.IsConducting && PumpCurrent(0) < .001f && PumpCurrent(1) > .035f,
                1 => Failure == ThermalFailure.SensorReference && MaxDisagreement > 8f && ReferenceGain > 1.3f,
                2 => Failure == ThermalFailure.StaleOutlet && StaleSamples > 0 && PeakSensorAge >= 4f && !Live(sensorPins[0]) && Live(sensorPins[1]),
                _ => false
            };
            if (!measured) return Reject("The circuit reading and temperature trace do not identify this fault.");
            footprint = ExpectedPart;
            faultWitnessed = true;
            evidence = activeFault switch
            {
                0 => $"Q1 loop A {PumpCurrent(0) * 1000f:F1} mA · loop B {PumpCurrent(1) * 1000f:F1} mA · {PumpLoss} missed A samples.",
                1 => $"R1 gain {ReferenceGain:F2} · {MaxDisagreement:F1} °C discrepancy against independent reference.",
                _ => $"TH1 · {StaleSamples} stale samples · {PeakSensorAge:F1} s max age · TH2 stayed live."
            };
            message = $"Isolate the bench. Select {parts[ExpectedPart].label} and its spare.";
            QueueCheckpoint();
            return true;
        }

        public void SelectSpare()
        {
            spare = (spare + 1) % 4;
            message = spare switch
            {
                1 => "Spare · Q1 pump A driver",
                2 => "Spare · R1 loop B reference",
                3 => "Spare · TH1 outlet contact",
                _ => "No spare selected."
            };
        }

        public bool Repair()
        {
            if (stage != 4 || activeFault < 0 || !faultWitnessed || reworked || powered)
                return Reject("Diagnose and isolate the bench before rework.");
            if (spare != activeFault + 1 || repairTarget != ExpectedPart)
                return Reject("Match the spare to the diagnosed footprint.");
            if (activeFault == 0) SetContact(pumpAContact, true);
            if (activeFault == 1) referenceLoad.SetResistance(100f);
            if (activeFault == 2) SetContact(outletContact, true);
            reworked = pumpAContact.IsConducting && Mathf.Abs(referenceLoad.Resistance - 100f) < .001f && outletContact.IsConducting;
            if (!reworked) return Reject("The native replacement did not seat.");
            InvalidateDiagnostics();
            QueueCheckpoint();
            BoardView();
            message = "Replacement seated. Measure, recommission and repeat thermal endurance.";
            return true;
        }

        public bool TestWatchdog()
        {
            if (stage != 5 || !Fresh || interrupted || Running || ProtectedBusy || WatchdogActive)
                return Reject("The watchdog test needs an idle, qualified bench before W465 removal.");
            watchdogObserved = false;
            watchdogUntil = time + .95f;
            message = "Holding the readiness contact high. Watch U3 reject a stale edge.";
            return true;
        }

        public bool StartProtected()
        {
            if (stage != 5 || !Fresh || interrupted && !safetyReset || Running || ProtectedBusy || WatchdogActive)
                return Reject("U3 requires fresh evidence and time-varying readiness before allowing radiator actuation.");
            armed = true;
            protectedReplay = new Thermal.Replay();
            ProtectedDelivered = verifiedPermits = ProtectedBackupGates = ProtectedHeatSamples = 0;
            nextSample = time;
            scope?.ResetTrace();
            message = interrupted ? "Verifying survival heat, backup flow and both radiator outputs." :
                "Protected trial active. Open W465 during loop B takeover.";
            return true;
        }

        private void FinishProtected(Thermal.Replay report)
        {
            protectedReplay = null;
            armed = protectedRequest = false;
            SetContact(workload, false);
            if (interrupted && safetyReset && report.Qualified && verifiedPermits == Thermal.FrameCount && Fresh && Award())
            {
                message = "W465 recovered. U3 verified both radiators and left them inhibited.";
                evidence = $"{ProtectedDelivered}/64 gate checks · {report.Core:F1} °C final · survival supply live.";
            }
            else if (!interrupted) message = "Repeat the protected trial and open W465 while both radiators are active.";
            else message = "Protected trial needs fresh thermal evidence and every gate check.";
        }

        private void AbortProtected()
        {
            UndeliveredAtTrip = Thermal.FrameCount - ProtectedDelivered;
            protectedReplay = null;
            armed = protectedRequest = safetyReset = false;
            SetContact(workload, false);
            minimumEpoch = epoch + 1;
            manager.ElectricalSolver.SolveNow();
        }

        public bool OpenW465()
        {
            if (stage != 5 || !linked || interrupted || !ProtectedBusy || ProtectedDelivered == 0 || Inhibited)
                return Reject("Open W465 during a protected trial while radiator actuation is active.");
            return RemoveWire(jumper) || Reject("The native registry could not open W465.");
        }

        public bool RestoreW465()
        {
            if (stage != 5 || !interrupted || linked) return Reject("Open W465 during a protected trial first.");
            if (!CreateWire(jumper)) return Reject("The native circuit rejected the W465 training contacts.");
            ReadGraph();
            message = "W465 restored. U3 still requires fresh pump and temperature evidence.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !Fresh || epoch < minimumEpoch || ProtectedBusy)
                return Reject("Requalify both loops and wait for fresh feedback before resetting U3.");
            safetyReset = true;
            message = "U3 reset. Run the final protected endurance trial.";
            return true;
        }

        private void InvalidateDiagnostics()
        {
            epoch++;
            measurementEpoch = calibrationEpoch = qualifiedEpoch = runEpoch = -1;
            checkedInputs = protectedRequest = false;
            replay = protectedReplay = null;
            feedbackIntervals = 0;
            lastRise = watchdogUntil = -1f;
            highFeedback = false;
            SetContact(workload, false);
        }

        private void UpdateSupervisor()
        {
            if (time >= nextHeartbeat)
            {
                nextHeartbeat = time + .14f;
                driverHeartbeat = Qualifies && (WatchdogActive || !driverHeartbeat);
                SetContact(heartbeat, driverHeartbeat);
                bool high = linked && Live(jumper.to);
                if (high && !highFeedback)
                {
                    feedbackIntervals = lastRise >= 0 && time - lastRise is > .21f and < .49f ? feedbackIntervals + 1 : 0;
                    lastRise = time;
                }
                highFeedback = high;
            }
            if (WatchdogActive && highFeedback && !Fresh && time - lastRise >= .49f && !watchdogObserved)
            {
                watchdogObserved = true;
                message = "U3 rejected held-high feedback. Wait for fresh heartbeat transitions.";
            }
            bool permit = stage == 5 && Fresh && (!interrupted || safetyReset) && ProtectedBusy && protectedRequest;
            SetContact(workload, permit);
            benchState = stage == 6 ? "TCX-35 QUALIFIED" : !powered ? "BENCH ISOLATED" :
                interrupted && !safetyReset ? linked ? "U3 LATCHED · RESET REQUIRED" : "W465 OPEN · U3 LATCHED" :
                !RailsReady ? "RAILS UNQUALIFIED" : !Inhibited ? "PROTECTED RADIATORS LIVE" : "THERMAL BENCH READY";
        }
    }
}
