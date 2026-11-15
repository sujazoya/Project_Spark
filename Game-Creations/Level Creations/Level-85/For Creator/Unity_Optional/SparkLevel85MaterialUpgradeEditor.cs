#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace ProjectSpark.EditorTools
{
    /// <summary>Art-only material/color upgrade. PBR PNG references need UV unwrap before use.</summary>
    public static class SparkLevel85MaterialUpgradeEditor
    {
        private static readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>
        {
            {"structural", new Color(.32f,.45f,.57f)}, {"titanium", new Color(.66f,.72f,.78f)},
            {"dark", new Color(.065f,.09f,.14f)}, {"graphite", new Color(.10f,.14f,.205f)},
            {"copper", new Color(.82f,.40f,.13f)}, {"gold", new Color(.97f,.72f,.24f)},
            {"ceramic", new Color(.8f,.85f,.89f)}, {"glass", new Color(.18f,.66f,.94f,.45f)},
            {"panel", new Color(.026f,.11f,.23f)}, {"pcb", new Color(.06f,.22f,.19f)},
            {"cyan", new Color(.18f,.85f,.98f)}, {"blue", new Color(.20f,.44f,.94f)},
            {"green", new Color(.14f,.94f,.61f)}, {"amber", new Color(1f,.63f,.17f)},
            {"violet", new Color(.62f,.42f,.98f)}, {"red", new Color(1f,.20f,.19f)},
            {"orange", new Color(1f,.40f,.10f)}, {"white", new Color(.90f,.96f,1f)},
            {"solar", new Color(.03f,.095f,.31f)}, {"stars", new Color(.66f,.78f,1f)},
            {"black", new Color(.022f,.026f,.036f)}
        };
        [MenuItem("Tools/Project Spark/Levels/85/Create & Apply HD Materials To Selected Art")]
        public static void Apply()
        {
            var selected = Selection.activeGameObject;
            if (selected == null) { Debug.LogError("[Level85Materials] Select L85_HD_VISUAL_ART."); return; }
            const string path = "Assets/ProjectSpark/Level85/Materials";
            if (!AssetDatabase.IsValidFolder("Assets/ProjectSpark")) AssetDatabase.CreateFolder("Assets", "ProjectSpark");
            if (!AssetDatabase.IsValidFolder("Assets/ProjectSpark/Level85")) AssetDatabase.CreateFolder("Assets/ProjectSpark", "Level85");
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder("Assets/ProjectSpark/Level85", "Materials");
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
            if (shader == null) { Debug.LogError("[Level85Materials] No compatible lit shader detected."); return; }
            var replacements = new Dictionary<string, Material>();
            foreach (var item in Colors)
            {
                string matPath = path + "/PS85_" + item.Key + ".mat";
                var m = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (m == null) { m = new Material(shader) { name = "PS85_" + item.Key }; AssetDatabase.CreateAsset(m, matPath); }
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", item.Value);
                if (m.HasProperty("_Color")) m.SetColor("_Color", item.Value);
                if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", (item.Key == "gold" || item.Key == "copper" || item.Key == "titanium") ? .85f : .10f);
                if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", item.Key == "glass" ? .87f : .63f);
                EditorUtility.SetDirty(m);
                replacements[item.Key] = m;
            }
            int assigned = 0;
            foreach (var render in selected.GetComponentsInChildren<Renderer>(true))
            {
                var old = render.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < old.Length; i++)
                {
                    if (old[i] == null) continue;
                    string source = old[i].name.ToLowerInvariant();
                    foreach (var kv in replacements)
                        if (source.Contains("ps_" + kv.Key) || source == kv.Key.ToLowerInvariant())
                        { old[i] = kv.Value; changed = true; assigned++; break; }
                }
                if (changed) { Undo.RecordObject(render, "Apply Level 85 materials"); render.sharedMaterials = old; EditorUtility.SetDirty(render); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Level85Materials] Assigned " + assigned + " material slots. Texture maps are separate references; mesh UV unwrap is not finished.");
        }
    }
}
#endif
