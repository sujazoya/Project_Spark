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
            bridge = candidate;
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Project Spark / Smart Level Binding Applier", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Preview first; then append missing runtime declarations and copy only configured " +
                "workstation slots to the bridge. Smart precedence is disabled for the generated " +
                "legacy mappings. Original prefab only; no runtime objects.", MessageType.Info);

            bridge = (SparkLevelWorkstationBindingBridge)EditorGUILayout.ObjectField(
                "Level Bridge", bridge, typeof(SparkLevelWorkstationBindingBridge), true);
            definition = (SparkLevelDefinition)EditorGUILayout.ObjectField(
                "Level Definition (optional)", definition, typeof(SparkLevelDefinition), false);
            verbose = EditorGUILayout.Toggle("Console report", verbose);

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
            EditorGUILayout.LabelField("Eligible workstations: " + preview.rules.Count, EditorStyles.boldLabel);
            foreach (SlotRule r in preview.rules)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(r.slot.name + " [" + r.id + "]", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Component", r.component);
                EditorGUILayout.LabelField("Terminal A", r.a);
                EditorGUILayout.LabelField("Terminal B", r.b);
                EditorGUILayout.EndVertical();
            }
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
                    "Will write bridge mappings: " + preview.rules.Count + "\n" +
                    "Will set Use Smart Slots = false. Existing unrelated declarations stay intact.",
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
                        "Add " + (preview.addComponents.Count + preview.addTerminals.Count) +
                        " missing declarations and replace bridge mappings for this prefab? " +
                        "Other level prefabs will not be changed.", "Apply", "Cancel"))
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
                if (!s.EnableSmartBinding)
                {
                    p.errors.Add(s.name + ": contains runtime IDs, but Enable Smart Binding is OFF. " +
                                 "Enable it or remove unused IDs from the slot.");
                    continue;
                }
                string id = Trim(s.SlotId);
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
                p.rules.Add(new SlotRule { slot=s, id=id, component=c, a=a, b=b });
            }
            if (p.rules.Count == 0)
                p.errors.Add("No configured workstation slots found.");

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
            // Rebuild immediately before mutation in case a slot changed after repaint.
            p = BuildPreview();
            if (!p.CanApply) { Report(p); return; }
            var bindingObject = new SerializedObject(p.bindings);
            var cp = bindingObject.FindProperty(p.componentProperty);
            var tp = bindingObject.FindProperty(p.terminalProperty);
            var bridgeObject = new SerializedObject(bridge);
            var rulesProp = bridgeObject.FindProperty("workstations");
            var smartProp = bridgeObject.FindProperty("useSmartSlots");
            if (!IsStringArray(cp) || !IsStringArray(tp) || rulesProp == null || !rulesProp.isArray || smartProp == null)
            {
                Debug.LogError("[LevelBindingApplier] Schema changed; no changes applied.", bridge);
                return;
            }
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Project Spark: Declare and Apply Level Bindings");
            Undo.RecordObjects(new UnityEngine.Object[] { p.bindings, bridge },
                "Declare runtime IDs and apply bridge mappings");
            AppendMissing(cp, p.addComponents);
            AppendMissing(tp, p.addTerminals);
            bindingObject.ApplyModifiedProperties();

            rulesProp.arraySize = p.rules.Count;
            for (int i = 0; i < p.rules.Count; i++)
            {
                SlotRule r = p.rules[i];
                SerializedProperty entry = rulesProp.GetArrayElementAtIndex(i);
                Set(entry, "slotId", r.id);
                Set(entry, "componentBindingId", r.component);
                Set(entry, "inputTerminalBindingId", r.a);
                Set(entry, "outputTerminalBindingId", r.b);
                Set(entry, "electronicObjectBindingId", "");
                Set(entry, "electricalTypeName", r.slot.Category == SparkShelfCategory.LED ? "SparkLED" : "SparkResistor");
                Set(entry, "electronicTypeName", "");
                Set(entry, "inputTerminalName", r.slot.Category == SparkShelfCategory.LED ? "_Anode" : "_Input");
                Set(entry, "outputTerminalName", r.slot.Category == SparkShelfCategory.LED ? "_Cathode" : "_Output");
                var exact = entry.FindPropertyRelative("exactTerminalNames");
                if (exact != null) exact.boolValue = false;
            }
            smartProp.boolValue = false;
            bridgeObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(p.bindings);
            EditorUtility.SetDirty(bridge);
            Undo.CollapseUndoOperations(group);
            Debug.Log("[LevelBindingApplier] APPLIED: " + p.rules.Count + " workstation mappings, " +
                      p.addComponents.Count + " component IDs, " + p.addTerminals.Count +
                      " terminal IDs. Use Smart Slots = OFF. Save this prefab.", bridge);
        }

        private static void Set(SerializedProperty parent, string key, string value)
        {
            SerializedProperty prop = parent.FindPropertyRelative(key);
            if (prop != null) prop.stringValue = value ?? "";
        }
        private void Report(Preview p)
        {
            string message = "[LevelBindingApplier] " + (p.CanApply ? "VALID" : "BLOCKED") +
                " | Eligible=" + p.rules.Count + " | Ignored=" + p.ignored.Count +
                " | AddComponents=" + p.addComponents.Count + " | AddTerminals=" + p.addTerminals.Count;
            if (p.errors.Count > 0) message += "\nERRORS:\n- " + string.Join("\n- ", p.errors);
            if (p.warnings.Count > 0) message += "\nWARNINGS:\n- " + string.Join("\n- ", p.warnings);
            if (verbose || !p.CanApply) Debug.Log(message, bridge);
        }
    }
}
#endif
