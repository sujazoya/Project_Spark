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
    public static class SparkLevel79Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_79_Layout";
        private const string PrefabPath = Folder + "/Level_79_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build Precision Bonding Network","Qualify Surfaces and Six Axis Alignment","Bond Logic and Memory","Complete and Test Four Chiplets","Survive Assembly Disturbances","Restore Critical Laser Feedback"
        };

        [MenuItem("Tools/Project Spark/Level 79/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 79 prefab already exists at " + PrefabPath);
                SparkLevel79DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_79.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_79.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.71f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.71f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.71f,.16f));

            var root = new GameObject("Level_79_Layout");
            try
            {
                var bench = Child(root.transform,"Bonding Workbench",Vector3.zero);
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
                foreach(var (plus,ret) in new[]{("PLC_5V","PLC_GND"),("VISION_5V","VISION_GND"),("MOTION_5V","MOTION_GND"),("LASER_5V","LASER_GND"),("SAFE_5V","SAFE_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"Process I/O endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                string[] wireOrder={"SUPPLY_POS","PLC_5V","SUPPLY_RET","PLC_GND","VISION_5V","VISION_GND","MOTION_5V","MOTION_GND","LASER_5V","LASER_GND","SAFE_5V","SAFE_GND","MOTION_ENABLE","GANTRY_ENABLE","X_CMD","X_SERVO","X_ENC","X_FEEDBACK","Y_CMD","Y_SERVO","Y_ENC","Y_FEEDBACK","Z_CMD","Z_SERVO","Z_ENC","Z_FEEDBACK","POSE_CMD","HEXAPOD_DRIVE","HEX_READY","POSE_READY","NANO_X_CMD","NANO_X_DRIVE","NANO_X_OUT","FINE_X","NANO_Y_CMD","NANO_Y_DRIVE","NANO_Y_OUT","FINE_Y","THETA_CMD","THETA_DRIVE","THETA_OUT","THETA_IN","TILT_CMD","TILT_DRIVE","TILT_OUT","TILT_IN","POSE_SAFE","MOTION_INTERLOCK","CAM_A_OUT","VISION_A","CAM_B_OUT","VISION_B","IR_REG_OUT","IR_REG_IN","MOIRE_OUT","OVERLAY_DIAG","WARP_OUT","WARP_IN","CAM_A_RES","ALIGN_A","CAM_B_RES","ALIGN_B","VISION_AGREE","ALIGN_VALID","LASER_INTERFEROMETER_POSITION_OUT","HEXAPOD_FINE_POSITION_IN","LASER_TELEM","LASER_CMD","LASER_ENABLE","LASER_HEALTH","SAFETY_LASER","LASER_CAL_OK","POSE_QUAL","BOND_POSE","OVERLAY_VIEW","IR_CERT","REG_CERT","WARP_COMP","WARP_AUTH","OPTICS_VALID","MOTION_VISION","GRIP_CMD","UPPER_GRIP","RELEASE_OUT","CHUCK_RELEASE_MON","CHUCK_READY","RELEASE_OK","LOWER_CMD","BASE_GRIP","BASE_HELD","BASE_HOLD_OK","FORCE_CMD","FORCE_ENABLE","FORCE_ARRAY","FORCE_IN","CONTACT_PERMIT","CONTACT_ENABLE","CONTACT_OUT","CONTACT_POSITION","GAP_OUT","GAP_IN","WAVE_OUT","WAVE_IN","WAVE_CERT","FRONT_CERT","CONTACT_CMD","CONTACT_REQ","RETRACT_CMD","RETRACT_OVERRIDE","APPROACH_OUT","SAFE_APPROACH","CHUCK_CAL","RELEASE_CAL","DIE_CERT","KNOWN_GOOD","PARTICLE_OUT","PARTICLES_IN","FLATNESS_OUT","FLATNESS_IN","ENERGY_OUT","SURFACE_ENERGY","CLEAN_CMD","DRY_CLEAN","CLEAN_OUT","CLEAN_DONE","ACTIVATE_CMD","SURFACE_ACT","ACTIVATE_OUT","ACT_DONE","ESD_GUARD","ESD_OK","REJECT_SENSOR","QUARANTINE_STATE","REJECT_CMD","REJECT_BIN","ALIGNER_OUT","BASE_ALIGNED","SURFACE_CERT","SURFACE_OK","LOCK_SAFE","LOADLOCK_OK","ANNEAL_ENABLE","HEATER_SET","HEATER_ACTUAL","THERMAL_TEMP","EXPANSION_TELEM","BOND_STRESS","THERMAL_SAMPLE","PROFILE_OK","THERMAL_CYCLE","THERMAL_DONE","STACK_WARP","ANNEAL_SAFE","HEATER_OFF","STRESS_OK","ACOUSTIC_REQ","ACOUSTIC_MAP","IR_SCAN_REQ","IR_VOID_MAP","SEAM_VIEW","ADHESION_RESULT","KELVIN_RESULT","DAISY_CHAIN","ROUTING_TEST","BER_RESULT","LINK_TEST","SYSTEM_CERT","BOND_TELEM","LINK_TELEM","ESTOP_NC","SAFE_STOP_IN","GANTRY_OFF","HEX_HOLD","RELEASE_INHIBIT","APPROACH_INHIBIT","AXIS_STATUS","ALIGN_STATUS","PLC_STATE","OPERATOR_CMD","POSE_TELEM","OVERLAY_MAP","BOND_MAP","CHIP_GRAPH","OBJECTIVE_EVENT","MOTION_RESET","FAULT_LATCH","LASER_LOSS","FINAL_CERT"};
                foreach(string id in wireOrder[2..])
                    if(id is not ("SAFE_STOP_IN" or "CONTACT_ENABLE") && !pins.Exists(p=>p.name==id)) Pin(carrier,id,0,0,cyan,pins);
                var stopLoad=Part<SparkResistor>(bench,"Safety PLC stop input",0,0,steel);
                var safeInput=Pin(stopLoad,"SAFE_STOP_IN",0,0,gold,pins);
                var returnRail=pins.Find(pin=>pin.name=="PLC_GND");
                Set(stopLoad,("terminalA",safeInput),("terminalB",returnRail),("resistance",10000f));
                var emergency=Child(bench,"Emergency stop NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="ESTOP_NC")),("state",SparkSwitchState.Closed));
                var enableLoad=Part<SparkResistor>(bench,"Contact permit input",0,0,steel);
                var enableNode=Pin(enableLoad,"CONTACT_ENABLE",0,0,gold,pins);
                Set(enableLoad,("terminalA",enableNode),("terminalB",returnRail),("resistance",1500f));
                var drive=Child(bench,"Precision contact safety relay",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(drive,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="CONTACT_PERMIT")),("state",SparkSwitchState.Open));
                var contacts=new[]{drive};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel79Process>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel79VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 79 prefab.");
                SparkLevel79DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 79 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_79_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel79Process c,Font font)
        {
            var obj=new GameObject("Level 79 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,271),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the hybrid bonding workstation.",font,new Vector2(1080,28),new Vector2(0,271));
            c.meterText=UIText(panel,"Measurements","SIX-AXIS HYBRID BOND ASSEMBLY",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-476f,139,c.Power);
            c.homeButton=Control(panel,font,"HOME MOTION",-357f,139,c.Home);
            c.cameraButton=Control(panel,font,"CAL CAMERAS",-238f,139,c.Camera);
            c.laserButton=Control(panel,font,"CAL LASER",-119f,139,c.Laser);
            c.waferButton=Control(panel,font,"LOAD BASE",0f,139,c.Wafer);
            c.inspectButton=Control(panel,font,"INSPECT SURFACES",119f,139,c.Inspect);
            c.prepButton=Control(panel,font,"PREP SURFACES",238f,139,c.Prep);
            c.loadButton=Control(panel,font,"LOAD CHIPLET",357f,139,c.Load);
            c.alignButton=Control(panel,font,"ALIGN SIX AXES",476f,139,c.Align);
            c.approachButton=Control(panel,font,"APPROACH",-476f,100,c.Approach);
            c.growButton=Control(panel,font,"INITIATE BOND",-357f,100,c.Grow);
            c.scanButton=Control(panel,font,"SCAN INTERFACE",-238f,100,c.Scan);
            c.annealButton=Control(panel,font,"CONSOLIDATE",-119f,100,c.Anneal);
            c.testButton=Control(panel,font,"TEST FOUR CHIPLETS",0f,100,c.Test);
            c.stopButton=Control(panel,font,"STOP",119f,100,c.Stop);
            c.discardButton=Control(panel,font,"DISCARD ASSEMBLY",238f,100,c.Discard);
            c.driftButton=Control(panel,font,"CAMERA DRIFT",357f,100,c.Drift);
            c.chuckButton=Control(panel,font,"CHUCK ANOMALY",476f,100,c.Chuck);
            c.particleButton=Control(panel,font,"PARTICLE FAULT",-476f,61,c.Particle);
            c.restoreButton=Control(panel,font,"RESTORE",-357f,61,c.Restore);
            c.eStopButton=Control(panel,font,"E-STOP",-238f,61,c.EStop);
            c.resetButton=Control(panel,font,"RESET SAFETY",-119f,61,c.Reset);
            c.armButton=Control(panel,font,"ARM W037 TEST",0f,61,c.Arm);
            c.verifyButton=Control(panel,font,"VERIFY LASER",119f,61,c.Verify);
            c.certifyButton=Control(panel,font,"CERTIFY",238f,61,c.Certify);
            var plot=Rect(canvas.transform,"SCADA trends",new Vector2(330,181),new Vector2(518,7));
            c.plot=plot.gameObject.AddComponent<SparkLevel79Trend>();
            c.plot.raycastTarget=false;
            var legend=UIText(plot,"Trend legend","16-REGION MAP  ·  OVERLAY / BOND / VOIDS",font,new Vector2(355,25),new Vector2(0,181));
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
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_78_Layout/Level_78_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Dielectric Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_79" || targets.arraySize!=SparkLevel79Process.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel79Process.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_79"),("displayName","Bond the Worlds"),
                ("description","Align, bond and test four chiplets, then restore W037 position feedback."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level79_bond_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel79Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_79.asset");
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
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L79_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel79Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel79Process.Gates[i];
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
            string path=$"{Folder}/L79 {name}.mat";
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

