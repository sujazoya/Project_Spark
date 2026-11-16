#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Optional stylized PBR material-color assignment only.
    /// Procedural PNG reference maps need UVs and explicit shader-channel mapping.</summary>
    public static class SparkLevel86MaterialUpgradeEditor
    {
        [MenuItem("Tools/Project Spark/Levels/86/Create & Apply HD Materials To Selected Art")]
        public static void Upgrade()
        {
            var target = Selection.activeGameObject;
            if (target == null)
            {
                EditorUtility.DisplayDialog("Level 86", "Select the L86_HD_VISUAL_ART GameObject.", "OK");
                return;
            }
            const string folder = "Assets/ProjectSpark/Level86/Materials";
            if (!AssetDatabase.IsValidFolder("Assets/ProjectSpark")) AssetDatabase.CreateFolder("Assets", "ProjectSpark");
            if (!AssetDatabase.IsValidFolder("Assets/ProjectSpark/Level86")) AssetDatabase.CreateFolder("Assets/ProjectSpark", "Level86");
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/ProjectSpark/Level86", "Materials");
            var colors = new Dictionary<string,Color>()
            {
                {"structural", new Color(.35f,.46f,.57f)}, {"titanium", new Color(.63f,.70f,.76f)},
                {"dark",new Color(.060f,.078f,.12f)}, {"graphite",new Color(.12f,.15f,.20f)},
                {"copper",new Color(.83f,.39f,.15f)}, {"gold",new Color(.95f,.69f,.22f)},
                {"ceramic",new Color(.85f,.88f,.88f)}, {"glass",new Color(.18f,.61f,.86f,.48f)},
                {"panel",new Color(.025f,.115f,.21f)}, {"pcb",new Color(.04f,.22f,.17f)},
                {"cyan",new Color(.16f,.83f,.96f)}, {"blue",new Color(.18f,.39f,.85f)},
                {"green",new Color(.10f,.91f,.56f)}, {"amber",new Color(1f,.59f,.13f)},
                {"violet",new Color(.64f,.43f,.96f)}, {"red",new Color(1f,.16f,.14f)},
                {"orange",new Color(1f,.38f,.07f)}, {"white",new Color(.95f,.96f,.94f)},
                {"radiator",new Color(.080f,.125f,.18f)}, {"stars",new Color(.77f,.87f,1f)},
                {"black",new Color(.025f,.03f,.04f)}
            };
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("HDRP/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) { Debug.LogError("[Level86Materials] No supported Lit shader found."); return; }
            var map = new Dictionary<string,Material>();
            foreach (var entry in colors)
            {
                string path = folder + "/PS_" + entry.Key + ".mat";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
                mat.color = entry.Value;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", entry.Value);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", entry.Value);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", entry.Key=="gold"||entry.Key=="titanium"||entry.Key=="copper"? .85f : .18f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", entry.Key=="gold"||entry.Key=="titanium" ? .68f : .38f);
                EditorUtility.SetDirty(mat);
                map[entry.Key] = mat;
            }
            int replaced = 0;
            foreach (Renderer r in target.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int j=0; j<mats.Length; j++)
                {
                    string name = mats[j] != null ? mats[j].name.Replace(" (Instance)", "").Replace("PS_", "") : "";
                    Material match;
                    if (map.TryGetValue(name, out match)) { mats[j] = match; replaced++; }
                }
                Undo.RecordObject(r, "Apply Level 86 reference materials");
                r.sharedMaterials = mats;
                EditorUtility.SetDirty(r);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Level86Materials] Assigned " + replaced + " material slots. This is COLOR/PBR preview; authored UV and final AAA bakes are not included.");
        }
    }
}
#endif
