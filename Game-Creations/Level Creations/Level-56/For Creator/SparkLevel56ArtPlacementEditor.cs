#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>ART ONLY: places Level 56 models under the selected layout, without electrical or gameplay modifications.</summary>
    public static class SparkLevel56ArtPlacementEditor
    {
        private const string RootName = "L56_HD_VISUAL_ART";
        private static readonly string[] Models = {
            "00_L56_IndustrialBase", "01_L56_RegenerativeMotor", "02_L56_FlywheelRotor",
            "03_L56_LeftBearing", "04_L56_RightBearing", "05_L56_EncoderABZ",
            "06_L56_BidirectionalConverter", "07_L56_SupercapacitorBank", "08_L56_CapacitorProtectionUnit",
            "09_L56_BrakingResistor", "10_L56_BrakeChopperDriver", "11_L56_CurrentSensor",
            "12_L56_VoltageSensor", "13_L56_EnergyMeter", "14_L56_RegenerationController",
            "15_L56_EnergyFlowMonitor", "16_L56_SafetyGuard", "17_L56_ThermalCoolingModule",
            "18_L56_ProgrammableLoad", "19_L56_Regulated5VSupply"
        };

        [MenuItem("Tools/Project Spark/Levels/56/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null) {
                EditorUtility.DisplayDialog("Level 56 Art", "Select the destination layout GameObject in the Hierarchy.", "OK");
                return;
            }
            GameObject[] prefabs = new GameObject[Models.Length];
            for (int i=0; i<Models.Length; ++i) {
                string modelPath = FindExactFBX(Models[i]);
                prefabs[i] = string.IsNullOrEmpty(modelPath) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                if (prefabs[i] == null) {
                    EditorUtility.DisplayDialog("Missing Level 56 FBX", "Import " + Models[i] + ".fbx first; no scene changes made.", "OK");
                    return;
                }
            }
            Transform old = selected.transform.Find(RootName);
            if (old != null && !EditorUtility.DisplayDialog("Replace generated art?", "Replace only the " + RootName + " subtree?", "Replace", "Cancel")) return;
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Level 56 HD Art");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Level 56 art root");
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
            Debug.Log("[Level56 Art] Placed 20 FBX models. Art only: no terminals, wiring, physics, objectives or evaluator created.", root);
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
