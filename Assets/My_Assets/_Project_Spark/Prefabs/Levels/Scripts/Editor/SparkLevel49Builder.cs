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
    public static class SparkLevel49Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_49_Layout";
        private const string PrefabPath = Folder + "/Level_49_Layout.prefab";
        private static readonly string[] Objectives = {
            "Wire the flying shear", "Home and calibrate", "Cut accurately at 30 RPM",
            "Cut accurately at 60 RPM", "Recover from web speed disturbance", "Verify guard interlock and restart"
        };

        [MenuItem("Tools/Project Spark/Level 49/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 49 prefab already exists at " + PrefabPath);
                SparkLevel49DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_49.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_49.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.49f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.62f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.56f,.16f));

            var root = new GameObject("Level_49_Layout");
            try
            {
                var bench = Child(root.transform,"Shear Workbench",Vector3.zero);
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


                var sensorPower=Part<SparkResistor>(bench,"Optical eye power",0,0,steel);
                var sp=Pin(sensorPower,"OPTICAL_PLUS",0,0,red,pins);
                var sg=Pin(sensorPower,"OPTICAL_GND",0,0,blue,pins);
                Set(sensorPower,("terminalA",sp),("terminalB",sg),("resistance",4700f));

                var driverPower=Part<SparkResistor>(bench,"Print driver power",0,0,steel);
                var dp=Pin(driverPower,"DRIVER_PLUS",0,0,red,pins);
                var dg=Pin(driverPower,"DRIVER_GND",0,0,blue,pins);
                Set(driverPower,("terminalA",dp),("terminalB",dg),("resistance",2200f));
                SparkTerminal Internal(SparkElectricalComponent owner,string name)
                {
                    var terminal=Child(owner.transform,name,Vector3.zero).gameObject.AddComponent<SparkTerminal>();
                    Set(terminal,("owner",owner));
                    return terminal;
                }
                SparkSwitch Contact(string name,SparkTerminal input,SparkTerminal output,bool closed)
                {
                    var contact=Child(bench,name,Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                    Set(contact,("inputTerminal",input),("outputTerminal",output),
                        ("state",closed ? SparkSwitchState.Closed : SparkSwitchState.Open),("interactionsEnabled",false));
                    return contact;
                }
                SparkMotor Motor(string name)
                {
                    var motor=Part<SparkMotor>(bench,name,0,0,steel);
                    motor.transform.Find("Body").gameObject.SetActive(false);
                    Set(motor,("windingResistance",33f),("startupVoltage",.2f),("maximumCurrent",.18f));
                    return motor;
                }
                var print=Motor("CARRIAGE_MOTOR");
                Set(print,("positiveTerminal",Pin(print,"CARRIAGE_MOTOR_PLUS",0,0,gold,pins)),
                    ("negativeTerminal",Pin(print,"CARRIAGE_MOTOR_MINUS",0,0,gold,pins)));
                var printStage=Child(bench,"Print drive contact",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(printStage,("inputTerminal",dp),("outputTerminal",Pin(printStage,"DRIVER_MOTOR_PLUS",0,0,gold,pins)),
                    ("state",SparkSwitchState.Open),("interactionsEnabled",false));
                var printReturn=Child(bench,"Print drive return",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(printReturn,("inputTerminal",Pin(printReturn,"DRIVER_MOTOR_MINUS",0,0,gold,pins)),
                    ("outputTerminal",dg),("state",SparkSwitchState.Closed),("interactionsEnabled",false));
                var transport=Motor("WEB_TRANSPORT_MOTOR");
                var internalPlus=Internal(transport,"Transport internal +");
                Set(transport,("positiveTerminal",internalPlus),("negativeTerminal",ground));
                var transportStage=Contact("Internal transport contact",positive,internalPlus,false);

                Signal("ENCODER_A",cyan); Sense("CTRL_ENC_A",cg,cyan);
                Signal("ENCODER_B",cyan); Sense("CTRL_ENC_B",cg,cyan);
                Signal("OPTICAL_SIGNAL",cyan); Sense("CTRL_MARK_IN",cg,cyan);
                Signal("CTRL_CARR_PWM",gold); Sense("DRIVER_PWM",dg,gold);
                Signal("CTRL_CUT_TRIG",gold); Sense("CUTTER_TRIG",ground,gold);

                var guard=Child(bench,"Normally closed guard contact",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                var home=Child(bench,"Carriage home contact",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(guard,("inputTerminal",positive),("outputTerminal",Pin(guard,"GUARD_NC_OUT",0,0,cyan,pins)),
                    ("state",SparkSwitchState.Closed));
                Set(home,("inputTerminal",positive),("outputTerminal",Pin(home,"HOME_OUT",0,0,cyan,pins)),
                    ("state",SparkSwitchState.Open));
                Sense("CTRL_GUARD_IN",cg,cyan);
                Sense("CTRL_HOME_IN",cg,cyan);
                string[] wireOrder={"SUPPLY_PLUS","CTRL_PLUS","SUPPLY_RETURN","CTRL_GND","OPTICAL_PLUS","OPTICAL_GND","ENCODER_A","CTRL_ENC_A","ENCODER_B","CTRL_ENC_B","OPTICAL_SIGNAL","CTRL_MARK_IN","CTRL_CARR_PWM","DRIVER_PWM","DRIVER_MOTOR_PLUS","CARRIAGE_MOTOR_PLUS","DRIVER_MOTOR_MINUS","CARRIAGE_MOTOR_MINUS","CTRL_CUT_TRIG","CUTTER_TRIG","DRIVER_PLUS","DRIVER_GND","GUARD_NC_OUT","CTRL_GUARD_IN","HOME_OUT","CTRL_HOME_IN"};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel49Shear>();
                controller.layout=layout;
                controller.source=source;

                controller.transportMotor=transport;
                controller.printMotor=print;
                controller.transportDrive=transportStage;
                controller.printDrive=printStage;
                controller.guardContact=guard;
                controller.homeContact=home;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel49VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 49 prefab.");
                SparkLevel49DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 49 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_49_Layout");
        }

        private static void BuildControls(Transform parent,SparkLevel49Shear c,Font font)
        {
            var obj=new GameObject("Level 49 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,196),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;

            c.statusText=UIText(panel,"Status","Build the registration circuit.",font,new Vector2(1080,28),new Vector2(0,163));
            c.meterText=UIText(panel,"Measurements","WEB / OPTICAL EYE / HEAD",font,new Vector2(1080,28),new Vector2(0,134));
            c.meterText.color=new Color(.40f,.92f,.93f); c.meterText.fontSize=20;
            c.detailText=UIText(panel,"Details","OFF",font,new Vector2(1080,28),new Vector2(0,106));
            c.detailText.fontSize=16;
            c.powerButton=Control(panel,font,"MAIN POWER",-450,64,c.MainPower);
            c.calibrateButton=Control(panel,font,"CALIBRATE",-270,64,c.Calibrate);
            c.manualButton=Control(panel,font,"MANUAL / AUTO",-90,64,c.ToggleAuto);
            c.startButton=Control(panel,font,"START",90,64,c.StartWeb);
            c.stopButton=Control(panel,font,"STOP",270,64,c.StopWeb);
            c.homeButton=Control(panel,font,"HOME",450,64,c.HomeCarriage);
            c.slowButton=Control(panel,font,"30 RPM",-360,17,c.Speed30);
            c.fastButton=Control(panel,font,"60 RPM",-180,17,c.Speed60);
            c.loadButton=Control(panel,font,"SPEED TEST",0,17,c.TestLoad);
            c.guardButton=Control(panel,font,"OPEN GUARD",180,17,c.ToggleGuard);
            c.resetButton=Control(panel,font,"RESET FAULT",360,17,c.ResetFault);
        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(173,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.49f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(173,37),Vector2.zero);
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_48_Layout/Level_48_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Registration Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_49" || targets.arraySize!=SparkLevel49Shear.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel49Shear.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_49"),("displayName","Cut on the Mark"),
                ("description","Synchronize the flying carriage to the moving web. Verify cut registration, reject out-of-sync strokes and recover from a guard trip."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level49_registration_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from solved wiring, quadrature position and verified print history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel49Shear.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_49.asset");
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
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel49Shear.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel49Shear.Gates[i];
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
            Set(terminal,("owner",owner),("kind",kind),("polarity",polarity),("maxConnections",3));
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
            text.characterSize=.022f; text.fontSize=49; text.color=Color.white;
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
            string path=$"{Folder}/L49 {name}.mat";
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




