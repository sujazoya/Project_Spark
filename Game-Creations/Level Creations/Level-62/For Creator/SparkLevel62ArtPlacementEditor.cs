#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Art placement only. No SparkTerminal, circuit physics, or objectives are created.</summary>
    public static class SparkLevel62ArtPlacementEditor
    {
        private const string RootName = "L62_HD_VISUAL_ART";
        private static readonly string[] Names =
        {
            "00_L62_IndustrialVoltageRestorationBase",
            "01_L62_ThreePhaseGridSource",
            "02_L62_ThreePhaseInputBus",
            "03_L62_MainInputContactor",
            "04_L62_SeriesInjectionTransformerA",
            "05_L62_SeriesInjectionTransformerB",
            "06_L62_SeriesInjectionTransformerC",
            "07_L62_DVRPowerConverter",
            "08_L62_DVROutputFilter",
            "09_L62_DVRDCLinkCapacitor",
            "10_L62_SupercapacitorEnergyBank",
            "11_L62_BidirectionalDCConverter",
            "12_L62_PrechargeProtectionUnit",
            "13_L62_SeriesBypassSwitch",
            "14_L62_InjectionContactorBank",
            "15_L62_InputVoltageSensor",
            "16_L62_OutputVoltageSensor",
            "17_L62_InjectionVoltageSensor",
            "18_L62_ThreePhaseCurrentSensor",
            "19_L62_PhaseReferencePLL",
            "20_L62_VoltageSagSwellDetector",
            "21_L62_DVRMasterController",
            "22_L62_VoltageCompensationController",
            "23_L62_SensitiveElectronicsLoad",
            "24_L62_PrecisionIndustrialMotor",
            "25_L62_HeavyMotorDisturbanceLoad",
            "26_L62_ProgrammableDisturbanceSimulator",
            "27_L62_PowerQualityEventRecorder",
            "28_L62_ThermalCoolingAssembly",
            "29_L62_ProtectionRelayModule",
            "30_L62_HolographicVoltageMonitor",
            "31_L62_TransparentSafetyEnclosure",
        };
        [MenuItem("Tools/Project Spark/Levels/62/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Level 62 Art", "Select an existing Level 62 layout root in the Hierarchy.", "OK");
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
                    EditorUtility.DisplayDialog("Missing Level 62 Model", Names[i] + ".fbx not found. Import the full Models directory first. Scene unchanged.", "OK");
                    return;
                }
            }
            Transform previous = selected.transform.Find(RootName);
            if (previous != null && !EditorUtility.DisplayDialog("Replace generated art?",
                "Replace ONLY the generated " + RootName + " subtree?", "Replace", "Cancel"))
                return;
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Level 62 Visual Art");
            if (previous != null) Undo.DestroyObjectImmediate(previous.gameObject);
            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Level 62 art root");
            root.transform.SetParent(selected.transform, false);
            for (int i = 0; i < assets.Length; i++)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(assets[i], root.transform) as GameObject;
                if (instance == null) throw new InvalidOperationException("Failed to instantiate " + Names[i]);
                Undo.RegisterCreatedObjectUndo(instance, "Add Level 62 model");
                instance.name = Names[i];
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = root;
            Debug.Log("[LEVEL 62 ART] Placed 32 individual FBXs (art only). Wiring, terminals, and simulation are NOT created.", root);
        }
    }
}
#endif
