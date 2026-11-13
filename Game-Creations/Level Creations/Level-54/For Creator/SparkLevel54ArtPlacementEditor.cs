#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// One-click placement of Level 54 FBX ART ONLY.
    /// It does not create SparkTerminals, electrical components, objectives or level definitions.
    /// Re-running will not overwrite the artist's changes to existing model transforms.
    /// </summary>
    public static class SparkLevel54ArtPlacementEditor
    {
        private struct Piece
        {
            public readonly string Name;
            public readonly Vector3 Position;
            public Piece(string n, float x, float y, float z)
            {
                Name = n; Position = new Vector3(x,y,z);
            }
        }

        private static readonly Piece[] Pieces =
        {
            new Piece("00_L54_IndustrialBase", 0f,0f,0f),
            new Piece("01_L54_DriveMotor", -18.2f,0f,0.45f),
            new Piece("02_L54_FlexibleShaft", 0f,0f,0.45f),
            new Piece("03_L54_TestRotor", 2.4f,0f,0.45f),
            new Piece("04_L54_LeftFlexureBearing", -10.5f,0f,0.45f),
            new Piece("05_L54_RightFlexureBearing", 16.8f,0f,0.45f),
            new Piece("06_L54_DisplacementProbeX", -0.8f,0f,0.45f),
            new Piece("07_L54_DisplacementProbeY", 4.9f,0f,0.45f),
            new Piece("08_L54_EncoderABZ", -13.6f,0f,0.45f),
            new Piece("09_L54_ActiveDamper", 10.1f,0f,0.45f),
            new Piece("10_L54_DamperDriver", -4.4f,0f,-9.7f),
            new Piece("11_L54_MotorDriver", -14.8f,0f,-9.7f),
            new Piece("12_L54_CriticalSpeedController", 4.6f,0f,10.2f),
            new Piece("13_L54_OrbitSpectrumMonitor", -13.5f,0f,10.2f),
            new Piece("14_L54_SafetyGuard", 0f,0f,0.45f),
            new Piece("15_L54_SensorCalibrationStand", 16.5f,0f,-9.7f),
            new Piece("16_L54_5VSupply", 19.3f,0f,10.2f)
        };

        [MenuItem("Tools/Project Spark/Levels/54/Place AAA Visual Models")]
        public static void PlaceArt()
        {
            Transform parent = Selection.activeTransform;
            Transform old = parent != null ? parent.Find("L54_VisualArt") : null;
            GameObject artRoot;
            if (old != null) artRoot = old.gameObject;
            else
            {
                artRoot = new GameObject("L54_VisualArt");
                Undo.RegisterCreatedObjectUndo(artRoot, "Create Level 54 art root");
                if (parent != null)
                {
                    Undo.SetTransformParent(artRoot.transform, parent, "Parent Level 54 art root");
                    artRoot.transform.localPosition = Vector3.zero;
                    artRoot.transform.localRotation = Quaternion.identity;
                    artRoot.transform.localScale = Vector3.one;
                }
            }

            int added=0, unchanged=0, missing=0;
            foreach (Piece piece in Pieces)
            {
                if (artRoot.transform.Find(piece.Name) != null) { unchanged++; continue; }
                GameObject asset = FindFbx(piece.Name);
                if (asset == null)
                {
                    Debug.LogError("[Level54 Art] Missing FBX asset: " + piece.Name + ".fbx");
                    missing++; continue;
                }
                GameObject instance = PrefabUtility.InstantiatePrefab(asset, artRoot.transform) as GameObject;
                if (instance == null) { missing++; continue; }
                Undo.RegisterCreatedObjectUndo(instance, "Place Level 54 model");
                instance.name = piece.Name;
                instance.transform.localPosition = piece.Position;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                added++;
            }
            Selection.activeGameObject = artRoot;
            Debug.Log("[Level54 Art] Created=" + added + ", already present=" + unchanged +
                      ", missing=" + missing + ". ART ONLY: wire electrical components separately.", artRoot);
        }

        private static GameObject FindFbx(string stem)
        {
            string[] candidates = AssetDatabase.FindAssets(stem);
            foreach (string id in candidates)
            {
                string p=AssetDatabase.GUIDToAssetPath(id);
                if (!p.EndsWith("/" + stem + ".fbx", StringComparison.OrdinalIgnoreCase)) continue;
                GameObject model=AssetDatabase.LoadAssetAtPath<GameObject>(p);
                if (model != null) return model;
            }
            return null;
        }
    }
}
#endif
