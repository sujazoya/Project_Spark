#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator.Editor
{
    /// <summary>
    /// Keeps the original SMX-4 photorealistic-style texture families. Packs glTF
    /// metallic/roughness/occlusion into URP-compatible maps created by the package builder.
    /// Only generates Level 96 materials; never changes global/project materials.
    /// </summary>
    internal static class SparkAutoL96PBRMaterials
    {
        private static readonly string[] Families = {
            "FR4", "Mask", "Gold", "Copper", "Solder", "Chip", "Ceramic",
            "ResBody", "Connector", "White", "RedPlastic", "LEDGreen",
            "Ink", "Glass", "BoardArtwork"
        };

        internal static Dictionary<string, Material> Build(SparkAutoLevelRecipe recipe,
            SparkAutoLevelBuildReport report)
        {
            string output = recipe.generatedAssetsFolder.TrimEnd('/') + "/URP_PBR_Materials";
            SparkAutoLevelSceneBuilder.EnsureFolder(output);
            string textures = recipe.sourceAssetsFolder.TrimEnd('/') + "/Textures/";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) throw new InvalidOperationException("URP/Lit or Standard shader missing.");
            var materials = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);

            foreach (string family in Families)
            {
                string matPath = output + "/L96_" + family + "_Realistic.mat";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null)
                {
                    mat = new Material(shader) { name = "L96_" + family + "_Realistic" };
                    AssetDatabase.CreateAsset(mat, matPath);
                }
                else mat.shader = shader;

                string source = textures + family;
                string packedPath = textures + "Unity_Packed/" + family + "_URP_MetallicSmoothness.png";
                string aoPath = textures + "Unity_Packed/" + family + "_URP_AO.png";
                string normalPath = source + "_Normal.png";

                EnsureDataTexture(packedPath);
                EnsureDataTexture(aoPath);
                EnsureNormalMap(normalPath);

                Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(source + "_Albedo.png");
                if (family == "BoardArtwork")
                    albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(textures +
                        "SMX4_Top_Silkscreen_FactoryOnly.png");
                Texture2D packed = AssetDatabase.LoadAssetAtPath<Texture2D>(packedPath);
                Texture2D ao = AssetDatabase.LoadAssetAtPath<Texture2D>(aoPath);
                Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
                if (family != "BoardArtwork" &&
                    (albedo == null || packed == null || ao == null || normal == null))
                    report.Warn("Missing source/PBR maps for L96_" + family);

                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", albedo);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", albedo);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
                if (mat.HasProperty("_MetallicGlossMap")) mat.SetTexture("_MetallicGlossMap", packed);
                if (mat.HasProperty("_OcclusionMap")) mat.SetTexture("_OcclusionMap", ao);
                if (mat.HasProperty("_BumpMap")) mat.SetTexture("_BumpMap", normal);
                if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", 0.55f);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", packed != null ? 1f : 0f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.9f);
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.9f);
                if (mat.HasProperty("_OcclusionStrength")) mat.SetFloat("_OcclusionStrength", 0.85f);
                if (packed != null) mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                if (ao != null) mat.EnableKeyword("_OCCLUSIONMAP");
                if (normal != null) mat.EnableKeyword("_NORMALMAP");
                EditorUtility.SetDirty(mat);
                materials[family] = mat;
            }
            AssetDatabase.SaveAssets();
            report.Info("Preserved 14 original PBR material families and silkscreen art. URP metallic, smoothness, normals, AO configured.");
            return materials;
        }

        private static void EnsureDataTexture(string path)
        {
            TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) return;
            bool changed = false;
            if (imp.sRGBTexture) { imp.sRGBTexture = false; changed = true; }
            if (imp.alphaSource != TextureImporterAlphaSource.FromInput)
            { imp.alphaSource = TextureImporterAlphaSource.FromInput; changed = true; }
            if (changed) imp.SaveAndReimport();
        }

        private static void EnsureNormalMap(string path)
        {
            TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) return;
            if (imp.textureType == TextureImporterType.NormalMap && !imp.sRGBTexture) return;
            imp.textureType = TextureImporterType.NormalMap;
            imp.sRGBTexture = false;
            imp.SaveAndReimport();
        }

        internal static void Apply(GameObject root, Dictionary<string, Material> materials)
        {
            if (root == null) return;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                Material[] current = renderer.sharedMaterials;
                if (current == null || current.Length == 0) continue;
                Material[] assigned = new Material[current.Length];
                bool changed = false;
                for (int i = 0; i < current.Length; i++)
                {
                    assigned[i] = current[i];
                    string name = current[i] == null ? string.Empty : current[i].name;
                    string family = Family(name);
                    if (family == null) family = Family(renderer.gameObject.name);
                    Material selected;
                    if (family != null && materials.TryGetValue(family, out selected))
                    { assigned[i] = selected; changed = true; }
                }
                if (changed) renderer.sharedMaterials = assigned;
            }
        }

        private static string Family(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            foreach (string family in Families)
            {
                if (name.Equals("L96_" + family, StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("L96_" + family + "_Realistic", StringComparison.OrdinalIgnoreCase) ||
                    name.IndexOf("MAT_" + family + "__", StringComparison.OrdinalIgnoreCase) >= 0)
                    return family;
            }
            return null;
        }
    }
}
#endif
