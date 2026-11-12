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
    public static class SparkLevel38Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_38_Layout";
        private const string PrefabPath = Folder + "/Level_38_One_Press_Layout.prefab";
        private static readonly string[] Objectives = {
            "Wire the counter input", "Observe raw contact bounce", "Reject a 100 ms glitch",
            "Count one filtered press", "Release and rearm", "Count two deliberate presses"
        };

        [MenuItem("Tools/Project Spark/Level 38/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 38 prefab already exists at " + PrefabPath);
                SparkLevel38DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_38.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_38.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.43f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.62f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Filtered signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Raw signal",new Color(.95f,.56f,.16f));

            var root = new GameObject("Level_38_One_Press_Layout");
            try
            {
                var bench = Child(root.transform,"Debounce Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.55f),board);
                var pins = new List<SparkTerminal>();
                var supply = Part<SparkPowerSupply>(bench,"PowerSupply_01",-.76f,.50f,steel);
                Set(supply,("positiveTerminal",Pin(supply,"Battery_positive",-.07f,0,red,pins,SparkTerminalPolarity.Positive)),
                    ("negativeTerminal",Pin(supply,"Battery_negative",.07f,0,blue,pins,SparkTerminalPolarity.Negative)),
                    ("outputVoltage",5f),("currentLimit",.05f),("state",SparkPowerSupplyState.Active));
                Label(supply.transform,"5 V SUPPLY  +  /  −",font);

                var contact=Part<SparkSwitch>(bench,"PUSHBUTTON",-.18f,.50f,steel);
                Set(contact,("inputTerminal",Pin(contact,"SW_IN",-.07f,0,red,pins)),
                    ("outputTerminal",Pin(contact,"SW_OUT",.07f,0,red,pins)),("state",SparkSwitchState.Open));
                Solid(contact.transform,"Button",PrimitiveType.Cylinder,new Vector3(0,.064f,0),new Vector3(.062f,.018f,.062f),red);
                Label(contact.transform,"PUSHBUTTON",font);
                var feed = Resistor(bench,"1 kΩ FEED","FEED_IN","FEED_OUT",.41f,.50f,1000f,.25f,steel,red,gold,font,pins);
                var pullDown = Resistor(bench,"10 kΩ PULL-DOWN","PULLDOWN_IN","PULLDOWN_OUT",-.62f,-.06f,10000f,.25f,steel,gold,blue,font,pins);
                var filter = Resistor(bench,"10 kΩ FILTER","FILTER_IN","FILTER_OUT",.04f,-.06f,10000f,.25f,steel,gold,cyan,font,pins);
                var capacitor=Part<SparkCapacitor>(bench,"47 µF CAPACITOR",.70f,-.06f,steel);
                Set(capacitor,("positiveTerminal",Pin(capacitor,"CAP_PLUS",-.07f,0,cyan,pins)),
                    ("negativeTerminal",Pin(capacitor,"CAP_MINUS",.07f,0,blue,pins)),
                    ("capacitanceFarads",47e-6f),("voltageRating",16f));
                Solid(capacitor.transform,"Capacitor can",PrimitiveType.Cylinder,new Vector3(0,.074f,.020f),new Vector3(.048f,.053f,.048f),cyan);
                Label(capacitor.transform,"47 µF CAPACITOR",font);
                var countLight = Beacon(bench,"PART DETECTED",new Vector3(.70f,.09f,-.36f),cyan,new Color(.20f,1f,.83f),font);

                var bindings = root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,supply);
                var layout = root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller = root.AddComponent<SparkLevel38OnePress>();
                controller.layout=layout; controller.source=supply;
                controller.contact=contact;
                controller.feed=feed;
                controller.pullDown=pullDown;
                controller.filter=filter;
                controller.capacitor=capacitor;
                controller.countIndicator=countLight;
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 38 prefab.");
                SparkLevel38DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 38 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_38_Layout");
        }

        private static SparkResistor Resistor(Transform bench,string name,string pinA,string pinB,float x,float z,float resistance,
            float maximumPower,Material body,Material a,Material b,Font font,List<SparkTerminal> pins)
        {
            var arm=Part<SparkResistor>(bench,name,x,z,body);
            var input=Pin(arm,pinA,-.07f,0,a,pins);
            var output=Pin(arm,pinB,.07f,0,b,pins);
            Set(arm,("terminalA",input),("terminalB",output),("resistance",resistance),("maximumPowerWatts",maximumPower));
            var element=Solid(arm.transform,"Cartridge",PrimitiveType.Cylinder,new Vector3(0,.075f,.042f),new Vector3(.041f,.056f,.041f),body);
            element.transform.localRotation=Quaternion.Euler(0,0,90);
            Label(arm.transform,name,font);
            return arm;
        }

        private static void BuildControls(Transform parent,SparkLevel38OnePress c,Font font)
        {
            var obj=new GameObject("Level 38 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1100,260),new Vector2(0,52));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;
            c.statusText=UIText(panel,"Status","Wire the button and filter.",font,new Vector2(1060,29),new Vector2(0,228));
            c.meterText=UIText(panel,"Measurements","RAW —   |   FILTERED —   |   RAW EDGES —   |   PART COUNT —",font,new Vector2(1060,28),new Vector2(0,201));
            c.meterText.color=new Color(.40f,.92f,.93f); c.meterText.fontSize=23;
            c.rawTrace=Plot(panel,font,"RAW  ·  0–5.2 V",-258,false);
            c.filteredTrace=Plot(panel,font,"FILTERED  ·  3.0 V / 2.0 V",258,true);
            c.rawTestButton=Control(panel,font,"RAW TEST",-306,56,c.RunRawTest);
            c.filteredModeButton=Control(panel,font,"FILTERED MODE",-102,56,c.SelectFiltered);
            c.glitchButton=Control(panel,font,"100 ms GLITCH",102,56,c.RunGlitch);
            c.pressButton=Control(panel,font,"BOUNCED PRESS",306,56,c.RunPress);
            c.releaseButton=Control(panel,font,"BOUNCED RELEASE",-204,13,c.RunRelease);
            c.cycleButton=Control(panel,font,"SECOND CYCLE",0,13,c.RunCycle);
            c.restartButton=Control(panel,font,"RESTART TEST",204,13,c.Restart);
        }

        private static RectTransform Plot(Transform parent,Font font,string title,float x,bool thresholds)
        {
            var caption=UIText(parent,title,title,font,new Vector2(492,20),new Vector2(x,180));
            caption.fontSize=15;
            caption.color=thresholds ? new Color(.31f,.92f,.86f) : new Color(1f,.72f,.30f);
            var plot=Rect(parent,title+" plot",new Vector2(492,78),new Vector2(x,102));
            var background=plot.gameObject.AddComponent<Image>();
            background.color=new Color(.015f,.03f,.05f);
            background.raycastTarget=false;
            if (thresholds)
            {
                foreach (float voltage in new[] { 2f, 3f })
                {
                    var guide=Rect(plot,"Threshold "+voltage,new Vector2(492,1),new Vector2(0,78f*voltage/5.2f));
                    var image=guide.gameObject.AddComponent<Image>();
                    image.color=new Color(.31f,.62f,.65f,.55f);
                    image.raycastTarget=false;
                }
            }
            return plot;
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
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_37_Layout/Level_37_Kelvin_Test_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Kelvin Test Workbench") : null;
            if(anchor==null)
            {
                previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_36_Layout/Level_36_Thermal_Interlock_Layout.prefab");
                anchor=previous!=null ? previous.transform.Find("Thermal Interlock Workbench") : null;
            }
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }
        private static GameObject Beacon(Transform parent,string name,Vector3 p,Material material,Color color,Font font)
        {
            var marker=Child(parent,name,p);
            var bulb=Solid(marker,"Indicator",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.058f,material);
            var light=bulb.AddComponent<Light>(); light.type=LightType.Point; light.range=.25f;
            light.intensity=2f; light.color=color;
            Label(marker,name,font,-.060f);
            bulb.SetActive(false);
            return bulb;
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_38" || targets.arraySize!=SparkLevel38OnePress.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel38OnePress.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_38"),("displayName","One Press, One Count"),
                ("description","Filter switch bounce and verify one part count per deliberate press."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level38_one_press_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from solved raw and filtered voltages and observed counts.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel38OnePress.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_38.asset");
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
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel38OnePress.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel38OnePress.Gates[i];
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
            string path=$"{Folder}/L38 {name}.mat";
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




