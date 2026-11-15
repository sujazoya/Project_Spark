#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Creates/assigns preview dielectric/PECVD/UV materials to imported PS_* FBX material regions. Visual art only.</summary>
    public static class SparkLevel78MaterialUpgradeEditor
    {
        private const string Menu = "Tools/Project Spark/Levels/78/Create & Apply HD Materials To Selected Art";
        private const string Folder = "Assets/ProjectSpark/Level78/Materials";
        private static readonly Dictionary<string, Color> Palette = new Dictionary<string, Color>()
        {
            {"copper", new Color(.86f,.49f,.24f)}, {"gold",new Color(.94f,.74f,.32f)},
            {"steel",new Color(.33f,.43f,.53f)}, {"silver",new Color(.79f,.86f,.89f)},
            {"light",new Color(.62f,.73f,.80f)}, {"navy",new Color(.05f,.11f,.19f)},
            {"dark",new Color(.10f,.17f,.23f)}, {"black",new Color(.035f,.07f,.11f)},
            {"cyan",new Color(.12f,.81f,.96f)}, {"ice",new Color(.58f,.94f,.98f)},
            {"teal",new Color(.18f,.76f,.71f)}, {"slurry",new Color(.22f,.77f,.89f,.60f)},
            {"glass",new Color(.34f,.73f,.86f,.38f)}, {"purpleglass",new Color(.58f,.32f,.86f,.47f)},
            {"amber",new Color(1f,.66f,.21f)}, {"green",new Color(.17f,.85f,.55f)},
            {"blue",new Color(.12f,.42f,.83f)}, {"violet",new Color(.58f,.31f,.93f)},
            {"red",new Color(.92f,.24f,.28f)}, {"white",new Color(.92f,.96f,.97f)},
            {"ceramic",new Color(.91f,.89f,.82f)}, {"iridescent",new Color(.41f,.73f,.89f)},
            {"rubber",new Color(.16f,.18f,.20f)}, {"pad",new Color(.20f,.22f,.29f)},
            {"padgroove",new Color(.07f,.11f,.16f)}, {"warning",new Color(.96f,.35f,.16f)},
            {"frost",new Color(.77f,.87f,.93f)}, {"warm",new Color(.98f,.72f,.39f)},
            {"laser",new Color(.29f,.95f,.91f)}, {"resist",new Color(.89f,.45f,.72f)},
            {"circuit",new Color(.41f,.98f,.72f)}, {"granite",new Color(.34f,.43f,.47f)},
            {"plasma",new Color(.67f,.36f,.94f)}, {"uv",new Color(.75f,.45f,.99f)}
        };

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ProjectSpark")) AssetDatabase.CreateFolder("Assets", "ProjectSpark");
            if (!AssetDatabase.IsValidFolder("Assets/ProjectSpark/Level78")) AssetDatabase.CreateFolder("Assets/ProjectSpark", "Level78");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/ProjectSpark/Level78", "Materials");
        }
        private static Material GetOrCreate(string key)
        {
            string name = "PS_" + key;
            string path = Folder + "/" + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("HDRP/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) throw new InvalidOperationException("No supported Lit or Standard shader found.");
            mat = new Material(shader); mat.name = name;
            Color col = Palette[key];
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", col);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", key == "copper" || key == "gold" || key == "silver" || key == "steel" ? .86f : .12f);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", .79f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", .79f);
            if (col.a < .99f)
            {
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface",1f);
                if (mat.HasProperty("_Mode")) mat.SetFloat("_Mode",3f);
                mat.renderQueue = 3000;
            }
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }
        [MenuItem(Menu)]
        public static void Apply()
        {
            GameObject root = Selection.activeGameObject;
            if (root == null) { EditorUtility.DisplayDialog("Level 78 Materials", "Select L78_HD_VISUAL_ART first.", "OK"); return; }
            EnsureFolders();
            var mats = new Dictionary<string, Material>();
            foreach (string key in Palette.Keys) mats.Add(key, GetOrCreate(key));
            int changed=0;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] assigned = renderer.sharedMaterials;
                bool found = false;
                for (int i=0; i<assigned.Length; i++)
                {
                    if (assigned[i] == null) continue;
                    string importedName = assigned[i].name;
                    string key = importedName.StartsWith("PS_", StringComparison.OrdinalIgnoreCase) ? importedName.Substring(3) : importedName;
                    int instanceTag = key.IndexOf(" (Instance)", StringComparison.Ordinal);
                    if (instanceTag >= 0) key = key.Substring(0, instanceTag);
                    if (!mats.TryGetValue(key.ToLowerInvariant(), out Material replacement)) continue;
                    assigned[i] = replacement; found = true;
                }
                if (found) { Undo.RecordObject(renderer, "Assign Level 78 HD preview materials"); renderer.sharedMaterials=assigned; changed++; }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Level 78] Preview materials created. Updated " + changed + " renderers; check transparent materials and actual render pipeline settings manually.");
        }
        [MenuItem(Menu, true)]
        private static bool ValidSelection() => Selection.activeGameObject != null;
    }
}
#endif
