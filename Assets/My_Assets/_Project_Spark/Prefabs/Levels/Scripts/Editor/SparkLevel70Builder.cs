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
    public static class SparkLevel70Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_70_Layout";
        private const string PrefabPath = Folder + "/Level_70_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the environmental control network","Commission clean air circulation","Master environmental stability","Achieve particle cleanliness","Survive the contamination breach","Restore critical pressure feedback"
        };

        [MenuItem("Tools/Project Spark/Level 70/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 70 prefab already exists at " + PrefabPath);
                SparkLevel70DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_70.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_70.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.70f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.70f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.70f,.16f));

            var root = new GameObject("Level_70_Layout");
            try
            {
                var bench = Child(root.transform,"Cleanroom Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Temporary board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.70f),board);
                var pins = new List<SparkTerminal>();
                SparkPowerSupply Supply(string name, float voltage, float current)
                {
                    var part=Part<SparkPowerSupply>(bench,name,0,0,steel);
                    Set(part,("outputVoltage",voltage),("currentLimit",current),("state",SparkPowerSupplyState.Active));
                    return part;
                }
                var source=Supply("CONTROL_POWER",5f,.70f);
                var positive=Pin(source,"SUPPLY_POS",0,0,red,pins,SparkTerminalPolarity.Positive);
                var ground=Pin(source,"SUPPLY_RET",0,0,blue,pins,SparkTerminalPolarity.Negative);
                Set(source,("positiveTerminal",positive),("negativeTerminal",ground));
                foreach(var (plus,ret) in new[]{("PLC_5V","PLC_GND"),("IO_5V","IO_GND"),("PID_5V","PID_GND"),("PSY_5V","PSY_GND"),("ROOM_DP_5V","ROOM_DP_GND"),("LOCK_DP_5V","LOCK_DP_GND"),("VELOCITY_5V","VELOCITY_GND"),("PART_ROOM_5V","PART_ROOM_GND"),("PART_LOCK_5V","PART_LOCK_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"Process I/O endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                string[] wireOrder={"SUPPLY_POS","PLC_5V","SUPPLY_RET","PLC_GND","IO_5V","IO_GND","PID_5V","PID_GND","PSY_5V","PSY_GND","PLC_SUP_RUN","SUP_RUN_IN","PID_SUP_CMD","SUP_SPEED_IN","SUP_TACH","IO_SUP_RPM","PLC_RET_RUN","RET_RUN_IN","RET_REF","RET_SPEED_IN","RET_TACH","IO_RET_RPM","PLC_EXH_RUN","EXH_RUN_IN","EXH_REF","EXH_SPEED_IN","EXH_TACH","IO_EXH_RPM","PLC_FFU_A","FFU_A_EN","PLC_FFU_B","FFU_B_EN","FFU_STATUS","PLC_FFU_OK","PLC_FRESH","FRESH_CMD","FRESH_POS","IO_FRESH","PID_ROOM_A","ROOM_DMP_CMD","ROOM_DMP_POS","IO_ROOM_DMP","PID_LOCK_B","LOCK_DMP_CMD","LOCK_DMP_POS","IO_LOCK_DMP","PLC_RELIEF","RELIEF_CMD","RELIEF_POS","IO_RELIEF","DAMPER_PERMIT","DAMPER_RUN","DAMPER_DIAG","ROOM_DP_5V","ROOM_DP_GND","ROOM_DP_MON","HMI_ROOM_DP","ROOM_DP_OK","PLC_DP_OK","LOCK_DP_5V","LOCK_DP_GND","LOCK_DP_OUT","PID_LOCK_IN","LOCK_DP_OK","IO_LOCK_VALID","VELOCITY_5V","VELOCITY_GND","AIR_VEL_OUT","AIR_PID_IN","AIR_VEL_OK","PLC_AIR_OK","PART_ROOM_5V","PART_ROOM_GND","ROOM_PART_OUT","PLC_ROOM_PART","ROOM_PART_OK","IO_ROOM_PART","PART_LOCK_5V","PART_LOCK_GND","LOCK_PART_OUT","PLC_LOCK_PART","LOCK_PART_OK","IO_LOCK_PART","FILTER_DP_OUT","HEPA_DP_IN","FILTER_DP_OK","IO_HEPA_DP","ROOM_TEMP","PSY_TEMP","ROOM_RH","PSY_RH","LOCK_TEMP","IO_LOCK_TEMP","LOCK_RH","IO_LOCK_RH","DEW_POINT","PSY_DEW","PSY_COOL","COIL_CMD","PSY_REHEAT","REHEAT_CMD","PSY_HUM","HUM_CMD","HUM_ACTIVE","IO_HUM","DRAIN_OK","IO_DRAIN","INNER_LOCK","INNER_CLOSED","OUTER_LOCK","OUTER_CLOSED","DOOR_FAULT","ESTOP_NC","SAFE_STOP_IN","SAFE_PLC_PERMIT","ROBOT_PERMIT","ROBOT_ENABLE","ROBOT_START","ROBOT_BATCH","SHOWER_EN","SHOWER_READY","PLC_DP_SET","HMI_AIR_REQ","HMI_ROOM_REQ","HMI_LOCK_REQ","PLC_PSY_STATUS","CONTAM_TRIP","BYPASS_TRIP","SCENARIO_IO","HMI_STATUS","HMI_OPERATOR","SAFETY_RESET","ROOM_DP_OUT","CASCADE_DP_IN"};
                foreach(string id in wireOrder[2..])
                    if(id is not ("SAFE_STOP_IN" or "ROBOT_ENABLE") && !pins.Exists(p=>p.name==id)) Pin(carrier,id,0,0,cyan,pins);
                var stopLoad=Part<SparkResistor>(bench,"Safety PLC stop input",0,0,steel);
                var safeInput=Pin(stopLoad,"SAFE_STOP_IN",0,0,gold,pins);
                var returnRail=pins.Find(pin=>pin.name=="PLC_GND");
                Set(stopLoad,("terminalA",safeInput),("terminalB",returnRail),("resistance",10000f));
                var emergency=Child(bench,"Emergency stop NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="ESTOP_NC")),("state",SparkSwitchState.Closed));
                var enableLoad=Part<SparkResistor>(bench,"Wafer robot enable input",0,0,steel);
                var enableNode=Pin(enableLoad,"ROBOT_ENABLE",0,0,gold,pins);
                Set(enableLoad,("terminalA",enableNode),("terminalB",returnRail),("resistance",1500f));
                var drive=Child(bench,"Wafer transfer safety contact",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(drive,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="ROBOT_PERMIT")),("state",SparkSwitchState.Open));
                var contacts=new[]{drive};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel70Process>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel70VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 70 prefab.");
                SparkLevel70DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 70 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_70_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel70Process c,Font font)
        {
            var obj=new GameObject("Level 70 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,270),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the cleanroom network.",font,new Vector2(1080,28),new Vector2(0,270));
            c.meterText=UIText(panel,"Measurements","CLEANROOM CONTROL",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-476.0f,139,c.Power);
            c.calPressureButton=Control(panel,font,"CAL PRESSURE",-340.0f,139,c.CalPressure);
            c.calAirButton=Control(panel,font,"CAL AIRFLOW",-204.0f,139,c.CalAir);
            c.calParticlesButton=Control(panel,font,"CAL PARTICLES",-68.0f,139,c.CalParticles);
            c.calTempButton=Control(panel,font,"CAL TEMP",68.0f,139,c.CalTemp);
            c.calHumidityButton=Control(panel,font,"CAL HUMIDITY",204.0f,139,c.CalHumidity);
            c.supplyButton=Control(panel,font,"START SUPPLY",340.0f,139,c.Supply);
            c.returnFanButton=Control(panel,font,"START RETURN",476.0f,139,c.ReturnFan);
            c.exhaustButton=Control(panel,font,"START EXHAUST",-476.0f,100,c.Exhaust);
            c.ffuAButton=Control(panel,font,"ENABLE FFU A",-340.0f,100,c.FfuA);
            c.ffuBButton=Control(panel,font,"ENABLE FFU B",-204.0f,100,c.FfuB);
            c.setRoomButton=Control(panel,font,"ROOM PRESSURE",-68.0f,100,c.SetRoom);
            c.setLockButton=Control(panel,font,"AIRLOCK PRESSURE",68.0f,100,c.SetLock);
            c.tunePressureButton=Control(panel,font,"PRESSURE GAINS",204.0f,100,c.TunePressure);
            c.tuneAirButton=Control(panel,font,"AIRFLOW GAINS",340.0f,100,c.TuneAir);
            c.setTempButton=Control(panel,font,"TEMP TARGET",476.0f,100,c.SetTemp);
            c.setRhButton=Control(panel,font,"RH TARGET",-476.0f,61,c.SetRh);
            c.environmentButton=Control(panel,font,"AUTO ENV",-340.0f,61,c.Environment);
            c.waferButton=Control(panel,font,"WAFER TEST",-204.0f,61,c.Wafer);
            c.doorButton=Control(panel,font,"AIRLOCK TEST",-68.0f,61,c.Door);
            c.loadingButton=Control(panel,font,"FILTER LOAD",68.0f,61,c.Loading);
            c.diagnoseButton=Control(panel,font,"CONFIRM LOAD",204.0f,61,c.Diagnose);
            c.leakButton=Control(panel,font,"HEPA LEAK",340.0f,61,c.Leak);
            c.serviceButton=Control(panel,font,"REQUEST SERVICE",476.0f,61,c.Service);
            c.isolateButton=Control(panel,font,"ISOLATE A",-476.0f,22,c.Isolate);
            c.replaceButton=Control(panel,font,"REPLACE HEPA A",-340.0f,22,c.Replace);
            c.verifyButton=Control(panel,font,"VERIFY SEAL",-204.0f,22,c.Verify);
            c.resetButton=Control(panel,font,"RESET SAFETY",-68.0f,22,c.Reset);
            c.armButton=Control(panel,font,"W86 TEST",68.0f,22,c.Arm);
            c.recalButton=Control(panel,font,"RECAL ROOM DP",204.0f,22,c.Recal);
            c.certifyButton=Control(panel,font,"CERTIFY",340.0f,22,c.Certify);
            c.eStopButton=Control(panel,font,"E-STOP",476.0f,22,c.EStop);
            var plot=Rect(canvas.transform,"SCADA trends",new Vector2(330,181),new Vector2(518,7));
            c.plot=plot.gameObject.AddComponent<SparkLevel70Trend>();
            c.plot.raycastTarget=false;
            var legend=UIText(plot,"Trend legend","LIVE TRENDS  ·  ROOM Pa   |   AIRLOCK Pa   |   PARTICLES/m³   |   RH",font,new Vector2(355,25),new Vector2(0,181));
            legend.fontSize=11;
            legend.color=new Color(.63f,.89f,.99f);

        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(130,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.70f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(130,37),Vector2.zero).fontSize=13;
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_69_Layout/Level_69_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Refrigeration Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_70" || targets.arraySize!=SparkLevel70Process.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel70Process.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_70"),("displayName","Guard the Air"),
                ("description","Commission clean air, tune pressure and humidity, certify wafers and recover a HEPA breach and sensor failure."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level70_air_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel70Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_70.asset");
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
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L70_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel70Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel70Process.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }


        private static T Part<T>(Transform parent,string name,float x,float z,Material material) where T:SparkComponent
        {
            var obj=Child(parent,name,new Vector3(x,.070f,z));
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
            obj.transform.localPosition=new Vector3(x,.070f,z); obj.transform.localScale=new Vector3(.075f,.018f,.075f);
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
            text.characterSize=.022f; text.fontSize=70; text.color=Color.white;
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
            string path=$"{Folder}/L70 {name}.mat";
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
