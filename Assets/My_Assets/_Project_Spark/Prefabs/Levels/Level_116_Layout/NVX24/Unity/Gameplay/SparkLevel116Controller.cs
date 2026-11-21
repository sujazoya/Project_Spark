using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.DataPhoenix
{
    public sealed partial class SparkLevel116Process
    {
        public SparkTerminal standby, aux, controllerRail, nandRail, linkSense, reserveSense, monitorSupply, protectedFeed;
        public SparkTerminal powerWitness, bankWitness;
        public SparkTerminal[] channelFeeds;
        public SparkSwitch bench, auxGate, controllerGate, nandGate, linkContact, reserveCharge, heartbeat, workload;
        public SparkSwitch[] channelContacts;
        public Renderer[] channelMarkers;
        public GameObject heatSpreader;
        public int channelPart, capacitorPart, metadataPart, channelViaIndex;

        private enum TestPhase { Idle, Traffic, EarlyCut, ReserveCut }
        public enum StorageFailure { None, ChannelTimeout, Ecc, HoldUp, Replay }
        private const float LoadWatts = 1.4f, NominalEsr = .09f, HighEsr = .70f, FullWriteSeconds = .188f;
        private readonly SparkLevel116Flash flash = new();
        private TestPhase test;
        private float time, nextStep, protectedAt, heartbeatAt, lastRise = -1f;
        private bool probed, queueReady, earlyReplay, rejectingDouble, protectedBusy, heartbeatDrive, highFeedback;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset;
        private int epoch, runEpoch = -1, qualifiedEpoch = -1, minimumEpoch, feedbackIntervals;
        private int activeFault = -1, repaired, repairTarget = -1, viaInspectionEpoch = -1;
        private int channelMask, address, spare, corrected, bitsRead, timeouts, cutAcks;
        private uint earlyExpected, reserveExpected, seed;
        private string evidence = "", benchState = "BENCH ISOLATED";

        public bool RailsReady => powered && Voltage(standby) > 4.7f && Voltage(aux) > 3.05f &&
            Voltage(controllerRail) > 1.14f && Voltage(nandRail) > 1.71f;
        public bool Testing => test != TestPhase.Idle;
        public bool Qualifies => RailsReady && queueReady && channelMask == 255 && qualifiedEpoch == epoch &&
            Voltage(linkSense) > 4.6f && ValidChannels() == 255;
        public bool Fresh => linked && Qualifies && feedbackIntervals >= 2 && time - lastRise < .51f;
        public int Epoch => epoch;
        public int Repaired => repaired;
        public int ReadyChannels { get; private set; }
        public int CorrectedPages => corrected;
        public int VerifiedBits => bitsRead;
        public int CommandTimeouts => timeouts;
        public int CutAcknowledgements => cutAcks;
        public int RecoveredPages => flash.RecoveredPages;
        public int RetiredMetadataBlocks => flash.RetiredMetadataBlocks;
        public int Acknowledgements => flash.Acknowledgements;
        public bool ProtectedBusy => protectedBusy;
        public bool RejectedDoubleBit => rejectingDouble;
        public bool EarlierCheckpointRecovered => earlyReplay;
        public bool AckSafeAtInterruption { get; private set; }
        public float HoldSeconds { get; private set; }
        public float CutPrimaryVolts { get; private set; }
        public StorageFailure Failure { get; private set; }
        public string Evidence => evidence;

        private Route ChannelVia => routes[channelViaIndex];
        private int ExpectedPart => activeFault switch { 0 => channelPart, 1 => capacitorPart, 2 => metadataPart, _ => -1 };
        private bool FaultLive(int index) => activeFault == index && !reworked;

        private void Solve() => manager.ElectricalSolver.SolveNow();

        public bool TogglePower()
        {
            if (Testing) return Reject("Wait for the supervised power-cut sequence to finish.");
            if (powered) { IsolatePower(); return true; }
            if (stage < 2 || !Construction || !linked && !interrupted)
                return Reject("Complete the native network and connect W289 before enabling the bench.");
            powered = true;
            SetContact(auxGate, true);
            SetPrimary(true);
            benchState = "NAND UNINITIALIZED";
            message = "Initialize all eight NAND channels, then probe the reserve and open the command queue.";
            return true;
        }

        private void SetPrimary(bool online)
        {
            SetContact(bench, online);
            bool ready = online && Voltage(powerWitness) > 4.7f;
            SetContact(controllerGate, ready);
            SetContact(nandGate, ready);
            SetContact(linkContact, ready);
            SetContact(reserveCharge, online && Voltage(bankWitness) > 4.7f);
            for (int i = 0; i < channelContacts.Length; i++)
                SetContact(channelContacts[i], ready && (i != 3 || !FaultLive(0)));
            Solve();
        }

        private void IsolatePower()
        {
            powered = false;
            InvalidateDiagnostics();
            SetPrimary(false);
            SetContact(auxGate, false);
            Solve();
            benchState = "BENCH ISOLATED";
            message = "Native controller and NAND rails are isolated for rework.";
        }

        public bool InitializeNand()
        {
            if (!RailsReady || Testing) return Reject("Stable controller and NAND rails are required to initialize the array.");
            InvalidateTrust();
            channelMask = ValidChannels();
            int count = 0;
            for (int i = 0; i < 8; i++) if ((channelMask & (1 << i)) != 0) count++;
            ReadyChannels = count;
            queueReady = false;
            benchState = count == 8 ? "NAND READY" : "NAND DEGRADED";
            message = $"Read native CE witnesses for {count}/8 channels. Probe the reserve.";
            return count > 0;
        }

        private int ValidChannels()
        {
            int mask = 0;
            for (int i = 0; i < channelFeeds.Length; i++)
                if (Voltage(channelFeeds[i]) > 4.6f) mask |= 1 << i;
            return mask;
        }

        public bool ProbeReserve()
        {
            if (!RailsReady || Testing) return Reject("Power the storage rails before measuring PLP.");
            HoldSeconds = SparkLevel116Reserve.HoldSeconds(Voltage(reserveSense), FaultLive(1) ? HighEsr : NominalEsr, LoadWatts);
            probed = true;
            evidence = $"Reserve {Voltage(reserveSense):F2} V · load {LoadWatts:F1} W\nMeasured hold-up {HoldSeconds * 1000:F0} ms · {ReadyChannels}/8 channels";
            message = "Reserve characterized. Initialize the bounded NVMe training queue.";
            return true;
        }

        public bool OpenQueue()
        {
            if (!RailsReady || Testing || !probed || channelMask == 0 || Voltage(linkSense) < 4.6f)
                return Reject("Measure reserve and initialize NAND with a solved link witness first.");
            if (stage == 2 && (channelMask != 255 || HoldSeconds < FullWriteSeconds))
                return Reject("Commissioning requires all eight channels and enough measured hold-up for a durable flush.");
            queueReady = true;
            benchState = "QUEUE READY · DEPTH 1";
            message = "Run the finite data, ECC and supervised power-cut sequence.";
            if (stage == 2) Award();
            return true;
        }

        public bool RunStorage()
        {
            if (stage is < 3 or > 5 || !RailsReady || !queueReady || Testing || protectedBusy)
                return Reject("Bring up the training queue with stable rails before testing.");
            if (FaultLive(2) && !flash.Replay())
            {
                runEpoch = epoch;
                FinishFailure(StorageFailure.Replay, "The rejected journal still omits an acknowledged page. Remap before writing.");
                return true;
            }
            InvalidateTrust();
            runEpoch = epoch;
            corrected = bitsRead = timeouts = cutAcks = address = 0;
            earlyReplay = rejectingDouble = false;
            CutPrimaryVolts = 0f;
            scope?.ResetTrace();
            SampleScope();
            Failure = StorageFailure.None;
            seed = 0x8dc5a71du ^ unchecked((uint)epoch * 0x9e3779b9u);
            test = TestPhase.Traffic;
            nextStep = time + .04f;
            benchState = "WRITING AND CHECKING PAGES";
            message = "Writing 32 address-seeded pages and verifying each through the ECC decoder and CRC.";
            return true;

        }

        private void TickController(float dt)
        {
            time += dt;
            if (!powered) return;
            if (Testing && time >= nextStep) StepTest();
            if (protectedBusy && time >= protectedAt)
            {
                if (!Fresh || Inhibited || !flash.Advance()) AbortProtectedWrite();
                else if (flash.Current == null)
                {
                    protectedBusy = false;
                    message = "The host received a durable completion.";
                    QueueCheckpoint();
                }
                else protectedAt = time + .18f;
            }
            if (qualifiedEpoch == epoch && (!RailsReady || Voltage(linkSense) < 4.6f || ValidChannels() != 255))
                InvalidateTrust();
            TickSupervisor();
        }

        private void StepTest()
        {
            nextStep = time + .032f;
            switch (test)
            {
                case TestPhase.Traffic:
                    RunPage();
                    break;
                case TestPhase.EarlyCut:
                    SetPrimary(true);
                    SampleScope();
                    earlyReplay = flash.Replay() && flash.Read(3, 0, out uint original) == SparkLevel116Flash.ReadResult.Clean &&
                        original == earlyExpected && cutAcks == 0;
                    if (!earlyReplay) { FinishFailure(StorageFailure.Replay, "The early cut failed to recover the previous acknowledged page."); return; }
                    StartReserveCut();
                    break;
                case TestPhase.ReserveCut:
                    int acknowledgements = flash.PowerLoss(HoldSeconds, FaultLive(2));
                    SetPrimary(false);
                    SampleScope();
                    SetPrimary(true);
                    SampleScope();
                    cutAcks = acknowledgements;
                    if (!flash.Replay())
                    { FinishFailure(StorageFailure.Replay, "The newest journal failed ECC. The older checkpoint is missing an acknowledged write."); return; }
                    if (cutAcks != 1 || flash.Read(5, 0, out uint recovered) != SparkLevel116Flash.ReadResult.Clean ||
                        recovered != reserveExpected)
                    { FinishFailure(StorageFailure.HoldUp, "The reserve ran out before completion. Replay preserved the previous durable data."); return; }
                    FinishQualification();
                    break;
            }
        }

        private void RunPage()
        {
            int channel = address / 4;
            if ((channelMask & (1 << channel)) == 0 || Voltage(channelFeeds[channel]) < 4.6f) timeouts++;
            else
            {
                uint value = SampleData(address);
                int bit = (address + 9) % 39;
                bool singleError = address % 4 == 0;
                long raw = singleError ? 1L << bit : 0L;
                if (!flash.Write(value, address) ||
                    flash.Read(address, raw, out uint actual) != (singleError ?
                        SparkLevel116Flash.ReadResult.Corrected : SparkLevel116Flash.ReadResult.Clean) || actual != value)
                { FinishFailure(StorageFailure.Ecc, "The page failed its ECC and address-bound CRC check."); return; }
                bitsRead += 32;
                if (singleError) corrected++;
            }
            address++;
            if (address < SparkLevel116Flash.PageCount) return;
            if (timeouts != 0)
            { FinishFailure(StorageFailure.ChannelTimeout, $"Channel 3 CE loss caused {timeouts} finite command timeouts."); return; }
            rejectingDouble = flash.Read(0, 3, out _) == SparkLevel116Flash.ReadResult.Uncorrectable;
            if (!rejectingDouble) { FinishFailure(StorageFailure.Ecc, "The decoder failed to reject a known double-bit error."); return; }

            earlyExpected = SampleData(3);
            if (!flash.Begin(3, earlyExpected ^ 0x76af8b1du) || !flash.Advance() || !flash.Advance())
            { FinishFailure(StorageFailure.Replay, "The interrupted candidate could not be staged."); return; }
            SetPrimary(false);
            RecordCut();
            cutAcks = flash.PowerLoss(0f);
            test = TestPhase.EarlyCut;
            nextStep = time + .25f;
            benchState = "REPLAYING PRE-COMMIT CUT";
            message = "Primary rail opened before publication. Recovering the previous acknowledged checkpoint.";
        }

        private uint SampleData(int page)
        {
            uint value = seed ^ unchecked((uint)page * 0x85ebca6bu);
            value ^= value << 13;
            value ^= value >> 17;
            return value ^ value << 5;
        }

        private void StartReserveCut()
        {
            reserveExpected = SampleData(5) ^ 0x1276bc19u;
            if (!flash.Begin(5, reserveExpected))
            { FinishFailure(StorageFailure.Replay, "The reserve-write intent could not be queued."); return; }
            HoldSeconds = SparkLevel116Reserve.HoldSeconds(Voltage(reserveSense), FaultLive(1) ? HighEsr : NominalEsr, LoadWatts);
            SetContact(bench, false);
            SetContact(reserveCharge, false);
            Solve();
            RecordCut();
            test = TestPhase.ReserveCut;
            nextStep = time + Mathf.Max(.25f, HoldSeconds);
            benchState = "PLP COMMIT AND COLD REPLAY";
            message = "Primary feed is open. The loaded reserve is racing the journal commit timer.";
        }

        private void RecordCut()
        {
            CutPrimaryVolts = Voltage(standby);
            SampleScope();
        }

        private void SampleScope() => scope?.Record(Voltage(standby), Voltage(controllerRail), Voltage(nandRail), Voltage(aux));

        private void FinishFailure(StorageFailure failure, string details)
        {
            test = TestPhase.Idle;
            Failure = failure;
            benchState = failure.ToString().ToUpperInvariant() + " · INHIBITED";
            message = details;
            evidence = $"{details}\nHold-up {HoldSeconds * 1000:F0} ms · recovered {flash.RecoveredPages} pages.";
            QueueCheckpoint();
        }

        private void FinishQualification()
        {
            test = TestPhase.Idle;
            if (!RailsReady || ValidChannels() != 255 || Voltage(linkSense) < 4.6f || runEpoch != epoch || CutPrimaryVolts > .15f)
            { FinishFailure(StorageFailure.Replay, "The final rail and channel evidence did not match the current run."); return; }
            qualifiedEpoch = epoch;
            benchState = "DURABLE STORAGE VERIFIED";
            evidence = $"{bitsRead} verified bits · {corrected} corrected pages · double-bit rejection\n" +
                $"{flash.RecoveredPages} replayed pages · {HoldSeconds * 1000:F0} ms PLP · journal {flash.DurableGeneration}";
            message = "The acknowledged write survived the cut. The earlier uncommitted candidate was discarded.";
            if (stage == 3)
            {
                if (Award()) message = "Durability qualified. Begin the first controlled storage fault.";
            }
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = repairTarget = -1;
                faultWitnessed = reworked = false;
                if (repaired == 3)
                {
                    if (Award()) message = "All repairs qualified. Wait for U30, then start the supervised W289 cut.";
                }
                else message = "Fresh read, ECC and replay tests verified the repair. Start the next fault.";
            }
            QueueCheckpoint();
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || !Qualifies || activeFault >= 0 || repaired >= 3)
                return Reject("Complete a healthy storage run before starting the next fault trial.");
            activeFault = repaired;
            repairTarget = -1;
            faultWitnessed = reworked = false;
            spare = 0;
            Failure = StorageFailure.None;
            InvalidateDiagnostics();
            SetPrimary(true);
            message = "A storage fault was introduced. Re-initialize the bench and collect a fresh failure trace.";
            QueueCheckpoint();
            return true;
        }

        public bool LocateFault()
        {
            bool observed = powered && runEpoch == epoch && (activeFault switch
            {
                0 => Failure == StorageFailure.ChannelTimeout && Voltage(channelFeeds[3]) < .15f,
                1 => Failure == StorageFailure.HoldUp && HoldSeconds < FullWriteSeconds && cutAcks == 0,
                2 => Failure == StorageFailure.Replay && !flash.Replay(),
                _ => false
            });
            if (!observed || reworked) return Reject("Measure a failed run on the current hardware before repair.");
            faultWitnessed = true;
            footprint = repairTarget = ExpectedPart;
            FocusPart();
            message = activeFault switch
            {
                0 => $"NAND4 CE opened. Isolate and inspect {ChannelVia.viaId}.",
                1 => $"PLP3 sustained only {HoldSeconds * 1000:F0} ms. Isolate and select the matched training spare.",
                _ => "NAND8's journal header is uncorrectable. Isolate and remap it from verified pages."
            };
            QueueCheckpoint();
            return true;
        }

        public bool InspectChannelVia()
        {
            if (stage != 4 || powered || !faultWitnessed || activeFault != 0 || repairTarget != channelPart)
                return Reject("Locate NAND4's continuity fault and isolate the rails first.");
            SelectRoute(channelViaIndex);
            PadView();
            viaInspectionEpoch = epoch;
            message = $"{ChannelVia.viaId} spans L{ChannelVia.layer:00} to L{ChannelVia.exitLayer:00}. Repair from its entry layer.";
            return true;
        }

        public bool SelectSpare()
        {
            if (powered || activeFault != 1 || repairTarget != capacitorPart)
                return Reject("Select PLP3 with power isolated before choosing a spare.");
            spare = (spare + 1) % 3;
            message = spare switch
            {
                0 => "Spare A: lower voltage class.",
                1 => "Spare B: matched reserve rating and low ESR.",
                _ => "Spare C: insufficient capacitance."
            };
            return true;
        }

        public bool Repair()
        {
            if (stage != 4 || powered || !faultWitnessed || reworked || repairTarget != ExpectedPart)
                return Reject("Isolate the bench and select the measured repair target.");
            if (activeFault == 0 && (viaInspectionEpoch != epoch || layer != ChannelVia.layer || !ChannelVia.barrel.IsClosed))
                return Reject("Inspect the diagnosed via and select its entry layer.");
            if (activeFault == 1 && spare != 1)
                return Reject("PLP3 needs the correctly rated, low-ESR training spare.");
            if (activeFault == 2 && !flash.RemapMetadata())
                return Reject("The remap could not recover every acknowledged page from physical storage.");
            reworked = true;
            message = activeFault switch
            {
                0 => "NAND4 microvia reworked. Restore power and verify channel 3.",
                1 => "PLP3 replaced with the matched spare. Repeat the measured power drop.",
                _ => "Damaged metadata retired. Verified physical pages seeded the replacement journal."
            };
            QueueCheckpoint();
            return true;
        }

        public bool ProtectedWrite()
        {
            if (!Fresh || Inhibited || Testing || protectedBusy || !flash.Begin(31, SampleData(31) ^ 0xcce561acu))
                return Reject("U30 must authorize the current epoch before accepting protected writes.");
            protectedBusy = true;
            protectedAt = time + .18f;
            message = "Protected host write queued. Completion follows durable publication.";
            return true;
        }

        private void AbortProtectedWrite()
        {
            if (!protectedBusy) return;
            protectedBusy = false;
            flash.PowerLoss(0f);
            AckSafeAtInterruption = flash.Replay();
        }

        public bool OpenW289()
        {
            if (stage != 5 || !armed || !Fresh || interrupted)
                return Reject("Wait for U30 to authorize the live protected workload.");
            if (!protectedBusy && !ProtectedWrite()) return false;
            minimumEpoch = epoch + 1;
            if (!RemoveWire(jumper)) return Reject("The native circuit could not open W289.");
            Solve();
            return true;
        }

        public bool RestoreW289()
        {
            if (stage != 5 || !interrupted || linked) return Reject("Record the supervised W289 opening first.");
            SelectRoute(routes.Length);
            if (!CreateWire(jumper)) return Reject("The native circuit rejected W289.");
            ReadGraph();
            Solve();
            message = "W289 restored. Re-initialize the bench and repeat ECC, reserve and replay testing.";
            return true;
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || epoch < minimumEpoch || !Fresh || !Inhibited ||
                Voltage(monitorSupply) < 3f)
                return Reject("Reconnect W289 and obtain a newer ECC, PLP and replay qualification before U30 reset.");
            safetyReset = true;
            ClearHeartbeat();
            Solve();
            message = "U30 reset accepted. Waiting for a new verified heartbeat.";
            return true;
        }

        private void InvalidateDiagnostics()
        {
            epoch++;
            AbortProtectedWrite();
            test = TestPhase.Idle;
            channelMask = ReadyChannels = 0;
            probed = queueReady = false;
            InvalidateTrust();
        }

        private void InvalidateTrust()
        {
            qualifiedEpoch = -1;
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
                benchState = "PROTECTED WRITES AUTHORIZED";
                message = "U30 restored protected writes after W289 recovery and fresh ECC, PLP and replay evidence.";
            }
        }
    }
}
