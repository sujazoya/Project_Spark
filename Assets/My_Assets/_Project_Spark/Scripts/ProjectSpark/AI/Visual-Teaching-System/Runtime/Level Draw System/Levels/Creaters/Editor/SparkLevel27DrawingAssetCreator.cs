#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.Gameplay.Editor
{
    /// <summary>
    /// Schema-safe Level 27 drawing asset creator.
    /// Clones an existing SparkLevelDrawingAsset with a known-valid serialized shape.
    /// It refuses to invent page/part/wire field layouts. The included SVG/PNG are
    /// authoritative technical references for manually populating the cloned pages.
    /// </summary>
    public static class SparkLevel27DrawingAssetCreator
    {
        private const string DefaultFolder = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_27_Layout";

        [MenuItem("Tools/Project Spark/Visual Teaching/Create Level 27 Drawing Asset (from template)")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Level 27 Drawing", "Exit Play Mode first.", "OK");
                return;
            }

            UnityEngine.Object selected = Selection.activeObject;
            if (selected == null || selected.GetType().Name != "SparkLevelDrawingAsset")
            {
                EditorUtility.DisplayDialog("Select a template", 
                    "Select an existing WORKING SparkLevelDrawingAsset (for example Level 26 Drawing), then run the menu again. " +
                    "A template is required because the project's drawing-page serialization schema has not been supplied.", "OK");
                return;
            }
            string sourcePath = AssetDatabase.GetAssetPath(selected);
            if (string.IsNullOrEmpty(sourcePath) || !sourcePath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[Level27Drawing] Selected template must be a saved .asset.");
                return;
            }
            string folder = AssetDatabase.IsValidFolder(DefaultFolder) ? DefaultFolder : "Assets";
            string targetPath = AssetDatabase.GenerateUniqueAssetPath(folder + "/Level_27_Drawing.asset");
            if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                Debug.LogError("[Level27Drawing] Could not clone template: " + sourcePath);
                return;
            }
            AssetDatabase.ImportAsset(targetPath);
            UnityEngine.Object clone = AssetDatabase.LoadMainAssetAtPath(targetPath);
            if (clone == null) { Debug.LogError("[Level27Drawing] Clone missing after import."); return; }
            SerializedObject so = new SerializedObject(clone);
            so.Update();
            int changed = 0;
            changed += SetFirstString(so, "Level 27 - Charge the Reservoir", "displayName", "drawingName", "title", "levelName");
            changed += SetFirstString(so, "Build a diode-isolated capacitor reservoir and LED load with a 5 V DC source.", "description", "summary");
            changed += SetFirstString(so, "level_27", "levelId", "levelID");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(clone);
            AssetDatabase.SaveAssets();
            Selection.activeObject = clone;
            EditorGUIUtility.PingObject(clone);
            Debug.Log("[Level27Drawing] Created " + targetPath + ". Updated " + changed +
                " known metadata fields. IMPORTANT: the schematic pages were copied from the template and " +
                "must be rebuilt for Level 27 using Visuals/Level27_Numbered_Wiring.png and Guide/Level27_Creation_Process.pdf. " +
                "No pages were silently guessed.", clone);
            EditorUtility.DisplayDialog("Level 27 Drawing Created", 
                "Created " + targetPath + "\n\nNow edit the cloned pages using the supplied Level 27 schematic. " +
                "Existing template pages are NOT valid Level 27 wiring until updated.", "OK");
        }

        private static int SetFirstString(SerializedObject so, string value, params string[] candidates)
        {
            foreach (string key in candidates)
            {
                SerializedProperty p = so.FindProperty(key);
                if (p == null || p.propertyType != SerializedPropertyType.String) continue;
                p.stringValue = value;
                return 1;
            }
            return 0;
        }
    }
}
#endif
