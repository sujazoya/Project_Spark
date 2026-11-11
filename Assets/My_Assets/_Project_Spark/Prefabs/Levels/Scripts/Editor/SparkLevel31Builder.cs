#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel31Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        private const string Folder = Parent + "/Level_31_Layout";
        private const string PrefabPath = Folder + "/Level_31_SealIn_Layout.prefab";
        private static readonly string[] Objectives = {
            "Power on with conveyor stopped", "START holds after release", "STOP stays effective after release",
            "Relay drops out on power loss", "Power restoration leaves conveyor stopped", "Conveyor restarts on a new START command"
        };

        [MenuItem("Tools/Project Spark/Level 31/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!AssetDatabase.IsValidFolder(Parent)) { Debug.LogError("Project Spark Prefabs/Levels folder is missing."); return; }
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null) { Debug.LogError("Level_31_SealIn_Layout.prefab already exists."); return; }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) { Debug.LogError("Missing lit shader or Unity's built-in font."); return; }
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent, "Level_31_Layout");

            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_31.asset");
            if (level != null && !MatchesLevel(level))
            { Debug.LogError("The existing Level_31.asset has different objectives. Import the supplied Level 31 definition first.", level); return; }
            if (level == null) level = CreateDefinition();

            var dark = MaterialAsset(shader, "Bench", new Color(.07f,.12f,.17f));
            var steel = MaterialAsset(shader, "Modules", new Color(.24f,.35f,.44f));
            var metal = MaterialAsset(shader, "Machined metal", new Color(.53f,.59f,.63f));
            var gold = MaterialAsset(shader, "Coil and guards", new Color(.87f,.53f,.16f));
            var blue = MaterialAsset(shader, "Sockets", new Color(.15f,.65f,.95f));
            var red = MaterialAsset(shader, "Positive", new Color(.90f,.23f,.30f));
            var green = MaterialAsset(shader, "Run indicator", new Color(.14f,.53f,.30f));

            var root = new GameObject("Level_31_SealIn_Layout");
            try
            {
                var bench = Child(root.transform, "Relay Workbench", Vector3.zero);
                AlignBench(root.transform, bench);
                Solid(bench,"Board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.55f),dark);
                var pins = new List<SparkTerminal>();
                var supply = Part<SparkPowerSupply>(bench, "PowerSupply_01", -.87f, .51f, steel);
                Set(supply,("positiveTerminal",Pin(supply,"Battery_positive",-.07f,0,red,pins,SparkTerminalPolarity.Positive)),
                    ("negativeTerminal",Pin(supply,"Battery_negative",.07f,0,blue,pins,SparkTerminalPolarity.Negative)),
                    ("outputVoltage",5f),("currentLimit",.35f),("state",SparkPowerSupplyState.Active));
                Label(supply.transform,"5 V SUPPLY",font);
                var main = Contact(bench,"MAIN","MAIN",-.53f,.51f,false,steel,blue,font,pins);
                var stop = Contact(bench,"STOP (NC)","STOP",-.20f,.51f,true,steel,blue,font,pins);
                var start = Contact(bench,"START (NO)","START",.15f,.51f,false,steel,blue,font,pins);
                var drive = Contact(bench,"MOTOR CONTACT","RUN",-.45f,-.27f,false,steel,blue,font,pins);

                var relay = Part<SparkRelay>(bench,"Holding Relay",.24f,.06f,steel);
                Solid(relay.transform,"Relay face",PrimitiveType.Cube,Vector3.zero,new Vector3(.40f,.10f,.25f),steel);
                Set(relay,("coilPositiveTerminal",Pin(relay,"Coil_plus",-.15f,.075f,blue,pins)),
                    ("coilNegativeTerminal",Pin(relay,"Coil_minus",.15f,.075f,blue,pins)),
                    ("commonTerminal",Pin(relay,"Hold_COM",-.15f,-.075f,blue,pins)),
                    ("normallyOpenTerminal",Pin(relay,"Hold_NO",.00f,-.075f,blue,pins)),
                    ("normallyClosedTerminal",Pin(relay,"Hold_NC_spare",.15f,-.075f,blue,pins)),
                    ("pickupVoltage",3.8f),("energized",false));
                Label(relay.transform,"HOLD RELAY",font,-.16f);
                Label(relay.transform,"COIL +                      COIL -",font,.13f);
                Label(relay.transform,"COM        NO        NC SPARE",font,-.118f);
                var coil = Solid(relay.transform,"Copper coil",PrimitiveType.Cylinder,new Vector3(-.048f,.081f,0),new Vector3(.069f,.046f,.069f),gold);
                coil.transform.localRotation = Quaternion.Euler(0,0,90);
                var arm = Child(relay.transform,"Moving armature",new Vector3(.016f,.09f,.012f));
                Solid(arm,"Armature",PrimitiveType.Cube,new Vector3(.038f,0,0),new Vector3(.081f,.018f,.027f),metal);

                var motor = Part<SparkMotor>(bench,"Conveyor Motor",-.06f,-.27f,steel);
                Set(motor,("positiveTerminal",Pin(motor,"Motor_A",-.07f,0,blue,pins)),
                    ("negativeTerminal",Pin(motor,"Motor_B",.07f,0,blue,pins)),
                    ("windingResistance",33f),("startupVoltage",3f),("maximumCurrent",.25f));
                Label(motor.transform,"MOTOR A / B",font);
                var motorCase = Solid(motor.transform,"Motor housing",PrimitiveType.Cylinder,new Vector3(.006f,.10f,.085f),new Vector3(.11f,.09f,.11f),metal);
                motorCase.transform.localRotation=Quaternion.Euler(0,0,90);
                var shaft = Child(motor.transform,"Rotating shaft",new Vector3(.114f,.10f,.085f));
                var shaftMesh = Solid(shaft,"Shaft",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.025f,.043f,.025f),metal);
                shaftMesh.transform.localRotation=Quaternion.Euler(0,0,90);
                Solid(shaft,"Rotation marker",PrimitiveType.Cube,new Vector3(0,.019f,0),new Vector3(.05f,.008f,.012f),gold);

                var conveyor = Child(bench,"Conveyor belt",new Vector3(.59f,.06f,-.44f));
                Solid(conveyor,"Belt base",PrimitiveType.Cube,Vector3.zero,new Vector3(.60f,.10f,.29f),dark);
                foreach (float side in new[] {-.16f,.16f})
                    Solid(conveyor,"Guard rail",PrimitiveType.Cube,new Vector3(0,.058f,side),new Vector3(.62f,.047f,.022f),gold);
                var slats = new Transform[10];
                for (int i=0;i<slats.Length;i++)
                    slats[i]=Solid(conveyor,"Moving belt slat",PrimitiveType.Cube,new Vector3(i*.52f/slats.Length-.26f,.064f,0),
                        new Vector3(.043f,.018f,.266f),steel).transform;
                var beacon = Solid(conveyor,"RUN beacon",PrimitiveType.Sphere,new Vector3(.25f,.15f,.20f),Vector3.one*.064f,green);
                var light = beacon.AddComponent<Light>();
                light.type=LightType.Point; light.color=new Color(.23f,1,.41f); light.range=.36f; light.intensity=1.8f; light.enabled=false;
                Label(conveyor,"CONVEYOR",font,-.21f);

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,supply);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel31SealIn>();
                controller.layout=layout; controller.source=supply; controller.main=main; controller.stop=stop;
                controller.start=start; controller.runContact=drive; controller.relay=relay; controller.motor=motor;
                controller.shaft=shaft; controller.relayArm=arm; controller.beltSlats=slats; controller.runLight=light;
                BuildControls(root.transform,controller,font);
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if (prefab == null) throw new InvalidOperationException("Could not save the Level 31 prefab.");
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 31 assets created. Register Level_31.asset and Level_31_SealIn_Layout.prefab in the active manager and layout catalog.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_31" || targets.arraySize!=SparkLevel31SealIn.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel31SealIn.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_31"),("displayName","Loop Connection - START, STOP, and Safe Restart"),
                ("description","Wire a relay holding loop and motor branch. Prove latching, stop, power loss, safe restoration, and deliberate restart."),
                ("completionMode",0),("requiredTargetCount",Objectives.Length),("requireClosedReturn",false),
                ("rejectShortCircuit",true),("rejectTargetShort",true),("requireMinimumVoltage",false),
                ("autoAdvanceOnCompletion",false));
            var so=new SerializedObject(level);
            var sources=so.FindProperty("powerSources"); sources.arraySize=1;
            var source=sources.GetArrayElementAtIndex(0);
            source.FindPropertyRelative("sourceName").stringValue="PowerSupply_01";
            source.FindPropertyRelative("positiveTerminalId").stringValue="Battery_positive";
            source.FindPropertyRelative("negativeTerminalId").stringValue="Battery_negative";
            source.FindPropertyRelative("nominalVoltage").floatValue=5f;
            var targets=so.FindProperty("targets"); targets.arraySize=Objectives.Length;
            for (int i=0;i<Objectives.Length;i++)
            {
                var target=targets.GetArrayElementAtIndex(i);
                target.FindPropertyRelative("targetId").stringValue=$"level31_seal_in_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded by the Level 31 controller from solved relay and motor behavior.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel31SealIn.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_31.asset");
            return level;
        }

        private static void WriteBindings(SparkLevelSceneBindings bindings,List<SparkTerminal> pins,SparkPowerSupply supply)
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
            power.FindPropertyRelative("id").stringValue="PowerSupply_01";
            power.FindPropertyRelative("component").objectReferenceValue=supply;
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel31SealIn.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel31SealIn.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SparkSwitch Contact(Transform parent,string name,string id,float x,float z,bool closed,Material body,Material pin,Font font,List<SparkTerminal> pins)
        {
            var contact=Part<SparkSwitch>(parent,name,x,z,body);
            Set(contact,("inputTerminal",Pin(contact,id+"_IN",-.07f,0,pin,pins)),
                ("outputTerminal",Pin(contact,id+"_OUT",.07f,0,pin,pins)),
                ("state",closed ? SparkSwitchState.Closed : SparkSwitchState.Open));
            Label(contact.transform,name,font);
            return contact;
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

        private static void BuildControls(Transform parent,SparkLevel31SealIn controller,Font font)
        {
            var obj=new GameObject("Level 31 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(900,110),new Vector2(0,125));
            panel.gameObject.AddComponent<Image>().color=new Color(.035f,.08f,.13f,.94f);
            controller.statusText=UIText(panel,"Status","Wire the relay holding and motor paths.",font,new Vector2(870,45),new Vector2(0,56));
            var main=Rect(panel,"MAIN",new Vector2(136,38),new Vector2(-170,10));
            controller.mainIndicator=main.gameObject.AddComponent<Image>();
            var toggle=main.gameObject.AddComponent<Button>(); toggle.targetGraphic=controller.mainIndicator;
            UnityEventTools.AddPersistentListener(toggle.onClick,controller.ToggleMain);
            UIText(main,"Label","MAIN",font,new Vector2(136,38),Vector2.zero);
            for (int i=0;i<2;i++)
            {
                bool isStop=i==1;
                string caption=isStop ? "STOP" : "START";
                var button=Rect(panel,caption,new Vector2(136,38),new Vector2(i*170,10));
                var image=button.gameObject.AddComponent<Image>();
                image.color=isStop ? new Color(.64f,.19f,.22f) : new Color(.13f,.43f,.32f);
                var input=button.gameObject.AddComponent<SparkLevel31MomentaryButton>();
                input.controller=controller; input.isStop=isStop; input.face=image;
                UIText(button,"Label",caption,font,new Vector2(136,38),Vector2.zero);
            }
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
            text.characterSize=.022f; text.fontSize=42; text.color=Color.white;
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
            string path=$"{Folder}/L31 {name}.mat";
            var existing=AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing!=null) return existing;
            var material=new Material(shader) {name=name,color=color};
            AssetDatabase.CreateAsset(material,path);
            return material;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_30_Layout/Level_30_Lift_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Lift Workbench") : null;
            if (anchor!=null)
            {
                CopyLocalTransform(root,previous.transform);
                CopyLocalTransform(bench,anchor);
                return;
            }
            previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_29_Layout/Level_29_Layout.prefab");
            if (previous==null) return;
            var bindings=previous.GetComponent<SparkLevelSceneBindings>();
            if (bindings==null || !bindings.TryGetTerminal("Battery_positive",out var left) ||
                !bindings.TryGetTerminal("LED_Anode",out var right)) return;
            Vector3 a=previous.transform.InverseTransformPoint(left.transform.position);
            Vector3 b=previous.transform.InverseTransformPoint(right.transform.position);
            Vector3 across=Vector3.ProjectOnPlane(b-a,Vector3.up);
            if (across.sqrMagnitude<.001f) return;
            CopyLocalTransform(root,previous.transform);
            float scale=across.magnitude/1.85f;
            bench.localScale=Vector3.one*scale;
            bench.localPosition=(a+b)*.5f-Vector3.up*(.12f*scale);
            bench.localRotation=Quaternion.LookRotation(Vector3.Cross(across.normalized,Vector3.up),Vector3.up);
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
