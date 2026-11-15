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
internal static class Level71ModelPlayModeProbe
{
    private const string Request = "Logs/Level71ModelTest.request";
    private const string Report = "Logs/Level71ModelTest.txt";
    private const string Running = "Codex.Level71.ModelTest";
    private static LevelGamePlayManager manager;
    private static SparkLevel71Process loop;

    static Level71ModelPlayModeProbe()
    {
        EditorApplication.update += CheckRequest;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    private static void CheckRequest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(Request)) return;
        try { File.Delete(Request); }
        catch (IOException) { return; }
        File.WriteAllText(Report, "Level 71 — Breadboard assembly and Etch the Future test\n");
        try
        {
            Check(SceneManager.GetActiveScene().path == SparkLevel71ModelInstaller.ScenePath, "Open MainMenu in Edit Mode.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SparkLevel71ModelInstaller.Prefab) != null)
                SparkLevel71ModelInstaller.Fit();
            SparkLevel71ModelInstaller.Install();
            Record("Installed the fitted assembly in the Level 71 prefab and registered the campaign.");
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
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel71ModelInstaller.Folder + "Level_71.asset");
            Check(definition != null && wireBuilder != null && loader != null, "Level definition, wire builder, or layout loader missing.");
            if (manager.LevelProgression != null) manager.LevelProgression.enabled = false;
            Write(manager, "enforceProgressionAccess", false);
            Write(manager, "autoAdvanceOnCompletion", false);
            Write(manager, "autoEvaluate", true);
            int levelIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), definition);
            Check(levelIndex >= 0 && manager.StartLevel(levelIndex), "Level 71 is missing from the campaign or could not start.");
            await Wait(() => loader.ActiveLayout != null && manager.ActiveLevel == definition, "Level 71 loaded");
            loop = loader.ActiveLayout.GetComponent<SparkLevel71Process>();
            await Wait(() => loop != null && Read<int>(loop, "topology") >= 0,
                "the active loop controller to check the empty circuit");

            var bench = loop.transform.Find("Plasma Workbench");
            var camera = Read<Camera>(wireBuilder, "targetCamera");
            var grid = SparkLevel71ModelInstaller.FindBoard(SceneManager.GetActiveScene());
            var ids = SparkLevel71ModelInstaller.TerminalIds.ToArray();
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
                Vector3 surface = SparkLevel71ModelInstaller.SurfacePoint(grid, foot, bench.up);
                maximumGap = Mathf.Max(maximumGap, Mathf.Abs(Vector3.Dot(surface - foot, bench.up)));
                Check(maximumGap < .001f, "Terminal base does not touch the breadboard: " + id);
                Check(grid.TryGetNearestHole(socket.position, .15f, out var hole, out int column, out int row) &&
                    holes.Add(new Vector2Int(column, row)) &&
                    Vector3.ProjectOnPlane(foot - hole, bench.up).magnitude < .001f,
                    "Terminal does not sit over its own wire hole: " + id);
            }
            Check(holes.Count == 140, "Expected 140 breadboard sockets.");
            Check(highestFoot - lowestFoot < .010f, $"Breadboard seating varies by {highestFoot-lowestFoot:F5} Unity units.");
            Record($"140/140 terminal feet touch the breadboard on distinct wire holes. Maximum seating error: {maximumGap:F6} Unity units.");
            var labels = bench.GetComponentsInChildren<TextMesh>();
            Check(labels.Length == 146 && Array.TrueForAll(labels, label => label.transform.localScale == Vector3.one * .20f),
                "TextMesh labels must use a local scale of 0.20.");
            Record("All 146 TextMesh labels use 0.20 local scale.");
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
            Record("140/140 sockets picked by the real Wire tool from the active game camera.");
            Record("Sockets overlapped by Level 71 buttons in the tested camera view: "+(covered.Count==0?"none":string.Join(", ",covered))+".");
            void Connect(int from, int to, WirePolarity polarity = WirePolarity.Neutral)
            {
                Check(wireBuilder.BeginWire(pins[ids[from]], polarity), "Wire tool could not start at " + ids[from]);
                Check(wireBuilder.TryEndWire(camera.WorldToScreenPoint(pins[ids[to]].transform.position)),
                    "Wire tool could not finish " + ids[from] + " to " + ids[to]);
            }






            Check(covered.Count==0,"Move the control overlay away from these terminals: "+string.Join(", ",covered));
            var map=SparkLevel71Process.Wires;
            await Click(loop.powerButton);
            for(int i=0;i<map.Length-1;i++)
            {
                var (from,to)=map[i];
                Connect(from,to,from==0?WirePolarity.Positive:from==2?WirePolarity.Negative:WirePolarity.Neutral);
            }
            await Wait(()=>loop.statusText.text.Contains("W90"),"missing optical feedback circuit");
            Record("Connected first 89 wires. "+loop.statusText.text);
            Check(!loop.loadButton.interactable&&manager.SatisfiedTargetCount==0,"Wafer transfer allowed without complete wiring.");
            Connect(map[89].a,map[89].b);
            await At(1);
            Time.captureDeltaTime=.12f;
            camera.enabled=false;

            var plant=Read<SparkLevel71EtchPlant>(loop,"plant");
            async Task Load()
            {
                await Click(loop.loadButton);
                await Click(loop.clampButton);
            }
            async Task Ignite()
            {
                await Click(loop.gasButton);
                await Click(loop.rfButton);
                await Click(loop.igniteButton);
                await Wait(()=>ReadProperty("PlasmaReady"),"process stability");
            }
            bool ReadProperty(string name)=> (bool)typeof(SparkLevel71Process).GetProperty(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(loop);
            async Task FinishWafer(int completed)
            {
                await Click(loop.etchButton);
                await Click(loop.metrologyButton);
                await Wait(()=>Read<int>(loop,"acceptedBatches")==completed,"accepted wafer metrology");
                Record($"Wafer {completed}: mean film {plant.MeanFilm:F2} nm, nonuniformity {plant.NonUniformity:P2}, substrate damage {plant.Damage:F3}.");
            }

            await Click(loop.calPressureButton);
            await Click(loop.calGasButton);
            await Click(loop.calRfButton);
            await Click(loop.calOpticalButton);
            await Click(loop.isolationButton);
            await Click(loop.backingButton);
            await Click(loop.turboButton);
            await Load();
            await At(2);
            await Click(loop.chillerButton);
            await Click(loop.pressurePidButton);
            await Click(loop.matchButton);
            await Ignite();
            await At(3);
            Check(plant.Delivered>.40f&&plant.Reflection<.075f&&plant.Activity>.53f,"Plasma stage did not represent a stable process.");
            await Click(loop.endpointButton);
            Capture(bench,pins,"Running",false);
            await FinishWafer(1);
            await At(4);

            await Ignite();
            await Click(loop.leakButton);
            await Wait(()=>Read<bool>(loop,"leakPassed"),"vacuum pressure excursion and recovery");
            Record($"Leak pressure peaked at {Read<float>(loop,"peakP"):F2} Pa and recovered to {plant.Pressure:F2} Pa.");
            await Click(loop.driftButton);
            await Wait(()=>Read<bool>(loop,"driftPassed"),"RF reflection excursion and recovery");
            Record($"Drift reflection peaked at {Read<float>(loop,"peakReflection"):P2} and recovered to {plant.Reflection:P2}.");
            await Click(loop.coolingButton);
            await Wait(()=>Read<bool>(loop,"coolingPassed")&&loop.contacts[0].IsOpen,"physical thermal trip and RF isolation");
            Check(plant.Temperature>50&&!loop.etchButton.interactable,"Thermal test bypassed its RF interlock.");
            await Click(loop.resetButton);
            await Load();
            await Ignite();
            await FinishWafer(2);
            await At(5);

            await Load();
            await Ignite();
            await Click(loop.etchButton);
            await Click(loop.armButton);
            Check(loop.contacts[0].IsClosed,"Arming tripped the RF enable before actual optical disconnection.");
            Check(wirePaths!=null&&wirePaths.UndoLastWire(),"Could not disconnect W90 through wire history.");
            await Wait(()=>loop.statusText.text.Contains("OPTICAL FEEDBACK INVALID")&&loop.contacts[0].IsOpen,
                "optical invalidation and physical RF contact opening");
            await Wait(()=>manager.ElectricalSolver.LatestSolveResult.TryGetVoltage(pins["RF_ENABLE"],pins["PLC_GND"],out float v)&&v<.5f,"solved RF enable below 0.5 V");
            Check(!loop.resetButton.interactable&&!loop.etchButton.interactable&&!Read<bool>(loop,"validOptical")&&manager.SatisfiedTargetCount==5,
                "W90 removal did not preserve 5/6 or inhibit the endpoint.");
            Record("W90 physically removed. RF enable deenergized, optical samples invalid, restart inhibited, and historical 5/6 retained.");
            Capture(bench,pins,"SensorFault",false);
            camera.enabled=true;
            Connect(map[89].a,map[89].b);
            camera.enabled=false;
            await Click(loop.recalButton);
            await Click(loop.resetButton);
            await Load();
            await Ignite();
            await FinishWafer(3);
            await Click(loop.certifyButton);
            await At(6);
            await Wait(()=>manager.IsCompleted,"campaign completion");
            Check(plant.BalanceError<.0001f,"Chamber gas balance residual exceeded tolerance.");
            Record("6/6 — Campaign manager reports Completed after W90 reconnection, optical recalibration, safety reset and a freshly certified wafer.");
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
            "Missing Level 71 action on " + button.name);
        button.onClick.Invoke();
    }

    private static void Capture(Transform bench, Dictionary<string, SparkTerminal> pins, string name, bool overhead)
    {
        var view = new GameObject("Level 71 test camera");
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
            File.WriteAllBytes("Logs/Level71_" + name + ".png", snapshot.EncodeToPNG());
            if (!overhead) return;
            var rows = new List<string> { "Terminal,X,Y" };

            foreach (var pair in pins)
            {
                var p = camera.WorldToViewportPoint(pair.Value.transform.position);
                rows.Add($"{pair.Key},{Mathf.RoundToInt(p.x * width)},{Mathf.RoundToInt((1f - p.y) * height)}");
            }
            File.WriteAllLines("Logs/Level71_Pins.csv", rows);
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
        var timeout = DateTime.UtcNow.AddSeconds(75);
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
