using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.PowerCore;
using SparkLevel103Process = ProjectSpark.Gameplay.PowerCore.SparkLevel103Process;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.PowerCore
{
    [InitializeOnLoad]
    internal static class Level103Acceptance
    {
        private const string Request = "Logs/Level103Acceptance.request";
        private const string Report = "Logs/Level103Acceptance.txt";
        private const string Session = "ProjectSpark.Level103.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel103Process board;

        static Level103Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/103/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE POWER CORE / CPX11-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel103Builder.Prefab);
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
                        PlayerPrefs.SetString(SparkLevel103Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel103Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel103Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel103Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel103Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel103Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index >= 0 && manager.StartLevel(index), "MainMenu loads The Power Core.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board)
                && board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.Powered &&
                !board.JumperPresent && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Campaign starts at 0/6 with the board empty and W117 open.");
            Check(board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 232 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 8 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 30,
                "Serialized board has 232 route endpoints, eight layers and 30 native via barrels.");
            Check(board.jumper.from.name == "U10.TRUSTED_VCORE_GOOD_OUT" &&
                board.jumper.to.name == "U11.TRUSTED_VCORE_GOOD_IN", "W117 maps to the intended U10 and U11 terminals.");
            var boardArt = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform
                .Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(boardArt.size.x - 10.13f) < .10f && Mathf.Abs(boardArt.size.z - 7.5975f) < .09f &&
                Mathf.Abs(boardArt.center.x - center.x) < .04f && Mathf.Abs(boardArt.center.z - center.z) < .04f,
                $"Board art fits {boardArt.size.x:F2} × {boardArt.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            await Task.Delay(1100);
            await Screenshot("Level103Opening");
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
            await Wait(() => PlayerPrefs.HasKey(SparkLevel103Process.CheckpointKey), "saved assembly");
            var initial = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Reload The Power Core for checkpoint verification.");
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
            await Screenshot("Level103Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                var route = board.routes[i];
                board.SelectRoute(i);
                if (route.barrel && !board.PlateVia()) throw new InvalidOperationException(route.viaId);
                if (!board.ConnectPads()) throw new InvalidOperationException(route.id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 116 && board.Plated == 30 && !board.JumperPresent && !board.TogglePower(),
                "An assembled 116-route network still requires W117.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "W117 connected through the native circuit graph.");
            await Stage(2);
            Check(board.witnesses.All(pin => board.Voltage(pin) > 11.5f), "All six training continuity branches conduct.");
            var defaultVrm = new ProjectSpark.VoltageDominion.SparkDominionVrm();
            for (int i = 0; i < 35000; i++) defaultVrm.Step(12, true, 2);
            Check(defaultVrm.PowerGood && Math.Abs(defaultVrm.Voltage - 1.05) < .01 && defaultVrm.ConnectedPhases == 15,
                "The shared model still runs the Level 110 default 1.05 V profile.");
            Check(!board.Calibrate() && !board.RunProfiles(), "Unpowered probes cannot authorize a capture.");
            await PowerUp();
            Check(!board.RunProfiles() && board.Calibrate(), "Startup capture requires calibrated independent probes.");
            await Capture();
            await Stage(3);
            Check(board.ProfileCount == 1 && board.Model.ConnectedPhases == 15 && board.Model.PowerGood &&
                Enumerable.Range(0, 4).All(phase => board.Model.PhaseCurrent(phase) > .4),
                "Each native path enables an active phase in the load-line commissioning capture.");
            await Capture();
            await Stage(4);
            Check(board.Qualifies && board.ProfileCount == 4, "Idle 2 A, nominal 10 A, peak 20 A and transient loads qualify.");

            for (int fault = 0; fault < 3; fault++)
            {
                int previous = board.RunNumber;
                Check(board.BeginFaultTrial() && !board.Qualifies && !board.Calibrate(), "Injecting the fault invalidates old calibration and qualification.");
                await Capture();
                Check(board.FaultWitnessed && !board.Qualifies && board.Powered && board.RunNumber > previous,
                    "The diagnostic load produces a new measured failure.");
                if (fault == 0)
                {
                    Check(board.PeakPhase2Bias > .80 && board.Model.CurrentSpread > .30 && board.Model.ConnectedPhases == 15,
                        "F1 changes Phase 2 sense and balance while all native phase paths remain intact.");
                }
                else if (fault == 1)
                {
                    var via = board.routes[board.phase3Route];
                    Check(via.id == "W041" && via.viaId == "V017" && via.exitLayer == 5 && !via.barrel.IsConducting &&
                        board.Plated == 29 && board.Routed == 115 && board.Model.ConnectedPhases == 11 &&
                        board.MinimumPhase3 < .05 && board.Voltage(board.drivers[2]) < .25f,
                        "F2 physically opens the Phase 3 output and removes its current contribution.");
                }
                else
                {
                    Check(board.PeakPhase4Rise > 18 && board.Model.Phase4ExtraLoss > 1.95 && board.Model.ConnectedPhases == 15,
                        "F3 creates excess measured Phase 4 heat with all four paths intact.");
                    await Task.Delay(170);
                    await Screenshot("Level103ThermalFault");
                }
                Check(!board.Repair() && board.TogglePower(), "Rework is rejected live and input isolation succeeds.");
                await Wait(() => board.Model.Voltage < .075 && board.Model.Temperature <= 54.8, "discharge and safe cooldown");
                board.SelectFootprint(0);
                board.SelectRoute(0);
                Check(!board.Repair() && board.LocateFault() && board.Repair(), "Only the diagnosed component or via accepts isolated rework.");
                await PowerUp();
                Check(board.Calibrate(), "Recalibrate after the repaired phase settles.");
                await Capture();
                Check(board.Repaired == fault + 1 && board.Qualifies && board.ProfileCount == 4 && board.RunNumber > previous + 1 &&
                    board.Model.ConnectedPhases == 15 && board.PeakPhase2Bias < .15 && board.PeakPhase4Rise < 18,
                    "A new full measured load set qualifies the repaired VRM.");
            }
            await Stage(5);

            await Wait(() => board.Fresh && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f, "live U11 CPU permit");
            Click("Open W117");
            await Wait(() => board.Inhibited && board.Voltage(board.protectedFeed) < .25f, "U11 removes CPU authorization");
            Check(!manager.CircuitSystem.HasConnectionBetween(board.jumper.from, board.jumper.to) &&
                board.Voltage(board.monitorSupply) > 3f && board.Voltage(board.standby) > 11.5f && board.Model.Voltage > .85 &&
                !board.Fresh && !board.Qualifies && !board.ResetSafety(),
                "W117 opens the trusted path and keeps independent power and VCORE available.");
            Click("Restore W117");
            await Wait(() => board.Fresh, "fresh W117 good-signal cadence");
            Check(board.Inhibited && !board.ResetSafety(), "Fresh pulses alone cannot clear U11 or reuse earlier measurements.");
            await Capture();
            Check(board.Qualifies && board.Inhibited && manager.SatisfiedTargetCount == 5,
                "Pre-reset diagnostic load qualification still leaves CPU authorization inhibited.");
            int resetRun = board.RunNumber;
            Click("Reset safety");
            await Wait(() => !board.Inhibited && board.Voltage(board.protectedFeed) > 3f, "U11 reauthorizes CPU loads");
            Check(!board.Qualifies && manager.SatisfiedTargetCount == 5, "Explicit reset alone cannot certify the level.");
            await Capture();
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && !board.Inhibited && board.ProfileCount == 4 &&
                board.RunNumber > resetRun && board.Voltage(board.protectedFeed) > 3f,
                "A new protected load set and the live campaign manager certify all six objectives.");
            board.BoardView();
            await Task.Delay(170);
            var plot = board.GetComponentInChildren<ProjectSpark.Gameplay.PowerCore.SparkLevel103RailPlot>();
            Check(plot && plot.TryGetComponent<CanvasRenderer>(out var renderer) && !renderer.cull,
                "The VCORE capture is visible on the scope.");
            await Screenshot("Level103Complete");
        }

        private static async Task PowerUp()
        {
            Check(board.TogglePower(), "Enable the protected 12 V input.");
            Check(!board.logicGate.IsConducting && !board.auxGate.IsConducting && !board.Calibrated,
                "Power-up starts with both auxiliary gates open and calibration invalid.");
            await Wait(() => board.ReadyForCalibration && board.Voltage(board.standby) > 11.5f &&
                board.Voltage(board.aux33) > 3.05f && board.Voltage(board.aux18) > 1.66f, "auxiliary rails, four phases and VCORE settle");
        }

        private static async Task Capture()
        {
            int previous = board.RunNumber;
            Check(board.RunProfiles(), "Acquire fresh voltage, current and thermal data.");
            await Wait(() => board.RunNumber > previous && !board.Testing, "workload capture");
            Log(board.Evidence.Replace("\n", " · "));
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
                    throw new TimeoutException($"{step}. Stage {board?.Stage}, evidence {board?.Evidence}, run {board?.RunNumber}, qualified {board?.Qualifies}, fresh {board?.Fresh}, manager {manager?.SatisfiedTargetCount}. " +
                        (board ? $"Input {board.Voltage(board.standby):F3} V, logic {board.Voltage(board.aux33):F3} V, AUX {board.Voltage(board.aux18):F3} V, U11 feed {board.Voltage(board.monitorSupply):F3} V, VCORE {board.Model.Voltage:F3} V, phase mask {board.Model.ConnectedPhases}, trip {board.Model.Trip}, heat {board.Model.Temperature:F1} °C." : "Board unavailable."));
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
