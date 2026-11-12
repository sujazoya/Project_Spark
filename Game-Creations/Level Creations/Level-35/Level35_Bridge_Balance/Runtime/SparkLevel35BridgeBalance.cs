using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel35BridgeBalance : MonoBehaviour
    {
        public static readonly string[] Gates = {
            "L35_Wired", "L35_Baseline", "L35_LeftFault",
            "L35_RightFault", "L35_FalseBalance", "L35_Repaired"
        };

        private const float NominalOhms = 1000f;
        private const float FaultOhms = 2200f;
        private const float NullTolerance = .05f;

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkResistor leftUpper, leftLower, rightUpper, rightLower;
        public Text statusText, meterText;
        public Light leftLamp, rightLamp;
        public Button capture, loadCase, chooseLeft, chooseRight, chooseBoth, fitLeft, fitRight, verify, restart;

        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalNetworkGraph checkedGraph;
        private SparkElectricalSolveResult measuredResult;
        private string wiringProblem, feedback;
        private int topology = -1, stage;
        private bool ready, caseLoaded, leftFitted, rightFitted;
        private float supply, left, right, difference;

        public void CaptureBaseline()
        {
            if (!CanControl() || stage != 1) return;
            if (!IsNominalBalance()) { feedback = "Check both midpoints against Battery − before capturing the baseline."; return; }
            Advance();
        }

        public void LoadCase()
        {
            if (!CanControl() || stage < 2 || stage > 4 || caseLoaded) return;
            caseLoaded = true;
            feedback = null;
            leftLower.SetResistance(stage == 3 ? NominalOhms : FaultOhms);
            rightLower.SetResistance(stage == 2 ? NominalOhms : FaultOhms);
        }

        public void DiagnoseLeft() => Diagnose(2);
        public void DiagnoseRight() => Diagnose(3);
        public void DiagnoseBoth() => Diagnose(4);

        private void Diagnose(int choice)
        {
            if (!CanControl() || !caseLoaded) return;
            if (!HasExpectedCaseReadings())
            {
                feedback = "Check the midpoint readings. The current case has not reached its expected voltages.";
                return;
            }
            if (choice != stage)
            {
                feedback = Mathf.Abs(difference) <= NullTolerance
                    ? "Compare both midpoint voltages with the 2.50 V baseline."
                    : "Find which midpoint rose above the 2.50 V baseline.";
                return;
            }
            Advance();
        }

        public void FitLeftNominal()
        {
            if (!CanControl() || stage != 5 || leftFitted) return;
            leftLower.SetResistance(NominalOhms);
            leftFitted = true;
            feedback = null;
        }

        public void FitRightNominal()
        {
            if (!CanControl() || stage != 5 || rightFitted) return;
            rightLower.SetResistance(NominalOhms);
            rightFitted = true;
            feedback = null;
        }

        public void VerifyRepair()
        {
            if (!CanControl() || stage != 5 || !leftFitted || !rightFitted) return;
            if (!IsNominalBalance()) { feedback = "Repair verification needs 2.50 V at both midpoints and a near-zero difference."; return; }
            Advance();
        }

        public void RestartTest()
        {
            if (CanControl()) ResetTrial();
        }

        private bool CanControl() => ready && isActiveAndEnabled && loader != null && loader.ActiveLayout == layout &&
            circuit != null && circuit.TopologyVersion == topology && solver != null && !solver.IsDirty && !solver.IsSolving &&
            ReferenceEquals(solver.LatestSolveResult, measuredResult);

        private void Update()
        {
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (layout == null || loader == null || loader.ActiveLayout != layout) return;
            if (manager == null) manager = FindFirstObjectByType<LevelGamePlayManager>();
            if (solver == null) solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (circuit == null) circuit = FindFirstObjectByType<SparkCircuitSystem>();
            if (solver == null || circuit == null) { Show("Waiting for the circuit and solver."); return; }
            if (bindings != loader.ActiveBindings || topology != circuit.TopologyVersion)
            {
                ResetTrial();
                bindings = loader.ActiveBindings;
                topology = circuit.TopologyVersion;
                checkedGraph = null;
            }
            if (bindings == null || Array.Exists(Gates, id => !bindings.IsComponentIdDeclared(id)))
            { Fault("Check the six Level 35 objective bindings."); return; }
            if (!Available(source) || !Available(leftUpper) || !Available(leftLower) || !Available(rightUpper) || !Available(rightLower) ||
                leftUpper.IsBurned || leftLower.IsBurned || rightUpper.IsBurned || rightLower.IsBurned)
            { Fault("Check that the source and all four bridge resistors are enabled and healthy."); return; }
            if (solver.IsSolving) return;
            if ((solver.IsDirty || solver.LatestSolveResult == null) && !solver.SolveNow())
            { Fault("The solver could not evaluate this circuit. Check the wiring."); return; }
            var result = solver.LatestSolveResult;
            if (result == null || solver.IsDirty) return;
            if (!ReferenceEquals(checkedGraph, result.Context.Graph))
            {
                checkedGraph = result.Context.Graph;
                wiringProblem = CheckWiring(checkedGraph);
            }
            if (wiringProblem != null) { Fault(wiringProblem); return; }
            supply = source.ElectricalState.Voltage;
            if (!source.IsOutputActive || source.IsOverloaded || !(supply >= 4.8f && supply <= 5.2f) ||
                !result.TryGetVoltage(leftUpper.TerminalB, source.NegativeTerminal, out left) ||
                !result.TryGetVoltage(rightUpper.TerminalB, source.NegativeTerminal, out right) ||
                float.IsNaN(left) || float.IsInfinity(left) || float.IsNaN(right) || float.IsInfinity(right))
            { Fault("Check the supply and both midpoint connections."); return; }
            difference = left - right;
            measuredResult = result;
            ready = true;
            if (stage == 0 && BranchPowered(leftUpper) && BranchPowered(rightUpper))
                Advance();
            Show(feedback ?? Prompt());
        }

        private bool IsNominalBalance() => Mathf.Abs(left - supply * .50f) <= .07f &&
            Mathf.Abs(right - supply * .50f) <= .07f && Mathf.Abs(difference) <= NullTolerance;

        private bool HasExpectedCaseReadings()
        {
            float nominal = supply * .50f;
            float raised = supply * (FaultOhms / (NominalOhms + FaultOhms));
            switch (stage)
            {
                case 2: return Near(left, raised) && Near(right, nominal) && difference > .70f;
                case 3: return Near(left, nominal) && Near(right, raised) && difference < -.70f;
                case 4: return Near(left, raised) && Near(right, raised) && Mathf.Abs(difference) <= NullTolerance;
                default: return false;
            }
        }

        private static bool Near(float reading, float expected) => Mathf.Abs(reading - expected) <= .08f;

        private void Advance()
        {
            if (!bindings.SetRuntimeComponentBinding(Gates[stage], source))
            { feedback = "Could not record the objective. Check the Level 35 bindings."; return; }
            stage++;
            caseLoaded = false;
            feedback = null;
            QueueEvaluation();
        }

        private void QueueEvaluation()
        {
            if (manager != null && manager.SceneBindings == bindings) manager.QueueEvaluation();
        }

        private string Prompt()
        {
            switch (stage)
            {
                case 0: return "Wire both branches and check the supply.";
                case 1: return "Check both midpoint readings, then press CAPTURE BASELINE.";
                case 2:
                case 3:
                case 4:
                    return caseLoaded ? "Compare LEFT, RIGHT, and DIFF. Choose which lower arm changed."
                        : $"Load Case {stage - 1} to test the bridge.";
                case 5:
                    return leftFitted && rightFitted ? "Check both midpoint readings and press VERIFY REPAIR."
                        : "Fit a 1 kΩ cartridge to each lower arm. Watch the readings change after each repair.";
                default:
                    return manager != null && manager.SceneBindings == bindings && manager.IsCompleted
                        ? "Bridge repaired. Both midpoint voltages are back to baseline."
                        : "Bridge verified. Waiting for level completion.";
            }
        }

        private void Show(string message)
        {
            if (statusText != null) statusText.text = message;
            float displayDifference = Mathf.Abs(difference) < .005f ? 0 : difference;
            if (meterText != null) meterText.text = ready
                ? $"LEFT  {left:0.00} V    |    RIGHT  {right:0.00} V    |    DIFF  {displayDifference:+0.00;-0.00;0.00} V"
                : "LEFT  —    |    RIGHT  —    |    DIFF  —";
            if (leftLamp != null) leftLamp.enabled = ready && BranchPowered(leftUpper);
            if (rightLamp != null) rightLamp.enabled = ready && BranchPowered(rightUpper);
            SetButton(capture, ready && stage == 1);
            SetButton(loadCase, ready && stage >= 2 && stage <= 4 && !caseLoaded);
            SetButton(chooseLeft, ready && caseLoaded);
            SetButton(chooseRight, ready && caseLoaded);
            SetButton(chooseBoth, ready && caseLoaded);
            SetButton(fitLeft, ready && stage == 5 && !leftFitted);
            SetButton(fitRight, ready && stage == 5 && !rightFitted);
            SetButton(verify, ready && stage == 5 && leftFitted && rightFitted);
            SetButton(restart, ready && stage > 0 && stage < Gates.Length);
        }

        private static void SetButton(Button button, bool state)
        {
            if (button != null) button.interactable = state;
        }

        private void ClearGates()
        {
            if (bindings != null) foreach (string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            if (stage > 0) QueueEvaluation();
        }

        private void ResetTrial()
        {
            ClearGates();
            ready = caseLoaded = leftFitted = rightFitted = false;
            stage = 0;
            feedback = null;
            measuredResult = null;
            if (leftLower != null) leftLower.SetResistance(NominalOhms);
            if (rightLower != null) rightLower.SetResistance(NominalOhms);
        }

        private void Fault(string reason)
        {
            if (ready || stage > 0) ResetTrial();
            Show(reason);
        }

        private void OnDisable()
        {
            if (gameObject.activeInHierarchy) ResetTrial();
            else ClearGates();
            bindings = null;
            ready = false;
        }

        private static bool Available(SparkElectricalComponent component) =>
            component != null && component.isActiveAndEnabled && component.ElectricalEnabled;

        private static bool BranchPowered(SparkResistor resistor) => Mathf.Abs(resistor.ElectricalState.Current) > .001f;

        private string CheckWiring(SparkElectricalNetworkGraph graph)
        {
            if (graph == null) return "Waiting for the solved circuit graph.";
            var parents = new Dictionary<int,int>();
            int Root(int node)
            {
                while (parents.TryGetValue(node, out int parent)) node = parent;
                return node;
            }
            foreach (var entry in graph.ComponentTerminals)
            {
                if (!(entry.Key is SparkSingleWire wire) || !Available(wire) ||
                    !graph.TryGetPrimaryComponentTerminals(wire, out var a, out var b) ||
                    !graph.TryGetNode(a, out int an) || !graph.TryGetNode(b, out int bn)) continue;
                int aRoot = Root(an), bRoot = Root(bn);
                if (aRoot != bRoot) parents[bRoot] = aRoot;
            }

            var nets = new[] {
                new[] { source.PositiveTerminal, leftUpper.TerminalA, rightUpper.TerminalA },
                new[] { source.NegativeTerminal, leftLower.TerminalB, rightLower.TerminalB },
                new[] { leftUpper.TerminalB, leftLower.TerminalA },
                new[] { rightUpper.TerminalB, rightLower.TerminalA }
            };
            var used = new Dictionary<int,string>();
            foreach (var net in nets)
            {
                if (net[0] == null || !graph.TryGetNode(net[0], out int first)) return "A required terminal is missing from the solver graph.";
                int root = Root(first);
                if (used.TryGetValue(root, out string other)) return $"Separate {net[0].name} from {other}. Check for a bypass or cross-wire.";
                used.Add(root, net[0].name);
                foreach (var pin in net)
                {
                    if (pin == null || !graph.TryGetNode(pin, out int node)) return $"A terminal is missing on the {net[0].name} path.";
                    if (Root(node) != root) return $"Connect {net[0].name} to {pin.name}. See the wiring diagram.";
                }
            }
            return null;
        }
    }
}
