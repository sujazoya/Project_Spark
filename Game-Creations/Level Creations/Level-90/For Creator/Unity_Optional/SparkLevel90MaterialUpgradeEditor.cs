#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Assigns source-palette PBR-like materials, not baked production UV textures.</summary>
    public static class SparkLevel90MaterialUpgradeEditor
    {
        [MenuItem("Tools/Project Spark/Levels/90/Create & Apply HD Materials To Selected Art")]
        public static void Upgrade()
        {
            GameObject target=Selection.activeGameObject;
            if(target==null){EditorUtility.DisplayDialog("Level 90", "Select L90_HD_VISUAL_ART in Hierarchy.", "OK");return;}
            const string folder="Assets/ProjectSpark/Level90/Materials";
            if(!AssetDatabase.IsValidFolder("Assets/ProjectSpark")) AssetDatabase.CreateFolder("Assets","ProjectSpark");
            if(!AssetDatabase.IsValidFolder("Assets/ProjectSpark/Level90")) AssetDatabase.CreateFolder("Assets/ProjectSpark","Level90");
            if(!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/ProjectSpark/Level90","Materials");
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null)shader=Shader.Find("HDRP/Lit");
            if(shader==null)shader=Shader.Find("Standard");
            if(shader==null){Debug.LogError("[Level90Materials] No compatible Lit shader found");return;}
            var palette=new Dictionary<string,Color>(){
                {"structural", new Color(0.390f,0.470f,0.570f)},
                {"titanium", new Color(0.720f,0.790f,0.850f)},
                {"dark", new Color(0.035f,0.054f,0.078f)},
                {"graphite", new Color(0.090f,0.130f,0.200f)},
                {"copper", new Color(0.880f,0.470f,0.190f)},
                {"gold", new Color(0.970f,0.690f,0.260f)},
                {"ceramic", new Color(0.920f,0.940f,0.950f)},
                {"glass", new Color(0.150f,0.560f,0.820f)},
                {"panel", new Color(0.018f,0.082f,0.160f)},
                {"pcb", new Color(0.045f,0.220f,0.170f)},
                {"cyan", new Color(0.080f,0.850f,1.000f)},
                {"blue", new Color(0.170f,0.400f,1.000f)},
                {"green", new Color(0.120f,0.920f,0.520f)},
                {"amber", new Color(1.000f,0.630f,0.130f)},
                {"violet", new Color(0.590f,0.390f,1.000f)},
                {"red", new Color(1.000f,0.170f,0.140f)},
                {"orange", new Color(1.000f,0.380f,0.055f)},
                {"white", new Color(0.980f,0.980f,0.950f)},
                {"plasma", new Color(0.180f,0.520f,1.000f)},
                {"solar", new Color(0.070f,0.170f,0.430f)},
                {"radiator", new Color(0.070f,0.110f,0.170f)},
                {"insulation", new Color(0.220f,0.280f,0.320f)},
                {"floor", new Color(0.230f,0.290f,0.370f)},
                {"stars", new Color(0.800f,0.900f,1.000f)},
                {"black", new Color(0.013f,0.019f,0.029f)},
            };
            var materials=new Dictionary<string,Material>();
            foreach(var entry in palette)
            {
                string path=folder+"/PS_"+entry.Key+".mat";
                Material material=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}
                if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",entry.Value);
                if(material.HasProperty("_Color"))material.SetColor("_Color",entry.Value);
                if(material.HasProperty("_Metallic"))material.SetFloat("_Metallic",(entry.Key=="copper"||entry.Key=="titanium"||entry.Key=="gold"||entry.Key=="structural")?.85f:.12f);
                if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",(entry.Key=="glass"||entry.Key=="gold")?.76f:.46f);
                EditorUtility.SetDirty(material);
                materials[entry.Key]=material;
            }
            int applied=0;
            foreach(var ren in target.GetComponentsInChildren<Renderer>(true))
            {
                Material[] current=ren.sharedMaterials;
                for(int i=0;i<current.Length;i++)
                {
                    string name=current[i]!=null?current[i].name.Replace(" (Instance)","").Replace("PS_",""):"";
                    Material candidate;
                    if(materials.TryGetValue(name,out candidate)){current[i]=candidate;applied++;}
                }
                Undo.RecordObject(ren,"Apply Level 90 preview materials");ren.sharedMaterials=current;EditorUtility.SetDirty(ren);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Level90Materials] Replaced "+applied+" material slots with HDRP/URP/Standard preview palette. Procedural texture PNGs require authored UVs, repacking and shader setup; not auto-baked.");
        }
    }
}
#endif
