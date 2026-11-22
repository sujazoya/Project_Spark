using UnityEngine;
using UnityEditor;
using System.IO;
namespace ProjectSpark.Level32.B850I.Editor
{
    public static class SparkL32MaterialTools
    {
        [MenuItem("Project Spark/Level 32 B850I/Create and Apply URP Materials to Selection")]
        private static void Apply()
        {
            var root = Selection.activeGameObject;
            if (!root) { Debug.LogWarning("Select the workshop root first."); return; }
            ApplyTo(root, false);
        }
        public static void ApplyTo(GameObject root, bool textured) {
            const string path = "Assets/ProjectSpark/Level32/B850ILightning/Materials";
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\','/'), "Materials");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) throw new System.InvalidOperationException("URP Lit shader not found.");
            string[] names = {"PCB","Silver","Steel","Black","White","Gold","Copper","Gray","Blue","Red","Green","Solder"};
            Color[] colors = {new Color(.18f,.20f,.21f),new Color(.70f,.73f,.75f),new Color(.42f,.45f,.48f),new Color(.035f,.039f,.045f),new Color(.83f,.85f,.86f),new Color(.8f,.58f,.19f),new Color(.52f,.25f,.10f),new Color(.27f,.29f,.31f),new Color(.04f,.22f,.65f),new Color(.55f,.05f,.08f),new Color(.08f,.32f,.16f),new Color(.58f,.60f,.63f)};
            var mats = new Material[names.Length];
            for(int i=0;i<names.Length;i++) {
                string file=path+"/L32_"+names[i]+(textured?"_PBR":"")+".mat";
                mats[i]=AssetDatabase.LoadAssetAtPath<Material>(file);
                if(!mats[i]) { mats[i]=new Material(shader); mats[i].SetColor("_BaseColor",colors[i]); bool metal=i==1||i==2||i==5||i==6||i==11; mats[i].SetFloat("_Metallic",metal?.85f:0f); mats[i].SetFloat("_Smoothness",metal?.65f:.28f); AssetDatabase.CreateAsset(mats[i],file); }
                if(textured) { string tex=Path.GetDirectoryName(path).Replace('\\','/')+"/Textures/"+names[i]; mats[i].SetColor("_BaseColor",Color.white); mats[i].SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(tex+"_BaseColor.png")); mats[i].SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(tex+"_MetallicSmoothness.png")); mats[i].EnableKeyword("_METALLICSPECGLOSSMAP"); mats[i].SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(tex+"_Normal.png")); mats[i].EnableKeyword("_NORMALMAP"); mats[i].SetFloat("_Smoothness",1f); EditorUtility.SetDirty(mats[i]); }
            }
            foreach(var r in root.GetComponentsInChildren<Renderer>(true)) {
                var array=r.sharedMaterials; bool changed=false;
                for(int j=0;j<array.Length;j++) for(int i=0;i<names.Length;i++) if((array[j] && (array[j].name==names[i]||array[j].name.EndsWith("::"+names[i])))||r.gameObject.name.EndsWith("_"+names[i])) { array[j]=mats[i]; changed=true; break; }
                if(changed) { Undo.RecordObject(r,"L32 URP Materials"); r.sharedMaterials=array; EditorUtility.SetDirty(r); }
            }
            AssetDatabase.SaveAssets();
        }
    }
}
