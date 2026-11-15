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
    public static class SparkLevel67Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_67_Layout";
        private const string PrefabPath = Folder + "/Level_67_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the smart process plant","Commission the liquid circuits","Tune cascade PID control","Produce the perfect mixture","Survive process disturbances","Restore lost flow feedback"
        };

        [MenuItem("Tools/Project Spark/Level 67/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 67 prefab already exists at " + PrefabPath);
                SparkLevel67DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_67.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_67.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.67f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.67f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.67f,.16f));

            var root = new GameObject("Level_67_Layout");
            try
            {
                var bench = Child(root.transform,"Process Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Temporary board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.67f),board);
                var pins = new List<SparkTerminal>();
                SparkPowerSupply Supply(string name, float voltage, float current)
                {
                    var part=Part<SparkPowerSupply>(bench,name,0,0,steel);
                    Set(part,("outputVoltage",voltage),("currentLimit",current),("state",SparkPowerSupplyState.Active));
                    return part;
                }
                var source=Supply("CONTROL_POWER",5f,.67f);
                var positive=Pin(source,"SUPPLY_POS",0,0,red,pins,SparkTerminalPolarity.Positive);
                var ground=Pin(source,"SUPPLY_RET",0,0,blue,pins,SparkTerminalPolarity.Negative);
                Set(source,("positiveTerminal",positive),("negativeTerminal",ground));
                foreach(var (plus,ret) in new[]{("PLC_5V","PLC_GND"),("IO_5V","IO_GND"),("SAFE_5V","SAFE_GND"),("FLOW_A_5V","FLOW_A_GND"),("FLOW_B_5V","FLOW_B_GND"),("FLOW_P_5V","FLOW_P_GND"),("PID_5V","PID_GND"),("DRIVE_5V","DRIVE_GND"),("QUALITY_5V","QUALITY_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"Process I/O endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                string[] wireOrder={"SUPPLY_POS","PLC_5V","SUPPLY_RET","PLC_GND","IO_5V","IO_GND","SAFE_5V","SAFE_GND","FLOW_A_5V","FLOW_A_GND","FLOW_B_5V","FLOW_B_GND","FLOW_P_5V","FLOW_P_GND","PID_5V","PID_GND","DRIVE_5V","DRIVE_GND","QUALITY_5V","QUALITY_GND","PLC_RUN_A","DRIVE_A_EN","PLC_RUN_B","DRIVE_B_EN","PLC_RUN_P","DRIVE_P_EN","PID_REF_A","DRIVE_A_CMD","PID_REF_B","DRIVE_B_CMD","PID_REF_P","DRIVE_P_CMD","DRIVE_A_TACH","IO_RPM_A","DRIVE_B_TACH","IO_RPM_B","DRIVE_P_TACH","IO_RPM_P","FLOW_A_OUT","RATIO_A_IN","FLOW_P_OUT","PID_FLOW_IN","FLOW_OK","PLC_FLOW_OK","LEVEL_A_OUT","IO_LEVEL_A","LEVEL_B_OUT","IO_LEVEL_B","LEVEL_P_OUT","LEVEL_PID_IN","SUCTION_KPA","IO_SUCTION","DISCHARGE_KPA","IO_DISCHARGE","PLC_VALVE_A","VALVE_A_CMD","VALVE_A_POS","IO_POS_A","PLC_VALVE_B","VALVE_B_CMD","VALVE_B_POS","IO_POS_B","PLC_OUTLET","OUTLET_CMD","OUTLET_POS","IO_OUTLET","SAFE_ISOLATE","XV_CMD","XV_POS","SAFE_XV","PLC_MIX","MIX_EN","MIX_RPM","IO_MIX","QA_BLEND","RATIO_QUALITY","QA_PASS","SAFE_RELEASE","LEVEL_FLOW_SP","PID_SP","RATIO_CMD","PLC_RATIO","PLC_READY","SCADA_READY","ESTOP_NC","SAFE_STOP_IN","CAVITATION","PROTECT_CAV","PROTECT_TRIP","SAFE_TRIP","HIGH_LEVEL","SAFE_HIGH","LOW_LEVEL","SAFE_LOW","PLC_RESET","SAFE_RESET","DEMAND_TEST","IO_DEMAND","SCADA_RPM_A","SCADA_RPM_B","SCADA_RPM_P","SCADA_FLOW_A","SCADA_FLOW_P","SCADA_VOL_A","SCADA_VOL_B","SCADA_VOL_P","SCADA_SUCTION","SCADA_HEAD","SCADA_BLEND","SCADA_TRIP","PID_PERMIT","IO_READY","TRANSFER_INHIBIT","SCADA_FLOW_SP","FLOW_B_OUT","RATIO_B_IN"};
                foreach(string id in wireOrder[20..])
                    if(id!="SAFE_STOP_IN") Pin(carrier,id,0,0,cyan,pins);
                var stopLoad=Part<SparkResistor>(bench,"Safety PLC stop input",0,0,steel);
                var safeInput=Pin(stopLoad,"SAFE_STOP_IN",0,0,gold,pins);
                var returnRail=pins.Find(pin=>pin.name=="SAFE_GND");
                Set(stopLoad,("terminalA",safeInput),("terminalB",returnRail),("resistance",10000f));
                var emergency=Child(bench,"Emergency stop NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="ESTOP_NC")),("state",SparkSwitchState.Closed));
                var enableLoad=Child(bench,"Drive enable input",Vector3.zero).gameObject.AddComponent<SparkResistor>();
                var enableNode=Child(enableLoad.transform,"Internal drive permit",Vector3.zero).gameObject.AddComponent<SparkTerminal>();
                Set(enableNode,("owner",enableLoad));
                Set(enableLoad,("terminalA",enableNode),("terminalB",returnRail),("resistance",1500f));
                var drive=Child(bench,"Drive protection contact",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(drive,("inputTerminal",positive),("outputTerminal",enableNode),("state",SparkSwitchState.Open));
                var contacts=new[]{drive};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel67Process>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel67VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 67 prefab.");
                SparkLevel67DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 67 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_67_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel67Process c,Font font)
        {
            var obj=new GameObject("Level 67 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,267),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the process network.",font,new Vector2(1080,28),new Vector2(0,267));
            c.meterText=UIText(panel,"Measurements","PROCESS CONTROL",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-453f,139,c.Power);
            c.calibrateButton=Control(panel,font,"CALIBRATE",-302f,139,c.Calibrate);
            c.primeAButton=Control(panel,font,"PRIME A",-151f,139,c.PrimeA);
            c.primeBButton=Control(panel,font,"PRIME B",0f,139,c.PrimeB);
            c.primePButton=Control(panel,font,"PRIME TRANSFER",151f,139,c.PrimeP);
            c.startAButton=Control(panel,font,"START A",302f,139,c.StartA);
            c.startBButton=Control(panel,font,"START B",453f,139,c.StartB);
            c.startPButton=Control(panel,font,"START TRANSFER",-453f,100,c.StartP);
            c.mixerButton=Control(panel,font,"MIXER",-302f,100,c.Mixer);
            c.cascadeButton=Control(panel,font,"CASCADE",-151f,100,c.Cascade);
            c.tuneLevelButton=Control(panel,font,"LEVEL GAINS",0f,100,c.TuneLevel);
            c.tuneFlowButton=Control(panel,font,"FLOW GAINS",151f,100,c.TuneFlow);
            c.setLevelButton=Control(panel,font,"LEVEL SP",302f,100,c.SetLevel);
            c.setFlowButton=Control(panel,font,"FLOW SP",453f,100,c.SetFlow);
            c.setRecipeButton=Control(panel,font,"RECIPE",-453f,61,c.SetRecipe);
            c.ratioButton=Control(panel,font,"RATIO CONTROL",-302f,61,c.Ratio);
            c.produceButton=Control(panel,font,"PRODUCTION",-151f,61,c.Produce);
            c.outletButton=Control(panel,font,"PRODUCT OUTLET",0f,61,c.Outlet);
            c.surgeButton=Control(panel,font,"DEMAND SURGE",151f,61,c.Surge);
            c.pressureButton=Control(panel,font,"PRESSURE LOSS",302f,61,c.Pressure);
            c.failBButton=Control(panel,font,"PUMP B FAILURE",453f,61,c.FailB);
            c.armFeedbackButton=Control(panel,font,"W74 TEST",-453f,22,c.ArmFeedback);
            c.recoverButton=Control(panel,font,"RECAL FLOW B",-302f,22,c.Recover);
            c.resetButton=Control(panel,font,"RESET FAULT",-151f,22,c.Reset);
            c.certifyButton=Control(panel,font,"CERTIFY",0f,22,c.Certify);
            c.stopButton=Control(panel,font,"STOP",151f,22,c.Stop);
            c.eStopButton=Control(panel,font,"E-STOP",302f,22,c.EStop);
            var plot=Rect(canvas.transform,"SCADA trends",new Vector2(330,181),new Vector2(518,7));
            c.plot=plot.gameObject.AddComponent<SparkLevel67Trend>();
            c.plot.raycastTarget=false;
            var legend=new (string caption,Color color)[]{
                ("A FLOW",new Color(.22f,.85f,.98f)),("B FLOW",new Color(1,.73f,.24f)),
                ("PRODUCT",new Color(.47f,.95f,.60f)),("LEVEL",new Color(.76f,.61f,.94f))
            };
            for(int i=0;i<legend.Length;i++)
            {
                var entry=UIText(plot,"Trend legend",legend[i].caption,font,new Vector2(76,22),new Vector2((i-1.5f)*78,181));
                entry.color=legend[i].color;
                entry.fontSize=13;
            }

        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(146,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.67f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(146,37),Vector2.zero).fontSize=14;
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_66_Layout/Level_66_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Maintenance Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_67" || targets.arraySize!=SparkLevel67Process.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel67Process.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_67"),("displayName","Master the Flow"),
                ("description","Commission a hydraulic process, tune cascade control, achieve 70/30 and recover lost Supply B feedback."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level67_flow_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel67Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_67.asset");
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
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L67_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel67Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel67Process.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }


        private static T Part<T>(Transform parent,string name,float x,float z,Material material) where T:SparkComponent
        {
            var obj=Child(parent,name,new Vector3(x,.067f,z));
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
            obj.transform.localPosition=new Vector3(x,.067f,z); obj.transform.localScale=new Vector3(.075f,.018f,.075f);
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
            text.characterSize=.022f; text.fontSize=67; text.color=Color.white;
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
            string path=$"{Folder}/L67 {name}.mat";
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


