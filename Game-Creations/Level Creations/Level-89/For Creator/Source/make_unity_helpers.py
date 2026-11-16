#!/usr/bin/env python3
"""Creates self-contained Unity C# optional art tools. Not playable gameplay scripts."""
from pathlib import Path
from build_level89 import NAMES,PIVOTS,core
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
    /// <summary>Imports visual modular Level 89 FBXs. Does not implement gameplay, SparkTerminals or simulation.</summary>
    public static class SparkLevel89ArtPlacementEditor
    {
        public const string Folder = "Assets/ProjectSpark/Level89/Models/";
        public const string VisualRoot = "L89_HD_VISUAL_ART";
        public static readonly string[] ModelNames = new string[]
        {
__MODEL_NAMES__
        };
        [MenuItem("Tools/Project Spark/Levels/89/Place HD Art Under Selected Layout")]
        public static void Place()
        {
            GameObject parent = Selection.activeGameObject;
            if (parent == null)
            {
                EditorUtility.DisplayDialog("Level 89", "Select the Level 89 layout root GameObject in the Hierarchy first.", "OK");
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
                Debug.LogError("[Level89Art] Missing FBX: "+string.Join("; ",missing));
                EditorUtility.DisplayDialog("Level 89", "Import all 92 numbered FBX models first. Missing: "+missing.Count, "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Level 89 Modular Art", 
                    "Replace ONLY the generated visual root '"+VisualRoot+"'? Gameplay objects outside this root remain intact.",
                    "Place Visual Art", "Cancel")) return;
            Transform old = parent.transform.Find(VisualRoot);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            var newArt = new GameObject(VisualRoot);
            Undo.RegisterCreatedObjectUndo(newArt, "Create Level 89 visual root");
            Undo.SetTransformParent(newArt.transform,parent.transform,"Parent Level 89 visual root");
            newArt.transform.localPosition=Vector3.zero;
            newArt.transform.localRotation=Quaternion.identity;
            newArt.transform.localScale=Vector3.one;
            int placed=0;
            for (int i=0; i<ModelNames.Length; i++)
            {
                GameObject inst=PrefabUtility.InstantiatePrefab(source[i],newArt.transform) as GameObject;
                if(inst==null) { Debug.LogError("[Level89Art] Cannot instantiate "+ModelNames[i]);continue; }
                Undo.RegisterCreatedObjectUndo(inst,"Place Level 89 model");
                inst.name=ModelNames[i];
                inst.transform.localPosition=Vector3.zero;
                inst.transform.localRotation=Quaternion.identity;
                inst.transform.localScale=Vector3.one;
                placed++;
            }
            EditorUtility.SetDirty(parent);
            if (parent.scene.IsValid()) EditorSceneManager.MarkSceneDirty(parent.scene);
            Selection.activeGameObject=newArt;
            Debug.Log("[Level89Art] Placed "+placed+"/92 modular models. Check FBX centimetre-to-metre conversion EXACTLY ONCE. No gameplay generated.");
        }
    }
}
#endif
'''.replace('__MODEL_NAMES__',models)
(dst/'SparkLevel89ArtPlacementEditor.cs').write_text(placement,encoding='utf8')
validator=r'''#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Level 89 art structure validation only. Does not test electrical/runtime gameplay.</summary>
    public static class SparkLevel89AssetValidatorEditor
    {
        public static readonly string[] PivotNames = new string[]
        {
__PIVOT_NAMES__
        };
        [MenuItem("Tools/Project Spark/Levels/89/Validate Imported HD Art")]
        public static void Validate()
        {
            int found=0;
            for(int i=0;i<SparkLevel89ArtPlacementEditor.ModelNames.Length;i++)
            {
                string path=SparkLevel89ArtPlacementEditor.Folder+i.ToString("00")+"_"+SparkLevel89ArtPlacementEditor.ModelNames[i]+".fbx";
                if(AssetDatabase.LoadAssetAtPath<GameObject>(path)!=null)found++;
                else Debug.LogError("[Level89Validator] Missing "+path);
            }
            Transform root=null;
            if(Selection.activeGameObject!=null)
            {
                root=Selection.activeGameObject.transform;
                if(root.name!=SparkLevel89ArtPlacementEditor.VisualRoot)root=root.Find(SparkLevel89ArtPlacementEditor.VisualRoot);
            }
            if(root==null)
            {
                Debug.LogWarning("[Level89Validator] Select the visual root or a layout containing it. FBX assets imported: "+found+"/92.");
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
                else Debug.LogError("[Level89Validator] Pivot "+name+" expected one instance; found "+(pc.ContainsKey(name)?pc[name]:0));
            }
            if(sockets.Count!=248)Debug.LogWarning("[Level89Validator] DECORATIVE socket nodes "+sockets.Count+"/248. Functional SparkTerminals not included.");
            Debug.Log("[Level89Validator] FBX "+found+"/92; pivots "+pv+"/76; decorative sockets "+sockets.Count+"/248; MeshRenderers "+root.GetComponentsInChildren<MeshRenderer>(true).Length+". Unity compilation/Play Mode NOT VERIFIED by generator.");
        }
    }
}
#endif
'''.replace('__PIVOT_NAMES__',pivots)
(dst/'SparkLevel89AssetValidatorEditor.cs').write_text(validator,encoding='utf8')
colordata='\n'.join('                {"'+key+'", new Color('+','.join(f'{x:.3f}f' for x in rgba[:3])+')},' for key,rgba in core.COLORS.items())
mat=r'''#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Assigns source-palette PBR-like materials, not baked production UV textures.</summary>
    public static class SparkLevel89MaterialUpgradeEditor
    {
        [MenuItem("Tools/Project Spark/Levels/89/Create & Apply HD Materials To Selected Art")]
        public static void Upgrade()
        {
            GameObject target=Selection.activeGameObject;
            if(target==null){EditorUtility.DisplayDialog("Level 89", "Select L89_HD_VISUAL_ART in Hierarchy.", "OK");return;}
            const string folder="Assets/ProjectSpark/Level89/Materials";
            if(!AssetDatabase.IsValidFolder("Assets/ProjectSpark")) AssetDatabase.CreateFolder("Assets","ProjectSpark");
            if(!AssetDatabase.IsValidFolder("Assets/ProjectSpark/Level89")) AssetDatabase.CreateFolder("Assets/ProjectSpark","Level89");
            if(!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/ProjectSpark/Level89","Materials");
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null)shader=Shader.Find("HDRP/Lit");
            if(shader==null)shader=Shader.Find("Standard");
            if(shader==null){Debug.LogError("[Level89Materials] No compatible Lit shader found");return;}
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
                Undo.RecordObject(ren,"Apply Level 89 preview materials");ren.sharedMaterials=current;EditorUtility.SetDirty(ren);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Level89Materials] Replaced "+applied+" material slots with HDRP/URP/Standard preview palette. Procedural texture PNGs require authored UVs, repacking and shader setup; not auto-baked.");
        }
    }
}
#endif
'''.replace('__COLORS__',colordata)
(dst/'SparkLevel89MaterialUpgradeEditor.cs').write_text(mat,encoding='utf8')
# Read-only actual-state visual animation facade. It deliberately implements NO Unity gameplay.
rig=r'''using System.Collections.Generic;
using UnityEngine;
namespace ProjectSpark.Visuals
{
    /// <summary>Level 89 read-only art rig. Simulation, wiring, dosimetry, safety and objective authority are external.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel89VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform artRoot;
        [SerializeField] private Light greenQualifiedLight;
        [SerializeField] private Light amberStormWarningLight;
        [SerializeField] private Light redCriticalFaultLight;
        private readonly Dictionary<string,Transform> pivots=new Dictionary<string,Transform>();
        private readonly Dictionary<string,Vector3> homePos=new Dictionary<string,Vector3>();
        private readonly Dictionary<string,Quaternion> homeRot=new Dictionary<string,Quaternion>();
        private readonly float[] actualShieldPosition01 = new float[4];
        private readonly float[] actualServoPosition01 = new float[4];
        private readonly float[] actualWaterLevel01 = new float[4];
        private float actualDoorClosed01,actualLocksEngaged01,actualPumpA01,actualPumpB01,actualAirFan01,actualSurveyAzimuth;
        private float measuredStormIntensity01,measuredShieldCoverage01,measuredShelterDoseVisual01;
        private bool trustedW149Valid,criticalFaultLatched,shelterQualified;
        private float pumpAngleA,pumpAngleB,fanAngle;
        private void Awake(){
            if(artRoot==null) artRoot=transform.Find("L89_HD_VISUAL_ART");
            if(artRoot==null) artRoot=transform;
            BindVisuals();
        }
        public void BindVisuals(){
            pivots.Clear();homePos.Clear();homeRot.Clear();
            foreach(Transform t in artRoot.GetComponentsInChildren<Transform>(true)) {
                if(t.name.StartsWith("ANIM_")&&!pivots.ContainsKey(t.name)) {
                    pivots.Add(t.name,t);homePos.Add(t.name,t.localPosition);homeRot.Add(t.name,t.localRotation);
                }
            }
        }
        private void Rotate(string key,Vector3 axis,float degrees){
            Transform t;Quaternion q;
            if(pivots.TryGetValue(key,out t)&&homeRot.TryGetValue(key,out q))t.localRotation=q*Quaternion.AngleAxis(degrees,axis);
        }
        private void Move(string key,Vector3 offset){
            Transform t;Vector3 p;
            if(pivots.TryGetValue(key,out t)&&homePos.TryGetValue(key,out p))t.localPosition=p+offset;
        }
        public void SetActualShieldMechanisms(float actualTopClosed01,float actualBottomClosed01,
            float actualPortClosed01,float actualStarboardClosed01,float actualLockEngagement01,bool actualDoorClosed){
            actualShieldPosition01[0]=Mathf.Clamp01(actualTopClosed01);
            actualShieldPosition01[1]=Mathf.Clamp01(actualBottomClosed01);
            actualShieldPosition01[2]=Mathf.Clamp01(actualPortClosed01);
            actualShieldPosition01[3]=Mathf.Clamp01(actualStarboardClosed01);
            actualLocksEngaged01=Mathf.Clamp01(actualLockEngagement01);
            actualDoorClosed01=actualDoorClosed?1f:0f;
        }
        public void SetActualWaterAndPumps(float actualPortA01,float actualPortB01,
            float actualStarboardA01,float actualStarboardB01,float actualPumpASpeed01,float actualPumpBSpeed01){
            actualWaterLevel01[0]=Mathf.Clamp01(actualPortA01);
            actualWaterLevel01[1]=Mathf.Clamp01(actualPortB01);
            actualWaterLevel01[2]=Mathf.Clamp01(actualStarboardA01);
            actualWaterLevel01[3]=Mathf.Clamp01(actualStarboardB01);
            actualPumpA01=Mathf.Clamp01(actualPumpASpeed01);actualPumpB01=Mathf.Clamp01(actualPumpBSpeed01);
        }
        public void SetActualSubsystemMotion(float actualServoA01,float actualServoB01,
            float actualServoC01,float actualServoD01,float actualVentilationFan01,float actualSurveyAzimuthDeg){
            actualServoPosition01[0]=Mathf.Clamp01(actualServoA01);
            actualServoPosition01[1]=Mathf.Clamp01(actualServoB01);
            actualServoPosition01[2]=Mathf.Clamp01(actualServoC01);
            actualServoPosition01[3]=Mathf.Clamp01(actualServoD01);
            actualAirFan01=Mathf.Clamp01(actualVentilationFan01);actualSurveyAzimuth=actualSurveyAzimuthDeg;
        }
        public void SetActualRadiationAndSafety(float measuredActualStormSeverity01,float measuredShieldCoverage01,
            float measuredShelterDoseVisual01,bool trustedW149FeedbackFreshAndValid,
            bool actualCriticalFaultLatched,bool actualShelterMissionQualified){
            measuredStormIntensity01=Mathf.Clamp01(measuredActualStormSeverity01);
            this.measuredShieldCoverage01=Mathf.Clamp01(measuredShieldCoverage01);
            this.measuredShelterDoseVisual01=Mathf.Clamp01(measuredShelterDoseVisual01);
            trustedW149Valid=trustedW149FeedbackFreshAndValid;
            criticalFaultLatched=actualCriticalFaultLatched;shelterQualified=actualShelterMissionQualified;
        }
        private void LateUpdate(){
            string[] cas={"ANIM_SHIELD_CASSETTE_TOP","ANIM_SHIELD_CASSETTE_BOTTOM","ANIM_SHIELD_CASSETTE_PORT","ANIM_SHIELD_CASSETTE_STARBOARD"};
            string[] srv={"ANIM_SHIELD_ACTUATOR_A","ANIM_SHIELD_ACTUATOR_B","ANIM_SHIELD_ACTUATOR_C","ANIM_SHIELD_ACTUATOR_D"};
            for(int i=0;i<4;i++) {
                Vector3 direction=i==0?Vector3.down:i==1?Vector3.up:i==2?Vector3.right:Vector3.left;
                Move(cas[i],direction*(1f-actualShieldPosition01[i])*.42f);
                Move(srv[i],Vector3.right*(actualServoPosition01[i]*.24f));
            }
            Move("ANIM_SHELTER_MAIN_SHIELD_DOOR",Vector3.down*(1f-actualDoorClosed01)*1.25f);
            Move("ANIM_LOCKING_PIN_A",Vector3.forward*actualLocksEngaged01*.20f);
            Move("ANIM_LOCKING_PIN_B",Vector3.forward*actualLocksEngaged01*.20f);
            pumpAngleA=Mathf.Repeat(pumpAngleA+Time.deltaTime*actualPumpA01*400f,360);
            pumpAngleB=Mathf.Repeat(pumpAngleB+Time.deltaTime*actualPumpB01*400f,360);
            fanAngle=Mathf.Repeat(fanAngle+Time.deltaTime*actualAirFan01*580f,360);
            Rotate("ANIM_WATER_PUMP_A_ROTOR",Vector3.forward,pumpAngleA);
            Rotate("ANIM_WATER_PUMP_B_ROTOR",Vector3.forward,pumpAngleB);
            Rotate("ANIM_SHELTER_AIR_BLOWER",Vector3.forward,fanAngle);
            Rotate("ANIM_SURVEY_ANTENNA_AZIMUTH",Vector3.up,actualSurveyAzimuth);
            if(greenQualifiedLight!=null)greenQualifiedLight.enabled=shelterQualified&&!criticalFaultLatched;
            if(amberStormWarningLight!=null)amberStormWarningLight.enabled=measuredStormIntensity01>.50f;
            if(redCriticalFaultLight!=null)redCriticalFaultLight.enabled=criticalFaultLatched||!trustedW149Valid;
        }
    }
}
'''
(dst/'SparkLevel89VisualRig.cs').write_text(rig,encoding='utf8')
print('LEVEL89 Unity art-only helpers',len(list(dst.glob('*.cs'))))
