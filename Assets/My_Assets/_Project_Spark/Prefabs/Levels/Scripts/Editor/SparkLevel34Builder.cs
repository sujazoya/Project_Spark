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
    public static class SparkLevel34Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_34_Layout";
        private const string PrefabPath = Folder + "/Level_34_Priority_Power_Layout.prefab";
        private static readonly string[] Objectives = {
            "Power the pump alone", "Power the pump and fan", "Record the demand sag",
            "Restore pump voltage by shedding the fan", "Keep the fan off after demand is removed", "Restore the fan manually"
        };

        [MenuItem("Tools/Project Spark/Level 34/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 34 prefab already exists at " + PrefabPath);
                SparkLevel34DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader==null || font==null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level=AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder+"/Level_34.asset");
            if (level!=null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_34.asset has different objectives.");
            if (level==null) level=CreateDefinition();

            var board=MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel=MaterialAsset(shader,"Modules",new Color(.25f,.35f,.43f));
            var blue=MaterialAsset(shader,"Return sockets",new Color(.15f,.62f,.92f));
            var red=MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var aqua=MaterialAsset(shader,"Bus and pump",new Color(.14f,.82f,.75f));
            var amber=MaterialAsset(shader,"Auxiliary and demand",new Color(.95f,.56f,.16f));
            var green=MaterialAsset(shader,"Pump on",new Color(.14f,.70f,.38f));
            var gray=MaterialAsset(shader,"Motor housings",new Color(.46f,.54f,.61f));

            var root=new GameObject("Level_34_Priority_Power_Layout");
            try
            {
                var bench=Child(root.transform,"Priority Power Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"Board",PrimitiveType.Cube,new Vector3(0,-.06f,0),new Vector3(2.15f,.10f,1.55f),board);
                var pins=new List<SparkTerminal>();
                var supply=Part<SparkPowerSupply>(bench,"PowerSupply_01",-.87f,.50f,steel);
                Set(supply,("positiveTerminal",Pin(supply,"Battery_positive",-.07f,0,red,pins,SparkTerminalPolarity.Positive)),
                    ("negativeTerminal",Pin(supply,"Battery_negative",.07f,0,blue,pins,SparkTerminalPolarity.Negative)),
                    ("outputVoltage",5f),("currentLimit",.35f),("state",SparkPowerSupplyState.Active));
                Label(supply.transform,"5 V SUPPLY",font);
                var feeder=Resistor(bench,"FEEDER 10 OHM","FEED",-.52f,.50f,10f,1f,steel,red,aqua,amber,font,pins);
                var pump=Motor(bench,"CRITICAL PUMP","Pump",.43f,.50f,steel,aqua,blue,gray,green,font,pins,out var pumpShaft);
                var aux=Contact(bench,"AUX CUTOFF","AUX",-.04f,.02f,steel,aqua,blue,font,pins);
                var fan=Motor(bench,"AUXILIARY FAN","Fan",.43f,.02f,steel,amber,blue,gray,amber,font,pins,out var fanShaft);
                var demand=Contact(bench,"DEMAND SWITCH","DEMAND",-.04f,-.46f,steel,aqua,blue,font,pins);
                var bank=Resistor(bench,"DEMAND 47 OHM","R_DEMAND",.43f,-.46f,47f,.50f,steel,amber,blue,gray,font,pins);

                var pumpLight=Beacon(bench,"PUMP RUN",new Vector3(.77f,.13f,.50f),green,new Color(.2f,1f,.43f),font);
                var fanLight=Beacon(bench,"FAN RUN",new Vector3(.77f,.13f,.02f),aqua,new Color(.2f,1f,.85f),font);
                var shedLight=Beacon(bench,"AUX SHED",new Vector3(.93f,.13f,.02f),red,new Color(1f,.14f,.15f),font);
                var demandLight=Beacon(bench,"DEMAND ON",new Vector3(.77f,.13f,-.46f),amber,new Color(1f,.58f,.21f),font);

                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,supply);
                var layout=root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller=root.AddComponent<SparkLevel34PriorityPower>();
                controller.layout=layout; controller.source=supply; controller.feeder=feeder;
                controller.pump=pump; controller.fan=fan; controller.auxContact=aux; controller.demandContact=demand;
                controller.demandBank=bank; controller.pumpShaft=pumpShaft; controller.fanShaft=fanShaft;
                controller.pumpLight=pumpLight; controller.fanLight=fanLight; controller.shedLight=shedLight; controller.demandLight=demandLight;
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 34 prefab.");
                SparkLevel34DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 34 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_34_Layout");
        }

        private static SparkSwitch Contact(Transform parent,string name,string id,float x,float z,Material body,Material a,Material b,Font font,List<SparkTerminal> pins)
        {
            var contact=Part<SparkSwitch>(parent,name,x,z,body);
            Set(contact,("inputTerminal",Pin(contact,id+"_IN",-.07f,0,a,pins)),
                ("outputTerminal",Pin(contact,id+"_OUT",.07f,0,b,pins)),("state",SparkSwitchState.Open));
            Label(contact.transform,name,font);
            return contact;
        }

        private static SparkResistor Resistor(Transform parent,string name,string id,float x,float z,float ohms,float watts,
            Material body,Material a,Material b,Material element,Font font,List<SparkTerminal> pins)
        {
            var resistor=Part<SparkResistor>(parent,name,x,z,body);
            Set(resistor,("terminalA",Pin(resistor,id+"_IN",-.07f,0,a,pins)),
                ("terminalB",Pin(resistor,id+"_OUT",.07f,0,b,pins)),("resistance",ohms),("maximumPowerWatts",watts));
            Set(resistor.GetComponent<SparkResistorBurn>(),("ratedWatts",watts),("coolingWattsPerC",watts*.02f));
            Solid(resistor.transform,"Resistor element",PrimitiveType.Cube,new Vector3(0,.074f,.038f),new Vector3(.10f,.028f,.036f),element);
            Label(resistor.transform,name,font);
            return resistor;
        }

        private static SparkMotor Motor(Transform parent,string name,string id,float x,float z,Material body,
            Material a,Material b,Material housingMaterial,Material accent,Font font,List<SparkTerminal> pins,out Transform shaft)
        {
            var motor=Part<SparkMotor>(parent,name,x,z,body);
            Set(motor,("positiveTerminal",Pin(motor,id+"_A",-.07f,0,a,pins)),
                ("negativeTerminal",Pin(motor,id+"_B",.07f,0,b,pins)),
                ("windingResistance",47f),("startupVoltage",2.70f),("maximumCurrent",.18f));
            Label(motor.transform,name,font);
            var housing=Solid(motor.transform,"Housing",PrimitiveType.Cylinder,new Vector3(-.005f,.11f,.095f),new Vector3(.105f,.064f,.105f),housingMaterial);
            housing.transform.localRotation=Quaternion.Euler(0,0,90);
            shaft=Child(motor.transform,"Rotating shaft",new Vector3(.080f,.11f,.095f));
            var hub=Solid(shaft,"Hub",PrimitiveType.Cylinder,new Vector3(.012f,0,0),new Vector3(.034f,.024f,.034f),housingMaterial);
            hub.transform.localRotation=Quaternion.Euler(0,0,90);
            for(int i=0;i<4;i++)
            {
                var blade=Child(shaft,"Rotor blade",Vector3.zero);
                blade.localRotation=Quaternion.Euler(90*i,0,0);
                Solid(blade,"Blade",PrimitiveType.Cube,new Vector3(.025f,.032f,0),new Vector3(.018f,.047f,.013f),accent);
            }
            return motor;
        }

        private static Light Beacon(Transform parent,string name,Vector3 p,Material material,Color color,Font font)
        {
            var marker=Child(parent,name,p);
            var bulb=Solid(marker,"Indicator",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.058f,material);
            var light=bulb.AddComponent<Light>(); light.type=LightType.Point; light.range=.25f;
            light.intensity=2f; light.color=color; light.enabled=false;
            Label(marker,name,font,-.060f);
            return light;
        }

        private static void BuildControls(Transform parent,SparkLevel34PriorityPower controller,Font font)
        {
            var obj=new GameObject("Level 34 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(990,162),new Vector2(0,91));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;
            controller.statusText=UIText(panel,"Status","Wire the feeder and three load branches.",font,new Vector2(950,30),new Vector2(0,126));
            controller.voltageText=UIText(panel,"Voltage","SOURCE —   |   BUS —   |   SAG —",font,new Vector2(950,27),new Vector2(0,93));
            controller.currentText=UIText(panel,"Current","PUMP —   |   FAN —   |   DEMAND —",font,new Vector2(950,27),new Vector2(0,65));
            controller.auxButton=Control(panel,font,"CONNECT AUX",-345,controller.ConnectAux,out _);
            controller.demandButton=Control(panel,font,"APPLY DEMAND",-115,controller.ToggleDemand,out controller.demandLabel);
            controller.restoreButton=Control(panel,font,"RESTORE AUX",115,controller.RestoreAux,out _);
            controller.retryButton=Control(panel,font,"RESTART TEST",345,controller.RestartTest,out _);
        }

        private static Button Control(Transform parent,Font font,string caption,float x,UnityAction action,out Text label)
        {
            var rect=Rect(parent,caption,new Vector2(205,38),new Vector2(x,13));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.48f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            label=UIText(rect,"Label",caption,font,new Vector2(205,38),Vector2.zero);
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_33_Layout/Level_33_Jam_Guard_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Jam Guard Workbench") : null;
            if(anchor==null)
            {
                previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_32_Layout/Level_32_Optical_Sorter_Layout.prefab");
                anchor=previous!=null ? previous.transform.Find("Optical Workbench") : null;
            }
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }
        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_34" || targets.arraySize!=SparkLevel34PriorityPower.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel34PriorityPower.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_34"),("displayName","Priority Power - Automatic Load Shedding"),
                ("description","Wire a shared bus, observe demand sag, shed the auxiliary load, and restore it after recovery."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level34_priority_power_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from solved bus voltage and branch currents during the load shedding test.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel34PriorityPower.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_34.asset");
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
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel34PriorityPower.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel34PriorityPower.Gates[i];
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
            string path=$"{Folder}/L34 {name}.mat";
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



