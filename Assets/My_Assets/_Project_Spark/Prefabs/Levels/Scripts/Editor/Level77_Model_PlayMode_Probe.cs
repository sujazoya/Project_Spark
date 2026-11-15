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
internal static class Level77ModelPlayModeProbe
{
    private const string Request = "Logs/Level77ModelTest.request";
    private const string Report = "Logs/Level77ModelTest.txt";
    private const string Running = "Codex.Level77.ModelTest";
    private static LevelGamePlayManager manager;
    private static SparkLevel77Process loop;

    static Level77ModelPlayModeProbe()
    {
        EditorApplication.update += CheckRequest;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    private static void CheckRequest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(Request)) return;
        try { File.Delete(Request); }
        catch (IOException) { return; }
        File.WriteAllText(Report, "Level 77 — Breadboard assembly and Reveal the Circuit test\n");
        try
        {
            Check(SceneManager.GetActiveScene().path == SparkLevel77ModelInstaller.ScenePath, "Open MainMenu in Edit Mode.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SparkLevel77ModelInstaller.Prefab) != null)
                SparkLevel77ModelInstaller.Fit();
            SparkLevel77ModelInstaller.Install();
            Record("Installed the fitted assembly in the Level 77 prefab and registered the campaign.");
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
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel77ModelInstaller.Folder + "Level_77.asset");
            Check(definition != null && wireBuilder != null && loader != null, "Level definition, wire builder, or layout loader missing.");
            if (manager.LevelProgression != null) manager.LevelProgression.enabled = false;
            Write(manager, "enforceProgressionAccess", false);
            Write(manager, "autoAdvanceOnCompletion", false);
            Write(manager, "autoEvaluate", true);
            int levelIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), definition);
            Check(levelIndex >= 0 && manager.StartLevel(levelIndex), "Level 77 is missing from the campaign or could not start.");
            await Wait(() => loader.ActiveLayout != null && manager.ActiveLevel == definition, "Level 77 loaded");
            loop = loader.ActiveLayout.GetComponent<SparkLevel77Process>();
            await Wait(() => loop != null && Read<int>(loop, "topology") >= 0,
                "the active loop controller to check the empty circuit");

            var bench = loop.transform.Find("Finishing Workbench");
            var camera = Read<Camera>(wireBuilder, "targetCamera");
            var grid = SparkLevel77ModelInstaller.FindBoard(SceneManager.GetActiveScene());
            var ids = SparkLevel77ModelInstaller.TerminalIds.ToArray();
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
                Vector3 surface = SparkLevel77ModelInstaller.SurfacePoint(grid, foot, bench.up);
                maximumGap = Mathf.Max(maximumGap, Mathf.Abs(Vector3.Dot(surface - foot, bench.up)));
                Check(maximumGap < .001f, "Terminal base does not touch the breadboard: " + id);
                Check(grid.TryGetNearestHole(socket.position, .15f, out var hole, out int column, out int row) &&
                    holes.Add(new Vector2Int(column, row)) &&
                    Vector3.ProjectOnPlane(foot - hole, bench.up).magnitude < .001f,
                    "Terminal does not sit over its own wire hole: " + id);
            }
            Check(holes.Count == 176, "Expected 176 breadboard sockets.");
            Check(highestFoot - lowestFoot < .010f, $"Breadboard seating varies by {highestFoot-lowestFoot:F5} Unity units.");
            Record($"176/176 terminal feet touch the breadboard on distinct wire holes. Maximum seating error: {maximumGap:F6} Unity units.");
            var labels = bench.GetComponentsInChildren<TextMesh>();
            Check(labels.Length == 182 && Array.TrueForAll(labels, label => label.transform.localScale == Vector3.one * .20f),
                "TextMesh labels must use a local scale of 0.20.");
            Record("All 182 TextMesh labels use 0.20 local scale.");
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
            Record("176/176 sockets picked by the real Wire tool from the active game camera.");
            Record("Sockets overlapped by Level 77 buttons in the tested camera view: "+(covered.Count==0?"none":string.Join(", ",covered))+".");
            void Connect(int from, int to, WirePolarity polarity = WirePolarity.Neutral)
            {
                Check(wireBuilder.BeginWire(pins[ids[from]], polarity), "Wire tool could not start at " + ids[from]);
                Check(wireBuilder.TryEndWire(camera.WorldToScreenPoint(pins[ids[to]].transform.position)),
                    "Wire tool could not finish " + ids[from] + " to " + ids[to]);
            }






            Check(covered.Count==0,"Move the control overlay away from these terminals: "+string.Join(", ",covered));
            var map=SparkLevel77Process.Wires;
            const int feedback=54;
            await Click(loop.powerButton);
            for(int i=0;i<map.Length;i++)
            {
                if(i==feedback)continue;
                var (from,to)=map[i];
                Connect(from,to,from==0?WirePolarity.Positive:from==2?WirePolarity.Negative:WirePolarity.Neutral);
            }
            await Wait(()=>loop.statusText.text.Contains("W055"),"missing direct barrier feedback");
            Check(!loop.loadButton.interactable&&manager.SatisfiedTargetCount==0,"Optical readiness and spectral telemetry bypassed W055.");
            Record("Connected the other 113 real wires; optical health and telemetry cannot substitute for W055.");
            Connect(map[feedback].a,map[feedback].b);
            await At(1);
            Time.captureDeltaTime=.10f;
            camera.enabled=false;
            var plant=Read<SparkLevel77Plant>(loop,"plant");
            bool Flag(string key)=>Read<bool>(loop,key);
            async Task Load()
            {
                await Click(loop.loadButton);
                await Click(loop.inspectButton);
            }
            async Task IntoC(bool first=false)
            {
                await Click(loop.bulkButton);
                if(first)
                {
                    await Wait(()=>plant.Force[0]>.55f&&plant.Eddy<108,"bulk removal");
                    Capture(bench,pins,"Running",false);
                }
                await Click(loop.transferBButton);
                await Click(loop.softButton);
                if(first)await At(3);
                await Click(loop.transferCButton);
                await Click(loop.barrierButton);
            }
            async Task EndC()
            {
                await Wait(()=>Flag("barrierClear"),"spectral endpoint at Station C");
                await Click(loop.unloadButton);
            }
            async Task Clean(int number)
            {
                if(!Flag("protect"))await Click(loop.protectButton);
                await Click(loop.brushButton);
                await Click(loop.megaButton);
                await Click(loop.rinseButton);
                await Click(loop.dryButton);
                await Click(loop.geometryButton);
                await Click(loop.electricalButton);
                await Wait(()=>Read<int>(loop,"acceptedBatches")==number,"accepted and isolated fresh wafer");
                Record($"Wafer {number}: Cu {plant.WorstCopper:F3} nm, barrier {plant.WorstBarrier:F3} nm, dishing {plant.WorstDishing:F3} nm, erosion {plant.WorstErosion:F3} nm, leakage {plant.Leakage:F3}.");
            }
            async Task OpticalRecovery()
            {
                await Click(loop.recalButton);
                await Click(loop.verifyButton);
                await Click(loop.resetButton);
                await Click(loop.unloadButton);
            }

            await Click(loop.flowButton);
            await Click(loop.drivesButton);
            await Click(loop.padsButton);
            await Click(loop.calCuButton);
            await Click(loop.calBarrierButton);
            await Load();
            await At(2);
            await IntoC(true);await EndC();await Clean(1);await At(4);

            await Load();await IntoC();
            await Click(loop.foulButton);
            await Wait(()=>Flag("foulingPassed")&&loop.contacts[0].IsOpen,"fouled viewport inhibits polish");
            Check(Read<float>(loop,"viewportFault")<.77f,"Viewport fault did not degrade measurement.");
            await Click(loop.restoreButton);
            await OpticalRecovery();

            await Load();await IntoC();
            await Click(loop.driftButton);
            await Wait(()=>Flag("selectivityPassed")&&loop.contacts[0].IsOpen,"selectivity fault inhibits polish");
            Check(Read<float>(loop,"selectivityFault")<.64f,"Selectivity fault did not affect barrier removal.");
            await Click(loop.restoreButton);
            await Click(loop.resetButton);
            await Click(loop.unloadButton);

            await Load();await IntoC();await EndC();
            await Click(loop.corrodeButton);
            await Wait(()=>Flag("corrosionPassed"),"unprotected wafer corrodes");
            Check(Read<float>(loop,"corrosionFault")>.15f&&plant.Corrosion>.15f,"Corrosion did not affect the wafer.");
            await Click(loop.restoreButton);
            await Click(loop.resetButton);
            await Load();await IntoC();await EndC();await Clean(2);await At(5);
            Record("Viewport fouling, selectivity drift and post-CMP corrosion triggered separate measured faults. A fresh wafer passed after recovery.");

            await Load();await IntoC();await Click(loop.armButton);
            Check(loop.contacts[0].IsClosed&&plant.Rpm[2]>45&&plant.Force[2]>.52f,"Stage C was not actively polishing before W055 removal.");
            Check(wirePaths!=null&&wirePaths.UndoLastWire(),"Wire history could not remove W055.");
            await Wait(()=>loop.statusText.text.Contains("BARRIER OPTICAL FEEDBACK INVALID")&&loop.contacts[0].IsOpen,"W055 optical invalidation and relay open");
            await Wait(()=>manager.ElectricalSolver.LatestSolveResult.TryGetVoltage(pins["POLISH_ENABLE"],pins["PLC_GND"],out float v)&&v<.5f,"polish permit below 0.5 V");
            Check(!loop.barrierButton.interactable&&!loop.resetButton.interactable&&!Flag("validOptical")&&Flag("feedbackObserved")&&manager.SatisfiedTargetCount==5,
                "W055 must invalidate optics, trip protection and retain the first five objectives.");
            await Wait(()=>plant.Rpm[2]<6&&plant.Lift[2]>.96f&&plant.Force[2]<.025f,"platen stopped and carrier retracted");
            float total=plant.TotalRemoved,t0=Read<float>(loop,"elapsed");
            await Wait(()=>Read<float>(loop,"elapsed")>t0+.35f,"no removal after stop");
            Check(plant.TotalRemoved==total,"Material removal continued after the trip.");
            Record($"W055 removed during Stage C at {Read<float>(loop,"rpmAtTrip"):F1} RPM and force {Read<float>(loop,"forceAtTrip"):F2}. Rail below 0.5 V, carrier retracted, removal stopped and 5/6 remained.");
            Capture(bench,pins,"SensorFault",false);
            camera.enabled=true;
            Connect(map[feedback].a,map[feedback].b);
            camera.enabled=false;
            await OpticalRecovery();
            await Load();await IntoC();await EndC();await Clean(3);
            await Click(loop.certifyButton);
            await At(6);
            await Wait(()=>manager.IsCompleted,"campaign completion");
            Record("6/6 — Campaign manager reports Completed after W055 reconnection, optical calibration, explicit reset and a fresh wafer passing isolation tests.");
            Capture(bench,pins,"Verified",false);
        }
        catch (Exception error)
        {
            Record("FAIL: " + error);
            if (loop != null) Record("Control panel: " + loop.statusText.text + " | " + loop.meterText.text + " | " + loop.detailText.text);
            if (loop != null) Record($"Electrical state: 5 V request {Read<bool>(loop, "power")}, solved rails {Read<bool>(loop, "rails")}, e-stop loop {Read<bool>(loop, "safety")}, optical valid {Read<bool>(loop, "validOptical")}, first wiring issue {Read<string>(loop, "wiring")}.");
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
            "Missing Level 77 action on " + button.name);
        button.onClick.Invoke();
    }

    private static void Capture(Transform bench, Dictionary<string, SparkTerminal> pins, string name, bool overhead)
    {
        var view = new GameObject("Level 77 test camera");
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
            File.WriteAllBytes("Logs/Level77_" + name + ".png", snapshot.EncodeToPNG());
            if (!overhead) return;
            var rows = new List<string> { "Terminal,X,Y" };

            foreach (var pair in pins)
            {
                var p = camera.WorldToViewportPoint(pair.Value.transform.position);
                rows.Add($"{pair.Key},{Mathf.RoundToInt(p.x * width)},{Mathf.RoundToInt((1f - p.y) * height)}");
            }
            File.WriteAllLines("Logs/Level77_Pins.csv", rows);
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
        var timeout = DateTime.UtcNow.AddSeconds(77);
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


