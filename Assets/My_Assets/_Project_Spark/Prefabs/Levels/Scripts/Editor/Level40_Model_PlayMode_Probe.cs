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
using Object = UnityEngine.Object;

[InitializeOnLoad]
internal static class Level40ModelPlayModeProbe
{
    private const string Request = "Temp/Level40ModelTest.request";
    private const string Report = "Temp/Level40ModelTest.txt";
    private const string Running = "Codex.Level40.ModelTest";
    private static LevelGamePlayManager manager;
    private static SparkLevel40TheLongRun loop;

    static Level40ModelPlayModeProbe()
    {
        EditorApplication.update += CheckRequest;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    private static void CheckRequest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(Request)) return;
        try { File.Delete(Request); }
        catch (IOException) { return; }
        File.WriteAllText(Report, "Level 40 — Breadboard assembly and The Long Run test\n");
        try
        {
            Check(SceneManager.GetActiveScene().path == SparkLevel40DisplayInstaller.ScenePath, "Open MainMenu in Edit Mode.");
            SparkLevel40DisplayInstaller.Fit();
            Record("Installed the fitted assembly in the existing Level 40 prefab.");
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
        try
        {
            await Wait(() => Object.FindFirstObjectByType<LevelGamePlayManager>() != null, "MainMenu manager");
            manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
            var loader = Object.FindFirstObjectByType<SparkLevelLayoutLoader>();
            var wireBuilder = Object.FindFirstObjectByType<SignalWireBuilder>();
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel40DisplayInstaller.Folder + "Level_40.asset");
            Check(definition != null && wireBuilder != null && loader != null, "Level definition, wire builder, or layout loader missing.");
            if (manager.LevelProgression != null) manager.LevelProgression.enabled = false;
            Write(manager, "enforceProgressionAccess", false);
            Write(manager, "autoAdvanceOnCompletion", false);
            Write(manager, "autoEvaluate", true);
            int levelIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), definition);
            Check(levelIndex >= 0 && manager.StartLevel(levelIndex), "Level 40 is missing from the campaign or could not start.");
            await Wait(() => loader.ActiveLayout != null && manager.ActiveLevel == definition, "Level 40 loaded");
            loop = loader.ActiveLayout.GetComponent<SparkLevel40TheLongRun>();
            await Wait(() => Read<int>(loop, "topology") >= 0 && loop.statusText.text.Contains("Connect "),
                "the active loop controller to check the empty circuit");

            var bench = loop.transform.Find("Current Loop Workbench");
            var camera = Read<Camera>(wireBuilder, "targetCamera");
            var grid = SparkLevel40DisplayInstaller.FindBoard(SceneManager.GetActiveScene());
            var ids = SparkLevel40DisplayInstaller.TerminalIds.ToArray();
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
                Vector3 surface = SparkLevel40DisplayInstaller.SurfacePoint(grid, foot, bench.up);
                maximumGap = Mathf.Max(maximumGap, Mathf.Abs(Vector3.Dot(surface - foot, bench.up)));
                Check(maximumGap < .001f, "Terminal base does not touch the breadboard: " + id);
                Check(grid.TryGetNearestHole(socket.position, .15f, out var hole, out int column, out int row) &&
                    holes.Add(new Vector2Int(column, row)) &&
                    Vector3.ProjectOnPlane(foot - hole, bench.up).magnitude < .001f,
                    "Terminal does not sit over its own wire hole: " + id);
            }
            Check(holes.Count == 10, "Expected 10 breadboard sockets.");
            Check(highestFoot - lowestFoot < .001f, "A socket is recessed below the flat breadboard deck.");
            Record($"10/10 terminal feet touch the breadboard on distinct wire holes. Maximum seating error: {maximumGap:F6} Unity units.");
            var labels = bench.GetComponentsInChildren<TextMesh>();
            Check(labels.Length == 22 && Array.TrueForAll(labels, label => label.transform.localScale == Vector3.one * .20f),
                "TextMesh labels must use a local scale of 0.20.");
            Record("All 22 TextMesh labels use 0.20 local scale.");
            Capture(bench, pins, "Unwired", true);
            foreach (var pair in pins)
            {
                Vector3 point = camera.WorldToScreenPoint(pair.Value.transform.position);
                Check(point.z > 0 && point.x >= 0 && point.y >= 0 && point.x < camera.pixelWidth && point.y < camera.pixelHeight,
                    $"Terminal outside the active camera: {pair.Key} at {point}; camera {camera.pixelWidth}x{camera.pixelHeight}");
                Check(ReferenceEquals(pick.Invoke(wireBuilder, new object[] { (Vector2)point }), pair.Value),
                    "Wire tool cannot select " + pair.Key + " from the active game camera.");
            }
            Record("10/10 sockets picked by the real Wire tool from the active game camera.");
            void Connect(int from, int to, WirePolarity polarity = WirePolarity.Neutral)
            {
                Check(wireBuilder.BeginWire(pins[ids[from]], polarity), "Wire tool could not start at " + ids[from]);
                Check(wireBuilder.TryEndWire(camera.WorldToScreenPoint(pins[ids[to]].transform.position)),
                    "Wire tool could not finish " + ids[from] + " to " + ids[to]);
            }
            Connect(0, 2, WirePolarity.Positive);
            Connect(3, 4);
            Connect(5, 6);
            Connect(7, 8);
            Connect(9, 1, WirePolarity.Negative);
            Check(Read<SparkCircuitSystem>(loop, "circuit") == wireBuilder.Circuit, "The loop and Wire tool used different circuits.");
            Record("5/5 series-loop wires completed through SignalWireBuilder.");
            var visuals = loop.GetComponent<SparkLevel40DisplayVisuals>();
            Check(visuals != null, "Fitted assembly visuals are missing.");
            await At(1, 4f, 1f, 10.8f, false);
            await Wait(() => !visuals.waterPivot.gameObject.activeSelf && visuals.receiverPercentReadout.text == "RECEIVER 0%",
                "empty tank and live zero");
            await Click(loop.midscaleButton);
            await At(2, 12f, 3f, 8.4f, false);
            await Wait(() => visuals.waterPivot.gameObject.activeSelf &&
                Mathf.Abs(visuals.waterPivot.localScale.y - visuals.fullWaterScale * .5f) < .002f &&
                visuals.tankReadout.text == "TANK 50%", "half-filled modeled tank");
            await Click(loop.cableButton);
            await At(3, 12f, 3f, 4.8f, false);
            await Click(loop.fullscaleButton);
            await At(4, 16.67f, 4.17f, 2f, true);
            await Wait(() => visuals.cableReadout.text == "350 Ω" && visuals.stateReadout.text == "HEADROOM LIMIT" &&
                visuals.receiverPercentReadout.text == "RECEIVER 79%" && visuals.tankReadout.text == "TANK 100%" &&
                Mathf.Abs(visuals.waterPivot.localScale.y - visuals.fullWaterScale) < .002f, "full tank and clipped receiver");
            Capture(bench, pins, "HeadroomLimit", false);
            await Click(loop.cableButton);
            await At(5, 20f, 5f, 6f, false);
            await Click(loop.lineButton);
            await Wait(() => Read<bool>(loop, "openObserved") && visuals.stateReadout.text == "LINE OPEN" &&
                visuals.openLamp.enabled && !visuals.closedLamp.enabled &&
                Quaternion.Angle(visuals.leverPivot.localRotation, Quaternion.Euler(0, 0, visuals.openLeverAngle)) < .1f &&
                visuals.receiverReadout.text == "0.00 V", "open lever, lamp and receiver");
            Check(manager.SatisfiedTargetCount == 5, "The level completed before the open line was repaired.");
            Record($"Open line. {loop.meterText.text} | {loop.tankText.text}");
            Capture(bench, pins, "LineOpen", false);
            await Click(loop.lineButton);
            await At(6, 20f, 5f, 6f, false);
            await Wait(() => manager.IsCompleted && visuals.closedLamp.enabled && !visuals.openLamp.enabled &&
                Quaternion.Angle(visuals.leverPivot.localRotation, Quaternion.Euler(0, 0, visuals.closedLeverAngle)) < .1f &&
                visuals.cableReadout.text == "50 Ω" && visuals.receiverPercentReadout.text == "RECEIVER 100%",
                "closed lever, full scale and campaign completion");
            Record("6/6 — Live zero, headroom limit and line restoration verified. Campaign manager reports Completed.");
            Capture(bench, pins, "Verified", false);
        }
        catch (Exception error)
        {
            Record("FAIL: " + error);
            if (loop != null) Record("Control panel: " + loop.statusText.text + " | " + loop.meterText.text);
            if (manager != null) Record($"Evaluation {manager.SatisfiedTargetCount}/{manager.ActiveTargetCount}: {manager.LastEvaluationMessage}");
        }
        finally { if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode(); }
    }

    private static readonly MethodInfo pick = typeof(SignalWireBuilder).GetMethod("FindTerminalAtScreenPosition",
        BindingFlags.Instance | BindingFlags.NonPublic);

    private static async Task At(int stage, float milliamps, float receiver, float transmitter, bool limited)
    {
        await Wait(() => Read<int>(loop, "stage") == stage && manager.SatisfiedTargetCount == stage, $"{stage}/6");
        Check(Mathf.Abs(loop.LoopCurrentAmps * 1000f - milliamps) < .16f, $"Unexpected current at {stage}/6.");
        Check(Mathf.Abs(loop.ReceiverVoltage - receiver) < .06f, $"Unexpected receiver voltage at {stage}/6.");
        Check(Mathf.Abs(Read<float>(loop, "transmitterVoltage") - transmitter) < .12f &&
            loop.IsHeadroomLimited == limited, $"Unexpected regulator reading at {stage}/6.");
        Record($"{stage}/6 — {loop.meterText.text} | {loop.tankText.text}");
    }

    private static async Task Click(Button button)
    {
        await Wait(() => button.isActiveAndEnabled && button.interactable, button.name + " available");
        Check(button.onClick.GetPersistentEventCount() == 1 && button.onClick.GetPersistentTarget(0) == loop,
            "Missing Level 40 action on " + button.name);
        button.onClick.Invoke();
    }

    private static void Capture(Transform bench, Dictionary<string, SparkTerminal> pins, string name, bool overhead)
    {
        var view = new GameObject("Level 40 test camera");
        var camera = view.AddComponent<Camera>();
        camera.enabled = false;
        view.transform.position = bench.TransformPoint(new Vector3(0, 2.20f, overhead ? -.58f : -1.92f));
        view.transform.rotation = Quaternion.LookRotation(bench.TransformPoint(new Vector3(0, .04f, 0)) -
            view.transform.position, bench.up);
        camera.orthographic = true;
        camera.orthographicSize = bench.TransformVector(new Vector3(0, .90f, 0)).magnitude;
        const int width = 1600, height = 1050;
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
            File.WriteAllBytes("Temp/Level40_" + name + ".png", snapshot.EncodeToPNG());
            if (!overhead) return;
            var rows = new List<string> { "Terminal,X,Y" };
            foreach (var pair in pins)
            {
                var p = camera.WorldToViewportPoint(pair.Value.transform.position);
                rows.Add($"{pair.Key},{Mathf.RoundToInt(p.x * width)},{Mathf.RoundToInt((1f - p.y) * height)}");
            }
            File.WriteAllLines("Temp/Level40_Pins.csv", rows);
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

    private static async Task Wait(Func<bool> condition, string step)
    {
        var timeout = DateTime.UtcNow.AddSeconds(60);
        while (EditorApplication.isPlaying && !condition())
        {
            Check(DateTime.UtcNow < timeout, "Timed out waiting for " + step);
            await Task.Delay(60);
        }
        Check(EditorApplication.isPlaying, "Play Mode stopped.");
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






