#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Visuals;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    public static class SparkLevel92Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        private const string Folder = Parent + "/Level_92_Layout";
        private const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        
        private static readonly string[] Objectives = {
            "Build the Asteroid Exploration Network",
            "Map the Hidden Geological Layers",
            "Deploy and Stabilize the Microgravity Lander",
            "Extract and Secure the Asteroid Core",
            "Recover Three Asteroid Sampling Failures",
            "Restore Critical Anchor Load Feedback"
        };

        [MenuItem("Tools/Project Spark/Levels/Create Level 92")]
        public static void Create() => Build(true);

        [MenuItem("Tools/Project Spark/Levels/92/Build Layout Prefab")]
        public static void BuildLayoutOnly() => Build(false);

        internal static void RebuildLayout() => Build(false, true);

        private static void Build(bool register, bool rebuild = false)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            string path = Folder + "/Level_92_Layout.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing && !rebuild)
            {
                if (register) Register(existing.GetComponent<SparkLevelLayout>());
                return;
            }

            var models = Directory.GetFiles(Folder + "/Models","*.fbx").OrderBy(p => p,StringComparer.Ordinal).ToArray();
            var portData = AssetDatabase.LoadAssetAtPath<TextAsset>(Folder + "/LogicalPorts.txt");
            var rows = portData.text.Split(new[]{ '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(row => row.Split('\t')).ToArray();
            if (models.Length != 98 || rows.Length != 376 || rows.Any(row => row.Length != 8))
                throw new InvalidOperationException("Level 92 needs 98 modular FBXs and 376 contact records.");

            var root = new GameObject("Level_92_Layout");
            try
            {
                var arena = Child(root.transform, "Asteroid Sampling Mission Deck", Vector3.zero);
                // Match the working Level 88 scene placement before fitting to the table at runtime.
                var reference = AssetDatabase.LoadAssetAtPath<GameObject>(Parent + "/Level_88_Layout/Level_88_Layout.prefab");
                if (reference)
                {
                    CopyLocalTransform(root.transform, reference.transform);
                    var referenceArena = reference.GetComponent<SparkLevel88Process>().arena;
                    CopyLocalTransform(arena, referenceArena);
                }

                var art = Child(arena,"L92_HD_VISUAL_ART",Vector3.zero);
                foreach (string file in models)
                {
                    string assetPath = file.Replace('\\','/');
                    string meshName = Path.GetFileNameWithoutExtension(file);
                    var model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(model,art);
                    instance.name = meshName.Substring(3);
                    instance.transform.SetLocalPositionAndRotation(Vector3.zero, model.transform.localRotation);
                    instance.transform.localScale = Vector3.one;
                }

                var panorama = art.Find("L92_PanoramicKhepri7ObservationWindow");
                if (panorama) panorama.localPosition = Vector3.back * 29f;
                SparkLevel92MaterialUpgradeEditor.Apply(art.gameObject, Folder);

                var baseArt = art.Find("L92_AsteroidSamplingMissionDeck");
                var meters = baseArt.GetComponentsInChildren<Renderer>().Select(r => ToLocalBounds(art,r.bounds)).Aggregate((a,b) => { a.Encapsulate(b); return a; });
                if (meters.size.x < 15 || meters.size.x > 55 || meters.size.z < 15 || meters.size.y > 4)
                    throw new InvalidOperationException($"Unexpected imported base size {meters.size}. Model rotation {baseArt.localRotation.eulerAngles}. Check the imported FBX axes and units.");
                var socketTransforms = art.GetComponentsInChildren<Transform>(true)
                    .Where(t => t.name.StartsWith("SOCKET_P",StringComparison.Ordinal))
                    .GroupBy(t => t.name).ToDictionary(group => group.Key,group => group.First());
                var connectors = Child(arena,"Fixed Equipment Contacts",Vector3.zero);
                var source = Supply(connectors,"CONTROL_POWER",5,.7f);
                var primary = Supply(connectors,"Anchor load cell transmitter",1,.01f);
                var secondary = Supply(connectors,"Independent tether protection",1,.01f);
                var carrier = Child(connectors,"Isolated signal endpoints",Vector3.zero).gameObject.AddComponent<SparkResistor>();
                Set(carrier,("electricalEnabled",false));
                var positive = HiddenPin(source,"SUPPLY_POS",SparkTerminalPolarity.Positive);
                var ground = HiddenPin(source,"SUPPLY_RET",SparkTerminalPolarity.Negative);
                Set(source,("positiveTerminal",positive),("negativeTerminal",ground));
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                var colors = new Dictionary<string, Material>();
                var pinList = new List<SparkTerminal>();
                var labelList = new List<TextMesh>();
                var names = new List<string>();
                var positions = new List<Vector3>();
                Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                for (int i=0;i<rows.Length;i++)
                {
                    var row = rows[i];
                    bool input = (i & 1) != 0;
                    int wire = i/2;
                    var owner = (SparkComponent)carrier;
                    if (!input && wire == 170) owner = primary;
                    if (!input && wire == 158) owner = secondary;
                    if (input && ((wire < 12 && row[3] != "RETURN") || wire == 170 || wire == 158))
                        owner = Child(connectors,row[0]+" receiver",Vector3.zero).gameObject.AddComponent<SparkResistor>();

                    if (!colors.TryGetValue(row[3],out var material))
                    {
                        material = MaterialAsset(shader,row[3],row[0].StartsWith("W171") ? new Color(1,.64f,.08f) : row[3] switch {
                            "POWER" => new Color(.98f,.18f,.28f), "RETURN" => new Color(.15f,.48f,1),
                            "ANALOG_SAFETY_FEEDBACK" or "ANALOG_SENSOR" => new Color(.15f,.8f,1), "SAFETY_INTERLOCK" => new Color(1,.67f,.16f),
                            _ => new Color(.23f,1,.64f) });
                        colors.Add(row[3],material);
                    }
                    if (wire == 170) material = MaterialAsset(shader,"W171",new Color(1,.55f,.09f));
                    var pin = Pin(owner,row[0],0,0,material,pinList);
                    var position = new Vector3(Parse(row[4]),Parse(row[5]),Parse(row[6]));
                    if (socketTransforms.TryGetValue("SOCKET_"+row[2],out var socket))
                        position = arena.InverseTransformPoint(socket.TryGetComponent<Renderer>(out var socketMesh) ? socketMesh.bounds.center : socket.position);
                    position.y += .10f;
                    Vector3 socketCenter = position;
                    int slot = 0;
                    while (positions.Any(other => (other-position).sqrMagnitude < .18f*.18f))
                    {
                        slot++;
                        int side = (slot % 2 == 0) ? -1 : 1;
                        int ring = (slot+1)/2;
                        position = socketCenter + new Vector3(side*.21f*((ring-1)%4+1),0,.21f*((ring-1)/4));
                    }
                    positions.Add(position);
                    pin.transform.position = arena.TransformPoint(position);
                    pin.transform.localScale = new Vector3(.16f,.085f,.16f);
                    Set(pin,("providesOutput",!input),("acceptsInput",input));
                    if (owner is SparkResistor load && load != carrier)
                        Set(load,("terminalA",pin),("terminalB",ground),("resistance",2200f));
                    var label = Child(connectors,row[0]+" label",position+Vector3.up*.33f).gameObject.AddComponent<TextMesh>();
                    label.text = $"W{wire+1:000} {(input ? "IN" : "OUT")}";
                    label.font = font; label.anchor = TextAnchor.MiddleCenter;
                    label.fontSize = 72; label.characterSize = .13f; label.color = Color.white;
                    label.GetComponent<Renderer>().sharedMaterial = font.material;
                    label.gameObject.SetActive(false);
                    labelList.Add(label);
                    names.Add(ObjectNames.NicifyVariableName(row[1].Substring(4)) + " / " + label.text);
                }

                for (int wire = 0; wire < 12; wire++)
                    Tap(connectors, rows[wire * 2][3] == "RETURN" ? ground : positive, pinList[wire * 2]);
                Set((SparkResistor)pinList[5].Owner, ("terminalB", pinList[7]));
                Set((SparkResistor)pinList[9].Owner, ("terminalB", pinList[15]));
                Set((SparkResistor)pinList[21].Owner, ("terminalB", pinList[23]));
                Set(primary,("positiveTerminal",pinList[340]),("negativeTerminal",ground));
                Set(secondary,("positiveTerminal",pinList[316]),("negativeTerminal",ground));

                var definition = CreateDefinition();
                var bindings = root.AddComponent<SparkLevelSceneBindings>();
                WriteBindings(bindings,pinList,source);
                var layout = root.AddComponent<SparkLevelLayout>();
                Set(layout,("levelDefinition",definition),("sceneBindings",bindings));
                var process = root.AddComponent<SparkLevel92Process>();
                process.layout = layout; process.source = source; process.primary = primary; process.secondary = secondary;
                process.pins = pinList.ToArray(); process.ground = ground; process.arena = arena;
                process.labels = labelList.ToArray(); process.endpoints = names.ToArray();
                process.purposes = rows.Where((_,i)=>i%2==0).Select(row=>row[7]).ToArray();
                process.visuals = arena.gameObject.AddComponent<SparkLevel92VisualRig>();
                Set(process.visuals,("visualRoot",art),
                    ("missionQualifiedLight", Accent(art, "Core qualified", new Vector3(0, 5, 1), new Color(.16f, 1f, .61f))),
                    ("normalOperationLight", Accent(art, "Sampling operating", new Vector3(-6, 8, -8), new Color(1f, .41f, .06f))),
                    ("criticalW171FailureLight", Accent(art, "Safety hold", new Vector3(2, 4, 0), new Color(1f, .07f, .08f))));
                BuildControls(root.transform,process,font);

                var prefab = PrefabUtility.SaveAsPrefabAsset(root,path);
                if (!prefab) throw new InvalidOperationException("Could not save Level 92 prefab.");
                AssetDatabase.SaveAssets();
                if (register) Register(prefab.GetComponent<SparkLevelLayout>());
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static Light Accent(Transform parent, string name, Vector3 position, Color color)
        {
            var light = Child(parent, name, position).gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 3f;
            light.intensity = 1.5f;
            light.color = color;
            light.enabled = false;
            return light;
        }

        private static float Parse(string value) => float.Parse(value,CultureInfo.InvariantCulture);

        private static Bounds ToLocalBounds(Transform space, Bounds world)
        {
            var b = new Bounds(space.InverseTransformPoint(world.center),Vector3.zero);
            for (int x=-1;x<=1;x+=2)
            for (int y=-1;y<=1;y+=2)
            for (int z=-1;z<=1;z+=2)
                b.Encapsulate(space.InverseTransformPoint(world.center+Vector3.Scale(world.extents,new Vector3(x,y,z))));
            return b;
        }

        private static SparkPowerSupply Supply(Transform parent,string name,float voltage,float current)
        {
            var source = Child(parent,name,Vector3.zero).gameObject.AddComponent<SparkPowerSupply>();
            Set(source,("objectId",name.Replace(' ','_')),("displayName",name),
                ("outputVoltage",voltage),("currentLimit",current),("state",SparkPowerSupplyState.Off));
            return source;
        }

        private static SparkTerminal HiddenPin(SparkPowerSupply owner,string name,SparkTerminalPolarity polarity)
        {
            var terminal = Child(owner.transform,name,Vector3.zero).gameObject.AddComponent<SparkTerminal>();
            Set(terminal,("owner",owner),("polarity",polarity),("maxConnections",32),
                ("kind",polarity==SparkTerminalPolarity.Positive?SparkTerminalKind.Power:SparkTerminalKind.Ground));
            return terminal;
        }

        private static void Tap(Transform parent,SparkTerminal input,SparkTerminal output)
        {
            var tap = Child(parent,"Internal tap "+output.name,Vector3.zero).gameObject.AddComponent<SparkSwitch>();
            Set(tap,("inputTerminal",input),("outputTerminal",output),("state",SparkSwitchState.Closed));
        }

        private static void Register(SparkLevelLayout prefab)
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
            try
            {
                if (scene.isDirty)
                {
                    Debug.LogWarning("Level 92 prefab is ready. Save your MainMenu changes and rerun Create Level 92 to register it.");
                    return;
                }
                T Find<T>() where T : Component => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)).Single();
                var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_92.asset");
                var loader = new SerializedObject(Find<SparkLevelLayoutLoader>());
                var campaign = new SerializedObject(Find<LevelGamePlayManager>());
                var catalog = new SerializedObject(loader.FindProperty("catalog").objectReferenceValue);
                var entries = catalog.FindProperty("entries");
                var entry = Enumerable.Range(0,entries.arraySize).Select(entries.GetArrayElementAtIndex)
                    .FirstOrDefault(item=>item.FindPropertyRelative("level").objectReferenceValue==definition);
                if (entry == null)
                {
                    entries.arraySize++;
                    entry=entries.GetArrayElementAtIndex(entries.arraySize-1);
                }
                entry.FindPropertyRelative("level").objectReferenceValue=definition;
                entry.FindPropertyRelative("prefab").objectReferenceValue=prefab;
                catalog.ApplyModifiedPropertiesWithoutUndo();
                var chapterAsset = AssetDatabase.LoadAssetAtPath<SparkChapterDefinition>(
                    "Assets/My_Assets/_Project_Spark/Scripts/ProjectSpark/Levels/Chapter/Chapter_09_SpaceSystems.asset");
                if (!chapterAsset) throw new InvalidOperationException("Space Systems chapter was not found.");
                var chapter = new SerializedObject(chapterAsset);
                var chapterLevels = chapter.FindProperty("levels");
                if (!Enumerable.Range(0, chapterLevels.arraySize).Any(i => chapterLevels.GetArrayElementAtIndex(i).objectReferenceValue == definition))
                {
                    chapterLevels.arraySize++;
                    chapterLevels.GetArrayElementAtIndex(chapterLevels.arraySize - 1).objectReferenceValue = definition;
                    chapter.ApplyModifiedPropertiesWithoutUndo();
                }

                var levels = campaign.FindProperty("levels");
                if (!Enumerable.Range(0,levels.arraySize).Any(i=>levels.GetArrayElementAtIndex(i).objectReferenceValue==definition))
                {
                    levels.arraySize++;
                    levels.GetArrayElementAtIndex(levels.arraySize-1).objectReferenceValue=definition;
                    campaign.ApplyModifiedPropertiesWithoutUndo();
                    EditorSceneManager.MarkSceneDirty(scene);
                }
                if (scene.isDirty && !EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save campaign registration.");
                AssetDatabase.SaveAssets();
                Debug.Log("Level 92 registered in MainMenu.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene,true); }
        }

        private static void BuildControls(Transform parent,SparkLevel92Process process,Font font)
        {
            var obj = new GameObject("Level 92 Controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas = obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=30;
            var scaler = obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1980,1080);
            var panel=Rect(canvas.transform,"Sampling SCADA",new Vector2(1100,169),new Vector2(-205,10));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.025f,.075f,.12f,.95f); back.raycastTarget=false;
            process.status=UIText(panel,"Status","SILENT CORE",font,new Vector2(1080,24),new Vector2(0,142));
            process.measurements=UIText(panel,"Measurements","",font,new Vector2(1080,24),new Vector2(0,117));
            process.measurements.color=new Color(.38f,.89f,.95f);
            process.instructions=UIText(panel,"Next action","",font,new Vector2(1092,28),new Vector2(0,89));
            process.instructions.fontSize=15;
            (string label,UnityAction action)[] controls = {
                ("5 V CONTROL",process.Power), ("CALIBRATE / SCAN",process.Calibrate), ("EXTRACT CORE",process.Run),
                ("ANCHOR SLIP",process.SlipAnchor), ("BIT STALL",process.StallBit), ("SEAL FAILURE",process.FailSeal),
                ("REPAIR",process.Repair), ("ARM W171",process.Arm), ("RESET SAFETY",process.ResetSafety),
                ("ARENA VIEW",process.ArenaView), ("SOCKET VIEW",process.SocketView), ("DEPLOY LANDER",process.Deploy)
            };
            for (int i=0;i<controls.Length;i++)
            {
                var button = Control(panel,font,controls[i].label,-420+168*(i%6),46-39*(i/6),controls[i].action);
                button.interactable=true;
            }
        }
        private static SparkLevelDefinition CreateDefinition()
        {
            var existing=AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder+"/Level_92.asset"); if(existing)return existing;
            var level=ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_92"),("displayName","The Silent Core"),
                ("description","Survey the asteroid, stabilize the lander and recover a sealed core under anchor-load protection."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level92_sampling_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel92Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.VoltageRange;
                target.FindPropertyRelative("minimumVoltage").floatValue=4.8f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=true;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(level,Folder+"/Level_92.asset");
            return level;
        }

        private static void WriteBindings(SparkLevelSceneBindings bindings,List<SparkTerminal> pins,SparkPowerSupply source)
        {
            var so=new SerializedObject(bindings);
            var terminals=so.FindProperty("terminals"); terminals.arraySize=pins.Count+2;
            for(int i=0;i<pins.Count;i++)
            {
                var row=terminals.GetArrayElementAtIndex(i);
                row.FindPropertyRelative("id").stringValue=pins[i].name;
                row.FindPropertyRelative("terminal").objectReferenceValue=pins[i];
            }
            var plus=terminals.GetArrayElementAtIndex(pins.Count+1);
            plus.FindPropertyRelative("id").stringValue=source.PositiveTerminal.name;
            plus.FindPropertyRelative("terminal").objectReferenceValue=source.PositiveTerminal;
            var ret=source.NegativeTerminal;
            var ground=terminals.GetArrayElementAtIndex(pins.Count);
            ground.FindPropertyRelative("id").stringValue=ret.name;
            ground.FindPropertyRelative("terminal").objectReferenceValue=ret;
            var fixedComponents=new List<(string id,SparkElectricalComponent component)> {
                ("CONTROL_POWER",source)
            };

            var components=so.FindProperty("components");
            components.arraySize=fixedComponents.Count;
            for(int i=0;i<fixedComponents.Count;i++)
            {
                var item=components.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue=fixedComponents[i].id;
                item.FindPropertyRelative("component").objectReferenceValue=fixedComponents[i].component;
            }
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel92Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel92Process.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }


        private static Button Control(Transform parent,Font font,string caption,float x,float y,UnityAction action)
        {
            var rect=Rect(parent,caption,new Vector2(158,37),new Vector2(x,y));
            var image=rect.gameObject.AddComponent<Image>(); image.color=new Color(.15f,.35f,.71f);
            var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.interactable=false;
            UnityEventTools.AddPersistentListener(button.onClick,action);
            UIText(rect,"Label",caption,font,new Vector2(158,37),Vector2.zero).fontSize=13;
            return button;
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

        private static Transform Child(Transform parent,string name,Vector3 position)
        {
            var child=new GameObject(name).transform;
            child.SetParent(parent,false); child.localPosition=position;
            return child;
        }

        private static Material MaterialAsset(Shader shader,string name,Color color)
        {
            string path=$"{Folder}/L92 {name}.mat";
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


