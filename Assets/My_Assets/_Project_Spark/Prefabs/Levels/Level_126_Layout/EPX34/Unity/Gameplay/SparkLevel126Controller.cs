using UnityEngine;
using Energy = ProjectSpark.Gameplay.EclipseCore.SparkLevel126Energy;

namespace ProjectSpark.Gameplay.EclipseCore
{
    public sealed partial class SparkLevel126Process
    {
        public SparkTerminal rail, secondaryRail, monitorSupply, telemetryPin, thermalPin, chargePin, dischargePin, sensePin;
        public SparkTerminal essentialPin, testAuxPin, protectedFeed;
        public SparkTerminal[] logicPins, solarPins;
        public SparkSwitch bench, secondaryPower, monitorGate, telemetryGate, thermalGate, chargeGate, dischargeGate, senseGate;
        public SparkSwitch essentialGate, stepGate, auxPower, testAuxGate, heartbeat, workload;
        public SparkSwitch[] logicGates, solarGates;
        public SparkResistor batterySense, filterSeries, essentialLoad, testAuxLoad, protectedLoad;
        public SparkResistor[] solarLoads;
        public Renderer[] sensorMarkers;
        public GameObject assemblyArt;
        public int solarPart, sensePart, filterPart;

        private const float TrainingWattsPerBenchWatt = 900f;
        public enum EnergyFailure { None, SolarOvercurrent, SenseDrift, EssentialSag, EnergyBalance }
        private readonly Energy energy = new();
        private Energy.Replay replay, protectedReplay;
        private float time, nextSample, nextHeartbeat, lastRise = -1f, watchdogUntil;
        private float loadedBus, peakSolarFault, measuredAux;
        private bool driverHeartbeat, highFeedback, checkedInputs, watchdogObserved, solarLatched;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset, showSensors, faultEnergized, protectedRequest;
        private int epoch, qualifiedEpoch = -1, measurementEpoch = -1, calibrationEpoch = -1, runEpoch = -1, minimumEpoch, feedbackIntervals;
        private int activeFault = -1, repaired, repairTarget = -1, spare, verifiedPermits;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool RailsReady => powered && Live(rail) && Live(secondaryRail) && Live(monitorSupply) &&
            Live(telemetryPin) && Live(thermalPin) && Live(chargePin) && Live(dischargePin) &&
            Live(logicPins[0]) && Live(logicPins[1]);
        public bool Measured => measurementEpoch == epoch;
        public bool Calibrated => calibrationEpoch == epoch;
        public bool Running => replay != null && !replay.Complete;
        public bool ProtectedBusy => protectedReplay != null;
        public bool Qualifies => qualifiedEpoch == epoch && Measured && Calibrated && checkedInputs &&
            RailsReady && Live(essentialPin) && !solarLatched && SenseGain is > .96f and < 1.04f;
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .49f;
        public bool WatchdogActive => time < watchdogUntil;
        public bool WatchdogObserved => watchdogObserved;
        public bool SolarLatched => solarLatched;
        public float PeakSolarFault => peakSolarFault;
        public float LoadedBus => loadedBus;
        public float SenseGain => Current(batterySense) / .05f;
        public float SolarCurrent(int channel) => Current(solarLoads[channel]);
        public float SolarWatts => BenchWatts(solarPins[0], solarLoads[0]) + BenchWatts(solarPins[1], solarLoads[1]);
        public float EssentialWatts => BenchWatts(essentialPin, essentialLoad);
        public float ProtectedAuxWatts => BenchWatts(protectedFeed, protectedLoad);
        public int Accepted => replay?.Good ?? 0;
        public int Sent => replay?.Sent ?? 0;
        public int ShedSamples => replay?.ShedSamples ?? 0;
        public int RecoverySamples => replay?.RecoverySamples ?? 0;
        public int SolarTrips => replay?.SolarTrips ?? 0;
        public int SenseHolds => replay?.SenseHolds ?? 0;
        public int SagHolds => replay?.SagHolds ?? 0;
        public float EclipseEnd => replay?.EclipseEnd ?? 0f;
        public float FinalEnergy => replay?.Stored ?? 0f;
        public float UnservedEssential => replay?.UnservedEssentialWh ?? 0f;
        public float LedgerResidual => replay?.BalanceError ?? 0f;
        public float MeterError => replay?.MaxMeterError ?? 0f;
        public float PeakSag => replay?.PeakSag ?? 0f;
        public float MinimumLoadedBus => replay?.MinimumLoadedBus ?? 0f;
        public int Repaired => repaired;
        public int Epoch => epoch;
        public int ProtectedDelivered { get; private set; }
        public int ProtectedSunriseSamples { get; private set; }
        public int UndeliveredAtTrip { get; private set; }
        public EnergyFailure Failure { get; private set; }
        public string Evidence => evidence;

