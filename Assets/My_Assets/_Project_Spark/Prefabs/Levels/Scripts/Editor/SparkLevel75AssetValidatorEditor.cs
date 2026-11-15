#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Art-only check of imported Level 75 equipment and pivot names. Not a gameplay validator:
    /// does not create or verify 106 player wires, 164 SparkTerminals or six objectives.
    /// </summary>
    public static class SparkLevel75AssetValidatorEditor
    {
        [MenuItem("Tools/Project Spark/Levels/75/Validate Imported HD Art")]
        public static void Validate()
        {
            string[] guids = AssetDatabase.FindAssets("t:Model");
            HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
            Dictionary<string, string> paths = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!Path.GetExtension(path).Equals(".fbx", StringComparison.OrdinalIgnoreCase)) continue;
                string name = Path.GetFileNameWithoutExtension(path);
                if (name.Length < 7) continue;
                if (!name.Contains("_L75_")) continue;
                if (!names.Add(name)) Debug.LogWarning("[L75] Ambiguous duplicate imported FBX name: " + name);
                else paths.Add(name, path);
            }
            List<string> missing = new List<string>();
            for (int i = 0; i < 58; i++)
            {
                string prefix = i.ToString("D2") + "_L75_";
                bool found = false;
                foreach (string n in names)
                    if (n.StartsWith(prefix, StringComparison.Ordinal)) { found = true; break; }
                if (!found) missing.Add(prefix + "*.fbx");
            }
            if (missing.Count > 0)
            {
                Debug.LogError("[L75] Missing model slots: " + string.Join(", ", missing));
                EditorUtility.DisplayDialog("Level 75 Art Validation", missing.Count + " model slots missing. See Console.", "OK");
                return;
            }
            GameObject selected = Selection.activeGameObject;
            Transform art = selected != null ? selected.GetComponentsInChildren<Transform>(true).FirstOrDefault(item=>item.name=="L75_HD_VISUAL_ART") : null;
            if (art == null)
            {
                Debug.Log("[L75] All 58 indexed FBX slots located. Select L75_HD_VISUAL_ART to validate placed geometry and pivot transforms.");
                EditorUtility.DisplayDialog("Level 75 Art Validation", "58 FBX slots found. Select the placed art root and rerun for scene bounds.", "OK");
                return;
            }
            HashSet<string> pivots = new HashSet<string>(StringComparer.Ordinal);
            bool fitted=false;
            foreach (Transform t in art.GetComponentsInChildren<Transform>(true))
            {
                int marker=t.name.IndexOf("ANIM_",StringComparison.Ordinal);
                if(marker>=0){pivots.Add(t.name.Substring(marker));fitted|=marker>0;}
            }
            Bounds bounds = default(Bounds);
            bool hasBounds = false;
            foreach (Renderer r in art.GetComponentsInChildren<Renderer>(true))
            {
                if (!hasBounds) { bounds = r.bounds; hasBounds = true; }
                else bounds.Encapsulate(r.bounds);
            }
            if (!hasBounds) { Debug.LogError("[L75] No placed renderers found."); return; }
            float width = bounds.size.x;
            bool scaleOK = fitted ? width>0&&pivots.Count==38 : Mathf.Abs(width - 19.2f) < 1f;
            string report = "FBX slots: 58. Named ANIM_ pivots found: " + pivots.Count +
                ". Placed world bounds width: " + width.ToString("F2") + (fitted ? " units at breadboard fit. Expected 38 motion groups." : " units. Expected ~19.2 metres.");
            if (!scaleOK) Debug.LogWarning("[L75] FBX import scale likely wrong: " + report);
            else Debug.Log("[L75] Art validated: " + report);
            EditorUtility.DisplayDialog("Level 75 Art Validation", report +
                "\nThis checks geometry only, not circuit/gameplay functionality.", "OK");
        }
    }
}
#endif
