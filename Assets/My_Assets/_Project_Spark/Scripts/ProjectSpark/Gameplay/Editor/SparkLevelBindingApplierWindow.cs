#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Prefab-safe, preview-first synchronization of workstation slot aliases,
    /// SparkLevelSceneBindings declarations and legacy bridge rules.
    /// No changes to gameplay, the circuit solver, or level asset targets.
    /// </summary>
    public sealed class SparkLevelBindingApplierWindow : EditorWindow
    {
        private SparkLevelWorkstationBindingBridge bridge;
        private SparkLevelDefinition definition;
        private Vector2 scroll;
        private bool verbose = true;
        // Editor-window selection only. Never destroys or disables a workstation.
        private readonly HashSet<int> excludedSlotInstances = new HashSet<int>();
        private SparkLevelWorkstationBindingBridge selectionOwner;

        // Change these two arrays if the serialized fields in
        // SparkLevelSceneBindings have different names in your codebase.
        private static readonly string[] ComponentArrayNames = {
            "runtimeComponentIds", "runtimeComponentIDs", "declaredRuntimeComponentIds",
            "runtimeComponentBindingIds", "runtimeComponentsIds"
        };
        private static readonly string[] TerminalArrayNames = {
            "runtimeTerminalIds", "runtimeTerminalIDs", "declaredRuntimeTerminalIds",
            "runtimeTerminalBindingIds", "runtimeTerminalsIds"
        };

        private sealed class SlotRule
        {
            public SparkShelfSlot slot;
            public string id, component, a, b;
        }

        private sealed class Preview
        {
            public readonly List<SlotRule> rules = new List<SlotRule>();
            public readonly List<SlotRule> available = new List<SlotRule>();
            public readonly List<string> ignored = new List<string>();
            public readonly List<string> errors = new List<string>();
            public readonly List<string> warnings = new List<string>();
            public readonly List<string> addComponents = new List<string>();
            public readonly List<string> addTerminals = new List<string>();
            public SparkLevelSceneBindings bindings;
            public string componentProperty, terminalProperty;
            public bool CanApply => errors.Count == 0 && rules.Count > 0;
        }

        [MenuItem("Tools/Project Spark/Levels/Smart Binding Applier")]
        private static void Open()
        {
            GetWindow<SparkLevelBindingApplierWindow>("Smart Level Bindings");
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject == null) return;
            var candidate = Selection.activeGameObject
                .GetComponentInParent<SparkLevelWorkstationBindingBridge>();
            if (candidate == null) return;
            if (bridge != candidate) excludedSlotInstances.Clear();
            bridge = candidate;
            selectionOwner = candidate;
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Project Spark / Smart Level Binding Applier", EditorStyles.boldLabel);
           EditorGUILayout.HelpBox(
    "Preview and validate Smart Slot IDs. " +
    "Apply declares missing runtime IDs in " +
    "SparkLevelSceneBindings and enables Smart Slots. " +
    "Existing legacy mappings are preserved. " +
    "Original prefab only.",
    MessageType.Info);

            bridge = (SparkLevelWorkstationBindingBridge)EditorGUILayout.ObjectField(
                "Level Bridge", bridge, typeof(SparkLevelWorkstationBindingBridge), true);
            definition = (SparkLevelDefinition)EditorGUILayout.ObjectField(
                "Level Definition (optional)", definition, typeof(SparkLevelDefinition), false);
            verbose = EditorGUILayout.Toggle("Console report", verbose);

            if (bridge != selectionOwner)
            {
                excludedSlotInstances.Clear();
                selectionOwner = bridge;
            }
            if (bridge == null) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUILayout.HelpBox("Stop Play Mode first.", MessageType.Error);
                return;
            }
            if (!IsOriginalPrefabStage(bridge))
            {
                EditorGUILayout.HelpBox(
                    "Open the ORIGINAL level layout prefab in Prefab Mode, then select its bridge. " +
                    "Scene instances and runtime clones are intentionally blocked.", MessageType.Error);
                return;
            }

            Preview preview = BuildPreview();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField(
                "Eligible workstations: " + preview.available.Count +
                "  |  Included: " + preview.rules.Count, EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Include All", GUILayout.Height(27)))
            {
                excludedSlotInstances.Clear();
                Repaint();
            }
            if (GUILayout.Button("− Exclude All", GUILayout.Height(27)))
            {
                foreach (SlotRule r in preview.available)
                    excludedSlotInstances.Add(r.slot.GetInstanceID());
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
            foreach (SlotRule r in preview.available)
            {
                bool selected = !excludedSlotInstances.Contains(r.slot.GetInstanceID());
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(r.slot.name + " [" + r.id + "]", EditorStyles.boldLabel);
                using (new EditorGUI.DisabledScope(selected))
                    if (GUILayout.Button("+", GUILayout.Width(34), GUILayout.Height(24)))
                    {
                        excludedSlotInstances.Remove(r.slot.GetInstanceID());
                        Repaint();
                    }
                using (new EditorGUI.DisabledScope(!selected))
                    if (GUILayout.Button("−", GUILayout.Width(34), GUILayout.Height(24)))
                    {
                        excludedSlotInstances.Add(r.slot.GetInstanceID());
                        Repaint();
                    }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField("Status", selected ? "Included in Apply" : "Excluded from Apply");
                EditorGUILayout.LabelField("Component", r.component);
                EditorGUILayout.LabelField("Terminal A", r.a);
                EditorGUILayout.LabelField("Terminal B", r.b);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.HelpBox("Minus excludes this slot from generated bridge mappings. " +
                "It does NOT delete the slot or remove existing runtime ID declarations. " +
                "Apply replaces the bridge mapping array with only INCLUDED slots.", MessageType.None);
            if (preview.ignored.Count > 0)
                EditorGUILayout.HelpBox("Skipped unconfigured slots:\n" +
                    string.Join("\n", preview.ignored), MessageType.Info);
            foreach (string warning in preview.warnings)
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            foreach (string error in preview.errors)
                EditorGUILayout.HelpBox(error, MessageType.Error);
            if (preview.errors.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Will add component declarations: " + preview.addComponents.Count + "\n" +
                    string.Join(", ", preview.addComponents) + "\n" +
                    "Will add terminal declarations: " + preview.addTerminals.Count + "\n" +
                    string.Join(", ", preview.addTerminals) + "\n" +
                   "Smart Slots included: " + preview.rules.Count + "\n" +
                    "Will set Use Smart Slots = true.\n" +
                    "Existing legacy rules remain unchanged.\n" +
                    "Existing unrelated runtime declarations stay intact.",
                    MessageType.Info);
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Preview / Validate (No Changes)", GUILayout.Height(30)))
                Report(preview);
            using (new EditorGUI.DisabledScope(!preview.CanApply))
            {
                if (GUILayout.Button("Declare Missing IDs + Apply Bridge", GUILayout.Height(38)))
                {
                    if (EditorUtility.DisplayDialog("Apply Level Bindings",
                        "Add " +
                    (preview.addComponents.Count + preview.addTerminals.Count) +
                    " missing runtime declarations and enable Smart Slots? " +
                    "Existing legacy workstation mappings will be preserved.",
                                            "Yes, Apply", "Cancel"))
                        Apply(preview);
                }
            }
        }

        private static bool IsOriginalPrefabStage(SparkLevelWorkstationBindingBridge b)
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            return stage != null && b != null && stage.prefabContentsRoot != null &&
                b.transform.root == stage.prefabContentsRoot.transform;
        }

        private Preview BuildPreview()
        {
            var p = new Preview();
            p.bindings = bridge.GetComponent<SparkLevelSceneBindings>();
            if (p.bindings == null)
                p.bindings = bridge.GetComponentInParent<SparkLevelSceneBindings>();
            if (p.bindings == null)
                p.errors.Add("SparkLevelSceneBindings not found on the prefab root.");

            var slots = bridge.GetComponentsInChildren<SparkShelfSlot>(true)
                .Where(s => s != null && s.Kind == SparkShelfSlotKind.Workstation)
                .OrderBy(s => s.SlotId, StringComparer.Ordinal).ToArray();
            var slotIds = new HashSet<string>(StringComparer.Ordinal);
            var aliases = new HashSet<string>(StringComparer.Ordinal);
            foreach (var s in slots)
            {
                string c = Trim(s.RuntimeComponentId);
                string a = Trim(s.RuntimeTerminalAId);
                string b = Trim(s.RuntimeTerminalBId);
                bool hasAny = c.Length > 0 || a.Length > 0 || b.Length > 0;
                if (!hasAny)
                {
                    p.ignored.Add(s.name + " (all IDs blank)");
                    continue;
                }
                string id = Trim(s.SlotId);
                var rule = new SlotRule { slot=s, id=id, component=c, a=a, b=b };
                p.available.Add(rule);
                if (excludedSlotInstances.Contains(s.GetInstanceID())) continue;
                if (!s.EnableSmartBinding)
                {
                    p.errors.Add(s.name + ": contains runtime IDs, but Enable Smart Binding is OFF. " +
                                 "Enable it or exclude this slot.");
                    continue;
                }
                if (id.Length == 0 || !slotIds.Add(id))
                    p.errors.Add(s.name + ": blank or duplicate SlotId '" + id + "'.");
                if (c.Length == 0 || a.Length == 0 || b.Length == 0)
                    p.errors.Add(s.name + ": configure component, Terminal A and Terminal B IDs.");
                if (a.Length > 0 && a == b)
                    p.errors.Add(s.name + ": Terminal A and B must be distinct.");
                if (c.Length > 0 && !aliases.Add("C:" + c))
                    p.errors.Add(s.name + ": duplicate component alias '" + c + "'.");
                if (a.Length > 0 && !aliases.Add("T:" + a))
                    p.errors.Add(s.name + ": duplicate terminal alias '" + a + "'.");
                if (b.Length > 0 && !aliases.Add("T:" + b))
                    p.errors.Add(s.name + ": duplicate terminal alias '" + b + "'.");
                p.rules.Add(rule);
            }
            if (p.rules.Count == 0)
                p.errors.Add("No included workstation slots found. Use + to include at least one configured slot.");

            if (p.bindings != null)
            {
                var so = new SerializedObject(p.bindings);
                var componentProp = LocateStringArray(so, ComponentArrayNames, "Runtime Component IDs");
                var terminalProp = LocateStringArray(so, TerminalArrayNames, "Runtime Terminal IDs");
                if (componentProp == null || terminalProp == null)
                {
                    p.errors.Add("Cannot find the serialized runtime ID arrays on SparkLevelSceneBindings. " +
                                 "Check their actual field names; the tool will not edit guessed fields.");
                }
                else
                {
                    p.componentProperty = componentProp.propertyPath;
                    p.terminalProperty = terminalProp.propertyPath;
                    var declaredC = ReadStrings(componentProp);
                    var declaredT = ReadStrings(terminalProp);
                    foreach (SlotRule r in p.rules)
                    {
                        if (r.component.Length > 0 && !p.bindings.IsComponentIdDeclared(r.component) &&
                            !p.addComponents.Contains(r.component)) p.addComponents.Add(r.component);
                        if (r.a.Length > 0 && !p.bindings.IsTerminalIdDeclared(r.a) &&
                            !p.addTerminals.Contains(r.a)) p.addTerminals.Add(r.a);
                        if (r.b.Length > 0 && !p.bindings.IsTerminalIdDeclared(r.b) &&
                            !p.addTerminals.Contains(r.b)) p.addTerminals.Add(r.b);
                    }
                    // If a declaration is already present, avoid adding it twice even if
                    // Is*Declared uses a cached lookup that has not refreshed yet.
                    p.addComponents.RemoveAll(id => declaredC.Contains(id));
                    p.addTerminals.RemoveAll(id => declaredT.Contains(id));
                }
            }

            // Optional cross-check: warn about target IDs not supplied by selected slots.
            // Targets can legitimately reference fixed objects, so don't fail closed here.
            if (definition != null && definition.Targets != null)
            {
                var providedC = new HashSet<string>(p.rules.Select(r => r.component), StringComparer.Ordinal);
                var providedT = new HashSet<string>(p.rules.SelectMany(r => new [] {r.a, r.b}), StringComparer.Ordinal);
                foreach (SparkLevelTarget t in definition.Targets)
                {
                    if (t == null) continue;
                    string c = Trim(t.TargetComponentId);
                    if (c.StartsWith("R_", StringComparison.Ordinal) && !providedC.Contains(c))
                        p.warnings.Add("Target '" + t.TargetId + "' uses " + c +
                                       " but no selected workstation supplies it.");
                    foreach (string id in new [] {Trim(t.RequiredPositiveTerminalId), Trim(t.RequiredNegativeTerminalId)})
                        if (id.StartsWith("R_", StringComparison.Ordinal) && !providedT.Contains(id))
                            p.warnings.Add("Target '" + t.TargetId + "' uses " + id +
                                           " but no selected workstation supplies it.");
                }
            }
            return p;
        }

        private static string Trim(string s) => (s ?? "").Trim();

        private static SerializedProperty LocateStringArray(SerializedObject so, string[] candidates, string displayName)
        {
            foreach (string name in candidates)
            {
                SerializedProperty match = so.FindProperty(name);
                if (IsStringArray(match)) return match;
            }
            // A display-name fallback supports common field renames, but only
            // if exactly one matching string-array property exists.
            SerializedProperty unique = null;
            SerializedProperty it = so.GetIterator();
            bool enterChildren = true;
            while (it.NextVisible(enterChildren))
            {
                enterChildren = true;
                if (!IsStringArray(it) || it.displayName != displayName) continue;
                if (unique != null) return null;
                unique = it.Copy();
            }
            return unique;
        }

        private static bool IsStringArray(SerializedProperty prop)
        {
            if (prop == null || !prop.isArray || prop.propertyType == SerializedPropertyType.String)
                return false;
            if (prop.arraySize == 0) return prop.arrayElementType == "string";
            return prop.GetArrayElementAtIndex(0).propertyType == SerializedPropertyType.String;
        }
        private static HashSet<string> ReadStrings(SerializedProperty property)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < property.arraySize; i++)
                ids.Add(Trim(property.GetArrayElementAtIndex(i).stringValue));
            return ids;
        }
        private static void AppendMissing(SerializedProperty property, IEnumerable<string> ids)
        {
            var existing = ReadStrings(property);
            foreach (string id in ids)
            {
                if (id.Length == 0 || !existing.Add(id)) continue;
                int n = property.arraySize;
                property.InsertArrayElementAtIndex(n);
                property.GetArrayElementAtIndex(n).stringValue = id;
            }
        }

       
