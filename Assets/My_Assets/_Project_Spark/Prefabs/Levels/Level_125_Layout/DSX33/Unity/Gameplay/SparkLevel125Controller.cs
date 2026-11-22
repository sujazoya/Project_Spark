using UnityEngine;
using Link = ProjectSpark.Gameplay.SignalHorizon.SparkLevel125Link;

namespace ProjectSpark.Gameplay.SignalHorizon
{
    public sealed partial class SparkLevel125Process
    {
        public SparkTerminal rail, driveRail, monitorSupply, telemetryPin, clockPin, secondaryClock, trimPin, resetPin, filterPin, protectedFeed;
        public SparkTerminal[] logicPins, sensorPins, transmitterPins, gatePins;
        public SparkSwitch bench, drivePower, monitorGate, telemetryGate, clockGate, secondaryClockGate, trimPower, resetGate;
        public SparkSwitch filterPower, clockContact, heartbeat, workload;
        public SparkSwitch[] logicGates, sensorGates, transmitterPower, outputContacts;
        public SparkResistor filterSeries;
        public SparkResistor[] transmitterLoads;
        public Renderer[] sensorMarkers;
        public GameObject assemblyArt;
        public int clockPart, filterPart, transmitterPart;

        public enum LinkFailure { None, ClockDrift, FilterNoise, OutputCurrent, LinkBudget }
        private readonly Link link = new();
        private readonly bool[] sensorLive = new bool[2];
        private readonly bool[] transmitterLive = new bool[2];
        private Link.Replay replay, protectedReplay;
        private float time, nextSample, nextHeartbeat, lastRise = -1f, measuredSkew, watchdogUntil;
        private bool driverHeartbeat, highFeedback, checkedInputs, watchdogObserved, currentLatched;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset, showSensors, faultEnergized;
        private int epoch, qualifiedEpoch = -1, clockEpoch = -1, calibrationEpoch = -1, runEpoch = -1, minimumEpoch, feedbackIntervals;
        private int activeFault = -1, repaired, repairTarget = -1, spare, verifiedPermits;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool RailsReady => powered && Live(rail) && Live(driveRail) && Live(monitorSupply) && Live(telemetryPin) &&
            Live(resetPin) && Live(logicPins[0]) && Live(logicPins[1]) && Live(logicPins[2]);
        public bool ClockReady => clockEpoch == epoch && Live(clockPin) && Live(secondaryClock) && Live(trimPin);
        public bool Calibrated => calibrationEpoch == epoch && link.Calibrated;
        public bool Running => replay != null && !replay.Complete;
        public bool ProtectedBusy => protectedReplay != null;
        public bool Qualifies => qualifiedEpoch == epoch && ClockReady && Calibrated && checkedInputs &&
            RailsReady && SensorCount == 2 && TransmitterCount == 2 && !currentLatched && Voltage(filterPin) > 4.60f;
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .49f;
        public bool WatchdogActive => time < watchdogUntil;
        public bool WatchdogObserved => watchdogObserved;
        public int SensorCount { get; private set; }
        public int TransmitterCount { get; private set; }
        public int ActiveTxGate { get; private set; } = -1;
        public int Accepted => replay?.Accepted ?? 0;
        public int Received => replay?.Received ?? 0;
        public int Sent => replay?.Sent ?? 0;
        public int Corrected => replay?.Corrected ?? 0;
        public int LowMargin => replay?.LowMargin ?? 0;
        public int Switchovers => replay?.Switchovers ?? 0;
        public int BackupFrames => replay?.BackupFrames ?? 0;
        public int StaleClock => replay?.StaleClock ?? 0;
        public int FilterRejects => replay?.NoisyFilter ?? 0;
        public int CurrentHolds => replay?.CurrentHolds ?? 0;
        public float MinimumMargin => replay?.MinimumMargin ?? 0f;
        public bool CurrentLatched => currentLatched;
        public float MeasuredSkew => measuredSkew;
        public float TransmitterCurrent(int channel) => Mathf.Abs(transmitterLoads[channel].Current);
        public int Repaired => repaired;
        public int Epoch => epoch;
        public int ProtectedDelivered { get; private set; }
        public int ProtectedBackupFrames { get; private set; }
        public int UndeliveredAtTrip { get; private set; }
        public LinkFailure Failure { get; private set; }
        public string Evidence => evidence;

