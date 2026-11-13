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
    public static class SparkLevel54Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_54_Layout";
        private const string PrefabPath = Folder + "/Level_54_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the stabilization system", "Establish the 45 RPM reference", "Measure the critical speed",
            "Reduce the resonance peak by 50%", "Cross and coast down safely", "Recover the Y probe and recertify"
        };

        [MenuItem("Tools/Project Spark/Level 54/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 54 prefab already exists at " + PrefabPath);
                SparkLevel54DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_54.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_54.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.54f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.62f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.56f,.16f));

            var root = new GameObject("Level_54_Layout");
            try
            {
                var bench = Child(root.transform,"Critical Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Temporary board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.55f),board);
                var pins = new List<SparkTerminal>();
                SparkPowerSupply Supply(string name, float voltage, float current)
                {
                    var part=Part<SparkPowerSupply>(bench,name,0,0,steel);
                    Set(part,("outputVoltage",voltage),("currentLimit",current),("state",SparkPowerSupplyState.Active));
                    return part;
                }
                var source=Supply("CONTROL_POWER",5f,.5f);
                var positive=Pin(source,"SUPPLY_POS",0,0,red,pins,SparkTerminalPolarity.Positive);
                var ground=Pin(source,"SUPPLY_NEG",0,0,blue,pins,SparkTerminalPolarity.Negative);
                Set(source,("positiveTerminal",positive),("negativeTerminal",ground));
                var power=Part<SparkResistor>(bench,"Critical-speed controller power",0,0,steel);
                var cp=Pin(power,"CTRL_POS",0,0,red,pins);
                var cg=Pin(power,"CTRL_GND",0,0,blue,pins);
                Set(power,("terminalA",cp),("terminalB",cg),("resistance",2200f));

                SparkPowerSupply Signal(string id,Material color)
                {
                    var output=Supply(id,0f,.025f);
                    Set(output,("positiveTerminal",Pin(output,id,0,0,color,pins)),("negativeTerminal",ground));
                    return output;
                }

                SparkTerminal Sense(string id,SparkTerminal ret,Material color)
                {
                    var resistor=Part<SparkResistor>(bench,"Input "+id,0,0,steel);
                    var input=Pin(resistor,id,0,0,color,pins);
                    Set(resistor,("terminalA",input),("terminalB",ret),("resistance",100000f));
                    return input;
                }



                SparkTerminal Load(string name,string plusId,string returnId)
                {
                    var load=Part<SparkResistor>(bench,name,0,0,steel);
                    var plus=Pin(load,plusId,0,0,red,pins);
                    Set(load,("terminalA",plus),("terminalB",Pin(load,returnId,0,0,blue,pins)),("resistance",2200f));
                    return plus;
                }


                Load("X probe power","XS_POS","XS_GND");
                Load("Y probe power","YS_POS","YS_GND");
                var motorRail=Load("Motor driver power","MDRV_POS","MDRV_GND");
                var balanceRail=Load("Damper driver power","DDRV_POS","DDRV_GND");
                SparkSwitch Contact(string name,SparkTerminal input,SparkTerminal output,bool on)
                {
                    var contact=Child(bench,name,Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                    Set(contact,("inputTerminal",input),("outputTerminal",output),("state",on?SparkSwitchState.Closed:SparkSwitchState.Open));
                    return contact;
                }
                SparkResistor Branch(string name,string pinId,Material color,float resistance,out SparkTerminal socket,out SparkTerminal inner)
                {
                    var part=Part<SparkResistor>(bench,name,0,0,steel);
                    socket=Pin(part,pinId,0,0,color,pins);
                    inner=Child(part.transform,"Internal",Vector3.zero).gameObject.AddComponent<SparkTerminal>();
                    Set(inner,("owner",part));
                    Set(part,("terminalA",inner),("terminalB",socket),("resistance",resistance),("maximumPowerWatts",3f));
                    return part;
                }
                var driver=Branch("Rotor PWM equivalent","MDRV_MPOS",gold,21f,out _,out var driverIn);
                var drive=Contact("Motor enable",motorRail,driverIn,false);
                var ret=Branch("Motor return","MDRV_MNEG",blue,.025f,out _,out var motorReturn);
                Contact("Internal motor return",pins.Find(p=>p.name=="MDRV_GND"),motorReturn,true);
                var motor=Part<SparkMotor>(bench,"ROTOR_MOTOR",0,0,steel);
                Set(motor,("positiveTerminal",Pin(motor,"MOTOR_POS",0,0,gold,pins)),("negativeTerminal",Pin(motor,"MOTOR_NEG",0,0,blue,pins)),
                    ("windingResistance",22f),("startupVoltage",.35f),("maximumCurrent",.23f));

                var actuator=Part<SparkResistor>(bench,"Two-axis damping actuator",0,0,steel);
                Set(actuator,("terminalA",Pin(actuator,"DAMP_POS",0,0,gold,pins)),
                    ("terminalB",Pin(actuator,"DAMP_NEG",0,0,blue,pins)),("resistance",68f),("maximumPowerWatts",1f));
                Branch("Damper supply","DDRV_ACTPOS",gold,.025f,out _,out var actuatorIn);
                var damperDrive=Contact("Damper enable",balanceRail,actuatorIn,false);
                Branch("Damper return","DDRV_ACTNEG",blue,.025f,out _,out var actuatorReturn);
                Contact("Internal damper return",pins.Find(pin=>pin.name=="DDRV_GND"),actuatorReturn,true);
                Branch("Guard pilot","GUARD_NC",cyan,10f,out _,out var guardIn);
                var safety=Contact("Normally closed guard",positive,guardIn,true);
                Signal("CTRL_MOTOR_PWM",cyan); Sense("MDRV_PWM",cg,cyan);
                Signal("XS_OUT",cyan); Sense("CTRL_X_IN",cg,cyan);
                Signal("YS_OUT",cyan); Sense("CTRL_Y_IN",cg,cyan);
                Signal("ENC_A",cyan); Sense("CTRL_ENC_A",cg,cyan);
                Signal("ENC_B",cyan); Sense("CTRL_ENC_B",cg,cyan);
                Signal("ENC_Z",cyan); Sense("CTRL_ENC_Z",cg,cyan);
                Signal("CTRL_DAMP_CMD",cyan); Sense("DDRV_CMD",cg,cyan);
                Sense("CTRL_SAFE_IN",cg,cyan);
                string[] wireOrder={"SUPPLY_POS","CTRL_POS","SUPPLY_NEG","CTRL_GND","MDRV_POS","MDRV_GND","XS_POS","XS_GND","YS_POS","YS_GND","DDRV_POS","DDRV_GND","CTRL_MOTOR_PWM","MDRV_PWM","MDRV_MPOS","MOTOR_POS","MDRV_MNEG","MOTOR_NEG","XS_OUT","CTRL_X_IN","YS_OUT","CTRL_Y_IN","ENC_A","CTRL_ENC_A","ENC_B","CTRL_ENC_B","ENC_Z","CTRL_ENC_Z","CTRL_DAMP_CMD","DDRV_CMD","DDRV_ACTPOS","DAMP_POS","DDRV_ACTNEG","DAMP_NEG","GUARD_NC","CTRL_SAFE_IN"};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,motor,actuator,driver,safety);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel54Critical>();
                controller.layout=layout;
                controller.source=source;


                controller.motor=motor;
                controller.actuator=actuator;
                controller.damperDrive=damperDrive;
                controller.driver=driver;
                controller.drive=drive;
                controller.safety=safety;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel54VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 54 prefab.");
                SparkLevel54DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 54 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_54_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel54Critical c,Font font)
        {
            var obj=new GameObject("Level 54 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,254),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the resonance rig.",font,new Vector2(1080,28),new Vector2(0,225));
            c.meterText=UIText(panel,"Measurements","ROTOR DYNAMICS",font,new Vector2(1080,28),new Vector2(0,198));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,172));
            c.calibrationText=UIText(panel,"Resonance results","",font,new Vector2(1080,25),new Vector2(0,148));
            c.calibrationText.fontSize=16;
            var plotPanel=Rect(canvas.transform,"Displacement and sweep",new Vector2(350,160),new Vector2(505,6));
            var plotBackground=plotPanel.gameObject.AddComponent<Image>();
            plotBackground.color=new Color(.035f,.08f,.13f,.94f);
            plotBackground.raycastTarget=false;
            UIText(plotPanel,"Title","X/Y ORBIT     |     SWEEP 44–74 RPM",font,new Vector2(340,24),new Vector2(0,131)).fontSize=16;
            c.plot=Rect(plotPanel,"Plot traces",new Vector2(330,102),new Vector2(0,28)).gameObject.AddComponent<SparkLevel54Plot>();
            c.plot.raycastTarget=false;
            UIText(plotPanel,"Legend","AMBER  BASELINE   /   CYAN  DAMPED",font,new Vector2(345,24),Vector2.zero).fontSize=14;
            c.powerButton=Control(panel,font,"POWER",-453f,103,c.Power);
            c.startButton=Control(panel,font,"START",-302f,103,c.StartMachine);
            c.stopButton=Control(panel,font,"STOP",-151f,103,c.StopMachine);
            c.guardButton=Control(panel,font,"OPEN GUARD",0f,103,c.Guard);
            c.calibrateButton=Control(panel,font,"CALIBRATE X/Y",151f,103,c.Calibrate);
            c.baselineButton=Control(panel,font,"BASELINE",302f,103,c.Baseline);
            c.sweepButton=Control(panel,font,"SPEED SWEEP",453f,103,c.Sweep);
            c.offButton=Control(panel,font,"DAMP OFF",-453f,60,c.Off);
            c.lowButton=Control(panel,font,"LOW DAMPING",-302f,60,c.Low);
            c.mediumButton=Control(panel,font,"MEDIUM DAMPING",-151f,60,c.Medium);
            c.highButton=Control(panel,font,"HIGH DAMPING",0f,60,c.High);
            c.autoButton=Control(panel,font,"AUTO DAMPING",151f,60,c.Auto);
            c.verifyButton=Control(panel,font,"VERIFY DAMPING",302f,60,c.Verify);
            c.crossButton=Control(panel,font,"CROSS CRITICAL",453f,60,c.Cross);
            c.slowButton=Control(panel,font,"45 RPM",-453f,17,c.Speed45);
            c.criticalButton=Control(panel,font,"60 RPM",-302f,17,c.Speed60);
            c.fastButton=Control(panel,font,"75 RPM",-151f,17,c.Speed75);
            c.coastButton=Control(panel,font,"COAST DOWN",0f,17,c.Coast);
            c.phaseButton=Control(panel,font,"SENSOR FAULT TEST",151f,17,c.PhaseTest);
            c.resetButton=Control(panel,font,"RESET FAULT",302f,17,c.ResetFault);
        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(146,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.54f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(146,37),Vector2.zero).fontSize=14;
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_53_Layout/Level_53_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Couple Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_54" || targets.arraySize!=SparkLevel54Critical.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel54Critical.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_54"),("displayName","Cross the Critical"),
                ("description","Measure a flexible rotor resonance, tune active damping, cross the critical speed and recover from lost Y feedback."),
                ("completionMode",0),("requiredTargetCount",Objectives.Length),("requireClosedReturn",false),
                ("rejectShortCircuit",true),("rejectTargetShort",true),("requireMinimumVoltage",false),
                ("autoAdvanceOnCompletion",false));
            var so=new SerializedObject(level);
            var sources=so.FindProperty("powerSources"); sources.arraySize=1;
            var source=sources.GetArrayElementAtIndex(0);
            source.FindPropertyRelative("sourceName").stringValue="CONTROL_POWER";
            source.FindPropertyRelative("positiveTerminalId").stringValue="SUPPLY_POS";
            source.FindPropertyRelative("negativeTerminalId").stringValue="SUPPLY_NEG";
            source.FindPropertyRelative("nominalVoltage").floatValue=5f;
            var targets=so.FindProperty("targets"); targets.arraySize=Objectives.Length;
            for (int i=0;i<Objectives.Length;i++)
            {
                var target=targets.GetArrayElementAtIndex(i);
                target.FindPropertyRelative("targetId").stringValue=$"level54_critical_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel54Critical.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_54.asset");
            return level;
        }

        private static void WriteBindings(SparkLevelSceneBindings bindings,List<SparkTerminal> pins,SparkPowerSupply source, SparkMotor motor, SparkResistor actuator, SparkResistor driver, SparkSwitch safety)
        {
            var so=new SerializedObject(bindings);
            var terminals=so.FindProperty("terminals"); terminals.arraySize=pins.Count;
            for(int i=0;i<pins.Count;i++)
            {
                var row=terminals.GetArrayElementAtIndex(i);
                row.FindPropertyRelative("id").stringValue=pins[i].name;
                row.FindPropertyRelative("terminal").objectReferenceValue=pins[i];
            }
            var fixedComponents = new (string id,SparkElectricalComponent component)[] {
                ("CONTROL_POWER",source), ("ROTOR_MOTOR",motor), ("DAMPER_ACTUATOR",actuator),
                ("MOTOR_DRIVER_PWM",driver), ("GUARD_INTERLOCK",safety)
            };
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Length;
            for(int i=0;i<fixedComponents.Length;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel54Critical.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel54Critical.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }


        private static T Part<T>(Transform parent,string name,float x,float z,Material material) where T:SparkElectricalComponent
        {
            var obj=Child(parent,name,new Vector3(x,.055f,z));
            Solid(obj,"Body",PrimitiveType.Cube,Vector3.zero,new Vector3(.22f,.09f,.13f),material);
            var component=obj.gameObject.AddComponent<T>();
            Set(component,("objectId",name.Replace(' ','_')),("displayName",name));
            return component;
        }

        private static SparkTerminal Pin(SparkElectricalComponent owner,string id,float x,float z,Material material,
            List<SparkTerminal> pins,SparkTerminalPolarity polarity=SparkTerminalPolarity.None)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.name=id; obj.transform.SetParent(owner.transform,false);
            obj.transform.localPosition=new Vector3(x,.065f,z); obj.transform.localScale=new Vector3(.075f,.018f,.075f);
            obj.GetComponent<Renderer>().sharedMaterial=material;
            int layer=LayerMask.NameToLayer("Pin"); if(layer>=0) obj.layer=layer;
            var terminal=obj.AddComponent<SparkTerminal>();
            var kind=polarity==SparkTerminalPolarity.Positive ? SparkTerminalKind.Power :
                polarity==SparkTerminalPolarity.Negative ? SparkTerminalKind.Ground : SparkTerminalKind.Generic;
            Set(terminal,("owner",owner),("kind",kind),("polarity",polarity),("maxConnections",polarity==SparkTerminalPolarity.None ? 1 : 5));
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
            text.characterSize=.022f; text.fontSize=54; text.color=Color.white;
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
            string path=$"{Folder}/L54 {name}.mat";
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
