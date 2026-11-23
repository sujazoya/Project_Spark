#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Assigns source-palette PBR-like materials, not baked production UV textures.</summary>
    public static class SparkLevel89MaterialUpgradeEditor
    {
        public static void Apply(GameObject target, string parent)
        {
            const string subfolder = "Materials";
            string folder = parent + "/" + subfolder;
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(parent, subfolder);
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            
            if(shader==null)shader=Shader.Find("Standard");
            if(shader==null){Debug.LogError("[Level89Materials] No compatible Lit shader found");return;}
            var palette=new Dictionary<string,Color>(){
                {"structural", new Color(0.400f,0.500f,0.610f)},
                {"titanium", new Color(0.710f,0.800f,0.870f)},
                {"dark", new Color(0.039f,0.054f,0.078f)},
                {"graphite", new Color(0.085f,0.120f,0.170f)},
                {"copper", new Color(0.910f,0.460f,0.160f)},
                {"gold", new Color(0.960f,0.630f,0.180f)},
                {"ceramic", new Color(0.900f,0.940f,0.950f)},
                {"glass", new Color(0.170f,0.660f,0.840f)},
                {"panel", new Color(0.014f,0.095f,0.160f)},
                {"pcb", new Color(0.025f,0.260f,0.190f)},
                {"cyan", new Color(0.060f,0.840f,0.990f)},
                {"blue", new Color(0.200f,0.430f,0.960f)},
                {"green", new Color(0.120f,0.920f,0.510f)},
                {"amber", new Color(1.000f,0.640f,0.120f)},
                {"violet", new Color(0.730f,0.420f,0.990f)},
                {"red", new Color(0.990f,0.130f,0.120f)},
                {"orange", new Color(1.000f,0.350f,0.060f)},
                {"white", new Color(0.960f,0.960f,0.910f)},
                {"water", new Color(0.100f,0.560f,0.920f)},
                {"shield", new Color(0.690f,0.750f,0.820f)},
                {"insulation", new Color(0.190f,0.240f,0.290f)},
                {"floor", new Color(0.240f,0.290f,0.360f)},
                {"stars", new Color(0.810f,0.910f,1.000f)},
                {"storm", new Color(1.000f,0.320f,0.048f)},
                {"solar", new Color(1.000f,0.730f,0.200f)},
                {"black", new Color(0.012f,0.019f,0.027f)},
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
                if (entry.Key is "cyan" or "blue" or "green" or "amber" or "red" or "storm" or "solar")
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", entry.Value * (entry.Key == "storm" ? 1.4f : .55f));
                }
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
                ren.sharedMaterials=current;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Level89Materials] Replaced "+applied+" material slots with HDRP/URP/Standard preview palette. Procedural texture PNGs require authored UVs, repacking and shader setup; not auto-baked.");
        }
    }
}
#endif

