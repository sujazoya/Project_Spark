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
internal static class Level82ModelPlayModeProbe
{
    private const string Request = "Logs/Level82ModelTest.request";
    private const string Report = "Logs/Level82ModelTest.txt";
    private const string Running = "Codex.Level82.ModelTest";
    private static LevelGamePlayManager manager;
    private static SparkLevel82Process loop;

    static Level82ModelPlayModeProbe()
    {
        EditorApplication.update += CheckRequest;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    private static void CheckRequest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(Request)) return;
        try { File.Delete(Request); }
        catch (IOException) { return; }
        File.WriteAllText(Report, "Level 82 — Equipment-mounted terminals and Silence the Spectrum test\n");
        try
        {
            Check(SceneManager.GetActiveScene().path == SparkLevel82ModelInstaller.ScenePath, "Open MainMenu in Edit Mode.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SparkLevel82ModelInstaller.Prefab) != null)
                SparkLevel82ModelInstaller.Fit();
            SparkLevel82ModelInstaller.ValidateAssets();
            Record("Fitted the existing campaign prefab to the workshop table.");
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
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel82ModelInstaller.Folder + "Level_82.asset");
            Check(definition != null && wireBuilder != null && loader != null && registry != null,
                "Level definition, native wiring, or layout loader missing.");
            if (manager.LevelProgression != null) manager.LevelProgression.enabled = false;
            Write(manager, "enforceProgressionAccess", false);
            Write(manager, "autoAdvanceOnCompletion", false);
            Write(manager, "autoEvaluate", true);
            int levelIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), definition);
            Check(levelIndex >= 0 && manager.StartLevel(levelIndex), "Level 82 is missing from the campaign or could not start.");
            await Wait(() => loader.ActiveLayout != null && manager.ActiveLevel == definition, "Level 82 loaded");
            loop = loader.ActiveLayout.GetComponent<SparkLevel82Process>();
            await Wait(() => loop != null && Read<int>(loop, "topology") >= 0,
                "the active loop controller to check the empty circuit");

            var bench = loop.transform.Find("EMC Workbench");
            var camera = Read<Camera>(wireBuilder, "targetCamera");
            var table = SparkLevel82ModelInstaller.FindTable(SceneManager.GetActiveScene());
            var ids = SparkLevel82ModelInstaller.TerminalIds.ToArray();
            var pins = new Dictionary<string, SparkTerminal>();
            var mounts = bench.Find(SparkLevel82ModelInstaller.MountsName);
            var art = loop.rig.art;
            Check(mounts && !bench.Find("Breadboard Labels"), "The old freestanding jack panel is still present.");
            var tableLocal = SparkLevel82ModelInstaller.MeasureArt(art, table.transform.worldToLocalMatrix);
            var worldTable = Vector3.Scale(table.size, table.transform.lossyScale);
            var worldFixture = Vector3.Scale(tableLocal.size, table.transform.lossyScale);
            if (worldFixture.x >= worldTable.x || worldFixture.z >= worldTable.z)
            {
                var modules = art.Cast<Transform>().Where(t => t.GetComponentInChildren<MeshFilter>());
                foreach (var module in modules.Select(t => (t, bounds: SparkLevel82ModelInstaller.MeasureArt(t, table.transform.worldToLocalMatrix)))
                    .OrderByDescending(m => Mathf.Abs(m.bounds.center.x) + m.bounds.extents.x).Take(4))
                    Record($"Outermost module {module.t.name}: {module.bounds}.");
            }
            Check(worldFixture.x < worldTable.x && worldFixture.z < worldTable.z &&
                Mathf.Max(worldFixture.x / worldTable.x, worldFixture.z / worldTable.z) > .86f,
                $"Fixture does not fill the table correctly: {worldFixture.x:F2} × {worldFixture.z:F2} on {worldTable.x:F2} × {worldTable.z:F2}.");
            Record($"Fixture fits the workshop table: {worldFixture.x:F2} × {worldFixture.z:F2} on {worldTable.x:F2} × {worldTable.z:F2} Unity units.");

            var source = AssetDatabase.LoadAssetAtPath<TextAsset>(SparkLevel82ModelInstaller.Folder + "Equipment_Terminal_Mounts.csv");
            var positions = source.text.Split('\n').Skip(1).Where(row => !string.IsNullOrWhiteSpace(row))
                .Select(row => row.Trim().Split(',')).ToDictionary(row => row[5]);
            var goldContacts = new Dictionary<string, List<Vector3>>();
            foreach (string equipment in positions.Values.Select(row => row[1]).Distinct())
            {
                var gold = new List<Vector3>();
                foreach (var filter in art.Find(equipment).GetComponentsInChildren<MeshFilter>()
                    .Where(filter => filter.name.EndsWith("__tintE9BC56", StringComparison.Ordinal)))
                {
                    using var data = MeshUtility.AcquireReadOnlyMeshData(filter.sharedMesh);
                    using var vertices = new Unity.Collections.NativeArray<Vector3>(data[0].vertexCount, Unity.Collections.Allocator.Temp);
                    data[0].GetVertices(vertices);
                    var toArt = art.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                    foreach (var vertex in vertices) gold.Add(toArt.MultiplyPoint3x4(vertex));
                }
                goldContacts.Add(equipment, gold);
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
                Vector3 expected = art.TransformPoint(.096f * new Vector3(Coordinate(2), Coordinate(3), -Coordinate(4)));
                Vector3 axis = art.InverseTransformPoint(socket.position);
                var gold = goldContacts[row[1]];
                float gap = gold.Count == 0 ? float.PositiveInfinity : Mathf.Sqrt(gold.Min(vertex => (axis - vertex).sqrMagnitude));
                socketGaps.Add((id, gap));
                float error = Vector3.Distance(expected, socket.parent.position);
                maxError = Mathf.Max(maxError, error);
                Check(socket.IsChildOf(mounts) && socket.parent.name.StartsWith(row[0] + " ", StringComparison.Ordinal)
                    && socket.parent.parent.name == row[1] + " Fixed Terminals" && error < .0002f,
                    $"Terminal {id} does not match its original equipment position. Gap {error:F5}.");
                Check(unique.Add(socket.position) && socket.TryGetComponent<BoxCollider>(out var collider)
                    && collider.enabled && socket.gameObject.layer == LayerMask.NameToLayer("Pin") &&
                    !socket.GetComponent<Renderer>() && !socket.Find("Jack recess"),
                    "Duplicate or standalone terminal visual found at " + id);
            }
            var offCollar = socketGaps.Where(item => item.distance > .009f).Select(item => $"{item.id} {item.distance:F5}").ToArray();
            Check(offCollar.Length == 0, "Terminals without a matching original gold collar: " + string.Join(", ", offCollar));
            Check(unique.Count == 206 && mounts.GetComponentsInChildren<SparkTerminal>(true).Length == 206
                && loop.GetComponentsInChildren<SparkTerminal>(true).Length == 206,
                "Expected exactly 206 functional sockets with no extra board terminals.");
            Record($"206/206 unique terminals match original equipment coordinates. Maximum mount error {maxError:F6} Unity units.");
            Check(loop.pinLabels.Length == 206 && loop.pinLabels.Count(label => label.gameObject.activeSelf) <= 3,
                "Only relevant socket labels should be shown during wiring.");
            Check(loop.plot.TryGetComponent<CanvasRenderer>(out _) && loop.plot.rectTransform.anchorMax == Vector2.one,
                "The live measurement display should render beside the fixture.");
            Check(wireBuilder.DrawingMode == WireDrawingMode.TerminalToTerminal, "MainMenu must use direct terminal-to-terminal wiring.");
            var shelf = GameObject.Find("ProjectSpark_Gameplay")?.transform.Find("Component_Shelf");
            Check(shelf && shelf.GetComponentsInChildren<Renderer>().All(renderer => renderer.forceRenderingOff),
                "The component shelf obscures the front of the fitted workbench.");
            Capture(bench, pins, "Unwired", true);
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
            Record("206/206 sockets picked by the real Wire tool from the active game camera.");
            Record("Sockets overlapped by Level 82 buttons in the tested camera view: "+(covered.Count==0?"none":string.Join(", ",covered))+".");
            void Connect(int from, int to, WirePolarity polarity = WirePolarity.Neutral)
            {
                Check(wireBuilder.BeginWire(pins[ids[from]], polarity), "Wire tool could not start at " + ids[from]);
                Check(wireBuilder.TryEndWire(camera.WorldToScreenPoint(pins[ids[to]].transform.position)),
                    "Wire tool could not finish " + ids[from] + " to " + ids[to]);
            }

            Check(covered.Count==0,"Move the control overlay away from these terminals: "+string.Join(", ",covered));
            var map=SparkLevel82Process.Wires;
            int surfaces=bench.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials)
                .Where(m=>m!=null&&m.HasProperty("_BumpMap")&&m.GetTexture("_BumpMap")!=null)
                .Select(m=>m.GetTexture("_BumpMap")).Distinct().Count();
            Check(surfaces==12,$"Expected twelve authored surface families; found {surfaces}.");
            Record("Twelve authored PBR surface families applied.");
            const int feedback=118;
            await Click(loop.powerButton);
            for(int i=0;i<map.Length;i++)
            {
                if(i==feedback)continue;
                var (from,to)=map[i];
                Connect(from,to,from==0?WirePolarity.Positive:from==2?WirePolarity.Negative:WirePolarity.Neutral);
            }
            await Wait(()=>loop.statusText.text.Contains("W119"),"missing direct field feedback");
            Check(!loop.doorButton.interactable&&manager.SatisfiedTargetCount==0,"Diagnostic field telemetry bypassed W119.");
            Record("Connected 133 wires. Field telemetry W120 and calibration health W125 could not replace W119.");
            registry.RefreshNow();
            var laterWire = registry.Wires.Last();
            Check(registry.RemoveWire(laterWire.WireId), "Could not prepare a connection installed after W119.");
            Connect(map[feedback].a,map[feedback].b);
            Check(registry.RestoreRemovedWire(laterWire.WireId), "Could not reinstall a cable after W119.");
            registry.SelectWire(null);
            await At(1);
            var hidden = Read<Dictionary<Renderer, bool>>(loop, "hiddenCableRenderers");
            Check(hidden.Count > 0 && hidden.Keys.All(renderer => renderer.forceRenderingOff),
                "The focused equipment view did not hide ordinary cable renderers.");
            await Click(loop.cableViewButton);
            Check(hidden.Count == 0 && registry.Count == 134, "Showing cables changed the electrical connections.");
            await Click(loop.cableViewButton);
            Check(hidden.Count > 0 && registry.Count == 134, "Equipment view changed the electrical connections.");
            Record("Cable View switches between all 134 cables and W119 plus the selected cable, with the circuit intact.");
            Time.captureDeltaTime=.10f;
            camera.enabled=false;
            var plant=Read<SparkLevel82Plant>(loop,"plant");
            bool Flag(string key)=>Read<bool>(loop,key);
            bool Idle()=>Read<SparkLevel82Plant.Mode>(loop,"mode")==SparkLevel82Plant.Mode.Idle;
            bool Voltage(string endpoint, float minimum, float maximum)=>
                manager.ElectricalSolver.LatestSolveResult.TryGetVoltage(pins[endpoint],pins["PLC_GND"],out float v)&&v>=minimum&&v<=maximum;
            async Task Scan()
            {
                await Click(loop.conductButton);
                await Wait(Idle,"conducted scan complete");
                await Click(loop.radiateButton);
                await Wait(()=>Idle()&&Read<int>(loop,"completedPoses")==72,"all 72 settled radiated poses");
                Check(Read<float[]>(loop,"radiation").All(value=>value>0),"At least one pose lacked a measured sample.");
                Record($"Emissions: conducted {Read<float>(loop,"conductedWorst"):F3}, guarded radiated {Read<float>(loop,"radiatedWorst"):F3}, 72 poses × 16 bins.");
            }
            async Task Near()
            {
                await Click(loop.nearButton);
                await Wait(()=>Flag("nearComplete")&&Read<int>(loop,"nearSamples")==16,"full measured near-field map");
                await Click(loop.diagnoseButton);
                Check(Read<float[]>(loop,"nearH").Zip(Read<float[]>(loop,"nearE"),(h,e)=>Mathf.Abs(h-e)).Max()>.09f,"Near-field E and H probes returned the same paths.");
            }
            async Task Expose()
            {
                await Click(loop.armRfButton);
                await Click(loop.immunityButton);
            }
            async Task WaitQualified(int number)
            {
                await Wait(()=>Read<int>(loop,"qualifiedSets")==number&&plant.RfSafe,"immunity profile and physical rundown");
                Check(Read<int>(loop,"validBands")==4&&plant.FunctionValid,"Immunity qualified without all four work passes.");
                Record($"Qualification {number}: all four field bands with live EUT function, physical shutdown proved, {plant.ValidCycles} observed valid band results.");
            }
            async Task Recover()
            {
                await Click(loop.verifyButton);await Click(loop.resetButton);
            }

            Check(Voltage("DOOR_CLOSED_IN",0,.48f)&&!loop.calMotionButton.interactable,"An open door failed to electrically inhibit the chamber.");
            await Click(loop.eStopButton);
            await Wait(()=>loop.emergency.IsOpen&&Voltage("SAFE_STOP_IN",0,.48f)&&loop.contacts[0].IsOpen,"independent hardwired RF stop");
            await Click(loop.eStopButton);
            await Wait(()=>Voltage("SAFE_STOP_IN",4.7f,5.3f),"reset hardwired stop contact");
            await Click(loop.doorButton);
            await Wait(()=>plant.Closed&&Voltage("DOOR_CLOSED_IN",4.7f,5.3f),"physically closed chamber and independent interlock");
            await Click(loop.diagnoseButton);await Click(loop.calFieldButton);await Recover();
            Record("Door interlock and emergency stop each changed independent solved 5 V input states.");
            await Click(loop.calMotionButton);await Click(loop.calEmiButton);
            await At(2);
            await Scan();
            Check(!Flag("conducted")&&!Flag("radiated")&&!loop.armRfButton.interactable,"A failed baseline was allowed to qualify.");
            await Near();
            Check(Flag("diagnosed"),"Baseline probe map did not identify both leakage paths.");
            await Click(loop.harnessButton);await Click(loop.gasketButton);await Click(loop.filterButton);await Click(loop.sensorButton);
            await Scan();await At(3);
            await Expose();
            await Wait(()=>plant.Field>.43f,"live field with valid monitoring");
            Capture(bench,pins,"Running",false);
            await WaitQualified(1);await At(4);
            await Click(loop.harnessFaultButton);await Near();
            Check(Flag("diagnosedFault")&&Read<float>(loop,"highH")>.29f,"Harness upset did not raise the magnetic hotspot.");
            await Click(loop.harnessButton);
            await Click(loop.seamFaultButton);await Near();
            Check(Flag("diagnosedFault")&&Read<float>(loop,"highE")>.29f,"Seam compression loss did not raise the electric hotspot.");
            await Click(loop.gasketButton);await Scan();await Expose();
            await Click(loop.sensorFaultButton);
            await Wait(()=>Flag("sensorPassed")&&loop.contacts[0].IsOpen,"actual ADC error inhibited exposure");
            Check(Read<float>(loop,"desenseAtTrip")>.132f&&plant.BlockedUnsafe>0,"Affected EUT samples did not block unsafe command evidence.");
            await Click(loop.diagnoseButton);await Click(loop.sensorButton);await Recover();
            await Scan();await Expose();await WaitQualified(2);await At(5);
            Record("Cable common-mode hotspot, seam electric leakage and RF desense verified as distinct fault paths.");
            await Expose();await Click(loop.armButton);
            Check(loop.contacts[0].IsClosed&&plant.Field>.42f,"No live RF field before W119 removal.");
            int seq=Read<int>(loop,"fieldSequence");
            await Click(loop.openFeedbackButton);
            Check(registry.Count == 133 && registry.TryGetWire(laterWire.WireId, out _),
                "OPEN W119 did not remove the intended connection independently of wire order.");
            await Wait(()=>!Flag("trusted")&&loop.contacts[0].IsOpen,"stale field sample and actual amplifier permit open");
            float amp=Read<float>(loop,"ampAtTrip");
            Check(amp>.40f&&plant.Amp>.026f&&plant.Amp<=amp,"Amplifier output disappeared without decay.");
            Check(!loop.immunityButton.interactable&&!loop.resetButton.interactable&&Flag("feedbackObserved")&&manager.SatisfiedTargetCount==5,
                "W119 loss did not inhibit exposure and retain history.");
            await Wait(()=>Voltage("AMP_ENABLE",0,.49f)&&plant.RfSafe,"solved permit below 0.5 V, amplifier rundown and shutter closure");
            Check(Read<int>(loop,"fieldSequence")<=seq+1,"Disconnected feedback kept advancing.");
            Record($"W119 opened at normalized output {amp:F3}. Stale detection cut the solved permit below 0.5 V and the independent shutter closed. Historical 5/6 retained.");
            Capture(bench,pins,"SensorFault",false);
            camera.enabled=true;
            Connect(map[feedback].a,map[feedback].b);
            camera.enabled=false;
            await Click(loop.diagnoseButton);await Click(loop.calFieldButton);await Recover();
            Check(Read<int>(loop,"fieldSequence")>seq,"Recovery reused stale field samples.");
            await Scan();await Expose();await WaitQualified(3);await Click(loop.certifyButton);
            await At(6);await Wait(()=>manager.IsCompleted,"campaign completion");
            Record("6/6 — Campaign manager reports Completed after W119 reconnection, field recalibration, explicit reset and fresh 72-pose emissions and immunity qualification.");
            Capture(bench,pins,"Verified",false);
            camera.enabled = true;
            string gameView = Path.GetFullPath("Logs/Level82_GameView.png");
            File.Delete(gameView);
            ScreenCapture.CaptureScreenshot(gameView);
            await Wait(() => File.Exists(gameView), "the completed Game View capture");
        }
        catch (Exception error)
        {
            Record("FAIL: " + error);
            if (loop != null) Record("Control panel: " + loop.statusText.text + " | " + loop.meterText.text + " | " + loop.detailText.text);
            if (loop != null) Record($"Electrical state: 5 V request {Read<bool>(loop, "power")}, solved rails {Read<bool>(loop, "rails")}, e-stop loop {Read<bool>(loop, "safety")}, trusted field sample {Read<bool>(loop, "trusted")}, first wiring issue {Read<string>(loop, "wiring")}.");
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
            "Missing Level 82 action on " + button.name);
        int pressedFrame=Time.frameCount;
        button.onClick.Invoke();
        await Wait(()=>Time.frameCount>pressedFrame,"next frame after "+button.name);
    }

    private static void Capture(Transform bench, Dictionary<string, SparkTerminal> pins, string name, bool overhead)
    {
        var view = new GameObject("Level 82 test camera");
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
            File.WriteAllBytes("Logs/Level82_" + name + ".png", snapshot.EncodeToPNG());
            if (!overhead) return;
            var rows = new List<string> { "Terminal,X,Y" };

            foreach (var pair in pins)
            {
                var p = camera.WorldToViewportPoint(pair.Value.transform.position);
                rows.Add($"{pair.Key},{Mathf.RoundToInt(p.x * width)},{Mathf.RoundToInt((1f - p.y) * height)}");
            }
            File.WriteAllLines("Logs/Level82_Pins.csv", rows);
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
        var timeout = DateTime.UtcNow.AddSeconds(82);
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











