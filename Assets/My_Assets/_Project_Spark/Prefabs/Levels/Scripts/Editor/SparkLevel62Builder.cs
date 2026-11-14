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
    public static class SparkLevel62Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_62_Layout";
        private const string PrefabPath = Folder + "/Level_62_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the voltage restoration network","Commission the DVR","Survive a deep voltage sag","Correct swell and phase imbalance","Protect motor startup","Repair feedback and certify"
        };

        [MenuItem("Tools/Project Spark/Level 62/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 62 prefab already exists at " + PrefabPath);
                SparkLevel62DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_62.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_62.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.62f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.62f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.62f,.16f));

            var root = new GameObject("Level_62_Layout");
            try
            {
                var bench = Child(root.transform,"Voltage Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Temporary board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.62f),board);
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
                var power=Part<SparkResistor>(bench,"Voltage restoration controller rails",0,0,steel);
                var cp=Pin(power,"CTRL_POS",0,0,red,pins);
                var cg=Pin(power,"CTRL_GND",0,0,blue,pins);
                Set(power,("terminalA",cp),("terminalB",cg),("resistance",2200f));

                foreach(var (plus,ret) in new[]{("PLL_5V","PLL_GND"),("DVR_5V","DVR_GND"),("SENSE_5V","SENSE_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"AC and relay endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                foreach(string id in new[]{"GRID_A","MAIN_A","GRID_B","MAIN_B","GRID_C","MAIN_C","MAIN_OUT_A","INPUT_A","MAIN_OUT_B","INPUT_B","MAIN_OUT_C","INPUT_C","TX_A_IN","TX_B_IN","TX_C_IN","TX_A_OUT","PROTECTED_A","TX_B_OUT","PROTECTED_B","TX_C_OUT","PROTECTED_C","DVR_A","FILTER_A","DVR_B","FILTER_B","DVR_C","FILTER_C","TX_A_SEC","TX_B_SEC","TX_C_SEC","STORE_POS","CONVERTER_POS","STORE_NEG","DC_NEG","CONVERTER_OUT","DC_POS","DVR_POS","DVR_NEG","CTRL_PRE","PRECHARGE_CMD","LINK_OUT","CTRL_LINK","INPUT_VA","INPUT_VB","INPUT_VC","OUTPUT_VA","OUTPUT_VB","OUTPUT_VC","INPUT_SENSOR_OUT","PLL_IN","INPUT_CT","CTRL_INPUT_I","OUTPUT_CT","CTRL_OUTPUT_I","MOTOR_A","MOTOR_B","MOTOR_C","SENSITIVE_IN","DISTURBANCE_BUS","HEAVY_IN","CTRL_EVENT","EVENT_CMD","CTRL_BYPASS","BYPASS_CMD","PROTECTION_OUT","CTRL_PROTECT","PLL_OUT","CTRL_PHASE","BYPASS_AUX","CTRL_AUX","INJECTION_V_FEEDBACK","DVR_V_IN"}) Pin(carrier,id,0,0,cyan,pins);
                SparkTerminal ById(string id)=>pins.Find(pin=>pin.name==id);
                var contacts=new SparkSwitch[9];
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
                    var input=ById($"INPUT_{phase}");
                    var output=ById($"PROTECTED_{phase}");
                    Contact(p,$"Main {phase}",ById($"MAIN_{phase}"),ById($"MAIN_OUT_{phase}"));
                    Contact(3+p,$"Injection {phase}",ById($"TX_{phase}_IN"),ById($"TX_{phase}_OUT"));
                    Contact(6+p,$"Bypass {phase}",input,output);
                }
                var emergency=Child(bench,"Guard interlock",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",Pin(emergency,"GUARD_NC",0,0,cyan,pins)),("state",SparkSwitchState.Closed));
                var sense=Part<SparkResistor>(bench,"Guard sensor",0,0,steel);
                Set(sense,("terminalA",Pin(sense,"CTRL_SAFE",0,0,cyan,pins)),("terminalB",cg),("resistance",10000f));
                string[] wireOrder={"SUPPLY_POS","CTRL_POS","SUPPLY_RET","CTRL_GND","PLL_5V","PLL_GND","DVR_5V","DVR_GND","SENSE_5V","SENSE_GND","GRID_A","MAIN_A","GRID_B","MAIN_B","GRID_C","MAIN_C","MAIN_OUT_A","INPUT_A","MAIN_OUT_B","INPUT_B","MAIN_OUT_C","INPUT_C","TX_A_IN","TX_B_IN","TX_C_IN","TX_A_OUT","PROTECTED_A","TX_B_OUT","PROTECTED_B","TX_C_OUT","PROTECTED_C","DVR_A","FILTER_A","DVR_B","FILTER_B","DVR_C","FILTER_C","TX_A_SEC","TX_B_SEC","TX_C_SEC","STORE_POS","CONVERTER_POS","STORE_NEG","DC_NEG","CONVERTER_OUT","DC_POS","DVR_POS","DVR_NEG","CTRL_PRE","PRECHARGE_CMD","LINK_OUT","CTRL_LINK","INPUT_VA","INPUT_VB","INPUT_VC","OUTPUT_VA","OUTPUT_VB","OUTPUT_VC","INPUT_SENSOR_OUT","PLL_IN","INPUT_CT","CTRL_INPUT_I","OUTPUT_CT","CTRL_OUTPUT_I","MOTOR_A","MOTOR_B","MOTOR_C","SENSITIVE_IN","DISTURBANCE_BUS","HEAVY_IN","CTRL_EVENT","EVENT_CMD","CTRL_BYPASS","BYPASS_CMD","PROTECTION_OUT","CTRL_PROTECT","PLL_OUT","CTRL_PHASE","BYPASS_AUX","CTRL_AUX","GUARD_NC","CTRL_SAFE","INJECTION_V_FEEDBACK","DVR_V_IN"};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel62VoltageRestoration>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel62VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 62 prefab.");
                SparkLevel62DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 62 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_62_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel62VoltageRestoration c,Font font)
        {
            var obj=new GameObject("Level 62 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,262),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the voltage restoration network.",font,new Vector2(1080,28),new Vector2(0,262));
            c.meterText=UIText(panel,"Measurements","VOLTAGE RESTORATION",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-453f,139,c.Power);
            c.guardButton=Control(panel,font,"GUARD",-302f,139,c.Guard);
            c.mainButton=Control(panel,font,"MAIN ON",-151f,139,c.Main);
            c.calibrateButton=Control(panel,font,"CALIBRATE",0f,139,c.Calibrate);
            c.storeButton=Control(panel,font,"CHECK STORAGE",151f,139,c.Store);
            c.prechargeButton=Control(panel,font,"PRECHARGE",302f,139,c.Precharge);
            c.pllButton=Control(panel,font,"LOCK PLL",453f,139,c.Pll);
            c.enableButton=Control(panel,font,"ENABLE DVR",-453f,97,c.Enable);
            c.sagButton=Control(panel,font,"DEEP SAG",-302f,97,c.Sag);
            c.swellButton=Control(panel,font,"SWELL",-151f,97,c.Swell);
            c.unbalanceButton=Control(panel,font,"UNBALANCE",0f,97,c.UnbalanceTest);
            c.motorButton=Control(panel,font,"HEAVY MOTOR",151f,97,c.Motor);
            c.faultButton=Control(panel,font,"W54 TEST",302f,97,c.Fault);
            c.resetButton=Control(panel,font,"RESET FAULT",453f,97,c.Reset);
            c.finalButton=Control(panel,font,"CERTIFY",-453f,55,c.Final);
            var plotRect=Rect(canvas.transform,"Voltage traces",new Vector2(340,159),new Vector2(519,6));
            c.plot=plotRect.gameObject.AddComponent<SparkLevel62Plot>();
            c.plot.raycastTarget=false;

        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(146,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.62f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(146,37),Vector2.zero).fontSize=14;
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_61_Layout/Level_61_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Power Quality Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_62" || targets.arraySize!=SparkLevel62VoltageRestoration.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel62VoltageRestoration.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_62"),("displayName","Hold the Voltage"),
                ("description","Use series injection and stored energy to protect loads from sag, swell, unbalance and motor inrush."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level62_voltage_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel62VoltageRestoration.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_62.asset");
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
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L62_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel62VoltageRestoration.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel62VoltageRestoration.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }


        private static T Part<T>(Transform parent,string name,float x,float z,Material material) where T:SparkComponent
        {
            var obj=Child(parent,name,new Vector3(x,.062f,z));
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
            text.characterSize=.022f; text.fontSize=62; text.color=Color.white;
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
            string path=$"{Folder}/L62 {name}.mat";
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



