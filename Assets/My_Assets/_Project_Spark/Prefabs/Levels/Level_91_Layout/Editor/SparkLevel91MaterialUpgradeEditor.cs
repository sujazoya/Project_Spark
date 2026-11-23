#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Assigns source-palette PBR-like materials, not baked production UV textures.</summary>
    public static class SparkLevel91MaterialUpgradeEditor
    {
        public static void Apply(GameObject target, string parent)
        {
            string folder = parent + "/Materials";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(parent, "Materials");
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null)shader=Shader.Find("HDRP/Lit");
            if(shader==null)shader=Shader.Find("Standard");
            if(shader==null){Debug.LogError("[Level91Materials] No compatible Lit shader found");return;}
            var palette=new Dictionary<string,Color>(){
                {"structural",new Color(0.3400f,0.4300f,0.5400f)},
                {"titanium",new Color(0.7500f,0.8100f,0.8600f)},
                {"silver",new Color(0.6300f,0.7100f,0.8000f)},
                {"dark",new Color(0.0320f,0.0470f,0.0740f)},
                {"graphite",new Color(0.0750f,0.1080f,0.1700f)},
                {"copper",new Color(0.8400f,0.4500f,0.2000f)},
                {"gold",new Color(0.9700f,0.7200f,0.3100f)},
                {"ceramic",new Color(0.9000f,0.9400f,0.9600f)},
                {"glass",new Color(0.1500f,0.5600f,0.8400f)},
                {"panel",new Color(0.0170f,0.0730f,0.1500f)},
                {"pcb",new Color(0.0450f,0.2000f,0.1700f)},
                {"cyan",new Color(0.0800f,0.8700f,1.0000f)},
                {"blue",new Color(0.1200f,0.4300f,1.0000f)},
                {"green",new Color(0.1300f,0.8900f,0.5500f)},
                {"amber",new Color(1.0000f,0.5800f,0.1100f)},
                {"violet",new Color(0.6200f,0.3400f,0.9500f)},
                {"red",new Color(1.0000f,0.1800f,0.1700f)},
                {"white",new Color(0.9600f,0.9700f,1.0000f)},
                {"holo",new Color(0.2200f,0.6400f,1.0000f)},
                {"star",new Color(0.7700f,0.8200f,1.0000f)},
                {"floor",new Color(0.2200f,0.2900f,0.4000f)},
                {"insulation",new Color(0.1800f,0.2400f,0.2800f)},
                {"purple",new Color(0.3600f,0.2000f,0.7200f)},
                {"black",new Color(0.0140f,0.0170f,0.0280f)},
                {"photon",new Color(0.9000f,0.4000f,1.0000f)},
                {"goldglass",new Color(0.9500f,0.7100f,0.2800f)},
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
            Debug.Log("[Level91Materials] Replaced "+applied+" material slots with HDRP/URP/Standard preview palette. Procedural texture PNGs require authored UVs, repacking and shader setup; not auto-baked.");
        }
    }
}
#endif

