#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ProjectSpark.Gameplay;
using ProjectSpark.Visuals;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel82Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_82_Layout";
        private const string PrefabPath = Folder + "/Level_82_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the EMC Qualification Network","Calibrate the Blackstar Chamber","Silence Conducted and Radiated Emissions","Prove RF Immunity","Survive Three EMC Failures","Restore Critical RF Field Feedback"
        };

        [MenuItem("Tools/Project Spark/Level 82/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 82 prefab already exists at " + PrefabPath);
                SparkLevel82DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_82.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_82.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.71f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.71f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.71f,.16f));

            var root = new GameObject("Level_82_Layout");
            try
            {
                var bench = Child(root.transform,"EMC Workbench",Vector3.zero);
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
                foreach(var (plus,ret) in new[]{("PLC_5V","PLC_GND"),("SCAN_5V","SCAN_GND"),("EMI_5V","EMI_GND"),("FIELD_5V","FIELD_GND"),("SAFE_5V","SAFE_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"Process I/O endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                string[] wireOrder={"SUPPLY_POS","PLC_5V","SUPPLY_RET","PLC_GND","SCAN_5V","SCAN_GND","EMI_5V","EMI_GND","FIELD_5V","FIELD_GND","SAFE_5V","SAFE_GND","AZ_CMD","TABLE_SET","AZ_ENCODER","AZ_RAW","AZ_ACTUAL","GEAR_CMD","TABLE_DRIVE","GEAR_READY","GEAR_OK","MAST_CMD","HEIGHT_SET","MAST_ENCODER","HEIGHT_ACTUAL","POL_CMD","POL_SET","POL_ENCODER","POL_ACTUAL","ANT_READY","ANT_CHECK","ANT_MEASURE","ANT_STATUS","XY_CMD","GANTRY_XY","XY_ENCODER","XY_ACTUAL","Z_CMD","GANTRY_Z","Z_ENCODER","Z_ACTUAL","MAG_CMD","H_PROBE","ELEC_CMD","E_PROBE","AZ_HOME","HOME_SET","LISN_CMD","LISN_ENABLE","LISN_OUT","CONDUCTED_IN","CONDSPEC_OUT","EMI_COND","CM_CLAMP","CM_I","DM_CLAMP","DM_I","CM_CAL","CM_ZERO","DM_CAL","DM_ZERO","BOND_MON","CHASSIS_OK","TERM_OUT","SHIELD_TERM","HARNESS_CM","CM_MON","HARNESS_DM","DM_MON","CHOKE_OUT","CHOKE_ATTEN","LC_OUT","LC_ATTEN","FEED_OUT","DC_PATH","SWITCH_OUT","RIPPLE_IN","COND_SCAN","COND_MARGIN","BW_CMD","EMI_BW","SPEC_REF","EMI_CAL","SCADA_COND","CONDUCT_CMD","CONDUCT_RUN","RADIATE_CMD","RADIATE_RUN","ANT_RF","EMI_RF","EMI_OUT","SPEC_IN","SCADA_RAD","SPEC_CAL","SPEC_ZERO","PRESEL_OUT","PRESEL_IN","FILTER_CMD","EMI_FILTER","HEIGHT_ACT","HEIGHT_TAG","POL_ACT","POL_TAG","AZ_ACT","AZ_TAG","H_OUT","H_SAMPLE","E_OUT","E_SAMPLE","SEAM_OUT","SEAM_SAMPLE","WORST_OUT","WORST_VALID","PEAK_OUT","PEAK_CERT","CAL_SRC","CAL_REF","HEATMAP_OUT","HEATMAP_IN","NEAR_CMD","PROBE_ENABLE","GASKET_CMD","GASKET_SET","GASKET_OUT","GASKET_ACT","CLAMP_CMD","SHIELD_CLAMP","CLAMP_OUT","CLAMP_CONTACT","CHOKE_CMD","CHOKE_SET","CHOKE_ACT","CHOKE_OK","LC_CMD","LC_SET","LC_ACT","LC_OK","FERRITE_CMD","FERRITE_SET","FERRITE_ACT","FERRITE_OK","FEED_CMD","FEED_SET","FEED_ACT","FEED_OK","ROUTE_CMD","ROUTE_SET","ROUTE_ACTUAL","BOND_CHECK","BOND_INTEGRITY","SENSOR_FILTER","SENSOR_RESP","RF_SWEEP_REQ","RF_WAVE","IMMUNITY_OK","AMP_ACTUAL","FIELD_AUX","UNIFORMITY_CMD","FIELD_REF","FIELD_CAL","SWEEP_TELEM","COUPLE_FIXTURE","FIXTURE_OK","FIXTURE_STOP","SENSOR_CHECK","OPTICAL_CHECK","CLOCK_CHECK","MISSION_REQ","ADC_SAMPLE","ADC_DEV","PACKET_JITTER","CLOCK_JITTER","ADC_COUPLING","SPREAD_TELEM","SENSOR_FILTERED","IR_THERMAL","WORKLOAD_OK","CMD_SAFE","CM_FAULT","RETURN_FAULT","SEAM_FAULT","DESENSE_FAULT","DIAGNOSE","SCADA_REPAIR","RF_ARM_REQ","DOOR_NC","DOOR_CLOSED_IN","RF_FIELD_MEASURED_OUT","RF_SAFE_FIELD_FEEDBACK_IN","SCADA_FIELD","AMP_PERMIT","AMP_ENABLE","RF_OVERRIDE","ESTOP_NC","SAFE_STOP_IN","AMP_DISABLED","FIELD_HEALTH","FIELD_LIMIT","EXPOSURE_AZ","EXPOSURE_HEIGHT","EUT_VALID","SEAL_OK","RF_LATCH","RF_RESET","OBJECTIVE_STATE","EMC_CERT"};
                foreach(string id in wireOrder[2..])
                    if(id is not ("SAFE_STOP_IN" or "AMP_ENABLE" or "DOOR_CLOSED_IN") && !pins.Exists(p=>p.name==id)) Pin(carrier,id,0,0,cyan,pins);
                var stopLoad=Part<SparkResistor>(bench,"Safety PLC stop input",0,0,steel);
                var safeInput=Pin(stopLoad,"SAFE_STOP_IN",0,0,gold,pins);
                var returnRail=pins.Find(pin=>pin.name=="PLC_GND");
                Set(stopLoad,("terminalA",safeInput),("terminalB",returnRail),("resistance",10000f));
                var emergency=Child(bench,"Emergency stop NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="ESTOP_NC")),("state",SparkSwitchState.Closed));
                var doorLoad=Part<SparkResistor>(bench,"Door interlock monitor",0,0,steel);
                var doorInput=Pin(doorLoad,"DOOR_CLOSED_IN",0,0,gold,pins);
                Set(doorLoad,("terminalA",doorInput),("terminalB",returnRail),("resistance",10000f));
                var door=Child(bench,"Chamber door interlock",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(door,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="DOOR_NC")),("state",SparkSwitchState.Open));
                var enableLoad=Part<SparkResistor>(bench,"RF amplifier permit input",0,0,steel);
                var enableNode=Pin(enableLoad,"AMP_ENABLE",0,0,gold,pins);
                Set(enableLoad,("terminalA",enableNode),("terminalB",returnRail),("resistance",1500f));
                var drive=Child(bench,"RF amplifier safety relay",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(drive,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="AMP_PERMIT")),("state",SparkSwitchState.Open));
                var contacts=new[]{drive};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel82Process>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.doorSwitch=door;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel82VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 82 prefab.");
                SparkLevel82DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 82 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_82_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel82Process c,Font font)
        {
            var obj=new GameObject("Level 82 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,271),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the EMC qualification workstation.",font,new Vector2(1080,28),new Vector2(0,271));
            c.meterText=UIText(panel,"Measurements","BLACKSTAR EMC QUALIFICATION",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-476f,139,c.Power);
            c.doorButton=Control(panel,font,"CHAMBER DOOR",-357f,139,c.Door);
            c.calMotionButton=Control(panel,font,"CAL MOTION",-238f,139,c.CalMotion);
            c.calEmiButton=Control(panel,font,"CAL RECEIVER",-119f,139,c.CalEmi);
            c.calFieldButton=Control(panel,font,"CAL FIELD PROBE",0f,139,c.CalField);
            c.conductButton=Control(panel,font,"SCAN CONDUCTED",119f,139,c.Conduct);
            c.radiateButton=Control(panel,font,"SCAN RADIATED",238f,139,c.Radiate);
            c.nearButton=Control(panel,font,"NEAR-FIELD E/H",357f,139,c.Near);
            c.harnessButton=Control(panel,font,"ROUTE + FERRITE",476f,139,c.Harness);
            c.gasketButton=Control(panel,font,"SEAL GASKET",-476f,100,c.Gasket);
            c.filterButton=Control(panel,font,"FIT POWER FILTER",-357f,100,c.Filter);
            c.sensorButton=Control(panel,font,"FIT SENSOR FILTER",-238f,100,c.Sensor);
            c.armRfButton=Control(panel,font,"ARM RF",-119f,100,c.ArmRf);
            c.immunityButton=Control(panel,font,"RUN IMMUNITY",0f,100,c.Immunity);
            c.stopButton=Control(panel,font,"STOP RF",119f,100,c.Stop);
            c.harnessFaultButton=Control(panel,font,"COMMON-MODE FAULT",238f,100,c.HarnessFault);
            c.seamFaultButton=Control(panel,font,"SEAM GAP",357f,100,c.SeamFault);
            c.sensorFaultButton=Control(panel,font,"SENSOR DESENSE",476f,100,c.SensorFault);
            c.diagnoseButton=Control(panel,font,"DIAGNOSE",-476f,61,c.Diagnose);
            c.verifyButton=Control(panel,font,"VERIFY SAFE",-357f,61,c.Verify);
            c.resetButton=Control(panel,font,"RESET SAFETY",-238f,61,c.Reset);
            c.armButton=Control(panel,font,"ARM W119 TEST",-119f,61,c.Arm);
            c.eStopButton=Control(panel,font,"E-STOP",0f,61,c.EStop);
            c.certifyButton=Control(panel,font,"CERTIFY",119f,61,c.Certify);
            c.openFeedbackButton=Control(panel,font,"OPEN W119",238f,61,c.OpenFeedbackWire);
            c.cableViewButton=Control(panel,font,"CABLE VIEW",357f,61,c.ToggleCableView);
            Control(panel,font,"RESET VIEW",476f,61,c.ResetWorkbenchView).interactable=true;
            var plot=Rect(canvas.transform,"SCADA trends",new Vector2(330,181),Vector2.zero);
            plot.anchorMin=plot.anchorMax=plot.pivot=Vector2.one;
            plot.anchoredPosition=new Vector2(-24,-22);
            c.plot=plot.gameObject.AddComponent<SparkLevel82Trend>();
            c.plot.raycastTarget=false;
            var legend=UIText(plot,"Trend legend","EMC MEASUREMENTS",font,new Vector2(330,22),new Vector2(0,149));
            legend.fontSize=16;
            legend.color=new Color(.63f,.89f,.99f);
            for(int channel=0;channel<4;channel++)
            {
                string id=new[]{"POLAR","COND","E/H","RF"}[channel];
                var axis=UIText(plot,"Band "+id,id,font,new Vector2(50,16),
                    new Vector2(-112+channel*79,17));
                axis.fontSize=13;
            }

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
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_81_Layout/Level_81_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Reliability Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_82" || targets.arraySize!=SparkLevel82Process.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel82Process.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_82"),("displayName","Silence the Spectrum"),
                ("description","Measure and repair EMC paths, qualify protected immunity, and restore the trusted field probe."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level82_emc_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel82Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_82.asset");
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
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L82_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel82Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel82Process.Gates[i];
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
            text.characterSize=.022f; text.fontSize=76; text.color=Color.white;
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
            string path=$"{Folder}/L82 {name}.mat";
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





