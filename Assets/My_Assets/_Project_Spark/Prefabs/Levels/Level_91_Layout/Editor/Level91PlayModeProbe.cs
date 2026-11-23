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
internal static class Level91PlayModeProbe
{
    const string Request = "Logs/SparkLevel91PlayMode.request";
    const string Report = "Logs/SparkLevel91PlayMode.txt";
    const string Key = "ProjectSpark.Level91.Finalization";

    static Level91PlayModeProbe()
    {
        EditorApplication.update += Poll;
        EditorApplication.playModeStateChanged += Play;
    }

    [MenuItem("Tools/Project Spark/Levels/91/Verify Complete Playthrough")]
    private static void Queue() => File.WriteAllText(Request, "run");

    private static void Poll()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
            EditorApplication.isUpdating || SceneManager.GetActiveScene().name != "MainMenu" ||
            !File.Exists(Request)) return;
        string options = File.ReadAllText(Request);
        File.Delete(Request);
        if (options.Contains("rebuild"))
        {
            try { ProjectSpark.EditorTools.SparkLevel91Builder.RebuildLayout(); }
            catch (Exception ex)
            {
                File.WriteAllText(Report, "Rebuild failed: " + ex);
                return;
            }
        }
        if (options.Contains("register"))
        {
            try
            {
                ProjectSpark.EditorTools.SparkLevel91Builder.BuildLayoutOnly();
                var scene = SceneManager.GetActiveScene();
                if (scene.isDirty && !UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("MainMenu could not be saved before registering Level 91.");
                ProjectSpark.EditorTools.SparkLevel91Builder.Create();
            }
            catch (Exception ex)
            {
                File.WriteAllText(Report, "Registration failed: " + ex);
                return;
            }
        }
        File.WriteAllText(Report,"Level 91 opening Play Mode check\n");
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
                levels.GetArrayElementAtIndex(i).objectReferenceValue is SparkLevelDefinition definition && definition.LevelId == "level_91");
            var settings = new SerializedObject(manager);
            settings.FindProperty("enforceProgressionAccess").boolValue = false;
            settings.ApplyModifiedPropertiesWithoutUndo();
            if (manager.LevelProgression) manager.LevelProgression.enabled = false;
            if (!manager.StartLevel(index)) throw new InvalidOperationException("Cannot start Level 91.");
            await Task.Delay(1800);
            var process = Object.FindFirstObjectByType<SparkLevel91Process>();
            if (!process || manager.ActiveTargetCount != 6 || manager.SatisfiedTargetCount != 0 ||
                process.pins.Length != 364 || !process.status.text.Contains("0/182"))
                throw new InvalidOperationException("Expected six unearned objectives and an unwired 364-contact layout.");
            var art = process.arena.Find("L91_HD_VISUAL_ART");
            var nodes = art.GetComponentsInChildren<Transform>(true);
            int sockets = nodes.Count(node => node.name.StartsWith("SOCKET_P", StringComparison.Ordinal));
            int pivots = nodes.Count(node => node.name.StartsWith("ANIM_", StringComparison.Ordinal));
            if (art.childCount != 99 || sockets != 260 || pivots != 84)
                throw new InvalidOperationException($"Unexpected art: {art.childCount - 3} models, {sockets} sockets, {pivots} pivots.");
            var deck = art.Find("L91_CosmicNavigationQualificationDeck");
            var bounds = deck.GetComponentsInChildren<Renderer>().Select(r=>r.bounds).Aggregate((a,b)=>{a.Encapsulate(b);return a;});
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var coverage = Mathf.Max(bounds.size.x/table.bounds.size.x,bounds.size.z/table.bounds.size.z);
            if (coverage < .96f || coverage > 1.01f || bounds.size.z < 2f || bounds.size.y > 1f)
                throw new InvalidOperationException($"Unexpected table coverage {coverage:P1}");
            File.AppendAllText(Report,$"PASS: Registered 0/6; {process.pins.Length} fixed contacts; {art.childCount - 3} modular models; deck {bounds.size.x:F2} x {bounds.size.z:F2}; limiting-axis table coverage {coverage:P1}.\n");
            var camera = Camera.main;
            if (!camera) throw new InvalidOperationException("Main camera missing");
            ScreenCapture.CaptureScreenshot("Logs/SparkLevel91PlayMode.png");
            await Task.Delay(1000);
            process.SocketView();
            await Task.Delay(1000);
            var wireBuilder = Object.FindFirstObjectByType<SignalWireBuilder>();
            
            Physics.SyncTransforms();
            for (int endpoint = 0; endpoint < 2; endpoint++)
            {
                Vector3 pin = camera.WorldToViewportPoint(process.pins[endpoint].transform.position);
                if (pin.z <= 0 || pin.x < .12f || pin.x > .88f || pin.y < .19f || pin.y > .84f)
                    throw new InvalidOperationException($"Socket View did not frame W001 endpoint {endpoint} at {pin}.");
                var screen = (Vector2)camera.WorldToScreenPoint(process.pins[endpoint].transform.position);
                if (!ReferenceEquals(process.pins[endpoint],wireBuilder.FindStartTerminal(screen)))
                    throw new InvalidOperationException($"Wire tool could not pick W001 endpoint {endpoint}.");
            }
            File.AppendAllText(Report,"PASS: Socket View frames both ends of W001 and the wire picker resolves both.\n");
            ScreenCapture.CaptureScreenshot("Logs/SparkLevel91SocketView.png");
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
    private static async Task VerifyObjectives(LevelGamePlayManager manager, SparkLevel91Process process)
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
        const int feedback = 162;
        for (int i = 0; i < 182; i++)
        {
            if (i != feedback) Connect(i);
            if (i % 10 == 9) await Task.Yield();
        }
        process.Power();
        await Wait(() => process.status.text.Contains("181/182"), "Wiring count");
        if (manager.SatisfiedTargetCount != 0 || registry.Wires.Count != 181)
            throw new InvalidOperationException("Incomplete wiring awarded an objective.");
        Connect(feedback);
        Time.timeScale = 4f;
        await At(1);
        Record("PASS: 182 routes accepted by the player wire builder. Incomplete wiring stayed at 0/6.");

        process.Calibrate();
        await At(2);
        process.Acquire();
        await At(3);
        process.Run();
        await At(4);
        Record("PASS: Timing calibration, four-pulsar acquisition and first complete correction reached 4/6.");
        var type = typeof(SparkLevel91Process);
        bool Flag(string field) => (bool)type.GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(process);
        var recovery = type.GetField("recovery", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        foreach (var inject in new Action[] { process.RaiseBackground, process.DriftClock, process.CorruptEphemeris })
        {
            inject();
            await Wait(() => Flag("faultProven"), "Disturbance " + inject.Method.Name);
            await Task.Delay(650);
            process.Repair();
            await Wait(() => (float)recovery.GetValue(process) <= 0, "Recovery " + inject.Method.Name);
            Record("PASS: Measured and repaired " + inject.Method.Name + ".");
        }
        process.Run();
        await At(5);
        process.Arm();
        var wire = registry.Wires.Single(item => item.TerminalA == process.pins[feedback * 2] ||
            item.TerminalB == process.pins[feedback * 2]);
        if (!registry.RemoveWire(wire.WireId)) throw new InvalidOperationException("W163 removal failed.");
        await Wait(() => Flag("latched") && !Flag("trusted") && Flag("backup") && process.status.text.Contains("BURN INHIBITED"), "W163 protective hold");
        var plant = type.GetField("plant", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(process);
        await Wait(() => !(bool)plant.GetType().GetProperty("BurnActive").GetValue(plant), "Correction inhibition");
        if (manager.SatisfiedTargetCount != 5 || manager.IsCompleted)
            throw new InvalidOperationException("W163 interruption did not preserve 5/6.");
        Record("PASS: W163 loss latched safety, preserved independent feedback and kept 5/6.");
        if (!registry.RestoreRemovedWire(wire.WireId)) throw new InvalidOperationException("W163 restore failed.");
        await Wait(() => Flag("trusted"), "W163 signal");
        await Task.Delay(350);
        process.Calibrate();
        process.ResetSafety();
        if (manager.SatisfiedTargetCount != 5 || Flag("latched") || !Flag("reset"))
            throw new InvalidOperationException("W163 reset did not restore authorization at 5/6.");
        process.Run();
        await At(6);
        await Wait(() => manager.IsCompleted, "Campaign completion");
        Record("PASS: Campaign manager completed 6/6 after a fresh acquisition and correction following W163 recovery.");
        ScreenCapture.CaptureScreenshot("Logs/SparkLevel91Complete.png");
        await Task.Delay(400);
    }
}
#endif








