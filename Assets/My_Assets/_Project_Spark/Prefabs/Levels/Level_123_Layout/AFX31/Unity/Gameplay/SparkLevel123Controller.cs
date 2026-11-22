using UnityEngine;
using Stand = ProjectSpark.Gameplay.FlightSovereign.SparkLevel123Stand;

namespace ProjectSpark.Gameplay.FlightSovereign
{
    public sealed partial class SparkLevel123Process
    {
        public SparkTerminal rail, driveRail, monitorSupply, clockPin, secondaryClock, resetPin, barometerReference, protectedFeed;
        public SparkTerminal[] logicPins, sensorPins, motorPins, gatePins;
        public SparkSwitch bench, drivePower, monitorGate, clockGate, secondaryClockGate, resetGate, heartbeat, workload;
        public SparkSwitch[] logicGates, sensorGates, motorPower, outputContacts;
        public SparkSwitch motorContact, imuContact, baroContact;
        public SparkResistor[] motorLoads;
        public Renderer[] sensorMarkers;
        public GameObject assemblyArt;
        public int motorPart, imuPart, baroPart, imuViaIndex;

        public enum SensorFailure { None, MotorGate, ImuClock, Barometer, StandEnvelope }

        private readonly Stand stand = new();
        private readonly bool[] sensorLive = new bool[6];
        private readonly bool[] motorsLive = new bool[8];
        private Stand.Replay replay, protectedReplay;
        private float time, nextSample, nextHeartbeat, lastRise = -1f, measuredSkew, watchdogUntil;
        private bool driverHeartbeat, highFeedback, checkedInputs, watchdogObserved;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset, showSensors, faultEnergized;
        private int epoch, qualifiedEpoch = -1, clockEpoch = -1, calibrationEpoch = -1, runEpoch = -1, minimumEpoch, feedbackIntervals;
        private int activeFault = -1, repaired, repairTarget = -1, viaInspectionEpoch = -1, spare, verifiedPermits;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool RailsReady => powered && Live(rail) && Live(driveRail) && Live(monitorSupply) &&
            Live(resetPin) && Live(logicPins[0]) && Live(logicPins[1]);
        public bool ClockReady => clockEpoch == epoch && Live(clockPin) && Live(secondaryClock);
        public bool Calibrated => calibrationEpoch == epoch && stand.Calibrated;
        public bool Running => replay != null && !replay.Complete;
        public bool ProtectedBusy => protectedReplay != null;
        public bool Qualifies => qualifiedEpoch == epoch && ClockReady && Calibrated && checkedInputs &&
            RailsReady && SensorCount == sensorPins.Length && MotorCount == motorPins.Length && Voltage(barometerReference) > 4.60f;
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .49f;
        public bool WatchdogActive => time < watchdogUntil;
        public bool WatchdogObserved => watchdogObserved;
        public int SensorCount { get; private set; }
        public int MotorCount { get; private set; }
        public int EnabledGates { get; private set; }
        public int Accepted => replay?.QualifiedSamples ?? 0;
        public int Sent => replay?.Sent ?? 0;
        public int MissingMotor => replay?.MissingMotor ?? 0;
        public int StaleB => replay?.StaleB ?? 0;
        public int AltitudeHolds => replay?.AltitudeHolds ?? 0;
        public int OvercurrentCuts => replay?.OvercurrentCuts ?? 0;
        public int EmergencyCuts => replay?.EmergencyCuts ?? 0;
        public float PeakAngle => (replay?.PeakAngle ?? 0f) * Mathf.Rad2Deg;
        public float AltitudeError => replay?.PeakAltitudeError ?? 0f;
        public float LandingHeight => replay?.LastHeight ?? 0f;
        public float MeasuredSkew => measuredSkew;
        public float MotorTestCurrent(int channel) => Mathf.Abs(motorLoads[channel].Current);
        public int Repaired => repaired;
        public int Epoch => epoch;
        public int ProtectedDelivered { get; private set; }
        public int UndeliveredAtTrip { get; private set; }
        public SensorFailure Failure { get; private set; }
        public string Evidence => evidence;

