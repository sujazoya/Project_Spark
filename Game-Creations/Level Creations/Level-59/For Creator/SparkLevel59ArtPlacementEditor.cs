#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Art-only placer for Level 59. Does not create terminals, wiring, electrical simulation or objectives.</summary>
    public static class SparkLevel59ArtPlacementEditor
    {
        private const string RootName = "L59_HD_VISUAL_ART";
        private static readonly string[] Names =
        {
            "00_L59_IndustrialBlackStartBase",
            "01_L59_BatteryStorageBank",
            "02_L59_BatteryManagementSystem",
            "03_L59_GridFormingInverter",
            "04_L59_DCPrechargeModule",
            "05_L59_DCPowerDistributionBus",
            "06_L59_ThreePhaseACBus",
            "07_L59_BackupGenerator",
            "08_L59_GeneratorStarter",
            "09_L59_GeneratorSynchronizationRelay",
            "10_L59_MainGridBreaker",
            "11_L59_GeneratorBreaker",
            "12_L59_EssentialLoadBreakerA",
            "13_L59_EssentialLoadBreakerB",
            "14_L59_EssentialLoadBreakerC",
            "15_L59_NonessentialLoadBreakerA",
            "16_L59_NonessentialLoadBreakerB",
            "17_L59_EmergencyLightingLoad",
            "18_L59_CriticalCoolingLoad",
            "19_L59_IndustrialMotorLoad",
            "20_L59_VoltageFrequencySensorHub",
            "21_L59_PowerEnergyMeter",
            "22_L59_RestorationSequencer",
            "23_L59_HolographicRestorationMonitor",
            "24_L59_TransparentSafetyEnclosure",
            "25_L59_Regulated5VControlSupply",
        };

        [MenuItem("Tools/Project Spark/Levels/59/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject destination = Selection.activeGameObject;
            if (!destination)
            {
                EditorUtility.DisplayDialog("Level 59 Art", "Select an existing Level 59 layout root in the Hierarchy.", "OK");
                return;
            }

            var prefabs = new GameObject[Names.Length];
            for (int i = 0; i < Names.Length; i++)
            {
                string exact = null;
                foreach (string guid in AssetDatabase.FindAssets(Names[i] + " t:Model"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetExtension(path).Equals(".fbx", StringComparison.OrdinalIgnoreCase)
                        && Path.GetFileNameWithoutExtension(path).Equals(Names[i], StringComparison.Ordinal))
                    { exact = path; break; }
                }
                if (exact == null || !(prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(exact)))
                {
                    EditorUtility.DisplayDialog("Missing Level 59 FBX", Names[i] + ".fbx was not found. Import all 26 individual models first. No changes were made.", "OK");
                    return;
                }
            }

            var existing = destination.transform.Find(RootName);
            if (existing && !EditorUtility.DisplayDialog("Replace generated art?",
                "Replace only the " + RootName + " subtree under this selected layout?", "Replace", "Cancel"))
                return;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Project Spark Level 59 Visual Assets");
            if (existing) Undo.DestroyObjectImmediate(existing.gameObject);
            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Level 59 art root");
            root.transform.SetParent(destination.transform, false);
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefabs[i], root.transform) as GameObject;
                if (!instance) throw new InvalidOperationException("Could not instantiate " + Names[i]);
                Undo.RegisterCreatedObjectUndo(instance, "Place Level 59 model");
                instance.name = Names[i];
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("[LEVEL 59 ART] Placed 26 individual FBXs. No real SparkTerminal or level objectives created.", root);
        }
    }
}
#endif
