using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.DeterministicGrid
{
    public sealed partial class SparkLevel118Process
    {
        public SparkTerminal rail, monitorSupply, primaryPin, trimPin, backupPin, pathA, pathB, sramPin, resetPin, protectedFeed;
        public SparkTerminal[] portPins;
        public SparkSwitch bench, monitorGate, primaryGate, trimGate, backupGate, pathGateA, pathGateB, sramGate, resetGate, heartbeat, workload;
        public SparkSwitch[] portGates;
        public Renderer[] portMarkers;
        public GameObject assemblyArt;
        public int clockPart, pathPart, sramPart, pathViaIndex;
        public int[] portParts;

        public enum GridFailure { None, PrimaryDrift, PathBVia, GateMemory, ScheduleLimits }
        private readonly bool[] portUp = new bool[6];
        private SparkLevel118Timebase primary = new(), backup = new();
        private SparkLevel118GateTable table;
        private SparkLevel118Schedule trial, protectedRun;
        private float time, sampleAt, holdoverSince = -1f, measuredAt, nextCycle, nextHeartbeat, lastRise = -1f;
        private bool measuring, reserved = true, driverHeartbeat, highFeedback, portMap;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset;
        private int epoch, clockEpoch = -1, linkEpoch = -1, gateEpoch = -1, qualifiedEpoch = -1, witnessedEpoch = -1;
        private int activeFault = -1, repaired, repairTarget = -1, viaInspectionEpoch = -1, spare, minimumEpoch, feedbackIntervals;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool RailsReady => powered && Live(rail) && Live(monitorSupply) && Live(resetPin);
        public bool Measuring => measuring;
        public bool Running => trial != null && !trial.Complete;
        public bool ProtectedBusy => protectedRun != null;
        public bool ClockReady => clockEpoch == epoch && primary.Locked && backup.Locked && time - measuredAt < 120f;
        public bool PathAReady => portUp[0] && Live(pathA);
        public bool PathBReady => portUp[1] && Live(pathB);
        public bool GateReady => gateEpoch == epoch && table != null && table.Intact && table.Guarded && Live(sramPin);
        public bool Qualifies => ClockReady && GateReady && linkEpoch == epoch && qualifiedEpoch == epoch && RailsReady &&
            PathAReady && PathBReady && TrainedPorts == 6;
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .49f;
        public int TrainedPorts { get; private set; }
        public int Repaired => repaired;
        public int Epoch => epoch;
        public int Sent => trial?.Sent ?? 0;
        public int Delivered => trial?.Delivered ?? 0;
        public int Misses => trial?.Misses ?? 0;
        public int CrcErrors => trial?.CrcErrors ?? 0;
        public int Suppressed => trial?.Suppressed ?? 0;
        public int CongestionDrops => trial?.BestEffortDrops ?? 0;
        public int BackgroundDelivered => trial?.BestEffortDelivered ?? 0;
        public int WorstUs => trial?.WorstUs ?? 0;
        public int JitterUs => trial?.JitterUs ?? 0;
        public int PathBOutages => trial?.PathBOutages ?? 0;
        public int Failovers => trial?.BFailovers ?? 0;
        public int PeakQueue => trial?.PeakQueue ?? 0;
        public int PathACopies => trial?.AValid ?? 0;
        public int PathBCopies => trial?.BValid ?? 0;
        public int UndeliveredAtTrip { get; private set; }
        public int ProtectedDelivered { get; private set; }
        public double PrimaryOffsetNs => primary.OffsetNs;
        public double PrimaryDriftPpm => primary.DriftPpm;
        public double BackupDriftPpm => backup.DriftPpm;
        public double HoldoverNs { get; private set; }
        public double ExpiredHoldoverNs { get; private set; }
        public GridFailure Failure { get; private set; }
        public string Evidence => evidence;
        public int DeliveredAt(int field) => trial?.AtField(field) ?? 0;

        private Route FaultVia => routes[pathViaIndex];
        private int ExpectedPart => activeFault switch { 0 => clockPart, 1 => pathPart, 2 => sramPart, _ => -1 };
        private bool FaultLive(int index) => activeFault == index && !reworked;
        private bool Live(SparkTerminal pin) => Voltage(pin) > 4.5f;
        private void Solve() => manager.ElectricalSolver.SolveNow();

        public bool TogglePower()
        {
            if (stage < 2 || stage >= 6 || !Construction || !linked && !interrupted)
                return Reject("Complete the training copper and independent feedback edge before power-up.");
            if (powered)
            {
                IsolatePower();
                return true;
            }
            powered = true;
            SetContact(bench, true);
            SetContact(monitorGate, true);
            SetContact(resetGate, true);
            SetContact(primaryGate, true);
            SetContact(backupGate, true);
            SetContact(pathGateA, true);
            SetContact(pathGateB, true);
            SetContact(trimGate, !FaultLive(0));
            SetContact(sramGate, !FaultLive(2));
            foreach (var port in portGates) SetContact(port, true);
            Solve();
            message = "Measure both clocks and train all six ports. Check both uplinks independently.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            foreach (var contact in new[] { bench, monitorGate, resetGate, primaryGate, backupGate,
                trimGate, pathGateA, pathGateB, sramGate, heartbeat, workload })
                SetContact(contact, false);
            foreach (var port in portGates) SetContact(port, false);
            InvalidateDiagnostics();
            Solve();
            message = "Bench isolated. Select the observed fault site and its matching repair kit.";
        }

        public bool MeasureClocks()
        {
            if (!RailsReady || !Live(primaryPin) || !Live(backupPin) || measuring || Running || ProtectedBusy)
                return Reject("Power both clocks and finish the active test before measurement.");
            primary = new SparkLevel118Timebase();
            backup = new SparkLevel118Timebase();
            measuring = true;
            holdoverSince = -1f;
            HoldoverNs = ExpiredHoldoverNs = 0;
            clockEpoch = qualifiedEpoch = gateEpoch = -1;
            sampleAt = time;
            Failure = GridFailure.None;
            message = "Measuring separate four-timestamp exchanges at Y1 and Y2.";
            return true;
        }

        private void UpdateClocks()
        {
            if (!measuring) return;
            if (!RailsReady || !Live(backupPin))
            { measuring = false; Reject("Timebase measurement interrupted by an open power path."); return; }
            if (holdoverSince >= 0)
            {
                float elapsed = time - holdoverSince;
                if (elapsed >= .18f && HoldoverNs == 0 && !Live(primaryPin))
                    HoldoverNs = backup.HoldoverUncertainty(.18);
                if (elapsed < .62f) return;
                ExpiredHoldoverNs = backup.HoldoverUncertainty(.62);
                bool independent = !Live(primaryPin) && Live(backupPin) && HoldoverNs is > 0 and < 570 && ExpiredHoldoverNs > 850;
                SetContact(primaryGate, true);
                Solve();
                measuring = false;
                if (!independent || !Live(primaryPin))
                { Reject("Backup uncertainty or primary restoration did not meet the holdover limits."); return; }
                clockEpoch = epoch;
                measuredAt = time;
                evidence = $"Y1 {PrimaryOffsetNs:+0;-0;0} ns / {PrimaryDriftPpm:F2} ppm   Y2 {BackupDriftPpm:F2} ppm\n" +
                    $"Backup {HoldoverNs:F0} ns at 180 ms; expires by 620 ms.";
                message = stage == 2 ? "Time references qualified. Train the six ports and both uplinks." :
                    "Clocks qualified. Train both paths, verify the gate table and run congestion traffic.";
                return;
            }
            if (time < sampleAt) return;
            sampleAt = time + .024f;
            primary.Sample(Live(primaryPin), Live(trimPin) ? .48 : 46.25);
            backup.Sample(Live(backupPin), 1.36);
            if (!primary.Complete || !backup.Complete) return;
            if (!primary.Locked || !backup.Locked)
            {
                measuring = false;
                if (FaultLive(0) && !Live(trimPin) && !primary.Locked && backup.Locked)
                { Failure = GridFailure.PrimaryDrift; witnessedEpoch = epoch; }
                evidence = $"Y1 {PrimaryOffsetNs:+0;-0;0} ns / {PrimaryDriftPpm:F2} ppm   Y2 {BackupDriftPpm:F2} ppm";
                message = "Y1 drift violates the gate clock budget. Backup remains available for bounded holdover.";
                return;
            }
            SetContact(primaryGate, false);
            Solve();
            holdoverSince = time;
            message = "Primary opened for the holdover benchmark. Watching the independently powered backup.";
        }

        public bool TrainPorts()
        {
            if (!RailsReady || Running || ProtectedBusy) return Reject("Power and release reset before training the ports.");
            SamplePorts();
            if (TrainedPorts != 6 || !PathAReady || !PathBReady)
            {
                linkEpoch = qualifiedEpoch = -1;
                return Reject("Training needs all six solved connector returns and both uplink paths.");
            }
            linkEpoch = epoch;
            evidence = "Six connector returns measured · A/J1 and B/J2 independently pass.";
            message = stage == 2 ? "Both uplinks trained. Measure the clocks if pending." :
                "Both uplinks trained. Load and verify the gate table.";
            return true;
        }

        private void SamplePorts()
        {
            TrainedPorts = 0;
            bool ready = RailsReady;
            for (int i = 0; i < portUp.Length; i++)
            {
                portUp[i] = ready && parts[portParts[i]].contact.IsClosed && Live(portPins[i]);
                if (portUp[i]) TrainedPorts++;
            }
        }

        public void SelectSchedule()
        {
            if (Running || ProtectedBusy || stage >= 6) return;
            reserved = !reserved;
            gateEpoch = qualifiedEpoch = -1;
            message = reserved ? "Protected reservation selected. Load the table." :
                "Shared windows selected. Load the table to check serialization and guard bands.";
        }

        public bool LoadSchedule()
        {
            if (!RailsReady || !ClockReady || linkEpoch != epoch || Running || ProtectedBusy)
                return Reject("Qualify the clocks and train the six ports before loading a gate table.");
            gateEpoch = qualifiedEpoch = -1;
            table = new SparkLevel118GateTable(reserved, Live(sramPin));
            if (!table.Intact)
            {
                Failure = GridFailure.GateMemory;
                witnessedEpoch = epoch;
                evidence = $"U15 data-line return {Voltage(sramPin):F2} V · checksum {table.Checksum:X8}.";
                return Reject("U15 gate-table readback differs from its source checksum. Protected output is inhibited.");
            }
            if (!table.Guarded)
            {
                Failure = GridFailure.ScheduleLimits;
                return Reject("A shared command window is too short for a complete frame and guard band.");
            }
            Failure = GridFailure.None;
            gateEpoch = epoch;
            evidence = $"U15 readback {table.Checksum:X8} · 250 µs cycle · guarded protected windows.";
            message = "Gate table verified. Run the seeded congestion test.";
            return true;
        }

        public bool RunSchedule()
        {
            if (stage is < 3 or > 5 || !RailsReady || !ClockReady || linkEpoch != epoch || !GateReady ||
                !linked || Running || ProtectedBusy)
                return Reject("Connect W321, qualify clocks and paths, then verify the gate table.");
            qualifiedEpoch = -1;
            Failure = GridFailure.None;
            trial = new SparkLevel118Schedule(table, 0x4b29e136u);
            nextCycle = time;
            scope?.ResetTrace();
            message = "Measuring reserved command windows while best-effort queues are overloaded.";
            return true;
        }

        private void TickController(float dt)
        {
            time += dt;
            UpdateClocks();
            SamplePorts();
            if (stage == 2 && RailsReady && ClockReady && linkEpoch == epoch && TrainedPorts == 6 &&
                PathAReady && PathBReady && Award())
                message = "Clocks, six ports and both paths qualified. Load the protected gate table.";
            UpdateSupervisor();
            if (time >= nextCycle)
            {
                if (Running) StepTraffic();
                else if (ProtectedBusy) StepProtected();
            }
            benchState = stage == 6 ? "TSX-26 QUALIFIED" : !powered ? "BENCH ISOLATED" :
                measuring ? holdoverSince >= 0 ? "BACKUP HOLDOVER" : "TIMESTAMP MEASUREMENT" :
                ProtectedBusy ? "PROTECTED COMMAND BURST" : Running ? "CONGESTION TEST" :
                interrupted && !safetyReset ? "U32 RECOVERY LATCH" : "DETERMINISTIC BENCH READY";
        }

        private void StepTraffic()
        {
            nextCycle = time + .013f;
            if (!RailsReady || !GateReady || !ClockReady)
            {
                trial = null;
                Reject("Live evidence expired during the run. Verify clocks and the gate table again.");
                return;
            }
            bool pathBExpected = !FaultLive(1) || trial.Cycle % 13 >= 5;
            if (pathGateB.IsClosed != pathBExpected)
            {
                SetContact(pathGateB, pathBExpected);
                Solve();
            }
            bool recoveringB = FaultLive(1) && trial.Cycle % 13 == 5 && PathBReady;
            trial.Step(PathAReady, PathBReady, portUp, (int)Math.Round(PrimaryOffsetNs / 1000.0), recoveringB);
            scope?.Record(Mathf.Min(1f, trial.LastDelayUs / 100f), trial.QueueDepth / 18f,
                PathBReady ? .58f : .05f, trial.LastReservedFinish / 250f);
            if (trial.Complete) FinishTraffic();
        }

        private void FinishTraffic()
        {
            witnessedEpoch = epoch;
            evidence = $"Worst {WorstUs} µs · jitter {JitterUs} µs · CRC errors {CrcErrors}\n" +
                $"A {PathACopies} / B {PathBCopies} copies · background drops {CongestionDrops}";
            if (!trial.Qualified)
            {
                Failure = PathBOutages > 0 && FaultLive(1) ? GridFailure.PathBVia : GridFailure.ScheduleLimits;
                message = Failure == GridFailure.PathBVia ?
                    "Path B opened under traffic. Path A carried the redundant commands. Probe the via." :
                    "Traffic missed the deadline, integrity or redundant-path requirements.";
                return;
            }
            qualifiedEpoch = epoch;
            if (stage == 3 && Award()) message = "Protected deadline and jitter proved under congestion. Begin the fault trials.";
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                faultWitnessed = reworked = false;
                QueueCheckpoint();
                message = repaired == 3 && Award() ?
                    "Three repairs qualified. Begin a protected command burst and open W321." :
                    "Repair requalified with fresh evidence. Begin the next trial.";
            }
            else if (stage == 4)
                message = "Qualification refreshed. Begin the next fault trial.";
            else if (stage == 5)
                message = interrupted ? "Fresh end-to-end test passed. Wait for U32 feedback, then reset it." :
                    "Fresh commands verified. Wait for U32 permission, then start the protected burst.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Qualifies || Running)
                return Reject("Finish a qualifying congestion test before staging the next fault.");
            activeFault = repaired;
            faultWitnessed = reworked = false;
            repairTarget = viaInspectionEpoch = -1;
            spare = 0;
            InvalidateDiagnostics();
            SetContact(trimGate, !FaultLive(0));
            SetContact(sramGate, !FaultLive(2));
            Solve();
            message = "A seeded fault is active. Remeasure both clocks, paths and the gate table.";
            QueueCheckpoint();
            return true;
        }

        public bool ProbeFault()
        {
            if (!powered || witnessedEpoch != epoch || activeFault < 0 || reworked)
                return Reject("Collect a fresh fault signature first.");
            bool diagnosed = activeFault switch
            {
                0 => Failure == GridFailure.PrimaryDrift && !Live(trimPin) && backup.Locked && Math.Abs(PrimaryDriftPpm) > 30,
                1 => Failure == GridFailure.PathBVia && PathBOutages > 0 && Failovers > 0 && PathAReady,
                2 => Failure == GridFailure.GateMemory && !Live(sramPin) && table != null && !table.Intact,
                _ => false
            };
            if (!diagnosed) return Reject("The diagnostic readings do not establish the current fault.");
            faultWitnessed = true;
            footprint = ExpectedPart;
            evidence = activeFault switch
            {
                0 => $"Y1 trim {Voltage(trimPin):F2} V · {PrimaryDriftPpm:F2} ppm · backup {BackupDriftPpm:F2} ppm.",
                1 => $"{FaultVia.viaId} opened in {PathBOutages} cycles. {Failovers} commands fell back to A.",
                _ => $"U15 return {Voltage(sramPin):F2} V. Gate-table readback failed CRC."
            };
            message = $"Isolate bench power. Select {parts[ExpectedPart].label} on the board and the matching spare.";
            QueueCheckpoint();
            return true;
        }

        public bool InspectPathVia()
        {
            if (activeFault != 1 || !faultWitnessed || powered)
                return Reject("Observe the Path B fault and isolate the bench first.");
            if (selectedRoute != pathViaIndex || layer != FaultVia.layer)
            {
                SelectRoute(pathViaIndex);
                PadView();
                return Reject($"Inspect {FaultVia.viaId} at its entry layer L{FaultVia.layer:D2}.");
            }
            viaInspectionEpoch = epoch;
            message = $"Confirmed {FaultVia.viaId} from L{FaultVia.layer:D2} to L{FaultVia.exitLayer:D2}.";
            return true;
        }

        public void SelectSpare()
        {
            spare = (spare + 1) % 4;
            message = spare switch
            {
                1 => "Spare · Y1 timebase trim contact",
                2 => "Spare · Path B plated via repair",
                3 => "Spare · U15 gate-memory data link",
                _ => "No spare selected."
            };
        }

        public bool Repair()
        {
            if (stage != 4 || activeFault < 0 || !faultWitnessed || reworked || powered)
                return Reject("Diagnose the fault and isolate the bench before repair.");
            if (spare != activeFault + 1 || repairTarget != ExpectedPart ||
                activeFault == 1 && (viaInspectionEpoch != epoch || layer != FaultVia.layer))
                return Reject("Match the spare and selected footprint. Inspect the Path B entry span when required.");
            reworked = true;
            InvalidateDiagnostics();
            QueueCheckpoint();
            BoardView();
            message = "Repair seated. Power up and acquire fresh clock, path, gate and load-test evidence.";
            return true;
        }

        public bool StartProtected()
        {
            if (stage != 5 || !Fresh || Inhibited || interrupted && !safetyReset || ProtectedBusy || Running)
                return Reject("U32 requires fresh qualification and its solved permit before sending protected commands.");
            armed = true;
            protectedRun = new SparkLevel118Schedule(table, 0x6129d47bu);
            ProtectedDelivered = 0;
            nextCycle = time;
            message = interrupted ? "Running the freshly armed protected burst." :
                "Burst active. Open W321 before all protected commands are delivered.";
            return true;
        }

        private void StepProtected()
        {
            nextCycle = time + .022f;
            if (Inhibited || !Fresh) { AbortProtectedOutput(); return; }
            protectedRun.Step(PathAReady, PathBReady, portUp, (int)Math.Round(PrimaryOffsetNs / 1000.0));
            ProtectedDelivered = protectedRun.Delivered;
            if (!protectedRun.Complete) return;
            bool qualified = protectedRun.Qualified;
            protectedRun = null;
            armed = false;
            if (interrupted && safetyReset && qualified && Award())
            {
                message = "W321 recovery verified. U32 authorized exactly-once protected delivery.";
                evidence = $"{ProtectedDelivered}/{SparkLevel118Schedule.CommandTotal} fresh protected commands confirmed.";
            }
            else if (!interrupted) message = "Start another protected burst and open W321 while commands are pending.";
        }

        private void AbortProtectedOutput()
        {
            UndeliveredAtTrip = protectedRun == null ? 0 : SparkLevel118Schedule.CommandTotal - protectedRun.Delivered;
            protectedRun = null;
            armed = safetyReset = false;
            SetContact(workload, false);
            minimumEpoch = epoch + 1;
            Solve();
        }

        public bool OpenW321()
        {
            if (stage != 5 || !linked || interrupted || !ProtectedBusy || ProtectedDelivered == 0 || Inhibited)
                return Reject("Open W321 during an active protected burst after the first command.");
            if (!RemoveWire(jumper)) return Reject("The native wire registry could not open W321.");
            Solve();
            return true;
        }

        public bool RestoreW321()
        {
            if (stage != 5 || !interrupted || linked)
                return Reject("Open W321 during the live supervised challenge before restoring it.");
            if (!CreateWire(jumper)) return Reject("The native circuit rejected the W321 pair.");
            ReadGraph();
            message = "W321 restored. Requalify clocks, both links, the gate table and commands before resetting U32.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !Fresh || epoch < minimumEpoch || ProtectedBusy)
                return Reject("U32 requires fresh post-repair timing, paths and protected command evidence.");
            safetyReset = true;
            UpdateSupervisor();
            message = "U32 reset. Run one final protected burst.";
            return true;
        }

        private void InvalidateDiagnostics()
        {
            epoch++;
            clockEpoch = linkEpoch = gateEpoch = qualifiedEpoch = witnessedEpoch = -1;
            if (holdoverSince >= 0 && powered) SetContact(primaryGate, true);
            measuring = false;
            holdoverSince = -1f;
            trial = protectedRun = null;
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
                driverHeartbeat = !driverHeartbeat && RailsReady && Qualifies;
                if (heartbeat.IsClosed != driverHeartbeat)
                {
                    SetContact(heartbeat, driverHeartbeat);
                    Solve();
                }
                bool high = linked && Live(jumper.to);
                if (high && !highFeedback)
                {
                    if (lastRise >= 0 && time - lastRise is > .21f and < .49f) feedbackIntervals++;
                    else feedbackIntervals = 0;
                    lastRise = time;
                }
                highFeedback = high;
            }
            bool permit = stage >= 5 && Fresh && (!interrupted || safetyReset);
            if (workload.IsClosed == permit) return;
            SetContact(workload, permit);
            Solve();
        }
    }
}
