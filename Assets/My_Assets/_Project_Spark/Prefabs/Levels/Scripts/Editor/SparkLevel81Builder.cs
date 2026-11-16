#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ProjectSpark.Gameplay;
using ProjectSpark.Visuals;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel81Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_81_Layout";
        private const string PrefabPath = Folder + "/Level_81_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the Mission Reliability Network","Commission and Prove Error Correction","Build Fault-Tolerant Computing","Survive the Virtual Radiation Mission","Defeat Complex Digital Failures","Restore Critical ECC Supervision"
        };

        [MenuItem("Tools/Project Spark/Level 81/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 81 prefab already exists at " + PrefabPath);
                SparkLevel81DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_81.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_81.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.71f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.71f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.71f,.16f));

            var root = new GameObject("Level_81_Layout");
            try
            {
                var bench = Child(root.transform,"Reliability Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Temporary board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.71f),board);
                var pins = new List<SparkTerminal>();
                SparkPowerSupply Supply(string name, float voltage, float current)
                {
                    var part=Part<SparkPowerSupply>(bench,name,0,0,steel);
                    Set(part,("outputVoltage",voltage),("currentLimit",current),("state",SparkPowerSupplyState.Active));
                    return part;
                }
                var source=Supply("CONTROL_POWER",5f,.71f);
                var positive=Pin(source,"SUPPLY_POS",0,0,red,pins,SparkTerminalPolarity.Positive);
                var ground=Pin(source,"SUPPLY_RET",0,0,blue,pins,SparkTerminalPolarity.Negative);
                Set(source,("positiveTerminal",positive),("negativeTerminal",ground));
                foreach(var (plus,ret) in new[]{("PLC_5V","PLC_GND"),("ECC_5V","ECC_GND"),("MEM_5V","MEM_GND"),("TMR_5V","TMR_GND"),("OPT_5V","OPT_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"Process I/O endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                string[] wireOrder={"SUPPLY_POS","PLC_5V","SUPPLY_RET","PLC_GND","ECC_5V","ECC_GND","MEM_5V","MEM_GND","TMR_5V","TMR_GND","OPT_5V","OPT_GND","TASK_READY","SEQ_READY","CORE_A_CLOCK","CORE_A_CLK","CORE_B_CLOCK","CORE_B_CLK","CORE_C_CLOCK","CORE_C_CLK","WD_SYNC","WD_CLOCK","WD_STATUS","WD_TIME","WD_REPLY","WD_VERIFY","SCHED_PERMIT","MISSION_SEQ","MEM_ADDR","ECC_ADDR","BANK_A_ADDR","MEM_A_ADDR","BANK_B_ADDR","MEM_B_ADDR","LOCKSTEP_OUT","SEQ_SYNC","REF_READY","REF_VALID","COMMAND_PERMIT","COMMAND_ENABLE","GUIDANCE_CMD","COMPUTE_IN","DEADLINE_OUT","DEADLINE_IN","MEM_WORD","ECC_ENCODE","ECC_A_OUT","BANK_A_WRITE","ECC_B_OUT","BANK_B_WRITE","BANK_A_READ","SYNDROME_A","BANK_B_READ","SYNDROME_B","SINGLE_POS","CORRECT_IN","CORRECT_A","BANK_A_FIX","CORRECT_B","BANK_B_FIX","DOUBLE_BIT","DOUBLE_ALARM","QUARANTINE_CMD","QUARANTINE_IN","SCRUB_RATE","SCRUB_CLOCK","SCRUB_ADDR","SCRUB_BANK","SCRUB_QUERY","SCRUB_SYNDROME","SCRUB_RESULT","CORR_RESULT","SCRUB_BW","SCRUB_ALLOW","SCRUB_LOAD","BW_TELEM","CHECKPOINT_CMD","CHECKPOINT_IN","VAULT_AUTH","BACKUP_ENABLE","RECHECK_OUT","PAGE_VERIFY","CHECKSUM_OUT","VAULT_REF","LOCK_A","LANE_A_IN","LOCK_B","LANE_B_IN","LOCK_C","LANE_C_IN","RESULT_A","VOTE_A","RESULT_B","VOTE_B","RESULT_C","VOTE_C","DISAGREE_OUT","DISAGREE_IN","FAULT_LANE","LANE_ISOLATE","LANE_A_HEALTH","PLC_A_HEALTH","LANE_B_HEALTH","PLC_B_HEALTH","LANE_C_HEALTH","PLC_C_HEALTH","INDEP_REF","REF_CHECK","CORRELATED_OUT","CORRELATED_IN","VOTE_PERMIT","VOTE_AUTH","VOTE_RESULT","COMMAND_VOTE","REF_GATE","VALID_CMD","CMD_SAFE","ACT_CMD","ACTUATE","SENSOR_PACKET","SENSOR_IN","NAV_PACKET","NAV_IN","CRC_FRAME","OPTICAL_TX","OPTICAL_RX","FRAME_IN","CRC_OK","SEQUENCE_IN","SEQUENCE_OUT","ORDER_VALID","CRC_STATUS","CRC_VALID","GAP_REQ","RETRANSMIT","RETRY_OUT","RETRY_IN","GOOD_PACKET","BUFFER_IN","NAV_DATA","NAV_VALID","OPTICAL_VALID","LINK_STATUS","TELEMETRY_PERMIT","TELEMETRY_AUTH","PACKET_AGE","DROP_FRAME","REPLAY_HOLD","SENSOR_FRESH","SCADA_INTEGRITY","INJECT_CMD","FAULT_SEED","BANK_A_UPSET","BANK_B_UPSET","CORE_A_FAULT","CORE_B_FAULT","CORE_C_FAULT","CORREL_EVENT","CORREL_PATTERN","PROPAGATION_RISK","ECC_DIAG_ALARM","ECC_SYNDROME_STATUS_OUT","CRITICAL_ECC_SYNDROME_IN","ECC_HEALTH","ECC_DISPLAY","TRACE_DIAG","COMPUTE_INHIBIT","PACKET_FAULT","SCADA_FAULTS","WATCHDOG_EXPIRED","WATCHDOG_TRUST","SAFE_HOLD","RECOVERY_APPROVAL","RESTORE_SUM","SUM_COMPARE","QUARANTINE_STATUS","WORD_FRESHNESS","ECC_FRESHNESS","RECERT_REQUEST","RECOVERY_OK","VOTE_CONFIDENCE","REF_QUAL","PACKET_QUAL","FAULT_CONSOLE","MANUAL_RESET","PROTECTED_CONTROL","SUPPLY_STATUS","ESTOP_NC","SAFE_STOP_IN","HOLD_FEEDBACK","INJECT_PERMIT","SCADA_PLC","SCADA_ECC","SCADA_QUARANTINE","SCADA_SCRUB","SCADA_TMR","SCADA_TIMING","SCADA_PACKET","MISSION_HEALTH","OBJECTIVE_CERT"};
                foreach(string id in wireOrder[2..])
                    if(id is not ("SAFE_STOP_IN" or "COMMAND_ENABLE") && !pins.Exists(p=>p.name==id)) Pin(carrier,id,0,0,cyan,pins);
                var stopLoad=Part<SparkResistor>(bench,"Safety PLC stop input",0,0,steel);
                var safeInput=Pin(stopLoad,"SAFE_STOP_IN",0,0,gold,pins);
                var returnRail=pins.Find(pin=>pin.name=="PLC_GND");
                Set(stopLoad,("terminalA",safeInput),("terminalB",returnRail),("resistance",10000f));
                var emergency=Child(bench,"Emergency stop NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="ESTOP_NC")),("state",SparkSwitchState.Closed));
                var enableLoad=Part<SparkResistor>(bench,"Mission command permit input",0,0,steel);
                var enableNode=Pin(enableLoad,"COMMAND_ENABLE",0,0,gold,pins);
                Set(enableLoad,("terminalA",enableNode),("terminalB",returnRail),("resistance",1500f));
                var drive=Child(bench,"Mission command safety relay",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(drive,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="COMMAND_PERMIT")),("state",SparkSwitchState.Open));
                var contacts=new[]{drive};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel81Process>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel81VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 81 prefab.");
                SparkLevel81DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 81 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_81_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel81Process c,Font font)
        {
            var obj=new GameObject("Level 81 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,271),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the mission reliability workstation.",font,new Vector2(1080,28),new Vector2(0,271));
            c.meterText=UIText(panel,"Measurements","MISSION RELIABILITY CONTROL",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-476f,139,c.Power);
            c.initButton=Control(panel,font,"INIT ECC BANKS",-357f,139,c.Init);
            c.singleButton=Control(panel,font,"INJECT SINGLE BIT",-238f,139,c.Single);
            c.scrubButton=Control(panel,font,"SCRUB / VERIFY",-119f,139,c.Scrub);
            c.tmrButton=Control(panel,font,"TEST THREE LANES",0f,139,c.Tmr);
            c.laneButton=Control(panel,font,"INJECT LANE FAULT",119f,139,c.Lane);
            c.watchButton=Control(panel,font,"PROVE WATCHDOG",238f,139,c.Watch);
            c.opticalButton=Control(panel,font,"CHECK TELEMETRY",357f,139,c.Optical);
            c.missionButton=Control(panel,font,"START MISSION",476f,139,c.Mission);
            c.doubleButton=Control(panel,font,"INJECT DOUBLE BIT",-476f,100,c.Double);
            c.commonButton=Control(panel,font,"COMMON MODE",-357f,100,c.Common);
            c.packetsButton=Control(panel,font,"PACKET FAULTS",-238f,100,c.Packets);
            c.diagnoseButton=Control(panel,font,"DIAGNOSE",-119f,100,c.Diagnose);
            c.restoreButton=Control(panel,font,"RESTORE / RECHECK",0f,100,c.Restore);
            c.revalidateButton=Control(panel,font,"VERIFY RECOVERY",119f,100,c.Revalidate);
            c.resetButton=Control(panel,font,"RESET SAFETY",238f,100,c.Reset);
            c.armButton=Control(panel,font,"ARM W094 TEST",357f,100,c.Arm);
            c.eStopButton=Control(panel,font,"E-STOP",476f,100,c.EStop);
            c.certifyButton=Control(panel,font,"CERTIFY",-476f,61,c.Certify);
            var plot=Rect(canvas.transform,"SCADA trends",new Vector2(330,181),new Vector2(-230,800));
            c.plot=plot.gameObject.AddComponent<SparkLevel81Trend>();
            c.plot.raycastTarget=false;
            var legend=UIText(plot,"Trend legend","ECC  /  WATCHDOG  /  COMMAND  /  DATA",font,new Vector2(355,25),new Vector2(0,181));
            legend.fontSize=16;
            legend.color=new Color(.63f,.89f,.99f);
            for(int channel=0;channel<4;channel++)
            {
                string id=new[]{"ECC","VOTE","WD","CRC"}[channel];
                var axis=UIText(plot,"Monitor "+id,id,font,new Vector2(38,16),
                    new Vector2(-112+channel*79,144));
                axis.fontSize=13;
            }

        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(114,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.71f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(114,37),Vector2.zero).fontSize=13;
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_80_Layout/Level_80_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Photonic Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_81" || targets.arraySize!=SparkLevel81Process.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel81Process.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_81"),("displayName","Outlast the Bitstorm"),
                ("description","Protect memory, validate votes, complete three mission phases and recover trusted ECC supervision."),
                ("completionMode",0),("requiredTargetCount",Objectives.Length),("requireClosedReturn",false),
                ("rejectShortCircuit",true),("rejectTargetShort",true),("requireMinimumVoltage",false),
                ("autoAdvanceOnCompletion",false));
            var so=new SerializedObject(level);
            var sources=so.FindProperty("powerSources"); sources.arraySize=1;
            var source=sources.GetArrayElementAtIndex(0);
            source.FindPropertyRelative("sourceName").stringValue="CONTROL_POWER";
            source.FindPropertyRelative("positiveTerminalId").stringValue="SUPPLY_POS";
            source.FindPropertyRelative("negativeTerminalId").stringValue="SUPPLY_RET";
            source.FindPropertyRelative("nominalVoltage").floatValue=5f;
            var targets=so.FindProperty("targets"); targets.arraySize=Objectives.Length;
            for (int i=0;i<Objectives.Length;i++)
            {
                var target=targets.GetArrayElementAtIndex(i);
                target.FindPropertyRelative("targetId").stringValue=$"level81_mission_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel81Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_81.asset");
            return level;
        }

        private static void WriteBindings(SparkLevelSceneBindings bindings,List<SparkTerminal> pins,SparkPowerSupply source, SparkSwitch[] contacts, SparkSwitch emergency)
        {
            var so=new SerializedObject(bindings);
            var terminals=so.FindProperty("terminals"); terminals.arraySize=pins.Count;
            for(int i=0;i<pins.Count;i++)
            {
                var row=terminals.GetArrayElementAtIndex(i);
                row.FindPropertyRelative("id").stringValue=pins[i].name;
                row.FindPropertyRelative("terminal").objectReferenceValue=pins[i];
            }
            var fixedComponents=new List<(string id,SparkElectricalComponent component)> {
                ("CONTROL_POWER",source),("ESTOP_INTERLOCK",emergency)
            };
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L81_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel81Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel81Process.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }


        private static T Part<T>(Transform parent,string name,float x,float z,Material material) where T:SparkComponent
        {
            var obj=Child(parent,name,new Vector3(x,.071f,z));
            Solid(obj,"Body",PrimitiveType.Cube,Vector3.zero,new Vector3(.22f,.09f,.13f),material);
            var component=obj.gameObject.AddComponent<T>();
            Set(component,("objectId",name.Replace(' ','_')),("displayName",name));
            return component;
        }

        private static SparkTerminal Pin(SparkComponent owner,string id,float x,float z,Material material,
            List<SparkTerminal> pins,SparkTerminalPolarity polarity=SparkTerminalPolarity.None)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.name=id; obj.transform.SetParent(owner.transform,false);
            obj.transform.localPosition=new Vector3(x,.071f,z); obj.transform.localScale=new Vector3(.075f,.018f,.075f);
            obj.GetComponent<Renderer>().sharedMaterial=material;
            int layer=LayerMask.NameToLayer("Pin"); if(layer>=0) obj.layer=layer;
            var terminal=obj.AddComponent<SparkTerminal>();
            var kind=polarity==SparkTerminalPolarity.Positive ? SparkTerminalKind.Power :
                polarity==SparkTerminalPolarity.Negative ? SparkTerminalKind.Ground : SparkTerminalKind.Generic;
            Set(terminal,("owner",owner),("kind",kind),("polarity",polarity),("maxConnections",polarity==SparkTerminalPolarity.None ? (id is "DC_NEG" or "DC_POS" or "BUS_A" or "BUS_B" or "BUS_C" or "SENSE_OUT" ? 6 : 1) : 5));
            pins.Add(terminal);
            return terminal;
        }


        private static RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 offset)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent,false); rect.anchorMin=rect.anchorMax=new Vector2(.5f,0); rect.pivot=new Vector2(.5f,0);
            rect.sizeDelta=size; rect.anchoredPosition=offset;
            return rect;
        }

        private static Text UIText(Transform parent,string name,string value,Font font,Vector2 size,Vector2 offset)
        {
            var text=Rect(parent,name,size,offset).gameObject.AddComponent<Text>();
            text.text=value; text.font=font; text.fontSize=18; text.alignment=TextAnchor.MiddleCenter;
            text.color=Color.white; text.raycastTarget=false;
            return text;
        }

        private static void Label(Transform parent,string value,Font font,float z=-.095f)
        {
            var obj=Child(parent,"Label "+value,new Vector3(0,.075f,z));
            obj.localRotation=Quaternion.Euler(90,0,0);
            var text=obj.gameObject.AddComponent<TextMesh>();
            text.text=value; text.font=font; text.anchor=TextAnchor.MiddleCenter;
            text.characterSize=.022f; text.fontSize=76; text.color=Color.white;
            obj.GetComponent<Renderer>().sharedMaterial=font.material;
        }

        private static Transform Child(Transform parent,string name,Vector3 position)
        {
            var child=new GameObject(name).transform;
            child.SetParent(parent,false); child.localPosition=position;
            return child;
        }

        private static GameObject Solid(Transform parent,string name,PrimitiveType shape,Vector3 position,Vector3 size,Material material)
        {
            var obj=GameObject.CreatePrimitive(shape); obj.name=name;
            obj.transform.SetParent(parent,false); obj.transform.localPosition=position; obj.transform.localScale=size;
            obj.GetComponent<Renderer>().sharedMaterial=material;
            Object.DestroyImmediate(obj.GetComponent<Collider>());
            return obj;
        }

        private static Material MaterialAsset(Shader shader,string name,Color color)
        {
            string path=$"{Folder}/L81 {name}.mat";
            var existing=AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing!=null) return existing;
            var material=new Material(shader) {name=name,color=color};
            AssetDatabase.CreateAsset(material,path);
            return material;
        }


        private static void CopyLocalTransform(Transform target,Transform source)
        {
            target.localPosition=source.localPosition;
            target.localRotation=source.localRotation;
            target.localScale=source.localScale;
        }

        private static void Set(Object target,params (string Field,object Value)[] settings)
        {
            var so=new SerializedObject(target);
            foreach (var entry in settings)
            {
                var p=so.FindProperty(entry.Field);
                if (p==null) throw new InvalidOperationException($"{target.GetType().Name} has no serialized field {entry.Field}.");
                switch (entry.Value)
                {
                    case Object obj: p.objectReferenceValue=obj; break;
                    case string text: p.stringValue=text; break;
                    case bool flag: p.boolValue=flag; break;
                    case float number: p.floatValue=number; break;
                    case Enum choice: p.enumValueIndex=Convert.ToInt32(choice); break;
                    case int number:
                        if (p.propertyType==SerializedPropertyType.Enum) p.enumValueIndex=number;
                        else p.intValue=number;
                        break;
                    default: throw new InvalidOperationException($"Unsupported field value for {entry.Field}.");
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif



