#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    internal static class SparkLevel104Materials
    {
        const string Folder="Assets/ProjectSpark/Level104";
        static readonly Dictionary<string,Material> Materials=new(StringComparer.OrdinalIgnoreCase);
        internal static void Build()
        {
            Materials.Clear();
            foreach(string family in Directory.GetFiles(Folder+"/Textures","*_BaseColor.png").Select(path=>Path.GetFileNameWithoutExtension(path).Replace("_BaseColor","")))
            {
                var ready=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/"+family+".mat");
                if(ready && ready.GetTexture("_BaseMap") && ready.GetTexture("_BumpMap") && ready.GetTexture("_MetallicGlossMap"))
                {
                    ready.SetTexture("_OcclusionMap",Map("AO",false,true)); if(family.StartsWith("LED_")){ready.SetTexture("_EmissionMap",Map("Emission"));ready.SetColor("_EmissionColor",Color.white*.3f);ready.EnableKeyword("_EMISSION");} EditorUtility.SetDirty(ready); Materials.Add(family,ready);
                    continue;
                }
                Texture2D Map(string suffix,bool normal=false,bool linear=false)
                {
                    string path=Folder+"/Textures/"+family+"_"+suffix+".png";
                    var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                    if(importer)
                    {
                        bool changed=importer.sRGBTexture==linear || normal && importer.textureType!=TextureImporterType.NormalMap ||
                            suffix=="MetallicRoughness" && !importer.isReadable;
                        importer.sRGBTexture=!linear;
                        if(normal)importer.textureType=TextureImporterType.NormalMap;
                        if(suffix=="MetallicRoughness")importer.isReadable=true;
                        if(changed)importer.SaveAndReimport();
                    }
                    return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                var albedo=Map("BaseColor");
                var normalMap=Map("Normal",true,true);
                var orm=Map("MetallicRoughness",false,true);
                Texture2D packed=null;
                if(orm)
                {
                    var pixels=orm.GetPixels32();
                    for(int i=0;i<pixels.Length;i++)pixels[i]=new Color32(pixels[i].b,0,0,(byte)(255-pixels[i].g));
                    var texture=new Texture2D(orm.width,orm.height,TextureFormat.RGBA32,false,true);
                    texture.SetPixels32(pixels);texture.Apply();
                    string mask=Folder+"/Materials/"+family+"_MetallicSmoothness.png";
                    File.WriteAllBytes(mask,texture.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(texture);
                    AssetDatabase.ImportAsset(mask);
                    var maskImporter=(TextureImporter)AssetImporter.GetAtPath(mask);
                    maskImporter.sRGBTexture=false;maskImporter.SaveAndReimport();
                    packed=AssetDatabase.LoadAssetAtPath<Texture2D>(mask);
                }
                string materialPath=Folder+"/Materials/"+family+".mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if(!material)
                {
                    material=new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                    AssetDatabase.CreateAsset(material,materialPath);
                }
                material.SetTexture("_BaseMap",albedo);
                material.SetTexture("_BumpMap",normalMap);
                material.SetTexture("_MetallicGlossMap",packed);
                material.SetFloat("_Smoothness",.72f); var ao=Map("AO",false,true); material.SetTexture("_OcclusionMap",ao); if(family.StartsWith("LED_")){material.SetTexture("_EmissionMap",Map("Emission"));material.SetColor("_EmissionColor",Color.white*.3f);material.EnableKeyword("_EMISSION");}
                material.EnableKeyword("_NORMALMAP");material.EnableKeyword("_METALLICSPECGLOSSMAP");
                EditorUtility.SetDirty(material);
                Materials.Add(family,material);
            }
            var silk=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/BoardArtwork.mat");
            if(!silk){silk=new Material(Materials["Soldermask_Green"]);AssetDatabase.CreateAsset(silk,Folder+"/Materials/BoardArtwork.mat");}
            silk.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/Textures/VGR5_Top_Silkscreen_FactoryOnly.png"));
            Materials["BoardArtwork"]=silk;
            AssetDatabase.SaveAssets();
        }
        internal static void Apply(GameObject root)
        {
            var recovery=File.ReadAllLines(Folder+"/Data/Recovered_Material_Map.csv").Skip(1).Select(row=>row.Split(',').Select(cell=>cell.Trim('"')).ToArray()).Where(row=>row[0]==root.name).ToDictionary(row=>row[1],row=>row[2]); foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var mapped=renderer.sharedMaterials;
                for(int i=0;i<mapped.Length;i++)
                {
                    string name=mapped[i]?mapped[i].name.Replace(" (Instance)",""):"Mask"; if(recovery.TryGetValue(renderer.name,out var family))name=family;
                    if(Materials.TryGetValue(name,out var material))mapped[i]=material;
                }
                renderer.sharedMaterials=mapped;
            }
        }
        internal static Material Surface(string name,Color color)
        {
            string path=Folder+"/Materials/"+name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
            material.color=color;
            material.SetFloat("_Metallic",.65f);material.SetFloat("_Smoothness",.68f);
            return material;
        }
    }
}
#endif




