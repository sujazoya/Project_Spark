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
internal static class Level69ModelPlayModeProbe
{
    private const string Request = "Logs/Level69ModelTest.request";
    private const string Report = "Logs/Level69ModelTest.txt";
    private const string Running = "Codex.Level69.ModelTest";
    private static LevelGamePlayManager manager;
    private static SparkLevel69Process loop;

    static Level69ModelPlayModeProbe()
    {
        EditorApplication.update += CheckRequest;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    private static void CheckRequest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(Request)) return;
        try { File.Delete(Request); }
        catch (IOException) { return; }
        File.WriteAllText(Report, "Level 69 — Breadboard assembly and Keep It Cold test\n");
        try
        {
            Check(SceneManager.GetActiveScene().path == SparkLevel69ModelInstaller.ScenePath, "Open MainMenu in Edit Mode.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SparkLevel69ModelInstaller.Prefab) != null)
                SparkLevel69ModelInstaller.Fit();
            SparkLevel69ModelInstaller.Install();
            Record("Installed the fitted assembly in the Level 69 prefab and registered the campaign.");
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
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel69ModelInstaller.Folder + "Level_69.asset");
            Check(definition != null && wireBuilder != null && loader != null, "Level definition, wire builder, or layout loader missing.");
            if (manager.LevelProgression != null) manager.LevelProgression.enabled = false;
            Write(manager, "enforceProgressionAccess", false);
            Write(manager, "autoAdvanceOnCompletion", false);
            Write(manager, "autoEvaluate", true);
            int levelIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), definition);
            Check(levelIndex >= 0 && manager.StartLevel(levelIndex), "Level 69 is missing from the campaign or could not start.");
            await Wait(() => loader.ActiveLayout != null && manager.ActiveLevel == definition, "Level 69 loaded");
            loop = loader.ActiveLayout.GetComponent<SparkLevel69Process>();
            await Wait(() => loop != null && Read<int>(loop, "topology") >= 0,
                "the active loop controller to check the empty circuit");

            var bench = loop.transform.Find("Refrigeration Workbench");
            var camera = Read<Camera>(wireBuilder, "targetCamera");
            var grid = SparkLevel69ModelInstaller.FindBoard(SceneManager.GetActiveScene());
            var ids = SparkLevel69ModelInstaller.TerminalIds.ToArray();
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
                Vector3 surface = SparkLevel69ModelInstaller.SurfacePoint(grid, foot, bench.up);
                maximumGap = Mathf.Max(maximumGap, Mathf.Abs(Vector3.Dot(surface - foot, bench.up)));
                Check(maximumGap < .001f, "Terminal base does not touch the breadboard: " + id);
                Check(grid.TryGetNearestHole(socket.position, .15f, out var hole, out int column, out int row) &&
                    holes.Add(new Vector2Int(column, row)) &&
                    Vector3.ProjectOnPlane(foot - hole, bench.up).magnitude < .001f,
                    "Terminal does not sit over its own wire hole: " + id);
            }
            Check(holes.Count == 128, "Expected 128 breadboard sockets.");
            Check(highestFoot - lowestFoot < .010f, $"Breadboard seating varies by {highestFoot-lowestFoot:F5} Unity units.");
            Record($"128/128 terminal feet touch the breadboard on distinct wire holes. Maximum seating error: {maximumGap:F6} Unity units.");
            var labels = bench.GetComponentsInChildren<TextMesh>();
            Check(labels.Length == 134 && Array.TrueForAll(labels, label => label.transform.localScale == Vector3.one * .20f),
                "TextMesh labels must use a local scale of 0.20.");
            Record("All 134 TextMesh labels use 0.20 local scale.");
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
            Record("128/128 sockets picked by the real Wire tool from the active game camera.");
            Record("Sockets overlapped by Level 69 buttons in the tested camera view: "+(covered.Count==0?"none":string.Join(", ",covered))+".");
            void Connect(int from, int to, WirePolarity polarity = WirePolarity.Neutral)
            {
                Check(wireBuilder.BeginWire(pins[ids[from]], polarity), "Wire tool could not start at " + ids[from]);
                Check(wireBuilder.TryEndWire(camera.WorldToScreenPoint(pins[ids[to]].transform.position)),
                    "Wire tool could not finish " + ids[from] + " to " + ids[to]);
            }






            var map=SparkLevel69Process.Wires;
            await Click(loop.powerButton);
            for(int i=0;i<map.Length-1;i++)
            {
                var (from,to)=map[i];
                Connect(from,to,from==0?WirePolarity.Positive:from==2?WirePolarity.Negative:WirePolarity.Neutral);
            }
            await Wait(()=>loop.statusText.text.Contains("W82"),"missing Evaporator A feedback circuit");
            Record("Connected first 81 wires. "+loop.statusText.text);
            Check(!loop.startButton.interactable&&manager.SatisfiedTargetCount==0,"Compressor enabled without feedback.");
            Connect(map[81].a,map[81].b);
            await At(1);
            Time.captureDeltaTime=.12f;
            camera.enabled=false;

            await Click(loop.safetyCheckButton);
            await Click(loop.calPressureButton);
            await Click(loop.calTempButton);
            await Click(loop.calMassButton);
            await Click(loop.condenserButton);
            await Click(loop.evaporatorButton);
            await Click(loop.enableAButton);
            await Click(loop.enableBButton);
            await Click(loop.startButton);
            await At(2);
            await Click(loop.calAButton);
            await Click(loop.calBButton);
            await Click(loop.tuneShButton);
            await Click(loop.tuneZonesButton);
            await Click(loop.autoZonesButton);
            await At(3);

            var plant=Read<SparkLevel69ColdPlant>(loop,"plant");
            Capture(bench,pins,"Running",false);
            await Click(loop.frostTestButton);
            await Click(loop.defrostButton);
            await Wait(()=>plant.HeaterKw>0,"isolated defrost heater");
            Check(plant.Zones[1].Valve<.028f&&plant.Zones[1].AirFan<.055f,"Defrost heater ran with branch B still feeding or blowing.");
            await At(4);
            Record($"Actual melt {plant.Melted*1000:F0} g; actual drainage {plant.Drained*1000:F0} g.");
            await Click(loop.doorTestButton);
            await Wait(()=>Read<bool>(loop,"doorPassed"),"recovery from added door load");
            await Click(loop.fanFaultButton);
            await Wait(()=>Read<bool>(loop,"fanObserved")&&loop.contacts[0].IsOpen,"measured high-side increase and compressor isolation");
            await Click(loop.resetButton);
            await Click(loop.startButton);
            await Click(loop.optimizeButton);
            await At(5);
            Record($"Measured baseline COP {Read<float>(loop,"baselineCop"):F2}; optimized COP {Read<float>(loop,"efficiencyCop"):F2}.");

            await Click(loop.armFeedbackButton);
            Check(wirePaths!=null&&wirePaths.UndoLastWire(),"Could not disconnect W82.");
            await Wait(()=>loop.statusText.text.Contains("EVAP A TEMP INVALID")&&loop.contacts[0].IsOpen,
                "Evaporator A invalidation and compressor isolation");
            var solve=manager.ElectricalSolver.LatestSolveResult;
            Check(solve!=null&&solve.TryGetVoltage(pins["COMP_EN"],pins["SAFE_GND"],out var enabledVoltage)&&enabledVoltage<.5f,
                "The compressor enable socket stayed energized after losing Evaporator A.");
            Check(!loop.resetButton.interactable&&!Read<bool>(loop,"validA")&&manager.SatisfiedTargetCount==5,
                "Feedback removal did not invalidate superheat or preserve the first five objectives.");
            Capture(bench,pins,"SensorFault",false);
            camera.enabled=true;
            Connect(map[81].a,map[81].b);
            camera.enabled=false;
            await Click(loop.recoverButton);
            await Click(loop.resetButton);
            await Click(loop.startButton);
            await Click(loop.doorTestButton);
            await Wait(()=>Read<bool>(loop,"freshDoor"),"fresh door load recovery");
            await Click(loop.certifyButton);
            await At(6);
            await Wait(()=>manager.IsCompleted,"campaign completion");
            Check(plant.MassError<.0001f&&plant.CycleResidual<.001f,"Refrigerant mass or cycle balance check failed.");
            Record(loop.meterText.text+" | "+loop.detailText.text);
            Record($"Refrigerant inventory error {plant.MassError:F7} kg. Four-state enthalpy closure {plant.CycleResidual:F6} kJ/kg.");
            Record("6/6 — Campaign manager reports Completed after defrost, high-pressure protection, optimization and actual W82 recovery.");
            Capture(bench,pins,"Verified",false);
        }
        catch (Exception error)
        {
            Record("FAIL: " + error);
            if (loop != null) Record("Control panel: " + loop.statusText.text + " | " + loop.meterText.text + " | " + loop.detailText.text);
            if (loop != null) Record($"Electrical state: 5 V request {Read<bool>(loop, "power")}, solved rails {Read<bool>(loop, "rails")}, e-stop loop {Read<bool>(loop, "safety")}, Evaporator A valid {Read<bool>(loop, "validA")}, first wiring issue {Read<string>(loop, "wiring")}.");
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
            "Missing Level 69 action on " + button.name);
        button.onClick.Invoke();
    }

    private static void Capture(Transform bench, Dictionary<string, SparkTerminal> pins, string name, bool overhead)
    {
        var view = new GameObject("Level 69 test camera");
        var camera = view.AddComponent<Camera>();
        camera.enabled = false;
        view.transform.position = bench.TransformPoint(new Vector3(0, 2.20f, overhead ? -.69f : -1.92f));
        view.transform.rotation = Quaternion.LookRotation(bench.TransformPoint(new Vector3(0, .04f, 0)) -
            view.transform.position, bench.up);
        camera.orthographic = true;
        camera.orthographicSize = bench.TransformVector(new Vector3(0, .90f, 0)).magnitude;
        const int width = 1600, height = 1069;
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
            File.WriteAllBytes("Logs/Level69_" + name + ".png", snapshot.EncodeToPNG());
            if (!overhead) return;
            var rows = new List<string> { "Terminal,X,Y" };

            foreach (var pair in pins)
            {
                var p = camera.WorldToViewportPoint(pair.Value.transform.position);
                rows.Add($"{pair.Key},{Mathf.RoundToInt(p.x * width)},{Mathf.RoundToInt((1f - p.y) * height)}");
            }
            File.WriteAllLines("Logs/Level69_Pins.csv", rows);
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

