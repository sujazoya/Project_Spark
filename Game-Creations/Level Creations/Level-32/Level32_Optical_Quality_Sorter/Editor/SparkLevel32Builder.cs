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
    public static class SparkLevel32Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_32_Layout";
        private const string PrefabPath = Folder + "/Level_32_Optical_Sorter_Layout.prefab";
        private static readonly string[] Objectives = {
            "Power the conveyor and sensor", "Calibrate both references", "Sort a dark part",
            "Sort a bright part", "Review borderline and reject glare", "Sort all six mixed parts"
        };

        [MenuItem("Tools/Project Spark/Level 32/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 32 layout already exists at " + PrefabPath);
                SparkLevel32DrawingAssetCreator.CreateAsset();
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            EnsureFolder();
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_32.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_32.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Steel",new Color(.29f,.39f,.47f));
            var slat = MaterialAsset(shader,"Belt",new Color(.20f,.27f,.31f));
            var blue = MaterialAsset(shader,"Terminals",new Color(.13f,.62f,.94f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.88f,.19f,.25f));
            var aqua = MaterialAsset(shader,"Optics",new Color(.15f,.82f,.75f));
            var amber = MaterialAsset(shader,"Review",new Color(.94f,.56f,.15f));
            var darkTile = MaterialAsset(shader,"Dark part",new Color(.09f,.11f,.17f));
            var brightTile = MaterialAsset(shader,"Bright part",new Color(.95f,.95f,.92f));

            var root = new GameObject("Level_32_Optical_Sorter_Layout");
            try
            {
                var bench = Child(root.transform,"Optical Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.55f),board);
                var pins = new List<SparkTerminal>();
                var supply = Part<SparkPowerSupply>(bench,"PowerSupply_01",-.87f,.50f,steel);
                Set(supply,("positiveTerminal",Pin(supply,"Battery_positive",-.07f,0,red,pins,SparkTerminalPolarity.Positive)),
                    ("negativeTerminal",Pin(supply,"Battery_negative",.07f,0,blue,pins,SparkTerminalPolarity.Negative)),
                    ("outputVoltage",5f),("currentLimit",.35f),("state",SparkPowerSupplyState.Active));
                Label(supply.transform,"5 V SUPPLY",font);
                var main = Part<SparkSwitch>(bench,"MAIN",-.52f,.50f,steel);
                Set(main,("inputTerminal",Pin(main,"MAIN_IN",-.07f,0,blue,pins)),
                    ("outputTerminal",Pin(main,"MAIN_OUT",.07f,0,blue,pins)),("state",SparkSwitchState.Open));
                Label(main.transform,"MAIN IN / OUT",font);
                var sensor = Part<SparkLDR>(bench,"Optical LDR",-.15f,.50f,steel);
                Set(sensor,("terminalA",Pin(sensor,"LDR_A",-.07f,0,blue,pins)),
                    ("terminalB",Pin(sensor,"LDR_B",.07f,0,aqua,pins)),("illumination",.50f),
                    ("darkResistanceOhms",100000f),("brightResistanceOhms",1000f));
                Label(sensor.transform,"LDR A / SENSE",font);
                var divider = Part<SparkResistor>(bench,"Divider Resistor",.22f,.50f,steel);
                Set(divider,("terminalA",Pin(divider,"R_DIV_IN",-.07f,0,aqua,pins)),
                    ("terminalB",Pin(divider,"R_DIV_OUT",.07f,0,blue,pins)),("resistance",10000f));
                Label(divider.transform,"10 kOhm IN / OUT",font);
                var motor = Part<SparkMotor>(bench,"Conveyor Motor",.61f,.50f,steel);
                Set(motor,("positiveTerminal",Pin(motor,"Motor_A",-.07f,0,blue,pins)),
                    ("negativeTerminal",Pin(motor,"Motor_B",.07f,0,blue,pins)),
                    ("windingResistance",33f),("startupVoltage",3f),("maximumCurrent",.25f));
                Label(motor.transform,"MOTOR A / B",font);
                var motorCase = Solid(motor.transform,"Housing",PrimitiveType.Cylinder,new Vector3(0,.080f,.065f),new Vector3(.064f,.065f,.064f),steel);
                motorCase.transform.localRotation=Quaternion.Euler(0,0,90);
                var shaft = Child(motor.transform,"Shaft",new Vector3(.072f,.080f,.065f));
                Solid(shaft,"Rotation marker",PrimitiveType.Cube,new Vector3(.02f,0,0),new Vector3(.045f,.012f,.041f),amber);

                var conveyor = Child(bench,"Optical sorting conveyor",new Vector3(0,.06f,-.22f));
                Solid(conveyor,"Base",PrimitiveType.Cube,new Vector3(-.07f,.015f,0),new Vector3(1.06f,.09f,.29f),board);
                foreach (float z in new[] {-.153f,.153f})
                    Solid(conveyor,"Rail",PrimitiveType.Cube,new Vector3(-.08f,.082f,z),new Vector3(1.02f,.045f,.018f),steel);
                var slats = new Transform[14];
                for (int i=0;i<slats.Length;i++)
                    slats[i]=Solid(conveyor,"Belt slat",PrimitiveType.Cube,new Vector3(i*.94f/slats.Length-.54f,.063f,0),
                        new Vector3(.060f,.017f,.27f),slat).transform;
                foreach (float z in new[] {-.18f,.18f})
                    Solid(conveyor,"Optical head support",PrimitiveType.Cube,new Vector3(-.08f,.14f,z),new Vector3(.047f,.24f,.04f),steel);
                var head = Solid(conveyor,"Optical head",PrimitiveType.Cube,new Vector3(-.08f,.27f,0),new Vector3(.13f,.052f,.40f),aqua);
                var lightObject = Child(head.transform,"Inspection light",new Vector3(0,-.54f,0));
                var light=lightObject.gameObject.AddComponent<Light>();
                light.type=LightType.Point; light.color=new Color(.39f,1f,.87f); light.range=.32f; light.intensity=2f; light.enabled=false;

                var gate = Child(conveyor,"Sorter gate pivot",new Vector3(.39f,.104f,0));
                Solid(gate,"Moving paddle",PrimitiveType.Cube,new Vector3(.07f,0,0),new Vector3(.17f,.04f,.028f),amber);
                MakeBin(conveyor,.71f,-.30f,"DARK",steel,board,font);
                MakeBin(conveyor,.71f,0,"REVIEW",amber,board,font);
                MakeBin(conveyor,.71f,.30f,"BRIGHT",aqua,board,font);
                var tile = Solid(conveyor,"Inspection part",PrimitiveType.Cube,new Vector3(-.54f,.12f,0),new Vector3(.080f,.044f,.072f),darkTile);
                tile.SetActive(false);

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,supply);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel32OpticalSorter>();
                controller.layout=layout; controller.source=supply; controller.main=main; controller.sensor=sensor;
                controller.divider=divider; controller.motor=motor; controller.beltSlats=slats; controller.shaft=shaft;
                controller.part=tile.transform; controller.partRenderer=tile.GetComponent<Renderer>(); controller.gate=gate;
                controller.darkPart=darkTile; controller.brightPart=brightTile; controller.reviewPart=amber;
                controller.inspectionLight=light;
                BuildControls(root.transform,controller,font);
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if (prefab == null) throw new InvalidOperationException("Could not save the Level 32 prefab.");
                SparkLevel32DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 32 created. Register the definition and prefab in the level manager and layout catalog.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_32_Layout");
        }

        private static void MakeBin(Transform parent,float x,float z,string caption,Material rim,Material floor,Font font)
        {
            var bin=Child(parent,caption+" bin",new Vector3(x,.025f,z));
            Solid(bin,"Bin floor",PrimitiveType.Cube,Vector3.zero,new Vector3(.31f,.055f,.23f),floor);
            Solid(bin,"Back wall",PrimitiveType.Cube,new Vector3(.146f,.038f,0),new Vector3(.024f,.068f,.23f),rim);
            foreach (float edge in new[] {-.105f,.105f})
                Solid(bin,"Side wall",PrimitiveType.Cube,new Vector3(0,.038f,edge),new Vector3(.31f,.068f,.02f),rim);
            Label(bin,caption,font,-.12f);
        }

        private static void BuildControls(Transform parent,SparkLevel32OpticalSorter controller,Font font)
        {
            var obj=new GameObject("Level 32 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(940,145),new Vector2(0,92));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;
            controller.statusText=UIText(panel,"Status","Wire the optical sorter.",font,new Vector2(915,31),new Vector2(0,105));
            controller.meterText=UIText(panel,"Sensor reading","SENSE  —",font,new Vector2(435,29),new Vector2(-220,69));
            controller.countText=UIText(panel,"Counts","DARK 0   |   BRIGHT 0   |   REVIEW 0",font,new Vector2(435,29),new Vector2(220,69));
            controller.mainIndicator=Control(panel,font,"MAIN",-315,controller.ToggleMain).targetGraphic as Image;
            controller.darkReference=Control(panel,font,"DARK REFERENCE",-105,controller.UseDarkReference);
            controller.brightReference=Control(panel,font,"BRIGHT REFERENCE",105,controller.UseBrightReference);
            controller.runTrial=Control(panel,font,"RUN TEST",315,controller.RunTrial);
        }

        private static Button Control(Transform parent,Font font,string caption,float x,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(194,38),new Vector2(x,16));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.48f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(194,38),Vector2.zero);
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_31_Layout/Level_31_SealIn_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Relay Workbench") : null;
            if (anchor==null)
            {
                previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_30_Layout/Level_30_Lift_Layout.prefab");
                anchor=previous!=null ? previous.transform.Find("Lift Workbench") : null;
            }
            if (anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }
        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_32" || targets.arraySize!=SparkLevel32OpticalSorter.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel32OpticalSorter.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_32"),("displayName","Optical Quality Sorter"),
                ("description","Wire and calibrate a light-sensor divider. Sort parts by measured voltage and reject brief glare."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level32_optical_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the solved sensor voltage and the sorting trial.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel32OpticalSorter.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_32.asset");
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
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel32OpticalSorter.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel32OpticalSorter.Gates[i];
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
            string path=$"{Folder}/L32 {name}.mat";
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

