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
internal static class Level95Check
{
    private const string Request="Logs/SparkLevel95Check.request", Report="Logs/SparkLevel95Check.txt";
    private const string Key="Spark.Level95.Verification";
    static Level95Check()
    {
        EditorApplication.update+=Poll;
        EditorApplication.playModeStateChanged+=StateChanged;
    }

    private static void Poll()
    {
        if(!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().name!="MainMenu") return;
        File.Delete(Request);
        File.WriteAllText(Report,"LEVEL 95 / PULSE ARCHITECT\n");
        try
        {
            SparkLevel95Builder.BakePrefabPresentation();
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
        string checkpoint = PlayerPrefs.GetString(SparkLevel95Process.CheckpointKey, "");
        const string victoryKey = "ProjectSpark.Tutorial.LevelWon.level_95";
        int victory = PlayerPrefs.GetInt(victoryKey, -1);
        SparkLevel95Process board = null;
        try
        {
            PlayerPrefs.DeleteKey(SparkLevel95Process.CheckpointKey);
            var manager=Object.FindFirstObjectByType<LevelGamePlayManager>();
            var serialized=new SerializedObject(manager);
            serialized.FindProperty("enforceProgressionAccess").boolValue=false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            if(manager.LevelProgression) manager.LevelProgression.enabled=false;
            var entries=serialized.FindProperty("levels");
            int index=Enumerable.Range(0,entries.arraySize).Single(i=>
                entries.GetArrayElementAtIndex(i).objectReferenceValue is SparkLevelDefinition level && level.LevelId=="level_95");
            if(!manager.StartLevel(index)) throw new InvalidOperationException("Level 95 did not start.");
            await Task.Delay(900);
            board=manager.LayoutLoader.ActiveLayout.GetComponent<SparkLevel95Process>();
            if(!board) throw new InvalidOperationException("Level 95 controller missing.");
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
            await At(0);
            if(board.parts.Length!=35 || board.parts.Any(part=>part.activeSelf) || board.routes.Length!=49 ||
                board.routes.Count(route=>route.via)!=10 || board.jumperArt.activeSelf || board.supply.IsOutputActive)
                throw new InvalidOperationException("Invalid cold startup board state.");
            var bounds = board.arena.Find("L95_ART/L95_FR4PrecisionBoardCore").GetComponentsInChildren<Renderer>();
            Bounds substrate = bounds[0].bounds;
            foreach (var surface in bounds) substrate.Encapsulate(surface.bounds);
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable")
                .GetComponent<BoxCollider>().bounds;
            if (substrate.size.x < table.size.x * .53f || substrate.size.x > table.size.x * .69f ||
                substrate.size.z < table.size.z * .64f || substrate.size.z > table.size.z * .80f)
                throw new InvalidOperationException($"Unexpected PCB {substrate.size} on table {table.size}.");
            var overview = Camera.main;
            await Wait(() => overview.orthographic &&
                !overview.GetComponent<Unity.Cinemachine.CinemachineBrain>().IsBlending, "Settle the board camera");
            foreach (float x in new[] { substrate.min.x, substrate.max.x })
            foreach (float z in new[] { substrate.min.z, substrate.max.z })
            {
                var corner = Camera.main.WorldToViewportPoint(new Vector3(x, substrate.center.y, z));
                if (corner.x < .225f || corner.x > .98f || corner.y < .025f || corner.y > .98f || corner.z <= 0)
                    throw new InvalidOperationException($"Board overlaps the service console or viewport: {corner}, " +
                        $"aspect {overview.aspect:F2}, size {overview.orthographicSize:F2}, camera {overview.transform.position}.");
            }
            ReportLine($"PASS 0/6 opening. PCB {substrate.size.x:F2} × {substrate.size.z:F2} on a {table.size.x:F2} × {table.size.z:F2} table.");
            var instruments=Object.FindFirstObjectByType<SparkLevel95Instruments>();
            if(!instruments) throw new InvalidOperationException("Bench instruments are missing.");
            instruments.Open(1);
            if(!instruments.MeterText().Contains("PROBE DISCONNECTED")) throw new InvalidOperationException("Multimeter should open with probe detached.");
            instruments.Close();
            ScreenCapture.CaptureScreenshot("Logs/SparkLevel95Opening.png");
            await Task.Delay(300);
            for(int i=0;i<48;i++) board.InspectNet();
            board.NextStockPart();
            board.FitPart();
            board.SolderPart();
            if (board.parts[0].activeSelf) throw new InvalidOperationException("Wrong stock passed.");
            board.MatchPart();
            board.FitPart();
            board.SolderPart();
            if (board.parts[0].activeSelf) throw new InvalidOperationException("Wrong orientation passed.");
            for(int i=0;i<35;i++)
            {
                board.MatchPart();
                for (int turns = 1 + i % 3; turns < 4; turns++) board.RotatePart();
                board.FitPart();
                if (board.parts[i].activeSelf) throw new InvalidOperationException("Unsoldered placement passed.");
                board.SolderPart();
            }
            await At(1);
            ReportLine("PASS Netlist inspected and all 35 fixed components fitted and soldered. W049 remains a separate part.");
            board.BoardView();
            await Task.Delay(300);
            ScreenCapture.CaptureScreenshot("Logs/SparkLevel95Copper.png");
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

            Drag(board.routes[0].from, board.routes[0].to);
            if (!manager.RestartLevel()) throw new InvalidOperationException("Restart failed.");
            await Task.Delay(700);
            board = manager.LayoutLoader.ActiveLayout.GetComponent<SparkLevel95Process>();
            instruments = Object.FindFirstObjectByType<SparkLevel95Instruments>();
            board.ResumeCheckpoint();
            await At(1);
            if (!manager.CircuitSystem.HasConnectionBetween(board.routes[0].from, board.routes[0].to) || board.supply.IsOutputActive)
                throw new InvalidOperationException("The checkpoint did not restore W001 in isolation.");
            ReportLine("PASS Immediate restart restored soldered parts and W001 with bench power isolated.");
            for(int i=1;i<48;i++)
            {
                var route=board.routes[i];
                if (route.via)
                {
                    board.BackCopper();
                    board.FitVia();
                    if (route.via.IsClosed || tool.TryResolveTerminalTarget(camera.WorldToScreenPoint(route.from.transform.position), out _))
                        throw new InvalidOperationException("Wrong copper face accepted the via or its front pad.");
                    board.FrontCopper();
                    board.FitVia();
                    Drag(route.from, route.viaTop);
                    board.BackCopper();
                    Drag(route.viaBottom, route.to);
                }
                else Drag(route.from, route.to);
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
                board.Repair();
                await Wait(()=>board.detail.text.Contains("Isolate the bench"), "Reject a live repair");
                board.TogglePower();
                board.Repair();
                await Wait(()=>board.detail.text.Contains("Repair applied"),"Repair after isolating power");
                board.TogglePower();
                await Wait(()=>board.readings.text.Contains("W049 TRUSTED"),"Repaired feedback");
                board.TestBoard();
                await Wait(()=>board.detail.text.Contains("Repair retest passed"), "Independent powered retest");
            }
            ReportLine("PASS Three fault cases independently diagnosed, isolated, repaired and retested.");
            await At(5);
            board.ArmJumper();
            board.UndoCopper();
            await Wait(()=>board.detail.text.Contains("Output latched off"),"Open W049");
            if(!board.readings.text.Contains("OUTPUT inhibited") || board.readings.text.Contains("CLOCKS  unavailable") ||
                manager.IsCompleted || manager.SatisfiedTargetCount!=5)
                throw new InvalidOperationException("Opening W049 altered completion history.");
            Drag(jumper.from,jumper.to);
            await Wait(()=>board.readings.text.Contains("W049 TRUSTED"),"Restore W049");
            board.Measure();
            board.ResetSafety();
            board.TestBoard();
            await Wait(()=>manager.IsCompleted && manager.SatisfiedTargetCount==6,"Campaign completion");
            ReportLine("PASS 6/6. W049 disconnected live, output inhibited, feedback reacquired and final capture passed.");
            await Task.Delay(450);
            ScreenCapture.CaptureScreenshot("Logs/SparkLevel95Complete.png");
            await Task.Delay(300);
        }
        catch(Exception ex){ReportLine("FAIL "+ex);}
        finally
        {
            if (board) board.enabled = false;
            if (string.IsNullOrEmpty(checkpoint)) PlayerPrefs.DeleteKey(SparkLevel95Process.CheckpointKey);
            else PlayerPrefs.SetString(SparkLevel95Process.CheckpointKey, checkpoint);
            if (victory < 0) PlayerPrefs.DeleteKey(victoryKey);
            else PlayerPrefs.SetInt(victoryKey, victory);
            PlayerPrefs.Save();
            Debug.unityLogger.filterLogType=logLevel;
            Application.runInBackground=background;
            EditorApplication.ExitPlaymode();
        }
    }

    private static void ReportLine(string value)=>File.AppendAllText(Report,value+"\n");
}
#endif
