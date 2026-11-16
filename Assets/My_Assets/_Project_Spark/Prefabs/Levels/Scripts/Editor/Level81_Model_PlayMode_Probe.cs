#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.EditorTools;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

[InitializeOnLoad]
internal static class Level81ModelPlayModeProbe
{
    private const string Request = "Logs/Level81ModelTest.request";
    private const string Report = "Logs/Level81ModelTest.txt";
    private const string Running = "Codex.Level81.ModelTest";
    private static LevelGamePlayManager manager;
    private static SparkLevel81Process loop;

    static Level81ModelPlayModeProbe()
    {
        EditorApplication.update += CheckRequest;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    private static void CheckRequest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(Request)) return;
        try { File.Delete(Request); }
        catch (IOException) { return; }
        File.WriteAllText(Report, "Level 81 — Breadboard assembly and Outlast the Bitstorm test\n");
        try
        {
            Check(SceneManager.GetActiveScene().path == SparkLevel81ModelInstaller.ScenePath, "Open MainMenu in Edit Mode.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SparkLevel81ModelInstaller.Prefab) != null)
                SparkLevel81ModelInstaller.Fit();
            SparkLevel81ModelInstaller.Install();
            Record("Installed the fitted assembly in the Level 81 prefab and registered the campaign.");
            SessionState.SetBool(Running, true);
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error) { Record("FAIL: " + error); }
    }

