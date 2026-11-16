#!/usr/bin/env python3
from pathlib import Path
import json,zipfile
import build_level93 as b
R=Path(__file__).resolve().parents[1];T=R/'Unity_Optional';T.mkdir(exist_ok=True)
shaderzip=Path('/mnt/data/ProjectSpark_ProGameRoom_AAA_Complete.zip')
with zipfile.ZipFile(shaderzip) as z:
 for pipeline in ('URP','Builtin'):
  src=f'ProjectSpark_ProGameRoom_AAA/Shaders/PS_RoomHologram{pipeline}.shader'
  data=z.read(src).decode().replace('ProjectSpark/RoomStudio/Hologram','ProjectSpark/Level93/Hologram')
  (R/'Shaders'/f'PS_Level93Hologram{pipeline}.shader').write_text(data)
model_names=',\n'.join('            "'+n+'"' for n in b.NAMES)
pivot_names=',\n'.join('            "'+p['Pivot']+'"' for p in b.PIVOTS)

placement=r'''#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
namespace ProjectSpark.Level93.EditorTools
{
    /// <summary>Art only. Keeps visual FBX instances isolated; never changes gameplay/electrical objects.</summary>
    public static class SparkLevel93ArtPlacementEditor
    {
        public const string ArtRootName = "L93_HD_VISUAL_ART";
        public static readonly string[] ModelNames = new string[]
        {
__MODEL_NAMES__
        };
        [MenuItem("Tools/Project Spark/Levels/93/2. Place 100 Modular FBX Models Under Selected Layout")]
        public static void Build()
        {
            var selected = Selection.activeGameObject;
            if (selected == null) { Debug.LogError("[L93] Select the Level 93 layout GameObject first."); return; }
            var matching = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            foreach (string guid in AssetDatabase.FindAssets("t:Model"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase)) continue;
                string filename = Path.GetFileNameWithoutExtension(path);
                int underscore = filename.IndexOf("_L93_",StringComparison.OrdinalIgnoreCase);
                if (underscore < 0) continue;
                string cleanName = filename.Substring(underscore + 1);
                if (!matching.ContainsKey(cleanName)) matching[cleanName]=path;
            }
            int missing=0; var fail = new List<string>();
            foreach (string model in ModelNames) if (!matching.ContainsKey(model)) { missing++; fail.Add(model); }
            if (missing>0)
            {
                Debug.LogError("[L93] Missing "+missing+" FBX models. Import Models/ before placement. Example: "+String.Join(", ",fail.Take(6).ToArray()));
                return;
            }
            Undo.RegisterFullObjectHierarchyUndo(selected,"Build Level 93 visual art");
            Transform old = selected.transform.Find(ArtRootName);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject artRoot=new GameObject(ArtRootName);
            Undo.RegisterCreatedObjectUndo(artRoot,"Create Level 93 art root");
            artRoot.transform.SetParent(selected.transform,false);
            artRoot.transform.localPosition=Vector3.zero;
            artRoot.transform.localRotation=Quaternion.identity;
            artRoot.transform.localScale=Vector3.one;
            int count=0;
            foreach (string name in ModelNames)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(matching[name]);
                if (prefab==null) { Debug.LogError("[L93] Failed loading "+matching[name]); continue; }
                GameObject go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if (go==null) { Debug.LogError("[L93] Could not instantiate "+name); continue; }
                go.transform.SetParent(artRoot.transform,false);
                go.transform.localPosition=Vector3.zero;
                go.transform.localRotation=Quaternion.identity;
                go.transform.localScale=Vector3.one;
                go.name=name;
                count++;
            }
            SparkLevel93MaterialUpgradeEditor.ApplyGeneratedMaterials(artRoot);
            Selection.activeGameObject=artRoot;
            Debug.Log("[L93] Placed "+count+"/100 real FBX models (visual-only, no gameplay simulation). All artwork uses common scene origin.");
        }
    }
}
#endif
'''.replace('__MODEL_NAMES__',model_names)
(T/'SparkLevel93ArtPlacementEditor.cs').write_text(placement)

materials=r'''#if UNITY_EDITOR
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
__MATERIAL_MAP__
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
'''
mp=json.loads((R/'Texture_Manifest.json').read_text())['material_to_texture_set']
materials=materials.replace('__MATERIAL_MAP__',',\n'.join('            {"'+k+'","'+v+'"}' for k,v in mp.items()))
(T/'SparkLevel93MaterialUpgradeEditor.cs').write_text(materials)

