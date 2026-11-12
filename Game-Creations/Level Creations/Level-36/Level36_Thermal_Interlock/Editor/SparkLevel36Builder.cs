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
    public static class SparkLevel36Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_36_Layout";
        private const string PrefabPath = Folder + "/Level_36_Thermal_Interlock_Layout.prefab";
        private static readonly string[] Objectives = {
            "Wire and verify cold start", "Trip cooling on heat", "Hold cooling in the warm band",
            "Recover and hold with cooling off", "Latch an open sensor fault", "Test the short fault and recover"
        };

        [MenuItem("Tools/Project Spark/Level 36/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 36 prefab already exists at " + PrefabPath);
                SparkLevel36DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_36.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_36.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.43f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.62f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Sensor and fan",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Heater",new Color(.95f,.56f,.16f));
            var amber = MaterialAsset(shader,"Fault LED",new Color(1f,.39f,.09f));
            var dim = MaterialAsset(shader,"Indicator off",new Color(.15f,.20f,.24f));

            var root = new GameObject("Level_36_Thermal_Interlock_Layout");
            try
            {
                var bench = Child(root.transform,"Thermal Interlock Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.62f),board);
                var pins = new List<SparkTerminal>();
                var supply = Part<SparkPowerSupply>(bench,"PowerSupply_01",0,.65f,steel);
                Set(supply,("positiveTerminal",Pin(supply,"Battery_positive",-.07f,0,red,pins,SparkTerminalPolarity.Positive)),
                    ("negativeTerminal",Pin(supply,"Battery_negative",.07f,0,blue,pins,SparkTerminalPolarity.Negative)),
                    ("outputVoltage",5f),("currentLimit",.25f),("state",SparkPowerSupplyState.Active));
                Label(supply.transform,"5 V SUPPLY",font);

                var pullUp = Resistor(bench,"10 kΩ PULL-UP","PULLUP",-.81f,.28f,10000f,.25f,steel,red,cyan,font,pins);
                var thermistor = Part<SparkResistor>(bench,"NTC SENSOR",-.81f,-.08f,steel);
                var disconnect = Child(thermistor.transform,"Sensor disconnect",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                var bypass = Child(thermistor.transform,"Sensor bypass",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                var sensorIn = Pin(disconnect,"SENSOR_IN",-.07f,0,cyan,pins);
                var sensorReturn = Pin(thermistor,"SENSOR_RETURN",.07f,0,blue,pins);
                var internalPin = Child(thermistor.transform,"NTC internal node",Vector3.zero).gameObject.AddComponent<SparkTerminal>();
                Set(internalPin,("owner",thermistor));
                Set(thermistor,("terminalA",internalPin),("terminalB",sensorReturn),("resistance",15000f));
                ConfigureContact(disconnect,sensorIn,internalPin,SparkSwitchState.Closed);
                ConfigureContact(bypass,sensorIn,sensorReturn,SparkSwitchState.Open);
                Label(thermistor.transform,"NTC SENSOR",font);

                var fanContact = OutputContact(bench,"FAN CONTACT","FAN_SW",-.27f,steel,red,cyan,font,pins);
                var heaterContact = OutputContact(bench,"HEATER CONTACT","HEAT_SW",.27f,steel,red,gold,font,pins);
                var faultContact = OutputContact(bench,"FAULT CONTACT","FAULT_SW",.81f,steel,red,amber,font,pins);
                var fan = Part<SparkMotor>(bench,"COOLING FAN",-.27f,-.08f,steel);
                Set(fan,("positiveTerminal",Pin(fan,"FAN_A",-.07f,0,cyan,pins)),
                    ("negativeTerminal",Pin(fan,"FAN_B",.07f,0,blue,pins)),
                    ("windingResistance",47f),("startupVoltage",2.7f),("maximumCurrent",.18f));
                Label(fan.transform,"COOLING FAN",font);
                var rotor = CreateFan(fan.transform,cyan,dim);
                var heater = Resistor(bench,"HEATER 100 Ω","HEATER",.27f,-.08f,100f,1f,steel,gold,blue,font,pins);
                var heaterGlow = Indicator(heater.transform,"Heater energized",gold,new Color(1f,.48f,.15f));
                var ledResistor = Resistor(bench,"330 Ω","LED_R",.81f,-.08f,330f,.25f,steel,amber,amber,font,pins);
                var faultLed = Part<SparkLED>(bench,"FAULT LED",.81f,-.45f,steel);
                Set(faultLed,("anodeTerminal",Pin(faultLed,"LED_A",-.07f,0,amber,pins)),
                    ("cathodeTerminal",Pin(faultLed,"LED_K",.07f,0,blue,pins)),
                    ("forwardVoltage",2f),("maximumForwardCurrent",.02f));
                Label(faultLed.transform,"FAULT LED",font);
                var warningGlow = Indicator(faultLed.transform,"Fault energized",amber,new Color(1f,.30f,.06f));

                var bindings = root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,supply);
                var layout = root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller = root.AddComponent<SparkLevel36ThermalInterlock>();
                controller.layout=layout; controller.source=supply;
                controller.pullUp=pullUp; controller.thermistor=thermistor;
                controller.sensorDisconnect=disconnect; controller.sensorBypass=bypass;
                controller.sensorInput=sensorIn; controller.sensorReturn=sensorReturn;
                controller.fanContact=fanContact; controller.heaterContact=heaterContact; controller.faultContact=faultContact;
                controller.fan=fan; controller.heater=heater; controller.faultLed=faultLed; controller.ledResistor=ledResistor;
                controller.fanRotor=rotor; controller.heaterGlow=heaterGlow; controller.warningGlow=warningGlow;
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 36 prefab.");
                SparkLevel36DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 36 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_36_Layout");
        }

        private static SparkResistor Resistor(Transform bench,string name,string id,float x,float z,float ohms,float watts,
            Material body,Material a,Material b,Font font,List<SparkTerminal> pins)
        {
            var arm=Part<SparkResistor>(bench,name,x,z,body);
            Set(arm,("terminalA",Pin(arm,id+"_IN",-.07f,0,a,pins)),
                ("terminalB",Pin(arm,id+"_OUT",.07f,0,b,pins)),("resistance",ohms),("maximumPowerWatts",watts));
            Label(arm.transform,name,font);
            return arm;
        }

        private static SparkSwitch OutputContact(Transform bench,string name,string id,float x,Material body,Material a,
            Material b,Font font,List<SparkTerminal> pins)
        {
            var contact=Part<SparkSwitch>(bench,name,x,.28f,body);
            ConfigureContact(contact,Pin(contact,id+"_IN",-.07f,0,a,pins),
                Pin(contact,id+"_OUT",.07f,0,b,pins),SparkSwitchState.Open);
            Label(contact.transform,name,font);
            return contact;
        }

        private static void ConfigureContact(SparkSwitch contact,SparkTerminal input,SparkTerminal output,SparkSwitchState state)
        {
            Set(contact,("inputTerminal",input),("outputTerminal",output),("state",state));
        }

        private static Transform CreateFan(Transform parent,Material blade,Material housing)
        {
            Solid(parent,"Fan housing",PrimitiveType.Cylinder,new Vector3(0,.075f,.042f),new Vector3(.10f,.009f,.10f),housing);
            var rotor=Child(parent,"Fan rotor",new Vector3(0,.093f,.042f));
            Solid(rotor,"Hub",PrimitiveType.Sphere,Vector3.zero,new Vector3(.024f,.017f,.024f),blade);
            for(int i=0;i<4;i++)
            {
                var bladeRoot=Child(rotor,"Blade "+(i+1),Vector3.zero);
                bladeRoot.localRotation=Quaternion.Euler(0,i*90,0);
                Solid(bladeRoot,"Blade",PrimitiveType.Cube,new Vector3(.026f,0,0),new Vector3(.038f,.008f,.014f),blade);
            }
            return rotor;
        }

        private static void BuildControls(Transform parent,SparkLevel36ThermalInterlock c,Font font)
        {
            var obj=new GameObject("Level 36 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1060,194),new Vector2(0,68));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;
            c.statusText=UIText(panel,"Status","Wire the sensor and outputs.",font,new Vector2(1010,29),new Vector2(0,158));
            c.meterText=UIText(panel,"Sense voltage","SENSE —",font,new Vector2(1010,29),new Vector2(0,130));
            c.meterText.color=new Color(.40f,.92f,.93f); c.meterText.fontSize=23;
            c.outputsText=UIText(panel,"Output status","FAN —    |    HEATER —    |    FAULT —",font,new Vector2(1010,26),new Vector2(0,103));
            c.coolButton=Control(panel,font,"COOL",-306,56,c.SelectCool);
            c.warmButton=Control(panel,font,"WARM",-102,56,c.SelectWarm);
            c.hotButton=Control(panel,font,"HOT",102,56,c.SelectHot);
            c.restartButton=Control(panel,font,"RESTART TEST",306,56,c.RestartTest);
            c.openButton=Control(panel,font,"OPEN SENSOR",-306,12,c.OpenSensor);
            c.shortButton=Control(panel,font,"SHORT SENSOR",-102,12,c.ShortSensor);
            c.restoreButton=Control(panel,font,"RESTORE SENSOR",102,12,c.RestoreSensor);
            c.resetButton=Control(panel,font,"RESET FAULT",306,12,c.ResetFault);
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
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_35_Layout/Level_35_Bridge_Balance_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Bridge Balance Workbench") : null;
            if(anchor==null)
            {
                previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_34_Layout/Level_34_Priority_Power_Layout.prefab");
                anchor=previous!=null ? previous.transform.Find("Priority Power Workbench") : null;
            }
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }
        private static GameObject Indicator(Transform parent,string name,Material material,Color color)
        {
            var bulb=Solid(parent,name,PrimitiveType.Sphere,new Vector3(0,.10f,.033f),Vector3.one*.058f,material);
            var light=bulb.AddComponent<Light>();
            light.type=LightType.Point; light.range=.25f; light.intensity=2f; light.color=color;
            bulb.SetActive(false);
            return bulb;
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_36" || targets.arraySize!=SparkLevel36ThermalInterlock.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel36ThermalInterlock.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_36"),("displayName","Thermal Interlock"),
                ("description","Wire and verify a cooling interlock with hysteresis and latched sensor faults."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level36_thermal_interlock_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from solved sensor voltage and output current.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel36ThermalInterlock.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_36.asset");
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
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel36ThermalInterlock.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel36ThermalInterlock.Gates[i];
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
            Set(terminal,("owner",owner),("kind",kind),("polarity",polarity),("maxConnections",4));
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
            string path=$"{Folder}/L36 {name}.mat";
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




