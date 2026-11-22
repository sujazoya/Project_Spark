using UnityEngine;
using Orbit = ProjectSpark.Gameplay.OrbitalBastion.SparkLevel124Orbit;

namespace ProjectSpark.Gameplay.OrbitalBastion
{
    public sealed partial class SparkLevel124Process
    {
        public SparkTerminal rail, driveRail, monitorSupply, clockPin, secondaryClock, resetPin, tachReference, protectedFeed;
        public SparkTerminal telemetryPin, unloadPin;
        public SparkSwitch telemetryGate, unloadGate;
        public SparkTerminal[] logicPins, sensorPins, wheelPins, gatePins;
        public SparkSwitch bench, drivePower, monitorGate, clockGate, secondaryClockGate, resetGate, heartbeat, workload;
        public SparkSwitch[] logicGates, sensorGates, wheelPower, outputContacts;
        public SparkSwitch trackerContact, tachContact;
        public SparkResistor[] wheelLoads;
        public Renderer[] sensorMarkers;
        public GameObject assemblyArt;
        public int wheelPart, trackerPart, tachPart, trackerViaIndex;

        public enum SensorFailure { None, WheelOvercurrent, TrackerClock, Tachometer, PointingEnvelope }

        private readonly Orbit orbit = new();
        private readonly bool[] sensorLive = new bool[5];
        private readonly bool[] wheelsLive = new bool[4];
        private Orbit.Replay replay, protectedReplay;
        private float time, nextSample, nextHeartbeat, lastRise = -1f, measuredSkew, watchdogUntil;
        private bool driverHeartbeat, highFeedback, checkedInputs, watchdogObserved, currentLatched;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset, showSensors, faultEnergized;
        private int epoch, qualifiedEpoch = -1, clockEpoch = -1, calibrationEpoch = -1, runEpoch = -1, minimumEpoch, feedbackIntervals;
        private int activeFault = -1, repaired, repairTarget = -1, viaInspectionEpoch = -1, spare, verifiedPermits;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool RailsReady => powered && Live(rail) && Live(driveRail) && Live(monitorSupply) &&
            Live(resetPin) && Live(telemetryPin) && Live(logicPins[0]) && Live(logicPins[1]);
        public bool ClockReady => clockEpoch == epoch && Live(clockPin) && Live(secondaryClock);
        public bool Calibrated => calibrationEpoch == epoch && orbit.Calibrated;
        public bool Running => replay != null && !replay.Complete;
        public bool ProtectedBusy => protectedReplay != null;
        public bool Qualifies => qualifiedEpoch == epoch && ClockReady && Calibrated && checkedInputs &&
            RailsReady && SensorCount == sensorPins.Length && WheelCount == wheelPins.Length && !currentLatched && Voltage(tachReference) > 4.60f;
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .49f;
        public bool WatchdogActive => time < watchdogUntil;
        public bool WatchdogObserved => watchdogObserved;
        public int SensorCount { get; private set; }
        public int WheelCount { get; private set; }
        public int EnabledGates { get; private set; }
        public int Accepted => replay?.Trusted ?? 0;
        public int Sent => replay?.Sent ?? 0;
        public int StaleB => replay?.StaleB ?? 0;
        public int OvercurrentHolds => replay?.OvercurrentHolds ?? 0;
        public int TachHolds => replay?.TachHolds ?? 0;
        public int CorrectedWords => replay?.EccCorrected ?? 0;
        public int SafeHolds => replay?.EccSafeHolds ?? 0;
        public int DesaturationSamples => replay?.DumpSamples ?? 0;
        public float StoredBefore => replay?.BeforeDump ?? 0f;
        public float StoredAfter => replay?.AfterDump ?? 0f;
        public float PointingError => (replay?.PointingEnd ?? 0f) * Mathf.Rad2Deg;
        public float EclipseError => (replay?.PointingDuringEclipse ?? 0f) * Mathf.Rad2Deg;
        public float TachDifference => replay?.PeakTachDifference ?? 0f;
        public bool CurrentLatched => currentLatched;
        public float MeasuredSkew => measuredSkew;
        public float WheelTestCurrent(int channel) => Mathf.Abs(wheelLoads[channel].Current);
        public int Repaired => repaired;
        public int Epoch => epoch;
        public int ProtectedDelivered { get; private set; }
        public int UndeliveredAtTrip { get; private set; }
        public int ProtectedUnloads { get; private set; }
        public int ProtectedSafeHolds { get; private set; }
        public SensorFailure Failure { get; private set; }
        public string Evidence => evidence;

