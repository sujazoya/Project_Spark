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
internal static class Level74ModelPlayModeProbe
{
    private const string Request = "Logs/Level74ModelTest.request";
    private const string Report = "Logs/Level74ModelTest.txt";
    private const string Running = "Codex.Level74.ModelTest";
    private static LevelGamePlayManager manager;
    private static SparkLevel74Process loop;

    static Level74ModelPlayModeProbe()
    {
        EditorApplication.update += CheckRequest;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    private static void CheckRequest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(Request)) return;
        try { File.Delete(Request); }
        catch (IOException) { return; }
        File.WriteAllText(Report, "Level 74 — Breadboard assembly and Align the Light test\n");
        try
        {
            Check(SceneManager.GetActiveScene().path == SparkLevel74ModelInstaller.ScenePath, "Open MainMenu in Edit Mode.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SparkLevel74ModelInstaller.Prefab) != null)
                SparkLevel74ModelInstaller.Fit();
            SparkLevel74ModelInstaller.Install();
            Record("Installed the fitted assembly in the Level 74 prefab and registered the campaign.");
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
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel74ModelInstaller.Folder + "Level_74.asset");
            Check(definition != null && wireBuilder != null && loader != null, "Level definition, wire builder, or layout loader missing.");
            if (manager.LevelProgression != null) manager.LevelProgression.enabled = false;
            Write(manager, "enforceProgressionAccess", false);
            Write(manager, "autoAdvanceOnCompletion", false);
            Write(manager, "autoEvaluate", true);
            int levelIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), definition);
            Check(levelIndex >= 0 && manager.StartLevel(levelIndex), "Level 74 is missing from the campaign or could not start.");
            await Wait(() => loader.ActiveLayout != null && manager.ActiveLevel == definition, "Level 74 loaded");
            loop = loader.ActiveLayout.GetComponent<SparkLevel74Process>();
            await Wait(() => loop != null && Read<int>(loop, "topology") >= 0,
                "the active loop controller to check the empty circuit");

            var bench = loop.transform.Find("Lithography Workbench");
            var camera = Read<Camera>(wireBuilder, "targetCamera");
            var grid = SparkLevel74ModelInstaller.FindBoard(SceneManager.GetActiveScene());
            var ids = SparkLevel74ModelInstaller.TerminalIds.ToArray();
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
                Vector3 surface = SparkLevel74ModelInstaller.SurfacePoint(grid, foot, bench.up);
                maximumGap = Mathf.Max(maximumGap, Mathf.Abs(Vector3.Dot(surface - foot, bench.up)));
                Check(maximumGap < .001f, "Terminal base does not touch the breadboard: " + id);
                Check(grid.TryGetNearestHole(socket.position, .15f, out var hole, out int column, out int row) &&
                    holes.Add(new Vector2Int(column, row)) &&
                    Vector3.ProjectOnPlane(foot - hole, bench.up).magnitude < .001f,
                    "Terminal does not sit over its own wire hole: " + id);
            }
            Check(holes.Count == 158, "Expected 158 breadboard sockets.");
            Check(highestFoot - lowestFoot < .010f, $"Breadboard seating varies by {highestFoot-lowestFoot:F5} Unity units.");
            Record($"158/158 terminal feet touch the breadboard on distinct wire holes. Maximum seating error: {maximumGap:F6} Unity units.");
            var labels = bench.GetComponentsInChildren<TextMesh>();
            Check(labels.Length == 164 && Array.TrueForAll(labels, label => label.transform.localScale == Vector3.one * .20f),
                "TextMesh labels must use a local scale of 0.20.");
            Record("All 164 TextMesh labels use 0.20 local scale.");
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
            Record("158/158 sockets picked by the real Wire tool from the active game camera.");
            Record("Sockets overlapped by Level 74 buttons in the tested camera view: "+(covered.Count==0?"none":string.Join(", ",covered))+".");
            void Connect(int from, int to, WirePolarity polarity = WirePolarity.Neutral)
            {
                Check(wireBuilder.BeginWire(pins[ids[from]], polarity), "Wire tool could not start at " + ids[from]);
                Check(wireBuilder.TryEndWire(camera.WorldToScreenPoint(pins[ids[to]].transform.position)),
                    "Wire tool could not finish " + ids[from] + " to " + ids[to]);
            }






            Check(covered.Count==0,"Move the control overlay away from these terminals: "+string.Join(", ",covered));
            var map=SparkLevel74Process.Wires;
            await Click(loop.powerButton);
            for(int i=0;i<map.Length-1;i++)
            {
                var (from,to)=map[i];
                Connect(from,to,from==0?WirePolarity.Positive:from==2?WirePolarity.Negative:WirePolarity.Neutral);
            }
            await Wait(()=>loop.statusText.text.Contains("W102"),"missing interferometer wiring");
            Check(!loop.loadButton.interactable&&manager.SatisfiedTargetCount==0,"Wafer loading allowed with W102 missing.");
            Record("First 101 wires connected. Adding W102 through the real Wire tool.");
            Connect(map[101].a,map[101].b);
            await At(1);
            Time.captureDeltaTime=.12f;
            camera.enabled=false;
            var plant=Read<SparkLevel74Plant>(loop,"plant");

            async Task Prepare()
            {
                await Click(loop.coatButton);
                await Click(loop.softBakeButton);
                await Click(loop.transferButton);
                await Click(loop.clampButton);
            }
            async Task Align()
            {
                await Click(loop.alignButton);
                await Click(loop.focusButton);
            }
            async Task DryScan()
            {
                await Click(loop.dryScanButton);
                await Wait(()=>Read<bool>(loop,"dryPassed"),"verified 4:1 dry scan");
                Check(plant.Shutter<.025f,"Dry scan opened the shutter.");
            }
            async Task Finish(int number)
            {
                await Click(loop.postBakeButton);
                await Click(loop.developButton);
                await Click(loop.rinseButton);
                await Click(loop.dryButton);
                await Click(loop.metrologyButton);
                await Wait(()=>Read<int>(loop,"acceptedBatches")==number,"accepted sixteen-field wafer");
                Check(plant.CountExposed()==16&&plant.DoseVariation<=.02&&plant.PatternScore>=.97f,"Metrology values failed the certification window.");
                Record($"Wafer {number}: resist {plant.Coating:F2} nm, 16/16 fields, CD {plant.Fields.Min(x=>x.CriticalDimension):F2}–{plant.Fields.Max(x=>x.CriticalDimension):F2} nm, maximum overlay {plant.Fields.Max(x=>x.WorstOverlay):F2} nm, maximum focus error {plant.Fields.Max(x=>x.WorstFocus):F2} nm, dose variation {plant.DoseVariation:P2}, pattern {plant.PatternScore:P2}.");
            }

            await Click(loop.calPrepButton);
            await Click(loop.loadButton);
            await Prepare();
            await At(2);
            await Click(loop.reticleButton);
            await Click(loop.homeButton);
            await Click(loop.calPositionButton);
            await Align();
            await Click(loop.isolateButton);
            await Click(loop.syncButton);
            await DryScan();
            await At(3);
            await Click(loop.lampButton);
            await Click(loop.calDoseButton);
            await Click(loop.printButton);
            await Wait(()=>plant.CountExposed()>=3&&plant.Shutter>.85f,"live scanner exposure");
            Capture(bench,pins,"Running",false);
            await Finish(1);
            await At(4);

            await Click(loop.loadButton);
            await Click(loop.nozzleButton);
            await Click(loop.coatButton);
            await Wait(()=>Read<bool>(loop,"nozzlePassed"),"restricted spin coating");
            Record($"Restricted nozzle reduced measured flow to {Read<float>(loop,"restrictedFlow"):F2} and resist thickness to {Read<float>(loop,"restrictedThickness"):F2} nm.");
            await Click(loop.restoreButton);
            await Click(loop.resetButton);
            await Click(loop.loadButton);
            await Prepare();
            await Align();
            await Click(loop.shakeButton);
            await Click(loop.dryScanButton);
            await Wait(()=>Read<bool>(loop,"shakePassed")&&!loop.contacts[0].IsClosed,"vibration trip");
            Record($"Isolation failure produced vibration {Read<float>(loop,"shakePeak"):F2} and following error {Read<double>(loop,"errorPeak"):F2} nm.");
            await Click(loop.restoreButton);
            await Click(loop.resetButton);
            await Click(loop.lensButton);
            await Wait(()=>Read<bool>(loop,"lensPassed"),"lens thermal hold");
            Record($"Lens drift reached {Read<float>(loop,"lensPeak"):F2}°C and invalidated focus.");
            await Click(loop.restoreButton);
            await Click(loop.resetButton);
            await Click(loop.loadButton);
            await Prepare();
            await Align();
            await DryScan();
            await Click(loop.printButton);
            await Finish(2);
            await At(5);

            await Click(loop.loadButton);
            await Prepare();
            await Align();
            await DryScan();
            await Click(loop.printButton);
            await Click(loop.armButton);
            int interrupted=Read<int>(loop,"field");
            Check(loop.contacts[0].IsClosed&&plant.Shutter>.85f&&plant.Speed>1000,"Shutter permit or live motion absent before physical disconnection.");
            Check(wirePaths!=null&&wirePaths.UndoLastWire(),"Could not disconnect W102 through wire history.");
            await Wait(()=>loop.statusText.text.Contains("POSITION FEEDBACK INVALID")&&loop.contacts[0].IsOpen,"position invalidation and shutter relay open");
            await Wait(()=>manager.ElectricalSolver.LatestSolveResult.TryGetVoltage(pins["SHUTTER_ENABLE"],pins["PLC_GND"],out float v)&&v<.5f,"solved shutter permit below 0.5 V");
            Check(!loop.printButton.interactable&&!loop.resetButton.interactable&&!Read<bool>(loop,"validPosition")&&manager.SatisfiedTargetCount==5&&
                plant.Fields[interrupted].State==SparkLevel74Plant.FieldState.Reject,"W102 did not reject the active field, preserve 5/6 or block restart.");
            await Wait(()=>plant.Shutter<.02f&&plant.Speed<8,"physical shutter close and braked stage");
            double before=Read<double>(loop,"measuredStop"),after=plant.X;
            Check(Math.Abs(before-after)>20&&Math.Abs(after)>100,"Stage failed to coast from the recorded position.");
            Record($"W102 physically disconnected. Field {interrupted+1:00} rejected, shutter permit below 0.5 V, 5/6 preserved. Stage braked from {before:F1} to {after:F1} nm.");
            Capture(bench,pins,"SensorFault",false);
            camera.enabled=true;
            Connect(map[101].a,map[101].b);
            camera.enabled=false;
            await Click(loop.recalButton);
            await Click(loop.referenceButton);
            await Click(loop.resetButton);
            await Click(loop.loadButton);
            await Prepare();
            await Align();
            await DryScan();
            await Click(loop.printButton);
            await Finish(3);
            await Click(loop.certifyButton);
            await At(6);
            await Wait(()=>manager.IsCompleted,"campaign completion");
            Record("6/6 — Campaign manager reports Completed after W102 reconnection, position calibration, re-referencing and a fresh patterned wafer.");
            Capture(bench,pins,"Verified",false);

        }
        catch (Exception error)
        {
            Record("FAIL: " + error);
            if (loop != null) Record("Control panel: " + loop.statusText.text + " | " + loop.meterText.text + " | " + loop.detailText.text);
            if (loop != null) Record($"Electrical state: 5 V request {Read<bool>(loop, "power")}, solved rails {Read<bool>(loop, "rails")}, e-stop loop {Read<bool>(loop, "safety")}, position valid {Read<bool>(loop, "validPosition")}, first wiring issue {Read<string>(loop, "wiring")}.");
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
            "Missing Level 74 action on " + button.name);
        button.onClick.Invoke();
    }

    private static void Capture(Transform bench, Dictionary<string, SparkTerminal> pins, string name, bool overhead)
    {
        var view = new GameObject("Level 74 test camera");
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
            File.WriteAllBytes("Logs/Level74_" + name + ".png", snapshot.EncodeToPNG());
            if (!overhead) return;
            var rows = new List<string> { "Terminal,X,Y" };

            foreach (var pair in pins)
            {
                var p = camera.WorldToViewportPoint(pair.Value.transform.position);
                rows.Add($"{pair.Key},{Mathf.RoundToInt(p.x * width)},{Mathf.RoundToInt((1f - p.y) * height)}");
            }
            File.WriteAllLines("Logs/Level74_Pins.csv", rows);
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
