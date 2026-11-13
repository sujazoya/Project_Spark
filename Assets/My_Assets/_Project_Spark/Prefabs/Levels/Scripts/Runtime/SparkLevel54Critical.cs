using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel54Critical : MonoBehaviour
    {
        public static readonly string[] Gates = {
            "L54_Wired", "L54_Reference", "L54_Resonance", "L54_Damped", "L54_Crossed", "L54_Recovered"
        };
        public static readonly (int a, int b)[] Wires = {
            (0,1),(2,3),(0,4),(2,5),(0,6),(2,7),(0,8),(2,9),(0,10),(2,11),
            (12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),
            (28,29),(30,31),(32,33),(34,35)
        };
        private const float Step = .01f, SafeDisplacement = 2.55f, SafeCrossingDisplacement = 1.45f;
        private const float TorqueConstant = .12f, Inertia = .0019f;
        private enum Run { Idle, BaselineSweep, ComparisonSweep, Crossing, AtSpeed, Coasting, Stopping }
        private enum Gain { Off, Low, Medium, High, Auto }
        private static readonly int[] SweepSpeeds = {45, 51, 55, 58, 61, 64, 68, 72};

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkMotor motor;
        public SparkResistor driver, actuator;
        public SparkSwitch drive, damperDrive, safety;
        public SparkTerminal[] pins;
        public SparkLevel54VisualRig rig;
        public SparkLevel54Plot plot;
        public TMP_Text controllerDisplay, qualityDisplay;
        public Text statusText, meterText, detailText, calibrationText;
        public Button powerButton, startButton, stopButton, slowButton, criticalButton, fastButton, calibrateButton, baselineButton;
        public Button sweepButton, offButton, lowButton, mediumButton, highButton, autoButton, verifyButton, crossButton, coastButton;
        public Button phaseButton, guardButton, resetButton;

        private readonly List<SparkCircuitConnection> wires = new();
        private readonly Dictionary<SparkTerminal, SparkTerminal> nets = new();
        private readonly bool[] routed = new bool[Wires.Length];
        private readonly SparkLevel54Rotor rotor = new();
        private readonly SparkLevel54Encoder encoder = new();
        private readonly SparkLevel54Probe probeX = new(.24f), probeY = new(-.18f);
        private readonly SparkLevel54Measurement measurement = new();
        private readonly SparkLevel54Measurement.Reading[] baselineSweep = new SparkLevel54Measurement.Reading[SweepSpeeds.Length];
        private readonly SparkLevel54Measurement.Reading[] dampedSweep = new SparkLevel54Measurement.Reading[SweepSpeeds.Length];
        private SparkLevel54Measurement.Reading readout, reference;
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkLevelSceneBindings bindings;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private Run run;
        private Gain gain, certifiedGain;
        private int topology = -1, stage, targetRpm = 45, sweepIndex, revolutions;
        private float accumulator, clock, omega, shaftAngle, setpoint = 45, driveTimer, hold, operationAge;
        private float motorVoltage, motorCurrent, actuatorCurrent, pwmDuty, baselinePeak, dampedPeak, criticalRpm, phaseShift, crossingMaximum;
        private bool enabledPower = true, ready, safe, xPower, yPower, damperPower, guardOpen;
        private bool running, measuring, comparisonReady, crossedUp, crossedDown, armed, sawYFault, recovered;
        private string wiring, fault, notice;

        public static float NominalCriticalRpm => SparkLevel54Rotor.NaturalRpm;
        private float ActualRpm => omega * 30 / Mathf.PI;
        private bool Stopped => !running && ActualRpm < .35f && rotor.Position.magnitude < .00015f;
        private bool Stable => running && encoder.Locked && Mathf.Abs(ActualRpm - targetRpm) < .85f &&
            Mathf.Abs(encoder.Rpm - targetRpm) < 2f;
        private bool Calibrated => probeX.Calibrated && probeY.Calibrated;
        private bool CanStart => Stopped && ready && safe && Calibrated && fault == null;
        private bool CanCalibrate => Stopped && ready && safe && fault == null && probeX.Valid && probeY.Valid;
        private bool CanReset => Stopped && ready && safe && fault != null;
        private bool AtReference => run == Run.Idle && targetRpm == 45 && Stable;
        private bool CanSweep => AtReference && (stage == 2 || stage == 3 && gain != Gain.Off);
        private bool CanCross => AtReference && stage is 4 or 5 && gain == certifiedGain;
        private bool CanTune => run == Run.Idle && !armed && fault == null && stage < 4;
        private float Reduction => baselinePeak > 0 ? 1 - dampedPeak / baselinePeak : 0;
        private Vector2 Displacement => new(probeX.Filtered, probeY.Filtered);
        private bool DamperEnabled => rotor.ExtraDamping > .001f;

        private void Start()
        {
            loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            manager = FindFirstObjectByType<LevelGamePlayManager>();
            if (!loader || !manager || !source || !motor || !drive || !damperDrive || !safety ||
                !actuator || !driver || pins == null || pins.Length != 36)
            {
                Debug.LogError("[LEVEL 54] Missing campaign, motor, interlock or terminal reference.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (loader.ActiveLayout != layout) return;
            solver = manager.ElectricalSolver;
            circuit = manager.CircuitSystem;
            if (!solver || !circuit || loader.ActiveBindings == null) return;
            if (bindings != loader.ActiveBindings)
            {
                bindings = loader.ActiveBindings;
                topology = -1;
            }
            if (topology != circuit.TopologyVersion)
            {
                topology = circuit.TopologyVersion;
                wiring = InspectWiring();
            }

            ReadElectrical();
            if (stage == 0 && ready && safe) Award();
            if (running && (!ready || !safe)) Trip(WiringFault());
            accumulator += Mathf.Min(Time.deltaTime, .15f);
            while (accumulator >= Step)
            {
                accumulator -= Step;
                Simulate();
            }
            Present();
        }

        private SparkTerminal Net(SparkTerminal pin)
        {
            while (nets.TryGetValue(pin, out var next)) pin = next;
            return pin;
        }

        private string InspectWiring()
        {
            circuit.CopyConnections(wires);
            nets.Clear();
            Array.Clear(routed, 0, routed.Length);
            foreach (var wire in wires)
            {
                if (wire.Kind != SparkConnectionKind.Wire) continue;
                var a = Net(wire.A);
                var b = Net(wire.B);
                if (a != b) nets[b] = a;
            }

            string issue = null;
            for (int i = 0; i < Wires.Length; i++)
            {
                var (a, b) = Wires[i];
                routed[i] = wires.Exists(wire => wire.Kind == SparkConnectionKind.Wire &&
                    (wire.A == pins[a] && wire.B == pins[b] || wire.A == pins[b] && wire.B == pins[a]));
                if (!routed[i] && issue == null)
                    issue = $"Connect W{i + 1:00} · {pins[a].name} → {pins[b].name}.";
            }

            var distinct = new HashSet<SparkTerminal> {Net(pins[0]), Net(pins[2])};
            if (distinct.Count != 2) return "Separate +5 V from return.";
            if (issue != null) return issue;
            for (int i = 10; i < Wires.Length; i++)
                if (!distinct.Add(Net(pins[Wires[i].a]))) return "Separate the signal and drive nets.";

            int localWires = wires.FindAll(w => w.Kind == SparkConnectionKind.Wire &&
                (Array.IndexOf(pins, w.A) >= 0 || Array.IndexOf(pins, w.B) >= 0)).Count;
            return localWires == Wires.Length ? null : "Remove extra wires from the Level 54 sockets.";
        }

        private void ReadElectrical()
        {
            var result = solver.LatestSolveResult;
            bool Rail(int plus, int minus) => result != null &&
                result.TryGetVoltage(pins[plus], pins[minus], out float volts) && volts is >= 4.8f and <= 5.2f;
            xPower = Rail(6, 7);
            yPower = Rail(8, 9);
            damperPower = Rail(10, 11);
            ready = enabledPower && wiring == null && source.IsOutputActive && !source.IsOverloaded &&
                Rail(1,3) && Rail(4,5) && xPower && yPower && damperPower;
            safe = !guardOpen && safety.IsClosed && routed[21] && Rail(35,3);
            motorVoltage = Mathf.Abs(motor.ElectricalState.Voltage);
            motorCurrent = Mathf.Abs(motor.ElectricalState.Current);
            actuatorCurrent = Mathf.Abs(actuator.ElectricalState.Current);
        }

        private static void SetContact(SparkSwitch contact, bool closed)
        {
            if (contact.IsClosed != closed) contact.SetState(closed ? SparkSwitchState.Closed : SparkSwitchState.Open);
        }

        public void Power()
        {
            enabledPower = !enabledPower;
            if (!enabledPower) StopMachine();
            source.SetOutput(enabledPower, default);
        }

        public void Guard()
        {
            if (!Stopped) return;
            guardOpen = !guardOpen;
            SetContact(safety, !guardOpen);
        }

        public void Calibrate()
        {
            if (!CanCalibrate) return;
            probeX.Zero(0);
            probeY.Zero(0);
            notice = "Both probe zeros recorded at rest.";
        }

        public void StartMachine()
        {
            if (!CanStart) return;
            running = true;
            encoder.ClearLock();
            ResetMeasurement();
            setpoint = 0;
            if (stage < 6 || armed) targetRpm = 45;
            notice = armed ? "At 45 RPM, undo W15 to test the safety stop." : null;
            SetContact(drive, true);
        }

        public void StopMachine()
        {
            running = false;
            run = Run.Idle;
            pwmDuty = 0;
            SetContact(drive, false);
            SetContact(damperDrive, false);
            ResetMeasurement();
        }

        public void Speed45() => PreviewSpeed(45);
        public void Speed60() => PreviewSpeed(60);
        public void Speed75() => PreviewSpeed(75);
        private void PreviewSpeed(int rpm)
        {
            if (run != Run.Idle || armed || fault != null || (rpm > 45 && stage < 5)) return;
            targetRpm = rpm;
            ResetMeasurement();
        }

        public void Off() => SelectGain(Gain.Off);
        public void Low() => SelectGain(Gain.Low);
        public void Medium() => SelectGain(Gain.Medium);
        public void High() => SelectGain(Gain.High);
        public void Auto() => SelectGain(Gain.Auto);
        private void SelectGain(Gain value)
        {
            if (!CanTune) return;
            gain = value;
            if (stage == 3) comparisonReady = false;
            notice = null;
        }

        public void Baseline()
        {
            if (stage != 1 || !AtReference || revolutions < 3 || !Calibrated) return;
            reference = readout;
            Award();
        }

        public void Sweep()
        {
            if (!CanSweep) return;
            if (stage == 2) gain = Gain.Low;
            else comparisonReady = false;
            run = stage == 2 ? Run.BaselineSweep : Run.ComparisonSweep;
            sweepIndex = 0;
            notice = null;
            StartSweepPoint();
            operationAge = 0;
        }

        public void Verify()
        {
            if (stage != 3 || !comparisonReady || !AtReference || fault != null) return;
            if (Reduction < .50f)
            {
                notice = $"Peak fell {Reduction:P0}. Select stronger damping and repeat SPEED SWEEP.";
                return;
            }
            certifiedGain = gain;
            Award();
        }

        public void Cross()
        {
            if (!CanCross || (stage == 5 && !recovered)) return;
            crossingMaximum = 0;
            crossedUp = crossedDown = false;
            targetRpm = 75;
            hold = operationAge = 0;
            run = Run.Crossing;
        }

        public void Coast()
        {
            if (run != Run.AtSpeed || stage != 4) return;
            targetRpm = 35;
            operationAge = 0;
            run = Run.Coasting;
        }

        public void PhaseTest()
        {
            if (stage != 5 || !Stopped || recovered || fault != null || !ready || !safe) return;
            armed = true;
            targetRpm = 45;
            notice = "Press START for the guarded sensor test, then undo W15.";
        }

        public void ResetFault()
        {
            if (!CanReset) return;
            fault = null;
            armed = false;
            recovered |= sawYFault;
            targetRpm = 45;
            encoder.ClearLock();
            notice = "Recalibrate X/Y before restarting.";
        }

        private string WiringFault()
        {
            if (!routed[14] || !yPower) return "Y DISPLACEMENT SIGNAL LOST";
            if (!routed[13] || !xPower) return "X DISPLACEMENT SIGNAL LOST";
            if (!safe) return "GUARD INTERLOCK OPEN";
            if (!routed[15] || !routed[16]) return "ENCODER A/B DISCONNECTED";
            if (!routed[17]) return "ENCODER Z DISCONNECTED";
            if (!damperPower || !routed[18] || !routed[19] || !routed[20]) return "DAMPER DRIVER DISCONNECTED";
            if (!routed[10]) return "MOTOR PWM DISCONNECTED";
            return source.IsOverloaded ? "SUPPLY OVERLOAD" : wiring ?? "SUPPLY OR DRIVE RAIL INVALID";
        }

        private void Trip(string reason)
        {
            if (fault != null) return;
            sawYFault |= armed && stage == 5 && ActualRpm > 15 && !routed[14] && reason == "Y DISPLACEMENT SIGNAL LOST";
            fault = reason;
            armed = false;
            StopMachine();
            encoder.ClearLock();
            probeX.InvalidateCalibration();
            probeY.InvalidateCalibration();
        }

        private float RequestedDamping() => gain switch
        {
            Gain.Low => 0,
            Gain.Medium => .38f,
            Gain.High => 1.05f,
            Gain.Auto => Mathf.Lerp(.83f, 1.20f, Mathf.Clamp01(1 - Mathf.Abs(ActualRpm - SparkLevel54Rotor.NaturalRpm) / 20)),
            _ => 0
        };

        private void Simulate()
        {
            clock += Step;
            float drag = .0103f + .00043f * omega;
            float slew = run == Run.Coasting ? 8 : run == Run.Crossing ? 9 : 6;
            setpoint = Mathf.MoveTowards(setpoint, targetRpm, slew * Step);
            driveTimer += Step;
            if (running && driveTimer >= .12f)
            {
                driveTimer = 0;
                float requestedCurrent = Mathf.Clamp((drag + .0058f * (setpoint * Mathf.PI / 30 - omega)) / TorqueConstant, .013f, .197f);
                pwmDuty = requestedCurrent * 22 / 5;
                float resistance = 5 / requestedCurrent - 22.03f;
                if (Mathf.Abs(driver.ResistanceOhms - resistance) > .10f) driver.SetResistance(resistance);
            }

            omega = Mathf.Max(0, omega + ((running ? motorCurrent * TorqueConstant : 0) - drag) * Step / Inertia);
            shaftAngle += omega * Step;
            bool validEncoder = encoder.Step(Step, shaftAngle, routed[15], routed[16], routed[17]);
            float demand = RequestedDamping();
            bool commandValid = running && ready && safe && fault == null && Calibrated && encoder.Locked &&
                probeX.Valid && probeY.Valid && demand > 0 && routed[18];
            SetContact(damperDrive, commandValid);
            rotor.Step(Step, shaftAngle, omega, demand, commandValid && actuatorCurrent > .035f);
            Vector2 raw = rotor.Position * 1000;
            probeX.Read(Step, clock, raw.x, xPower, routed[13]);
            probeY.Read(Step, clock, raw.y, yPower, routed[14]);

            if (!float.IsFinite(raw.sqrMagnitude) || !float.IsFinite(omega))
            {
                Trip("INVALID SIMULATION STATE");
                return;
            }
            if (!running)
            {
                if (run == Run.Stopping && Stopped)
                {
                    run = Run.Idle;
                    targetRpm = 45;
                    Award();
                }
                return;
            }
            if (motor.IsOverCurrent || source.IsOverloaded || ActualRpm > 81)
                Trip("MOTOR OVERCURRENT OR OVERSPEED");
            else if (!validEncoder || ActualRpm > 36 && encoder.TimedOut)
                Trip("ENCODER PHASE INVALID");
            else if (!probeX.Valid || !probeY.Valid)
                Trip("DISPLACEMENT PROBE SIGNAL OUT OF RANGE");
            else if (raw.magnitude > SafeDisplacement)
                Trip("DISPLACEMENT LIMIT EXCEEDED");
            else if (rotor.Limited)
                Trip("DAMPER FORCE LIMIT EXCEEDED");
            if (!running) return;

            Observe();
            if (run is Run.BaselineSweep or Run.ComparisonSweep) AdvanceSweep();
            if (run is Run.Crossing or Run.AtSpeed or Run.Coasting) AdvanceCrossing(raw.magnitude);
        }

        private void Observe()
        {
            if (!Stable || !Calibrated || !probeX.Valid || !probeY.Valid)
            {
                ResetMeasurement();
                return;
            }
            if (encoder.Index)
            {
                if (measuring)
                {
                    readout = measurement.Result(clock);
                    revolutions++;
                }
                measuring = true;
                measurement.Clear();
            }
            if (measuring) measurement.Sample(Step, ActualRpm, Displacement, encoder.Phase);
        }

        private void ResetMeasurement()
        {
            measuring = false;
            revolutions = 0;
            measurement.Clear();
        }

        private void StartSweepPoint()
        {
            targetRpm = SweepSpeeds[sweepIndex];
            ResetMeasurement();
        }

        private void AdvanceSweep()
        {
            operationAge += Step;
            if (operationAge > 78) { Trip("SWEEP DID NOT STABILIZE"); return; }
            if (revolutions < 4 || !Stable) return;
            bool baselineRun = run == Run.BaselineSweep;
            var results = baselineRun ? baselineSweep : dampedSweep;
            results[sweepIndex++] = readout;
            if (sweepIndex < SweepSpeeds.Length)
            {
                StartSweepPoint();
                return;
            }

            run = Run.Idle;
            targetRpm = 45;
            ResetMeasurement();
            int peak = 0;
            for (int i = 1; i < results.Length; i++)
                if (results[i].RadialRms > results[peak].RadialRms) peak = i;
            if (baselineRun)
            {
                baselinePeak = results[peak].RadialRms;
                criticalRpm = results[peak].Rpm;
                phaseShift = Mathf.Abs(Mathf.DeltaAngle(results[0].Phase, results[results.Length-1].Phase));
                if (peak == 0 || peak == results.Length - 1 || baselinePeak < reference.RadialRms * 2 ||
                    phaseShift < 65 || Mathf.Abs(criticalRpm - SparkLevel54Rotor.NaturalRpm) > 5)
                {
                    notice = "Peak or phase evidence insufficient. Repeat the low-damping sweep.";
                    return;
                }
                Award();
                return;
            }

            dampedPeak = results[peak].RadialRms;
            comparisonReady = true;
            for (int i = 0; i < results.Length; i++)
                if (Mathf.Abs(results[i].Rpm - baselineSweep[i].Rpm) > 1) comparisonReady = false;
            if (!comparisonReady) notice = "Sweep speeds differed. Repeat at the same test points.";
        }

        private void AdvanceCrossing(float displacement)
        {
            operationAge += Step;
            if (operationAge > 22) { Trip("CROSSING OR COAST DID NOT STABILIZE"); return; }
            crossingMaximum = Mathf.Max(crossingMaximum, displacement);
            if (displacement > SafeCrossingDisplacement || !DamperEnabled || actuatorCurrent < .035f)
            {
                Trip("CROSSING EXCEEDED SAFE DISPLACEMENT OR LOST DAMPING");
                return;
            }
            if (ActualRpm > criticalRpm + 4) crossedUp = true;
            if (run == Run.Crossing)
            {
                hold = Stable && crossedUp ? hold + Step : 0;
                if (hold < 2.4f) return;
                if (stage == 5 && recovered)
                {
                    run = Run.Idle;
                    Award();
                }
                else
                {
                    run = Run.AtSpeed;
                    operationAge = 0;
                }
            }
            else if (run == Run.AtSpeed)
            {
                operationAge = 0;
            }
            else if (crossedUp && ActualRpm < criticalRpm - 4)
            {
                crossedDown = true;
                if (ActualRpm > 36) return;
                StopMachine();
                run = Run.Stopping;
            }
        }

        private void Award()
        {
            if (stage >= Gates.Length || !bindings.SetRuntimeComponentBinding(Gates[stage], source)) return;
            stage++;
            notice = null;
            manager.QueueEvaluation();
        }

        private string Prompt()
        {
            if (fault != null)
                return $"SAFE STOP · {fault} · " + (CanReset ? "RESET FAULT" : wiring ?? "Wait for the rotor to settle.");
            if (wiring != null) return wiring;
            if (!safe) return "Close the guard and check W22.";
            if (!ready) return "Turn on POWER and verify the solved 5 V rails and returns.";
            if (run is Run.BaselineSweep or Run.ComparisonSweep)
                return $"SPEED SWEEP · POINT {sweepIndex + 1}/{SweepSpeeds.Length} · {targetRpm} RPM · {revolutions}/4 valid turns";
            if (run == Run.Crossing) return "Guarded crossing in progress. Hold 75 RPM.";
            if (run == Run.AtSpeed) return "75 RPM stable. Select COAST DOWN.";
            if (run is Run.Coasting or Run.Stopping) return "Controlled coast-down. Wait for the shaft to settle.";
            if (stage == 6) return "CERTIFIED · Passed the final crossing after Y sensor recovery.";
            if (!Calibrated) return "At rest, select CALIBRATE X/Y.";
            if (armed) return AtReference ? "Undo W15 now to test Y-sensor shutdown." : notice ?? "Wait for 45 RPM and encoder Z.";
            if (stage == 5 && !recovered) return "Select SENSOR FAULT TEST to prepare the guarded 45 RPM test.";
            if (!running) return "Press START to acquire encoder Z.";
            if (notice != null) return notice;
            return stage switch
            {
                1 => "At 45 RPM, wait for three valid turns, then select BASELINE.",
                2 => "Select SPEED SWEEP to measure the low-damping peak and phase.",
                3 when comparisonReady => "Return to 45 RPM and select VERIFY DAMPING.",
                3 => "Select a damping gain and repeat SPEED SWEEP. Compare to the baseline peak.",
                4 or 5 => "At stable 45 RPM, select CROSS CRITICAL.",
                _ => "Waiting for the electrical solver."
            };
        }

        private void Present()
        {
            statusText.text = Prompt();
            string samples = probeX.Valid && probeY.Valid && Calibrated
                ? $"X {probeX.Filtered:+0.00;-0.00;0.00}  Y {probeY.Filtered:+0.00;-0.00;0.00} mm"
                : $"X {(probeX.Valid ? "READY" : "LOST")}  Y {(probeY.Valid ? "READY" : "LOST")}";
            meterText.text = $"{ActualRpm:F1}/{targetRpm} RPM   {motorVoltage:F2} V / {motorCurrent * 1000:F0} mA   PWM {pwmDuty:P0}   {samples}";
            detailText.text = $"DAMP {(fault == null ? gain.ToString().ToUpperInvariant() : "FAULT")}  {rotor.DamperForce.magnitude * 1000:F1} mN   |   Z {(encoder.Locked ? "LOCK" : "—")}   |   GUARD {(safe ? "CLOSED" : "OPEN")}   |   {manager.SatisfiedTargetCount}/6";
            calibrationText.text = baselinePeak > 0
                ? $"PEAK {baselinePeak:F2} mm RMS AT {criticalRpm:F1} RPM  •  PHASE SHIFT {phaseShift:F0}°  •  DAMPED {(dampedPeak > 0 ? $"{dampedPeak:F2} mm / {Reduction:P0} LOWER" : "—")}"
                : $"X/Y RMS {readout.Xrms:F2}/{readout.Yrms:F2} mm  •  X PHASE {readout.Phase:F0}°  •  NATURAL SPEED {SparkLevel54Rotor.NaturalRpm:F1} RPM";
            if (controllerDisplay) controllerDisplay.text = $"{ActualRpm:F1} RPM  {motorCurrent * 1000:F0} mA\n{(fault != null ? "SAFE STOP" : running ? "RUN" : "READY")}";
            if (qualityDisplay) qualityDisplay.text = $"{samples}\nRMS {readout.RadialRms:F2} mm  φ {readout.Phase:F0}°";
            plot?.Show(probeX.History, probeY.History, probeX.Cursor, probeX.Valid && probeY.Valid && Calibrated,
                baselineSweep, dampedSweep);
            var orbit = rotor.Position * 1000;
            rig.ApplyState(new SparkLevel54VisualRig.VisualState
            {
                ShaftAngleDegrees = shaftAngle * Mathf.Rad2Deg,
                ShaftOrbitMillimeters = orbit,
                LeftBearingMillimeters = orbit * .32f,
                RightBearingMillimeters = orbit * .45f,
                DamperGain01 = rotor.ExtraDamping / 1.2f,
                DamperForceMillinewtons = rotor.DamperForce * 1000,
                SensorsCalibrated = Calibrated,
                XSensorValid = probeX.Valid,
                YSensorValid = probeY.Valid,
                DamperEnabled = DamperEnabled,
                SafetyGuardOpen = guardOpen,
                CriticalFault = fault != null
            });

            powerButton.interactable = true;
            guardButton.interactable = Stopped;
            guardButton.GetComponentInChildren<Text>().text = guardOpen ? "CLOSE GUARD" : "OPEN GUARD";
            calibrateButton.interactable = CanCalibrate && !Calibrated;
            startButton.interactable = CanStart && (stage != 5 || armed || recovered);
            stopButton.interactable = running;
            slowButton.interactable = run == Run.Idle && !armed && targetRpm != 45;
            criticalButton.interactable = stage == 6 && run == Run.Idle && targetRpm != 60;
            fastButton.interactable = stage == 6 && run == Run.Idle && targetRpm != 75;
            baselineButton.interactable = stage == 1 && AtReference && revolutions >= 3;
            sweepButton.interactable = CanSweep;
            offButton.interactable = CanTune && gain != Gain.Off && stage == 3;
            lowButton.interactable = CanTune && gain != Gain.Low && stage == 3;
            mediumButton.interactable = CanTune && gain != Gain.Medium && stage == 3;
            highButton.interactable = CanTune && gain != Gain.High && stage == 3;
            autoButton.interactable = CanTune && gain != Gain.Auto && stage == 3;
            verifyButton.interactable = stage == 3 && comparisonReady && AtReference;
            crossButton.interactable = CanCross && (stage != 5 || recovered);
            coastButton.interactable = run == Run.AtSpeed && stage == 4;
            phaseButton.interactable = stage == 5 && Stopped && !armed && !recovered && fault == null && ready && safe;
            resetButton.interactable = CanReset;
        }

        private void OnDisable()
        {
            if (drive && damperDrive && gameObject.activeInHierarchy) StopMachine();
            if (bindings != null)
                foreach (string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            bindings = null;
            stage = 0;
        }
    }
}

