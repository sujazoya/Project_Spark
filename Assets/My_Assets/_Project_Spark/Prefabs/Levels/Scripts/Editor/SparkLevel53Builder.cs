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
    public static class SparkLevel53Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_53_Layout";
        private const string PrefabPath = Folder + "/Level_53_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the dynamic balancer", "Discover couple imbalance", "Calibrate plane A",
            "Calibrate plane B", "Balance both planes", "Certify at 60 RPM and recover feedback"
        };

        [MenuItem("Tools/Project Spark/Level 53/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 53 prefab already exists at " + PrefabPath);
                SparkLevel53DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_53.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_53.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.53f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.62f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.56f,.16f));

            var root = new GameObject("Level_53_Layout");
            try
            {
                var bench = Child(root.transform,"Couple Workbench",Vector3.zero);
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
                var positive=Pin(source,"SUPPLY_PLUS",0,0,red,pins,SparkTerminalPolarity.Positive);
                var ground=Pin(source,"SUPPLY_RETURN",0,0,blue,pins,SparkTerminalPolarity.Negative);
                Set(source,("positiveTerminal",positive),("negativeTerminal",ground));
                var power=Part<SparkResistor>(bench,"Registration controller power",0,0,steel);
                var cp=Pin(power,"CTRL_PLUS",0,0,red,pins);
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


                Load("Left sensor power","VIB_LEFT_PLUS","VIB_LEFT_GND");
                Load("Right sensor power","VIB_RIGHT_PLUS","VIB_RIGHT_GND");
                var motorRail=Load("Motor driver power","MDRV_PLUS","MDRV_GND");
                var balanceRail=Load("Balance driver power","BDRV_PLUS","BDRV_GND");
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
                var driver=Branch("Rotor PWM equivalent","MDRV_MP",gold,21f,out _,out var driverIn);
                var drive=Contact("Motor enable",motorRail,driverIn,false);
                var ret=Branch("Motor return","MDRV_MN",blue,.025f,out _,out var motorReturn);
                Contact("Internal motor return",pins.Find(p=>p.name=="MDRV_GND"),motorReturn,true);
                var motor=Part<SparkMotor>(bench,"ROTOR_MOTOR",0,0,steel);
                Set(motor,("positiveTerminal",Pin(motor,"MOTOR_POS",0,0,gold,pins)),("negativeTerminal",Pin(motor,"MOTOR_NEG",0,0,blue,pins)),
                    ("windingResistance",22f),("startupVoltage",.35f),("maximumCurrent",.23f));

                SparkResistor Actuator(string name,string positivePin,string negativePin,string drivePositive,string driveNegative,out SparkSwitch enable)
                {
                    var load=Part<SparkResistor>(bench,name,0,0,steel);
                    Set(load,("terminalA",Pin(load,positivePin,0,0,gold,pins)),
                        ("terminalB",Pin(load,negativePin,0,0,blue,pins)),("resistance",68f),("maximumPowerWatts",1f));
                    Branch(name+" drive",drivePositive,gold,.025f,out _,out var input);
                    enable=Contact(name+" enable",balanceRail,input,false);
                    Branch(name+" return",driveNegative,blue,.025f,out _,out var retPin);
                    Contact(name+" internal return",pins.Find(p=>p.name=="BDRV_GND"),retPin,true);
                    return load;
                }
                var actuatorA=Actuator("Plane A actuator","PLANE_A_PLUS","PLANE_A_NEG","BDRV_AP","BDRV_AN",out var driveA);
                var actuatorB=Actuator("Plane B actuator","PLANE_B_PLUS","PLANE_B_NEG","BDRV_BP","BDRV_BN",out var driveB);
                Branch("Guard pilot","GUARD_NC",cyan,10f,out _,out var guardIn);
                var safety=Contact("Normally closed guard",positive,guardIn,true);
                Signal("CTRL_PWM",cyan); Sense("MDRV_PWM",cg,cyan);
                Signal("VIB_LEFT_OUT",cyan); Sense("CTRL_VIB_L",cg,cyan);
                Signal("VIB_RIGHT_OUT",cyan); Sense("CTRL_VIB_R",cg,cyan);
                Signal("ENCODER_A",cyan); Sense("CTRL_ENC_A",cg,cyan);
                Signal("ENCODER_B",cyan); Sense("CTRL_ENC_B",cg,cyan);
                Signal("ENCODER_Z",cyan); Sense("CTRL_ENC_Z",cg,cyan);
                Signal("CTRL_BAL",cyan); Sense("BDRV_CMD",cg,cyan);
                Sense("CTRL_SAFE",cg,cyan);
                string[] wireOrder={"SUPPLY_PLUS","CTRL_PLUS","SUPPLY_RETURN","CTRL_GND","MDRV_PLUS","MDRV_GND","VIB_LEFT_PLUS","VIB_LEFT_GND","VIB_RIGHT_PLUS","VIB_RIGHT_GND","BDRV_PLUS","BDRV_GND","CTRL_PWM","MDRV_PWM","MDRV_MP","MOTOR_POS","MDRV_MN","MOTOR_NEG","VIB_LEFT_OUT","CTRL_VIB_L","VIB_RIGHT_OUT","CTRL_VIB_R","ENCODER_A","CTRL_ENC_A","ENCODER_B","CTRL_ENC_B","ENCODER_Z","CTRL_ENC_Z","CTRL_BAL","BDRV_CMD","BDRV_AP","PLANE_A_PLUS","BDRV_AN","PLANE_A_NEG","BDRV_BP","PLANE_B_PLUS","BDRV_BN","PLANE_B_NEG","GUARD_NC","CTRL_SAFE"};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel53Couple>();
                controller.layout=layout;
                controller.source=source;


                controller.motor=motor;
                controller.actuatorA=actuatorA; controller.actuatorB=actuatorB;
                controller.driveA=driveA; controller.driveB=driveB;
                controller.driver=driver;
                controller.drive=drive;
                controller.safety=safety;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel53VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 53 prefab.");
                SparkLevel53DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 53 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_53_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel53Couple c,Font font)
        {
            var obj=new GameObject("Level 53 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,253),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the balancing rig.",font,new Vector2(1080,28),new Vector2(0,225));
            c.meterText=UIText(panel,"Measurements","BALANCING RIG",font,new Vector2(1080,28),new Vector2(0,198));
            c.meterText.color=new Color(.40f,.92f,.93f); c.meterText.fontSize=19;
            c.detailText=UIText(panel,"Details","OFF",font,new Vector2(1080,28),new Vector2(0,172));
            c.detailText.fontSize=16;
            c.calibrationText=UIText(panel,"Influence and correction","",font,new Vector2(1080,25),new Vector2(0,148));
            c.calibrationText.fontSize=15;
            c.powerButton=Control(panel,font,"POWER",-432.5f,103,c.Power);
            c.slowButton=Control(panel,font,"45 RPM",-259.5f,103,c.Speed45);
            c.fastButton=Control(panel,font,"60 RPM",-86.5f,103,c.Speed60);
            c.startButton=Control(panel,font,"START",86.5f,103,c.StartMachine);
            c.stopButton=Control(panel,font,"STOP",259.5f,103,c.StopMachine);
            c.baselineButton=Control(panel,font,"BASELINE",432.5f,103,c.Baseline);
            c.modeButton=Control(panel,font,"MODE: MANUAL",-432.5f,60,c.ToggleMode);
            c.trialAButton=Control(panel,font,"TRIAL A",-259.5f,60,c.TrialA);
            c.trialBButton=Control(panel,font,"TRIAL B",-86.5f,60,c.TrialB);
            c.solveButton=Control(panel,font,"SOLVE",86.5f,60,c.Solve);
            c.applyButton=Control(panel,font,"APPLY",259.5f,60,c.Apply);
            c.verifyButton=Control(panel,font,"VERIFY",432.5f,60,c.Verify);
            c.phaseButton=Control(panel,font,"SENSOR FAULT TEST",-432.5f,17,c.PhaseTest);
            c.guardButton=Control(panel,font,"OPEN GUARD",-259.5f,17,c.Guard);
            c.resetButton=Control(panel,font,"RESET FAULT",-86.5f,17,c.ResetFault);
        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(166,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.53f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(166,37),Vector2.zero);
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_52_Layout/Level_52_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Maintenance Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_53" || targets.arraySize!=SparkLevel53Couple.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel53Couple.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_53"),("displayName","The Hidden Couple"),
                ("description","Measure both bearings, identify a hidden couple, calibrate both planes and certify recovery from lost feedback."),
                ("completionMode",0),("requiredTargetCount",Objectives.Length),("requireClosedReturn",false),
                ("rejectShortCircuit",true),("rejectTargetShort",true),("requireMinimumVoltage",false),
                ("autoAdvanceOnCompletion",false));
            var so=new SerializedObject(level);
            var sources=so.FindProperty("powerSources"); sources.arraySize=1;
            var source=sources.GetArrayElementAtIndex(0);
            source.FindPropertyRelative("sourceName").stringValue="CONTROL_POWER";
            source.FindPropertyRelative("positiveTerminalId").stringValue="SUPPLY_PLUS";
            source.FindPropertyRelative("negativeTerminalId").stringValue="SUPPLY_RETURN";
            source.FindPropertyRelative("nominalVoltage").floatValue=5f;
            var targets=so.FindProperty("targets"); targets.arraySize=Objectives.Length;
            for (int i=0;i<Objectives.Length;i++)
            {
                var target=targets.GetArrayElementAtIndex(i);
                target.FindPropertyRelative("targetId").stringValue=$"level53_registration_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from solved wiring, quadrature position and verified print history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel53Couple.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_53.asset");
            return level;
        }

        private static void WriteBindings(SparkLevelSceneBindings bindings,List<SparkTerminal> pins,SparkPowerSupply source)
        {
            var so=new SerializedObject(bindings);
            var terminals=so.FindProperty("terminals"); terminals.arraySize=pins.Count;
            for(int i=0;i<pins.Count;i++)
            {
                var row=terminals.GetArrayElementAtIndex(i);
                row.FindPropertyRelative("id").stringValue=pins[i].name;
                row.FindPropertyRelative("terminal").objectReferenceValue=pins[i];
            }
            var components=so.FindProperty("components"); components.arraySize=1;
            var power=components.GetArrayElementAtIndex(0);
            power.FindPropertyRelative("id").stringValue="CONTROL_POWER";
            power.FindPropertyRelative("component").objectReferenceValue=source;
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel53Couple.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel53Couple.Gates[i];
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
            text.characterSize=.022f; text.fontSize=53; text.color=Color.white;
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
            string path=$"{Folder}/L53 {name}.mat";
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




