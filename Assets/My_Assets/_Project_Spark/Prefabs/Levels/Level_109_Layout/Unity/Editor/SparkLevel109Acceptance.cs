using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.ThermalEndurance;
using SparkLevel109Process = ProjectSpark.Gameplay.ThermalEndurance.SparkLevel109Process;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.ThermalEndurance
{
    [InitializeOnLoad]
    internal static class Level109Acceptance
    {
        private const string Request = "Logs/Level109Acceptance.request";
        private const string Report = "Logs/Level109Acceptance.txt";
        private const string Session = "ProjectSpark.Level109.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel109Process board;

        static Level109Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/109/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THERMAL ENDURANCE / TRX17-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel109Builder.Prefab);
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
                        PlayerPrefs.SetString(SparkLevel109Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel109Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel109Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel109Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel109Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel109Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index == levels.arraySize - 1 && manager.LevelProgression && manager.LevelProgression.CanPlayLevel(index) &&
                manager.StartLevel(index), "MainMenu loads Thermal Endurance with normal access and existing campaign positions preserved.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board)
                && board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.Powered &&
                !board.JumperPresent && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Campaign starts at 0/6 with the board empty and W165 open.");
            Check(board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 328 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 10 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 54,
                "Serialized board has 328 route endpoints, ten layers and 54 native via barrels.");
            Check(board.jumper.from.name == "U18.TRUSTED_THERMAL_HEALTH_OUT" &&
                board.jumper.to.name == "U19.TRUSTED_THERMAL_HEALTH_IN", "W165 maps to the intended U18 and U19 terminals.");
            var boardArt = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform
                .Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(boardArt.size.x - 10.13f) < .10f && Mathf.Abs(boardArt.size.z - 7.429f) < .09f &&
                Mathf.Abs(boardArt.center.x - center.x) < .04f && Mathf.Abs(boardArt.center.z - center.z) < .04f,
                $"Board art fits {boardArt.size.x:F2} × {boardArt.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            var legend = board.fixture.Find("TRX-17 placement legend").GetComponent<MeshFilter>();
            Check(legend.sharedMesh.vertexCount >= 124 * 12, "Authored footprint markings are visible at the original placements.");
            Check(board.boardArt.GetComponentsInChildren<MeshRenderer>().SelectMany(item => item.sharedMaterials)
                .Any(material => material.GetTexture("_BaseMap")?.name == "L109_PCB_BaseColor" &&
                    material.GetTexture("_BumpMap")?.name == "L109_PCB_Normal"), "The graphite board uses the current TRX-17 base and normal maps.");
            await Task.Delay(1100);
            await Screenshot("Level109Opening");
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
            await Wait(() => PlayerPrefs.HasKey(SparkLevel109Process.CheckpointKey), "saved assembly");
            var initial = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Reload Thermal Endurance for checkpoint verification.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board != initial && board.GetComponentInChildren<Canvas>(), "reloaded board");
            Click("Resume");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && !board.Powered && !board.Calibrated,
                "Resume restores the solder contact with power isolated and training cleared.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                if (!board.FitPart() || ((i < 3 || board.parts[i].label.StartsWith("J", StringComparison.Ordinal)) && !board.InspectContacts()) || !board.Solder()) throw new InvalidOperationException("Assembly part " + i);
                if (i % 14 == 0) await Task.Yield();
            }
            Check(board.Installed == 124 && manager.SatisfiedTargetCount == 0 && !board.CoolingMounted,
                "All electronic parts alone cannot satisfy the assembly objective.");
            Click("CPU heatsink");
            Check(board.heatsink.activeSelf && !board.CoolingMounted, "The physical heatsink installs separately from the board components.");
            Click("VRM spreader");
            Check(board.spreader.activeSelf && board.CoolingMounted, "Both source cooling assemblies mount at their original locations.");
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
            await Screenshot("Level109Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                var route = board.routes[i];
                board.SelectRoute(i);
                if (route.barrel && !board.PlateVia()) throw new InvalidOperationException(route.viaId);
                if (!board.ConnectPads()) throw new InvalidOperationException(route.id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 164 && board.Plated == 54 && !board.JumperPresent && !board.TogglePower(),
                "An assembled 164-route network still requires W165.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "W165 connected through the native circuit graph.");
            await Stage(2);
            Check(board.witnesses.All(pin => board.Voltage(pin) > 4.7f), "All seven native training continuity branches conduct.");
            Check(!board.CalibrateCooling() && !board.RunEndurance(), "Unpowered calibration and endurance are blocked.");
            await PowerUp();
            Check(!board.ToggleCooling(true) && !board.ToggleCooling(false) && !board.RunEndurance(),
                "Hot cooling service and uncalibrated endurance are blocked.");
            await Calibrate();
            await Stage(3);
            Check(board.Voltage(board.fanSense) > 3f && board.CpuTemperature > board.SinkTemperature,
                "Cold-idle CPU and heatsink samples are distinct and the fan response is solved.");
            await Endurance();
            await Stage(4);
            Check(board.Qualifies && board.Bits == 49152 && board.Errors == 0 && board.PeakCpu > board.CpuTemperature &&
                board.PeakMemory >= 49f, "A 48-second cold/warm cycle verifies thermal response and 49,152 memory bits.");

            for (int fault = 0; fault < 3; fault++)
            {
                int oldEpoch = board.Epoch;
                Click("Fault trial");
                Check(!board.Qualifies && !board.LocateFault(), "Fault injection revokes earlier data and blocks diagnosis without measurement.");
                await Calibrate();
                await Endurance();
                Check(!board.Qualifies, "The fault prevents current endurance qualification.");
                if (fault == 0)
                    Check(board.phases[2].mosfet.Resistance > 2f && board.PeakPhaseSpread >= 5f &&
                        board.PeakPhaseLossRatio >= 2.4f && board.Errors == 0, "Q05 adds native phase 3 loss and heat under equivalent loads.");
                else if (fault == 1)
                    Check(board.heatsink.activeSelf && board.PeakDelta >= 18f && board.Errors == 0,
                        "With the heatsink mounted, its defective interface produces excess junction-to-sink delta.");
                else
                    Check(board.HotOpenings >= 2 && board.ColdRecoveries >= 1 && board.HotErrors > 0 &&
                        board.ColdErrors == 0 && board.Voltage(board.ddrRail) > 1.14f,
                        "U3 loses its actual receive path across repeat warm cycles and recovers in cold samples while DDR remains powered.");
                Click("Locate fault");
                Check(board.FaultWitnessed && !board.Repair(), "The measured fault is localized and energized rework is rejected.");
                await Task.Delay(170);
                await Screenshot("Level109Fault" + (fault + 1));
                Click("Bench power");
                board.SelectFootprint(1);
                Check(!board.Repair(), "Selecting the wrong component cannot clear the fault.");
                if (fault == 1)
                {
                    board.InspectCooling(true);
                    Check(!board.Repair(), "TIM service requires physical heatsink removal.");
                    Click("CPU heatsink");
                    Check(!board.heatsink.activeSelf && !board.CoolingMounted && !board.TogglePower(),
                        "The heatsink is removable, and the open cooling mount blocks bench power.");
                    Click("Apply rework");
                    Click("CPU heatsink");
                    Check(board.CoolingMounted, "Renewed TIM and retention require remounting the original heatsink.");
                }
                else
                {
                    board.SelectFootprint(fault == 0 ? board.mosfetPart : board.memoryPart);
                    Click("Apply rework");
                    if (fault == 2)
                    {
                        Check(board.Reworking && !board.TogglePower(), "Bench power is locked out during U3 BGA rework.");
                        await Wait(() => !board.Reworking, "isolated U3 rework complete");
                    }
                }
                await PowerUp();
                await Calibrate();
                await Endurance();
                await Wait(() => board.Repaired == fault + 1, "fresh repair qualification");
                Check(board.Qualifies && board.Epoch > oldEpoch && board.Errors == 0 && board.Bits == 49152,
                    "The same fresh endurance test passes after the targeted repair.");
            }
            await Stage(5);
            await Wait(() => board.Fresh && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f, "fresh U19 peak permit");
            int previous = board.Epoch;
            Click("Open W165");
            Check(!manager.CircuitSystem.HasConnectionBetween(board.jumper.from, board.jumper.to) && board.RailsReady &&
                board.Inhibited && board.Voltage(board.protectedFeed) < .25f && board.Voltage(board.monitorSupply) > 3f &&
                !board.Qualifies && !board.ResetSafety(), "W165 physically opens, U19 inhibits peak operation, and independent diagnostics stay powered.");
            Click("Restore W165");
            Check(board.Inhibited && !board.ResetSafety() && !board.RunEndurance(), "W165 reconnection alone cannot reset the latch.");
            await Calibrate();
            await Endurance();
            await Wait(() => board.Fresh, "new pre-reset thermal cadence");
            Check(board.Epoch > previous && board.Inhibited && manager.SatisfiedTargetCount == 5,
                "Fresh pre-reset qualification alone keeps the protected relay open.");
            int beforeReset = board.Epoch;
            Click("Reset safety");
            Check(!board.Calibrated && !board.Qualifies && !board.RunEndurance(), "Explicit reset requires another new cooling and endurance result.");
            await Calibrate();
            await Endurance();
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && board.Epoch > beforeReset && !board.Inhibited &&
                board.Voltage(board.protectedFeed) > 3f && board.Qualifies, "Post-reset evidence restores the native protected peak relay.");
            Click("Board overview");
            await Task.Delay(160);
            var plot = board.GetComponentInChildren<SparkLevel109TrainingPlot>();
            Check(plot && plot.TryGetComponent<CanvasRenderer>(out var renderer) && !renderer.cull, "The sampled thermal traces render.");
            await Screenshot("Level109Complete");
            Click("Thermal camera");
            await Task.Delay(140);
            Check(board.thermalMarkers.All(marker => marker.enabled), "Thermal camera displays measured locations on the board.");
            await Screenshot("Level109Thermal");
            board.SetLayer(1);
            Click("Layer view");
            await Task.Delay(140);
            await Screenshot("Level109LayerView");
            for (int layer = 1; layer <= 10; layer++)
            {
                Check(board.routes.Any(route => route.copper.enabled) &&
                    board.routes.All(route => route.copper.enabled == (route.layer == layer)) &&
                    board.Powered && manager.IsCompleted, $"Topology overlays isolate layer {layer:00} without changing the solved level.");
                if (layer < 10) Click("Next layer");
            }
        }

        private static async Task PowerUp()
        {
            Check(board.TogglePower(), "Enable isolated bench power.");
            await Wait(() => board.RailsReady, "verified auxiliary, CPU and DDR rails");
        }

        private static async Task Calibrate()
        {
            await Wait(() => board.CpuTemperature < 57.5f, "idle sensor cooldown");
            Check(board.CalibrateCooling(), "Start a fresh cold-idle fan and sensor calibration.");
            await Wait(() => board.Calibrated, "native fan response and idle sensors");
        }

        private static async Task Endurance()
        {
            int previous = board.Captures;
            Check(board.RunEndurance(), "Start the fixed-step workload and thermal cycle.");
            await Wait(() => board.Captures > previous && !board.Testing, "completed 48-second simulated endurance");
            Log(board.Evidence);
            Log($"Warm opens {board.HotOpenings}, cool recoveries {board.ColdRecoveries}, warm/cold errors {board.HotErrors}/{board.ColdErrors}. Phase loss ratio {board.PeakPhaseLossRatio:F2}.");
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
                    string rails = $"Bench {board.Voltage(board.standby):F3}, CPU {board.Voltage(board.cpuRail):F3}, DDR {board.Voltage(board.ddrRail):F3}, fan {board.Voltage(board.fanSense):F3}";
                    throw new TimeoutException($"{step}. Stage {board.Stage}, epoch {board.Epoch}. {rails}. " +
                        $"CPU {board.CpuTemperature:F1}, sink {board.SinkTemperature:F1}, U3 max {board.PeakMemory:F1} °C. {board.Evidence}");
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
