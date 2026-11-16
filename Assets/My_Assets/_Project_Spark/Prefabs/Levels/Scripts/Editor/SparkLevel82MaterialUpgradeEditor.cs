#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Creates/assigns preview mission-reliability materials to imported PS_* FBX material regions. Visual art only.</summary>
    public static class SparkLevel82MaterialUpgradeEditor
    {
        private const string Menu = "Tools/Project Spark/Levels/82/Create & Apply HD Materials To Selected Art";
        private const string Folder = "Assets/ProjectSpark/Level82/Materials";
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
            {"plasma",new Color(.67f,.36f,.94f)}, {"uv",new Color(.75f,.45f,.99f)},
            {"magenta",new Color(.93f,.28f,.94f)}, {"storm",new Color(.67f,.28f,.96f,.67f)},
            {"deepglass",new Color(.31f,.60f,.84f,.37f)}, {"emerald",new Color(.19f,.93f,.63f)},
            {"corrupt",new Color(.97f,.36f,.25f)}, {"signal",new Color(.20f,.97f,.90f)},
            {"vault",new Color(.99f,.77f,.37f)}, {"graphite",new Color(.085f,.13f,.19f)},
            {"platinum",new Color(.74f,.82f,.89f)},
            {"absorber",new Color(.07f,.11f,.16f)}, {"absorber2",new Color(.10f,.15f,.20f)},
            {"carbon",new Color(.15f,.17f,.20f)}, {"titanium",new Color(.48f,.55f,.61f)},
            {"rf",new Color(.47f,.26f,.96f)}, {"field",new Color(.25f,.68f,1f,.59f)},
            {"fieldhot",new Color(1f,.41f,.23f)}, {"gridblue",new Color(.08f,.30f,.41f)},
            {"ground",new Color(.84f,.62f,.29f)}, {"chrome",new Color(.79f,.85f,.90f)},
            {"screen",new Color(.10f,.38f,.55f)}, {"warningglass",new Color(.97f,.46f,.23f,.46f)},
            {"hologlass",new Color(.27f,.77f,.97f,.42f)}, {"cabinet",new Color(.11f,.15f,.22f)}
        };

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ProjectSpark")) AssetDatabase.CreateFolder("Assets", "ProjectSpark");
            if (!AssetDatabase.IsValidFolder("Assets/ProjectSpark/Level82")) AssetDatabase.CreateFolder("Assets/ProjectSpark", "Level82");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/ProjectSpark/Level82", "Materials");
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
            // Optional original preview PBR texture pack: copy Textures/ into
            // Assets/ProjectSpark/Level82/Textures before running this menu.
            string mapSet = null;
            switch (key)
            {
                case "steel": case "silver": case "light": case "platinum": case "chrome": case "titanium": mapSet = "AntennaTitanium"; break;
                case "copper": case "ground": mapSet = "GroundCopper"; break;
                case "gold": case "vault": case "amber": mapSet = "GoldContact"; break;
                case "navy": case "dark": case "black": case "graphite": case "cabinet": mapSet = "FilterHousing"; break;
                case "ceramic": case "carbon": mapSet = "FerriteComposite"; break;
                case "iridescent": mapSet = "SpacecraftPCB"; break;
                case "rubber": case "absorber": case "absorber2": mapSet = "AbsorberFoam"; break;
                case "glass": case "frost": case "deepglass": case "field": case "hologlass": mapSet = "EMCBlueDisplay"; break;
                case "granite": mapSet = "AnechoicFloor"; break;
            }
            if (!string.IsNullOrEmpty(mapSet))
            {
                string texDir = "Assets/ProjectSpark/Level82/Textures/";
                Texture2D baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(texDir + mapSet + "_Albedo.png");
                if (baseMap != null)
                {
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", baseMap);
                    if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", baseMap);
                }
                string normalPath = texDir + mapSet + "_Normal.png";
                TextureImporter normalImporter = AssetImporter.GetAtPath(normalPath) as TextureImporter;
                if (normalImporter != null && normalImporter.textureType != TextureImporterType.NormalMap)
                {
                    normalImporter.textureType = TextureImporterType.NormalMap;
                    normalImporter.SaveAndReimport();
                }
                Texture2D normalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
                if (normalMap != null)
                {
                    if (mat.HasProperty("_BumpMap")) mat.SetTexture("_BumpMap", normalMap);
                    if (mat.HasProperty("_NormalMap")) mat.SetTexture("_NormalMap", normalMap);
                    mat.EnableKeyword("_NORMALMAP");
                }
                // R=metallic, G=roughness, B=AO map is supplied separately for
                // manual shader-specific mapping (Unity Standard expects differently packed channels).
            }
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
            if (root == null) { EditorUtility.DisplayDialog("Level 82 Materials", "Select L82_HD_VISUAL_ART first.", "OK"); return; }
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
                if (found) { Undo.RecordObject(renderer, "Assign Level 82 HD preview materials"); renderer.sharedMaterials=assigned; changed++; }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Level 82] Preview materials created. Updated " + changed + " renderers; check transparent materials and actual render pipeline settings manually.");
        }
        [MenuItem(Menu, true)]
        private static bool ValidSelection() => Selection.activeGameObject != null;
    }
}
#endif
