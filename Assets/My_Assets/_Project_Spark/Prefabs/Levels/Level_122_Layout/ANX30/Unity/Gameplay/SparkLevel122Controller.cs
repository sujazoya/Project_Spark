using UnityEngine;
using Navigation = ProjectSpark.Gameplay.AutonomySentinel.SparkLevel122Navigation;

namespace ProjectSpark.Gameplay.AutonomySentinel
{
    public sealed partial class SparkLevel122Process
    {
        public SparkTerminal rail, monitorSupply, clockPin, pllPin, referencePin, navWitness, protectedFeed;
        public SparkTerminal[] sensorPins;
        public SparkSwitch bench, monitorGate, clockGate, pllGate, referenceGate, heartbeat, workload;
        public SparkSwitch pllContact, rfContact, imuContact;
        public SparkSwitch[] sensorGates;
        public Renderer[] sensorMarkers;
        public GameObject assemblyArt;
        public int pllPart, gnssPart, imuPart, gnssViaIndex;

        public enum SensorFailure { None, RadarPll, GnssAFeed, ImuReference, Navigation }
        private readonly Navigation navigation = new();
        private readonly bool[] sensorLive = new bool[6];
        private Navigation.Replay replay, protectedReplay;
        private float time, nextSample, nextHeartbeat, lastRise = -1f, measuredPhase;
        private bool driverHeartbeat, highFeedback, checkedInputs;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset, showSensors, faultEnergized;
        private int epoch, qualifiedEpoch = -1, clockEpoch = -1, calibrationEpoch = -1, runEpoch = -1, minimumEpoch, feedbackIntervals;
        private int activeFault = -1, repaired, repairTarget = -1, viaInspectionEpoch = -1, spare, verifiedPermits;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool RailsReady => powered && Live(rail) && Live(monitorSupply) && Live(navWitness);
        public bool ClockReady => clockEpoch == epoch && Live(clockPin) && Live(pllPin);
        public bool Calibrated => calibrationEpoch == epoch && navigation.Calibrated;
        public bool Running => replay != null && !replay.Complete;
        public bool ProtectedBusy => protectedReplay != null;
        public bool Qualifies => qualifiedEpoch == epoch && ClockReady && Calibrated && checkedInputs &&
            RailsReady && SensorCount == sensorPins.Length && Live(referencePin);
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .49f;
        public int SensorCount { get; private set; }
        public int Accepted => replay?.Trusted ?? 0;
        public int Sent => replay?.Sent ?? 0;
        public int SafeDecisions => replay?.SafeDecisions ?? 0;
        public int BrakeHolds => replay?.BrakeHolds ?? 0;
        public int BoundaryHolds => replay?.BoundaryHolds ?? 0;
        public int AgedCommands => replay?.AgedCommands ?? 0;
        public int Indoor => replay?.Indoor ?? 0;
        public int MissingA => replay?.MissingA ?? 0;
        public int BadRadar => replay?.BadRadar ?? 0;
        public int Diverged => replay?.Diverged ?? 0;
        public float PeakError => replay?.MaxError ?? 0f;
        public float MaxUncertainty => replay?.MaxUncertainty ?? 0f;
        public float PeakPhase => replay?.PeakPhase ?? 0f;
        public float PeakImuDelta => replay?.PeakImuDelta ?? 0f;
        public float MeasuredPhase => measuredPhase;
        public int Repaired => repaired;
        public int Epoch => epoch;
        public int ProtectedDelivered { get; private set; }
        public int UndeliveredAtTrip { get; private set; }
        public SensorFailure Failure { get; private set; }
        public string Evidence => evidence;

        private Route GnssVia => routes[gnssViaIndex];
        private int ExpectedPart => activeFault switch { 0 => pllPart, 1 => gnssPart, 2 => imuPart, _ => -1 };
        private bool FaultLive(int index) => activeFault == index && !reworked && faultEnergized;
        private bool Live(SparkTerminal pin) => Voltage(pin) > 4.55f;

