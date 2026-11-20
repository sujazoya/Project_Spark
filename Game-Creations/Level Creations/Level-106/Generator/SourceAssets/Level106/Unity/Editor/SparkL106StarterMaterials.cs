#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

namespace ProjectSpark.Level106.Editor
{
    public static class SparkL106StarterMaterials
    {
        [MenuItem("Tools/Project Spark/Levels/106/3. Create PBR Starter Materials")]
        public static void Create()
        {
            var ids = AssetDatabase.FindAssets("L106_PCB_BaseColor t:Texture2D");
            if (ids.Length == 0) { Debug.LogWarning("[L106] Import textures first."); return; }
            string texturePath = AssetDatabase.GUIDToAssetPath(ids[0]);
            string dir = Path.GetDirectoryName(texturePath).Replace('\\','/');
            string output = dir + "/../GeneratedMaterials";
            if (!AssetDatabase.IsValidFolder(output)) {
                var parent = Path.GetDirectoryName(output).Replace('\\','/');
                if (!AssetDatabase.IsValidFolder(parent)) parent = dir;
                AssetDatabase.CreateFolder(parent, "GeneratedMaterials");
                output = parent + "/GeneratedMaterials";
            }
            foreach (string family in new [] {"PCB","GOLD","COPPER","SOLDER","EPOXY","CERAMIC","BLACK","ALUMINUM","POLYMER","PLASTIC","RED","LED_GREEN","LED_BLUE"}) {
                var s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (s == null) continue;
                var m = new Material(s);
                string tex = dir + "/L106_" + family + "_BaseColor.png";
                var baseTex = AssetDatabase.LoadAssetAtPath<Texture2D>(tex);
                if (baseTex != null) {
                    if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", baseTex);
                    if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", baseTex);
                }
                AssetDatabase.CreateAsset(m, output + "/L106_" + family + ".mat");
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[L106] Starter materials generated; assign FBX slots and configure metallic/smoothness for your render pipeline.");
        }
    }
}
#endif