        private Route ImuVia => routes[imuViaIndex];
        private int ExpectedPart => activeFault switch { 0 => motorPart, 1 => imuPart, 2 => baroPart, _ => -1 };
        private bool FaultLive(int index) => activeFault == index && !reworked && faultEnergized;
        private bool Live(SparkTerminal pin) => Voltage(pin) > 4.55f;

        public bool TogglePower()
        {
            if (stage < 2 || stage >= 6 || !Construction || !linked && !interrupted)
                return Reject("Complete native continuity and W401 before enabling the ground stand.");
            if (powered) { IsolatePower(); return true; }
            powered = true;
            faultEnergized = false;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                foreach (var contact in new[] { bench, drivePower, monitorGate, clockGate, secondaryClockGate, resetGate })
                    SetContact(contact, true);
                foreach (var gate in logicGates) SetContact(gate, true);
                foreach (var gate in sensorGates) SetContact(gate, true);
                foreach (var gate in motorPower) SetContact(gate, true);
                foreach (var gate in outputContacts) SetContact(gate, true);
                // The fixture commissions at idle. A staged fault is stressed during the service run.
                SetContact(motorContact, true);
                SetContact(imuContact, true);
                SetContact(baroContact, true);
            }
            ReadStand();
            message = "Verify both clocks and reset release before commissioning the stand.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                foreach (var contact in new[] { bench, drivePower, monitorGate, clockGate, secondaryClockGate, resetGate, heartbeat, workload })
                    SetContact(contact, false);
                foreach (var gate in logicGates) SetContact(gate, false);
                foreach (var gate in sensorGates) SetContact(gate, false);
                foreach (var gate in motorPower) SetContact(gate, false);
                InvalidateDiagnostics();
            }
            ReadStand();
            message = "Bench isolated. Select the diagnosed contact and matching spare.";
        }

        public bool MeasureClock()
        {
            if (!RailsReady || !Live(clockPin) || !Live(secondaryClock) || Running || ProtectedBusy)
                return Reject("Power both clocks and stop the trial before measuring timing.");
            measuredSkew = 0f;
            for (int tick = 0; tick < 24; tick++) measuredSkew = Mathf.Max(measuredSkew, Stand.ClockSkew(tick));
            if (measuredSkew > Stand.MaximumClockSkew) return Reject("Clock disagreement exceeds the training window.");
            clockEpoch = epoch;
            message = "Dual timing measured. Commission the sensors and drive test loads.";
            evidence = $"24 clock samples · {measuredSkew:F0} µs peak skew · 175 µs limit.";
            return true;
        }

        public bool Calibrate()
        {
            if (!RailsReady || !ClockReady || Running || ProtectedBusy || !ReadStand().Ready)
                return Reject("Measure both clocks and verify all sensor and motor return paths.");
            stand.Calibrate();
            calibrationEpoch = epoch;
            qualifiedEpoch = -1;
            checkedInputs = false;
            if (stage == 2) Award();
            evidence = "Calibrated triple IMU, barometer and dual GNSS. All eight test shunts conduct.";
            message = "Challenge the interlocks, then run the 64-sample ground stand.";
            return true;
        }

        public bool CheckGuardRejection()
        {
            if (!Calibrated || !ClockReady || Running || ProtectedBusy || !ReadStand().Ready)
                return Reject("Commission the stand before challenging the interlocks.");
            var inputs = ReadStand();
            bool Test(Stand.Challenge trial, Stand.Hold expected)
            {
                var check = new Stand.Replay();
                stand.Advance(check, inputs, trial);
                return !check.Last.RequestGate && check.Last.Reason == expected;
            }
            checkedInputs = Test(Stand.Challenge.CommonImuBias, Stand.Hold.ImuDisagreement) &&
                Test(Stand.Challenge.Overload, Stand.Hold.Current) && Test(Stand.Challenge.EmergencyCut, Stand.Hold.Emergency);
            if (!checkedInputs) return Reject("The stand did not reject all three challenged states.");
            evidence = "Blocked common-mode IMU bias, overcurrent and emergency cut.";
            message = "Interlock tests passed. Run the ground stand.";
            return true;
        }

        public bool RunStand()
        {
            if (stage is < 3 or > 5 || !RailsReady || !linked || !Calibrated || !checkedInputs ||
                Running || ProtectedBusy || !ClockReady || WatchdogActive)
                return Reject("Measure, commission and test the interlocks before the stand run.");
            if (activeFault >= 0 && !reworked) EnergizeFault();
            replay = new Stand.Replay();
            qualifiedEpoch = runEpoch = -1;
            Failure = SensorFailure.None;
            nextSample = time;
            scope?.ResetTrace();
            message = "Running spin-up, gust recovery, descent, overload and emergency cut.";
            return true;
        }

        private Stand.Inputs ReadStand()
        {
            SensorCount = MotorCount = 0;
            int mask = 0;
            for (int i = 0; i < sensorLive.Length; i++)
            {
                sensorLive[i] = powered && Live(sensorPins[i]);
                if (sensorLive[i]) SensorCount++;
            }
            for (int i = 0; i < motorsLive.Length; i++)
            {
                motorsLive[i] = powered && Live(motorPins[i]) && MotorTestCurrent(i) > .035f;
                if (!motorsLive[i]) continue;
                MotorCount++;
                mask |= 1 << i;
            }
            return new Stand.Inputs(mask, sensorLive[0], sensorLive[1], sensorLive[2], sensorLive[3], sensorLive[4],
                sensorLive[5], Voltage(barometerReference));
        }

        private void TickController(float dt)
        {
            time += dt;
            ReadStand();
            if (stage is >= 2 and <= 6) UpdateSupervisor();
            if (time < nextSample || !Running && !ProtectedBusy) return;
            bool protect = ProtectedBusy;
            var run = protect ? protectedReplay : replay;
            if (protect && !Fresh)
            {
                AbortProtected();
                message = "Trusted readiness expired. Requalify the stand before authorizing its gates.";
                return;
            }

            if (FaultLive(1)) SetContact(imuContact, !Stand.ImuDropout(run.Sent));
            stand.Advance(run, ReadStand());
            nextSample = time + (protect ? .051f : .028f);
            var sample = run.Last;
            scope?.Record(Mathf.Clamp01(sample.Mix), Mathf.Clamp01(sample.PeakDemand / 2.7f),
                Mathf.Clamp01(sample.Height / .75f), Mathf.Clamp01(sample.Attitude * 4f));
            if (!protect)
            {
                if (run.Complete) FinishStand(run);
                return;
            }

            SetContact(workload, Fresh && sample.RequestGate);
            ReadEnabledGates();
            if (EnabledGates == (sample.RequestGate ? 8 : 0)) verifiedPermits++;
            ProtectedDelivered = run.Sent;
            if (run.Complete) FinishProtected(run);
        }

        private void FinishStand(Stand.Replay report)
        {
            runEpoch = epoch;
            if (FaultLive(1))
            {
                SetContact(imuContact, false);
                ReadStand();
            }
            Failure = report.MissingMotor > 0 ? SensorFailure.MotorGate : report.StaleB > 0 ? SensorFailure.ImuClock :
                report.AltitudeHolds > 0 ? SensorFailure.Barometer : report.Qualified ? SensorFailure.None : SensorFailure.StandEnvelope;
            evidence = $"{report.QualifiedSamples}/64 qualified · {report.PeakAngle * Mathf.Rad2Deg:F1}° peak tilt\n" +
                $"{report.OvercurrentCuts} current cuts · {report.EmergencyCuts} e-stop holds · {report.LastHeight:F2} m landing";
            if (!report.Qualified)
            {
                message = Failure switch
                {
                    SensorFailure.MotorGate => "M5 gate drive open. Probe the missing test current.",
                    SensorFailure.ImuClock => "IMU B timing stale. Probe the SPI clock via.",
                    SensorFailure.Barometer => "Barometer and dual GNSS disagree. Probe the pressure reference.",
                    _ => "Stand envelope exceeded. Inspect the trial measurements."
                };
                return;
            }

            qualifiedEpoch = epoch;
            if (stage == 3 && Award()) message = "Stand qualified. Begin the three hardware fault trials.";
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                faultWitnessed = reworked = false;
                QueueCheckpoint();
                message = repaired == 3 && Award() ? "Repairs verified. Run the protected stand and open W401 during gate drive." :
                    "Repair verified by fresh stand trials. Begin the next fault.";
            }
            else if (stage == 4) message = "Stand evidence refreshed. Begin the next hardware fault.";
            else if (stage == 5) message = interrupted ? "Stand requalified. Wait for trusted feedback, then reset U3." :
                "Stand requalified. Wait for trusted feedback before starting a protected run.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Qualifies || Running)
                return Reject("Finish a healthy ground-stand run before staging a fault.");
            activeFault = repaired;
            faultWitnessed = reworked = false;
            repairTarget = viaInspectionEpoch = -1;
            spare = 0;
            qualifiedEpoch = runEpoch = -1;
            EnergizeFault();
            message = "Fault staged. Run the stand to capture the measured signature.";
            QueueCheckpoint();
            return true;
        }

        private void EnergizeFault()
        {
            faultEnergized = true;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                SetContact(motorContact, !FaultLive(0));
                SetContact(imuContact, !FaultLive(1));
                SetContact(baroContact, !FaultLive(2));
            }
        }

        public bool ProbeFault()
        {
            if (!powered || runEpoch != epoch || activeFault < 0 || reworked)
                return Reject("Run the stand with the fault present before probing.");
            bool measured = activeFault switch
            {
                0 => Failure == SensorFailure.MotorGate && MissingMotor > 0 && Live(driveRail) &&
                    MotorTestCurrent(4) < .001f && MotorTestCurrent(3) > .035f,
                1 => Failure == SensorFailure.ImuClock && StaleB > 0 && Live(sensorPins[0]) &&
                    !Live(sensorPins[1]) && Live(sensorPins[2]),
                2 => Failure == SensorFailure.Barometer && AltitudeHolds > 0 &&
                    Live(sensorPins[3]) && Live(sensorPins[4]) && Live(sensorPins[5]) && Voltage(barometerReference) < 4.3f,
                _ => false
            };
            if (!measured) return Reject("The live reading and stand trace do not identify this fault.");
            footprint = ExpectedPart;
            faultWitnessed = true;
            evidence = activeFault switch
            {
                0 => $"M5 shunt {MotorTestCurrent(4) * 1000f:F1} mA · M4 {MotorTestCurrent(3) * 1000f:F1} mA · {MissingMotor} drive holds.",
                1 => $"{ImuVia.viaId} · {StaleB} stale IMU B samples · A and C remained live.",
                _ => $"U10 ref {Voltage(barometerReference):F2} V · {AltitudeError:F2} m altitude disagreement."
            };
            message = $"Isolate power. Select {parts[ExpectedPart].label} and its matching spare.";
            QueueCheckpoint();
            return true;
        }

        public bool InspectImuVia()
        {
            if (activeFault != 1 || !faultWitnessed || powered) return Reject("Probe IMU B and isolate power first.");
            if (selectedRoute != imuViaIndex || layer != ImuVia.layer)
            {
                SelectRoute(imuViaIndex);
                PadView();
                return Reject($"Inspect {ImuVia.viaId} on entry layer L{ImuVia.layer:D2}.");
            }
            viaInspectionEpoch = epoch;
            message = $"Confirmed {ImuVia.viaId} from L{ImuVia.layer:D2} to L{ImuVia.exitLayer:D2}.";
            return true;
        }

        public void SelectSpare()
        {
            spare = (spare + 1) % 4;
            message = spare switch
            {
                1 => "Spare · J5 high-side gate contact",
                2 => "Spare · U8 SPI via contact",
                3 => "Spare · U10 barometer trim contact",
                _ => "No spare selected."
            };
        }

        public bool Repair()
        {
            if (stage != 4 || activeFault < 0 || !faultWitnessed || reworked || powered)
                return Reject("Diagnose and isolate the bench before rework.");
            if (spare != activeFault + 1 || repairTarget != ExpectedPart ||
                activeFault == 1 && (viaInspectionEpoch != epoch || layer != ImuVia.layer))
                return Reject("Match the spare and footprint. Inspect the SPI via entry layer when required.");
            var replacement = activeFault switch { 0 => motorContact, 1 => imuContact, _ => baroContact };
            SetContact(replacement, true);
            reworked = replacement.IsConducting;
            if (!reworked) return Reject("The replacement contact did not seat.");
            InvalidateDiagnostics();
            QueueCheckpoint();
            BoardView();
            message = "Replacement seated. Measure, recommission and repeat the stand run.";
            return true;
        }

        public bool TestWatchdog()
        {
            if (stage != 5 || !Fresh || interrupted || Running || ProtectedBusy || WatchdogActive)
                return Reject("The watchdog check needs a ready, idle stand before W401 removal.");
            watchdogObserved = false;
            watchdogUntil = time + .95f;
            message = "Holding the simulated readiness contact high. Watch U3 reject a stale edge.";
            return true;
        }

        public bool StartProtected()
        {
            if (stage != 5 || !Fresh || interrupted && !safetyReset || Running || ProtectedBusy || WatchdogActive)
                return Reject("U3 requires fresh commissioning and time-varying readiness before authorizing the stand.");
            armed = true;
            protectedReplay = new Stand.Replay();
            ProtectedDelivered = verifiedPermits = 0;
            nextSample = time;
            scope?.ResetTrace();
            message = interrupted ? "Verifying every protected gate, including current and emergency cuts." :
                "Protected run active. Open W401 while the gates are enabled.";
            return true;
        }

        private void FinishProtected(Stand.Replay report)
        {
            protectedReplay = null;
            armed = false;
            SetContact(workload, false);
            ReadEnabledGates();
            if (interrupted && safetyReset && report.Qualified && verifiedPermits == Stand.FrameCount && Fresh && Award())
            {
                message = "W401 recovered. U3 verified all eight gates through landing and emergency cut.";
                evidence = $"{ProtectedDelivered}/64 gate checks · final height {report.LastHeight:F2} m · gates cut.";
            }
            else if (!interrupted) message = "Repeat the protected run and open W401 while its gates are enabled.";
            else message = "Protected trials need fresh evidence and all eight gate checks.";
        }

        private void AbortProtected()
        {
            UndeliveredAtTrip = Stand.FrameCount - ProtectedDelivered;
            protectedReplay = null;
            armed = safetyReset = false;
            SetContact(workload, false);
            minimumEpoch = epoch + 1;
            manager.ElectricalSolver.SolveNow();
            ReadEnabledGates();
        }

        public bool OpenW401()
        {
            if (stage != 5 || !linked || interrupted || !ProtectedBusy || ProtectedDelivered == 0 || Inhibited)
                return Reject("Open W401 during a protected run while the gates are enabled.");
            return RemoveWire(jumper) || Reject("The native registry could not open W401.");
        }

        public bool RestoreW401()
        {
            if (stage != 5 || !interrupted || linked) return Reject("Open W401 during a protected run first.");
            if (!CreateWire(jumper)) return Reject("The native circuit rejected the W401 training contacts.");
            ReadGraph();
            message = "W401 restored. U3 still requires new sensor and stand evidence.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !Fresh || epoch < minimumEpoch || ProtectedBusy)
                return Reject("Requalify the stand and wait for fresh feedback before resetting U3.");
            safetyReset = true;
            message = "U3 reset. Run the final protected stand.";
            return true;
        }

        private void InvalidateDiagnostics()
        {
            epoch++;
            clockEpoch = calibrationEpoch = qualifiedEpoch = runEpoch = -1;
            checkedInputs = false;
            stand.ClearCalibration();
            replay = protectedReplay = null;
            feedbackIntervals = 0;
            lastRise = watchdogUntil = -1f;
            highFeedback = false;
            SetContact(workload, false);
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
            bool permit = stage == 5 && Fresh && (!interrupted || safetyReset) &&
                ProtectedBusy && protectedReplay.Sent > 0 && protectedReplay.Last.RequestGate;
            SetContact(workload, permit);
            ReadEnabledGates();
            benchState = stage == 6 ? "AFX-31 QUALIFIED" : !powered ? "BENCH ISOLATED" :
                interrupted && !safetyReset ? linked ? "U3 LATCHED · RESET REQUIRED" : "W401 OPEN · U3 LATCHED" :
                activeFault >= 0 && FaultLive(activeFault) ? "FAULT TRIAL · GATES HELD" :
                !RailsReady ? "RAILS UNQUALIFIED" : "GROUND STAND READY";
        }
    }
}
