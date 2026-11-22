using UnityEngine;

namespace ProjectSpark.Gameplay.PerceptionNexus
{
    public sealed partial class SparkLevel121Process
    {
        public SparkTerminal rail, monitorSupply, clockPin, trimPin, resetPin, imuPin, lidarBias, lidarPin, fusionWitness, protectedFeed;
        public SparkTerminal[] cameraPins;
        public SparkSwitch bench, monitorGate, clockGate, trimGate, resetGate, imuGate, lidarGate, lidarPulse, heartbeat, workload;
        public SparkSwitch[] cameraGates;
        public SparkResistor filterLoss;
        public Renderer[] sensorMarkers;
        public GameObject assemblyArt;
        public int cameraPart, clockPart, filterPart, cameraViaIndex;

        public enum SensorFailure { None, CameraB, TriggerSkew, LidarSupply, FusionLimits }
        private readonly SparkLevel121Fusion fusion = new();
        private readonly bool[] cameras = new bool[4];
        private SparkLevel121Fusion.Replay replay;
        private SparkLevel121Fusion.Replay protectedReplay;
        private float time, nextSample, nextHeartbeat, lastRise = -1f, measuredSkew, idleRail, pulseRail;
        private bool driverHeartbeat, highFeedback, checkedInputs;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset, showSensors, faultEnergized;
        private int epoch, qualifiedEpoch = -1, clockEpoch = -1, calibrationEpoch = -1, runEpoch = -1, minimumEpoch, feedbackIntervals;
        private int activeFault = -1, repaired, repairTarget = -1, viaInspectionEpoch = -1, spare;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool RailsReady => powered && Live(rail) && Live(monitorSupply) && Live(resetPin) && Live(fusionWitness);
        public bool ClockReady => clockEpoch == epoch && Live(clockPin) && Live(trimPin);
        public bool Calibrated => calibrationEpoch == epoch && fusion.Calibrated;
        public bool Running => replay != null && !replay.Complete;
        public bool ProtectedBusy => protectedReplay != null;
        public bool Qualifies => qualifiedEpoch == epoch && ClockReady && Calibrated && checkedInputs &&
            RailsReady && CameraCount == 4 && Live(lidarBias) && Live(imuPin) && Voltage(lidarPin) >= 4.52f;
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .49f;
        public int CameraCount { get; private set; }
        public int Accepted => replay?.Accepted ?? 0;
        public int Sent => replay?.Sent ?? 0;
        public int Obstacles => replay?.Obstacles ?? 0;
        public int CorrectObstacles => replay?.CorrectObstacles ?? 0;
        public int MissingB => replay?.MissingB ?? 0;
        public int Stale => replay?.Stale ?? 0;
        public int RangeDrops => replay?.RangeDrops ?? 0;
        public float PeakErrorMm => replay?.PeakErrorMm ?? 0;
        public float MinConfidence => replay?.MinimumConfidence ?? 0;
        public float ReplaySkew => replay?.PeakSkew ?? 0;
        public float MinRail => replay?.MinimumRail ?? 0;
        public float Sag => replay?.PeakSag ?? 0;
        public float MeasuredSkew => measuredSkew;
        public float PulseRail => pulseRail;
        public int Repaired => repaired;
        public int Epoch => epoch;
        public int ProtectedDelivered { get; private set; }
        public int UndeliveredAtTrip { get; private set; }
        public SensorFailure Failure { get; private set; }
        public string Evidence => evidence;

        private Route CameraVia => routes[cameraViaIndex];
        private int ExpectedPart => activeFault switch { 0 => cameraPart, 1 => clockPart, 2 => filterPart, _ => -1 };
        private bool FaultLive(int fault) => activeFault == fault && !reworked && faultEnergized;
        private bool Live(SparkTerminal pin) => Voltage(pin) > 4.55f;

