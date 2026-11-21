using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.ThermalDominion;
using SparkLevel114Process = ProjectSpark.Gameplay.ThermalDominion.SparkLevel114Process;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.ThermalDominion
{
    [InitializeOnLoad]
    internal static class Level114Acceptance
    {
        private const string Request = "Logs/Level114Acceptance.request";
        private const string Report = "Logs/Level114Acceptance.txt";
        private const string Session = "ProjectSpark.Level114.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel114Process board;

        static Level114Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/114/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE THERMAL DOMINION / TDX22-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel114Builder.Prefab);
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
                        PlayerPrefs.SetString(SparkLevel114Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel114Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel114Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel114Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel114Process.CheckpointKey);
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
            Check(manager && loader && wire, "Campaign manager, layout loader and native Wire tool resolve.");
            if (manager.LevelProgression) manager.LevelProgression.enabled = false;
            var data = new SerializedObject(manager);
            data.FindProperty("autoAdvanceOnCompletion").boolValue = false;
            data.FindProperty("autoEvaluate").boolValue = true;
            data.ApplyModifiedPropertiesWithoutUndo();
            var levels = data.FindProperty("levels");
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel114Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index >= 0 && index == levels.arraySize - 1 && manager.LevelProgression &&
                manager.LevelProgression.CanPlayLevel(index) && manager.StartLevel(index),
                "MainMenu starts The Thermal Dominion with existing campaign positions preserved.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.JumperPresent &&
                !board.Powered && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Starts empty at 0/6 with W257 open and bench power isolated.");
            Check(board.parts.Length == 182 && board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 512 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 20 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 88,
                "Serialized board contains 182 footprints, 256 route pairs, 20 layers and 88 native vias.");
            Check(board.jumper.from.name == "U11.SAFE_THERMAL_HEALTH_OUT" &&
                board.jumper.to.name == "U10.SAFE_THERMAL_HEALTH_IN",
                "W257 connects the fictional U11 health output to the independent U10 input.");
            var bounds = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(bounds.size.x - 10.13f) < .10f && Mathf.Abs(bounds.size.z - 7.091f) < .09f &&
                Mathf.Abs(bounds.center.x - center.x) < .04f && Mathf.Abs(bounds.center.z - center.z) < .04f,
                $"Board art fits {bounds.size.x:F2} × {bounds.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            Check(board.fixture.Find("TDX-22 placement legend").GetComponent<MeshFilter>().sharedMesh.vertexCount > 2000,
                "The original footprints have visible placement markings.");
            await Screenshot("Level114Opening");
            board.SelectPart(1);
            Check(!board.FitPart(), "Mismatched parts are rejected.");
            board.SelectPart(0);
            board.RotatePart();
            Check(!board.FitPart() && !board.Solder(), "Wrong orientation cannot be secured.");
            board.SelectPart(0);
            Click("Fit part");
            Check(board.parts[0].model.activeSelf && !board.parts[0].contact.IsClosed && !board.Solder(),
                "Placement preview requires contact inspection before closing the contact.");
            Click("Inspect contacts");
            Click("Secure part");
            await Wait(() => board.Installed == 1 && PlayerPrefs.HasKey(SparkLevel114Process.CheckpointKey), "saved first part");
            var firstBoard = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Campaign reloads the board for checkpoint verification.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) && board != firstBoard &&
                board.GetComponentInChildren<Canvas>(), "reloaded board");
            Click("Resume");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && !board.Powered && !board.Calibrated,
                "Checkpoint restores the real assembly contact with power isolated.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                if (!board.FitPart() ||
                    ((i < 12 || board.coolingParts.Contains(i) || board.parts[i].label.StartsWith("J", StringComparison.Ordinal)) &&
                    !board.InspectContacts()) || !board.Solder())
                    throw new InvalidOperationException("Assembly failed at " + board.parts[i].label);
                if (i % 16 == 0) await Task.Yield();
            }
            await Stage(1);
            Check(board.Installed == 182 && board.CoolingMounted && !board.JumperPresent, "All original placements and five cooling mounts complete.");
            var first = board.routes[0];
            board.SetLayer(2);
            Check(!board.PlateVia() && !board.ConnectPads(), "Wrong-layer plating and wiring are blocked.");
            board.SelectRoute(0);
            Check(!board.ConnectPads() && board.PlateVia(), "The first route needs its own plated span.");
            board.PadView();
            var start = Camera.main.WorldToScreenPoint(first.from.transform.position);
            var end = Camera.main.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(end), "Native Wire tool connects the highlighted physical terminals.");
            await Wait(() => board.Routed == 1 && board.Plated == 1, "first route and via");
            await Screenshot("Level114Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                board.SelectRoute(i);
                if (board.routes[i].barrel && !board.PlateVia() || !board.ConnectPads())
                    throw new InvalidOperationException("Native connection failed at " + board.routes[i].id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 256 && board.Plated == 88 && !board.TogglePower(),
                "All copper still requires the independent W257 connection.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "W257 connects in the authoritative circuit.");
            await Stage(2);
            Check(board.witnesses.All(pin => board.Voltage(pin) > 4.7f) && !board.CalibrateCooling(),
                "All continuity groups solve and unpowered calibration is blocked.");
            Click("Bench power");
            await CheckCalibration();
            await Stage(3);
            await HeatRun();
            await Stage(4);
            Check(board.Qualifies && board.MeasuredAirflow >= .68f && board.PeakDelta < 18f,
                "Independent heat profile passed on both dies, the VRM and FAN1.");
            Click("Thermal camera");
            await Screenshot("Level114Thermal");

            for (int fault = 0; fault < 3; fault++)
            {
                int prior = board.Epoch;
                Check(board.BeginFaultTrial() && board.Epoch > prior && !board.LocateFault(),
                    $"Fault {fault + 1} invalidates earlier evidence.");
                await CheckCalibration();
                await HeatRun();
                Check(!board.Qualifies && board.LocateFault() && !board.Repair(),
                    $"Fault {fault + 1} is diagnosed from a failed run and energized service is blocked.");
                Check(fault == 0 ? board.PeakDelta >= 18f :
                    fault == 1 ? board.TachDisagreement > .50f && board.MeasuredAirflow < .35f :
                    board.PeakVrmDrop >= 14f && board.routes[board.thermalViaIndex].barrel.IsClosed,
                    $"Fault {fault + 1} has distinct measured evidence.");
                await Screenshot("Level114Fault" + (fault + 1));
                Check(board.TogglePower(), "Bench isolated for service.");
                if (fault < 2)
                {
                    Check(!board.Repair() && board.ToggleService() && !board.CoolingMounted && !board.TogglePower(),
                        "Removing the diagnosed assembly blocks power and exposes its mount.");
                    Check(board.Repair() && board.ToggleService() && board.CoolingMounted, "The repaired assembly is retained again.");
                }
                else
                {
                    Check(!board.Repair() && board.InspectThermalVia(), "VRM sleeve repair requires the diagnosed span.");
                    var route = board.routes[board.thermalViaIndex];
                    board.SetLayer(route.exitLayer);
                    Check(!board.Repair(), "Wrong-layer thermal rework is blocked.");
                    board.SetLayer(route.layer);
                    Check(board.Repair() && route.barrel.IsClosed, "Thermal sleeve rebuilt with electrical continuity preserved.");
                }
                board.BoardView();
                Check(board.TogglePower(), "Power restored only after service.");
                await CheckCalibration();
                await HeatRun();
                int completed = fault + 1;
                await Wait(() => board.Repaired == completed && board.Qualifies, "fresh repaired profile");
            }
            await Stage(5);
            await Wait(() => !board.Inhibited && board.Fresh, "initial independent U10 permit");
            Check(board.OpenW257() && !board.JumperPresent && board.Inhibited && board.Powered &&
                board.Voltage(board.monitorSupply) > 3f && board.Voltage(board.protectedFeed) < .15f &&
                manager.SatisfiedTargetCount == 5, "Opening W257 removes the native edge and inhibits workload while diagnostics remain powered.");
            Check(!board.ResetSafety() && board.RestoreW257() && !board.ResetSafety(),
                "Restoring the native wire alone cannot reset the independent latch.");
            await CheckCalibration();
            await HeatRun();
            await Wait(() => board.Fresh, "fresh W257 heartbeat before reset");
            Check(board.Inhibited && board.ResetSafety() && board.Inhibited && !board.Calibrated,
                "The isolated U10 reset requires and invalidates pre-reset evidence.");
            await CheckCalibration();
            await HeatRun();
            await Stage(6);
            Check(manager.IsCompleted && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f,
                "LevelGamePlayManager completes the level with fresh calibration, heat evidence and restored protected load.");
            board.BoardView();
            await Screenshot("Level114Complete");
            Click("Layer view");
            for (int layer = 1; layer <= 20; layer++)
            {
                board.SetLayer(layer);
                Check(board.routes.Count(route => route.copper.enabled) > 0 &&
                    board.routes.All(route => route.copper.enabled == (route.layer == layer)) &&
                    board.CoolingMounted && board.Routed == 256 && board.Plated == 88,
                    $"Layer {layer:00} isolates its native copper overlay with the installed graph intact.");
            }
            await Screenshot("Level114LayerView");
            Click("Angle view");
            Check(Vector3.Angle(Camera.main.transform.forward, Vector3.down) > 25f &&
                board.routes.All(route => !route.copper.enabled) && board.CoolingMounted,
                "Angled inspection exposes both original cooling stacks while retaining the solved board.");
            await Screenshot("Level114Inspection");
        }

        private static async Task CheckCalibration()
        {
            await Wait(() => board.RailsReady, "solved power rails");
            Check(board.CalibrateCooling(), "Calibrate eight sensor feeds and the independent airflow sweep.");
            await Wait(() => board.Calibrated, "fresh sensor and airflow calibration");
            Log(board.Evidence);
        }

        private static async Task HeatRun()
        {
            Check(board.RunEndurance(), "Start the 48-second simulated heat profile.");
            await Wait(() => !board.Testing, "completed thermal profile");
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
                    string rails = $"Bench {board.Voltage(board.standby):F3}, AUX {board.Voltage(board.aux):F3}, U1 {board.Voltage(board.cpuRail):F3}, U2 {board.Voltage(board.acceleratorRail):F3}";
                    throw new TimeoutException($"{step}. Stage {board.Stage}, epoch {board.Epoch}. {rails}. {board.Evidence}");
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
