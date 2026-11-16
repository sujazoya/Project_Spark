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
internal static class Level87ModelPlayModeProbe
{
    private const string Request = "Logs/Level87ModelTest.request";
    private const string Report = "Logs/Level87ModelTest.txt";
    private const string Running = "Codex.Level87.ModelTest";
    private static LevelGamePlayManager manager;
    private static SparkLevel87Process loop;

    public static void Launch()
    {
        File.WriteAllText(Request,"start");
        CheckRequest();
    }
    static Level87ModelPlayModeProbe()
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
        File.WriteAllText(Report, "Level 87 — Fixed terminal assembly and The Gravity Engine test\n");
        try
        {
            if(SceneManager.GetActiveScene().path!=SparkLevel87ModelInstaller.ScenePath)
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(SparkLevel87ModelInstaller.ScenePath);
            if (refit && AssetDatabase.LoadAssetAtPath<GameObject>(SparkLevel87ModelInstaller.Prefab) != null)
                SparkLevel87ModelInstaller.Fit();
            SparkLevel87ModelInstaller.Install();
            Record("Installed the fitted assembly in the Level 87 prefab and registered the campaign.");
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
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel87ModelInstaller.Folder + "Level_87.asset");
            Check(definition != null && wireBuilder != null && loader != null, "Level definition, wire builder, or layout loader missing.");
            if (manager.LevelProgression != null) manager.LevelProgression.enabled = false;
            Write(manager, "enforceProgressionAccess", false);
            Write(manager, "autoAdvanceOnCompletion", false);
            Write(manager, "autoEvaluate", true);
            int levelIndex = Array.IndexOf(Read<SparkLevelDefinition[]>(manager, "levels"), definition);
            Check(levelIndex >= 0 && manager.StartLevel(levelIndex), "Level 87 is missing from the campaign or could not start.");
            await Wait(() => loader.ActiveLayout != null && manager.ActiveLevel == definition, "Level 87 loaded");
            loop = loader.ActiveLayout.GetComponent<SparkLevel87Process>();
            await Wait(() => loop != null && loop.Topology >= 0,
                "the active loop controller to check the empty circuit");

