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
    public static class SparkLevel74Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_74_Layout";
        private const string PrefabPath = Folder + "/Level_74_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the lithography control network","Prepare the photosensitive wafer","Align and synchronize the scanner","Print the perfect circuit pattern","Survive the precision disturbances","Restore nanometre position feedback"
        };

        [MenuItem("Tools/Project Spark/Level 74/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 74 prefab already exists at " + PrefabPath);
                SparkLevel74DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_74.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_74.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.71f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.71f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.71f,.16f));

            var root = new GameObject("Level_74_Layout");
            try
            {
                var bench = Child(root.transform,"Lithography Workbench",Vector3.zero);
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
                foreach(var (plus,ret) in new[]{("PLC_5V","PLC_GND"),("IO_5V","IO_GND"),("STAGE_5V","STAGE_GND"),("DUV_5V","DUV_GND"),("LASER_5V","LASER_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"Process I/O endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                string[] wireOrder={"SUPPLY_POS","PLC_5V","SUPPLY_RET","PLC_GND","IO_5V","IO_GND","STAGE_5V","STAGE_GND","DUV_5V","DUV_GND","LASER_5V","LASER_GND","PLC_WAFER","WAFER_RUN","PLC_RETICLE","RETICLE_RUN","WAFER_X_SET","SERVO_X_CMD","WAFER_Y_SET","SERVO_Y_CMD","RETICLE_X_SET","RETICLE_X_CMD","RETICLE_Y_SET","RETICLE_Y_CMD","WAFER_ENC_X","IO_WAFER_X","WAFER_ENC_Y","IO_WAFER_Y","RETICLE_ENC_X","IO_RETICLE_X","RETICLE_ENC_Y","IO_RETICLE_Y","WAFER_HOME","IO_WAFER_HOME","RETICLE_HOME","IO_RET_HOME","WAFER_READY","PLC_WAFER_RDY","RETICLE_READY","PLC_RET_RDY","STAGE_FAULT","IO_STAGE_FAULT","SERVO_CURRENT","IO_SERVO_I","PLC_LASER","LASER_EN","LASER_VALID","IO_LASER_VALID","LASER_REF","SYNC_ZERO","LASER_Y_MON","IO_LASER_Y","MARK_A","ALIGN_MARK_A","MARK_B","ALIGN_MARK_B","ALIGN_THETA","WAFER_THETA","ALIGN_OK","PLC_ALIGN_OK","HEIGHT_SENSOR","FOCUS_HEIGHT","MAP_READY","IO_HEIGHT_MAP","Z_SET","Z_SERVO_CMD","TILT_X_SET","TILT_X_CMD","TILT_Y_SET","TILT_Y_CMD","FOCUS_ERR","IO_FOCUS_ERR","Z_FB","IO_Z_POS","FOCUS_OK","PLC_FOCUS_OK","SHUTTER_PERMIT","SHUTTER_ENABLE","PLC_DUV","DUV_RUN","DUV_LEVEL","IO_DUV_LEVEL","DUV_READY","PLC_DUV_RDY","PLC_SLIT","SLIT_CMD","SLIT_POS","IO_SLIT","SHUTTER_POS","IO_SHUTTER","PHOTO_CURRENT","DOSE_MEAS","DOSE_CAL","PHOTO_ZERO","DOSE_READY","PLC_DOSE_OK","NA_SET","APERTURE_CMD","NA_POS","IO_NA_POS","LENS_TEMP","IO_LENS_TEMP","LENS_COOL","COOL_CMD","PLC_SPIN","COATER_RUN","SPIN_TACH","IO_COAT_RPM","RESIST_PUMP","COAT_PUMP_CMD","RESIST_FLOW","IO_RESIST_FLOW","RESIST_THICK","IO_RESIST_NM","HOTPLATE_TEMP","IO_BAKE_TEMP","NOZZLE_CMD","DISPENSE_EN","COATER_VALID","COAT_SCAN_RDY","WAFER_TRANSFER","SOFTBAKE_CMD","POSTBAKE_TEMP","POSTBAKE_EN","DEVELOPER_EN","RINSE_EN","SPIN_DRY_EN","DRY_SPEED_MON","CD_SCANNER_EN","CD_FOCUS_MON","OVERLAY_EN","OVERLAY_POS","DEFECT_CAM_EN","METROLOGY_RDY","ESTOP_NC","SAFE_STOP_IN","PLC_SAFE","DUV_GUARD","TRIP_STAGE","TRIP_LENS","VIB_MONITOR","ISOLATOR_FAULT","ISOLATOR_EN","FOCUS_INTERLOCK","COAT_TRIP","SYNC_WAFER_X","SYNC_RETICLE_X","SYNC_LASER_OK","SCADA_DOSE","SCADA_FOCUS","SCADA_QUALITY","SCADA_SAFE","SCADA_MOTION","SCADA_LENS","SCADA_RESIST","SCADA_ALIGN","WAFER_STAGE_INTERFEROMETER_SIGNAL_OUT","STAGE_SYNCHRONIZATION_POSITION_FEEDBACK_IN"};
                foreach(string id in wireOrder[2..])
                    if(id is not ("SAFE_STOP_IN" or "SHUTTER_ENABLE") && !pins.Exists(p=>p.name==id)) Pin(carrier,id,0,0,cyan,pins);
                var stopLoad=Part<SparkResistor>(bench,"Safety PLC stop input",0,0,steel);
                var safeInput=Pin(stopLoad,"SAFE_STOP_IN",0,0,gold,pins);
                var returnRail=pins.Find(pin=>pin.name=="PLC_GND");
                Set(stopLoad,("terminalA",safeInput),("terminalB",returnRail),("resistance",10000f));
                var emergency=Child(bench,"Emergency stop NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="ESTOP_NC")),("state",SparkSwitchState.Closed));
                var enableLoad=Part<SparkResistor>(bench,"Exposure shutter input",0,0,steel);
                var enableNode=Pin(enableLoad,"SHUTTER_ENABLE",0,0,gold,pins);
                Set(enableLoad,("terminalA",enableNode),("terminalB",returnRail),("resistance",1500f));
                var drive=Child(bench,"Exposure shutter safety relay",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(drive,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="SHUTTER_PERMIT")),("state",SparkSwitchState.Open));
                var contacts=new[]{drive};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel74Process>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel74VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 74 prefab.");
                SparkLevel74DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 74 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_74_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel74Process c,Font font)
        {
            var obj=new GameObject("Level 74 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,271),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the lithography workstation.",font,new Vector2(1080,28),new Vector2(0,271));
            c.meterText=UIText(panel,"Measurements","LITHOGRAPHY SCANNER CONTROL",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-476f,139,c.Power);
            c.calPrepButton=Control(panel,font,"CAL PREP",-357f,139,c.CalPrep);
            c.loadButton=Control(panel,font,"LOAD WAFER",-238f,139,c.Load);
            c.coatButton=Control(panel,font,"SPIN COAT",-119f,139,c.Coat);
            c.softBakeButton=Control(panel,font,"SOFT BAKE",0f,139,c.SoftBake);
            c.transferButton=Control(panel,font,"TRANSFER",119f,139,c.Transfer);
            c.reticleButton=Control(panel,font,"LOAD RETICLE",238f,139,c.Reticle);
            c.clampButton=Control(panel,font,"CLAMP WAFER",357f,139,c.Clamp);
            c.homeButton=Control(panel,font,"HOME STAGES",476f,139,c.Home);
            c.calPositionButton=Control(panel,font,"CAL POSITION",-476f,100,c.CalPosition);
            c.alignButton=Control(panel,font,"ALIGN MARKS",-357f,100,c.Align);
            c.focusButton=Control(panel,font,"MAP FOCUS",-238f,100,c.Focus);
            c.isolateButton=Control(panel,font,"ISOLATION",-119f,100,c.Isolate);
            c.syncButton=Control(panel,font,"4:1 SYNC",0f,100,c.Sync);
            c.dryScanButton=Control(panel,font,"DRY SCAN",119f,100,c.DryScan);
            c.lampButton=Control(panel,font,"VIRTUAL LIGHT",238f,100,c.Lamp);
            c.calDoseButton=Control(panel,font,"CAL DOSE",357f,100,c.CalDose);
            c.apertureButton=Control(panel,font,"APERTURE",476f,100,c.Aperture);
            c.printButton=Control(panel,font,"EXPOSE 16",-476f,61,c.Print);
            c.stopButton=Control(panel,font,"STOP",-357f,61,c.Stop);
            c.postBakeButton=Control(panel,font,"POST BAKE",-238f,61,c.PostBake);
            c.developButton=Control(panel,font,"DEVELOP",-119f,61,c.Develop);
            c.rinseButton=Control(panel,font,"RINSE",0f,61,c.Rinse);
            c.dryButton=Control(panel,font,"SPIN DRY",119f,61,c.Dry);
            c.metrologyButton=Control(panel,font,"METROLOGY",238f,61,c.Metrology);
            c.nozzleButton=Control(panel,font,"NOZZLE RESTRICT",357f,61,c.Nozzle);
            c.shakeButton=Control(panel,font,"ISOLATION TEST",476f,61,c.Shake);
            c.lensButton=Control(panel,font,"LENS DRIFT",-476f,22,c.Lens);
            c.restoreButton=Control(panel,font,"RESTORE FAULT",-357f,22,c.Restore);
            c.eStopButton=Control(panel,font,"E-STOP",-238f,22,c.EStop);
            c.resetButton=Control(panel,font,"RESET SAFETY",-119f,22,c.Reset);
            c.armButton=Control(panel,font,"W102 TEST",0f,22,c.Arm);
            c.recalButton=Control(panel,font,"RECAL LASER",119f,22,c.Recal);
            c.referenceButton=Control(panel,font,"REFERENCE",238f,22,c.Reference);
            c.certifyButton=Control(panel,font,"CERTIFY",357f,22,c.Certify);
            var plot=Rect(canvas.transform,"SCADA trends",new Vector2(330,181),new Vector2(518,7));
            c.plot=plot.gameObject.AddComponent<SparkLevel74Trend>();
            c.plot.raycastTarget=false;
            var legend=UIText(plot,"Trend legend","16-FIELD MAP  ·  DOSE / OVERLAY / FOCUS",font,new Vector2(355,25),new Vector2(0,181));
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
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_73_Layout/Level_73_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("CMP Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_74" || targets.arraySize!=SparkLevel74Process.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel74Process.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_74"),("displayName","Align the Light"),
                ("description","Coat and align a wafer, scan sixteen fields, verify metrology and restore position feedback."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level74_litho_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel74Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_74.asset");
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
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L74_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel74Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel74Process.Gates[i];
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
            text.characterSize=.022f; text.fontSize=74; text.color=Color.white;
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
            string path=$"{Folder}/L74 {name}.mat";
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
