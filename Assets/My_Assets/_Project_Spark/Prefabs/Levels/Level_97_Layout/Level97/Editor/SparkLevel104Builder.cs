#if UNITY_EDITOR
using System;using System.Collections.Generic;using System.Globalization;using System.IO;using System.Linq;
using ProjectSpark.Gameplay;using ProjectSpark.PCBVirtualStudio;using UnityEditor;using UnityEditor.Events;using UnityEditor.SceneManagement;
using UnityEngine;using UnityEngine.Events;using UnityEngine.SceneManagement;using UnityEngine.UI;using Object=UnityEngine.Object;
namespace ProjectSpark.EditorTools
{
 [InitializeOnLoad]public static class SparkLevel104Builder
 {
  internal const string Kit="Assets/ProjectSpark/Level104";const string Folder=Kit+"/Prefabs";
  const string ScenePath="Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
  static readonly string[] Objectives={"Install and solder 50 VGR-5 components","Route 66 copper tasks and plate 14 vias","Sequence and measure 5 V / 3V3 / 1V8 rails","Qualify light / nominal / peak loads","Repair R7, V05 and Q1 with fresh retests","Recover W067 trusted power-good loss"};
  static SparkLevel104Builder()=>EditorApplication.update+=Poll;
  static void Poll()
  {
   if(!File.Exists("Logs/SparkLevel104.request")||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
   File.Delete("Logs/SparkLevel104.request");try{Build(true);File.WriteAllText("Logs/SparkLevel104.txt","BUILT Level104 Voltage Guardian VGR5-EDU-1. Gameplay acceptance pending.\n");}catch(Exception e){File.WriteAllText("Logs/SparkLevel104.txt","BUILD FAILED\n"+e);Debug.LogException(e);}
  }
  [MenuItem("Tools/Project Spark/Levels/Create Level 104")]
  public static void Create()=>Build(true);
  internal static void Build(bool register)
  {
   if(EditorApplication.isPlayingOrWillChangePlaymode)return;
   SparkLevel104Materials.Build();
   var root=new GameObject("Level_104_Layout");
   try
   {
    Transform arena=Child(root.transform,"L104_BoardPhysicalRoot",Vector3.zero),art=Child(arena,"L104_BoardArtRoot",new Vector3(0,1.3f,0));art.localScale=Vector3.one*40;
    var modules=new Dictionary<string,GameObject>();
    var rows=File.ReadAllLines(Kit+"/Data/Asset_Manifest_68.csv").Skip(1).Where(l=>l.Length>0).Select(l=>l.Split(',')).ToArray();
    foreach(var row in rows)
    {
     string path=Kit+"/Models_Rebuilt/"+row[1]+".fbx";var importer=AssetImporter.GetAtPath(path) as ModelImporter;
     if(importer&&Mathf.Abs(importer.globalScale-100)>.01f){importer.globalScale=100;importer.SaveAndReimport();}
     var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!model||model.GetComponentsInChildren<Renderer>(true).Length==0)throw new InvalidOperationException("Recovered FBX geometry unavailable: "+path);
     var go=(GameObject)PrefabUtility.InstantiatePrefab(model,art);go.name=row[1];
     go.transform.localRotation=Quaternion.Euler(0,180,0)*model.transform.localRotation;go.transform.localScale=Vector3.one;
     go.transform.localPosition=new Vector3(F(row[4]),int.Parse(row[0]) is >=1 and <=50?.00092f:0,F(row[6]));
     SparkLevel104Materials.Apply(go);modules.Add(row[1],go);
     if(int.Parse(row[0])>=51)go.SetActive(false);
    }
    var board=modules["00_L97_VGR5_Empty_PCB_160x110mm"];var bounds=Measure(board);
    if(Mathf.Abs(bounds.size.x/40-.160f)>.002f)throw new InvalidOperationException("Recovered board width is "+bounds.size.x/40+" m; expected .160 m.");
    var design=SparkLevel104Schematic.Build(Kit,modules,art); SparkLevel104ERC.Validate(design,Kit); SparkLevel104ArtOptimization.Combine(Kit,modules);
    foreach(var part in design.parts){var label=Child(modules[part.model].transform,part.reference+" label",new Vector3(0,.0008f,.5f*part.depthMm*.001f+.0008f));label.position=art.TransformPoint(new Vector3(part.xMm*.001f,.004f,part.zMm*.001f+.5f*part.depthMm*.001f+.0008f));label.rotation=art.rotation*Quaternion.Euler(90,0,0);label.localScale=Vector3.one*.2f;var text=label.gameObject.AddComponent<TextMesh>();text.text=part.reference;text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=48;text.characterSize=.008f;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(.8f,1,.94f);label.GetComponent<MeshRenderer>().sharedMaterial=text.font.material;} var layout=root.AddComponent<SparkLevelLayout>();var bindings=root.AddComponent<SparkLevelSceneBindings>();
    Set(layout,("levelDefinition",CreateDefinition()),("sceneBindings",bindings));
    var p=root.AddComponent<SparkLevel104Process>();p.design=design;p.layout=layout;p.arena=arena;p.art=art;p.effects=root.AddComponent<SparkLevel104Effects>();
    var particle=AssetDatabase.LoadAssetAtPath<Material>(Kit+"/Materials/Contact particles.mat");
    if(!particle){particle=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));AssetDatabase.CreateAsset(particle,Kit+"/Materials/Contact particles.mat");}p.effects.particlesMaterial=particle;
    var topology=Child(arena,"L104_SolverTopology",Vector3.zero);var pads=Child(arena,"L104_PhysicalPads",Vector3.zero);var slots=Child(arena,"L104_ComponentSlots",Vector3.zero);
    var copper=Child(arena,"L104_PlayerCopper",Vector3.zero);var owner=Child(pads,"Passive PCB pad owner",Vector3.zero).gameObject.AddComponent<SparkElectronicObject>();
    var gold=SparkLevel104Materials.Surface("Contacts",new Color(.82f,.6f,.22f));var material=SparkLevel104Materials.Surface("Player copper",new Color(.8f,.38f,.12f));
    var pins=new Dictionary<string,SparkTerminal>();var internals=new Dictionary<string,SparkTerminal>();var allPins=new List<SparkTerminal>();
    Vector3 Position(Vector3 mm)=>new(mm.x*.04f,1.3f+mm.y*.04f,mm.z*.04f);
    SparkTerminal Pin(string name,Vector3 mm,bool visible)
    {
     var node=visible?GameObject.CreatePrimitive(PrimitiveType.Cylinder):new GameObject(name);node.name=name;node.transform.SetParent(pads,false);node.transform.localPosition=Position(mm);
     if(visible){node.transform.localScale=new Vector3(.022f,.002f,.022f);node.GetComponent<Renderer>().sharedMaterial=gold;int layer=LayerMask.NameToLayer("Pin");if(layer>=0)node.layer=layer;}
     var pin=node.AddComponent<SparkTerminal>();Set(pin,("owner",owner),("providesOutput",true),("acceptsInput",true),("maxConnections",64));allPins.Add(pin);return pin;
    }
    foreach(var c in design.contacts){pins.Add(c.id,Pin(c.id,c.positionMm,true));internals.Add(c.id,Pin(c.id+"#internal",c.positionMm,false));}
    SparkTerminal Pad(string id)=>pins[id];SparkTerminal Internal(string id)=>internals[id];
    string P(string part,int i)=>SparkLevel104Schematic.P(part,i);
    foreach(var b in design.factory)Tap(topology,Pad(b.from),Pad(b.to));
    p.parts=design.parts.Select(part=>
    {
     var targetGo=Child(slots,part.reference,Position(new Vector3(part.xMm,2.5f,part.zMm))).gameObject;targetGo.layer=LayerMask.NameToLayer("UI");
     var hit=targetGo.AddComponent<BoxCollider>();hit.size=new Vector3(Mathf.Max(.06f,part.widthMm*.04f),.08f,Mathf.Max(.06f,part.depthMm*.04f));
     var target=targetGo.AddComponent<SparkVirtualPCBTarget>();target.Configure(part.reference,part.reference+" "+part.value,"VGR5-EDU-1",SparkPCBTargetKind.IC,SparkBoardDefect.None,0,25,0,null,null,null,null);
     targetGo.AddComponent<SparkPCBPhysicalClickBridge>();
     var cs=design.contacts.Where(c=>c.reference==part.reference).ToArray();
     return new SparkLevel104Process.Part{art=modules[part.model],target=target,joints=cs.Select(c=>Tap(topology,Pad(c.id),Internal(c.id),false)).ToArray(),devices=Array.Empty<SparkElectricalComponent>()};
    }).ToArray();
    var devices=design.parts.ToDictionary(part=>part.reference,_=>new List<SparkElectricalComponent>());
    T Device<T>(string id,string name)where T:SparkElectricalComponent{var dev=Child(topology,name,Vector3.zero).gameObject.AddComponent<T>();if(id!=null)devices[id].Add(dev);return dev;}
    SparkResistor R(string id,string name,SparkTerminal a,SparkTerminal b,float value)
    {var r=Device<SparkResistor>(id,name);Set(r,("terminalA",a),("terminalB",b),("resistance",Mathf.Max(.00001f,value)),("maximumPowerWatts",100f));return r;}
    SparkPowerSupply S(string id,string name,SparkTerminal a,SparkTerminal b,float volts,bool on=false)
    {var s=Device<SparkPowerSupply>(id,name);Set(s,("positiveTerminal",a),("negativeTerminal",b),("outputVoltage",volts),("currentLimit",3f),("state",on?SparkPowerSupplyState.Active:SparkPowerSupplyState.Off));return s;}
    p.ground=Internal(P("J1",2));
    var certPlus=Pin("SUPPLY_POS",Vector3.zero,false);var certMinus=Pin("SUPPLY_RET",Vector3.zero,false);
    p.certification=S(null,"CONTROL_POWER",certPlus,certMinus,5,true);Set(certPlus,("owner",p.certification));Set(certMinus,("owner",p.certification));
    var carrier=pads.gameObject.AddComponent<SparkResistor>();Set(carrier,("terminalA",certPlus),("terminalB",certMinus),("resistance",100000000f));R(null,"Control reference load",certPlus,certMinus,10000);
    p.supply=S("J1","12 V isolated input",Internal(P("J1",1)),p.ground,12);
    var diode=Device<SparkDiode>("D1","D1 reverse-polarity protection");Set(diode,("anodeTerminal",Internal(P("D1",1))),("cathodeTerminal",Internal(P("D1",2))),("forwardVoltage",.3f));
    var protector=Tap(topology,Internal(P("U4",1)),Internal(P("U4",3)));devices["U4"].Add(protector);
    var shunt=R("R1","R1 current shunt",Internal(P("R1",1)),Internal(P("R1",2)),.1f);
    R("U5","U5 current monitor pass-through",Internal(P("U5",1)),Internal(P("U5",2)),.00001f);
    R("Q1","Q1 input conduction",Internal(P("Q1",2)),Internal(P("Q1",3)),.045f);
    p.input=Internal(P("U1",1));p.inputDraw=R("U1","Input power equivalent",p.input,Internal(P("U1",2)),1000000); p.inputDraw.GetComponent<SparkResistorBurn>().enabled=false;
    p.mainStage=S("U1","Stateful main buck equivalent",Internal(P("U1",3)),Internal(P("U1",2)),.001f);
    p.secondaryStage=S("U2","3V3 finite soft-start equivalent",Internal(P("U2",3)),Internal(P("U2",2)),.001f);
    p.coreStage=S("U3","1V8 finite soft-start equivalent",Internal(P("U3",3)),Internal(P("U3",2)),.001f);
    for(int i=1;i<=3;i++)R("L"+i,"L"+i+" winding resistance",Internal(P("L"+i,1)),Internal(P("L"+i,2)),.035f);
    p.mainRail=Internal(P("U7",3));p.secondaryRail=Internal(P("J2",2));p.coreRail=Internal(P("J2",3));
    p.supervisor=S("U7","Independent rail supervisor PG",Internal(P("U7",4)),Internal(P("U7",2)),3.3f);
    p.trustedInput=Internal(P("U8",3));R("U8","PG input impedance",p.trustedInput,Internal(P("U8",2)),1000000);
    var safeOutput=S("U8","Safety enable presentation",Internal(P("U8",4)),Internal(P("U8",2)),3.3f);
    // SAFE5 load path physically crosses Q4 and the player's W010 route.
    p.safeLoad=Tap(topology,Internal(P("Q4",2)),Internal(P("Q4",3)),false);devices["Q4"].Add(p.safeLoad);
    p.load=R(null,"Virtual qualification load bank",Internal(P("J2",1)),Internal(P("J2",4)),33); Set(p.load,("maximumPowerWatts",10f)); Set(p.load.GetComponent<SparkResistorBurn>(),("ratedWatts",10f),("thermalCapacityJPerC",2f),("coolingWattsPerC",.25f)); R("R13","R13 output bleeder",Internal(P("R13",1)),Internal(P("R13",2)),10000);
    for(int i=2;i<=13;i++)
    {
     if(i==13)continue;string id="R"+i;float value=i==7?52500:i==8?10000:i==9||i==10?1000:10000;
     var r=R(id,id,Internal(P(id,1)),Internal(P(id,2)),value);if(i==7)p.feedbackTop=r;
    }
    for(int i=1;i<=14;i++)
    {
     string id="C"+i;var c=Device<SparkCapacitor>(id,id);Set(c,("positiveTerminal",Internal(P(id,1))),("negativeTerminal",Internal(P(id,2))),("capacitanceFarads",i==1?220e-6f:i==2?470e-6f:10e-6f));
    }
    for(int i=1;i<=2;i++){string id="LED"+i;var led=Device<SparkLED>(id,id);Set(led,("anodeTerminal",Internal(P(id,1))),("cathodeTerminal",Internal(P(id,2))),("forwardVoltage",2.1f));}
    foreach(string id in new[]{"D2","D3"}){var d=Device<SparkDiode>(id,id);Set(d,("anodeTerminal",Internal(P(id,1))),("cathodeTerminal",Internal(P(id,2))),("forwardVoltage",.3f));}
    foreach(string id in new[]{"Q2","Q3"})R(id,id+" on-state equivalent",Internal(P(id,2)),Internal(P(id,3)),.045f);
    for(int i=0;i<p.parts.Length;i++)p.parts[i].devices=devices[design.parts[i].reference].ToArray();
    p.vias=design.vias.Select(v=>
    {
     var top=Pin(v.id+".L1",v.positionMm,true);var pos=v.positionMm;pos.y=-.8f;var bottom=Pin(v.id+".L4",pos,true);
     return new SparkLevel104Process.Via{top=top,bottom=bottom,barrel=Tap(topology,top,bottom,false),art=top.GetComponent<Renderer>()};
    }).ToArray();
    var escapePads=new Dictionary<string,SparkTerminal>(); var escapeReport=new List<string>{"ID,Pad,Net,StartLayer,EndLayer,X_mm,Z_mm,Provenance"}; SparkTerminal Escape(string id,int layer){string key=id+".FactoryL"+layer;if(escapePads.TryGetValue(key,out var existing))return existing;bool through=id.StartsWith("V",StringComparison.Ordinal); int vi=through?Array.FindIndex(design.vias,v=>v.id==id.Substring(0,3)):-1; var c=through?null:design.contacts.Single(c=>c.id==id);string net=through?design.vias[vi].net:c.net;var mm=through?design.vias[vi].positionMm:c.positionMm;mm.y=layer==2?.25f:layer==3?-.25f:-.9f;var terminal=Pin(key,mm,true);var tap=Tap(topology,through?p.vias[vi].top:Pad(id),terminal,!through);if(through)p.vias[vi].layerTaps=p.vias[vi].layerTaps.Concat(new[]{tap}).ToArray();escapePads.Add(key,terminal);escapeReport.Add(key+","+id+","+net+",1,"+layer+","+mm.x.ToString(CultureInfo.InvariantCulture)+","+mm.z.ToString(CultureInfo.InvariantCulture)+",VGR5-EDU authored blind via-in-pad");return terminal;} p.routes=design.routes.Select(spec=>
    {
     var list=new List<SparkLevel104Process.Segment>();var indices=new List<int>();
     void Segment(SparkTerminal a,SparkTerminal b,int layer){list.Add(new(){from=a,to=b,layer=layer,line=Copper(copper,spec.id+".L"+layer,a,b,material)});}
     if(!string.IsNullOrEmpty(spec.viaA))
     {
      int a=Array.FindIndex(design.vias,v=>v.id==spec.viaA),b=Array.FindIndex(design.vias,v=>v.id==spec.viaB);indices.Add(a);indices.Add(b);
      Segment(Pad(spec.from),p.vias[a].top,1);Segment(p.vias[a].bottom,p.vias[b].bottom,4);Segment(p.vias[b].top,Pad(spec.to),1);
     }
     else {int layer=spec.net=="GND"?2:spec.net is "V5" or "V33" or "V18"?3:1;Segment(layer==1?Pad(spec.from):Escape(spec.from,layer),layer==1?Pad(spec.to):Escape(spec.to,layer),layer);}
     return new SparkLevel104Process.Route{segments=list.ToArray(),vias=indices.ToArray()};
    }).ToArray();
    SparkLevel104Routing.Plan(p,Kit,Escape); File.WriteAllLines(Kit+"/Data/Factory_Blind_Vias.csv",escapeReport); BuildControls(arena,p);foreach(var part in p.parts){part.target.SetWorkstation(p.workstation);part.target.GetComponent<SparkPCBPhysicalClickBridge>().Bind(p.workstation,part.target);}
    WriteBindings(bindings,allPins.Where(pin=>pin!=certPlus&&pin!=certMinus).ToList(),p.certification);
    var key=Child(arena,"PCB inspection key",new Vector3(0,7,-2)).gameObject.AddComponent<Light>();key.type=LightType.Spot;key.range=25;key.spotAngle=115;key.intensity=18;key.color=new Color(.88f,.95f,1);key.shadows=LightShadows.Soft;key.transform.localRotation=Quaternion.Euler(75,0,0);
    var prefab=PrefabUtility.SaveAsPrefabAsset(root,Folder+"/Level_104_Layout.prefab");AssetDatabase.SaveAssets();if(register)Register(prefab.GetComponent<SparkLevelLayout>());
   }
   finally{Object.DestroyImmediate(root);}
  }
  static float F(string s)=>float.Parse(s,CultureInfo.InvariantCulture);
  static Bounds Measure(GameObject go){var rs=go.GetComponentsInChildren<Renderer>(true);var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
  static SparkSwitch Tap(Transform parent,SparkTerminal a,SparkTerminal b,bool closed=true)
  {var s=Child(parent,a.name+" to "+b.name,Vector3.zero).gameObject.AddComponent<SparkSwitch>();Set(s,("inputTerminal",a),("outputTerminal",b),("closedResistance",.001f),("state",closed?SparkSwitchState.Closed:SparkSwitchState.Open));return s;}
  static LineRenderer Copper(Transform parent,string id,SparkTerminal a,SparkTerminal b,Material mat)
  {var l=Child(parent,id,Vector3.zero).gameObject.AddComponent<LineRenderer>();l.sharedMaterial=mat;l.useWorldSpace=false;l.positionCount=3;l.widthMultiplier=.008f;var x=parent.InverseTransformPoint(a.transform.position);var y=parent.InverseTransformPoint(b.transform.position);l.SetPositions(new[]{x,new Vector3(y.x,x.y,x.z),y});l.enabled=false;return l;}
        static void BuildControls(Transform arena,SparkLevel104Process process)
        {
            var node=new GameObject("Voltage Guardian controls",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster));
            node.transform.SetParent(arena,false);
            var canvas=node.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
            var rect=(RectTransform)node.transform;
            rect.sizeDelta=new Vector2(1280,410);rect.localPosition=new Vector3(0,1.65f,-2.7f);
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
                ("PREV ROUTE",process.PreviousRoute),("NEXT ROUTE",process.NextRoute),("PLATE VIA",process.PlateVia),("ROUTE SEGMENT",process.RouteCopper),("UNDO / W067",process.UndoCopper),
                ("12 V POWER",process.TogglePower),("ENABLE 5 V",process.EnableMain),("ENABLE 3V3",process.EnableSecondary),("ENABLE 1V8",process.EnableCore),("MEASURE RAILS",process.CheckRails),
                ("TEST 3 LOADS",process.TestBoard),("F1 R7",process.FaultFeedback),("F2 V05",process.FaultVia),("F3 Q1",process.FaultLoss),("DIAGNOSE",process.Diagnose),
                ("REPAIR",process.Repair),("ARM W067",process.ArmW067),("RESET SAFETY",process.ResetSafety),("BOARD VIEW",process.BoardView),("PAD VIEW",process.PadView),
                ("LAYER",process.LayerNext),("XRAY",process.ToggleXray),("WIRE MODE",process.WireMode),("INSPECT MODE",process.InspectMode),("SAVE BOARD",process.SaveBoard),("RESUME BOARD",process.ResumeBoard)
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
                    File.Copy(scene.path,"Logs/Level104_MainMenu_before_registration.unity",true);
                T Find<T>() where T : Component => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)).Single();
                var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder + "/Level_104.asset");
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
                Debug.Log("Level 104 registered in MainMenu.");
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene,true); }
        }

        private static SparkLevelDefinition CreateDefinition()
        {
            var existing=AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(Folder+"/Level_104.asset");
            var level=existing ? existing : ScriptableObject.CreateInstance<SparkLevelDefinition>();
            Set(level,("levelId","level_104"),("displayName","Voltage Guardian"),
                ("description","Build a multi-rail power board, qualify its loads, repair R7 / V05 / Q1 and recover W067 trusted power-good."),
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
                target.FindPropertyRelative("targetId").stringValue=$"level104_pcb_{i+1}";
                target.FindPropertyRelative("displayName").stringValue=Objectives[i];
                target.FindPropertyRelative("description").stringValue="Recorded from the electrical graph, valid calibrated feedback and the level’s verified run history.";
                target.FindPropertyRelative("targetComponentId").stringValue=SparkLevel104Process.Gates[i];
                target.FindPropertyRelative("targetType").enumValueIndex=(int)SparkLevelTarget.TargetType.ComponentPowered;
                target.FindPropertyRelative("minimumVoltage").floatValue=0f;
                target.FindPropertyRelative("useMaximumVoltage").boolValue=false;
                target.FindPropertyRelative("maximumVoltage").floatValue=5.2f;
                target.FindPropertyRelative("requireElectricalEnabled").boolValue=true;
                target.FindPropertyRelative("requireConduction").boolValue=false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            if(!existing) AssetDatabase.CreateAsset(level,Folder+"/Level_104.asset");
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
            var gates=so.FindProperty("runtimeComponentIds"); gates.arraySize=SparkLevel104Process.Gates.Length;
            for(int i=0;i<gates.arraySize;i++) gates.GetArrayElementAtIndex(i).stringValue=SparkLevel104Process.Gates[i];
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




















