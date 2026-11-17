#!/usr/bin/env python3
"""Build optional Unity editor/runtime helpers for Level 94. Art/data only, not gameplay simulation."""
from pathlib import Path
import json,build_level94 as b
R=Path(__file__).resolve().parents[1];U=R/'Unity';U.mkdir(exist_ok=True);E=U/'Editor';E.mkdir(exist_ok=True)
matmap=json.loads((R/'Materials'/'PBR_Material_Bindings.json').read_text())['source_material_map']
matlines=',\n'.join(f'            {{"PS_{k}","{v}"}}' for k,v in sorted(matmap.items()))
models=',\n'.join('            "'+name+'"' for name in b.MODEL_NAMES)
material=r'''#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace ProjectSpark.Level94.EditorTools
{
    /// <summary>Converts the original Level94 PBR maps to editable native Unity materials.
    /// No gameplay state is read or changed. URP/HDRP/Built-In Lit shader chosen at import time.</summary>
    public static class SparkLevel94PBRMaterialEditor
    {
        public const string OutputPath="Assets/ProjectSpark/Level94/GeneratedMaterials";
        private static readonly Dictionary<string,string> Sets = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase) {
__MAP__
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
                    && p.IndexOf("Level94",StringComparison.OrdinalIgnoreCase)>=0) return p;
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
            if(lit==null) { Debug.LogError("[Level94] No native Lit shader: "+shaderName);return Cache; }
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
                    string custom=pipe=="URP"?"ProjectSpark/Level94/HologramURP":pipe=="BuiltIn"?"ProjectSpark/Level94/HologramBuiltin":"HDRP/Unlit";
                    var sh=Shader.Find(custom); if(sh!=null)desired=sh;
                }
                m.shader=desired;
                Texture2D albedo=LoadMap(set+"_BaseColor");
                Texture2D normal=LoadMap(set+"_Normal",true);
                Texture2D mask=LoadMap(set+"_UnityMaskMap");
                Texture2D ao=LoadMap(set+"_Occlusion");
                Texture2D em=LoadMap(set+"_Emission");
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
            Debug.Log("[Level94] Created/updated "+Cache.Count+" native PBR materials for "+pipe+". Actual render outcome must be verified in Unity.");
            return Cache;
        }
        [MenuItem("Tools/Project Spark/Levels/94/1. Create Native PBR Materials")]
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
            Debug.Log("[Level94] Bound "+applied+" PBR material slots; unmatched "+missing+" (check FBX material names).");
        }
        [MenuItem("Tools/Project Spark/Levels/94/3. Apply PBR To Selected Art")]
        public static void ApplySelected(){if(Selection.activeGameObject!=null)ApplyMaterials(Selection.activeGameObject);else Debug.LogError("[Level94] Select Level94 art first.");}
    }
}
#endif
'''.replace('__MAP__',matlines)
(E/'SparkLevel94PBRMaterialEditor.cs').write_text(material)
assembly=r'''#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level94.EditorTools
{
    /// <summary>Art-only assembly of modular binary FBX files. Never adds or modifies gameplay components.</summary>
    public static class SparkLevel94ArtAssembler
    {
        public const string RootName="L94_BOARD_FOUNDRY_VISUALS";
        public static readonly string[] Names = {
__NAMES__
        };
        [MenuItem("Tools/Project Spark/Levels/94/2. Assemble 48 Modular FBXs Under Selected Layout")]
        public static void Assemble()
        {
            GameObject selected=Selection.activeGameObject;
            if(selected==null){Debug.LogError("[Level94] Select a Level94 layout GameObject.");return;}
            var source=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            foreach(var guid in AssetDatabase.FindAssets("t:Model"))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                if(!path.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase))continue;
                string stem=Path.GetFileNameWithoutExtension(path);
                int cut=stem.IndexOf("_L94_",StringComparison.OrdinalIgnoreCase);
                if(cut<0)continue;
                string name=stem.Substring(cut+1);
                if(!source.ContainsKey(name))source[name]=path;
            }
            var missing=Names.Where(n=>!source.ContainsKey(n)).ToArray();
            if(missing.Length>0){Debug.LogError("[Level94] Missing "+missing.Length+" FBXs; examples: "+String.Join(", ",missing.Take(6).ToArray()));return;}
            Transform existing=selected.transform.Find(RootName);
            if(existing!=null)Undo.DestroyObjectImmediate(existing.gameObject);
            GameObject root=new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root,"Create Level94 visual root");root.transform.SetParent(selected.transform,false);
            root.transform.localPosition=Vector3.zero;root.transform.localRotation=Quaternion.identity;root.transform.localScale=Vector3.one;
            var groups=new Dictionary<string,Transform>();
            foreach(string gn in new[]{"00_Bench","01_BareBoard","02_Components_27","03_TestInstruments"})
            {
                var grp=new GameObject(gn);Undo.RegisterCreatedObjectUndo(grp,"Create Level94 group");
                grp.transform.SetParent(root.transform,false);groups[gn]=grp.transform;
            }
            for(int i=0;i<Names.Length;i++)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(source[Names[i]]);
                if(prefab==null){Debug.LogError("[Level94] Could not load "+Names[i]);continue;}
                var part=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if(part==null)continue;
                var group=groups[i<8?"00_Bench":i<16?"01_BareBoard":i<43?"02_Components_27":"03_TestInstruments"];
                part.transform.SetParent(group,false);part.transform.localPosition=Vector3.zero;
                part.transform.localRotation=Quaternion.identity;part.transform.localScale=Vector3.one;
                part.name=Names[i];
            }
            SparkLevel94PBRMaterialEditor.ApplyMaterials(root);
            Selection.activeGameObject=root;
            Debug.Log("[Level94] Assembled 48 modular FBX visual models. This scene does not create actual electrical circuits, component placement tasks or objectives.");
        }
    }
}
#endif
'''.replace('__NAMES__',models)
(E/'SparkLevel94ArtAssembler.cs').write_text(assembly)
runtime=r'''using UnityEngine;
namespace ProjectSpark.Level94
{
    /// <summary>Visual-only toggles for the board's inspection modes.
    /// No authority over gameplay, component placement, copper nets, voltage or objective success.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel94BoardVisualController : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private bool showTop = true;
        [SerializeField] private bool showBottom = true;
        [SerializeField] private bool showPlacedComponentPreview = true;
        [SerializeField, Range(.5f,1.5f)] private float brightness = 1.0f;
        private Transform FindUnderRoot(string n)
        {
            if(visualRoot==null)return null;
            foreach(var t in visualRoot.GetComponentsInChildren<Transform>(true))
                if(t.name==n)return t;
            return null;
        }
        public void SetVisualLayers(bool top,bool bottom)
        {showTop=top;showBottom=bottom;ApplyVisualState();}
        public void ShowAssembledBoardPreview(bool visible)
        {showPlacedComponentPreview=visible;ApplyVisualState();}
        [ContextMenu("Refresh Visual-Only Board Display")]
        public void ApplyVisualState()
        {
            var a=FindUnderRoot("L94_TopGreenSoldermask"); if(a!=null)a.gameObject.SetActive(showTop);
            a=FindUnderRoot("L94_TopSMDGoldPads");if(a!=null)a.gameObject.SetActive(showTop);
            a=FindUnderRoot("L94_BottomGreenSoldermask");if(a!=null)a.gameObject.SetActive(showBottom);
            a=FindUnderRoot("L94_BottomCopperPadSet");if(a!=null)a.gameObject.SetActive(showBottom);
            a=FindUnderRoot("02_Components_27");if(a!=null)a.gameObject.SetActive(showPlacedComponentPreview);
        }
        private void OnValidate(){brightness=Mathf.Clamp(brightness,.5f,1.5f);}
    }
}
'''
(U/'SparkLevel94BoardVisualController.cs').write_text(runtime)
data=r'''using UnityEngine;
namespace ProjectSpark.Level94
{
    /// <summary>Text assets authored in CSV, assigned manually or by Editor importer.
    /// Source-of-truth planning data only; actual SparkTerminal and circuit graph must be built separately.</summary>
    [CreateAssetMenu(fileName="L94_PCBDefinition",menuName="Project Spark/Level94/PCB Planning Data")]
    public sealed class SparkLevel94PCBDefinition : ScriptableObject
    {
        public Vector2 physicalBoardSizeMm=new Vector2(100,70);
        public float physicalThicknessMm=1.6f;
        public int copperLayers=2;
        public float visualPresentationScale=14f;
        public TextAsset componentBOM;
        public TextAsset copperTraceTasks;
        public TextAsset viaTasks;
        public TextAsset logicalPadAnchors;
        public TextAsset factoryNetlist;
        public TextAsset w035CriticalJumper;
        public TextAsset objectiveRequirements;
        public TextAsset faultScenarios;
        public TextAsset testPlan;
    }
}
'''
(U/'SparkLevel94PCBDefinition.cs').write_text(data)
importer=r'''#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level94.EditorTools
{
    public static class SparkLevel94PlanningDataEditor
    {
        private static readonly string Out="Assets/ProjectSpark/Level94/Data/L94_PCBDefinition.asset";
        private static TextAsset Find(string filename)
        {
            foreach(var guid in AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(filename)))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                if(Path.GetFileName(path).Equals(filename,StringComparison.OrdinalIgnoreCase) && path.Contains("Level94"))
                    return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            }
            return null;
        }
        [MenuItem("Tools/Project Spark/Levels/94/4. Create PCB Planning Data Asset")]
        public static void Create()
        {
            SparkLevel94PBRMaterialEditor.EnsureFolder("Assets/ProjectSpark/Level94/Data");
            var asset=AssetDatabase.LoadAssetAtPath<ProjectSpark.Level94.SparkLevel94PCBDefinition>(Out);
            if(asset==null){asset=ScriptableObject.CreateInstance<ProjectSpark.Level94.SparkLevel94PCBDefinition>();AssetDatabase.CreateAsset(asset,Out);}
            asset.componentBOM=Find("Component_BOM_27.csv");
            asset.copperTraceTasks=Find("Copper_Trace_Tasks_W001_W034.csv");
            asset.viaTasks=Find("Required_Via_Placements_V01_V06.csv");
            asset.logicalPadAnchors=Find("Logical_Pad_Locations.csv");
            asset.factoryNetlist=Find("Factory_Existing_Nets.csv");
            asset.w035CriticalJumper=Find("Critical_W035_Feedback.csv");
            asset.objectiveRequirements=Find("Objective_Certification_Plan_6.csv");
            asset.faultScenarios=Find("Three_Fault_Repair_Scenarios.csv");
            asset.testPlan=Find("Engineering_Acceptance_Tests_56.csv");
            EditorUtility.SetDirty(asset);AssetDatabase.SaveAssets();Selection.activeObject=asset;
            Debug.Log("[Level94] Created PCB source-data asset. CSVs describe intended component/trace tasks, not live gameplay connections.");
        }
    }
}
#endif
'''
(E/'SparkLevel94PlanningDataEditor.cs').write_text(importer)
validator=r'''#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level94.EditorTools
{
    public static class SparkLevel94ArtValidator
    {
        [MenuItem("Tools/Project Spark/Levels/94/5. Validate Imported Assets And Plans")]
        public static void Validate()
        {
            int modelCount=0,invalid=0;
            foreach(string guid in AssetDatabase.FindAssets("t:Model"))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                if(!Path.GetFileNameWithoutExtension(path).Contains("_L94_"))continue;
                if(path.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase))modelCount++;
            }
            if(modelCount!=48){Debug.LogError("[Level94] Expected 48 modular FBXs; found "+modelCount);invalid++;}
            string[] required={"Component_BOM_27.csv","Copper_Trace_Tasks_W001_W034.csv","Required_Via_Placements_V01_V06.csv","Logical_Pad_Locations.csv","Critical_W035_Feedback.csv","Objective_Certification_Plan_6.csv"};
            foreach(string file in required)
            {
                if(SparkLevel94PBRMaterialEditor.FindPathExact(file)==null){Debug.LogError("[Level94] Missing "+file);invalid++;}
            }
            var root=Selection.activeGameObject;
            if(root!=null)
            {
                int meshes=root.GetComponentsInChildren<MeshFilter>(true).Count(m=>m.sharedMesh!=null);
                Debug.Log("[Level94] Selected visual root contains "+meshes+" mesh filters.");
            }
            if(invalid==0)Debug.Log("[Level94] Imported art/data presence checks passed. This DOES NOT validate PCB electrical gameplay or Unity Play Mode.");
            else Debug.LogError("[Level94] Validation failed with "+invalid+" missing/incorrect asset groups.");
        }
    }
}
#endif
'''
(E/'SparkLevel94ArtValidator.cs').write_text(validator)
print('UNITY SCRIPTS CREATED',len(list(U.rglob('*.cs'))))
