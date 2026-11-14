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
    public static class SparkLevel66Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_66_Layout";
        private const string PrefabPath = Folder + "/Level_66_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the diagnostic network","Establish a healthy baseline","Diagnose the bearing fault","Perform safe predictive maintenance","Prove machine health under load","Recover lost condition feedback"
        };

        [MenuItem("Tools/Project Spark/Level 66/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 66 prefab already exists at " + PrefabPath);
                SparkLevel66DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_66.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_66.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.66f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.66f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.66f,.16f));

            var root = new GameObject("Level_66_Layout");
            try
            {
                var bench = Child(root.transform,"Maintenance Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Temporary board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.66f),board);
                var pins = new List<SparkTerminal>();
                SparkPowerSupply Supply(string name, float voltage, float current)
                {
                    var part=Part<SparkPowerSupply>(bench,name,0,0,steel);
                    Set(part,("outputVoltage",voltage),("currentLimit",current),("state",SparkPowerSupplyState.Active));
                    return part;
                }
                var source=Supply("CONTROL_POWER",5f,.66f);
                var positive=Pin(source,"SUPPLY_POS",0,0,red,pins,SparkTerminalPolarity.Positive);
                var ground=Pin(source,"SUPPLY_RET",0,0,blue,pins,SparkTerminalPolarity.Negative);
                Set(source,("positiveTerminal",positive),("negativeTerminal",ground));
                var power=Part<SparkResistor>(bench,"Maintenance PLC supply",0,0,steel);
                var cp=Pin(power,"PLC_5V",0,0,red,pins);
                var cg=Pin(power,"PLC_GND",0,0,blue,pins);
                Set(power,("terminalA",cp),("terminalB",cg),("resistance",2200f));

                foreach(var (plus,ret) in new[]{("IO_5V","IO_GND"),("VFD_5V","VFD_GND"),("RELAY_5V","RELAY_GND"),("CM_5V","CM_GND"),("FFT_5V","FFT_GND"),("TWIN_5V","TWIN_GND"),("SENSOR_5V","SENSOR_GND"),("DRIVE_5V","DRIVE_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"Maintenance signal endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                foreach(string id in new[]{"PLC_RUN","VFD_RUN","PLC_SPEED","VFD_REF","VFD_A","MOTOR_A","VFD_B","MOTOR_B","VFD_C","MOTOR_C","VFD_OK","PLC_READY","VFD_FAULT","RELAY_IN","RELAY_OUT","PLC_TRIP","VFD_REF_MON","TWIN_REF","CT_A","CM_IA","CT_B","CM_IB","CT_C","CM_IC","ENC_PULSE","CM_RPM","PLC_RPM","FFT_SPEED","BEARING_TEMP","CM_BT","MOTOR_TEMP","CM_MT","IR_OUT","CM_IR","OIL_OUT","CM_OIL","ALIGN_OUT","CM_ALIGN","GEAR_OUT","CM_GEAR","VIB2_OUT","CM_VIB2","VIB3_Y","FFT_Y","VIB3_Z","FFT_Z","BEARING_BT_MON","TWIN_BT","MOTOR_MT_MON","TWIN_MT","CT_MON","TWIN_I","ENC_RATE","TWIN_SPEED","FFT_OUT","CM_FFT","AI_FFT","CM_HEALTH","AI_HEALTH","TWIN_HEALTH","AI_OUT","PLC_SERVICE","TWIN_DIAG","HMI_SPECTRUM","HMI_HEALTH","PLC_COOL","FAN_IN","CONVEYOR_RUN","PLC_LOCK","VFD_INHIBIT","LOCK_AUX","PLC_LOCK_FB","MOUNT_OUT","PLC_MOUNT","GAUGE_IN","TORQUE_OUT","PLC_TORQUE","TOOL_OUT","PLC_TOOL","TWIN_TRIP","HMI_LOCK","HMI_TRIP","TWIN_SPECTRUM","VIB3_X","FFT_X"}) Pin(carrier,id,0,0,cyan,pins);
                SparkSwitch Contact(string name,SparkTerminal input,SparkTerminal output,bool state)
                {
                    var item=Child(bench,name,Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                    Set(item,("inputTerminal",input),("outputTerminal",output),("state",state?SparkSwitchState.Closed:SparkSwitchState.Open));
                    return item;
                }
                SparkSwitch Interlock(string name,string output)
                {
                    var jack=Pin(carrier,output,0,0,cyan,pins);
                    return Contact(name,positive,jack,true);
                }
                var emergency=Interlock("Emergency stop","ESTOP_NC");
                var door=Interlock("Safety door","GUARD_NC");
                var motorLoad=Child(bench,"VFD control load",Vector3.zero).gameObject.AddComponent<SparkResistor>();
                var driveNode=Child(motorLoad.transform,"VFD internal enable",Vector3.zero).gameObject.AddComponent<SparkTerminal>();
                Set(driveNode,("owner",motorLoad));
                Set(motorLoad,("terminalA",driveNode),("terminalB",cg),("resistance",1500f));
                var drive=Contact("Drive isolation",positive,driveNode,true);
                var contacts=new[]{door,drive};
                foreach(string channel in new[]{"CTRL_SAFE","PLC_DOOR","PLC_ESTOP"})
                {
                    var sense=Part<SparkResistor>(bench,channel,0,0,steel);
                    Set(sense,("terminalA",Pin(sense,channel,0,0,cyan,pins)),("terminalB",cg),("resistance",10000f));
                }
                string[] wireOrder={"SUPPLY_POS","PLC_5V","SUPPLY_RET","PLC_GND","IO_5V","IO_GND","VFD_5V","VFD_GND","RELAY_5V","RELAY_GND","CM_5V","CM_GND","FFT_5V","FFT_GND","TWIN_5V","TWIN_GND","SENSOR_5V","SENSOR_GND","DRIVE_5V","DRIVE_GND","PLC_RUN","VFD_RUN","PLC_SPEED","VFD_REF","VFD_A","MOTOR_A","VFD_B","MOTOR_B","VFD_C","MOTOR_C","VFD_OK","PLC_READY","VFD_FAULT","RELAY_IN","RELAY_OUT","PLC_TRIP","VFD_REF_MON","TWIN_REF","CT_A","CM_IA","CT_B","CM_IB","CT_C","CM_IC","ENC_PULSE","CM_RPM","PLC_RPM","FFT_SPEED","BEARING_TEMP","CM_BT","MOTOR_TEMP","CM_MT","IR_OUT","CM_IR","OIL_OUT","CM_OIL","ALIGN_OUT","CM_ALIGN","GEAR_OUT","CM_GEAR","VIB2_OUT","CM_VIB2","VIB3_Y","FFT_Y","VIB3_Z","FFT_Z","BEARING_BT_MON","TWIN_BT","MOTOR_MT_MON","TWIN_MT","CT_MON","TWIN_I","ENC_RATE","TWIN_SPEED","FFT_OUT","CM_FFT","AI_FFT","CM_HEALTH","AI_HEALTH","TWIN_HEALTH","AI_OUT","PLC_SERVICE","TWIN_DIAG","HMI_SPECTRUM","HMI_HEALTH","PLC_COOL","FAN_IN","CONVEYOR_RUN","PLC_LOCK","VFD_INHIBIT","LOCK_AUX","PLC_LOCK_FB","MOUNT_OUT","PLC_MOUNT","GAUGE_IN","TORQUE_OUT","PLC_TORQUE","TOOL_OUT","PLC_TOOL","GUARD_NC","CTRL_SAFE","PLC_DOOR","ESTOP_NC","PLC_ESTOP","TWIN_TRIP","HMI_LOCK","HMI_TRIP","TWIN_SPECTRUM","VIB3_X","FFT_X"};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel66Maintenance>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel66VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 66 prefab.");
                SparkLevel66DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 66 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_66_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel66Maintenance c,Font font)
        {
            var obj=new GameObject("Level 66 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,266),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the diagnostic network.",font,new Vector2(1080,28),new Vector2(0,266));
            c.meterText=UIText(panel,"Measurements","CONDITION MONITORING",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-453f,139,c.Power);
            c.calibrateButton=Control(panel,font,"CALIBRATE",-302f,139,c.Calibrate);
            c.runButton=Control(panel,font,"START MOTOR",-151f,139,c.Run);
            c.baselineButton=Control(panel,font,"RECORD BASELINE",0f,139,c.Baseline);
            c.wearButton=Control(panel,font,"BEARING FAULT",151f,139,c.Wear);
            c.diagnoseButton=Control(panel,font,"DIAGNOSE BPFO",302f,139,c.Diagnose);
            c.stopButton=Control(panel,font,"STOP MOTOR",453f,139,c.Stop);
            c.isolateButton=Control(panel,font,"ISOLATE DRIVE",-453f,97,c.Isolate);
            c.lockButton=Control(panel,font,"LOCKOUT",-302f,97,c.Lock);
            c.guardButton=Control(panel,font,"SAFETY DOOR",-151f,97,c.Guard);
            c.extractButton=Control(panel,font,"EXTRACT BEARING",0f,97,c.Extract);
            c.installButton=Control(panel,font,"FIT REPLACEMENT",151f,97,c.Install);
            c.alignButton=Control(panel,font,"ALIGN SHAFT",302f,97,c.Align);
            c.torqueButton=Control(panel,font,"VERIFY TORQUE",453f,97,c.Torque);
            c.speedButton=Control(panel,font,"SPEED TEST",-453f,55,c.Speed);
            c.loadButton=Control(panel,font,"LOAD TEST",-302f,55,c.Load);
            c.faultButton=Control(panel,font,"W70 TEST",-151f,55,c.Fault);
            c.resetButton=Control(panel,font,"RESET FAULT",0f,55,c.Reset);
            c.finalButton=Control(panel,font,"CERTIFY",151f,55,c.Final);
            c.emergencyStopButton=Control(panel,font,"E-STOP",302f,55,c.EmergencyStop);
            var plotRect=Rect(canvas.transform,"Vibration spectrum",new Vector2(340,159),new Vector2(519,6));
            c.plot=plotRect.gameObject.AddComponent<SparkLevel66Plot>();
            c.plot.raycastTarget=false;

        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(146,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.66f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(146,37),Vector2.zero).fontSize=14;
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_65_Layout/Level_65_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Factory Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_66" || targets.arraySize!=SparkLevel66Maintenance.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel66Maintenance.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_66"),("displayName","Save the Factory"),
                ("description","Measure condition, diagnose a worn bearing and prove the repair under load."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level66_maintenance_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel66Maintenance.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_66.asset");
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
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L66_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel66Maintenance.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel66Maintenance.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }


        private static T Part<T>(Transform parent,string name,float x,float z,Material material) where T:SparkComponent
        {
            var obj=Child(parent,name,new Vector3(x,.066f,z));
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
            obj.transform.localPosition=new Vector3(x,.066f,z); obj.transform.localScale=new Vector3(.075f,.018f,.075f);
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
            text.characterSize=.022f; text.fontSize=66; text.color=Color.white;
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
            string path=$"{Folder}/L66 {name}.mat";
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









