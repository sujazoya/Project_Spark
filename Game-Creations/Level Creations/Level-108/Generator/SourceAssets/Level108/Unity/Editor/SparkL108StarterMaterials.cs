#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
namespace ProjectSpark.Level108.Editor
{
    public static class SparkL108StarterMaterials
    {
        [MenuItem("Tools/Project Spark/Levels/108/2. Generate PBR Starter Materials")]
        public static void Create()
        {
            const string folder = "Assets/ProjectSpark/Level108/Materials_Generated";
            Directory.CreateDirectory(folder);
            string[,] defs = {
                {"PCB_Diagnostic_Blue", "#0B2846", "0.05"},
                {"IC_Epoxy", "#171E26", "0.0"},
                {"Gold_Pads", "#BFA255", "0.85"},
                {"Copper", "#B97E39", "0.8"},
                {"Solder", "#A8B4C1", "0.72"},
                {"Aluminum", "#8E9CA6", "0.8"},
                {"Ceramic", "#D3C9B4", "0.0"},
                {"FR4_Core", "#9B8053", "0.0"},
                {"Diagnostic_Orange", "#E88C42", "0.1"},
                {"Fault_Red", "#BB4554", "0.1"},
                {"Diagnostic_Cyan", "#38ADD0", "0.1"}
            };
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) { Debug.LogError("[L108] No supported shader found"); return; }
            for (int i=0;i<defs.GetLength(0);i++)
            {
                string path = folder+"/"+defs[i,0]+".mat";
                if (AssetDatabase.LoadAssetAtPath<Material>(path)!=null) continue;
                var m = new Material(shader);
                if (ColorUtility.TryParseHtmlString(defs[i,1], out var col))
                {
                    if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", col);
                    if (m.HasProperty("_Color")) m.SetColor("_Color", col);
                }
                float met=float.Parse(defs[i,2], System.Globalization.CultureInfo.InvariantCulture);
                if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",met);
                if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",0.45f);
                AssetDatabase.CreateAsset(m,path);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[L108] Art-only starter materials created; assign actual PBR maps and validate FBX slots.");
        }
    }
}
#endif