private void Apply(Preview p)
{
    // Rebuild before changing any serialized data.
    p = BuildPreview();

    if (!p.CanApply)
    {
        Report(p);
        return;
    }

    if (bridge == null || p.bindings == null)
    {
        Debug.LogError(
            "[LevelBindingApplier] Missing bridge or scene bindings.");
        return;
    }

    if (!IsOriginalPrefabStage(bridge))
    {
        Debug.LogError(
            "[LevelBindingApplier] Open the original prefab in Prefab Mode.",
            bridge);
        return;
    }

    SerializedObject bindingObject =
        new SerializedObject(p.bindings);

    SerializedProperty componentProp =
        bindingObject.FindProperty(p.componentProperty);

    SerializedProperty terminalProp =
        bindingObject.FindProperty(p.terminalProperty);

    SerializedObject bridgeObject =
        new SerializedObject(bridge);

    SerializedProperty smartProp =
        bridgeObject.FindProperty("useSmartSlots");

    if (!IsStringArray(componentProp) ||
        !IsStringArray(terminalProp) ||
        smartProp == null)
    {
        Debug.LogError(
            "[LevelBindingApplier] Serialized schema mismatch. " +
            "No changes applied.",
            bridge);
        return;
    }

    // Preserve Undo support for both components.
    Undo.RegisterCompleteObjectUndo(
        new UnityEngine.Object[]
        {
            p.bindings,
            bridge
        },
        "Project Spark - Apply Smart Binding Declarations");

    // Add missing runtime component IDs.
    AppendMissing(
        componentProp,
        p.addComponents);

    // Add missing runtime terminal IDs.
    AppendMissing(
        terminalProp,
        p.addTerminals);

    bindingObject.ApplyModifiedProperties();

    // Smart slots are authoritative.
    // Do not replace the legacy workstation array.
    smartProp.boolValue = true;

    bridgeObject.ApplyModifiedProperties();

    EditorUtility.SetDirty(p.bindings);
    EditorUtility.SetDirty(bridge);

    PrefabStage stage =
        PrefabStageUtility.GetCurrentPrefabStage();

    if (stage != null &&
        stage.prefabContentsRoot != null)
    {
        EditorSceneManager.MarkSceneDirty(
            stage.scene);
    }

    Debug.Log(
        "[LevelBindingApplier V2] SUCCESS\n" +
        "Smart Slots: ENABLED\n" +
        "Included Slots: " + p.rules.Count + "\n" +
        "Component IDs Added: " +
        p.addComponents.Count + "\n" +
        "Terminal IDs Added: " +
        p.addTerminals.Count + "\n" +
        "Legacy Rules: PRESERVED\n" +
        "Original prefab: " + bridge.name,
        bridge);
}

        private static void Set(SerializedProperty parent, string key, string value)
        {
            SerializedProperty prop = parent.FindPropertyRelative(key);
            if (prop != null) prop.stringValue = value ?? "";
        }
        private void Report(Preview p)
        {
            string message = "[LevelBindingApplier] " + (p.CanApply ? "VALID" : "BLOCKED") +
                " | Available=" + p.available.Count + " | Included=" + p.rules.Count + " | Ignored=" + p.ignored.Count +
                " | AddComponents=" + p.addComponents.Count + " | AddTerminals=" + p.addTerminals.Count;
            if (p.errors.Count > 0) message += "\nERRORS:\n- " + string.Join("\n- ", p.errors);
            if (p.warnings.Count > 0) message += "\nWARNINGS:\n- " + string.Join("\n- ", p.warnings);
            if (verbose || !p.CanApply) Debug.Log(message, bridge);
        }
    }
}
#endif
