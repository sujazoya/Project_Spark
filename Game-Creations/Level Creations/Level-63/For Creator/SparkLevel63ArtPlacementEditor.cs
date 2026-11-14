#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Art placement only. No SparkTerminal, circuit physics, or objectives are created.</summary>
    public static class SparkLevel63ArtPlacementEditor
    {
        private const string RootName = "L63_HD_VISUAL_ART";
        private static readonly string[] Names =
        {
            "00_L63_IndustrialFrequencyLabBase",
            "01_L63_ThreePhaseMicrogridSource",
            "02_L63_ThreePhaseDistributionBus",
            "03_L63_MainGridBreaker",
            "04_L63_SynchronousGenerator",
            "05_L63_GeneratorPrimeMover",
            "06_L63_GeneratorBreaker",
            "07_L63_HighInertiaFlywheelRotor",
            "08_L63_FlywheelMotorGenerator",
            "09_L63_FlywheelMagneticBearing",
            "10_L63_FlywheelShaftCoupling",
            "11_L63_FlywheelSpeedEncoder",
            "12_L63_FlywheelVibrationSensor",
            "13_L63_FlywheelPowerConverter",
            "14_L63_FlywheelDCEnergyLink",
            "15_L63_BatteryEnergyBuffer",
            "16_L63_BatteryManagementModule",
            "17_L63_GridFormingInverter",
            "18_L63_VirtualSynchronousMachineController",
            "19_L63_FrequencyDroopController",
            "20_L63_PowerOscillationDamper",
            "21_L63_FrequencyROCOFSensor",
            "22_L63_ThreePhaseVoltageSensor",
            "23_L63_CurrentTransformerBank",
            "24_L63_ProgrammableLoadBank",
            "25_L63_IndustrialMotorLoad",
            "26_L63_RenewableSourceSimulator",
            "27_L63_GeneratorTripSimulator",
            "28_L63_FastFrequencyResponseRelay",
            "29_L63_EnergyManagementController",
            "30_L63_PowerQualityEventRecorder",
            "31_L63_HolographicFrequencyMonitor",
            "32_L63_TransparentFlywheelSafetyEnclosure",
            "33_L63_Regulated5VControlSupply",
        };
        [MenuItem("Tools/Project Spark/Levels/63/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Level 63 Art", "Select an existing Level 63 layout root in the Hierarchy.", "OK");
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
                    EditorUtility.DisplayDialog("Missing Level 63 Model", Names[i] + ".fbx not found. Import the full Models directory first. Scene unchanged.", "OK");
                    return;
                }
            }
            Transform previous = selected.transform.Find(RootName);
            if (previous != null && !EditorUtility.DisplayDialog("Replace generated art?",
                "Replace ONLY the generated " + RootName + " subtree?", "Replace", "Cancel"))
                return;
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Level 63 Visual Art");
            if (previous != null) Undo.DestroyObjectImmediate(previous.gameObject);
            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Level 63 art root");
            root.transform.SetParent(selected.transform, false);
            for (int i = 0; i < assets.Length; i++)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(assets[i], root.transform) as GameObject;
                if (instance == null) throw new InvalidOperationException("Failed to instantiate " + Names[i]);
                Undo.RegisterCreatedObjectUndo(instance, "Add Level 63 model");
                instance.name = Names[i];
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = root;
            Debug.Log("[LEVEL 62 ART] Placed 34 individual FBXs (art only). Wiring, terminals, and simulation are NOT created.", root);
        }
    }
}
#endif
