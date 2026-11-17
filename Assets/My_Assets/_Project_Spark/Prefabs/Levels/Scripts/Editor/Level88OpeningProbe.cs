#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[InitializeOnLoad]
internal static class Level88OpeningProbe
{
    const string Request = "Logs/SparkLevel88Opening.request";
    const string Report = "Logs/SparkLevel88Opening.txt";
    const string Key = "ProjectSpark.Level88.Opening";

    static Level88OpeningProbe()
    {
        EditorApplication.update += Poll;
        EditorApplication.playModeStateChanged += Play;
    }

    private static void Poll()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
            SceneManager.GetActiveScene().isDirty || SceneManager.GetActiveScene().name != "MainMenu" ||
            !File.Exists(Request)) return;
        File.Delete(Request);
        File.WriteAllText(Report,"Level 88 opening Play Mode check\n");
        SessionState.SetBool(Key,true);
        EditorApplication.EnterPlaymode();
    }

    private static async void Play(PlayModeStateChange mode)
    {
        if (!SessionState.GetBool(Key,false)) return;
        if (mode == PlayModeStateChange.EnteredEditMode)
        {
            SessionState.EraseBool(Key);
            File.AppendAllText(Report,"Returned to Edit Mode.\n");
        }
        if (mode != PlayModeStateChange.EnteredPlayMode) return;
        bool previous = Application.runInBackground;
        float previousTimeScale = Time.timeScale;
        var previousLogFilter = Debug.unityLogger.filterLogType;
        Debug.unityLogger.filterLogType = LogType.Error;
        Application.runInBackground = true;
        try
        {
            var manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
            var levels = new SerializedObject(manager).FindProperty("levels");
            var index = Enumerable.Range(0,levels.arraySize).Single(i =>
                levels.GetArrayElementAtIndex(i).objectReferenceValue is SparkLevelDefinition definition && definition.LevelId == "level_88");
            var settings = new SerializedObject(manager);
            settings.FindProperty("enforceProgressionAccess").boolValue = false;
            settings.ApplyModifiedPropertiesWithoutUndo();
            if (manager.LevelProgression) manager.LevelProgression.enabled = false;
            if (!manager.StartLevel(index)) throw new InvalidOperationException("Cannot start Level 88.");
            await Task.Delay(1800);
            var process = Object.FindFirstObjectByType<SparkLevel88Process>();
            if (!process || manager.ActiveTargetCount != 6 || manager.SatisfiedTargetCount != 0 ||
                process.pins.Length != 328 || !process.status.text.Contains("0/164"))
                throw new InvalidOperationException("Expected six unearned objectives and an unwired 328-contact layout.");
            var art = process.arena.Find("L88_HD_VISUAL_ART");
            var deck = art.Find("L88_HabitatAtmosphereQualificationArenaBase");
            var bounds = deck.GetComponentsInChildren<Renderer>().Select(r=>r.bounds).Aggregate((a,b)=>{a.Encapsulate(b);return a;});
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var coverage = Mathf.Max(bounds.size.x/table.bounds.size.x,bounds.size.z/table.bounds.size.z);
            if (coverage < .92f || coverage > 1.01f)
                throw new InvalidOperationException($"Unexpected table coverage {coverage:P1}");
            File.AppendAllText(Report,$"PASS: Registered 0/6; {process.pins.Length} fixed contacts; {art.childCount} modules; deck {bounds.size.x:F2} x {bounds.size.z:F2}; limiting-axis table coverage {coverage:P1}.\n");
            var camera = Camera.main;
            if (!camera) throw new InvalidOperationException("Main camera missing");
            ScreenCapture.CaptureScreenshot("Logs/SparkLevel88Opening.png");
            await Task.Delay(1000);
            process.SocketView();
            await Task.Delay(1000);
            var wireBuilder = Object.FindFirstObjectByType<SignalWireBuilder>();
            var pick = typeof(SignalWireBuilder).GetMethod("FindTerminalForNewMode",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            Physics.SyncTransforms();
            for (int endpoint = 0; endpoint < 2; endpoint++)
            {
                Vector3 pin = camera.WorldToViewportPoint(process.pins[endpoint].transform.position);
                if (pin.z <= 0 || pin.x < .12f || pin.x > .88f || pin.y < .19f || pin.y > .84f)
                    throw new InvalidOperationException($"Socket View did not frame W001 endpoint {endpoint} at {pin}.");
                var screen = (Vector2)camera.WorldToScreenPoint(process.pins[endpoint].transform.position);
                if (!ReferenceEquals(process.pins[endpoint],pick.Invoke(wireBuilder,new object[]{screen})))
                    throw new InvalidOperationException($"Wire tool could not pick W001 endpoint {endpoint}.");
            }
            File.AppendAllText(Report,"PASS: Socket View frames both ends of W001 and the wire picker resolves both.\n");
            ScreenCapture.CaptureScreenshot("Logs/SparkLevel88SocketView.png");
            await Task.Delay(500);
            process.ArenaView();
            await VerifyObjectives(manager,process);
        }
        catch (Exception ex) { File.AppendAllText(Report,"FAIL: "+ex+"\n"); }
        finally
        {
            Debug.unityLogger.filterLogType = previousLogFilter;
            Time.timeScale = previousTimeScale;
            Application.runInBackground = previous;
            EditorApplication.ExitPlaymode();
        }
    }
    private static async Task VerifyObjectives(LevelGamePlayManager manager, SparkLevel88Process process)
    {
        void Record(string text) => File.AppendAllText(Report,text+"\n");
        var builder = Object.FindFirstObjectByType<SignalWireBuilder>();
        var registry = Object.FindFirstObjectByType<SparkWireRegistry>();
        void Connect(int route)
        {
            if (!builder.TryConnectTerminals(process.pins[route*2],process.pins[route*2+1]))
                throw new InvalidOperationException($"Wire tool rejected W{route+1:000}.");
        }
        async Task Wait(Func<bool> predicate,string description)
        {
            var until=DateTime.UtcNow.AddSeconds(38);
            while(!predicate())
            {
                if(!EditorApplication.isPlaying || DateTime.UtcNow>=until)
                    throw new InvalidOperationException($"{description}: {manager.SatisfiedTargetCount}/6 / {manager.LastEvaluationMessage} / {process.status.text} / {process.measurements.text} / {process.instructions.text}");
                await Task.Delay(40);
            }
        }
        Task At(int count) => Wait(()=>manager.SatisfiedTargetCount==count,$"Waiting for {count}/6");
        const int pressure = 144;
        for(int i=0;i<164;i++)
        {
            if(i!=pressure) Connect(i);
            if(i%10==9) await Task.Yield();
        }
        process.Power();
        await Wait(()=>process.status.text.Contains("163/164"),"Wiring count");
        if(manager.SatisfiedTargetCount!=0 || registry.Wires.Count!=163)
            throw new InvalidOperationException("An incomplete network must start at zero objectives.");
        Connect(pressure);
        Time.timeScale = 5f;
        await At(1);
        Record("All 164 routes accepted by the player wire builder. Incomplete network stayed at 0/6.");
        await Task.Delay(1000);
        process.Calibrate();
        await At(3);
        Record("Atmosphere calibrated and both beds commissioned with measured flow. 3/6.");
        await Wait(()=>process.CanRun,"Atmosphere ready for endurance");
        process.Run();
        await At(4);
        Record("Initial endurance passed. 4/6.");
        foreach(var inject in new Action[]{process.Breakthrough,process.Freeze,process.Leak})
        {
            inject();
            var faultProven=typeof(SparkLevel88Process).GetField("faultProven",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            await Wait(()=>(bool)faultProven.GetValue(process),"Measured disturbance");
            await Task.Delay(200);
            process.Repair();
            await Wait(()=>process.CanRun,"Measured return to healthy atmosphere");
            Record("Measured and repaired "+inject.Method.Name+".");
        }
        await Wait(()=>process.CanRun,"Atmosphere ready for endurance");
        process.Run();
        await At(5);
        process.Arm();
        var wire = registry.Wires.Single(w=>w.TerminalA==process.pins[pressure*2] || w.TerminalB==process.pins[pressure*2]);
        if(!registry.RemoveWire(wire.WireId)) throw new InvalidOperationException("Could not remove W145.");
        await Wait(()=>process.status.text.Contains("W145 INVALID") && process.status.text.Contains("BACKUP LIVE"),"W145 safety inhibition");
        if(manager.SatisfiedTargetCount!=5 || manager.IsCompleted) throw new InvalidOperationException("W145 lost historical objectives.");
        Record("Physical W145 removal invalidated feedback. Backup remained live and 5/6 remained earned.");
        if(!registry.RestoreRemovedWire(wire.WireId)) throw new InvalidOperationException("Could not restore W145.");
        await Wait(()=>process.status.text.Contains("W145 LIVE"),"Fresh W145 feedback");
        process.Calibrate();
        process.ResetSafety();
        if(manager.SatisfiedTargetCount!=5) throw new InvalidOperationException("Reset alone completed the level.");
        await Wait(()=>process.CanRun,"Atmosphere ready for endurance");
        process.Run();
        await At(6);
        await Wait(()=>manager.IsCompleted,"Authoritative campaign completion");
        Record("PASS: Campaign manager completed at 6/6 after fresh post-W145 endurance.");
        ScreenCapture.CaptureScreenshot("Logs/SparkLevel88Complete.png");
        await Task.Delay(500);
    }}
#endif





