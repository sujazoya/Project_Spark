using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel31SealIn : MonoBehaviour
    {
        public static readonly string[] Gates = {
            "L31_PowerOnIdle", "L31_StartLatched", "L31_StopReleased", "L31_PowerLoss",
            "L31_NoAutoRestart", "L31_DeliberateRestart"
        };

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkSwitch main, start, stop, runContact;
        public SparkRelay relay;
        public SparkMotor motor;
        public Transform shaft, relayArm;
        public Transform[] beltSlats;
        public Light runLight;
        public Text statusText;
        public Image mainIndicator;

        public int Stage { get; private set; }
        public string Status { get; private set; }
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalSolveResult checkedResult;
        private int topology = -1;
        private string wiringProblem;
        private bool ready, sawButtonAction, restarted;
        private float holdTime, beltOffset;

        private bool Configured => layout != null && source != null && main != null && start != null &&
            stop != null && runContact != null && relay != null && motor != null;
        private bool Active => loader != null && loader.ActiveLayout == layout && layout.LevelDefinition?.LevelId == "level_31";

        public void ToggleMain()
        {
            if (!Configured || !Active) return;
            if (main.IsClosed) SetContact(main, false);
            else if (CanControl()) SetContact(main, true);
        }

        public void SetStartPressed(bool pressed)
        {
            if (start == null || (pressed && !CanControl())) return;
            SetContact(start, pressed);
            // Switches request a synchronous solve. Preserve coil pickup even on a short click.
            if (pressed && (Stage == 1 || Stage == 3 || Stage == 5) && main.IsClosed && stop.IsClosed && relay.IsEnergized)
                sawButtonAction = true;
        }

        public void SetStopPressed(bool pressed)
        {
            if (stop == null) return;
            SetContact(stop, !pressed);
            if (pressed && Configured && Stage == 2 && main.IsClosed && start.IsOpen && !relay.IsEnergized)
                sawButtonAction = true;
        }

        private bool CanControl() => Configured && Active && ready && circuit != null && topology == circuit.TopologyVersion;

        private void Update()
        {
            if (!Configured) { SetStatus("Level 31 setup is missing a part reference."); return; }
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (!Active) return;
            if (manager == null) manager = FindFirstObjectByType<LevelGamePlayManager>();
            if (solver == null) solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (circuit == null) circuit = FindFirstObjectByType<SparkCircuitSystem>();
            if (solver == null || circuit == null) { SetStatus("Waiting for the circuit and solver."); return; }

            if (bindings != loader.ActiveBindings || topology != circuit.TopologyVersion)
            {
                ClearGates();
                bindings = loader.ActiveBindings;
                topology = circuit.TopologyVersion;
                ResetTrial();
            }
            if (bindings == null || Array.Exists(Gates, id => !bindings.IsComponentIdDeclared(id)))
            { SetStatus("Declare the six L31 objective IDs on the Level 31 layout."); return; }

            if (!Available(source) || !Available(main) || !Available(start) || !Available(stop) ||
                !Available(runContact) || !Available(relay) || !Available(motor))
            {
                if (ready || Stage > 0) ResetTrial();
                SetStatus("Enable every Level 31 electrical component.");
                return;
            }
            if (solver.SolverRegistry == null || !solver.SolverRegistry.TryGetDefinition(relay, out _) ||
                !solver.SolverRegistry.TryGetDefinition(motor, out _))
            { SetStatus("Register the relay and reversible motor definitions in the active solver registry."); return; }

            var result = solver.LatestSolveResult;
            if (result == null || solver.IsDirty || solver.IsSolving)
            {
                if (!solver.IsSolving) solver.SolveNow();
                return;
            }
            if (!ReferenceEquals(result, checkedResult))
            {
                checkedResult = result;
                wiringProblem = CheckWiring(result.Context.Graph);
            }
            if (wiringProblem != null)
            {
                if (ready || Stage > 0) ResetTrial();
                SetStatus(wiringProblem);
                return;
            }
            float supply = source.ElectricalState.Voltage;
            if (!source.IsOutputActive || !(supply >= 4.8f && supply <= 5.2f) || source.IsOverloaded || motor.IsOverCurrent)
            {
                if (ready || Stage > 0) ResetTrial();
                SetStatus("Check the 5 V supply and motor current before restarting.");
                return;
            }
            ready = true;

            // The solver stamps the relay's holding contact. The motor contact is its second pole.
            if (runContact.IsClosed != relay.IsEnergized)
            {
                SetContact(runContact, relay.IsEnergized);
                return; // Verify the next solution after the motor contact moves.
            }

            float dt = Time.deltaTime;
            bool running = relay.IsEnergized && runContact.IsClosed && motor.IsRunning &&
                motor.ElectricalState.Voltage >= motor.StartupVoltage && motor.ElectricalState.Current >= .02f;
            bool stopped = !relay.IsEnergized && runContact.IsOpen &&
                Mathf.Abs(relay.ElectricalState.Voltage) < .25f && Mathf.Abs(motor.ElectricalState.Current) <= .001f;
            UpdateVisuals(running, dt);
            Observe(dt, running, stopped);
        }

        private void Observe(float dt, bool running, bool stopped)
        {
            bool released = start.IsOpen && stop.IsClosed;
            bool latched = main.IsClosed && released && running;
            bool pass;
            switch (Stage)
            {
                case 0:
                    SetStatus("Turn MAIN on. The conveyor should remain stopped.");
                    pass = main.IsClosed && released && stopped;
                    break;
                case 1:
                    sawButtonAction |= main.IsClosed && start.IsClosed && stop.IsClosed && running;
                    SetStatus(sawButtonAction ? "Release START. Check that the motor keeps running." : "Press START, then release it.");
                    pass = sawButtonAction && latched;
                    break;
                case 2:
                    sawButtonAction |= main.IsClosed && stop.IsOpen && start.IsOpen && stopped;
                    SetStatus(sawButtonAction ? "Release STOP. Check that the motor stays stopped." : "Press and release STOP.");
                    pass = sawButtonAction && main.IsClosed && released && stopped;
                    break;
                case 3:
                    sawButtonAction |= main.IsClosed && start.IsClosed && stop.IsClosed && running;
                    restarted |= sawButtonAction && latched;
                    SetStatus(restarted ? "Turn MAIN off. Wait for the relay and motor to drop out." : "Restart with START, release it, then turn MAIN off.");
                    pass = restarted && main.IsOpen && released && stopped;
                    break;
                case 4:
                    if (main.IsClosed && (!released || !stopped))
                    {
                        ResetTrial();
                        SetStatus("Restore MAIN with both buttons released. Repeat the test from power-on.");
                        return;
                    }
                    SetStatus("Restore MAIN with START released. The motor must stay stopped.");
                    pass = main.IsClosed && released && stopped;
                    break;
                case 5:
                    sawButtonAction |= main.IsClosed && start.IsClosed && stop.IsClosed && running;
                    SetStatus(sawButtonAction ? "Release START to verify the deliberate restart." : "Safe restart verified. Press and release START again.");
                    pass = sawButtonAction && latched;
                    break;
                default:
                    SetStatus("Test complete. START holds, STOP drops out, and MAIN restoration needs a new START command.");
                    return;
            }

            holdTime = pass ? holdTime + dt : 0f;
            if (!pass || holdTime < .20f) return;
            if (!bindings.SetRuntimeComponentBinding(Gates[Stage], source)) return;
            Stage++;
            holdTime = 0f;
            sawButtonAction = false;
            QueueEvaluation();
        }

        private void ResetTrial()
        {
            ClearGates();
            ready = restarted = sawButtonAction = false;
            Stage = 0;
            holdTime = 0f;
            checkedResult = null;
            SetContact(main, false);
            SetContact(runContact, false);
            SetContact(start, false);
            SetContact(stop, true);
            UpdateVisuals(false, 0f);
        }

        private void ClearGates()
        {
            if (bindings == null) return;
            foreach (string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            if (Stage > 0) QueueEvaluation();
        }

        private void QueueEvaluation()
        {
            if (manager != null && manager.SceneBindings == bindings) manager.QueueEvaluation();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus || !Configured || !Active) return;
            SetStartPressed(false);
            SetStopPressed(false);
        }

        private void OnDisable()
        {
            if (Configured && gameObject.activeInHierarchy) ResetTrial();
            ClearGates();
            bindings = null;
            if (runLight != null) runLight.enabled = false;
        }

        private static bool Available(SparkElectricalComponent component) => component.isActiveAndEnabled && component.ElectricalEnabled;
        private static void SetContact(SparkSwitch contact, bool closed)
        {
            if (contact != null && contact.IsClosed != closed)
                contact.SetState(closed ? SparkSwitchState.Closed : SparkSwitchState.Open);
        }

        private void SetStatus(string text)
        {
            if (Status == text) return;
            Status = text;
            if (statusText != null) statusText.text = text;
        }

        private void UpdateVisuals(bool running, float dt)
        {
            if (runLight != null) runLight.enabled = running;
            if (relayArm != null) relayArm.localRotation = Quaternion.Euler(0, 0, relay.IsEnergized ? 0 : 24);
            if (mainIndicator != null) mainIndicator.color = main.IsClosed ? new Color(.12f,.56f,.35f) : new Color(.28f,.33f,.38f);
            if (!running) return;
            if (shaft != null) shaft.Rotate(Vector3.right, motor.ElectricalState.Voltage * 160f * dt, Space.Self);
            if (beltSlats == null || beltSlats.Length == 0) return;
            beltOffset = Mathf.Repeat(beltOffset + dt * .24f, .52f);
            for (int i = 0; i < beltSlats.Length; i++)
            {
                if (beltSlats[i] == null) continue;
                Vector3 p = beltSlats[i].localPosition;
                p.x = Mathf.Repeat(i * .52f / beltSlats.Length + beltOffset, .52f) - .26f;
                beltSlats[i].localPosition = p;
            }
        }

        private string CheckWiring(SparkElectricalNetworkGraph graph)
        {
            if (graph == null) return "Waiting for the solved circuit graph.";
            var wires = new Dictionary<int, int>();
            int Root(int node)
            {
                while (wires.TryGetValue(node, out int parent)) node = parent;
                return node;
            }
            foreach (var entry in graph.ComponentTerminals)
            {
                if (!(entry.Key is SparkSingleWire wire) || !Available(wire) ||
                    !graph.TryGetPrimaryComponentTerminals(wire, out var a, out var b) ||
                    !graph.TryGetNode(a, out int aNode) || !graph.TryGetNode(b, out int bNode)) continue;
                int aRoot = Root(aNode), bRoot = Root(bNode);
                if (aRoot != bRoot) wires[bRoot] = aRoot;
            }

            var nets = new[] {
                new[] { source.PositiveTerminal, main.InputTerminal },
                new[] { main.OutputTerminal, stop.InputTerminal, runContact.InputTerminal },
                new[] { stop.OutputTerminal, start.InputTerminal, relay.CommonTerminal },
                new[] { start.OutputTerminal, relay.CoilPositiveTerminal, relay.NormallyOpenTerminal },
                new[] { source.NegativeTerminal, relay.CoilNegativeTerminal, motor.NegativeTerminal },
                new[] { runContact.OutputTerminal, motor.PositiveTerminal },
                new[] { relay.NormallyClosedTerminal }
            };
            var used = new Dictionary<int, string>();
            foreach (var net in nets)
            {
                var first = net[0];
                if (first == null || !graph.TryGetNode(first, out int firstNode))
                    return "A required terminal is missing from the solved graph.";
                int root = Root(firstNode);
                if (used.TryGetValue(root, out string other))
                    return $"Separate {first.name} from {other}. Check for a bypass wire.";
                used.Add(root, first.name);
                foreach (var pin in net)
                {
                    if (pin == null || !graph.TryGetNode(pin, out int node))
                        return $"A terminal on the {first.name} path is missing from the graph.";
                    if (Root(node) != root) return $"Connect {first.name} to {pin.name}. See the wiring list.";
                }
            }
            return null;
        }
    }
}
