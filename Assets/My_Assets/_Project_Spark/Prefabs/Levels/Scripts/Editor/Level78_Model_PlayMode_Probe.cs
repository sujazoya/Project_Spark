#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AAAUI.VFX;
using Unity.Collections;
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
internal static class Level78ModelPlayModeProbe
{
    private const string Request = "Logs/Level78ModelTest.request";
    private const string Report = "Logs/Level78ModelTest.txt";
    private const string Running = "Codex.Level78.ModelTest";
    private static LevelGamePlayManager manager;
    private static SparkLevel78Process loop;

    static Level78ModelPlayModeProbe()
    {
        EditorApplication.update += CheckRequest;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    private static void CheckRequest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(Request)) return;
        try { File.Delete(Request); }
        catch (IOException) { return; }
        File.WriteAllText(Report, "Level 78 — Equipment workbench and Seal the Signal test\n");
        try
        {
            Check(SceneManager.GetActiveScene().path == SparkLevel78ModelInstaller.ScenePath, "Open MainMenu in Edit Mode.");
            SparkLevel78ModelInstaller.Fit();
            SparkLevel78ModelInstaller.ValidateAssets();
            Record("Fitted and validated the existing Level 78 campaign prefab.");
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
        manager = null;
        loop = null;
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
            var registry = Object.FindFirstObjectByType<SparkWireRegistry>();
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel78ModelInstaller.Folder + "Level_78.asset");
            Check(definition != null && wireBuilder != null && loader != null, "Level definition, wire builder, or layout loader missing.");
            if (manager.LevelProgression != null) manager.LevelProgression.enabled = false;
            Write(manager, "enforceProgressionAccess", false);
            Write(manager, "autoAdvanceOnCompletion", false);
            Write(manager, "autoEvaluate", true);
            int levelIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), definition);
            Check(levelIndex >= 0 && manager.StartLevel(levelIndex), "Level 78 is missing from the campaign or could not start.");
            await Wait(() => loader.ActiveLayout != null && manager.ActiveLevel == definition, "Level 78 loaded");
            loop = loader.ActiveLayout.GetComponent<SparkLevel78Process>();
            await Wait(() => loop != null && Read<int>(loop, "topology") >= 0,
                "the active loop controller to check the empty circuit");

            var bench = loop.transform.Find("Dielectric Workbench");
            var camera = Read<Camera>(wireBuilder, "targetCamera");
            var ids = SparkLevel78ModelInstaller.TerminalIds.ToArray();
            var pins = new Dictionary<string, SparkTerminal>();
            var table = SparkEquipmentWorkbenchFit.FindTable(SceneManager.GetActiveScene());
            var art = loop.rig.art;
            var fixture = SparkEquipmentWorkbenchFit.MeasureArt(art, table.transform.worldToLocalMatrix);
            Vector3 tableMin = table.center - table.size * .5f;
            Vector3 tableMax = table.center + table.size * .5f;
            var tableSize = Vector3.Scale(table.size, table.transform.lossyScale);
            var fixtureSize = Vector3.Scale(fixture.size, table.transform.lossyScale);
            Check(fixture.min.x > tableMin.x && fixture.max.x < tableMax.x &&
                fixture.min.z > tableMin.z && fixture.max.z < tableMax.z &&
                Mathf.Max(fixtureSize.x / tableSize.x, fixtureSize.z / tableSize.z) > .86f,
                "Level 78 fixture must fill the table without overhang.");
            Record($"Fixture fits the workshop table: {fixtureSize.x:F2} × {fixtureSize.z:F2} on {tableSize.x:F2} × {tableSize.z:F2} Unity units.");

            var mounts = SparkLevel78ModelInstaller.ContactRows("Equipment_Terminal_Mounts.csv").ToDictionary(row => row[5]);
            const string meterOut = "DIELECTRIC_LEAKAGE_ELECTROMETER_OUT";
            const string controllerIn = "DIELECTRIC_RELIABILITY_CONTROLLER_LEAKAGE_IN";
            Check(mounts[meterOut][1] == "L78_DielectricLeakageCurrentElectrometer" && mounts[meterOut][6] == "Modeled" &&
                mounts[controllerIn][1] == "L78_DielectricReliabilityController" && mounts[controllerIn][6] == "Modeled",
                "W083 must use original molded meter and controller sockets.");

            var used = new HashSet<Vector3>();
            float maxGap = 0f;
            foreach (string id in ids)
            {
                Check(manager.SceneBindings.TryGetTerminal(id, out var terminal), "Missing " + id);
                pins.Add(id, terminal);
                var socket = terminal.transform;
                var row = mounts[id];
                Vector3 expected = art.TransformPoint(SparkLevel78ModelInstaller.MountPosition(row));
                maxGap = Mathf.Max(maxGap, Vector3.Distance(socket.position, expected));
                Check(used.Add(socket.position) && socket.parent.parent == art.Find(row[1]) &&
                    terminal.TryGetComponent<BoxCollider>(out var collider) && collider.enabled &&
                    socket.Cast<Transform>().Count(child => child.name == "Contact face") == 1 &&
                    socket.Cast<Transform>().Count(child => child.name == "Panel socket housing") == 1 &&
                    !socket.Find("Jack recess"),
                    "Duplicate, misplaced or nonfunctional socket: " + id);
            }
            Check(maxGap < .0002f && used.Count == 182 &&
                art.GetComponentsInChildren<SparkTerminal>(true).Length == 182 &&
                loop.GetComponentsInChildren<SparkTerminal>(true).Length == 182 &&
                !bench.Find("Breadboard Labels") && !bench.Find("Temporary board"), "Unexpected terminal or old board.");
            Check(loop.pinLabels.Length == 182 && loop.pinLabels.Count(label => label.gameObject.activeSelf) <= 3,
                "Only relevant terminal labels should be visible.");
            Check(loop.plot.TryGetComponent<CanvasRenderer>(out _) && loop.plot.rectTransform.anchorMax == Vector2.one,
                "The live 16-region map must be visible beside the equipment.");
            Check(wireBuilder.DrawingMode == WireDrawingMode.TerminalToTerminal, "Level 78 must use direct equipment wiring.");
            Record($"182/182 functional sockets at the equipment coordinates. Maximum mount error: {maxGap:F6} Unity units.");
            VerifyMoldedContacts(art, mounts.Values);
            Capture(camera, pins, "Unwired", true);
            var covered = new List<string>();
            foreach (var pair in pins)
            {
                Vector3 point = camera.WorldToScreenPoint(pair.Value.transform.position);
                Check(point.z > 0 && point.x >= 0 && point.y >= 0 && point.x < camera.pixelWidth && point.y < camera.pixelHeight,
                    $"Terminal outside the active camera: {pair.Key} at {point}; camera {camera.pixelWidth}x{camera.pixelHeight}");
                var pixel = new Vector2(Mathf.Round(point.x), Mathf.Round(point.y));
                Check(wireBuilder.FindStartTerminal(pixel) == pair.Value,
                    "Wire tool cannot select " + pair.Key + " from the active game camera.");
                if (EventSystem.current)
                {
                    var hits=new List<RaycastResult>();
                    EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);
                    if(hits.Any(hit=>hit.gameObject.transform.IsChildOf(loop.powerButton.transform.parent))) covered.Add(pair.Key);
                }
            }
            Record("182/182 sockets picked by the real Wire tool from the active game camera.");
            Record("Sockets overlapped by Level 78 buttons in the tested camera view: "+(covered.Count==0?"none":string.Join(", ",covered))+".");
            void Connect(int from, int to, WirePolarity polarity = WirePolarity.Neutral)
            {
                Check(wireBuilder.BeginWire(pins[ids[from]], polarity), "Wire tool could not start at " + ids[from]);
                Check(wireBuilder.TryEndWire(camera.WorldToScreenPoint(pins[ids[to]].transform.position)),
                    "Wire tool could not finish " + ids[from] + " to " + ids[to]);
            }






            Check(covered.Count==0,"Move the control overlay away from these terminals: "+string.Join(", ",covered));
            var map=SparkLevel78Process.Wires;
            const int feedback=82;
            var overview = camera.transform.position;
            await Click(loop.focusWireButton);
            Check(Vector3.Distance(overview, camera.transform.position) > .1f, "Focus Wire did not navigate to the next pair.");
            var viewControl = loop.GetComponent<SparkEquipmentWorkbenchView>();
            viewControl.ResetView();
            await Wait(() => Vector3.Distance(overview, camera.transform.position) < .0001f, "reset workbench view");
            Record("Focus Wire frames the pending connection and Reset View restores the fixture overview.");
            await Click(loop.powerButton);
            for(int i=0;i<map.Length;i++)
            {
                if(i==feedback) continue;
                var (from,to)=map[i];
                Connect(from,to,from==0?WirePolarity.Positive:from==2?WirePolarity.Negative:WirePolarity.Neutral);
            }
            await Wait(()=>loop.statusText.text.Contains("W083"),"missing direct electrometer feedback");
            Check(!loop.pumpButton.interactable&&manager.SatisfiedTargetCount==0,"Instrument health bypassed W083.");
            Record("Connected the other 117 real wires. W084 instrument health and telemetry could not replace W083.");
            Connect(map[feedback].a,map[feedback].b);
            await At(1);
            Check(registry != null, "Native wire registry is missing.");
            registry.RefreshNow();
            Check(loop.cableViewButton.interactable && registry.Wires.Count == 118, "The native registry must contain all 118 wires.");
            var paths = registry.Wires.Select(record => record.Path).Where(path => path).ToArray();
            int VisiblePaths() => paths.Count(path => path.GetComponentsInChildren<Renderer>().Any(renderer => !renderer.forceRenderingOff));
            Check(VisiblePaths() < 118, "Equipment view should declutter installed wiring.");
            await Click(loop.cableViewButton);
            Check(VisiblePaths() == 118 && registry.Wires.Count == 118, "Cable View must reveal every installed cable.");
            await Click(loop.cableViewButton);
            Record("Cable View declutters the workbench while preserving all 118 native wire connections.");
            Time.captureDeltaTime=.10f;
            camera.enabled=false;
            var plant=Read<SparkLevel78Plant>(loop,"plant");
            bool Flag(string key)=>Read<bool>(loop,key);
            async Task Load()
            {
                await Click(loop.loadButton);
                await Click(loop.inspectButton);
                await Click(loop.prepButton);
            }
            async Task Seal(bool snapshot=false)
            {
                await Click(loop.capButton);
                if(snapshot)
                {
                    await Wait(()=>plant.MeanCap>5&&plant.RF>.7f,"physical cap growth");
                    Capture(camera,pins,"Running",false);
                }
                await Click(loop.capInspectButton);
            }
            async Task StartCure()
            {
                await Click(loop.lowKButton);
                await Click(loop.cureButton);
            }
            async Task StartScreen()
            {
                await Click(loop.metroButton);
                await Click(loop.stressButton);
            }
            async Task Wafer()
            {
                await Load();await Seal();await StartCure();await StartScreen();
            }
            async Task ReadQualified(int number)
            {
                await Wait(()=>Read<int>(loop,"acceptedBatches")==number&&plant.Field<.025f,"qualified wafer and discharged coupon");
                Record($"Wafer {number}: cap {plant.MeanCap:F2} nm, pinholes {plant.Pinhole:P2}, film {plant.MeanFilm:F2} nm, nonuniformity {plant.Uniformity:P2}, k {plant.Dielectric:F3}, moisture {plant.Humidity:P2}, leakage {plant.LeakageIndex:F3}.");
            }
            async Task Recover()
            {
                await Click(loop.restoreButton);
                await Click(loop.resetButton);
                await Click(loop.discardButton);
            }

            await Click(loop.pumpButton);
            await Wait(()=>plant.VacuumReady,"backing and turbo vacuum");
            await Click(loop.calGasButton);
            await Click(loop.calRfButton);
            await Click(loop.calMeterButton);
            await Wait(()=>Flag("gasCal")&&Flag("rfCal")&&Flag("validLeak"),"gas, RF and electrometer calibration");
            await Load();
            await At(2);
            await Seal(true);await At(3);
            await StartCure();await StartScreen();await ReadQualified(1);await At(4);

            await Load();await Click(loop.capButton);
            await Click(loop.gasFaultButton);
            await Wait(()=>Flag("gasPassed"),"cap delivery fault");
            Check(Read<float>(loop,"gasAtTrip")<.64f,"Flow imbalance did not physically change.");
            await Recover();

            await Load();await Seal();await Click(loop.lowKButton);
            await Click(loop.chargeFaultButton);
            await Wait(()=>Flag("chargePassed"),"charge protection");
            Check(Read<float>(loop,"chargeAtTrip")>.34f,"RF fault did not build wafer charge.");
            await Recover();

            await Load();await Seal();await StartCure();
            await Click(loop.uvFaultButton);
            await Wait(()=>Flag("uvPassed"),"cure shutter protection");
            Check(Read<float>(loop,"shutterAtTrip")<.43f,"UV fault did not restrict the shutter.");
            await Recover();
            await Wafer();await ReadQualified(2);await At(5);
            Record("Measured flow asymmetry, RF charging and UV shutter faults latched separately. A fresh wafer passed after recovery.");

            await Wafer();await Click(loop.armButton);
            Check(loop.contacts[0].IsClosed&&plant.Field>.66f,"Coupon stress was not live before W083 removal.");
            await Click(loop.openFeedbackButton);
            await Wait(()=>!Flag("validLeak")&&loop.contacts[0].IsOpen,"W083 invalidation and source relay open");
            float atTrip=Read<float>(loop,"fieldAtTrip");
            Check(atTrip>.63f&&plant.Field>.025f&&plant.Field<=atTrip,"The stored fixture field disappeared at disconnect.");
            Check(!loop.stressButton.interactable&&!loop.resetButton.interactable&&Flag("feedbackObserved")&&manager.SatisfiedTargetCount==5,
                "W083 must invalidate the meter, trip protection and preserve the first five objectives.");
            await Wait(()=>manager.ElectricalSolver.LatestSolveResult.TryGetVoltage(pins["STRESS_ENABLE"],pins["PLC_GND"],out float v)&&v<.5f,"fixture permit below 0.5 V");
            await Wait(()=>plant.Field<.025f,"stored field discharged");
            float exposure=plant.Exposure,t0=Read<float>(loop,"elapsed");
            Check(!Flag("screened"),"Interrupted coupon was accepted.");
            await Wait(()=>Read<float>(loop,"elapsed")>t0+.35f,"stress exposure stopped");
            Check(plant.Exposure==exposure,"Coupon exposure continued after the trip.");
            Record($"W083 removed at field {atTrip:F3}. Permit fell below 0.5 V, fixture discharged, exposure stopped and 5/6 remained.");
            Capture(camera,pins,"SensorFault",false);
            camera.enabled=true;
            Connect(map[feedback].a,map[feedback].b);
            camera.enabled=false;
            await Click(loop.calMeterButton);
            await Click(loop.verifyButton);
            await Click(loop.resetButton);
            await Click(loop.discardButton);
            await Wafer();await ReadQualified(3);
            await Click(loop.certifyButton);
            await At(6);
            await Wait(()=>manager.IsCompleted,"campaign completion");
            Record("6/6 — Campaign manager reports Completed after W083 reconnection, electrometer calibration, explicit reset and a fresh wafer and coupon.");
            camera.enabled = true;
            Capture(camera,pins,"Verified",false);
            string screenPath = "Logs/Level78_GameView.png";
            DateTime beforeCapture = DateTime.UtcNow;
            ScreenCapture.CaptureScreenshot(screenPath);
            await Wait(() => File.Exists(screenPath) && File.GetLastWriteTimeUtc(screenPath) >= beforeCapture.AddSeconds(-1), "saved Game View screenshot");
        }
        catch (Exception error)
        {
            Record("FAIL: " + error);
            if (loop != null) Record("Control panel: " + loop.statusText.text + " | " + loop.meterText.text + " | " + loop.detailText.text);
            if (loop != null) Record($"Electrical state: 5 V request {Read<bool>(loop, "power")}, solved rails {Read<bool>(loop, "rails")}, e-stop loop {Read<bool>(loop, "safety")}, leakage valid {Read<bool>(loop, "validLeak")}, first wiring issue {Read<string>(loop, "wiring")}.");
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

    private static void VerifyMoldedContacts(Transform art, IEnumerable<string[]> mounts)
    {
        var taken = mounts.Where(row => row[6] == "Modeled").ToArray();
        var baked = SparkLevel78ModelInstaller.ContactRows("Molded_Contact_Centers.csv")
            .Select(row => new[] { "", row[0], row[1], row[2], row[3], "", "Modeled" }).ToArray();
        foreach (var module in baked.GroupBy(row => row[1]))
        {
            var copper = new List<Vector3>();
            foreach (var filter in art.Find(module.Key).GetComponentsInChildren<MeshFilter>()
                .Where(filter => filter.name.EndsWith("tintDC803E", StringComparison.Ordinal)))
            {
                using var data = MeshUtility.AcquireReadOnlyMeshData(filter.sharedMesh);
                using var vertices = new NativeArray<Vector3>(data[0].vertexCount, Allocator.Temp);
                data[0].GetVertices(vertices);
                var toArt = art.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                for (int sub = 0; sub < data[0].subMeshCount; sub++)
                {
                    using var indices = new NativeArray<int>(data[0].GetSubMesh(sub).indexCount, Allocator.Temp);
                    data[0].GetIndices(indices, sub);
                    foreach (int index in indices) copper.Add(toArt.MultiplyPoint3x4(vertices[index]));
                }
            }
            foreach (var row in module)
            {
                Vector3 center = SparkLevel78ModelInstaller.MountPosition(row);
                Check(copper.All(point => Vector3.Distance(point, center) >= .0032f),
                    $"Copper from an old molded contact remains at {row[1]} {center}.");
            }
        }
        Check(taken.All(row => baked.Any(source => source.Skip(1).Take(4).SequenceEqual(row.Skip(1).Take(4)))),
            "Each molded terminal must use a physical source contact center.");
        Record($"{taken.Length} functional objects use original molded positions, and {baked.Length - taken.Length} unused molded contacts were removed.");
    }

    private static async Task At(int stage)
    {
        await Wait(() => Read<int>(loop, "stage") == stage && manager.SatisfiedTargetCount == stage, $"{stage}/6");
        Record($"{stage}/6 — {loop.statusText.text} | {loop.meterText.text}");
    }

    private static async Task Click(Button button)
    {
        await Wait(() => button.isActiveAndEnabled && button.interactable, button.name + " available");
        Check(button.onClick.GetPersistentEventCount() == 1 && button.onClick.GetPersistentTarget(0) == loop,
            "Missing Level 78 action on " + button.name);
        int frame = Time.frameCount;
        button.onClick.Invoke();
        await Wait(() => Time.frameCount > frame, "next frame after " + button.name);
    }

    private static void Capture(Camera source, Dictionary<string, SparkTerminal> pins, string name, bool overhead)
    {
        var view = new GameObject("Level 78 test camera");
        var camera = view.AddComponent<Camera>();
        camera.CopyFrom(source);
        camera.enabled = false;
        view.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
        const int width = 1600, height = 900;
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
            File.WriteAllBytes("Logs/Level78_" + name + ".png", snapshot.EncodeToPNG());
            if (!overhead) return;
            var rows = new List<string> { "Terminal,X,Y" };

            foreach (var pair in pins)
            {
                var p = camera.WorldToViewportPoint(pair.Value.transform.position);
                rows.Add($"{pair.Key},{Mathf.RoundToInt(p.x * width)},{Mathf.RoundToInt((1f - p.y) * height)}");
            }
            File.WriteAllLines("Logs/Level78_Pins.csv", rows);
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
        var timeout = DateTime.UtcNow.AddSeconds(78);
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