        private int ExpectedPart => activeFault switch { 0 => clockPart, 1 => filterPart, 2 => transmitterPart, _ => -1 };
        private bool FaultLive(int index) => activeFault == index && !reworked && faultEnergized;
        private bool Live(SparkTerminal pin) => Voltage(pin) > 4.55f;

        public bool TogglePower()
        {
            if (stage < 2 || stage >= 6 || !Construction || !linked && !interrupted)
                return Reject("Complete native continuity and W433 before enabling the signal bench.");
            if (powered) { IsolatePower(); return true; }
            powered = true;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                foreach (var contact in new[] { bench, drivePower, monitorGate, telemetryGate, clockGate, secondaryClockGate,
                    trimPower, resetGate, filterPower })
                    SetContact(contact, true);
                foreach (var gate in logicGates) SetContact(gate, true);
                foreach (var gate in sensorGates) SetContact(gate, true);
                foreach (var gate in transmitterPower) SetContact(gate, true);
                SetContact(clockContact, !FaultLive(0));
                filterSeries.SetResistance(FaultLive(1) ? 43f : .12f);
                transmitterLoads[0].SetResistance(FaultLive(2) ? 12f : 100f);
            }
            ReadLink();
            if (!FaultLive(2) && TransmitterCurrent(0) < Link.CurrentLimit) currentLatched = false;
            message = "Verify both clocks and the reference trim before commissioning.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                foreach (var contact in new[] { bench, drivePower, monitorGate, telemetryGate, clockGate, secondaryClockGate,
                    trimPower, resetGate, filterPower, heartbeat })
                    SetContact(contact, false);
                foreach (var gate in logicGates) SetContact(gate, false);
                foreach (var gate in sensorGates) SetContact(gate, false);
                foreach (var gate in transmitterPower) SetContact(gate, false);
                InvalidateDiagnostics();
            }
            ReadLink();
            message = "Bench isolated. Select the diagnosed footprint and its matching spare.";
        }

        public bool MeasureClock()
        {
            if (!RailsReady || !Live(clockPin) || !Live(secondaryClock) || Running || ProtectedBusy)
                return Reject("Power both clocks and stop the trial before measuring timing.");
            measuredSkew = 0f;
            bool trim = Live(trimPin);
            for (int tick = 0; tick < 24; tick++) measuredSkew = Mathf.Max(measuredSkew, Link.ClockSkew(tick, trim));
            if (measuredSkew > Link.SkewLimitNs)
            {
                clockEpoch = qualifiedEpoch = -1;
                evidence = $"Reference skew {measuredSkew:F0} ns exceeds the 125 ns frame window.";
                return Reject("Timing is out of range. Use the link trial to capture dropped frames.");
            }
            clockEpoch = epoch;
            message = "References measured. Commission both receive and transmit channels.";
            evidence = $"24 timing samples · {measuredSkew:F0} ns peak skew · 125 ns limit.";
            return true;
        }

        public bool Calibrate()
        {
            if (!RailsReady || !ClockReady || Running || ProtectedBusy || !ReadLink().Ready)
                return Reject("Measure both clocks and verify both RX stages, filter voltage and TX test currents.");
            if (!link.Calibrate()) return Reject("The decoder failed its CRC or path-loss reference vector.");
            calibrationEpoch = epoch;
            qualifiedEpoch = -1;
            checkedInputs = false;
            if (stage == 2) Award();
            evidence = "CRC-16 check 0x29B1 · virtual reference link margin +14.71 dB.";
            message = "Challenge the decoder, then run the 64-frame virtual link trial.";
            return true;
        }

        public bool CheckGuardRejection()
        {
            if (!Calibrated || !ClockReady || Running || ProtectedBusy || !ReadLink().Ready)
                return Reject("Commission both receive and transmit stages before testing the decoder.");
            var inputs = ReadLink();
            bool Test(Link.Challenge challenge, Link.Hold expected)
            {
                var test = new Link.Replay();
                link.Advance(test, inputs, challenge);
                return test.Last.Tx < 0 && test.Last.Reason == expected;
            }
            checkedInputs = Test(Link.Challenge.Duplicate, Link.Hold.Duplicate) &&
                Test(Link.Challenge.Stale, Link.Hold.OldFrame) && Test(Link.Challenge.Corrupt, Link.Hold.Uncorrectable);
            if (!checkedInputs) return Reject("The decoder did not reject every challenged frame.");
            evidence = "Rejected duplicate sequence, stale timestamp and uncorrectable payload.";
            message = "Decoder challenges passed. Start the link trial.";
            return true;
        }

        public bool RunLink()
        {
            if (stage is < 3 or > 5 || !RailsReady || !linked || !Calibrated || !checkedInputs ||
                Running || ProtectedBusy || !ClockReady && activeFault != 0 || WatchdogActive)
                return Reject("Measure, commission and test the decoder before running the link trial.");
            if (activeFault >= 0 && !reworked) EnergizeFault();
            replay = new Link.Replay();
            qualifiedEpoch = runEpoch = -1;
            Failure = LinkFailure.None;
            nextSample = time;
            scope?.ResetTrace();
            message = "Sampling link margin, forward error correction and redundant handover.";
            return true;
        }

        private Link.Inputs ReadLink()
        {
            SensorCount = TransmitterCount = 0;
            bool overcurrent = false;
            for (int channel = 0; channel < sensorLive.Length; channel++)
            {
                sensorLive[channel] = powered && Live(sensorPins[channel]);
                if (sensorLive[channel]) SensorCount++;
                float amps = TransmitterCurrent(channel);
                transmitterLive[channel] = powered && Live(transmitterPins[channel]) && amps > .035f;
                if (transmitterLive[channel]) TransmitterCount++;
                overcurrent |= powered && amps > Link.CurrentLimit;
            }
            if (overcurrent) currentLatched = true;
            return new Link.Inputs(sensorLive[0], sensorLive[1], transmitterLive[0], transmitterLive[1],
                Live(trimPin), overcurrent || currentLatched, Voltage(filterPin));
        }

        private void TickController(float dt)
        {
            time += dt;
            ReadLink();
            if (stage is >= 2 and <= 6) UpdateSupervisor();
            if (time < nextSample || !Running && !ProtectedBusy) return;
            bool protect = ProtectedBusy;
            var run = protect ? protectedReplay : replay;
            if (protect && !Fresh)
            {
                AbortProtected();
                message = "Trusted readiness expired. Requalify before enabling virtual transmit.";
                return;
            }

            link.Advance(run, ReadLink());
            var sample = run.Last;
            nextSample = time + (protect ? !interrupted && sample.Tx == 1 ? .14f : .051f : .028f);
            scope?.Record(Mathf.Clamp01((sample.Margin + 5f) / 21f), Mathf.Clamp01(sample.SkewNs / 280f),
                Mathf.Clamp01(Voltage(filterPin) / 5f), Mathf.Clamp01(TransmitterCurrent(0) / .45f));
            if (!protect)
            {
                if (run.Complete) FinishLink(run);
                return;
            }

            SetProtectedTx(Fresh ? sample.Tx : -1);
            ReadEnabledGates();
            if (ActiveTxGate == sample.Tx)
            {
                verifiedPermits++;
                if (ActiveTxGate == 1) ProtectedBackupFrames++;
            }
            if (!interrupted && ActiveTxGate == 1 && ProtectedBackupFrames == 1)
                message = "Backup transmit active. Open W433 to inhibit both paths.";
            ProtectedDelivered = run.Sent;
            if (run.Complete) FinishProtected(run);
        }

        private void FinishLink(Link.Replay report)
        {
            runEpoch = epoch;
            Failure = report.StaleClock > 0 ? LinkFailure.ClockDrift : report.NoisyFilter > 0 ? LinkFailure.FilterNoise :
                report.CurrentHolds > 0 ? LinkFailure.OutputCurrent : report.Qualified ? LinkFailure.None : LinkFailure.LinkBudget;
            evidence = $"{report.Received}/64 decoded · {report.Corrected} FEC corrections · {report.BackupFrames} backup frames";
            if (!report.Qualified)
            {
                message = Failure switch
                {
                    LinkFailure.ClockDrift => "Y1 reference drift. Probe the measured frame skew.",
                    LinkFailure.FilterNoise => "RX B lost margin. Probe the C1 filter output.",
                    LinkFailure.OutputCurrent => "U3 latched TX overcurrent. Probe the Q1 shunt.",
                    _ => "Link qualification failed. Review the trace and fixture readings."
                };
                return;
            }

            evidence += $"\n{report.Switchovers} handovers · {report.MinimumMargin:+0.00;-0.00;0.00} dB minimum margin";
            qualifiedEpoch = epoch;
            if (stage == 3 && Award()) message = "Signal link qualified. Begin the three repair trials.";
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                faultWitnessed = reworked = false;
                QueueCheckpoint();
                message = repaired == 3 && Award() ? "Repairs verified. Start a protected downlink and open W433 during failover." :
                    "Repair verified by a fresh link trial. Begin the next hardware fault.";
            }
            else if (stage == 4) message = "Link evidence refreshed. Begin the next hardware fault.";
            else if (stage == 5) message = interrupted ? "Link requalified. Wait for fresh feedback and reset U3." :
                "Link requalified. Wait for trusted feedback before starting a protected downlink.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Qualifies || Running)
                return Reject("Finish a healthy link trial before staging a fault.");
            activeFault = repaired;
            faultWitnessed = reworked = false;
            repairTarget = -1;
            spare = 0;
            qualifiedEpoch = runEpoch = -1;
            EnergizeFault();
            message = "Fault staged. Run the link trial to capture the measured signature.";
            QueueCheckpoint();
            return true;
        }

        private void EnergizeFault()
        {
            faultEnergized = true;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                SetContact(clockContact, !FaultLive(0));
                filterSeries.SetResistance(FaultLive(1) ? 43f : .12f);
                transmitterLoads[0].SetResistance(FaultLive(2) ? 12f : 100f);
            }
        }

        public bool ProbeFault()
        {
            if (!powered || runEpoch != epoch || activeFault < 0 || reworked)
                return Reject("Run the trial with the fault present before probing.");
            bool measured = activeFault switch
            {
                0 => Failure == LinkFailure.ClockDrift && StaleClock > 0 && Live(clockPin) && !Live(trimPin) && measuredSkew > Link.SkewLimitNs,
                1 => Failure == LinkFailure.FilterNoise && FilterRejects > 0 && Live(sensorPins[1]) && Voltage(filterPin) < 4.0f,
                2 => Failure == LinkFailure.OutputCurrent && CurrentHolds > 0 && currentLatched &&
                    TransmitterCurrent(0) > Link.CurrentLimit && TransmitterCurrent(1) < .10f,
                _ => false
            };
            if (!measured) return Reject(activeFault == 0 ? "Measure the Y1 timing trace, then probe the rejected frames." :
                "The circuit reading and link trace do not identify this fault.");
            footprint = ExpectedPart;
            faultWitnessed = true;
            evidence = activeFault switch
            {
                0 => $"Y1 trim {Voltage(trimPin):F2} V · {measuredSkew:F0} ns skew · {StaleClock} timing holds.",
                1 => $"C1 supply {Voltage(filterPin):F2} V · {FilterRejects} noisy RX B samples · RX A still live.",
                _ => $"TX A {TransmitterCurrent(0) * 1000f:F0} mA · TX B {TransmitterCurrent(1) * 1000f:F0} mA · U3 latched."
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
                1 => "Spare · Y1 reference contact",
                2 => "Spare · C1 receiver B filter",
                3 => "Spare · Q1 transmitter A shunt",
                _ => "No spare selected."
            };
        }

        public bool Repair()
        {
            if (stage != 4 || activeFault < 0 || !faultWitnessed || reworked || powered)
                return Reject("Diagnose and isolate the bench before rework.");
            if (spare != activeFault + 1 || repairTarget != ExpectedPart)
                return Reject("Match the spare to the diagnosed footprint.");
            if (activeFault == 0) SetContact(clockContact, true);
            if (activeFault == 1) filterSeries.SetResistance(.12f);
            if (activeFault == 2) transmitterLoads[0].SetResistance(100f);
            reworked = activeFault switch
            {
                0 => clockContact.IsConducting,
                1 => Mathf.Abs(filterSeries.Resistance - .12f) < .001f,
                _ => Mathf.Abs(transmitterLoads[0].Resistance - 100f) < .001f
            };
            if (!reworked) return Reject("The replacement did not seat.");
            InvalidateDiagnostics();
            QueueCheckpoint();
            BoardView();
            message = "Replacement seated. Measure, recommission and repeat the link trial.";
            return true;
        }

        public bool TestWatchdog()
        {
            if (stage != 5 || !Fresh || interrupted || Running || ProtectedBusy || WatchdogActive)
                return Reject("The watchdog test needs a ready, idle bench before W433 removal.");
            watchdogObserved = false;
            watchdogUntil = time + .95f;
            message = "Holding the readiness contact high. Watch U3 reject a stale edge.";
            return true;
        }

        public bool StartProtected()
        {
            if (stage != 5 || !Fresh || interrupted && !safetyReset || Running || ProtectedBusy || WatchdogActive)
                return Reject("U3 requires fresh measurements and time-varying readiness before authorizing transmit.");
            armed = true;
            protectedReplay = new Link.Replay();
            ProtectedDelivered = verifiedPermits = ProtectedBackupFrames = 0;
            nextSample = time;
            scope?.ResetTrace();
            message = interrupted ? "Verifying both protected transmit paths through failover." :
                "Protected downlink active. Open W433 during backup transmit.";
            return true;
        }

        private void FinishProtected(Link.Replay report)
        {
            protectedReplay = null;
            armed = false;
            SetProtectedTx(-1);
            ReadEnabledGates();
            if (interrupted && safetyReset && report.Qualified && verifiedPermits == Link.FrameCount && Fresh && Award())
            {
                message = "W433 recovered. U3 verified both transmit paths and left them inhibited.";
                evidence = $"{ProtectedDelivered}/64 gate checks · {ProtectedBackupFrames} protected backup frames.";
            }
            else if (!interrupted) message = "Repeat the protected downlink and open W433 while a transmitter is active.";
            else message = "Protected downlink needs fresh link evidence and every gate check.";
        }

        private void AbortProtected()
        {
            UndeliveredAtTrip = Link.FrameCount - ProtectedDelivered;
            protectedReplay = null;
            armed = safetyReset = false;
            SetProtectedTx(-1);
            minimumEpoch = epoch + 1;
            manager.ElectricalSolver.SolveNow();
            ReadEnabledGates();
        }

        public bool OpenW433()
        {
            if (stage != 5 || !linked || interrupted || !ProtectedBusy || ProtectedDelivered == 0 || Inhibited)
                return Reject("Open W433 during protected downlink while a transmitter is active.");
            return RemoveWire(jumper) || Reject("The native registry could not open W433.");
        }

        public bool RestoreW433()
        {
            if (stage != 5 || !interrupted || linked) return Reject("Open W433 during protected downlink first.");
            if (!CreateWire(jumper)) return Reject("The native circuit rejected the W433 training contacts.");
            ReadGraph();
            message = "W433 restored. U3 still requires fresh link evidence.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !Fresh || epoch < minimumEpoch || ProtectedBusy)
                return Reject("Requalify the link and wait for fresh feedback before resetting U3.");
            safetyReset = true;
            message = "U3 reset. Run the final protected downlink.";
            return true;
        }

        private void InvalidateDiagnostics()
        {
            epoch++;
            clockEpoch = calibrationEpoch = qualifiedEpoch = runEpoch = -1;
            checkedInputs = false;
            link.ClearCalibration();
            replay = protectedReplay = null;
            feedbackIntervals = 0;
            lastRise = watchdogUntil = -1f;
            highFeedback = false;
            SetProtectedTx(-1);
        }

        private void SetProtectedTx(int channel)
        {
            bool active = channel >= 0;
            if (workload.IsClosed == active && outputContacts[0].IsClosed == (channel == 0) &&
                outputContacts[1].IsClosed == (channel == 1)) return;
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                SetContact(workload, active);
                SetContact(outputContacts[0], channel == 0);
                SetContact(outputContacts[1], channel == 1);
            }
        }

        private void ReadEnabledGates()
        {
            bool a = Live(gatePins[0]), b = Live(gatePins[1]);
            ActiveTxGate = a && b ? -2 : a ? 0 : b ? 1 : -1;
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
                message = "U3 rejected held-high feedback. Wait for new transitions before authorizing transmit.";
            }
            bool permit = stage == 5 && Fresh && !currentLatched && (!interrupted || safetyReset) &&
                ProtectedBusy && protectedReplay.Sent > 0;
            SetProtectedTx(permit ? protectedReplay.Last.Tx : -1);
            ReadEnabledGates();
            benchState = stage == 6 ? "DSX-33 QUALIFIED" : !powered ? "BENCH ISOLATED" :
                interrupted && !safetyReset ? linked ? "U3 LATCHED · RESET REQUIRED" : "W433 OPEN · U3 LATCHED" :
                currentLatched ? "TX A OVERCURRENT · U3 LATCHED" : !RailsReady ? "RAILS UNQUALIFIED" :
                ActiveTxGate == 1 ? "PROTECTED TX B" : ActiveTxGate == 0 ? "PROTECTED TX A" : "SIGNAL BENCH READY";
        }
    }
}
