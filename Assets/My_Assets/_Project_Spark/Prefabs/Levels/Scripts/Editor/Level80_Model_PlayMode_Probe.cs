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
        File.WriteAllText(Report, "Level 80 — Equipment-mounted terminals and Command the Light test\n");
        try
        {
            Check(SceneManager.GetActiveScene().path == SparkLevel80ModelInstaller.ScenePath, "Open MainMenu in Edit Mode.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SparkLevel80ModelInstaller.Prefab) != null)
                SparkLevel80ModelInstaller.Fit();
            SparkLevel80ModelInstaller.ValidateAssets();
            Record("Fitted the existing Level 80 campaign prefab to the workshop table.");
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
            var registry = Object.FindFirstObjectByType<SparkWireRegistry>();
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel80ModelInstaller.Folder + "Level_80.asset");
            Check(definition != null && wireBuilder != null && loader != null && registry != null, "Level definition, wire builder, or layout loader missing.");
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
            var table = SparkEquipmentWorkbenchFit.FindTable(SceneManager.GetActiveScene());
            var ids = SparkLevel80ModelInstaller.TerminalIds.ToArray();
            var pins = new Dictionary<string, SparkTerminal>();
            var mounts = bench.Find(SparkLevel80ModelInstaller.MountsName);
            var art = loop.rig.art;
            Check(mounts && !bench.Find("Breadboard Labels"), "The old freestanding jack panel is still present.");
            var tableLocal = SparkEquipmentWorkbenchFit.MeasureArt(art, table.transform.worldToLocalMatrix);
            var worldTable = Vector3.Scale(table.size, table.transform.lossyScale);
            var worldFixture = Vector3.Scale(tableLocal.size, table.transform.lossyScale);
            if (worldFixture.x >= worldTable.x || worldFixture.z >= worldTable.z)
            {
                var modules = art.Cast<Transform>().Where(t => t.GetComponentInChildren<MeshFilter>());
                foreach (var module in modules.Select(t => (t, bounds: SparkEquipmentWorkbenchFit.MeasureArt(t, table.transform.worldToLocalMatrix)))
                    .OrderByDescending(m => Mathf.Abs(m.bounds.center.x) + m.bounds.extents.x).Take(4))
                    Record($"Outermost module {module.t.name}: {module.bounds}.");
            }
            Check(worldFixture.x < worldTable.x && worldFixture.z < worldTable.z &&
                Mathf.Max(worldFixture.x / worldTable.x, worldFixture.z / worldTable.z) > .86f,
                $"Fixture does not fill the table correctly: {worldFixture.x:F2} × {worldFixture.z:F2} on {worldTable.x:F2} × {worldTable.z:F2}.");
            Record($"Fixture fits the workshop table: {worldFixture.x:F2} × {worldFixture.z:F2} on {worldTable.x:F2} × {worldTable.z:F2} Unity units.");

            var source = AssetDatabase.LoadAssetAtPath<TextAsset>(SparkLevel80ModelInstaller.Folder + "Equipment_Terminal_Mounts.csv");
            var positions = source.text.Split('\n').Skip(1).Where(row => !string.IsNullOrWhiteSpace(row))
                .Select(row => row.Trim().Split(',')).ToDictionary(row => row[5]);
            var meshVertices = new Dictionary<string, List<Vector3>>();
            foreach (string equipment in positions.Values.Where(row => row[6] == "Modeled").Select(row => row[1]).Distinct())
            {
                var gold = new List<Vector3>();
                foreach (var filter in art.Find(equipment).GetComponentsInChildren<MeshFilter>()
                    .Where(filter => filter.sharedMesh && !filter.name.StartsWith("Panel socket", StringComparison.Ordinal) && filter.name != "Contact insert"))
                {
                    using var data = MeshUtility.AcquireReadOnlyMeshData(filter.sharedMesh);
                    using var vertices = new Unity.Collections.NativeArray<Vector3>(data[0].vertexCount, Unity.Collections.Allocator.Temp);
                    data[0].GetVertices(vertices);
                    var toArt = art.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                    foreach (var vertex in vertices) gold.Add(toArt.MultiplyPoint3x4(vertex));
                }
                meshVertices.Add(equipment, gold);
            }
            var unique = new HashSet<Vector3>();
            float maxError = 0;
            var socketGaps = new List<(string id, float distance)>();
            Physics.SyncTransforms();
            foreach (string id in ids)
            {
                Check(manager.SceneBindings.TryGetTerminal(id, out var terminal), "Missing " + id);
                pins.Add(id, terminal);
                var socket = terminal.transform;
                var row = positions[id];
                float Coordinate(int index) => float.Parse(row[index], System.Globalization.CultureInfo.InvariantCulture);
                Vector3 expected = art.TransformPoint(SparkEquipmentWorkbenchFit.SourceArtUnit * new Vector3(Coordinate(2), Coordinate(3), -Coordinate(4)));
                Vector3 axis = art.InverseTransformPoint(socket.position);
                if (row[6] == "Modeled")
                {
                    var vertices = meshVertices[row[1]];
                    float gap = vertices.Count == 0 ? float.PositiveInfinity : Mathf.Sqrt(vertices.Min(vertex => (axis - vertex).sqrMagnitude));
                    socketGaps.Add((id, gap));
                    Check(!socket.parent.Find("Panel socket housing"), "A modeled socket has duplicate geometry: " + id);
                }
                else Check(socket.parent.Find("Panel socket housing"), "No collar at the source-planned mount: " + id);
                float error = Vector3.Distance(expected, socket.parent.position);
                maxError = Mathf.Max(maxError, error);
                Check(socket.IsChildOf(art) && socket.parent.name.StartsWith(row[0] + " ", StringComparison.Ordinal)
                    && socket.parent.parent == art.Find(row[1]) && error < .0002f,
                    $"Terminal {id} does not match its original equipment position. Gap {error:F5}.");
                Check(unique.Add(socket.position) && socket.TryGetComponent<BoxCollider>(out var collider)
                    && collider.enabled && socket.gameObject.layer == LayerMask.NameToLayer("Pin") &&
                    !socket.GetComponent<Renderer>() && !socket.Find("Jack recess"),
                    "Duplicate or standalone terminal visual found at " + id);
            }
            var offCollar = socketGaps.Where(item => item.distance > .009f).Select(item => $"{item.id} {item.distance:F5}").ToArray();
            Check(offCollar.Length == 0, "Terminals without matching imported socket geometry: " + string.Join(", ", offCollar));
            Record($"{socketGaps.Count} terminals coincide with the original imported contact geometry. The remaining {positions.Count - socketGaps.Count} use source-planned attachment points.");
            Check(unique.Count == 194 && art.GetComponentsInChildren<SparkTerminal>(true).Length == 194
                && loop.GetComponentsInChildren<SparkTerminal>(true).Length == 194,
                "Expected exactly 194 functional sockets with no extra board terminals.");
            Record($"194/194 unique terminals match original equipment coordinates. Maximum mount error {maxError:F6} Unity units.");
            Check(loop.pinLabels.Length == 194 && loop.pinLabels.Count(label => label.gameObject.activeSelf) <= 3,
                "Only relevant socket labels should be shown during wiring.");
            Check(loop.plot.TryGetComponent<CanvasRenderer>(out _) && loop.plot.rectTransform.anchorMax == Vector2.one,
                "The mission measurement display should render beside the fixture.");
            Check(wireBuilder.DrawingMode == WireDrawingMode.TerminalToTerminal, "MainMenu must use direct terminal-to-terminal wiring.");
            var shelf = GameObject.Find("ProjectSpark_Gameplay")?.transform.Find("Component_Shelf");
            Check(shelf && shelf.GetComponentsInChildren<Renderer>().All(renderer => renderer.forceRenderingOff),
                "The component shelf obscures the front of the fitted workbench.");
            Capture(camera, pins, "Unwired", true);
            var covered = new List<string>();
            foreach (var pair in pins)
            {
                Vector3 point = camera.WorldToScreenPoint(pair.Value.transform.position);
                Check(point.z > 0 && point.x >= 0 && point.y >= 0 && point.x < camera.pixelWidth && point.y < camera.pixelHeight,
                    $"Terminal outside the active camera: {pair.Key} at {point}; camera {camera.pixelWidth}x{camera.pixelHeight}");
                Check(wireBuilder.FindStartTerminal(point) == pair.Value,
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
            float overview = camera.orthographicSize;
            await Click(loop.focusWireButton);
            await Wait(() => camera.orthographicSize < overview * .95f, "close-up of the next wire");
            var firstWire = SparkLevel80Process.Wires[0];
            foreach (int endpoint in new[] {firstWire.a, firstWire.b})
            {
                var point = camera.WorldToViewportPoint(loop.pins[endpoint].transform.position);
                Check(point.x is > .05f and < .95f && point.y is > .29f and < .95f,
                    "Focus Wire must frame both endpoints above the level controls.");
            }
            Capture(camera, pins, "FirstWire", false);
            loop.ResetWorkbenchView();
            await Wait(() => Mathf.Abs(camera.orthographicSize - overview) < .001f, "restored overview");
            Record("Focus Wire frames both original sockets and Reset View restores the overview.");
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
            Check(surfaces==9,$"Expected nine authored surface families; found {surfaces}.");
            Record("Nine authored PBR surface families applied.");
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
            registry.RefreshNow();
            var laterWire = registry.Wires.Last();
            Check(registry.RemoveWire(laterWire.WireId), "Could not prepare a distinct cable for the W078 removal test.");
            Connect(map[feedback].a,map[feedback].b);
            Check(registry.RestoreRemovedWire(laterWire.WireId), "Could not reinstall the other cable after W078.");
            registry.SelectWire(null);
            await At(1);
            var hidden = Read<Dictionary<Renderer, bool>>(loop, "hiddenCableRenderers");
            Check(hidden.Count > 0 && hidden.Keys.All(renderer => renderer.forceRenderingOff), "The equipment view still shows ordinary cables.");
            await Click(loop.cableViewButton);
            Check(hidden.Count == 0 && registry.Count == 126, "Cable visibility disturbed the native connections.");
            await Click(loop.cableViewButton);
            Record("Equipment view hides ordinary cable geometry while preserving all 126 native connections.");
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
            Capture(camera,pins,"Running",false);

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
            await Click(loop.openFeedbackButton);
            Check(registry.Count == 125 && registry.TryGetWire(laterWire.WireId, out _),
                "OPEN W078 removed the wrong connection or did not remove W078.");
            await Wait(()=>!Flag("rxValid")&&loop.contacts[0].IsOpen,"W078 invalidation and source relay open");
            float atTrip=Read<float>(loop,"launchAtTrip");
            Check(atTrip>.76f&&plant.Launch>.025f&&plant.Launch<=atTrip,"Source output disappeared instantaneously.");
            Check(!loop.testButton.interactable&&!loop.resetButton.interactable&&Flag("feedbackObserved")&&manager.SatisfiedTargetCount==5,
                "W078 must invalidate receiver samples and preserve completed history.");
            await Wait(()=>manager.ElectricalSolver.LatestSolveResult.TryGetVoltage(pins["LASER_ENABLE"],pins["PLC_GND"],out float v)&&v<.5f,"source permit below 0.5 V");
            await Wait(()=>plant.SafelyClosed&&plant.ThroughPower<.0001f,"both shutters closed and output decayed");
            Check(!Flag("sending")&&plant.Bits.All(n=>n==0),"Stale data retained qualification after W078 removal.");
            Record($"W078 opened at normalized output {atTrip:F3}. Permit fell below 0.5 V, both shutter mechanisms closed, optical output decayed and 5/6 remained.");
            Capture(camera,pins,"SensorFault",false);
            camera.enabled=true;
            Connect(map[feedback].a,map[feedback].b);
            camera.enabled=false;
            await Click(loop.calRxButton);await Click(loop.verifyButton);await Click(loop.resetButton);
            await Link();await RunData(3);await Click(loop.certifyButton);
            await At(6);await Wait(()=>manager.IsCompleted,"campaign completion");
            Record("6/6 — Campaign manager reports Completed after W078 reconnection, independent shutdown verification, receiver recalibration, explicit reset and fresh four-channel data.");
            Capture(camera,pins,"Verified",false);
            camera.enabled = true;
            string gameView = Path.GetFullPath("Logs/Level80_GameView.png");
            File.Delete(gameView);
            ScreenCapture.CaptureScreenshot(gameView);
            await Wait(() => File.Exists(gameView), "completed Game View screenshot");
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

    private static void Capture(Camera source, Dictionary<string, SparkTerminal> pins, string name, bool recordPinPositions)
    {
        var view = new GameObject("Level 81 test camera");
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
            File.WriteAllBytes("Logs/Level80_" + name + ".png", snapshot.EncodeToPNG());
            if (!recordPinPositions) return;
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
