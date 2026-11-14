#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Level 58 visual ART ONLY. Requires the 24 FBXs to be imported first.
    /// Does not create SparkTerminal, wiring, relay behavior, or gameplay objectives.
    /// </summary>
    public static class SparkLevel58ArtPlacementEditor
    {
        private const string ArtRootName = "L58_HD_VISUAL_ART";
        private static readonly string[] ModelNames =
        {
            "00_L58_IndustrialProtectionBase", "01_L58_ThreePhaseGridSource",
            "02_L58_MainIncomingBreaker", "03_L58_ThreePhaseDistributionBus",
            "04_L58_FeederBreakerA", "05_L58_FeederBreakerB", "06_L58_FeederBreakerC",
            "07_L58_MainCurrentTransformer", "08_L58_FeederCurrentTransformerA",
            "09_L58_FeederCurrentTransformerB", "10_L58_FeederCurrentTransformerC",
            "11_L58_MainProtectionRelay", "12_L58_FeederProtectionRelayA",
            "13_L58_FeederProtectionRelayB", "14_L58_FeederProtectionRelayC",
            "15_L58_VoltageMonitoringModule", "16_L58_SmartLoadBankA",
            "17_L58_MotorLoadB", "18_L58_PriorityLoadC",
            "19_L58_FaultInjectionUnit", "20_L58_ProtectionCoordinationController",
            "21_L58_HolographicProtectionMonitor", "22_L58_TransparentSafetyEnclosure",
            "23_L58_Regulated5VSupply"
        };

        [MenuItem("Tools/Project Spark/Levels/58/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Level 58 Art", "Select the Level 58 layout root in the Hierarchy.", "OK");
                return;
            }
            GameObject[] models = new GameObject[ModelNames.Length];
            for (int i = 0; i < ModelNames.Length; i++)
            {
                string path = FindModel(ModelNames[i]);
                if (string.IsNullOrEmpty(path))
                {
                    EditorUtility.DisplayDialog("Missing Level 58 FBX", ModelNames[i] + ".fbx is missing. No changes made.", "OK");
                    return;
                }
                models[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (models[i] == null) throw new InvalidOperationException("FBX could not load: " + path);
            }
            Transform oldRoot = selected.transform.Find(ArtRootName);
            if (oldRoot != null && !EditorUtility.DisplayDialog("Replace art only?",
                "Replace only the old L58_HD_VISUAL_ART subtree?", "Replace", "Cancel")) return;

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Level 58 HD Art");
            if (oldRoot != null) Undo.DestroyObjectImmediate(oldRoot.gameObject);
            var root = new GameObject(ArtRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Level 58 art root");
            root.transform.SetParent(selected.transform, false);
            foreach (GameObject model in models)
            {
                var instance = PrefabUtility.InstantiatePrefab(model, root.transform) as GameObject;
                if (instance == null) throw new InvalidOperationException("Could not instantiate " + model.name);
                Undo.RegisterCreatedObjectUndo(instance, "Place " + model.name);
                instance.name = model.name;
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("[L58 Art] Placed 24 FBXs. No terminals, 38 wires, relay protection, or objectives created.", root);
        }

        private static string FindModel(string filename)
        {
            foreach (string guid in AssetDatabase.FindAssets(filename + " t:Model"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetExtension(path).Equals(".fbx", StringComparison.OrdinalIgnoreCase) &&
                    Path.GetFileNameWithoutExtension(path).Equals(filename, StringComparison.Ordinal))
                    return path;
            }
            return null;
        }
    }
}
#endif
