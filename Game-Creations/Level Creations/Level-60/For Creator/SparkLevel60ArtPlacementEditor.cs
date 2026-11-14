#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Art placement only. No SparkTerminal, circuit physics, or objectives are created.</summary>
    public static class SparkLevel60ArtPlacementEditor
    {
        private const string RootName = "L60_HD_VISUAL_ART";
        private static readonly string[] Names =
        {
            "00_L60_IndustrialResilienceBase",
            "01_L60_MainGridInterface",
            "02_L60_GridFormingInverter",
            "03_L60_BatteryStorageSystem",
            "04_L60_BatteryManagementUnit",
            "05_L60_BidirectionalPowerConverter",
            "06_L60_SolarPVSimulator",
            "07_L60_SolarMPPTController",
            "08_L60_BackupGenerator",
            "09_L60_GeneratorStarter",
            "10_L60_ThreePhaseDistributionBus",
            "11_L60_MainGridBreaker",
            "12_L60_GeneratorBreaker",
            "13_L60_BatteryInverterBreaker",
            "14_L60_RenewableSourceBreaker",
            "15_L60_EssentialLoadBreaker",
            "16_L60_PriorityLoadBreaker",
            "17_L60_IndustrialLoadBreaker",
            "18_L60_AuxiliaryLoadBreaker",
            "19_L60_CriticalServiceLoad",
            "20_L60_IndustrialMotorLoad",
            "21_L60_SmartProgrammableLoad",
            "22_L60_ProtectionRelayBank",
            "23_L60_VoltageCurrentSensorHub",
            "24_L60_ResilienceMasterController",
            "25_L60_HolographicSCADAMonitor",
            "26_L60_FaultScenarioSimulator",
            "27_L60_TransparentSafetyEnclosure",
        };
        [MenuItem("Tools/Project Spark/Levels/60/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Level 60 Art", "Select an existing Level 60 layout root in the Hierarchy.", "OK");
                return;
            }
            GameObject[] assets = new GameObject[Names.Length];
            for (int i = 0; i < Names.Length; i++)
            {
                string exactPath = null;
                foreach (string guid in AssetDatabase.FindAssets(Names[i] + " t:Model"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetExtension(path).Equals(".fbx", StringComparison.OrdinalIgnoreCase) &&
                        Path.GetFileNameWithoutExtension(path).Equals(Names[i], StringComparison.Ordinal))
                    {
                        exactPath = path;
                        break;
                    }
                }
                if (exactPath == null || (assets[i] = AssetDatabase.LoadAssetAtPath<GameObject>(exactPath)) == null)
                {
                    EditorUtility.DisplayDialog("Missing Level 60 Model", Names[i] + ".fbx not found. Import the full Models directory first. Scene unchanged.", "OK");
                    return;
                }
            }
            Transform previous = selected.transform.Find(RootName);
            if (previous != null && !EditorUtility.DisplayDialog("Replace generated art?",
                "Replace ONLY the generated " + RootName + " subtree?", "Replace", "Cancel"))
                return;
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Level 60 Visual Art");
            if (previous != null) Undo.DestroyObjectImmediate(previous.gameObject);
            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Level 60 art root");
            root.transform.SetParent(selected.transform, false);
            for (int i = 0; i < assets.Length; i++)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(assets[i], root.transform) as GameObject;
                if (instance == null) throw new InvalidOperationException("Failed to instantiate " + Names[i]);
                Undo.RegisterCreatedObjectUndo(instance, "Add Level 60 model");
                instance.name = Names[i];
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = root;
            Debug.Log("[LEVEL 60 ART] Placed 28 individual FBXs (art only). Wiring, terminals, and simulation are NOT created.", root);
        }
    }
}
#endif
