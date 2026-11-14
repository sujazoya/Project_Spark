#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>ART ONLY: places Level 57 models under the selected layout, without electrical or gameplay modifications.</summary>
    public static class SparkLevel57ArtPlacementEditor
    {
        private const string RootName = "L57_HD_VISUAL_ART";
        private static readonly string[] Models = {
            "00_L57_IndustrialMicrogridBase",
            "01_L57_PrimaryGridInverter",
            "02_L57_SecondaryGridInverter",
            "03_L57_DCSource5V",
            "04_L57_EnergyStorageModule",
            "05_L57_ThreePhaseBusbar",
            "06_L57_SynchronizationRelay",
            "07_L57_ThreePoleContactor",
            "08_L57_GridVoltageSensor",
            "09_L57_InverterVoltageSensor",
            "10_L57_FrequencyPhaseDetector",
            "11_L57_CurrentTransformerBank",
            "12_L57_SmartLoadBank",
            "13_L57_IndustrialMotorLoad",
            "14_L57_PowerQualityAnalyzer",
            "15_L57_MicrogridController",
            "16_L57_HolographicGridMonitor",
            "17_L57_ProtectionRelayUnit",
            "18_L57_IslandModeTransferSwitch",
            "19_L57_CoolingAssembly",
            "20_L57_TransparentSafetyEnclosure",
            "21_L57_EmergencyStopStation"
        };

        [MenuItem("Tools/Project Spark/Levels/57/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null) {
                EditorUtility.DisplayDialog("Level 57 Art", "Select the destination layout GameObject in the Hierarchy.", "OK");
                return;
            }
            GameObject[] prefabs = new GameObject[Models.Length];
            for (int i=0; i<Models.Length; ++i) {
                string modelPath = FindExactFBX(Models[i]);
                prefabs[i] = string.IsNullOrEmpty(modelPath) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                if (prefabs[i] == null) {
                    EditorUtility.DisplayDialog("Missing Level 57 FBX", "Import " + Models[i] + ".fbx first; no scene changes made.", "OK");
                    return;
                }
            }
            Transform old = selected.transform.Find(RootName);
            if (old != null && !EditorUtility.DisplayDialog("Replace generated art?", "Replace only the " + RootName + " subtree?", "Replace", "Cancel")) return;
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Level 57 HD Art");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Level 57 art root");
            root.transform.SetParent(selected.transform, false);
            for (int i=0; i<prefabs.Length; ++i) {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefabs[i], root.transform) as GameObject;
                if (instance == null) throw new InvalidOperationException("Failed to instantiate " + Models[i]);
                Undo.RegisterCreatedObjectUndo(instance, "Place " + Models[i]);
                instance.name = Models[i];
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }
            Selection.activeGameObject = root;
            Undo.CollapseUndoOperations(group);
            Debug.Log("[Level57 Art] Placed 22 FBX models. Art only: no terminals, wiring, physics, objectives or evaluator created.", root);
        }
        private static string FindExactFBX(string filename)
        {
            foreach (string guid in AssetDatabase.FindAssets(filename + " t:Model")) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetExtension(path).Equals(".fbx", StringComparison.OrdinalIgnoreCase) &&
                    Path.GetFileNameWithoutExtension(path).Equals(filename, StringComparison.Ordinal)) return path;
            }
            return null;
        }
    }
}
#endif
