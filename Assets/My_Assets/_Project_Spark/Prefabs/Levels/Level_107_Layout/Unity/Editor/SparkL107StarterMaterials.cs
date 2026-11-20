#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace ProjectSpark.Level107.Editor
{
    public static class SparkL107StarterMaterials
    {
        [MenuItem("Tools/Project Spark/Levels/107/3. Create PBR Starter Materials")]
        public static void Generate()
        {
            const string folder="Assets/ProjectSpark/Level107/Materials_Generated";
            Directory.CreateDirectory(folder);
            string[][] specs = {
                new []{"PCB_Midnight_Blue","#0B294D","0.1"},
                new []{"Soldermask_Gloss","#103765","0.3"},
                new []{"FR4_Dielectric","#C7A56C","0.1"},
                new []{"Gold_Plating","#B99742","0.85"},
                new []{"Copper","#B77935","0.8"},
                new []{"Solder_Silver","#ADB7C1","0.75"},
                new []{"IC_Epoxy","#161B22","0.0"},
                new []{"Connector_Polymer","#20242A","0.0"},
                new []{"Brushed_Aluminum","#8F969E","0.75"},
                new []{"Flash_U6_Primary","#28252A","0.0"},
                new []{"Flash_U7_Recovery","#222D27","0.0"},
                new []{"Protection_Red","#B03D4B","0.1"}
            };
            var shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null) shader=Shader.Find("Standard");
            if(shader==null) { Debug.LogError("[L107] No supported shader found."); return; }
            foreach(var sp in specs)
            {
                var path=folder+"/"+sp[0]+".mat";
                if(AssetDatabase.LoadAssetAtPath<Material>(path)!=null) continue;
                var mat=new Material(shader);
                Color col=Color.white; ColorUtility.TryParseHtmlString(sp[1],out col);
                if(mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor",col);
                if(mat.HasProperty("_Color")) mat.SetColor("_Color",col);
                var metallic=float.Parse(sp[2],System.Globalization.CultureInfo.InvariantCulture);
                if(mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic",metallic);
                if(mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness",0.48f);
                AssetDatabase.CreateAsset(mat,path);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[L107] Starter materials created. Assign FBX slots and validate your render pipeline.");
        }
    }
}
#endif
