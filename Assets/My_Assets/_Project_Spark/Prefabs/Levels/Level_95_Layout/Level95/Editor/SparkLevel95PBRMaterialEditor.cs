#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace ProjectSpark.Level95.EditorTools
{
    /// <summary>Converts the original Level95 PBR maps to editable native Unity materials.
    /// No gameplay state is read or changed. URP/HDRP/Built-In Lit shader chosen at import time.</summary>
    public static class SparkLevel95PBRMaterialEditor
    {
        public const string OutputPath="Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_95_Layout/Level95/GeneratedMaterials";
        private static readonly Dictionary<string,string> Sets = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase) {
            {"PS_aluminum","TitaniumBench"},
            {"PS_amber","StatusAmberLED"},
            {"PS_black","ElectronicBlackMold"},
            {"PS_blue","StatusBlueLED"},
            {"PS_capacitor","MultiLayerCeramicCapacitor"},
            {"PS_ceramic","TechnicalWhiteCeramic"},
            {"PS_copper","RolledCopper"},
            {"PS_cyan","InspectionCyanLED"},
            {"PS_fr4","FiberglassFR4"},
            {"PS_frame","TitaniumBench"},
            {"PS_glass","PrecisionCoatedGlass"},
            {"PS_gold","ImmersionGoldENIG"},
            {"PS_graphite","AnodizedGraphite"},
            {"PS_green","StatusGreenLED"},
            {"PS_holo","InspectionCyanLED"},
            {"PS_ic","ElectronicBlackMold"},
            {"PS_mask","DeepNavyPCBMask"},
            {"PS_maskdark","DeepNavyPCBMask"},
            {"PS_panel","BlueBlackScreenGlass"},
            {"PS_red","StatusRedLED"},
            {"PS_resistor","CarbonFilmResistor"},
            {"PS_rubber","ESDSafeRubber"},
            {"PS_solder","LeadFreeSolder"},
            {"PS_steel","TitaniumBench"},
            {"PS_violet","CalibrationVioletLED"},
            {"PS_whiteink","LaserSilkscreenWhite"}
        };
        private static readonly Dictionary<string,Material> Cache = new Dictionary<string,Material>(StringComparer.OrdinalIgnoreCase);
        public static void EnsureFolder(string folder)
        {
            if (folder=="Assets" || AssetDatabase.IsValidFolder(folder)) return;
            int sep=folder.LastIndexOf('/');
            if (sep<1) throw new Exception("Expected Assets/ path");
            string parent=folder.Substring(0,sep);EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent,folder.Substring(sep+1));
        }
        public static string FindPathExact(string filename)
        {
            string stem=Path.GetFileNameWithoutExtension(filename);
            foreach(string guid in AssetDatabase.FindAssets(stem))
            {
                string p=AssetDatabase.GUIDToAssetPath(guid);
                if(Path.GetFileName(p).Equals(filename,StringComparison.OrdinalIgnoreCase)
                    && p.IndexOf("Level95",StringComparison.OrdinalIgnoreCase)>=0) return p;
            }
            return null;
        }
        public static Texture2D LoadMap(string name,bool isNormal=false)
        {
            string p=FindPathExact(name+".png"); if(String.IsNullOrEmpty(p))return null;
            var importer=AssetImporter.GetAtPath(p) as TextureImporter;
            if(isNormal && importer!=null && importer.textureType!=TextureImporterType.NormalMap)
            {
                importer.textureType=TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
        }
        static string Pipeline()
        {
            var a=GraphicsSettings.currentRenderPipeline;
            if(a==null)return "BuiltIn";
            string n=a.GetType().Name.ToLowerInvariant();
            return n.Contains("hd") ? "HDRP" : "URP";
        }
        static void SetTex(Material m, string key, Texture2D tex)
        {
            if(tex!=null && m.HasProperty(key))m.SetTexture(key,tex);
        }
        public static Dictionary<string,Material> EnsureMaterials()
        {
            EnsureFolder(OutputPath);
            Cache.Clear();
            string pipe=Pipeline();
            string shaderName=pipe=="URP"?"Universal Render Pipeline/Lit":pipe=="HDRP"?"HDRP/Lit":"Standard";
            var lit=Shader.Find(shaderName);
            if(lit==null) { Debug.LogError("[Level95] No native Lit shader: "+shaderName);return Cache; }
            foreach(var kv in Sets)
            {
                string source=kv.Key, set=kv.Value;
                string target=OutputPath+"/"+source+".mat";
                var m=AssetDatabase.LoadAssetAtPath<Material>(target);
                if(m==null) { m=new Material(lit){name=source};AssetDatabase.CreateAsset(m,target); }
                var hue=source.ToLowerInvariant();
                bool glass=(hue=="ps_glass");
                bool emissive=hue=="ps_cyan"||hue=="ps_holo"||hue=="ps_green"||hue=="ps_amber"||hue=="ps_orange"||hue=="ps_red"||hue=="ps_blue";
                Shader desired=lit;
                if(hue=="ps_holo")
                {
                    string custom=pipe=="URP"?"ProjectSpark/Level95/PulseHologramURP":pipe=="BuiltIn"?"ProjectSpark/Level95/PulseHologramBuiltIn":"HDRP/Unlit";
                    var sh=Shader.Find(custom); if(sh!=null)desired=sh;
                }
                m.shader=desired;
                Texture2D albedo=LoadMap(set+"_BaseColor");
                Texture2D normal=LoadMap(set+"_Normal",true);
                Texture2D mask=LoadMap(set+"_UnityMaskMap");
                Texture2D ao=LoadMap(set+"_Occlusion");
                Texture2D em=LoadMap(set+"_Emission");
                SetTex(m,"_MainTex",albedo); // Supported by custom hologram and Built-in Standard.
                if(pipe=="URP")
                {
                    SetTex(m,"_BaseMap",albedo);SetTex(m,"_BumpMap",normal);
                    SetTex(m,"_MetallicGlossMap",mask);SetTex(m,"_OcclusionMap",ao);SetTex(m,"_EmissionMap",em);
                    if(m.HasProperty("_Surface"))m.SetFloat("_Surface",glass?1:0);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",1);
                }
                else if(pipe=="HDRP")
                {
                    SetTex(m,"_BaseColorMap",albedo);SetTex(m,"_NormalMap",normal);
                    SetTex(m,"_MaskMap",mask);SetTex(m,"_EmissiveColorMap",em);
                    if(m.HasProperty("_SurfaceType"))m.SetFloat("_SurfaceType",glass?1:0);
                }
                else
                {
                    SetTex(m,"_MainTex",albedo);SetTex(m,"_BumpMap",normal);
                    SetTex(m,"_MetallicGlossMap",mask);SetTex(m,"_OcclusionMap",ao);SetTex(m,"_EmissionMap",em);
                }
                if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",Color.white);
                if(m.HasProperty("_Color"))m.SetColor("_Color",Color.white);
                if(m.HasProperty("_Tint"))m.SetColor("_Tint",new Color(.12f,.88f,1f,.55f));
                if(normal!=null)m.EnableKeyword("_NORMALMAP");
                if(mask!=null)m.EnableKeyword("_METALLICGLOSSMAP");
                if(emissive) {
                    if(m.HasProperty("_EmissionColor"))m.SetColor("_EmissionColor",Color.white*2.0f);
                    if(m.HasProperty("_EmissiveColor"))m.SetColor("_EmissiveColor",Color.white*2.0f);
                    m.EnableKeyword("_EMISSION");
                }
                if(glass) { m.renderQueue=(int)RenderQueue.Transparent; m.SetOverrideTag("RenderType","Transparent"); }
                EditorUtility.SetDirty(m);Cache[source]=m;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Level95] Created/updated "+Cache.Count+" native PBR materials for "+pipe+". Actual render outcome must be verified in Unity.");
            return Cache;
        }
        [MenuItem("Tools/Project Spark/Levels/95/1. Create Native PBR Materials")]
        public static void CreateFromMenu(){EnsureMaterials();}
        public static void ApplyMaterials(GameObject target)
        {
            if(target==null)return;
            var map=EnsureMaterials();
            int applied=0,missing=0;
            foreach(var render in target.GetComponentsInChildren<Renderer>(true))
            {
                var arr=render.sharedMaterials;bool changed=false;
                for(int i=0;i<arr.Length;i++)
                {
                    if(arr[i]==null)continue;
                    string key=arr[i].name.Replace(" (Instance)","").Trim();
                    if(map.TryGetValue(key,out var m)) {arr[i]=m;applied++;changed=true;} else missing++;
                }
                if(changed)render.sharedMaterials=arr;
            }
            Debug.Log("[Level95] Bound "+applied+" PBR material slots; unmatched "+missing+" (check FBX material names).");
        }
        [MenuItem("Tools/Project Spark/Levels/95/3. Apply PBR To Selected Art")]
        public static void ApplySelected(){if(Selection.activeGameObject!=null)ApplyMaterials(Selection.activeGameObject);else Debug.LogError("[Level95] Select Level95 art first.");}
    }
}
#endif

