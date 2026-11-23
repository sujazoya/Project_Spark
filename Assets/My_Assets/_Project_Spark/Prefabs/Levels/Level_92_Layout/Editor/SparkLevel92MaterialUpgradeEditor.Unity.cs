#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Assigns source-palette PBR-like materials, not baked production UV textures.</summary>
    public static class SparkLevel92MaterialUpgradeEditor
    {
        public static void Apply(GameObject target, string parent)
        {
            string folder = parent + "/Materials";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(parent, "Materials");
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null)shader=Shader.Find("HDRP/Lit");
            if(shader==null)shader=Shader.Find("Standard");
            if(shader==null){Debug.LogError("[Level92Materials] No compatible Lit shader found");return;}
            var palette=new Dictionary<string,Color>(){
                {"rock",new Color(0.1200f,0.1350f,0.1650f)},
                {"regolith",new Color(0.2100f,0.2300f,0.2500f)},
                {"fractured",new Color(0.3200f,0.2900f,0.2600f)},
                {"carbon",new Color(0.0600f,0.0800f,0.1000f)},
                {"mineral",new Color(0.5300f,0.3500f,0.2100f)},
                {"amberrock",new Color(0.4700f,0.2700f,0.1600f)},
                {"titanium",new Color(0.7300f,0.8000f,0.8600f)},
                {"white",new Color(0.9200f,0.9400f,0.9600f)},
                {"silver",new Color(0.5400f,0.6300f,0.7100f)},
                {"structural",new Color(0.3000f,0.4000f,0.4800f)},
                {"graphite",new Color(0.0650f,0.0900f,0.1300f)},
                {"dark",new Color(0.0400f,0.0600f,0.0800f)},
                {"black",new Color(0.0160f,0.0250f,0.0350f)},
                {"ceramic",new Color(0.8700f,0.8900f,0.8300f)},
                {"gold",new Color(0.9300f,0.7000f,0.3000f)},
                {"copper",new Color(0.8400f,0.4800f,0.2200f)},
                {"blue",new Color(0.1400f,0.5100f,0.9600f)},
                {"cyan",new Color(0.0800f,0.8800f,1.0000f)},
                {"amber",new Color(1.0000f,0.5500f,0.1000f)},
                {"red",new Color(0.9400f,0.1700f,0.1200f)},
                {"green",new Color(0.1900f,0.8400f,0.4700f)},
                {"glass",new Color(0.1700f,0.5100f,0.6700f)},
                {"holo",new Color(0.1100f,0.7400f,1.0000f)},
                {"yellow",new Color(0.9500f,0.7400f,0.2000f)},
                {"floor",new Color(0.1600f,0.2100f,0.2700f)},
                {"panel",new Color(0.0180f,0.0550f,0.0950f)},
                {"pcb",new Color(0.0900f,0.2500f,0.1900f)},
                {"purple",new Color(0.4500f,0.2800f,0.7500f)},
                {"soilblue",new Color(0.1000f,0.5800f,0.8600f)},
                {"soilgreen",new Color(0.2300f,0.8800f,0.5600f)},
                {"soilgold",new Color(0.9600f,0.6500f,0.2000f)},
                {"soilred",new Color(0.9600f,0.2500f,0.2800f)},
                {"insulation",new Color(0.1400f,0.2100f,0.2500f)},
            };
            var materials=new Dictionary<string,Material>();
            AssetDatabase.StartAssetEditing();
            try
            {
            foreach(var entry in palette)
            {
                string path=folder+"/PS_"+entry.Key+".mat";
                Material material=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}
                if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",entry.Value);
                if(material.HasProperty("_Color"))material.SetColor("_Color",entry.Value);
                if(material.HasProperty("_Metallic"))material.SetFloat("_Metallic",(entry.Key=="copper"||entry.Key=="titanium"||entry.Key=="gold"||entry.Key=="structural")?.85f:.12f);
                if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",(entry.Key=="glass"||entry.Key=="gold")?.76f:.46f);
                if (entry.Key is "cyan" or "blue" or "green" or "amber" or "red" or "violet")
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", entry.Value * .65f);
                }
                EditorUtility.SetDirty(material);
                materials[entry.Key]=material;
            }
            }
            finally { AssetDatabase.StopAssetEditing(); }
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
                ren.sharedMaterials=current;EditorUtility.SetDirty(ren);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Level92Materials] Replaced "+applied+" material slots with HDRP/URP/Standard preview palette. Procedural texture PNGs require authored UVs, repacking and shader setup; not auto-baked.");
        }
    }
}
#endif

