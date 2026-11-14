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
    public static class SparkLevel56Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_56_Layout";
        private const string PrefabPath = Folder + "/Level_56_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the energy-recovery system", "Store flywheel energy at 90 RPM", "Recover braking energy",
            "Measure at least 60% recovery", "Reuse energy to reduce supply demand", "Recover from voltage feedback loss"
        };

        [MenuItem("Tools/Project Spark/Level 56/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 56 prefab already exists at " + PrefabPath);
                SparkLevel56DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_56.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_56.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.56f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.62f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.56f,.16f));

            var root = new GameObject("Level_56_Layout");
            try
            {
                var bench = Child(root.transform,"Energy Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Temporary board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.56f),board);
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


                Load("Powered sensors","SENSE_POS","SENSE_GND");
                var converterRail=Load("Bidirectional converter supply","CONV_POS","CONV_GND");
                var chopperRail=Load("Chopper supply","CHOP_POS","CHOP_GND");
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
                var driver=Branch("Converter input load","CONV_MPLUS",gold,250f,out _,out var driverIn);
                var drive=Contact("Motor drive enable",converterRail,driverIn,false);
                var motor=Part<SparkMotor>(bench,"Motor branch load",0,0,steel);
                Set(motor,("positiveTerminal",Pin(motor,"MOTOR_PLUS",0,0,gold,pins)),
                    ("negativeTerminal",Pin(motor,"MOTOR_MINUS",0,0,blue,pins)),
                    ("windingResistance",1.15f),("startupVoltage",.1f),("maximumCurrent",.62f));
                Set(driver,("maximumPowerWatts",4f));
                Set(driver.GetComponent<SparkResistorBurn>(),("ratedWatts",4f),("thermalCapacityJPerC",2.6f),("coolingWattsPerC",.18f));

                var brake=Part<SparkResistor>(bench,"Chopper resistor",0,0,steel);
                Set(brake,("terminalA",Pin(brake,"BRAKE_PLUS",0,0,gold,pins)),
                    ("terminalB",Pin(brake,"BRAKE_MINUS",0,0,blue,pins)),("resistance",12f),("maximumPowerWatts",3f));
                Set(brake.GetComponent<SparkResistorBurn>(),("ratedWatts",3f),("thermalCapacityJPerC",2.6f),("coolingWattsPerC",.18f));
                Branch("Chopper output","CHOP_RPLUS",gold,1000f,out _,out var chopperOut);
                // The shared solver validates the branch and its return. Rotor
                // and resistor energy accounting is owned by the averaged converter.
                var brakeDrive=Contact("Chopper enable",chopperRail,chopperOut,false);
                var capLoad=Part<SparkResistor>(bench,"Bank high-impedance interface",0,0,steel);
                Set(capLoad,("terminalA",Pin(capLoad,"CAP_PLUS",0,0,red,pins)),
                    ("terminalB",Pin(capLoad,"CAP_MINUS",0,0,blue,pins)),("resistance",1000000f));
                Branch("Storage positive routing","CONV_STORE_PLUS",gold,1000000f,out _,out var storageIn);
                Sense("PROT_IN_PLUS",cg,gold);
                Sense("PROT_IN_MINUS",cg,blue);
                var protectedOut=Sense("PROT_OUT_PLUS",cg,gold);
                var protection=Contact("Storage protection relay",pins.Find(pin=>pin.name=="PROT_IN_PLUS"),protectedOut,false);
                Branch("Guard pilot","GUARD_OUT",cyan,10f,out _,out var guardIn);
                var safety=Contact("Normally closed guard",positive,guardIn,true);
                Signal("ENC_A",cyan); Sense("CTRL_ENC_A",cg,cyan);
                Signal("ENC_B",cyan); Sense("CTRL_ENC_B",cg,cyan);
                Signal("ENC_Z",cyan); Sense("CTRL_ENC_Z",cg,cyan);
                Signal("CURRENT_OUT",cyan); Sense("CTRL_CURRENT",cg,cyan);
                Sense("CTRL_BUS_V",cg,cyan);
                Signal("CAP_VSENSE",cyan); Sense("CTRL_CAP_V",cg,cyan);
                Signal("CTRL_REGEN",cyan); Sense("CONV_REGEN",cg,cyan);
                Signal("CTRL_PWM",cyan); Sense("CONV_PWM",cg,cyan);
                Signal("CTRL_CHOP",cyan); Sense("CHOP_CMD",cg,cyan);
                Sense("CTRL_SAFE",cg,cyan);
                Signal("CTRL_REUSE",cyan); Sense("LOAD_ENABLE",cg,cyan);
                Signal("METER_OUT",cyan); Sense("CTRL_METER",cg,cyan);
                Sense("CTRL_PROTECTION",cg,cyan);
                Signal("LOAD_OUT",cyan); Sense("CONV_LOAD",cg,cyan);
                string[] wireOrder={"SUPPLY_POS","CTRL_POS","SUPPLY_RET","CTRL_GND","CONV_POS","CONV_GND","SENSE_POS","SENSE_GND","CHOP_POS","CHOP_GND","CONV_MPLUS","MOTOR_PLUS","MOTOR_MINUS","ENC_A","CTRL_ENC_A","ENC_B","CTRL_ENC_B","ENC_Z","CTRL_ENC_Z","CURRENT_OUT","CTRL_CURRENT","CTRL_BUS_V","CAP_VSENSE","CTRL_CAP_V","CONV_STORE_PLUS","PROT_IN_PLUS","PROT_IN_MINUS","PROT_OUT_PLUS","CAP_PLUS","CAP_MINUS","CTRL_REGEN","CONV_REGEN","CTRL_PWM","CONV_PWM","CTRL_CHOP","CHOP_CMD","CHOP_RPLUS","BRAKE_PLUS","BRAKE_MINUS","GUARD_OUT","CTRL_SAFE","CTRL_REUSE","LOAD_ENABLE","METER_OUT","CTRL_METER","CTRL_PROTECTION","LOAD_OUT","CONV_LOAD"};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,motor,brake,capLoad,driver,safety);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel56Harvest>();
                controller.layout=layout;
                controller.source=source;


                controller.brake=brake; controller.capLoad=capLoad;
                controller.brakeDrive=brakeDrive;
                controller.protection=protection;
                controller.driver=driver;
                controller.drive=drive;
                controller.safety=safety;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel56VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 56 prefab.");
                SparkLevel56DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 56 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_56_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel56Harvest c,Font font)
        {
            var obj=new GameObject("Level 56 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,256),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the regenerative drive.",font,new Vector2(1080,28),new Vector2(0,260));
            c.meterText=UIText(panel,"Measurements","ENERGY RECOVERY",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Energy balance","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            var plotPanel=Rect(canvas.transform,"Displacement and sweep",new Vector2(350,160),new Vector2(505,6));
            var plotBackground=plotPanel.gameObject.AddComponent<Image>();
            plotBackground.color=new Color(.035f,.08f,.13f,.94f);
            plotBackground.raycastTarget=false;
            UIText(plotPanel,"Title","FLYWHEEL AND BANK",font,new Vector2(340,24),new Vector2(0,131)).fontSize=16;
            c.plot=Rect(plotPanel,"Plot traces",new Vector2(330,102),new Vector2(0,28)).gameObject.AddComponent<SparkLevel56Plot>();
            c.plot.raycastTarget=false;
            UIText(plotPanel,"Legend","CYAN SPEED  /  AMBER BANK VOLTAGE",font,new Vector2(345,24),Vector2.zero).fontSize=14;
            c.powerButton=Control(panel,font,"MAIN POWER",-453f,139,c.Power);
            c.guardButton=Control(panel,font,"OPEN GUARD",-302f,139,c.Guard);
            c.calibrateButton=Control(panel,font,"CALIBRATE",-151f,139,c.Calibrate);
            c.startButton=Control(panel,font,"START",0f,139,c.StartMotor);
            c.stopButton=Control(panel,font,"STOP",151f,139,c.Stop);
            c.slowButton=Control(panel,font,"45 RPM",302f,139,c.Speed45);
            c.midButton=Control(panel,font,"60 RPM",453f,139,c.Speed60);
            c.fastButton=Control(panel,font,"90 RPM",-453f,97,c.Speed90);
            c.motoringButton=Control(panel,font,"MOTORING",-302f,97,c.Motor);
            c.regenButton=Control(panel,font,"REGENERATION",-151f,97,c.Regen);
            c.reuseButton=Control(panel,font,"REUSE ENERGY",0f,97,c.Reuse);
            c.resistiveButton=Control(panel,font,"RESISTIVE BRAKE",151f,97,c.Resistive);
            c.recoveryButton=Control(panel,font,"BEGIN RECOVERY",302f,97,c.BeginRecovery);
            c.optimizeButton=Control(panel,font,"OPTIMIZE",453f,97,c.Optimize);
            c.verifyButton=Control(panel,font,"VERIFY EFFICIENCY",-453f,55,c.Verify);
            c.highButton=Control(panel,font,"HIGH CURRENT",-302f,55,c.High);
            c.lowButton=Control(panel,font,"LOW CURRENT",-151f,55,c.Low);
            c.reuseTestButton=Control(panel,font,"ENERGY REUSE TEST",0f,55,c.ReuseTest);
            c.protectButton=Control(panel,font,"PROTECTION TEST",151f,55,c.ProtectionTest);
            c.phaseButton=Control(panel,font,"FAULT TEST",302f,55,c.FaultTest);
            c.resetButton=Control(panel,font,"RESET FAULT",453f,55,c.ResetFault);
            c.finalButton=Control(panel,font,"FINAL CERTIFICATION",-453f,13,c.FinalCertification);
        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(146,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.56f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(146,37),Vector2.zero).fontSize=14;
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_55_Layout/Level_55_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Levitation Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_56" || targets.arraySize!=SparkLevel56Harvest.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel56Harvest.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_56"),("displayName","Harvest the Spin"),
                ("description","Recover flywheel energy during braking, optimize the converter, reuse stored energy and recover from lost capacitor feedback."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level56_harvest_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel56Harvest.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_56.asset");
            return level;
        }

        private static void WriteBindings(SparkLevelSceneBindings bindings,List<SparkTerminal> pins,SparkPowerSupply source, SparkMotor motor, SparkResistor brake, SparkResistor capLoad, SparkResistor driver, SparkSwitch safety)
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
                ("CONTROL_POWER",source), ("ROTOR_MOTOR",motor), ("BRAKE_RESISTOR",brake), ("CAP_INTERFACE",capLoad),
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
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel56Harvest.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel56Harvest.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }


        private static T Part<T>(Transform parent,string name,float x,float z,Material material) where T:SparkElectricalComponent
        {
            var obj=Child(parent,name,new Vector3(x,.056f,z));
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
            Set(terminal,("owner",owner),("kind",kind),("polarity",polarity),("maxConnections",polarity==SparkTerminalPolarity.None ? (id is "CONV_POS" or "CONV_GND" or "CHOP_GND" or "PROT_IN_PLUS" or "PROT_IN_MINUS" ? 2 : 1) : 5));
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
            text.characterSize=.022f; text.fontSize=56; text.color=Color.white;
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
            string path=$"{Folder}/L56 {name}.mat";
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
