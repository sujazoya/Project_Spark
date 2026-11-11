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
    public static class SparkLevel33Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_33_Layout";
        private const string PrefabPath = Folder + "/Level_33_Jam_Guard_Layout.prefab";
        private static readonly string[] Objectives = {
            "Start with timer discharged", "Ignore a brief blockage", "Trip during a sustained jam",
            "Reject reset during a jam", "Stay stopped after the jam clears", "Restart only on manual reset"
        };

        [MenuItem("Tools/Project Spark/Level 33/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 33 prefab already exists at " + PrefabPath);
                SparkLevel33DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader==null || font==null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level=AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder+"/Level_33.asset");
            if (level!=null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_33.asset has different objectives.");
            if (level==null) level=CreateDefinition();

            var board=MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel=MaterialAsset(shader,"Modules",new Color(.25f,.35f,.43f));
            var slat=MaterialAsset(shader,"Belt",new Color(.20f,.27f,.31f));
            var blue=MaterialAsset(shader,"Sockets",new Color(.15f,.62f,.92f));
            var red=MaterialAsset(shader,"Positive and JAM",new Color(.90f,.24f,.28f));
            var aqua=MaterialAsset(shader,"TIMER",new Color(.14f,.82f,.75f));
            var amber=MaterialAsset(shader,"Parcel and guards",new Color(.95f,.56f,.16f));
            var green=MaterialAsset(shader,"RUN",new Color(.14f,.70f,.38f));

            var root=new GameObject("Level_33_Jam_Guard_Layout");
            try
            {
                var bench=Child(root.transform,"Jam Guard Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.55f),board);
                var pins=new List<SparkTerminal>();
                var supply=Part<SparkPowerSupply>(bench,"PowerSupply_01",-.86f,.52f,steel);
                Set(supply,("positiveTerminal",Pin(supply,"Battery_positive",-.07f,0,red,pins,SparkTerminalPolarity.Positive)),
                    ("negativeTerminal",Pin(supply,"Battery_negative",.07f,0,blue,pins,SparkTerminalPolarity.Negative)),
                    ("outputVoltage",5f),("currentLimit",.35f),("state",SparkPowerSupplyState.Active));
                Label(supply.transform,"5 V SUPPLY",font);
                var charge=Contact(bench,"JAM CHARGE","JAM",-.52f,.52f,false,steel,blue,font,pins);
                var rCharge=Resistor(bench,"4.7 kOhm","R_CHARGE",-.18f,.52f,4700f,steel,blue,aqua,font,pins);
                var cap=Part<SparkCapacitor>(bench,"TIMER CAPACITOR",.16f,.52f,steel);
                Set(cap,("positiveTerminal",Pin(cap,"Timer_Positive",-.07f,0,aqua,pins,SparkTerminalPolarity.Positive)),
                    ("negativeTerminal",Pin(cap,"Timer_Negative",.07f,0,blue,pins,SparkTerminalPolarity.Negative)),
                    ("capacitanceFarads",.00047f),("voltageRating",16f));
                Label(cap.transform,"TIMER 470 uF",font);
                Solid(cap.transform,"Capacitor can",PrimitiveType.Cylinder,new Vector3(0,.09f,.012f),new Vector3(.061f,.052f,.061f),aqua);
                var rClear=Resistor(bench,"1 kOhm","R_CLEAR",.51f,.52f,1000f,steel,aqua,blue,font,pins);
                var clear=Contact(bench,"CLEAR DISCHARGE","CLEAR",.85f,.52f,true,steel,blue,font,pins);

                var drive=Contact(bench,"MOTOR CONTACT","RUN",-.68f,.19f,false,steel,blue,font,pins);
                var motor=Part<SparkMotor>(bench,"Conveyor Motor",-.33f,.19f,steel);
                Set(motor,("positiveTerminal",Pin(motor,"Motor_A",-.07f,0,blue,pins)),
                    ("negativeTerminal",Pin(motor,"Motor_B",.07f,0,blue,pins)),
                    ("windingResistance",33f),("startupVoltage",3f),("maximumCurrent",.25f));
                Label(motor.transform,"MOTOR A / B",font);
                var housing=Solid(motor.transform,"Motor housing",PrimitiveType.Cylinder,new Vector3(0,.08f,.068f),new Vector3(.06f,.068f,.06f),steel);
                housing.transform.localRotation=Quaternion.Euler(0,0,90);
                var shaft=Child(motor.transform,"Motor shaft",new Vector3(.084f,.08f,.068f));
                Solid(shaft,"Rotation marker",PrimitiveType.Cube,Vector3.zero,new Vector3(.045f,.012f,.041f),amber);

                var belt=Child(bench,"Jam test conveyor",new Vector3(.19f,.06f,-.35f));
                Solid(belt,"Base",PrimitiveType.Cube,Vector3.zero,new Vector3(1.08f,.09f,.31f),board);
                foreach(float z in new[]{-.159f,.159f})
                    Solid(belt,"Rail",PrimitiveType.Cube,new Vector3(0,.076f,z),new Vector3(1.06f,.043f,.02f),steel);
                var slats=new Transform[14];
                for(int i=0;i<slats.Length;i++)
                    slats[i]=Solid(belt,"Belt slat",PrimitiveType.Cube,new Vector3(i*.96f/slats.Length-.48f,.064f,0),
                        new Vector3(.061f,.018f,.278f),slat).transform;
                foreach(float z in new[]{-.183f,.183f})
                    Solid(belt,"Sensor post",PrimitiveType.Cube,new Vector3(-.03f,.152f,z),new Vector3(.05f,.25f,.045f),steel);
                Solid(belt,"Jam sensor",PrimitiveType.Cube,new Vector3(-.03f,.27f,0),new Vector3(.095f,.045f,.40f),aqua);
                var parcel=Solid(belt,"Obstruction",PrimitiveType.Cube,new Vector3(-.03f,.12f,0),new Vector3(.16f,.087f,.13f),amber);
                parcel.SetActive(false);
                var tower=Child(belt,"RUN and JAM indicators",new Vector3(.46f,.12f,-.28f));
                var runLight=Beacon(tower,"RUN",new Vector3(-.07f,.05f,0),green,new Color(.2f,1f,.43f));
                var jamLight=Beacon(tower,"JAM",new Vector3(.07f,.05f,0),red,new Color(1f,.14f,.15f));
                Label(tower,"RUN       JAM",font,-.071f);

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,supply);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel33JamGuard>();
                controller.layout=layout; controller.source=supply; controller.chargeContact=charge;
                controller.clearContact=clear; controller.motorContact=drive; controller.chargeResistor=rCharge;
                controller.dischargeResistor=rClear; controller.timerCapacitor=cap; controller.motor=motor;
                controller.beltSlats=slats; controller.shaft=shaft; controller.parcel=parcel.transform;
                controller.runLight=runLight; controller.jamLight=jamLight;
                BuildControls(root.transform,controller,font);
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 33 prefab.");
                SparkLevel33DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 33 assets created. Register the definition, prefab and drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_33_Layout");
        }

        private static SparkSwitch Contact(Transform parent,string name,string id,float x,float z,bool closed,Material body,Material socket,Font font,List<SparkTerminal> pins)
        {
            var contact=Part<SparkSwitch>(parent,name,x,z,body);
            Set(contact,("inputTerminal",Pin(contact,id+"_IN",-.07f,0,socket,pins)),
                ("outputTerminal",Pin(contact,id+"_OUT",.07f,0,socket,pins)),("state",closed ? SparkSwitchState.Closed : SparkSwitchState.Open));
            Label(contact.transform,name,font);
            return contact;
        }

        private static SparkResistor Resistor(Transform parent,string name,string id,float x,float z,float ohms,Material body,Material a,Material b,Font font,List<SparkTerminal> pins)
        {
            var resistor=Part<SparkResistor>(parent,name,x,z,body);
            Set(resistor,("terminalA",Pin(resistor,id+"_IN",-.07f,0,a,pins)),
                ("terminalB",Pin(resistor,id+"_OUT",.07f,0,b,pins)),("resistance",ohms));
            Label(resistor.transform,name,font);
            return resistor;
        }

        private static Light Beacon(Transform parent,string name,Vector3 p,Material material,Color color)
        {
            var bulb=Solid(parent,name,PrimitiveType.Sphere,p,Vector3.one*.06f,material);
            var light=bulb.AddComponent<Light>(); light.type=LightType.Point; light.range=.30f;
            light.intensity=2f; light.color=color; light.enabled=false;
            return light;
        }

        private static void BuildControls(Transform parent,SparkLevel33JamGuard controller,Font font)
        {
            var obj=new GameObject("Level 33 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(960,140),new Vector2(0,92));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;
            controller.statusText=UIText(panel,"Status","Wire the Jam Guard workbench.",font,new Vector2(925,30),new Vector2(0,103));
            controller.readingsText=UIText(panel,"Readings","TIMER —   |   MOTOR —",font,new Vector2(920,29),new Vector2(0,68));
            controller.startButton=Control(panel,font,"START TEST",-372,controller.StartTest,out _);
            controller.briefButton=Control(panel,font,"BRIEF BLOCK",-186,controller.BriefBlock,out _);
            controller.jamButton=Control(panel,font,"JAM",0,controller.ToggleJam,out controller.jamLabel);
            controller.resetButton=Control(panel,font,"RESET",186,controller.ResetTrip,out _);
            controller.retryButton=Control(panel,font,"RESTART TEST",372,controller.RestartTrial,out _);
        }

        private static Button Control(Transform parent,Font font,string caption,float x,UnityAction action,out Text label)
        {
            var rect=Rect(parent,caption,new Vector2(172,38),new Vector2(x,15));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.48f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            label=UIText(rect,"Label",caption,font,new Vector2(172,38),Vector2.zero);
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_32_Layout/Level_32_Optical_Sorter_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Optical Workbench") : null;
            if(anchor==null)
            {
                previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_31_Layout/Level_31_SealIn_Layout.prefab");
                anchor=previous!=null ? previous.transform.Find("Relay Workbench") : null;
            }
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }
        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_33" || targets.arraySize!=SparkLevel33JamGuard.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel33JamGuard.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_33"),("displayName","Jam Guard - Delayed Trip and Manual Reset"),
                ("description","Build an RC jam timer, verify a delayed motor trip, and restore drive with a manual reset."),
                ("completionMode",0),("requiredTargetCount",Objectives.Length),("requireClosedReturn",false),
                ("rejectShortCircuit",true),("rejectTargetShort",true),("requireMinimumVoltage",false),
                ("autoAdvanceOnCompletion",false));
            var so=new SerializedObject(level);
            var sources=so.FindProperty("powerSources"); sources.arraySize=1;
            var source=sources.GetArrayElementAtIndex(0);
            source.FindPropertyRelative("sourceName").stringValue="PowerSupply_01";
            source.FindPropertyRelative("positiveTerminalId").stringValue="Battery_positive";
            source.FindPropertyRelative("negativeTerminalId").stringValue="Battery_negative";
            source.FindPropertyRelative("nominalVoltage").floatValue=5f;
            var targets=so.FindProperty("targets"); targets.arraySize=Objectives.Length;
            for (int i=0;i<Objectives.Length;i++)
            {
                var target=targets.GetArrayElementAtIndex(i);
                target.FindPropertyRelative("targetId").stringValue=$"level33_jam_guard_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the solved timer voltage and motor state during the jam test.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel33JamGuard.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_33.asset");
            return level;
        }

        private static void WriteBindings(SparkLevelSceneBindings bindings,List<SparkTerminal> pins,SparkPowerSupply supply)
        {
            var so=new SerializedObject(bindings);
            var terminals=so.FindProperty("terminals"); terminals.arraySize=pins.Count;
            for(int i=0;i<pins.Count;i++)
            {
                var row=terminals.GetArrayElementAtIndex(i);
                row.FindPropertyRelative("id").stringValue=pins[i].name;
                row.FindPropertyRelative("terminal").objectReferenceValue=pins[i];
            }
            var components=so.FindProperty("components"); components.arraySize=1;
            var power=components.GetArrayElementAtIndex(0);
            power.FindPropertyRelative("id").stringValue="PowerSupply_01";
            power.FindPropertyRelative("component").objectReferenceValue=supply;
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel33JamGuard.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel33JamGuard.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }


        private static T Part<T>(Transform parent,string name,float x,float z,Material material) where T:SparkElectricalComponent
        {
            var obj=Child(parent,name,new Vector3(x,.055f,z));
            Solid(obj,"Body",PrimitiveType.Cube,Vector3.zero,new Vector3(.22f,.09f,.13f),material);
            var component=obj.gameObject.AddComponent<T>();
            Set(component,("objectId",name.Replace(' ','_')),("displayName",name));
            return component;
        }

        private static SparkTerminal Pin(SparkElectricalComponent owner,string id,float x,float z,Material material,
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
            Set(terminal,("owner",owner),("kind",kind),("polarity",polarity),("maxConnections",3));
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
            text.characterSize=.022f; text.fontSize=42; text.color=Color.white;
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
            string path=$"{Folder}/L33 {name}.mat";
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


