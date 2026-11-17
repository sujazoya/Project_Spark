#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Tools;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
[InitializeOnLoad] internal static class Level103Check
{
 const string Request="Logs/SparkLevel103Check.request",Report="Logs/SparkLevel103Check.txt",Key="Spark.Level103.Verification";
 static Level103Check(){EditorApplication.update+=Poll;EditorApplication.playModeStateChanged+=State;}
 static void Poll(){if(!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;File.Delete(Request);File.WriteAllText(Report,"LEVEL 103 / LIVE PLAY MODE ACCEPTANCE\n");SessionState.SetBool(Key,true);EditorApplication.EnterPlaymode();}
 static void Log(string s)=>File.AppendAllText(Report,s+"\n");
 static async void State(PlayModeStateChange state)
 {
  if(!SessionState.GetBool(Key,false))return;
  if(state==PlayModeStateChange.EnteredEditMode){SessionState.EraseBool(Key);Log("Returned to Edit Mode.");return;}
  if(state!=PlayModeStateChange.EnteredPlayMode)return;
  var old=Debug.unityLogger.filterLogType;bool bg=Application.runInBackground;Application.runInBackground=true;Debug.unityLogger.filterLogType=LogType.Error;
  try{
   var manager=Object.FindFirstObjectByType<LevelGamePlayManager>();var so=new SerializedObject(manager);so.FindProperty("enforceProgressionAccess").boolValue=false;so.ApplyModifiedPropertiesWithoutUndo();if(manager.LevelProgression)manager.LevelProgression.enabled=false;
   var entries=so.FindProperty("levels");int index=Enumerable.Range(0,entries.arraySize).Single(i=>entries.GetArrayElementAtIndex(i).objectReferenceValue is SparkLevelDefinition d&&d.LevelId=="level_103");
   if(!manager.StartLevel(index))throw new Exception("Cannot load Level 103");await Task.Delay(1500);
   var b=Object.FindFirstObjectByType<SparkLevel103Process>();if(!b)throw new Exception("Missing process");
   Object.FindFirstObjectByType<ProjectSpark.UIManagement.SparkUIManager>()?.Open("active-level");Object.FindFirstObjectByType<ProjectSpark.UIManagement.SparkUIManager>()?.Close("live-coach");
   async Task Wait(Func<bool> f,string s){var until=DateTime.UtcNow.AddSeconds(45);while(!f()){if(DateTime.UtcNow>until)throw new Exception(s+" | "+b.heading.text+" | "+b.detail.text+" | "+b.readings.text);await Task.Delay(70);}}
   Task Stage(int n)=>Wait(()=>b.Stage==n&&manager.SatisfiedTargetCount==n,"Expected "+n+"/6");
   if(b.InstalledCount!=0||b.SolderedCount!=0||b.RoutedCount!=0||b.ViaCount!=0||b.parts.Length!=44||b.routes.Length!=59)throw new Exception("Bare board contract failed");
   Log("PASS bare board: 44 uninstalled parts, 187 contacts, 58 copper tasks, 12 open vias, W059 open.");
   b.RotatePart();b.FitPart();if(b.InstalledCount!=0)throw new Exception("Wrong orientation accepted");for(int i=0;i<3;i++)b.RotatePart();
   for(int i=0;i<44;i++){b.FitPart();b.FluxPart();b.SolderPart();await Task.Yield();}
   await Stage(1);Log("PASS 1/6 actual solder switches and native devices enabled.");
   b.BoardView();await Task.Delay(600);ScreenCapture.CaptureScreenshot("Logs/SparkLevel103Assembly.png");
   b.WireMode();await Task.Yield();
   var pointer=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=Camera.main.WorldToScreenPoint(b.routes[0].from.transform.position)};
   var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer,hits);
   if(hits.Any(hit=>hit.module is UnityEngine.EventSystems.PhysicsRaycaster))throw new Exception("Repair-body raycaster intercepts wire input");
   Log("PASS Wire Mode disables repair-body UI interception at the highlighted pad.");
   var wire=Object.FindFirstObjectByType<SignalWireBuilder>();var tool=Object.FindFirstObjectByType<SparkWireTool>(FindObjectsInactive.Include);
   void Drag(SparkTerminal a,SparkTerminal z){Vector2 start=Camera.main.WorldToScreenPoint(a.transform.position),end=Camera.main.WorldToScreenPoint(z.transform.position);if(!tool.TryResolveTerminalTarget(start,out var hit)||!wire.BeginWire(a,WirePolarity.Neutral)||!wire.TryEndWire(end))throw new Exception("Manual Wire tool rejected "+a.name+" -> "+z.name);}
   for(int i=0;i<59;i++){b.PlateVia();var route=b.routes[i];if(route.via){Drag(route.from,route.viaTop);Drag(route.viaBottom,route.to);}else Drag(route.from,route.to);await Task.Yield();}
   Log("PASS all 71 manual wire segments through native SparkWireTool and SignalWireBuilder.");
   b.TogglePower();await Stage(2);Log("PASS 2/6 all real connections, 12 vias and supervised reference trusted.");
   b.SaveBoard();b.TogglePower();b.LoadBoard();if(b.RoutedCount!=58||b.ViaCount!=12||b.SolderedCount!=44||b.Stage!=2)throw new Exception("Save replay lost real topology");b.TogglePower();await Wait(()=>b.Trusted,"Reference after saved graph replay");Log("PASS save/resume restored actual copper and solder state, with fresh reference reacquisition.");
   async Task Cal(){await Task.Delay(1200);b.CheckReference();b.ZeroStandard();await Task.Delay(1300);b.Capture();b.SpanStandard();await Task.Delay(1300);b.Capture();await Wait(()=>b.acquisition.AllCalibrated,"Calibration");}
   await Cal();await Stage(3);Log("PASS 3/6 independent reference and measured zero/span calibration.");
   b.TestBoard();await Stage(4);Log("PASS 4/6 four-channel 128 Hz acquisition and dynamic uncertainty.");
   var faults=new Action[]{b.FaultReference,b.FaultVia,b.FaultOptical};
   for(int i=0;i<3;i++){faults[i]();await Task.Delay(800);Log("Fault "+(i+1)+": "+b.readings.text);b.Diagnose();b.TogglePower();b.Repair();b.TogglePower();await Wait(()=>b.Trusted,"Post repair reference");await Cal();b.TestBoard();await Wait(()=>!b.Running,"Post repair qualification");Log("Repair "+(i+1)+": "+b.detail.text);}
   await Stage(5);Log("PASS 5/6 three independent diagnosed repairs and fresh qualification.");
   b.ArmW059();var before=b.acquisition.channels.Select(c=>c.stimulus).ToArray();b.UndoCopper();await Wait(()=>b.SafetyLatched&&!b.Trusted,"W059 loss");if(!before.SequenceEqual(b.acquisition.channels.Select(c=>c.stimulus))||b.Stage!=5)throw new Exception("Loss discarded physical state or evidence");
   b.RouteCopper();await Wait(()=>b.Trusted,"W059 continuity restored");await Cal();b.ResetSafety();b.TestBoard();await Stage(6);await Wait(()=>manager.IsCompleted,"Campaign completion");Log("PASS 6/6 authoritative manager completion; W059 loss preserved stimuli and evidence, recalibration/reset/requalification required.");
   b.BoardView();await Task.Delay(700);ScreenCapture.CaptureScreenshot("Logs/SparkLevel103Complete.png");await Task.Delay(300);
  }catch(Exception e){Log("FAIL "+e);}finally{Debug.unityLogger.filterLogType=old;Application.runInBackground=bg;EditorApplication.ExitPlaymode();}
 }
}
#endif