        public bool TogglePower()
        {
            if (stage < 2 || stage >= 6 || !Construction || !linked && !interrupted)
                return Reject("Complete native continuity and W369 before enabling the training bench.");
            if (powered) { IsolatePower(); return true; }
            powered = true;
            faultEnergized = false;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                SetContact(bench, true);
                SetContact(monitorGate, true);
                SetContact(resetGate, true);
                SetContact(clockGate, true);
                SetContact(trimGate, !FaultLive(1));
                SetContact(imuGate, true);
                SetContact(lidarGate, true);
                foreach (var gate in cameraGates) SetContact(gate, true);
                filterLoss.SetResistance(FaultLive(2) ? 22f : .12f);
            }
            message = "Measure the common PPS and trigger distribution before calibrating.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                foreach (var contact in new[] { bench, monitorGate, resetGate, clockGate, trimGate, imuGate,
                    lidarGate, lidarPulse, heartbeat, workload })
                    SetContact(contact, false);
                foreach (var gate in cameraGates) SetContact(gate, false);
                InvalidateDiagnostics();
            }
            message = "Bench isolated. Select the diagnosed board site and matching spare.";
        }

        public bool MeasureClock()
        {
            if (!RailsReady || !Live(clockPin) || Running || ProtectedBusy)
                return Reject("Power the trigger reference and stop live acquisition before measuring.");
            measuredSkew = 0f;
            bool trim = Live(trimPin);
            for (int frame = 0; frame < 24; frame++)
                measuredSkew = Mathf.Max(measuredSkew, SparkLevel121Fusion.TriggerSkew(frame, trim));
            if (measuredSkew > SparkLevel121Fusion.MaxSkewUs)
            {
                clockEpoch = qualifiedEpoch = -1;
                evidence = $"Trigger skew {measuredSkew:F0} µs exceeds the 220 µs training window.";
                return Reject("Timing out of budget. A service replay can identify stale pairings.");
            }
            clockEpoch = epoch;
            evidence = $"24 trigger samples · {measuredSkew:F0} µs peak skew / 220 µs limit.";
            message = "Trigger measured. Calibrate both stereo pairs, LiDAR and IMU.";
            return true;
        }

        public bool Calibrate()
        {
            if (!RailsReady || !ClockReady || Running || ProtectedBusy)
                return Reject("Finish the measured trigger check before calibrating.");
            if (!ReadSensors(false) || CameraCount != 4 || pulseRail < 4.52f)
                return Reject("Calibration needs all four camera returns, a valid LiDAR pulse and live IMU.");
            fusion.Calibrate();
            calibrationEpoch = epoch;
            qualifiedEpoch = -1;
            checkedInputs = false;
            if (stage == 2) Award();
            evidence = "Four camera pixel offsets, LiDAR range and IMU bias measured against the reference fixture.";
            message = "Check frame rejection, then run a fresh 64-set fusion replay.";
            return true;
        }

        public bool CheckFrameRejection()
        {
            if (!Calibrated || !ClockReady || Running || ProtectedBusy || !ReadSensors(false) || CameraCount != 4)
                return Reject("Measure timing and finish calibration before challenging the frame parser.");
            var challenge = new SparkLevel121Fusion.Replay();
            fusion.Step(challenge, cameras, true, true, true, true, idleRail, pulseRail);
            foreach (var fault in new[] { SparkLevel121Fusion.InjectedFrame.Stale,
                SparkLevel121Fusion.InjectedFrame.Duplicate, SparkLevel121Fusion.InjectedFrame.Corrupt })
                fusion.Step(challenge, cameras, true, true, true, true, idleRail, pulseRail, fault);
            checkedInputs = challenge.Accepted == 1 && challenge.Rejected == 3 &&
                challenge.Stale == 1 && challenge.Duplicates == 1 && challenge.CrcFailures == 1;
            if (!checkedInputs) return Reject("The parser did not reject all three invalid samples.");
            evidence = "Rejected a stale acquisition, a duplicate sequence and a corrupt payload CRC.";
            message = "Frame rejection verified. Run the synchronized test fixture.";
            return true;
        }

        public bool RunFusion()
        {
            if (stage is < 3 or > 5 || !RailsReady || !linked || !Calibrated || !checkedInputs ||
                Running || ProtectedBusy || !ClockReady && activeFault != 1)
                return Reject("Calibrate the sensors and prove input rejection before replay.");
            if (activeFault >= 0 && !reworked) EnergizeFault();
            replay = new SparkLevel121Fusion.Replay();
            qualifiedEpoch = runEpoch = -1;
            Failure = SensorFailure.None;
            nextSample = time;
            scope?.ResetTrace();
            message = "Acquiring 64 synthetic target sets through the live training sensor paths.";
            return true;
        }

        private bool ReadSensors(bool pulse)
        {
            CameraCount = 0;
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i] = RailsReady && Live(cameraPins[i]);
                if (cameras[i]) CameraCount++;
            }
            idleRail = Voltage(lidarPin);
            if (pulse || !Running && !ProtectedBusy)
            {
                using (manager.ElectricalSolver.BeginConfigurationBatch())
                    SetContact(lidarPulse, true);
                pulseRail = Voltage(lidarPin);
                using (manager.ElectricalSolver.BeginConfigurationBatch())
                    SetContact(lidarPulse, false);
            }
            return Live(lidarBias) && Live(imuPin);
        }

        private void TickController(float dt)
        {
            time += dt;
            CameraCount = 0;
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i] = powered && Live(cameraPins[i]);
                if (cameras[i]) CameraCount++;
            }
            UpdateSupervisor();
            if (time >= nextSample && (Running || ProtectedBusy))
            {
                bool protect = ProtectedBusy;
                if (protect && (Inhibited || !Fresh)) AbortProtected();
                else
                {
                    nextSample = time + (protect ? .035f : .026f);
                    var run = protect ? protectedReplay : replay;
                    SetContact(cameraGates[1], protect || !FaultLive(0) || run.Sent % 11 >= 4);
                    ReadSensors(true);
                    fusion.Step(run, cameras, Live(clockPin), Live(trimPin), Live(lidarBias), Live(imuPin), idleRail, pulseRail);
                    scope?.Record(run.Last.Valid ? run.Last.Depth / 2.25f : 0, run.Last.Confidence,
                        Mathf.Clamp01(run.Last.Skew / 650f), Mathf.Clamp01((pulseRail - 3.0f) / 2f));
                    if (protect) ProtectedDelivered = run.Accepted;
                    if (run.Complete)
                    {
                        if (protect) FinishProtected(run);
                        else FinishReplay();
                    }
                }
            }
            benchState = stage == 6 ? "PNX-29 QUALIFIED" : !powered ? "BENCH ISOLATED" :
                ProtectedBusy ? "PROTECTED PERCEPTION" : Running ? "SYNCHRONIZED ACQUISITION" :
                interrupted && !safetyReset ? "U6 RECOVERY LATCH" : "PERCEPTION BENCH READY";
        }

        private void FinishReplay()
        {
            SetContact(cameraGates[1], true);
            runEpoch = epoch;
            Failure = MissingB > 0 ? SensorFailure.CameraB : Stale > 0 ? SensorFailure.TriggerSkew :
                RangeDrops > 0 ? SensorFailure.LidarSupply : replay.Qualified ? SensorFailure.None : SensorFailure.FusionLimits;
            evidence = $"Peak range/pose error {PeakErrorMm:F1} mm · skew {ReplaySkew:F0} µs\n" +
                $"Confidence {MinConfidence:P0} min · {CorrectObstacles}/{Sent} decisions";
            if (Failure != SensorFailure.None)
            {
                message = Failure switch
                {
                    SensorFailure.CameraB => "Camera B lost frames. Stereo A/B rejected the incomplete sets.",
                    SensorFailure.TriggerSkew => "Trigger drift exceeded the timestamp window. Fusion rejected stale samples.",
                    SensorFailure.LidarSupply => "LiDAR voltage fell during the echo load step. Range sets were rejected.",
                    _ => "Replay failed its confidence, obstacle or tracking limits."
                };
                return;
            }
            qualifiedEpoch = epoch;
            if (stage == 3 && Award()) message = "Fusion qualified. Begin the controlled fault trials.";
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                faultWitnessed = reworked = false;
                QueueCheckpoint();
                message = repaired == 3 && Award() ? "All repairs passed. Start a protected replay and open W369." :
                    "Repair verified with fresh calibration and replay. Begin the next trial.";
            }
            else if (stage == 4) message = "Evidence refreshed. Begin the next fault trial.";
            else if (stage == 5) message = interrupted ? "Fresh fusion passed. Wait for U6 feedback, then reset its latch." :
                "Fresh fusion passed. Wait for U6 permit and start protected replay.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Qualifies || Running)
                return Reject("Complete a healthy synchronized replay before staging a fault.");
            activeFault = repaired;
            faultWitnessed = reworked = false;
            repairTarget = viaInspectionEpoch = -1;
            spare = 0;
            qualifiedEpoch = runEpoch = -1;
            EnergizeFault();
            message = "A seeded fault is staged. Run the service replay and examine its measured rejection.";
            QueueCheckpoint();
            return true;
        }

        private void EnergizeFault()
        {
            faultEnergized = true;
            SetContact(trimGate, !FaultLive(1));
            filterLoss.SetResistance(FaultLive(2) ? 22f : .12f);
        }

        public bool ProbeFault()
        {
            if (!powered || runEpoch != epoch || activeFault < 0 || reworked)
                return Reject("Obtain a fresh fault signature before probing.");
            bool measured = activeFault switch
            {
                0 => Failure == SensorFailure.CameraB && MissingB > 0 && Live(cameraPins[0]),
                1 => Failure == SensorFailure.TriggerSkew && Stale > 0 && Live(clockPin) && !Live(trimPin),
                2 => Failure == SensorFailure.LidarSupply && Sag > .7f && RangeDrops > 0 && Live(rail),
                _ => false
            };
            if (!measured) return Reject("The measured evidence does not establish this fault.");
            footprint = ExpectedPart;
            faultWitnessed = true;
            evidence = activeFault switch
            {
                0 => $"{CameraVia.viaId} · {MissingB} missing Camera B frames; Camera A stayed live.",
                1 => $"U4 trim {Voltage(trimPin):F2} V · {ReplaySkew:F0} µs peak skew · {Stale} rejected.",
                _ => $"LiDAR pulse rail {MinRail:F2} V · {Sag:F2} V load sag · {RangeDrops} range drops."
            };
            message = $"Isolate power, select {parts[ExpectedPart].label} and choose the matching repair spare.";
            QueueCheckpoint();
            return true;
        }

        public bool InspectCameraVia()
        {
            if (activeFault != 0 || !faultWitnessed || powered) return Reject("Probe Camera B and isolate the bench first.");
            if (selectedRoute != cameraViaIndex || layer != CameraVia.layer)
            {
                SelectRoute(cameraViaIndex);
                PadView();
                return Reject($"Inspect {CameraVia.viaId} on entry layer L{CameraVia.layer:D2}.");
            }
            viaInspectionEpoch = epoch;
            message = $"Confirmed {CameraVia.viaId} from L{CameraVia.layer:D2} to L{CameraVia.exitLayer:D2}.";
            return true;
        }

        public void SelectSpare()
        {
            spare = (spare + 1) % 4;
            message = spare switch
            {
                1 => "Spare · Camera B via repair",
                2 => "Spare · PPS distribution contact",
                3 => "Spare · C12 training filter capacitor",
                _ => "No spare selected."
            };
        }

        public bool Repair()
        {
            if (stage != 4 || activeFault < 0 || !faultWitnessed || reworked || powered)
                return Reject("Diagnose and isolate the bench before rework.");
            if (spare != activeFault + 1 || repairTarget != ExpectedPart ||
                activeFault == 0 && (viaInspectionEpoch != epoch || layer != CameraVia.layer))
                return Reject("Match the spare and board site. Inspect the via entry layer when required.");
            reworked = true;
            if (activeFault == 2) filterLoss.SetResistance(.12f);
            InvalidateDiagnostics();
            QueueCheckpoint();
            BoardView();
            message = "Repair seated. Remeasure the trigger, calibrate and replay.";
            return true;
        }

        public bool StartProtected()
        {
            if (stage != 5 || !Fresh || Inhibited || interrupted && !safetyReset || Running || ProtectedBusy)
                return Reject("U6 requires fresh sensor qualification and a solved permit before protected output.");
            armed = true;
            protectedReplay = new SparkLevel121Fusion.Replay();
            ProtectedDelivered = 0;
            nextSample = time;
            message = interrupted ? "Running freshly authorized protected perception." :
                "Replay active. Open W369 while qualified frame sets are being delivered.";
            return true;
        }

        private void FinishProtected(SparkLevel121Fusion.Replay report)
        {
            protectedReplay = null;
            armed = false;
            if (interrupted && safetyReset && report.Qualified && Fresh && !Inhibited && Award())
            {
                message = "W369 recovery verified. U6 authorized the complete protected replay.";
                evidence = $"{ProtectedDelivered}/64 valid sets · {report.PeakErrorMm:F1} mm peak fusion error.";
            }
            else if (!interrupted) message = "Repeat the protected replay and open W369 while frames are pending.";
        }

        private void AbortProtected()
        {
            UndeliveredAtTrip = SparkLevel121Fusion.FrameCount - ProtectedDelivered;
            protectedReplay = null;
            armed = safetyReset = false;
            SetContact(workload, false);
            minimumEpoch = epoch + 1;
            manager.ElectricalSolver.SolveNow();
        }

        public bool OpenW369()
        {
            if (stage != 5 || !linked || interrupted || !ProtectedBusy || ProtectedDelivered == 0 || Inhibited)
                return Reject("Open W369 during protected acquisition after the first valid frame.");
            return RemoveWire(jumper) || Reject("The native registry could not open W369.");
        }

        public bool RestoreW369()
        {
            if (stage != 5 || !interrupted || linked) return Reject("Open W369 during live protected acquisition first.");
            if (!CreateWire(jumper)) return Reject("The native circuit rejected the training contact pair.");
            ReadGraph();
            message = "W369 restored. U6 still requires fresh timing, calibration and replay evidence.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !Fresh || epoch < minimumEpoch || ProtectedBusy)
                return Reject("Requalify all sensors and wait for fresh feedback before resetting U6.");
            safetyReset = true;
            UpdateSupervisor();
            message = "U6 reset. Run one final protected replay.";
            return true;
        }

        private void InvalidateDiagnostics()
        {
            epoch++;
            clockEpoch = calibrationEpoch = qualifiedEpoch = runEpoch = -1;
            checkedInputs = false;
            fusion.ForgetCalibration();
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
                if (heartbeat.IsClosed != driverHeartbeat) SetContact(heartbeat, driverHeartbeat);
                bool high = linked && Live(jumper.to);
                if (high && !highFeedback)
                {
                    feedbackIntervals = lastRise >= 0 && time - lastRise is > .21f and < .49f ? feedbackIntervals + 1 : 0;
                    lastRise = time;
                }
                highFeedback = high;
            }
            bool permit = stage >= 5 && Fresh && (!interrupted || safetyReset);
            if (workload.IsClosed != permit) SetContact(workload, permit);
        }
    }
}

