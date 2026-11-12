using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel30Lift : MonoBehaviour
    {
        public static readonly string[] Gates = { "L30_BottomInterlock", "L30_Rise", "L30_TopStopLamp", "L30_Descend", "L30_BottomStop" };

        [SerializeField] private SparkLevelLayout layout;
        [SerializeField] private SparkPowerSupply source;
        [SerializeField] private SparkMotor motor;
        [SerializeField] private SparkSwitch upFeed, upReturn, downFeed, downReturn;
        [SerializeField] private SparkSwitch upperLimit, lowerLimit, lampContact;
        [SerializeField] private SparkResistor lampResistor;
        [SerializeField] private SparkLED lamp;
        [SerializeField] private Transform platform;
        [SerializeField] private Light lampLight;
        [SerializeField] private Text statusText;
        [SerializeField, Min(0.1f)] private float travelHeight = 0.55f;
        [SerializeField, Min(0.5f)] private float travelSeconds = 3f;

        private enum Direction { Off, Up, Down }
        private Direction direction;
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolveResult checkedResult;
        private SparkSwitch[] contacts;
        private Vector3 platformBottom;
        private int topologyVersion = -1;
        private float position, hold;
        private bool configured, graphReady;
        private string wiringProblem;
        public int Stage { get; private set; }
        public string Status { get; private set; }

        private void Awake()
        {
            contacts = new[] { upFeed, upReturn, downFeed, downReturn, upperLimit, lowerLimit, lampContact };
            configured = layout != null && source != null && motor != null && lampResistor != null &&
                lamp != null && platform != null && Array.TrueForAll(contacts, contact => contact != null) &&
                new HashSet<SparkSwitch>(contacts).Count == contacts.Length;
            if (platform != null) platformBottom = platform.localPosition;
        }

        public void SelectUp() => Select(Direction.Up);
        public void SelectOff() => Select(Direction.Off);
        public void SelectDown() => Select(Direction.Down);

        private void Select(Direction command)
        {
            if (!configured || (command != Direction.Off &&
                (!graphReady || wiringProblem != null || circuit == null || topologyVersion != circuit.TopologyVersion))) return;
            if (Stage == 0 && command == Direction.Up)
            {
                SetStatus("Test DOWN at the bottom first. The lower limit should block it.");
                return;
            }
            if (direction == command) return;
            direction = command;
            hold = 0f;
            ApplyContacts();
        }

        private void Update()
        {
            if (!configured) { SetStatus("Level 30 setup is incomplete. Check the lift references."); return; }
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (loader == null || loader.ActiveLayout != layout || layout.LevelDefinition?.LevelId != "level_30") return;
            if (manager == null) manager = FindFirstObjectByType<LevelGamePlayManager>();
            if (solver == null) solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (circuit == null) circuit = FindFirstObjectByType<SparkCircuitSystem>();
            if (solver == null || circuit == null) { SetStatus("Level 30 needs the active solver and circuit system."); return; }
            if (solver.SolverRegistry == null || !solver.SolverRegistry.TryGetDefinition(motor, out _))
            {
                SetStatus("Register Level30_MotorSolver in the active electrical solver registry.");
                return;
            }

            if (bindings != loader.ActiveBindings || topologyVersion != circuit.TopologyVersion)
            {
                ClearGates();
                bindings = loader.ActiveBindings;
                topologyVersion = circuit.TopologyVersion;
                ResetTrial();
            }
            if (bindings == null || Array.Exists(Gates, id => !bindings.IsComponentIdDeclared(id)))
            {
                SetStatus("Declare the five Level 30 runtime objective IDs in the layout bindings.");
                return;
            }

            var result = solver.LatestSolveResult;
            if (result == null || solver.IsDirty || solver.IsSolving)
            {
                if (!solver.IsSolving) solver.SolveNow();
                return;
            }
            if (!ReferenceEquals(checkedResult, result))
            {
                checkedResult = result;
                wiringProblem = CheckWiring(result.Context.Graph);
                graphReady = true;
            }
            if (wiringProblem != null)
            {
                if (Stage != 0 || direction != Direction.Off) ResetTrial();
                SetStatus(wiringProblem);
                return;
            }
            if (motor.IsOverCurrent || source.IsOverloaded || lampResistor.IsBurned || lamp.IsOverCurrent)
            {
                ResetTrial();
                SetStatus("Electrical fault. Check the motor, LED resistor, and supply before restarting.");
                return;
            }
            if (!source.IsOutputActive || Mathf.Abs(source.ElectricalState.Voltage - 5f) > 0.2f)
            {
                ResetTrial();
                SetStatus("Enable the 5 V supply to begin the test.");
                return;
            }

            float dt = Time.deltaTime;
            bool lampLit = lamp.IsOn && Mathf.Abs(lamp.ElectricalState.Current) >= .002f;
            if (lampLight != null) lampLight.enabled = lampLit;
            float motorVoltage = motor.ElectricalState.Voltage;
            bool powered = motor.IsRunning && Mathf.Abs(motor.ElectricalState.Current) >= .02f;
            bool rising = powered && motorVoltage > 0 && direction == Direction.Up;
            bool descending = powered && motorVoltage < 0 && direction == Direction.Down;
            float previousPosition = position;
            if (rising || descending)
                position = Mathf.Clamp01(position + (rising ? 1f : -1f) * dt / travelSeconds);
            platform.localPosition = platformBottom + Vector3.up * (position * travelHeight);
            if (AtLimit(previousPosition) != AtLimit(position))
            {
                ApplyContacts();
                return; // Use the next solved sample to verify contact changes.
            }
            Observe(dt, rising, descending, lampLit);
        }

        private void Observe(float dt, bool rising, bool descending, bool lampLit)
        {
            bool stopped = Mathf.Abs(motor.ElectricalState.Current) <= .001f &&
                Mathf.Abs(motor.ElectricalState.Voltage) < motor.StartupVoltage;
            bool lampOff = !lamp.IsOn && Mathf.Abs(lamp.ElectricalState.Current) <= .0001f;
            bool passed;
            string message;
            switch (Stage)
            {
                case 0:
                    message = "Select DOWN at the bottom to test the lower limit.";
                    passed = position <= 0f && direction == Direction.Down && lowerLimit.IsOpen && stopped && lampOff;
                    break;
                case 1:
                    message = "Lower limit passed. Select UP to raise the platform.";
                    passed = rising && position > 0f && position < 1f && lampOff;
                    break;
                case 2:
                    message = "Keep UP selected. At the top, the motor must stop and the lamp must light.";
                    passed = position >= 1f && direction == Direction.Up && upperLimit.IsOpen && stopped && lampLit;
                    break;
                case 3:
                    message = "Upper limit passed. Select DOWN and check that the lamp goes out.";
                    passed = descending && position < 1f && position > 0f && lampOff;
                    break;
                case 4:
                    message = "Keep DOWN selected until the lower limit stops the motor.";
                    passed = position <= 0f && direction == Direction.Down && lowerLimit.IsOpen && stopped && lampOff;
                    break;
                default:
                    SetStatus("Round trip verified. Motor stopped and lamp off at the bottom.");
                    return;
            }

            SetStatus(message);
            hold = passed ? hold + dt : 0f;
            if (!passed || ((Stage == 0 || Stage == 2 || Stage == 4) && hold < 0.15f)) return;
            if (!bindings.SetRuntimeComponentBinding(Gates[Stage], source))
            {
                SetStatus("Could not record the Level 30 objective. Check its runtime binding.");
                return;
            }
            Stage++;
            hold = 0f;
            QueueEvaluation();
        }

        private static int AtLimit(float value) => value <= 0f ? -1 : value >= 1f ? 1 : 0;

        private void ResetTrial()
        {
            ClearGates();
            Stage = 0;
            hold = position = 0f;
            direction = Direction.Off;
            graphReady = false;
            checkedResult = null;
            platform.localPosition = platformBottom;
            if (lampLight != null) lampLight.enabled = false;
            ApplyContacts();
        }

        private void ApplyContacts()
        {
            var desired = new[] { direction == Direction.Up, direction == Direction.Up,
                direction == Direction.Down, direction == Direction.Down, position < 1f, position > 0f, position >= 1f };
            // Open old contacts first. SparkSwitch can trigger a solve on every contact change.
            for (int i = 0; i < contacts.Length; i++)
                if (!desired[i] && contacts[i].IsClosed) contacts[i].SetState(SparkSwitchState.Open);
            for (int i = 0; i < contacts.Length; i++)
                if (desired[i] && contacts[i].IsOpen) contacts[i].SetState(SparkSwitchState.Closed);
        }

        private void OnDisable()
        {
            ClearGates();
            bindings = null;
            graphReady = false;
            if (configured && gameObject.activeInHierarchy)
            {
                direction = Direction.Off;
                ApplyContacts();
            }
            if (lampLight != null) lampLight.enabled = false;
        }

        private void ClearGates()
        {
            if (bindings == null) return;
            foreach (string id in Gates) bindings.ClearRuntimeComponentBinding(id);
            if (Stage > 0) QueueEvaluation();
        }

        private void QueueEvaluation()
        {
            if (manager != null && manager.SceneBindings == bindings) manager.QueueEvaluation();
        }

        private void SetStatus(string text)
        {
            if (Status == text) return;
            Status = text;
            if (statusText != null) statusText.text = text;
        }

        private string CheckWiring(SparkElectricalNetworkGraph graph)
        {
            if (graph == null) return "Waiting for the electrical graph.";
            var wires = new Dictionary<int, int>();
            int Root(int node)
            {
                while (wires.TryGetValue(node, out int parent)) node = parent;
                return node;
            }
            foreach (var entry in graph.ComponentTerminals)
            {
                if (!(entry.Key is SparkSingleWire wire) || !wire.isActiveAndEnabled || !wire.ElectricalEnabled ||
                    !graph.TryGetPrimaryComponentTerminals(wire, out var a, out var b) ||
                    !graph.TryGetNode(a, out int aNode) || !graph.TryGetNode(b, out int bNode)) continue;
                int aRoot = Root(aNode), bRoot = Root(bNode);
                if (aRoot != bRoot) wires[bRoot] = aRoot;
            }

            var nets = new[] {
                new[] { source.PositiveTerminal, upFeed.InputTerminal, downFeed.InputTerminal, lampContact.InputTerminal },
                new[] { source.NegativeTerminal, upReturn.OutputTerminal, downReturn.OutputTerminal, lamp.CathodeTerminal },
                new[] { upFeed.OutputTerminal, upperLimit.InputTerminal },
                new[] { downFeed.OutputTerminal, lowerLimit.InputTerminal },
                new[] { upperLimit.OutputTerminal, motor.PositiveTerminal, downReturn.InputTerminal },
                new[] { lowerLimit.OutputTerminal, motor.NegativeTerminal, upReturn.InputTerminal },
                new[] { lampContact.OutputTerminal, lampResistor.TerminalA },
                new[] { lampResistor.TerminalB, lamp.AnodeTerminal }
            };
            var used = new Dictionary<int, string>();
            foreach (var net in nets)
            {
                var first = net[0];
                if (first == null || !graph.TryGetNode(first, out int firstNode))
                    return "A required terminal is missing from the solved graph.";
                int root = Root(firstNode);
                if (used.TryGetValue(root, out string other))
                    return $"Separate {first.name} from {other}. These paths must be isolated.";
                used.Add(root, first.name);
                foreach (var pin in net)
                {
                    if (pin == null || !graph.TryGetNode(pin, out int node))
                        return $"A terminal on the {first.name} path is missing from the graph.";
                    if (Root(node) != root) return $"Connect {first.name} to {pin.name}. See the wire list.";
                }
            }
            return null;
        }
    }
}
