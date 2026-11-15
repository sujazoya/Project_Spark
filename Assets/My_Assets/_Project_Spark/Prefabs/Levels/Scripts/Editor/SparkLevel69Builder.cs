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
    public static class SparkLevel69Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_69_Layout";
        private const string PrefabPath = Folder + "/Level_69_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the refrigeration network","Commission the vapor-compression cycle","Stabilize both cooling zones","Defrost and restore cooling","Protect and optimize the cold chain","Restore superheat feedback"
        };

        [MenuItem("Tools/Project Spark/Level 69/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 69 prefab already exists at " + PrefabPath);
                SparkLevel69DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_69.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_69.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.69f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.69f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.69f,.16f));

            var root = new GameObject("Level_69_Layout");
            try
            {
                var bench = Child(root.transform,"Refrigeration Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Temporary board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.69f),board);
                var pins = new List<SparkTerminal>();
                SparkPowerSupply Supply(string name, float voltage, float current)
                {
                    var part=Part<SparkPowerSupply>(bench,name,0,0,steel);
                    Set(part,("outputVoltage",voltage),("currentLimit",current),("state",SparkPowerSupplyState.Active));
                    return part;
                }
                var source=Supply("CONTROL_POWER",5f,.69f);
                var positive=Pin(source,"SUPPLY_POS",0,0,red,pins,SparkTerminalPolarity.Positive);
                var ground=Pin(source,"SUPPLY_RET",0,0,blue,pins,SparkTerminalPolarity.Negative);
                Set(source,("positiveTerminal",positive),("negativeTerminal",ground));
                foreach(var (plus,ret) in new[]{("PLC_5V","PLC_GND"),("IO_5V","IO_GND"),("SAFE_5V","SAFE_GND"),("TEMP_5V","TEMP_GND"),("SH_5V","SH_GND"),("ZONE_5V","ZONE_GND"),("ENERGY_5V","ENERGY_GND"),("DRIVE_5V","DRIVE_GND"),("DEFROST_5V","DEFROST_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"Process I/O endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                string[] wireOrder={"SUPPLY_POS","PLC_5V","SUPPLY_RET","PLC_GND","IO_5V","IO_GND","SAFE_5V","SAFE_GND","TEMP_5V","TEMP_GND","SH_5V","SH_GND","ZONE_5V","ZONE_GND","ENERGY_5V","ENERGY_GND","DRIVE_5V","DRIVE_GND","DEFROST_5V","DEFROST_GND","COMP_PERMIT","COMP_EN","PLC_C_REF","DRIVE_REF","COMP_TACH","IO_RPM","DRIVE_READY","PLC_DRIVE_OK","DRIVE_TRIP","SAFE_C_TRIP","COMP_POWER","ENERGY_KW","PLC_COND","FAN_C_CMD","FAN_C_TACH","IO_C_FAN","FAN_C_OK","SAFE_C_FAN","LIQ_LINE_OK","PLC_SUBCOOL","SH_A_CMD","EEV_A_IN","SH_B_CMD","EEV_B_IN","EEV_A_POS","IO_EEV_A","EEV_B_POS","IO_EEV_B","EEV_A_OK","PLC_EEV_A","EEV_B_OK","PLC_EEV_B","ZONE_A_FAN","EVAP_FAN_A","ZONE_B_FAN","EVAP_FAN_B","EVAP_A_TACH","IO_EF_A","EVAP_B_TACH","IO_EF_B","SUCTION_P","SH_PRESS_IN","DISCHARGE_P","SAFE_HIGH_P","LOW_P_ALARM","SAFE_LOW_P","TRANS_P_OK","PLC_PRESS_OK","SUCTION_T","PLC_SUCTION","DISCHARGE_T","SAFE_DISCH_T","LIQ_LINE_T","ENERGY_LIQ","EVAP_B_TEMP_OUT","SH_B_TEMP_IN","CHAMBER_A_T","ZONE_A_TEMP","CHAMBER_B_T","ZONE_B_TEMP","FROST_B","DEF_FROST_IN","DEF_ENABLE","HEATER_IN","HEATER_FB","DEF_HEAT_FB","DRAIN_LEVEL","DEF_DRAIN_IN","DEF_FAN_LOCK","IO_FAN_LOCK","ESTOP_NC","SAFE_STOP_IN","MASS_FLOW","ENERGY_FLOW","ACC_LEVEL","SAFE_ACC","OIL_LEVEL","SAFE_OIL","DOOR_TEST","IO_DOOR","PLC_RESET","SAFE_RESET","SAFE_STATE","PLC_SAFE","REF_CHARGE","IO_CHARGE","HMI_RPM","HMI_KW","HMI_CFAN","HMI_LOW_P","HMI_HIGH_P","HMI_SUCTION","HMI_DISCH_T","HMI_LIQ_T","HMI_SH_B","HMI_A_TEMP","HMI_B_TEMP","HMI_FROST","HMI_DRAIN","HMI_FLOW","HMI_ACC","HMI_OIL","HMI_SAFE","HMI_EEV_A","HMI_EEV_B","FAN_TEST","EVAP_A_TEMP_OUT","SH_A_TEMP_IN"};
                foreach(string id in wireOrder[20..])
                    if(id is not ("SAFE_STOP_IN" or "COMP_EN")) Pin(carrier,id,0,0,cyan,pins);
                var stopLoad=Part<SparkResistor>(bench,"Safety PLC stop input",0,0,steel);
                var safeInput=Pin(stopLoad,"SAFE_STOP_IN",0,0,gold,pins);
                var returnRail=pins.Find(pin=>pin.name=="SAFE_GND");
                Set(stopLoad,("terminalA",safeInput),("terminalB",returnRail),("resistance",10000f));
                var emergency=Child(bench,"Emergency stop NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="ESTOP_NC")),("state",SparkSwitchState.Closed));
                var enableLoad=Part<SparkResistor>(bench,"Compressor enable input",0,0,steel);
                var enableNode=Pin(enableLoad,"COMP_EN",0,0,gold,pins);
                Set(enableLoad,("terminalA",enableNode),("terminalB",returnRail),("resistance",1500f));
                var drive=Child(bench,"Compressor protection contact",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(drive,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="COMP_PERMIT")),("state",SparkSwitchState.Open));
                var contacts=new[]{drive};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel69Process>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel69VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 69 prefab.");
                SparkLevel69DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 69 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_69_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel69Process c,Font font)
        {
            var obj=new GameObject("Level 69 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,269),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the refrigeration network.",font,new Vector2(1080,28),new Vector2(0,269));
            c.meterText=UIText(panel,"Measurements","COLD-CHAIN CONTROL",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-476.0f,139,c.Power);
            c.safetyCheckButton=Control(panel,font,"SAFETY CHECK",-340.0f,139,c.SafetyCheck);
            c.calPressureButton=Control(panel,font,"CAL PRESSURE",-204.0f,139,c.CalPressure);
            c.calTempButton=Control(panel,font,"CAL TEMP",-68.0f,139,c.CalTemp);
            c.calMassButton=Control(panel,font,"CAL MASS FLOW",68.0f,139,c.CalMass);
            c.condenserButton=Control(panel,font,"CONDENSER FAN",204.0f,139,c.Condenser);
            c.evaporatorButton=Control(panel,font,"EVAP FANS",340.0f,139,c.Evaporator);
            c.enableAButton=Control(panel,font,"ENABLE EEV A",476.0f,139,c.EnableA);
            c.enableBButton=Control(panel,font,"ENABLE EEV B",-476.0f,100,c.EnableB);
            c.startButton=Control(panel,font,"START COMPRESSOR",-340.0f,100,c.StartCompressor);
            c.setRpmButton=Control(panel,font,"RPM LIMIT",-204.0f,100,c.SetRpm);
            c.calAButton=Control(panel,font,"CAL SUPERHEAT A",-68.0f,100,c.CalA);
            c.calBButton=Control(panel,font,"CAL SUPERHEAT B",68.0f,100,c.CalB);
            c.autoZonesButton=Control(panel,font,"DUAL-ZONE AUTO",204.0f,100,c.AutoZones);
            c.tuneShButton=Control(panel,font,"SUPERHEAT GAINS",340.0f,100,c.TuneSh);
            c.tuneZonesButton=Control(panel,font,"ZONE GAINS",476.0f,100,c.TuneZones);
            c.targetAButton=Control(panel,font,"SET TARGET A",-476.0f,61,c.TargetA);
            c.targetBButton=Control(panel,font,"SET TARGET B",-340.0f,61,c.TargetB);
            c.frostTestButton=Control(panel,font,"FROST TEST",-204.0f,61,c.FrostTest);
            c.defrostButton=Control(panel,font,"REQUEST DEFROST",-68.0f,61,c.Defrost);
            c.doorTestButton=Control(panel,font,"DOOR TEST",68.0f,61,c.DoorTest);
            c.fanFaultButton=Control(panel,font,"FAN FAULT",204.0f,61,c.FanFault);
            c.optimizeButton=Control(panel,font,"OPTIMIZE",340.0f,61,c.Optimize);
            c.armFeedbackButton=Control(panel,font,"W82 TEST",476.0f,61,c.ArmFeedback);
            c.recoverButton=Control(panel,font,"RECAL EVAP A",-476.0f,22,c.Recover);
            c.resetButton=Control(panel,font,"RESET",-340.0f,22,c.Reset);
            c.certifyButton=Control(panel,font,"CERTIFY",-204.0f,22,c.Certify);
            c.stopButton=Control(panel,font,"STOP",-68.0f,22,c.Stop);
            c.eStopButton=Control(panel,font,"E-STOP",68.0f,22,c.EStop);
            var plot=Rect(canvas.transform,"SCADA trends",new Vector2(330,181),new Vector2(518,7));
            c.plot=plot.gameObject.AddComponent<SparkLevel69Trend>();
            c.plot.raycastTarget=false;
            var legend=UIText(plot,"Trend legend","LIVE PRESSURE–ENTHALPY  ·  1 SUCTION   2 DISCHARGE   3 LIQUID   4 EXPANSION",font,new Vector2(355,25),new Vector2(0,181));
            legend.fontSize=11;
            legend.color=new Color(.52f,.88f,.99f);

        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(130,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.69f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(130,37),Vector2.zero).fontSize=13;
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_68_Layout/Level_68_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Thermal Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_69" || targets.arraySize!=SparkLevel69Process.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel69Process.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_69"),("displayName","Keep It Cold"),
                ("description","Commission a refrigeration cycle, stabilize both zones, defrost and recover lost superheat feedback."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level69_cold_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel69Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_69.asset");
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
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L69_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel69Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel69Process.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }


        private static T Part<T>(Transform parent,string name,float x,float z,Material material) where T:SparkComponent
        {
            var obj=Child(parent,name,new Vector3(x,.069f,z));
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
            obj.transform.localPosition=new Vector3(x,.069f,z); obj.transform.localScale=new Vector3(.075f,.018f,.075f);
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
            text.characterSize=.022f; text.fontSize=69; text.color=Color.white;
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
            string path=$"{Folder}/L69 {name}.mat";
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
