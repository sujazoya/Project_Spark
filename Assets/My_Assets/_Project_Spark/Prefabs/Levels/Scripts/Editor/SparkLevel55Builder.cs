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
    public static class SparkLevel55Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_55_Layout";
        private const string PrefabPath = Folder + "/Level_55_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the four-axis magnetic bearing system", "Calibrate and float the shaft",
            "Record contact-free operation at 45 RPM", "Recover from left and right disturbances",
            "Certify 75 RPM and return safely", "Recover W13, relift and recertify"
        };

        [MenuItem("Tools/Project Spark/Level 55/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 55 prefab already exists at " + PrefabPath);
                SparkLevel55DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_55.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_55.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.55f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.62f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.56f,.16f));

            var root = new GameObject("Level_55_Layout");
            try
            {
                var bench = Child(root.transform,"Levitation Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Temporary board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.55f),board);
                var pins = new List<SparkTerminal>();
                SparkPowerSupply Supply(string name, float voltage, float current)
                {
                    var part=Part<SparkPowerSupply>(bench,name,0,0,steel);
                    Set(part,("outputVoltage",voltage),("currentLimit",current),("state",SparkPowerSupplyState.Active));
                    return part;
                }
                var source=Supply("CONTROL_POWER",5f,.65f);
                var positive=Pin(source,"SUPPLY_POS",0,0,red,pins,SparkTerminalPolarity.Positive);
                var ground=Pin(source,"SUPPLY_RET",0,0,blue,pins,SparkTerminalPolarity.Negative);
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


                Load("Four-axis sensor hub power","HUB_POS","HUB_GND");
                var motorRail=Load("Motor driver power","MD_POS","MD_GND");
                var leftRail=Load("Left magnetic driver power","LD_POS","LD_GND");
                var rightRail=Load("Right magnetic driver power","RD_POS","RD_GND");
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
                var driver=Branch("Rotor PWM equivalent","MD_MPLUS",gold,21f,out _,out var driverIn);
                var drive=Contact("Motor enable",motorRail,driverIn,false);
                var ret=Branch("Motor return","MD_MMINUS",blue,.025f,out _,out var motorReturn);
                Contact("Internal motor return",pins.Find(p=>p.name=="MD_GND"),motorReturn,true);
                var motor=Part<SparkMotor>(bench,"ROTOR_MOTOR",0,0,steel);
                Set(motor,("positiveTerminal",Pin(motor,"MOTOR_PLUS",0,0,gold,pins)),("negativeTerminal",Pin(motor,"MOTOR_MINUS",0,0,blue,pins)),
                    ("windingResistance",22f),("startupVoltage",.35f),("maximumCurrent",.23f));

                SparkResistor Station(string name, string driverPrefix, string magnetPrefix, SparkTerminal rail, out SparkSwitch enable)
                {
                    var actuator=Part<SparkResistor>(bench,name,0,0,steel);
                    Set(actuator,("terminalA",Pin(actuator,magnetPrefix+"_PWRPOS",0,0,gold,pins)),
                        ("terminalB",Pin(actuator,magnetPrefix+"_PWRNEG",0,0,blue,pins)),("resistance",44f),("maximumPowerWatts",1f));
                    Branch(name+" supply",driverPrefix+"_PWRPOS",gold,.025f,out _,out var supply);
                    Branch(name+" return",driverPrefix+"_PWRNEG",blue,.025f,out _,out var windingReturn);
                    enable=Contact(name+" enable",rail,supply,false);
                    Contact(name+" internal return",pins.Find(pin=>pin.name==driverPrefix+"_GND"),windingReturn,true);
                    return actuator;
                }

                var leftCoils=Station("Left magnetic winding","LD","MAG_A",leftRail,out var leftDrive);
                var rightCoils=Station("Right magnetic winding","RD","MAG_B",rightRail,out var rightDrive);
                Branch("Guard pilot","GUARD_NC_OUT",cyan,10f,out _,out var guardIn);
                var safety=Contact("Normally closed guard",positive,guardIn,true);
                Signal("HUB_XL",cyan); Sense("CTRL_XL",cg,cyan);
                Signal("HUB_YL",cyan); Sense("CTRL_YL",cg,cyan);
                Signal("HUB_XR",cyan); Sense("CTRL_XR",cg,cyan);
                Signal("HUB_YR",cyan); Sense("CTRL_YR",cg,cyan);
                Signal("ENC_A",cyan); Sense("CTRL_ENC_A",cg,cyan);
                Signal("ENC_B",cyan); Sense("CTRL_ENC_B",cg,cyan);
                Signal("ENC_Z",cyan); Sense("CTRL_ENC_Z",cg,cyan);
                Signal("CTRL_PWM",cyan); Sense("MD_PWM",cg,cyan);
                Signal("CTRL_CMD_LEFT",cyan); Sense("LD_CMD",cg,cyan);
                Signal("CTRL_CMD_RIGHT",cyan); Sense("RD_CMD",cg,cyan);
                Sense("CTRL_SAFE",cg,cyan);
                Signal("TOUCHDOWN_OUT",cyan); Sense("CTRL_TOUCHDOWN",cg,cyan);
                string[] wireOrder={"SUPPLY_POS","CTRL_POS","SUPPLY_RET","CTRL_GND","MD_POS","MD_GND","LD_POS","LD_GND","RD_POS","RD_GND","HUB_POS","HUB_GND","HUB_XL","CTRL_XL","HUB_YL","CTRL_YL","HUB_XR","CTRL_XR","HUB_YR","CTRL_YR","ENC_A","CTRL_ENC_A","ENC_B","CTRL_ENC_B","ENC_Z","CTRL_ENC_Z","CTRL_PWM","MD_PWM","MD_MPLUS","MOTOR_PLUS","MD_MMINUS","MOTOR_MINUS","CTRL_CMD_LEFT","LD_CMD","CTRL_CMD_RIGHT","RD_CMD","LD_PWRPOS","MAG_A_PWRPOS","LD_PWRNEG","MAG_A_PWRNEG","RD_PWRPOS","MAG_B_PWRPOS","RD_PWRNEG","MAG_B_PWRNEG","GUARD_NC_OUT","CTRL_SAFE","TOUCHDOWN_OUT","CTRL_TOUCHDOWN"};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                ConfigurePowerHardware(driver,leftCoils,rightCoils);
                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,motor,leftCoils,rightCoils,driver,safety);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel55Float>();
                controller.layout=layout;
                controller.source=source;


                controller.motor=motor;
                controller.leftCoils=leftCoils; controller.rightCoils=rightCoils;
                controller.leftDrive=leftDrive; controller.rightDrive=rightDrive;
                controller.driver=driver;
                controller.drive=drive;
                controller.safety=safety;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel55VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 55 prefab.");
                SparkLevel55DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 55 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void ConfigurePowerHardware(SparkResistor driver,SparkResistor left,SparkResistor right)
        {
            foreach (var load in new[] {driver,left,right})
            {
                var thermal = load.GetComponent<SparkResistorBurn>();
                Set(thermal,("ratedWatts",load.MaximumPowerWatts),("thermalCapacityJPerC",1.4f),
                    ("coolingWattsPerC",.016f));
            }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_55_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel55Float c,Font font)
        {
            var obj=new GameObject("Level 55 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,255),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the magnetic bearing rig.",font,new Vector2(1080,28),new Vector2(0,260));
            c.meterText=UIText(panel,"Measurements","MAGNETIC LEVITATION",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Recovery measurements","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            var plotPanel=Rect(canvas.transform,"Displacement and sweep",new Vector2(350,160),new Vector2(505,6));
            var plotBackground=plotPanel.gameObject.AddComponent<Image>();
            plotBackground.color=new Color(.035f,.08f,.13f,.94f);
            plotBackground.raycastTarget=false;
            UIText(plotPanel,"Title","LEFT JOURNAL ORBIT",font,new Vector2(340,24),new Vector2(0,131)).fontSize=16;
            c.plot=Rect(plotPanel,"Plot traces",new Vector2(330,102),new Vector2(0,28)).gameObject.AddComponent<SparkLevel55Plot>();
            c.plot.raycastTarget=false;
            UIText(plotPanel,"Legend","X/Y • ±1.20 mm touchdown clearance",font,new Vector2(345,24),Vector2.zero).fontSize=14;
            c.powerButton=Control(panel,font,"MAIN POWER",-453f,139,c.Power);
            c.guardButton=Control(panel,font,"OPEN GUARD",-302f,139,c.Guard);
            c.calibrateButton=Control(panel,font,"CALIBRATE ALL",-151f,139,c.Calibrate);
            c.enableButton=Control(panel,font,"ENABLE MAGNETS",0f,139,c.EnableMagnets);
            c.liftButton=Control(panel,font,"LIFT SHAFT",151f,139,c.Lift);
            c.centerButton=Control(panel,font,"CENTER SHAFT",302f,139,c.Center);
            c.startButton=Control(panel,font,"START MOTOR",453f,139,c.StartMotor);
            c.stopButton=Control(panel,font,"STOP MOTOR",-453f,97,c.StopMotor);
            c.slowButton=Control(panel,font,"45 RPM",-302f,97,c.Speed45);
            c.fastButton=Control(panel,font,"75 RPM",-151f,97,c.Speed75);
            c.autoButton=Control(panel,font,"AUTO STABILIZE",0f,97,c.Auto);
            c.manualButton=Control(panel,font,"MANUAL TUNE",151f,97,c.Manual);
            c.kpDownButton=Control(panel,font,"KP −",302f,97,c.LessKp);
            c.kpUpButton=Control(panel,font,"KP +",453f,97,c.MoreKp);
            c.kdDownButton=Control(panel,font,"KD −",-453f,55,c.LessKd);
            c.kdUpButton=Control(panel,font,"KD +",-302f,55,c.MoreKd);
            c.leftButton=Control(panel,font,"LEFT DISTURBANCE",-151f,55,c.LeftTest);
            c.rightButton=Control(panel,font,"RIGHT DISTURBANCE",0f,55,c.RightTest);
            c.recoveryButton=Control(panel,font,"VERIFY RECOVERY",151f,55,c.VerifyRecovery);
            c.highButton=Control(panel,font,"HIGH-SPEED TEST",302f,55,c.HighTest);
            c.coastButton=Control(panel,font,"CONTROLLED STOP",453f,55,c.Coast);
            c.phaseButton=Control(panel,font,"SENSOR FAULT TEST",-453f,13,c.FaultTest);
            c.resetButton=Control(panel,font,"RESET FAULT",-302f,13,c.ResetFault);
            c.reliftButton=Control(panel,font,"RELIFT",-151f,13,c.Relift);
            c.finalButton=Control(panel,font,"FINAL CERTIFICATION",0f,13,c.FinalCertification);
        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(146,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.55f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(146,37),Vector2.zero).fontSize=14;
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_54_Layout/Level_54_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Critical Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_55" || targets.arraySize!=SparkLevel55Float.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel55Float.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_55"),("displayName","Float the Shaft"),
                ("description","Build a magnetic bearing system, lift a shaft with four-axis feedback, diagnose contact and recertify after a sensor failure."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level55_float_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel55Float.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_55.asset");
            return level;
        }

        private static void WriteBindings(SparkLevelSceneBindings bindings,List<SparkTerminal> pins,SparkPowerSupply source, SparkMotor motor, SparkResistor leftCoils, SparkResistor rightCoils, SparkResistor driver, SparkSwitch safety)
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
                ("CONTROL_POWER",source), ("ROTOR_MOTOR",motor), ("LEFT_MAGNETS",leftCoils), ("RIGHT_MAGNETS",rightCoils),
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
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel55Float.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel55Float.Gates[i];
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
            text.characterSize=.022f; text.fontSize=55; text.color=Color.white;
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
            string path=$"{Folder}/L55 {name}.mat";
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