    private static void StateChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(Running, false)) return;
        if (state == PlayModeStateChange.EnteredPlayMode) Run();
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            SessionState.EraseBool(Running);
            Record("Returned to Edit Mode.");
        }
    }

    private static async void Run()
    {
        LogType logFilter = Debug.unityLogger.filterLogType;
        int vsync = QualitySettings.vSyncCount;
        bool runInBackground = Application.runInBackground;
        try
        {
            Debug.unityLogger.filterLogType = LogType.Warning;
            Application.runInBackground = true;
            Time.timeScale = 4f;
            QualitySettings.vSyncCount = 0;
            Record("Entered Play Mode.");
            await Wait(() => Object.FindFirstObjectByType<LevelGamePlayManager>() != null, "MainMenu manager");
            manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
            var loader = Object.FindFirstObjectByType<SparkLevelLayoutLoader>();
            var wireBuilder = Object.FindFirstObjectByType<SignalWireBuilder>();
            var wirePaths = Object.FindFirstObjectByType<SignalPath_Manager>();
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel81ModelInstaller.Folder + "Level_81.asset");
            Check(definition != null && wireBuilder != null && loader != null, "Level definition, wire builder, or layout loader missing.");
            if (manager.LevelProgression != null) manager.LevelProgression.enabled = false;
            Write(manager, "enforceProgressionAccess", false);
            Write(manager, "autoAdvanceOnCompletion", false);
            Write(manager, "autoEvaluate", true);
            int levelIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), definition);
            Check(levelIndex >= 0 && manager.StartLevel(levelIndex), "Level 81 is missing from the campaign or could not start.");
            await Wait(() => loader.ActiveLayout != null && manager.ActiveLevel == definition, "Level 81 loaded");
            loop = loader.ActiveLayout.GetComponent<SparkLevel81Process>();
            await Wait(() => loop != null && Read<int>(loop, "topology") >= 0,
                "the active loop controller to check the empty circuit");

            var bench = loop.transform.Find("Reliability Workbench");
            var camera = Read<Camera>(wireBuilder, "targetCamera");
            var grid = SparkLevel81ModelInstaller.FindBoard(SceneManager.GetActiveScene());
            var ids = SparkLevel81ModelInstaller.TerminalIds.ToArray();
            var pins = new Dictionary<string, SparkTerminal>();
            var holes = new HashSet<Vector2Int>();
            float maximumGap = 0;
            float lowestFoot = float.PositiveInfinity, highestFoot = float.NegativeInfinity;
            Physics.SyncTransforms();
            foreach (string id in ids)
            {
                Check(manager.SceneBindings.TryGetTerminal(id, out var terminal), "Missing " + id);
                pins.Add(id, terminal);
                var socket = terminal.transform;
                Vector3 foot = socket.TransformPoint(Vector3.down);
                float height = Vector3.Dot(foot, bench.up);
                lowestFoot = Mathf.Min(lowestFoot, height);
                highestFoot = Mathf.Max(highestFoot, height);
                Vector3 surface = SparkLevel81ModelInstaller.SurfacePoint(grid, foot, bench.up);
                maximumGap = Mathf.Max(maximumGap, Mathf.Abs(Vector3.Dot(surface - foot, bench.up)));
                Check(maximumGap < .001f, "Terminal base does not touch the breadboard: " + id);
                Check(grid.TryGetNearestHole(socket.position, .15f, out var hole, out int column, out int row) &&
                    holes.Add(new Vector2Int(column, row)) &&
                    Vector3.ProjectOnPlane(foot - hole, bench.up).magnitude < .001f,
                    "Terminal does not sit over its own wire hole: " + id);
            }
            Check(holes.Count == 200, "Expected 200 breadboard sockets.");
            Check(highestFoot - lowestFoot < .010f, $"Breadboard seating varies by {highestFoot-lowestFoot:F5} Unity units.");
            Record($"200/200 terminal feet touch the breadboard on distinct wire holes. Maximum seating error: {maximumGap:F6} Unity units.");
            var labels = bench.GetComponentsInChildren<TextMesh>();
            Check(labels.Length == 206 && Array.TrueForAll(labels, label => label.transform.localScale == Vector3.one * .20f),
                "TextMesh labels must use a local scale of 0.20.");
            Record("All 206 TextMesh labels use 0.20 local scale.");
            Capture(bench, pins, "Unwired", true);
            var covered = new List<string>();
            foreach (var pair in pins)
            {
                Vector3 point = camera.WorldToScreenPoint(pair.Value.transform.position);
                Check(point.z > 0 && point.x >= 0 && point.y >= 0 && point.x < camera.pixelWidth && point.y < camera.pixelHeight,
                    $"Terminal outside the active camera: {pair.Key} at {point}; camera {camera.pixelWidth}x{camera.pixelHeight}");
                Check(ReferenceEquals(pick.Invoke(wireBuilder, new object[] { (Vector2)point }), pair.Value),
                    "Wire tool cannot select " + pair.Key + " from the active game camera.");
                if (EventSystem.current)
                {
                    var hits=new List<RaycastResult>();
                    EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);
                    if(hits.Any(hit=>hit.gameObject.transform.IsChildOf(loop.powerButton.transform.parent))) covered.Add(pair.Key);
                }
            }
            Record("200/200 sockets picked by the real Wire tool from the active game camera.");
            Record("Sockets overlapped by Level 81 buttons in the tested camera view: "+(covered.Count==0?"none":string.Join(", ",covered))+".");
            void Connect(int from, int to, WirePolarity polarity = WirePolarity.Neutral)
            {
                Check(wireBuilder.BeginWire(pins[ids[from]], polarity), "Wire tool could not start at " + ids[from]);
                Check(wireBuilder.TryEndWire(camera.WorldToScreenPoint(pins[ids[to]].transform.position)),
                    "Wire tool could not finish " + ids[from] + " to " + ids[to]);
            }






            Check(covered.Count==0,"Move the control overlay away from these terminals: "+string.Join(", ",covered));
            var map=SparkLevel81Process.Wires;
            int surfaces=bench.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials)
                .Where(m=>m!=null&&m.HasProperty("_BumpMap")&&m.GetTexture("_BumpMap")!=null)
                .Select(m=>m.GetTexture("_BumpMap")).Distinct().Count();
            Check(surfaces==12,$"Expected twelve authored surface families; found {surfaces}.");
            Record("Twelve authored PBR surface families applied.");
            const int feedback=93;
            await Click(loop.powerButton);
            for(int i=0;i<map.Length;i++)
            {
                if(i==feedback)continue;
                var (from,to)=map[i];
                Connect(from,to,from==0?WirePolarity.Positive:from==2?WirePolarity.Negative:WirePolarity.Neutral);
            }
            await Wait(()=>loop.statusText.text.Contains("W094"),"missing direct ECC heartbeat");
            Check(!loop.initButton.interactable&&manager.SatisfiedTargetCount==0,"Diagnostic telemetry bypassed W094.");
            Record("Connected 129 wires. W093, W095 and supervisory telemetry could not replace W094.");
            Connect(map[feedback].a,map[feedback].b);
            await At(1);
            Time.captureDeltaTime=.10f;
            camera.enabled=false;
            var plant=Read<SparkLevel81Plant>(loop,"plant");
            bool Flag(string key)=>Read<bool>(loop,key);
            async Task RunMission(int count)
            {
                int before=plant.Committed;
                await Click(loop.missionButton);
                await Wait(()=>Read<int>(loop,"acceptedMissions")==count,"three live mission phases");
                Check(plant.Committed==before+3&&Flag("missionComplete"),"A mission skipped or double-counted a command.");
                Record($"Mission {count}: navigation, maneuver and data return committed through the qualified relay; {plant.Committed} total safe commits.");
            }
            async Task Recover()
            {
                await Click(loop.diagnoseButton);await Click(loop.restoreButton);
                await Click(loop.revalidateButton);await Click(loop.resetButton);
            }

            await Click(loop.initButton);await Click(loop.singleButton);await Click(loop.scrubButton);await At(2);
            Check(plant.SingleVerified&&plant.Corrections==1&&plant.ValidateMemory(),"Single-bit repair failed verified writeback.");
            Record("Both banks initialized to 256 SECDED words. One injected bit repaired with verified writeback.");
            await Click(loop.tmrButton);await Click(loop.laneButton);
            await Wait(()=>Flag("laneVerified"),"majority masked a lane fault");
            Check(plant.FailedLane==0&&plant.Vote==plant.Reference,"Single faulty lane was not masked.");
            await Click(loop.watchButton);await Click(loop.opticalButton);await At(3);
            await RunMission(1);await At(4);
            Capture(bench,pins,"Running",false);

            await Click(loop.doubleButton);
            await Wait(()=>Flag("memoryPassed")&&plant.QuarantineCount==1,"SECDED double bit quarantined");
            Check(!plant.Read(128,out _)&&!loop.resetButton.interactable,"Quarantined data was accessible or reset could bypass verified restore.");
            await Recover();
            Check(plant.CheckpointVerified&&plant.QuarantineCount==0&&plant.Read(128,out _),"Trusted checkpoint restore failed.");
            int beforeFaults=plant.Committed;
            await Click(loop.commonButton);
            await Wait(()=>Flag("commonPassed"),"independent reference rejected a bad majority");
            Check(plant.Vote!=plant.Reference&&plant.Committed==beforeFaults&&plant.Blocked>0,"An unsafe majority was released.");
            await Recover();
            await Click(loop.packetsButton);
            await Wait(()=>Flag("packetPassed"),"CRC, gap, expired, duplicate and replay cases rejected");
            Check(plant.CorruptFrames>0&&plant.Gaps>0&&plant.Replays>0&&plant.Duplicates>0&&plant.ExpiredFrames>0,"Packet faults were not distinguished.");
            await Recover();
            await RunMission(2);await At(5);
            Record("Double-bit quarantine/restore, wrong majority and all five packet error classes passed with no unsafe commit.");

            await Click(loop.missionButton);await Click(loop.armButton);
            Check(Read<int>(loop,"phase")>0&&loop.contacts[0].IsClosed,"No live command permit before W094 removal.");
            int localAtTrip=Read<int>(loop,"heartbeatSequence"),commitsAtTrip=plant.Committed;
            Check(wirePaths!=null&&wirePaths.UndoLastWire(),"Wire history could not physically remove W094.");
            await Wait(()=>!Flag("trusted")&&loop.contacts[0].IsOpen,"W094 stale feedback and open command relay");
            Check(!loop.missionButton.interactable&&!loop.resetButton.interactable&&Flag("feedbackObserved")&&manager.SatisfiedTargetCount==5,
                "W094 loss failed to latch command hold while preserving history.");
            await Wait(()=>manager.ElectricalSolver.LatestSolveResult.TryGetVoltage(pins["COMMAND_ENABLE"],pins["PLC_GND"],out float v)&&v<.5f,"command permit below 0.5 V");
            await Wait(()=>Read<float>(loop,"holdTravel")>.95f&&Read<int>(loop,"heartbeatSequence")>localAtTrip,"controlled safe hold while local ECC remains active");
            Check(plant.Committed==commitsAtTrip&&Read<int>(loop,"receivedSequence")==Read<int>(loop,"w094Sequence"),"Stale heartbeat or live command escaped safe hold.");
            Record("W094 physically removed while the mission permit was live. Local ECC kept ticking, supervisory feedback went stale, and the command gate fell below 0.5 V with 5/6 retained.");
            Capture(bench,pins,"SensorFault",false);
            camera.enabled=true;
            Connect(map[feedback].a,map[feedback].b);
            camera.enabled=false;
            await Recover();
            Check(Read<int>(loop,"receivedSequence")>Read<int>(loop,"w094Sequence"),"Recovery reused a stale heartbeat.");
            await RunMission(3);await Click(loop.certifyButton);
            await At(6);await Wait(()=>manager.IsCompleted,"campaign completion");
            Record("6/6 — Campaign manager reports Completed after W094 reconnection, fresh ECC, memory/voter/watchdog/data requalification and a fresh three-phase mission.");
            Capture(bench,pins,"Verified",false);
        }
        catch (Exception error)
        {
            Record("FAIL: " + error);
            if (loop != null) Record("Control panel: " + loop.statusText.text + " | " + loop.meterText.text + " | " + loop.detailText.text);
            if (loop != null) Record($"Electrical state: 5 V request {Read<bool>(loop, "power")}, solved rails {Read<bool>(loop, "rails")}, e-stop loop {Read<bool>(loop, "safety")}, trusted ECC {Read<bool>(loop, "trusted")}, first wiring issue {Read<string>(loop, "wiring")}.");
            if (manager != null) Record($"Evaluation {manager.SatisfiedTargetCount}/{manager.ActiveTargetCount}: {manager.LastEvaluationMessage}");
        }
        finally
        {
            var wireTool=Object.FindFirstObjectByType<SignalWireBuilder>();
            if(wireTool && Read<Camera>(wireTool,"targetCamera") is Camera cam) cam.enabled=true;
            Time.captureDeltaTime = 0;
            Time.timeScale = 1f;
            QualitySettings.vSyncCount = vsync;
            Debug.unityLogger.filterLogType = logFilter;
            Application.runInBackground = runInBackground;
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        }
    }

    private static readonly MethodInfo pick = typeof(SignalWireBuilder).GetMethod("FindTerminalAtScreenPosition",
        BindingFlags.Instance | BindingFlags.NonPublic);

    private static async Task At(int stage)
    {
        await Wait(() => Read<int>(loop, "stage") == stage && manager.SatisfiedTargetCount == stage, $"{stage}/6");
        Record($"{stage}/6 — {loop.statusText.text} | {loop.meterText.text}");
    }

    private static async Task Click(Button button)
    {
        await Wait(() => button.isActiveAndEnabled && button.interactable, button.name + " available");
        Check(button.onClick.GetPersistentEventCount() == 1 && button.onClick.GetPersistentTarget(0) == loop,
            "Missing Level 81 action on " + button.name);
        int pressedFrame=Time.frameCount;
        button.onClick.Invoke();
        await Wait(()=>Time.frameCount>pressedFrame,"next frame after "+button.name);
    }

    private static void Capture(Transform bench, Dictionary<string, SparkTerminal> pins, string name, bool overhead)
    {
        var view = new GameObject("Level 81 test camera");
        var camera = view.AddComponent<Camera>();
        camera.enabled = false;
        view.transform.position = bench.TransformPoint(new Vector3(0, 2.20f, overhead ? -.70f : -1.92f));
        view.transform.rotation = Quaternion.LookRotation(bench.TransformPoint(new Vector3(0, .04f, 0)) -
            view.transform.position, bench.up);
        camera.orthographic = true;
        camera.orthographicSize = bench.TransformVector(new Vector3(0, .90f, 0)).magnitude;
        const int width = 1600, height = 1071;
        var target = new RenderTexture(width, height, 24);
        var snapshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            snapshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            snapshot.Apply();
            File.WriteAllBytes("Logs/Level81_" + name + ".png", snapshot.EncodeToPNG());
            if (!overhead) return;
            var rows = new List<string> { "Terminal,X,Y" };

            foreach (var pair in pins)
            {
                var p = camera.WorldToViewportPoint(pair.Value.transform.position);
                rows.Add($"{pair.Key},{Mathf.RoundToInt(p.x * width)},{Mathf.RoundToInt((1f - p.y) * height)}");
            }
            File.WriteAllLines("Logs/Level81_Pins.csv", rows);
        }
        finally
        {
            RenderTexture.active = previous;
            camera.targetTexture = null;
            Object.Destroy(target);
            Object.Destroy(snapshot);
            Object.Destroy(view);
        }
    }

    private static Task Wait(Func<bool> condition, string step)
    {
        if (condition()) return Task.CompletedTask;
        var completion = new TaskCompletionSource<bool>();
        var timeout = DateTime.UtcNow.AddSeconds(81);
        void Poll()
        {
            try
            {
                Check(EditorApplication.isPlaying, "Play Mode stopped while waiting for " + step);
                if (!condition())
                {
                    Check(DateTime.UtcNow < timeout, "Timed out waiting for " + step);
                    return;
                }
                EditorApplication.update -= Poll;
                completion.SetResult(true);
            }
            catch (Exception error)
            {
                EditorApplication.update -= Poll;
                completion.SetException(error);
            }
        }
        EditorApplication.update += Poll;
        return completion.Task;
    }

    private static FieldInfo Field(object target, string name) =>
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new MissingFieldException(name);
    private static T Read<T>(object target, string name) => (T)Field(target, name).GetValue(target);
    private static void Write(object target, string name, object value) => Field(target, name).SetValue(target, value);
    private static void Record(string message) => File.AppendAllText(Report, message + Environment.NewLine);
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
#endif









