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
    public static class SparkLevel57Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_57_Layout";
        private const string PrefabPath = Folder + "/Level_57_Layout.prefab";
        private static readonly string[] Objectives = {
            "Wire the smart microgrid", "Establish the 50 Hz reference", "Synchronize and parallel inverter B",
            "Balance active power after a load step", "Sustain and restore the island", "Recover W24 and certify the parallel grid"
        };

        [MenuItem("Tools/Project Spark/Level 57/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 57 prefab already exists at " + PrefabPath);
                SparkLevel57DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_57.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_57.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.57f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.62f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Ladder signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Digital input",new Color(.95f,.57f,.16f));

            var root = new GameObject("Level_57_Layout");
            try
            {
                var bench = Child(root.transform,"Microgrid Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Temporary board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.57f),board);
                var pins = new List<SparkTerminal>();
                SparkPowerSupply Supply(string name, float voltage, float current)
                {
                    var part=Part<SparkPowerSupply>(bench,name,0,0,steel);
                    Set(part,("outputVoltage",voltage),("currentLimit",current),("state",SparkPowerSupplyState.Active));
                    return part;
                }
                var source=Supply("CONTROL_POWER",5f,.65f);
                var positive=Pin(source,"SUPPLY_POS",0,0,red,pins,SparkTerminalPolarity.Positive);
                var ground=Pin(source,"SUPPLY_RET",0,0,blue,pins,SparkTerminalPolarity.Negative);
                Set(source,("positiveTerminal",positive),("negativeTerminal",ground));
                var power=Part<SparkResistor>(bench,"Critical-speed controller power",0,0,steel);
                var cp=Pin(power,"CTRL_POS",0,0,red,pins);
                var cg=Pin(power,"CTRL_GND",0,0,blue,pins);
                Set(power,("terminalA",cp),("terminalB",cg),("resistance",2200f));

                SparkPowerSupply Signal(string id,Material color)
                {
                    var output=Supply(id,0f,.025f);
                    Set(output,("positiveTerminal",Pin(output,id,0,0,color,pins)),("negativeTerminal",ground));
                    return output;
                }

                SparkTerminal Sense(string id,SparkTerminal ret,Material color)
                {
                    var resistor=Part<SparkResistor>(bench,"Input "+id,0,0,steel);
                    var input=Pin(resistor,id,0,0,color,pins);
                    Set(resistor,("terminalA",input),("terminalB",ret),("resistance",100000f));
                    return input;
                }



                SparkTerminal Load(string name,string plusId,string returnId)
                {
                    var load=Part<SparkResistor>(bench,name,0,0,steel);
                    var plus=Pin(load,plusId,0,0,red,pins);
                    Set(load,("terminalA",plus),("terminalB",Pin(load,returnId,0,0,blue,pins)),("resistance",2200f));
                    return plus;
                }


                Load("Primary inverter rails","INV_A_DC","INV_A_RET");
                Load("Secondary inverter rails","INV_B_DC","INV_B_RET");
                Load("Sensors and protection rails","SENSE_DC","SENSE_RET");
                SparkSwitch Contact(string name,SparkTerminal input,SparkTerminal output,bool on)
                {
                    var contact=Child(bench,name,Vector3.zero).gameObject.AddComponent<SparkSwitch>();
                    Set(contact,("inputTerminal",input),("outputTerminal",output),("state",on?SparkSwitchState.Closed:SparkSwitchState.Open));
                    return contact;
                }
                SparkResistor Branch(string name,string pinId,Material color,float resistance,out SparkTerminal socket,out SparkTerminal inner)
                {
                    var part=Part<SparkResistor>(bench,name,0,0,steel);
                    socket=Pin(part,pinId,0,0,color,pins);
                    inner=Child(part.transform,"Internal",Vector3.zero).gameObject.AddComponent<SparkTerminal>();
                    Set(inner,("owner",part));
                    Set(part,("terminalA",inner),("terminalB",socket),("resistance",resistance),("maximumPowerWatts",3f));
                    return part;
                }
                // AC terminals are graph endpoints. Their AC voltages and currents
                // are owned by the three-phase model, never stamped as DC sources.
                var module=Part<SparkResistor>(bench,"AC terminal carrier",0,0,steel);
                Set(module,("electricalEnabled",false));
                foreach (string id in new[]{"GRID_A","BUS_A","GRID_B","BUS_B","GRID_C","BUS_C","GRID_N","BUS_N","INV_B_A","CONTACT_A","INV_B_B","CONTACT_B","INV_B_C","CONTACT_C","INV_B_N","LOAD_A","LOAD_B","LOAD_C","LOAD_N","MOTOR_A","MOTOR_B","MOTOR_C"})
                    Pin(module,id,0,0,cyan,pins);
                SparkTerminal FindPin(string id) => pins.Find(pin=>pin.name==id);
                SparkTerminal SignalPin(string id) => Pin(module,id,0,0,cyan,pins);
                var poleA=Contact("Tie pole A",FindPin("CONTACT_A"),FindPin("BUS_A"),false);
                var poleB=Contact("Tie pole B",FindPin("CONTACT_B"),FindPin("BUS_B"),false);
                var poleC=Contact("Tie pole C",FindPin("CONTACT_C"),FindPin("BUS_C"),false);
                var upstream=Child(bench,"Internal upstream source",Vector3.zero).gameObject.AddComponent<SparkTerminal>();
                var isolation=Contact("Upstream isolation",upstream,FindPin("GRID_A"),false);
                Set(upstream,("owner",isolation));
                Branch("Emergency pilot","ESTOP_NC",cyan,10f,out _,out var pilot);
                var emergency=Contact("Normally closed emergency stop",positive,pilot,true);
                Sense("CTRL_ESTOP",cg,cyan);
                foreach(string id in new[]{"GRID_VSENSE","CTRL_GRID_V","INV_VSENSE","CTRL_INV_V","PHASE_OUT","CTRL_PHASE","CTRL_CLOSE","CONTACT_CMD","CONTACT_AUX","CTRL_CONTACT","CT_OUT","CTRL_CURRENT","ANALYZER_OUT","CTRL_QUALITY","PROTECTION_OK","CTRL_PROTECT","CTRL_ISLAND","TRANSFER_CMD","TRANSFER_AUX","CTRL_TRANSFER"})
                    SignalPin(id);
                string[] wireOrder={"SUPPLY_POS","CTRL_POS","SUPPLY_RET","CTRL_GND","INV_A_DC","INV_A_RET","INV_B_DC","INV_B_RET","SENSE_DC","SENSE_RET","GRID_A","BUS_A","GRID_B","BUS_B","GRID_C","BUS_C","GRID_N","BUS_N","INV_B_A","CONTACT_A","INV_B_B","CONTACT_B","INV_B_C","CONTACT_C","INV_B_N","LOAD_A","LOAD_B","LOAD_C","LOAD_N","MOTOR_A","MOTOR_B","MOTOR_C","GRID_VSENSE","CTRL_GRID_V","INV_VSENSE","CTRL_INV_V","PHASE_OUT","CTRL_PHASE","CTRL_CLOSE","CONTACT_CMD","CONTACT_AUX","CTRL_CONTACT","CT_OUT","CTRL_CURRENT","ANALYZER_OUT","CTRL_QUALITY","PROTECTION_OK","CTRL_PROTECT","CTRL_ISLAND","TRANSFER_CMD","TRANSFER_AUX","CTRL_TRANSFER","ESTOP_NC","CTRL_ESTOP"};
                var ordered=Array.ConvertAll(wireOrder,id=>pins.Find(pin=>pin.name==id));

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,source,isolation,poleA,poleB,poleC,emergency);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel57Grid>();
                controller.layout=layout;
                controller.source=source;


                controller.isolation=isolation;
                controller.poleA=poleA; controller.poleB=poleB; controller.poleC=poleC;
                controller.emergency=emergency;
                controller.pins=ordered;
                controller.rig=root.AddComponent<SparkLevel57VisualRig>();
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 57 prefab.");
                SparkLevel57DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 57 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_57_Layout");
        }

        internal static void BuildControls(Transform parent,SparkLevel57Grid c,Font font)
        {
            var obj=new GameObject("Level 57 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1080,257),new Vector2(-220,6));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;



            c.statusText=UIText(panel,"Status","Wire the three-phase microgrid.",font,new Vector2(1080,28),new Vector2(0,260));
            c.meterText=UIText(panel,"Measurements","SMART MICROGRID",font,new Vector2(1080,28),new Vector2(0,233));
            c.meterText.color=new Color(.40f,.92f,.93f);
            c.detailText=UIText(panel,"Details","",font,new Vector2(1080,28),new Vector2(0,207));
            c.calibrationText=UIText(panel,"Phase and power quality","",font,new Vector2(1080,25),new Vector2(0,181));
            c.calibrationText.fontSize=16;
            var plotPanel=Rect(canvas.transform,"Displacement and sweep",new Vector2(350,160),new Vector2(505,6));
            var plotBackground=plotPanel.gameObject.AddComponent<Image>();
            plotBackground.color=new Color(.035f,.08f,.13f,.94f);
            plotBackground.raycastTarget=false;
            UIText(plotPanel,"Title","SYNCHRONOSCOPE / PHASE VIEW",font,new Vector2(340,24),new Vector2(0,131)).fontSize=16;
            c.plot=Rect(plotPanel,"Plot traces",new Vector2(330,102),new Vector2(0,28)).gameObject.AddComponent<SparkLevel57Plot>();
            c.plot.raycastTarget=false;
            UIText(plotPanel,"Legend","PHASES A, B AND C",font,new Vector2(345,24),Vector2.zero).fontSize=14;
            c.powerButton=Control(panel,font,"MAIN POWER",-453f,139,c.Power);
            c.guardButton=Control(panel,font,"E-STOP",-302f,139,c.Emergency);
            c.calibrateButton=Control(panel,font,"CALIBRATE",-151f,139,c.Calibrate);
            c.startButton=Control(panel,font,"START GRID A",0f,139,c.StartGrid);
            c.enableButton=Control(panel,font,"ENABLE INVERTER B",151f,139,c.EnableB);
            c.checkButton=Control(panel,font,"SYNC CHECK",302f,139,c.CheckSync);
            c.pllButton=Control(panel,font,"PLL LOCK",453f,139,c.LockPll);
            c.hzButton=Control(panel,font,"MATCH FREQUENCY",-453f,97,c.MatchHz);
            c.voltageButton=Control(panel,font,"MATCH VOLTAGE",-302f,97,c.MatchVoltage);
            c.syncButton=Control(panel,font,"AUTO SYNCHRONIZE",-151f,97,c.AutoSync);
            c.closeButton=Control(panel,font,"CLOSE CONTACTOR",0f,97,c.Close);
            c.openButton=Control(panel,font,"OPEN CONTACTOR",151f,97,c.Open);
            c.autoButton=Control(panel,font,"AUTO SHARE",302f,97,c.AutoShare);
            c.manualButton=Control(panel,font,"MANUAL SHARE",453f,97,c.ManualShare);
            c.moreButton=Control(panel,font,"INCREASE LOAD",-453f,55,c.More);
            c.lessButton=Control(panel,font,"DECREASE LOAD",-302f,55,c.Less);
            c.stepButton=Control(panel,font,"LOAD STEP",-151f,55,c.LoadStep);
            c.islandButton=Control(panel,font,"ISLAND MODE",0f,55,c.Island);
            c.restoreButton=Control(panel,font,"RESTORE GRID",151f,55,c.Restore);
            c.phaseButton=Control(panel,font,"PHASE VIEW",302f,55,c.PhaseView);
            c.faultButton=Control(panel,font,"FAULT TEST",453f,55,c.FaultTest);
            c.resetButton=Control(panel,font,"RESET FAULT",-453f,13,c.ResetFault);
            c.finalButton=Control(panel,font,"FINAL CERTIFY",-302f,13,c.Final);
        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(146,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.57f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(146,37),Vector2.zero).fontSize=14;
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_56_Layout/Level_56_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Energy Workbench") : null;
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_57" || targets.arraySize!=SparkLevel57Grid.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel57Grid.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_57"),("displayName","Synchronize the Grid"),
                ("description","Match three-phase inverter references, balance load, sustain an island and recover from lost grid-voltage feedback."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level57_grid_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel57Grid.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_57.asset");
            return level;
        }

        private static void WriteBindings(SparkLevelSceneBindings bindings,List<SparkTerminal> pins,SparkPowerSupply source, SparkSwitch isolation, SparkSwitch poleA, SparkSwitch poleB, SparkSwitch poleC, SparkSwitch emergency)
        {
            var so=new SerializedObject(bindings);
            var terminals=so.FindProperty("terminals"); terminals.arraySize=pins.Count;
            for(int i=0;i<pins.Count;i++)
            {
                var row=terminals.GetArrayElementAtIndex(i);
                row.FindPropertyRelative("id").stringValue=pins[i].name;
                row.FindPropertyRelative("terminal").objectReferenceValue=pins[i];
            }
            var fixedComponents = new (string id,SparkElectricalComponent component)[] {
                ("CONTROL_POWER",source),("GRID_ISOLATION",isolation),("TIE_A",poleA),
                ("TIE_B",poleB),("TIE_C",poleC),("EMERGENCY_STOP",emergency)
            };
            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Length;
            for(int i=0;i<fixedComponents.Length;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel57Grid.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel57Grid.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }


        private static T Part<T>(Transform parent,string name,float x,float z,Material material) where T:SparkComponent
        {
            var obj=Child(parent,name,new Vector3(x,.057f,z));
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
            obj.transform.localPosition=new Vector3(x,.065f,z); obj.transform.localScale=new Vector3(.075f,.018f,.075f);
            obj.GetComponent<Renderer>().sharedMaterial=material;
            int layer=LayerMask.NameToLayer("Pin"); if(layer>=0) obj.layer=layer;
            var terminal=obj.AddComponent<SparkTerminal>();
            var kind=polarity==SparkTerminalPolarity.Positive ? SparkTerminalKind.Power :
                polarity==SparkTerminalPolarity.Negative ? SparkTerminalKind.Ground : SparkTerminalKind.Generic;
            Set(terminal,("owner",owner),("kind",kind),("polarity",polarity),("maxConnections",polarity==SparkTerminalPolarity.None ? (id is "BUS_A" or "BUS_B" or "BUS_C" or "BUS_N" ? 3 : 1) : 5));
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
            text.characterSize=.022f; text.fontSize=57; text.color=Color.white;
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
            string path=$"{Folder}/L57 {name}.mat";
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

