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
internal static class Level81ModelPlayModeProbe
{
    private const string Request = "Logs/Level81ModelTest.request";
    private const string Report = "Logs/Level81ModelTest.txt";
    private const string Running = "Codex.Level81.ModelTest";
    private static LevelGamePlayManager manager;
    private static SparkLevel81Process loop;

    static Level81ModelPlayModeProbe()
    {
        EditorApplication.update += CheckRequest;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    private static void CheckRequest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(Request)) return;
        try { File.Delete(Request); }
        catch (IOException) { return; }
        File.WriteAllText(Report, "Level 81 — Equipment-mounted terminals and Outlast the Bitstorm test\n");
        try
        {
            Check(SceneManager.GetActiveScene().path == SparkLevel81ModelInstaller.ScenePath, "Open MainMenu in Edit Mode.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SparkLevel81ModelInstaller.Prefab) != null)
                SparkLevel81ModelInstaller.Fit();
            SparkLevel81ModelInstaller.ValidateAssets();
            Record("Fitted the existing Level 81 campaign prefab to the workshop table.");
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
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel81ModelInstaller.Folder + "Level_81.asset");
            Check(definition != null && wireBuilder != null && registry != null && loader != null, "Level definition, wire builder, or layout loader missing.");
            if (manager.LevelProgression != null) manager.LevelProgression.enabled = false;
            Write(manager, "enforceProgressionAccess", false);
            Write(manager, "autoAdvanceOnCompletion", false);
            Write(manager, "autoEvaluate", true);
            int levelIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), definition);
            Check(levelIndex >= 0 && manager.StartLevel(levelIndex), "Level 81 is missing from the campaign or could not start.");
            await Wait(() => loader.ActiveLayout != null && manager.ActiveLevel == definition, "Level 81 loaded");
            loop = loader.ActiveLayout.GetComponent<SparkLevel81Process>();
            await Wait(() => loop != null && Read<int>(loop, "topology") >= 0,
                "the active loop controller to check the empty circuit");

            var bench = loop.transform.Find("Reliability Workbench");
            var camera = Read<Camera>(wireBuilder, "targetCamera");
            var table = SparkEquipmentWorkbenchFit.FindTable(SceneManager.GetActiveScene());
            var ids = SparkLevel81ModelInstaller.TerminalIds.ToArray();
            var pins = new Dictionary<string, SparkTerminal>();
            var mounts = bench.Find(SparkLevel81ModelInstaller.MountsName);
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

            var source = AssetDatabase.LoadAssetAtPath<TextAsset>(SparkLevel81ModelInstaller.Folder + "Equipment_Terminal_Mounts.csv");
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
                Vector3 expected = art.TransformPoint(SparkEquipmentWorkbenchFit.SourceArtUnit * new Vector3(Coordinate(2), Coordinate(3), -Coordinate(4)));
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
            Check(unique.Count == 200 && mounts.GetComponentsInChildren<SparkTerminal>(true).Length == 200
                && loop.GetComponentsInChildren<SparkTerminal>(true).Length == 200,
                "Expected exactly 200 functional sockets with no extra board terminals.");
            Record($"200/200 unique terminals match original equipment coordinates. Maximum mount error {maxError:F6} Unity units.");
            Check(loop.pinLabels.Length == 200 && loop.pinLabels.Count(label => label.gameObject.activeSelf) <= 3,
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
            Record("200/200 sockets picked by the real Wire tool from the active game camera.");
            Record("Sockets overlapped by Level 81 buttons in the tested camera view: "+(covered.Count==0?"none":string.Join(", ",covered))+".");
            float overview = camera.orthographicSize;
            await Click(loop.focusWireButton);
            await Wait(() => camera.orthographicSize < overview * .95f, "close-up of the next wire");
            var firstWire = SparkLevel81Process.Wires[0];
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
            var map=SparkLevel81Process.Wires;
            int surfaces=bench.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials)
                .Where(m=>m!=null&&m.HasProperty("_BumpMap")&&m.GetTexture("_BumpMap")!=null)
                .Select(m=>m.GetTexture("_BumpMap")).Distinct().Count();
            Check(surfaces==12,$"Expected twelve authored surface families; found {surfaces}.");
            Record("Twelve authored PBR surface families applied.");
            const int feedback=93;
            await Click(loop.powerButton);
            for(int i=0;i<map.Length;i++)
            {
                if(i==feedback)continue;
                var (from,to)=map[i];
                Connect(from,to,from==0?WirePolarity.Positive:from==2?WirePolarity.Negative:WirePolarity.Neutral);
            }
            await Wait(()=>loop.statusText.text.Contains("W094"),"missing direct ECC heartbeat");
            Check(!loop.initButton.interactable&&manager.SatisfiedTargetCount==0,"Diagnostic telemetry bypassed W094.");
            Record("Connected 129 wires. W093, W095 and supervisory telemetry could not replace W094.");
            registry.RefreshNow();
            var laterWire = registry.Wires.Last();
            Check(registry.RemoveWire(laterWire.WireId), "Could not prepare a connection installed after W094.");
            Connect(map[feedback].a,map[feedback].b);
            Check(registry.RestoreRemovedWire(laterWire.WireId), "Could not reinstall a cable after W094.");
            registry.SelectWire(null);
            await At(1);
            var hidden = Read<Dictionary<Renderer, bool>>(loop, "hiddenCableRenderers");
            Check(hidden.Count > 0 && hidden.Keys.All(renderer => renderer.forceRenderingOff),
                "Equipment view should hide ordinary cable renderers.");
            await Click(loop.cableViewButton);
            Check(hidden.Count == 0 && registry.Count == 130, "Showing all cables changed the electrical connections.");
            await Click(loop.cableViewButton);
            Check(hidden.Count > 0 && registry.Count == 130, "Equipment view changed the electrical connections.");
            Record("Cable View toggles all 130 cables while keeping W094 and the selected cable visible in equipment view.");
            Time.captureDeltaTime=.10f;
            camera.enabled=false;
            var plant=Read<SparkLevel81Plant>(loop,"plant");
            bool Flag(string key)=>Read<bool>(loop,key);
            async Task RunMission(int count)
            {
                int before=plant.Committed;
                await Click(loop.missionButton);
                await Wait(()=>Read<int>(loop,"acceptedMissions")==count,"three live mission phases");
                Check(plant.Committed==before+3&&Flag("missionComplete"),"A mission skipped or double-counted a command.");
                Record($"Mission {count}: navigation, maneuver and data return committed through the qualified relay; {plant.Committed} total safe commits.");
            }
            async Task Recover()
            {
                await Click(loop.diagnoseButton);await Click(loop.restoreButton);
                await Click(loop.revalidateButton);await Click(loop.resetButton);
            }

            await Click(loop.initButton);await Click(loop.singleButton);await Click(loop.scrubButton);await At(2);
            Check(plant.SingleVerified&&plant.Corrections==1&&plant.ValidateMemory(),"Single-bit repair failed verified writeback.");
            Record("Both banks initialized to 256 SECDED words. One injected bit repaired with verified writeback.");
            await Click(loop.tmrButton);await Click(loop.laneButton);
            await Wait(()=>Flag("laneVerified"),"majority masked a lane fault");
            Check(plant.FailedLane==0&&plant.Vote==plant.Reference,"Single faulty lane was not masked.");
            await Click(loop.watchButton);await Click(loop.opticalButton);await At(3);
            await RunMission(1);await At(4);
            Capture(camera,pins,"Running",false);

            await Click(loop.doubleButton);
            await Wait(()=>Flag("memoryPassed")&&plant.QuarantineCount==1,"SECDED double bit quarantined");
            Check(!plant.Read(128,out _)&&!loop.resetButton.interactable,"Quarantined data was accessible or reset could bypass verified restore.");
            await Recover();
            Check(plant.CheckpointVerified&&plant.QuarantineCount==0&&plant.Read(128,out _),"Trusted checkpoint restore failed.");
            int beforeFaults=plant.Committed;
            await Click(loop.commonButton);
            await Wait(()=>Flag("commonPassed"),"independent reference rejected a bad majority");
            Check(plant.Vote!=plant.Reference&&plant.Committed==beforeFaults&&plant.Blocked>0,"An unsafe majority was released.");
            await Recover();
            await Click(loop.packetsButton);
            await Wait(()=>Flag("packetPassed"),"CRC, gap, expired, duplicate and replay cases rejected");
            Check(plant.CorruptFrames>0&&plant.Gaps>0&&plant.Replays>0&&plant.Duplicates>0&&plant.ExpiredFrames>0,"Packet faults were not distinguished.");
            await Recover();
            await RunMission(2);await At(5);
            Record("Double-bit quarantine/restore, wrong majority and all five packet error classes passed with no unsafe commit.");

            await Click(loop.missionButton);await Click(loop.armButton);
            Check(Read<int>(loop,"phase")>0&&loop.contacts[0].IsClosed,"No live command permit before W094 removal.");
            int localAtTrip=Read<int>(loop,"heartbeatSequence"),commitsAtTrip=plant.Committed;
            await Click(loop.openFeedbackButton);
            Check(registry.Count == 129 && registry.TryGetWire(laterWire.WireId, out _),
                "OPEN W094 did not remove the intended connection independently of wire order.");
            await Wait(()=>!Flag("trusted")&&loop.contacts[0].IsOpen,"W094 stale feedback and open command relay");
            Check(!loop.missionButton.interactable&&!loop.resetButton.interactable&&Flag("feedbackObserved")&&manager.SatisfiedTargetCount==5,
                "W094 loss failed to latch command hold while preserving history.");
            await Wait(()=>manager.ElectricalSolver.LatestSolveResult.TryGetVoltage(pins["COMMAND_ENABLE"],pins["PLC_GND"],out float v)&&v<.5f,"command permit below 0.5 V");
            await Wait(()=>Read<float>(loop,"holdTravel")>.95f&&Read<int>(loop,"heartbeatSequence")>localAtTrip,"controlled safe hold while local ECC remains active");
            Check(plant.Committed==commitsAtTrip&&Read<int>(loop,"receivedSequence")==Read<int>(loop,"w094Sequence"),"Stale heartbeat or live command escaped safe hold.");
            Record("W094 physically removed while the mission permit was live. Local ECC kept ticking, supervisory feedback went stale, and the command gate fell below 0.5 V with 5/6 retained.");
            Capture(camera,pins,"SensorFault",false);
            camera.enabled=true;
            Connect(map[feedback].a,map[feedback].b);
            camera.enabled=false;
            await Recover();
            Check(Read<int>(loop,"receivedSequence")>Read<int>(loop,"w094Sequence"),"Recovery reused a stale heartbeat.");
            await RunMission(3);await Click(loop.certifyButton);
            await At(6);await Wait(()=>manager.IsCompleted,"campaign completion");
            Record("6/6 — Campaign manager reports Completed after W094 reconnection, fresh ECC, memory/voter/watchdog/data requalification and a fresh three-phase mission.");
            Capture(camera,pins,"Verified",false);
            camera.enabled = true;
            string gameView = Path.GetFullPath("Logs/Level81_GameView.png");
            File.Delete(gameView);
            ScreenCapture.CaptureScreenshot(gameView);
            await Wait(() => File.Exists(gameView), "completed Game View screenshot");

            var next = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel82ModelInstaller.Folder + "Level_82.asset");
            int nextIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), next);
            Check(nextIndex >= 0 && manager.StartLevel(nextIndex), "Level 82 no longer loads.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent<SparkLevel82WorkbenchView>(out var previous) &&
                previous.Bench && previous.FramingBounds.size.x > 0 && camera.orthographic, "Level 82 inherited workbench camera and serialized fit");
            Record("Level 82 still loads its serialized workbench camera and fixture framing.");
        }
        catch (Exception error)
        {
            Record("FAIL: " + error);
            if (loop != null) Record("Control panel: " + loop.statusText.text + " | " + loop.meterText.text + " | " + loop.detailText.text);
            if (loop != null) Record($"Electrical state: 5 V request {Read<bool>(loop, "power")}, solved rails {Read<bool>(loop, "rails")}, e-stop loop {Read<bool>(loop, "safety")}, trusted ECC {Read<bool>(loop, "trusted")}, first wiring issue {Read<string>(loop, "wiring")}.");
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
            "Missing Level 81 action on " + button.name);
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
            File.WriteAllBytes("Logs/Level81_" + name + ".png", snapshot.EncodeToPNG());
            if (!recordPinPositions) return;
            var rows = new List<string> { "Terminal,X,Y" };

            foreach (var pair in pins)
            {
                var p = camera.WorldToViewportPoint(pair.Value.transform.position);
                rows.Add($"{pair.Key},{Mathf.RoundToInt(p.x * width)},{Mathf.RoundToInt((1f - p.y) * height)}");
            }
            File.WriteAllLines("Logs/Level81_Pins.csv", rows);
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









