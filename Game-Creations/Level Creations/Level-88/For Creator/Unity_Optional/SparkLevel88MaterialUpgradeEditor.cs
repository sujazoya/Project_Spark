#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Assigns source-palette PBR-like materials, not baked production UV textures.</summary>
    public static class SparkLevel88MaterialUpgradeEditor
    {
        [MenuItem("Tools/Project Spark/Levels/88/Create & Apply HD Materials To Selected Art")]
        public static void Upgrade()
        {
            GameObject target=Selection.activeGameObject;
            if(target==null){EditorUtility.DisplayDialog("Level 88", "Select L88_HD_VISUAL_ART in Hierarchy.", "OK");return;}
            const string folder="Assets/ProjectSpark/Level88/Materials";
            if(!AssetDatabase.IsValidFolder("Assets/ProjectSpark")) AssetDatabase.CreateFolder("Assets","ProjectSpark");
            if(!AssetDatabase.IsValidFolder("Assets/ProjectSpark/Level88")) AssetDatabase.CreateFolder("Assets/ProjectSpark","Level88");
            if(!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/ProjectSpark/Level88","Materials");
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null)shader=Shader.Find("HDRP/Lit");
            if(shader==null)shader=Shader.Find("Standard");
            if(shader==null){Debug.LogError("[Level88Materials] No compatible Lit shader found");return;}
            var palette=new Dictionary<string,Color>(){
                {"structural", new Color(0.350f,0.430f,0.520f)},
                {"titanium", new Color(0.660f,0.750f,0.820f)},
                {"dark", new Color(0.045f,0.065f,0.090f)},
                {"graphite", new Color(0.080f,0.120f,0.170f)},
                {"copper", new Color(0.890f,0.470f,0.170f)},
                {"gold", new Color(0.970f,0.650f,0.200f)},
                {"ceramic", new Color(0.890f,0.930f,0.910f)},
                {"glass", new Color(0.200f,0.650f,0.840f)},
                {"panel", new Color(0.012f,0.110f,0.200f)},
                {"pcb", new Color(0.025f,0.250f,0.200f)},
                {"cyan", new Color(0.070f,0.820f,0.980f)},
                {"blue", new Color(0.170f,0.430f,0.930f)},
                {"green", new Color(0.080f,0.900f,0.490f)},
                {"amber", new Color(1.000f,0.600f,0.120f)},
                {"violet", new Color(0.630f,0.390f,0.990f)},
                {"red", new Color(0.990f,0.140f,0.120f)},
                {"orange", new Color(1.000f,0.380f,0.090f)},
                {"white", new Color(0.970f,0.970f,0.910f)},
                {"water", new Color(0.110f,0.540f,0.930f)},
                {"adsorbent", new Color(0.150f,0.680f,0.890f)},
                {"foliage", new Color(0.100f,0.520f,0.170f)},
                {"foliage2", new Color(0.170f,0.640f,0.240f)},
                {"floor", new Color(0.230f,0.300f,0.390f)},
                {"stars", new Color(0.850f,0.900f,1.000f)},
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
                Undo.RecordObject(ren,"Apply Level 88 preview materials");ren.sharedMaterials=current;EditorUtility.SetDirty(ren);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Level88Materials] Replaced "+applied+" material slots with HDRP/URP/Standard preview palette. Procedural texture PNGs require authored UVs, repacking and shader setup; not auto-baked.");
        }
    }
}
#endif
