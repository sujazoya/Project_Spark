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
    public static class SparkLevel42Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_42_Layout";
        private const string PrefabPath = Folder + "/Level_42_SAR_ADC_Layout.prefab";
        private static readonly string[] Objectives = {
            "Wire and verify the comparator", "Resolve the most significant bit", "Convert Sensor A",
            "Convert Sensor B", "Diagnose reversed comparator inputs", "Verify all eight input bands"
        };

        [MenuItem("Tools/Project Spark/Level 42/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 42 prefab already exists at " + PrefabPath);
                SparkLevel42DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_42.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_42.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.43f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.62f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.56f,.16f));

            var root = new GameObject("Level_42_SAR_ADC_Layout");
            try
            {
                var bench = Child(root.transform,"SAR ADC Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"DAC board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.55f),board);
                var pins = new List<SparkTerminal>();
                var source=Supply(bench,steel,red,blue,font,pins);
                var high=new SparkSwitch[3];
                var low=new SparkSwitch[3];
                var outputs=new SparkTerminal[3];
                var branches=new SparkResistor[3];
                for (int bit=0;bit<3;bit++)
                {
                    float x=.64f*(1-bit);
                    Driver(bench,source,bit,x,steel,gold,font,pins,out high[bit],out low[bit],out outputs[bit]);
                    branches[bit]=Resistor(bench,$"B{bit} 20 kΩ",$"B{bit}_R_IN",$"B{bit}_R_OUT",x,-.07f,20000f,steel,gold,cyan,font,pins);
                }
                var link21=Resistor(bench,"LINK 21  10 kΩ","LINK21_IN","LINK21_OUT",-.32f,.24f,10000f,steel,cyan,cyan,font,pins);
                var link10=Resistor(bench,"LINK 10  10 kΩ","LINK10_IN","LINK10_OUT",.32f,.24f,10000f,steel,cyan,cyan,font,pins);
                var termination=Termination(bench,steel,cyan,blue,font,pins,out var termContact);
                var sensor=Part<SparkPowerSupply>(bench,"SENSOR SOURCE",-.385f,.505f,steel);
                Set(sensor,("positiveTerminal",Pin(sensor,"SENSOR_OUT",0,0,cyan,pins)),
                    ("negativeTerminal",source.NegativeTerminal),("outputVoltage",3.2f),
                    ("currentLimit",.005f),("state",SparkPowerSupplyState.Active));
                var comparator=Child(bench,"Comparator inputs",Vector3.zero);
                SparkTerminal Input(string id, float x)
                {
                    var probe=Part<SparkResistor>(comparator,id+" sense input",x,.352f,steel);
                    var pin=Pin(probe,id,0,0,cyan,pins);
                    Set(probe,("terminalA",pin),("terminalB",source.NegativeTerminal),("resistance",100000000f));
                    return pin;
                }
                var plus=Input("CMP_PLUS",.06f);
                var minus=Input("CMP_MINUS",.32f);

                var bindings = root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source);
                var layout = root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller = root.AddComponent<SparkLevel42SarAdc>();
                controller.layout=layout;
                controller.source=source;
                controller.highContacts=high;
                controller.lowContacts=low;
                controller.bitOutputs=outputs;
                controller.bitResistors=branches;
                controller.link21=link21;
                controller.link10=link10;
                controller.termination=termination;
                controller.terminationContact=termContact;
                controller.sensor=sensor;
                controller.comparatorPlus=plus;
                controller.comparatorMinus=minus;
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 42 prefab.");
                SparkLevel42DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 42 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_42_Layout");
        }

        private static SparkPowerSupply Supply(Transform bench,Material body,Material positive,Material negative,Font font,List<SparkTerminal> pins)
        {
            var supply=Part<SparkPowerSupply>(bench,"DAC_REFERENCE",-.76f,.55f,body);
            var reference=Pin(supply,"DAC_REF",-.07f,0,positive,pins,SparkTerminalPolarity.Positive);
            reference.GetComponent<Renderer>().enabled=false;
            Object.DestroyImmediate(reference.GetComponent<Collider>());
            Set(supply,("positiveTerminal",reference),
                ("negativeTerminal",Pin(supply,"DAC_RETURN",.07f,0,negative,pins,SparkTerminalPolarity.Negative)),
                ("outputVoltage",5f),("currentLimit",.05f),("state",SparkPowerSupplyState.Active));
            Label(supply.transform,"5 V REF  /  RETURN",font);
            return supply;
        }

        private static void Driver(Transform bench,SparkPowerSupply source,int bit,float x,Material body,Material pinMaterial,
            Font font,List<SparkTerminal> pins,out SparkSwitch high,out SparkSwitch low,out SparkTerminal output)
        {
            high=Part<SparkSwitch>(bench,$"B{bit} DRIVER",x,-.47f,body);
            low=Child(high.transform,$"B{bit} low throw",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
            output=Pin(high,$"BIT{bit}",0f,0f,pinMaterial,pins);
            Set(high,("inputTerminal",source.PositiveTerminal),("outputTerminal",output),("state",SparkSwitchState.Open));
            Set(low,("inputTerminal",source.NegativeTerminal),("outputTerminal",output),("state",SparkSwitchState.Closed),
                ("interactionsEnabled",false));
            Label(high.transform,$"B{bit} DRIVER",font);
        }

        private static SparkResistor Termination(Transform bench,Material body,Material signal,Material ground,
            Font font,List<SparkTerminal> pins,out SparkSwitch contact)
        {
            var resistor=Part<SparkResistor>(bench,"20 kΩ TERMINATION",.77f,.52f,body);
            var junction=Child(resistor.transform,"Internal termination junction",Vector3.zero).gameObject.AddComponent<SparkTerminal>();
            Set(junction,("owner",resistor));
            Set(resistor,("terminalA",Pin(resistor,"TERM_R_IN",-.07f,0,signal,pins)),
                ("terminalB",junction),("resistance",20000f),("maximumPowerWatts",.25f));
            contact=Child(resistor.transform,"Termination link",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
            Set(contact,("inputTerminal",junction),("outputTerminal",Pin(contact,"TERM_OUT",.07f,0,ground,pins)),
                ("state",SparkSwitchState.Closed));
            Label(resistor.transform,"20 kΩ TERMINATION",font);
            return resistor;
        }

        private static SparkResistor Resistor(Transform bench,string name,string pinA,string pinB,float x,float z,float resistance,
            Material body,Material a,Material b,Font font,List<SparkTerminal> pins)
        {
            var arm=Part<SparkResistor>(bench,name,x,z,body);
            var input=Pin(arm,pinA,-.07f,0,a,pins);
            var output=Pin(arm,pinB,.07f,0,b,pins);
            Set(arm,("terminalA",input),("terminalB",output),("resistance",resistance),("maximumPowerWatts",.25f));
            var element=Solid(arm.transform,"Cartridge",PrimitiveType.Cylinder,new Vector3(0,.075f,.042f),new Vector3(.042f,.056f,.042f),body);
            element.transform.localRotation=Quaternion.Euler(0,0,90);
            Label(arm.transform,name,font);
            return arm;
        }

        private static void BuildControls(Transform parent,SparkLevel42SarAdc c,Font font)
        {
            var obj=new GameObject("Level 42 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1240,200),new Vector2(0,56));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;
            c.statusText=UIText(panel,"Status","Wire the ladder and comparator.",font,new Vector2(1190,28),new Vector2(0,166));
            c.meterText=UIText(panel,"Measurements","TRIAL —   |   DAC —",font,new Vector2(1190,28),new Vector2(0,135));
            c.meterText.color=new Color(.40f,.92f,.93f); c.meterText.fontSize=23;
            c.sweepText=UIText(panel,"Sweep","SENSOR A",font,new Vector2(1100,26),new Vector2(0,106));
            c.testButton=Control(panel,font,"TEST NEXT BIT",-306,57,c.TestBit);
            c.keepButton=Control(panel,font,"KEEP BIT",-102,57,c.Keep);
            c.clearButton=Control(panel,font,"CLEAR BIT",102,57,c.Clear);
            c.submitButton=Control(panel,font,"SUBMIT CODE",306,57,c.Submit);
            c.nextButton=Control(panel,font,"LOAD SENSOR B",-204,12,c.NextSignal);
            c.sweepButton=Control(panel,font,"RUN SWEEP",0,12,c.RunSweep);
            c.restartButton=Control(panel,font,"RESTART TEST",204,12,c.Restart);
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
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_41_Layout/Level_41_Analog_Output_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("DAC Ladder Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_42" || targets.arraySize!=SparkLevel42SarAdc.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel42SarAdc.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_42"),("displayName","Read the Analog Signal"),
                ("description","Read two unknown signals with a 3-bit successive-approximation ADC. Diagnose reversed comparator inputs and verify all eight input bands."),
                ("completionMode",0),("requiredTargetCount",Objectives.Length),("requireClosedReturn",false),
                ("rejectShortCircuit",true),("rejectTargetShort",true),("requireMinimumVoltage",false),
                ("autoAdvanceOnCompletion",false));
            var so=new SerializedObject(level);
            var sources=so.FindProperty("powerSources"); sources.arraySize=1;
            var source=sources.GetArrayElementAtIndex(0);
            source.FindPropertyRelative("sourceName").stringValue="DAC_REFERENCE";
            source.FindPropertyRelative("positiveTerminalId").stringValue="DAC_REF";
            source.FindPropertyRelative("negativeTerminalId").stringValue="DAC_RETURN";
            source.FindPropertyRelative("nominalVoltage").floatValue=5f;
            var targets=so.FindProperty("targets"); targets.arraySize=Objectives.Length;
            for (int i=0;i<Objectives.Length;i++)
            {
                var target=targets.GetArrayElementAtIndex(i);
                target.FindPropertyRelative("targetId").stringValue=$"level42_sar_adc_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from solved comparator readings and verified conversion steps.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel42SarAdc.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_42.asset");
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
            power.FindPropertyRelative("id").stringValue="DAC_REFERENCE";
            power.FindPropertyRelative("component").objectReferenceValue=source;
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel42SarAdc.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel42SarAdc.Gates[i];
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
            string path=$"{Folder}/L42 {name}.mat";
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




