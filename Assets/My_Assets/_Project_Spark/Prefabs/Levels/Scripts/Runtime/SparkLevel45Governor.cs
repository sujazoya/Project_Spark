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
    public sealed class SparkLevel45Governor : MonoBehaviour
    {
        public static readonly string[] Gates = {
            "L45_Wired", "L45_Encoder", "L45_30RPM", "L45_60RPM", "L45_LoadRecovery", "L45_FeedbackRecovery"
        };
        public static readonly (int from, int to)[] Wiring = {
            (0,2), (1,3), (0,4), (1,5), (6,7), (8,9), (10,11), (12,13), (14,15)
        };
        public const int CountsPerRevolution = 80;
        private static readonly int[] Quadrature = {0, 2, 3, 1};
        private static readonly int[] Transitions = {0,-1,1,0, 1,0,0,-1, -1,0,0,1, 0,1,-1,0};
        private static readonly float[] BrakeTorque = {0f, .004f, .0085f, .046f};
        private static readonly string[] BrakeNames = {"RELEASED", "LIGHT", "TEST LOAD", "OVERLOAD"};
        private enum Mode { Off, Open, Closed, Fault }

        public SparkLevelLayout layout;
        public SparkPowerSupply source, encoderA, encoderB, pwm;
        public SparkMotor motor;
        public SparkSwitch driveEnable;
        public SparkResistor driveImpedance;
        public SparkTerminal[] pins;
        public SparkLevel45VisualRig rig;
        public Text statusText, meterText, detailText;
        public Button openButton, closedButton, startButton, rpm30Button, rpm60Button, loadButton, resetButton, restartButton;
        public TMP_Text monitorReadout, controllerReadout;
        public GameObject[] lamps;

        [Header("Motor and governor tuning")]
        [SerializeField, Min(.0001f)] private float inertia = .0026f;
        [SerializeField, Min(0)] private float torqueConstant = .23f;
        [SerializeField, Min(0)] private float backEmfConstant = .25f;
        [SerializeField, Min(0)] private float viscousFriction = .0022f;
        [SerializeField, Range(0, 1)] private float maximumDuty = .98f;
        [SerializeField, Min(0)] private float proportionalGain = .009f;
        [SerializeField, Min(0)] private float integralGain = .027f;
        [SerializeField, Range(.2f, 1f)] private float measurementInterval = .5f;
        [SerializeField, Range(.1f, 1f)] private float measurementFilter = .65f;
        [SerializeField, Min(65f)] private float overspeedRpm = 90f;

        private readonly List<SparkCircuitConnection> connections = new();
        private readonly Dictionary<SparkTerminal, SparkTerminal> parent = new();
        private LevelGamePlayManager manager;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings bindings;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private Mode mode, selected = Mode.Open;
        private int topology = -1, stage, target = 30, load, count, shaftTicks, previous, transitions, sampleCount;
        private float shaftAngle, omega, measured, rpm, duty, applied, integral, timer, sampleTime, controlTime, hold;
        private float edgeTime, stalledFor, baselineDuty, baselineRpm;
        private bool ready, missingB, observedDrop, observedDuty, feedbackFault, resetAfterFeedback;
        private string wiring, fault;
        private const float RadToRpm = 60f / (2f * Mathf.PI);
        private const float RadiansPerEdge = 2f * Mathf.PI / CountsPerRevolution;

        private void Update()
        {
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (loader == null || loader.ActiveLayout != layout) return;
            if (manager == null) manager = FindFirstObjectByType<LevelGamePlayManager>();
            if (manager == null) return;
            circuit = manager.CircuitSystem;
            solver = manager.ElectricalSolver;
            if (circuit == null || solver == null) return;
            if (bindings != loader.ActiveBindings)
            {
                bindings = loader.ActiveBindings;
                if (bindings == null) return;
                Restart();
            }
            if (topology != circuit.TopologyVersion)
            {
                topology = circuit.TopologyVersion;
                wiring = CheckWiring();
                if (wiring != null && IsRunning)
                {
                    if (missingB) Trip("ENCODER B LOST · DRIVE DISABLED", stage == 5);
                    else Trip("CONNECTION LOST · DRIVE DISABLED");
                }
            }
            ready = wiring == null && ReadInputs(out _);
            if (stage == 0 && ready) Award();

            timer += Mathf.Min(Time.unscaledDeltaTime, .12f);
            const float step = .01f;
            while (timer >= step)
            {
                timer -= step;
                Advance(step);
            }
            Display();
        }

        private bool IsRunning => mode == Mode.Open || mode == Mode.Closed;
        private bool CanControl => ready && circuit.TopologyVersion == topology && mode != Mode.Fault;

        public void SelectOpen() => SelectMode(Mode.Open);
        public void SelectClosed() => SelectMode(Mode.Closed);
        private void SelectMode(Mode next)
        {
            if (mode == Mode.Fault) return;
            selected = next;
            if (IsRunning) mode = next;
            integral = hold = 0f;
        }

        public void Select30() => SelectTarget(30);
        public void Select60() => SelectTarget(60);
        private void SelectTarget(int value)
        {
            if (target == value) return;
            target = value;
            integral = hold = 0f;
        }

        public void StartStop()
        {
            if (IsRunning) { StopDrive(Mode.Off); return; }
            if (!CanControl || !ReadInputs(out previous)) return;
            mode = selected;
            transitions = 0;
            sampleCount = count;
            sampleTime = edgeTime = controlTime = stalledFor = hold = integral = 0f;
        }

        public void CycleBrake() => SetBrake((load + 1) % BrakeTorque.Length);
        public void ApplyTestLoad() => SetBrake(load == 2 ? 0 : 2);
        private void SetBrake(int setting)
        {
            if (setting == 2 && load != 2 && stage == 4 && mode == Mode.Closed && target == 60 && Mathf.Abs(rpm - 60f) < 3f)
            {
                baselineRpm = rpm;
                baselineDuty = duty;
                observedDrop = observedDuty = false;
            }
            else if (setting != 2) baselineRpm = 0f;
            load = setting;
            hold = 0f;
        }

        public void ResetFault()
        {
            if (mode != Mode.Fault || !ready || rpm > 4f || load == 3) return;
            resetAfterFeedback = feedbackFault && stage == 5;
            StopDrive(Mode.Off);
            fault = null;
        }

        public void Restart()
        {
            if (bindings == null) return;
            StopDrive(Mode.Off);
            foreach (var gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            selected = Mode.Open;
            stage = load = count = shaftTicks = previous = transitions = sampleCount = 0;
            target = 30;
            shaftAngle = omega = rpm = measured = timer = sampleTime = edgeTime = baselineRpm = 0f;
            feedbackFault = resetAfterFeedback = observedDrop = observedDuty = false;
            fault = null;
            topology = -1;
            SetSource(encoderA, 0f);
            SetSource(encoderB, 0f);
            manager.QueueEvaluation();
        }

        private void Advance(float dt)
        {
            controlTime += dt;
            if (IsRunning && controlTime >= .05f)
            {
                controlTime = 0f;
                if (!ready || !ReadInputs(out _)) { Trip("SUPPLY OR SIGNAL LOST · DRIVE DISABLED"); }
                else
                {
                    float feedforward = target / 84f;
                    float error = target - measured;
                    float candidate = Mathf.Clamp(integral + integralGain * error * .05f, -.28f, .65f);
                    float output = feedforward + proportionalGain * error + candidate;
                    if (output >= 0f && output <= maximumDuty || output > maximumDuty && error < 0f || output < 0f && error > 0f)
                        integral = candidate;
                    duty = Mathf.Clamp(mode == Mode.Open ? feedforward : feedforward + proportionalGain * error + integral,
                        0f, maximumDuty);
                    ApplyDrive(duty);
                }
            }

            float current = driveEnable.IsClosed ? Mathf.Abs(motor.ElectricalState.Current) : 0f;
            float torque = torqueConstant * Mathf.Max(0f, current - backEmfConstant * omega / motor.WindingResistance);
            float drag = viscousFriction * omega + BrakeTorque[load];
            omega = Mathf.Max(0f, omega + (torque - drag) * dt / inertia);
            rpm = omega * RadToRpm;
            shaftAngle += omega * dt;
            while (shaftAngle >= RadiansPerEdge)
            {
                shaftAngle -= RadiansPerEdge;
                shaftTicks = (shaftTicks + 1) % CountsPerRevolution;
                int phase = Quadrature[shaftTicks % Quadrature.Length];
                SetSource(encoderA, (phase & 2) != 0 ? 5f : 0f);
                SetSource(encoderB, (phase & 1) != 0 ? 5f : 0f);
                if (!ready || !ReadInputs(out int next)) continue;
                int delta = Transitions[previous * 4 + next];
                if (IsRunning && (delta < 0 || next != previous && delta == 0))
                {
                    Trip("INVALID ENCODER TRANSITION · DRIVE DISABLED");
                    break;
                }
                count += delta;
                previous = next;
                if (delta != 0) { transitions++; edgeTime = 0f; }
            }
            edgeTime += dt;
            sampleTime += dt;
            if (sampleTime + .00001f >= measurementInterval)
            {
                float sample = (count - sampleCount) * 60f / (CountsPerRevolution * sampleTime);
                measured = Mathf.Lerp(measured, sample, measurementFilter);
                sampleCount = count;
                sampleTime = 0f;
            }
            if (!IsRunning) return;

            if (rpm > overspeedRpm) { Trip("OVERSPEED · DRIVE DISABLED"); return; }
            if (motor.IsOverCurrent || source.IsOverloaded) { Trip("OVERCURRENT · DRIVE DISABLED"); return; }
            if (rpm > 12f && edgeTime > .22f) { Trip("ENCODER FEEDBACK STALLED · DRIVE DISABLED"); return; }
            stalledFor = duty > .30f && rpm < 5f ? stalledFor + dt : 0f;
            if (stalledFor >= 1.1f) { Trip("BRAKE OVERLOAD OR MOTOR STALL · DRIVE DISABLED"); return; }

            if (stage == 1 && transitions >= 12 && measured >= 10f && rpm >= 12f && count > 0) Award();
            if (stage == 4 && load == 2 && baselineRpm > 0f)
            {
                observedDrop |= rpm <= baselineRpm - 1.4f;
                observedDuty |= duty >= baselineDuty + .09f;
            }

            bool speedStable = mode == Mode.Closed && Mathf.Abs(rpm - target) <= 3f &&
                Mathf.Abs(measured - target) <= 3f;
            bool requested = stage == 2 && target == 30 || stage == 3 && target == 60 ||
                stage == 4 && target == 60 && load == 2 && observedDrop && observedDuty ||
                stage == 5 && target == 60 && resetAfterFeedback && transitions >= 12;
            hold = speedStable && requested ? hold + dt : 0f;
            if (hold >= 2f) Award();
        }

        private void ApplyDrive(float requestedDuty)
        {
            SetSource(pwm, requestedDuty * 5f);
            var solved = solver.LatestSolveResult;
            if (solved == null || !solved.TryGetVoltage(pins[11], pins[5], out float signal) ||
                !solved.TryGetVoltage(pins[4], pins[5], out float rail) || rail < 4.7f)
            {
                Trip("PWM OR DRIVER SUPPLY LOST · DRIVE DISABLED");
                return;
            }
            float next = Mathf.Clamp01(signal / 5f);
            if (next <= .006f) { driveEnable.SetState(SparkSwitchState.Open); applied = 0f; return; }
            if (Mathf.Abs(next - applied) >= .004f || driveEnable.IsOpen)
            {
                // DC-equivalent drive. The actual supply, winding, and return remain in the solved path.
                driveImpedance.SetResistance(motor.WindingResistance * (1f / next - 1f));
                applied = next;
                driveEnable.SetState(SparkSwitchState.Closed);
            }
        }

        private void StopDrive(Mode next)
        {
            mode = next;
            duty = applied = integral = hold = 0f;
            if (driveEnable != null) driveEnable.SetState(SparkSwitchState.Open);
            if (pwm != null && gameObject.activeInHierarchy) SetSource(pwm, 0f);
        }

        private void Trip(string message, bool encoderFault = false)
        {
            if (mode == Mode.Fault) return;
            fault = message;
            feedbackFault = encoderFault;
            resetAfterFeedback = false;
            StopDrive(Mode.Fault);
            Debug.Log($"[LEVEL 45] {message}", this);
        }

        private static void SetSource(SparkPowerSupply output, float voltage)
        {
            if (Mathf.Abs(output.OutputVoltage - voltage) < .005f) return;
            output.SetOutput(false, default);
            if (!output.TryConfigure(voltage, .025f, out var reason)) Debug.LogError(reason, output);
            output.SetOutput(true, default);
        }

        private bool ReadInputs(out int phase)
        {
            phase = 0;
            if (solver.IsSolving || (solver.IsDirty && !solver.SolveNow())) return false;
            var solved = solver.LatestSolveResult;
            if (solved == null || solver.IsDirty || !source.IsOutputActive || source.IsOverloaded ||
                !solved.TryGetVoltage(pins[2], pins[3], out float supply) || supply < 4.8f ||
                !solved.TryGetVoltage(pins[7], pins[3], out float a) ||
                !solved.TryGetVoltage(pins[9], pins[3], out float b)) return false;
            phase = (a > 2.5f ? 2 : 0) | (b > 2.5f ? 1 : 0);
            return true;
        }

        private string CheckWiring()
        {
            circuit.CopyConnections(connections);
            parent.Clear();
            SparkTerminal Root(SparkTerminal pin)
            {
                while (parent.TryGetValue(pin, out var next)) pin = next;
                return pin;
            }
            foreach (var wire in connections)
            {
                if (wire.Kind != SparkConnectionKind.Wire) continue;
                var a = Root(wire.A);
                var b = Root(wire.B);
                if (a != b) parent[b] = a;
            }
            missingB = Root(pins[8]) != Root(pins[9]);
            foreach (var (from,to) in Wiring)
                if (Root(pins[from]) != Root(pins[to])) return $"Connect {pins[from].name} to {pins[to].name}.";
            var nets = new HashSet<SparkTerminal>();
            foreach (int id in new[] {0,1,6,8,10,12,14})
                if (!nets.Add(Root(pins[id]))) return "Separate the supply, encoder, PWM and motor nets.";
            return null;
        }

        private void Award()
        {
            if (!bindings.SetRuntimeComponentBinding(Gates[stage], source)) return;
            stage++;
            hold = 0f;
            manager.QueueEvaluation();
        }

        private string Prompt()
        {
            if (fault != null) return fault + (ready ? "  |  Release the brake and reset when stopped." : "  |  " + wiring);
            if (wiring != null) return wiring;
            if (!ready) return "Waiting for solved supply and encoder inputs.";
            return stage switch
            {
                0 => "Build the speed loop.",
                1 => "Start in OPEN LOOP and observe valid encoder feedback.",
                2 => "Select CLOSED LOOP and hold 30 RPM for two seconds.",
                3 => "Select 60 RPM and hold it for two seconds.",
                4 => "Apply TEST LOAD at 60 RPM. Watch speed drop, duty rise and speed recover.",
                5 => resetAfterFeedback ? "Start CLOSED LOOP at 60 RPM and verify recovery." :
                    "While running, undo W06 (encoder B). Reconnect it, then reset the fault.",
                _ => "All speed governor objectives verified."
            };
        }

        private void Display()
        {
            string modeText = mode == Mode.Off ? $"OFF / {selected.ToString().ToUpperInvariant()} READY" :
                mode == Mode.Fault ? "FAULT" : mode == Mode.Open ? "OPEN LOOP" : "CLOSED LOOP";
            statusText.text = Prompt();
            meterText.text = $"TARGET {target} RPM   |   ACTUAL {measured:0.0} RPM   |   ERROR {target-measured:+0.0;-0.0;0.0}   |   PWM {duty:P0}";
            detailText.text = $"{modeText}   •   {BrakeNames[load]}   •   A {previous >> 1}  B {(missingB ? "LOST" : (previous & 1).ToString())}   •   COUNT {count}   •   {manager.SatisfiedTargetCount}/6";
            monitorReadout.text = $"{measured:0.0} RPM\nTARGET {target}    PWM {duty:P0}";
            controllerReadout.text = $"{modeText}\nA {previous >> 1}   B {(missingB ? "—" : (previous & 1).ToString())}   COUNT {count}";
            rig.SetMeasuredRpm(rpm);
            rig.SetBrakeLoad(load / 3f);
            int phase = Quadrature[shaftTicks % Quadrature.Length];
            bool locked = IsRunning && Mathf.Abs(rpm - target) <= 3f && Mathf.Abs(measured - target) <= 3f;
            bool[] visible = {source.IsOutputActive, (phase & 2) != 0, (phase & 1) != 0, driveEnable.IsClosed, duty > .9f,
                mode == Mode.Fault, IsRunning && !locked, locked, ready, load > 0, locked, load > 0, mode == Mode.Fault};
            for (int i = 0; i < lamps.Length; i++)
                if (lamps[i].activeSelf != visible[i]) lamps[i].SetActive(visible[i]);
            openButton.interactable = closedButton.interactable = mode != Mode.Fault;
            startButton.interactable = IsRunning || CanControl;
            startButton.GetComponentInChildren<Text>().text = IsRunning ? "STOP" : "START";
            rpm30Button.interactable = rpm60Button.interactable = mode != Mode.Fault;
            loadButton.interactable = true;
            loadButton.GetComponentInChildren<Text>().text = "BRAKE: " + BrakeNames[load];
            resetButton.interactable = mode == Mode.Fault && ready && rpm <= 4f && load != 3;
            restartButton.interactable = true;
        }

        private void OnDisable()
        {
            if (gameObject.activeInHierarchy) StopDrive(Mode.Off);
            if (rig != null) rig.StopVisualMotion();
            if (bindings != null)
                foreach (var id in Gates) bindings.ClearRuntimeComponentBinding(id);
            bindings = null;
        }
    }
}
