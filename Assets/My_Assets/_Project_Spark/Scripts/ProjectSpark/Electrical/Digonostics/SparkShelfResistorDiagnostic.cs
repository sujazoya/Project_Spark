#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Read-only Play Mode diagnostic for a shelf-placed resistor.
    /// Select the resistor (or one of its children) and run the menu command.
    /// Does not call SolveNow or change graph / component / scene state.
    /// </summary>
    public static class SparkShelfResistorDiagnostic
    {
        [MenuItem("Tools/Project Spark/Electrical/Diagnose Selected Shelf Resistor")]
        private static void Diagnose()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Shelf Resistor Diagnostic",
                    "Enter Play Mode, place a shelf resistor on the workstation, select it in the Hierarchy, and run this command.", "OK");
                return;
            }

            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("[SPARK SHELF CHECK] Select the PLACED resistor in the Hierarchy first.");
                return;
            }

            SparkResistor resistor = selected.GetComponentInParent<SparkResistor>();
            if (resistor == null) resistor = selected.GetComponentInChildren<SparkResistor>(true);
            if (resistor == null)
            {
                Debug.LogWarning("[SPARK SHELF CHECK] Selection contains no SparkResistor.", selected);
                return;
            }

            SparkElectricalSolver solver = UnityEngine.Object.FindFirstObjectByType<SparkElectricalSolver>();
            if (solver == null)
            {
                Debug.LogError("[SPARK SHELF CHECK] No ACTIVE SparkElectricalSolver found.", resistor);
                return;
            }

            var report = new StringBuilder(3000);
            report.AppendLine("========== PROJECT SPARK / PLACED RESISTOR ==========");
            report.AppendLine("Resistor: " + PathOf(resistor.transform));
            report.AppendLine("Active & enabled: " + resistor.isActiveAndEnabled);
            report.AppendLine("ElectricalEnabled: " + resistor.ElectricalEnabled);
            report.AppendLine("Resistance (ohms): " + resistor.ResistanceOhms);
            report.AppendLine("Solver dirty: " + solver.IsDirty);
            report.AppendLine("Solver has latest result: " + (solver.LatestSolveResult != null));
            SparkShelfItem shelfItem = resistor.GetComponentInParent<SparkShelfItem>();
            if (shelfItem != null)
                report.AppendLine("Shelf item IsAtWorkbench: " + shelfItem.IsAtWorkbench);
            else
                report.AppendLine("Shelf item: none in resistor parent hierarchy");

            bool inDiscoveredList = false;
            var components = solver.ElectricalComponents;
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] == resistor) { inDiscoveredList = true; break; }
            }
            report.AppendLine("DISCOVERED BY SOLVER: " + (inDiscoveredList ? "YES" : "NO  << CHECK SEARCH ROOT"));

            var so = new SerializedObject(solver);
            SerializedProperty search = so.FindProperty("componentSearchRoot");
            SerializedProperty auto = so.FindProperty("autoDiscoverElectricalComponents");
            SerializedProperty circuitProp = so.FindProperty("circuit");
            SparkCircuitSystem circuit = circuitProp != null
                ? circuitProp.objectReferenceValue as SparkCircuitSystem : null;
            Transform searchRoot = search != null ? search.objectReferenceValue as Transform : null;
            if (searchRoot == null) searchRoot = circuit != null ? circuit.transform : solver.transform;
            bool autoDiscover = auto != null && auto.boolValue;
            report.AppendLine("Auto discover: " + autoDiscover);
            report.AppendLine("Effective component search root: " + PathOf(searchRoot));
            report.AppendLine("Resistor below search root: " + resistor.transform.IsChildOf(searchRoot));

            SparkTerminal a = resistor.TerminalA;
            SparkTerminal b = resistor.TerminalB;
            report.AppendLine("Terminal A: " + (a != null ? PathOf(a.transform) : "MISSING"));
            report.AppendLine("Terminal B: " + (b != null ? PathOf(b.transform) : "MISSING"));

            if (circuit != null)
            {
                var connections = new List<SparkCircuitConnection>();
                circuit.CopyConnections(connections);
                report.AppendLine("Circuit connections total: " + connections.Count);
                ReportTerminalWiring(report, "A", a, connections);
                ReportTerminalWiring(report, "B", b, connections);
            }
            else
            {
                report.AppendLine("CIRCUIT REFERENCE MISSING on solver; cannot check electrical connections.");
            }

            SparkElectricalNetworkGraph graph = solver.CurrentGraph;
            SparkElectricalSolveResult result = solver.LatestSolveResult;
            if (graph == null)
            {
                report.AppendLine("CURRENT GRAPH: NULL (not solved/refreshed)");
            }
            else
            {
                bool hasA = a != null && graph.TryGetNode(a, out _);
                bool hasB = b != null && graph.TryGetNode(b, out _);
                report.AppendLine("IN GRAPH: A=" + hasA + " B=" + hasB);
                if (hasA && hasB)
                {
                    graph.TryGetNode(a, out int nodeA);
                    graph.TryGetNode(b, out int nodeB);
                    report.AppendLine("Graph nodes: A=" + nodeA + " B=" + nodeB);
                    if (nodeA == nodeB) report.AppendLine("WARNING: A and B are on the SAME electrical node (possible bypass/short).");
                }
            }

            if (result != null && a != null && b != null)
            {
                if (result.TryGetTerminalVoltage(a, out float va) &&
                    result.TryGetTerminalVoltage(b, out float vb))
                {
                    report.AppendLine(string.Format("SOLVED TERMINALS: A={0:F4} V  B={1:F4} V  Delta={2:F4} V", va, vb, va - vb));
                }
                else report.AppendLine("SOLVED TERMINALS: one/both missing from result");
            }
            if (solver.TryCalculateComponentCurrent(resistor, out float current))
                report.AppendLine(string.Format("SOLVER CURRENT: {0:F4} mA", current * 1000f));
            else report.AppendLine("SOLVER CURRENT: unavailable (dirty, missing result/registration, or disabled)");

            report.AppendLine(string.Format("COMPONENT REPORTED: V={0:F4} V, I={1:F4} mA", resistor.Voltage, resistor.Current * 1000f));
            report.AppendLine("--- NEXT ACTION ---");
            if (!resistor.isActiveAndEnabled || !resistor.ElectricalEnabled)
                report.AppendLine("Enable the placed resistor GameObject and ElectricalEnabled; inspect shelf activation logic.");
            else if (!inDiscoveredList)
                report.AppendLine("Place resistor under solver's Component Search Root OR adjust root to contain active workstation components; refresh topology after placement.");
            else if (a == null || b == null)
                report.AppendLine("Assign SparkResistor.TerminalA / TerminalB on the shelf prefab; ensure each is a SparkTerminal.");
            else if (graph == null || !graph.ContainsTerminal(a) || !graph.ContainsTerminal(b))
                report.AppendLine("Registered component is missing from current graph. Rebuild topology AFTER placement; verify ownership and active state.");
            else if (solver.IsDirty)
                report.AppendLine("Solver is dirty. Check failed solves; refresh wiring and request a new solve after placement.");
            else report.AppendLine("Review A/B connection counts and solved node voltages. Visible wires must create SparkCircuitSystem connections, not only visual cable meshes.");

            Debug.Log(report.ToString(), resistor);
            EditorUtility.DisplayDialog("Shelf Resistor Diagnostic",
                "Diagnostic written to Console for '" + resistor.name + "'. Click the log to inspect the full report.", "OK");
        }

        private static void ReportTerminalWiring(
            StringBuilder report, string label, SparkTerminal terminal,
            List<SparkCircuitConnection> connections)
        {
            if (terminal == null) return;
            int valid = 0;
            for (int i = 0; i < connections.Count; i++)
            {
                SparkCircuitConnection link = connections[i];
                if (!link.IsValid) continue;
                SparkTerminal neighbor = null;
                if (link.A == terminal) neighbor = link.B;
                else if (link.B == terminal) neighbor = link.A;
                if (neighbor == null) continue;
                valid++;
                report.AppendLine("  " + label + " -> " + PathOf(neighbor.transform) +
                    " (kind " + link.Kind + ")");
            }
            report.AppendLine("Terminal " + label + " valid connections: " + valid);
        }

        private static string PathOf(Transform t)
        {
            if (t == null) return "<none>";
            string path = t.name;
            while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
            return path;
        }
    }
}
#endif
