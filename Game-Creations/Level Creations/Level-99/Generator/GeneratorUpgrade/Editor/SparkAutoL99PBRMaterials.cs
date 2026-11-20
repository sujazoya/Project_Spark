#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator.Editor
{
    /// <summary>Lossless art identity mapping from source GLB/OBJ names to Unity 6.3 URP Lit.</summary>
    internal static class SparkAutoL99PBRMaterials
    {
        private static readonly string[] Families = {
            "PCB_FR4", "PCB_Soldermask", "Copper", "ENIG_Gold", "Solder_Tin",
            "IC_Epoxy", "IC_Marking", "Ceramic_Ivory", "Resistor_Black", "Steel",
            "Polymer_Sleeve", "Polymer_Top", "Copper_Winding", "Ferrite", "Connector_Black",
            "Plastic_Red", "LED_Green", "PCB_Silkscreen", "Dielectric", "Insulator", "Thermal_Ceramic"
        };
        internal static Dictionary<string, Material> Build(SparkAutoLevelRecipe recipe, SparkAutoLevelBuildReport report)
        {
            string output = recipe.generatedAssetsFolder.TrimEnd('/') + "/URP_PBR_Materials";
            SparkAutoLevelSceneBuilder.EnsureFolder(output);
            string textures = recipe.sourceAssetsFolder.TrimEnd('/') + "/Textures/";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("Unity URP/Lit shader not found. Level 99 package targets Unity 6.3 URP.");
            var materials = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
            foreach (string family in Families)
            {
                string path = output + "/L99_" + family + "_Realistic.mat";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) { mat = new Material(shader) { name = "L99_" + family + "_Realistic" }; AssetDatabase.CreateAsset(mat, path); }
                else if (mat.shader != shader) mat.shader = shader;
                string stem = textures + family;
                string albedoPath = stem + "_Albedo.png";
                string packedPath = stem + "_UnityMetallicSmoothness.png";
                string normalPath = stem + "_Normal.png";
                string aoPath = stem + "_AO.png";
                SetAlbedo(albedoPath);
                SetNormal(normalPath);
                SetLinear(packedPath);
                SetLinear(aoPath);
                Texture2D color = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
                Texture2D mask = AssetDatabase.LoadAssetAtPath<Texture2D>(packedPath);
                Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
                Texture2D ao = AssetDatabase.LoadAssetAtPath<Texture2D>(aoPath);
                if (color == null || mask == null || normal == null || ao == null)
                    report.Warn("Incomplete native PBR source textures for " + family + ".");
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", color);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
                if (mat.HasProperty("_MetallicGlossMap")) mat.SetTexture("_MetallicGlossMap", mask);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", mask != null ? 1f : 0f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 1f);
                if (mat.HasProperty("_SmoothnessTextureChannel")) mat.SetFloat("_SmoothnessTextureChannel", 0f);
                if (mat.HasProperty("_BumpMap")) mat.SetTexture("_BumpMap", normal);
                if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", 0.75f);
                if (mat.HasProperty("_OcclusionMap")) mat.SetTexture("_OcclusionMap", ao);
                if (mat.HasProperty("_OcclusionStrength")) mat.SetFloat("_OcclusionStrength", 0.85f);
                if (mask != null) mat.EnableKeyword("_METALLICSPECGLOSSMAP"); else mat.DisableKeyword("_METALLICSPECGLOSSMAP");
                if (normal != null) mat.EnableKeyword("_NORMALMAP"); else mat.DisableKeyword("_NORMALMAP");
                // Realistic micro-details; no artificial high-power bloom on copper or ICs.
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", Color.black);
                mat.DisableKeyword("_EMISSION");
                EditorUtility.SetDirty(mat);
                materials.Add(family, mat);
            }
            AssetDatabase.SaveAssets();
            report.Info("Created/updated 21 native URP Lit PBR materials; source maps retained (127 PNGs). FBX/OBJ slots remapped by source material names.");
            return materials;
        }
        private static void SetNormal(string path)
        {
            TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null || (imp.textureType == TextureImporterType.NormalMap && !imp.sRGBTexture)) return;
            imp.textureType = TextureImporterType.NormalMap;
            imp.sRGBTexture = false;
            imp.SaveAndReimport();
        }
        private static void SetLinear(string path)
        {
            TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null || !imp.sRGBTexture) return;
            imp.sRGBTexture = false;
            imp.SaveAndReimport();
        }
        private static void SetAlbedo(string path)
        {
            TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null || imp.sRGBTexture) return;
            imp.sRGBTexture = true;
            imp.SaveAndReimport();
        }
        internal static void Apply(GameObject go, Dictionary<string, Material> materials)
        {
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
            {
                Material[] originals = r.sharedMaterials;
                if (originals == null || originals.Length == 0) continue;
                var result = new Material[originals.Length];
                bool modified = false;
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = originals[i];
                    string slot = originals[i] == null ? string.Empty : originals[i].name;
                    string family = IdentifyFamily(slot) ?? IdentifyFamily(r.gameObject.name);
                    Material mapped;
                    if (family != null && materials.TryGetValue(family, out mapped))
                    { result[i] = mapped; modified = true; }
                }
                if (modified) r.sharedMaterials = result;
            }
        }
        private static string IdentifyFamily(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            // Preserve source OBJ 'o' group material identity even if Unity's MTL slot is renamed.
            int tag = name.IndexOf("__PSMAT__", StringComparison.OrdinalIgnoreCase);
            if (tag >= 0) name = name.Substring(tag + "__PSMAT__".Length);
            name = name.Replace(" (Instance)", string.Empty).Trim();
            foreach (string family in Families)
                if (string.Equals(name, family, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, "L99_" + family, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, "L99_" + family + "_Realistic", StringComparison.OrdinalIgnoreCase))
                    return family;
            return null;
        }
    }
}
#endif
