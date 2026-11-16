#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ProjectSpark.Gameplay;
using ProjectSpark.Visuals;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel87Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_87_Layout";
        private const string PrefabPath = Folder + "/Level_87_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the Gravity Control Network","Commission the Magnetic Levitation System","Accelerate the Artificial Gravity Ring","Maintain Stable Artificial Gravity","Recover Three Critical Rotation Failures","Restore Critical Magnetic Bearing Feedback"
        };

        [MenuItem("Tools/Project Spark/Level 87/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 87 prefab already exists at " + PrefabPath);
                SparkLevel87DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_87.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_87.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.71f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.71f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.71f,.16f));

            var root = new GameObject("Level_87_Layout");
            try
            {
                var bench = Child(root.transform,"Gravity Engine Arena",Vector3.zero);
                AlignBench(root.transform,bench);
                var pins = new List<SparkTerminal>();
                SparkPowerSupply Supply(string name, float voltage, float current)
                {
                    var part=Part<SparkPowerSupply>(bench,name,0,0,steel);
                    Set(part,("outputVoltage",voltage),("currentLimit",current),("state",SparkPowerSupplyState.Active));
                    return part;
                }


                var source=Supply("CONTROL_POWER",5f,.71f);
                var internals=new List<SparkTerminal>();
                var positive=Pin(source,"SUPPLY_POS",0,0,red,internals,SparkTerminalPolarity.Positive);
                var ground=Pin(source,"SUPPLY_RET",0,0,blue,internals,SparkTerminalPolarity.Negative);
                Set(source,("positiveTerminal",positive),("negativeTerminal",ground));
                foreach(var terminal in internals)
                {
                    terminal.GetComponent<Renderer>().enabled=false;
                    terminal.GetComponent<Collider>().enabled=false;
                }
                var carrier=Part<SparkResistor>(bench,"Gravity data connectors",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                var signal=Supply("Trusted primary radial-gap carrier",0f,.01f);
                Set(signal,("state",SparkPowerSupplyState.Off));
                var secondary=Supply("Independent radial-gap diagnostic carrier",0f,.01f);
                Set(secondary,("state",SparkPowerSupplyState.Off));
                var trip=Child(bench,"Emergency stop NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                var links=SparkLevel87Wiring.Wires;
                var controlLoads=new HashSet<int>();
                foreach(var i in SparkLevel87Wiring.Power)controlLoads.Add(links[i].b);
                foreach(int i in new[]{122,123,134,136})controlLoads.Add(links[i].b);
                for(int i=0;i<SparkLevel87Wiring.Pins.Length;i++)
                {
                    string id=SparkLevel87Wiring.Pins[i];
                    bool input=i%2==1;
                    var owner=controlLoads.Contains(i)?Part<SparkResistor>(bench,id+" receiver",0,0,steel)
                        : i==links[136].a?(SparkComponent)signal:i==links[134].a?secondary:carrier;
                    var pin=Pin(owner,id,0,0,controlLoads.Contains(i)?red:cyan,pins);
                    Set(pin,("providesOutput",!input),("acceptsInput",input));
                    if(owner is SparkResistor resistor&&resistor!=carrier)
                        Set(resistor,("terminalA",pin),("terminalB",ground),("resistance",2200f));
                }
                foreach(int i in SparkLevel87Wiring.Power)
                {
                    var tap=Child(bench,"Internal 5 V tap "+i,Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                    Set(tap,("inputTerminal",positive),("outputTerminal",pins[links[i].a]),("state",SparkSwitchState.Closed));
                }
                Set(signal,("positiveTerminal",pins[links[136].a]),("negativeTerminal",ground));
                Set(secondary,("positiveTerminal",pins[links[134].a]),("negativeTerminal",ground));
                Set(trip,("inputTerminal",positive),("outputTerminal",pins[links[122].a]),("state",SparkSwitchState.Closed));
                var permit=Child(bench,"Independent isolation relay NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(permit,("inputTerminal",positive),("outputTerminal",pins[links[123].a]),("state",SparkSwitchState.Closed));
                var contacts=new[]{permit};
                var emergency=trip;
                var ordered=pins.ToArray();
                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel87Process>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.transducer=signal;
                controller.secondary=secondary;
                controller.ground=ground;
                controller.pins=ordered;

                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 87 prefab.");
                SparkLevel87DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 87 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_87_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel87Process c,Font font)
        {
            var obj=new GameObject("Level 87 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,255),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;




            c.statusText=UIText(panel,"Status","Wire the gravity control network.",font,new Vector2(1080,28),new Vector2(0,251));
            c.meterText=UIText(panel,"Measurements","MAGNETIC BEARINGS AND ROTOR SYSTEMS",font,new Vector2(1080,28),new Vector2(0,223));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,195));
            c.detailText.fontSize=16;
            c.eventText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,169));
            c.eventText.fontSize=16;


            (string label,UnityAction run)[] actions={
                ("5 V CONTROL",c.Power),("SAFETY TEST",c.Safety),("CALIBRATE GAP",c.Calibrate),("LEVITATE",c.Levitate),
                ("MOTOR PRECHARGE",c.Precharge),("LOW-SPEED TRIAL",c.LowSpeed),("RESONANCE SCAN",c.Scan),("BALANCE A/B",c.Balance),
                ("SPIN UP TO 3 RPM",c.Spin),("HABITAT LOADS",c.Loads),("CONTROLLED STOP",c.Rundown),("SENSOR DRIFT",c.DriftFault),
                ("AMPLIFIER HEAT",c.HeatFault),("MASS IMBALANCE",c.ImbalanceFault),("DIAGNOSE",c.Diagnose),("REPAIR",c.Repair),
                ("ARM W137 TEST",c.Arm),("RECAL / CHECK",c.CheckSafe),("RESET SAFETY",c.ResetSafety),("E-STOP",c.Stop),
                ("ARENA VIEW",c.ArenaView),("ROTOR VIEW",c.RotorView),("CABLE VIEW",c.CableView),("SOCKET VIEW",c.SocketView),("SHOWCASE VIEW",c.ShowcaseView)
            };
            c.buttons=new Button[actions.Length];
            for(int i=0;i<actions.Length;i++)
                c.buttons[i]=Control(panel,font,actions[i].label,-476f+119f*(i%9),126f-39f*(i/9),actions[i].run);
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
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_86_Layout/Level_86_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Orbital Service Arena") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_87" || targets.arraySize!=SparkLevel87Process.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel87Process.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_87"),("displayName","The Gravity Engine"),
                ("description","Commission eight active bearings, spin up the habitat ring, sustain its essential loads, and recover trusted radial-gap feedback."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level87_gravity_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel87Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_87.asset");
            return level;
        }

        private static void WriteBindings(SparkLevelSceneBindings bindings,List<SparkTerminal> pins,SparkPowerSupply source, SparkSwitch[] contacts, SparkSwitch emergency)
        {
            var so=new SerializedObject(bindings);
            var terminals=so.FindProperty("terminals"); terminals.arraySize=pins.Count+2;
            for(int i=0;i<pins.Count;i++)
            {
                var row=terminals.GetArrayElementAtIndex(i);
                row.FindPropertyRelative("id").stringValue=pins[i].name;
                row.FindPropertyRelative("terminal").objectReferenceValue=pins[i];
            }
            var plus=terminals.GetArrayElementAtIndex(pins.Count+1);
            plus.FindPropertyRelative("id").stringValue=source.PositiveTerminal.name;
            plus.FindPropertyRelative("terminal").objectReferenceValue=source.PositiveTerminal;
            var ret=source.NegativeTerminal;
            var ground=terminals.GetArrayElementAtIndex(pins.Count);
            ground.FindPropertyRelative("id").stringValue=ret.name;
            ground.FindPropertyRelative("terminal").objectReferenceValue=ret;
            var fixedComponents=new List<(string id,SparkElectricalComponent component)> {
                ("CONTROL_POWER",source),("ESTOP_INTERLOCK",emergency)
            };
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L87_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel87Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel87Process.Gates[i];
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
            text.characterSize=.022f; text.fontSize=76; text.color=Color.white;
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
            string path=$"{Folder}/L87 {name}.mat";
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







