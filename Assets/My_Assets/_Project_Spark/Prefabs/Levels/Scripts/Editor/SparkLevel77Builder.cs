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
    public static class SparkLevel77Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_77_Layout";
        private const string PrefabPath = Folder + "/Level_77_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the Triple-Stage Copper CMP Network","Commission the Three Polishing Stations","Remove Copper Without Destroying the Lines","Reveal and Electrically Isolate the Circuits","Survive Copper Finishing Disturbances","Restore Critical Barrier Endpoint Feedback"
        };

        [MenuItem("Tools/Project Spark/Level 77/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 77 prefab already exists at " + PrefabPath);
                SparkLevel77DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_77.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_77.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.71f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.71f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.71f,.16f));

            var root = new GameObject("Level_77_Layout");
            try
            {
                var bench = Child(root.transform,"Finishing Workbench",Vector3.zero);
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
                foreach(var (plus,ret) in new[]{("PLC_5V","PLC_GND"),("IO_5V","IO_GND"),("DRV_5V","DRV_GND"),("OPT_5V","OPT_GND"),("SAFE_5V","SAFE_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"Process I/O endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                string[] wireOrder={"SUPPLY_POS","PLC_5V","SUPPLY_RET","PLC_GND","IO_5V","IO_GND","DRV_5V","DRV_GND","OPT_5V","OPT_GND","SAFE_5V","SAFE_GND","BULK_RPM_CMD","BULK_DRV_SET","BULK_RPM_OUT","PLC_BULK_RPM","BULK_SPIN_CMD","BULK_SPIN_SET","BULK_SPIN_OUT","PLC_BULK_SPIN","BULK_LIFT_CMD","BULK_LIFT_SET","BULK_LIFT_OUT","PLC_BULK_LIFT","BULK_FORCE_OUT","PLC_BULK_FORCE","ZONE_FORCE_CMD","ZONE_FORCE_SET","ZONE_FORCE_OUT","PLC_ZONE_FORCE","SOFT_RPM_CMD","SOFT_DRV_SET","SOFT_RPM_OUT","PLC_SOFT_RPM","SOFT_SPIN_CMD","SOFT_SPIN_SET","SOFT_FORCE_OUT","PLC_SOFT_FORCE","BARR_RPM_CMD","BARR_DRV_SET","BARR_RPM_OUT","PLC_BARR_RPM","BARR_LIFT_CMD","BARR_LIFT_SET","BARR_LIFT_OUT","PLC_BARR_LIFT","TORQUE_OUT","PLC_TORQUE","SLURRY_PUMP_CMD","SLURRY_PUMP_RUN","SLURRY_PUMP_OUT","PLC_SLURRY_PUMP","SLURRY_FLOW_OUT","PLC_SLURRY_FLOW","SLURRY_NOZZLE_CMD","SLURRY_DISPENSE","SLURRY_NOZZLE_OUT","PLC_SLURRY_NOZZLE","PAD_CMD","PAD_RUN","PAD_READY","PLC_PAD_READY","BARR_PUMP_CMD","BARR_PUMP_RUN","BARR_PUMP_OUT","PLC_BARR_PUMP","BARR_FLOW_OUT","PLC_BARR_FLOW","BARR_NOZZLE_OUT","PLC_BARR_NOZZLE","SOFT_FLOW_CMD","SOFT_MANIFOLD","SOFT_FLOW_OUT","PLC_SOFT_FLOW","BRUSH_CMD","BRUSH_RUN","BRUSH_RPM_OUT","PLC_BRUSH_RPM","MEGA_CMD","MEGA_RUN","RINSE_READY","PLC_RINSE","DRY_RPM_OUT","PLC_DRY_RPM","EDDY_OUT","BULK_THICKNESS","COPPER_OPT_OUT","BULK_CLEAR_IN","FORCE_SAMPLE","BULK_FORCE_IN","PAD_WEAR_OUT","PAD_WEAR_IN","COPPER_MAP_OUT","THICK_TELEM","LOAD_TORQUE_OUT","TORQUE_TELEM","THERMAL_OUT","PLC_TEMP","BARRIER_MAP_OUT","PLC_BARR_MAP","BARRIER_ENDPOINT_SPECTROMETER_SIGNAL_OUT","THREE_STAGE_CMP_CONTROLLER_ENDPOINT_IN","WINDOW_HEALTH_OUT","OPTICS_READY","SPECTRAL_TELEM_OUT","SCADA_SPECTRAL","PROFILE_OUT","PLC_PROFILE","RESIDUE_OUT","PLC_RESIDUE","BULK_DONE_OUT","STAGE_B_PERMIT","OPTICAL_HEALTH_OUT","OPTICAL_SAFETY_IN","BARR_RETRACT_CMD","BARR_RETRACT","SELECTIVITY_CMD","SELECTIVITY_START","RATIO_OUT","PLC_RATIO","BARR_FLUID_READY","PLC_BARR_FLUID","BARR_FEED_CMD","BARR_FLUID_SET","BARR_FLOW_SAMPLE","SELECT_FLOW_IN","SOFT_FLUID_OUT","SELECT_SOFT_IN","SELECT_ALARM","SELECT_CLEAR","SELECT_COPPER","SELECT_TEMP","SCADA_RATIO","SOFT_MODE","REMOVAL_RATIO","POLISH_PERMIT","POLISH_ENABLE","CORROSION_RISK","PROTECT_ENABLE","PROTECT_STATUS","CLEAN_RESIDUE","BRUSH_JOB","BRUSH_DONE","MEGA_JOB","MEGA_DONE","RINSE_SUPPLY","DRY_DONE","DEFECT_COUNT","SCADA_CLEAN","INTERLINE_LEAK","VIA_KELVIN","DIELECTRIC_TEST","SURFACE_DEFECTS","GEOMETRY_MAP","CONDUCTIVE_MAP","INCOMING_SCAN","SCADA_VIA","SCADA_ISOLATION","SCADA_DIELECTRIC","WAFER_CERT","TEST_CAL","ESTOP_NC","SAFE_STOP_IN","BULK_SAFE_OFF","SOFT_SAFE_OFF","BARR_SAFE_OFF","BULK_SAFE_LIFT","SOFT_SAFE_LIFT","BARR_SAFE_LIFT","SAFE_OPTICS","STAGE_CONTROL","SCADA_SAFE","OPERATOR_CMD","SCADA_PLC","OBJECTIVE_EVENT","SCADA_CERT"};
                foreach(string id in wireOrder[2..])
                    if(id is not ("SAFE_STOP_IN" or "POLISH_ENABLE") && !pins.Exists(p=>p.name==id)) Pin(carrier,id,0,0,cyan,pins);
                var stopLoad=Part<SparkResistor>(bench,"Safety PLC stop input",0,0,steel);
                var safeInput=Pin(stopLoad,"SAFE_STOP_IN",0,0,gold,pins);
                var returnRail=pins.Find(pin=>pin.name=="PLC_GND");
                Set(stopLoad,("terminalA",safeInput),("terminalB",returnRail),("resistance",10000f));
                var emergency=Child(bench,"Emergency stop NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="ESTOP_NC")),("state",SparkSwitchState.Closed));
                var enableLoad=Part<SparkResistor>(bench,"Polishing permit input",0,0,steel);
                var enableNode=Pin(enableLoad,"POLISH_ENABLE",0,0,gold,pins);
                Set(enableLoad,("terminalA",enableNode),("terminalB",returnRail),("resistance",1500f));
                var drive=Child(bench,"Polishing safety relay",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(drive,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="POLISH_PERMIT")),("state",SparkSwitchState.Open));
                var contacts=new[]{drive};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel77Process>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel77VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 77 prefab.");
                SparkLevel77DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 77 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_77_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel77Process c,Font font)
        {
            var obj=new GameObject("Level 77 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,271),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the copper CMP workstation.",font,new Vector2(1080,28),new Vector2(0,271));
            c.meterText=UIText(panel,"Measurements","TRIPLE-STAGE COPPER CMP",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-476f,139,c.Power);
            c.flowButton=Control(panel,font,"PRIME FLOW",-357f,139,c.Flow);
            c.drivesButton=Control(panel,font,"CHECK DRIVES",-238f,139,c.Drives);
            c.padsButton=Control(panel,font,"QUALIFY PADS",-119f,139,c.Pads);
            c.calCuButton=Control(panel,font,"CAL COPPER",0f,139,c.CalCu);
            c.calBarrierButton=Control(panel,font,"CAL OPTICS",119f,139,c.CalBarrier);
            c.loadButton=Control(panel,font,"LOAD WAFER",238f,139,c.Load);
            c.inspectButton=Control(panel,font,"INSPECT INCOMING",357f,139,c.Inspect);
            c.bulkButton=Control(panel,font,"A · BULK",476f,139,c.Bulk);
            c.transferBButton=Control(panel,font,"TRANSFER B",-476f,100,c.TransferB);
            c.softButton=Control(panel,font,"B · SOFT LAND",-357f,100,c.Soft);
            c.transferCButton=Control(panel,font,"TRANSFER C",-238f,100,c.TransferC);
            c.barrierButton=Control(panel,font,"C · BARRIER",-119f,100,c.Barrier);
            c.stopButton=Control(panel,font,"STOP POLISH",0f,100,c.Stop);
            c.unloadButton=Control(panel,font,"UNLOAD",119f,100,c.Unload);
            c.protectButton=Control(panel,font,"SURFACE PROTECT",238f,100,c.Protect);
            c.brushButton=Control(panel,font,"BRUSH",357f,100,c.Brush);
            c.megaButton=Control(panel,font,"MEGASONIC",476f,100,c.Mega);
            c.rinseButton=Control(panel,font,"RINSE",-476f,61,c.Rinse);
            c.dryButton=Control(panel,font,"SPIN DRY",-357f,61,c.Dry);
            c.geometryButton=Control(panel,font,"GEOMETRY",-238f,61,c.Geometry);
            c.electricalButton=Control(panel,font,"ELECTRICAL",-119f,61,c.Electrical);
            c.foulButton=Control(panel,font,"VIEWPORT FOUL",0f,61,c.Foul);
            c.driftButton=Control(panel,font,"SELECTIVITY DRIFT",119f,61,c.Drift);
            c.corrodeButton=Control(panel,font,"CORROSION",238f,61,c.Corrode);
            c.restoreButton=Control(panel,font,"RESTORE",357f,61,c.Restore);
            c.eStopButton=Control(panel,font,"E-STOP",476f,61,c.EStop);
            c.resetButton=Control(panel,font,"RESET SAFETY",-476f,22,c.Reset);
            c.armButton=Control(panel,font,"ARM W055 TEST",-357f,22,c.Arm);
            c.recalButton=Control(panel,font,"RECAL OPTICS",-238f,22,c.Recal);
            c.verifyButton=Control(panel,font,"VERIFY OPTICS",-119f,22,c.Verify);
            c.certifyButton=Control(panel,font,"CERTIFY",0f,22,c.Certify);
            var plot=Rect(canvas.transform,"SCADA trends",new Vector2(330,181),new Vector2(518,7));
            c.plot=plot.gameObject.AddComponent<SparkLevel77Trend>();
            c.plot.raycastTarget=false;
            var legend=UIText(plot,"Trend legend","16-REGION MAP  ·  COPPER / BARRIER",font,new Vector2(355,25),new Vector2(0,181));
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
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_76_Layout/Level_76_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Plating Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_77" || targets.arraySize!=SparkLevel77Process.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel77Process.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_77"),("displayName","Reveal the Circuit"),
                ("description","Polish copper in three stages, certify the isolated circuits, and restore W055 optical feedback."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level77_finish_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel77Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_77.asset");
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
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L77_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel77Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel77Process.Gates[i];
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
            string path=$"{Folder}/L76 {name}.mat";
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
