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
    public static class SparkLevel71Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_71_Layout";
        private const string PrefabPath = Folder + "/Level_71_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the 90-wire plasma control network","Commission vacuum and clamp a wafer","Stabilize pressure and RF plasma","Detect optical endpoint and pass metrology","Recover vacuum, RF and cooling disturbances","Restore optical feedback and certify a fresh wafer"
        };

        [MenuItem("Tools/Project Spark/Level 71/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 71 prefab already exists at " + PrefabPath);
                SparkLevel71DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_71.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_71.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.71f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.71f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.71f,.16f));

            var root = new GameObject("Level_71_Layout");
            try
            {
                var bench = Child(root.transform,"Plasma Workbench",Vector3.zero);
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
                foreach(var (plus,ret) in new[]{("PLC_5V","PLC_GND"),("IO_5V","IO_GND"),("VAC_5V","VAC_GND"),("RF_5V","RF_GND"),("OES_5V","OES_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"Process I/O endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                string[] wireOrder={"SUPPLY_POS","PLC_5V","SUPPLY_RET","PLC_GND","IO_5V","IO_GND","VAC_5V","VAC_GND","RF_5V","RF_GND","OES_5V","OES_GND","PLC_BACKING","BACKING_START","BACKING_SPEED","PLC_BACK_RPM","PLC_TURBO","TURBO_START","TURBO_SPEED","IO_TURBO_RPM","PLC_ISOLATION","ISOLATION_CMD","ISOLATION_OPEN","IO_ISOLATION","PID_THROTTLE","THROTTLE_CMD","THROTTLE_POS","IO_THROTTLE","CHAMBER_PA","PID_PRESSURE","FORELINE_PA","PLC_FORELINE","LOCK_PA","PLC_LOCK_PRESS","ROUGH_PA","IO_ROUGH","HIGH_PA","IO_HIGH","RGA_SIGNAL","PLC_RGA","PLC_GAS_A","GAS_A_ENABLE","PLC_GAS_B","GAS_B_ENABLE","MFC_A_SET","GAS_A_CMD","MFC_B_SET","GAS_B_CMD","GAS_A_ACT","IO_FLOW_A","GAS_B_ACT","IO_FLOW_B","GAS_A_LEVEL","PLC_INV_A","GAS_B_LEVEL","PLC_INV_B","VALVE_A_STATE","IO_VALVE_A","VALVE_B_STATE","IO_VALVE_B","MIX_FLOW","PLC_MIX_FLOW","MIX_RATIO","PLC_MIX_RATIO","RF_PERMIT","RF_ENABLE","PLC_RF_SET","RF_POWER_CMD","RF_FORWARD","IO_FORWARD","RF_REFLECT","IO_REFLECT","RF_CURRENT","MATCH_I","RF_VOLTAGE","MATCH_V","MATCH_A","TUNER_A_CMD","MATCH_B","TUNER_B_CMD","TUNER_A_POS","IO_CAP_A","TUNER_B_POS","IO_CAP_B","RF_ARC","PLC_ARC_STOP","RF_READY","IO_RF_READY","RF_TEMP","IO_RF_TEMP","COUPLER_OK","PLC_RF_METER","RF_FREQ","IO_FREQ","BIAS_CMD","WAFER_BIAS","BIAS_RD","IO_BIAS","PLASMA_STATE","PLC_PLASMA","PLC_GATE","GATE_CMD","PLC_ROBOT","ROBOT_CMD","GATE_MON","ROBOT_MON","TRANSFER_OK","LOCK_EQUAL","LIFT_CMD","CLAMP_CMD","CHUCK_MON","PLC_GATE_MON","OES_GATE","OPT_APERTURE","OES_FLOW_A","OES_FLOW_B","OES_PRESS","OES_LOCK","UNIFORMITY_IN","RF_MATCH_A","RF_MATCH_B","MATCH_MON","METROLOGY_EN","METROLOGY_MON","ESTOP_NC","SAFE_STOP_IN","RF_ARC_TRIP","VAC_PERMIT","CHILL_PERMIT","CHUCK_BIAS_MON","WAFER_TEMP","CHILLER_CMD","ETCH_ENABLE","SCADA_FWD","SCADA_REF","SCADA_PLASMA","SCADA_PRESS","SCADA_GAS","OES_SIGNAL_OUT","ENDPOINT_OPTICAL_IN"};
                foreach(string id in wireOrder[2..])
                    if(id is not ("SAFE_STOP_IN" or "RF_ENABLE") && !pins.Exists(p=>p.name==id)) Pin(carrier,id,0,0,cyan,pins);
                var stopLoad=Part<SparkResistor>(bench,"Safety PLC stop input",0,0,steel);
                var safeInput=Pin(stopLoad,"SAFE_STOP_IN",0,0,gold,pins);
                var returnRail=pins.Find(pin=>pin.name=="PLC_GND");
                Set(stopLoad,("terminalA",safeInput),("terminalB",returnRail),("resistance",10000f));
                var emergency=Child(bench,"Emergency stop NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="ESTOP_NC")),("state",SparkSwitchState.Closed));
                var enableLoad=Part<SparkResistor>(bench,"RF enable input",0,0,steel);
                var enableNode=Pin(enableLoad,"RF_ENABLE",0,0,gold,pins);
                Set(enableLoad,("terminalA",enableNode),("terminalB",returnRail),("resistance",1500f));
                var drive=Child(bench,"RF safety contact",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(drive,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="RF_PERMIT")),("state",SparkSwitchState.Open));
                var contacts=new[]{drive};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel71Process>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel71VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 71 prefab.");
                SparkLevel71DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 71 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_71_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel71Process c,Font font)
        {
            var obj=new GameObject("Level 71 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,271),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the plasma workstation.",font,new Vector2(1080,28),new Vector2(0,271));
            c.meterText=UIText(panel,"Measurements","PLASMA PROCESS CONTROL",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-476f,139,c.Power);
            c.calPressureButton=Control(panel,font,"CAL PRESSURE",-357f,139,c.CalPressure);
            c.calGasButton=Control(panel,font,"CAL GAS",-238f,139,c.CalGas);
            c.calRfButton=Control(panel,font,"CAL RF",-119f,139,c.CalRf);
            c.calOpticalButton=Control(panel,font,"CAL OPTICAL",0f,139,c.CalOptical);
            c.backingButton=Control(panel,font,"START BACKING",119f,139,c.Backing);
            c.turboButton=Control(panel,font,"START TURBO",238f,139,c.Turbo);
            c.isolationButton=Control(panel,font,"VAC ISOLATION",357f,139,c.Isolation);
            c.throttleButton=Control(panel,font,"MANUAL THROTTLE",476f,139,c.Throttle);
            c.loadButton=Control(panel,font,"LOAD WAFER",-476f,100,c.Load);
            c.clampButton=Control(panel,font,"CLAMP CHUCK",-357f,100,c.Clamp);
            c.chillerButton=Control(panel,font,"CHILLER",-238f,100,c.Chiller);
            c.pressurePidButton=Control(panel,font,"PRESSURE PID",-119f,100,c.PressurePid);
            c.setAButton=Control(panel,font,"SET GAS A",0f,100,c.SetA);
            c.setBButton=Control(panel,font,"SET GAS B",119f,100,c.SetB);
            c.gasButton=Control(panel,font,"GAS FLOW",238f,100,c.Gas);
            c.setPowerButton=Control(panel,font,"RF POWER",357f,100,c.SetPower);
            c.matchButton=Control(panel,font,"AUTO MATCH",476f,100,c.Match);
            c.rfButton=Control(panel,font,"ENABLE RF",-476f,61,c.Rf);
            c.tuneAButton=Control(panel,font,"TUNE CAP A",-357f,61,c.TuneA);
            c.tuneBButton=Control(panel,font,"TUNE CAP B",-238f,61,c.TuneB);
            c.igniteButton=Control(panel,font,"IGNITE",-119f,61,c.Ignite);
            c.endpointButton=Control(panel,font,"ENDPOINT AUTO",0f,61,c.Endpoint);
            c.etchButton=Control(panel,font,"ETCH",119f,61,c.Etch);
            c.stopButton=Control(panel,font,"STOP PROCESS",238f,61,c.Stop);
            c.metrologyButton=Control(panel,font,"METROLOGY",357f,61,c.Metrology);
            c.leakButton=Control(panel,font,"VAC LEAK",476f,61,c.Leak);
            c.driftButton=Control(panel,font,"RF DRIFT",-476f,22,c.Drift);
            c.coolingButton=Control(panel,font,"COOLING TEST",-357f,22,c.Cooling);
            c.eStopButton=Control(panel,font,"E-STOP",-238f,22,c.EStop);
            c.resetButton=Control(panel,font,"RESET SAFETY",-119f,22,c.Reset);
            c.armButton=Control(panel,font,"W90 TEST",0f,22,c.Arm);
            c.recalButton=Control(panel,font,"RECAL OPTICAL",119f,22,c.Recal);
            c.certifyButton=Control(panel,font,"CERTIFY",238f,22,c.Certify);
            var plot=Rect(canvas.transform,"SCADA trends",new Vector2(330,181),new Vector2(518,7));
            c.plot=plot.gameObject.AddComponent<SparkLevel71Trend>();
            c.plot.raycastTarget=false;
            var legend=UIText(plot,"Trend legend","LIVE TRENDS  ·  Pa  |  °C  |  REF. kW  |  OPTICAL",font,new Vector2(355,25),new Vector2(0,181));
            legend.fontSize=11;
            legend.color=new Color(.63f,.89f,.99f);

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
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_70_Layout/Level_70_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Cleanroom Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_71" || targets.arraySize!=SparkLevel71Process.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel71Process.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_71"),("displayName","Etch the Future"),
                ("description","Commission a plasma reactor, etch and inspect wafers, and recover from equipment and optical-feedback faults."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level71_etch_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel71Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_71.asset");
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
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L71_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel71Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel71Process.Gates[i];
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
            text.characterSize=.022f; text.fontSize=71; text.color=Color.white;
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
            string path=$"{Folder}/L71 {name}.mat";
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
