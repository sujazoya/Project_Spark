using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.HiddenLayers;
using SparkLevel101Process = ProjectSpark.Gameplay.HiddenLayers.SparkLevel101Process;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.HiddenLayers
{
    [InitializeOnLoad]
    internal static class Level101Acceptance
    {
        private const string Request = "Logs/Level101Acceptance.request";
        private const string Report = "Logs/Level101Acceptance.txt";
        private const string Session = "ProjectSpark.Level101.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel101Process board;

        static Level101Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/101/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE HIDDEN LAYERS / MLX9-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel101Builder.Prefab);
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
                        PlayerPrefs.SetString(SparkLevel101Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel101Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel101Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel101Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel101Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel101Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index >= 0 && manager.StartLevel(index), "MainMenu loads The Hidden Layers.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board)
                && board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.Powered &&
                !board.JumperPresent && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Campaign starts at 0/6 with the board empty and W101 open.");
            Check(board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 200 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 6 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 24,
                "Serialized board has 200 route endpoints, six layers and 24 native via barrels.");
            Check(board.jumper.from.name == "U7.TRUSTED_LINK_HEALTH_OUT" &&
                board.jumper.to.name == "U8.TRUSTED_LINK_HEALTH_IN", "W101 maps to the intended U7 and U8 terminals.");
            var boardArt = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform
                .Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(boardArt.size.x - 10.13f) < .10f && Mathf.Abs(boardArt.size.z - 7.047f) < .09f &&
                Mathf.Abs(boardArt.center.x - center.x) < .04f && Mathf.Abs(boardArt.center.z - center.z) < .04f,
                $"Board art fits {boardArt.size.x:F2} × {boardArt.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            await Task.Delay(1100);
            await Screenshot("Level101Opening");
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
            await Wait(() => PlayerPrefs.HasKey(SparkLevel101Process.CheckpointKey), "saved assembly");
            var initial = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Reload The Hidden Layers for checkpoint verification.");
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
            await Screenshot("Level101Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                var route = board.routes[i];
                board.SelectRoute(i);
                if (route.barrel && !board.PlateVia()) throw new InvalidOperationException(route.viaId);
                if (!board.ConnectPads()) throw new InvalidOperationException(route.id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 100 && board.Plated == 24 && !board.JumperPresent && !board.TogglePower(),
                "An assembled 100-route network still requires W101.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "W101 connected through the native circuit graph.");
            await Stage(2);
            Check(board.witnesses.All(pin => board.Voltage(pin) > 4.7f), "All six training continuity branches conduct.");
            Check(!board.Program() && !board.BootAndTestMemory() && !board.Calibrate() && !board.Train(),
                "Unpowered tools cannot program, boot or certify the board.");
            await PowerUp();
            Check(!board.BootAndTestMemory() && !board.Calibrate(), "Blank flash blocks boot and calibration.");
            board.SelectImage();
            Check(!board.Program(), "The programmer rejects a mismatched target.");
            board.SelectImage();
            Check(!board.Program(), "The programmer rejects a corrupt CRC.");
            board.SelectImage();
            Check(board.Program(), "The programmer accepts the approved virtual MLX-9 image.");
            await Wait(() => board.HasImage && !board.Programming, "flash program and readback");
            await BootMemory();
            await Stage(3);
            Check(board.Running && board.MemoryVerified && board.Inhibited, "A verified flash and RAM test boots the MCU with the protected output inhibited.");
            Check(!board.Train(), "Uncalibrated probes cannot train the lanes.");
            await TrainLanes();
            Check(board.lanes.All(lane => board.Voltage(lane.receiveN) > 1.65f && board.Voltage(lane.receiveP) < .15f),
                "The native differential receivers reflect the verified transmit polarity.");
            await Capture();
            await Stage(4);
            Check(board.Qualifies && board.ReturnOhms < .3f && board.MeasuredSkew(0) < 15f &&
                board.ObservedErrors(0) == 0 && board.ObservedErrors(1) == 0,
                "Both lanes pass the return, skew and finite 4096-bit tests.");

            for (int fault = 0; fault < 3; fault++)
            {
                int epoch = board.Epoch;
                int previous = board.RunNumber;
                Check(board.BeginFaultTrial() && !board.Qualifies, "The fault invalidates the previous qualification.");
                await Capture();
                Check(board.FaultWitnessed && !board.Qualifies && board.Running && board.Epoch == epoch && board.RunNumber > previous,
                    "A fresh failed capture leaves the MCU running.");
                if (fault == 0)
                {
                    var via = board.routes[board.memoryRoute];
                    Check(via.id == "W035" && via.viaId == "V09" && via.exitLayer == 3 && !via.barrel.IsConducting &&
                        board.Plated == 23 && board.Routed == 99 && !board.MemoryVerified &&
                        board.FlashDrive > 2.65f && board.FlashReceive < .25f,
                        "F1 opens V09 and the physical SPI clock disappears only at U2.");
                }
                else if (fault == 1)
                {
                    Check(!board.groundStitch.IsConducting && board.ReturnOhms > 20f && board.Routed == 100 && board.Plated == 24,
                        "F2 raises measured return impedance while the visible copper tasks remain complete.");
                    await Task.Delay(140);
                    await Screenshot("Level101GroundReturn");
                }
                else
                {
                    Check(board.MeasuredSkew(0) > 70f && board.MeasuredSkew(1) < 15f &&
                        board.routes[board.skewRoute].copper.positionCount == 6 && board.Routed == 100 && board.Plated == 24,
                        "F3 lengthens the Lane A signal path and preserves DC continuity.");
                    await Task.Delay(140);
                    await Screenshot("Level101PairSkew");
                }
                board.SelectRoute(0);
                Check(!board.Repair() && board.TogglePower() && !board.Repair() && board.LocateFault() && board.Repair(),
                    "Internal rework requires the measured target and an isolated bench.");
                await PowerUp();
                Check(board.HasImage && !board.MemoryVerified, "The programmed image survives isolation and requires a new memory test.");
                await BootMemory();
                await TrainLanes();
                await Capture();
                Check(board.Repaired == fault + 1 && board.Epoch > epoch && board.Qualifies &&
                    board.ReturnOhms < .3f && board.MeasuredSkew(0) < 15f,
                    "New memory, calibration and lane evidence verify the repair.");
            }
            await Stage(5);

            await Wait(() => board.Fresh && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f, "live U8 authorization");
            int protectedEpoch = board.Epoch;
            int runningSamples = board.Samples;
            Click("Open W101");
            await Wait(() => board.Inhibited && board.Voltage(board.protectedFeed) < .25f && board.Samples > runningSamples,
                "U8 inhibits the permit while the MCU stays powered");
            Check(!manager.CircuitSystem.HasConnectionBetween(board.jumper.from, board.jumper.to) &&
                board.Running && board.Voltage(board.monitorSupply) > 3f && board.Voltage(board.laneRail) > 1.66f &&
                !board.Fresh && !board.Qualifies && !board.ResetSafety(),
                "W101 opens the actual feedback path and leaves U8 and MCU powered.");
            Click("Restore W101");
            await Wait(() => board.Fresh, "fresh W101 pulse cadence");
            Check(board.Inhibited && board.Epoch == protectedEpoch && !board.ResetSafety(),
                "Reconnection and fresh edges require a new qualifying capture before reset.");
            await Capture();
            Check(board.Qualifies && board.Inhibited && manager.SatisfiedTargetCount == 5,
                "A qualifying capture without explicit reset still cannot clear the latch.");
            Click("Reset safety");
            Check(board.Inhibited && !board.Trained && !board.Qualifies && !board.RunDiagnostics(),
                "Reset invalidates old data and requires training again.");
            await TrainLanes();
            Check(board.Epoch > protectedEpoch && board.Inhibited, "Fresh training alone cannot authorize transmission.");
            await Capture();
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f,
                "Fresh two-lane evidence and the restored safety permit certify all six objectives.");
            board.BoardView();
            await Task.Delay(170);
            var plot = board.GetComponentInChildren<ProjectSpark.Gameplay.SignalPhantom.SparkLevel102SignalPlot>();
            Check(plot && plot.TryGetComponent<CanvasRenderer>(out var renderer) && !renderer.cull,
                "The signal plot has an active Canvas renderer.");
            await Screenshot("Level101Complete");
        }

        private static async Task PowerUp()
        {
            Check(board.TogglePower(), "Enable the isolated protected bench.");
            Check(!board.regulator.IsConducting && !board.transceiverRegulator.IsConducting && !board.Calibrated,
                "Regulator gates and calibration start unqualified.");
            await Wait(() => board.ControllerReady && board.Voltage(board.vdd) > 3.05f &&
                board.Voltage(board.laneRail) > 1.66f && board.Voltage(board.resetSense) > 2.65f, "rail, clock and reset startup");
        }

        private static async Task BootMemory()
        {
            Check(board.BootAndTestMemory(), "Boot the approved image and check the native flash clock.");
            await Wait(() => board.MemoryVerified && board.Running, "flash CRC and RAM readback");
        }

        private static async Task TrainLanes()
        {
            Check(board.Calibrate() && board.Train(), "Calibrate probes and train both native lanes.");
            await Wait(() => board.Trained, "both receiver polarities");
        }

        private static async Task Capture()
        {
            int previous = board.RunNumber;
            Check(board.RunDiagnostics(), "Acquire fresh return, TDR, eye and finite PRBS evidence.");
            await Wait(() => board.RunNumber > previous && !board.Measuring, "both signal captures");
            Log($"{board.Evidence} · L5 return {board.ReturnOhms:F2} Ω · Lane A skew {board.MeasuredSkew(0):F1} ps");
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
                        (board ? $"Input {board.Voltage(board.standby):F3} V, logic {board.Voltage(board.vdd):F3} V, link rail {board.Voltage(board.laneRail):F3} V, U8 feed {board.Voltage(board.monitorSupply):F3} V, clock {board.Voltage(board.clockSense):F3} V, reset {board.Voltage(board.resetSense):F3} V." : "Board unavailable."));
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
