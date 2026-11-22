#if UNITY_EDITOR
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
            "L94_AssemblyBenchFrame",
            "L94_AntiStaticWorkSurface",
            "L94_PrecisionFixtureBase",
            "L94_FixtureClampLeft",
            "L94_FixtureClampRight",
            "L94_InspectionMicroscopeArm",
            "L94_MicroscopeLensAndRingLight",
            "L94_ComponentRepairTray",
            "L94_FR4BoardSubstrate",
            "L94_TopGreenSoldermask",
            "L94_BottomGreenSoldermask",
            "L94_TopSMDGoldPads",
            "L94_BottomCopperPadSet",
            "L94_SilkscreenAndTestFiducials",
            "L94_CopperEdgeAndMountBosses",
            "L94_W035DetachableFeedbackJumper",
            "L94_U1_MCU_QFN48",
            "L94_U2_LDO_SOT223",
            "L94_U3_I2CTemperatureSensor_SOIC8",
            "L94_U4_CurrentMonitor_SOIC8",
            "L94_Q1_LoadMOSFET_SOT23",
            "L94_D1_InputSchottkySMA",
            "L94_LED1_PowerGreen",
            "L94_LED2_StatusBlue",
            "L94_LED3_LoadAmber",
            "L94_R1_SCLPullup_4K7",
            "L94_R2_SDAPullup_4K7",
            "L94_R3_GateSeries_100R",
            "L94_R4_GatePulldown_100K",
            "L94_R5_PowerLED_1K",
            "L94_R6_StatusLED_330R",
            "L94_R7_LoadLED_330R",
            "L94_R8_Shunt_0R1",
            "L94_C1_InputBulk_10u",
            "L94_C2_OutputBulk_22u",
            "L94_C3_MCUDecoupling_100n",
            "L94_C4_SensorDecoupling_100n",
            "L94_C5_CurrentMonitorDecoupling_100n",
            "L94_C6_ResetFilter_10n",
            "L94_C7_AnalogFilter_1u",
            "L94_SW1_TactileReset",
            "L94_J1_Protected5VTerminal",
            "L94_J2_DebugProgrammingHeader",
            "L94_BenchOscilloscope",
            "L94_BenchDigitalMultimeter",
            "L94_ProtectedBenchPowerSupply",
            "L94_FourChannelLogicAnalyzer",
            "L94_PCBAssemblySCADAPanel"
        };
        [MenuItem("Tools/Project Spark/Levels/101/2. Assemble 48 Modular FBXs Under Selected Layout")]
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

