#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator.Editor
{
    /// <summary>
    /// Level-105-only PBR adapter. Uses the original source texture families instead of replacing
    /// them with the V2 generic flat-color materials. OBJ fallback models contain usemtl L105_family.
    /// Each source MetalRoughAO was channel-packed offline into URP Metallic(R)/Smoothness(A) and
    /// Occlusion(G) maps. No material changes are applied to previous levels.
    /// </summary>
    internal static class SparkAutoL105PBRMaterials
    {
        private static readonly string[] Families = {
            "pcb", "mask", "copper", "gold", "silver", "tin", "black", "chip",
            "ceramic", "resistor", "green", "red", "white", "core", "dielectric", "solder"
        };

        internal static Dictionary<string, Material> Build(SparkAutoLevelRecipe recipe, SparkAutoLevelBuildReport report)
        {
            string output = recipe.generatedAssetsFolder.TrimEnd('/') + "/URP_PBR_Materials";
            SparkAutoLevelSceneBuilder.EnsureFolder(output);
            string textures = recipe.sourceAssetsFolder.TrimEnd('/') + "/Textures/";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) throw new InvalidOperationException("No URP/Lit or Standard shader found.");
            var result = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < Families.Length; i++)
            {
                string family = Families[i];
                string path = output + "/L105_" + family + "_PBR.mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(shader) { name = "L105_" + family + "_PBR" };
                    AssetDatabase.CreateAsset(material, path);
                }
                else material.shader = shader;

                EnsureTextureImport(textures + "Unity_Packed/" + family + "_URP_MetallicSmoothness.png", false);
                EnsureTextureImport(textures + "Unity_Packed/" + family + "_URP_Occlusion.png", false);
                EnsureNormalImport(textures + family + "_Normal.png");
                Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(textures + family + "_Albedo.png");
                Texture2D packed = AssetDatabase.LoadAssetAtPath<Texture2D>(textures + "Unity_Packed/" + family + "_URP_MetallicSmoothness.png");
                Texture2D ao = AssetDatabase.LoadAssetAtPath<Texture2D>(textures + "Unity_Packed/" + family + "_URP_Occlusion.png");
                string normalPath = textures + family + "_Normal.png";
                Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
                if (albedo == null || packed == null || ao == null || normal == null)
                {
                    report.Warn("Some original PBR maps are missing for L105_" + family + "; check " + textures);
                }
                if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", albedo);
                if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", albedo);
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
                if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
                if (material.HasProperty("_MetallicGlossMap")) material.SetTexture("_MetallicGlossMap", packed);
                if (material.HasProperty("_OcclusionMap")) material.SetTexture("_OcclusionMap", ao);
                if (material.HasProperty("_BumpMap")) material.SetTexture("_BumpMap", normal);
                if (material.HasProperty("_BumpScale")) material.SetFloat("_BumpScale", 0.38f);
                if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 1f);
                if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 1f);
                if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 1f);
                if (material.HasProperty("_OcclusionStrength")) material.SetFloat("_OcclusionStrength", 0.75f);
                if (packed != null) material.EnableKeyword("_METALLICSPECGLOSSMAP");
                if (ao != null) material.EnableKeyword("_OCCLUSIONMAP");
                if (normal != null) material.EnableKeyword("_NORMALMAP");
                EditorUtility.SetDirty(material);
                result[family] = material;
            }
            AssetDatabase.SaveAssets();
            report.Info("Level 105 PBR: loaded " + result.Count + " original material families with URP-packed metallic, roughness and AO channels.");
            return result;
        }

        private static void EnsureTextureImport(string path, bool srgb)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.sRGBTexture == srgb) return;
            importer.sRGBTexture = srgb;
            importer.SaveAndReimport();
        }

        private static void EnsureNormalImport(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || (importer.textureType == TextureImporterType.NormalMap && !importer.sRGBTexture)) return;
            importer.textureType = TextureImporterType.NormalMap;
            importer.sRGBTexture = false;
            importer.SaveAndReimport();
        }

        internal static void Apply(GameObject root, Dictionary<string, Material> mapping)
        {
            if (root == null || mapping == null) return;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                Material[] original = renderer.sharedMaterials;
                if (original == null || original.Length == 0) continue;
                var updated = new Material[original.Length];
                bool modified = false;
                for (int slot = 0; slot < original.Length; slot++)
                {
                    updated[slot] = original[slot];
                    string originalName = original[slot] != null ? original[slot].name : "";
                    string lookupName = originalName;
                    if (string.IsNullOrEmpty(lookupName) || lookupName.Contains("Default"))
                        lookupName = renderer.gameObject.name;
                    string family = FamilyFromName(lookupName);
                    if (!string.IsNullOrEmpty(family))
                    {
                        Material mat;
                        if (mapping.TryGetValue(family, out mat) && mat != null)
                        {
                            updated[slot] = mat;
                            modified = true;
                        }
                    }
                }
                if (modified) renderer.sharedMaterials = updated;
            }
        }

        private static string FamilyFromName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            string lower = name.ToLowerInvariant();
            for (int i = 0; i < Families.Length; i++)
            {
                string f = Families[i];
                if (lower == "l105_" + f || lower == "l105_" + f + "_pbr" ||
                    lower.Contains("_" + f + "_") || lower.EndsWith("_" + f, StringComparison.Ordinal))
                    return f;
            }
            return null;
        }
    }
}
#endif
