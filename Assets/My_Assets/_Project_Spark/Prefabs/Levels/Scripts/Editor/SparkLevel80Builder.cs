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
    public static class SparkLevel80Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_80_Layout";
        private const string PrefabPath = Folder + "/Level_80_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build the Photonic Control Network","Commission the Optical Engine","Lock the Four Wavelengths","Transmit Four Independent Data Streams","Survive Optical Communication Disturbances","Restore Critical Optical Power Feedback"
        };

        [MenuItem("Tools/Project Spark/Level 80/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 80 prefab already exists at " + PrefabPath);
                SparkLevel80DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_80.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_80.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.71f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.71f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.71f,.16f));

            var root = new GameObject("Level_80_Layout");
            try
            {
                var bench = Child(root.transform,"Photonic Workbench",Vector3.zero);
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
                foreach(var (plus,ret) in new[]{("PLC_5V","PLC_GND"),("TX_5V","TX_GND"),("RING_5V","RING_GND"),("RX_5V","RX_GND"),("SAFE_5V","SAFE_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"Process I/O endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                string[] wireOrder={"SUPPLY_POS","PLC_5V","SUPPLY_RET","PLC_GND","TX_5V","TX_GND","RING_5V","RING_GND","RX_5V","RX_GND","SAFE_5V","SAFE_GND","LASER_PERMIT","LASER_ENABLE","LASER_CMD","LASER_DAC","LAUNCH_TAP","LAUNCH_IN","SOURCE_POWER","SHUTTER_CMD","SHUTTER_OPEN","SHUTTER_FB","SHUTTER_CLOSED","ATTEN_CMD","ATTEN_DAC","ATTEN_OUT","ATTEN_IN","COMB_OUT","COMB_LOCK","LASER_TEMP","PLC_TEMP","TEMP_CMD","TEC_SET","REF_OUT","SPECTRAL_REF","STRAY_OUT","SAFE_STRAY","ISOLATOR_CMD","ISOLATOR_SET","ISOLATOR_OUT","ISO_READY","POL_OUT","POL_READY","TAP_HEALTH","TAP_READY","SCADA_LAUNCH","TX_CMD","TX_ENABLE","TX_DRIVE","TX_MOD","BIAS_CMD","MOD_BIAS","MOD_RESP","TX_RESPONSE","LOGIC_OUT","TX_LOGIC","MEMORY_OUT","TX_MEMORY","IO_OUT","TX_IO","SENSOR_OUT","TX_SENSOR","PRBS_CMD","PRBS_START","PRBS_OUT","REF_PATTERN","TX_STATUS","TX_ACTIVE","MUX_CMD","MUX_PERMIT","MUX_OUT","MUX_OK","MOD_CAL","TX_CAL_OK","ROUTE_CMD","ROUTE_IN","ROUTE_OUT","TX_ROUTES","TX_TELEM","SCADA_TX","BIAS_OUT","SCADA_BIAS","RING_CMD","RING_ENABLE","HEAT_DAC","RING_HEAT","TEMP_ARRAY","RING_TEMP","CROSS_HEAT","CROSS_IN","A_HEAT","A_DAC","A_TEMP","A_TEMPERATURE","B_HEAT","B_DAC","B_TEMP","B_TEMPERATURE","C_HEAT","C_DAC","C_TEMP","C_TEMPERATURE","D_HEAT","D_DAC","D_TEMP","D_TEMPERATURE","A_LOCK","A_RESONANCE","B_LOCK","B_RESONANCE","C_LOCK","C_RESONANCE","D_LOCK","D_RESONANCE","CROSS_CMD","CROSS_COMP","DEMOD_OUT","DEMOD_LOCK","TEMP_TELEM","SCADA_TEMP","COOL_CMD","COOLING_SET","DEMUX_CMD","DEMUX_CONFIG","DEMUX_OUT","DEMUX_OK","PD_CMD","PD_ENABLE","PD_OUT","PD_CURRENT","TIA_OUT","RX_ANALOG","CDR_OUT","CDR_LOCK","RX_TAP","RX_METER","SCADA_RX_TAP","SPEC_OUT","SPEC_IN","BUDGET_OUT","BUDGET_IN","DROP_OUT","DROP_OK","RX_OPTICAL_POWER_SIGNAL_OUT","LINK_SUPERVISOR_RX_POWER_IN","METER_HEALTH","TIA_GAIN_REQ","TIA_ACTUAL","PD_READY","SCADA_SPEC","SCADA_LOSS","BER_ENABLE","PRBS_EXPECT","TEST_BITS","BER_RESULT","EYE_RESULT","PAM4_QUALITY","FEC_STATUS","RAW_BITS","FEC_BITS","CDR_REQ","CLOCK_OUT","ROUTE_PASS","MEMORY_RESP","LOGIC_STATUS","IO_STATUS","SENSOR_TELEM","FIBER_LOSS","FAULT_STATE","INJECT_ENABLE","WAVEGUIDE_SCOPE","COUPON_OK","ESD_STATUS","DOCK_SEATED","EDGE_ALIGN","FIBER_ID","CROSSTALK_MAP","WAVEGUIDE_MAP","WDM_HOLOGRAM","ROUTE_HOLOGRAM","DIAGNOSTICS","ESTOP_NC","SAFE_STOP_IN","OVERRIDE_SAFE","OPTICAL_PERMIT","PLC_ALLOW","SCADA_QUAL","OPERATOR_CMD","RX_LOSS_TRIP","LASER_OFF","FAIL_CLOSED","W078_VALID","OBJECTIVE_EVENT","SAFETY_ALARM"};
                foreach(string id in wireOrder[2..])
                    if(id is not ("SAFE_STOP_IN" or "LASER_ENABLE") && !pins.Exists(p=>p.name==id)) Pin(carrier,id,0,0,cyan,pins);
                var stopLoad=Part<SparkResistor>(bench,"Safety PLC stop input",0,0,steel);
                var safeInput=Pin(stopLoad,"SAFE_STOP_IN",0,0,gold,pins);
                var returnRail=pins.Find(pin=>pin.name=="PLC_GND");
                Set(stopLoad,("terminalA",safeInput),("terminalB",returnRail),("resistance",10000f));
                var emergency=Child(bench,"Emergency stop NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="ESTOP_NC")),("state",SparkSwitchState.Closed));
                var enableLoad=Part<SparkResistor>(bench,"Optical source permit input",0,0,steel);
                var enableNode=Pin(enableLoad,"LASER_ENABLE",0,0,gold,pins);
                Set(enableLoad,("terminalA",enableNode),("terminalB",returnRail),("resistance",1500f));
                var drive=Child(bench,"Optical source safety relay",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(drive,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="LASER_PERMIT")),("state",SparkSwitchState.Open));
                var contacts=new[]{drive};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel80Process>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel80VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 80 prefab.");
                SparkLevel80DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 80 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_80_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel80Process c,Font font)
        {
            var obj=new GameObject("Level 80 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,271),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the four-channel photonic workstation.",font,new Vector2(1080,28),new Vector2(0,271));
            c.meterText=UIText(panel,"Measurements","PHOTONIC WDM LINK CONTROL",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-476f,139,c.Power);
            c.safetyButton=Control(panel,font,"TEST SHUTTERS",-357f,139,c.Safety);
            c.packageButton=Control(panel,font,"VERIFY PACKAGE",-238f,139,c.Package);
            c.calSourceButton=Control(panel,font,"CAL SOURCE",-119f,139,c.CalSource);
            c.dockButton=Control(panel,font,"DOCK FIBER",0f,139,c.Dock);
            c.calRxButton=Control(panel,font,"CAL RX POWER",119f,139,c.CalRx);
            c.sourceButton=Control(panel,font,"ENABLE SOURCE",238f,139,c.Source);
            c.tuneAButton=Control(panel,font,"TUNE A",357f,139,c.TuneA);
            c.tuneBButton=Control(panel,font,"TUNE B",476f,139,c.TuneB);
            c.tuneCButton=Control(panel,font,"TUNE C",-476f,100,c.TuneC);
            c.tuneDButton=Control(panel,font,"TUNE D",-357f,100,c.TuneD);
            c.compButton=Control(panel,font,"COMPENSATE",-238f,100,c.Comp);
            c.spectrumButton=Control(panel,font,"VERIFY SPECTRUM",-119f,100,c.Spectrum);
            c.budgetButton=Control(panel,font,"VERIFY POWER",0f,100,c.Budget);
            c.receiverButton=Control(panel,font,"RECOVER CLOCKS",119f,100,c.Receiver);
            c.sendButton=Control(panel,font,"SEND 4 STREAMS",238f,100,c.Send);
            c.fecButton=Control(panel,font,"ENABLE FEC",357f,100,c.Fec);
            c.testButton=Control(panel,font,"TEST EYE / BER",476f,100,c.Test);
            c.driftButton=Control(panel,font,"SOURCE DRIFT",-476f,61,c.Drift);
            c.thermalButton=Control(panel,font,"THERMAL FAULT",-357f,61,c.Thermal);
            c.bendButton=Control(panel,font,"FIBER BEND",-238f,61,c.Bend);
            c.restoreButton=Control(panel,font,"RESTORE",-119f,61,c.Restore);
            c.eStopButton=Control(panel,font,"E-STOP",0f,61,c.EStop);
            c.resetButton=Control(panel,font,"RESET SAFETY",119f,61,c.Reset);
            c.armButton=Control(panel,font,"ARM W078 TEST",238f,61,c.Arm);
            c.verifyButton=Control(panel,font,"VERIFY RX / SAFE",357f,61,c.Verify);
            c.certifyButton=Control(panel,font,"CERTIFY",476f,61,c.Certify);
            var plot=Rect(canvas.transform,"SCADA trends",new Vector2(330,181),new Vector2(-230,800));
            c.plot=plot.gameObject.AddComponent<SparkLevel80Trend>();
            c.plot.raycastTarget=false;
            var legend=UIText(plot,"Trend legend","4-CHANNEL  ·  EYE OPENING",font,new Vector2(355,25),new Vector2(0,181));
            legend.fontSize=16;
            legend.color=new Color(.63f,.89f,.99f);
            for(int channel=0;channel<4;channel++)
            {
                string id=((char)('A'+channel)).ToString();
                var axis=UIText(plot,"Channel "+id,id,font,new Vector2(38,16),
                    new Vector2(channel%2==0?-135:25,channel<2?144:62));
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
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_79_Layout/Level_79_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Bonding Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_80" || targets.arraySize!=SparkLevel80Process.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel80Process.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_80"),("displayName","Command the Light"),
                ("description","Tune four wavelengths, qualify data links, and recover trusted receiver power feedback."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level80_photon_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel80Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_80.asset");
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
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L80_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel80Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel80Process.Gates[i];
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
            string path=$"{Folder}/L80 {name}.mat";
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


