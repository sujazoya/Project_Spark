#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator.Editor
{
    /// <summary>
    /// Deterministic URP material overrides for generated scenes. Uses supplied base-color maps and
    /// conservative metallic/smoothness scalars. Standalone roughness textures need channel-packing
    /// before use as Unity URP metallic+smoothness maps; never treat roughness as smoothness directly.
    /// </summary>
    internal static class SparkAutoURPMaterialBuilder
    {
        private static readonly string[] Names = {
            "PCB", "COPPER", "GOLD", "ALUMINUM", "DARK_METAL", "EPOXY", "CERAMIC",
            "SOLDER", "POLYMER", "SILK", "PAD", "RUBBER", "BLUE", "CYAN",
            "GREEN", "RED", "PURPLE", "WHITE", "BLACK"
        };
        private static readonly float[] Metallic = {
            0.05f, 0.85f, 0.95f, 0.85f, 0.75f, 0, 0, 0.80f, 0, 0, 0,
            0, 0, 0.06f, 0.03f, 0, 0, 0, 0
        };
        private static readonly float[] Smoothness = {
            0.23f, 0.64f, 0.75f, 0.69f, 0.58f, 0.26f, 0.34f, 0.72f, 0.35f,
            0.24f, 0.13f, 0.15f, 0.31f, 0.40f, 0.42f, 0.38f, 0.32f, 0.35f, 0.20f
        };

        public static Dictionary<string, Material> Build(SparkAutoLevelRecipe recipe, SparkAutoLevelBuildReport report)
        {
            string folder = recipe.generatedAssetsFolder.TrimEnd('/') + "/URP_PBR_Materials";
            SparkAutoLevelSceneBuilder.EnsureFolder(folder);
            string texFolder = recipe.sourceAssetsFolder.TrimEnd('/') + "/Textures/";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
                report.Warn("URP/Lit shader absent; generated materials use Standard shader fallback");
            }
            if (shader == null) { report.Error("No supported material shader found"); return new Dictionary<string, Material>(); }

            var map = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < Names.Length; i++)
            {
                string name = Names[i];
                string assetPath = folder + "/" + name + "_AutoPBR.mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (material == null)
                {
                    material = new Material(shader) { name = name + "_AutoPBR" };
                    AssetDatabase.CreateAsset(material, assetPath);
                }
                else material.shader = shader;
                Texture2D baseMap = FindTexture(texFolder, name + "_BaseColor");
                if (baseMap != null)
                {
                    if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", baseMap);
                    if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", baseMap);
                    if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
                    if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
                }
                else report.Warn("Starter base-color texture absent for " + name);
                if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", Metallic[i]);
                if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", Smoothness[i]);
                if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", Smoothness[i]);
                if (name == "CYAN" || name == "RED")
                {
                    // Highlight materials are separate from authoritative electrical/thermal state.
                    if (material.HasProperty("_EmissionColor"))
                    {
                        material.EnableKeyword("_EMISSION");
                        material.SetColor("_EmissionColor", name == "CYAN" ? new Color(0.005f, 0.15f, 0.19f) : new Color(0.14f, 0.005f, 0.005f));
                    }
                }
                EditorUtility.SetDirty(material);
                map.Add(name, material);
            }
            AssetDatabase.SaveAssets();
            report.Info("Generated/updated " + map.Count + " starter material overrides (not a completed AAA PBR texture pass)");
            return map;
        }

        public static void Apply(GameObject root, Dictionary<string, Material> available)
        {
            if (available == null || root == null) return;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] old = renderers[i].sharedMaterials;
                if (old == null || old.Length == 0) continue;
                bool changed = false;
                var updated = new Material[old.Length];
                for (int slot = 0; slot < old.Length; slot++)
                {
                    updated[slot] = old[slot];
                    if (old[slot] == null) continue;
                    string n = old[slot].name.ToUpperInvariant().Replace(" (INSTANCE)", "");
                    for (int k = 0; k < Names.Length; k++)
                    {
                        string key = Names[k];
                        if (n == key || n.EndsWith("_" + key, StringComparison.Ordinal) || n.EndsWith("_" + key + " (MATERIAL)", StringComparison.Ordinal))
                        { updated[slot] = available[key]; changed = true; break; }
                    }
                }
                if (changed) renderers[i].sharedMaterials = updated;
            }
        }

        private static Texture2D FindTexture(string folder, string search)
        {
            string direct = folder + search + ".png";
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(direct);
            if (tex != null) return tex;
            string[] ids = AssetDatabase.FindAssets(search + " t:Texture2D", new[] { folder.TrimEnd('/') });
            for (int i = 0; i < ids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(ids[i]);
                if (Path.GetFileNameWithoutExtension(path).EndsWith(search, StringComparison.OrdinalIgnoreCase))
                    return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            return null;
        }
    }
}
#endif
