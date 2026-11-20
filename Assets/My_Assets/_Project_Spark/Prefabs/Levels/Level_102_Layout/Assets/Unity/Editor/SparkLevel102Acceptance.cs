using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.SignalPhantom;
using SparkLevel102Process = ProjectSpark.Gameplay.SignalPhantom.SparkLevel102Process;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.SignalPhantom
{
    [InitializeOnLoad]
    internal static class Level102Acceptance
    {
        private const string Request = "Logs/Level102Acceptance.request";
        private const string Report = "Logs/Level102Acceptance.txt";
        private const string Session = "ProjectSpark.Level102.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel102Process board;

        static Level102Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/102/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "SIGNAL PHANTOM / HSX10-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel102Builder.Prefab);
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
                        PlayerPrefs.SetString(SparkLevel102Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel102Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel102Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel102Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel102Process.CheckpointKey);
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
            data.FindProperty("enforceProgressionAccess").boolValue = false;
            data.FindProperty("autoAdvanceOnCompletion").boolValue = false;
            data.FindProperty("autoEvaluate").boolValue = true;
            data.ApplyModifiedPropertiesWithoutUndo();
            var levels = data.FindProperty("levels");
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel102Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index >= 0 && manager.StartLevel(index), "MainMenu loads Signal Phantom.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board)
                && board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.Powered &&
                !board.JumperPresent && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Campaign starts at 0/6 with the board empty and W109 open.");
            Check(board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 216 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 6 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 28,
                "Serialized board has 216 route endpoints, six layers and 28 native via barrels.");
            Check(board.jumper.from.name == "U9.TRUSTED_LINK_QUALITY_OUT" &&
                board.jumper.to.name == "U11.TRUSTED_LINK_QUALITY_IN", "W109 maps to the intended U9 and U11 terminals.");
            var boardArt = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform
                .Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(boardArt.size.x - 10.13f) < .10f && Mathf.Abs(boardArt.size.z - 7.091f) < .09f &&
                Mathf.Abs(boardArt.center.x - center.x) < .04f && Mathf.Abs(boardArt.center.z - center.z) < .04f,
                $"Board art fits {boardArt.size.x:F2} × {boardArt.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            await Task.Delay(1100);
            await Screenshot("Level102Opening");
            board.SelectPart(1);
            Check(!board.FitPart(), "Wrong component is rejected.");
            board.SelectPart(0);
            board.RotatePart();
            Check(!board.FitPart() && !board.Solder(), "Wrong orientation and incomplete placement cannot be soldered.");
            board.SelectPart(0);
            Click("Fit part");
            Check(board.parts[0].model.activeSelf && !board.parts[0].contact.IsClosed, "Fit preview leaves the solder contact open.");
            Click("Solder");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && board.parts[0].model.activeSelf,
                "Assembly controls close a real contact.");
            await Wait(() => PlayerPrefs.HasKey(SparkLevel102Process.CheckpointKey), "saved assembly");
            var initial = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Reload Signal Phantom for checkpoint verification.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board != initial && board.GetComponentInChildren<Canvas>(), "reloaded board");
            Click("Resume");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && !board.Powered && !board.Calibrated,
                "Resume restores the solder contact with power isolated and calibration cleared.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                if (!board.FitPart() || !board.Solder()) throw new InvalidOperationException("Assembly part " + i);
                if (i % 14 == 0) await Task.Yield();
            }
            await Stage(1);
            var first = board.routes[0];
            board.SetLayer(first.layer % 6 + 1);
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
            await Screenshot("Level102Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                var route = board.routes[i];
                board.SelectRoute(i);
                if (route.barrel && !board.PlateVia()) throw new InvalidOperationException(route.viaId);
                if (!board.ConnectPads()) throw new InvalidOperationException(route.id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 108 && board.Plated == 28 && !board.JumperPresent && !board.TogglePower(),
                "An assembled 108-route network still requires W109.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "W109 connected through the native circuit graph.");
            await Stage(2);
            Check(board.witnesses.All(pin => board.Voltage(pin) > 4.7f), "All six training continuity branches conduct.");
            Check(!board.Calibrate() && !board.Train() && !board.RunDiagnostics(), "Unpowered checks cannot create a capture.");
            await PowerUp();
            Check(!board.Train() && !board.RunDiagnostics(), "Calibration is required before either lane trains.");
            await TrainLanes();
            await Stage(3);
            Check(board.lanes.All(lane => board.Voltage(lane.receiveN) > 1.65f && board.Voltage(lane.receiveP) < .15f),
                "Both differential receivers follow the verified transmit polarity.");
            await Capture();
            await Stage(4);
            Check(board.Qualifies && board.ObservedErrors(0) == 0 && board.ObservedErrors(1) == 0 &&
                Mathf.Abs(board.MeasuredOhms(0) - 100f) < 1f && Mathf.Abs(board.MeasuredOhms(1) - 100f) < 1f,
                "Both lanes pass a finite 4096-bit interval with 100 Ω TDR terminations.");

            for (int fault = 0; fault < 3; fault++)
            {
                int epoch = board.Epoch;
                int priorRun = board.RunNumber;
                Check(board.BeginFaultTrial() && !board.Qualifies, "Fault invalidates the previous qualification.");
                await Capture();
                Check(board.FaultWitnessed && !board.Qualifies && board.Running && board.Epoch == epoch && board.RunNumber > priorRun,
                    "A new failing capture leaves the controller and lane training online.");
                if (fault == 0)
                {
                    Check(Mathf.Abs(board.lanes[0].termination.Resistance - 150f) < .1f &&
                        Mathf.Abs(board.MeasuredOhms(0) - 150f) < 1f && Mathf.Abs(board.MeasuredOhms(1) - 100f) < 1f,
                        "F1 affects the native R1 load and Lane A TDR alone reports 150 Ω.");
                }
                else if (fault == 1)
                {
                    float coupled = board.MeasuredCoupling(0);
                    Check(coupled > .15f && board.SpacingMm < .3f, "F2 compresses the modeled parallel spacing and raises measured coupling.");
                    Check(board.LocateFault() && !board.Repair(), "Trace spacing cannot be repaired live.");
                    board.ToggleAggressor();
                    await Capture();
                    Check(board.CouplingIsolated && board.MeasuredCoupling(0) < .03f && !board.Qualifies,
                        "Pausing the aggressor isolates coupling without awarding a qualification.");
                }
                else
                {
                    var via = board.routes[board.stubRoute];
                    Check(via.id == "W039" && via.viaId == "V19" && via.exitLayer == 3 && via.barrel.IsConducting &&
                        board.Plated == 28 && board.Routed == 108 && board.MeasuredStub(1) >= .16f,
                        "F3 produces a Lane B stub reflection while V19 still conducts.");
                    await Task.Delay(150);
                    await Screenshot("Level102ViaStub");
                }
                board.SelectFootprint(0);
                board.SelectRoute(0);
                Check(!board.Repair() && board.TogglePower() && !board.Repair() && board.LocateFault() && board.Repair(),
                    "Only the diagnosed target can be repaired after power isolation.");
                await PowerUp();
                await TrainLanes();
                await Capture();
                Check(board.Repaired == fault + 1 && board.Qualifies && board.Epoch > epoch &&
                    board.MeasuredCoupling(0) < .15f && board.MeasuredStub(1) < .16f,
                    "A later calibrated training epoch and full capture certify the repair.");
            }
            await Stage(5);

            await Wait(() => board.Fresh && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f, "live transmission authorization");
            int protectedEpoch = board.Epoch;
            int runningSamples = board.Samples;
            Click("Open W109");
            await Wait(() => board.Inhibited && board.Voltage(board.protectedFeed) < .25f && board.Samples > runningSamples,
                "U11 inhibits the permit while the controller continues");
            Check(!manager.CircuitSystem.HasConnectionBetween(board.jumper.from, board.jumper.to) &&
                board.Running && board.Voltage(board.monitorSupply) > 3f && board.Voltage(board.laneRail) > 1.66f &&
                !board.Fresh && !board.Qualifies && !board.ResetSafety(),
                "W109 opening breaks the actual health connection without depowering U11 or the lanes.");
            Click("Restore W109");
            await Wait(() => board.Fresh, "new W109 pulse cadence");
            Check(board.Inhibited && board.Epoch == protectedEpoch, "Reconnection and fresh edges alone keep the permit inhibited.");
            await Capture();
            Check(board.Qualifies && board.Inhibited && manager.SatisfiedTargetCount == 5, "A new capture without explicit reset cannot clear the latch.");
            Click("Reset safety");
            Check(board.Inhibited && !board.Trained && !board.Qualifies && !board.RunDiagnostics(),
                "U11 reset requires retraining and invalidates any old capture.");
            await TrainLanes();
            Check(board.Epoch > protectedEpoch && board.Inhibited, "A fresh training epoch alone cannot enable the output.");
            await Capture();
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f,
                "The fresh two-lane capture and restored hardware permit certify all six objectives.");
            board.BoardView();
            Click("TDR / Eye");
            await Task.Delay(170);
            var plot = board.GetComponentInChildren<SparkLevel102SignalPlot>();
            Check(plot && plot.TryGetComponent<CanvasRenderer>(out var plotRenderer) && !plotRenderer.cull,
                "The eye plot has an active Canvas renderer.");
            await Screenshot("Level102Complete");
        }

        private static async Task PowerUp()
        {
            Check(board.TogglePower(), "Enable the isolated protected bench.");
            Check(!board.regulator.IsConducting && !board.transceiverRegulator.IsConducting && !board.Calibrated,
                "Bench startup waits for both gates and starts uncalibrated.");
            await Wait(() => board.Running && board.Voltage(board.vdd) > 3.05f &&
                board.Voltage(board.laneRail) > 1.66f && board.Voltage(board.resetSense) > 2.65f, "rail and reference startup");
        }

        private static async Task TrainLanes()
        {
            Check(board.Calibrate() && board.Train(), "Calibrate probes and train both native lanes.");
            await Wait(() => board.Trained, "both receiver polarities");
        }

        private static async Task Capture()
        {
            int previous = board.RunNumber;
            Check(board.RunDiagnostics(), "Acquire fresh TDR, eye and finite PRBS capture.");
            await Wait(() => board.RunNumber > previous && !board.Measuring, "both signal captures");
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
                    throw new TimeoutException($"{step}. Stage {board?.Stage}, evidence {board?.Evidence}, epoch {board?.Epoch}, qualified {board?.Qualifies}, fresh {board?.Fresh}, manager {manager?.SatisfiedTargetCount}. " +
                        (board ? $"Input {board.Voltage(board.standby):F3} V, logic {board.Voltage(board.vdd):F3} V, link rail {board.Voltage(board.laneRail):F3} V, U11 feed {board.Voltage(board.monitorSupply):F3} V, clock {board.Voltage(board.clockSense):F3} V, reset {board.Voltage(board.resetSense):F3} V." : "Board unavailable."));
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
