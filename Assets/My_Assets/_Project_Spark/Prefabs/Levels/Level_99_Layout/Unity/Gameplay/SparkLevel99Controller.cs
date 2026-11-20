using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel99Process
    {
        private enum Boot { Off, Ramp, ResetHold, Oscillator, Pll, Verify, Init, Running, Halt }
        private enum Diagnostic { None, GpioHigh, GpioLow, Timer, SpiWrite, SpiRead, I2cIdle, I2cStart, I2cAck, Watchdog }
        private static readonly string[] FaultNames = { "F01 · C1 CRYSTAL LOAD", "F02 · V11 SPI DATA", "F03 · R18 RESET PULL-UP" };
        private readonly SparkLevel99Firmware firmware = new();
        private SparkLevel99Firmware.Image selectedImage;
        private readonly byte[] flashDiagnosticPage = new byte[32];
        private Boot boot;
        private Diagnostic diagnostic;
        private int activeFault = -1, repaired, bootEpoch, requiredEpoch, intervalCount, timerTicks, diagnosticTimer, watchdogServices, diagnosticServices;
        private int passedDiagnostics;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset, lastHigh, localPulse;
        private float time, phaseAt, nextSolve, nextPulse, lastRise = float.NegativeInfinity, nextTimer, lastService;
        private string benchState = "BENCH ISOLATED";
        private string evidence = "SWD image slot empty";

        public bool Running => boot == Boot.Running;
        public bool Programming => firmware.Busy;
        public bool ProgramVerified => firmware.IsVerified;
        public bool Diagnosing => diagnostic != Diagnostic.None;
        public bool FaultWitnessed => faultWitnessed;
        public int Repaired => repaired;
        public int Epoch => bootEpoch;
        public int TimerTicks => timerTicks;
        public int DiagnosticsPassed => passedDiagnostics;
        public string Evidence => evidence;
        public bool Fresh => linked && intervalCount >= 2 && time - lastRise < .54f;
        private bool FaultLive(int index) => activeFault == index && !reworked;
        private bool HasPower => powered && regulator.IsConducting && TrialTopology &&
            Voltage(standby) > 4.7f && Voltage(vdd) is > 3.05f and < 3.50f;
        private bool IsBooting => boot is >= Boot.Ramp and <= Boot.Init;
        private float Age => time - phaseAt;

        private void Solve()
        {
            var solver = manager.ElectricalSolver;
            solver.MarkDirty();
            solver.SolveNow();
        }

        public bool TogglePower()
        {
            if (powered)
            {
                IsolatePower();
                message = "Bench isolated. Choose the diagnosed part or contact.";
                return true;
            }
            if (stage < 2 || !TrialTopology || !linked)
                return Reject("Complete the board, routes, vias and W083 before powering up.");
            powered = true;
            SetContact(bench, true);
            ConfigureFaultDevices();
            MoveTo(Boot.Ramp, "5 V INPUT · MCU RAMP");
            message = firmware.IsVerified ? "Wait for 3.3 V, then run a fresh boot." : "Select the approved diagnostic image and program U1 over SWD.";
            return true;
        }

        private void IsolatePower()
        {
            powered = false;
            InvalidateBoot();
            SetContact(regulator, false);
            SetContact(bench, false);
            MoveTo(Boot.Off, "BENCH ISOLATED");
            Solve();
        }

        private void InvalidateBoot()
        {
            bootRequested = false;
            firmware.Abort();
            InvalidateDiagnostics();
            ClearHeartbeat();
            SetContact(clockDrive, false);
            SetContact(clockDischarge, true);
            SetContact(resetRelease, false);
            MoveTo(powered ? Boot.ResetHold : Boot.Off, powered ? "RESET HELD · AWAITING BOOT" : "BENCH ISOLATED");
        }

        private void InvalidateDiagnostics()
        {
            diagnostic = Diagnostic.None;
            passedDiagnostics = 0;
            SetContact(gpioDrive, false);
            SetContact(i2cClockDrive, false);
            SetContact(i2cDataDrive, false);
        }

        private void ConfigureFaultDevices()
        {
            SetContact(healthyClockCap, !FaultLive(0));
            SetContact(faultyClockCap, FaultLive(0));
            resetPullup.SetResistance(FaultLive(2) ? 470000f : 180f);
        }

        public void CycleImage() => selectedImage = (SparkLevel99Firmware.Image)(((int)selectedImage + 1) % 3);

        public bool Program()
        {
            if (stage < 2 || !HasPower || Voltage(swdSense) < 2.7f || Voltage(resetSense) > .25f || firmware.Busy)
                return Reject("SWD requires powered U1, SWD continuity and reset held low.");
            if (!firmware.Begin(selectedImage, out var reason)) return Reject(reason);
            message = "SWD accepted the curated image. Writing and verifying internal memory.";
            return true;
        }

        public bool HoldReset()
        {
            if (!HasPower || firmware.Busy) return Reject("Stable power and an idle programmer are required.");
            InvalidateBoot();
            return true;
        }

        public bool StartBoot()
        {
            if (!HasPower || !firmware.IsVerified || firmware.Busy || boot == Boot.Ramp)
                return Reject("Apply valid supply and verify the internal image before booting.");
            InvalidateBoot();
            MoveTo(Boot.ResetHold, "RESET HELD · DISCHARGING Y1");
            bootRequested = true;
            return true;
        }

        private bool bootRequested;

        private void MoveTo(Boot state, string label)
        {
            boot = state;
            phaseAt = time;
            benchState = label;
            if (state is Boot.Off or Boot.Halt or Boot.Running) bootRequested = false;
        }

        private void Halt(string reason, int identifiedFault)
        {
            InvalidateBoot();
            MoveTo(Boot.Halt, reason);
            if (identifiedFault >= 0 && FaultLive(identifiedFault))
            {
                faultWitnessed = true;
                evidence = identifiedFault == 0 ? $"C1/Y1 failed to reach the 8 MHz startup threshold" : "Reset failed to rise · R18 pull-up open";
                message = "Fault evidence captured. Isolate the bench and repair the diagnosed part.";
                QueueCheckpoint();
            }
            else message = reason;
        }

        private void TickController(float delta)
        {
            time += delta;
            if (!powered) return;
            if (time >= nextSolve)
            {
                nextSolve = time + .055f;
                Solve();
            }
            if (boot == Boot.Ramp)
            {
                SetContact(regulator, Age > .18f && Voltage(standby) > 4.7f);
                if (HasPower) MoveTo(Boot.ResetHold, "3.3 V VALID · RESET HELD");
                return;
            }

            if (!HasPower)
            {
                if (IsBooting || Running) Halt("BROWNOUT · MCU RESET", -1);
                UpdateSupervisor();
                return;
            }

            if (firmware.Busy && firmware.Tick(delta))
            {
                evidence = firmware.Identity;
                message = "SWD write and readback verified. Run the application boot.";
                QueueCheckpoint();
            }

            switch (boot)
            {
                case Boot.ResetHold when bootRequested && Age > .25f:
                    SetContact(clockDischarge, false);
                    SetContact(clockDrive, true);
                    MoveTo(Boot.Oscillator, "Y1 · QUALIFYING 8 MHz");
                    break;
                case Boot.Oscillator:
                    if (Age > .14f && Voltage(clockSense) > 2.70f)
                        MoveTo(Boot.Pll, "Y1 8 MHz · PLL ×6");
                    else if (Age > .74f)
                        Halt("E01 · Y1 CLOCK STARTUP FAILED", 0);
                    break;
                case Boot.Pll when Age > .20f:
                    SetContact(resetRelease, true);
                    MoveTo(Boot.Verify, "BOOT ROM · RESET AND IMAGE CHECK");
                    break;
                case Boot.Verify when Age > .18f:
                    if (Voltage(resetSense) < 2.65f) Halt("E03 · RESET HELD LOW", 2);
                    else if (!firmware.VerifyReadback()) Halt("E10 · IMAGE DIGEST FAILED", -1);
                    else MoveTo(Boot.Init, "48 MHz · INITIALIZING PERIPHERALS");
                    break;
                case Boot.Init when Age > .19f:
                    bootEpoch++;
                    timerTicks = watchdogServices = 0;
                    lastService = time;
                    MoveTo(Boot.Running, "APPLICATION RUNNING · 48 MHz");
                    if (stage == 2) Award();
                    message = "Run GPIO, timer, external SPI, I²C and watchdog diagnostics.";
                    break;
                case Boot.Running:
                    if (Voltage(clockSense) < 2.55f || Voltage(resetSense) < 2.55f)
                        Halt("E11 · CLOCK OR RESET INVALIDATED", -1);
                    else if (time >= nextTimer)
                    {
                        nextTimer = time + .085f;
                        timerTicks++;
                        if (timerTicks % 3 == 0) { watchdogServices++; lastService = time; }
                    }
                    if (time - lastService > .52f) Halt("E12 · WATCHDOG EXPIRED", -1);
                    break;
            }
            UpdateSupervisor();
            if (Running && Diagnosing) AdvanceDiagnostics();
            SampleLogic(delta);
            if (stage == 5 && interrupted && safetyReset && bootEpoch >= requiredEpoch && passedDiagnostics == 5 &&
                Fresh && !Inhibited && Voltage(protectedFeed) > 3f && Award())
                message = "Silicon Brain certified. New boot, diagnostics and W083 recovery verified.";
        }

        private void ClearHeartbeat()
        {
            intervalCount = 0;
            lastHigh = localPulse = false;
            lastRise = float.NegativeInfinity;
            SetContact(heartbeat, false);
            SetContact(workload, false);
        }

        private void UpdateSupervisor()
        {
            if (!Running || !HasPower)
            {
                ClearHeartbeat();
                return;
            }
            if (time >= nextPulse)
            {
                nextPulse = time + .16f;
                localPulse = !localPulse;
                SetContact(heartbeat, localPulse);
                Solve();
            }
            bool high = linked && Voltage(jumper.to) > 2.6f;
            if (high && !lastHigh)
            {
                float gap = time - lastRise;
                intervalCount = gap is >= .24f and <= .46f ? intervalCount + 1 : 0;
                lastRise = time;
            }
            lastHigh = high;
            SetContact(workload, Fresh && (!interrupted || safetyReset && bootEpoch >= requiredEpoch && passedDiagnostics == 5));
            if (stage == 5 && !interrupted && !Inhibited && passedDiagnostics == 5) armed = true;
        }

        public bool RunDiagnostics()
        {
            if (!Running || !HasPower || Diagnosing || stage < 3)
                return Reject("Run a fresh application boot before starting peripheral diagnostics.");
            passedDiagnostics = 0;
            SetContact(gpioDrive, true);
            MoveDiagnostic(Diagnostic.GpioHigh, "GPIO HIGH · SAMPLING");
            message = "The board is sampling actual GPIO, SPI and I²C loopback contacts.";
            return true;
        }

        private void MoveDiagnostic(Diagnostic step, string label)
        {
            diagnostic = step;
            phaseAt = time;
            evidence = label;
        }

        private void AdvanceDiagnostics()
        {
            if (Age < .15f) return;
            switch (diagnostic)
            {
                case Diagnostic.GpioHigh:
                    if (Voltage(gpioSense) < 2.7f) { FailDiagnostic("GPIO high witness missing"); break; }
                    SetContact(gpioDrive, false);
                    MoveDiagnostic(Diagnostic.GpioLow, "GPIO LOW · SAMPLING");
                    break;
                case Diagnostic.GpioLow:
                    if (Voltage(gpioSense) > .25f) { FailDiagnostic("GPIO did not fall"); break; }
                    passedDiagnostics = 1;
                    diagnosticTimer = timerTicks;
                    MoveDiagnostic(Diagnostic.Timer, "TIMER · WAITING FOR INTERRUPTS");
                    break;
                case Diagnostic.Timer:
                    if (timerTicks - diagnosticTimer < 2) return;
                    passedDiagnostics = 2;
                    MoveDiagnostic(Diagnostic.SpiWrite, "U2 SPI · WRITE TEST PAGE");
                    break;
                case Diagnostic.SpiWrite:
                    if (!VerifySpiLink()) break;
                    for (int i = 0; i < flashDiagnosticPage.Length; i++) flashDiagnosticPage[i] = (byte)(i * 7 ^ 0xA5);
                    MoveDiagnostic(Diagnostic.SpiRead, "U2 SPI · READ BACK TEST PAGE");
                    break;
                case Diagnostic.SpiRead:
                    if (!VerifySpiLink() || !VerifySpiPage()) break;
                    passedDiagnostics = 3;
                    SetContact(i2cClockDrive, true);
                    SetContact(i2cDataDrive, true);
                    MoveDiagnostic(Diagnostic.I2cIdle, "I²C · VERIFYING IDLE BUS");
                    break;
                case Diagnostic.I2cIdle:
                    if (Voltage(i2cClock) < 2.7f || Voltage(i2cData) < 2.7f) { FailDiagnostic("I²C idle levels invalid"); break; }
                    SetContact(i2cDataDrive, false);
                    MoveDiagnostic(Diagnostic.I2cStart, "I²C START · DATA FALLS WITH SCL HIGH");
                    break;
                case Diagnostic.I2cStart:
                    if (Voltage(i2cClock) < 2.7f || Voltage(i2cData) > .25f) { FailDiagnostic("I²C START invalid"); break; }
                    MoveDiagnostic(Diagnostic.I2cAck, "I²C · MODEL TARGET ACK");
                    break;
                case Diagnostic.I2cAck:
                    if (Voltage(i2cData) > .25f) { FailDiagnostic("I²C acknowledgement missing"); break; }
                    SetContact(i2cDataDrive, true);
                    passedDiagnostics = 4;
                    diagnosticServices = watchdogServices;
                    MoveDiagnostic(Diagnostic.Watchdog, "WATCHDOG · CHECKING SERVICE CADENCE");
                    break;
                case Diagnostic.Watchdog:
                    if (watchdogServices - diagnosticServices < 2) return;
                    if (time - lastService > .40f) { FailDiagnostic("Watchdog service timed out"); break; }
                    passedDiagnostics = 5;
                    diagnostic = Diagnostic.None;
                    FinishDiagnostics();
                    break;
            }
        }

        private bool VerifySpiLink()
        {
            if (Voltage(spiSense) > 2.7f) return true;
            if (FaultLive(1)) { faultWitnessed = true; QueueCheckpoint(); }
            FailDiagnostic("E02 · U2 DATA OPEN AT V11 / U1 STILL RUNNING");
            return false;
        }

        private bool VerifySpiPage()
        {
            for (int i = 0; i < flashDiagnosticPage.Length; i++)
            {
                if (flashDiagnosticPage[i] == (byte)(i * 7 ^ 0xA5)) continue;
                FailDiagnostic("U2 readback mismatch");
                return false;
            }
            return true;
        }

        private void FailDiagnostic(string note)
        {
            diagnostic = Diagnostic.None;
            evidence = note;
            message = faultWitnessed ? "Isolate power and repair the diagnosed SPI barrel." : note;
        }

        private void FinishDiagnostics()
        {
            evidence = $"Epoch {bootEpoch} · GPIO, timer, SPI, I²C and watchdog passed";
            if (stage == 3) Award();
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                reworked = faultWitnessed = false;
                if (repaired == 3) Award();
                QueueCheckpoint();
            }
            if (stage == 4) message = "Diagnostics passed. Start the next fault trial.";
            else if (stage == 5 && !interrupted) message = "Wait for a fresh heartbeat, then open W083 under power.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Running || passedDiagnostics != 5 || Diagnosing)
                return Reject("Finish a healthy boot and diagnostics before triggering a new fault.");
            activeFault = repaired;
            faultWitnessed = reworked = false;
            ConfigureFaultDevices();
            if (activeFault == 1)
            {
                SetContact(routes[spiRoute].barrel, false);
                ReadGraph();
                message = "V11 opened. Keep U1 running and test the external SPI bus.";
            }
            else
            {
                InvalidateBoot();
                message = $"Run Boot to reproduce {FaultNames[activeFault]}.";
            }
            QueueCheckpoint();
            return true;
        }

        public bool LocateFault()
        {
            if (!faultWitnessed || activeFault < 0) return Reject("Capture the fault before selecting a repair target.");
            if (activeFault == 1) { SelectRoute(spiRoute); PadView(); }
            else { SelectFootprint(activeFault == 0 ? capacitorPart : resetPart); FocusPart(); }
            return true;
        }

        public bool Repair()
        {
            if (activeFault < 0 || !faultWitnessed || reworked || powered)
                return Reject("Observe the failed test and isolate the bench before repair.");
            if (activeFault == 1 ? selectedRoute != spiRoute : footprint != (activeFault == 0 ? capacitorPart : resetPart))
                return Reject("Select the diagnosed footprint or via.");
            reworked = true;
            ConfigureFaultDevices();
            if (activeFault == 1) SetContact(routes[spiRoute].barrel, true);
            ReadGraph();
            QueueCheckpoint();
            BoardView();
            message = "Repair applied. Verify it with a new boot and every peripheral test.";
            return true;
        }

        public bool OpenW083()
        {
            if (stage != 5 || !powered || !armed || !linked || Inhibited || Diagnosing)
                return Reject("Open W083 only after all three repairs and a live protected output.");
            return RemoveWire(jumper);
        }

        public bool RestoreW083()
        {
            SelectRoute(routes.Length);
            return ConnectPads();
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !Running || !Fresh || !Construction)
                return Reject("Reconnect W083 and acquire fresh edge cadence before authorizing U8 reset.");
            requiredEpoch = bootEpoch + 1;
            safetyReset = true;
            message = "U8 reset authorized. Collect a new boot and diagnostic epoch.";
            return true;
        }
    }
}