validation=r'''#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level93.EditorTools
{
    public static class SparkLevel93ArtValidatorEditor
    {
        public static readonly string[] RequiredPivots = new string[]
        {
__PIVOT_NAMES__
        };
        [MenuItem("Tools/Project Spark/Levels/93/4. Validate Imported HD Art")]
        public static void Validate()
        {
            var obj=Selection.activeGameObject;
            if(obj==null){Debug.LogError("[L93] Select L93_HD_VISUAL_ART to validate.");return;}
            var found=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int meshcount=0,verts=0,sockets=0;
            foreach(var t in obj.GetComponentsInChildren<Transform>(true))
            {
                found.Add(t.name);
                if(t.name.StartsWith("SOCKET_P")) sockets++;
                var f=t.GetComponent<MeshFilter>();if(f!=null&&f.sharedMesh!=null){meshcount++;verts+=f.sharedMesh.vertexCount;}
            }
            int missing=0;
            foreach(var p in RequiredPivots) if(!found.Contains(p)){missing++;Debug.LogWarning("[L93] Missing pivot "+p);}
            int models=0;
            foreach(var t in obj.transform)if(t.name.StartsWith("L93_"))models++;
            Debug.Log("[L93] Art validation: "+models+"/100 model roots, "+(RequiredPivots.Length-missing)+"/92 pivots, "+sockets+"/272 sockets, "+meshcount+" mesh chunks, "+verts+" vertices. Unity Play Mode/electrical safety must be checked separately.");
        }
    }
}
#endif
'''.replace('__PIVOT_NAMES__',pivot_names)
(T/'SparkLevel93ArtValidatorEditor.cs').write_text(validation)

rig=r'''using System;
using System.Collections.Generic;
using UnityEngine;
namespace ProjectSpark.Level93.Visuals
{
    /// <summary>Visual art facade ONLY. Must receive measured/authoritative values from a
    /// separately implemented level simulator. Does not decide objectives or safety.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel93VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform artRoot;
        private readonly Dictionary<string,Transform> pivots=new Dictionary<string,Transform>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string,Quaternion> baseRotation=new Dictionary<string,Quaternion>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string,Vector3> basePosition=new Dictionary<string,Vector3>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string,float> measuredAngle=new Dictionary<string,float>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string,Vector3> measuredDisplacement=new Dictionary<string,Vector3>(StringComparer.OrdinalIgnoreCase);
        private void Awake(){ if(artRoot==null)artRoot=transform.Find("L93_HD_VISUAL_ART");if(artRoot==null)artRoot=transform;Rebind(); }
        public void Rebind()
        {
            pivots.Clear();baseRotation.Clear();basePosition.Clear();
            foreach(var t in artRoot.GetComponentsInChildren<Transform>(true))
            {
                if(!t.name.StartsWith("ANIM_",StringComparison.OrdinalIgnoreCase)||pivots.ContainsKey(t.name))continue;
                pivots.Add(t.name,t);baseRotation.Add(t.name,t.localRotation);basePosition.Add(t.name,t.localPosition);
            }
        }
        public void SetActualAngleDegrees(string pivotName,float measuredDegrees){measuredAngle[pivotName]=measuredDegrees;}
        public void SetActualDisplacement(string pivotName,Vector3 measuredMeters){measuredDisplacement[pivotName]=measuredMeters;}
        private void LateUpdate()
        {
            foreach(var p in measuredAngle)
            {
                Transform t;Quaternion home;
                if(pivots.TryGetValue(p.Key,out t)&&baseRotation.TryGetValue(p.Key,out home))
                    t.localRotation=home*Quaternion.AngleAxis(p.Value,Vector3.up);
            }
            foreach(var p in measuredDisplacement)
            {
                Transform t;Vector3 home;
                if(pivots.TryGetValue(p.Key,out t)&&basePosition.TryGetValue(p.Key,out home))
                    t.localPosition=home+p.Value;
            }
        }
    }
}
'''
(T/'SparkLevel93VisualRig.cs').write_text(rig)
print('Created 4 Unity C# helpers, 2 shaders, 100 model references, 92 pivots',flush=True)
