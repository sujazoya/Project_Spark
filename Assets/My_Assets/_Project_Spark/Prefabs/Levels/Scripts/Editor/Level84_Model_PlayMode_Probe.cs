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
internal static class Level84ModelPlayModeProbe
{
    private const string Request = "Logs/Level84ModelTest.request";
    private const string Report = "Logs/Level84ModelTest.txt";
    private const string Running = "Codex.Level84.ModelTest";
    private static LevelGamePlayManager manager;
    private static SparkLevel84Process loop;

    public static void Launch()
    {
        File.WriteAllText(Request,"start");
        CheckRequest();
    }
    static Level84ModelPlayModeProbe()
    {
        EditorApplication.update += CheckRequest;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    private static void CheckRequest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !File.Exists(Request)) return;
        bool refit;
        try { refit=File.ReadAllText(Request).Trim()=="fit"; File.Delete(Request); }
        catch (IOException) { return; }
        File.WriteAllText(Report, "Level 84 — Fixed terminal assembly and Defy the Tumble test\n");
        try
        {
            if(SceneManager.GetActiveScene().path!=SparkLevel84ModelInstaller.ScenePath)
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(SparkLevel84ModelInstaller.ScenePath);
            if (refit && AssetDatabase.LoadAssetAtPath<GameObject>(SparkLevel84ModelInstaller.Prefab) != null)
                SparkLevel84ModelInstaller.Fit();
            SparkLevel84ModelInstaller.Install();
            Record("Installed the fitted assembly in the Level 84 prefab and registered the campaign.");
            SessionState.SetBool(Running, true);
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error) { Record("FAIL: " + error); if(Application.isBatchMode)EditorApplication.Exit(1); }
    }

    private static void StateChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(Running, false)) return;
        if (state == PlayModeStateChange.EnteredPlayMode) Run();
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            SessionState.EraseBool(Running);
            Record("Returned to Edit Mode.");
            if(Application.isBatchMode) EditorApplication.Exit(File.ReadAllText(Report).Contains("FAIL:")?1:0);
        }
    }

    private static async void Run()
    {
        LogType logFilter = Debug.unityLogger.filterLogType;
        int vsync = QualitySettings.vSyncCount;
        bool runInBackground = Application.runInBackground;
        try
        {
            Debug.unityLogger.filterLogType = LogType.Error;
            Application.runInBackground = true;
            Time.timeScale = 5f;
            QualitySettings.vSyncCount = 0;
            Record("Entered Play Mode.");
            await Wait(() => Object.FindFirstObjectByType<LevelGamePlayManager>() != null, "MainMenu manager");
            manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
            var loader = Object.FindFirstObjectByType<SparkLevelLayoutLoader>();
            var wireBuilder = Object.FindFirstObjectByType<SignalWireBuilder>();
            var wirePaths = Object.FindFirstObjectByType<SignalPath_Manager>();
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel84ModelInstaller.Folder + "Level_84.asset");
            Check(definition != null && wireBuilder != null && loader != null, "Level definition, wire builder, or layout loader missing.");
            if (manager.LevelProgression != null) manager.LevelProgression.enabled = false;
            Write(manager, "enforceProgressionAccess", false);
            Write(manager, "autoAdvanceOnCompletion", false);
            Write(manager, "autoEvaluate", true);
            int levelIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), definition);
            Check(levelIndex >= 0 && manager.StartLevel(levelIndex), "Level 84 is missing from the campaign or could not start.");
            await Wait(() => loader.ActiveLayout != null && manager.ActiveLevel == definition, "Level 84 loaded");
            loop = loader.ActiveLayout.GetComponent<SparkLevel84Process>();
            await Wait(() => loop != null && loop.Topology >= 0,
                "the active loop controller to check the empty circuit");

            var bench = loop.transform.Find("Celestial Test Arena");
            var camera = Read<Camera>(wireBuilder, "targetCamera");
            var pins = loop.pins.ToDictionary(pin=>pin.name);
            var poses = new HashSet<Vector3>();
            Physics.SyncTransforms();
            float maxGap=0;
            foreach(var pin in loop.pins)
            {
                Check(poses.Add(pin.transform.position), "Duplicate fixed socket pose "+pin.name);
                Check(pin.transform.parent.name.StartsWith("P",StringComparison.Ordinal),"Missing equipment mount "+pin.name);
                float gap=Vector3.Distance(pin.transform.TransformPoint(Vector3.down),pin.transform.parent.position);
                maxGap=Mathf.Max(maxGap,gap);
                Check(gap<.0002f,"Socket is detached from its equipment mount "+pin.name);
            }
            Check(pins.Count==275,"Unexpected socket count.");
            Check(loop.rig.GetComponentsInChildren<Transform>(true).Count(t=>t.name.StartsWith("ANIM_",StringComparison.Ordinal))==58,
                "Asset rig lost a pivot.");
            Check(wireBuilder.DrawingMode==WireDrawingMode.TerminalToTerminal,"MainMenu must use the direct terminal cable mode.");
            Record($"275 distinct fixed equipment sockets, max mount gap {maxGap:0.00000} Unity units. 58 animation pivots verified.");
            int inView=0,selected=0;
            var hardToPick=new List<string>();
            foreach(var pair in pins)
            {
                Vector3 screen=camera.WorldToScreenPoint(pair.Value.transform.position);
                if(screen.z<=0||screen.x<0||screen.y<0||screen.x>=camera.pixelWidth||screen.y>=camera.pixelHeight)continue;
                inView++;
                bool hit=false;
                for(int x=-3;x<=3&&!hit;x+=3)
                    for(int y=-3;y<=3&&!hit;y+=3)
                        hit=ReferenceEquals(pick.Invoke(wireBuilder,new object[]{(Vector2)screen+new Vector2(x,y)}),pair.Value);
                if(hit)selected++;else hardToPick.Add(pair.Key);
            }
            Record($"Default view: {inView}/275 on screen; {selected} directly picked in that view before camera navigation.");
            if(hardToPick.Count>0)Record("Socket groups to approach with camera zoom: "+string.Join(", ",hardToPick));
            Capture(bench,pins,"Unwired",true);
            var wires=SparkLevel84Wiring.Wires;
            const int critical=SparkLevel84Wiring.Feedback;
            void Connect(int index)
            {
                var wire=wires[index];
                Check(wireBuilder.TryConnectTerminals(loop.pins[wire.a],loop.pins[wire.b]),$"New terminal wire builder rejected W{index+1:000}");
            }
            for(int i=0;i<wires.Length;i++)
            {
                if(i!=critical)Connect(i);
                if(i%14==13)await Task.Yield();
            }
            await Click(loop.buttons[0]);await Click(loop.buttons[1]);
            Check(loop.Stage==0,"Network falsely qualified with W097 open.");
            Connect(critical);await At(1);
            var hidden=Read<Dictionary<Renderer,bool>>(loop,"hiddenCableRenderers");
            Check(hidden.Count>0&&hidden.Keys.All(renderer=>renderer.forceRenderingOff),"Focused equipment view must hide ordinary cables.");
            await Click(loop.buttons[20]);Check(hidden.Count==0,"Show all cables restores the regular wire view.");
            await Click(loop.buttons[20]);Check(hidden.Count>0,"Equipment view can be restored without rewiring.");
            Record("Cable View toggles rendered connections without changing the 142-wire circuit. The trusted W097 and selected cables stay visible in equipment view.");
            await Click(loop.buttons[2]);await Click(loop.buttons[3]);await Click(loop.buttons[4]);await At(2);
            await Click(loop.buttons[5]);await At(3);
            await Click(loop.buttons[6]);await Wait(()=>loop.Unloaded,"measured magnetic unloading");
            await Click(loop.buttons[7]);await Click(loop.buttons[8]);
            Capture(bench,pins,"Tracking",false);
            await At(4);
            Record($"Two scheduled optical passes. Magnetic unloading reduced momentum by {loop.Plant.DumpChange:F3} and applied impulse {loop.Plant.DumpImpulse:F3}.");
            for(int i=0;i<3;i++)
            {
                await Click(loop.buttons[9+i]);await Click(loop.buttons[12]);await Click(loop.buttons[13]);
                await Wait(()=>loop.buttons[9+(i+1)%3].interactable||loop.buttons[8].interactable,"recovery after disturbance "+(i+1));
                Record("Diagnosed and serviced disturbance "+(i+1)+".");
            }
            await Click(loop.buttons[8]);await At(5);
            await Click(loop.buttons[8]);await Wait(()=>loop.Plant.OpticalLocked,"live optical beam");
            await Click(loop.buttons[14]);
            var registry=Object.FindFirstObjectByType<SparkWireRegistry>();
            Check(registry!=null&&registry.Wires.Count==142,"All 142 fixed connections must appear in Wire Controls.");
            var last=registry.Wires.Last();
            var attitude=loop.Plant.Body;
            Check(registry.RemoveWire(last.WireId),"Remove the installed W097 through Wire Controls.");
            await Wait(()=>loop.FeedbackLatched&&loop.FeedbackObserved,"stale trusted quaternion and real link loss");
            Check(loop.Stage==5&&manager.SatisfiedTargetCount==5&&!loop.Trusted&&!loop.Plant.OpticalLocked,
                "W097 must retain 5/6 and inhibit optical lock.");
            Check(Quaternion.Angle(attitude,loop.Plant.Body)<12&&Quaternion.Angle(loop.Plant.Body,Quaternion.identity)>5,
                "W097 disconnect must preserve continuous attitude.");
            Record("W097 disconnected during the live optical link. Fine authorization dropped; attitude remained continuous and 5/6 retained.");
            Capture(bench,pins,"FeedbackFault",false);
            Check(registry.RestoreRemovedWire(last.WireId),"Restore W097 through Wire Controls.");
            await Click(loop.buttons[4]);await Click(loop.buttons[15]);await Click(loop.buttons[16]);
            await Click(loop.buttons[8]);await At(6);await Wait(()=>manager.IsCompleted,"campaign completion");
            Record($"6/6 — Campaign manager Completed after fresh two-pass mission {loop.RunId}.");
            Capture(bench,pins,"Verified",false);
        }
        catch (Exception error)
        {
            Record("FAIL: " + error);
            if (loop != null) Record("Control panel: " + loop.statusText.text + " | " + loop.meterText.text + " | " + loop.detailText.text);
            if(loop!=null)Record($"Diagnostics: stage {loop.Stage}, rate {loop.Plant.RateDegrees.magnitude:F2}, stars {loop.Plant.Confidence:F2}, uncertainty {loop.Plant.Uncertainty:F2}, momentum {loop.Plant.RelativeMomentum:F3}, dump {loop.Plant.DumpChange:F3}, fault {Read<int>(loop,"fault")}, W097 latch {loop.FeedbackLatched}, live loss {loop.FeedbackObserved}. {loop.eventText.text}");
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

    private static readonly MethodInfo pick = typeof(SignalWireBuilder).GetMethod("FindTerminalForNewMode",
        BindingFlags.Instance | BindingFlags.NonPublic);

    private static async Task At(int stage)
    {
        await Wait(() => loop.Stage == stage && manager.SatisfiedTargetCount == stage, $"{stage}/6");
        Record($"{stage}/6 — {loop.statusText.text} | {loop.meterText.text}");
    }

    private static async Task Click(Button button)
    {
        await Wait(() => button.isActiveAndEnabled && button.interactable, button.name + " available");
        Check(button.onClick.GetPersistentEventCount() == 1 && button.onClick.GetPersistentTarget(0) == loop,
            "Missing Level 84 action on " + button.name);
        int pressedFrame=Time.frameCount;
        button.onClick.Invoke();
        await Wait(()=>Time.frameCount>pressedFrame,"next frame after "+button.name);
    }

    private static void Capture(Transform bench, Dictionary<string, SparkTerminal> pins, string name, bool overhead)
    {
        var view = new GameObject("Level 84 test camera");
        var camera = view.AddComponent<Camera>();
        camera.enabled = false;
        view.transform.position = bench.TransformPoint(new Vector3(0, 2.90f, overhead ? -.86f : -2.84f));
        view.transform.rotation = Quaternion.LookRotation(bench.TransformPoint(new Vector3(0, .73f, 0)) -
            view.transform.position, bench.up);
        camera.orthographic = true;
        camera.orthographicSize = bench.TransformVector(new Vector3(0, 1.23f, 0)).magnitude;
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
            File.WriteAllBytes("Logs/Level84_" + name + ".png", snapshot.EncodeToPNG());
            if (!overhead) return;
            var rows = new List<string> { "Terminal,X,Y" };

            foreach (var pair in pins)
            {
                var p = camera.WorldToViewportPoint(pair.Value.transform.position);
                rows.Add($"{pair.Key},{Mathf.RoundToInt(p.x * width)},{Mathf.RoundToInt((1f - p.y) * height)}");
            }
            File.WriteAllLines("Logs/Level84_Pins.csv", rows);
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
        var timeout = DateTime.UtcNow.AddSeconds(48);
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











