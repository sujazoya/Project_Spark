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
    public static class SparkLevel73Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_73_Layout";
        private const string PrefabPath = Folder + "/Level_73_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the precision polishing network","Commission the CMP workstation","Master pressure and material removal","Achieve perfect planarization","Survive the polishing disturbances","Restore critical downforce feedback"
        };

        [MenuItem("Tools/Project Spark/Level 73/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 73 prefab already exists at " + PrefabPath);
                SparkLevel73DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_73.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_73.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.71f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.71f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.71f,.16f));

            var root = new GameObject("Level_73_Layout");
            try
            {
                var bench = Child(root.transform,"CMP Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Temporary board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.71f),board);
                var pins = new List<SparkTerminal>();
                SparkPowerSupply Supply(string name, float voltage, float current)
                {
                    var part=Part<SparkPowerSupply>(bench,name,0,0,steel);
                    Set(part,("outputVoltage",voltage),("currentLimit",current),("state",SparkPowerSupplyState.Active));
                    return part;
                }
                var source=Supply("CONTROL_POWER",5f,.71f);
                var positive=Pin(source,"SUPPLY_POS",0,0,red,pins,SparkTerminalPolarity.Positive);
                var ground=Pin(source,"SUPPLY_RET",0,0,blue,pins,SparkTerminalPolarity.Negative);
                Set(source,("positiveTerminal",positive),("negativeTerminal",ground));
                foreach(var (plus,ret) in new[]{("PLC_5V","PLC_GND"),("IO_5V","IO_GND"),("DRIVE_5V","DRIVE_GND"),("MET_5V","MET_GND"),("FORCE_5V","FORCE_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"Process I/O endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                string[] wireOrder={"SUPPLY_POS","PLC_5V","SUPPLY_RET","PLC_GND","IO_5V","IO_GND","DRIVE_5V","DRIVE_GND","MET_5V","MET_GND","FORCE_5V","FORCE_GND","PLC_PLATEN","PLATEN_RUN","PLC_CARRIER","CARRIER_RUN","PLC_PL_RPM","PLATEN_SET","PLC_CA_RPM","CARRIER_SET","PLATEN_ENC_A","DRIVE_P_ENC_A","PLATEN_ENC_B","DRIVE_P_ENC_B","CARRIER_ENC_A","DRIVE_C_ENC_A","CARRIER_ENC_B","DRIVE_C_ENC_B","PLATEN_TORQUE","IO_P_TORQUE","CARRIER_TORQUE","IO_C_TORQUE","PLATEN_CURRENT","IO_P_CURRENT","CARRIER_CURRENT","IO_C_CURRENT","PLATEN_READY","PLC_P_READY","CARRIER_READY","PLC_C_READY","DRIVE_FAULT","PLC_DRIVE_ALARM","CURRENT_OK","IO_DRIVE_OK","CONTACT_PERMIT","CONTACT_ENABLE","PLC_Z_MOVE","CARRIER_Z_CMD","Z_POSITION","IO_HEAD_Z","PID_CENTER","ZONE_CENTER","PID_MIDDLE","ZONE_MIDDLE","PID_EDGE","ZONE_EDGE","PID_RING","RING_PRESSURE","CENTER_MEAS","IO_CENTER","MIDDLE_MEAS","IO_MIDDLE","EDGE_MEAS","IO_EDGE","RING_MEAS","IO_RING","GIMBAL_POS","IO_GIMBAL","FORCE_ZERO","PID_FORCE_ZERO","FORCE_VALID","PLC_FORCE_VALID","OVERFORCE","PLC_FORCE_TRIP","FORCE_LIMIT","PID_FORCE_LIMIT","PLC_SLURRY","PUMP_RUN","SLURRY_SPEED","PUMP_CMD","PUMP_TACH","IO_PUMP_RPM","SLURRY_FLOW","IO_FLOW","SLURRY_TEMP","IO_SL_TEMP","CONCENTRATION","IO_CONC","SLURRY_LEVEL","IO_TANK","PLC_MIXER","MIXER_RUN","PLC_NOZZLE","NOZZLE_CMD","NOZZLE_READY","IO_NOZZLE","PLC_COOL","COOL_RUN","COOL_READY","IO_COOL","INTERFACE_TEMP","IO_INTERFACE","WASTE_LEVEL","IO_WASTE","PLC_CONDITION","COND_RUN","PLC_COND_ARM","COND_ARM_CMD","COND_ARM_POS","IO_COND_ARM","PAD_WEAR","IO_PAD_WEAR","DISC_RUN","COND_MOTOR_RUN","PAD_SCAN_EN","PLC_PAD_WARN","PAD_DRAG_MON","PAD_WET_MON","PAD_FLAT_MON","PAD_TEMP_MON","OPTICAL_EN","METRO_ARM","OPTICAL_REF","EDDY_EN","ENDPOINT_CURRENT","ENDPOINT_LOAD","PROFILE_SCAN","PROFILE_VALID","TOPOGRAPHY_IN","DEFECT_INSPECT","DISHING_MON","EROSION_MON","ESTOP_NC","SAFE_STOP_IN","PLC_SAFE","GUARD_LOCK","ROBOT_TRANSFER","WAFER_LIFT","RINSE_ENABLE","BRUSH_EN","SPIN_DRY_EN","CARRIER_CLAMP","SAFETY_ALARM","HMI_CONTACT","HMI_SLURRY","HMI_EDGE","HMI_LOAD","HMI_METRO","RESET_READY","HMI_FAULT","CARRIER_DOWNFORCE_SENSOR_OUT","MULTIZONE_PRESSURE_PID_FORCE_FEEDBACK_IN"};
                foreach(string id in wireOrder[2..])
                    if(id is not ("SAFE_STOP_IN" or "CONTACT_ENABLE") && !pins.Exists(p=>p.name==id)) Pin(carrier,id,0,0,cyan,pins);
                var stopLoad=Part<SparkResistor>(bench,"Safety PLC stop input",0,0,steel);
                var safeInput=Pin(stopLoad,"SAFE_STOP_IN",0,0,gold,pins);
                var returnRail=pins.Find(pin=>pin.name=="PLC_GND");
                Set(stopLoad,("terminalA",safeInput),("terminalB",returnRail),("resistance",10000f));
                var emergency=Child(bench,"Emergency stop NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="ESTOP_NC")),("state",SparkSwitchState.Closed));
                var enableLoad=Part<SparkResistor>(bench,"Carrier contact input",0,0,steel);
                var enableNode=Pin(enableLoad,"CONTACT_ENABLE",0,0,gold,pins);
                Set(enableLoad,("terminalA",enableNode),("terminalB",returnRail),("resistance",1500f));
                var drive=Child(bench,"Carrier contact safety relay",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(drive,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="CONTACT_PERMIT")),("state",SparkSwitchState.Open));
                var contacts=new[]{drive};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel73Process>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel73VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 73 prefab.");
                SparkLevel73DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 73 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_73_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel73Process c,Font font)
        {
            var obj=new GameObject("Level 73 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,271),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the CMP workbench.",font,new Vector2(1080,28),new Vector2(0,271));
            c.meterText=UIText(panel,"Measurements","CMP PROCESS CONTROL",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-476f,139,c.Power);
            c.calSpeedButton=Control(panel,font,"CAL SPEED",-357f,139,c.CalSpeed);
            c.calForceButton=Control(panel,font,"CAL FORCE",-238f,139,c.CalForce);
            c.calSlurryButton=Control(panel,font,"CAL SLURRY",-119f,139,c.CalSlurry);
            c.calTorqueButton=Control(panel,font,"CAL TORQUE",0f,139,c.CalTorque);
            c.calEndpointButton=Control(panel,font,"CAL ENDPOINT",119f,139,c.CalEndpoint);
            c.inspectButton=Control(panel,font,"INSPECT PAD",238f,139,c.Inspect);
            c.conditionButton=Control(panel,font,"CONDITION PAD",357f,139,c.Condition);
            c.platenButton=Control(panel,font,"START PLATEN",476f,139,c.Platen);
            c.carrierButton=Control(panel,font,"START CARRIER",-476f,100,c.Carrier);
            c.slurryButton=Control(panel,font,"SLURRY PUMP",-357f,100,c.Slurry);
            c.coolButton=Control(panel,font,"COOLING",-238f,100,c.Cool);
            c.loadButton=Control(panel,font,"LOAD WAFER",-119f,100,c.Load);
            c.clampButton=Control(panel,font,"CLAMP WAFER",0f,100,c.Clamp);
            c.pidButton=Control(panel,font,"ZONE PID",119f,100,c.Pid);
            c.engageButton=Control(panel,font,"ENGAGE",238f,100,c.Engage);
            c.speedButton=Control(panel,font,"PLATEN RPM",357f,100,c.Speed);
            c.stopButton=Control(panel,font,"STOP / RETRACT",476f,100,c.Stop);
            c.unloadButton=Control(panel,font,"UNLOAD",-476f,61,c.Unload);
            c.rinseButton=Control(panel,font,"RINSE",-357f,61,c.Rinse);
            c.brushButton=Control(panel,font,"BRUSH CLEAN",-238f,61,c.Brush);
            c.dryButton=Control(panel,font,"SPIN DRY",-119f,61,c.Dry);
            c.metrologyButton=Control(panel,font,"METROLOGY",0f,61,c.Metrology);
            c.glazeButton=Control(panel,font,"PAD GLAZE",119f,61,c.Glaze);
            c.restrictButton=Control(panel,font,"SLURRY RESTRICT",238f,61,c.Restrict);
            c.driftButton=Control(panel,font,"EDGE DRIFT",357f,61,c.Drift);
            c.restoreButton=Control(panel,font,"RESTORE FAULT",476f,61,c.Restore);
            c.eStopButton=Control(panel,font,"E-STOP",-476f,22,c.EStop);
            c.resetButton=Control(panel,font,"RESET SAFETY",-357f,22,c.Reset);
            c.armButton=Control(panel,font,"W98 TEST",-238f,22,c.Arm);
            c.recalButton=Control(panel,font,"RECAL FORCE",-119f,22,c.Recal);
            c.verifyButton=Control(panel,font,"VERIFY ZERO",0f,22,c.Verify);
            c.certifyButton=Control(panel,font,"CERTIFY",119f,22,c.Certify);
            var plot=Rect(canvas.transform,"SCADA trends",new Vector2(330,181),new Vector2(518,7));
            c.plot=plot.gameObject.AddComponent<SparkLevel73Trend>();
            c.plot.raycastTarget=false;
            var legend=UIText(plot,"Trend legend","LIVE TRENDS  ·  FORCE  |  SLURRY  |  FILM nm  |  PAD",font,new Vector2(355,25),new Vector2(0,181));
            legend.fontSize=11;
            legend.color=new Color(.63f,.89f,.99f);

        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(114,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.71f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(114,37),Vector2.zero).fontSize=13;
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_72_Layout/Level_72_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Deposition Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_73" || targets.arraySize!=SparkLevel73Process.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel73Process.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_73"),("displayName","Polish to Perfection"),
                ("description","Commission the CMP workstation, polish and clean a wafer, and restore critical downforce feedback."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level73_cmp_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel73Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_73.asset");
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
                ("CONTROL_POWER",source),("ESTOP_INTERLOCK",emergency)
            };
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L73_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel73Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel73Process.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }


        private static T Part<T>(Transform parent,string name,float x,float z,Material material) where T:SparkComponent
        {
            var obj=Child(parent,name,new Vector3(x,.071f,z));
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
            obj.transform.localPosition=new Vector3(x,.071f,z); obj.transform.localScale=new Vector3(.075f,.018f,.075f);
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
            text.characterSize=.022f; text.fontSize=73; text.color=Color.white;
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
            string path=$"{Folder}/L73 {name}.mat";
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
