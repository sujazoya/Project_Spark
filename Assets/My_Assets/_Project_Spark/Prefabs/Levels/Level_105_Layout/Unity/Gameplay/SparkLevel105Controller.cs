using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.ExpansionNexus
{
    public sealed partial class SparkLevel105Process
    {
        public enum CardPosition { Detached, Grounded, Seated }
        public enum LinkState { PoweredOff, Reset, Detect, Polling, Configuration, L0, Recovery, Fault }

        [Serializable]
        public sealed class Lane
        {
            public SparkSwitch[] drive = new SparkSwitch[4];
            public SparkTerminal[] sense = new SparkTerminal[4];
            public float[] lengthMm = new float[4];
        }

        public Transform card, cardArt;
        public BoxCollider cardSurface;
        public Vector3 parkPosition, seatPosition;
        public Quaternion parkRotation;
        public SparkTerminal standby, aux, endpointPower, cardGround, presence, clockP, clockN, resetSense, monitorSupply, protectedFeed;
        public SparkSwitch bench, auxGate, groundContact, presenceContact, powerContact, resetGate, clockDriveP, clockDriveN;
        public SparkSwitch coupling, clockBuffer, intermittentContact, heartbeat, workload;
        public SparkSwitch[] slotContacts;
        public Lane[] lanes;
        public int couplingPart, clockPart, slotPart;

        private static readonly string[] FaultNames =
        {
            "C22 · Lane 2 TX coupling", "U3 · REFCLK negative leg", "J1 · Lane 3 RX contact"
        };
        private const float LinkMarginPs = 42f;
        private readonly float[] txMargin = new float[4], rxMargin = new float[4];
        private readonly SparkLevel105Traffic traffic = new();
        private CardPosition cardPosition;
        private LinkState linkState;
        private float time, powerAt, clockAt, linkAt, trafficAt, feedbackAt, lastRise = -1f;
        private bool feedbackHigh, heartbeatDrive, armed, interrupted, safetyReset;
        private bool clockPhase, faultWitnessed, reworked, testing, configRead;
        private int clockMask, clockSamples, trainingPhases, linkMask, width, epoch, enumeratedEpoch = -1, qualifiedEpoch = -1;
        private int activeFault = -1, repaired, targetFaultPart = -1, seatingRevision, disturbedSeat;
        private int feedbackIntervals, faultContactOpens, interruptionEpoch, captures;
        private float rate;
        private string evidence = "";

        public CardPosition Card => cardPosition;
        public LinkState Link => linkState;
        public int Width => width;
        public float Rate => rate;
        public int Epoch => epoch;
        public int Repaired => repaired;
        public bool ClockReady => clockMask == 3;
        public bool CardPowered => powered && Voltage(endpointPower) > 3f;
        public bool ResetReleased => Voltage(resetSense) > 3f;
        public bool Training => linkState is LinkState.Detect or LinkState.Polling or LinkState.Configuration;
        public bool FullLink => linkState == LinkState.L0 && width == 4 && rate == 5f && ClockReady && ResetReleased && CardPowered;
        public bool Enumerated => FullLink && enumeratedEpoch == epoch;
        public bool Qualifies => Enumerated && qualifiedEpoch == epoch && traffic.Errors == 0;
        public bool Testing => testing;
        public bool FaultWitnessed => faultWitnessed;
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .52f;
        public int Captures => captures;
        public int Frames => traffic.Frames;
        public int PayloadBytes => traffic.Bytes;
        public int Errors => traffic.Errors;
        public int FailingLane => traffic.FirstLane;
        public int FaultContactOpens => faultContactOpens;
        public float TxMargin(int lane) => txMargin[lane];
        public float RxMargin(int lane) => rxMargin[lane];
        public string Evidence => evidence;

        private void Solve() => manager.ElectricalSolver.SolveNow();
        private bool FaultLive(int fault) => activeFault == fault && !reworked;

        public bool AlignCard()
        {
            if (stage < 2 || powered || cardPosition != CardPosition.Detached)
                return Reject("Isolate power and start with ECX-4 in its tray.");
            SetCardPosition(CardPosition.Grounded);
            InspectSlot();
            message = "ECX-4 is aligned. The ground and presence contacts close first. Seat it to engage signals and auxiliary power.";
            QueueCheckpoint();
            return true;
        }

        public bool SeatCard()
        {
            if (stage < 2 || powered || cardPosition != CardPosition.Grounded)
                return Reject("Align ECX-4 with bench power off before pushing it into J1.");
            SetCardPosition(CardPosition.Seated);
            seatingRevision++;
            InspectSlot();
            message = "Retention latched. Use bench power to measure the slot rails, clock and reset.";
            QueueCheckpoint();
            return true;
        }

        public bool RemoveCard()
        {
            if (stage < 2 || powered || cardPosition == CardPosition.Detached)
                return Reject("Isolate bench power before removing ECX-4.");
            SetCardPosition(CardPosition.Detached);
            BoardView();
            QueueCheckpoint();
            message = "ECX-4 is detached from J1. All slot contacts are open.";
            return true;
        }

        private void SetCardPosition(CardPosition position)
        {
            cardPosition = position;
            bool seatedCard = position == CardPosition.Seated;
            SetContact(powerContact, seatedCard);
            foreach (var contact in slotContacts) SetContact(contact, seatedCard);
            SetContact(presenceContact, position != CardPosition.Detached);
            SetContact(groundContact, position != CardPosition.Detached);
            if (position == CardPosition.Detached)
                card.SetLocalPositionAndRotation(parkPosition, parkRotation);
            else
                card.SetLocalPositionAndRotation(seatPosition + (seatedCard ? Vector3.zero : new Vector3(0, .0075f, -.003f)),
                    Quaternion.Euler(seatedCard ? -90f : -77f, 0, 0));
            InvalidateCommissioning();
            Solve();
        }

        public bool TogglePower()
        {
            if (powered)
            {
                IsolatePower();
                message = "Bench isolated. Card service and targeted repair are available.";
                return true;
            }
            if (stage < 2 || !Construction || cardPosition == CardPosition.Detached || !linked && !interrupted)
                return Reject("Complete copper and W133, then align ECX-4 before energizing the current-limited bench.");
            powered = true;
            powerAt = time;
            SetContact(bench, true);
            InvalidateCommissioning();
            Solve();
            message = "Protected 12 V applied. Waiting for auxiliary power, complementary REFCLK and reset release.";
            QueueCheckpoint();
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            SetContact(bench, false);
            SetContact(auxGate, false);
            InvalidateCommissioning();
            Solve();
            QueueCheckpoint();
        }

        private void InvalidateCommissioning()
        {
            clockMask = clockSamples = 0;
            clockAt = time;
            clockPhase = false;
            SetContact(resetGate, false);
            SetContact(clockDriveP, false);
            SetContact(clockDriveN, false);
            InvalidateDiagnostics();
        }

        private void InvalidateDiagnostics()
        {
            linkState = powered ? LinkState.Reset : LinkState.PoweredOff;
            width = 0;
            rate = 0;
            testing = configRead = false;
            enumeratedEpoch = qualifiedEpoch = -1;
            Array.Clear(txMargin, 0, txMargin.Length);
            Array.Clear(rxMargin, 0, rxMargin.Length);
            ClearHeartbeat();
        }

        public bool Train()
        {
            if (!powered || !CardPowered || Voltage(presence) < 3f || cardPosition != CardPosition.Seated || Training || testing)
                return Reject("Fully seat and power ECX-4 before starting link training.");
            InvalidateDiagnostics();
            traffic.Reset();
            evidence = "";
            epoch++;
            trainingPhases = 0;
            linkMask = 15;
            linkState = LinkState.Detect;
            linkAt = time + .15f;
            message = "Detecting the endpoint and qualifying the differential clock.";
            return true;
        }

        public bool ReadConfiguration()
        {
            if (!FullLink || testing) return Reject("Reach Gen2-style ×4 L0 before reading the configuration header.");
            configRead = true;
            message = "ECX-4 responded. Device E104, training endpoint class, 4 KiB memory request. Assign its resource window.";
            return true;
        }

        public bool AssignResources()
        {
            if (!FullLink || !configRead || testing) return Reject("Read the ECX-4 configuration header on the current link first.");
            enumeratedEpoch = epoch;
            message = "ECX-4 ready. BAR0 is mapped to 0x80001000–0x80001FFF. Run bidirectional traffic.";
            return true;
        }

        public bool RunTraffic()
        {
            if (!Enumerated || testing) return Reject("Enumerate ECX-4 on the current ×4 link first.");
            testing = true;
            traffic.Reset();
            faultContactOpens = 0;
            qualifiedEpoch = -1;
            trafficAt = time;
            ClearHeartbeat();
            message = "Sending and checking sequence-tagged frames in both directions.";
            return true;
        }

        private void TickController(float elapsed)
        {
            time += elapsed;
            if (!powered) return;
            if (!auxGate.IsClosed && time - powerAt >= .18f && Voltage(standby) > 11.5f)
            {
                SetContact(auxGate, true);
                Solve();
            }
            if (time >= clockAt && CardPowered) SampleClock();
            if (Training && time >= linkAt) TickTraining();
            if (testing && time >= trafficAt) TickTransfer();
            if (stage == 2 && FullLink && Award()) message = "Gen2-style ×4 L0 established. Read the endpoint configuration.";
            if (stage == 3 && Qualifies && Award()) message = "Bidirectional traffic passed. Start the first controlled fault trial.";
            if (stage == 4 && reworked && Qualifies)
            {
                repaired++;
                activeFault = targetFaultPart = -1;
                faultWitnessed = reworked = false;
                QueueCheckpoint();
                if (repaired == 3)
                {
                    if (Award()) message = "Three repairs verified. Wait for the trusted W133 cadence, then open the feedback jumper.";
                }
                else message = "Repair verified on a fresh link and traffic run. Start the next fault trial.";
            }
            TickSupervisor();
        }

        private void SampleClock()
        {
            clockAt = time + .12f;
            clockPhase = !clockPhase;
            SetContact(clockDriveP, clockPhase);
            SetContact(clockDriveN, !clockPhase);
            Solve();
            float differential = Voltage(clockP) - Voltage(clockN);
            bool valid = clockPhase ? differential > 2.7f : differential < -2.7f;
            clockMask = valid ? clockMask | (clockPhase ? 1 : 2) : 0;
            clockSamples++;
            if (resetGate.IsClosed != ClockReady)
            {
                SetContact(resetGate, ClockReady);
                Solve();
            }
            if (!ClockReady && linkState == LinkState.L0)
            {
                InvalidateDiagnostics();
                linkState = LinkState.Recovery;
                message = "REFCLK lost a complementary state. Reset asserted and link evidence cleared.";
            }
        }

        private void TickTraining()
        {
            linkAt = time + .16f;
            switch (linkState)
            {
                case LinkState.Detect:
                    linkState = LinkState.Polling;
                    message = "Receiver detected. Polling both states of each differential pair.";
                    break;
                case LinkState.Polling:
                    if (!ClockReady || !ResetReleased)
                    {
                        if (clockSamples < 2) return;
                        linkState = LinkState.Fault;
                        evidence = "Both REFCLK polarities were not verified. Reset is asserted.";
                        message = "Polling stopped. Inspect the reference clock.";
                        return;
                    }
                    bool positive = trainingPhases == 0;
                    DriveLanes(positive);
                    linkMask &= ReadLanes(positive, true);
                    if (++trainingPhases == 2) linkState = LinkState.Configuration;
                    break;
                case LinkState.Configuration:
                    width = linkMask == 15 ? 4 : (linkMask & 3) == 3 ? 2 : (linkMask & 1) == 1 ? 1 : 0;
                    rate = width == 0 ? 0 : 5f;
                    linkState = width == 0 ? LinkState.Fault : LinkState.L0;
                    evidence = width == 4 ? "All eight TX/RX pairs passed both differential states."
                        : $"Only the first {width} contiguous lanes can negotiate. Failed lane mask 0x{15 ^ linkMask:X}.";
                    message = width == 4 ? "Link trained. Read and assign ECX-4 configuration before each new traffic run."
                        : "Link width is degraded. Inspect the lane margins and coupling path.";
                    break;
            }
        }

        private void DriveLanes(bool positive)
        {
            foreach (var lane in lanes)
            for (int i = 0; i < lane.drive.Length; i++)
                SetContact(lane.drive[i], i % 2 == (positive ? 0 : 1));
            Solve();
        }

        private int ReadLanes(bool positive, bool train)
        {
            int mask = 0;
            for (int i = 0; i < lanes.Length; i++)
            {
                var lane = lanes[i];
                float tx = PairMargin(lane, 0, positive), rx = PairMargin(lane, 2, positive);
                if (train)
                {
                    txMargin[i] = trainingPhases == 0 ? tx : Mathf.Min(txMargin[i], tx);
                    rxMargin[i] = trainingPhases == 0 ? rx : Mathf.Min(rxMargin[i], rx);
                    if (tx >= LinkMarginPs && rx >= LinkMarginPs) mask |= 1 << i;
                }
                else
                {
                    if (tx >= LinkMarginPs) mask |= 1 << (2 * i);
                    if (rx >= LinkMarginPs) mask |= 1 << (2 * i + 1);
                }
            }
            return mask;
        }

        private float PairMargin(Lane lane, int first, bool positive)
        {
            float volts = (Voltage(lane.sense[first]) - Voltage(lane.sense[first + 1])) * (positive ? 1 : -1);
            if (volts < 2.6f) return 0f;
            float a = lane.lengthMm[first], b = lane.lengthMm[first + 1];
            return Mathf.Max(0, 95f - Mathf.Abs(a - b) * .62f - Mathf.Max(a, b) * .17f - (3.3f - volts) * 25f);
        }

        private void TickTransfer()
        {
            trafficAt = time + .078f;
            bool disturbance = FaultLive(2) && traffic.Burst % 4 is 1 or 2;
            SetContact(intermittentContact, !disturbance);
            if (disturbance) faultContactOpens++;
            bool positive = traffic.Burst % 2 == 1;
            DriveLanes(positive);
            traffic.CompareBurst(FullLink ? ReadLanes(positive, false) : 0);
            if (traffic.Burst < SparkLevel105Traffic.Bursts) return;

            testing = false;
            SetContact(intermittentContact, true);
            Solve();
            captures++;
            evidence = $"Run {captures} · epoch {epoch} · {traffic.Frames} frames · {traffic.Bytes} B · {traffic.Errors} CRC errors";
            if (traffic.Errors == 0 && Enumerated)
            {
                qualifiedEpoch = epoch;
                message = "Traffic verified in both directions. Link and endpoint evidence are current.";
            }
            else
            {
                linkState = LinkState.Recovery;
                qualifiedEpoch = enumeratedEpoch = -1;
                message = $"Recovery required. First damaged frame on lane {traffic.FirstLane} {traffic.FirstDirection}.";
            }
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Qualifies)
                return Reject("Complete a clean traffic run before starting the next fault trial.");
            activeFault = repaired;
            faultWitnessed = reworked = false;
            disturbedSeat = seatingRevision;
            targetFaultPart = -1;
            traffic.Reset();
            evidence = "";
            if (FaultLive(1))
                InvalidateCommissioning();
            else
                InvalidateDiagnostics();
            ConfigureFaultDevices();
            Solve();
            message = "Fault injected. Repeat link training and, if it reaches ×4, enumerate and run traffic. Inspect the measured failure.";
            QueueCheckpoint();
            return true;
        }

        private void ConfigureFaultDevices()
        {
            SetContact(coupling, !FaultLive(0));
            SetContact(clockBuffer, !FaultLive(1));
            SetContact(intermittentContact, cardPosition == CardPosition.Seated);
        }

        public bool LocateFault()
        {
            bool proven = powered && (activeFault switch
            {
                0 => linkState == LinkState.L0 && width == 2 && txMargin[2] < LinkMarginPs && rxMargin[2] >= LinkMarginPs,
                1 => linkState == LinkState.Fault && clockSamples >= 2 && !ClockReady && !ResetReleased,
                2 => linkState == LinkState.Recovery && traffic.FirstLane == 3 && traffic.Errors > 0 && faultContactOpens > 0,
                _ => false
            });
            if (!proven || reworked) return Reject("Collect the failing clock, lane or traffic evidence before locating a repair.");
            faultWitnessed = true;
            targetFaultPart = ExpectedFaultPart;
            footprint = targetFaultPart;
            if (activeFault == 2) InspectSlot();
            else FocusPart();
            evidence = FaultNames[activeFault] + "\n" + evidence;
            message = activeFault == 2
                ? "Lane 3 opens under traffic load. Isolate power, remove ECX-4, then align and reseat it before contact rework."
                : "The measured fault is highlighted. Isolate power before replacing or reworking the selected part.";
            QueueCheckpoint();
            return true;
        }

        private int ExpectedFaultPart => activeFault switch { 0 => couplingPart, 1 => clockPart, 2 => slotPart, _ => -1 };

        public bool Repair()
        {
            if (stage != 4 || powered || !faultWitnessed || reworked || targetFaultPart != ExpectedFaultPart)
                return Reject("Choose the diagnosed target and isolate bench power before rework.");
            if (activeFault == 2 && (cardPosition != CardPosition.Seated || seatingRevision <= disturbedSeat))
                return Reject("Remove, align and fully reseat ECX-4 to service the measured lane contact.");
            reworked = true;
            ConfigureFaultDevices();
            Solve();
            message = "Contact rework applied. Power up, train, enumerate and run fresh traffic to verify the repair.";
            BoardView();
            QueueCheckpoint();
            return true;
        }

        public bool OpenW133()
        {
            if (stage != 5 || !powered || !armed || interrupted || !Fresh || !linked) return Reject("Wait for a qualified W133 heartbeat and an enabled permit.");
            interruptionEpoch = epoch;
            if (!RemoveWire(jumper)) return Reject("Could not open the physical W133 link.");
            Solve();
            return true;
        }

        public bool RestoreW133()
        {
            if (stage != 5 || !interrupted || linked) return Reject("The supervised W133 opening must be recorded first.");
            SelectRoute(routes.Length);
            if (!CreateWire(jumper)) return Reject("The W133 terminals rejected the connection.");
            ReadGraph();
            Solve();
            message = "W133 restored. Retrain ×4, read and assign ECX-4, run a new transfer and wait for fresh feedback.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || !Fresh || epoch <= interruptionEpoch || !Inhibited || Voltage(monitorSupply) < 3f)
                return Reject("Restore W133 and verify fresh link, enumeration, traffic and heartbeat before resetting U13.");
            safetyReset = true;
            message = "U13 reset accepted. Checking the independent workload permit.";
            return true;
        }

        private void ClearHeartbeat()
        {
            feedbackIntervals = 0;
            lastRise = -1;
            feedbackHigh = heartbeatDrive = false;
            feedbackAt = time;
            SetContact(heartbeat, false);
            SetContact(workload, false);
        }

        private void TickSupervisor()
        {
            if (Qualifies && time >= feedbackAt)
            {
                heartbeatDrive = !heartbeatDrive;
                feedbackAt = time + .15f;
                SetContact(heartbeat, heartbeatDrive);
                Solve();
                bool high = Voltage(jumper.to) > 2.8f;
                if (high && !feedbackHigh)
                {
                    float period = time - lastRise;
                    feedbackIntervals = lastRise >= 0 && period is >= .23f and <= .45f ? feedbackIntervals + 1 : 0;
                    lastRise = time;
                }
                feedbackHigh = high;
            }
            bool permit = Fresh && Voltage(monitorSupply) > 3f && (!interrupted || safetyReset);
            if (workload.IsClosed != permit)
            {
                SetContact(workload, permit);
                Solve();
            }
            if (!permit || Voltage(protectedFeed) <= 3f) return;
            if (stage == 5 && !interrupted) armed = true;
            if (stage == 5 && interrupted && safetyReset && Award())
                message = "Expansion Nexus certified. W133 and the independent U13 workload permit are restored.";
        }
    }
}
