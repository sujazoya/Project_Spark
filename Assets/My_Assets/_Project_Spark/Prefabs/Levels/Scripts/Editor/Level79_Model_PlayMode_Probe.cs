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
internal static class Level79ModelPlayModeProbe
{
    private const string Request = "Logs/Level79ModelTest.request";
    private const string Report = "Logs/Level79ModelTest.txt";
    private const string Running = "Codex.Level79.ModelTest";
    private static LevelGamePlayManager manager;
    private static SparkLevel79Process loop;

    static Level79ModelPlayModeProbe()
    {
        EditorApplication.update += CheckRequest;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    private static void CheckRequest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(Request)) return;
        try { File.Delete(Request); }
        catch (IOException) { return; }
        File.WriteAllText(Report, "Level 79 — Breadboard assembly and Bond the Worlds test\n");
        try
        {
            Check(SceneManager.GetActiveScene().path == SparkLevel79ModelInstaller.ScenePath, "Open MainMenu in Edit Mode.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SparkLevel79ModelInstaller.Prefab) != null)
                SparkLevel79ModelInstaller.Fit();
            SparkLevel79ModelInstaller.Install();
            Record("Installed the fitted assembly in the Level 79 prefab and registered the campaign.");
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
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel79ModelInstaller.Folder + "Level_79.asset");
            Check(definition != null && wireBuilder != null && loader != null, "Level definition, wire builder, or layout loader missing.");
            if (manager.LevelProgression != null) manager.LevelProgression.enabled = false;
            Write(manager, "enforceProgressionAccess", false);
            Write(manager, "autoAdvanceOnCompletion", false);
            Write(manager, "autoEvaluate", true);
            int levelIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), definition);
            Check(levelIndex >= 0 && manager.StartLevel(levelIndex), "Level 79 is missing from the campaign or could not start.");
            await Wait(() => loader.ActiveLayout != null && manager.ActiveLevel == definition, "Level 79 loaded");
            loop = loader.ActiveLayout.GetComponent<SparkLevel79Process>();
            await Wait(() => loop != null && Read<int>(loop, "topology") >= 0,
                "the active loop controller to check the empty circuit");

            var bench = loop.transform.Find("Bonding Workbench");
            var camera = Read<Camera>(wireBuilder, "targetCamera");
            var grid = SparkLevel79ModelInstaller.FindBoard(SceneManager.GetActiveScene());
            var ids = SparkLevel79ModelInstaller.TerminalIds.ToArray();
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
                Vector3 surface = SparkLevel79ModelInstaller.SurfacePoint(grid, foot, bench.up);
                maximumGap = Mathf.Max(maximumGap, Mathf.Abs(Vector3.Dot(surface - foot, bench.up)));
                Check(maximumGap < .001f, "Terminal base does not touch the breadboard: " + id);
                Check(grid.TryGetNearestHole(socket.position, .15f, out var hole, out int column, out int row) &&
                    holes.Add(new Vector2Int(column, row)) &&
                    Vector3.ProjectOnPlane(foot - hole, bench.up).magnitude < .001f,
                    "Terminal does not sit over its own wire hole: " + id);
            }
            Check(holes.Count == 188, "Expected 188 breadboard sockets.");
            Check(highestFoot - lowestFoot < .010f, $"Breadboard seating varies by {highestFoot-lowestFoot:F5} Unity units.");
            Record($"188/188 terminal feet touch the breadboard on distinct wire holes. Maximum seating error: {maximumGap:F6} Unity units.");
            var labels = bench.GetComponentsInChildren<TextMesh>();
            Check(labels.Length == 194 && Array.TrueForAll(labels, label => label.transform.localScale == Vector3.one * .20f),
                "TextMesh labels must use a local scale of 0.20.");
            Record("All 194 TextMesh labels use 0.20 local scale.");
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
            Record("188/188 sockets picked by the real Wire tool from the active game camera.");
            Record("Sockets overlapped by Level 79 buttons in the tested camera view: "+(covered.Count==0?"none":string.Join(", ",covered))+".");
            void Connect(int from, int to, WirePolarity polarity = WirePolarity.Neutral)
            {
                Check(wireBuilder.BeginWire(pins[ids[from]], polarity), "Wire tool could not start at " + ids[from]);
                Check(wireBuilder.TryEndWire(camera.WorldToScreenPoint(pins[ids[to]].transform.position)),
                    "Wire tool could not finish " + ids[from] + " to " + ids[to]);
            }






            Check(covered.Count==0,"Move the control overlay away from these terminals: "+string.Join(", ",covered));
            var map=SparkLevel79Process.Wires;
            const int feedback=36;
            await Click(loop.powerButton);
            for(int i=0;i<map.Length;i++)
            {
                if(i==feedback)continue;
                var (from,to)=map[i];
                Connect(from,to,from==0?WirePolarity.Positive:from==2?WirePolarity.Negative:WirePolarity.Neutral);
            }
            await Wait(()=>loop.statusText.text.Contains("W037"),"missing direct interferometer feedback");
            Check(!loop.homeButton.interactable&&manager.SatisfiedTargetCount==0,"Laser status bypassed W037.");
            Record("Connected the other 121 player wires. Laser telemetry, health and calibration could not substitute for W037.");
            Connect(map[feedback].a,map[feedback].b);
            await At(1);
            Time.captureDeltaTime=.10f;
            camera.enabled=false;
            var plant=Read<SparkLevel79Plant>(loop,"plant");
            bool Flag(string key)=>Read<bool>(loop,key);
            async Task Wafer()
            {
                await Click(loop.waferButton);await Click(loop.inspectButton);await Click(loop.prepButton);
            }
            async Task Align()
            {
                await Click(loop.loadButton);await Click(loop.alignButton);
            }
            async Task Bond(bool snapshot=false)
            {
                await Click(loop.approachButton);await Click(loop.growButton);
                if(snapshot)
                {
                    await Wait(()=>plant.Force>.29f&&plant.RegionBond(0)>.22f,"active contact-front growth");
                    Capture(bench,pins,"Running",false);
                }
                int n=Read<int>(loop,"scanned");
                await Click(loop.scanButton);
                await Wait(()=>Read<int>(loop,"scanned")==n+1,"acoustic and IR scan acceptance");
            }
            async Task Electrical(int number)
            {
                await Click(loop.annealButton);await Click(loop.testButton);
                await Wait(()=>Read<int>(loop,"acceptedBatches")==number,"full assembly electrical qualification");
                Record($"Assembly {number}: 4 accepted chiplets, worst void {plant.WorstVoid():P3}, stress {plant.StackStress:F4}, max Kelvin {Read<float>(loop,"maximumResistance"):F4}, modeled BER {Read<float>(loop,"worstBer"):F6}.");
            }
            async Task Assemble(int number)
            {
                await Wafer();
                for(int i=0;i<4;i++){await Align();await Bond();}
                await Electrical(number);
            }
            async Task Recover()
            {
                await Click(loop.restoreButton);await Click(loop.resetButton);await Click(loop.discardButton);
            }

            await Click(loop.homeButton);
            await Click(loop.cameraButton);await Click(loop.laserButton);
            await Wait(()=>Flag("cameraCal")&&Flag("validLaser"),"vision and fresh laser calibration");
            await Wafer();await At(2);
            await Align();await Bond(true);
            await Align();await Bond();
            await At(3);
            await Align();await Bond();
            await Align();await Bond();await Electrical(1);await At(4);

            await Wafer();await Align();
            await Click(loop.driftButton);
            await Wait(()=>Flag("driftPassed"),"camera offset protection");
            Check(Read<float>(loop,"overlayAtTrip")>.24f,"Camera drift did not change the measured overlay.");
            await Recover();
            await Click(loop.cameraButton);
            await Wait(()=>Flag("cameraCal"),"camera drift recalibration");
            await Wafer();await Align();await Click(loop.approachButton);
            await Click(loop.chuckButton);
            await Wait(()=>Flag("chuckPassed"),"chuck residual-force protection");
            Check(Read<float>(loop,"chuckAtTrip")>.29f,"Chuck force anomaly did not physically develop.");
            await Recover();
            await Wafer();await Align();await Click(loop.approachButton);await Click(loop.growButton);
            await Click(loop.particleButton);
            await Wait(()=>Flag("particlePassed"),"particle contamination protection");
            Check(Read<float>(loop,"particlesAtTrip")>.118f,"Particles did not contaminate the interface.");
            await Recover();
            await Assemble(2);await At(5);
            Record("Camera drift, residual chuck force and interface particles triggered separate protections. A fresh four-chiplet assembly qualified after recovery.");

            await Wafer();await Align();await Click(loop.approachButton);await Click(loop.armButton);
            Check(loop.contacts[0].IsClosed&&plant.Motion>.26f,"Fine approach was not live before W037 removal.");
            Check(wirePaths!=null&&wirePaths.UndoLastWire(),"Wire history could not physically remove W037.");
            await Wait(()=>!Flag("validLaser")&&loop.contacts[0].IsOpen,"laser feedback stale and relay open");
            float speed=Read<float>(loop,"speedAtTrip");
            Check(speed>.24f&&plant.Motion>0&&plant.Motion<=speed+.01f,"Fine-axis velocity did not begin a controlled stop.");
            Check(!loop.growButton.interactable&&!loop.resetButton.interactable&&!Flag("homed")&&Flag("feedbackObserved")&&manager.SatisfiedTargetCount==5,
                "W037 must inhibit contact, invalidate home and retain the first five objectives.");
            await Wait(()=>manager.ElectricalSolver.LatestSolveResult.TryGetVoltage(pins["CONTACT_ENABLE"],pins["PLC_GND"],out float v)&&v<.5f,"contact permit below 0.5 V");
            await Wait(()=>plant.PoseMotion<.035f&&plant.Force<.025f,"controlled motion hold");
            Check(plant.RegionBond(0)==0,"Bond front continued without trusted feedback.");
            Record($"W037 removed during active approach at normalized speed {speed:F3}. Permit fell below 0.5 V, axes braked to hold, no bond started, and 5/6 remained.");
            Capture(bench,pins,"SensorFault",false);
            camera.enabled=true;
            Connect(map[feedback].a,map[feedback].b);
            camera.enabled=false;
            await Click(loop.homeButton);await Click(loop.laserButton);
            await Click(loop.verifyButton);await Click(loop.resetButton);await Click(loop.discardButton);
            await Assemble(3);await Click(loop.certifyButton);await At(6);
            await Wait(()=>manager.IsCompleted,"campaign completion");
            Record("6/6 — Campaign manager reports Completed after W037 reconnection, safe homing, laser recalibration and explicit reset, followed by a freshly tested four-chiplet assembly.");
            Capture(bench,pins,"Verified",false);
        }
        catch (Exception error)
        {
            Record("FAIL: " + error);
            if (loop != null) Record("Control panel: " + loop.statusText.text + " | " + loop.meterText.text + " | " + loop.detailText.text);
            if (loop != null) Record($"Electrical state: 5 V request {Read<bool>(loop, "power")}, solved rails {Read<bool>(loop, "rails")}, e-stop loop {Read<bool>(loop, "safety")}, laser valid {Read<bool>(loop, "validLaser")}, first wiring issue {Read<string>(loop, "wiring")}.");
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
            "Missing Level 79 action on " + button.name);
        int pressedFrame=Time.frameCount;
        button.onClick.Invoke();
        await Wait(()=>Time.frameCount>pressedFrame,"next frame after "+button.name);
    }

    private static void Capture(Transform bench, Dictionary<string, SparkTerminal> pins, string name, bool overhead)
    {
        var view = new GameObject("Level 79 test camera");
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
            File.WriteAllBytes("Logs/Level79_" + name + ".png", snapshot.EncodeToPNG());
            if (!overhead) return;
            var rows = new List<string> { "Terminal,X,Y" };

            foreach (var pair in pins)
            {
                var p = camera.WorldToViewportPoint(pair.Value.transform.position);
                rows.Add($"{pair.Key},{Mathf.RoundToInt(p.x * width)},{Mathf.RoundToInt((1f - p.y) * height)}");
            }
            File.WriteAllLines("Logs/Level79_Pins.csv", rows);
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
        var timeout = DateTime.UtcNow.AddSeconds(79);
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





