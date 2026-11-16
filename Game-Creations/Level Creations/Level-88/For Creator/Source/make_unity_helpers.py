#!/usr/bin/env python3
"""Creates self-contained Unity C# optional art tools. Not playable gameplay scripts."""
from pathlib import Path
from build_level88 import NAMES,PIVOTS,core
ROOT=Path(__file__).resolve().parent.parent;dst=ROOT/'Unity_Optional';dst.mkdir(exist_ok=True)
models='\n'.join('            "'+m+'",' for m in NAMES)
pivots='\n'.join('            "'+p['Pivot']+'",' for p in PIVOTS)
placement=r'''#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ProjectSpark.EditorTools
{
    /// <summary>Imports visual modular Level 88 FBXs. Does not implement gameplay, SparkTerminals or simulation.</summary>
    public static class SparkLevel88ArtPlacementEditor
    {
        public const string Folder = "Assets/ProjectSpark/Level88/Models/";
        public const string VisualRoot = "L88_HD_VISUAL_ART";
        public static readonly string[] ModelNames = new string[]
        {
__MODEL_NAMES__
        };
        [MenuItem("Tools/Project Spark/Levels/88/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject parent = Selection.activeGameObject;
            if (parent == null)
            {
                EditorUtility.DisplayDialog("Level 88", "Select the Level 88 layout root GameObject in the Hierarchy first.", "OK");
                return;
            }
            var source = new GameObject[ModelNames.Length];
            var missing = new List<string>();
            for (int i=0; i<ModelNames.Length; i++)
            {
                string path = Folder+i.ToString("00")+"_"+ModelNames[i]+".fbx";
                source[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (source[i] == null) missing.Add(path);
            }
            if (missing.Count > 0)
            {
                Debug.LogError("[Level88Art] Missing FBX: "+string.Join("; ",missing));
                EditorUtility.DisplayDialog("Level 88", "Import all 90 numbered FBX models first. Missing: "+missing.Count, "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Level 88 Modular Art", 
                    "Replace ONLY the generated visual root '"+VisualRoot+"'? Gameplay objects outside this root remain intact.",
                    "Place Visual Art", "Cancel")) return;
            Transform old = parent.transform.Find(VisualRoot);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var newArt = new GameObject(VisualRoot);
            Undo.RegisterCreatedObjectUndo(newArt, "Create Level 88 visual root");
            Undo.SetTransformParent(newArt.transform,parent.transform,"Parent Level 88 visual root");
            newArt.transform.localPosition=Vector3.zero;
            newArt.transform.localRotation=Quaternion.identity;
            newArt.transform.localScale=Vector3.one;
            int placed=0;
            for (int i=0; i<ModelNames.Length; i++)
            {
                GameObject inst=PrefabUtility.InstantiatePrefab(source[i],newArt.transform) as GameObject;
                if(inst==null) { Debug.LogError("[Level88Art] Cannot instantiate "+ModelNames[i]);continue; }
                Undo.RegisterCreatedObjectUndo(inst,"Place Level 88 model");
                inst.name=ModelNames[i];
                inst.transform.localPosition=Vector3.zero;
                inst.transform.localRotation=Quaternion.identity;
                inst.transform.localScale=Vector3.one;
                placed++;
            }
            EditorUtility.SetDirty(parent);
            if (parent.scene.IsValid()) EditorSceneManager.MarkSceneDirty(parent.scene);
            Selection.activeGameObject=newArt;
            Debug.Log("[Level88Art] Placed "+placed+"/90 modular models. Check FBX centimetre-to-metre conversion EXACTLY ONCE. No gameplay generated.");
        }
    }
}
#endif
'''.replace('__MODEL_NAMES__',models)
(dst/'SparkLevel88ArtPlacementEditor.cs').write_text(placement,encoding='utf8')
validator=r'''#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Level 88 art structure validation only. Does not test electrical/runtime gameplay.</summary>
    public static class SparkLevel88AssetValidatorEditor
    {
        public static readonly string[] PivotNames = new string[]
        {
__PIVOT_NAMES__
        };
        [MenuItem("Tools/Project Spark/Levels/88/Validate Imported HD Art")]
        public static void Validate()
        {
            int found=0;
            for(int i=0;i<SparkLevel88ArtPlacementEditor.ModelNames.Length;i++)
            {
                string path=SparkLevel88ArtPlacementEditor.Folder+i.ToString("00")+"_"+SparkLevel88ArtPlacementEditor.ModelNames[i]+".fbx";
                if(AssetDatabase.LoadAssetAtPath<GameObject>(path)!=null)found++;
                else Debug.LogError("[Level88Validator] Missing "+path);
            }
            Transform root=null;
            if(Selection.activeGameObject!=null)
            {
                root=Selection.activeGameObject.transform;
                if(root.name!=SparkLevel88ArtPlacementEditor.VisualRoot)root=root.Find(SparkLevel88ArtPlacementEditor.VisualRoot);
            }
            if(root==null)
            {
                Debug.LogWarning("[Level88Validator] Select the visual root or a layout containing it. FBX assets imported: "+found+"/90.");
                return;
            }
            var pc=new Dictionary<string,int>(); var sockets=new HashSet<string>();
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if(t.name.StartsWith("ANIM_")) {int old;pc.TryGetValue(t.name,out old);pc[t.name]=old+1;}
                if(t.name.StartsWith("SOCKET_P"))sockets.Add(t.name);
            }
            int pv=0;
            foreach(string name in PivotNames)
            {
                int count;
                if(pc.TryGetValue(name,out count) && count==1)pv++;
                else Debug.LogError("[Level88Validator] Pivot "+name+" expected one instance; found "+(pc.ContainsKey(name)?pc[name]:0));
            }
            if(sockets.Count!=242)Debug.LogWarning("[Level88Validator] DECORATIVE socket nodes "+sockets.Count+"/242. Functional SparkTerminals not included.");
            Debug.Log("[Level88Validator] FBX "+found+"/90; pivots "+pv+"/72; decorative sockets "+sockets.Count+"/242; MeshRenderers "+root.GetComponentsInChildren<MeshRenderer>(true).Length+". Unity compilation/Play Mode NOT VERIFIED by generator.");
        }
    }
}
#endif
'''.replace('__PIVOT_NAMES__',pivots)
(dst/'SparkLevel88AssetValidatorEditor.cs').write_text(validator,encoding='utf8')
colordata='\n'.join('                {"'+key+'", new Color('+','.join(f'{x:.3f}f' for x in rgba[:3])+')},' for key,rgba in core.COLORS.items())
mat=r'''#if UNITY_EDITOR
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
__COLORS__
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
'''.replace('__COLORS__',colordata)
(dst/'SparkLevel88MaterialUpgradeEditor.cs').write_text(mat,encoding='utf8')
# Rig consumes authority state ONLY. Changes transforms and optional lights; no simulator classes.
rig=r'''using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// ART-ONLY Level 88 animation facade. All setters accept measured ACTUAL states.
    /// It has no electrical topology, ECLSS physics, PLC, safety or objective authority.
    /// All pressures/gas rates and numerical qualification are handled externally.
    /// </summary>
    public sealed class SparkLevel88VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform artRoot;
        [SerializeField] private Light atmosphereHealthyLight;
        [SerializeField] private Light lowOxygenWarningLight;
        [SerializeField] private Light criticalW145FaultLight;
        [SerializeField] private bool autoBind=true;
        private readonly Dictionary<string,Transform> pivots=new Dictionary<string,Transform>();
        private readonly Dictionary<string,Quaternion> homeRotations=new Dictionary<string,Quaternion>();
        private readonly Dictionary<string,Vector3> homePositions=new Dictionary<string,Vector3>();
        private float mainFan01,backupFan01,adsA01,adsB01,waterPump01,oxygenFlow01,condenserFlow01;
        private readonly float[] dampers=new float[4];
        private float airlockOpen01,CO2ValveAngle,oxygenValveAngle,pressureRegAngle,waterFilter01;
        private bool validW145,criticalFault,atmosphereSafe,lowOxygen;
        private void Awake()
        {
            if(artRoot==null)artRoot=transform.Find("L88_HD_VISUAL_ART");
            if(artRoot==null)artRoot=transform;
            if(autoBind)BindVisuals();
        }
        public void BindVisuals()
        {
            pivots.Clear();homeRotations.Clear();homePositions.Clear();
            foreach(Transform t in artRoot.GetComponentsInChildren<Transform>(true))
            {
                if(t.name.StartsWith("ANIM_")&&!pivots.ContainsKey(t.name))pivots.Add(t.name,t);
            }
            foreach(var entry in pivots)
            {
                homeRotations[entry.Key]=entry.Value.localRotation;
                homePositions[entry.Key]=entry.Value.localPosition;
            }
        }
        private void Rotate(string name,Vector3 axis,float degrees)
        {
            Transform p;Quaternion home;
            if(pivots.TryGetValue(name,out p) && homeRotations.TryGetValue(name,out home))
                p.localRotation=home*Quaternion.AngleAxis(degrees,axis);
        }
        private void Move(string name,Vector3 offset)
        {
            Transform p;Vector3 home;
            if(pivots.TryGetValue(name,out p)&&homePositions.TryGetValue(name,out home))p.localPosition=home+offset;
        }
        public void SetActualCO2Processing(float actualBedASaturation01,float actualBedBSaturation01,float actualValveAngleDegrees)
        {
            adsA01=Mathf.Clamp01(actualBedASaturation01);adsB01=Mathf.Clamp01(actualBedBSaturation01);
            CO2ValveAngle=Mathf.Clamp(actualValveAngleDegrees,-180f,180f);
        }
        public void SetActualOxygenProcess(float actualDeliveryFlow01,float actualRegulatorAngleDegrees)
        {
            oxygenFlow01=Mathf.Clamp01(actualDeliveryFlow01);
            oxygenValveAngle=Mathf.Clamp(actualRegulatorAngleDegrees,0f,90f);
        }
        public void SetActualWaterAndHumidity(float actualWaterPumpSpeed01,float actualCondenserFlow01,float actualFilterDrawerOpen01)
        {
            waterPump01=Mathf.Clamp01(actualWaterPumpSpeed01);
            condenserFlow01=Mathf.Clamp01(actualCondenserFlow01);
            waterFilter01=Mathf.Clamp01(actualFilterDrawerOpen01);
        }
        public void SetActualVentilation(float actualMainFanSpeed01,float actualBackupFanSpeed01,
            float actualDamperAOpen01,float actualDamperBOpen01,float actualDamperCOpen01,float actualDamperDOpen01)
        {
            mainFan01=Mathf.Clamp01(actualMainFanSpeed01);backupFan01=Mathf.Clamp01(actualBackupFanSpeed01);
            dampers[0]=Mathf.Clamp01(actualDamperAOpen01);dampers[1]=Mathf.Clamp01(actualDamperBOpen01);
            dampers[2]=Mathf.Clamp01(actualDamperCOpen01);dampers[3]=Mathf.Clamp01(actualDamperDOpen01);
        }
        public void SetActualSafety(bool actualW145TrustedAndFresh,bool actualCriticalFaultLatched,bool actualAtmosphereQualified,bool actualLowOxygen,bool actualAirlockOpen,float actualPressureRegulatorDegrees)
        {
            validW145=actualW145TrustedAndFresh;criticalFault=actualCriticalFaultLatched;
            atmosphereSafe=actualAtmosphereQualified;lowOxygen=actualLowOxygen;
            airlockOpen01=actualAirlockOpen?1f:0f;pressureRegAngle=Mathf.Clamp(actualPressureRegulatorDegrees,0f,90f);
        }
        private float fanRotA,fanRotB,pumpRot;
        private void LateUpdate()
        {
            if(pivots.Count==0)return;
            fanRotA=Mathf.Repeat(fanRotA+mainFan01*Time.deltaTime*650f,360f);
            fanRotB=Mathf.Repeat(fanRotB+backupFan01*Time.deltaTime*650f,360f);
            pumpRot=Mathf.Repeat(pumpRot+waterPump01*Time.deltaTime*360f,360f);
            Rotate("ANIM_PRIMARY_BLOWER_ROTOR",Vector3.forward,fanRotA);
            Rotate("ANIM_BACKUP_BLOWER_ROTOR",Vector3.forward,fanRotB);
            Rotate("ANIM_RECOVERED_WATER_PUMP",Vector3.forward,pumpRot);
            Rotate("ANIM_WATER_FEED_PUMP",Vector3.forward,pumpRot);
            Rotate("ANIM_ADSORBER_SWITCHING_MANIFOLD",Vector3.up,CO2ValveAngle);
            Rotate("ANIM_O2_DELIVERY_REGULATOR",Vector3.up,oxygenValveAngle);
            Rotate("ANIM_PRESSURE_REGULATOR",Vector3.up,pressureRegAngle);
            string[] names={"ANIM_DAMPER_A","ANIM_DAMPER_B","ANIM_DAMPER_C","ANIM_DAMPER_D"};
            for(int i=0;i<4;i++)Rotate(names[i],Vector3.forward,dampers[i]*90f);
            Move("ANIM_ADSORBENT_A_COLUMN_SCAN",Vector3.up*(adsA01*.3f));
            Move("ANIM_ADSORBENT_B_COLUMN_SCAN",Vector3.up*(adsB01*.3f));
            Move("ANIM_WATER_FILTER_DRAWER",Vector3.right*(waterFilter01*.33f));
            Move("ANIM_AIRLOCK_DOOR",Vector3.right*(airlockOpen01*.50f));
            if(atmosphereHealthyLight!=null)atmosphereHealthyLight.enabled=atmosphereSafe&&!criticalFault;
            if(lowOxygenWarningLight!=null)lowOxygenWarningLight.enabled=lowOxygen;
            if(criticalW145FaultLight!=null)criticalW145FaultLight.enabled=criticalFault||!validW145;
        }
    }
}
'''
(dst/'SparkLevel88VisualRig.cs').write_text(rig,encoding='utf8')
print('GENERATED',len(list(dst.glob('*.cs'))),'UNITY ART-ONLY HELPERS')
