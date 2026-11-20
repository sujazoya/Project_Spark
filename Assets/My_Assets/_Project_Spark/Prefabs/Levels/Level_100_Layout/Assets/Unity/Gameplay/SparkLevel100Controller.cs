using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel100Process
    {
        private enum Boot { Off, Ramp, ResetHold, Oscillator, Pll, Verify, Init, Running, Halt }
        private enum Diagnostic { None, I2cLow, I2cRise, SpiHigh, SpiLow, UartHigh, UartLow }

        [Serializable]
        public sealed class BusPort
        {
            public SparkTerminal sender, receiver;
            public SparkSwitch drive, sink;
        }

        private const int Sda = 0, Scl = 1, Sck = 2, Tx = 7, Rx = 8;
        private const int Baud = 115200, FaultBaud = 9600;
        private readonly SparkLevel100Firmware firmware = new();
        private SparkLevel100Firmware.Image selectedImage;
        private readonly SparkLevel100Protocols protocols = new();
        private int uartBaud = Baud;
        private float i2cRise;
        private string traffic = "No qualified traffic";
        private Boot boot;
        private Diagnostic diagnostic;
        private int activeFault = -1, repaired, bootEpoch, requiredEpoch, intervalCount, timerTicks;
        private int passedDiagnostics;
        private bool faultWitnessed, reworked, armed, interrupted, safetyReset, lastHigh, localPulse;
        private float time, phaseAt, nextSolve, nextPulse, lastRise = float.NegativeInfinity, nextTimer;
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
        public string Traffic => traffic;
        public int UartBaud => uartBaud;
        public float I2cRise => i2cRise;
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
                return Reject("Complete the board, routes, vias and W093 before powering up.");
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
            SetContact(ports[Sda].sink, false);
            SetContact(ports[Scl].sink, false);
            SetOutputs(Sck, Rx, false);
            traffic = "No fresh bus capture";
        }

        private void ConfigureFaultDevices()
        {
            i2cPullup.SetResistance(FaultLive(0) ? 47000f : 2200f);
        }

        public void CycleImage() => selectedImage = (SparkLevel100Firmware.Image)(((int)selectedImage + 1) % 3);

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

        private void Halt(string reason)
        {
            InvalidateBoot();
            MoveTo(Boot.Halt, reason);
            message = reason;
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
                if (IsBooting || Running) Halt("BROWNOUT · MCU RESET");
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
                        Halt("E01 · Y1 CLOCK STARTUP FAILED");
                    break;
                case Boot.Pll when Age > .20f:
                    SetContact(resetRelease, true);
                    MoveTo(Boot.Verify, "BOOT ROM · RESET AND IMAGE CHECK");
                    break;
                case Boot.Verify when Age > .18f:
                    if (Voltage(resetSense) < 2.65f) Halt("E03 · RESET HELD LOW");
                    else if (!firmware.VerifyReadback()) Halt("E10 · IMAGE DIGEST FAILED");
                    else MoveTo(Boot.Init, "48 MHz · INITIALIZING PERIPHERALS");
                    break;
                case Boot.Init when Age > .19f:
                    bootEpoch++;
                    timerTicks = 0;
                    MoveTo(Boot.Running, "APPLICATION RUNNING · 48 MHz");
                    message = "Run bus tests to capture I²C, SPI and UART traffic.";
                    break;
                case Boot.Running:
                    if (Voltage(clockSense) < 2.55f || Voltage(resetSense) < 2.55f)
                        Halt("E11 · CLOCK OR RESET INVALIDATED");
                    else if (time >= nextTimer)
                    {
                        nextTimer = time + .085f;
                        timerTicks++;
                    }
                    break;
            }
            UpdateSupervisor();
            if (Running && Diagnosing) AdvanceDiagnostics();
            SampleLogic(delta);
            if (stage == 5 && interrupted && safetyReset && bootEpoch >= requiredEpoch && passedDiagnostics == 3 &&
                Fresh && !Inhibited && Voltage(protectedFeed) > 3f && Award())
                message = "Digital Nexus certified. New boot, diagnostics and W093 recovery verified.";
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
            SetContact(workload, stage >= 5 && Fresh && passedDiagnostics == 3 &&
                (!interrupted || safetyReset && bootEpoch >= requiredEpoch));
            if (stage == 5 && !interrupted && !Inhibited && passedDiagnostics == 3) armed = true;
        }

        public bool RunDiagnostics()
        {
            if (!Running || !HasPower || Diagnosing || stage < 2)
                return Reject("Boot the verified DNX-8 image before capturing bus traffic.");
            passedDiagnostics = 0;
            i2cRise = 0f;
            traffic = "Acquiring fresh bus traffic";
            SetContact(ports[Sda].sink, true);
            SetContact(ports[Scl].sink, true);
            DiagnosticStep(Diagnostic.I2cLow, "I²C · PULLING SDA AND SCL LOW");
            return true;
        }

        private void DiagnosticStep(Diagnostic next, string detail)
        {
            diagnostic = next;
            phaseAt = time;
            evidence = detail;
        }

        private void SetOutputs(int first, int last, bool high)
        {
            for (int i = first; i <= last; i++) SetContact(ports[i].drive, high);
        }

        private bool Level(int port, bool high) => high ? Voltage(ports[port].receiver) > 2.6f :
            Voltage(ports[port].receiver) < .30f;

        private bool Levels(int first, int last, bool high)
        {
            for (int i = first; i <= last; i++)
                if (!Level(i, high)) return false;
            return true;
        }

        private void AdvanceDiagnostics()
        {
            if (Age < .13f) return;
            switch (diagnostic)
            {
                case Diagnostic.I2cLow:
                    if (!Levels(Sda, Scl, false))
                    {
                        FailDiagnostic(-1, "I²C low level missing at the receiver.");
                        return;
                    }
                    SetContact(ports[Sda].sink, false);
                    SetContact(ports[Scl].sink, false);
                    DiagnosticStep(Diagnostic.I2cRise, "I²C · MEASURING RELEASE AND RISE");
                    break;
                case Diagnostic.I2cRise:
                    if (!Levels(Sda, Scl, true))
                    {
                        if (Age < .44f) return;
                        i2cRise = Age;
                        FailDiagnostic(0, $"F01 · R1 SDA rise missed the window · {Voltage(ports[Sda].receiver):F2} V");
                        return;
                    }
                    i2cRise = Age;
                    if (!protocols.ExchangeI2c(out string i2c))
                    {
                        FailDiagnostic(-1, "I²C scan or register exchange failed.");
                        return;
                    }
                    passedDiagnostics = 1;
                    traffic = i2c;
                    SetOutputs(Sck, Tx - 1, true);
                    DiagnosticStep(Diagnostic.SpiHigh, "SPI · OBSERVING CLOCK AT BOTH ENDS");
                    break;
                case Diagnostic.SpiHigh:
                    if (!Levels(Sck, Tx - 1, true))
                    {
                        bool openClock = Voltage(ports[Sck].sender) > 2.6f && Voltage(ports[Sck].receiver) < .30f;
                        FailDiagnostic(openClock ? 1 : -1, $"SPI SCK · drive {Voltage(ports[Sck].sender):F2} V / receive {Voltage(ports[Sck].receiver):F2} V");
                        return;
                    }
                    SetOutputs(Sck, Tx - 1, false);
                    DiagnosticStep(Diagnostic.SpiLow, "SPI · CHECKING CLOCK FALL AND CHIP SELECTS");
                    break;
                case Diagnostic.SpiLow:
                    if (!Levels(Sck, Tx - 1, false) || !protocols.ExchangeSpi(out string spi))
                    {
                        FailDiagnostic(-1, "SPI clock, chip-select or data verification failed.");
                        return;
                    }
                    passedDiagnostics = 2;
                    traffic += "\n" + spi;
                    if (stage == 2)
                    {
                        Award();
                        diagnostic = Diagnostic.None;
                        evidence = "I²C and SPI commissioned";
                        message = "Run bus tests again to qualify UART and the full multi-bus packet set.";
                        return;
                    }
                    SetOutputs(Tx, Rx, true);
                    DiagnosticStep(Diagnostic.UartHigh, "UART · VERIFYING TX AND RX IDLE LEVELS");
                    break;
                case Diagnostic.UartHigh:
                    if (!Levels(Tx, Rx, true))
                    {
                        FailDiagnostic(-1, "UART idle level missing at a receiver.");
                        return;
                    }
                    SetOutputs(Tx, Rx, false);
                    DiagnosticStep(Diagnostic.UartLow, "UART · SAMPLING START, DATA AND STOP BITS");
                    break;
                case Diagnostic.UartLow:
                    if (!Levels(Tx, Rx, false))
                    {
                        FailDiagnostic(-1, "UART start bit missing at a receiver.");
                        return;
                    }
                    if (!protocols.ExchangeUart(Baud, uartBaud, out string serial))
                    {
                        traffic += "\n" + serial;
                        FailDiagnostic(2, "F03 · U8 UART frame or payload mismatch");
                        return;
                    }
                    SetOutputs(Tx, Rx, true);
                    passedDiagnostics = 3;
                    traffic += "\n" + serial;
                    diagnostic = Diagnostic.None;
                    FinishDiagnostics();
                    break;
            }
        }

        private void FailDiagnostic(int fault, string result)
        {
            diagnostic = Diagnostic.None;
            evidence = result;
            if (fault >= 0 && FaultLive(fault)) faultWitnessed = true;
            message = faultWitnessed && activeFault == 2 ?
                "Locate U8, then apply the 115200 8N1 setting and retest." :
                faultWitnessed ? "Locate the fault and isolate bench power before repair." : result;
            if (faultWitnessed) QueueCheckpoint();
        }

        private void FinishDiagnostics()
        {
            evidence = $"Epoch {bootEpoch} · I²C / SPI / UART verified";
            if (stage == 3) Award();
            else if (stage == 4 && activeFault >= 0 && reworked)
            {
                repaired++;
                activeFault = -1;
                reworked = faultWitnessed = false;
                if (repaired == 3) Award();
                QueueCheckpoint();
            }
            if (stage == 4) message = "Multi-bus diagnostics passed. Start the next fault trial.";
            else if (stage == 5 && !interrupted) message = "Wait for a fresh heartbeat, then open W093 under power.";
        }

        public bool BeginFaultTrial()
        {
            if (stage != 4 || activeFault >= 0 || repaired >= 3 || !Running || passedDiagnostics != 3 || Diagnosing)
                return Reject("Finish a healthy boot and all bus tests before starting a fault trial.");
            activeFault = repaired;
            faultWitnessed = reworked = false;
            ConfigureFaultDevices();
            if (activeFault == 1)
            {
                SetContact(routes[spiRoute].barrel, false);
                ReadGraph();
            }
            else if (activeFault == 2) uartBaud = FaultBaud;
            message = "The application stays running. Capture new bus traffic to isolate the failure.";
            QueueCheckpoint();
            return true;
        }

        public bool LocateFault()
        {
            if (!faultWitnessed || activeFault < 0) return Reject("Capture failing bus traffic before selecting a repair target.");
            if (activeFault == 1) { SelectRoute(spiRoute); PadView(); }
            else { SelectFootprint(activeFault == 0 ? pullupPart : uartPart); FocusPart(); }
            return true;
        }

        public bool Repair()
        {
            if (activeFault < 0 || !faultWitnessed || reworked)
                return Reject("Capture the failure before repairing the selected target.");
            if (activeFault == 2)
            {
                if (!Running || !HasPower || footprint != uartPart) return Reject("Select powered U8 to apply its serial setting.");
                uartBaud = Baud;
                reworked = true;
                InvalidateBoot();
                message = "U8 configured for 115200 8N1. Boot again and verify both packet directions.";
            }
            else
            {
                if (powered) return Reject("Isolate the bench before replacing R1 or plating V11.");
                if (activeFault == 1 ? selectedRoute != spiRoute : footprint != pullupPart)
                    return Reject("Select the diagnosed resistor or via.");
                reworked = true;
                ConfigureFaultDevices();
                if (activeFault == 1) SetContact(routes[spiRoute].barrel, true);
                ReadGraph();
                message = "Repair applied. Power up, boot and verify all buses.";
            }
            BoardView();
            QueueCheckpoint();
            return true;
        }

        public bool OpenW093()
        {
            if (stage != 5 || !powered || !armed || !linked || Inhibited || Diagnosing)
                return Reject("Open W093 only after all three repairs and a live protected output.");
            return RemoveWire(jumper);
        }

        public bool RestoreW093()
        {
            SelectRoute(routes.Length);
            return ConnectPads();
        }

        public bool ResetSafety()
        {
            if (stage != 5 || !interrupted || safetyReset || !Running || !Fresh || !Construction)
                return Reject("Reconnect W093 and acquire fresh edge cadence before authorizing U9 reset.");
            requiredEpoch = bootEpoch + 1;
            safetyReset = true;
            message = "U9 reset authorized. Collect a new boot and diagnostic epoch.";
            return true;
        }
    }
}
