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
internal static class Level80ModelPlayModeProbe
{
    private const string Request = "Logs/Level80ModelTest.request";
    private const string Report = "Logs/Level80ModelTest.txt";
    private const string Running = "Codex.Level80.ModelTest";
    private static LevelGamePlayManager manager;
    private static SparkLevel80Process loop;

    static Level80ModelPlayModeProbe()
    {
        EditorApplication.update += CheckRequest;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    private static void CheckRequest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(Request)) return;
        try { File.Delete(Request); }
        catch (IOException) { return; }
        File.WriteAllText(Report, "Level 80 — Breadboard assembly and Command the Light test\n");
        try
        {
            Check(SceneManager.GetActiveScene().path == SparkLevel80ModelInstaller.ScenePath, "Open MainMenu in Edit Mode.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SparkLevel80ModelInstaller.Prefab) != null)
                SparkLevel80ModelInstaller.Fit();
            SparkLevel80ModelInstaller.Install();
            Record("Installed the fitted assembly in the Level 80 prefab and registered the campaign.");
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
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel80ModelInstaller.Folder + "Level_80.asset");
            Check(definition != null && wireBuilder != null && loader != null, "Level definition, wire builder, or layout loader missing.");
            if (manager.LevelProgression != null) manager.LevelProgression.enabled = false;
            Write(manager, "enforceProgressionAccess", false);
            Write(manager, "autoAdvanceOnCompletion", false);
            Write(manager, "autoEvaluate", true);
            int levelIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), definition);
            Check(levelIndex >= 0 && manager.StartLevel(levelIndex), "Level 80 is missing from the campaign or could not start.");
            await Wait(() => loader.ActiveLayout != null && manager.ActiveLevel == definition, "Level 80 loaded");
            loop = loader.ActiveLayout.GetComponent<SparkLevel80Process>();
            await Wait(() => loop != null && Read<int>(loop, "topology") >= 0,
                "the active loop controller to check the empty circuit");

            var bench = loop.transform.Find("Photonic Workbench");
            var camera = Read<Camera>(wireBuilder, "targetCamera");
            var grid = SparkLevel80ModelInstaller.FindBoard(SceneManager.GetActiveScene());
            var ids = SparkLevel80ModelInstaller.TerminalIds.ToArray();
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
                Vector3 surface = SparkLevel80ModelInstaller.SurfacePoint(grid, foot, bench.up);
                maximumGap = Mathf.Max(maximumGap, Mathf.Abs(Vector3.Dot(surface - foot, bench.up)));
                Check(maximumGap < .001f, "Terminal base does not touch the breadboard: " + id);
                Check(grid.TryGetNearestHole(socket.position, .15f, out var hole, out int column, out int row) &&
                    holes.Add(new Vector2Int(column, row)) &&
                    Vector3.ProjectOnPlane(foot - hole, bench.up).magnitude < .001f,
                    "Terminal does not sit over its own wire hole: " + id);
            }
            Check(holes.Count == 194, "Expected 194 breadboard sockets.");
            Check(highestFoot - lowestFoot < .010f, $"Breadboard seating varies by {highestFoot-lowestFoot:F5} Unity units.");
            Record($"194/194 terminal feet touch the breadboard on distinct wire holes. Maximum seating error: {maximumGap:F6} Unity units.");
            var labels = bench.GetComponentsInChildren<TextMesh>();
            Check(labels.Length == 200 && Array.TrueForAll(labels, label => label.transform.localScale == Vector3.one * .20f),
                "TextMesh labels must use a local scale of 0.20.");
            Record("All 200 TextMesh labels use 0.20 local scale.");
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
            Record("194/194 sockets picked by the real Wire tool from the active game camera.");
            Record("Sockets overlapped by Level 80 buttons in the tested camera view: "+(covered.Count==0?"none":string.Join(", ",covered))+".");
            void Connect(int from, int to, WirePolarity polarity = WirePolarity.Neutral)
            {
                Check(wireBuilder.BeginWire(pins[ids[from]], polarity), "Wire tool could not start at " + ids[from]);
                Check(wireBuilder.TryEndWire(camera.WorldToScreenPoint(pins[ids[to]].transform.position)),
                    "Wire tool could not finish " + ids[from] + " to " + ids[to]);
            }






            Check(covered.Count==0,"Move the control overlay away from these terminals: "+string.Join(", ",covered));
            var map=SparkLevel80Process.Wires;
            int surfaces=bench.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials)
                .Where(m=>m!=null&&m.HasProperty("_BumpMap")&&m.GetTexture("_BumpMap")!=null)
                .Select(m=>m.GetTexture("_BumpMap")).Distinct().Count();
            Check(surfaces==9,$"Expected nine supplied PBR surface families; found {surfaces}.");
            Record("Nine authored PBR surface families applied in the fitted workbench.");
            const int feedback=77;
            await Click(loop.powerButton);
            for(int i=0;i<map.Length;i++)
            {
                if(i==feedback)continue;
                var (from,to)=map[i];
                Connect(from,to,from==0?WirePolarity.Positive:from==2?WirePolarity.Negative:WirePolarity.Neutral);
            }
            await Wait(()=>loop.statusText.text.Contains("W078"),"missing direct receiver power feedback");
            Check(!loop.safetyButton.interactable&&manager.SatisfiedTargetCount==0,"Telemetry bypassed W078.");
            Record("Connected the other 125 player wires. W074, W075, W076 and W079 could not substitute for W078.");
            Connect(map[feedback].a,map[feedback].b);
            await At(1);
            Time.captureDeltaTime=.10f;
            camera.enabled=false;
            var plant=Read<SparkLevel80Plant>(loop,"plant");
            bool Flag(string key)=>Read<bool>(loop,key);
            async Task Link()
            {
                await Click(loop.sourceButton);
                await Click(loop.tuneAButton);await Click(loop.tuneBButton);await Click(loop.tuneCButton);await Click(loop.tuneDButton);
                await Click(loop.compButton);await Click(loop.spectrumButton);await Click(loop.budgetButton);
                await Click(loop.receiverButton);await Click(loop.sendButton);await Click(loop.fecButton);
            }
            async Task RunData(int number)
            {
                await Click(loop.testButton);
                await Wait(()=>Read<int>(loop,"acceptedRuns")==number,"four-stream run accepted");
                for(int i=0;i<4;i++)
                    Record($"Run {number}, {((char)('A'+i))}: RX {plant.Rx[i]:F3}, eye {plant.Eye[i]:F3}, raw BER {plant.RawBer(i):F5}, corrected BER {plant.FinalBer(i):F6}, {plant.Bits[i]} bits.");
            }
            async Task Recover()
            {
                await Click(loop.restoreButton);await Click(loop.resetButton);
            }

            await Click(loop.safetyButton);
            await Wait(()=>Flag("safetyProof"),"latched open-and-close shutter proof");
            Check(Flag("safetyOpenObserved")&&plant.SafelyClosed,"Proof did not open and reclose both shutters with the source inhibited.");
            await Click(loop.packageButton);
            await Click(loop.calSourceButton);await Click(loop.dockButton);await Click(loop.calRxButton);
            await At(2);
            await Link();await At(3);await RunData(1);await At(4);
            Capture(bench,pins,"Running",false);

            await Click(loop.driftButton);
            await Wait(()=>Flag("driftPassed"),"source drift protection");
            Check(Read<float>(loop,"driftAtTrip")>.19f,"The laser reference did not drift.");
            await Recover();await Link();
            await Click(loop.thermalButton);
            await Wait(()=>Flag("thermalPassed"),"coupled heater protection");
            Check(Read<float>(loop,"thermalAtTrip")>.20f,"Thermal coupling did not physically rise.");
            await Recover();await Link();
            await Click(loop.bendButton);
            await Wait(()=>Flag("bendPassed"),"fiber attenuation protection");
            Check(Read<float>(loop,"bendAtTrip")>.32f,"Fiber loss did not physically rise.");
            await Recover();await Link();await RunData(2);await At(5);
            Record("Source drift, thermal crosstalk and fiber bend loss triggered separate protections. A fresh four-stream run qualified.");

            await Click(loop.armButton);
            Check(loop.contacts[0].IsClosed&&plant.Launch>.76f&&plant.ClocksReady,"Optical link was not live before the W078 test.");
            Check(wirePaths!=null&&wirePaths.UndoLastWire(),"Wire history could not physically remove W078.");
            await Wait(()=>!Flag("rxValid")&&loop.contacts[0].IsOpen,"W078 invalidation and source relay open");
            float atTrip=Read<float>(loop,"launchAtTrip");
            Check(atTrip>.76f&&plant.Launch>.025f&&plant.Launch<=atTrip,"Source output disappeared instantaneously.");
            Check(!loop.testButton.interactable&&!loop.resetButton.interactable&&Flag("feedbackObserved")&&manager.SatisfiedTargetCount==5,
                "W078 must invalidate receiver samples and preserve completed history.");
            await Wait(()=>manager.ElectricalSolver.LatestSolveResult.TryGetVoltage(pins["LASER_ENABLE"],pins["PLC_GND"],out float v)&&v<.5f,"source permit below 0.5 V");
            await Wait(()=>plant.SafelyClosed&&plant.ThroughPower<.0001f,"both shutters closed and output decayed");
            Check(!Flag("sending")&&plant.Bits.All(n=>n==0),"Stale data retained qualification after W078 removal.");
            Record($"W078 opened at normalized output {atTrip:F3}. Permit fell below 0.5 V, both shutter mechanisms closed, optical output decayed and 5/6 remained.");
            Capture(bench,pins,"SensorFault",false);
            camera.enabled=true;
            Connect(map[feedback].a,map[feedback].b);
            camera.enabled=false;
            await Click(loop.calRxButton);await Click(loop.verifyButton);await Click(loop.resetButton);
            await Link();await RunData(3);await Click(loop.certifyButton);
            await At(6);await Wait(()=>manager.IsCompleted,"campaign completion");
            Record("6/6 — Campaign manager reports Completed after W078 reconnection, independent shutdown verification, receiver recalibration, explicit reset and fresh four-channel data.");
            Capture(bench,pins,"Verified",false);
        }
        catch (Exception error)
        {
            Record("FAIL: " + error);
            if (loop != null) Record("Control panel: " + loop.statusText.text + " | " + loop.meterText.text + " | " + loop.detailText.text);
            if (loop != null) Record($"Electrical state: 5 V request {Read<bool>(loop, "power")}, solved rails {Read<bool>(loop, "rails")}, e-stop loop {Read<bool>(loop, "safety")}, RX power valid {Read<bool>(loop, "rxValid")}, first wiring issue {Read<string>(loop, "wiring")}.");
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
            "Missing Level 80 action on " + button.name);
        int pressedFrame=Time.frameCount;
        button.onClick.Invoke();
        await Wait(()=>Time.frameCount>pressedFrame,"next frame after "+button.name);
    }

    private static void Capture(Transform bench, Dictionary<string, SparkTerminal> pins, string name, bool overhead)
    {
        var view = new GameObject("Level 80 test camera");
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
            File.WriteAllBytes("Logs/Level80_" + name + ".png", snapshot.EncodeToPNG());
            if (!overhead) return;
            var rows = new List<string> { "Terminal,X,Y" };

            foreach (var pair in pins)
            {
                var p = camera.WorldToViewportPoint(pair.Value.transform.position);
                rows.Add($"{pair.Key},{Mathf.RoundToInt(p.x * width)},{Mathf.RoundToInt((1f - p.y) * height)}");
            }
            File.WriteAllLines("Logs/Level80_Pins.csv", rows);
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
        var timeout = DateTime.UtcNow.AddSeconds(80);
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