        private int ExpectedPart => activeFault switch { 0 => solarPart, 1 => sensePart, 2 => filterPart, _ => -1 };
        private bool FaultLive(int index) => activeFault == index && !reworked && faultEnergized;
        private bool Live(SparkTerminal pin) => Voltage(pin) > 4.55f;
        private static float Current(SparkResistor load) => Mathf.Abs(load.Current);
        private float BenchWatts(SparkTerminal pin, SparkResistor load) =>
            Mathf.Max(0f, Voltage(pin)) * Current(load) * TrainingWattsPerBenchWatt;

        public bool TogglePower()
        {
            if (stage < 2 || stage >= 6 || !Construction || !linked && !interrupted)
                return Reject("Complete native continuity and W449 before enabling the energy bench.");
            if (powered) { IsolatePower(); return true; }
            powered = true;
            if (!FaultLive(0)) solarLatched = false;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                foreach (var contact in new[] { bench, secondaryPower, monitorGate, telemetryGate, thermalGate, chargeGate,
                    dischargeGate, senseGate, essentialGate, auxPower })
                    SetContact(contact, true);
                foreach (var gate in logicGates) SetContact(gate, true);
                solarLoads[0].SetResistance(FaultLive(0) ? 12f : 250f);
                batterySense.SetResistance(FaultLive(1) ? 68f : 100f);
                filterSeries.SetResistance(FaultLive(2) ? 78f : .12f);
            }
            ApplySun(true);
            message = "Measure both solar inputs, the signed-current scale and the essential load step.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                foreach (var contact in new[] { bench, secondaryPower, monitorGate, telemetryGate, thermalGate, chargeGate, dischargeGate,
                    senseGate, essentialGate, auxPower, testAuxGate, stepGate, heartbeat, workload })
                    SetContact(contact, false);
                foreach (var gate in logicGates) SetContact(gate, false);
                foreach (var gate in solarGates) SetContact(gate, false);
                InvalidateDiagnostics();
            }
            message = "Bench isolated. Select the diagnosed footprint and its matching spare.";
        }

        private void ApplySun(bool light)
        {
            bool a = powered && light && !solarLatched;
            bool b = powered && light;
            if (solarGates[0].IsClosed != a || solarGates[1].IsClosed != b)
            {
                using (manager.ElectricalSolver.BeginConfigurationBatch())
                {
                    SetContact(solarGates[0], a);
                    SetContact(solarGates[1], b);
                }
            }
            if (a && SolarCurrent(0) > Energy.PeakSolarCurrent)
            {
                peakSolarFault = SolarCurrent(0);
                solarLatched = true;
                SetContact(solarGates[0], false);
            }
        }

        private void MeasureLoadStep()
        {
            SetContact(stepGate, true);
            loadedBus = Voltage(essentialPin);
            SetContact(stepGate, false);
        }

        public bool MeasureBench()
        {
            if (!RailsReady || Running || ProtectedBusy)
                return Reject("Power the bench and stop the trial before measuring.");
            ApplySun(true);
            MeasureLoadStep();
            SetContact(testAuxGate, true);
            measuredAux = BenchWatts(testAuxPin, testAuxLoad);
            SetContact(testAuxGate, false);
            measurementEpoch = epoch;
            evidence = $"{SolarWatts:F1} W solar · {EssentialWatts:F1} W essential · {measuredAux:F1} W auxiliary\n" +
                $"Loaded bus {loadedBus:F3} V · signed-current gain {SenseGain:F3}";
            bool good = !solarLatched && SolarWatts > 170f && EssentialWatts > 55f &&
                measuredAux > 40f && loadedBus >= 4.55f && SenseGain is > .96f and < 1.04f;
            message = good ? "Supply and load-step measurements verified. Commission the converters." :
                "A measurement is out of range. Use the eclipse trial to capture the fault.";
            return good;
        }

        public bool Calibrate()
        {
            if (!RailsReady || !Measured || Running || ProtectedBusy || solarLatched || SolarWatts < 170f ||
                EssentialWatts < 55f || measuredAux < 40f || loadedBus < 4.55f || SenseGain is < .96f or > 1.04f)
                return Reject("Measure healthy solar, essential, auxiliary and signed-current paths before commissioning.");
            calibrationEpoch = epoch;
            qualifiedEpoch = -1;
            checkedInputs = false;
            if (stage == 2) Award();
            evidence = "5 V training bench · 240 Wh store · 60 Wh protected reserve.";
            message = "Test the ledger guards, then run the 90-minute eclipse.";
            return true;
        }

        public bool CheckGuardRejection()
        {
            if (!Calibrated || !Measured || Running || ProtectedBusy)
                return Reject("Commission the converters before testing the energy ledger.");
            var full = new Energy.Replay { Sent = Energy.FirstSunrise, Stored = 239f };
            energy.Advance(full, new Energy.Inputs(180f, 60f, 45f, 5f, 5f, 1f, false));
            var empty = new Energy.Replay { Sent = Energy.FirstEclipse, Stored = 0f };
            energy.Advance(empty, new Energy.Inputs(0f, 60f, 0f, 5f, 5f, 1f, false));
            var invalid = new Energy.Replay();
            energy.Advance(invalid, new Energy.Inputs(float.NaN, 60f, 0f, 5f, 5f, 1f, false));
            checkedInputs = full.Stored <= Energy.Capacity && full.SpilledWh > 0f && empty.UnservedEssentialWh > 2f &&
                !Energy.RequestAux(empty, 0f, 5f) && invalid.Invalid && !Energy.RequestAux(invalid, 180f, 5f);
            if (!checkedInputs) return Reject("The ledger failed capacity, depletion or invalid-input checks.");
            evidence = "Saturated battery spills surplus. Depletion records unserved load. Invalid samples hold auxiliary power.";
            message = "Ledger guards passed. Start the eclipse trial.";
            return true;
        }

        public bool RunEnergy()
        {
            if (stage is < 3 or > 5 || !RailsReady || !linked || !Calibrated || !Measured || !checkedInputs ||
                Running || ProtectedBusy || WatchdogActive)
                return Reject("Measure, commission and exercise the ledger guards before running the eclipse trial.");
            if (activeFault >= 0 && !reworked) EnergizeFault();
            replay = new Energy.Replay();
            qualifiedEpoch = runEpoch = -1;
            Failure = EnergyFailure.None;
            nextSample = time;
            scope?.ResetTrace();
            message = "Sampling solar input, eclipse reserve and sunrise recovery.";
            return true;
        }

        private void TickController(float dt)
        {
            time += dt;
            if (stage is >= 2 and <= 6) UpdateSupervisor();
            if (time < nextSample || !Running && !ProtectedBusy) return;
            bool protect = ProtectedBusy;
            var run = protect ? protectedReplay : replay;
            if (protect && !Fresh)
            {
                AbortProtected();
                message = "Trusted readiness expired. Requalify before enabling auxiliary loads.";
                return;
            }

            var phase = Energy.PhaseAt(run.Sent);
            ApplySun(phase != Energy.Phase.Eclipse);
            SetContact(essentialGate, powered && (phase != Energy.Phase.Eclipse || run.Stored > 0f));
            bool request = Energy.RequestAux(run, SolarWatts, Voltage(essentialPin));
            if (protect)
            {
                protectedRequest = request;
                SetContact(workload, Fresh && protectedRequest);
            }
            else SetContact(testAuxGate, request);
            if (run.Sent % 8 == 0) MeasureLoadStep();
            float auxiliary = protect ? ProtectedAuxWatts : BenchWatts(testAuxPin, testAuxLoad);
            energy.Advance(run, new Energy.Inputs(SolarWatts, EssentialWatts, auxiliary,
                Voltage(essentialPin), loadedBus, SenseGain, solarLatched));

            nextSample = time + (protect ? !interrupted && phase == Energy.Phase.Sunrise ? .15f : .051f : .028f);
            var sample = run.Last;
            scope?.Record(Mathf.Clamp01(sample.Stored / Energy.Capacity), Mathf.Clamp01(sample.Solar / 210f),
                Mathf.Clamp01(sample.Auxiliary / 60f), Mathf.Clamp01((sample.Bus - 3.5f) / 1.5f));
            if (!protect)
            {
                if (run.Complete) FinishEnergy(run);
                return;
            }

            if (request ? ProtectedAuxWatts > 40f && !Inhibited : ProtectedAuxWatts < .5f && Inhibited)
            {
                verifiedPermits++;
                if (phase == Energy.Phase.Sunrise && request) ProtectedSunriseSamples++;
            }
            if (!interrupted && ProtectedSunriseSamples == 1 && phase == Energy.Phase.Sunrise)
                message = "Sunrise restored the auxiliary load. Open W449 while it is powered.";
            ProtectedDelivered = run.Sent;
            if (run.Complete) FinishProtected(run);
        }

        private void FinishEnergy(Energy.Replay report)
        {
            runEpoch = epoch;
            SetContact(testAuxGate, false);
            ApplySun(true);
            Failure = report.SolarTrips > 0 ? EnergyFailure.SolarOvercurrent : report.SenseHolds > 0 ? EnergyFailure.SenseDrift :
                report.SagHolds > 0 ? EnergyFailure.EssentialSag : report.Qualified ? EnergyFailure.None : EnergyFailure.EnergyBalance;
            evidence = $"{report.EclipseEnd:F1} Wh after eclipse · {report.Stored:F1} Wh after sunrise\n" +
                $"{report.ShedSamples} shed samples · {report.LossWh:F2} Wh losses · {report.BalanceError:F3} Wh residual";
            if (!report.Qualified)
            {
                message = Failure switch
                {
                    EnergyFailure.SolarOvercurrent => "Solar A tripped. Probe Q1 and compare solar B.",
                    EnergyFailure.SenseDrift => "Battery meter disagrees with the independent ledger. Probe R1.",
                    EnergyFailure.EssentialSag => "The essential bus sags under the load step. Probe C1.",
                    _ => "Reserve or energy balance failed. Review the eclipse trace."
                };
                return;
            }
            qualifiedEpoch = epoch;
            if (stage == 3 && Award()) message = "Eclipse resilience qualified. Begin the three repair trials.";
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                faultWitnessed = reworked = false;
                QueueCheckpoint();
                message = repaired == 3 && Award() ? "Repairs verified. Start a protected trial and open W449 at sunrise." :
                    "Repair verified by a new eclipse trial. Begin the next hardware fault.";
            }
            else if (stage == 4) message = "Eclipse evidence refreshed. Begin the next hardware fault.";
            else if (stage == 5) message = interrupted ? "Energy requalified. Wait for fresh feedback and reset U3." :
                "Energy requalified. Wait for trusted feedback before the protected trial.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Qualifies || Running)
                return Reject("Finish a healthy eclipse trial before staging a fault.");
            activeFault = repaired;
            faultWitnessed = reworked = false;
            repairTarget = -1;
            spare = 0;
            qualifiedEpoch = runEpoch = -1;
            EnergizeFault();
            message = "Fault staged. Run an eclipse trial to capture its measured signature.";
            QueueCheckpoint();
            return true;
        }

        private void EnergizeFault()
        {
            faultEnergized = true;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                solarLoads[0].SetResistance(FaultLive(0) ? 12f : 250f);
                batterySense.SetResistance(FaultLive(1) ? 68f : 100f);
                filterSeries.SetResistance(FaultLive(2) ? 78f : .12f);
            }
            ApplySun(true);
        }

        public bool ProbeFault()
        {
            if (!powered || runEpoch != epoch || activeFault < 0 || reworked)
                return Reject("Run the trial with the fault present before probing.");
            bool measured = activeFault switch
            {
                0 => Failure == EnergyFailure.SolarOvercurrent && SolarTrips > 0 && solarLatched &&
                    peakSolarFault > Energy.PeakSolarCurrent && SolarCurrent(1) > .018f,
                1 => Failure == EnergyFailure.SenseDrift && MeterError > 25f && SenseGain > 1.3f,
                2 => Failure == EnergyFailure.EssentialSag && PeakSag > .20f && MinimumLoadedBus < 4.3f && Live(rail),
                _ => false
            };
            if (!measured) return Reject("The circuit reading and ledger do not identify this fault.");
            footprint = ExpectedPart;
            faultWitnessed = true;
            evidence = activeFault switch
            {
                0 => $"Q1 peaked at {peakSolarFault * 1000f:F0} mA · solar A isolated · B delivers {SolarWatts:F0} W.",
                1 => $"R1 gain {SenseGain:F2} · independent ledger disagrees by {MeterError:F1} Wh.",
                _ => $"C1 loaded bus {MinimumLoadedBus:F2} V · {PeakSag:F2} V load-step sag."
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
                1 => "Spare · Q1 solar A switch",
                2 => "Spare · R1 battery sense shunt",
                3 => "Spare · C1 essential filter",
                _ => "No spare selected."
            };
        }

        public bool Repair()
        {
            if (stage != 4 || activeFault < 0 || !faultWitnessed || reworked || powered)
                return Reject("Diagnose and isolate the bench before rework.");
            if (spare != activeFault + 1 || repairTarget != ExpectedPart)
                return Reject("Match the spare to the diagnosed footprint.");
            SparkResistor replacement = activeFault switch { 0 => solarLoads[0], 1 => batterySense, _ => filterSeries };
            float nominal = activeFault switch { 0 => 250f, 1 => 100f, _ => .12f };
            replacement.SetResistance(nominal);
            reworked = Mathf.Abs(replacement.Resistance - nominal) < .001f;
            if (!reworked) return Reject("The native replacement did not seat.");
            InvalidateDiagnostics();
            QueueCheckpoint();
            BoardView();
            message = "Replacement seated. Measure, recommission and repeat the eclipse trial.";
            return true;
        }

        public bool TestWatchdog()
        {
            if (stage != 5 || !Fresh || interrupted || Running || ProtectedBusy || WatchdogActive)
                return Reject("The watchdog test needs an idle, qualified bench before W449 removal.");
            watchdogObserved = false;
            watchdogUntil = time + .95f;
            message = "Holding the readiness contact high. Watch U3 reject a stale edge.";
            return true;
        }

        public bool StartProtected()
        {
            if (stage != 5 || !Fresh || interrupted && !safetyReset || Running || ProtectedBusy || WatchdogActive)
                return Reject("U3 requires fresh energy measurements and time-varying readiness before authorizing auxiliary loads.");
            armed = true;
            protectedReplay = new Energy.Replay();
            ProtectedDelivered = verifiedPermits = ProtectedSunriseSamples = 0;
            nextSample = time;
            scope?.ResetTrace();
            message = interrupted ? "Verifying protected load shedding and sunrise recovery." :
                "Protected trial active. Open W449 when sunrise restores auxiliary power.";
            return true;
        }

        private void FinishProtected(Energy.Replay report)
        {
            protectedReplay = null;
            armed = protectedRequest = false;
            SetContact(workload, false);
            if (interrupted && safetyReset && report.Qualified && verifiedPermits == Energy.FrameCount && Fresh && Award())
            {
                message = "W449 recovered. U3 verified shedding and sunrise recovery, then inhibited auxiliary output.";
                evidence = $"{ProtectedDelivered}/64 gate checks · {report.Stored:F1} Wh final · essential power live.";
            }
            else if (!interrupted) message = "Repeat the protected trial and open W449 while auxiliary power is active.";
            else message = "Protected trial needs fresh energy evidence and all gate checks.";
        }

        private void AbortProtected()
        {
            UndeliveredAtTrip = Energy.FrameCount - ProtectedDelivered;
            protectedReplay = null;
            armed = protectedRequest = safetyReset = false;
            SetContact(workload, false);
            minimumEpoch = epoch + 1;
            manager.ElectricalSolver.SolveNow();
        }

        public bool OpenW449()
        {
            if (stage != 5 || !linked || interrupted || !ProtectedBusy || ProtectedDelivered == 0 || Inhibited)
                return Reject("Open W449 during a protected trial while the auxiliary load is active.");
            return RemoveWire(jumper) || Reject("The native registry could not open W449.");
        }

        public bool RestoreW449()
        {
            if (stage != 5 || !interrupted || linked) return Reject("Open W449 during the protected trial first.");
            if (!CreateWire(jumper)) return Reject("The native circuit rejected the W449 training contacts.");
            ReadGraph();
            message = "W449 restored. U3 still requires fresh energy and load-step evidence.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !Fresh || epoch < minimumEpoch || ProtectedBusy)
                return Reject("Requalify the energy system and wait for fresh feedback before resetting U3.");
            safetyReset = true;
            message = "U3 reset. Run the final protected eclipse trial.";
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
                message = "U3 rejected held-high feedback. Wait for new transitions before authorizing auxiliary loads.";
            }
            bool permit = stage == 5 && Fresh && (!interrupted || safetyReset) && ProtectedBusy && protectedRequest;
            SetContact(workload, permit);
            benchState = stage == 6 ? "EPX-34 QUALIFIED" : !powered ? "BENCH ISOLATED" :
                interrupted && !safetyReset ? linked ? "U3 LATCHED · RESET REQUIRED" : "W449 OPEN · U3 LATCHED" :
                solarLatched ? "SOLAR A TRIPPED" : !RailsReady ? "RAILS UNQUALIFIED" :
                !Inhibited ? "PROTECTED AUXILIARY LIVE" : "ENERGY BENCH READY";
        }
    }
}
