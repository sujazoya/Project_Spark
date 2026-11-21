using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.PacketCitadel
{
    public sealed partial class SparkLevel117Process
    {
        public SparkTerminal standby, aux, coreRail, moduleRail, clock, timingBias, resetSense, monitorSupply, protectedFeed;
        public SparkTerminal powerWitness;
        public SparkTerminal[] moduleFeeds, biasFeeds, laneFeeds, portWitnesses;
        public SparkSwitch bench, auxGate, coreGate, moduleGate, clockContact, timingContact, resetContact, heartbeat, workload;
        public SparkSwitch[] portContacts, biasContacts, laneContacts;
        public Renderer[] portMarkers;
        public GameObject opticalArt;
        public int[] phyParts;
        public int biasPart, clockPart, lanePart, laneViaIndex;

        public enum NetworkFailure { None, OpticalBias, ClockSkew, LaneContinuity, DataCheck }
        private readonly SparkLevel117PortSample[] ports = new SparkLevel117PortSample[4];
        private SparkLevel117Clock ptp = new();
        private SparkLevel117PacketRun packets, protectedRun;
        private float time, nextStep, protectedAt, heartbeatAt, lastRise = -1f;
        private bool syncing, testing, heartbeatDrive, highFeedback;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset;
        private int epoch, trainedEpoch = -1, clockEpoch = -1, runEpoch = -1, packetEpoch = -1, minimumEpoch, feedbackIntervals;
        private int activeFault = -1, repaired, repairTarget = -1, viaInspectionEpoch = -1, spare;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool RailsReady => powered && Voltage(standby) > 4.7f && Voltage(aux) > 3.05f &&
            Voltage(coreRail) > 1.14f && Voltage(moduleRail) > 3.05f && Voltage(clock) > 4.62f && Voltage(resetSense) > 3f;
        public bool Syncing => syncing;
        public bool Testing => testing;
        public bool ClockLocked => clockEpoch == epoch && ptp.Locked;
        public bool Qualifies => RailsReady && trainedEpoch == epoch && ClockLocked && packetEpoch == epoch &&
            packets != null && packets.Qualified;
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .51f;
        public int Epoch => epoch;
        public int Repaired => repaired;
        public int TrainedLinks { get; private set; }
        public int Sent => packets?.Sent ?? 0;
        public int Received => packets?.Received ?? 0;
        public int Drops => packets?.Dropped ?? 0;
        public int CrcErrors => packets?.BadCrc ?? 0;
        public int Duplicates => packets?.Duplicates ?? 0;
        public int PeakQueue => packets?.PeakQueue ?? 0;
        public double P95 => packets?.P95 ?? 0;
        public double PriorityMean => packets?.PriorityMean ?? 0;
        public double NormalMean => packets?.RegularMean ?? 0;
        public double ClockOffsetUs => ptp.Offset;
        public double PathDelayUs => ptp.Path;
        public float MinimumPort4Eye { get; private set; }
        public int LaneOpenSamples { get; private set; }
        public bool ProtectedBusy => protectedRun != null;
        public int InterruptedFrames { get; private set; }
        public int ProtectedDelivered { get; private set; }
        public NetworkFailure Failure { get; private set; }
        public string Evidence => evidence;
        public int DeliveredAt(int port) => packets?.DeliveredAt(port) ?? 0;
        public float TransmitDbm(int port) => ports[port].TransmitDbm;

        private Route LaneVia => routes[laneViaIndex];
        private int ExpectedPart => activeFault switch { 0 => biasPart, 1 => clockPart, 2 => lanePart, _ => -1 };
        private bool FaultLive(int index) => activeFault == index && !reworked;

        private void Solve() => manager.ElectricalSolver.SolveNow();

        public bool TogglePower()
        {
            if (Testing || Syncing) return Reject("Wait for the measured test to finish before isolating the bench.");
            if (powered) { IsolatePower(); return true; }
            if (stage < 2 || !Construction || !linked && !interrupted)
                return Reject("Complete the native network and connect W305 before enabling the bench.");
            powered = true;
            ApplyPower(true);
            benchState = "LINKS UNTRAINED";
            message = "Train all four optical links using the native rail and clock witnesses.";
            return true;
        }

        private void ApplyPower(bool enabled)
        {
            SetContact(bench, enabled);
            SetContact(auxGate, enabled);
            bool rail = enabled && Voltage(powerWitness) > 4.7f;
            SetContact(coreGate, rail);
            SetContact(moduleGate, rail);
            SetContact(clockContact, rail);
            SetContact(timingContact, rail && !FaultLive(1));
            SetContact(resetContact, rail);
            for (int port = 0; port < 4; port++)
            {
                bool module = rail && parts[phyParts[port]].contact.IsClosed && Voltage(portWitnesses[port]) > 4.62f;
                SetContact(portContacts[port], module);
                SetContact(biasContacts[port], module && (port != 2 || !FaultLive(0)));
                SetContact(laneContacts[port], module);
            }
            Solve();
            SamplePorts();
        }

        private void IsolatePower()
        {
            powered = false;
            InvalidateDiagnostics();
            ApplyPower(false);
            benchState = "BENCH ISOLATED";
            message = "Module and core rails are isolated for qualified rework.";
        }

        private int SamplePorts()
        {
            bool clocked = powered && Voltage(clock) > 4.62f && Voltage(resetSense) > 3f;
            int links = 0;
            for (int port = 0; port < 4; port++)
            {
                ports[port] = new SparkLevel117PortSample(port, parts[phyParts[port]].contact.IsClosed,
                    Voltage(moduleFeeds[port]) > 3.05f, Voltage(biasFeeds[port]), Voltage(laneFeeds[port]), clocked);
                if (ports[port].Link) links++;
            }
            return links;
        }

        public bool TrainLinks()
        {
            if (!RailsReady || Testing || Syncing || ProtectedBusy)
                return Reject("Stable module rails, clock and released reset are required to train links.");
            InvalidateTrust();
            clockEpoch = -1;
            trainedEpoch = epoch;
            TrainedLinks = SamplePorts();
            benchState = TrainedLinks == 4 ? "FOUR OPTICAL LINKS TRAINED" : "OPTICAL TRAINING DEGRADED";
            WriteOpticalEvidence();
            message = "Optical transmit, receive and lane margins measured. Synchronize the independent clock.";
            if (stage == 2 && TrainedLinks == 4) Award();
            return true;
        }

        public bool ProbeOptics()
        {
            if (!RailsReady) return Reject("Power the module rail before probing optical levels.");
            SamplePorts();
            WriteOpticalEvidence();
            showPorts = true;
            if (!Testing && !Syncing) BoardView();
            message = "Per-port transmit power and lane eye margins measured from the current circuit.";
            return true;
        }

        private void WriteOpticalEvidence() => evidence =
            $"TX dBm   P1 {ports[0].TransmitDbm:F1}   P2 {ports[1].TransmitDbm:F1}   P3 {ports[2].TransmitDbm:F1}   P4 {ports[3].TransmitDbm:F1}\n" +
            $"RX dBm   P1 {ports[0].ReceiveDbm:F1}   P2 {ports[1].ReceiveDbm:F1}   P3 {ports[2].ReceiveDbm:F1}   P4 {ports[3].ReceiveDbm:F1}";

        public bool SyncPtp()
        {
            if (stage < 3 || !RailsReady || trainedEpoch != epoch || Syncing || Testing || ProtectedBusy ||
                !ports[0].Link) return Reject("Train the reference port and supply clock before capturing PTP exchanges.");
            InvalidateTrust();
            clockEpoch = -1;
            ptp = new SparkLevel117Clock();
            syncing = true;
            nextStep = time + .035f;
            benchState = "MEASURING PTP TIMESTAMPS";
            message = "Estimating offset and path delay from 32 four-timestamp exchanges.";
            return true;
        }

        public bool RunTraffic()
        {
            if (stage < 3 || !RailsReady || trainedEpoch != epoch || clockEpoch != epoch ||
                Testing || Syncing || ProtectedBusy)
                return Reject("Train the four ports and capture the current PTP measurement before sending packets.");
            InvalidateTrust();
            runEpoch = epoch;
            Failure = NetworkFailure.None;
            LaneOpenSamples = 0;
            MinimumPort4Eye = 1f;
            packets = new SparkLevel117PacketRun(0x1d5722a5u ^ unchecked((uint)epoch * 0x9e3779b9u));
            testing = true;
            nextStep = time + .035f;
            scope?.ResetTrace();
            benchState = "TESTING DATA PLANE";
            message = "Sending 512 seeded frames through four egress queues with CRC and priority verification.";
            return true;
        }

        private void TickController(float dt)
        {
            time += dt;
            if (!powered) return;
            if (syncing && time >= nextStep) ObserveClock();
            if (testing && time >= nextStep) ObserveTraffic();
            if (ProtectedBusy && time >= protectedAt)
            {
                if (!Fresh || Inhibited) AbortProtectedTraffic();
                else
                {
                    protectedRun.Step(ports);
                    ProtectedDelivered = protectedRun.Received;
                    if (protectedRun.Complete)
                    {
                        message = $"Protected burst delivered {ProtectedDelivered}/{protectedRun.Sent} accepted frames.";
                        protectedRun = null;
                    }
                    else protectedAt = time + .03f;
                }
            }
            if (packetEpoch == epoch && (!RailsReady || Voltage(timingBias) < 4.62f || SamplePorts() != 4))
                InvalidateTrust();
            TickSupervisor();
        }

        private void ObserveClock()
        {
            if (!RailsReady || SamplePorts() == 0 || !ports[0].Link)
            {
                syncing = false;
                message = "Timestamp collection stopped because its reference link was lost.";
                return;
            }
            ptp.Observe(Voltage(timingBias) > 4.62f);
            nextStep = time + .029f;
            if (!ptp.Complete) return;
            syncing = false;
            clockEpoch = epoch;
            benchState = ClockLocked ? "PTP LOCKED" : "PTP HOLDOVER";
            evidence = $"PTP {ptp.Offset:+0.00;-0.00;0.00} µs offset · {ptp.Path:F2} µs path\n32 exchanges · late peak {ptp.PeakResidual:F2} µs";
            message = ClockLocked ? "Clock locked. Measure traffic and priority queues." :
                "The clock cannot stay locked. Run the packet comparison to isolate timing from data integrity.";
        }

        private void ObserveTraffic()
        {
            if (!RailsReady)
            {
                testing = false;
                Failure = NetworkFailure.DataCheck;
                message = "Data test stopped when native power or reset dropped.";
                return;
            }
            bool laneClosed = !FaultLive(2) || packets.Slot < 14 || packets.Slot % 9 > 1;
            if (laneContacts[3].IsClosed != laneClosed)
            {
                SetContact(laneContacts[3], laneClosed);
                Solve();
            }
            SamplePorts();
            MinimumPort4Eye = Mathf.Min(MinimumPort4Eye, ports[3].Eye);
            if (Voltage(laneFeeds[3]) < .15f) LaneOpenSamples++;
            scope?.Record(ports[0].Eye, ports[1].Eye, ports[2].Eye, ports[3].Eye);
            packets.Step(ports);
            nextStep = time + .027f;
            if (!packets.Complete) return;
            testing = false;
            SetContact(laneContacts[3], true);
            Solve();
            SamplePorts();
            FinishTraffic();
        }

        private void FinishTraffic()
        {
            Failure = !ports[2].OpticalReady && Drops > 0 ? NetworkFailure.OpticalBias :
                !ClockLocked && packets.Qualified ? NetworkFailure.ClockSkew :
                LaneOpenSamples > 0 && CrcErrors > 0 ? NetworkFailure.LaneContinuity :
                runEpoch != epoch || !packets.Qualified || !ClockLocked || SamplePorts() != 4 ? NetworkFailure.DataCheck :
                NetworkFailure.None;
            evidence = $"{Received}/{Sent} accepted · {Drops} dropped · {CrcErrors} CRC\n" +
                $"P95 {P95:F2} µs · PTP {ClockOffsetUs:+0.00;-0.00;0.00} µs";
            if (Failure != NetworkFailure.None)
            {
                benchState = "QUALIFICATION FAILED";
                message = "Compare measured optical level, timing offset and port-four eye to localize this fault.";
                QueueCheckpoint();
                return;
            }

            packetEpoch = epoch;
            benchState = "DATA AND TIMING VERIFIED";
            message = "All four ports passed CRC, latency, priority and timing checks.";
            if (stage == 3)
            {
                if (Award()) message = "Packet and PTP evidence accepted. Begin the first controlled fault.";
            }
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = repairTarget = -1;
                faultWitnessed = reworked = false;
                if (repaired == 3)
                {
                    if (Award()) message = "All repairs verified. Wait for U36 and open W305 during a protected burst.";
                }
                else message = "A fresh link, timing and packet test verified the repair. Start the next fault.";
            }
            QueueCheckpoint();
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || !Qualifies || activeFault >= 0 || repaired >= 3)
                return Reject("Complete a healthy optical, clock and packet run before starting the next fault.");
            activeFault = repaired;
            repairTarget = -1;
            faultWitnessed = reworked = false;
            spare = 0;
            Failure = NetworkFailure.None;
            InvalidateDiagnostics();
            ApplyPower(true);
            benchState = "FAULT TRIAL";
            message = "A fault was introduced. Train the ports, capture PTP and measure a new packet run.";
            QueueCheckpoint();
            return true;
        }

        public bool LocateFault()
        {
            bool observed = powered && runEpoch == epoch && (activeFault switch
            {
                0 => Failure == NetworkFailure.OpticalBias && ports[2].TransmitDbm < -20f && Voltage(biasFeeds[2]) < .15f,
                1 => Failure == NetworkFailure.ClockSkew && Math.Abs(ClockOffsetUs) > 4 && CrcErrors == 0,
                2 => Failure == NetworkFailure.LaneContinuity && MinimumPort4Eye < .52f && LaneOpenSamples > 0,
                _ => false
            });
            if (!observed || reworked) return Reject("Measure the current fault before selecting a repair target.");
            faultWitnessed = true;
            footprint = repairTarget = ExpectedPart;
            FocusPart();
            message = activeFault switch
            {
                0 => $"P3 bias collapsed. Isolate U5 and select the matched optical driver.",
                1 => $"Y2 measured {ClockOffsetUs:F2} µs offset with clean CRCs. Isolate and select its matched oscillator.",
                _ => $"P4 eye fell to {MinimumPort4Eye:P0}. Isolate and inspect {LaneVia.viaId}."
            };
            QueueCheckpoint();
            return true;
        }

        public bool InspectLaneVia()
        {
            if (stage != 4 || powered || !faultWitnessed || activeFault != 2 || repairTarget != lanePart)
                return Reject("Locate the measured P4 fault and isolate the bench before inspecting the lane.");
            SelectRoute(laneViaIndex);
            PadView();
            viaInspectionEpoch = epoch;
            message = $"{LaneVia.viaId} spans L{LaneVia.layer:00} to L{LaneVia.exitLayer:00}. Repair from the entry layer.";
            return true;
        }

        public bool SelectSpare()
        {
            if (powered || activeFault is < 0 or > 1 || repairTarget != ExpectedPart || !faultWitnessed)
                return Reject("Isolate and select the measured module or oscillator before choosing a spare.");
            spare = (spare + 1) % 3;
            message = spare switch
            {
                0 => "Spare A: incompatible drive or footprint.",
                1 => activeFault == 0 ? "Spare B: matched optical-bias driver." : "Spare B: matched Y2 precision oscillator.",
                _ => "Spare C: insufficient stability margin."
            };
            return true;
        }

        public bool Repair()
        {
            if (stage != 4 || powered || !faultWitnessed || reworked || repairTarget != ExpectedPart)
                return Reject("Isolate the bench and select the measured repair target.");
            if (activeFault == 2 && (viaInspectionEpoch != epoch || layer != LaneVia.layer || !LaneVia.barrel.IsClosed))
                return Reject("Inspect the diagnosed lane span and select its entry layer.");
            if (activeFault < 2 && spare != 1) return Reject("The measured target requires its matched training spare.");
            reworked = true;
            message = activeFault switch
            {
                0 => "U5 driver renewed. Repeat optical training and CRC testing.",
                1 => "Y2 replaced. Capture new timestamp exchanges and packet evidence.",
                _ => "P4 escape via reworked. Repeat the warmed lane test."
            };
            QueueCheckpoint();
            return true;
        }

        public bool StartProtectedBurst()
        {
            if (!Fresh || Inhibited || Testing || Syncing || ProtectedBusy)
                return Reject("U36 must authorize a fresh network epoch before protected traffic.");
            protectedRun = new SparkLevel117PacketRun(0xf235762bu ^ (uint)epoch);
            ProtectedDelivered = 0;
            protectedAt = time + .025f;
            message = "Protected traffic burst in progress.";
            return true;
        }

        private void AbortProtectedTraffic()
        {
            if (!ProtectedBusy) return;
            InterruptedFrames = SparkLevel117PacketRun.FrameCount - protectedRun.Received;
            protectedRun = null;
        }

        public bool OpenW305()
        {
            if (stage != 5 || !armed || !Fresh || interrupted)
                return Reject("Wait for U36 to authorize the protected workload.");
            if (!ProtectedBusy && !StartProtectedBurst()) return false;
            minimumEpoch = epoch + 1;
            if (!RemoveWire(jumper)) return Reject("The native circuit could not open W305.");
            Solve();
            return true;
        }

        public bool RestoreW305()
        {
            if (stage != 5 || !interrupted || linked) return Reject("Record the supervised W305 opening first.");
            SelectRoute(routes.Length);
            if (!CreateWire(jumper)) return Reject("The native circuit rejected W305.");
            ReadGraph();
            Solve();
            message = "W305 restored. Retrain optics, resynchronize PTP and rerun packet tests.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || epoch < minimumEpoch || !Fresh || !Inhibited ||
                Voltage(monitorSupply) < 3f)
                return Reject("U36 requires a connected jumper, fresh optics, locked PTP and current packets.");
            safetyReset = true;
            ClearHeartbeat();
            Solve();
            message = "U36 reset accepted. Waiting for a new measured heartbeat.";
            return true;
        }

        private void InvalidateDiagnostics()
        {
            epoch++;
            AbortProtectedTraffic();
            testing = syncing = false;
            trainedEpoch = clockEpoch = -1;
            TrainedLinks = 0;
            InvalidateTrust();
        }

        private void InvalidateTrust()
        {
            packetEpoch = -1;
            ClearHeartbeat();
        }

        private void ClearHeartbeat()
        {
            feedbackIntervals = 0;
            lastRise = -1;
            highFeedback = heartbeatDrive = false;
            heartbeatAt = time;
            SetContact(heartbeat, false);
            SetContact(workload, false);
        }

        private void TickSupervisor()
        {
            if (Qualifies && time >= heartbeatAt)
            {
                heartbeatDrive = !heartbeatDrive;
                heartbeatAt = time + .15f;
                SetContact(heartbeat, heartbeatDrive);
                Solve();
                bool high = Voltage(jumper.to) > 2.8f;
                if (high && !highFeedback)
                {
                    float interval = time - lastRise;
                    feedbackIntervals = lastRise >= 0 && interval is >= .23f and <= .46f ? feedbackIntervals + 1 : 0;
                    lastRise = time;
                }
                highFeedback = high;
            }
            bool permit = Fresh && Voltage(monitorSupply) > 3f && (!interrupted || safetyReset && epoch >= minimumEpoch);
            if (workload.IsClosed != permit)
            {
                SetContact(workload, permit);
                Solve();
            }
            if (!permit || Voltage(protectedFeed) < 3f || stage != 5) return;
            if (!interrupted) armed = true;
            else if (safetyReset && Award())
            {
                benchState = "PROTECTED NETWORK AUTHORIZED";
                message = "U36 restored protected traffic following the W305 recovery and fresh qualification.";
            }
        }
    }
}
