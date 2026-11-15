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
internal static class Level76ModelPlayModeProbe
{
    private const string Request = "Logs/Level76ModelTest.request";
    private const string Report = "Logs/Level76ModelTest.txt";
    private const string Running = "Codex.Level76.ModelTest";
    private static LevelGamePlayManager manager;
    private static SparkLevel76Process loop;

    static Level76ModelPlayModeProbe()
    {
        EditorApplication.update += CheckRequest;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    private static void CheckRequest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(Request)) return;
        try { File.Delete(Request); }
        catch (IOException) { return; }
        File.WriteAllText(Report, "Level 76 — Breadboard assembly and Forge the Connections test\n");
        try
        {
            Check(SceneManager.GetActiveScene().path == SparkLevel76ModelInstaller.ScenePath, "Open MainMenu in Edit Mode.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SparkLevel76ModelInstaller.Prefab) != null)
                SparkLevel76ModelInstaller.Fit();
            SparkLevel76ModelInstaller.Install();
            Record("Installed the fitted assembly in the Level 76 prefab and registered the campaign.");
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
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel76ModelInstaller.Folder + "Level_76.asset");
            Check(definition != null && wireBuilder != null && loader != null, "Level definition, wire builder, or layout loader missing.");
            if (manager.LevelProgression != null) manager.LevelProgression.enabled = false;
            Write(manager, "enforceProgressionAccess", false);
            Write(manager, "autoAdvanceOnCompletion", false);
            Write(manager, "autoEvaluate", true);
            int levelIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), definition);
            Check(levelIndex >= 0 && manager.StartLevel(levelIndex), "Level 76 is missing from the campaign or could not start.");
            await Wait(() => loader.ActiveLayout != null && manager.ActiveLevel == definition, "Level 76 loaded");
            loop = loader.ActiveLayout.GetComponent<SparkLevel76Process>();
            await Wait(() => loop != null && Read<int>(loop, "topology") >= 0,
                "the active loop controller to check the empty circuit");

            var bench = loop.transform.Find("Plating Workbench");
            var camera = Read<Camera>(wireBuilder, "targetCamera");
            var grid = SparkLevel76ModelInstaller.FindBoard(SceneManager.GetActiveScene());
            var ids = SparkLevel76ModelInstaller.TerminalIds.ToArray();
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
                Vector3 surface = SparkLevel76ModelInstaller.SurfacePoint(grid, foot, bench.up);
                maximumGap = Mathf.Max(maximumGap, Mathf.Abs(Vector3.Dot(surface - foot, bench.up)));
                Check(maximumGap < .001f, "Terminal base does not touch the breadboard: " + id);
                Check(grid.TryGetNearestHole(socket.position, .15f, out var hole, out int column, out int row) &&
                    holes.Add(new Vector2Int(column, row)) &&
                    Vector3.ProjectOnPlane(foot - hole, bench.up).magnitude < .001f,
                    "Terminal does not sit over its own wire hole: " + id);
            }
            Check(holes.Count == 170, "Expected 170 breadboard sockets.");
            Check(highestFoot - lowestFoot < .010f, $"Breadboard seating varies by {highestFoot-lowestFoot:F5} Unity units.");
            Record($"170/170 terminal feet touch the breadboard on distinct wire holes. Maximum seating error: {maximumGap:F6} Unity units.");
            var labels = bench.GetComponentsInChildren<TextMesh>();
            Check(labels.Length == 176 && Array.TrueForAll(labels, label => label.transform.localScale == Vector3.one * .20f),
                "TextMesh labels must use a local scale of 0.20.");
            Record("All 176 TextMesh labels use 0.20 local scale.");
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
            Record("170/170 sockets picked by the real Wire tool from the active game camera.");
            Record("Sockets overlapped by Level 76 buttons in the tested camera view: "+(covered.Count==0?"none":string.Join(", ",covered))+".");
            void Connect(int from, int to, WirePolarity polarity = WirePolarity.Neutral)
            {
                Check(wireBuilder.BeginWire(pins[ids[from]], polarity), "Wire tool could not start at " + ids[from]);
                Check(wireBuilder.TryEndWire(camera.WorldToScreenPoint(pins[ids[to]].transform.position)),
                    "Wire tool could not finish " + ids[from] + " to " + ids[to]);
            }






            Check(covered.Count==0,"Move the control overlay away from these terminals: "+string.Join(", ",covered));
            var map=SparkLevel76Process.Wires;
            const int feedback=75;
            await Click(loop.powerButton);
            for(int i=0;i<map.Length;i++)
            {
                if(i==feedback)continue;
                var (from,to)=map[i];
                Connect(from,to,from==0?WirePolarity.Positive:from==2?WirePolarity.Negative:WirePolarity.Neutral);
            }
            await Wait(()=>loop.statusText.text.Contains("W076"),"missing reference potential wiring");
            Check(!loop.loadButton.interactable&&manager.SatisfiedTargetCount==0,"The reference validity status bypassed W076.");
            Record("The other 109 wires were connected, including W084. Adding W076 last with the real Wire tool.");
            Connect(map[feedback].a,map[feedback].b);
            await At(1);
            Time.captureDeltaTime=.12f;
            camera.enabled=false;
            var plant=Read<SparkLevel76Plant>(loop,"plant");

            async Task Load()
            {
                await Click(loop.loadButton);
                await Click(loop.seedButton);
                await Click(loop.prewetButton);
                await Click(loop.clampButton);
            }
            async Task Plate()
            {
                if(!Read<bool>(loop,"zones"))await Click(loop.zonesButton);
                await Click(loop.plateButton);
            }
            async Task Finish(int number)
            {
                await Wait(()=>Read<bool>(loop,"complete")&&plant.Current<.025f,"fill target and current rundown");
                await Click(loop.unloadButton);
                await Click(loop.rinseButton);
                await Click(loop.dryButton);
                await Click(loop.inspectButton);
                await Click(loop.electricalButton);
                await Wait(()=>Read<int>(loop,"acceptedBatches")==number,"accepted copper wafer");
                Check(plant.LeastFill>=.995f&&plant.WorstVoid<=.005f&&plant.Uniformity<=.03f&&plant.TestResistance<=1.05f&&plant.MassError<.001f,
                    "A copper metrology measurement failed.");
                Record($"Wafer {number}: 16/16 regions, least fill {plant.LeastFill:P3}, worst void {plant.WorstVoid:P3}, nonuniformity {plant.Uniformity:P2}, overburden {plant.Overburden:F3}, normalized test resistance {plant.TestResistance:F3}, charge-to-copper balance residual {plant.MassError:F6}.");
            }
            async Task Disturbance(Button action,string flag)
            {
                await Load();
                await Plate();
                await Click(action);
                await Wait(()=>Read<bool>(loop,flag)&&loop.contacts[0].IsOpen,"safety hold from "+action.name);
                await Click(loop.restoreButton);
                await Click(loop.resetButton);
                await Click(loop.unloadButton);
            }

            await Click(loop.calFlowButton);
            await Click(loop.pumpButton);
            await Click(loop.filterButton);
            await Click(loop.degasButton);
            await Click(loop.heatButton);
            await Click(loop.doseButton);
            await Load();
            await At(2);
            await Click(loop.calCurrentButton);
            await Click(loop.calRefButton);
            await Click(loop.waveformButton);
            await Plate();
            await At(3);
            Capture(bench,pins,"Running",false);
            await Finish(1);
            await At(4);

            await Disturbance(loop.contactFaultButton,"contactPassed");
            Record($"Contact fault raised measured resistance to {Read<float>(loop,"resistanceFault"):F3}.");
            await Disturbance(loop.flowFaultButton,"flowPassed");
            Record($"Manifold restriction lowered flow to {Read<float>(loop,"flowAtFault"):F3} and raised pressure to {Read<float>(loop,"pressureAtFault"):F3}.");
            await Disturbance(loop.additiveFaultButton,"additivePassed");
            Record($"Additive drift reduced accelerator concentration to {Read<float>(loop,"additiveAtFault"):F3}.");
            await Load();
            await Plate();
            await Finish(2);
            await At(5);

            await Load();
            await Plate();
            await Click(loop.armButton);
            Check(loop.contacts[0].IsClosed&&plant.Current>.65f,"Live plating current was absent before disconnect.");
            Check(wirePaths!=null&&wirePaths.UndoLastWire(),"Could not remove W076 through wire history.");
            await Wait(()=>loop.statusText.text.Contains("REFERENCE POTENTIAL INVALID")&&loop.contacts[0].IsOpen,"reference invalidation and rectifier relay opening");
            await Wait(()=>manager.ElectricalSolver.LatestSolveResult.TryGetVoltage(pins["RECT_ENABLE"],pins["PLC_GND"],out float v)&&v<.5f,"rectifier permit below 0.5 V");
            Check(!loop.plateButton.interactable&&!loop.resetButton.interactable&&!Read<bool>(loop,"validReference")&&manager.SatisfiedTargetCount==5&&
                Read<float>(loop,"currentAtTrip")>.65f,"W076 did not remove trusted reference feedback while retaining five objectives.");
            await Wait(()=>plant.Current<.0001f,"physical current rundown");
            float stopCopper=plant.MeanCopper;
            float t0=Read<float>(loop,"elapsed");
            await Wait(()=>Read<float>(loop,"elapsed")>t0+.35f,"no deposition after rundown");
            Check(Mathf.Abs(plant.MeanCopper-stopCopper)<.00005f,"Copper continued growing after the rectifier reached zero.");
            Record($"W076 opened with W084 still installed. Rectifier permit below 0.5 V, current decayed from {Read<float>(loop,"currentAtTrip"):F3}, deposition stopped and 5/6 remained.");
            Capture(bench,pins,"SensorFault",false);
            camera.enabled=true;
            Connect(map[feedback].a,map[feedback].b);
            camera.enabled=false;
            await Click(loop.recalButton);
            await Click(loop.verifyButton);
            await Click(loop.resetButton);
            await Click(loop.unloadButton);
            await Load();
            await Plate();
            await Finish(3);
            await Click(loop.certifyButton);
            await At(6);
            await Wait(()=>manager.IsCompleted,"campaign completion");
            Record("6/6 — Campaign manager reports Completed after W076 reconnection, reference calibration, explicit reset and a freshly certified wafer.");
            Capture(bench,pins,"Verified",false);

        }
        catch (Exception error)
        {
            Record("FAIL: " + error);
            if (loop != null) Record("Control panel: " + loop.statusText.text + " | " + loop.meterText.text + " | " + loop.detailText.text);
            if (loop != null) Record($"Electrical state: 5 V request {Read<bool>(loop, "power")}, solved rails {Read<bool>(loop, "rails")}, e-stop loop {Read<bool>(loop, "safety")}, reference valid {Read<bool>(loop, "validReference")}, first wiring issue {Read<string>(loop, "wiring")}.");
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
            "Missing Level 76 action on " + button.name);
        button.onClick.Invoke();
    }

    private static void Capture(Transform bench, Dictionary<string, SparkTerminal> pins, string name, bool overhead)
    {
        var view = new GameObject("Level 76 test camera");
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
            File.WriteAllBytes("Logs/Level76_" + name + ".png", snapshot.EncodeToPNG());
            if (!overhead) return;
            var rows = new List<string> { "Terminal,X,Y" };

            foreach (var pair in pins)
            {
                var p = camera.WorldToViewportPoint(pair.Value.transform.position);
                rows.Add($"{pair.Key},{Mathf.RoundToInt(p.x * width)},{Mathf.RoundToInt((1f - p.y) * height)}");
            }
            File.WriteAllLines("Logs/Level76_Pins.csv", rows);
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
        var timeout = DateTime.UtcNow.AddSeconds(76);
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
