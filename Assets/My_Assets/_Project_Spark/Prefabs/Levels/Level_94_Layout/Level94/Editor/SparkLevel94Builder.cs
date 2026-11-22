#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.Level94.EditorTools;
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
    [InitializeOnLoad]
    public static class SparkLevel94Builder
    {
        private const string Parent = "Assets/My_Assets/_Project_Spark/Prefabs/Levels";
        private const string Folder = Parent + "/Level_94_Layout";
        private const string ScenePath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Request = "Logs/SparkLevel94.request", Report = "Logs/SparkLevel94.txt";
        private const string PrefabPath = Folder + "/Level_94_Layout.prefab";
        private const string PreviewRequest = "Logs/SparkLevel94Preview.request";
        private const string PreviewReport = "Logs/SparkLevel94Preview.txt";

        internal const string Kit = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_94_Layout/Level94";
        private static readonly string[] ObjectiveDetails = {
            "Review the 34 route records in the board netlist.",
            "Choose, align, fit, and solder each of the original footprint components.",
            "Wire the specified native pads and plate the six through-layer barrels.",
            "Read the live bench rail and feedback before acquiring a fresh load-current sample.",
            "Diagnose each seeded fault, isolate power to repair it, and validate a fresh run.",
            "Prove live W035 interruption, restored feedback, explicit reset, and fresh qualification."
        };
        private static readonly string[] Objectives = {
            "Inspect the PCB schematic and nets", "Install all 27 components",
            "Route 34 copper tasks and plate six vias", "Commission the protected board",
            "Repair all three faults and retest", "Recover W035 feedback and requalify"
        };

        static SparkLevel94Builder() => EditorApplication.update += Poll;
        private static void Poll()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (File.Exists(PreviewRequest))
            {
                File.Delete(PreviewRequest);
                try { BakePrefabPresentation(); }
                catch (Exception error) { File.WriteAllText(PreviewReport, "FAIL " + error); Debug.LogException(error); }
                return;
            }
            if (!File.Exists(Request)) return;
            File.Delete(Request);
            try { Create(); File.WriteAllText(Report,"Level 94 prefab generated. Inspect the Console for campaign registration status."); }
            catch(Exception error){ File.WriteAllText(Report,error.ToString()); Debug.LogException(error); }
        }

        [MenuItem("Tools/Project Spark/Level 94/Save Tabletop Prefab Presentation")]
        private static void BakePrefabPresentation()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var currentStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (currentStage != null && currentStage.assetPath == PrefabPath)
                throw new InvalidOperationException("Close Level 94 Prefab Mode before saving its tabletop presentation.");

            var opening = ReadWorktopOpening();
            var contents = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var process = contents.GetComponent<SparkLevel94Process>();
                if (!process) throw new InvalidOperationException("The Board Foundry controller is missing.");
                PreparePrefabPresentation(process, opening);
                PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath, out bool saved);
                if (!saved) throw new IOException("Level 94 tabletop presentation could not be saved.");
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }

            var verified = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var level = verified.GetComponent<SparkLevel94Process>();
                var substrate = level.arena.Find("L94_ART/L94_FR4BoardSubstrate").GetComponentsInChildren<Renderer>();
                var bounds = substrate[0].bounds;
                foreach (var surface in substrate) bounds.Encapsulate(surface.bounds);
                if (level.parts.Length != 27 || level.parts.Any(part => part.activeSelf) ||
                    level.arena.Find("L94_ART/L94_AssemblyBenchFrame").gameObject.activeSelf ||
                    Mathf.Abs(bounds.size.z - opening.y * .79f) > .02f || Mathf.Abs(bounds.min.y - .025f) > .01f)
                    throw new InvalidOperationException("The saved prefab did not retain its tabletop opening state.");
                File.WriteAllText(PreviewReport, $"PASS Saved board-only opening state in {PrefabPath}\n" +
                    $"PASS PCB {bounds.size.x:F2} × {bounds.size.z:F2} on the {opening.x:F2} × {opening.y:F2} table opening.\n" +
                    "PASS All 27 stock parts are inactive and the standalone fixture is inactive.\n");
            }
            finally { PrefabUtility.UnloadPrefabContents(verified); }
        }

        private static Vector2 ReadWorktopOpening()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded)
                throw new InvalidOperationException("Open MainMenu to read the shared game's table dimensions.");
            var room = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(child => child.name == "PS_PRO_GAME_ROOM");
            var deck = room ? room.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable") : null;
            if (!deck || !deck.TryGetComponent<BoxCollider>(out var table))
                throw new InvalidOperationException("The shared MainMenu worktop collider could not be located.");
            var opening = Vector3.Scale(table.size, deck.lossyScale);
            return new Vector2(opening.x, opening.z);
        }

        private static void PreparePrefabPresentation(SparkLevel94Process process, Vector2 opening)
        {
            if (!SparkLevel94BoardView.FitBoardToWorktop(process.arena, opening, process.transform.position, out _))
                throw new InvalidOperationException("The Board Foundry substrate could not be fitted.");
            foreach (var part in process.parts) part.SetActive(false);
            process.jumperArt.SetActive(false);
            foreach (var route in process.routes)
            {
                route.copper.enabled = false;
                if (route.backCopper) route.backCopper.enabled = false;
            }
            process.heading.GetComponentInParent<Canvas>(true).gameObject.SetActive(false);
        }

        [MenuItem("Tools/Project Spark/Levels/Create Level 94")]
        public static void Create()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            string prefabPath = PrefabPath;
            var existing=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if(existing)
            {
                var source=existing.GetComponent<SparkLevel94Process>().certification;
                if(source.PositiveTerminal.Owner != source || !source.transform.Find("Certification reference load") ||
                    !source.transform.parent.Find("PCB pad carrier/Passive terminal owner") ||
                    !source.transform.parent.Find("PCB pad carrier").GetComponent<SparkResistor>().ElectricalEnabled)
                {
                    var contents=PrefabUtility.LoadPrefabContents(prefabPath);
                    try
                    {
                        var existingSource=contents.GetComponent<SparkLevel94Process>().certification;
                        ConfigureCertification(existingSource);
                        ConfigurePads(existingSource);
                        PrefabUtility.SaveAsPrefabAsset(contents,prefabPath);
                    }
                    finally { PrefabUtility.UnloadPrefabContents(contents); }
                    existing=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                }
                CreateDefinition();
                Register(existing.GetComponent<SparkLevelLayout>());
                return;
            }
            var root=new GameObject("Level_94_Layout");
            try
            {
                var arena=Child(root.transform,"Board Foundry Arena",Vector3.zero);
                var art=Child(arena,"L94_ART",Vector3.zero);
                var modules=new Dictionary<string,GameObject>();
                foreach(var path in Directory.GetFiles(Kit+"/Models","*.fbx").OrderBy(p=>p,StringComparer.Ordinal))
                {
                    var source=AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace('\\','/'));
                    if(!source || source.GetComponentsInChildren<Renderer>().Length==0)
                        throw new InvalidOperationException("FBX lacks imported geometry: "+path);
                    var module=(GameObject)PrefabUtility.InstantiatePrefab(source,art);
                    module.name=Path.GetFileNameWithoutExtension(path).Substring(3);
                    module.transform.SetLocalPositionAndRotation(Vector3.zero,Quaternion.identity);
                    module.transform.localScale=Vector3.one;
                    modules.Add(module.name,module);
                }
                if(modules.Count!=48)throw new InvalidOperationException("Expected 48 modular models.");
                SparkLevel94PBRMaterialEditor.ApplyMaterials(art.gameObject);
                var baseModule=modules["L94_AssemblyBenchFrame"].transform;
                Vector3 Marker(string name)=>arena.InverseTransformPoint(baseModule.Find(name).position);
                var origin=Marker("BOARD_ORIGIN");
                var vx=Marker("BOARD_X")-origin;
                var vy=Marker("BOARD_Y")-origin;
                var vz=Marker("BOARD_Z")-origin;
                Vector3 Position(float x,float y,float z)=>origin+x*vx+y*vy+z*vz;

                var layout=root.AddComponent<SparkLevelLayout>();
                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                var definition=CreateDefinition();
                Set(layout,("levelDefinition",definition),("sceneBindings",bindings));
                var process=root.AddComponent<SparkLevel94Process>();
                process.layout=layout; process.arena=arena; process.board=arena;
                var fx=root.AddComponent<SparkLevel94Effects>();
                fx.particlesMaterial=MaterialAsset(Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit"),"Particles",Color.white);
                process.effects=fx;
                var topology=Child(arena,"Board topology",Vector3.zero);
                var carrier=Child(topology,"PCB pad carrier",Vector3.zero).gameObject.AddComponent<SparkResistor>();
                Set(carrier,("electricalEnabled",false));
                var shader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                var padMaterial=MaterialAsset(shader,"Pad rim",new Color(.9f,.61f,.20f));
                var copper=MaterialAsset(shader,"Top copper",new Color(.91f,.41f,.14f));
                var buriedCopper=MaterialAsset(shader,"Back copper",new Color(.13f,.72f,.86f));
                var pinList=new List<SparkTerminal>();
                var pins=new Dictionary<string,SparkTerminal>();
                SparkTerminal MakePin(string id,Vector3 position,bool visible=true)
                {
                    var pin=Pin(carrier,id,0,0,padMaterial,pinList);
                    var tr=pin.transform; tr.localPosition=position; tr.localScale=new Vector3(.013f,.0024f,.012f);
                    if(!visible){pin.GetComponent<Collider>().enabled=false;pin.GetComponent<Renderer>().enabled=false;}
                    Set(pin,("providesOutput",true),("acceptsInput",true),("maxConnections",32));
                    pins.Add(id,pin);
                    return pin;
                }

                foreach(var row in Rows("Logical_Pad_Locations"))
                    MakePin(row[0],Position(Parse(row[7]),Parse(row[8])+.012f,Parse(row[9])));
                if(pins.Count!=127)throw new InvalidOperationException("Expected 127 PCB pad records.");
                SparkTerminal P(string id)=>pins[id=="GND"?"J1.GND":id=="+3V3"?"U2.VOUT":id];
                foreach(var row in Rows("Factory_Existing_Nets")) Tap(topology,P(row[0]),P(row[1]));
                var positive=MakePin("SUPPLY_POS",Vector3.zero,false);
                var ground=MakePin("SUPPLY_RET",Vector3.zero,false);
                var cert=Supply(topology,"CONTROL_POWER",5,.05f,positive,ground,true);
                ConfigureCertification(cert);
                ConfigurePads(cert);
                process.certification=cert;
                Tap(topology,P("J1.GND"),ground);
                process.supply=Supply(topology,"Protected input",5,.11f,P("J1.5V_IN"),ground);
                process.regulator=Supply(topology,"Educational U2 regulator",3.3f,.09f,P("U2.VOUT"),ground);
                process.feedback=Supply(topology,"U4 feedback carrier",3.3f,.01f,P("U4.CURRENT_OK_OUT"),ground);
                process.supplyReturn=ground;
                process.protectedInput=P("U2.VIN"); process.rail=P("U1.VDD");
                process.feedbackInput=P("U1.CURRENT_OK_IN"); process.shunt=P("R8.B");
                Resist(topology,"U2 input impedance",P("U2.VIN"),ground,3300);
                Resist(topology,"U1 digital load",P("U1.VDD"),ground,1000);
                Resist(topology,"Feedback receiver",P("U1.CURRENT_OK_IN"),ground,4700);
                var diode=Child(topology,"D1 input protection",Vector3.zero).gameObject.AddComponent<SparkDiode>();
                Set(diode,("anodeTerminal",P("D1.A")),("cathodeTerminal",P("D1.K")),("forwardVoltage",.28f));
                var resistance=new[]{4700f,4700,100,100000,1000,330,330,.1f};
                for(int i=0;i<resistance.Length;i++)
                {
                    string id=$"R{i+1}";
                    var component=Resist(topology,id,P(id+".A"),P(id+".B"),resistance[i]);
                    if(i==1)process.pullup=component;
                }
                for(int i=1;i<=3;i++)
                {
                    string id=$"LED{i}";
                    var led=Child(topology,id,Vector3.zero).gameObject.AddComponent<SparkLED>();
                    Set(led,("anodeTerminal",P(id+".A")),("cathodeTerminal",P(id+".C")),("forwardVoltage",i==2?2.75f:1.8f));
                }
                var capacitances=new[]{10e-6f,22e-6f,100e-9f,100e-9f,100e-9f,10e-9f,1e-6f};
                for(int i=0;i<capacitances.Length;i++)
                {
                    string id=$"C{i+1}";
                    var cap=Child(topology,id,Vector3.zero).gameObject.AddComponent<SparkCapacitor>();
                    Set(cap,("positiveTerminal",P(id+".POS")),("negativeTerminal",P(id+".NEG")),("capacitanceFarads",capacitances[i]));
                }
                process.loadGate=Tap(topology,P("Q1.D"),P("Q1.S"),false);
                process.bridge=Tap(topology,P("Q1.G"),P("Q1.D"),false);

                var viaRows=Rows("Required_Via_Placements_V01_V06").ToDictionary(row=>row[0]);
                var routes=new List<SparkLevel94Process.Route>();
                var traces=Child(arena,"Player copper",Vector3.zero);
                foreach(var row in Rows("Copper_Trace_Tasks_W001_W034"))
                {
                    var route=new SparkLevel94Process.Route {from=P(row[1]),to=P(row[2]),net=row[3],viaName=row[6]};
                    if(viaRows.TryGetValue(row[6],out var spec))
                    {
                        float x=Parse(spec[2])*.014f,z=Parse(spec[3])*.014f;
                        route.viaTop=MakePin(spec[0]+".Top",Position(x,1.29f,z));
                        route.viaBottom=MakePin(spec[0]+".Bottom",Position(x,1.276f,z),false);
                        route.via=Tap(topology,route.viaTop,route.viaBottom,false);
                        route.copper=Copper(traces,row[0]+" top",route.from,route.viaTop,copper);
                        route.backCopper=Copper(traces,row[0]+" back",route.viaBottom,route.to,buriedCopper);
                    }
                    else route.copper=Copper(traces,row[0],route.from,route.to,copper);
                    routes.Add(route);
                }
                routes.Add(new SparkLevel94Process.Route {from=P("J3.A"),to=P("J3.B"),net="Trusted load feedback",
                    viaName="",copper=Copper(traces,"W035",P("J3.A"),P("J3.B"),buriedCopper)});
                process.routes=routes.ToArray();
                process.jumperArt=modules["L94_W035DetachableFeedbackJumper"];
                process.jumperArt.SetActive(false);
                var components=Rows("Component_Placement_27");
                process.parts=components.Select(row=>modules[row[7]]).ToArray();
                process.references=components.Select(row=>row[0]).ToArray();
                process.values=components.Select(row=>row[6]).ToArray();
                BuildControls(root.transform,process,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
                PreparePrefabPresentation(process, ReadWorktopOpening());
                WriteBindings(bindings,pinList.Where(p=>p!=positive && p!=ground).ToList(),cert);
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,prefabPath);
                if(!prefab)throw new InvalidOperationException("Level 94 could not save its prefab.");
                AssetDatabase.SaveAssets();
                Register(prefab.GetComponent<SparkLevelLayout>());
            }
            finally{Object.DestroyImmediate(root);}
        }

        private static string[][] Rows(string name)=>File.ReadAllLines(Kit+"/Data/"+name+".csv").Skip(1)
            .Where(line=>!string.IsNullOrWhiteSpace(line)).Select(line=>line.Split(',')).ToArray();
        private static float Parse(string value)=>float.Parse(value,CultureInfo.InvariantCulture);

        private static SparkPowerSupply Supply(Transform parent,string id,float voltage,float current,SparkTerminal a,SparkTerminal b,bool active=false)
        {
            var supply=Child(parent,id,Vector3.zero).gameObject.AddComponent<SparkPowerSupply>();
            Set(supply,("objectId",id.Replace(' ','_')),("displayName",id),("positiveTerminal",a),("negativeTerminal",b),
                ("outputVoltage",voltage),("currentLimit",current),("state",active?SparkPowerSupplyState.Active:SparkPowerSupplyState.Off));
            return supply;
        }

        private static void ConfigurePads(SparkPowerSupply source)
        {
            var carrier=source.transform.parent.Find("PCB pad carrier");
            Set(carrier.GetComponent<SparkResistor>(),("electricalEnabled",true),("terminalA",source.PositiveTerminal),
                ("terminalB",source.NegativeTerminal),("resistance",1000000f));
            var host=carrier.Find("Passive terminal owner");
            if(!host) host=Child(carrier,"Passive terminal owner",Vector3.zero);
            var owner=host.GetComponent<SparkElectronicObject>() ?? host.gameObject.AddComponent<SparkElectronicObject>();
            foreach(var pin in carrier.GetComponentsInChildren<SparkTerminal>(true))
                if(pin!=source.PositiveTerminal && pin!=source.NegativeTerminal)
                    Set(pin,("owner",owner));
        }

        private static void ConfigureCertification(SparkPowerSupply source)
        {
            Set(source.PositiveTerminal,("owner",source));
            Set(source.NegativeTerminal,("owner",source));
            if(!source.transform.Find("Certification reference load"))
                Resist(source.transform,"Certification reference load",source.PositiveTerminal,source.NegativeTerminal,10000);
        }

        private static SparkResistor Resist(Transform parent,string id,SparkTerminal a,SparkTerminal b,float ohms)
        {
            var resistor=Child(parent,id,Vector3.zero).gameObject.AddComponent<SparkResistor>();
            Set(resistor,("terminalA",a),("terminalB",b),("resistance",ohms));
            return resistor;
        }

        private static SparkSwitch Tap(Transform parent,SparkTerminal a,SparkTerminal b,bool closed=true)
        {
            var tap=Child(parent,a.name+" to "+b.name,Vector3.zero).gameObject.AddComponent<SparkSwitch>();
            Set(tap,("inputTerminal",a),("outputTerminal",b),("state",closed?SparkSwitchState.Closed:SparkSwitchState.Open));
            return tap;
        }

        private static LineRenderer Copper(Transform parent,string id,SparkTerminal from,SparkTerminal to,Material material)
        {
            var line=Child(parent,id,Vector3.zero).gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace=false; line.positionCount=3; line.widthMultiplier=.005f;
            var a=parent.InverseTransformPoint(from.transform.position)+Vector3.up*.003f;
            var b=parent.InverseTransformPoint(to.transform.position)+Vector3.up*.003f;
            line.SetPositions(new[]{a,new Vector3(b.x,(a.y+b.y)*.5f,a.z),b});
            line.sharedMaterial=material; line.numCornerVertices=1; line.enabled=false;
            return line;
        }

        private static void BuildControls(Transform parent,SparkLevel94Process c,Font font)
        {
            var obj=new GameObject("Board Foundry controls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.transform.SetParent(parent,false);
            var canvas=obj.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=32;
            var scaler=obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            var panel=Rect(canvas.transform,"Control deck",new Vector2(1148,212),new Vector2(-165,10));
            var back=panel.gameObject.AddComponent<Image>(); back.color=new Color(.018f,.061f,.098f,.97f); back.raycastTarget=false;
            c.heading=UIText(panel,"Heading","THE BOARD FOUNDRY",font,new Vector2(1130,26),new Vector2(0,182));
            c.readings=UIText(panel,"Live readings","",font,new Vector2(1130,24),new Vector2(0,156));
            c.readings.color=new Color(.27f,.91f,.96f);
            c.detail=UIText(panel,"Context","",font,new Vector2(1130,29),new Vector2(0,125)); c.detail.fontSize=15;
            (string,UnityAction)[] actions={
                ("INSPECT NET",c.InspectNet),("NEXT PART",c.NextPart),("ROTATE 90°",c.RotatePart),("FIT PART",c.FitPart),
                ("PREV ROUTE",c.PreviousRoute),("NEXT ROUTE",c.NextRoute),("PLATE VIA",c.FitVia),("FRONT COPPER",c.FrontCopper),
                ("UNDO COPPER",c.UndoCopper),("PROTECTED 5 V",c.TogglePower),("QUALIFY",c.TestBoard),("MEASURE",c.Measure),
                ("VIA FAULT",c.InjectVia),("PULL-UP FAULT",c.InjectPullup),("BRIDGE FAULT",c.InjectBridge),("REPAIR",c.Repair),
                ("ARM W035",c.ArmJumper),("RESET SAFETY",c.ResetSafety),("BOARD VIEW",c.BoardView),("PAD VIEW",c.PadView),("SHOWCASE",c.Showcase)
            };
            for(int i=0;i<actions.Length;i++)
                Control(panel,font,actions[i].Item1,-486+162*(i%7),85-39*(i/7),actions[i].Item2).interactable=true;
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
                    Debug.LogWarning("Level 94 prefab is ready. Save your MainMenu changes and rerun Create Level 94 to register it.");
                    return;
                }
                T Find<T>() where T : Component => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)).Single();
                var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_94.asset");
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
                Debug.Log("Level 94 registered in MainMenu.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene,true); }
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var existing=AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder+"/Level_94.asset");
            var level=existing ? existing : ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_94"),("displayName","The Board Foundry"),
                ("description","Build and qualify a two-layer PCB, repair three faults, and recover the removable W035 jumper."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level94_pcb_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue=ObjectiveDetails[i];
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel94Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.ComponentPowered;
                target.FindPropertyRelative("minimumVoltage").floatValue=0f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=false;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            if(!existing) AssetDatabase.CreateAsset(level,Folder+"/Level_94.asset");
            else AssetDatabase.SaveAssetIfDirty(level);
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
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel94Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel94Process.Gates[i];
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
            string path=$"{Folder}/L101 {name}.mat";
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












