#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel72Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_72_Layout";
        private const string PrefabPath = Folder + "/Level_72_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the atomic deposition network","Commission vacuum and wafer temperature","Master the four-stage ALD cycle","Build a precise conformal film","Survive the deposition disturbances","Restore critical purge feedback"
        };

        [MenuItem("Tools/Project Spark/Level 72/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 72 prefab already exists at " + PrefabPath);
                SparkLevel72DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_72.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_72.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.71f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.71f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.71f,.16f));

            var root = new GameObject("Level_72_Layout");
            try
            {
                var bench = Child(root.transform,"Deposition Workbench",Vector3.zero);
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
                foreach(var (plus,ret) in new[]{("PLC_5V","PLC_GND"),("IO_5V","IO_GND"),("VAC_5V","VAC_GND"),("HEAT_5V","HEAT_GND"),("PURGE_5V","PURGE_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"Process I/O endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                string[] wireOrder={"SUPPLY_POS","PLC_5V","SUPPLY_RET","PLC_GND","IO_5V","IO_GND","VAC_5V","VAC_GND","HEAT_5V","HEAT_GND","PURGE_5V","PURGE_GND","PLC_BACKING","BACKING_RUN","BACKING_SPEED","IO_BACK_RPM","PLC_TURBO","TURBO_RUN","TURBO_SPEED","IO_TURBO_RPM","PLC_ISOLATION","ISOLATION_CMD","ISOLATION_POS","IO_ISOLATION","PID_THROTTLE","THROTTLE_CMD","THROTTLE_POS","IO_THROTTLE","CHAMBER_PA","PID_PRESSURE","FORELINE_PA","IO_FORELINE","LOADLOCK_PA","PLC_LOCK_PA","ROUGH_PA","IO_ROUGH","HIGH_PA","IO_HIGH","RESIDUAL_PA","PLC_RGA","DOSE_PERMIT","DOSE_ENABLE","PLC_A_ENABLE","SOURCE_A_EN","PLC_B_ENABLE","SOURCE_B_EN","SEQ_A_CMD","VALVE_A_CMD","SEQ_B_CMD","VALVE_B_CMD","VALVE_A_POS","IO_A_POSITION","VALVE_B_POS","IO_B_POSITION","MFC_A_SET","FLOW_A_CMD","MFC_B_SET","FLOW_B_CMD","FLOW_A_OUT","IO_FLOW_A","FLOW_B_OUT","IO_FLOW_B","STOCK_A_OUT","IO_STOCK_A","STOCK_B_OUT","IO_STOCK_B","MIX_A_OUT","MIX_A_MON","MIX_B_OUT","MIX_B_MON","A_RESIDUAL","SEQ_A_RESIDUAL","B_RESIDUAL","SEQ_B_RESIDUAL","OVERLAP_OUT","PLC_OVERLAP","PLC_PURGE","SOURCE_P_EN","SEQ_PURGE_CMD","PURGE_VALVE_CMD","PURGE_VALVE_POS","IO_PURGE_POSITION","P_FLOW_SET","PURGE_REG_CMD","PURGE_REG_POS","IO_PURGE_REG","PURGE_READY","PLC_PURGE_READY","P_STOCK_OUT","IO_PURGE_STOCK","P_FLOW_OK","PLC_FLOW_OK","P_SENSOR_OK","IO_P_VALID","P_ZERO_REQ","P_ZERO_CMD","A_CLEAR_OUT","SEQ_A_CLEAR","B_CLEAR_OUT","SEQ_B_CLEAR","PURGE_TRIP","IO_PURGE_ALARM","RESIDUAL_OK","PLC_CLEAR","PLC_GATE","GATE_CMD","PLC_ROBOT","ROBOT_CMD","GATE_MON","ROBOT_MON","TRANSFER_OK","GATE_PRESS_OK","WAFER_LIFT","CHUCK_CLAMP","HEAT_PERMIT","CHUCK_CLOSED","ZONE_PID_EN","ZONE_TEMP_MON","METROLOGY_REQ","QCM_FEED","QCM_RESPONSE","OPTICAL_ENABLE","OPTICAL_READY","FILM_THICKNESS_MON","FILM_SCAN_EN","UNIFORMITY_MON","COVERAGE_MON","REACTION_MON","QUALITY_INTERLOCK","TRENCH_SCAN","ESTOP_NC","SAFE_STOP_IN","PLC_SAFETY","LID_LOCK","INTERLOCK_MON","SAFE_PURGE","LOCK_STATUS","HEATER_ALARM","STUCK_ALARM","RESET_READY","HMI_PRESS","HMI_REACTANT","HMI_QUALITY","HMI_SEQUENCE","PURGE_FLOW_TRANSMITTER_SIGNAL_OUT","ALD_SEQUENCER_PURGE_FEEDBACK_IN"};
                foreach(string id in wireOrder[2..])
                    if(id is not ("SAFE_STOP_IN" or "DOSE_ENABLE") && !pins.Exists(p=>p.name==id)) Pin(carrier,id,0,0,cyan,pins);
                var stopLoad=Part<SparkResistor>(bench,"Safety PLC stop input",0,0,steel);
                var safeInput=Pin(stopLoad,"SAFE_STOP_IN",0,0,gold,pins);
                var returnRail=pins.Find(pin=>pin.name=="PLC_GND");
                Set(stopLoad,("terminalA",safeInput),("terminalB",returnRail),("resistance",10000f));
                var emergency=Child(bench,"Emergency stop NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="ESTOP_NC")),("state",SparkSwitchState.Closed));
                var enableLoad=Part<SparkResistor>(bench,"Precursor enable input",0,0,steel);
                var enableNode=Pin(enableLoad,"DOSE_ENABLE",0,0,gold,pins);
                Set(enableLoad,("terminalA",enableNode),("terminalB",returnRail),("resistance",1500f));
                var drive=Child(bench,"Precursor safety contact",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(drive,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="DOSE_PERMIT")),("state",SparkSwitchState.Open));
                var contacts=new[]{drive};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel72Process>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel72VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 72 prefab.");
                SparkLevel72DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 72 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_72_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel72Process c,Font font)
        {
            var obj=new GameObject("Level 72 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,271),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the ALD workstation.",font,new Vector2(1080,28),new Vector2(0,271));
            c.meterText=UIText(panel,"Measurements","ATOMIC DEPOSITION CONTROL",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-458.5f,139,c.Power);
            c.calPressureButton=Control(panel,font,"CAL PRESSURE",-327.5f,139,c.CalPressure);
            c.calFlowButton=Control(panel,font,"CAL FLOW",-196.5f,139,c.CalFlow);
            c.calPurgeButton=Control(panel,font,"CAL PURGE",-65.5f,139,c.CalPurge);
            c.calHeatButton=Control(panel,font,"CAL HEAT",65.5f,139,c.CalHeat);
            c.isolationButton=Control(panel,font,"VAC ISOLATION",196.5f,139,c.Isolation);
            c.backingButton=Control(panel,font,"START BACKING",327.5f,139,c.Backing);
            c.turboButton=Control(panel,font,"START TURBO",458.5f,139,c.Turbo);
            c.loadButton=Control(panel,font,"LOAD WAFER",-458.5f,100,c.Load);
            c.clampButton=Control(panel,font,"CLAMP CHUCK",-327.5f,100,c.Clamp);
            c.heatButton=Control(panel,font,"WAFER HEAT",-196.5f,100,c.Heat);
            c.pressurePidButton=Control(panel,font,"PRESSURE PID",-65.5f,100,c.PressurePid);
            c.conditionButton=Control(panel,font,"CARRIER FLOW",65.5f,100,c.Condition);
            c.doseAButton=Control(panel,font,"A DOSE",196.5f,100,c.DoseA);
            c.purgeAButton=Control(panel,font,"PURGE A",327.5f,100,c.PurgeA);
            c.doseBButton=Control(panel,font,"B DOSE",458.5f,100,c.DoseB);
            c.purgeBButton=Control(panel,font,"PURGE B",-458.5f,61,c.PurgeB);
            c.autoButton=Control(panel,font,"AUTO BATCH",-327.5f,61,c.Auto);
            c.metrologyButton=Control(panel,font,"METROLOGY",-196.5f,61,c.Metrology);
            c.stopButton=Control(panel,font,"STOP / PUMP",-65.5f,61,c.Stop);
            c.restrictionButton=Control(panel,font,"PURGE RESTRICT",65.5f,61,c.Restriction);
            c.driftButton=Control(panel,font,"HEATER DRIFT",196.5f,61,c.Drift);
            c.stuckButton=Control(panel,font,"STUCK A",327.5f,61,c.Stuck);
            c.repairButton=Control(panel,font,"REPAIR FAULT",458.5f,61,c.Repair);
            c.eStopButton=Control(panel,font,"E-STOP",-458.5f,22,c.EStop);
            c.resetButton=Control(panel,font,"RESET SAFETY",-327.5f,22,c.Reset);
            c.armButton=Control(panel,font,"W94 TEST",-196.5f,22,c.Arm);
            c.recalButton=Control(panel,font,"RECAL PURGE",-65.5f,22,c.Recal);
            c.verifyButton=Control(panel,font,"VERIFY PURGE",65.5f,22,c.Verify);
            c.certifyButton=Control(panel,font,"CERTIFY",196.5f,22,c.Certify);
            c.windowButton=Control(panel,font,"DOSE WINDOW",327.5f,22,c.Window);
            var plot=Rect(canvas.transform,"SCADA trends",new Vector2(330,181),new Vector2(518,7));
            c.plot=plot.gameObject.AddComponent<SparkLevel72Trend>();
            c.plot.raycastTarget=false;
            var legend=UIText(plot,"Trend legend","LIVE TRENDS  ·  Pa  |  °C  |  FILM nm  |  PURGE",font,new Vector2(355,25),new Vector2(0,181));
            legend.fontSize=11;
            legend.color=new Color(.63f,.89f,.99f);

        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(126,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.71f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(126,37),Vector2.zero).fontSize=13;
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_71_Layout/Level_71_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Plasma Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_72" || targets.arraySize!=SparkLevel72Process.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel72Process.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_72"),("displayName","Layer by Layer"),
                ("description","Commission vacuum and wafer heat, demonstrate the four-stage deposition sequence, grow a conformal film and recover purge feedback."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level72_ald_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel72Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_72.asset");
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
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L72_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel72Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel72Process.Gates[i];
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
            text.characterSize=.022f; text.fontSize=72; text.color=Color.white;
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
            string path=$"{Folder}/L72 {name}.mat";
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
