#!/usr/bin/env python3
"""Generate art-only Unity 2022.3+ integration helpers for the Level 73 FBX pack."""
from pathlib import Path
import json
ROOT=Path('/mnt/data/ProjectSpark_Level73_HD_AAA');ED=ROOT/'Unity_Optional';ED.mkdir(exist_ok=True)
models=json.loads((ROOT/'Model_Manifest.json').read_text())['models']
namelist=',\n'.join('            "'+m['file'][:-4]+'"' for m in models)
art=f'''#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.EditorTools
{{
    /// <summary>Places Level 73 FBX art only; does not construct 98 player wires, 152 SparkTerminals,
    /// CMP physics, six objectives, or an authoritative LevelGamePlayManager.</summary>
    public static class SparkLevel73ArtPlacementEditor
    {{
        private const string ArtRoot = "L73_HD_VISUAL_ART";
        private static readonly string[] Models = {{
{namelist}
        }};
        [MenuItem("Tools/Project Spark/Levels/73/Place HD Art Under Selected Layout")]
        public static void Place()
        {{
            var destination = Selection.activeGameObject;
            if (destination == null)
            {{
                EditorUtility.DisplayDialog("Level 73 Art", "Select the Level 73 layout root first.", "OK");
                return;
            }}
            GameObject[] prefabs = new GameObject[Models.Length];
            for (int i=0; i<Models.Length; i++)
            {{
                string found = null;
                foreach (string guid in AssetDatabase.FindAssets(Models[i]+" t:Model"))
                {{
                    string path=AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetExtension(path).Equals(".fbx",StringComparison.OrdinalIgnoreCase) &&
                        Path.GetFileNameWithoutExtension(path).Equals(Models[i],StringComparison.Ordinal))
                    {{ found=path; break; }}
                }}
                if (found==null || (prefabs[i]=AssetDatabase.LoadAssetAtPath<GameObject>(found))==null)
                {{
                    EditorUtility.DisplayDialog("Level 73 FBX missing",Models[i]+".fbx not imported.\nImport all 54 numbered FBX models. No changes made.","OK");
                    return;
                }}
            }}
            Transform previous=destination.transform.Find(ArtRoot);
            if (previous != null && !EditorUtility.DisplayDialog("Replace Level 73 visual art?",
                "Replace only the "+ArtRoot+" child? Other gameplay objects are preserved.","Replace Visual Art","Cancel")) return;
            Undo.IncrementCurrentGroup();int g=Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Place Project Spark Level 73 CMP art");
            if (previous!=null) Undo.DestroyObjectImmediate(previous.gameObject);
            GameObject root=new GameObject(ArtRoot);
            Undo.RegisterCreatedObjectUndo(root,"Create Level 73 art root");
            root.transform.SetParent(destination.transform,false);
            root.transform.localPosition=Vector3.zero;
            root.transform.localRotation=Quaternion.identity;
            root.transform.localScale=Vector3.one;
            for (int i=0;i<Models.Length;i++)
            {{
                GameObject o=PrefabUtility.InstantiatePrefab(prefabs[i],root.transform) as GameObject;
                if(o==null) throw new InvalidOperationException("Cannot instantiate "+Models[i]);
                Undo.RegisterCreatedObjectUndo(o,"Place "+Models[i]);
                o.name=Models[i];
                o.transform.localPosition=Vector3.zero;
                o.transform.localRotation=Quaternion.identity;
                o.transform.localScale=Vector3.one;
            }}
            Undo.CollapseUndoOperations(g);
            EditorUtility.SetDirty(root);
            Debug.Log("[Level 73 Art] Placed "+Models.Length+" independent FBXs under "+ArtRoot+". Art only. Verify cm-to-m import conversion (workstation ~17.15m wide).");
        }}
    }}
}}
#endif
'''
(ED/'SparkLevel73ArtPlacementEditor.cs').write_text(art)

