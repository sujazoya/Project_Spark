#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.EditorTools;
using ProjectSpark.Gameplay;
using ProjectSpark.Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

[InitializeOnLoad]
internal static class Level102Check
{
    private const string Request="Logs/SparkLevel102Check.request", Report="Logs/SparkLevel102Check.txt";
    private const string Key="Spark.Level102.Verification";
    static Level102Check()
    {
        EditorApplication.update+=Poll;
        EditorApplication.playModeStateChanged+=StateChanged;
    }

    private static void Poll()
    {
        if(!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().name!="MainMenu") return;
        File.Delete(Request);
        File.WriteAllText(Report,"LEVEL 102 / PULSE ARCHITECT\n");
        try
        {
            SparkLevel102Builder.Create();
            SessionState.SetBool(Key,true);
            EditorApplication.EnterPlaymode();
        }
        catch(Exception ex){ReportLine("FAIL "+ex);}
    }

    private static async void StateChanged(PlayModeStateChange state)
    {
        if(!SessionState.GetBool(Key,false)) return;
        if(state==PlayModeStateChange.EnteredEditMode){ SessionState.EraseBool(Key); ReportLine("Returned to Edit Mode."); }
        if(state!=PlayModeStateChange.EnteredPlayMode) return;
        bool background=Application.runInBackground;
        var logLevel=Debug.unityLogger.filterLogType;
        Application.runInBackground=true;
        Debug.unityLogger.filterLogType=LogType.Error;
        try
        {
            var manager=Object.FindFirstObjectByType<LevelGamePlayManager>();
            var serialized=new SerializedObject(manager);
            serialized.FindProperty("enforceProgressionAccess").boolValue=false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            if(manager.LevelProgression) manager.LevelProgression.enabled=false;
            var entries=serialized.FindProperty("levels");
            int index=Enumerable.Range(0,entries.arraySize).Single(i=>
                entries.GetArrayElementAtIndex(i).objectReferenceValue is SparkLevelDefinition level && level.LevelId=="level_102");
            if(!manager.StartLevel(index)) throw new InvalidOperationException("Level 102 did not start.");
            await Task.Delay(900);
            var board=Object.FindFirstObjectByType<SparkLevel102Process>();
            if(!board) throw new InvalidOperationException("Level 102 controller missing.");
            async Task Wait(Func<bool> ready,string action)
            {
                var until=DateTime.UtcNow.AddSeconds(17);
                while(!ready())
                {
                    if(DateTime.UtcNow>until) throw new InvalidOperationException($"{action} | {board.heading.text} | {board.detail.text} | {board.readings.text} | {manager.LastEvaluationMessage}");
                    await Task.Delay(50);
                }
            }
            Task At(int count)=>Wait(()=>manager.SatisfiedTargetCount==count,$"Expected {count}/6");
            await At(1);
            if(board.parts.Length!=35 || board.parts.Any(part=>!part.activeInHierarchy) || board.routes.Length!=49 ||
                board.routes.Count(route=>route.via)!=10 || board.jumperArt.activeSelf)
                throw new InvalidOperationException("Invalid normal startup board state.");
            ReportLine("Normal mode: 35 fixed parts visible, 48 traces and ten vias uncompleted, W049 unplugged.");
            var instruments=Object.FindFirstObjectByType<SparkLevel102Instruments>();
            if(!instruments) throw new InvalidOperationException("Physical bench instruments are missing.");
            var boardSide=Vector3.Dot(Camera.main.transform.position-board.arena.position,board.arena.forward);
            if(boardSide>=0f) throw new InvalidOperationException("Board camera opened from the old side.");
            instruments.Open(1);
            if(!instruments.MeterText().Contains("PROBE DISCONNECTED")) throw new InvalidOperationException("Multimeter should open with probe detached.");
            instruments.Close();
            ReportLine("Startup camera faces the corrected side. Bench instrument panels are available.");
            board.ToggleChallengeMode();
            await At(0);
            if(board.parts.Any(part=>part.activeSelf)) throw new InvalidOperationException("Challenge did not hide the components.");
            for(int i=0;i<48;i++) board.InspectNet();
            for(int i=0;i<35;i++)
            {
                for(int j=i%4;j>0&&j<4;j++) board.RotatePart();
                board.FitPart();
            }
            await At(1);
            ReportLine("Challenge mode: inspected and fitted all 35 fixed components.");
            var dashboard=Object.FindFirstObjectByType<ProjectSpark.UIManagement.SparkUIMonitorDashboard>();
            if(dashboard)dashboard.SetVisible(false);
            await Task.Delay(650);
            ScreenCapture.CaptureScreenshot("Logs/SparkLevel102Board.png");
            await Task.Delay(200);

            var wire=Object.FindFirstObjectByType<SignalWireBuilder>();
            var tool=Object.FindFirstObjectByType<SparkWireTool>(FindObjectsInactive.Include);
            var camera=Camera.main;
            void Drag(SparkTerminal a,SparkTerminal b)
            {
                Vector2 start=camera.WorldToScreenPoint(a.transform.position);
                Vector2 end=camera.WorldToScreenPoint(b.transform.position);
                if(!tool.TryResolveTerminalTarget(start,out var hit) || hit.Collider.GetComponent<SparkTerminal>()!=a ||
                    !wire.BeginWire(a,WirePolarity.Neutral) || !wire.TryEndWire(end))
                    throw new InvalidOperationException($"Cannot draw {a.name} to {b.name}: {board.detail.text}");
            }

            for(int i=0;i<48;i++)
            {
                var route=board.routes[i];
                board.FitVia();
                if(route.via){ Drag(route.from,route.viaTop); Drag(route.viaBottom,route.to); }
                else Drag(route.from,route.to);
                await Task.Yield();
            }
            var jumper=board.routes[48];
            Drag(jumper.from,jumper.to);
            instruments.Open(0);
            instruments.Operate();
            instruments.Close();
            await At(2);
            instruments.Open(1);
            instruments.Operate();
            if(!instruments.MeterText().Contains("3.3"))
                throw new InvalidOperationException("Multimeter did not read the 3.3 V monitor rail: "+instruments.MeterText());
            ReportLine("Bench supply enabled from its instrument panel. Multimeter: "+instruments.MeterText().Replace("\n"," / "));
            instruments.Close();
            ReportLine("All 48 copper tasks, ten plated vias and the removable W049 verified by the circuit.");
            instruments.Open(2);
            instruments.Operate();
            instruments.Operate();
            await At(3);
            instruments.Close();
            ReportLine("Clock capture passed: "+board.detail.text);
            int dutyCount=0;
            foreach(var duty in new Action[]{board.Duty25,board.Duty50,board.Duty75})
            {
                duty();
                await Task.Delay(450);
                instruments.Open(3);
                instruments.Operate();
                instruments.Close();
                dutyCount++;
                string status="DUTIES "+dutyCount+"/3";
                await Wait(()=>board.detail.text.Contains(status),"PWM capture did not qualify");
                ReportLine(board.detail.text);
            }
            await At(4);
            string[] diagnosis={"V06 open","22 nF","gate-to-drain"};
            var faults=new Action[]{board.InjectVia,board.InjectPullup,board.InjectBridge};
            for(int i=0;i<faults.Length;i++)
            {
                faults[i]();
                await Wait(()=>board.detail.text.Contains("Fault active"),"Inject fault");
                await Task.Delay(300);
                board.Measure();
                int kind=i;
                await Wait(()=>board.detail.text.Contains(diagnosis[kind]),"Measure fault");
                ReportLine(board.detail.text);
                board.TogglePower();
                board.Repair();
                await Wait(()=>board.detail.text.Contains("Repair applied"),"Repair after isolating power");
                board.TogglePower();
                await Wait(()=>board.readings.text.Contains("W049 TRUSTED"),"Repaired feedback");
            }
            board.TestBoard();
            await At(5);
            board.ArmJumper();
            board.UndoCopper();
            await Wait(()=>board.detail.text.Contains("Output latched off"),"Open W049");
            if(manager.IsCompleted || manager.SatisfiedTargetCount!=5)
                throw new InvalidOperationException("Opening W049 altered completion history.");
            Drag(jumper.from,jumper.to);
            await Wait(()=>board.readings.text.Contains("W049 TRUSTED"),"Restore W049");
            board.Measure();
            board.ResetSafety();
            board.TestBoard();
            await Wait(()=>manager.IsCompleted && manager.SatisfiedTargetCount==6,"Campaign completion");
            ReportLine("PASS 6/6. W049 disconnected live, output inhibited, feedback reacquired and final capture passed.");
            instruments.Open(2);
            await Task.Delay(250);
            ScreenCapture.CaptureScreenshot("Logs/SparkLevel102Complete.png");
            await Task.Delay(300);
        }
        catch(Exception ex){ReportLine("FAIL "+ex);}
        finally
        {
            Debug.unityLogger.filterLogType=logLevel;
            Application.runInBackground=background;
            EditorApplication.ExitPlaymode();
        }
    }

    private static void ReportLine(string value)=>File.AppendAllText(Report,value+"\n");
}
#endif
