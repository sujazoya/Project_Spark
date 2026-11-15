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
    public static class SparkLevel68Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_68_Layout";
        private const string PrefabPath = Folder + "/Level_68_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the thermal process network","Commission heating and cooling","Stabilize cascade temperature control","Recover waste heat","Survive thermal disturbances","Recover critical temperature feedback"
        };

        [MenuItem("Tools/Project Spark/Level 68/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 68 prefab already exists at " + PrefabPath);
                SparkLevel68DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_68.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_68.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.68f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.68f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.68f,.16f));

            var root = new GameObject("Level_68_Layout");
            try
            {
                var bench = Child(root.transform,"Thermal Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Temporary board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.68f),board);
                var pins = new List<SparkTerminal>();
                SparkPowerSupply Supply(string name, float voltage, float current)
                {
                    var part=Part<SparkPowerSupply>(bench,name,0,0,steel);
                    Set(part,("outputVoltage",voltage),("currentLimit",current),("state",SparkPowerSupplyState.Active));
                    return part;
                }
                var source=Supply("CONTROL_POWER",5f,.68f);
                var positive=Pin(source,"SUPPLY_POS",0,0,red,pins,SparkTerminalPolarity.Positive);
                var ground=Pin(source,"SUPPLY_RET",0,0,blue,pins,SparkTerminalPolarity.Negative);
                Set(source,("positiveTerminal",positive),("negativeTerminal",ground));
                foreach(var (plus,ret) in new[]{("PLC_5V","PLC_GND"),("IO_5V","IO_GND"),("SAFE_5V","SAFE_GND"),("RTD_5V","RTD_GND"),("PID_5V","PID_GND"),("ENERGY_5V","ENERGY_GND"),("DRIVE_5V","DRIVE_GND"),("HEATER_5V","HEATER_GND"),("METER_5V","METER_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"Process I/O endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                string[] wireOrder={"SUPPLY_POS","PLC_5V","SUPPLY_RET","PLC_GND","IO_5V","IO_GND","SAFE_5V","SAFE_GND","RTD_5V","RTD_GND","PID_5V","PID_GND","ENERGY_5V","ENERGY_GND","DRIVE_5V","DRIVE_GND","HEATER_5V","HEATER_GND","METER_5V","METER_GND","PLC_H_RUN","HOT_EN","PLC_H_REF","HOT_CMD","HOT_RPM","IO_H_RPM","HOT_STATUS","PLC_H_OK","HOT_TRIP","SAFE_H_TRIP","PLC_C_RUN","COLD_EN","PLC_C_REF","COLD_CMD","COLD_RPM","IO_C_RPM","COLD_TRIP","SAFE_C_TRIP","PLC_P_RUN","PROD_EN","PLC_P_REF","PROD_CMD","PROD_RPM","IO_P_RPM","PROD_STATUS","PLC_P_OK","SAFE_HEAT","HEAT_EN","PID_HEAT","HEAT_CMD","HEAT_KW","IO_HEAT_KW","HEAT_LIMIT","SAFE_LIMIT","HEAT_TRIP","SAFE_HEAT_TRIP","INLET_RTD","PID_INLET","HOT_RTD","PID_HOT","COLD_RTD","IO_COLD","BUFFER_RTD","ENERGY_BUF","RTD_OK","PLC_RTD_OK","PLC_H_VALVE","HV_CMD","HV_POS","IO_H_VALVE","PLC_C_VALVE","CV_CMD","CV_POS","IO_C_VALVE","PLC_MIX","MIX_CMD","MIX_POS","IO_MIX","PLC_BYPASS","BYP_CMD","BYP_POS","IO_BYPASS","SAFE_ISO","ISO_CMD","ISO_POS","SAFE_ISO_FB","FLOW_P","PID_P_FLOW","FLOW_H","SAFE_H_FLOW","FLOW_C","SAFE_C_FLOW","HX_DP","ENERGY_DP","HX_FLUX","ENERGY_FLUX","ESTOP_NC","SAFE_STOP_IN","OVERTEMP","SAFE_OT","PLC_RESET","SAFE_RESET","HMI_H_RPM","HMI_C_RPM","HMI_P_RPM","ENERGY_KW","HMI_INLET","HMI_HOT","HMI_COLD","HMI_BUF","ENERGY_P_FLOW","HMI_H_FLOW","HMI_C_FLOW","FOUL_DP","HX_RECOVERY","HMI_RECOVERY","PID_PERMIT","HMI_ISO","FOUL_TEST","INLET_TEST","OUTLET_RTD_OUT","PID_PRODUCT_TEMP_IN"};
                foreach(string id in wireOrder[20..])
                    if(id is not ("SAFE_STOP_IN" or "HEAT_EN")) Pin(carrier,id,0,0,cyan,pins);
                var stopLoad=Part<SparkResistor>(bench,"Safety PLC stop input",0,0,steel);
                var safeInput=Pin(stopLoad,"SAFE_STOP_IN",0,0,gold,pins);
                var returnRail=pins.Find(pin=>pin.name=="SAFE_GND");
                Set(stopLoad,("terminalA",safeInput),("terminalB",returnRail),("resistance",10000f));
                var emergency=Child(bench,"Emergency stop NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="ESTOP_NC")),("state",SparkSwitchState.Closed));
                var enableLoad=Part<SparkResistor>(bench,"Heater enable input",0,0,steel);
                var enableNode=Pin(enableLoad,"HEAT_EN",0,0,gold,pins);
                Set(enableLoad,("terminalA",enableNode),("terminalB",returnRail),("resistance",1500f));
                var drive=Child(bench,"Heater protection contact",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(drive,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="SAFE_HEAT")),("state",SparkSwitchState.Open));
                var contacts=new[]{drive};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel68Process>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel68VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 68 prefab.");
                SparkLevel68DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 68 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_68_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel68Process c,Font font)
        {
            var obj=new GameObject("Level 68 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,268),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the thermal process network.",font,new Vector2(1080,28),new Vector2(0,268));
            c.meterText=UIText(panel,"Measurements","THERMAL PROCESS",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-453f,139,c.Power);
            c.calTempButton=Control(panel,font,"CAL TEMP",-302f,139,c.CalTemp);
            c.calFlowButton=Control(panel,font,"CAL FLOW",-151f,139,c.CalFlow);
            c.calPressureButton=Control(panel,font,"CAL PRESSURE",0f,139,c.CalPressure);
            c.startHotButton=Control(panel,font,"START HOT",151f,139,c.StartHot);
            c.startColdButton=Control(panel,font,"START COOLING",302f,139,c.StartCold);
            c.startProductButton=Control(panel,font,"START PRODUCT",453f,139,c.StartProduct);
            c.heaterButton=Control(panel,font,"ENABLE HEATER",-453f,100,c.Heater);
            c.cascadeButton=Control(panel,font,"CASCADE",-302f,100,c.Cascade);
            c.tuneOuterButton=Control(panel,font,"OUTER GAINS",-151f,100,c.TuneOuter);
            c.tuneInnerButton=Control(panel,font,"INNER GAINS",0f,100,c.TuneInner);
            c.setTargetButton=Control(panel,font,"TEMP TARGET",151f,100,c.SetTarget);
            c.regenerateButton=Control(panel,font,"REGENERATION",302f,100,c.Regenerate);
            c.bypassButton=Control(panel,font,"BYPASS",453f,100,c.Bypass);
            c.transferButton=Control(panel,font,"PRODUCT TRANSFER",-453f,61,c.Transfer);
            c.coldTestButton=Control(panel,font,"COLD INLET",-302f,61,c.ColdTest);
            c.surgeButton=Control(panel,font,"FLOW SURGE",-151f,61,c.Surge);
            c.foulButton=Control(panel,font,"FOULING TEST",0f,61,c.Foul);
            c.diagnoseButton=Control(panel,font,"CONFIRM FOULING",151f,61,c.Diagnose);
            c.armFeedbackButton=Control(panel,font,"W78 TEST",302f,61,c.ArmFeedback);
            c.recoverButton=Control(panel,font,"RECAL OUTLET RTD",453f,61,c.Recover);
            c.resetButton=Control(panel,font,"RESET FAULT",-453f,22,c.Reset);
            c.certifyButton=Control(panel,font,"CERTIFY",-302f,22,c.Certify);
            c.stopButton=Control(panel,font,"STOP",-151f,22,c.Stop);
            c.eStopButton=Control(panel,font,"E-STOP",0f,22,c.EStop);
            var plot=Rect(canvas.transform,"SCADA trends",new Vector2(330,181),new Vector2(518,7));
            c.plot=plot.gameObject.AddComponent<SparkLevel68Trend>();
            c.plot.raycastTarget=false;
            var legend=new (string caption,Color color)[]{
                ("PRODUCT",new Color(.22f,.85f,.98f)),("HOT LOOP",new Color(1,.73f,.24f)),
                ("INLET",new Color(.47f,.95f,.60f)),("BUFFER",new Color(.76f,.61f,.94f))
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
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.68f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(146,37),Vector2.zero).fontSize=14;
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_67_Layout/Level_67_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Process Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_68" || targets.arraySize!=SparkLevel68Process.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel68Process.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_68"),("displayName","Tame the Heat"),
                ("description","Commission three loops, tune thermal cascade control, recover waste heat and restore critical RTD feedback."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level68_heat_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel68Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_68.asset");
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
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L68_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel68Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel68Process.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }


        private static T Part<T>(Transform parent,string name,float x,float z,Material material) where T:SparkComponent
        {
            var obj=Child(parent,name,new Vector3(x,.068f,z));
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
            obj.transform.localPosition=new Vector3(x,.068f,z); obj.transform.localScale=new Vector3(.075f,.018f,.075f);
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
            text.characterSize=.022f; text.fontSize=68; text.color=Color.white;
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
            string path=$"{Folder}/L68 {name}.mat";
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

