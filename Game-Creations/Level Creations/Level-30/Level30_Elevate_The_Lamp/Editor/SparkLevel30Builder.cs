#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel30Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        private const string Folder = Parent + "/Level_30_Layout";
        private const string MotorAsset = Folder + "/Level30_MotorSolver.asset";
        private static readonly string[] Titles = {
            "Lower limit blocks DOWN", "Motor raises lift", "Upper limit stops motor and lamp lights",
            "Motor reverses and lamp goes out", "Lower limit stops motor after round trip"
        };

        [MenuItem("Tools/Project Spark/Level 30/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!AssetDatabase.IsValidFolder(Parent))
            {
                Debug.LogError("Level 30 builder requires the Project Spark Prefabs/Levels folder.");
                return;
            }
            if (AssetDatabase.IsValidFolder(Folder))
            {
                Debug.LogError("Level_30_Layout already exists. Move or rename that folder before generating a new copy.");
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) { Debug.LogError("Missing a lit shader or Unity's built-in font."); return; }

            AssetDatabase.CreateFolder(Parent, "Level_30_Layout");
            var definition = CreateDefinition();
            var motorDefinition = ScriptableObject.CreateInstance<SparkLevel30MotorSolverDefinition>();
            Set(motorDefinition, ("solverId", "Level30ReversibleMotor"));
            AssetDatabase.CreateAsset(motorDefinition, MotorAsset);

            var dark = MaterialAsset(shader, "Base", new Color(.07f,.12f,.17f));
            var steel = MaterialAsset(shader, "Contacts", new Color(.23f,.35f,.44f));
            var amber = MaterialAsset(shader, "Lift", new Color(.93f,.53f,.12f));
            var blue = MaterialAsset(shader, "Sockets", new Color(.15f,.65f,.95f));
            var red = MaterialAsset(shader, "Positive", new Color(.93f,.23f,.30f));
            var root = new GameObject("Level_30_Layout");
            try
            {
                var board = Child(root.transform, "Lift Workbench", Vector3.zero);
                AlignWithExistingBench(root.transform, board.transform);
                var t = board.transform;
                Solid(t, "Board", PrimitiveType.Cube, new Vector3(0,-.055f,0), new Vector3(2.15f,.10f,1.55f), dark);

                var terminals = new List<SparkTerminal>();
                var power = Part<SparkPowerSupply>(t, "PowerSupply_01", -.88f, .50f, steel);
                var positive = Socket(power, "Battery_positive", -.06f, red, terminals, SparkTerminalPolarity.Positive);
                var negative = Socket(power, "Battery_negative", .06f, blue, terminals, SparkTerminalPolarity.Negative);
                Set(power, ("positiveTerminal", positive), ("negativeTerminal", negative),
                    ("outputVoltage", 5f), ("currentLimit", .35f), ("state", SparkPowerSupplyState.Active));
                Label(power.transform, "5 V SUPPLY", font);

                var upFeed = Contact(t, "UP Feed", "UP_Feed", -.55f, .50f, steel, blue, font, terminals);
                var upReturn = Contact(t, "UP Return", "UP_Return", -.55f, .22f, steel, blue, font, terminals);
                var downFeed = Contact(t, "DOWN Feed", "DOWN_Feed", -.55f, -.07f, steel, blue, font, terminals);
                var downReturn = Contact(t, "DOWN Return", "DOWN_Return", -.55f, -.36f, steel, blue, font, terminals);
                var upper = Contact(t, "Upper Limit", "Upper_Limit", -.18f, .50f, steel, blue, font, terminals, true);
                var lower = Contact(t, "Lower Limit", "Lower_Limit", -.18f, -.07f, steel, blue, font, terminals);
                var indicator = Contact(t, "Top Lamp Contact", "Top_Lamp", -.18f, -.36f, steel, blue, font, terminals);

                var motor = Part<SparkMotor>(t, "Lift Motor", .19f, .35f, amber);
                Set(motor, ("positiveTerminal", Socket(motor,"Motor_A",-.07f,blue,terminals)),
                    ("negativeTerminal",Socket(motor,"Motor_B",.07f,blue,terminals)),
                    ("windingResistance",33f),("startupVoltage",3f),("maximumCurrent",.25f));
                Label(motor.transform, "MOTOR A / B", font);

                var resistor = Part<SparkResistor>(t, "LED Resistor", .19f, -.36f, amber);
                Set(resistor, ("terminalA", Socket(resistor,"R_LED_IN",-.07f,blue,terminals)),
                    ("terminalB",Socket(resistor,"R_LED_OUT",.07f,blue,terminals)), ("resistance",330f));
                Label(resistor.transform, "330 OHM", font);
                var lamp = Part<SparkLED>(t, "Top Lamp", .51f, -.36f, amber);
                Set(lamp, ("anodeTerminal",Socket(lamp,"Lamp_A",-.07f,blue,terminals)),
                    ("cathodeTerminal",Socket(lamp,"Lamp_K",.07f,blue,terminals)), ("forwardVoltage",2f));
                Label(lamp.transform, "LAMP A / K", font);

                // The indicator is wired at the bench. Its visible bulb rides on the platform.
                Solid(t, "Lift pedestal", PrimitiveType.Cube, new Vector3(.72f,.04f,.29f), new Vector3(.42f,.08f,.42f),steel);
                foreach (float x in new[] { .54f, .90f })
                    Solid(t, "Guide rail", PrimitiveType.Cube, new Vector3(x,.34f,.40f),new Vector3(.035f,.68f,.035f),steel);
                var platform = Child(t,"Moving platform", new Vector3(.72f,.12f,.29f));
                Solid(platform.transform,"Platform",PrimitiveType.Cube,Vector3.zero,new Vector3(.34f,.06f,.34f),amber);
                var bulb = Solid(platform.transform,"Bulb",PrimitiveType.Sphere,new Vector3(0,.11f,0),Vector3.one*.14f,amber);
                var light = bulb.AddComponent<Light>();
                light.type = LightType.Point; light.range = .5f; light.intensity = 1.6f; light.color = new Color(1f,.82f,.27f); light.enabled = false;

                var bindings = root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings, terminals, power);
                var layout = root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",definition),("sceneBindings",bindings));
                var controller = root.AddComponent<SparkLevel30Lift>();
                Text status = BuildControls(root.transform, controller, font);
                Set(controller,("layout",layout),("source",power),("motor",motor),
                    ("upFeed",upFeed),("upReturn",upReturn),("downFeed",downFeed),("downReturn",downReturn),
                    ("upperLimit",upper),("lowerLimit",lower),("lampContact",indicator),
                    ("lampResistor",resistor),("lamp",lamp),("platform",platform.transform),("lampLight",light),("statusText",status));
                Selection.activeObject = PrefabUtility.SaveAsPrefabAsset(root, Folder + "/Level_30_Layout.prefab");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 30 assets created. Add the motor definition to the active solver registry, then register Level_30 and its layout in the manager and catalog.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        [MenuItem("Tools/Project Spark/Level 30/Register Motor Solver in Selected Registry")]
        private static void RegisterMotor()
        {
            var registry = Selection.activeObject as SparkElectricalSolverRegistry;
            var motor = AssetDatabase.LoadAssetAtPath<SparkLevel30MotorSolverDefinition>(MotorAsset);
            if (EditorApplication.isPlayingOrWillChangePlaymode || registry == null || motor == null)
            { Debug.LogError("Generate Level 30, exit Play Mode, then select the registry used by SparkElectricalSolver."); return; }
            foreach (var existing in registry.Definitions)
            {
                if (existing == motor) return;
                if (existing != null && existing.SupportedComponentType == typeof(SparkMotor))
                { Debug.LogError("This registry already has a SparkMotor definition. Review it before adding a second."); return; }
            }
            Undo.RecordObject(registry,"Register Level 30 motor solver");
            var so = new SerializedObject(registry);
            var items = so.FindProperty("definitions");
            int index = items.arraySize;
            items.arraySize = index + 1;
            items.GetArrayElementAtIndex(index).objectReferenceValue = motor;
            so.ApplyModifiedProperties();
            registry.ClearRuntimeCache();
            AssetDatabase.SaveAssets();
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level = ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_30"),("displayName","Elevate the Lamp - Motor Control and Limit Switches"),
                ("description","Wire a reversible lift. Verify both end stops, the top-only lamp, and a complete UP/DOWN round trip."),
                ("completionMode",0),("requiredTargetCount",Titles.Length),("requireClosedReturn",false),
                ("rejectShortCircuit",true),("rejectTargetShort",true),("requireMinimumVoltage",false),
                ("autoAdvanceOnCompletion",false));
            var so = new SerializedObject(level);
            var supply = so.FindProperty("powerSources"); supply.arraySize = 1;
            var p = supply.GetArrayElementAtIndex(0);
            p.FindPropertyRelative("sourceName").stringValue="PowerSupply_01";
            p.FindPropertyRelative("positiveTerminalId").stringValue="Battery_positive";
            p.FindPropertyRelative("negativeTerminalId").stringValue="Battery_negative";
            p.FindPropertyRelative("nominalVoltage").floatValue=5f;
            var targets = so.FindProperty("targets"); targets.arraySize = Titles.Length;
            for(int i=0;i<Titles.Length;i++)
            {
                var target=targets.GetArrayElementAtIndex(i);
                target.FindPropertyRelative("targetId").stringValue=$"level30_lift_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Titles[i];
                target.FindPropertyRelative("description").stringValue="Recorded by the lift controller from solved electrical behavior.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel30Lift.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_30.asset");
            return level;
        }

        private static void WriteBindings(SparkLevelSceneBindings bindings, List<SparkTerminal> terminals, SparkPowerSupply power)
        {
            var so=new SerializedObject(bindings);
            var list=so.FindProperty("terminals"); list.arraySize=terminals.Count;
            for(int i=0;i<terminals.Count;i++)
            {
                var row=list.GetArrayElementAtIndex(i);
                row.FindPropertyRelative("id").stringValue=terminals[i].name;
                row.FindPropertyRelative("terminal").objectReferenceValue=terminals[i];
            }
            var components=so.FindProperty("components"); components.arraySize=1;
            var entry=components.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("id").stringValue="PowerSupply_01";
            entry.FindPropertyRelative("component").objectReferenceValue=power;
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel30Lift.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel30Lift.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SparkSwitch Contact(Transform parent,string name,string id,float x,float z,Material body,
            Material pin,Font font,List<SparkTerminal> pins,bool closed=false)
        {
            var contact=Part<SparkSwitch>(parent,name,x,z,body);
            Set(contact,("inputTerminal",Socket(contact,id+"_IN",-.07f,pin,pins)),
                ("outputTerminal",Socket(contact,id+"_OUT",.07f,pin,pins)),
                ("state",closed ? SparkSwitchState.Closed : SparkSwitchState.Open));
            Label(contact.transform,name.ToUpperInvariant(),font);
            return contact;
        }

        private static T Part<T>(Transform parent,string name,float x,float z,Material body) where T:SparkElectricalComponent
        {
            var go=Child(parent,name,new Vector3(x,.055f,z));
            Solid(go.transform,"Body",PrimitiveType.Cube,Vector3.zero,new Vector3(.22f,.09f,.13f),body);
            var part=go.AddComponent<T>();
            Set(part,("objectId",name.Replace(' ','_')),("displayName",name));
            return part;
        }

        private static SparkTerminal Socket(SparkElectricalComponent owner,string id,float x,Material material,
            List<SparkTerminal> pins,SparkTerminalPolarity polarity=SparkTerminalPolarity.None)
        {
            var socket=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            socket.name=id; socket.transform.SetParent(owner.transform,false);
            socket.transform.localPosition=new Vector3(x,.065f,0f);
            socket.transform.localScale=new Vector3(.077f,.018f,.077f);
            socket.GetComponent<Renderer>().sharedMaterial=material;
            int pinLayer=LayerMask.NameToLayer("Pin");
            if(pinLayer>=0) socket.layer=pinLayer;
            var terminal=socket.AddComponent<SparkTerminal>();
            var kind = polarity == SparkTerminalPolarity.Positive ? SparkTerminalKind.Power :
                polarity == SparkTerminalPolarity.Negative ? SparkTerminalKind.Ground : SparkTerminalKind.Generic;
            Set(terminal,("owner",owner),("kind",kind),("polarity",polarity),("maxConnections",3));
            pins.Add(terminal);
            return terminal;
        }

        private static void Label(Transform parent,string value,Font font)
        {
            var obj=Child(parent,"Label",new Vector3(0,.015f,-.092f));
            obj.transform.localRotation=Quaternion.Euler(90,0,0);
            var text=obj.AddComponent<TextMesh>();
            text.text=value; text.font=font; text.anchor=TextAnchor.MiddleCenter;
            text.characterSize=.022f; text.fontSize=44; text.color=Color.white;
            obj.GetComponent<Renderer>().sharedMaterial=font.material;
        }

        private static Text BuildControls(Transform parent,SparkLevel30Lift controller,Font font)
        {
            var canvasObject=new GameObject("Level 30 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent,false);
            var canvas=canvasObject.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=UIRect(canvas.transform,"Panel",new Vector2(760,95),new Vector2(0,125));
            panel.gameObject.AddComponent<Image>().color=new Color(.035f,.08f,.13f,.94f);
            var status=UIText(panel,"Status","Wire the Level 30 lift using the connection list.",font,new Vector2(730,42),new Vector2(0,45));
            var actions=new UnityEngine.Events.UnityAction[]{controller.SelectUp,controller.SelectOff,controller.SelectDown};
            string[] labels={"UP","OFF","DOWN"};
            for(int i=0;i<labels.Length;i++)
            {
                var rect=UIRect(panel,labels[i],new Vector2(115,34),new Vector2((i-1)*130,7));
                var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.13f,.34f,.49f);
                var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image;
                UnityEventTools.AddPersistentListener(button.onClick,actions[i]);
                UIText(rect,"Label",labels[i],font,new Vector2(115,34),Vector2.zero);
            }
            return status;
        }

        private static RectTransform UIRect(Transform parent,string name,Vector2 size,Vector2 offset)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent,false); rect.anchorMin=rect.anchorMax=new Vector2(.5f,0f); rect.pivot=new Vector2(.5f,0f);
            rect.sizeDelta=size; rect.anchoredPosition=offset;
            return rect;
        }

        private static Text UIText(Transform parent,string name,string value,Font font,Vector2 size,Vector2 offset)
        {
            var text=UIRect(parent,name,size,offset).gameObject.AddComponent<Text>();
            text.text=value; text.font=font; text.fontSize=18; text.alignment=TextAnchor.MiddleCenter;
            text.color=Color.white; text.raycastTarget=false;
            return text;
        }

        private static GameObject Child(Transform parent,string name,Vector3 position)
        {
            var go=new GameObject(name); go.transform.SetParent(parent,false); go.transform.localPosition=position;
            return go;
        }

        private static GameObject Solid(Transform parent,string name,PrimitiveType kind,Vector3 position,Vector3 size,Material material)
        {
            var go=GameObject.CreatePrimitive(kind); go.name=name;
            go.transform.SetParent(parent,false); go.transform.localPosition=position; go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=material;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static Material MaterialAsset(Shader shader,string name,Color color)
        {
            var material=new Material(shader){name=name,color=color};
            AssetDatabase.CreateAsset(material,$"{Folder}/{name}.mat");
            return material;
        }

        private static void AlignWithExistingBench(Transform root,Transform board)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_29_Layout/Level_29_Layout.prefab");
            if(previous==null) return;
            var refs=previous.GetComponent<SparkLevelSceneBindings>();
            if(refs==null || !refs.TryGetTerminal("Battery_positive",out var left) ||
                !refs.TryGetTerminal("LED_Anode",out var right)) return;
            root.localPosition=previous.transform.localPosition;
            root.localRotation=previous.transform.localRotation;
            root.localScale=previous.transform.localScale;
            Vector3 a=previous.transform.InverseTransformPoint(left.transform.position);
            Vector3 b=previous.transform.InverseTransformPoint(right.transform.position);
            Vector3 across=Vector3.ProjectOnPlane(b-a,Vector3.up);
            if(across.sqrMagnitude<.001f) return;
            float scale=across.magnitude/1.85f;
            board.localScale=Vector3.one*scale;
            board.localPosition=(a+b)*.5f-Vector3.up*(.12f*scale);
            board.localRotation=Quaternion.LookRotation(Vector3.Cross(across.normalized,Vector3.up),Vector3.up);
        }

        private static void Set(Object target,params (string Field,object Value)[] settings)
        {
            var so=new SerializedObject(target);
            foreach(var entry in settings)
            {
                var p=so.FindProperty(entry.Field);
                if(p==null) throw new InvalidOperationException($"{target.GetType().Name} has no serialized field {entry.Field}.");
                switch(entry.Value)
                {
                    case Object obj: p.objectReferenceValue=obj; break;
                    case string text: p.stringValue=text; break;
                    case bool flag: p.boolValue=flag; break;
                    case float number: p.floatValue=number; break;
                    case Enum choice: p.enumValueIndex=Convert.ToInt32(choice); break;
                    case int integer:
                        if(p.propertyType==SerializedPropertyType.Enum) p.enumValueIndex=integer;
                        else p.intValue=integer;
                        break;
                    default: throw new InvalidOperationException($"Unsupported field value for {entry.Field}.");
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
