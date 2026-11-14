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
    public static class SparkLevel61Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_61_Layout";
        private const string PrefabPath = Folder + "/Level_61_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the power quality network","Diagnose harmonic distortion","Clean the source current","Correct power factor","Survive dynamic load change","Repair feedback and certify"
        };

        [MenuItem("Tools/Project Spark/Level 61/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 61 prefab already exists at " + PrefabPath);
                SparkLevel61DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_61.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_61.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.61f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.62f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.61f,.16f));

            var root = new GameObject("Level_61_Layout");
            try
            {
                var bench = Child(root.transform,"Power Quality Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Temporary board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.61f),board);
                var pins = new List<SparkTerminal>();
                SparkPowerSupply Supply(string name, float voltage, float current)
                {
                    var part=Part<SparkPowerSupply>(bench,name,0,0,steel);
                    Set(part,("outputVoltage",voltage),("currentLimit",current),("state",SparkPowerSupplyState.Active));
                    return part;
                }
                var source=Supply("CONTROL_POWER",5f,.65f);
                var positive=Pin(source,"SUPPLY_POS",0,0,red,pins,SparkTerminalPolarity.Positive);
                var ground=Pin(source,"SUPPLY_RET",0,0,blue,pins,SparkTerminalPolarity.Negative);
                Set(source,("positiveTerminal",positive),("negativeTerminal",ground));
                var power=Part<SparkResistor>(bench,"Power quality controller rails",0,0,steel);
                var cp=Pin(power,"CTRL_POS",0,0,red,pins);
                var cg=Pin(power,"CTRL_GND",0,0,blue,pins);
                Set(power,("terminalA",cp),("terminalB",cg),("resistance",2200f));

                foreach(var (plus,ret) in new[]{("PLL_5V","PLL_GND"),("APF_5V","APF_GND"),("QA_5V","QA_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"AC and relay endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                foreach(string id in new[]{"GRID_A","MAIN_A","GRID_B","MAIN_B","GRID_C","MAIN_C","MAIN_OUT_A","BUS_A","MAIN_OUT_B","BUS_B","MAIN_OUT_C","BUS_C","VS_A","VS_B","VS_C","VFD_A","VFD_B","VFD_C","VFD_OUT","MOTOR_IN","RECT_A","SMPS_B","CTRL_LOAD","LOAD_ENABLE","APF_A","APF_B","APF_C","CTRL_FILTER","APF_ENABLE","LINK_OUT","APF_LINK","APF_CMD","CONVERTER_CMD","CONVERTER_OUT","LCL_IN","SRC_CT_A","METER_IA","SRC_CT_B","METER_IB","SRC_CT_C","METER_IC","LOAD_CT_A","APF_IA","LOAD_CT_B","APF_IB","LOAD_CT_C","APF_IC","PLL_IN","PLL_OUT","APF_PHASE","CAP_A","CAP_B","CAP_C","CTRL_CAP","CAP_CMD","CAP_AUX","CTRL_CAP_SAFE","ANALYZER_PHASE","METER_OUT","ANALYZER_IN","PROTECT_OUT","CTRL_PROTECT","MAIN_FEEDBACK","CTRL_MAIN","FILTER_CURRENT_FEEDBACK","APF_CURRENT_IN"}) Pin(carrier,id,0,0,cyan,pins);
                SparkTerminal ById(string id)=>pins.Find(pin=>pin.name==id);
                var contacts=new SparkSwitch[12];
                SparkSwitch Contact(int index,string name,SparkTerminal input,SparkTerminal output)
                {
                    var contact=Child(bench,name,Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                    SparkTerminal Internal(string id)
                    {
                        var terminal=Child(contact.transform,id,Vector3.zero).gameObject.AddComponent<SparkTerminal>();
                        Set(terminal,("owner",contact));
                        return terminal;
                    }
                    Set(contact,("inputTerminal",input??Internal("Input")),("outputTerminal",output??Internal("Output")),("state",SparkSwitchState.Open));
                    contacts[index]=contact;
                    return contact;
                }
                for(int p=0;p<3;p++)
                {
                    char phase="ABC"[p];
                    Contact(p,$"Main {phase}",ById($"MAIN_{phase}"),ById($"MAIN_OUT_{phase}"));
                    Contact(3+p,$"Filter {phase}",ById($"APF_{phase}"),null);
                    Contact(6+p,$"Capacitor {phase}",ById($"CAP_{phase}"),null);
                    Contact(9+p,$"Nonlinear load {phase}",ById($"VFD_{phase}"),null);
                }
                var emergency=Child(bench,"Guard interlock",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",Pin(emergency,"GUARD_NC",0,0,cyan,pins)),("state",SparkSwitchState.Closed));
                var sense=Part<SparkResistor>(bench,"Guard sensor",0,0,steel);
                Set(sense,("terminalA",Pin(sense,"CTRL_SAFE",0,0,cyan,pins)),("terminalB",cg),("resistance",10000f));
                string[] wireOrder={"SUPPLY_POS","CTRL_POS","SUPPLY_RET","CTRL_GND","PLL_5V","PLL_GND","APF_5V","APF_GND","QA_5V","QA_GND","GRID_A","MAIN_A","GRID_B","MAIN_B","GRID_C","MAIN_C","MAIN_OUT_A","BUS_A","MAIN_OUT_B","BUS_B","MAIN_OUT_C","BUS_C","VS_A","VS_B","VS_C","VFD_A","VFD_B","VFD_C","VFD_OUT","MOTOR_IN","RECT_A","SMPS_B","CTRL_LOAD","LOAD_ENABLE","APF_A","APF_B","APF_C","CTRL_FILTER","APF_ENABLE","LINK_OUT","APF_LINK","APF_CMD","CONVERTER_CMD","CONVERTER_OUT","LCL_IN","SRC_CT_A","METER_IA","SRC_CT_B","METER_IB","SRC_CT_C","METER_IC","LOAD_CT_A","APF_IA","LOAD_CT_B","APF_IB","LOAD_CT_C","APF_IC","PLL_IN","PLL_OUT","APF_PHASE","CAP_A","CAP_B","CAP_C","CTRL_CAP","CAP_CMD","CAP_AUX","CTRL_CAP_SAFE","ANALYZER_PHASE","METER_OUT","ANALYZER_IN","PROTECT_OUT","CTRL_PROTECT","MAIN_FEEDBACK","CTRL_MAIN","GUARD_NC","CTRL_SAFE","FILTER_CURRENT_FEEDBACK","APF_CURRENT_IN"};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel61PowerQuality>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel61VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 61 prefab.");
                SparkLevel61DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 61 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_61_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel61PowerQuality c,Font font)
        {
            var obj=new GameObject("Level 61 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,261),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the power quality network.",font,new Vector2(1080,28),new Vector2(0,261));
            c.meterText=UIText(panel,"Measurements","POWER QUALITY",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-453f,139,c.Power);
            c.guardButton=Control(panel,font,"GUARD",-302f,139,c.Guard);
            c.mainButton=Control(panel,font,"MAIN ON",-151f,139,c.Main);
            c.calibrateButton=Control(panel,font,"CALIBRATE",0f,139,c.Calibrate);
            c.loadButton=Control(panel,font,"NONLINEAR LOADS",151f,139,c.Load);
            c.pllButton=Control(panel,font,"LOCK PLL",302f,139,c.Pll);
            c.baselineButton=Control(panel,font,"RECORD THD",453f,139,c.Baseline);
            c.filterButton=Control(panel,font,"ENABLE FILTER",-453f,97,c.Filter);
            c.gainButton=Control(panel,font,"TUNE GAIN",-302f,97,c.GainSelect);
            c.capButton=Control(panel,font,"CAP STAGES",-151f,97,c.Cap);
            c.stepButton=Control(panel,font,"LOAD STEP",0f,97,c.LoadStep);
            c.faultButton=Control(panel,font,"W50 TEST",151f,97,c.Fault);
            c.resetButton=Control(panel,font,"RESET FAULT",302f,97,c.Reset);
            c.finalButton=Control(panel,font,"CERTIFY",453f,97,c.Final);
            var plotRect=Rect(canvas.transform,"Waveforms and spectrum",new Vector2(340,159),new Vector2(519,6));
            c.plot=plotRect.gameObject.AddComponent<SparkLevel61Plot>();
            c.plot.raycastTarget=false;

        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(146,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.61f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(146,37),Vector2.zero).fontSize=14;
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_60_Layout/Level_60_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Resilience Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_61" || targets.arraySize!=SparkLevel61PowerQuality.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel61PowerQuality.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_61"),("displayName","Clean the Wave"),
                ("description","Measure harmonics, tune active current cancellation, correct reactive power and recover from missing filter feedback."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level61_quality_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel61PowerQuality.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_61.asset");
            return level;
        }

        private static void WriteBindings(SparkLevelSceneBindings bindings,List<SparkTerminal> pins,SparkPowerSupply source, SparkSwitch[] contacts, SparkSwitch emergency)
        {
            var so=new SerializedObject(bindings);
            var terminals=so.FindProperty("terminals"); terminals.arraySize=pins.Count;
            for(int i=0;i<pins.Count;i++)
            {
                var row=terminals.GetArrayElementAtIndex(i);
                row.FindPropertyRelative("id").stringValue=pins[i].name;
                row.FindPropertyRelative("terminal").objectReferenceValue=pins[i];
            }
            var fixedComponents=new List<(string id,SparkElectricalComponent component)> {
                ("CONTROL_POWER",source),("GUARD_INTERLOCK",emergency)
            };
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L61_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel61PowerQuality.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel61PowerQuality.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }


        private static T Part<T>(Transform parent,string name,float x,float z,Material material) where T:SparkComponent
        {
            var obj=Child(parent,name,new Vector3(x,.061f,z));
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
            obj.transform.localPosition=new Vector3(x,.065f,z); obj.transform.localScale=new Vector3(.075f,.018f,.075f);
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
            text.characterSize=.022f; text.fontSize=61; text.color=Color.white;
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
            string path=$"{Folder}/L61 {name}.mat";
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