            var bench = loop.transform.Find("Gravity Engine Arena");
            var camera = Read<Camera>(wireBuilder, "targetCamera");
            Check(loop.componentNames.Length==316&&loop.componentNames.All(item=>!string.IsNullOrWhiteSpace(item))&&
                loop.componentLabels.Length==316&&loop.componentLabels.All(item=>item)&&loop.pinLabels.All(item=>item),"All terminals need a readable device name and an installed socket label.");
            var stage=bench.Find("L87_HD_VISUAL_ART/L87_GravityQualificationArenaBase");
            var floor=stage.GetComponentsInChildren<Renderer>();
            var floorBounds=floor[0].bounds;
            foreach(var item in floor)floorBounds.Encapsulate(item.bounds);
            Record($"Labelled arena deck: {floorBounds.size.x:F2} x {floorBounds.size.z:F2} Unity units. All 316 port labels are linked to equipment names.");
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
            Check(pins.Count==316,"Unexpected socket count.");
            Check(loop.rig.GetComponentsInChildren<Transform>(true).Count(t=>t.name.StartsWith("ANIM_",StringComparison.Ordinal))==68,
                "Asset rig lost a pivot.");
            Check(wireBuilder.DrawingMode==WireDrawingMode.TerminalToTerminal,"MainMenu must use the direct terminal cable mode.");
            Record($"316 distinct fixed equipment sockets, max mount gap {maxGap:0.00000} Unity units. 68 animation pivots verified.");
            var director=loop.GetComponent<SparkLevel87ArenaView>();
            var brain=director.Brain;
            Check(brain&&brain.GetComponent<Camera>()==camera,"Cinemachine must drive the wire tool's original Unity camera.");
            var legacy=camera.GetComponent<ProjectSpark.HolographicViewer.HolographicViewerCamera>();
            Check(!legacy||!legacy.enabled,"Legacy controller must yield its transform while Cinemachine is active.");
            foreach(var preset in new[]{(23,"Socket Camera"),(21,"Rotor Camera"),(24,"Showcase Camera"),(20,"Arena Camera")})
            {
                var initial=camera.transform.position;
                float initialZoom=camera.orthographicSize;
                Quaternion initialAim=camera.transform.rotation;
                await Click(loop.buttons[preset.Item1]);
                bool blended=false;
                await Wait(()=>
                {
                    blended|=brain.IsBlending;
                    return brain.ActiveVirtualCamera?.Name==preset.Item2&&!brain.IsBlending;
                },"blend to "+preset.Item2);
                bool changed=Vector3.Distance(initial,camera.transform.position)>.035f||
                    Mathf.Abs(initialZoom-camera.orthographicSize)>.035f||Quaternion.Angle(initialAim,camera.transform.rotation)>1;
                Record($"{preset.Item2} blend observed={blended}; orthographic size {initialZoom:F2} to {camera.orthographicSize:F2}.");
                Check(blended&&changed,
                    "The camera must blend to the new view.");
            }
            Record("Cinemachine blends verified across Arena, Socket, Rotor and Showcase on the wire tool's original camera.");
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
            Record($"Default view: {inView}/316 on screen; {selected} directly picked in that view before camera navigation.");
            if(hardToPick.Count>0)Record("Socket groups to approach with camera zoom: "+string.Join(", ",hardToPick));
            Capture(bench,pins,"Unwired",true);
            Check(inView==316&&selected==316,"All equipment sockets must be directly pickable in the default overview.");
            var rotor=bench.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="ANIM_GRAVITY_RING_MAIN_ROTATION");
            var homeRotation=rotor.localRotation;
            Vector3 homePrimary=pins["W137_OUT_ANALOG_FEEDBACK"].transform.position;
            var wires=SparkLevel87Wiring.Wires;
            const int critical=SparkLevel87Wiring.Feedback;
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
            Check(loop.Stage==0,"Network falsely qualified with W137 open.");
            Connect(critical);await Click(loop.buttons[2]);await At(1);
            var hidden=Read<Dictionary<Renderer,bool>>(loop,"hiddenCableRenderers");
            Check(hidden.Count>0&&hidden.Keys.All(renderer=>renderer.forceRenderingOff),"Focused equipment view must hide ordinary cables.");
            await Click(loop.buttons[22]);Check(hidden.Count==0,"Show all cables restores the regular wire view.");
            await Click(loop.buttons[22]);Check(hidden.Count>0,"Equipment view can be restored without rewiring.");
            Record("Cable View toggles rendered connections without changing the 158-wire circuit. The trusted W137 and selected cables stay visible in equipment view.");
            async Task StartTrial()
            {
                await Click(loop.buttons[3]);await Click(loop.buttons[4]);await Click(loop.buttons[5]);
                await Wait(()=>loop.Plant.Rpm>1.06f&&loop.Plant.LevitationQualified,"low-speed levitation trial");
            }
            async Task FinishCycle()
            {
                foreach(int i in new[]{6,7,8,9})await Click(loop.buttons[i]);
                await Wait(()=>loop.Plant.RestoredThermalTime>4.5f,"loaded stable gravity interval");
                Check(loop.Plant.Gravity>.42f&&loop.Plant.Gravity<.49f&&loop.Plant.LoadPower>3800&&loop.Plant.OpticalQuality>.85f,
                    "Certification requires actual gravity, powered essential loads and optical data.");
                Record($"Run {loop.RunId}: {loop.Plant.Rpm:F3} RPM, {loop.Plant.Gravity:F3} g, {loop.Plant.LoadPower:F0} W, data {loop.Plant.OpticalQuality:P1}, spectrum {loop.Plant.PeakFrequency:F2} Hz.");
            }
            async Task Support()
            {
                await Click(loop.buttons[10]);await Wait(()=>loop.Plant.Supported&&loop.buttons[3].interactable,"controlled touchdown");
            }

            await StartTrial();await At(2);
            await Wait(()=>Quaternion.Angle(homeRotation,rotor.localRotation)>5,"visible rotor phase advance");
            Check(Vector3.Distance(homePrimary,pins["W137_OUT_ANALOG_FEEDBACK"].transform.position)<.0002f,
                "Stationary gap sensor cable must remain seated on P019 while the ring rotates.");
            await FinishCycle();await At(4);Capture(bench,pins,"Running",false);
            for(int i=11;i<=13;i++)
            {
                await Click(loop.buttons[i]);await Click(loop.buttons[14]);await Click(loop.buttons[15]);
                await Wait(()=>Read<int>(loop,"fault")==0,"actual measured fault recovery");
                Record($"Measured fault {i-10} diagnosed and repaired.");
                await StartTrial();await FinishCycle();
                if(i<13)Check(loop.Stage==4,"Fewer than three repairs cannot earn the fifth objective.");
            }

            await At(5);await Support();
            await StartTrial();await Click(loop.buttons[16]);
            var registry=Object.FindFirstObjectByType<SparkWireRegistry>();
            Check(registry!=null&&registry.Wires.Count==158,"Wire Controls must contain all 158 actual connections.");
            var feedback=registry.Wires.Last();
            Check(registry.RemoveWire(feedback.WireId),"Physically remove W137 through Wire Controls.");
            await Wait(()=>loop.FeedbackLatched&&loop.FeedbackObserved,"physical W137 loss latch");
            Check(!loop.Trusted&&loop.Plant.Rpm>.4f&&loop.Stage==5&&manager.SatisfiedTargetCount==5,"W137 loss must preserve inertia, break trust and retain earned 5/6.");
            Capture(bench,pins,"FeedbackFault",false);
            await Wait(()=>loop.Plant.Supported,"physical coast-down and touchdown");
            Record($"W137 removed at {loop.SpeedAtLoss:F3} RPM. Rotor coasted onto touchdown support without losing 5/6.");
            Check(registry.RestoreRemovedWire(feedback.WireId),"Physically restore W137 through Wire Controls.");
            await Click(loop.buttons[17]);await Click(loop.buttons[18]);
            Check(loop.Stage==5,"Reconnect and reset alone cannot certify the mission.");
            await StartTrial();await FinishCycle();await At(6);
            await Wait(()=>manager.IsCompleted,"campaign completion");
            Record($"6/6 — Campaign manager Completed after fresh post-W137 gravity run {loop.RunId}.");
            Capture(bench,pins,"Verified",false);
            CaptureRoom();
            director.enabled=false;
            Check(!legacy||legacy.enabled,"The shared camera must return control to the legacy viewer after Level 87.");
            Record("Camera ownership returned to the shared viewer on teardown.");
        }
        catch (Exception error)
        {
            Record("FAIL: " + error);
            if (loop != null) Record("Control panel: " + loop.statusText.text + " | " + loop.meterText.text + " | " + loop.detailText.text);
            if(loop!=null)Record($"Diagnostics: run {loop.RunId}, RPM {loop.Plant.Rpm:F3}, gap {loop.Plant.MinimumGap:F2}, amp {loop.Plant.AmplifierTemperature:F1}, vibration {loop.Plant.Vibration:F3}, fault {Read<int>(loop,"fault")}, phase {Read<object>(loop,"phase")}, latch {loop.FeedbackLatched}, trusted {loop.Trusted}. {loop.eventText.text}");
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

    private static void CaptureRoom()
    {
        var room=GameObject.Find("PS_PRO_GAME_ROOM");
        if(!room)return;
        var go=new GameObject("Room overview capture");
        var camera=go.AddComponent<Camera>();camera.enabled=false;camera.fieldOfView=68;
        float scale=room.transform.lossyScale.y;
        go.transform.position=room.transform.TransformPoint(new Vector3(-6.1f,3.8f,6.3f));
        go.transform.LookAt(room.transform.TransformPoint(new Vector3(.55f,1.8f,-1.2f)));
        camera.nearClipPlane=.30f*scale;
        var target=new RenderTexture(1600,900,24);
        var old=RenderTexture.active;
        var shot=new Texture2D(1600,900,TextureFormat.RGB24,false);
        try
        {
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            shot.ReadPixels(new Rect(0,0,1600,900),0,0);shot.Apply();
            File.WriteAllBytes("Logs/SparkRoom_Validated.png",shot.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active=old;Object.Destroy(target);Object.Destroy(shot);Object.Destroy(go);
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
            "Missing Level 87 action on " + button.name);
        int pressedFrame=Time.frameCount;
        button.onClick.Invoke();
        await Wait(()=>Time.frameCount>pressedFrame,"next frame after "+button.name);
    }

    private static void Capture(Transform bench, Dictionary<string, SparkTerminal> pins, string name, bool overhead)
    {
        var view = new GameObject("Level 87 test camera");
        var camera = view.AddComponent<Camera>();
        camera.enabled = false;
        view.transform.position = bench.TransformPoint(new Vector3(0, 2.90f, overhead ? -3.50f : -3.78f));
        view.transform.position=SparkLevel87ArenaView.KeepCameraInsideRoom(GameObject.Find("PS_PRO_GAME_ROOM")?.transform,view.transform.position);
        view.transform.rotation = Quaternion.LookRotation(bench.TransformPoint(new Vector3(0, 1.05f, .1f)) -
            view.transform.position, bench.up);
        camera.orthographic = true;
        camera.orthographicSize = bench.TransformVector(new Vector3(0, 1.48f, 0)).magnitude;
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
            File.WriteAllBytes("Logs/Level87_" + name + ".png", snapshot.EncodeToPNG());
            if (!overhead) return;
            var rows = new List<string> { "Terminal,X,Y" };

            foreach (var pair in pins)
            {
                var p = camera.WorldToViewportPoint(pair.Value.transform.position);
                rows.Add($"{pair.Key},{Mathf.RoundToInt(p.x * width)},{Mathf.RoundToInt((1f - p.y) * height)}");
            }
            File.WriteAllLines("Logs/Level87_Pins.csv", rows);
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


