        private Route TrackerVia => routes[trackerViaIndex];
        private int ExpectedPart => activeFault switch { 0 => wheelPart, 1 => trackerPart, 2 => tachPart, _ => -1 };
        private bool FaultLive(int index) => activeFault == index && !reworked && faultEnergized;
        private bool Live(SparkTerminal pin) => Voltage(pin) > 4.55f;

        public bool TogglePower()
        {
            if (stage < 2 || stage >= 6 || !Construction || !linked && !interrupted)
                return Reject("Complete native continuity and W417 before enabling the attitude bench.");
            if (powered) { IsolatePower(); return true; }
            powered = true;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                foreach (var contact in new[] { bench, drivePower, monitorGate, clockGate, secondaryClockGate, resetGate, telemetryGate })
                    SetContact(contact, true);
                foreach (var gate in logicGates) SetContact(gate, true);
                foreach (var gate in sensorGates) SetContact(gate, true);
                foreach (var gate in wheelPower) SetContact(gate, true);
                foreach (var gate in outputContacts) SetContact(gate, true);
                wheelLoads[2].SetResistance(FaultLive(0) ? 12f : 100f);
                SetContact(trackerContact, !FaultLive(1));
                SetContact(tachContact, !FaultLive(2));
            }
            ReadOrbit();
            if (!FaultLive(0) && WheelTestCurrent(2) < Orbit.CurrentLimit) currentLatched = false;
            message = "Verify both clocks and reset release before commissioning the attitude bench.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                foreach (var contact in new[] { bench, drivePower, monitorGate, clockGate, secondaryClockGate, resetGate, telemetryGate, heartbeat, workload, unloadGate })
                    SetContact(contact, false);
                foreach (var gate in logicGates) SetContact(gate, false);
                foreach (var gate in sensorGates) SetContact(gate, false);
                foreach (var gate in wheelPower) SetContact(gate, false);
                InvalidateDiagnostics();
            }
            ReadOrbit();
            message = "Bench isolated. Select the diagnosed contact and matching spare.";
        }

        public bool MeasureClock()
        {
            if (!RailsReady || !Live(clockPin) || !Live(secondaryClock) || Running || ProtectedBusy)
                return Reject("Power both clocks and stop the trial before measuring timing.");
            measuredSkew = 0f;
            for (int tick = 0; tick < 24; tick++) measuredSkew = Mathf.Max(measuredSkew, Orbit.ClockSkew(tick));
            if (measuredSkew > Orbit.MaximumClockSkew) return Reject("Clock disagreement exceeds the training window.");
            clockEpoch = epoch;
            message = "Dual timing measured. Commission the sensors and drive test loads.";
            evidence = $"24 clock samples · {measuredSkew:F0} µs peak skew · 140 µs limit.";
            return true;
        }

        public bool Calibrate()
        {
            if (!RailsReady || !ClockReady || Running || ProtectedBusy || !ReadOrbit().Ready)
                return Reject("Measure both clocks and verify the sensor and wheel return paths.");
            orbit.Calibrate();
            calibrationEpoch = epoch;
            qualifiedEpoch = -1;
            checkedInputs = false;
            if (stage == 2) Award();
            evidence = "Calibrated both trackers, gyro, sun and tachometer. All four wheel shunts conduct.";
            message = "Challenge the interlocks, then run the 64-sample attitude bench.";
            return true;
        }

        public bool CheckGuardRejection()
        {
            if (!Calibrated || !ClockReady || Running || ProtectedBusy || !ReadOrbit().Ready)
                return Reject("Commission attitude sensing before challenging the interlocks.");
            var inputs = ReadOrbit();
            bool Test(Orbit.Challenge trial, Orbit.Hold expected)
            {
                var check = new Orbit.Replay();
                orbit.Advance(check, inputs, trial);
                return !check.Last.RequestGate && check.Last.Reason == expected;
            }
            checkedInputs = Test(Orbit.Challenge.TrackerStale, Orbit.Hold.TrackerStale) &&
                Test(Orbit.Challenge.Overcurrent, Orbit.Hold.WheelCurrent) &&
                Test(Orbit.Challenge.Uncorrectable, Orbit.Hold.EccSafeMode);
            if (!checkedInputs) return Reject("The attitude fixture did not reject all three challenged states.");
            evidence = "Rejected stale tracking, overcurrent and an uncorrectable memory word.";
            message = "Guard tests passed. Run the attitude trial.";
            return true;
        }

        public bool RunOrbit()
        {
            if (stage is < 3 or > 5 || !RailsReady || !linked || !Calibrated || !checkedInputs ||
                Running || ProtectedBusy || !ClockReady || WatchdogActive)
                return Reject("Measure, commission and test the interlocks before the attitude trial.");
            if (activeFault >= 0 && !reworked) EnergizeFault();
            replay = new Orbit.Replay();
            qualifiedEpoch = runEpoch = -1;
            Failure = SensorFailure.None;
            nextSample = time;
            scope?.ResetTrace();
            message = "Running acquisition, eclipse pointing, momentum dump and ECC safe mode.";
            return true;
        }

        private Orbit.Inputs ReadOrbit()
        {
            SensorCount = WheelCount = 0;
            for (int i = 0; i < sensorLive.Length; i++)
            {
                sensorLive[i] = powered && Live(sensorPins[i]);
                if (sensorLive[i]) SensorCount++;
            }
            bool overcurrent = false;
            for (int i = 0; i < wheelsLive.Length; i++)
            {
                float amps = WheelTestCurrent(i);
                wheelsLive[i] = powered && Live(wheelPins[i]) && amps > .035f;
                if (!wheelsLive[i]) continue;
                WheelCount++;
                if (amps > Orbit.CurrentLimit) overcurrent = true;
            }
            if (powered && overcurrent) currentLatched = true;
            return new Orbit.Inputs(WheelCount, sensorLive[0], sensorLive[1], sensorLive[2], sensorLive[3], sensorLive[4],
                overcurrent || currentLatched, Voltage(tachReference));
        }

        private void TickController(float dt)
        {
            time += dt;
            ReadOrbit();
            if (stage is >= 2 and <= 6) UpdateSupervisor();
            if (time < nextSample || !Running && !ProtectedBusy) return;
            bool protect = ProtectedBusy;
            var run = protect ? protectedReplay : replay;
            if (protect && !Fresh)
            {
                AbortProtected();
                message = "Trusted readiness expired. Requalify attitude control before authorizing its gates.";
                return;
            }

            if (FaultLive(1)) SetContact(trackerContact, !Orbit.TrackerDropout(run.Sent));
            orbit.Advance(run, ReadOrbit());
            var sample = run.Last;
            nextSample = time + (protect ? !interrupted && sample.Desaturating ? .17f : .051f : .028f);
            scope?.Record(Mathf.Clamp01(sample.PointingError), sample.Momentum, sample.Confidence,
                Mathf.Clamp01(WheelTestCurrent(2) / .45f));
            if (!protect)
            {
                if (run.Complete) FinishOrbit(run);
                return;
            }

            SetProtectedOutputs(Fresh && sample.RequestGate, sample.Desaturating);
            ReadEnabledGates();
            bool unloadLive = Live(unloadPin);
            if (!interrupted && unloadLive && ProtectedUnloads == 0)
                message = "Momentum dump active. Open W417 to cut the wheel and unload outputs.";
            if (EnabledGates == (sample.RequestGate ? 4 : 0) && unloadLive == sample.Desaturating)
            {
                verifiedPermits++;
                if (unloadLive) ProtectedUnloads++;
                if (sample.Reason == Orbit.Hold.EccSafeMode) ProtectedSafeHolds++;
            }
            ProtectedDelivered = run.Sent;
            if (run.Complete) FinishProtected(run);
        }

        private void FinishOrbit(Orbit.Replay report)
        {
            runEpoch = epoch;
            if (FaultLive(1))
            {
                SetContact(trackerContact, false);
                ReadOrbit();
            }
            Failure = report.OvercurrentHolds > 0 ? SensorFailure.WheelOvercurrent : report.StaleB > 0 ? SensorFailure.TrackerClock :
                report.TachHolds > 0 ? SensorFailure.Tachometer : report.Qualified ? SensorFailure.None : SensorFailure.PointingEnvelope;
            evidence = $"{report.Trusted}/64 qualified · final pointing {report.PointingEnd * Mathf.Rad2Deg:F2}°\n" +
                $"Momentum {report.BeforeDump:F2} → {report.AfterDump:F2} · {report.EccCorrected} ECC repairs";
            if (!report.Qualified)
            {
                message = Failure switch
                {
                    SensorFailure.WheelOvercurrent => "U3 latched wheel overcurrent. Probe the RW3 shunt.",
                    SensorFailure.TrackerClock => "Star tracker B stale. Probe its reference-clock via.",
                    SensorFailure.Tachometer => "Wheel speed disagrees with tach telemetry. Probe its reference.",
                    _ => "Pointing limits exceeded. Inspect the attitude evidence."
                };
                return;
            }

            qualifiedEpoch = epoch;
            if (stage == 3 && Award()) message = "Attitude qualified. Begin the three hardware fault trials.";
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                faultWitnessed = reworked = false;
                QueueCheckpoint();
                message = repaired == 3 && Award() ? "Repairs verified. Run the protected attitude trial and open W417 during gate drive." :
                    "Repair verified by fresh orbit trials. Begin the next fault.";
            }
            else if (stage == 4) message = "Orbit evidence refreshed. Begin the next hardware fault.";
            else if (stage == 5) message = interrupted ? "Orbit requalified. Wait for trusted feedback, then reset U3." :
                "Orbit requalified. Wait for trusted feedback before starting a protected run.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Qualifies || Running)
                return Reject("Finish a healthy attitude trial before staging a fault.");
            activeFault = repaired;
            faultWitnessed = reworked = false;
            repairTarget = viaInspectionEpoch = -1;
            spare = 0;
            qualifiedEpoch = runEpoch = -1;
            EnergizeFault();
            message = "Fault staged. Run the attitude trial to capture the measured signature.";
            QueueCheckpoint();
            return true;
        }

        private void EnergizeFault()
        {
            faultEnergized = true;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                wheelLoads[2].SetResistance(FaultLive(0) ? 12f : 100f);
                SetContact(trackerContact, !FaultLive(1));
                SetContact(tachContact, !FaultLive(2));
            }
        }

        public bool ProbeFault()
        {
            if (!powered || runEpoch != epoch || activeFault < 0 || reworked)
                return Reject("Run the attitude trial with the fault present before probing.");
            bool measured = activeFault switch
            {
                0 => Failure == SensorFailure.WheelOvercurrent && OvercurrentHolds > 0 && currentLatched &&
                    WheelTestCurrent(2) > Orbit.CurrentLimit && WheelTestCurrent(1) < .10f,
                1 => Failure == SensorFailure.TrackerClock && StaleB > 0 && Live(sensorPins[0]) &&
                    !Live(sensorPins[1]) && Live(sensorPins[2]),
                2 => Failure == SensorFailure.Tachometer && TachHolds > 0 && Live(sensorPins[4]) && Voltage(tachReference) < 4.3f,
                _ => false
            };
            if (!measured) return Reject("The live reading and attitude trace do not identify this fault.");
            footprint = ExpectedPart;
            faultWitnessed = true;
            evidence = activeFault switch
            {
                0 => $"RW3 shunt {WheelTestCurrent(2) * 1000f:F0} mA · RW2 {WheelTestCurrent(1) * 1000f:F0} mA · U3 latched.",
                1 => $"{TrackerVia.viaId} · {StaleB} stale tracker B frames · tracker A and gyro stayed live.",
                _ => $"SN4 reference {Voltage(tachReference):F2} V · {TachDifference:P0} normalized speed difference."
            };
            message = $"Isolate power. Select {parts[ExpectedPart].label} and its matching spare.";
            QueueCheckpoint();
            return true;
        }

        public bool InspectTrackerVia()
        {
            if (activeFault != 1 || !faultWitnessed || powered) return Reject("Probe tracker B and isolate power first.");
            if (selectedRoute != trackerViaIndex || layer != TrackerVia.layer)
            {
                SelectRoute(trackerViaIndex);
                PadView();
                return Reject($"Inspect {TrackerVia.viaId} on entry layer L{TrackerVia.layer:D2}.");
            }
            viaInspectionEpoch = epoch;
            message = $"Confirmed {TrackerVia.viaId} from L{TrackerVia.layer:D2} to L{TrackerVia.exitLayer:D2}.";
            return true;
        }

        public void SelectSpare()
        {
            spare = (spare + 1) % 4;
            message = spare switch
            {
                1 => "Spare · RW3 current shunt",
                2 => "Spare · U7 reference-clock via contact",
                3 => "Spare · SN4 tachometer reference contact",
                _ => "No spare selected."
            };
        }

        public bool Repair()
        {
            if (stage != 4 || activeFault < 0 || !faultWitnessed || reworked || powered)
                return Reject("Diagnose and isolate the bench before rework.");
            if (spare != activeFault + 1 || repairTarget != ExpectedPart ||
                activeFault == 1 && (viaInspectionEpoch != epoch || layer != TrackerVia.layer))
                return Reject("Match the spare and footprint. Inspect the tracker clock via entry layer when required.");
            if (activeFault == 0)
            {
                wheelLoads[2].SetResistance(100f);
                reworked = Mathf.Abs(wheelLoads[2].Resistance - 100f) < .001f;
            }
            else
            {
                var replacement = activeFault == 1 ? trackerContact : tachContact;
                SetContact(replacement, true);
                reworked = replacement.IsConducting;
            }
            if (!reworked) return Reject("The replacement contact did not seat.");
            InvalidateDiagnostics();
            QueueCheckpoint();
            BoardView();
            message = "Replacement seated. Measure, recommission and repeat the attitude trial.";
            return true;
        }

        public bool TestWatchdog()
        {
            if (stage != 5 || !Fresh || interrupted || Running || ProtectedBusy || WatchdogActive)
                return Reject("The watchdog check needs a ready, idle bench before W417 removal.");
            watchdogObserved = false;
            watchdogUntil = time + .95f;
            message = "Holding the simulated readiness contact high. Watch U3 reject a stale edge.";
            return true;
        }

        public bool StartProtected()
        {
            if (stage != 5 || !Fresh || interrupted && !safetyReset || Running || ProtectedBusy || WatchdogActive)
                return Reject("U3 requires fresh commissioning and time-varying readiness before authorizing the wheel gates.");
            armed = true;
            protectedReplay = new Orbit.Replay();
            ProtectedDelivered = verifiedPermits = ProtectedUnloads = ProtectedSafeHolds = 0;
            nextSample = time;
            scope?.ResetTrace();
            message = interrupted ? "Verifying wheel and momentum-dump gates through ECC safe mode." :
                "Protected run active. Open W417 during the momentum dump.";
            return true;
        }

        private void FinishProtected(Orbit.Replay report)
        {
            protectedReplay = null;
            armed = false;
            SetProtectedOutputs(false, false);
            ReadEnabledGates();
            if (interrupted && safetyReset && report.Qualified && verifiedPermits == Orbit.FrameCount && Fresh && Award())
            {
                message = "W417 recovered. U3 verified wheel and unload gates through ECC safe mode.";
                evidence = $"{ProtectedDelivered}/64 gate checks · {report.EccSafeHolds} safe-mode holds · output inhibited.";
            }
            else if (!interrupted) message = "Repeat the protected trial and open W417 while wheels are enabled.";
            else message = "Protected trials need fresh attitude evidence and all gate checks.";
        }

        private void AbortProtected()
        {
            UndeliveredAtTrip = Orbit.FrameCount - ProtectedDelivered;
            protectedReplay = null;
            armed = safetyReset = false;
            SetProtectedOutputs(false, false);
            minimumEpoch = epoch + 1;
            manager.ElectricalSolver.SolveNow();
            ReadEnabledGates();
        }

        public bool OpenW417()
        {
            if (stage != 5 || !linked || interrupted || !ProtectedBusy || ProtectedDelivered == 0 || Inhibited)
                return Reject("Open W417 during a protected run while the gates are enabled.");
            return RemoveWire(jumper) || Reject("The native registry could not open W417.");
        }

        public bool RestoreW417()
        {
            if (stage != 5 || !interrupted || linked) return Reject("Open W417 during a protected run first.");
            if (!CreateWire(jumper)) return Reject("The native circuit rejected the W417 training contacts.");
            ReadGraph();
            message = "W417 restored. U3 still requires new sensor and attitude evidence.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !Fresh || epoch < minimumEpoch || ProtectedBusy)
                return Reject("Requalify attitude control and wait for fresh feedback before resetting U3.");
            safetyReset = true;
            message = "U3 reset. Run the final protected attitude trial.";
            return true;
        }

        private void InvalidateDiagnostics()
        {
            epoch++;
            clockEpoch = calibrationEpoch = qualifiedEpoch = runEpoch = -1;
            checkedInputs = false;
            orbit.ClearCalibration();
            replay = protectedReplay = null;
            feedbackIntervals = 0;
            lastRise = watchdogUntil = -1f;
            highFeedback = false;
            SetProtectedOutputs(false, false);
        }

        private void SetProtectedOutputs(bool enabled, bool unload)
        {
            bool unloading = enabled && unload;
            if (workload.IsClosed == enabled && unloadGate.IsClosed == unloading) return;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                SetContact(workload, enabled);
                SetContact(unloadGate, unloading);
            }
        }

        private void ReadEnabledGates()
        {
            EnabledGates = 0;
            foreach (var pin in gatePins)
                if (Live(pin)) EnabledGates++;
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
                message = "U3 rejected held-high feedback. Wait for new transitions before arming.";
            }
            bool permit = stage == 5 && Fresh && !currentLatched && (!interrupted || safetyReset) &&
                ProtectedBusy && protectedReplay.Sent > 0 && protectedReplay.Last.RequestGate;
            SetProtectedOutputs(permit, ProtectedBusy && protectedReplay.Last.Desaturating);
            ReadEnabledGates();
            benchState = stage == 6 ? "OBX-32 QUALIFIED" : !powered ? "BENCH ISOLATED" :
                interrupted && !safetyReset ? linked ? "U3 LATCHED · RESET REQUIRED" : "W417 OPEN · U3 LATCHED" :
                currentLatched ? "RW3 OVERCURRENT · U3 LATCHED" :
                ProtectedBusy && protectedReplay.Last.Reason == Orbit.Hold.EccSafeMode ? "ECC SAFE MODE · GATES HELD" :
                activeFault >= 0 && FaultLive(activeFault) ? "FAULT TRIAL · GATES HELD" :
                !RailsReady ? "RAILS UNQUALIFIED" : "GROUND STAND READY";
        }
    }
}
