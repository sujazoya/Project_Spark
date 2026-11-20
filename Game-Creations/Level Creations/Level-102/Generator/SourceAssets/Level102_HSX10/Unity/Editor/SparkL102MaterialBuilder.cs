#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level102.Editor
{
    public static class SparkL102MaterialBuilder
    {
        [MenuItem("Tools/Project Spark/Levels/102/3. Create Native PBR Materials")]
        public static void Build()
        {
            var guids=AssetDatabase.FindAssets("t:Texture2D");
            const string outdir="Assets/ProjectSparkGenerated/Level102/Materials";
            Directory.CreateDirectory(outdir);
            int count=0;
            foreach(var guid in guids)
            {
                var path=AssetDatabase.GUIDToAssetPath(guid);
                if(!path.EndsWith("_Albedo.png") || !path.Contains("Level102")) continue;
                var matname=Path.GetFileNameWithoutExtension(path).Replace("_Albedo","");
                var shader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
                if(!shader) {Debug.LogError("[L102] No supported lit shader found.");return;}
                var material=new Material(shader){name="L102_"+matname};
                var albedo=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if(material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap",albedo);
                if(material.HasProperty("_MainTex")) material.SetTexture("_MainTex",albedo);
                var roughPath=path.Replace("_Albedo.png","_Roughness.png");
                // Roughness source is retained. Unity metallic/smoothness packing requires pipeline-specific conversion.
                AssetDatabase.CreateAsset(material,outdir+"/L102_"+matname+".mat");count++;
            }
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Debug.Log($"[L102] Created {count} base-color lit materials. Assign material slots and pack pipeline masks before final production use.");
        }
    }
}
#endif