        public bool TogglePower()
        {
            if (stage < 2 || stage >= 6 || !Construction || !linked && !interrupted)
                return Reject("Complete native continuity and W385 before enabling the training bench.");
            if (powered) { IsolatePower(); return true; }
            powered = true;
            faultEnergized = false;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                foreach (var contact in new[] { bench, monitorGate, clockGate, pllGate, referenceGate })
                    SetContact(contact, true);
                foreach (var gate in sensorGates) SetContact(gate, true);
                // The reference fixture commissions at idle. Staged contacts are stressed during the service run.
                SetContact(pllContact, true);
                SetContact(rfContact, true);
                SetContact(imuContact, true);
            }
            ReadSensors();
            message = "Measure the U17 reference and U18 radar PLL before commissioning.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                foreach (var contact in new[] { bench, monitorGate, clockGate, pllGate, referenceGate, heartbeat, workload })
                    SetContact(contact, false);
                foreach (var gate in sensorGates) SetContact(gate, false);
                InvalidateDiagnostics();
            }
            ReadSensors();
            message = "Bench isolated. Select the diagnosed board site and matching spare.";
        }

        public bool MeasureClock()
        {
            if (!RailsReady || !Live(clockPin) || Running || ProtectedBusy)
                return Reject("Power the U17 reference and stop the acquisition before measuring.");
            measuredPhase = 0f;
            bool trim = Live(pllPin);
            for (int frame = 0; frame < 24; frame++)
                measuredPhase = Mathf.Max(measuredPhase, Navigation.Phase(frame, trim));
            if (measuredPhase > Navigation.MaximumPhaseNs)
            {
                clockEpoch = qualifiedEpoch = -1;
                evidence = $"Radar PLL jitter {measuredPhase:F1} ns exceeds the 62 ns training limit.";
                return Reject("Radar timing is out of tolerance. A service replay can measure its effect.");
            }
            clockEpoch = epoch;
            message = "Reference and PLL measured. Commission the six sensor paths.";
            evidence = $"24 clock samples · {measuredPhase:F1} ns peak PLL jitter · 62 ns limit.";
            return true;
        }

        public bool Calibrate()
        {
            if (!RailsReady || !ClockReady || Running || ProtectedBusy || !ReadSensors().Ready)
                return Reject("Measure both clocks and verify all six sensor paths and the IMU reference.");
            navigation.Calibrate();
            calibrationEpoch = epoch;
            qualifiedEpoch = -1;
            checkedInputs = false;
            if (stage == 2) Award();
            evidence = "Dual GNSS offsets, radar reflectors and stationary IMU bias measured.";
            message = "Challenge the safety checks, then replay navigation through the facility.";
            return true;
        }

        public bool CheckGuardRejection()
        {
            if (!Calibrated || !ClockReady || Running || ProtectedBusy || !ReadSensors().Ready)
                return Reject("Commission all sensor paths before testing the supervisor.");
            var inputs = ReadSensors();
            bool Test(Navigation.Challenge stimulus, Navigation.Hold expected)
            {
                var challenge = new Navigation.Replay();
                navigation.Advance(challenge, inputs, stimulus);
                return !challenge.Last.Move && challenge.Last.Reason == expected;
            }
            checkedInputs = Test(Navigation.Challenge.Spoof, Navigation.Hold.SatelliteIntegrity) &&
                Test(Navigation.Challenge.Stale, Navigation.Hold.StaleCommand) &&
                Test(Navigation.Challenge.ImuSplit, Navigation.Hold.ImuDivergence);
            if (!checkedInputs) return Reject("The supervisor did not hold on every challenged input.");
            evidence = "Blocked a displaced GNSS fix, an aged command and a split IMU reference.";
            message = "Guard tests passed. Start the 64-sample navigation replay.";
            return true;
        }

        public bool RunNavigation()
        {
            if (stage is < 3 or > 5 || !RailsReady || !linked || !Calibrated || !checkedInputs ||
                Running || ProtectedBusy || !ClockReady && activeFault != 0)
                return Reject("Measure, commission and prove the navigation guard checks before replay.");
            if (activeFault >= 0 && !reworked) EnergizeFault();
            replay = new Navigation.Replay();
            qualifiedEpoch = runEpoch = -1;
            Failure = SensorFailure.None;
            nextSample = time;
            scope?.ResetTrace();
            message = "Replaying outdoor localization, indoor radar stops and the exit geofence.";
            return true;
        }

        private Navigation.Inputs ReadSensors()
        {
            SensorCount = 0;
            for (int i = 0; i < sensorLive.Length; i++)
            {
                sensorLive[i] = powered && Live(sensorPins[i]);
                if (sensorLive[i]) SensorCount++;
            }
            return new Navigation.Inputs(sensorLive[0], sensorLive[1], sensorLive[2], sensorLive[3],
                sensorLive[4], sensorLive[5], Live(clockPin), Live(pllPin), Live(referencePin));
        }

        private void TickController(float dt)
        {
            time += dt;
            ReadSensors();
            if (stage is >= 3 and <= 6) UpdateSupervisor();
            if (time < nextSample || !Running && !ProtectedBusy) return;
            bool protect = ProtectedBusy;
            var run = protect ? protectedReplay : replay;
            if (protect && !Fresh)
            {
                AbortProtected();
                message = "Trusted feedback expired. Requalify before requesting motion.";
                return;
            }

            if (FaultLive(1)) SetContact(rfContact, !Navigation.GnssDropout(run.Sent));
            navigation.Advance(run, ReadSensors());
            nextSample = time + (protect ? .047f : .028f);
            var sample = run.Last;
            scope?.Record(Mathf.Clamp01(sample.RadarRange / 2.4f), sample.Confidence,
                Mathf.Clamp01(sample.PhaseNs / 180f), Mathf.Clamp01(sample.Clearance / 1.8f));
            if (protect)
            {
                SetContact(workload, Fresh && sample.Move);
                if (Live(protectedFeed) == sample.Move) verifiedPermits++;
                ProtectedDelivered = run.Sent;
                if (run.Complete) FinishProtected(run);
            }
            else if (run.Complete) FinishNavigation(run);
        }

        private void FinishNavigation(Navigation.Replay report)
        {
            runEpoch = epoch;
            if (FaultLive(1))
            {
                SetContact(rfContact, false);
                ReadSensors();
            }
            Failure = report.Diverged > 0 ? SensorFailure.ImuReference : report.BadRadar > 0 ? SensorFailure.RadarPll :
                report.MissingA > 0 ? SensorFailure.GnssAFeed : report.Qualified ? SensorFailure.None : SensorFailure.Navigation;
            evidence = $"Peak pose error {report.MaxError:F2} m · uncertainty {report.MaxUncertainty:F2} m\n" +
                $"{report.SafeDecisions}/64 correct · {report.BrakeHolds} obstacle stops · {report.BoundaryHolds} boundary holds";
            if (!report.Qualified)
            {
                message = Failure switch
                {
                    SensorFailure.RadarPll => "RADAR_PLL_UNQUALIFIED · probe the range disagreement.",
                    SensorFailure.GnssAFeed => "RF_FEED_OPEN · probe the GNSS A contact.",
                    SensorFailure.ImuReference => "IMU_DIVERGENCE · probe the redundant reference.",
                    _ => "LOCALIZATION_UNCERTAIN · inspect the trial evidence."
                };
                return;
            }

            qualifiedEpoch = epoch;
            if (stage == 3 && Award()) message = "Navigation qualified. Begin the controlled hardware fault trials.";
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                faultWitnessed = reworked = false;
                QueueCheckpoint();
                message = repaired == 3 && Award() ? "All repairs verified. Start a protected drive and open W385." :
                    "Repair verified by fresh trials. Begin the next hardware fault.";
            }
            else if (stage == 4) message = "Navigation verified. Begin the next controlled hardware fault.";
            else if (stage == 5) message = interrupted ? "Fresh trials passed. Wait for trusted feedback, then reset U4." :
                "Fresh trials passed. Wait for feedback, then start a protected drive.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Qualifies || Running)
                return Reject("Complete healthy navigation trials before staging a hardware fault.");
            activeFault = repaired;
            faultWitnessed = reworked = false;
            repairTarget = viaInspectionEpoch = -1;
            spare = 0;
            qualifiedEpoch = runEpoch = -1;
            EnergizeFault();
            message = "A hardware fault is staged. Run navigation to measure its effect.";
            QueueCheckpoint();
            return true;
        }

        private void EnergizeFault()
        {
            faultEnergized = true;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                SetContact(pllContact, !FaultLive(0));
                SetContact(rfContact, !FaultLive(1));
                SetContact(imuContact, !FaultLive(2));
            }
        }

        public bool ProbeFault()
        {
            if (!powered || runEpoch != epoch || activeFault < 0 || reworked)
                return Reject("Replay the hardware fault to obtain fresh observations.");
            bool measured = activeFault switch
            {
                0 => Failure == SensorFailure.RadarPll && BadRadar > 0 && Live(clockPin) && !Live(pllPin),
                1 => Failure == SensorFailure.GnssAFeed && MissingA > 0 && Live(sensorPins[1]) && !Live(sensorPins[0]),
                2 => Failure == SensorFailure.ImuReference && Diverged > 0 && Live(sensorPins[4]) && Live(sensorPins[5]) && !Live(referencePin),
                _ => false
            };
            if (!measured) return Reject("The live reading and replay do not identify this fault.");
            footprint = ExpectedPart;
            faultWitnessed = true;
            evidence = activeFault switch
            {
                0 => $"U18 trim {Voltage(pllPin):F2} V · {PeakPhase:F1} ns peak jitter · {BadRadar} radar holds.",
                1 => $"{GnssVia.viaId} · {MissingA} outdoor GNSS A dropouts · GNSS B stayed live.",
                _ => $"U11 reference {Voltage(referencePin):F2} V · {PeakImuDelta * Mathf.Rad2Deg:F1}° mismatch · {Diverged} holds."
            };
            message = $"Isolate power. Select {parts[ExpectedPart].label} and its matching spare.";
            QueueCheckpoint();
            return true;
        }

        public bool InspectGnssVia()
        {
            if (activeFault != 1 || !faultWitnessed || powered) return Reject("Probe the GNSS A fault and isolate power first.");
            if (selectedRoute != gnssViaIndex || layer != GnssVia.layer)
            {
                SelectRoute(gnssViaIndex);
                PadView();
                return Reject($"Inspect {GnssVia.viaId} on entry layer L{GnssVia.layer:D2}.");
            }
            viaInspectionEpoch = epoch;
            message = $"Confirmed {GnssVia.viaId} from L{GnssVia.layer:D2} to L{GnssVia.exitLayer:D2}.";
            return true;
        }

        public void SelectSpare()
        {
            spare = (spare + 1) % 4;
            message = spare switch
            {
                1 => "Spare · U18 radar PLL contact",
                2 => "Spare · GNSS A feed via contact",
                3 => "Spare · U11 inertial reference contact",
                _ => "No spare selected."
            };
        }

        public bool Repair()
        {
            if (stage != 4 || activeFault < 0 || !faultWitnessed || reworked || powered)
                return Reject("Diagnose and isolate the bench before rework.");
            if (spare != activeFault + 1 || repairTarget != ExpectedPart ||
                activeFault == 1 && (viaInspectionEpoch != epoch || layer != GnssVia.layer))
                return Reject("Match the spare and board site. Inspect the GNSS via entry layer when required.");
            var replacement = activeFault switch { 0 => pllContact, 1 => rfContact, _ => imuContact };
            SetContact(replacement, true);
            reworked = replacement.IsConducting;
            if (!reworked) return Reject("The replacement contact did not seat.");
            InvalidateDiagnostics();
            QueueCheckpoint();
            BoardView();
            message = "Replacement fitted. Power up, recommission and repeat the navigation trials.";
            return true;
        }

        public bool StartProtected()
        {
            if (stage != 5 || !Fresh || Inhibited || interrupted && !safetyReset || Running || ProtectedBusy)
                return Reject("U4 requires fresh navigation evidence and a solved permit before motion.");
            armed = true;
            protectedReplay = new Navigation.Replay();
            ProtectedDelivered = verifiedPermits = 0;
            nextSample = time;
            message = interrupted ? "Rechecking authorized drive and stop decisions." :
                "Drive active. Open W385 while U4 permits motion.";
            return true;
        }

        private void FinishProtected(Navigation.Replay report)
        {
            protectedReplay = null;
            armed = false;
            UpdateSupervisor();
            if (interrupted && safetyReset && report.Qualified && verifiedPermits == Navigation.FrameCount && Fresh && Award())
            {
                message = "W385 recovery verified. U4 checked all drive, stop and boundary decisions.";
                evidence = $"{ProtectedDelivered}/64 verified gate decisions · {report.MaxError:F2} m peak pose error.";
            }
            else if (!interrupted) message = "Start another run and open W385 while U4 permits motion.";
            else message = "The protected trials need fresh evidence and all motion-gate decisions.";
        }

        private void AbortProtected()
        {
            UndeliveredAtTrip = Navigation.FrameCount - ProtectedDelivered;
            protectedReplay = null;
            armed = safetyReset = false;
            SetContact(workload, false);
            minimumEpoch = epoch + 1;
            manager.ElectricalSolver.SolveNow();
        }

        public bool OpenW385()
        {
            if (stage != 5 || !linked || interrupted || !ProtectedBusy || ProtectedDelivered == 0 || Inhibited)
                return Reject("Open W385 during a drive sample with an active motion permit.");
            return RemoveWire(jumper) || Reject("The native registry could not open W385.");
        }

        public bool RestoreW385()
        {
            if (stage != 5 || !interrupted || linked) return Reject("Open W385 during an authorized drive first.");
            if (!CreateWire(jumper)) return Reject("The native circuit rejected the W385 training contacts.");
            ReadGraph();
            message = "W385 restored. U4 still requires new sensor and navigation evidence.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !Fresh || epoch < minimumEpoch || ProtectedBusy)
                return Reject("Requalify navigation and wait for fresh feedback before resetting U4.");
            safetyReset = true;
            UpdateSupervisor();
            message = "U4 reset. Run the final protected drive and stop trials.";
            return true;
        }

        private void InvalidateDiagnostics()
        {
            epoch++;
            clockEpoch = calibrationEpoch = qualifiedEpoch = runEpoch = -1;
            checkedInputs = false;
            navigation.ClearCalibration();
            replay = protectedReplay = null;
            feedbackIntervals = 0;
            lastRise = -1f;
            highFeedback = false;
            SetContact(workload, false);
        }

        private void UpdateSupervisor()
        {
            if (time >= nextHeartbeat)
            {
                nextHeartbeat = time + .14f;
                driverHeartbeat = !driverHeartbeat && Qualifies;
                SetContact(heartbeat, driverHeartbeat);
                bool high = linked && Live(jumper.to);
                if (high && !highFeedback)
                {
                    feedbackIntervals = lastRise >= 0 && time - lastRise is > .21f and < .49f ? feedbackIntervals + 1 : 0;
                    lastRise = time;
                }
                highFeedback = high;
            }
            bool permit = stage >= 5 && Fresh && (!interrupted || safetyReset) &&
                (!ProtectedBusy || protectedReplay.Sent == 0 || protectedReplay.Last.Move);
            SetContact(workload, permit);
            benchState = stage == 6 ? "ANX-30 QUALIFIED" : !powered ? "BENCH ISOLATED" : interrupted && !safetyReset ?
                "SAFETY_LINK_OPEN · U4 LATCHED" : !RailsReady ? "RAILS UNQUALIFIED" :
                activeFault >= 0 && FaultLive(activeFault) ? "FAULT TRIAL · MOTION HELD" : "NAVIGATION BENCH READY";
        }
    }
}
