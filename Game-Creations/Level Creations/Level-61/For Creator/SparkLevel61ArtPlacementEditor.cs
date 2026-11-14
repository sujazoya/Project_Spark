#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Art placement only. No SparkTerminal, circuit physics, or objectives are created.</summary>
    public static class SparkLevel61ArtPlacementEditor
    {
        private const string RootName = "L61_HD_VISUAL_ART";
        private static readonly string[] Names =
        {
            "00_L61_IndustrialPowerQualityBase",
            "01_L61_ThreePhaseGridSource",
            "02_L61_ThreePhaseDistributionBus",
            "03_L61_MainPowerContactor",
            "04_L61_NonlinearVFDLoad",
            "05_L61_IndustrialMotorLoad",
            "06_L61_RectifierLoadModule",
            "07_L61_SwitchedModePowerLoad",
            "08_L61_ActiveHarmonicFilter",
            "09_L61_FilterPowerConverter",
            "10_L61_FilterInductorBank",
            "11_L61_FilterDCCapacitor",
            "12_L61_PowerFactorCapacitorBank",
            "13_L61_CapacitorSwitchingContactor",
            "14_L61_CapacitorProtectionModule",
            "15_L61_GridVoltageSensor",
            "16_L61_SourceCurrentTransformerBank",
            "17_L61_LoadCurrentTransformerBank",
            "18_L61_FilterCurrentTransformerBank",
            "19_L61_HarmonicSpectrumAnalyzer",
            "20_L61_PowerQualityMeter",
            "21_L61_PhaseReferencePLL",
            "22_L61_HarmonicDetectionController",
            "23_L61_ActiveFilterController",
            "24_L61_ReactivePowerController",
            "25_L61_ThermalCoolingAssembly",
            "26_L61_ProtectionRelayModule",
            "27_L61_HolographicWaveformMonitor",
            "28_L61_TransparentSafetyEnclosure",
            "29_L61_Regulated5VControlSupply",
        };
        [MenuItem("Tools/Project Spark/Levels/61/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Level 61 Art", "Select an existing Level 61 layout root in the Hierarchy.", "OK");
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
                    EditorUtility.DisplayDialog("Missing Level 61 Model", Names[i] + ".fbx not found. Import the full Models directory first. Scene unchanged.", "OK");
                    return;
                }
            }
            Transform previous = selected.transform.Find(RootName);
            if (previous != null && !EditorUtility.DisplayDialog("Replace generated art?",
                "Replace ONLY the generated " + RootName + " subtree?", "Replace", "Cancel"))
                return;
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Level 61 Visual Art");
            if (previous != null) Undo.DestroyObjectImmediate(previous.gameObject);
            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Level 61 art root");
            root.transform.SetParent(selected.transform, false);
            for (int i = 0; i < assets.Length; i++)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(assets[i], root.transform) as GameObject;
                if (instance == null) throw new InvalidOperationException("Failed to instantiate " + Names[i]);
                Undo.RegisterCreatedObjectUndo(instance, "Add Level 61 model");
                instance.name = Names[i];
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = root;
            Debug.Log("[LEVEL 60 ART] Placed 30 individual FBXs (art only). Wiring, terminals, and simulation are NOT created.", root);
        }
    }
}
#endif
