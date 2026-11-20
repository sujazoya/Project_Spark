using System;
using UnityEngine;
using ProjectSpark.Circuit;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel107Process
    {
        private static readonly string[] PostSteps = { "Standby", "SPI image", "Controller", "DDR training", "Storage", "Ready" };
        private readonly SparkPfx15Flash flash = new();
        private float time, postClock, nextPulse, lastPulse = float.NegativeInfinity;
        private int postStep, rises, fault = -1, repaired;
        private bool postRunning, recoveryVerified, writingUnlocked, primaryVerified, initialRestored;
        private bool testWrite, cutObserved, chipSelectFault, pullupFault, causeObserved, reworked, requestProved;
        private bool safetyInterrupted, safetyReset, high, armed;
        private string primaryText = "Unchecked", recoveryText = "Unchecked", eventText = "No service measurements recorded.";
        public bool Busy => flash.Busy;
        public int PagesWritten => flash.PagesWritten;
        public bool PrimaryValid => flash.PrimaryValid;
        public bool RecoveryValid => flash.RecoveryValid;
        public bool InterruptionObserved => cutObserved;
        public bool BootReady => postStep == PostSteps.Length;
        public bool Inhibited => !workload.IsConducting;
        public bool Fresh => jumperPresent && rises >= 2 && time - lastPulse < .46f;
        public int Repairs => repaired;
        public string ServiceReading => eventText;
        private bool PowerAvailable => powered && Construction && Voltage(standby, ground) > 11f;
        private bool PrimaryBusReady => Voltage(primaryBus, ground) > 2.6f;
        private bool RecoveryBusReady => Voltage(recoveryBus, ground) > 2.6f;

        private static void SetContact(SparkSwitch contact, bool closed)
        {
            if (contact.IsClosed != closed) contact.SetState(closed ? SparkSwitchState.Closed : SparkSwitchState.Open);
        }

        private void OnCircuitChanged(bool wasConnected)
        {
            if (stage == 5 && armed && wasConnected && !jumperPresent && powered)
            {
                safetyInterrupted = true;
                safetyReset = false;
                message = "W149 opened. Standby stays on while the supervisor isolates the protected load.";
            }
            if (!jumperPresent || !Construction)
            {
                flash.Interrupt();
                testWrite = false;
            }
            InvalidateTrust();
        }

        private void InvalidateTrust()
        {
            postStep = 0;
            postRunning = primaryVerified = recoveryVerified = false;
            rises = 0;
            high = false;
            lastPulse = float.NegativeInfinity;
            SetContact(heartbeat, false);
            SetContact(workload, false);
        }

        private void UpdateContacts()
        {
            SetContact(benchPower, powered);
            SetContact(primaryContact, powered && !chipSelectFault);
            SetContact(recoveryContact, powered);
            SetContact(programGate, powered && writingUnlocked);
            SetContact(pullupContact, powered && !pullupFault);
        }

        public bool TogglePower()
        {
            if (!powered && (stage < 2 || !Construction))
                return Reject("Finish the assembly and copper network before powering the bench.");
            if (stage >= 6) return Reject("Qualification is complete.");
            powered = !powered;
            if (!powered && flash.Busy)
            {
                bool damaged = flash.Interrupt();
                if (testWrite && damaged) cutObserved = true;
                testWrite = false;
                primaryText = damaged ? "Partial image" : "Unchecked";
                eventText = damaged ? "Power loss · U6 readback invalid. The protected U7 bank is intact." : "Power loss before U6 changed.";
            }
            writingUnlocked = false;
            InvalidateTrust();
            UpdateContacts();
            manager.ElectricalSolver.SolveNow();
            QueueCheckpoint();
            if (!powered && RepairRouting)
            {
                for (int i = 0; i < routes.Length; i++)
                    if (!HasWire(routes[i]) || (routes[i].via && !routes[i].via.IsClosed)) { SelectRoute(i); break; }
                message = "Inspect and restore the open route with bench power isolated.";
            }
            else message = powered ? "Bench power on. Verify the requested evidence." : "Bench isolated.";
            return true;
        }

        public bool MeasurePrimary()
        {
            if (!PowerAvailable || flash.Busy) return Reject("Power the bench and wait for the flash bus to become idle.");
            if (!PrimaryBusReady)
            {
                primaryText = "No CS response";
                if (fault == 0 && chipSelectFault) causeObserved = true;
                eventText = $"U6 CS witness {Voltage(primaryBus, ground):F2} V · no SPI response.";
                message = "Inspect the U6 chip-select contact and isolate power before rework.";
                return true;
            }
            primaryVerified = flash.PrimaryValid;
            primaryText = primaryVerified ? "Digest verified" : "Digest mismatch";
            if (fault == 1 && !primaryVerified) causeObserved = true;
            eventText = primaryVerified ? $"U6 SPI response · approved training digest {flash.Digest}."
                : "U6 SPI transport responds. The image fails the trusted digest check.";
            message = primaryVerified ? "U6 readback is valid." : "Authenticate protected U7 before rewriting U6.";
            return true;
        }

        public bool AuthenticateRecovery()
        {
            if (!PowerAvailable || flash.Busy || !RecoveryBusReady) return Reject("Power the board and verify that the recovery bus answers.");
            recoveryVerified = flash.RecoveryValid;
            recoveryText = recoveryVerified ? "Verified / protected" : "Digest mismatch";
            eventText = recoveryVerified ? $"U7 training manifest verified · PFX-15 v4 · {flash.Digest}." : "U7 trust check failed.";
            message = recoveryVerified ? "U7 is readable. The programmer only targets primary U6." : "Writes are inhibited.";
            return recoveryVerified;
        }

        public bool ToggleWriteProtect()
        {
            if (!PowerAvailable || flash.Busy) return Reject("Power the idle bench before changing primary write protection.");
            writingUnlocked = !writingUnlocked;
            UpdateContacts();
            manager.ElectricalSolver.SolveNow();
            message = writingUnlocked ? "U6 programming enabled. U7 remains protected." : "U6 write protection engaged.";
            return true;
        }

        private bool BeginWrite(bool trial)
        {
            if (!PowerAvailable || !PrimaryBusReady || !RecoveryBusReady || !recoveryVerified || Voltage(writeSense, ground) < 2.6f)
                return Reject("Authenticate U7, confirm both SPI buses, and unlock primary U6.");
            if (fault == 1 && !causeObserved) return Reject("Measure U6 and distinguish its invalid image from a failed SPI contact.");
            if (!flash.Begin()) return Reject("The flash programmer is already busy.");
            testWrite = trial;
            primaryVerified = false;
            postRunning = false;
            postStep = 0;
            primaryText = "Erase in progress";
            message = trial ? "Cut bench power during erase or programming, then recover the partial image." : "Writing verified U7 pages into primary U6.";
            return true;
        }

        public bool RewritePrimary()
        {
            if (stage is < 2 or > 5 || flash.PrimaryValid) return Reject("A rewrite requires an invalid primary image.");
            return BeginWrite(false);
        }

        public bool BeginCutoverTrial()
        {
            if (stage != 3 || cutObserved || !BootReady) return Reject("Begin the interruption trial after the first verified boot.");
            return BeginWrite(true);
        }

        public bool StartBoot()
        {
            if (!PowerAvailable || !PrimaryBusReady || !primaryVerified || !flash.PrimaryValid || flash.Busy || writingUnlocked)
                return Reject("Verify U6 readback and engage write protection before starting POST.");
            if (postRunning) return Reject("POST is already running.");
            if (fault == 2 && reworked && !requestProved)
                return Reject("Capture a fresh recovery input transition before requalification.");
            postStep = 0;
            postClock = 0;
            postRunning = true;
            message = "Running POST and storage checks.";
            return true;
        }

        public bool SeedFault()
        {
            if (stage != 4 || repaired >= 3 || fault >= 0 || !BootReady || !PowerAvailable)
                return Reject("Start the next fault exercise from a healthy POST.");
            fault = repaired;
            causeObserved = reworked = requestProved = false;
            InvalidateTrust();
            if (fault == 0) chipSelectFault = true;
            else if (fault == 1) flash.Corrupt();
            else pullupFault = true;
            primaryText = "Unchecked";
            UpdateContacts();
            manager.ElectricalSolver.SolveNow();
            message = fault switch
            {
                0 => "The host cannot read primary flash. Use the U6 SPI measurement.",
                1 => "The host rejected a primary image. Compare SPI response and authentication.",
                _ => "The recovery request is unreliable. Capture its idle and pressed levels."
            };
            QueueCheckpoint();
            return true;
        }

        public bool MeasureRequest()
        {
            if (!PowerAvailable || flash.Busy) return Reject("Power the idle bench to test the recovery input.");
            float idle = Voltage(requestSense, ground);
            SetContact(requestButton, true);
            manager.ElectricalSolver.SolveNow();
            float pressed = Voltage(requestSense, ground);
            SetContact(requestButton, false);
            manager.ElectricalSolver.SolveNow();
            float released = Voltage(requestSense, ground);
            bool valid = idle > 2.6f && pressed < .25f && released > 2.6f;
            eventText = $"Recovery input · idle {idle:F2} V / pressed {pressed:F2} V / released {released:F2} V.";
            if (fault == 2 && !valid) causeObserved = true;
            if (fault == 2 && reworked) requestProved = valid;
            message = valid ? "Valid high–low–high transition." : "No valid idle high. Inspect the recovery pull-up footprint.";
            return valid;
        }

        public void FindRepair()
        {
            if (fault == 0 && causeObserved) SelectFootprint(primaryIndex);
            if (fault == 2 && causeObserved) SelectFootprint(pullupIndex);
            if (fault is 0 or 2 && causeObserved) FocusPoint(parts[footprint].footprint.position);
        }

        public bool RepairContact()
        {
            if (powered || !causeObserved || reworked || (fault != 0 && fault != 2)
                || (fault == 0 && footprint != primaryIndex) || (fault == 2 && footprint != pullupIndex))
                return Reject("Measure the fault, select its footprint and isolate power before rework.");
            if (fault == 0) chipSelectFault = false;
            else pullupFault = false;
            reworked = true;
            message = "Contact repaired. Apply power and capture fresh proof.";
            QueueCheckpoint();
            return true;
        }

        private void TickFirmware(float delta)
        {
            time += delta;
            if (flash.Busy)
            {
                if (!PowerAvailable || !RecoveryBusReady || !PrimaryBusReady || Voltage(writeSense, ground) < 2.6f)
                {
                    flash.Interrupt();
                    testWrite = false;
                    primaryText = "Transfer interrupted";
                    message = "Recheck power, bus continuity, and programming enable.";
                }
                else
                {
                    flash.Tick(delta);
                    primaryText = flash.Erasing ? "Erasing U6" : $"Programmed {flash.PagesWritten}/{flash.Pages} pages";
                    if (!flash.Busy)
                    {
                        primaryVerified = flash.PrimaryValid;
                        primaryText = primaryVerified ? "Readback verified" : "Write failed";
                        if (!testWrite && primaryVerified)
                        {
                            if (stage == 2) initialRestored = true;
                            if (fault == 1 && causeObserved) reworked = true;
                        }
                        testWrite = false;
                        eventText = primaryVerified ? $"Page-program readback matched {flash.Digest}." : "U6 readback failed verification.";
                        message = "Engage U6 write protection, then run POST.";
                        QueueCheckpoint();
                    }
                }
            }
            if (postRunning)
            {
                if (!PowerAvailable || !PrimaryBusReady || !primaryVerified)
                {
                    postStep = 0;
                    postRunning = false;
                    message = "POST halted on live power or primary SPI.";
                }
                else
                {
                    postClock += delta;
                    postStep = Mathf.Min(PostSteps.Length, Mathf.FloorToInt(postClock / .16f));
                    if (BootReady) { postRunning = false; FinishBoot(); }
                }
            }
            TickSupervisor();
        }

        private void FinishBoot()
        {
            eventText = "POST trace · standby, SPI, controller, DDR training, and storage verified.";
            if (stage == 2 && initialRestored && recoveryVerified && Award())
                message = "Initial boot verified. Unlock U6, start the update trial, and cut power while busy.";
            else if (stage == 3 && cutObserved && recoveryVerified && Award())
                message = "Interrupted update recovered from U7. Load the first repair exercise.";
            else if (stage == 4 && fault >= 0 && reworked && primaryVerified && (fault != 2 || requestProved))
            {
                repaired++;
                fault = -1;
                causeObserved = reworked = false;
                if (repaired == 3 && Award())
                    message = "All repairs verified. Wait for W149 pulses, then open the jumper.";
                else message = "Repair verified with a fresh boot. Load the next fault.";
                QueueCheckpoint();
            }
            else message = stage == 5 && safetyInterrupted ? "Fresh POST verified. Wait for the live heartbeat, then reset the supervisor." : "POST passed.";
        }

        private void TickSupervisor()
        {
            bool produce = stage >= 5 && PowerAvailable && BootReady && primaryVerified;
            if (produce && time >= nextPulse)
            {
                nextPulse = time + .14f;
                SetContact(heartbeat, !heartbeat.IsClosed);
                manager.ElectricalSolver.SolveNow();
            }
            else if (!produce) SetContact(heartbeat, false);
            bool signalHigh = produce && jumperPresent && Voltage(receiver, ground) > 2.6f;
            if (signalHigh && !high) { rises++; lastPulse = time; }
            high = signalHigh;
            if (!produce || !jumperPresent) { rises = 0; lastPulse = float.NegativeInfinity; }
            SetContact(workload, produce && Fresh && (!safetyInterrupted || safetyReset));
            if (!Inhibited && !safetyInterrupted) armed = true;
            if (stage == 5 && safetyInterrupted && safetyReset && Fresh && !Inhibited && Voltage(protectedFeed, ground) > 11f && Award())
                message = "Independent supervisor and protected load verified.";
        }

        public bool OpenJumper()
        {
            if (stage != 5 || !powered || !armed || Inhibited || safetyInterrupted)
                return Reject("Start from a live, freshly qualified W149 link.");
            registry?.RefreshNow();
            if (registry)
                foreach (var entry in registry.Wires)
                    if ((entry.TerminalA == jumper.from && entry.TerminalB == jumper.to ||
                         entry.TerminalB == jumper.from && entry.TerminalA == jumper.to) && registry.RemoveWire(entry.WireId))
                    { ReadGraph(); SelectRoute(routes.Length); return true; }
            foreach (var edge in edges)
                if (edge.Kind == SparkConnectionKind.Wire && Matches(edge, jumper) && manager.CircuitSystem.TryRemoveConnection(edge.Id, out _))
                { ReadGraph(); SelectRoute(routes.Length); return true; }
            return Reject("W149 is already open.");
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !safetyInterrupted || safetyReset || !PowerAvailable || !recoveryVerified || !primaryVerified || !BootReady || !Fresh)
                return Reject("Restore W149, authenticate U7, verify U6, rerun POST and wait for fresh W149 pulses.");
            safetyReset = true;
            eventText = "Supervisor reset with current W149, bank trust and POST evidence.";
            return true;
        }
    }
}
