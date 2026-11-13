#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Safe ART-ONLY placement of the 17 imported Level 53 FBX visual models.
    /// Does not create electrical components, terminals, wires, bindings, targets,
    /// level definitions or gameplay simulation. Parent art is local Y-up.
    /// </summary>
    public static class SparkLevel53ArtPlacementEditor
    {
        private struct Item
        {
            public readonly string AssetName;
            public readonly Vector3 Position;
            public Item(string n, float x, float y, float z)
            {
                AssetName = n;
                Position = new Vector3(x, y, z);
            }
        }

        private static readonly Item[] Items =
        {
            new Item("00_L53_IndustrialBase", 0f, 0f, 0f),
            new Item("01_L53_DCMotor", -20.80f, 0f, 1.75f),
            new Item("02_L53_LongRotorShaft", 0f, 0f, 1.75f),
            new Item("03_L53_CorrectionPlaneA", -7.10f, 0f, 1.75f),
            new Item("04_L53_CorrectionPlaneB", 7.10f, 0f, 1.75f),
            new Item("05_L53_LeftBearing", -13.90f, 0f, 1.75f),
            new Item("06_L53_RightBearing", 13.90f, 0f, 1.75f),
            new Item("07_L53_LeftVibrationSensor", -21.70f, 0f, 6.90f),
            new Item("08_L53_RightVibrationSensor", 21.20f, 0f, 6.90f),
            new Item("09_L53_EncoderABZ", 20.00f, 0f, 1.75f),
            new Item("10_L53_BalanceDriver", -5.10f, 0f, -8.20f),
            new Item("11_L53_MotorDriver", -16.40f, 0f, -8.20f),
            new Item("12_L53_BalanceController", 4.40f, 0f, 9.10f),
            new Item("13_L53_VectorMonitor", -12.80f, 0f, 9.10f),
            new Item("14_L53_SafetyEnclosure", 0f, 0f, 1.75f),
            new Item("15_L53_TrialWeightTray", 18.80f, 0f, -8.10f),
            new Item("16_L53_5VSupply", 21.60f, 0f, 9.15f)
        };

        [MenuItem("Tools/Project Spark/Levels/53/Place AAA Visual Models")]
        public static void PlaceModels()
        {
            GameObject selection = Selection.activeGameObject;
            Transform parent = selection != null ? selection.transform : null;
            Transform existing = parent != null ? parent.Find("L53_VisualArt") : null;
            GameObject artRoot;
            if (existing != null)
            {
                artRoot = existing.gameObject;
            }
            else
            {
                artRoot = new GameObject("L53_VisualArt");
                Undo.RegisterCreatedObjectUndo(artRoot, "Create Level 53 Art Root");
                if (parent != null)
                {
                    Undo.SetTransformParent(artRoot.transform, parent, "Parent Level 53 Art");
                    artRoot.transform.localPosition = Vector3.zero;
                    artRoot.transform.localRotation = Quaternion.identity;
                    artRoot.transform.localScale = Vector3.one;
                }
            }

            int created = 0, foundExisting = 0, missing = 0;
            foreach (Item item in Items)
            {
                if (artRoot.transform.Find(item.AssetName) != null)
                {
                    foundExisting++;
                    continue; // Do not clobber artist's edits on reruns.
                }

                GameObject model = FindModel(item.AssetName);
                if (model == null)
                {
                    Debug.LogError("[L53 Art] Missing imported FBX: " + item.AssetName + ".fbx");
                    missing++;
                    continue;
                }

                GameObject instance = PrefabUtility.InstantiatePrefab(model, artRoot.transform) as GameObject;
                if (instance == null)
                {
                    Debug.LogError("[L53 Art] Could not instantiate: " + item.AssetName);
                    missing++;
                    continue;
                }

                Undo.RegisterCreatedObjectUndo(instance, "Place Level 53 Art: " + item.AssetName);
                instance.name = item.AssetName;
                instance.transform.localPosition = item.Position;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                created++;
            }

            Selection.activeGameObject = artRoot;
            Debug.Log($"[L53 Art] Created {created}; already present {foundExisting}; missing {missing}. " +
                      "Model placement only — configure SparkTerminals, circuits and gameplay separately.", artRoot);
        }

        private static GameObject FindModel(string fileStem)
        {
            string[] ids = AssetDatabase.FindAssets(fileStem);
            foreach (string id in ids)
            {
                string path = AssetDatabase.GUIDToAssetPath(id);
                if (!path.EndsWith("/" + fileStem + ".fbx", StringComparison.OrdinalIgnoreCase))
                    continue;
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model != null) return model;
            }
            return null;
        }
    }
}
#endif