palette=json.loads((ROOT/'Material_Palette.json').read_text())['materials']
matnames=[k[3:] for k in palette]
matname_str=','.join('"'+n+'"' for n in matnames)
mathex=','.join('"#%02X%02X%02X"'%tuple(palette['PS_'+n]['rgba_255'][:3]) for n in matnames)
material=f'''#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace ProjectSpark.EditorTools
{{
    /// <summary>Editable palette material preview for Level 73. Does not create PBR maps.</summary>
    public static class SparkLevel73MaterialUpgradeEditor
    {{
        private const string Folder="Assets/ProjectSpark/Level73/Materials";
        private static readonly string[] Names={{{matname_str}}};
        private static readonly string[] Hex={{{mathex}}};
        [MenuItem("Tools/Project Spark/Levels/73/Create & Apply HD Materials To Selected Art")]
        public static void Apply()
        {{
            if(Selection.activeGameObject==null)
            {{
                EditorUtility.DisplayDialog("Level 73 materials","Select L73_HD_VISUAL_ART or its parent.","OK");return;
            }}
            EnsureFolder(Folder);
            Shader sh=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
            if(sh==null){{ Debug.LogError("[L73] URP/HDRP/Standard shader not found.");return; }}
            Material[] mats=new Material[Names.Length];
            for(int i=0;i<Names.Length;i++)
            {{
                string path=Folder+"/PS_"+Names[i]+".mat";
                Material m=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(m==null){{m=new Material(sh){{name="PS_"+Names[i]}};AssetDatabase.CreateAsset(m,path);}}
                Undo.RecordObject(m,"Update Level 73 material");
                Color c;
                if(!ColorUtility.TryParseHtmlString(Hex[i],out c)) c=Color.gray;
                bool transparent=Names[i]=="glass" || Names[i]=="slurry" || Names[i]=="plasma" || Names[i]=="purpleglass";
                if(transparent) c.a=Names[i]=="glass"?.32f:.62f;
                if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",c);
                if(m.HasProperty("_Color"))m.SetColor("_Color",c);
                m.color=c;
                bool metal=Names[i]=="steel" || Names[i]=="light" || Names[i]=="silver" || Names[i]=="gold" || Names[i]=="copper";
                if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metal?.84f:.12f);
                if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",metal?.81f:.63f);
                if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",metal?.81f:.63f);
                if(transparent)
                {{
                    if(m.HasProperty("_Surface"))m.SetFloat("_Surface",1f);
                    if(m.HasProperty("_Mode"))m.SetFloat("_Mode",3f);
                    m.SetOverrideTag("RenderType","Transparent");
                    m.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);
                    m.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);
                    m.SetInt("_ZWrite",0);
                    m.EnableKeyword("_ALPHABLEND_ON");
                    m.renderQueue=(int)RenderQueue.Transparent;
                }}
                mats[i]=m; EditorUtility.SetDirty(m);
            }}
            int changed=0;
            foreach(Renderer r in Selection.activeGameObject.GetComponentsInChildren<Renderer>(true))
            {{
                Material[] old=r.sharedMaterials;Material[] next=(Material[])old.Clone();bool different=false;
                for(int j=0;j<old.Length;j++)
                {{
                    if(old[j]==null)continue;
                    for(int i=0;i<Names.Length;i++)
                        if(old[j].name.IndexOf("PS_"+Names[i],StringComparison.OrdinalIgnoreCase)>=0)
                        {{ if(next[j]!=mats[i]){{next[j]=mats[i];different=true;}}break; }}
                }}
                if(different){{Undo.RecordObject(r,"Apply Level 73 material");r.sharedMaterials=next;changed++;}}
            }}
            AssetDatabase.SaveAssets();
            Debug.Log("[L73 Materials] "+Names.Length+" material palettes created/updated; "+changed+" renderers changed. Adjust transparency for your render pipeline.");
        }}
        private static void EnsureFolder(string path)
        {{
            string[] a=path.Split('/');string cur=a[0];
            for(int i=1;i<a.Length;i++){{string next=cur+"/"+a[i];if(!AssetDatabase.IsValidFolder(next))AssetDatabase.CreateFolder(cur,a[i]);cur=next;}}
        }}
    }}
}}
#endif
'''
(ED/'SparkLevel73MaterialUpgradeEditor.cs').write_text(material)

