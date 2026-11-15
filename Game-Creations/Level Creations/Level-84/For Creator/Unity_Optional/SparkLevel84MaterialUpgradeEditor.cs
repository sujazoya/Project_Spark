#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Create preview URP/Standard materials for imported Level84 visual art.
    /// Procedural PNG PBR maps are reference maps; final UV/shader authoring is required.
    /// MRAO: R metallic, G roughness, B ambient occlusion; NOT a standard Unity mask packing.
    /// </summary>
    public static class SparkLevel84MaterialUpgradeEditor
    {
        private const string Menu = "Tools/Project Spark/Levels/84/Create & Apply HD Materials To Selected Art";
        private const string Folder = "Assets/ProjectSpark/Level84/Materials";
        private static readonly Dictionary<string, Color> Palette = new Dictionary<string, Color>()
        {
            {"structural", new Color(.35f,.45f,.56f)}, {"titanium", new Color(.57f,.64f,.68f)},
            {"dark", new Color(.075f,.105f,.15f)}, {"graphite", new Color(.11f,.16f,.2f)},
            {"copper", new Color(.8f,.4f,.16f)}, {"gold", new Color(.95f,.71f,.28f)},
            {"ceramic", new Color(.80f,.83f,.81f)}, {"glass", new Color(.21f,.56f,.75f,.55f)},
            {"panel", new Color(.045f,.13f,.29f)}, {"pcb", new Color(.07f,.24f,.20f)},
            {"cyan", new Color(.22f,.8f,.99f)}, {"blue", new Color(.24f,.47f,.96f)},
            {"green", new Color(.17f,.94f,.66f)}, {"amber", new Color(1f,.64f,.2f)},
            {"violet", new Color(.66f,.38f,1f)}, {"red", new Color(1f,.23f,.2f)},
            {"orange", new Color(1f,.43f,.15f)}, {"white", new Color(.91f,.96f,1f)},
            {"solar", new Color(.055f,.16f,.41f)}, {"stars", new Color(.62f,.78f,1f)}
        };
        private static readonly Dictionary<string, string> MaterialMap = new Dictionary<string, string>()
        {
            {"structural", "BrushedSteel"}, {"titanium", "TitaniumAlloy"},
            {"dark", "BlackCeramic"}, {"graphite", "CarbonComposite"},
            {"copper", "CopperCoils"}, {"gold", "GoldContacts"},
            {"ceramic", "BlackCeramic"}, {"glass", "OpticalGlass"},
            {"panel", "InstrumentPanel"}, {"pcb", "SpacecraftAlloy"},
            {"solar", "SolarCellBlue"}, {"stars", "CelestialHologram"}
        };
        [MenuItem(Menu)]
        public static void Apply()
        {
            Transform root = Selection.activeTransform;
            if (root == null) { EditorUtility.DisplayDialog("Level 84 Materials", "Select L84_HD_VISUAL_ART.", "OK"); return; }
            if (!AssetDatabase.IsValidFolder("Assets/ProjectSpark")) AssetDatabase.CreateFolder("Assets", "ProjectSpark");
            if (!AssetDatabase.IsValidFolder("Assets/ProjectSpark/Level84")) AssetDatabase.CreateFolder("Assets/ProjectSpark", "Level84");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/ProjectSpark/Level84", "Materials");
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) { Debug.LogError("[Level84] No compatible preview shader."); return; }
            var mats = new Dictionary<string, Material>();
            foreach (var kv in Palette)
            {
                string path = Folder + "/L84_PS_" + kv.Key + ".mat";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) { mat = new Material(shader); mat.name = "L84_PS_" + kv.Key; AssetDatabase.CreateAsset(mat, path); }
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", kv.Value);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", kv.Value);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", kv.Key == "gold" || kv.Key == "copper" || kv.Key == "structural" || kv.Key == "titanium" ? .8f : .07f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", kv.Key == "glass" || kv.Key == "gold" ? .72f : .52f);
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", .49f);
                string materialSet;
                if (MaterialMap.TryGetValue(kv.Key, out materialSet))
                {
                    Texture2D tex = FindTexture(materialSet + "_Albedo.png");
                    if (tex != null)
                    {
                        // Original procedural geometry has no authored 2D UV atlas. This is a preview only.
                        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                    }
                }
                if (kv.Key == "glass")
                {
                    if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
                    if (mat.HasProperty("_Mode")) mat.SetFloat("_Mode", 3f);
                    mat.renderQueue = 3000;
                }
                EditorUtility.SetDirty(mat);
                mats.Add(kv.Key, mat);
            }
            int assigned = 0;
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] original = r.sharedMaterials;
                bool modified = false;
                for (int i = 0; i < original.Length; i++)
                {
                    if (original[i] == null) continue;
                    string originalName = original[i].name;
                    foreach (var kv in mats)
                    {
                        if (originalName.IndexOf("PS_" + kv.Key, StringComparison.OrdinalIgnoreCase) < 0) continue;
                        original[i] = kv.Value; modified = true; assigned++; break;
                    }
                }
                if (modified) { Undo.RecordObject(r, "Apply Level84 preview material"); r.sharedMaterials = original; EditorUtility.SetDirty(r); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Level84 Materials] Created/reused " + mats.Count + " preview materials; assigned " + assigned + " slots. Final UV/texture baking and MRAO repacking required.");
        }
        private static Texture2D FindTexture(string filename)
        {
            foreach (string guid in AssetDatabase.FindAssets(System.IO.Path.GetFileNameWithoutExtension(filename) + " t:Texture2D"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(filename, StringComparison.OrdinalIgnoreCase)) return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            return null;
        }
        [MenuItem(Menu, true)] private static bool SelectionValid() { return Selection.activeTransform != null; }
    }
}
#endif
