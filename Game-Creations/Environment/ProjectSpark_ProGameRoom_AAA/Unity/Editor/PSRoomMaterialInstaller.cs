#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectSpark.RoomStudio.EditorTools
{
    /// <summary>Creates real native Unity .mat files from shared PBR maps (URP/HDRP/Built-in).
    /// Works without importing any external package, and does not alter other level materials.</summary>
    public static class PSRoomMaterialInstaller
    {
        public const string GeneratedFolder = "Assets/ProjectSpark/RoomStudio/GeneratedMaterials";
        private static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);

        [MenuItem("Tools/Project Spark/Room Studio/1. Create or Upgrade PBR Materials")]
        public static void CreateMaterialsMenu()
        {
            string root = PSRoomStudioBuilder.FindPackageRoot();
            if (string.IsNullOrEmpty(root)) { Debug.LogError("[PS Room] Cannot find 00_PS_Studio_FloorSlab.fbx"); return; }
            CreateAll(root);
        }

        public static Dictionary<string, Material> CreateAll(string root)
        {
            EnsureFolder("Assets/ProjectSpark");
            EnsureFolder("Assets/ProjectSpark/RoomStudio");
            EnsureFolder(GeneratedFolder);
            Cache.Clear();
            var table = AssetDatabase.LoadAssetAtPath<TextAsset>(root + "/Docs/Materials_Manifest.csv");
            if (table == null) throw new FileNotFoundException("Missing Docs/Materials_Manifest.csv");
            var lines = table.text.Replace("\r", "").Split('\n');
            string rp = PipelineName();
            foreach (var line in lines)
            {
                if (!line.StartsWith("PS_", StringComparison.OrdinalIgnoreCase)) continue;
                var row = line.Split(',');
                if (row.Length < 8) continue;
                string name = row[0].Trim();
                string key = name.Substring(3);
                Color hex = Color.white;
                ColorUtility.TryParseHtmlString(row[1].Trim(), out hex);
                float metallic = Float(row[2], 0.2f);
                float roughness = Float(row[3], .4f);
                float opacity = Float(row[4], 1f);
                Color emission = new Color(Float(row[5], 0f), Float(row[6], 0f), Float(row[7], 0f), 1f);
                bool transparent = opacity < .99f;
                Shader shader;
                if (name.Equals("PS_holo", StringComparison.OrdinalIgnoreCase))
                {
                    string special = rp == "URP" ? "ProjectSpark/RoomStudio/HologramURP" : (rp == "HDRP" ? "HDRP/Unlit" : "ProjectSpark/RoomStudio/HologramBuiltin");
                    shader = Shader.Find(special);
                    if (shader == null) shader = FindLit(rp);
                }
                else shader = FindLit(rp);
                if (shader == null) throw new Exception("No compatible Unity Lit shader was found");

                string matPath = GeneratedFolder + "/" + name + ".mat";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null)
                {
                    mat = new Material(shader) { name = name };
                    AssetDatabase.CreateAsset(mat, matPath);
                }
                else mat.shader = shader;

                Texture2D baseMap = LoadMap(root, name, "BaseColor", false);
                Texture2D mask = LoadMap(root, name, "UnityMaskMap", false);
                Texture2D normal = LoadMap(root, name, "Normal", true);
                Texture2D emap = LoadMap(root, name, "Emission", false);

                SetIf(mat, "_BaseMap", baseMap);
                SetIf(mat, "_BaseColorMap", baseMap);
                SetIf(mat, "_UnlitColorMap", baseMap);
                SetIf(mat, "_MainTex", baseMap);
                SetIf(mat, "_MetallicGlossMap", mask);
                SetIf(mat, "_MaskMap", mask);
                SetIf(mat, "_OcclusionMap", mask);
                SetIf(mat, "_BumpMap", normal);
                SetIf(mat, "_NormalMap", normal);
                SetIf(mat, "_EmissionMap", emap);
                SetIf(mat, "_EmissiveColorMap", emap);
                SetIf(mat, "_EmissionTex", emap);
                Color untinted = new Color(1f,1f,1f,opacity); // BaseColor maps already carry the material colour.
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", untinted);
                if (mat.HasProperty("_UnlitColor")) mat.SetColor("_UnlitColor", untinted);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", untinted);
                if (mat.HasProperty("_Tint")) mat.SetColor("_Tint", new Color(hex.r,hex.g,hex.b, opacity));
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 1f-roughness);
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 1f-roughness);
                if (mat.HasProperty("_GlossMapScale")) mat.SetFloat("_GlossMapScale", 1f);
                if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", 1f);
                if (mat.HasProperty("_NormalScale")) mat.SetFloat("_NormalScale", 1f);
                if (mat.HasProperty("_OcclusionStrength")) mat.SetFloat("_OcclusionStrength", 1f);
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission);
                if (mat.HasProperty("_EmissiveColor")) mat.SetColor("_EmissiveColor", emission);
                if (mat.HasProperty("_EmissionStrength")) mat.SetFloat("_EmissionStrength", Mathf.Max(1f, emission.maxColorComponent));
                mat.EnableKeyword("_NORMALMAP");
                mat.EnableKeyword("_METALLICGLOSSMAP");
                if (emission.maxColorComponent > .0001f)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }
                else mat.DisableKeyword("_EMISSION");
                ApplySurfaceMode(mat, rp, transparent);
                EditorUtility.SetDirty(mat);
                Cache[name] = mat;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[PS Room] Created/updated {Cache.Count} PBR material assets for {rp}. Masks: R metallic, G AO, A smoothness. Normal maps flagged correctly.");
            return new Dictionary<string,Material>(Cache);
        }

        public static void ApplyMaterials(GameObject target, Dictionary<string,Material> mapped)
        {
            foreach (var r in target.GetComponentsInChildren<MeshRenderer>(true))
            {
                Material[] old = r.sharedMaterials;
                if (old == null || old.Length == 0) continue;
                var next = new Material[old.Length];
                for (int i=0; i<old.Length; i++)
                {
                    string m = old[i] == null ? "" : old[i].name;
                    m = m.Replace(" (Instance)", "").Replace(" (Clone)", "");
                    if (m.Contains("::")) m=m.Substring(m.LastIndexOf("::",StringComparison.Ordinal)+2);
                    if (mapped.TryGetValue(m, out var found)) next[i]=found;
                    else
                    {
                        // Model importers sometimes add the mesh name to the material key.
                        foreach (var p in mapped) if (m.Contains(p.Key)) { found = p.Value; break; }
                        next[i] = found != null ? found : old[i];
                    }
                }
                r.sharedMaterials = next;
            }
        }

        private static string PipelineName()
        {
            var rp = GraphicsSettings.currentRenderPipeline;
            string n = rp == null ? "" : rp.GetType().FullName;
            if (n.Contains("HDRenderPipeline")) return "HDRP";
            if (n.Contains("UniversalRenderPipeline")) return "URP";
            return "Builtin";
        }

        private static Shader FindLit(string rp)
        {
            var s = rp == "HDRP" ? Shader.Find("HDRP/Lit") :
                    rp == "URP" ? Shader.Find("Universal Render Pipeline/Lit") :
                    Shader.Find("Standard");
            return s != null ? s : Shader.Find("Standard");
        }

        private static void ApplySurfaceMode(Material mat, string rp, bool transparent)
        {
            if (!transparent)
            {
                mat.renderQueue = (int)RenderQueue.Geometry;
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface",0);
                if (mat.HasProperty("_SurfaceType")) mat.SetFloat("_SurfaceType",0);
                if (mat.HasProperty("_Mode")) mat.SetFloat("_Mode",0);
                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                return;
            }
            mat.renderQueue = (int)RenderQueue.Transparent;
            mat.SetOverrideTag("RenderType", "Transparent");
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1);
            if (mat.HasProperty("_SurfaceType")) mat.SetFloat("_SurfaceType", 1);
            if (mat.HasProperty("_Mode")) mat.SetFloat("_Mode", 3);
            if (mat.HasProperty("_SrcBlend")) mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend")) mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite",0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            if (rp == "Builtin")
            {
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }
        }

        private static Texture2D LoadMap(string root, string material, string suffix, bool normal)
        {
            string path = root + "/Textures/" + material + "_" + suffix + ".png";
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) { Debug.LogWarning("Missing texture: "+path); return null; }
            TextureImporterType typ = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            bool srgb = suffix == "BaseColor" || suffix == "Emission";
            if (imp.textureType != typ || (!normal && imp.sRGBTexture != srgb))
            {
                imp.textureType = typ;
                imp.sRGBTexture = srgb;
                imp.mipmapEnabled = true;
                imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static float Float(string v, float fallback)
        {
            return float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : fallback;
        }
        private static void SetIf(Material m, string name, Texture t)
        {
            if (t != null && m.HasProperty(name)) m.SetTexture(name,t);
        }
        public static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\','/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent,Path.GetFileName(folder));
        }
    }
}
#endif
