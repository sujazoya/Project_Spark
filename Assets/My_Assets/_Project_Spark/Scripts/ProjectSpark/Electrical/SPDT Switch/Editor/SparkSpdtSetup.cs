using System;
using UnityEditor;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Spdt.Editor
{
    public static class SparkSpdtSetup
    {
        [MenuItem("Tools/Project Spark/SPDT/Configure Selected Slot 06")]
        public static void Configure()
        {
            var root = Selection.activeGameObject;
            var slot = root != null ? root.GetComponent<SparkShelfSlot>() : null;
            if (slot == null || slot.SlotId != "Slot_06")
                throw new InvalidOperationException("Open the Level 28 layout or Slot_06 prefab and select Slot_06 itself.");
            var routers = root.GetComponentsInChildren<SparkSpdtDualSwitchRouter>(true);
            if (routers.Length != 1) throw new InvalidOperationException("Expected exactly one SPDT router under Slot_06.");
            var r = routers[0];
            if (r.CommonToA == null || r.CommonToB == null || r.CommonToA == r.CommonToB ||
                r.Common == null || r.A == null || r.B == null ||
                r.Common == r.A || r.Common == r.B || r.A == r.B)
                throw new InvalidOperationException("Assign two distinct switches and three distinct terminals on the router first.");
            var controller = r.GetComponent<SparkSpdtSwitchController>();
            var interaction = r.GetComponent<SparkSpdtInteraction>();
            var interactable = r.GetComponent<SparkInteractable>();
            if (controller == null || interaction == null || interactable == null)
                throw new InvalidOperationException("SPDT root needs Controller, Interaction and SparkInteractable.");
            // Check package versions before changing the hierarchy or any values.
            Require(interactable, "spdtInteraction", "Replace Gameplay/Interactcion/SparkInteractable.cs with the package version and let Unity compile.");
            Require(controller, "circuitRouter", "Install the complete SPDT package and let Unity compile.");
            Require(interaction, "controller", "Install the complete SPDT package and let Unity compile.");
            Undo.RegisterFullObjectHierarchyUndo(root, "Configure SPDT Slot 06");
            // The graph builder collects child terminals. Keep world-space anchors fixed.
            Undo.SetTransformParent(r.Common.transform, r.CommonToA.transform, "Parent SPDT COM");
            Undo.SetTransformParent(r.A.transform, r.CommonToA.transform, "Parent SPDT A");
            Undo.SetTransformParent(r.B.transform, r.CommonToB.transform, "Parent SPDT B");
            Set(r.Common, "owner", r.CommonToA);
            Set(r.A, "owner", r.CommonToA);
            Set(r.B, "owner", r.CommonToB);
            Set(r.CommonToA, "inputTerminal", r.Common); Set(r.CommonToA, "outputTerminal", r.A);
            Set(r.CommonToB, "inputTerminal", r.Common); Set(r.CommonToB, "outputTerminal", r.B);
            Set(r.CommonToA, "electricalEnabled", true); Set(r.CommonToB, "electricalEnabled", true);
            Set(r.CommonToA, "state", 0); Set(r.CommonToB, "state", 0);
            Set(r.CommonToA, "objectId", "SPDT_COM_A");
            Set(r.CommonToB, "objectId", "SPDT_COM_B");
            // Terminals check their electrical owner's InteractionsEnabled when wiring.
            Set(r.CommonToA, "interactionsEnabled", true);
            Set(r.CommonToB, "interactionsEnabled", true);
            r.CommonToA.enabled = r.CommonToB.enabled = true;
            r.CommonToA.gameObject.SetActive(true); r.CommonToB.gameObject.SetActive(true);
            // Disabled MonoBehaviours still receive Awake. The legacy controller's
            // Awake snaps its fallback transform (the whole switch), so remove it.
            Set(interactable, "switchLeverController", null);
            foreach (var lever in r.GetComponentsInChildren<SwitchLeverController>(true))
                Undo.DestroyObjectImmediate(lever);
            Set(controller, "circuitRouter", r);
            Set(controller, "commonTerminal", r.Common.transform);
            Set(controller, "terminalA", r.A.transform); Set(controller, "terminalB", r.B.transform);
            Set(controller, "initializeRouterOnStart", true); Set(controller, "allowMouseClick", false);
            Set(interaction, "controller", controller);
            Set(interactable, "spdtInteraction", interaction);
            Set(interactable, "operateSwitchWhenPlaced", true);
            Set(interactable, "interactionType", r.GetComponent<SparkShelfItem>() != null ? 2 : 1);
            var binder = slot.GetComponent<SparkSpdtSlotBinding>();
            if (binder == null) binder = Undo.AddComponent<SparkSpdtSlotBinding>(slot.gameObject);
            Set(binder, "fixedInstalledRouter", r.GetComponentInParent<SparkShelfItem>() == null ? r : null);
            Set(slot, "enableSmartBinding", true); Set(slot, "category", (int)SparkShelfCategory.Switch);
            Set(slot, "runtimeComponentId", "SPDT_COM_A");
            Set(slot, "runtimeTerminalAId", "SPDT_COM"); Set(slot, "runtimeTerminalBId", "SPDT_A");
            foreach (var b in root.GetComponentsInParent<SparkLevelSceneBindings>(true)) Declare(b);
            EditorUtility.SetDirty(root);
            Debug.Log("[SPDT] Configured. Save the prefab/layout. On the Level 28 Scene Bindings component run Declare SPDT IDs if it was outside this selection's parents. Check lever pivot and shelf occupancy using README.", root);
        }
        [MenuItem("CONTEXT/SparkLevelSceneBindings/Declare SPDT IDs")]
        private static void DeclareContext(MenuCommand command) => Declare((SparkLevelSceneBindings)command.context);
        private static void Declare(SparkLevelSceneBindings b)
        {
            Undo.RecordObject(b, "Declare SPDT IDs");
            var so = new SerializedObject(b);
            Add(so.FindProperty("runtimeComponentIds"), "SPDT_COM_A", "SPDT_COM_B");
            Add(so.FindProperty("runtimeTerminalIds"), "SPDT_COM", "SPDT_A", "SPDT_B");
            so.ApplyModifiedProperties();
        }
        private static void Add(SerializedProperty array, params string[] ids)
        {
            foreach (string id in ids)
            {
                bool exists = false;
                for (int i = 0; i < array.arraySize; i++) exists |= array.GetArrayElementAtIndex(i).stringValue == id;
                if (!exists) { int i = array.arraySize; array.InsertArrayElementAtIndex(i); array.GetArrayElementAtIndex(i).stringValue = id; }
            }
        }
        private static void Set(UnityEngine.Object target, string field, object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null) throw new InvalidOperationException(target.name + " missing field " + field);
            if (value is bool flag) p.boolValue = flag;
            else if (value is int number) p.intValue = number;
            else if (value is string text) p.stringValue = text;
            else p.objectReferenceValue = (UnityEngine.Object)value;
            so.ApplyModifiedProperties();
        }
        private static void Require(UnityEngine.Object target, string field, string remedy)
        {
            if (new SerializedObject(target).FindProperty(field) == null)
                throw new InvalidOperationException("[SPDT] Installed script version mismatch: " + target.GetType().Name + "." + field + " is missing. " + remedy + " No setup changes were made by this run.");
        }
    }
}
