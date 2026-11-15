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
internal static class Level70ModelPlayModeProbe
{
    private const string Request = "Logs/Level70ModelTest.request";
    private const string Report = "Logs/Level70ModelTest.txt";
    private const string Running = "Codex.Level70.ModelTest";
    private static LevelGamePlayManager manager;
    private static SparkLevel70Process loop;

    static Level70ModelPlayModeProbe()
    {
        EditorApplication.update += CheckRequest;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    private static void CheckRequest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(Request)) return;
        try { File.Delete(Request); }
        catch (IOException) { return; }
        File.WriteAllText(Report, "Level 70 — Breadboard assembly and Guard the Air test\n");
        try
        {
            Check(SceneManager.GetActiveScene().path == SparkLevel70ModelInstaller.ScenePath, "Open MainMenu in Edit Mode.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SparkLevel70ModelInstaller.Prefab) != null)
                SparkLevel70ModelInstaller.Fit();
            SparkLevel70ModelInstaller.Install();
            Record("Installed the fitted assembly in the Level 70 prefab and registered the campaign.");
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
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel70ModelInstaller.Folder + "Level_70.asset");
            Check(definition != null && wireBuilder != null && loader != null, "Level definition, wire builder, or layout loader missing.");
            if (manager.LevelProgression != null) manager.LevelProgression.enabled = false;
            Write(manager, "enforceProgressionAccess", false);
            Write(manager, "autoAdvanceOnCompletion", false);
            Write(manager, "autoEvaluate", true);
            int levelIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), definition);
            Check(levelIndex >= 0 && manager.StartLevel(levelIndex), "Level 70 is missing from the campaign or could not start.");
            await Wait(() => loader.ActiveLayout != null && manager.ActiveLevel == definition, "Level 70 loaded");
            loop = loader.ActiveLayout.GetComponent<SparkLevel70Process>();
            await Wait(() => loop != null && Read<int>(loop, "topology") >= 0,
                "the active loop controller to check the empty circuit");

            var bench = loop.transform.Find("Cleanroom Workbench");
            var camera = Read<Camera>(wireBuilder, "targetCamera");
            var grid = SparkLevel70ModelInstaller.FindBoard(SceneManager.GetActiveScene());
            var ids = SparkLevel70ModelInstaller.TerminalIds.ToArray();
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
                Vector3 surface = SparkLevel70ModelInstaller.SurfacePoint(grid, foot, bench.up);
                maximumGap = Mathf.Max(maximumGap, Mathf.Abs(Vector3.Dot(surface - foot, bench.up)));
                Check(maximumGap < .001f, "Terminal base does not touch the breadboard: " + id);
                Check(grid.TryGetNearestHole(socket.position, .15f, out var hole, out int column, out int row) &&
                    holes.Add(new Vector2Int(column, row)) &&
                    Vector3.ProjectOnPlane(foot - hole, bench.up).magnitude < .001f,
                    "Terminal does not sit over its own wire hole: " + id);
            }
            Check(holes.Count == 134, "Expected 134 breadboard sockets.");
            Check(highestFoot - lowestFoot < .010f, $"Breadboard seating varies by {highestFoot-lowestFoot:F5} Unity units.");
            Record($"134/134 terminal feet touch the breadboard on distinct wire holes. Maximum seating error: {maximumGap:F6} Unity units.");
            var labels = bench.GetComponentsInChildren<TextMesh>();
            Check(labels.Length == 140 && Array.TrueForAll(labels, label => label.transform.localScale == Vector3.one * .20f),
                "TextMesh labels must use a local scale of 0.20.");
            Record("All 140 TextMesh labels use 0.20 local scale.");
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
            Record("134/134 sockets picked by the real Wire tool from the active game camera.");
            Record("Sockets overlapped by Level 70 buttons in the tested camera view: "+(covered.Count==0?"none":string.Join(", ",covered))+".");
            void Connect(int from, int to, WirePolarity polarity = WirePolarity.Neutral)
            {
                Check(wireBuilder.BeginWire(pins[ids[from]], polarity), "Wire tool could not start at " + ids[from]);
                Check(wireBuilder.TryEndWire(camera.WorldToScreenPoint(pins[ids[to]].transform.position)),
                    "Wire tool could not finish " + ids[from] + " to " + ids[to]);
            }






            var map=SparkLevel70Process.Wires;
            await Click(loop.powerButton);
            for(int i=0;i<map.Length-1;i++)
            {
                var (from,to)=map[i];
                Connect(from,to,from==0?WirePolarity.Positive:from==2?WirePolarity.Negative:WirePolarity.Neutral);
            }
            await Wait(()=>loop.statusText.text.Contains("W86"),"missing pressure feedback circuit");
            Record("Connected first 85 wires. "+loop.statusText.text);
            Check(!loop.waferButton.interactable&&manager.SatisfiedTargetCount==0,"Wafer transfer enabled without pressure feedback.");
            Connect(map[85].a,map[85].b);
            await At(1);
            Time.captureDeltaTime=.12f;
            camera.enabled=false;

            await Click(loop.calPressureButton);
            await Click(loop.calAirButton);
            await Click(loop.calParticlesButton);
            await Click(loop.calTempButton);
            await Click(loop.calHumidityButton);
            await Click(loop.supplyButton);
            await Click(loop.returnFanButton);
            await Click(loop.exhaustButton);
            await Click(loop.ffuAButton);
            await Click(loop.ffuBButton);
            await At(2);
            await Click(loop.tunePressureButton);
            await Click(loop.tuneAirButton);
            await Click(loop.environmentButton);
            await At(3);

            var plant=Read<SparkLevel70AirPlant>(loop,"plant");
            await Click(loop.waferButton);
            await Wait(()=>Read<int>(loop,"batches")==1,"first wafer inspection");
            await Click(loop.waferButton);
            await At(4);
            Record($"Two cleanroom batches accepted. Second mean exposure {Read<float>(loop,"lastExposure"):F0} particles/m³.");
            Capture(bench,pins,"Running",false);
            await Click(loop.doorButton);
            await Wait(()=>Read<bool>(loop,"doorPassed"),"physical airlock pressure and particle recovery");
            Check(!plant.HadDoorConflict,"The airlock issued simultaneous commands.");
            await Click(loop.loadingButton);
            await Click(loop.diagnoseButton);
            Record($"Measured filter loading increased HEPA A drop from {Read<float>(loop,"loadBaseDrop"):F1} to {plant.Filters[0].Drop:F1} Pa.");
            await Click(loop.leakButton);
            await Wait(()=>Read<bool>(loop,"breachObserved")&&loop.contacts[0].IsOpen,"measured seal-bypass rise and wafer inhibition");
            Check(!loop.waferButton.interactable,"Wafer transfer stayed enabled during the HEPA breach.");
            await Click(loop.serviceButton);
            Check(!loop.replaceButton.interactable,"Replacement enabled before the branch was isolated.");
            await Click(loop.isolateButton);
            await Wait(()=>loop.replaceButton.interactable,"physical isolation and stopped FFU A");
            Check(plant.Filters[0].Flow<.028f&&plant.Fans[3].Rpm<35,"HEPA branch was not isolated before service.");
            await Click(loop.replaceButton);
            await Click(loop.verifyButton);
            await Click(loop.resetButton);
            await At(5);
            Check(loop.contacts[0].IsClosed,"Wafer robot permit did not return after measured clean purge.");

            await Click(loop.armButton);
            Check(loop.contacts[0].IsClosed,"Arming the test tripped the robot before a real disconnect.");
            float priorPressure=plant.PressureRoom;
            Check(wirePaths!=null&&wirePaths.UndoLastWire(),"Could not disconnect W86.");
            await Wait(()=>loop.statusText.text.Contains("PRESSURE FEEDBACK INVALID")&&loop.contacts[0].IsOpen,
                "pressure invalidation and robot permit drop");
            var solve=manager.ElectricalSolver.LatestSolveResult;
            Check(solve!=null&&solve.TryGetVoltage(pins["ROBOT_ENABLE"],pins["PLC_GND"],out var voltage)&&voltage<.5f,
                "The wafer robot enable socket stayed energized after losing pressure feedback.");
            Check(!loop.resetButton.interactable&&!Read<bool>(loop,"pressureValid")&&manager.SatisfiedTargetCount==5,
                "Pressure feedback removal did not preserve five historical gates or invalidate pressure.");
            await Wait(()=>Mathf.Abs(plant.PressureRoom-priorPressure)>.9f&&plant.Fans[0].Rpm>300,
                "continuing physical pressure dynamics in fallback");
            Record($"Fallback supply fan {plant.Fans[0].Rpm:F0} RPM; physical room pressure {plant.PressureRoom:F1} Pa with invalid feedback.");
            Capture(bench,pins,"SensorFault",false);
            camera.enabled=true;
            Connect(map[85].a,map[85].b);
            camera.enabled=false;
            await Click(loop.recalButton);
            await Click(loop.resetButton);
            await Click(loop.environmentButton);
            await Click(loop.doorButton);
            await Wait(()=>Read<bool>(loop,"freshDoor"),"fresh airlock disturbance recovery");
            await Click(loop.waferButton);
            await Wait(()=>Read<int>(loop,"freshBatches")>0,"fresh accepted wafer");
            await Click(loop.certifyButton);
            await At(6);
            await Wait(()=>manager.IsCompleted,"campaign completion");
            Check(plant.LastMassResidual<.001f,"Pneumatic volume balance check failed.");
            Record(loop.meterText.text+" | "+loop.detailText.text);
            Record($"Pneumatic volume balance residual {plant.LastMassResidual:F8} m³. Verified dew point {plant.Room.DewPoint:F2}°C.");
            Record("6/6 — Campaign manager reports Completed after wafer inspection, filter replacement, W86 recovery and recertification.");
            Capture(bench,pins,"Verified",false);
        }
        catch (Exception error)
        {
            Record("FAIL: " + error);
            if (loop != null) Record("Control panel: " + loop.statusText.text + " | " + loop.meterText.text + " | " + loop.detailText.text);
            if (loop != null) Record($"Electrical state: 5 V request {Read<bool>(loop, "power")}, solved rails {Read<bool>(loop, "rails")}, e-stop loop {Read<bool>(loop, "safeStop")}, pressure valid {Read<bool>(loop, "pressureValid")}, first wiring issue {Read<string>(loop, "wiring")}.");
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
            "Missing Level 70 action on " + button.name);
        button.onClick.Invoke();
    }

    private static void Capture(Transform bench, Dictionary<string, SparkTerminal> pins, string name, bool overhead)
    {
        var view = new GameObject("Level 70 test camera");
        var camera = view.AddComponent<Camera>();
        camera.enabled = false;
        view.transform.position = bench.TransformPoint(new Vector3(0, 2.20f, overhead ? -.70f : -1.92f));
        view.transform.rotation = Quaternion.LookRotation(bench.TransformPoint(new Vector3(0, .04f, 0)) -
            view.transform.position, bench.up);
        camera.orthographic = true;
        camera.orthographicSize = bench.TransformVector(new Vector3(0, .90f, 0)).magnitude;
        const int width = 1600, height = 1070;
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
            File.WriteAllBytes("Logs/Level70_" + name + ".png", snapshot.EncodeToPNG());
            if (!overhead) return;
            var rows = new List<string> { "Terminal,X,Y" };

            foreach (var pair in pins)
            {
                var p = camera.WorldToViewportPoint(pair.Value.transform.position);
                rows.Add($"{pair.Key},{Mathf.RoundToInt(p.x * width)},{Mathf.RoundToInt((1f - p.y) * height)}");
            }
            File.WriteAllLines("Logs/Level70_Pins.csv", rows);
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
