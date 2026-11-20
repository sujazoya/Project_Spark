#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ProjectSpark.Gameplay;
using ProjectSpark.PCBVirtualStudio;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    [InitializeOnLoad]
    public static class SparkLevel103Builder
    {
        internal const string Kit="Assets/ProjectSpark/Level103";
        const string Folder=Kit+"/Prefabs";
        const string ScenePath="Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        const string Request="Logs/SparkLevel103.request",Report="Logs/SparkLevel103.txt";
        static readonly string[] Objectives={
            "Install and solder all 44 Sensor Matrix components","Route 58 copper tasks, 12 vias and trusted W059",
            "Verify reference and calibrate all four channels","Qualify four-channel acquisition at 128 samples/s/channel",
            "Diagnose and recover all three sensor-chain faults","Recover W059 loss with fresh calibration and qualification"
        };
        static SparkLevel103Builder()=>EditorApplication.update+=Poll;
        static void Poll()
        {
            if(!File.Exists(Request)||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            File.Delete(Request);
            try{Create();File.WriteAllText(Report,"BUILT Level 103 / Sensor Matrix. Educational SMX4-EDU-1 schematic; 62 modular assets, 44 parts, 187 pads, 58 routes, 12 vias and W059.\n");}
            catch(Exception e){File.WriteAllText(Report,"BUILD FAILED\n"+e);Debug.LogException(e);}
        }

        [MenuItem("Tools/Project Spark/Levels/Create Level 103")]
        public static void Create()=>Build(true);
        internal static void Build(bool registerCampaign)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            var design=SparkLevel103Schematic.Build();
            if(!design.IsResolved(out var problem))throw new InvalidOperationException(problem);
            SparkLevel103Materials.Build();
            if(!AssetDatabase.IsValidFolder(Folder+"/Modules"))AssetDatabase.CreateFolder(Folder,"Modules");
            var root=new GameObject("Level_103_Layout");
            try
            {
                var arena=Child(root.transform,"L103_BoardPhysicalRoot",Vector3.zero);
                var art=Child(arena,"L103_BoardArtRoot",new Vector3(0,1.3f,0));
                art.localScale=Vector3.one*40f;
                var manifest=File.ReadAllLines(Kit+"/Data/Asset_Manifest_62.csv").Skip(1)
                    .Where(line=>!string.IsNullOrWhiteSpace(line)).Select(line=>line.Split(',')).ToArray();
                var modules=new Dictionary<string,GameObject>();
                foreach(var row in manifest)
                {
                    string path=Kit+"/Models_Rebuilt/"+Path.GetFileName(row[2]);
                    var importer=AssetImporter.GetAtPath(path) as ModelImporter;
                    if(importer && Mathf.Abs(importer.globalScale-100f)>.01f){importer.globalScale=100f;importer.SaveAndReimport();}
                    var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    File.AppendAllText("Logs/Level103_Model_Transforms.txt",path+" rotation="+model.transform.localEulerAngles+" scale="+model.transform.localScale+" bounds="+Measure(model)+"\\n");
                                        if(!model || model.GetComponentsInChildren<Renderer>(true).Length==0)
                        throw new InvalidOperationException("Missing FBX geometry "+path+"; main asset "+
                            AssetDatabase.GetMainAssetTypeAtPath(path)+"; subassets "+
                            string.Join(",",AssetDatabase.LoadAllAssetsAtPath(path).Select(asset=>asset.GetType().Name)));
                    var module=(GameObject)PrefabUtility.InstantiatePrefab(model,art);
                    module.name=row[1];module.transform.localPosition=Vector3.zero;
                    module.transform.localRotation=Quaternion.Euler(0,180,0)*model.transform.localRotation;module.transform.localScale=Vector3.one;
                    foreach(var mesh in module.GetComponentsInChildren<Renderer>(true))mesh.gameObject.SetActive(true);
                    module.SetActive(true);
                    modules.Add(row[1],module);
                    SparkLevel103Materials.Apply(module);
                    
                    if(int.Parse(row[0]) is >=48 and <=53)module.SetActive(false);
                }
                foreach(var part in design.parts)modules[part.model].transform.localPosition=new Vector3(part.xMm*.001f,0,part.zMm*.001f);
                // Optional detail modules are local-space accessories, not another assembled board.
                foreach(var pair in modules)
                    if(pair.Key.Contains("TestPads")||pair.Key.Contains("Guard_Ring")||pair.Key.Contains("Land_Pattern")||pair.Key.Contains("Connector_Contacts")||pair.Key.Contains("Probe_Housing")||pair.Key.Contains("Lens_Cover")||pair.Key.Contains("CalibrationPads")||pair.Key.Contains("Identification_Plate"))pair.Value.SetActive(false);
                var header=modules.FirstOrDefault(pair=>pair.Key.Contains("HeaderBase")).Value;
                if(header){var patch=design.parts.Single(part=>part.reference=="W059");header.transform.localPosition=new Vector3(patch.xMm*.001f,0,patch.zMm*.001f);}
                SparkLevel103ContactGeometry.Align(design,modules,art);
                AssetDatabase.StartAssetEditing();
                try{foreach(var pair in modules)PrefabUtility.SaveAsPrefabAsset(pair.Value,Folder+"/Modules/"+pair.Key+".prefab");}
                finally{AssetDatabase.StopAssetEditing();}
                var taskLight=Child(arena,"Board inspection key light",new Vector3(0,7,-2)).gameObject.AddComponent<Light>();
                taskLight.type=LightType.Spot;taskLight.range=25;taskLight.spotAngle=115;taskLight.intensity=18;
                taskLight.color=new Color(.88f,.95f,1);taskLight.shadows=LightShadows.Soft;taskLight.transform.localRotation=Quaternion.Euler(75,0,0);
                var board=modules["L96_SMX4_PCB_140x95mm_Empty"];
                var baseBounds=Measure(board);
                float boardWidth=baseBounds.size.x/art.lossyScale.x;
                if(Mathf.Abs(boardWidth-.14f)>.003f)
                    throw new InvalidOperationException("Source FBX physical width "+boardWidth+" m differs from 0.140 m. Check model import units.");
                var layout=root.AddComponent<SparkLevelLayout>();
                var bindings=root.AddComponent<SparkLevelSceneBindings>();
                Set(layout,("levelDefinition",CreateDefinition()),("sceneBindings",bindings));
                var process=root.AddComponent<SparkLevel103Process>();
                process.design=design;process.layout=layout;process.arena=arena;process.art=art;
                process.effects=root.AddComponent<SparkLevel103Effects>();
                var particles=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                string particlePath=Kit+"/Materials/Contact particles.mat";
                var particleAsset=AssetDatabase.LoadAssetAtPath<Material>(particlePath);
                if(!particleAsset){AssetDatabase.CreateAsset(particles,particlePath);particleAsset=particles;}
                else Object.DestroyImmediate(particles);
                process.effects.particlesMaterial=particleAsset;
                var topology=Child(arena,"L103_SolverTopology",Vector3.zero);
                var slots=Child(arena,"L103_ComponentSlots",Vector3.zero);
                var pads=Child(arena,"L103_PhysicalPads",Vector3.zero);
                var viaRoot=Child(arena,"L103_ViaSockets",Vector3.zero);
                var copperRoot=Child(arena,"L103_PlayerCopperF_Cu",Vector3.zero);
                var reverseCopper=Child(arena,"L103_PlayerCopperB_Cu",Vector3.zero);
                Child(arena,"L103_RepairTargets",Vector3.zero);
                Child(arena,"L103_InstrumentationAnchors",Vector3.zero);
                var owner=Child(pads,"Passive PCB pad owner",Vector3.zero).gameObject.AddComponent<SparkElectronicObject>();
                var gold=SparkLevel103Materials.Surface("ENIG contacts",new Color(.84f,.57f,.16f));
                var topMaterial=SparkLevel103Materials.Surface("Front copper",new Color(.79f,.35f,.12f));
                var bottomMaterial=SparkLevel103Materials.Surface("Back copper",new Color(.14f,.66f,.72f));
                var pins=new Dictionary<string,SparkTerminal>();
                var internals=new Dictionary<string,SparkTerminal>();
                var pinList=new List<SparkTerminal>();
                Vector3 Position(Vector3 mm)=>new(mm.x*.04f,1.3f+mm.y*.04f,mm.z*.04f);
                SparkTerminal Pin(string name,Vector3 position,bool visible)
                {
                    var node=visible?GameObject.CreatePrimitive(PrimitiveType.Cylinder):new GameObject(name);
                    node.name=name;node.transform.SetParent(pads,false);node.transform.localPosition=position;
                    if(visible)
                    {
                        node.transform.localScale=new Vector3(.025f,.003f,.025f);
                        node.GetComponent<Renderer>().sharedMaterial=gold;
                        int layer=LayerMask.NameToLayer("Pin");if(layer>=0)node.layer=layer;
                    }
                    var terminal=node.AddComponent<SparkTerminal>();
                    Set(terminal,("owner",owner),("providesOutput",true),("acceptsInput",true),("maxConnections",32));
                    pinList.Add(terminal);return terminal;
                }
                foreach(var contact in design.contacts)
                {
                    pins.Add(contact.id,Pin(contact.id,Position(contact.positionMm),true));
                    internals.Add(contact.id,Pin(contact.id+"#internal",Position(contact.positionMm),false));
                }
                SparkTerminal Pad(string id)=>pins[id];
                SparkTerminal Internal(string id)=>internals[id];
                foreach(var branch in design.factory)Tap(topology,Pad(branch.from),Pad(branch.to));
                process.parts=design.parts.Select(part=>
                {
                    var targetObject=Child(slots,part.reference,Position(new Vector3(part.xMm,2.5f,part.zMm))).gameObject;
                    targetObject.layer=LayerMask.NameToLayer("UI");
                    var collider=targetObject.AddComponent<BoxCollider>();
                    collider.size=new Vector3(Mathf.Max(.06f,part.widthMm*.04f),.07f,Mathf.Max(.06f,part.depthMm*.04f));
                    var target=targetObject.AddComponent<SparkVirtualPCBTarget>();
                    target.Configure(part.reference,part.reference+" "+part.value,"SMX4-EDU-1",SparkPCBTargetKind.IC,
                        SparkBoardDefect.None,0,25,0,null,null,null,null);
                    var bridge=targetObject.AddComponent<SparkPCBPhysicalClickBridge>();
                    var contacts=design.contacts.Where(pin=>pin.reference==part.reference).ToArray();
                    var record=new SparkLevel103Process.Part {art=modules[part.model],target=target,
                        joints=contacts.Select(pin=>Tap(topology,Pad(pin.id),Internal(pin.id),false)).ToArray(),
                        devices=Array.Empty<SparkElectricalComponent>()};
                    return record;
                }).ToArray();
                var devices=design.parts.ToDictionary(part=>part.reference,_=>new List<SparkElectricalComponent>());
                T Device<T>(string reference,string label) where T:SparkElectricalComponent
                {
                    var device=Child(topology,label,Vector3.zero).gameObject.AddComponent<T>();
                    devices[reference].Add(device);return device;
                }
                SparkPowerSupply Supply(string part,string label,SparkTerminal a,SparkTerminal b,float voltage,bool active=false)
                {
                    var supply=Device<SparkPowerSupply>(part,label);
                    Set(supply,("positiveTerminal",a),("negativeTerminal",b),("outputVoltage",voltage),
                        ("currentLimit",.5f),("state",active?SparkPowerSupplyState.Active:SparkPowerSupplyState.Off));
                    return supply;
                }
                SparkResistor Resistor(string part,string label,SparkTerminal a,SparkTerminal b,float resistance)
                {
                    var resistor=Device<SparkResistor>(part,label);
                    Set(resistor,("terminalA",a),("terminalB",b),("resistance",Mathf.Max(.00001f,resistance)),
                        ("maximumPowerWatts",2f));
                    return resistor;
                }
                process.ground=Internal("J1.P02");
                var certPlus=Pin("SUPPLY_POS",Vector3.zero,false);
                var certMinus=Pin("SUPPLY_RET",Vector3.zero,false);
                process.certification=Supply("J1","CONTROL_POWER",certPlus,certMinus,5,true);
                devices["J1"].Remove(process.certification);
                var carrier=pads.gameObject.AddComponent<SparkResistor>();
                Set(carrier,("terminalA",certPlus),("terminalB",certMinus),("resistance",100000000f));
                Set(certPlus,("owner",process.certification));Set(certMinus,("owner",process.certification));
                var referenceLoad=Child(topology,"Certification reference load",Vector3.zero).gameObject.AddComponent<SparkResistor>();
                Set(referenceLoad,("terminalA",certPlus),("terminalB",certMinus),("resistance",10000f));
                process.supply=Supply("J1","Protected 5 V supply",Internal("J1.P01"),process.ground,5);
                process.logic=Supply("U7","Acquisition MCU integrated 3V3 LDO",Internal("U7.P03"),Internal("U7.P02"),3.3f);
                process.reference=Supply("U6","Precision 2V500 reference",Internal("U6.P03"),Internal("U6.P02"),2.5f);
                process.supervisor=Supply("U8","Reference-valid digital output",Internal("U8.P04"),Internal("U8.P02"),3.3f);
                process.safetyLamp=Supply("U9","Certified acquisition status",Internal("U9.P04"),Internal("U9.P02"),3.3f);
                process.protectedInput=Internal("U7.P01");process.logicInput=Internal("U1.P01");
                process.referenceInput=Internal("U1.P03");process.supervisorInput=Internal("U8.P03");process.trustedInput=Internal("U9.P05");
                process.excitations=new SparkPowerSupply[4];process.sensorOutputs=new SparkPowerSupply[4];
                process.frontEnds=new SparkPowerSupply[4];process.sensorInputs=new SparkTerminal[4];
                process.minusInputs=new SparkTerminal[4];process.adcInputs=new SparkTerminal[4];
                process.componentReturns=new SparkTerminal[4];
                for(int channel=0;channel<4;channel++)
                {
                    string amplifier="U"+(channel+2),sensor="S"+(channel+1);
                    process.excitations[channel]=Supply(amplifier,"CH"+channel+" excitation",Internal(amplifier+".P03"),Internal(amplifier+".P02"),2.5f);
                    process.sensorOutputs[channel]=Supply(sensor,"CH"+channel+" sensor equivalent",Internal(sensor+".P02"),Internal(sensor+(channel==1?".P04":".P03")),.5f);
                    process.frontEnds[channel]=Supply(amplifier,"CH"+channel+" analog front end",Internal(amplifier+".P05"),Internal(amplifier+".P02"),.2f);
                    process.sensorInputs[channel]=Internal(amplifier+".P04");
                    process.minusInputs[channel]=channel==1?Internal("U3.P06"):process.ground;
                    process.adcInputs[channel]=Internal("U1.P"+(channel+4).ToString("00"));
                    process.componentReturns[channel]=Internal(amplifier+".P02");
                    Resistor("U1","ADC channel "+channel+" input impedance",process.adcInputs[channel],Internal("U1.P02"),100000000f);
                }
                // The bridge module has a separate, ratiometric common-mode output.
                process.bridgeCommon=Supply("S2","Bridge common mode",Internal("S2.P03"),Internal("S2.P04"),1.25f);
                Tap(topology,Internal("S2.P03"),Internal("U3.P07"));
                // This factory connection terminates at an internal model node, never bypassing V02.
                var diode=Device<SparkDiode>("D1","D1 protected input diode");
                Set(diode,("anodeTerminal",Internal("D1.P01")),("cathodeTerminal",Internal("D1.P02")),("forwardVoltage",.7f));
                var led=Device<SparkLED>("LED1","LED1 acquisition trusted indicator");
                Set(led,("anodeTerminal",Internal("LED1.P01")),("cathodeTerminal",Internal("LED1.P02")),("forwardVoltage",2.1f));
                float[] resistances={1000,10000,100,4700,2200,.00001f,47000,1000,10000,100,4700,2200,.00001f,47000};
                for(int i=0;i<14;i++)
                {
                    string part="R"+(i+1);
                    var resistor=Resistor(part,part,Internal(part+".P01"),Internal(part+".P02"),resistances[i]);
                    if(i==6)process.opticalFeedback=resistor;
                }
                Resistor("RV1","RV1 upper",Internal("RV1.P01"),Internal("RV1.P02"),4700);
                Resistor("RV1","RV1 lower",Internal("RV1.P02"),Internal("RV1.P03"),4700);
                float[] capacitances={100e-9f,1e-6f,10e-9f,4.7e-6f,10e-6f,100e-9f,1e-6f,10e-9f,4.7e-6f,10e-6f,100e-9f};
                for(int i=0;i<11;i++)
                {
                    string part="C"+(i+1);var capacitor=Device<SparkCapacitor>(part,part);
                    Set(capacitor,("positiveTerminal",Internal(part+".P01")),("negativeTerminal",Internal(part+".P02")),
                        ("capacitanceFarads",capacitances[i]));
                }
                for(int i=0;i<design.parts.Length;i++)process.parts[i].devices=devices[design.parts[i].reference].ToArray();
                var routeRecords=new List<SparkLevel103Process.Route>();
                foreach(var spec in design.routes)
                {
                    var route=new SparkLevel103Process.Route {from=Pad(spec.from),to=Pad(spec.to)};
                    if(!string.IsNullOrEmpty(spec.via))
                    {
                        Vector3 midpoint=(route.from.transform.localPosition+route.to.transform.localPosition)*.5f;
                        route.viaTop=Pin(spec.via+".Top",midpoint+Vector3.up*.012f,true);
                        route.viaTop.transform.SetParent(viaRoot,true);
                        route.viaBottom=Pin(spec.via+".Bottom",new Vector3(midpoint.x,1.215f,midpoint.z),false);
                        var bottomHit=route.viaBottom.gameObject.AddComponent<SphereCollider>();bottomHit.radius=.018f;
                        route.via=Tap(topology,route.viaTop,route.viaBottom,false);
                        route.top=Copper(copperRoot,spec.id,route.from,route.viaTop,topMaterial);
                        route.bottom=Copper(reverseCopper,spec.id,route.viaBottom,route.to,bottomMaterial);
                    }
                    else route.top=Copper(copperRoot,spec.id,route.from,route.to,topMaterial);
                    routeRecords.Add(route);
                }
                process.routes=routeRecords.ToArray();
                BuildControls(arena,process);
                foreach(var part in process.parts)
                {
                    part.target.SetWorkstation(process.workstation);
                    part.target.GetComponent<SparkPCBPhysicalClickBridge>().Bind(process.workstation,part.target);
                }
                WriteBindings(bindings,pinList.Where(pin=>pin!=certPlus && pin!=certMinus).ToList(),process.certification);
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,Folder+"/Level_103_Layout.prefab");
                AssetDatabase.SaveAssets();
                if(registerCampaign)Register(prefab.GetComponent<SparkLevelLayout>());
            }
            finally{Object.DestroyImmediate(root);}
        }

        static Bounds Measure(GameObject node)
        {
            var renderers=node.GetComponentsInChildren<Renderer>(true);
            var bounds=renderers[0].bounds;
            foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
            return bounds;
        }
        static SparkSwitch Tap(Transform parent,SparkTerminal from,SparkTerminal to,bool closed=true)
        {
            var gate=Child(parent,from.name+" to "+to.name,Vector3.zero).gameObject.AddComponent<SparkSwitch>();
            Set(gate,("inputTerminal",from),("outputTerminal",to),("state",closed?SparkSwitchState.Closed:SparkSwitchState.Open));
            return gate;
        }
        static LineRenderer Copper(Transform parent,string name,SparkTerminal from,SparkTerminal to,Material material)
        {
            var line=Child(parent,name,Vector3.zero).gameObject.AddComponent<LineRenderer>();
            line.sharedMaterial=material;line.useWorldSpace=false;line.positionCount=3;line.widthMultiplier=.009f;
            var a=parent.InverseTransformPoint(from.transform.position);
            var b=parent.InverseTransformPoint(to.transform.position);
            line.SetPositions(new[]{a,new Vector3(b.x,a.y,a.z),b});line.numCornerVertices=2;line.enabled=false;
            return line;
        }

        static void BuildControls(Transform arena,SparkLevel103Process process)
        {
            var node=new GameObject("Sensor Matrix controls",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster));
            node.transform.SetParent(arena,false);
            var canvas=node.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
            var rect=(RectTransform)node.transform;
            rect.sizeDelta=new Vector2(1280,374);rect.localPosition=new Vector3(0,1.65f,-2.7f);
            rect.localRotation=Quaternion.Euler(64,0,0);rect.localScale=Vector3.one*.004f;
            var background=node.AddComponent<Image>();background.color=new Color(.018f,.058f,.083f,.98f);background.raycastTarget=false;
            process.heading=Label(rect,"Heading",new Vector2(0,-12),new Vector2(1250,26),18);
            process.readings=Label(rect,"Readings",new Vector2(0,-44),new Vector2(1250,44),14);
            process.readings.color=new Color(.35f,.92f,1);
            process.detail=Label(rect,"Target and instruction",new Vector2(0,-92),new Vector2(1250,48),16);
            process.workstation=node.AddComponent<SparkVirtualPCBWorkstation>();
            process.workstation.Configure(null,null,null,null,null,null,null,null,null,null,null);
            (string,UnityAction)[] actions={
                ("FIT PART",process.FitPart),("FLUX",process.FluxPart),("SOLDER",process.SolderPart),("NEXT PART",process.NextPart),("ROTATE",process.RotatePart),
                ("PREV ROUTE",process.PreviousRoute),("NEXT ROUTE",process.NextRoute),("PLATE VIA",process.PlateVia),("ROUTE COPPER",process.RouteCopper),("UNDO",process.UndoCopper),
                ("5 V POWER",process.TogglePower),("CHECK REFERENCE",process.CheckReference),("ZERO STANDARD",process.ZeroStandard),("SPAN STANDARD",process.SpanStandard),("CAPTURE",process.Capture),
                ("RUN ACQUISITION",process.TestBoard),("F1 REF DRIFT",process.FaultReference),("F2 VIA OPEN",process.FaultVia),("F3 OPTICAL",process.FaultOptical),("DIAGNOSE",process.Diagnose),
                ("REPAIR",process.Repair),("ARM W059",process.ArmW059),("RESET SAFETY",process.ResetSafety),("BOARD VIEW",process.BoardView),("PAD VIEW",process.PadView),("SAVE BOARD",process.SaveBoard),("RESUME BOARD",process.LoadBoard),("WIRE MODE",process.WireMode),("INSPECT MODE",process.InspectMode)
            };
            for(int i=0;i<actions.Length;i++)
            {
                var buttonNode=new GameObject(actions[i].Item1,typeof(RectTransform),typeof(Image),typeof(Button));
                var buttonRect=(RectTransform)buttonNode.transform;buttonRect.SetParent(rect,false);
                buttonRect.anchorMin=buttonRect.anchorMax=new Vector2(.5f,1);
                buttonRect.pivot=new Vector2(.5f,1);buttonRect.sizeDelta=new Vector2(242,32);
                buttonRect.anchoredPosition=new Vector2(-500+250*(i%5),-145-36*(i/5));
                buttonNode.GetComponent<Image>().color=new Color(.025f,.23f,.31f);
                var button=buttonNode.GetComponent<Button>();
                UnityEventTools.AddPersistentListener(button.onClick,actions[i].Item2);
                var text=Label(buttonRect,"Label",Vector2.zero,new Vector2(240,30),14);text.text=actions[i].Item1;
            }
        }
        static Text Label(RectTransform parent,string name,Vector2 position,Vector2 size,int font)
        {
            var node=new GameObject(name,typeof(RectTransform),typeof(Text));
            var rect=(RectTransform)node.transform;rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);rect.pivot=new Vector2(.5f,1);
            rect.anchoredPosition=position;rect.sizeDelta=size;
            var text=node.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize=font;text.color=Color.white;text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;
            return text;
        }
        static Transform Child(Transform parent,string name,Vector3 position)
        {
            var node=new GameObject(name).transform;node.SetParent(parent,false);node.localPosition=position;return node;
        }
        private static void Register(SparkLevelLayout prefab)
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
            try
            {
                if (scene.isDirty)
                    File.Copy(scene.path,"Logs/Level103_MainMenu_before_registration.unity",true);
                T Find<T>() where T : Component => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)).Single();
                var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_103.asset");
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
                Debug.Log("Level 103 registered in MainMenu.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene,true); }
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var existing=AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder+"/Level_103.asset");
            var level=existing ? existing : ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_103"),("displayName","Sensor Matrix"),
                ("description","Build and commission a four-channel sensor acquisition board, repair its faults and recover W059 trusted reference feedback."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level103_pcb_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel103Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.ComponentPowered;
                target.FindPropertyRelative("minimumVoltage").floatValue=0f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=false;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            if(!existing) AssetDatabase.CreateAsset(level,Folder+"/Level_103.asset");
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
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel103Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel103Process.Gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
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









