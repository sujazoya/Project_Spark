#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching.Editor
{
    /// <summary>
    /// Adds the read-only adapter to the existing solver; never rebuilds a scene,
    /// changes circuit connections, or overwrites existing electrical components.
    /// </summary>
    public static class SparkSolverLiveMeasurementBridgeInstaller
    {
        private const string Menu = "Tools/Project Spark/Electrical/";

        [MenuItem(Menu + "Install Live Solver Measurement Bridge")]
        private static void Install()
        {
            SparkElectricalSolver solver = null;
            if (Selection.activeGameObject != null)
                solver = Selection.activeGameObject.GetComponent<SparkElectricalSolver>();
            if (solver == null)
                solver = UnityEngine.Object.FindFirstObjectByType<SparkElectricalSolver>(
                    FindObjectsInactive.Include);
            if (solver == null)
            {
                EditorUtility.DisplayDialog("Project Spark", "No SparkElectricalSolver exists in the open scene.", "OK");
                return;
            }

            var bridge = solver.GetComponent<SparkSolverLiveMeasurementBridge>();
            if (bridge == null)
                bridge = Undo.AddComponent<SparkSolverLiveMeasurementBridge>(solver.gameObject);

            var publishers = UnityEngine.Object.FindObjectsByType<SparkVerifiedElectricalPublisher>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            SparkVerifiedElectricalPublisher publisher = publishers.Length > 0
                ? publishers[0]
                : Undo.AddComponent<SparkVerifiedElectricalPublisher>(solver.gameObject);

            var nodeFeeds = UnityEngine.Object.FindObjectsByType<SparkMeterVerifiedNodeFeed>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            SparkMeterVerifiedNodeFeed nodeFeed = nodeFeeds.Length > 0
                ? nodeFeeds[0]
                : Undo.AddComponent<SparkMeterVerifiedNodeFeed>(solver.gameObject);

            var sceneBindingCandidates = UnityEngine.Object.FindObjectsByType<SparkLevelSceneBindings>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            var loader = UnityEngine.Object.FindFirstObjectByType<SparkLevelLayoutLoader>(
                FindObjectsInactive.Include);

            var so = new SerializedObject(bridge);
            SetReferenceIfEmpty(so, "solver", solver);
            SetReferenceIfEmpty(so, "publisher", publisher);
            SetReferenceIfEmpty(so, "nodeFeed", nodeFeed);
            if (loader != null)
            {
                SetReferenceIfEmpty(so, "levelLoader", loader);
                so.FindProperty("followActiveLevel").boolValue = true;
                so.FindProperty("autoMapActiveLevel").boolValue = true;
                // Do not pin the bridge to any one level's inactive bindings.
                so.FindProperty("sceneBindings").objectReferenceValue = null;
            }
            else if (sceneBindingCandidates.Length == 1)
            {
                SetReferenceIfEmpty(so, "sceneBindings", sceneBindingCandidates[0]);
                so.FindProperty("followActiveLevel").boolValue = false;
            }
            so.ApplyModifiedProperties();

            // Meter may already have explicit handoff references. Never overwrite them.
            var meters = UnityEngine.Object.FindObjectsByType<SparkVirtualMultimeter>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (SparkVirtualMultimeter meter in meters)
            {
                SerializedObject meterObj = new SerializedObject(meter);
                SetReferenceIfEmpty(meterObj, "publisher", publisher);
                SetReferenceIfEmpty(meterObj, "nodeFeed", nodeFeed);
                meterObj.ApplyModifiedProperties();
            }

            Selection.activeGameObject = solver.gameObject;
            EditorGUIUtility.PingObject(bridge);
            EditorUtility.SetDirty(bridge);

            string hint = loader != null
                ? "Single-scene mode enabled. Active bindings and circuit ID will be " +
                  "selected automatically when SparkLevelLayoutLoader loads each level. " +
                  "Do NOT auto-map one inactive level manually."
                : "No SparkLevelLayoutLoader found. Assign your loader to the bridge " +
                  "and run Enable Automatic Level Switching; or use manual mapping mode.";
            Debug.Log("[Spark Live Bridge] Installed. " + hint, bridge);
            EditorUtility.DisplayDialog("Spark Live Bridge Installed",
                "Installed on " + solver.name + ".\n\n" + hint, "OK");
        }

        [MenuItem(Menu + "Enable Automatic Level Switching")]
        private static void EnableAutoLevelSwitching()
        {
            var bridge = UnityEngine.Object.FindFirstObjectByType<SparkSolverLiveMeasurementBridge>(
                FindObjectsInactive.Include);
            var loader = UnityEngine.Object.FindFirstObjectByType<SparkLevelLayoutLoader>(
                FindObjectsInactive.Include);
            if (bridge == null || loader == null)
            {
                EditorUtility.DisplayDialog("Project Spark",
                    "Open a scene with SparkSolverLiveMeasurementBridge and " +
                    "SparkLevelLayoutLoader, then try again.", "OK");
                return;
            }

            Undo.RecordObject(bridge, "Enable Spark active-level switching");
            var so = new SerializedObject(bridge);
            so.FindProperty("levelLoader").objectReferenceValue = loader;
            so.FindProperty("followActiveLevel").boolValue = true;
            so.FindProperty("autoMapActiveLevel").boolValue = true;
            so.FindProperty("sceneBindings").objectReferenceValue = null;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(bridge);
            Selection.activeGameObject = bridge.gameObject;
            EditorGUIUtility.PingObject(bridge);
            Debug.Log("[Spark Live Bridge] Automatic single-scene level mapping enabled. " +
                      "Mappings rebuild from loader.ActiveBindings on every new layout.", bridge);
            EditorUtility.DisplayDialog("Project Spark",
                "Automatic switching enabled. Save this scene. " +
                "Each level keeps its own SparkLevelSceneBindings; no per-level " +
                "Inspector setup is needed when those IDs and probe markers are configured.", "OK");
        }

        [MenuItem(Menu + "Auto-map Live Bridge From Selected Scene Bindings")]
        private static void AutoMap()
        {
            SparkLevelSceneBindings bindings = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<SparkLevelSceneBindings>() : null;
            SparkSolverLiveMeasurementBridge bridge = null;
            if (Selection.activeGameObject != null)
                bridge = Selection.activeGameObject.GetComponent<SparkSolverLiveMeasurementBridge>();
            if (bridge == null)
                bridge = UnityEngine.Object.FindFirstObjectByType<SparkSolverLiveMeasurementBridge>(
                    FindObjectsInactive.Include);
            if (bridge == null)
            {
                EditorUtility.DisplayDialog("Project Spark", "Install the live bridge first.", "OK");
                return;
            }
            var bridgeSO = new SerializedObject(bridge);
            if (bindings == null)
            {
                bindings = bridgeSO.FindProperty("sceneBindings").objectReferenceValue as SparkLevelSceneBindings;
            }
            if (bindings == null)
            {
                EditorUtility.DisplayDialog("Project Spark",
                    "Select the correct SparkLevelSceneBindings GameObject, or assign " +
                    "the reference in the bridge Inspector first.", "OK");
                return;
            }
            // In single-scene auto mode, manual mapping is unnecessary and temporary.
            SerializedProperty autoMode = bridgeSO.FindProperty("followActiveLevel");
            if (autoMode != null && autoMode.boolValue)
            {
                EditorUtility.DisplayDialog("Project Spark",
                    "Automatic level switching is enabled. The bridge already maps " +
                    "each loaded layout using SparkLevelLayoutLoader.ActiveBindings. " +
                    "No manual per-level mapping is required.", "OK");
                return;
            }
            // Explicit user action overwrites old bridge mappings, not scene binding assets.
            bool proceed = EditorUtility.DisplayDialog("Populate Live Measurement IDs",
                "Replace only this bridge's resistor/node mapping lists using the selected " +
                "scene bindings? Existing manual mapping entries will be replaced.",
                "Populate", "Cancel");
            if (!proceed) return;

            var sceneSO = new SerializedObject(bindings);
            var components = sceneSO.FindProperty("components");
            var terminals = sceneSO.FindProperty("terminals");
            var targets = new List<ResistorInfo>();
            var nodes = new List<NodeInfo>();
            var usedAnchors = new HashSet<string>(StringComparer.Ordinal);
            var usedTerminals = new HashSet<string>(StringComparer.Ordinal);

            if (components != null)
            {
                for (int i = 0; i < components.arraySize && targets.Count < 32; i++)
                {
                    SerializedProperty entry = components.GetArrayElementAtIndex(i);
                    string id = entry.FindPropertyRelative("id").stringValue;
                    var resistor = entry.FindPropertyRelative("component").objectReferenceValue as SparkResistor;
                    if (resistor == null || !ValidId(id)) continue;
                    string anchor = FindExplicitAnchor(resistor, id);
                    if (!ValidId(anchor) || !usedAnchors.Add(anchor)) continue;
                    targets.Add(new ResistorInfo { anchor = anchor, bindingId = id });
                }
            }

            if (terminals != null)
            {
                for (int i = 0; i < terminals.arraySize && nodes.Count < 128; i++)
                {
                    SerializedProperty entry = terminals.GetArrayElementAtIndex(i);
                    string id = entry.FindPropertyRelative("id").stringValue;
                    var terminal = entry.FindPropertyRelative("terminal").objectReferenceValue as SparkTerminal;
                    if (terminal == null || !ValidId(id)) continue;
                    string meterId = FindMeterId(terminal, id);
                    if (!ValidId(meterId) || !usedTerminals.Add(meterId)) continue;
                    nodes.Add(new NodeInfo { terminalId = meterId, bindingId = id });
                }
            }

            // If a meter-marked terminal is not included in scene terminal bindings,
            // use only an explicit SparkTerminal ancestor on its collider GameObject.
            var markers = UnityEngine.Object.FindObjectsByType<SparkMeterProbeTerminal>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (SparkMeterProbeTerminal marker in markers)
            {
                if (nodes.Count >= 128) break;
                string id = marker.TerminalId;
                if (!ValidId(id) || usedTerminals.Contains(id)) continue;
                var terminal = marker.GetComponent<SparkTerminal>() ??
                    marker.GetComponentInParent<SparkTerminal>();
                if (terminal == null) continue;
                usedTerminals.Add(id);
                nodes.Add(new NodeInfo { terminalId = id, direct = terminal });
            }

            Undo.RecordObject(bridge, "Populate live solver mappings");
            bridgeSO.Update();
            bridgeSO.FindProperty("sceneBindings").objectReferenceValue = bindings;

            var mappings = bridgeSO.FindProperty("resistorBindings");
            mappings.arraySize = targets.Count;
            for (int i = 0; i < targets.Count; i++)
            {
                var row = mappings.GetArrayElementAtIndex(i);
                row.FindPropertyRelative("anchorId").stringValue = targets[i].anchor;
                row.FindPropertyRelative("componentBindingId").stringValue = targets[i].bindingId;
                row.FindPropertyRelative("directResistor").objectReferenceValue = null;
                row.FindPropertyRelative("positiveTerminalBindingId").stringValue = "";
                row.FindPropertyRelative("negativeTerminalBindingId").stringValue = "";
            }

            var terminalMappings = bridgeSO.FindProperty("terminalBindings");
            terminalMappings.arraySize = nodes.Count;
            for (int i = 0; i < nodes.Count; i++)
            {
                var row = terminalMappings.GetArrayElementAtIndex(i);
                row.FindPropertyRelative("terminalId").stringValue = nodes[i].terminalId;
                row.FindPropertyRelative("terminalBindingId").stringValue = nodes[i].bindingId ?? "";
                row.FindPropertyRelative("directTerminal").objectReferenceValue = nodes[i].direct;
            }
            bridgeSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(bridge);
            Selection.activeGameObject = bridge.gameObject;
            Debug.Log("[Spark Live Bridge] Mapped " + targets.Count + " resistor(s), " +
                      nodes.Count + " terminal(s). Verify every meter/teaching ID and the circuitId " +
                      "in Inspector before Play Mode. No circuit elements changed.", bridge);
        }

        private static string FindExplicitAnchor(SparkResistor resistor, string fallback)
        {
            var anchor = resistor.GetComponent<SparkTeachingAnchor>();
            if (anchor != null && ValidId(anchor.AnchorId)) return anchor.AnchorId;
            var markers = resistor.GetComponentsInChildren<SparkMeterProbeTerminal>(true);
            string result = null;
            foreach (var marker in markers)
            {
                if (!ValidId(marker.ComponentAnchorId)) continue;
                if (result != null && result != marker.ComponentAnchorId)
                    return fallback; // Ambiguous marker data, do not guess.
                result = marker.ComponentAnchorId;
            }
            return result ?? fallback;
        }

        private static string FindMeterId(SparkTerminal terminal, string fallback)
        {
            var markers = terminal.GetComponentsInChildren<SparkMeterProbeTerminal>(true);
            string result = null;
            foreach (var marker in markers)
            {
                if (!ValidId(marker.TerminalId)) continue;
                if (result != null && result != marker.TerminalId)
                    return fallback; // Prefer explicit stable scene-binding ID when ambiguous.
                result = marker.TerminalId;
            }
            return result ?? fallback;
        }

        private static void SetReferenceIfEmpty(SerializedObject so, string field, UnityEngine.Object value)
        {
            SerializedProperty prop = so.FindProperty(field);
            if (prop != null && prop.objectReferenceValue == null)
                prop.objectReferenceValue = value;
        }

        private static bool ValidId(string id) =>
            !string.IsNullOrWhiteSpace(id) && id.Length <= 80;

        private sealed class ResistorInfo
        {
            public string anchor;
            public string bindingId;
        }

        private sealed class NodeInfo
        {
            public string terminalId;
            public string bindingId;
            public SparkTerminal direct;
        }
    }
}
#endif
