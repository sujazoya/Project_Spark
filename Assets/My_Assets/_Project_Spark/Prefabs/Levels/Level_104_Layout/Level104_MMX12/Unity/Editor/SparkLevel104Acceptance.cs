using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.MemoryMatrix;
using SparkLevel104Process = ProjectSpark.Gameplay.MemoryMatrix.SparkLevel104Process;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.MemoryMatrix
{
    [InitializeOnLoad]
    internal static class Level104Acceptance
    {
        private const string Request = "Logs/Level104Acceptance.request";
        private const string Report = "Logs/Level104Acceptance.txt";
        private const string Session = "ProjectSpark.Level104.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel104Process board;

        static Level104Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/104/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "MEMORY MATRIX / MMX12-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel104Builder.Prefab);
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
                        PlayerPrefs.SetString(SparkLevel104Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel104Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel104Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel104Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel104Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel104Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index >= 0 && manager.StartLevel(index), "MainMenu loads Memory Matrix.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board)
                && board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.Powered &&
                !board.JumperPresent && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Campaign starts at 0/6 with the board empty and W125 open.");
            Check(board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 248 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 8 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 34,
                "Serialized board has 248 route endpoints, eight layers and 34 native via barrels.");
            Check(board.jumper.from.name == "U1.TRUSTED_MEMORY_READY_OUT" &&
                board.jumper.to.name == "U9.TRUSTED_MEMORY_READY_IN", "W125 maps to the intended U1 and U9 terminals.");
            var boardArt = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform
                .Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(boardArt.size.x - 10.13f) < .10f && Mathf.Abs(boardArt.size.z - 7.208f) < .09f &&
                Mathf.Abs(boardArt.center.x - center.x) < .04f && Mathf.Abs(boardArt.center.z - center.z) < .04f,
                $"Board art fits {boardArt.size.x:F2} × {boardArt.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            await Task.Delay(1100);
            await Screenshot("Level104Opening");
            board.SelectPart(1);
            Check(!board.FitPart(), "Wrong component is rejected.");
            board.SelectPart(0);
            board.RotatePart();
            Check(!board.FitPart() && !board.Solder(), "Wrong orientation and incomplete placement cannot be soldered.");
            board.SelectPart(0);
            Click("Fit part");
            Check(board.parts[0].model.activeSelf && !board.parts[0].contact.IsClosed, "Fit preview leaves the solder contact open.");
            Check(!board.Solder(), "BGA solder is blocked before alignment inspection.");
            Click("Inspect BGA");
            Click("Solder");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && board.parts[0].model.activeSelf,
                "Assembly controls close a real contact.");
            await Wait(() => PlayerPrefs.HasKey(SparkLevel104Process.CheckpointKey), "saved assembly");
            var initial = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Reload Memory Matrix for checkpoint verification.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board != initial && board.GetComponentInChildren<Canvas>(), "reloaded board");
            Click("Resume");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && !board.Powered && !board.Initialized,
                "Resume restores the solder contact with power isolated and training cleared.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                if (!board.FitPart() || (i < 3 && !board.InspectBga()) || !board.Solder()) throw new InvalidOperationException("Assembly part " + i);
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
            await Screenshot("Level104Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                var route = board.routes[i];
                board.SelectRoute(i);
                if (route.barrel && !board.PlateVia()) throw new InvalidOperationException(route.viaId);
                if (!board.ConnectPads()) throw new InvalidOperationException(route.id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 124 && board.Plated == 34 && !board.JumperPresent && !board.TogglePower(),
                "An assembled 124-route network still requires W125.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "W125 connected through the native circuit graph.");
            await Stage(2);
            Check(board.witnesses.All(pin => board.Voltage(pin) > 4.7f), "All six training continuity branches conduct.");
            Check(!board.InitializeMemory() && !board.Train() && !board.RunData(), "Unpowered conditions block initialization, training and memory traffic.");
            Check(board.TogglePower(), "Enable the protected input.");
            Check(!board.vppGate.IsConducting && !board.vddGate.IsConducting &&
                !board.vttGate.IsConducting && !board.InitializeMemory(), "Initialization cannot skip the sequenced supplies.");
            await Wait(() => board.vppGate.IsConducting, "VPP comes up first");
            Check(!board.vddGate.IsConducting && !board.vttGate.IsConducting, "The wordline rail comes up before the data and termination rails.");
            await Wait(() => board.SupplyReady, "all DDR supplies and VREF");
            Check(!board.Train() && !board.RunData(), "Valid rails alone cannot train or test uninitialized devices.");
            Click("Initialize DDR4");
            await Wait(() => board.Initialized, "both initialization targets");
            await Stage(3);
            Check(board.CkVerified && board.CaLevel > .9f && board.CaReflection < .01f,
                "Both complementary CK states and RN1 command termination are measured.");
            Check(!board.RunData(), "Initialized devices still require lane timing training.");
            await Train();
            await Data();
            await Stage(4);
            Check(board.Qualifies && board.Bits == 32768 && board.Errors == 0 &&
                Enumerable.Range(0, 4).All(lane => board.ReadMargin(lane) >= 85f && board.WriteMargin(lane) >= 85f),
                "Four lanes scan valid read/write taps and compare 32,768 bits across four patterns.");

            for (int fault = 0; fault < 3; fault++)
            {
                int oldEpoch = board.Epoch;
                Check(board.BeginFaultTrial() && !board.Qualifies && !board.Trained, "Fault invalidates old timing and data evidence.");
                if (fault < 2)
                {
                    Check(board.Train(), "Start a new electrical training probe.");
                    await Wait(() => board.FaultWitnessed && !board.Training, "failing training evidence");
                    Check(!board.Trained && !board.RunData() && board.Initialized, "Measured timing failure blocks data qualification without wiping initialization.");
                    if (fault == 0)
                    {
                        var route = board.routes[board.strobeRoute];
                        Check(route.id == "W063" && route.viaId == "V19" && route.exitLayer == 3 &&
                            !route.barrel.IsConducting && board.Routed == 123 && board.Plated == 33 &&
                            board.Lane2StrobeHigh < .20f && board.ReadMargin(2) == 0 &&
                            board.ReadMargin(0) > 85f, "F1 opens the native byte-2 DQS path and the other lane remains measurable.");
                    }
                    else
                    {
                        Check(Mathf.Abs(board.termination.Resistance - 220f) < .1f && board.CaReflection > .60f &&
                            board.CaLevel > 1.12f && board.WriteMargin(0) < 85f,
                            "F2 changes solved CA high voltage and compresses read/write margins.");
                        await Task.Delay(120);
                        await Screenshot("Level104TerminationFault");
                    }
                }
                else
                {
                    await Train();
                    await Data();
                    Check(board.FaultWitnessed && !board.Qualifies && board.Hottest >= 44f &&
                        board.HotBgaOpens > 0 && board.Errors > 0 && board.FirstFailure.Contains("U3 BYTE 3"),
                        "F3 trains at low activity, then opens a hot contact under load and identifies byte 3 data corruption.");
                    await Task.Delay(130);
                    await Screenshot("Level104IntermittentFault");
                }
                board.SelectFootprint(0);
                board.SelectRoute(0);
                Check(!board.Repair() && board.TogglePower() && !board.Repair() && board.LocateFault() && board.Repair(),
                    "Isolated rework requires the measured fault target.");
                if (fault == 2)
                {
                    Check(board.Reflowing && !board.TogglePower(), "Isolated BGA rework blocks bench power during the thermal cycle.");
                    await Wait(() => !board.Reflowing, "simulated BGA rework and cooldown");
                }
                await PowerUp();
                await Initialize();
                await Train();
                await Data();
                Check(board.Repaired == fault + 1 && board.Qualifies && board.Epoch > oldEpoch &&
                    board.Bits == 32768 && board.Errors == 0 && board.HotBgaOpens == 0,
                    "Fresh initialization, training and a clean finite data test qualify the reworked board.");
            }
            await Stage(5);

            await Wait(() => board.Fresh && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f, "native U9 workload permit");
            int before = board.Epoch;
            Click("Open W125");
            await Wait(() => board.Inhibited && board.Voltage(board.protectedFeed) < .25f, "U9 inhibits protected diagnostic workload");
            Check(!manager.CircuitSystem.HasConnectionBetween(board.jumper.from, board.jumper.to) && board.Initialized &&
                board.Voltage(board.monitorSupply) > 3f && board.Voltage(board.vdd) > 1.14f && !board.Fresh && !board.Trained &&
                !board.Qualifies && !board.ResetSafety(), "W125 physically opens while U9 and both DDR devices remain powered.");
            Click("Restore W125");
            Check(board.Inhibited && !board.ResetSafety() && !board.RunData(), "Reconnection alone cannot reuse old training or data.");
            await Train();
            await Data();
            await Wait(() => board.Fresh, "fresh qualified W125 pulse cadence");
            Check(board.Epoch > before && board.Inhibited && manager.SatisfiedTargetCount == 5,
                "Fresh pre-reset evidence retains the pending latch.");
            int resetEpoch = board.Epoch;
            Click("Reset safety");
            Check(!board.Trained && !board.Qualifies && !board.RunData() && board.Inhibited,
                "The explicit reset clears stale evidence and requires another full pass.");
            await Train();
            await Data();
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && !board.Inhibited && board.Epoch > resetEpoch &&
                board.Voltage(board.protectedFeed) > 3f && board.Qualifies,
                "Fresh post-reset four-lane training and data restore the physical U9 permit and certify all objectives.");
            board.BoardView();
            await Task.Delay(180);
            var plot = board.GetComponentInChildren<ProjectSpark.Gameplay.MemoryMatrix.SparkLevel104TrainingPlot>();
            Check(plot && plot.TryGetComponent<CanvasRenderer>(out var renderer) && !renderer.cull, "The DDR4 timing plot is visible.");
            await Screenshot("Level104Complete");
            board.SetLayer(1);
            Click("Layer view");
            await Screenshot("Level104LayerView");
            for (int layer = 1; layer <= 8; layer++)
            {
                Check(board.routes.Any(route => route.copper.enabled) &&
                    board.routes.All(route => route.copper.enabled == (route.layer == layer)) &&
                    board.jumper.copper.enabled && board.Powered && manager.IsCompleted,
                    $"Live diagnostics isolate copper on layer {layer} and retain the certified powered board.");
                if (layer < 8) Click("Next layer");
            }
            Click("Board view");
            Check(board.routes.All(route => !route.copper.enabled), "Board view clears the layer overlay.");
        }

        private static async Task PowerUp()
        {
            Check(board.TogglePower(), "Enable the isolated bench.");
            await Wait(() => board.SupplyReady, "qualified DDR4 voltage domains");
        }

        private static async Task Initialize()
        {
            Check(board.InitializeMemory(), "Start native CK/CA verification and initialization.");
            await Wait(() => board.Initialized && board.CkVerified, "initialized DRAM");
        }

        private static async Task Train()
        {
            Check(board.Train(), "Begin four-lane timing training.");
            await Wait(() => board.Trained && !board.Training, "qualified lane windows");
        }

        private static async Task Data()
        {
            int previous = board.CaptureNumber;
            Check(board.RunData(), "Run address, walking-bit and seeded write/read tests.");
            await Wait(() => board.CaptureNumber > previous && !board.Testing, "all memory bursts");
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
                        (board ? $"Input {board.Voltage(board.standby):F3} V, AUX {board.Voltage(board.aux):F3} V, VPP {board.Voltage(board.vpp):F3} V, VDD {board.Voltage(board.vdd):F3} V, VTT {board.Voltage(board.vtt):F3} V, VREF {board.Voltage(board.vref):F3} V, U9 {board.Voltage(board.monitorSupply):F3} V." : "Board unavailable."));
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
