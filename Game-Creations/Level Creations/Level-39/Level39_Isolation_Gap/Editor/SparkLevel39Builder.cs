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
    public static class SparkLevel39Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        internal const string Folder = Parent + "/Level_39_Layout";
        private const string PrefabPath = Folder + "/Level_39_Isolation_Gap_Layout.prefab";
        private static readonly string[] Objectives = {
            "Wire both circuits", "Send an active-low signal", "Verify the receiver pull-up",
            "Prove independent receiver power", "Detect the return bridge", "Restore and verify isolation"
        };

        [MenuItem("Tools/Project Spark/Level 39/Create Level Assets")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder();
            if (AssetDatabase.LoadMainAssetAtPath(PrefabPath) != null)
            {
                Debug.Log("Level 39 prefab already exists at " + PrefabPath);
                SparkLevel39DrawingAssetCreator.CreateAsset();
                return;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (shader == null || font == null) throw new InvalidOperationException("Could not load a lit shader or the built-in font.");
            var level = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_39.asset");
            if (level != null && !MatchesLevel(level)) throw new InvalidOperationException("Existing Level_39.asset has different objectives.");
            if (level == null) level = CreateDefinition();

            var board = MaterialAsset(shader,"Bench",new Color(.07f,.12f,.16f));
            var steel = MaterialAsset(shader,"Modules",new Color(.25f,.35f,.43f));
            var blue = MaterialAsset(shader,"Return",new Color(.15f,.62f,.92f));
            var red = MaterialAsset(shader,"Supply positive",new Color(.90f,.24f,.28f));
            var cyan = MaterialAsset(shader,"Receiver signal",new Color(.14f,.82f,.75f));
            var gold = MaterialAsset(shader,"Transmitter LED",new Color(.95f,.56f,.16f));
            var violet = MaterialAsset(shader,"Receiver return",new Color(.58f,.45f,.98f));

            var root = new GameObject("Level_39_Isolation_Gap_Layout");
            try
            {
                var bench = Child(root.transform,"Isolation Workbench",Vector3.zero);
                AlignBench(root.transform,bench);
                Solid(bench,"TX board",PrimitiveType.Cube,new Vector3(-.56f,-.06f,0),new Vector3(1.0f,.10f,1.55f),board);
                Solid(bench,"RX board",PrimitiveType.Cube,new Vector3(.56f,-.06f,0),new Vector3(1.0f,.10f,1.55f),board);
                Label(Child(bench,"Isolation gap label",new Vector3(0,0,.02f)),"ISOLATION GAP",font,0f);
                var pins = new List<SparkTerminal>();
                var tx=Supply(bench,"TX_SUPPLY","TX",-.68f,steel,red,blue,font,pins);
                var rx=Supply(bench,"RX_SUPPLY","RX",.68f,steel,red,violet,font,pins);
                var send=Contact(bench,"SEND SWITCH","SEND_IN","SEND_OUT",-.68f,.10f,steel,red,gold,font,pins);
                var txResistor=Resistor(bench,"470 Ω TX LIMIT","TX_R_IN","TX_R_OUT",-.73f,-.27f,470f,.25f,steel,gold,gold,font,pins);
                var pullUp=Resistor(bench,"1 kΩ PULL-UP","PULLUP_IN","PULLUP_OUT",.68f,.10f,1000f,.25f,steel,red,cyan,font,pins);
                var emitter=Part<SparkLED>(bench,"OPTO LED",-.23f,-.27f,steel);
                Set(emitter,("anodeTerminal",Pin(emitter,"OPTO_A",-.07f,0,gold,pins)),
                    ("cathodeTerminal",Pin(emitter,"OPTO_K",.07f,0,blue,pins)),
                    ("forwardVoltage",2f),("maximumForwardCurrent",.02f));
                Label(emitter.transform,"OPTO LED  A  /  K",font);
                var output=Contact(bench,"OPTO OUTPUT","OPTO_C","OPTO_E",.24f,-.27f,steel,cyan,violet,font,pins);
                var txGlow=Beacon(bench,"TX LIGHT",new Vector3(-.23f,.09f,-.50f),gold,new Color(1f,.55f,.20f),font);
                var rxGlow=Beacon(bench,"RX LOW",new Vector3(.24f,.09f,-.50f),cyan,new Color(.20f,1f,.83f),font);
                var isolation=Beacon(bench,"ISOLATED",new Vector3(.73f,.09f,-.27f),cyan,new Color(.20f,1f,.83f),font);
                var warning=Beacon(bench,"BRIDGED",new Vector3(.73f,.09f,-.53f),red,new Color(1f,.15f,.14f),font);

                var bindings = root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pins,tx,rx);
                var layout = root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",level),("sceneBindings",bindings));
                var controller = root.AddComponent<SparkLevel39IsolationGap>();
                controller.layout=layout;
                controller.transmitterSupply=tx;
                controller.receiverSupply=rx;
                controller.sendContact=send;
                controller.receiverContact=output;
                controller.transmitterResistor=txResistor;
                controller.pullUp=pullUp;
                controller.transmitterLed=emitter;
                controller.transmitterGlow=txGlow;
                controller.receiverGlow=rxGlow;
                controller.isolationGlow=isolation;
                controller.bridgeGlow=warning;
                BuildControls(root.transform,controller,font);

                var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                if(prefab==null) throw new InvalidOperationException("Could not save the Level 39 prefab.");
                SparkLevel39DrawingAssetCreator.CreateAsset();
                AssetDatabase.SaveAssets();
                Selection.activeObject=prefab;
                Debug.Log("Level 39 assets created. Register the definition, prefab and teaching drawing in the active scene.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        internal static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Parent)) throw new InvalidOperationException("Project Spark Prefabs/Levels folder is missing.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Parent,"Level_39_Layout");
        }

        private static SparkPowerSupply Supply(Transform bench,string name,string id,float x,Material body,Material positive,
            Material negative,Font font,List<SparkTerminal> pins)
        {
            var supply=Part<SparkPowerSupply>(bench,name,x,.53f,body);
            Set(supply,("positiveTerminal",Pin(supply,id+"_POS",-.07f,0,positive,pins,SparkTerminalPolarity.Positive)),
                ("negativeTerminal",Pin(supply,id+"_RETURN",.07f,0,negative,pins,SparkTerminalPolarity.Negative)),
                ("outputVoltage",5f),("currentLimit",.05f),("state",SparkPowerSupplyState.Active));
            Label(supply.transform,id+" 5 V  +  /  RETURN",font);
            return supply;
        }

        private static SparkSwitch Contact(Transform bench,string name,string inputId,string outputId,float x,float z,
            Material body,Material input,Material output,Font font,List<SparkTerminal> pins)
        {
            var contact=Part<SparkSwitch>(bench,name,x,z,body);
            Set(contact,("inputTerminal",Pin(contact,inputId,-.07f,0,input,pins)),
                ("outputTerminal",Pin(contact,outputId,.07f,0,output,pins)),("state",SparkSwitchState.Open));
            Label(contact.transform,name,font);
            return contact;
        }

        private static SparkResistor Resistor(Transform bench,string name,string pinA,string pinB,float x,float z,float resistance,
            float maximumPower,Material body,Material a,Material b,Font font,List<SparkTerminal> pins)
        {
            var arm=Part<SparkResistor>(bench,name,x,z,body);
            var input=Pin(arm,pinA,-.07f,0,a,pins);
            var output=Pin(arm,pinB,.07f,0,b,pins);
            Set(arm,("terminalA",input),("terminalB",output),("resistance",resistance),("maximumPowerWatts",maximumPower));
            var element=Solid(arm.transform,"Cartridge",PrimitiveType.Cylinder,new Vector3(0,.075f,.042f),new Vector3(.041f,.056f,.041f),body);
            element.transform.localRotation=Quaternion.Euler(0,0,90);
            Label(arm.transform,name,font);
            return arm;
        }

        private static void BuildControls(Transform parent,SparkLevel39IsolationGap c,Font font)
        {
            var obj=new GameObject("Level 39 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Panel",new Vector2(1100,127),new Vector2(0,65));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.035f,.08f,.13f,.94f); back.raycastTarget=false;
            c.statusText=UIText(panel,"Status","Wire the transmitter and receiver.",font,new Vector2(1060,28),new Vector2(0,91));
            c.meterText=UIText(panel,"Measurements","TX LED —   |   RX OUT —   |   CHECK WIRING",font,new Vector2(1060,28),new Vector2(0,61));
            c.meterText.color=new Color(.40f,.92f,.93f); c.meterText.fontSize=23;
            c.sendButton=Control(panel,font,"SEND ON",-204,13,c.ToggleSend);
            c.receiverPowerButton=Control(panel,font,"RX POWER OFF",0,13,c.ToggleReceiverPower);
            c.restartButton=Control(panel,font,"RESTART TEST",204,13,c.Restart);
            c.sendLabel=c.sendButton.GetComponentInChildren<Text>();
            c.receiverPowerLabel=c.receiverPowerButton.GetComponentInChildren<Text>();
        }

        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(192,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.48f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(192,37),Vector2.zero);
            return button;
        }

        private static void AlignBench(Transform root,Transform bench)
        {
            var previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_38_Layout/Level_38_One_Press_Layout.prefab");
            var anchor=previous!=null ? previous.transform.Find("Debounce Workbench") : null;
            if(anchor==null)
            {
                previous=AssetDatabase.LoadAssetAtPath<GameObject>(Parent+"/Level_37_Layout/Level_37_Kelvin_Test_Layout.prefab");
                anchor=previous!=null ? previous.transform.Find("Kelvin Test Workbench") : null;
            }
            if(anchor==null) return;
            CopyLocalTransform(root,previous.transform);
            CopyLocalTransform(bench,anchor);
        }
        private static GameObject Beacon(Transform parent,string name,Vector3 p,Material material,Color color,Font font)
        {
            var marker=Child(parent,name,p);
            var bulb=Solid(marker,"Indicator",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.058f,material);
            var light=bulb.AddComponent<Light>(); light.type=LightType.Point; light.range=.25f;
            light.intensity=2f; light.color=color;
            Label(marker,name,font,-.060f);
            bulb.SetActive(false);
            return bulb;
        }

        private static bool MatchesLevel(SparkLevelDefinition level)
        {
            var targets=new SerializedObject(level).FindProperty("targets");
            if (level.LevelId!="level_39" || targets.arraySize!=SparkLevel39IsolationGap.Gates.Length) return false;
            for (int i=0;i<targets.arraySize;i++)
                if (targets.GetArrayElementAtIndex(i).FindPropertyRelative("targetComponentId").stringValue!=SparkLevel39IsolationGap.Gates[i]) return false;
            return true;
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_39"),("displayName","Across the Isolation Gap"),
                ("description","Send an active-low signal across isolated power circuits and detect a return bridge."),
                ("completionMode",0),("requiredTargetCount",Objectives.Length),("requireClosedReturn",false),
                ("rejectShortCircuit",true),("rejectTargetShort",true),("requireMinimumVoltage",false),
                ("autoAdvanceOnCompletion",false));
            var so=new SerializedObject(level);
            var sources=so.FindProperty("powerSources"); sources.arraySize=2;
            for (int i=0;i<sources.arraySize;i++)
            {
                string id=i==0 ? "TX" : "RX";
                var source=sources.GetArrayElementAtIndex(i);
                source.FindPropertyRelative("sourceName").stringValue=id+"_SUPPLY";
                source.FindPropertyRelative("positiveTerminalId").stringValue=id+"_POS";
                source.FindPropertyRelative("negativeTerminalId").stringValue=id+"_RETURN";
                source.FindPropertyRelative("nominalVoltage").floatValue=5f;
            }
            var targets=so.FindProperty("targets"); targets.arraySize=Objectives.Length;
            for (int i=0;i<Objectives.Length;i++)
            {
                var target=targets.GetArrayElementAtIndex(i);
                target.FindPropertyRelative("targetId").stringValue=$"level39_isolation_gap_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from solved LED current, receiver voltage, and wire topology.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel39IsolationGap.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_39.asset");
            return level;
        }

        private static void WriteBindings(SparkLevelSceneBindings bindings,List<SparkTerminal> pins,SparkPowerSupply tx,SparkPowerSupply rx)
        {
            var so=new SerializedObject(bindings);
            var terminals=so.FindProperty("terminals"); terminals.arraySize=pins.Count;
            for(int i=0;i<pins.Count;i++)
            {
                var row=terminals.GetArrayElementAtIndex(i);
                row.FindPropertyRelative("id").stringValue=pins[i].name;
                row.FindPropertyRelative("terminal").objectReferenceValue=pins[i];
            }
            var components=so.FindProperty("components"); components.arraySize=2;
            for (int i=0;i<components.arraySize;i++)
            {
                var power=components.GetArrayElementAtIndex(i);
                power.FindPropertyRelative("id").stringValue=i==0 ? "TX_SUPPLY" : "RX_SUPPLY";
                power.FindPropertyRelative("component").objectReferenceValue=i==0 ? tx : rx;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel39IsolationGap.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel39IsolationGap.Gates[i];
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
            string path=$"{Folder}/L39 {name}.mat";
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




