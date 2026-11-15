#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace ProjectSpark.EditorTools
{
    /// <summary>Color/material preview helper only; procedural art has no finished PBR texture maps.</summary>
    public static class SparkLevel74MaterialUpgradeEditor
    {
        private const string Folder = "Assets/ProjectSpark/Level74/Materials";
        private static readonly string[] Names = { "navy", "steel", "light", "silver", "dark", "black", "cyan", "blue", "ice", "copper", "amber", "red", "green", "white", "glass", "frost", "rubber", "gold", "violet", "plasma", "purpleglass", "teal", "ceramic", "iridescent", "warning", "pad", "padgroove", "slurry", "granite", "uv", "resist", "warm", "laser", "circuit" };
        private static readonly string[] Colors = { "#0D1D2F", "#47657C", "#96B3C3", "#C2CFD6", "#1D303F", "#0C1620", "#27D7F6", "#1B76D5", "#A6E8F9", "#DD813E", "#FCB344", "#EE484F", "#32E095", "#E5F0F5", "#55B2DE", "#CDE5F4", "#2D3034", "#E9BC56", "#A65CF2", "#AE5AF2", "#965BE6", "#3FD6CA", "#E0DED3", "#66C5EF", "#FF7043", "#3C404D", "#1A232F", "#44CCEE", "#576771", "#C078FF", "#F072B6", "#FFC265", "#55F8F4", "#7BFED1" };
        private static readonly bool[] Transparent = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, true, true, false, false, false, false, false, false, true, false, true, false, false, false, false };
        [MenuItem("Tools/Project Spark/Levels/74/Create & Apply HD Materials To Selected Art")]
        public static void Apply()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null) { EditorUtility.DisplayDialog("Level 74 Materials", "Select L74_HD_VISUAL_ART (or its root).", "OK"); return; }
            EnsureFolder(Folder);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
            if (shader == null) { Debug.LogError("[L74] No URP, HDRP or Standard Lit shader available."); return; }
            Material[] palette = new Material[Names.Length];
            for (int i = 0; i < Names.Length; i++)
            {
                string assetPath = Folder + "/PS_" + Names[i] + ".mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (material == null) { material = new Material(shader); material.name = "PS_" + Names[i]; AssetDatabase.CreateAsset(material, assetPath); }
                Undo.RecordObject(material, "Level 74 material settings");
                Color c;
                if (!ColorUtility.TryParseHtmlString(Colors[i], out c)) c = Color.gray;
                if (Transparent[i]) c.a = Names[i] == "glass" ? .30f : .55f;
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", c);
                if (material.HasProperty("_Color")) material.SetColor("_Color", c);
                bool metal = Names[i] == "steel" || Names[i] == "silver" || Names[i] == "light" || Names[i] == "copper" || Names[i] == "gold";
                if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metal ? .86f : .12f);
                if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", metal ? .84f : .56f);
                if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", metal ? .84f : .56f);
                if (Transparent[i])
                {
                    if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
                    if (material.HasProperty("_Mode")) material.SetFloat("_Mode", 3f);
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.renderQueue = (int)RenderQueue.Transparent;
                }
                EditorUtility.SetDirty(material);
                palette[i] = material;
            }
            int count = 0;
            foreach (Renderer renderer in selected.GetComponentsInChildren<Renderer>(true))
            {
                Material[] old = renderer.sharedMaterials;
                Material[] next = (Material[])old.Clone();
                bool changed = false;
                for (int j = 0; j < next.Length; j++)
                {
                    if (old[j] == null) continue;
                    for (int i = 0; i < Names.Length; i++)
                        if (old[j].name.IndexOf("PS_" + Names[i], StringComparison.OrdinalIgnoreCase) >= 0)
                        { if (next[j] != palette[i]) { next[j] = palette[i]; changed = true; } break; }
                }
                if (changed) { Undo.RecordObject(renderer, "Update Level 74 renderer"); renderer.sharedMaterials = next; count++; }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[L74 Materials] " + palette.Length + " materials prepared; " + count + " renderers updated. Adjust glass and emissions for your pipeline.");
        }
        private static void EnsureFolder(string path)
        {
            string[] s = path.Split('/'); string cur = s[0];
            for (int i = 1; i < s.Length; i++)
            {
                string next = cur + "/" + s[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, s[i]);
                cur = next;
            }
        }
    }
}
#endif
