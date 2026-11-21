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
    public static class SparkLevel78Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_78_Layout";
        private const string PrefabPath = Folder + "/Level_78_Layout.prefab";
        private static readonly string[] Objectives = {
            "Build dielectric protection network","Commission vacuum and cap chamber","Seal the copper","Build and qualify low-k insulation","Survive dielectric disturbances","Restore critical dielectric leakage feedback"
        };

        [MenuItem("Tools/Project Spark/Level 78/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 78 prefab already exists at " + PrefabPath);
                SparkLevel78DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_78.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_78.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.71f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.71f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.71f,.16f));

            var root = new GameObject("Level_78_Layout");
            try
            {
                var bench = Child(root.transform,"Dielectric Workbench",Vector3.zero);
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
                foreach(var (plus,ret) in new[]{("PLC_5V","PLC_GND"),("SCADA_5V","SCADA_GND"),("GAS_5V","GAS_GND"),("METER_5V","METER_GND"),("SAFE_5V","SAFE_GND")})
                {
                    var rail=Part<SparkResistor>(bench,plus,0,0,steel);
                    Set(rail,("terminalA",Pin(rail,plus,0,0,red,pins)),("terminalB",Pin(rail,ret,0,0,blue,pins)),("resistance",2200f));
                }
                var carrier=Part<SparkResistor>(bench,"Process I/O endpoints",0,0,steel);
                Set(carrier,("electricalEnabled",false));
                string[] wireOrder={"SUPPLY_POS","PLC_5V","SUPPLY_RET","PLC_GND","SCADA_5V","SCADA_GND","GAS_5V","GAS_GND","METER_5V","METER_GND","SAFE_5V","SAFE_GND","BACK_CMD","BACK_RUN","TURBO_CMD","TURBO_RUN","BACK_READY","VAC_BACK_OK","VAC_DIAG","PRESSURE_OUT","VAC_PRESSURE","GATE_CMD","GATE_DRIVE","GATE_CLOSED","VAC_GATE_OK","LOADLOCK_CMD","LOCK_PUMP","LOCK_PRESSURE","VAC_LOCK_OK","ROBOT_CMD","ROBOT_DRIVE","ROBOT_HOME","TRANSFER_HOME","ALIGN_CMD","ALIGN_START","ALIGN_OUT","TRANSFER_ALIGN","PREP_CMD","PREP_START","PREP_OUT","TRANSFER_SURFACE","PREP_DONE","PLC_PREP_OK","VAC_PERMIT","VAC_AUTH","HUB_SAFE","TRANSFER_SAFE","CAP_CMD","CAP_RUN","CAP_LID_CMD","CAP_LID","CAP_LID_OUT","CAP_INTERLOCK","CAP_FLOW_CMD","CAP_MFC_SET","CAP_FLOW_OUT","CAP_FLOW_IN","CAP_RESERVOIR","OUT_CAP_FUEL","CAP_MANIFOLD","CAP_DELIVERY","CAP_RF_CMD","CAP_RF_RUN","CAP_RF_OUT","CAP_MATCH_IN","CAP_MATCH_OUT","CAP_RF_OK","CAP_TEMP_CMD","CAP_HEAT","CAP_TEMP_OUT","CAP_HEAT_OK","CAP_THICK_OUT","CAP_THICK_IN","CAP_MAP_OUT","CAP_UNIFORM","CAP_PINHOLE_OUT","CAP_DEFECTS","CAP_EXHAUST_CMD","CAP_THROTTLE","LOWK_CMD","LOWK_RUN","SHOWER_CMD","LOWK_SHOWER","LOWK_RF_CMD","LOWK_RF_RUN","LOWK_RF_OUT","LOWK_MATCH_IN","LOWK_MATCH_OUT","LOWK_RF_OK","LOWK_TEMP_CMD","LOWK_HEAT","LOWK_TEMP_OUT","LOWK_HEAT_OK","MATRIX_READY","LOWK_MATRIX","POROGEN_READY","LOWK_POROGEN","DUAL_MFC_CMD","GAS_MIXER","MIX_FLOW_CMD","LOWK_MIX_REQ","LOWK_FLOW_OUT","LOWK_FLOW_IN","ION_ENERGY_OUT","DAMAGE_IN","CHARGE_OUT","CHARGE_IN","FTIR_OUT","BOND_IN","LOWK_THICK_OUT","GROWTH_IN","CURE_CMD","CURE_RUN","CURE_DOOR_OUT","CURE_SAFE","LAMP_CMD","UV_ENABLE","LAMP_OUT","UV_ACTUAL","SHUTTER_CMD","SHUTTER_SET","SHUTTER_OUT","SHUTTER_POS","PLATEN_CMD","CURE_SPIN","PLATEN_OUT","CURE_RPM","CURE_TEMP_CMD","CURE_HEAT","CURE_TEMP_OUT","CURE_TEMP","OUTGAS_IN","PORE_STATUS","CURE_DONE","UV_SAFE","ELLIPSOMETRY","FILM_BOW","MOISTURE_IN","BOND_QUALITY","RC_IN","COMB_READY","BIAS_STATUS","STRESS_CMD","DIELECTRIC_LEAKAGE_ELECTROMETER_OUT","DIELECTRIC_RELIABILITY_CONTROLLER_LEAKAGE_IN","METER_HEALTH","TEST_TELEM","TDDB_GRAPH","FILM_TELEM","STRESS_PERMIT","STRESS_ENABLE","INCOMING_CHECK","PINHOLE_WARN","COVERAGE_OK","DAMAGE_STATUS","CHARGE_WARN","LOWK_CERT","TEMP_HISTORY","OUTGAS_OK","STRESS_HEALTH","MOISTURE_OK","RC_RESULT","METER_CAL_OK","COMB_ISOLATION","RELIABILITY_OK","ESTOP_NC","SAFE_STOP_IN","SAFETY_HEARTBEAT","CAP_RF_OFF","LOWK_RF_OFF","UV_OFF","STRESS_OFF","FAULT_LATCH","PLC_STATE","OPERATOR_CMD","RELIABILITY_REQ","LEAK_VALID","LEAK_LOSS_TRIP","TRANSFER_HOLD","OBJECTIVE_EVENT","SCADA_CERT","SCADA_ALARM"};
                foreach(string id in wireOrder[2..])
                    if(id is not ("SAFE_STOP_IN" or "STRESS_ENABLE") && !pins.Exists(p=>p.name==id)) Pin(carrier,id,0,0,cyan,pins);
                var stopLoad=Part<SparkResistor>(bench,"Safety PLC stop input",0,0,steel);
                var safeInput=Pin(stopLoad,"SAFE_STOP_IN",0,0,gold,pins);
                var returnRail=pins.Find(pin=>pin.name=="PLC_GND");
                Set(stopLoad,("terminalA",safeInput),("terminalB",returnRail),("resistance",10000f));
                var emergency=Child(bench,"Emergency stop NC",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(emergency,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="ESTOP_NC")),("state",SparkSwitchState.Closed));
                var enableLoad=Part<SparkResistor>(bench,"Stress source permit input",0,0,steel);
                var enableNode=Pin(enableLoad,"STRESS_ENABLE",0,0,gold,pins);
                Set(enableLoad,("terminalA",enableNode),("terminalB",returnRail),("resistance",1500f));
                var drive=Child(bench,"Coupon stress safety relay",Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                Set(drive,("inputTerminal",positive),("outputTerminal",pins.Find(pin=>pin.name=="STRESS_PERMIT")),("state",SparkSwitchState.Open));
                var contacts=new[]{drive};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,contacts,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel78Process>();
                controller.layout=layout;
                controller.source=source;


                controller.contacts=contacts;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel78VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 78 prefab.");
                SparkLevel78DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 78 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_78_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel78Process c,Font font)
        {
            var obj=new GameObject("Level 78 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,271),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the dielectric workstation.",font,new Vector2(1080,28),new Vector2(0,271));
            c.meterText=UIText(panel,"Measurements","DIELECTRIC PROCESS AND COUPON TEST",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Event log","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            c.powerButton=Control(panel,font,"5 V CONTROL",-476f,139,c.Power);
            c.pumpButton=Control(panel,font,"START VACUUM",-357f,139,c.Pump);
            c.loadButton=Control(panel,font,"LOAD WAFER",-238f,139,c.Load);
            c.inspectButton=Control(panel,font,"INSPECT INCOMING",-119f,139,c.Inspect);
            c.prepButton=Control(panel,font,"PREP COPPER",0f,139,c.Prep);
            c.calGasButton=Control(panel,font,"CAL GAS",119f,139,c.CalGas);
            c.calRfButton=Control(panel,font,"CAL RF",238f,139,c.CalRf);
            c.calMeterButton=Control(panel,font,"CAL ELECTROMETER",357f,139,c.CalMeter);
            c.capButton=Control(panel,font,"DEPOSIT CAP",476f,139,c.Cap);
            c.capInspectButton=Control(panel,font,"CHECK CAP",-476f,100,c.CapInspect);
            c.lowKButton=Control(panel,font,"DEPOSIT LOW-K",-357f,100,c.LowK);
            c.cureButton=Control(panel,font,"UV CURE",-238f,100,c.Cure);
            c.metroButton=Control(panel,font,"FILM METROLOGY",-119f,100,c.Metro);
            c.stressButton=Control(panel,font,"SCREEN COUPON",0f,100,c.Stress);
            c.stopButton=Control(panel,font,"STOP",119f,100,c.Stop);
            c.discardButton=Control(panel,font,"DISCARD WAFER",238f,100,c.Discard);
            c.gasFaultButton=Control(panel,font,"CAP FLOW FAULT",357f,100,c.GasFault);
            c.chargeFaultButton=Control(panel,font,"RF CHARGE FAULT",476f,100,c.ChargeFault);
            c.uvFaultButton=Control(panel,font,"UV SHUTTER FAULT",-476f,61,c.UvFault);
            c.restoreButton=Control(panel,font,"RESTORE",-357f,61,c.Restore);
            c.eStopButton=Control(panel,font,"E-STOP",-238f,61,c.EStop);
            c.resetButton=Control(panel,font,"RESET SAFETY",-119f,61,c.Reset);
            c.armButton=Control(panel,font,"ARM W083 TEST",0f,61,c.Arm);
            c.verifyButton=Control(panel,font,"VERIFY ELECTROMETER",119f,61,c.Verify);
            c.certifyButton=Control(panel,font,"CERTIFY",238f,61,c.Certify);
            c.focusWireButton=Control(panel,font,"FOCUS WIRE",-476f,22,c.FocusNextWire);
            c.focusWireButton.interactable=true;
            c.cableViewButton=Control(panel,font,"CABLE VIEW",-357f,22,c.ToggleCableView);
            Control(panel,font,"RESET VIEW",-238f,22,c.ResetWorkbenchView).interactable=true;
            c.openFeedbackButton=Control(panel,font,"OPEN W083",0f,22,c.OpenFeedbackWire);
            var plot=Rect(canvas.transform,"SCADA trends",new Vector2(330,181),Vector2.zero);
            plot.anchorMin=plot.anchorMax=plot.pivot=Vector2.one;
            plot.anchoredPosition=new Vector2(-24,-45);
            c.plot=plot.gameObject.AddComponent<SparkLevel78Trend>();
            c.plot.raycastTarget=false;
            var legend=UIText(plot,"Trend legend","16-REGION MAP  ·  CAP / LOW-K / LEAKAGE",font,new Vector2(355,25),new Vector2(0,181));
            legend.fontSize=14;
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
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_77_Layout/Level_77_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Finishing Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_78" || targets.arraySize!=SparkLevel78Process.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel78Process.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_78"),("displayName","Seal the Signal"),
                ("description","Protect patterned copper with a cap and low-k film, qualify a monitor coupon, and restore W083."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level78_seal_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel78Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_78.asset");
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
            for(int i=0;i<contacts.Length;i++) fixedComponents.Add(($"L78_CONTACT_{i}",contacts[i]));
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel78Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel78Process.Gates[i];
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
