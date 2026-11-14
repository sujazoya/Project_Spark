#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Adds imported Level 55 meshes and 48 inert art-only socket anchor transforms
    /// under the selected level layout. Does not modify any circuit or objective assets.
    /// Copy this file into an Editor folder; import the FBX pack into Assets first.
    /// </summary>
    public static class SparkLevel55ArtPlacementEditor
    {
        private const string RootName = "L55_HD_VISUAL_ART";
        private static readonly string[] Names =
        {
            "00_L55_IndustrialBase", "01_L55_LevitationRotor",
            "02_L55_LeftMagneticBearing", "03_L55_RightMagneticBearing",
            "04_L55_LeftTouchdownBearing", "05_L55_RightTouchdownBearing",
            "06_L55_DriveMotor", "07_L55_EncoderABZ",
            "08_L55_PositionSensorHub", "09_L55_LeftGapSensors",
            "10_L55_RightGapSensors", "11_L55_LeftMagneticDriver",
            "12_L55_RightMagneticDriver", "13_L55_MotorDriver",
            "14_L55_LevitationController", "15_L55_HolographicMonitor",
            "16_L55_DisturbanceActuator", "17_L55_SafetyEnclosure",
            "18_L55_AxialGuide", "19_L55_5VSupply"
        };

        [MenuItem("Tools/Project Spark/Levels/55/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Level 55 Art", "Select the destination Level 55 layout object in the Hierarchy.", "OK");
                return;
            }
            GameObject[] prefabs = new GameObject[Names.Length];
            for (int i = 0; i < Names.Length; i++)
            {
                string path = FindExactFbx(Names[i]);
                prefabs[i] = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefabs[i] == null)
                {
                    EditorUtility.DisplayDialog("Level 55 Art — Missing Model",
                        "Import the FBX asset first: " + Names[i] + ".fbx\nNo scene changes were made.", "OK");
                    return;
                }
            }

            Transform existing = selected.transform.Find(RootName);
            if (existing != null && !EditorUtility.DisplayDialog("Replace Level 55 art?",
                    "Replace only the previously generated " + RootName + " subtree? Other layout objects will not change.",
                    "Replace", "Cancel")) return;

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Project Spark Level 55 HD Art");
            if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);
            GameObject artRoot = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(artRoot, "Create Level 55 art root");
            artRoot.transform.SetParent(selected.transform, false);
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefabs[i], artRoot.transform) as GameObject;
                if (instance == null) throw new InvalidOperationException("Cannot instantiate: " + Names[i]);
                Undo.RegisterCreatedObjectUndo(instance, "Add Level 55 FBX");
                instance.name = Names[i];
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }

            GameObject anchorRoot = new GameObject("ART_ONLY_SOCKET_ANCHORS_NOT_SPARK_TERMINALS");
            Undo.RegisterCreatedObjectUndo(anchorRoot, "Create art-only socket anchors");
            anchorRoot.transform.SetParent(artRoot.transform, false);
            string csvPath = FindMatchingAnchorCsv();
            int anchorCount = 0;
            if (!string.IsNullOrEmpty(csvPath))
            {
                string[] rows = File.ReadAllLines(csvPath);
                for (int i = 1; i < rows.Length; i++)
                {
                    string[] fields = rows[i].Split(',');
                    if (fields.Length < 6) continue;
                    if (!float.TryParse(fields[3], System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float x) ||
                        !float.TryParse(fields[4], System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float y) ||
                        !float.TryParse(fields[5], System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float z)) continue;
                    GameObject anchor = new GameObject("SOCKET_" + fields[0]);
                    Undo.RegisterCreatedObjectUndo(anchor, "Create Level 55 anchor");
                    anchor.transform.SetParent(anchorRoot.transform, false);
                    anchor.transform.localPosition = new Vector3(x, y, z);
                    anchorCount++;
                }
            }
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = artRoot;
            Debug.Log($"[LEVEL 55 ART] Placed {prefabs.Length} FBXs and {anchorCount} visual socket anchors. " +
                      "No SparkTerminal/SparkElectricalComponent/level targets were added.", artRoot);
        }

        private static string FindExactFbx(string filename)
        {
            string[] ids = AssetDatabase.FindAssets(filename + " t:Model");
            foreach (string id in ids)
            {
                string path = AssetDatabase.GUIDToAssetPath(id);
                if (Path.GetExtension(path).Equals(".fbx", StringComparison.OrdinalIgnoreCase) &&
                    Path.GetFileNameWithoutExtension(path).Equals(filename, StringComparison.Ordinal))
                    return path;
            }
            return null;
        }

        private static string FindMatchingAnchorCsv()
        {
            foreach (string id in AssetDatabase.FindAssets("Terminal_Anchors"))
            {
                string path = AssetDatabase.GUIDToAssetPath(id);
                if (Path.GetFileName(path) == "Terminal_Anchors.csv" &&
                    path.IndexOf("Level55", StringComparison.OrdinalIgnoreCase) >= 0) return path;
            }
            return null;
        }
    }
}
#endif
