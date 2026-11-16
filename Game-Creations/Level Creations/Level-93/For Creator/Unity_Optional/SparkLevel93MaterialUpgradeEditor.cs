#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace ProjectSpark.Level93.EditorTools
{
    /// <summary>Create pipeline-appropriate native .mat assets from source texture maps.
    /// Material names match the actual PS_* materials in the exported FBX geometry.</summary>
    public static class SparkLevel93MaterialUpgradeEditor
    {
        private const string OutputFolder = "Assets/ProjectSpark/Level93/GeneratedMaterials";
        private static readonly Dictionary<string,string> TextureSets=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase) {
            {"white","WhiteCeramicLab"},
            {"ceramic","WhiteCeramicLab"},
            {"titanium","BrushedTitanium"},
            {"silver","HighVacuumStainlessSteel"},
            {"structural","BrushedTitanium"},
            {"graphite","AnalyticalGraphite"},
            {"dark","AnalyticalGraphite"},
            {"black","AnalyticalGraphite"},
            {"panel","SCADADeepBlue"},
            {"floor","IndustrialLabFloor"},
            {"gold","OpticalGoldTerminal"},
            {"copper","CopperVacuumTube"},
            {"glass","SampleVaultGlass"},
            {"glassgold","SampleVaultGlass"},
            {"inert","SampleVaultGlass"},
            {"cyan","CyanScientificHologram"},
            {"holo","CyanScientificHologram"},
            {"blue","CyanScientificHologram"},
            {"purple","PurpleSpectralHologram"},
            {"laser","PurpleSpectralHologram"},
            {"spectrum","PurpleSpectralHologram"},
            {"amber","ThermalWarningAmber"},
            {"green","StatusGreen"},
            {"red","StatusRed"},
            {"core","CarbonaceousCoreRock"},
            {"regolith","CarbonaceousCoreRock"},
            {"mineral","AsteroidMineralInclusions"},
            {"ore","HighReflectiveOre"},
            {"vacuum","QuartzSpectralOptics"}
        };
        private static readonly Dictionary<string,Material> Generated=new Dictionary<string,Material>(StringComparer.OrdinalIgnoreCase);
        [MenuItem("Tools/Project Spark/Levels/93/1. Generate HD PBR Materials And Shaders")]
        public static void CreateAllMenu() { CreateAll(); }
        private static bool MakeFolder(string path)
        {
            if(AssetDatabase.IsValidFolder(path))return true;
            int slash=path.LastIndexOf('/');if(slash<0)return false;
            var parent=path.Substring(0,slash);if (!MakeFolder(parent)) return false;
            AssetDatabase.CreateFolder(parent,path.Substring(slash+1));return true;
        }
        private static string FindTexturePath(string name)
        {
            foreach(string guid in AssetDatabase.FindAssets(name+" t:Texture2D"))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                if(Path.GetFileNameWithoutExtension(path).Equals(name,StringComparison.OrdinalIgnoreCase))return path;
            }
            return null;
        }
        private static Texture2D GetTex(string textureName,bool normal)
        {
            string path=FindTexturePath(textureName);
            if(string.IsNullOrEmpty(path))return null;
            if(normal)
            {
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer!=null && importer.textureType!=TextureImporterType.NormalMap)
                { importer.textureType=TextureImporterType.NormalMap;importer.SaveAndReimport(); }
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        private static string Pipeline()
        {
            var current=GraphicsSettings.currentRenderPipeline;
            if(current==null)return "BuiltIn";
            return current.GetType().Name.ToLowerInvariant().Contains("hd")?"HDRP":"URP";
        }
        private static Shader FindShader(string pipeline)
        {
            string name=pipeline=="HDRP"?"HDRP/Lit":pipeline=="URP"?"Universal Render Pipeline/Lit":"Standard";
            return Shader.Find(name);
        }
        private static void SetTexture(Material m,string prop,Texture tex)
        { if(m.HasProperty(prop) && tex!=null) m.SetTexture(prop,tex); }
        private static bool IsGlow(string material)
        { return material=="cyan"||material=="blue"||material=="purple"||material=="holo"||material=="laser"||material=="spectrum"||material=="amber"||material=="green"||material=="red"; }
        public static void CreateAll()
        {
            MakeFolder(OutputFolder);
            var pipe=Pipeline();var lit=FindShader(pipe);
            if(lit==null) { Debug.LogError("[L93] Compatible Lit shader unavailable. Install the chosen pipeline or switch to Built-in.");return; }
            Generated.Clear();int created=0;
            foreach(var item in TextureSets)
            {
                string key=item.Key;string set=item.Value;string matName="PS_"+key;
                Shader shader=lit;
                if(key=="holo"||key=="spectrum")
                {
                    var holo=Shader.Find(pipe=="URP"?"ProjectSpark/Level93/HologramURP":"ProjectSpark/Level93/HologramBuiltin");
                    if(holo!=null && pipe!="HDRP")shader=holo;
                }
                string path=OutputFolder+"/"+matName+".mat";
                Material mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(mat==null) {mat=new Material(shader);mat.name=matName;AssetDatabase.CreateAsset(mat,path);} else mat.shader=shader;
                var albedo=GetTex(set+"_Albedo",false);
                var normal=GetTex(set+"_Normal",true);
                var mask=GetTex(set+"_UnityMetallicSmoothness",false);
                var occlusion=GetTex(set+"_Occlusion",false);
                var emission=GetTex(set+"_Emission",false);
                SetTexture(mat,"_BaseMap",albedo);SetTexture(mat,"_BaseColorMap",albedo);SetTexture(mat,"_MainTex",albedo);
                SetTexture(mat,"_MetallicGlossMap",mask);SetTexture(mat,"_MaskMap",mask);
                SetTexture(mat,"_BumpMap",normal);SetTexture(mat,"_NormalMap",normal);
                SetTexture(mat,"_OcclusionMap",occlusion);
                SetTexture(mat,"_EmissionMap",emission);SetTexture(mat,"_EmissiveColorMap",emission);
                if(mat.HasProperty("_BumpScale"))mat.SetFloat("_BumpScale",0.9f);
                if(mat.HasProperty("_Metallic"))mat.SetFloat("_Metallic",1f);
                if(mat.HasProperty("_Smoothness"))mat.SetFloat("_Smoothness",0.65f);
                if(mat.HasProperty("_Glossiness"))mat.SetFloat("_Glossiness",0.65f);
                if(IsGlow(key))
                {
                    mat.EnableKeyword("_EMISSION");
                    if(mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor",new Color(.35f,.8f,1.0f));
                }
                else mat.DisableKeyword("_EMISSION");
                bool transparent = key=="glass" || key=="inert" || key=="glassgold";
                if(transparent)
                {
                    if(pipe=="URP")
                    {
                        if(mat.HasProperty("_Surface"))mat.SetFloat("_Surface",1f);
                        if(mat.HasProperty("_Blend"))mat.SetFloat("_Blend",0f);
                    }
                    if(pipe=="BuiltIn")
                    {
                        if(mat.HasProperty("_Mode"))mat.SetFloat("_Mode",3f);
                        mat.EnableKeyword("_ALPHABLEND_ON");
                    }
                    if(pipe=="HDRP" && mat.HasProperty("_SurfaceType"))mat.SetFloat("_SurfaceType",1f);
                    mat.SetOverrideTag("RenderType","Transparent");
                    mat.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite",0);
                    mat.renderQueue=(int)RenderQueue.Transparent;
                    if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",new Color(1f,1f,1f,.37f));
                    if(mat.HasProperty("_Color"))mat.SetColor("_Color",new Color(1f,1f,1f,.37f));
                }
                if(mask!=null)mat.EnableKeyword("_METALLICGLOSSMAP");
                if(normal!=null)mat.EnableKeyword("_NORMALMAP");
                EditorUtility.SetDirty(mat);Generated[matName]=mat;created++;
            }
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            if(Selection.activeGameObject!=null)ApplyGeneratedMaterials(Selection.activeGameObject);
            Debug.Log("[L93] Generated/updated "+created+" pipeline-compatible PBR materials using "+pipe+" shader. Texture maps are procedural reference assets.");
        }
        public static void ApplyGeneratedMaterials(GameObject root)
        {
            if(root==null)return;
            if(Generated.Count==0)
                foreach(var kv in TextureSets)
                {
                    var mat=AssetDatabase.LoadAssetAtPath<Material>(OutputFolder+"/PS_"+kv.Key+".mat");
                    if(mat!=null)Generated["PS_"+kv.Key]=mat;
                }
            int changed=0;
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var src=renderer.sharedMaterials;bool dirty=false;
                for(int j=0;j<src.Length;j++)
                {
                    string n=src[j]!=null?src[j].name:"";
                    int instance=n.IndexOf(" (Instance)",StringComparison.OrdinalIgnoreCase);
                    if(instance>=0)n=n.Substring(0,instance);
                    // FBX import may append another suffix to internal material assets.
                    if(n.StartsWith("PS_",StringComparison.OrdinalIgnoreCase))
                    {
                        int last=n.IndexOf(" (Material)",StringComparison.OrdinalIgnoreCase);
                        if(last>=0)n=n.Substring(0,last);
                    }
                    Material target;
                    if(Generated.TryGetValue(n,out target) && src[j]!=target){src[j]=target;dirty=true;changed++;}
                }
                if(dirty) renderer.sharedMaterials=src;
            }
            Debug.Log("[L93] Assigned "+changed+" generated material slots to the selected visual root.");
        }
        [MenuItem("Tools/Project Spark/Levels/93/3. Apply Generated PBR Materials To Selected Art")]
        public static void ApplySelection() { ApplyGeneratedMaterials(Selection.activeGameObject); }
    }
}
#endif
