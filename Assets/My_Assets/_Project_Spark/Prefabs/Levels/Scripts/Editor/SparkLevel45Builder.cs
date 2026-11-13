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
    public static class SparkLevel45Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_45_Layout";
        private const string PrefabPath = Folder + "/Level_45_Governor_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the speed loop", "Verify encoder feedback", "Hold 30 RPM",
            "Hold 60 RPM", "Recover from load disturbance", "Recover safely from feedback loss"
        };

        [MenuItem("Tools/Project Spark/Level 45/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 45 prefab already exists at " + PrefabPath);
                SparkLevel45DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_45.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_45.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.45f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.62f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.56f,.16f));

            var root = new GameObject("Level_45_Governor_Layout");
            try
            {
                var bench = Child(root.transform,"Governor Workbench",Vector3.zero);
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
                var positive=Pin(source,"SUPPLY_PLUS_5V",0,0,red,pins,SparkTerminalPolarity.Positive);
                var ground=Pin(source,"SUPPLY_RETURN",0,0,blue,pins,SparkTerminalPolarity.Negative);
                Set(source,("positiveTerminal",positive),("negativeTerminal",ground));
                var controllerPower=Part<SparkResistor>(bench,"Controller power",0,0,steel);
                var ctrlPlus=Pin(controllerPower,"CONTROLLER_PLUS_5V",0,0,red,pins);
                var ctrlGround=Pin(controllerPower,"CONTROLLER_GND",0,0,blue,pins);
                Set(controllerPower,("terminalA",ctrlPlus),("terminalB",ctrlGround),("resistance",2200f));
                var driver=Part<SparkSwitch>(bench,"PWM driver enable",0,0,steel);
                var driverPlus=Pin(driver,"DRIVER_PLUS_5V",0,0,red,pins);
                var driverGround=Pin(driver,"DRIVER_GND",0,0,blue,pins);

                SparkPowerSupply Signal(string name,string inputId,Material color,SparkTerminal returnPin)
                {
                    var output=Supply(name,0f,.025f);
                    Set(output,("positiveTerminal",Pin(output,name,0,0,color,pins)),("negativeTerminal",ground));
                    var sense=Part<SparkResistor>(bench,"Input "+inputId,0,0,steel);
                    Set(sense,("terminalA",Pin(sense,inputId,0,0,color,pins)),
                        ("terminalB",returnPin),("resistance",100000f));
                    return output;
                }
                var signalA=Signal("ENCODER_A","CONTROLLER_A",cyan,ctrlGround);
                var signalB=Signal("ENCODER_B","CONTROLLER_B",blue,ctrlGround);
                var pwm=Signal("CONTROLLER_PWM_OUT","DRIVER_PWM_IN",gold,driverGround);
                var impedance=Part<SparkResistor>(bench,"Driver equivalent impedance",0,0,steel);
                var junction=Child(impedance.transform,"Switched rail",Vector3.zero).gameObject.AddComponent<SparkTerminal>();
                Set(junction,("owner",impedance));
                var driverOutput=Pin(impedance,"DRIVER_MOTOR_PLUS",0,0,gold,pins);
                Set(impedance,("terminalA",junction),("terminalB",driverOutput),("resistance",33f),("maximumPowerWatts",2f));
                Set(driver,("inputTerminal",driverPlus),("outputTerminal",junction),("state",SparkSwitchState.Open));
                var motor=Part<SparkMotor>(bench,"Governor motor",0,0,steel);
                var motorPlus=Pin(motor,"MOTOR_PLUS",0,0,gold,pins);
                var driverReturn=Pin(driver,"DRIVER_MOTOR_MINUS",0,0,blue,pins);
                var motorMinus=Pin(motor,"MOTOR_MINUS",0,0,blue,pins);
                Set(motor,("positiveTerminal",motorPlus),("negativeTerminal",motorMinus),("windingResistance",33f),
                    ("startupVoltage",.2f),("maximumCurrent",.165f));
                var returnContact=Child(driver.transform,"Driver return bus",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(returnContact,("inputTerminal",driverReturn),("outputTerminal",driverGround),
                    ("state",SparkSwitchState.Closed),("interactionsEnabled",false));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel45Governor>();
                controller.layout=layout;
                controller.source=source;
                controller.encoderA=signalA;
                controller.encoderB=signalB;
                controller.pwm=pwm;
                controller.motor=motor;
                controller.driveEnable=driver;
                controller.driveImpedance=impedance;
                controller.pins=pins.ToArray();
                controller.rig=root.AddComponent<SparkLevel45VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 45 prefab.");
                SparkLevel45DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 45 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_45_Layout");
        }

        private static void BuildControls(Transform parent,SparkLevel45Governor c,Font font)
        {
            var obj=new GameObject("Level 45 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1100,216),new Vector2(-160,24));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;
            c.statusText=UIText(panel,"Status","Wire the speed governor.",font,new Vector2(1060,28),new Vector2(0,180));
            c.meterText=UIText(panel,"Measurements","TARGET 30 RPM",font,new Vector2(1060,28),new Vector2(0,147));
            c.meterText.color=new Color(.40f,.92f,.93f); c.meterText.fontSize=23;
            c.detailText=UIText(panel,"Details","OFF",font,new Vector2(1060,28),new Vector2(0,113));
            c.openButton=Control(panel,font,"OPEN LOOP",-400,64,c.SelectOpen);
            c.closedButton=Control(panel,font,"CLOSED LOOP",-200,64,c.SelectClosed);
            c.startButton=Control(panel,font,"START",0,64,c.StartStop);
            c.rpm30Button=Control(panel,font,"30 RPM",200,64,c.Select30);
            c.rpm60Button=Control(panel,font,"60 RPM",400,64,c.Select60);
            c.loadButton=Control(panel,font,"BRAKE: RELEASED",-300,17,c.CycleBrake);
            Control(panel,font,"TEST LOAD",-100,17,c.ApplyTestLoad).interactable=true;
            c.resetButton=Control(panel,font,"RESET FAULT",100,17,c.ResetFault);
            c.restartButton=Control(panel,font,"RESTART",300,17,c.Restart);
        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(192,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.48f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(192,37),Vector2.zero);
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_44_Layout/Level_44_Encoder_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Encoder Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_45" || targets.arraySize!=SparkLevel45Governor.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel45Governor.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_45"),("displayName","Hold the Speed"),
                ("description","Build a closed-loop motor governor and maintain RPM under changing brake load."),
                ("completionMode",0),("requiredTargetCount",Objectives.Length),("requireClosedReturn",false),
                ("rejectShortCircuit",true),("rejectTargetShort",true),("requireMinimumVoltage",false),
                ("autoAdvanceOnCompletion",false));
            var so=new SerializedObject(level);
            var sources=so.FindProperty("powerSources"); sources.arraySize=1;
            var source=sources.GetArrayElementAtIndex(0);
            source.FindPropertyRelative("sourceName").stringValue="CONTROL_POWER";
            source.FindPropertyRelative("positiveTerminalId").stringValue="SUPPLY_PLUS_5V";
            source.FindPropertyRelative("negativeTerminalId").stringValue="SUPPLY_RETURN";
            source.FindPropertyRelative("nominalVoltage").floatValue=5f;
            var targets=so.FindProperty("targets"); targets.arraySize=Objectives.Length;
            for (int i=0;i<Objectives.Length;i++)
            {
                var target=targets.GetArrayElementAtIndex(i);
                target.FindPropertyRelative("targetId").stringValue=$"level45_governor_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from solved wiring, motor current and measured encoder feedback.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel45Governor.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_45.asset");
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
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel45Governor.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel45Governor.Gates[i];
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
            text.characterSize=.022f; text.fontSize=45; text.color=Color.white;
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
            string path=$"{Folder}/L45 {name}.mat";
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




