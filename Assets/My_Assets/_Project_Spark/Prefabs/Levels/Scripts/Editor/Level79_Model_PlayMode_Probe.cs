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
        File.WriteAllText(Report, "Level 79 — Equipment workbench and Bond the Worlds test\n");
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
            var registry = Object.FindFirstObjectByType<SparkWireRegistry>();
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel79ModelInstaller.Folder + "Level_79.asset");
            Check(definition != null && wireBuilder != null && loader != null && registry != null, "Level definition, wire builder, or layout loader missing.");
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
            var table = SparkEquipmentWorkbenchFit.FindTable(SceneManager.GetActiveScene());
            var ids = SparkLevel79ModelInstaller.TerminalIds.ToArray();
            var pins = new Dictionary<string, SparkTerminal>();
            var mounts = bench.Find(SparkLevel79ModelInstaller.MountsName);
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

            var source = AssetDatabase.LoadAssetAtPath<TextAsset>(SparkLevel79ModelInstaller.Folder + "Equipment_Terminal_Mounts.csv");
            var positions = source.text.Split('\n').Skip(1).Where(row => !string.IsNullOrWhiteSpace(row))
                .Select(row => row.Trim().Split(',')).ToDictionary(row => row[5]);
            Check(positions["LASER_INTERFEROMETER_POSITION_OUT"][1] == "L79_LaserDisplacementInterferometer" &&
                positions["HEXAPOD_FINE_POSITION_IN"][1] == "L79_HexapodFineAlignmentStage",
                "W037 must connect the authored interferometer and fine-alignment stage.");
            var meshVertices = new Dictionary<string, List<Vector3>>();
            foreach (string equipment in positions.Values.Where(row => row[6] == "Modeled").Select(row => row[1]).Distinct())
            {
                var renderedVertices = new List<Vector3>();
                foreach (var filter in art.Find(equipment).GetComponentsInChildren<MeshFilter>()
                    .Where(filter => filter.sharedMesh && !filter.name.StartsWith("Panel socket", StringComparison.Ordinal) && filter.name != "Contact insert"))
                {
                    using var data = MeshUtility.AcquireReadOnlyMeshData(filter.sharedMesh);
                    using var vertices = new Unity.Collections.NativeArray<Vector3>(data[0].vertexCount, Unity.Collections.Allocator.Temp);
                    data[0].GetVertices(vertices);
                    var toArt = art.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                    var referenced = new HashSet<int>();
                    for (int sub = 0; sub < data[0].subMeshCount; sub++)
                    {
                        using var indices = new Unity.Collections.NativeArray<int>(data[0].GetSubMesh(sub).indexCount, Unity.Collections.Allocator.Temp);
                        data[0].GetIndices(indices, sub);
                        foreach (int index in indices)
                            if (referenced.Add(index)) renderedVertices.Add(toArt.MultiplyPoint3x4(vertices[index]));
                    }
                }
                meshVertices.Add(equipment, renderedVertices);
            }
            Vector3 supplyPositive = SparkLevel79ModelInstaller.MountPosition(positions["SUPPLY_POS"]);
            Vector3 supplyReturn = SparkLevel79ModelInstaller.MountPosition(positions["SUPPLY_RET"]);
            var supplyVertices = meshVertices["L79_Regulated5VControlSupply"];
            for (int i = 1; i < 8; i++)
            {
                Vector3 unused = Vector3.Lerp(supplyPositive, supplyReturn, i / 8f);
                Check(supplyVertices.All(vertex => (vertex - unused).sqrMagnitude > .0026f * .0026f),
                    $"Decorative power contact {i + 1} still protrudes from the supply panel.");
            }
            Record("Removed seven unused molded contacts from the supply panel.");
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
                Vector3 expected = art.TransformPoint(SparkLevel79ModelInstaller.MountPosition(row));
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
            var offCollar = socketGaps.Where(item => item.distance > .002f).Select(item => $"{item.id} {item.distance:F5}").ToArray();
            Check(offCollar.Length == 0, "Terminals without matching imported socket geometry: " + string.Join(", ", offCollar));
            Record($"{socketGaps.Count} terminals coincide with the original imported contact geometry. The remaining {positions.Count - socketGaps.Count} use source-planned attachment points.");
            Check(unique.Count == 188 && art.GetComponentsInChildren<SparkTerminal>(true).Length == 188
                && loop.GetComponentsInChildren<SparkTerminal>(true).Length == 188,
                "Expected exactly 188 functional sockets with no extra board terminals.");
            Record($"188/188 unique terminals match original equipment coordinates. Maximum mount error {maxError:F6} Unity units.");
            Check(loop.pinLabels.Length == 188 && loop.pinLabels.Count(label => label.gameObject.activeSelf) <= 3,
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
            Record("188/188 sockets picked by the real Wire tool from the active game camera.");
            Record("Sockets overlapped by Level 79 buttons in the tested camera view: "+(covered.Count==0?"none":string.Join(", ",covered))+".");
            float overview = camera.orthographicSize;
            await Click(loop.focusWireButton);
            await Wait(() => camera.orthographicSize < overview * .95f, "close-up of the next wire");
            var firstWire = SparkLevel79Process.Wires[0];
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
            var map=SparkLevel79Process.Wires;
            int surfaces = bench.GetComponentsInChildren<Renderer>(true).SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material && material.HasProperty("_BumpMap") && material.GetTexture("_BumpMap"))
                .Select(material => material.GetTexture("_BumpMap")).Distinct().Count();
            Check(surfaces >= 8, $"Expected the workbench surface finishes. Found {surfaces}.");
            Record($"{surfaces} authored PBR surface families applied.");
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
            registry.RefreshNow();
            var laterWire = registry.Wires.Last();
            Check(registry.RemoveWire(laterWire.WireId), "Could not prepare a distinct later cable.");
            Connect(map[feedback].a,map[feedback].b);
            Check(registry.RestoreRemovedWire(laterWire.WireId), "Could not reinstall the later cable.");
            registry.SelectWire(null);
            await At(1);
            var hidden = Read<Dictionary<Renderer, bool>>(loop, "hiddenCableRenderers");
            Check(hidden.Count > 0 && hidden.Keys.All(renderer => renderer.forceRenderingOff),
                "Ordinary cables remain visible in equipment view.");
            await Click(loop.cableViewButton);
            Check(hidden.Count == 0 && registry.Count == 122, "Cable visibility altered the circuit.");
            await Click(loop.cableViewButton);
            Record("Equipment view declutters the fixture while preserving all 122 native wire connections.");
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
                    Capture(camera,pins,"Running",false);
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
            await Click(loop.openFeedbackButton);
            Check(registry.Count == 121, "Open W037 must remove exactly the feedback cable.");
            await Wait(()=>!Flag("validLaser")&&loop.contacts[0].IsOpen,"laser feedback stale and relay open");
            float speed=Read<float>(loop,"speedAtTrip");
            Check(speed>.24f&&plant.Motion>0&&plant.Motion<=speed+.01f,"Fine-axis velocity did not begin a controlled stop.");
            Check(!loop.growButton.interactable&&!loop.resetButton.interactable&&!Flag("homed")&&Flag("feedbackObserved")&&manager.SatisfiedTargetCount==5,
                "W037 must inhibit contact, invalidate home and retain the first five objectives.");
            await Wait(()=>manager.ElectricalSolver.LatestSolveResult.TryGetVoltage(pins["CONTACT_ENABLE"],pins["PLC_GND"],out float v)&&v<.5f,"contact permit below 0.5 V");
            await Wait(()=>plant.PoseMotion<.035f&&plant.Force<.025f,"controlled motion hold");
            Check(plant.RegionBond(0)==0,"Bond front continued without trusted feedback.");
            Record($"W037 removed during active approach at normalized speed {speed:F3}. Permit fell below 0.5 V, axes braked to hold, no bond started, and 5/6 remained.");
            Capture(camera,pins,"SensorFault",false);
            camera.enabled=true;
            Connect(map[feedback].a,map[feedback].b);
            camera.enabled=false;
            await Click(loop.homeButton);await Click(loop.laserButton);
            await Click(loop.verifyButton);await Click(loop.resetButton);await Click(loop.discardButton);
            await Assemble(3);await Click(loop.certifyButton);await At(6);
            await Wait(()=>manager.IsCompleted,"campaign completion");
            Record("6/6 — Campaign manager reports Completed after W037 reconnection, safe homing, laser recalibration and explicit reset, followed by a freshly tested four-chiplet assembly.");
            Capture(camera,pins,"Verified",false);
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

    private static void Capture(Camera source, Dictionary<string, SparkTerminal> pins, string name, bool recordPinPositions)
    {
        var view = new GameObject("Level 79 test camera");
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
            File.WriteAllBytes("Logs/Level79_" + name + ".png", snapshot.EncodeToPNG());
            if (!recordPinPositions) return;
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





