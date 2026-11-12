using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel37KelvinTest : MonoBehaviour
    {
        public static readonly string[] Gates =
        {
            "L37_ForceLoop", "L37_KelvinBaseline", "L37_LeadFault",
            "L37_ConnectorFault", "L37_ConnectorRepaired", "L37_FixtureRestored"
        };

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkResistor limiter, leadPositive, connector, leadReturn, senseMeter;
        public Text statusText, meterText;
        public GameObject currentIndicator, senseIndicator;
        public GameObject supplyReady, senseTips;
        public GameObject[] forceIndicators;
        public TextMesh currentReadout, loopReadout, kelvinReadout;
        public Button readLoop, readKelvin, loadCase, diagnoseLead, diagnoseConnector, fitConnector, replaceLead, restart;

        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkCircuitSystem circuit;
        private SparkElectricalSolver solver;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalNetworkGraph checkedGraph;
        private readonly List<SparkCircuitConnection> playerConnections = new List<SparkCircuitConnection>();
        private SparkElectricalSolveResult measuredResult;
        private string wiringIssue, senseIssue, feedback;
        private int topology = -1;
        private int stage;
        private bool forceReady, senseWired, senseReady, caseLoaded, repairRequested;
        private float testCurrent, loopOhms, kelvinOhms, stableTime;

        public void ReadLoop()
        {
            if (CanControl() && stage == 0 && Near(loopOhms, 25f)) Award();
        }

        public void ReadKelvin()
        {
            if (CanControl() && stage == 1 && HasReadings(25f, 5f)) Award();
        }

        public void LoadCase()
        {
            if (!CanControl() || !senseReady || caseLoaded || (stage != 2 && stage != 3)) return;
            feedback = null;
            caseLoaded = true;
            if (stage == 2) leadPositive.SetResistance(20f);
            else connector.SetResistance(12f);
        }

        public void DiagnoseLead() => Diagnose(2);
        public void DiagnoseConnector() => Diagnose(3);

        private void Diagnose(int choice)
        {
            if (!CanControl() || !caseLoaded || !senseReady) return;
            bool caseVerified = stage == 2 ? HasReadings(35f, 5f) : stage == 3 && HasReadings(42f, 12f);
            if (!caseVerified)
            {
                feedback = "Check the readings after the test settles.";
                return;
            }
            if (choice == stage) Award();
            else feedback = "Compare both readings with the previous case. Which reading changed?";
        }

        public void FitConnector()
        {
            if (!CanControl() || !senseReady || stage != 4 || repairRequested) return;
            repairRequested = true;
            connector.SetResistance(5f);
        }

        public void ReplaceLead()
        {
            if (!CanControl() || !senseReady || stage != 5 || repairRequested) return;
            repairRequested = true;
            leadPositive.SetResistance(10f);
        }

        public void Restart()
        {
            if (CanControl() && stage < Gates.Length) ResetTrial();
        }

        private bool CanControl()
        {
            if (!isActiveAndEnabled || loader == null || loader.ActiveLayout != layout || circuit == null ||
                solver == null || solver.IsSolving || circuit.TopologyVersion != topology)
                return false;
            if (solver.IsDirty || !ReferenceEquals(solver.LatestSolveResult, measuredResult))
                RefreshReadings(0f);
            return forceReady && !solver.IsDirty && ReferenceEquals(solver.LatestSolveResult, measuredResult);
        }

        private void Update() => RefreshReadings(Time.unscaledDeltaTime);

        private void RefreshReadings(float elapsed)
        {
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (layout == null || loader == null || loader.ActiveLayout != layout) return;
            if (manager == null) manager = FindFirstObjectByType<LevelGamePlayManager>();
            if (circuit == null) circuit = FindFirstObjectByType<SparkCircuitSystem>();
            if (solver == null) solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (circuit == null || solver == null)
            {
                Show("Waiting for the circuit and solver.");
                return;
            }
            if (bindings != loader.ActiveBindings)
            {
                ResetTrial();
                bindings = loader.ActiveBindings;
                topology = -1;
                checkedGraph = null;
            }
            if (topology != circuit.TopologyVersion)
            {
                // Keep the two-wire measurement while the player adds the sense wires.
                if (stage >= 2 && stage < Gates.Length) ResetTrial();
                topology = circuit.TopologyVersion;
                checkedGraph = null;
                measuredResult = null;
            }
            if (bindings == null || Array.Exists(Gates, gate => !bindings.IsComponentIdDeclared(gate)))
            {
                Invalidate("Check the six Level 37 objective bindings.");
                return;
            }
            if (!Available(source) || !Healthy(limiter) || !Healthy(leadPositive) || !Healthy(connector) ||
                !Healthy(leadReturn) || !Healthy(senseMeter))
            {
                Invalidate("Check that the fixture components are active and healthy.");
                return;
            }
            if (solver.IsSolving) return;
            if ((solver.IsDirty || solver.LatestSolveResult == null) && !solver.SolveNow())
            {
                Invalidate("The solver could not evaluate the fixture wiring.");
                return;
            }
            var result = solver.LatestSolveResult;
            if (result == null || solver.IsDirty) return;
            if (!ReferenceEquals(result.Context.Graph, checkedGraph))
            {
                checkedGraph = result.Context.Graph;
                wiringIssue = CheckWiring(checkedGraph);
            }
            if (wiringIssue != null)
            {
                Invalidate(wiringIssue);
                return;
            }

            float rail = source.ElectricalState.Voltage;
            testCurrent = Mathf.Abs(limiter.ElectricalState.Current);
            if (!source.IsOutputActive || source.IsOverloaded || !(rail >= 4.8f && rail <= 5.2f) ||
                !(testCurrent >= .025f && testCurrent <= .050f) ||
                !result.TryGetVoltage(limiter.TerminalB, source.NegativeTerminal, out float loopVolts) ||
                !IsFinite(loopVolts))
            {
                Invalidate("Check the 5 V supply and the test current through the 100 Ω limiter.");
                return;
            }

            loopOhms = loopVolts / testCurrent;
            forceReady = true;
            senseReady = false;
            if (senseWired && result.TryGetVoltage(senseMeter.TerminalA, senseMeter.TerminalB, out float senseVolts) &&
                IsFinite(senseVolts))
            {
                kelvinOhms = senseVolts / testCurrent;
                senseReady = true;
            }
            measuredResult = result;

            bool repaired = repairRequested &&
                (stage == 4 && HasReadings(35f, 5f) || stage == 5 && HasReadings(25f, 5f));
            stableTime = repaired ? stableTime + elapsed : 0f;
            if (stableTime >= .2f) Award();
            Show(feedback ?? Prompt());
        }

        private bool HasReadings(float expectedLoop, float expectedKelvin) =>
            senseReady && Near(loopOhms, expectedLoop) && Near(kelvinOhms, expectedKelvin);

        private static bool Near(float reading, float expected) => Mathf.Abs(reading - expected) <= .15f;
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private string Prompt() => stage switch
        {
            0 => "FORCE loop ready. Press READ LOOP.",
            1 => senseReady ? "SENSE leads verified. Press READ KELVIN."
                : senseIssue ?? "Connect SENSE + and SENSE − to the connector terminals.",
            2 => caseLoaded ? "Which part changed? Compare with the 25 Ω / 5 Ω baseline." : "Press LOAD CASE to test the first fault.",
            3 => caseLoaded ? "Which part changed? Compare with the previous 35 Ω / 5 Ω readings." : "Press LOAD CASE to test the second fault.",
            4 => repairRequested ? "Checking the connector repair with the bad lead still installed." : "Press FIT CONNECTOR to repair the connector.",
            5 => repairRequested ? "Checking both readings against baseline." : "Press REPLACE LEAD to restore the fixture.",
            _ => manager != null && manager.SceneBindings == bindings && manager.IsCompleted
                ? "Kelvin test complete. Connector and fixture verified."
                : "All six checks recorded. Waiting for level completion."
        };

        private void Award()
        {
            if (!bindings.SetRuntimeComponentBinding(Gates[stage], source))
            {
                feedback = "Could not record the Level 37 objective.";
                return;
            }
            stage++;
            caseLoaded = repairRequested = false;
            stableTime = 0f;
            feedback = null;
            QueueEvaluation();
        }

        private void Show(string message)
        {
            if (statusText != null) statusText.text = message;
            string current = forceReady ? $"{testCurrent * 1000f:0.0} mA" : "—";
            string twoWire = forceReady ? $"{loopOhms:0.00} Ω" : "—";
            string kelvin = senseReady ? $"{kelvinOhms:0.00} Ω" : "—";
            if (meterText != null)
                meterText.text = $"TEST CURRENT  {current}    |    TWO-WIRE  {twoWire}    |    KELVIN  {kelvin}";
            if (currentReadout != null) currentReadout.text = $"CURRENT {current}";
            if (loopReadout != null) loopReadout.text = $"TWO-WIRE {twoWire}";
            if (kelvinReadout != null) kelvinReadout.text = $"KELVIN {kelvin}";
            SetVisual(supplyReady, source != null && source.IsOutputActive);
            SetVisual(currentIndicator, forceReady);
            SetVisual(senseIndicator, senseReady);
            SetVisual(senseTips, senseReady);
            if (forceIndicators != null)
                foreach (var indicator in forceIndicators) SetVisual(indicator, forceReady);
            SetButton(readLoop, forceReady && stage == 0 && Near(loopOhms, 25f));
            SetButton(readKelvin, stage == 1 && HasReadings(25f, 5f));
            SetButton(loadCase, senseReady && (stage == 2 || stage == 3) && !caseLoaded);
            SetButton(diagnoseLead, senseReady && caseLoaded);
            SetButton(diagnoseConnector, senseReady && caseLoaded);
            SetButton(fitConnector, senseReady && stage == 4 && !repairRequested);
            SetButton(replaceLead, senseReady && stage == 5 && !repairRequested);
            SetButton(restart, forceReady && stage > 0 && stage < Gates.Length);
        }

        private static void SetVisual(GameObject visual, bool on)
        {
            if (visual != null && visual.activeSelf != on) visual.SetActive(on);
        }

        private static void SetButton(Button button, bool enabled)
        {
            if (button != null) button.interactable = enabled;
        }

        private void QueueEvaluation()
        {
            if (manager != null && manager.SceneBindings == bindings) manager.QueueEvaluation();
        }

        private void ClearGates()
        {
            if (bindings != null)
                foreach (string gate in Gates) bindings.ClearRuntimeComponentBinding(gate);
            if (stage > 0) QueueEvaluation();
        }

        private void ResetTrial()
        {
            ClearGates();
            stage = 0;
            stableTime = 0f;
            feedback = null;
            measuredResult = null;
            forceReady = senseReady = caseLoaded = repairRequested = false;
            if (leadPositive != null) leadPositive.SetResistance(10f);
            if (connector != null) connector.SetResistance(5f);
        }

        private void Invalidate(string issue)
        {
            if (forceReady || stage > 0) ResetTrial();
            Show(issue);
        }

        private void OnDisable()
        {
            if (gameObject.activeInHierarchy) ResetTrial();
            else ClearGates();
            bindings = null;
            forceReady = senseReady = false;
        }

        private static bool Available(SparkElectricalComponent component) =>
            component != null && component.isActiveAndEnabled && component.ElectricalEnabled;

        private static bool Healthy(SparkResistor resistor) => Available(resistor) && !resistor.IsBurned;

        private string CheckWiring(SparkElectricalNetworkGraph graph)
        {
            senseWired = false;
            senseIssue = null;
            if (graph == null) return "Waiting for the solved circuit graph.";

            var parents = new Dictionary<int, int>();
            int Root(int node)
            {
                while (parents.TryGetValue(node, out int parent)) node = parent;
                return node;
            }
            void Join(SparkTerminal a, SparkTerminal b)
            {
                if (a == null || b == null || !graph.TryGetNode(a, out int an) ||
                    !graph.TryGetNode(b, out int bn)) return;
                int aRoot = Root(an);
                int bRoot = Root(bn);
                if (aRoot != bRoot) parents[bRoot] = aRoot;
            }
            circuit.CopyConnections(playerConnections);
            foreach (var connection in playerConnections)
                if (connection.Kind == SparkConnectionKind.Wire) Join(connection.A, connection.B);

            foreach (var component in graph.ComponentTerminals.Keys)
            {
                if (!(component is SparkSingleWire wire) || !Available(wire) ||
                    !graph.TryGetPrimaryComponentTerminals(wire, out var a, out var b)) continue;
                Join(a, b);
            }

            var nets = new[]
            {
                (source.PositiveTerminal, limiter.TerminalA),
                (limiter.TerminalB, leadPositive.TerminalA),
                (leadPositive.TerminalB, connector.TerminalA),
                (connector.TerminalB, leadReturn.TerminalA),
                (leadReturn.TerminalB, source.NegativeTerminal)
            };
            var used = new Dictionary<int, string>();
            var roots = new int[nets.Length];
            for (int i = 0; i < nets.Length; i++)
            {
                var (a, b) = nets[i];
                if (a == null || b == null || !graph.TryGetNode(a, out int an) || !graph.TryGetNode(b, out int bn))
                    return "A required FORCE terminal is missing from the solver graph.";
                int root = Root(an);
                if (Root(bn) != root) return $"Connect {a.name} to {b.name}.";
                if (used.TryGetValue(root, out string other)) return $"Separate {a.name} from {other}.";
                used.Add(root, a.name);
                roots[i] = root;
            }

            bool sensePlus = senseMeter.TerminalA != null && graph.TryGetNode(senseMeter.TerminalA, out int sp) && Root(sp) == roots[2];
            bool senseMinus = senseMeter.TerminalB != null && graph.TryGetNode(senseMeter.TerminalB, out int sm) && Root(sm) == roots[3];
            senseWired = sensePlus && senseMinus;
            if (!sensePlus) senseIssue = "Connect SENSE_PLUS to CONNECTOR_PLUS.";
            else if (!senseMinus) senseIssue = "Connect SENSE_MINUS to CONNECTOR_MINUS.";
            return null;
        }
    }
}
