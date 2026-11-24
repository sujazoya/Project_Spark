using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.RedundancyCitadel
{
    public sealed partial class SparkLevel113Process
    {
        private const float MinimumBus = 10.5f;
        private const float HoldCapacitance = .047f;
        private const float TransferInterval = .080f;
        public SparkPowerSupply reserve;
        public SparkTerminal reserveInput, bus, rail5, rail33, monitorSupply, protectedFeed;
        public SparkTerminal[] continuityReturns;
        public SparkSwitch primaryPath, reservePath, enable5, enable33, heartbeat, workload;
        public SparkDiode primaryIsolation, reserveIsolation;
        public SparkResistor busLoad;
        public int[] serviceParts;
        public int eccRoute, eccDomain;

        private int activeFault = -1, repaired, repairTarget = -1;
        private bool faultWitnessed, reworked, armed, interrupted, latch, inhibitionObserved, safetyReset;
        private bool railsProven, transferProven, singleProven, doubleProven, timingProven;
        private int eccCase;
        private byte writtenWord, receivedWord;
        private float time, transferTime, holdStart, holdCurrent, eccBeforeFault;
        private bool transferring;
        private string evidence = "Install the board with both input paths isolated.";

        public int Repairs => repaired;
        public int ActiveFault => activeFault;
        public bool Transferring => transferring;
        public float TransferMilliseconds { get; private set; }
        public float MinimumTransferVoltage { get; private set; }
        public bool Latched => latch;
        private bool Qualified => railsProven && transferProven && singleProven && doubleProven && timingProven;
        private Route EccVia => routes[eccRoute];
        private bool ExpectedEccGap => stage == 4 && activeFault == 1 && !reworked &&
            installed == PartCount && !unauthorized && routed == RouteCount - 1 && plated == ViaCount - 1 &&
            !EccVia.barrel.IsClosed && HasWire(EccVia);
        private bool RailsLive => Voltage(rail5) is > 4.85f and < 5.15f &&
            Voltage(rail33) is > 3.18f and < 3.42f && Voltage(monitorSupply) > 3.15f;

        private void Solve() => manager.ElectricalSolver.SolveNow();

        public bool TogglePower()
        {
            if (stage == 6) return Reject("The completed board is in inspection mode.");
            if (powered) { IsolatePower(); return true; }
            ReadGraph();
            if (stage is < 2 or > 5 || (!Construction && !ExpectedEccGap) || !linked && !interrupted)
                return Reject("Build and verify the complete circuit before enabling both inputs.");
            powered = true;
            SetContact(primaryPath, activeFault != 0 || reworked);
            SetContact(reservePath, true);
            Solve();
            SyncProtection();
            message = "Inputs enabled. Commission the 12 V feeds, isolation and regulated rails.";
            return true;
        }

        private void IsolatePower()
        {
            powered = transferring = false;
            SetContact(primaryPath, false);
            SetContact(reservePath, false);
            SyncProtection();
            InvalidateDiagnostics();
            message = "Both input paths isolated. The selected fault can be serviced.";
        }

        private void InvalidateDiagnostics()
        {
            railsProven = transferProven = singleProven = doubleProven = timingProven = false;
            eccCase = 0;
            armed = safetyReset = false;
        }

        private void SyncProtection()
        {
            Solve();
            float busVoltage = Voltage(bus);
            bool regulated = powered && (transferring ? MinimumTransferVoltage >= MinimumBus : busVoltage >= MinimumBus);
            SetContact(enable5, regulated);
            SetContact(enable33, regulated);
            SetContact(heartbeat, regulated && timingProven && !(activeFault == 2 && !reworked));
            Solve();
            SetContact(workload, regulated && !latch && timingProven && linked && Voltage(jumper.to) > 3.0f);
            Solve();
        }

        public bool CommissionRails()
        {
            if (!powered || transferring || (!Construction && !ExpectedEccGap))
                return Reject("Enable bench power and wait for the transfer to finish.");
            bool inputs = Voltage(input) is > 11.7f and < 12.4f && Voltage(reserveInput) is > 11.7f and < 12.4f;
            bool normal = activeFault != 0 || reworked;
            bool isolated = normal ? primaryIsolation.IsForwardConducting && !reserveIsolation.IsForwardConducting :
                reserveIsolation.IsForwardConducting && !primaryIsolation.IsForwardConducting;
            railsProven = inputs && isolated && Voltage(bus) > 11.4f && RailsLive;
            evidence = $"P {Voltage(input):F2} V · R {Voltage(reserveInput):F2} V · bus {Voltage(bus):F2} V\n" +
                $"5 V {Voltage(rail5):F2} · 3.3 V {Voltage(rail33):F2} · isolation {(isolated ? "verified" : "failed")}";
            if (!railsProven) return Reject("The measured rails or input isolation did not qualify.");
            if (stage == 2) Award();
            message = "Power rails qualified. Measure transfer, verify both ECC cases, then audit timing.";
            return true;
        }

        public bool StartTransfer()
        {
            if (stage is < 3 or > 5 || !powered || transferring || !railsProven || !primaryIsolation.IsForwardConducting)
                return Reject("Commission both inputs and restore the primary path before attempting transfer.");
            transferring = true;
            transferProven = false;
            transferTime = 0f;
            holdStart = Voltage(bus);
            holdCurrent = Mathf.Abs(busLoad.ElectricalState.Current);
            MinimumTransferVoltage = holdStart;
            TransferMilliseconds = 0f;
            scope?.ResetTrace();
            RecordTransfer(holdStart);
            SetContact(reservePath, false);
            SetContact(primaryPath, false);
            SyncProtection();
            message = "Primary dropped. The modeled capacitor holds the bus until reserve takeover.";
            return true;
        }

        private void TickController(float delta)
        {
            time += delta;
            if (!transferring) return;
            // Expand the short simulated event for inspection. Only the contact
            // transfer and rail samples come from the native electrical solver.
            transferTime = Mathf.Min(TransferInterval, transferTime + delta * .075f);
            float held = holdStart - holdCurrent * transferTime / HoldCapacitance;
            MinimumTransferVoltage = Mathf.Min(MinimumTransferVoltage, held);
            RecordTransfer(held);
            if (transferTime < TransferInterval) return;
            SetContact(reservePath, true);
            transferring = false;
            Solve();
            float takeoverVoltage = Voltage(bus);
            MinimumTransferVoltage = Mathf.Min(MinimumTransferVoltage, takeoverVoltage);
            TransferMilliseconds = transferTime * 1000f;
            transferProven = reserveIsolation.IsForwardConducting && !primaryIsolation.IsForwardConducting &&
                MinimumTransferVoltage >= MinimumBus && holdCurrent > .25f;
            RecordTransfer(takeoverVoltage);
            SetContact(primaryPath, activeFault != 0 || reworked);
            SyncProtection();
            transferProven &= RailsLive;
            evidence = $"Reserve transfer {TransferMilliseconds:F0} ms · {holdCurrent:F2} A load\n" +
                $"Hold-up minimum {MinimumTransferVoltage:F2} V · reserve bus {takeoverVoltage:F2} V";
            message = transferProven ? "Reserve carried the load within the hold-up budget." : "The loaded failover did not meet the hold-up threshold.";
        }

        public bool InjectEcc(int bits)
        {
            if (stage is < 3 or > 5 || !powered || !railsProven || transferring || bits is < 1 or > 2)
                return Reject("Commission the rails before writing a test word.");
            writtenWord = SparkLevel113Ecc.Encode(0xB);
            bool eccLink = Voltage(witnesses[eccDomain], continuityReturns[eccDomain]) > 4.7f;
            // An open data span introduces a deterministic additional bit error.
            receivedWord = (byte)(writtenWord ^ 0x04 ^ (bits == 2 ? 0x20 : 0) ^ (eccLink ? 0 : 0x40));
            eccCase = bits;
            var observed = SparkLevel113Ecc.Inspect(receivedWord);
            evidence = $"TX {Convert.ToString(writtenWord, 2).PadLeft(8, '0')} · RX {Convert.ToString(receivedWord, 2).PadLeft(8, '0')}\n" +
                $"Syndrome {observed.position} · overall parity {(observed.odd ? "ODD" : "EVEN")}";
            message = "Interpret the syndrome and parity before choosing correction or quarantine.";
            return true;
        }

        public bool ResolveEcc(bool quarantine)
        {
            if (eccCase == 0 || !powered || !railsProven) return Reject("Inject a test error and read the syndrome first.");
            var observed = SparkLevel113Ecc.Inspect(receivedWord);
            int pending = eccCase;
            bool eccLink = Voltage(witnesses[eccDomain], continuityReturns[eccDomain]) > 4.7f;
            if (quarantine)
            {
                if (observed.odd || observed.position == 0) return Reject("This parity and syndrome do not indicate a double-bit error.");
                eccCase = 0;
                if (eccLink && pending == 2) doubleProven = true;
                message = "Uncorrectable word quarantined. No corrupted payload was released.";
                return true;
            }
            if (!observed.odd) return Reject("Correction requires odd overall parity. Quarantine a double-bit error.");
            int bit = observed.position == 0 ? 8 : observed.position;
            byte corrected = (byte)(receivedWord ^ (1 << (bit - 1)));
            eccCase = 0;
            if (corrected != writtenWord)
                return Reject("Readback did not match the written word. Test the ECC via before retrying.");
            if (eccLink && pending == 1) singleProven = true;
            message = "Single-bit correction verified by scrub readback.";
            return true;
        }

        public bool AuditTiming()
        {
            if (stage is < 3 or > 5 || !powered || transferring || !railsProven)
                return Reject("Commission the rails before running the independent clock and watchdog audit.");
            bool drift = activeFault == 2 && !reworked;
            var timing = SparkLevel113Ecc.TestTiming(drift ? .018f : .0102f);
            timingProven = timing.skew <= 2f && timing.healthy && timing.timeout > 0f && timing.timeout <= 35f;
            SyncProtection();
            evidence = $"Clock A 10.0 ms · clock B {(drift ? 18f : 10.2f):F1} ms · max skew {timing.skew:F1} ms\n" +
                $"Virtual watchdog {(timing.healthy ? "healthy" : "tripped")} · gap trip {timing.timeout:F0} ms";
            message = timingProven ? "Independent clocks agreed and the withheld heartbeat tripped the watchdog." :
                "Clock disagreement withheld authorization. Inspect the timing reference.";
            return timingProven;
        }

        public bool AcceptQualification()
        {
            if (!powered || transferring || !Construction || !linked || !Qualified)
                return Reject("Acquire current rail, transfer, both ECC cases and timing evidence first.");
            if (stage == 3)
            {
                if (Inhibited) return Reject("The protected relay did not pick up with W233 connected.");
                if (!Award()) return false;
                message = "Baseline qualified. Start the controlled fault trials.";
                return true;
            }
            if (stage == 4 && reworked)
            {
                repaired++;
                activeFault = -1;
                reworked = faultWitnessed = false;
                if (repaired == 3)
                {
                    if (!Award()) return false;
                    message = "Three repairs verified. Arm the supervised W233 removal.";
                }
                else message = "Repair verified under load. Start the next fault trial.";
                QueueCheckpoint();
                return true;
            }
            if (stage == 5 && safetyReset && interrupted && !Inhibited && !latch)
            {
                if (!Award()) return false;
                message = "Qualification complete. The protected load resumed after explicit U6 reset.";
                return true;
            }
            return Reject("Complete the active fault or safety step before signing the qualification.");
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || repaired >= 3 || activeFault >= 0 || !powered || transferring)
                return Reject("Finish the active fault and enable the bench before starting a trial.");
            activeFault = repaired;
            repairTarget = -1;
            InvalidateDiagnostics();
            if (activeFault == 0) SetContact(primaryPath, false);
            if (activeFault == 1)
            {
                eccBeforeFault = Voltage(witnesses[eccDomain], continuityReturns[eccDomain]);
                SetContact(EccVia.barrel, false);
            }
            ReadGraph();
            SyncProtection();
            message = "The supervised fault is active. Use the diagnostic probe to capture its signature.";
            QueueCheckpoint();
            return true;
        }

        public bool LocateFault()
        {
            if (stage != 4 || activeFault < 0 || reworked || !powered)
                return Reject("Activate a fault with the bench powered before probing it.");
            switch (activeFault)
            {
                case 0:
                    faultWitnessed = !primaryIsolation.IsForwardConducting && reserveIsolation.IsForwardConducting && RailsLive;
                    evidence = $"Primary {primaryIsolation.ElectricalState.Current:F3} A · reserve {reserveIsolation.ElectricalState.Current:F3} A\nQ1 primary switch path open.";
                    break;
                case 1:
                    float measured = Voltage(witnesses[eccDomain], continuityReturns[eccDomain]);
                    faultWitnessed = eccBeforeFault > 4.7f && measured < .5f && !EccVia.barrel.IsConducting && RailsLive;
                    evidence = $"ECC endpoint {eccBeforeFault:F2} → {measured:F2} V under disturbance\n{EccVia.viaId} open on L{EccVia.layer:00}–L{EccVia.exitLayer:00}.";
                    break;
                case 2:
                    bool ready = CommissionRails();
                    faultWitnessed = ready && !AuditTiming() && Inhibited;
                    evidence += "\nProbe the Y1 clock reference.";
                    break;
            }
            message = faultWitnessed ? "Fault signature captured. Isolate power, select the affected station or via, and rework it." :
                "The expected fault signature has not been measured.";
            return faultWitnessed;
        }

        public bool SelectFaultSite()
        {
            if (!faultWitnessed || activeFault < 0 || reworked) return Reject("Capture a fault signature before opening its work area.");
            if (activeFault == 1) { SelectRoute(eccRoute); PadView(); }
            else
            {
                SelectFootprint(ExpectedRepairPart());
                FocusPart();
            }
            return true;
        }

        private int ExpectedRepairPart() => Array.FindIndex(parts, item => item.label == (activeFault == 0 ? "Q1" : "Y1"));

        public bool Repair()
        {
            if (stage != 4 || powered || activeFault < 0 || !faultWitnessed || reworked)
                return Reject("Locate the active fault and isolate both inputs before rework.");
            if (activeFault == 1)
            {
                if (selectedRoute != eccRoute || layer != EccVia.layer || !HasWire(EccVia))
                    return Reject($"Select {EccVia.id} on L{EccVia.layer:00} to rebuild its plated span.");
                SetContact(EccVia.barrel, true);
            }
            else if (repairTarget != ExpectedRepairPart()) return Reject("Select the diagnosed component footprint.");
            reworked = true;
            BoardView();
            ReadGraph();
            InvalidateDiagnostics();
            message = "Rework applied. Enable power and repeat the complete qualification before signing the repair.";
            return true;
        }

        public bool ArmSafety()
        {
            if (stage != 5 || !powered || transferring || !Qualified || !linked || Inhibited || interrupted)
                return Reject("Arm only with current baseline evidence and the protected load enabled.");
            armed = true;
            message = "Supervised removal armed. Open W233 to challenge the independent monitor.";
            return true;
        }

        public bool OpenW233()
        {
            if (!armed || stage != 5 || !powered || !linked || transferring)
                return Reject("Arm the supervised test with W233 linked and the protected load powered.");
            return RemoveWire(jumper);
        }

        public bool ObserveInhibit()
        {
            if (stage != 5 || !powered || !interrupted || linked || !latch || !Inhibited ||
                Voltage(jumper.to) > .25f || Voltage(monitorSupply) < 3.15f)
                return Reject("The load must be inhibited while the independent monitor remains powered.");
            inhibitionObserved = true;
            evidence = $"U6 monitor {Voltage(monitorSupply):F2} V · permit {Voltage(jumper.to):F2} V\n" +
                $"Protected load {Voltage(protectedFeed):F2} V · latch confirmed";
            message = "Safety containment proved. Restore W233, then acquire fresh rail, transfer, ECC and timing evidence.";
            QueueCheckpoint();
            return true;
        }

        public bool RestoreW233()
        {
            if (stage != 5 || linked || interrupted && !inhibitionObserved)
                return Reject("Capture the open-jumper containment before restoring W233.");
            SelectRoute(routes.Length);
            return ConnectPads();
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !powered || transferring || !interrupted || !inhibitionObserved ||
                !linked || !latch || !Qualified || Voltage(jumper.to) < 3.0f || !RailsLive)
                return Reject("Restore W233 and repeat all rail, transfer, ECC and timing checks before resetting U6.");
            latch = false;
            safetyReset = true;
            SyncProtection();
            if (Inhibited) return Reject("The protected relay did not re-enable after reset.");
            message = "U6 reset acknowledged and protected load restored. Sign the final qualification.";
            return true;
        }
    }
}

