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
    public static class SparkLevel35Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_35_Layout";
        private const string PrefabPath = Folder + "/Level_35_Bridge_Balance_Layout.prefab";
        private static readonly string[] Objectives = {
            "Wire and power both bridge branches", "Capture the balanced baseline", "Identify the left lower fault",
            "Identify the right lower fault", "Detect false balance", "Restore both bridge arms"
        };

        [MenuItem("Tools/Project Spark/Level 35/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 35 prefab already exists at " + PrefabPath);
                SparkLevel35DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_35.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_35.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.43f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.62f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Left midpoint",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Right midpoint",new Color(.95f,.56f,.16f));

            var root = new GameObject("Level_35_Bridge_Balance_Layout");
            try
            {
                var bench = Child(root.transform,"Bridge Balance Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.55f),board);
                var pins = new List<SparkTerminal>();
                var supply = Part<SparkPowerSupply>(bench,"PowerSupply_01",0,.56f,steel);
                Set(supply,("positiveTerminal",Pin(supply,"Battery_positive",-.07f,0,red,pins,SparkTerminalPolarity.Positive)),
                    ("negativeTerminal",Pin(supply,"Battery_negative",.07f,0,blue,pins,SparkTerminalPolarity.Negative)),
                    ("outputVoltage",5f),("currentLimit",.10f),("state",SparkPowerSupplyState.Active));
                Label(supply.transform,"5 V SUPPLY",font);

                var leftUpper = Arm(bench,"LEFT UPPER","LU",-.52f,.18f,steel,red,cyan,font,pins);
                var leftLower = Arm(bench,"LEFT LOWER","LL",-.52f,-.35f,steel,cyan,blue,font,pins);
                var rightUpper = Arm(bench,"RIGHT UPPER","RU",.52f,.18f,steel,red,gold,font,pins);
                var rightLower = Arm(bench,"RIGHT LOWER","RL",.52f,-.35f,steel,gold,blue,font,pins);
                var leftLight = Beacon(bench,"LEFT MID",new Vector3(-.82f,.09f,-.09f),cyan,new Color(.20f,1f,.83f),font);
                var rightLight = Beacon(bench,"RIGHT MID",new Vector3(.82f,.09f,-.09f),gold,new Color(1f,.63f,.24f),font);
                Label(Child(bench,"Bridge meter label",Vector3.zero),"DIFFERENCE = LEFT - RIGHT",font,.015f);

                var bindings = root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,supply);
                var layout = root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller = root.AddComponent<SparkLevel35BridgeBalance>();
                controller.layout=layout; controller.source=supply;
                controller.leftUpper=leftUpper; controller.leftLower=leftLower;
                controller.rightUpper=rightUpper; controller.rightLower=rightLower;
                controller.leftLamp=leftLight; controller.rightLamp=rightLight;
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 35 prefab.");
                SparkLevel35DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 35 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_35_Layout");
        }

        private static SparkResistor Arm(Transform bench,string name,string id,float x,float z,Material body,Material a,Material b,
            Font font,List<SparkTerminal> pins)
        {
            var arm=Part<SparkResistor>(bench,name,x,z,body);
            Set(arm,("terminalA",Pin(arm,id+"_IN",-.07f,0,a,pins)),
                ("terminalB",Pin(arm,id+"_OUT",.07f,0,b,pins)),("resistance",1000f),("maximumPowerWatts",.25f));
            var element=Solid(arm.transform,"Resistor cartridge",PrimitiveType.Cylinder,new Vector3(0,.075f,.042f),new Vector3(.041f,.056f,.041f),body);
            element.transform.localRotation=Quaternion.Euler(0,0,90);
            Label(arm.transform,name,font);
            return arm;
        }

        private static void BuildControls(Transform parent,SparkLevel35BridgeBalance c,Font font)
        {
            var obj=new GameObject("Level 35 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1060,176),new Vector2(0,80));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;
            c.statusText=UIText(panel,"Status","Wire both bridge branches.",font,new Vector2(1010,29),new Vector2(0,143));
            c.meterText=UIText(panel,"Voltages","LEFT  —    |    RIGHT  —    |    DIFF  —",font,new Vector2(1010,29),new Vector2(0,110));
            c.meterText.color=new Color(.40f,.92f,.93f); c.meterText.fontSize=23;
            c.capture=Control(panel,font,"CAPTURE BASELINE",-408,63,c.CaptureBaseline);
            c.loadCase=Control(panel,font,"LOAD CASE",-204,63,c.LoadCase);
            c.chooseLeft=Control(panel,font,"LEFT LOWER",0,63,c.DiagnoseLeft);
            c.chooseRight=Control(panel,font,"RIGHT LOWER",204,63,c.DiagnoseRight);
            c.chooseBoth=Control(panel,font,"BOTH LOWER",408,63,c.DiagnoseBoth);
            c.fitLeft=Control(panel,font,"FIT LEFT 1 kΩ",-306,15,c.FitLeftNominal);
            c.fitRight=Control(panel,font,"FIT RIGHT 1 kΩ",-102,15,c.FitRightNominal);
            c.verify=Control(panel,font,"VERIFY REPAIR",102,15,c.VerifyRepair);
            c.restart=Control(panel,font,"RESTART TEST",306,15,c.RestartTest);
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
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_34_Layout/Level_34_Priority_Power_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Priority Power Workbench") : null;
            if(anchor==null)
            {
                previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_33_Layout/Level_33_Jam_Guard_Layout.prefab");
                anchor=previous!=null ? previous.transform.Find("Jam Guard Workbench") : null;
            }
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }
        private static Light Beacon(Transform parent,string name,Vector3 p,Material material,Color color,Font font)
        {
            var marker=Child(parent,name,p);
            var bulb=Solid(marker,"Indicator",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.058f,material);
            var light=bulb.AddComponent<Light>(); light.type=LightType.Point; light.range=.25f;
            light.intensity=2f; light.color=color; light.enabled=false;
            Label(marker,name,font,-.060f);
            return light;
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_35" || targets.arraySize!=SparkLevel35BridgeBalance.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel35BridgeBalance.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_35"),("displayName","Bridge Balance - Find the Hidden Fault"),
                ("description","Wire a Wheatstone bridge and use signed midpoint readings to diagnose and repair hidden faults."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level35_bridge_balance_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from solved midpoint voltages and verified bridge diagnoses.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel35BridgeBalance.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_35.asset");
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
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel35BridgeBalance.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel35BridgeBalance.Gates[i];
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
            string path=$"{Folder}/L35 {name}.mat";
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




