#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Threading.Tasks;
using UnityEditor;using UnityEngine;using UnityEngine.EventSystems;using UnityEngine.UI;
using AAAUI.VFX;using ProjectSpark.Gameplay;using ProjectSpark.PCBVirtualStudio;using ProjectSpark.Tools;using ProjectSpark.UIManagement;using ProjectSpark.VoltageGuardian;using Object=UnityEngine.Object;
[InitializeOnLoad]static class Level104Acceptance
{
 const string Prefix="Logs/SparkLevel104Acceptance",Key="Spark104.Acceptance";
 const string BoardSaveKey="ProjectSpark.Level104.Board.v1";
 static SparkLevel104Process b;static LevelGamePlayManager manager;
 static Level104Acceptance(){EditorApplication.update+=Poll;EditorApplication.playModeStateChanged+=State;}
 static void Check(bool pass,string detail){File.AppendAllText(Prefix+".txt",(pass?"PASS ":"FAIL ")+detail+"\n");if(!pass)throw new Exception(detail);}
 static void Poll()
 {
  if(!File.Exists(Prefix+".request")||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
  File.Delete(Prefix+".request");File.WriteAllText(Prefix+".txt","LEVEL 104 / ACTUAL UNITY ACCEPTANCE\n");
  try
  {
   var d=AssetDatabase.LoadAssetAtPath<SparkLevel104Design>("Assets/ProjectSpark/Level104/Data/VGR5_Educational_Design.asset");Check(d&&d.IsResolved(out _),"Educational schematic resolved; 50 parts / 67 routes / 14 vias");
   Check(d.contacts.Select(c=>c.id).Distinct().Count()==d.contacts.Length,"Unique pad identities");
   var plant=new SparkVoltageGuardianPlant();Check(!plant.TryEnable(1)&&!plant.TryEnable(2),"Out-of-sequence secondary startup rejected");plant.TryEnable(0);
   for(int i=0;i<250&&!plant.PowerGood;i++)plant.Advance(.01,12,true,false);
   Check(plant.PowerGood&&!plant.TryEnable(1),"5 V reaches tolerance before 3V3 may be enabled");
   for(int i=0;i<100&&!plant.CanEnableSecondary;i++)plant.Advance(.01,12,true,false);
   Check(plant.TryEnable(1),"3V3 enabled after the 5 V stability dwell");
   for(int i=0;i<100&&!plant.CanEnableCore;i++)plant.Advance(.01,12,true,false);
   Check(plant.TryEnable(2),"1V8 enabled after the 3V3 stability dwell");
   for(int i=0;i<100;i++)plant.Advance(.01,12,true,false);
   Check(plant.StableRails,"Finite 3V3 and 1V8 soft-start");
   plant.ObserveTrusted(true,true,true);plant.ResetSafety(true);plant.LoadOhms=10;for(int i=0;i<200;i++)plant.Advance(.01,12,true,true);Check(plant.Qualified(plant.latest,true),"Nominal load efficiency / ripple / temperature");
   double before=plant.output;plant.ObserveTrusted(false,false,true);plant.Advance(.02,12,true,true);Check(plant.safetyLatched&&!plant.latest.loadsEnabled&&plant.output>before*.95,"W067 UNKNOWN inhibits loads without collapsing physical rail");
   Check(!plant.ResetSafety(false),"Stale independent measurements cannot reset safety");
   SessionState.SetBool(Key,true);EditorApplication.EnterPlaymode();
  }catch(Exception e){File.AppendAllText(Prefix+".txt",e+"\n");}
 }
 static async void State(PlayModeStateChange state)
 {
  if(!SessionState.GetBool(Key,false))return;
  if(state==PlayModeStateChange.EnteredEditMode)
  {
   SessionState.EraseBool(Key);
   File.AppendAllText(Prefix+".txt","Returned to Edit Mode.\n");
   return;
  }
  if(state!=PlayModeStateChange.EnteredPlayMode)return;
  var prior=Debug.unityLogger.filterLogType;Debug.unityLogger.filterLogType=LogType.Error;bool background=Application.runInBackground;Application.runInBackground=true;
  bool hadSave=PlayerPrefs.HasKey(BoardSaveKey);
  string originalSave=PlayerPrefs.GetString(BoardSaveKey);
  try{await Play();File.AppendAllText(Prefix+".txt","COMPLETE: actual manager 6/6.\n");}
  catch(Exception e){File.AppendAllText(Prefix+".txt","SCENARIO STOPPED "+e+"\n"); if(b)File.WriteAllLines(Prefix+"_devices.txt",b.GetComponentsInChildren<SparkElectricalComponent>(true).Select(c=>c.name+" "+c.GetType().Name+" enabled="+c.ElectricalEnabled+" V="+c.ElectricalState.Voltage+" I="+c.ElectricalState.Current+(c is SparkResistor r?" burned="+r.IsBurned+" R="+r.Resistance:"")));}
  finally
  {
   if(hadSave)PlayerPrefs.SetString(BoardSaveKey,originalSave);else PlayerPrefs.DeleteKey(BoardSaveKey);
   PlayerPrefs.Save();
   Debug.unityLogger.filterLogType=prior;
   Application.runInBackground=background;
   EditorApplication.ExitPlaymode();
  }
 }
 static async Task Wait(Func<bool> predicate,string task,int seconds=40)
 {
  var until=DateTime.UtcNow.AddSeconds(seconds);
  while(!predicate()){if(DateTime.UtcNow>until)throw new Exception(task+" | "+b?.heading.text+" | "+b?.readings.text+" | "+b?.detail.text);await Task.Delay(80);}
 }
 static async Task Stage(int value){await Wait(()=>b.Stage==value&&manager.SatisfiedTargetCount==value,"Expected "+value+"/6");Check(true,"Manager confirms "+value+"/6");}
 static async Task Sequence()
 {
  b.TogglePower();
  b.EnableMain();
  await Wait(()=>b.CanEnableSecondary,"5 V stability dwell");
  b.EnableSecondary();
  Check(b.plant.enableSecondary,"3V3 enable accepted");
  await Wait(()=>b.CanEnableCore,"3V3 stability dwell");
  b.EnableCore();
  Check(b.plant.enableCore,"1V8 enable accepted");
  await Wait(()=>b.plant.StableRails&&b.Measured.valid&&b.Measured.core>1.75,"1V8 startup");
  await Task.Delay(400);
  b.CheckRails();
 }
 static async Task Qualify(){b.TestBoard();Check(b.Running,"Three-load test started from valid authority");await Wait(()=>!b.Running,"Three-load qualification");}
 static void SelectRoute(int index)
 {
  for(int i=0;i<b.routes.Length&&b.SelectedRoute!=index;i++)b.NextRoute();
 }
 static bool HasCopper(SparkLevel104Process.Segment segment) =>
  manager.CircuitSystem.HasConnectionBetween(segment.from,segment.to);
 static async Task CheckControlsAndRouting(SparkUIManager ui, SignalWireBuilder wire)
 {
  var canvas=b.workstation.GetComponent<Canvas>();
  await Wait(()=>ui.Find("active-level"),"Current Level Controls registered");
  ui.Open("active-level");
  Check(ui.Close("active-level")&&!canvas.enabled&&ui.Open("active-level")&&canvas.enabled,
   "Master UI can close and reopen the active controls");
  int routeIndex=Array.FindIndex(b.routes,route=>route.segments.Length>1);
  SelectRoute(routeIndex);
  b.PlateVia();b.PlateVia();b.PadView();b.WireMode();
  await Task.Delay(700);
  Canvas.ForceUpdateCanvases();
  var corners=new Vector3[4];
  var buttons=canvas.GetComponentsInChildren<Button>();
  Check(canvas.renderMode==RenderMode.ScreenSpaceOverlay&&buttons.Length==31&&buttons.All(button=>
  {
   ((RectTransform)button.transform).GetWorldCorners(corners);
   return corners.All(point=>canvas.pixelRect.Contains(RectTransformUtility.WorldToScreenPoint(null,point)));
  }),"All 31 controls are on screen in Pad View");

  var route=b.routes[routeIndex];
  var first=route.segments[0];
  var tool=Object.FindFirstObjectByType<SparkWireTool>(FindObjectsInactive.Include);
  Vector2 start=Camera.main.WorldToScreenPoint(first.from.transform.position);
  Vector2 end=Camera.main.WorldToScreenPoint(first.to.transform.position);
  var pointer=new PointerEventData(EventSystem.current){position=start};
  var hits=new List<RaycastResult>();
  EventSystem.current.RaycastAll(pointer,hits);
  Check(!hits.Any(hit=>hit.module is PhysicsRaycaster||hit.module is GraphicRaycaster)
   &&tool.TryResolveTerminalTarget(start,out var resolved)&&resolved.Collider.GetComponent<SparkTerminal>()==first.from,
   "Wire tool reaches the highlighted pad with controls visible");
  Check(wire.BeginWire(first.from,WirePolarity.Neutral)&&wire.TryEndWire(end)&&HasCopper(first),
   "Native wire input creates an authoritative copper edge");
  b.SaveBoard();
  b.UndoCopper();
  Check(!HasCopper(first),"Undo removes the same native edge");
  Check(b.LoadBoard()&&HasCopper(first)&&!HasCopper(route.segments[1])&&b.ViaCount==2&&b.RoutedCount==0,
   "Save/resume restores both plated vias and partial copper without awarding route completion");
  b.InspectMode();
  b.BoardView();
 }
 static async Task Play()
 {
  manager=Object.FindFirstObjectByType<LevelGamePlayManager>();var so=new SerializedObject(manager);so.FindProperty("enforceProgressionAccess").boolValue=false;so.ApplyModifiedPropertiesWithoutUndo();if(manager.LevelProgression)manager.LevelProgression.enabled=false;
  var levels=so.FindProperty("levels");int index=Enumerable.Range(0,levels.arraySize).Single(i=>levels.GetArrayElementAtIndex(i).objectReferenceValue is SparkLevelDefinition d&&d.LevelId=="level_104");
  Check(manager.StartLevel(index),"Campaign loads Level 104");await Task.Delay(1200);b=Object.FindFirstObjectByType<SparkLevel104Process>();Check(b,"Native process instantiated");
  var ui=Object.FindFirstObjectByType<SparkUIManager>();
  var wire=Object.FindFirstObjectByType<SignalWireBuilder>();
  ui.Close("live-coach");
  Check(b.InstalledCount==0&&b.SolderedCount==0&&b.RoutedCount==0&&b.ViaCount==0&&manager.SatisfiedTargetCount==0,"Empty startup has zero component/copper/via/target authority");
  Check(!b.TryFitPart("U1","U2",out _),"Wrong footprint identity rejected");b.RotatePart();b.FitPart();Check(b.InstalledCount==0,"Reversed pin-1 rejected");for(int i=0;i<3;i++)b.RotatePart();
  foreach(var p in b.design.parts){Check(b.TryFitPart(p.reference,p.reference,out _),"Fit "+p.reference);int pi=Array.FindIndex(b.design.parts,x=>x.reference==p.reference);b.TryApplyRepair(b.parts[pi].target,SparkVirtualTool.Flux,out _);Check(b.TryApplyRepair(b.parts[pi].target,SparkVirtualTool.Solder,out _),"Solder "+p.reference);}
  await Stage(1);
  await CheckControlsAndRouting(ui,wire);
  using(manager.ElectricalSolver.BeginConfigurationBatch())
  {
   for(int route=0;route<67;route++)
   {
    SelectRoute(route);
    b.PlateVia();b.PlateVia();
    foreach(var segment in b.routes[route].segments)
     if(!HasCopper(segment))b.RouteCopper();
   }
  }
  await Stage(2);await Sequence();await Stage(3);await Qualify();await Stage(4);
  for(int f=0;f<3;f++)
  {
   Check(b.InjectFault(f),"Inject real fault "+f);
   string reference=f==0?"R7":f==1?"L1":"Q1";
   var target=b.parts[Array.FindIndex(b.design.parts,part=>part.reference==reference)].target;
   var instrument=f==0?SparkVirtualTool.Multimeter:f==1?SparkVirtualTool.Continuity:SparkVirtualTool.ThermalCamera;
   Check(!b.TryApplyRepair(target,SparkVirtualTool.Solder,out _),"Energized rework blocked for "+reference);
   if(f==2)
    await Wait(()=>b.plant.temperature>=b.plant.configuration.ambient+12,"Q1 thermal rise");
   else
   {
    Check(!b.TryReadProbe(target,instrument,out _),"Powered resistance/continuity check rejected");
    b.TogglePower();
   }
   b.TryReadProbe(target,SparkVirtualTool.Inspect,out _);
   if(f!=2)
   {
    b.Repair();
    Check(!b.InjectFault((f+1)%3),"Inspect does not authorize repair");
   }
   Check(b.TryReadProbe(target,instrument,out string diagnosis),"Instrument confirms "+diagnosis);
   if(f==2)b.TogglePower();
   b.Repair();
   await Wait(()=>!b.plant.latest.valid,"Input isolated before restart");
   await Sequence();await Qualify();
  }
  await Stage(5);b.ArmW067();double retained=b.Measured.main;b.UndoCopper();await Task.Delay(350);
  Check(b.plant.feedback==TrustedFeedback.Missing&&b.plant.safetyLatched&&!b.safeLoad.IsConducting
   &&b.Measured.current<.001&&b.Measured.main>retained*.95,"W067 dropout retains physical 5 V and opens the dependent load path");
  b.ResetSafety();Check(b.plant.safetyLatched,"Reset before reconnection rejected");b.RouteCopper();await Wait(()=>b.plant.StableRails,"Rails retained after W067 recovery");await Task.Delay(300);b.TestBoard();Check(!b.Running&&b.Stage==5,"Reconnect alone cannot qualify");b.CheckRails();b.ResetSafety();await Qualify();await Stage(6);
  Check(manager.IsCompleted&&manager.SatisfiedTargetCount==6,"AUTHORITATIVE LEVEL COMPLETE 6/6");b.BoardView();await Task.Delay(500);ScreenCapture.CaptureScreenshot("Logs/SparkLevel104Complete.png");await Task.Delay(300);
  Check(manager.PreviousLevel(),"Campaign navigation returns to Level 103");
  await Wait(()=>!b&&Object.FindFirstObjectByType<SparkLevel103Process>()!=null,"Level 104 unload / Level 103 handoff");
  Check(manager.ActiveLevel.LevelId=="level_103"&&wire.TerminalPickerOverride?.Method.DeclaringType==typeof(SparkLevel103Process),
   "Level 103 owns the loaded layout and wire picker");
  Check(manager.PreviousLevel(),"Campaign navigation returns to Level 95");
  await Wait(()=>Object.FindFirstObjectByType<SparkLevel95Process>()!=null
   &&wire.TerminalPickerOverride?.Method.DeclaringType==typeof(SparkLevel95Process),"Level 95 startup and wire picker handoff");
  Check(manager.ActiveLevel.LevelId=="level_95"&&!Object.FindFirstObjectByType<SparkLevel103Process>(),
   "Level 95 owns the loaded layout and wire picker");
 }
}
#endif

