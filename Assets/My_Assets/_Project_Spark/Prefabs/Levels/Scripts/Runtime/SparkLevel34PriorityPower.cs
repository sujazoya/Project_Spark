using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel34PriorityPower : MonoBehaviour
    {
        public static readonly string[] Gates = {
            "L34_PumpOnly", "L34_BothNormal", "L34_SagObserved",
            "L34_PumpRecovered", "L34_HoldsShed", "L34_ManualRestore"
        };

        public SparkLevelLayout layout;
        public SparkPowerSupply source;
        public SparkResistor feeder, demandBank;
        public SparkMotor pump, fan;
        public SparkSwitch auxContact, demandContact;
        public Transform pumpShaft, fanShaft;
        public Light pumpLight, fanLight, shedLight, demandLight;
        public Text statusText, voltageText, currentText, demandLabel;
        public Button auxButton, demandButton, restoreButton, retryButton;
        public Transform cutoffArm, demandRocker;
        public Vector3 fanRotorAxis = Vector3.right;
        public Renderer pumpIndicator, fanIndicator, shedIndicator, connectedIndicator, demandIndicator;
        public TextMesh busReadout, sagReadout, stateReadout;

        private const float SagThreshold = 3.25f;
        private const float RestoreThreshold = 3.90f;
        private const float CurrentOn = .035f;
        private const float CurrentOff = .001f;
        private SparkLevelLayoutLoader loader;
        private LevelGamePlayManager manager;
        private SparkElectricalSolver solver;
        private SparkCircuitSystem circuit;
        private SparkLevelSceneBindings bindings;
        private SparkElectricalNetworkGraph checkedGraph;
        private string wiringProblem;
        private int topology = -1;
        private int stage;
        private bool ready, shed, restoreRequested;
        private float bus, sag, stableTime;

        public void ConnectAux()
        {
            if (CanControl() && stage == 1 && !shed) SetContact(auxContact, true);
        }

        public void ToggleDemand()
        {
            if (!CanControl()) return;
            if (stage == 2 && !shed) SetContact(demandContact, true);
            else if (stage == 4 && shed) SetContact(demandContact, false);
        }

        public void RestoreAux()
        {
            if (!CanControl() || stage != 5 || !shed || demandContact.IsClosed || bus < RestoreThreshold) return;
            restoreRequested = true;
            shed = false;
            SetContact(auxContact, true);
        }

        public void RestartTest()
        {
            if (CanControl()) ResetTrial();
        }

        private bool CanControl() => ready && isActiveAndEnabled && loader != null && loader.ActiveLayout == layout &&
            circuit != null && circuit.TopologyVersion == topology && solver != null && !solver.IsDirty && !solver.IsSolving;

        private void Update()
        {
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (layout == null || loader == null || loader.ActiveLayout != layout) return;
            if (manager == null) manager = FindFirstObjectByType<LevelGamePlayManager>();
            if (solver == null) solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (circuit == null) circuit = FindFirstObjectByType<SparkCircuitSystem>();
            if (solver == null || circuit == null) { Show("Waiting for the circuit and solver.", false, false, false); return; }

            if (bindings != loader.ActiveBindings || topology != circuit.TopologyVersion)
            {
                ResetTrial();
                bindings = loader.ActiveBindings;
                topology = circuit.TopologyVersion;
                checkedGraph = null;
            }
            if (bindings == null || Array.Exists(Gates, id => !bindings.IsComponentIdDeclared(id)))
            { Fault("Check the six Level 34 objective bindings."); return; }
            if (!Available(source) || !Available(feeder) || !Available(demandBank) || !Available(pump) || !Available(fan) ||
                !Available(auxContact) || !Available(demandContact) || solver.SolverRegistry == null ||
                !solver.SolverRegistry.TryGetDefinition(pump, out _) || !solver.SolverRegistry.TryGetDefinition(fan, out _))
            { Fault("Check the bench components and the motor solver registration."); return; }

            if (solver.IsSolving) return;
            if ((solver.IsDirty || solver.LatestSolveResult == null) && !solver.SolveNow())
            { Fault("The solver could not evaluate this wiring."); return; }
            var result = solver.LatestSolveResult;
            if (result == null || solver.IsDirty) return;
            if (!ReferenceEquals(checkedGraph, result.Context.Graph))
            {
                checkedGraph = result.Context.Graph;
                wiringProblem = CheckWiring(checkedGraph);
            }
            if (wiringProblem != null) { Fault(wiringProblem); return; }
            if (feeder.IsBurned || demandBank.IsBurned || feeder.IsOverPower || demandBank.IsOverPower)
            { Fault("Check the feeder and demand resistor ratings. Reload the level if either resistor burned."); return; }
            if (!source.IsOutputActive || source.ElectricalState.Voltage < 4.8f ||
                source.ElectricalState.Voltage > 5.2f || source.IsOverloaded || pump.IsOverCurrent || fan.IsOverCurrent ||
                !result.TryGetVoltage(feeder.TerminalB, source.NegativeTerminal, out bus) || float.IsNaN(bus) || float.IsInfinity(bus))
            { Fault("Check the supply, motor current, and BUS connections."); return; }

            ready = true;
            bool pumpOn = pump.IsRunning && pump.ElectricalState.Current >= CurrentOn;
            bool fanOn = fan.IsRunning && fan.ElectricalState.Current >= CurrentOn;
            bool fanOff = Mathf.Abs(fan.ElectricalState.Current) < CurrentOff;
            bool demandOn = demandContact.IsClosed && demandBank.ElectricalState.Current >= CurrentOn;
            bool demandOff = demandContact.IsOpen && Mathf.Abs(demandBank.ElectricalState.Current) < CurrentOff;

            if (stage == 2 && !shed && auxContact.IsClosed && pumpOn && fanOn && demandOn && bus < SagThreshold)
            {
                sag = bus;
                if (!Award()) { Fault("Could not record the observed voltage sag."); return; }
                shed = true;
                SetContact(auxContact, false);
                Show("Sag recorded. The auxiliary contact opened. Checking pump recovery.", pumpOn, false, demandOn);
                return;
            }

            bool passed = stage switch {
                0 => pumpOn && fanOff && demandOff && bus >= RestoreThreshold,
                1 => auxContact.IsClosed && pumpOn && fanOn && demandOff && bus >= SagThreshold,
                3 => shed && auxContact.IsOpen && pumpOn && fanOff && demandOn && bus >= SagThreshold && bus > sag + .18f,
                4 => shed && auxContact.IsOpen && pumpOn && fanOff && demandOff && bus >= RestoreThreshold,
                5 => restoreRequested && auxContact.IsClosed && pumpOn && fanOn && demandOff && bus >= SagThreshold,
                _ => false
            };
            stableTime = passed ? stableTime + Time.deltaTime : 0;
            if (stableTime >= .20f && !Award()) { Fault("Could not register the Level 34 objective."); return; }
            Show(Prompt(), pumpOn, fanOn, demandOn);

            if (pumpOn && pumpShaft != null) pumpShaft.Rotate(Vector3.right,bus * 140f * Time.deltaTime,Space.Self);
            if (fanOn && fanShaft != null) fanShaft.Rotate(fanRotorAxis,bus * 200f * Time.deltaTime,Space.Self);
        }

        private bool Award()
        {
            if (!bindings.SetRuntimeComponentBinding(Gates[stage], source)) return false;
            stage++;
            stableTime = 0;
            QueueEvaluation();
            return true;
        }

        private void QueueEvaluation()
        {
            if (manager != null && manager.SceneBindings == bindings) manager.QueueEvaluation();
        }

        private string Prompt() => stage switch {
            0 => "Verify the critical pump is running on the bus.",
            1 => "Pump-only voltage verified. Press CONNECT AUX.",
            2 => demandContact.IsClosed ? "Demand on. Checking the bus for a sag below 3.25 V." : "Both loads verified. Press APPLY DEMAND.",
            3 => "Keep the demand bank on. Verify the pump recovered with the fan off.",
            4 => "Pump recovered. Press REMOVE DEMAND and confirm the fan remains off.",
            5 => "Demand removed and bus above 3.9 V. Press RESTORE AUX.",
            _ => manager != null && manager.SceneBindings == bindings && manager.IsCompleted
                ? "Priority Power complete. The pump recovered before the fan was restored."
                : "All six electrical checks passed. Waiting for level completion."
        };

        private void Show(string message, bool pumpOn, bool fanOn, bool demandOn)
        {
            if (statusText != null && statusText.text != message) statusText.text = message;
            if (voltageText != null) voltageText.text = ready
                ? $"SOURCE {source.ElectricalState.Voltage:0.00} V   |   BUS {bus:0.00} V   |   SAG {(sag > 0f ? $"{sag:0.00} V" : "—")}"
                : "SOURCE —   |   BUS —   |   SAG —";
            if (currentText != null) currentText.text = ready
                ? $"PUMP {1000f * pump.ElectricalState.Current:0} mA   |   FAN {1000f * fan.ElectricalState.Current:0} mA   |   DEMAND {1000f * demandBank.ElectricalState.Current:0} mA{(shed ? "   |   AUX SHED" : "")}"
                : "PUMP —   |   FAN —   |   DEMAND —";
            if (pumpLight != null) pumpLight.enabled = pumpOn;
            if (fanLight != null) fanLight.enabled = fanOn;
            if (demandLight != null) demandLight.enabled = demandOn;
            if (shedLight != null) shedLight.enabled = shed;
            if (cutoffArm != null && auxContact != null)
                cutoffArm.localRotation = Quaternion.Euler(0, 0, auxContact.IsClosed ? 0 : 15f);
            if (demandRocker != null && demandContact != null)
                demandRocker.localRotation = Quaternion.Euler(demandContact.IsClosed ? 11f : -11f, 0, 0);
            if (pumpIndicator != null) pumpIndicator.enabled = pumpOn;
            if (fanIndicator != null) fanIndicator.enabled = fanOn;
            if (demandIndicator != null) demandIndicator.enabled = demandOn;
            if (shedIndicator != null) shedIndicator.enabled = shed;
            if (connectedIndicator != null) connectedIndicator.enabled = ready && auxContact != null && auxContact.IsClosed;
            if (busReadout != null) busReadout.text = ready ? $"BUS {bus:0.00} V" : "BUS —";
            if (sagReadout != null) sagReadout.text = ready && sag > 0f ? $"SAG {sag:0.00} V" : "SAG —";
            if (stateReadout != null)
            {
                stateReadout.text = shed ? "AUX SHED" : fanOn ? "PUMP + FAN" : pumpOn ? "PUMP ONLY" : "READY TO WIRE";
                stateReadout.color = shed ? new Color(1f,.73f,.31f) : new Color(.49f,.95f,1f);
            }
            if (auxButton != null) auxButton.interactable = ready && stage == 1 && !auxContact.IsClosed;
            if (demandButton != null) demandButton.interactable = ready && (stage == 2 && !demandContact.IsClosed || stage == 4 && demandContact.IsClosed);
            if (demandLabel != null) demandLabel.text = stage == 4 ? "REMOVE DEMAND" : "APPLY DEMAND";
            if (restoreButton != null) restoreButton.interactable = ready && stage == 5 && shed && demandContact.IsOpen && bus >= RestoreThreshold;
            if (retryButton != null) retryButton.interactable = ready && stage > 0 && stage < Gates.Length;
        }

        private void ResetTrial()
        {
            ClearGates();
            ready = shed = restoreRequested = false;
            stage = 0;
            sag = bus = stableTime = 0;
            SetContact(auxContact, false);
            SetContact(demandContact, false);
        }

        private void ClearGates()
        {
            if (bindings != null) foreach (string id in Gates) bindings.ClearRuntimeComponentBinding(id);
            if (stage > 0) QueueEvaluation();
        }

        private void Fault(string reason)
        {
            if (ready || stage > 0) ResetTrial();
            Show(reason, false, false, false);
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

        private static void SetContact(SparkSwitch contact, bool closed)
        {
            if (contact != null && contact.IsClosed != closed)
                contact.SetState(closed ? SparkSwitchState.Closed : SparkSwitchState.Open);
        }

        private string CheckWiring(SparkElectricalNetworkGraph graph)
        {
            if (graph == null) return "Waiting for the solved circuit graph.";
            var parents = new Dictionary<int,int>();
            int Root(int node)
            {
                while (parents.TryGetValue(node,out int parent)) node = parent;
                return node;
            }
            foreach (var entry in graph.ComponentTerminals)
            {
                if (!(entry.Key is SparkSingleWire wire) || !Available(wire) ||
                    !graph.TryGetPrimaryComponentTerminals(wire,out var a,out var b) ||
                    !graph.TryGetNode(a,out int an) || !graph.TryGetNode(b,out int bn)) continue;
                int aRoot=Root(an), bRoot=Root(bn);
                if (aRoot != bRoot) parents[bRoot]=aRoot;
            }
            var nets = new[] {
                new[] { source.PositiveTerminal, feeder.TerminalA },
                new[] { feeder.TerminalB, pump.PositiveTerminal, auxContact.InputTerminal, demandContact.InputTerminal },
                new[] { auxContact.OutputTerminal, fan.PositiveTerminal },
                new[] { demandContact.OutputTerminal, demandBank.TerminalA },
                new[] { source.NegativeTerminal, pump.NegativeTerminal, fan.NegativeTerminal, demandBank.TerminalB }
            };
            var used=new Dictionary<int,string>();
            foreach (var net in nets)
            {
                if (net[0] == null || !graph.TryGetNode(net[0],out int first)) return "A required terminal is missing from the solver graph.";
                int root=Root(first);
                if (used.TryGetValue(root,out string other)) return $"Separate {net[0].name} from {other}. Check for a bypass wire.";
                used.Add(root,net[0].name);
                foreach (var pin in net)
                {
                    if (pin == null || !graph.TryGetNode(pin,out int node)) return $"A terminal is missing on the {net[0].name} path.";
                    if (Root(node) != root) return $"Connect {net[0].name} to {pin.name}. See the wiring diagram.";
                }
            }
            return null;
        }
    }
}
