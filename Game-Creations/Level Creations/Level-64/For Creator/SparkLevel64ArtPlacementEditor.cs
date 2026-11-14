#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Places Level 64 ART only. Does not build SparkTerminal wiring, protection physics or objectives.</summary>
    public static class SparkLevel64ArtPlacementEditor
    {
        private const string RootName = "L64_HD_VISUAL_ART";
        private static readonly string[] Names =
        {
            "00_L64_IndustrialTransientStabilityBase",
            "01_L64_SynchronousGeneratorA",
            "02_L64_SynchronousGeneratorB",
            "03_L64_PrimeMoverA",
            "04_L64_PrimeMoverB",
            "05_L64_GeneratorBreakerA",
            "06_L64_GeneratorBreakerB",
            "07_L64_ThreePhaseBusA",
            "08_L64_ThreePhaseBusB",
            "09_L64_TransmissionFeederA",
            "10_L64_TransmissionFeederB",
            "11_L64_FeederBreakerA_In",
            "12_L64_FeederBreakerA_Out",
            "13_L64_FeederBreakerB_In",
            "14_L64_FeederBreakerB_Out",
            "15_L64_GeneratorEncoderA",
            "16_L64_GeneratorEncoderB",
            "17_L64_SynchrophasorMeasurementUnitA",
            "18_L64_SynchrophasorMeasurementUnitB",
            "19_L64_CurrentTransformerBankA",
            "20_L64_CurrentTransformerBankB",
            "21_L64_DistanceProtectionRelayA",
            "22_L64_DistanceProtectionRelayB",
            "23_L64_DifferentialProtectionRelay",
            "24_L64_OutOfStepProtectionRelay",
            "25_L64_FaultInjectionChamber",
            "26_L64_CriticalClearingTimeAnalyzer",
            "27_L64_PowerAngleAnalyzer",
            "28_L64_GeneratorExcitationController",
            "29_L64_GeneratorGovernorController",
            "30_L64_PowerSystemStabilizer",
            "31_L64_IndustrialLoadBank",
            "32_L64_TransientStabilityMasterController",
            "33_L64_HolographicStabilityMonitor",
            "34_L64_TransparentSafetyEnclosure",
            "35_L64_Regulated5VControlSupply",
        };

        [MenuItem("Tools/Project Spark/Levels/64/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Level 64 Art", "Select a Level 64 layout root in the Hierarchy first.", "OK");
                return;
            }
            var prefabs = new GameObject[Names.Length];
            for (int i = 0; i < Names.Length; ++i)
            {
                string pathFound = null;
                foreach (string guid in AssetDatabase.FindAssets(Names[i] + " t:Model"))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetExtension(path).Equals(".fbx", StringComparison.OrdinalIgnoreCase) &&
                        Path.GetFileNameWithoutExtension(path).Equals(Names[i], StringComparison.Ordinal))
                    {
                        pathFound = path;
                        break;
                    }
                }
                if (pathFound == null || (prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(pathFound)) == null)
                {
                    EditorUtility.DisplayDialog("Level 64 Model Missing", Names[i] + ".fbx not found. Import all 36 model files; scene unchanged.", "OK");
                    return;
                }
            }
            Transform existing = selected.transform.Find(RootName);
            if (existing != null && !EditorUtility.DisplayDialog("Replace generated Level 64 art?",
                "Replace ONLY the generated " + RootName + " subtree? Other gameplay components remain untouched.", "Replace", "Cancel"))
                return;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Level 64 Visual Art");
            if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);
            var root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Level 64 art root");
            root.transform.SetParent(selected.transform, false);
            for (int i = 0; i < prefabs.Length; ++i)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefabs[i], root.transform) as GameObject;
                if (instance == null) throw new InvalidOperationException("Cannot instantiate " + Names[i]);
                Undo.RegisterCreatedObjectUndo(instance, "Add Level 64 model");
                instance.name = Names[i];
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("[Level64Art] Placed 36 individual FBXs under " + root.name + ". Art only: wire, simulate, and bind objectives separately.");
        }
    }
}
#endif