rig='''using UnityEngine;
namespace ProjectSpark.Visuals
{
    /// <summary>Level 73 ART-ONLY rig: consumes actual simulated values. No CMP physics, no wires, no objectives.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel73VisualRig : MonoBehaviour
    {
        [Header("Pivot names from Animation_Rigs.csv / FBX parent null groups")]
        [SerializeField] private Transform platen;
        [SerializeField] private Transform pad;
        [SerializeField] private Transform platenServo;
        [SerializeField] private Transform carrierSpindle;
        [SerializeField] private Transform carrierZ;
        [SerializeField] private Transform conditionerArm;
        [SerializeField] private Transform conditionerDisc;
        [SerializeField] private Transform slurryPump;
        [SerializeField] private Transform slurryArm;
        [SerializeField] private Transform slurryNozzle;
        [SerializeField] private Transform waferLift;
        [SerializeField] private Transform robotBase;
        [SerializeField] private Transform robotArm;
        [SerializeField] private Transform robotTool;
        [SerializeField] private Transform cleaningBrush;
        [SerializeField] private Transform spinDryer;
        [SerializeField] private Transform profilometer;
        [SerializeField] private ParticleSystem slurryVFX;
        [SerializeField] private Light processLight;
        private float platenRPM,carrierRPM,conditionerRPM,slurryPumpRPM,brushRPM,dryerRPM;
        private float carrierHeight,armFraction,nozzleHeight,waferLiftFraction;
        private float robotBaseAngle,robotArmAngle,robotToolAngle,inspectionFraction;
        private bool slurryOn, polishOn, faulted;
        private Vector3 homeCarrierZ,homeNozzle,homeLift,homeProfilometer;
        private Quaternion homeCondArm,homeSlurryArm,homeRobotBase,homeRobotArm,homeRobotTool;
        private void Awake()
        {
            AutoBindByPivotName();
            if(carrierZ!=null) homeCarrierZ=carrierZ.localPosition;
            if(slurryNozzle!=null) homeNozzle=slurryNozzle.localPosition;
            if(waferLift!=null) homeLift=waferLift.localPosition;
            if(profilometer!=null) homeProfilometer=profilometer.localPosition;
            homeCondArm=LocalRotation(conditionerArm);
            homeSlurryArm=LocalRotation(slurryArm);
            homeRobotBase=LocalRotation(robotBase);
            homeRobotArm=LocalRotation(robotArm);
            homeRobotTool=LocalRotation(robotTool);
        }
        private static Quaternion LocalRotation(Transform t) => t!=null?t.localRotation:Quaternion.identity;
        private void AutoBindByPivotName()
        {
            var ts=GetComponentsInChildren<Transform>(true);
            foreach(var t in ts)
            {
                switch(t.name)
                {
                    case "ANIM_PLATEN_ROTATION": if(platen==null)platen=t;break;
                    case "ANIM_PAD_ROTATION": if(pad==null)pad=t;break;
                    case "ANIM_PLATEN_SERVO_ROTOR": if(platenServo==null)platenServo=t;break;
                    case "ANIM_CARRIER_ROTATION": if(carrierSpindle==null)carrierSpindle=t;break;
                    case "ANIM_CARRIER_Z": if(carrierZ==null)carrierZ=t;break;
                    case "ANIM_CONDITIONER_ARM": if(conditionerArm==null)conditionerArm=t;break;
                    case "ANIM_CONDITIONER_DISC": if(conditionerDisc==null)conditionerDisc=t;break;
                    case "ANIM_SLURRY_PUMP": if(slurryPump==null)slurryPump=t;break;
                    case "ANIM_SLURRY_ARM": if(slurryArm==null)slurryArm=t;break;
                    case "ANIM_SLURRY_NOZZLE": if(slurryNozzle==null)slurryNozzle=t;break;
                    case "ANIM_WAFER_LIFT": if(waferLift==null)waferLift=t;break;
                    case "ANIM_WAFER_ROBOT_BASE": if(robotBase==null)robotBase=t;break;
                    case "ANIM_WAFER_ROBOT_ARM": if(robotArm==null)robotArm=t;break;
                    case "ANIM_WAFER_ROBOT_TOOL": if(robotTool==null)robotTool=t;break;
                    case "ANIM_CLEANING_BRUSH": if(cleaningBrush==null)cleaningBrush=t;break;
                    case "ANIM_SPIN_DRYER": if(spinDryer==null)spinDryer=t;break;
                    case "ANIM_PROFILOMETER": if(profilometer==null)profilometer=t;break;
                }
            }
        }
        /// <summary>RPM values are measured/derived by external simulation; not commands.</summary>
        public void SetActualRPM(float platenRpm,float carrierRpm,float conditionerRpm,float slurryPumpRpm,float brushRpm,float dryerRpm)
        {
            platenRPM=Mathf.Max(0,platenRpm);carrierRPM=Mathf.Max(0,carrierRpm);
            conditionerRPM=Mathf.Max(0,conditionerRpm);slurryPumpRPM=Mathf.Max(0,slurryPumpRpm);
            brushRPM=Mathf.Max(0,brushRpm);dryerRPM=Mathf.Max(0,dryerRpm);
        }
        /// <summary>Positions in normalized 0..1 range. Actual force state is owned externally.</summary>
        public void SetActualActuatorPositions(float carrierZFraction,float conditionerArmFraction,float slurryArmFraction,float nozzleFraction,float waferLiftFraction)
        {
            carrierHeight=Mathf.Clamp01(carrierZFraction);armFraction=Mathf.Clamp01(conditionerArmFraction);
            nozzleHeight=Mathf.Clamp01(nozzleFraction);waferLiftFraction=Mathf.Clamp01(waferLiftFraction);
            this.slurryArmFraction=Mathf.Clamp01(slurryArmFraction);
        }
        private float slurryArmFraction;
        public void SetWaferRobotPose(float baseDeg,float armDeg,float toolDeg)
        {robotBaseAngle=baseDeg;robotArmAngle=armDeg;robotToolAngle=toolDeg;}
        public void SetProcessVisualState(bool measuredSlurryFlowPresent,bool actualPolishingContact,bool criticalFault,float scanFraction)
        {
            slurryOn=measuredSlurryFlowPresent;polishOn=actualPolishingContact;faulted=criticalFault;inspectionFraction=Mathf.Clamp01(scanFraction);
        }
        private static void SpinY(Transform t,float rpm,float dt){if(t!=null)t.Rotate(Vector3.up,rpm*6f*dt,Space.Self);}
        private void Update()
        {
            float dt=Time.deltaTime;
            SpinY(platen,platenRPM,dt);SpinY(pad,platenRPM,dt);SpinY(platenServo,platenRPM,dt);
            SpinY(carrierSpindle,carrierRPM,dt);SpinY(conditionerDisc,conditionerRPM,dt);SpinY(slurryPump,slurryPumpRPM,dt);
            SpinY(cleaningBrush,brushRPM,dt);SpinY(spinDryer,dryerRPM,dt);
            if(carrierZ!=null)carrierZ.localPosition=homeCarrierZ+Vector3.up*(-.22f*carrierHeight);
            if(conditionerArm!=null)conditionerArm.localRotation=homeCondArm*Quaternion.Euler(0f,45f*armFraction,0f);
            if(slurryArm!=null)slurryArm.localRotation=homeSlurryArm*Quaternion.Euler(0f,35f*slurryArmFraction,0f);
            if(slurryNozzle!=null)slurryNozzle.localPosition=homeNozzle+Vector3.up*(-.16f*nozzleHeight);
            if(waferLift!=null)waferLift.localPosition=homeLift+Vector3.up*(.17f*waferLiftFraction);
            if(robotBase!=null)robotBase.localRotation=homeRobotBase*Quaternion.Euler(0f,robotBaseAngle,0f);
            if(robotArm!=null)robotArm.localRotation=homeRobotArm*Quaternion.Euler(0f,0f,robotArmAngle);
            if(robotTool!=null)robotTool.localRotation=homeRobotTool*Quaternion.Euler(0f,0f,robotToolAngle);
            if(profilometer!=null)profilometer.localPosition=homeProfilometer+Vector3.right*(.7f*inspectionFraction);
            if(slurryVFX!=null)
            {if(slurryOn && !slurryVFX.isPlaying)slurryVFX.Play();else if(!slurryOn && slurryVFX.isPlaying)slurryVFX.Stop(true,ParticleSystemStopBehavior.StopEmitting);}
            if(processLight!=null){processLight.color=faulted?new Color(1f,.23f,.14f):(polishOn?new Color(.09f,.89f,1f):Color.white);processLight.intensity=polishOn?2f:.45f;}
        }
    }
}
'''
(ED/'SparkLevel73VisualRig.cs').write_text(rig)
print('wrote Unity art placement, palette and runtime visual rig helpers')
