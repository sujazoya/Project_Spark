using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.SignalCrucible;
using SparkLevel111Process = ProjectSpark.Gameplay.SignalCrucible.SparkLevel111Process;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.SignalCrucible
{
    [InitializeOnLoad]
    internal static class Level111Acceptance
    {
        private const string Request = "Logs/Level111Acceptance.request";
        private const string Report = "Logs/Level111Acceptance.txt";
        private const string Session = "ProjectSpark.Level111.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel111Process board;

        static Level111Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/111/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE SIGNAL CRUCIBLE / SIX19-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel111Builder.Prefab);
                Check(prefab && prefab.LevelDefinition && prefab.SceneBindings, "Serialized campaign prefab resolves.");
                SessionState.SetBool(Session, true);
                EditorApplication.EnterPlaymode();
            }
            catch (Exception error) { Log("FAIL " + error); }
        }

        private static async void Play(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Session, false)) return;
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.EraseBool(Session);
                if (SessionState.GetBool(Session + ".checkpointCaptured", false))
                {
                    if (SessionState.GetBool(Session + ".hadCheckpoint", false))
                        PlayerPrefs.SetString(SparkLevel111Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel111Process.CheckpointKey);
                    PlayerPrefs.Save();
                    SessionState.EraseBool(Session + ".checkpointCaptured");
                    SessionState.EraseBool(Session + ".hadCheckpoint");
                    SessionState.EraseString(Session + ".checkpoint");
                }
                Log("Returned to Edit Mode.");
            }
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            var background = Application.runInBackground;
            var filter = Debug.unityLogger.filterLogType;
            Application.runInBackground = true;
            Debug.unityLogger.filterLogType = LogType.Error;
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel111Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel111Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel111Process.CheckpointKey);
            try
            {
                await Run();
                Log("COMPLETE. Live LevelGamePlayManager evaluated all six objectives.");
            }
            catch (Exception error) { Log("FAIL " + error); }
            finally
            {
                Application.runInBackground = background;
                Debug.unityLogger.filterLogType = filter;
                EditorApplication.ExitPlaymode();
            }
        }

        private static async Task Run()
        {
            manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
            var loader = Object.FindFirstObjectByType<SparkLevelLayoutLoader>();
            var wire = Object.FindFirstObjectByType<SignalWireBuilder>();
            Check(manager && loader && wire, "Campaign and native Wire tool resolve.");
            if (manager.LevelProgression) manager.LevelProgression.enabled = false;
            var data = new SerializedObject(manager);
            data.FindProperty("autoAdvanceOnCompletion").boolValue = false;
            data.FindProperty("autoEvaluate").boolValue = true;
            data.ApplyModifiedPropertiesWithoutUndo();
            var levels = data.FindProperty("levels");
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel111Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index == levels.arraySize - 1 && manager.LevelProgression && manager.LevelProgression.CanPlayLevel(index) &&
                manager.StartLevel(index), "MainMenu loads The Signal Crucible with normal access and existing campaign positions preserved.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board)
                && board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.Powered &&
                !board.JumperPresent && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Campaign starts at 0/6 with the board empty and W185 open.");
            Check(board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 368 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 14 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 60,
                "Serialized board has 368 route endpoints, 14 layers and 60 native via barrels.");
            Check(board.jumper.from.name == "U18.TRUSTED_LINK_HEALTH_OUT" &&
                board.jumper.to.name == "U19.TRUSTED_LINK_HEALTH_IN", "W185 maps to the intended U18 and U19 terminals.");
            var boardArt = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform
                .Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(boardArt.size.x - 10.13f) < .10f && Mathf.Abs(boardArt.size.z - 7.367f) < .09f &&
                Mathf.Abs(boardArt.center.x - center.x) < .04f && Mathf.Abs(boardArt.center.z - center.z) < .04f,
                $"Board art fits {boardArt.size.x:F2} × {boardArt.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            var legend = board.fixture.Find("SIX-19 placement legend").GetComponent<MeshFilter>();
            Check(legend.sharedMesh.vertexCount >= 138 * 12, "Authored footprint markings are visible at the original placements.");
            await Task.Delay(1100);
            await Screenshot("Level111Opening");
            board.SelectPart(1);
            Check(!board.FitPart(), "Wrong component is rejected.");
            board.SelectPart(0);
            board.RotatePart();
            Check(!board.FitPart() && !board.Solder(), "Wrong orientation and incomplete placement cannot be soldered.");
            board.SelectPart(0);
            Click("Fit part");
            Check(board.parts[0].model.activeSelf && !board.parts[0].contact.IsClosed, "Fit preview leaves the solder contact open.");
            Check(!board.Solder(), "Precision solder is blocked before contact inspection.");
            Click("Inspect contacts");
            Click("Solder");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && board.parts[0].model.activeSelf,
                "Assembly controls close a real contact.");
            await Wait(() => PlayerPrefs.HasKey(SparkLevel111Process.CheckpointKey), "saved assembly");
            var initial = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Reload The Signal Crucible for checkpoint verification.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board != initial && board.GetComponentInChildren<Canvas>(), "reloaded board");
            Click("Resume");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && !board.Powered && !board.Commissioned,
                "Resume restores the solder contact with power isolated and training cleared.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                if (!board.FitPart() || ((i < 20 || board.parts[i].label.StartsWith("J", StringComparison.Ordinal)) && !board.InspectContacts()) || !board.Solder()) throw new InvalidOperationException("Assembly part " + i);
                if (i % 14 == 0) await Task.Yield();
            }
            await Stage(1);
            var first = board.routes[0];
            board.SetLayer(first.layer % 8 + 1);
            Check(!board.PlateVia() && !board.ConnectPads(), "Wrong-layer copper and via actions are blocked.");
            board.SelectRoute(0);
            Check(!board.ConnectPads() && board.PlateVia(), "An open barrel blocks its required copper.");
            board.PadView();
            var camera = Camera.main;
            var start = camera.WorldToScreenPoint(first.from.transform.position);
            var end = camera.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(end), "Native Wire tool connects the exact pair of highlighted pads.");
            await Wait(() => board.Routed == 1 && board.Plated == 1, "first route and via registered");
            await Task.Delay(120);
            await Screenshot("Level111Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                var route = board.routes[i];
                board.SelectRoute(i);
                if (route.barrel && !board.PlateVia()) throw new InvalidOperationException(route.viaId);
                if (!board.ConnectPads()) throw new InvalidOperationException(route.id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 184 && board.Plated == 60 && !board.JumperPresent && !board.TogglePower(),
                "An assembled 184-route network still requires W185.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "W185 connected through the native circuit graph.");
            await Stage(2);
            Check(board.witnesses.All(pin => board.Voltage(pin) > 4.7f), "All six native continuity groups conduct.");
            Check(!board.Commission() && !board.ProbeTdr() && !board.ProbeEye() && !board.RunPattern(),
                "Unpowered commissioning and instrument actions are blocked.");
            await PowerUp();
            Check(!board.Commission(), "Commissioning requires an explicit differential reference check.");
            await VerifyClock(true);
            Check(!board.RunPattern() && !board.CalibrateEqualizer(), "Clock qualification alone cannot run or calibrate the pattern.");
            await Commission(true);
            await Stage(3);
            Check(!board.RunPattern(), "The four-lane launch alone cannot substitute for TDR, eye or equalizer evidence.");
            await QualifyPattern();
            await Stage(4);
            Check(board.Qualifies && board.Bits == 65536 && board.Errors == 0 && board.PeakTemperature >= 53f &&
                Enumerable.Range(0, 4).All(board.HealthyLane), "Four-lane hot qualification checked 65,536 bits with zero observed errors.");

            for (int fault = 0; fault < 3; fault++)
            {
                int previousEpoch = board.Epoch;
                Click("Fault trial");
                Check(!board.Qualifies && !board.LocateFault(), "Fault injection invalidates old evidence and blocks speculative diagnosis.");
                await VerifyClock(fault != 1);
                if (fault == 0)
                {
                    Check(!board.routes[board.returnRoute].barrel.IsConducting && board.Plated == 59 && board.Routed == 183 &&
                        board.Voltage(board.returnSense) > 1f && board.Powered,
                        "V37 opens the actual lane 2 return branch while retaining diagnostic power.");
                    await Commission(false);
                    await QualifyPattern();
                    Check(board.TdrLoss < 14f && Mathf.Abs(board.TdrDistance - board.returnDistanceMm) < .08f &&
                        board.FirstErrorLane == 2 && board.Errors > 0 && !board.Qualifies,
                        "An excessive TDR echo locates V37 and the finite comparison detects lane 2 errors.");
                }
                else if (fault == 1)
                {
                    Check(board.ClockObserved && !board.ClockLocked && board.ClockJitter >= 18f && board.RailsReady &&
                        board.clockNoiseP.IsConducting && board.clockNoiseN.IsConducting && !board.Commission(),
                        "Both clock states remain detectable, but U5 phase-noise evidence blocks four-lane commissioning.");
                }
                else
                {
                    await Wait(() => board.Temperature < 45f, "ambient U8 for thermal comparison");
                    await Commission(true);
                    float ambientEye = board.EyeMv(0);
                    await QualifyPattern();
                    Check(board.PeakTemperature >= 53f && board.Voltage(board.equalizerBias) < 1.25f &&
                        board.EyeMv(0) < ambientEye && board.Errors > 0 && !board.Qualifies,
                        "U8 can train while cool, then a real hot bias change closes the eyes and causes pattern errors.");
                }

                Click("Locate fault");
                Check(board.FaultWitnessed && !board.Repair(), "The measurement locates the fault and blocks hot repair.");
                await Task.Delay(170);
                await Screenshot("Level111Fault" + (fault + 1));
                Click("Bench power");
                if (fault == 0)
                {
                    board.SetLayer(4);
                    Check(!board.Repair() && !board.PlateVia(), "Wrong-layer repair and generic via plating cannot bypass the diagnosed fault.");
                    board.SetLayer(3);
                }
                else
                {
                    board.SelectFootprint(0);
                    Check(!board.Repair(), "The wrong component cannot clear the measured fault.");
                    board.SelectFootprint(fault == 1 ? board.clockPart : board.retimerPart);
                }
                Click("Apply rework");
                if (fault == 2)
                {
                    Check(board.Reworking && !board.TogglePower(), "Bench power stays isolated during U8 thermal rework.");
                    await Wait(() => !board.Reworking, "thermal rework complete");
                }
                await PowerUp();
                await VerifyClock(true);
                await Commission(true);
                await QualifyPattern();
                await Wait(() => board.Repaired == fault + 1, "fresh independent repair evidence");
                Check(board.Epoch > previousEpoch && board.Qualifies && board.Errors == 0 &&
                    board.Bits == 65536 && board.PeakTemperature >= 53f, "Fresh TDR, eye, reference verification and clean hot-load data confirm the repair.");
            }
            await Stage(5);
            await Wait(() => board.Fresh && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f, "U19 independent permit");
            int old = board.Epoch;
            Click("Open W185");
            Check(!manager.CircuitSystem.HasConnectionBetween(board.jumper.from, board.jumper.to) && board.RailsReady &&
                board.Inhibited && board.Voltage(board.monitorSupply) > 3f && board.Voltage(board.protectedFeed) < .25f &&
                !board.Commissioned && !board.Qualifies && !board.ResetSafety(),
                "W185 physically opens, the workload is inhibited and independent diagnostics remain powered.");
            Click("Restore W185");
            Check(board.Inhibited && !board.ResetSafety() && !board.RunPattern(), "Physical reconnection alone cannot reset the U19 latch.");
            await Commission(true);
            await QualifyPattern();
            await Wait(() => board.Fresh, "fresh pre-reset W185 cadence");
            Check(board.Epoch > old && board.Inhibited && manager.SatisfiedTargetCount == 5,
                "U19 stays inhibited through the fresh pre-reset qualification.");
            int resetEpoch = board.Epoch;
            Click("Reset safety");
            Check(!board.Commissioned && !board.RunPattern() && !board.Qualifies && board.Inhibited,
                "Explicit reset requires another independent signal-evidence epoch.");
            await Commission(true);
            await QualifyPattern();
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && board.Epoch > resetEpoch && board.Qualifies &&
                !board.Inhibited && board.Voltage(board.protectedFeed) > 3f, "Post-reset four-lane evidence restores the physical U19 workload permit.");
            Click("Board overview");
            await Task.Delay(180);
            var plot = board.GetComponentInChildren<SparkLevel111TrainingPlot>();
            Check(plot && plot.TryGetComponent<CanvasRenderer>(out var renderer) && !renderer.cull, "The four-lane eye view renders.");
            await Screenshot("Level111Complete");
            board.SetLayer(3);
            Click("Layer view");
            await Task.Delay(140);
            await Screenshot("Level111LayerView");
            board.SetLayer(1);
            for (int layer = 1; layer <= 14; layer++)
            {
                Check(board.routes.Any(route => route.copper.enabled) &&
                    board.routes.All(route => route.copper.enabled == (route.layer == layer)) &&
                    board.Powered && manager.IsCompleted, $"Live topology isolates layer {layer:00}.");
                if (layer < 14) Click("Next layer");
            }
        }

        private static async Task PowerUp()
        {
            Check(board.TogglePower(), "Enable protected bench power.");
            await Wait(() => board.RailsReady, "3.3 V auxiliary and 1.8 V retimer supplies");
        }

        private static async Task VerifyClock(bool healthy)
        {
            await Wait(() => board.VerifyClock(), "both complementary clock states");
            Check(board.ClockObserved && board.ClockLocked == healthy, "Live differential REFCLK samples produce the expected jitter classification.");
        }

        private static async Task Commission(bool healthy)
        {
            Check(board.Commission(), "Run low-rate and 10 Gb/s commissioning.");
            await Wait(() => !board.Commissioning, "four-lane commissioning result");
            Check(board.Commissioned == healthy, "The current topology and signal samples produce the expected link result.");
        }

        private static async Task QualifyPattern()
        {
            Click("Probe TDR");
            Click("Measure eye");
            Click("Calibrate EQ");
            int last = board.Captures;
            Check(board.RunPattern(), "Start a new four-lane hot-load pattern.");
            await Wait(() => board.Captures > last && !board.Testing, "finite PRBS31 comparison");
            Log(board.Evidence);
        }

        private static async Task Screenshot(string name)
        {
            string path = Path.GetFullPath("Logs/" + name + ".png");
            File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            await Wait(() => File.Exists(path) && new FileInfo(path).Length > 5000, name + " capture");
        }

        private static void Click(string name)
        {
            var button = board.GetComponentsInChildren<Button>().Single(item => item.name == name);
            Check(button.interactable, name + " is available.");
            button.onClick.Invoke();
        }

        private static async Task Stage(int value)
        {
            await Wait(() => board.Stage == value && manager.SatisfiedTargetCount == value, $"{value}/6 objectives");
            Log($"PASS Manager objective {value}/6.");
        }

        private static async Task Wait(Func<bool> ready, string step)
        {
            var until = DateTime.UtcNow.AddSeconds(24);
            while (!ready())
            {
                if (DateTime.UtcNow > until)
                {
                    if (!board) throw new TimeoutException(step + ". Board unavailable.");
                    string rails = $"Bench {board.Voltage(board.standby):F3}, AUX {board.Voltage(board.aux):F3}, retimer {board.Voltage(board.retimerRail):F3}";
                    string eyes = string.Join(", ", Enumerable.Range(0, 4).Select(i => board.EyeMv(i).ToString("F0")));
                    throw new TimeoutException($"{step}. Stage {board.Stage}, epoch {board.Epoch}. {rails}. Jitter {board.ClockJitter:F1} ps. " +
                        $"Eyes {eyes} mV. Temperature {board.Temperature:F0} C. {board.Evidence}");
                }
                await Task.Delay(35);
            }
        }

        private static void Check(bool success, string label)
        {
            if (!success) throw new InvalidOperationException(label);
            Log("PASS " + label);
        }

        private static void Log(string label) => File.AppendAllText(Report, label + Environment.NewLine);
    }
}
