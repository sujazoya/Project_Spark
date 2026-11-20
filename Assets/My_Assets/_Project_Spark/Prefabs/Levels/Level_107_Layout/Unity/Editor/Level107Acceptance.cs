using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    [InitializeOnLoad]
    internal static class Level107Acceptance
    {
        private const string Request = "Logs/Level107Acceptance.request";
        private const string Report = "Logs/Level107Acceptance.txt";
        private const string Session = "ProjectSpark.Level107.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel107Process board;

        static Level107Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/107/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "PHOENIX PROTOCOL / PFX15-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel107Builder.Prefab);
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
                Log("Returned to Edit Mode.");
            }
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            var background = Application.runInBackground;
            var filter = Debug.unityLogger.filterLogType;
            Application.runInBackground = true;
            Debug.unityLogger.filterLogType = LogType.Error;
            bool hadCheckpoint = PlayerPrefs.HasKey(SparkLevel107Process.CheckpointKey);
            string prior = PlayerPrefs.GetString(SparkLevel107Process.CheckpointKey);
            PlayerPrefs.DeleteKey(SparkLevel107Process.CheckpointKey);
            try
            {
                await Run();
                Log("COMPLETE. Live LevelGamePlayManager evaluated all six objectives.");
            }
            catch (Exception error) { Log("FAIL " + error); }
            finally
            {
                if (hadCheckpoint) PlayerPrefs.SetString(SparkLevel107Process.CheckpointKey, prior);
                else PlayerPrefs.DeleteKey(SparkLevel107Process.CheckpointKey);
                PlayerPrefs.Save();
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel107Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index >= 0 && manager.StartLevel(index), "MainMenu loads Phoenix Protocol.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board)
                && board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.Powered && !board.JumperPresent &&
                board.parts.All(part => !part.model.activeSelf), "Playable board starts empty with W149 disconnected.");
            Check(board.primaryLamp && board.recoveryLamp && board.trustLamp && board.GetComponentInChildren<SparkLevel107TrustTrace>(),
                "Firmware status indicators and native receiver scope resolve.");
            Bounds pcb = board.modelSpace.Find("L107_BoardArtRoot").GetComponentsInChildren<Renderer>()
                .Select(renderer => renderer.bounds).Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(pcb.size.x - 10.13f) < .08f && Mathf.Abs(pcb.size.z - 7.51f) < .08f &&
                Mathf.Abs(pcb.center.x - center.x) < .03f && Mathf.Abs(pcb.center.z - center.z) < .03f,
                $"PFX-15 is {pcb.size.x:F2} × {pcb.size.z:F2} on the {size.x:F2} × {size.z:F2} tabletop.");
            board.SelectPart(1);
            Check(!board.FitPart(), "Wrong component is rejected.");
            board.SelectPart(0);
            Click("Fit part");
            Click("Solder");
            Check(board.Installed == 1, "Normal UI installs a real solder contact.");
            await Wait(() => PlayerPrefs.HasKey(SparkLevel107Process.CheckpointKey), "assembly checkpoint");
            var initial = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Restart Phoenix Protocol for checkpoint test.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board != initial && board.GetComponentInChildren<Canvas>(), "replacement layout");
            Click("Resume");
            Check(board.Installed == 1 && board.parts[0].solder.IsClosed && !board.Powered, "Resume reconstructs native contact and keeps power isolated.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                if (!board.FitPart() || !board.Solder()) throw new InvalidOperationException("Assembly part " + i);
                if (i % 20 == 0) await Task.Yield();
            }
            await Stage(1);
            var first = board.routes[0];
            Check(!board.ConnectPads() && board.PlateVia(), "Copper cannot bypass its unplated via.");
            board.PadView();
            var cam = Camera.main;
            var start = cam.WorldToScreenPoint(first.from.transform.position);
            var end = cam.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) && wire.TryEndWire(end),
                "Native Wire gesture reaches both highlighted pads.");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                var route = board.routes[i];
                board.SelectRoute(i);
                if (route.via && !board.PlateVia()) throw new InvalidOperationException(route.viaId);
                if (!board.ConnectPads()) throw new InvalidOperationException(route.id);
                if (i % 20 == 0) await Task.Yield();
            }
            board.SelectRoute(board.routes.Length);
            Check(board.Routed == 148 && board.Plated == 46 && !board.TogglePower() && board.ConnectPads(),
                "148 routes and 46 vias require the actual W149 edge.");
            await Stage(2);
            Check(board.TogglePower() && board.MeasurePrimary() && !board.PrimaryValid && !board.RewritePrimary(),
                "U6 responds with corrupted data; the programmer rejects unverified recovery.");
            await Rewrite();
            await Boot();
            await Stage(3);

            Check(board.ToggleWriteProtect() && board.BeginCutoverTrial(), "Begin an authorized primary update.");
            await Wait(() => board.PagesWritten >= 2 && board.Busy, "interruption window");
            Check(board.TogglePower() && board.InterruptionObserved && !board.PrimaryValid && board.RecoveryValid,
                "Power interruption leaves a partial U6 image and intact protected U7.");
            Check(board.TogglePower() && !board.StartBoot(), "A partial image cannot boot.");
            await Rewrite();
            await Boot();
            await Stage(4);

            Check(board.SeedFault() && board.MeasurePrimary() && Voltage(board.primaryBus) < .25f,
                "F1 removes actual modeled U6 chip-select voltage.");
            Check(!board.RepairContact() && board.TogglePower(), "Live repair of F1 is blocked.");
            board.SelectFootprint(board.recoveryIndex);
            Check(!board.RepairContact(), "Wrong U7 footprint cannot clear U6.");
            board.FindRepair();
            Check(board.RepairContact() && board.TogglePower() && board.MeasurePrimary() && Voltage(board.primaryBus) > 3f,
                "Correct U6 contact rework restores the solved SPI witness.");
            await Boot();
            Check(board.Repairs == 1, "Fresh POST verifies repaired F1.");

            Check(board.SeedFault() && board.MeasurePrimary() && Voltage(board.primaryBus) > 3f && !board.PrimaryValid,
                "F2 preserves SPI response while invalidating image integrity.");
            await Rewrite();
            await Boot();
            Check(board.Repairs == 2, "Authenticated U6 rewrite and POST verify F2.");

            Check(board.SeedFault() && !board.MeasureRequest() && Voltage(board.requestSense) < .25f && board.TogglePower(),
                "F3 produces no valid recovery request transition.");
            board.FindRepair();
            Check(board.RepairContact() && board.TogglePower() && board.MeasureRequest() && board.MeasurePrimary(),
                "Pull-up rework restores real high-low-high input readings.");
            await Boot();
            await Stage(5);
            Check(board.Repairs == 3, "All three evidence-led repairs verified.");
            await Wait(() => board.Fresh && !board.Inhibited && Voltage(board.protectedFeed) > 11f, "qualified workload");
            Click("Open W149");
            await Wait(() => board.Inhibited && Voltage(board.protectedFeed) < .30f, "native safety inhibition");
            Check(!manager.CircuitSystem.HasConnectionBetween(board.jumper.from, board.jumper.to) &&
                Voltage(board.standby) > 11f && !board.ResetSafety(), "Open W149 isolates only the protected load and rejects reset.");
            Click("Restore W149");
            Check(board.JumperPresent && board.Inhibited && !board.ResetSafety(), "Jumper reconnection alone leaves safety latched.");
            Check(board.MeasurePrimary(), "Fresh U6 readback acquired.");
            await Boot();
            await Wait(() => board.Fresh, "fresh pulse sequence");
            Check(!board.ResetSafety() && board.AuthenticateRecovery() && board.ResetSafety(),
                "Both current bank checks and POST are required before the explicit reset.");
            await Stage(6);
            Check(manager.IsCompleted && Voltage(board.protectedFeed) > 11f && board.Inhibited == false,
                "Native permissive and campaign completion agree.");
            Check(board.GetComponentsInChildren<SparkResistor>().All(resistor => !resistor.IsBurned),
                "Bench model loads stay intact through the whole exercise.");
            board.BoardView();
            string screenshot = Path.GetFullPath("Logs/Level107Complete.png");
            File.Delete(screenshot);
            ScreenCapture.CaptureScreenshot(screenshot);
            await Wait(() => File.Exists(screenshot), "completion screenshot");
        }

        private static async Task Rewrite()
        {
            Check(board.AuthenticateRecovery() && board.ToggleWriteProtect() && board.RewritePrimary(), "Verified U7 authorizes a primary-only rewrite.");
            await Wait(() => !board.Busy, "firmware page program");
            Check(board.PrimaryValid && board.RecoveryValid && board.ToggleWriteProtect(), "Primary digest verifies and U6 protection restored.");
        }

        private static async Task Boot()
        {
            Check(board.StartBoot(), "Start fresh POST.");
            await Wait(() => board.BootReady, "POST readiness");
        }

        private static float Voltage(SparkTerminal terminal)
        {
            var solve = manager.ElectricalSolver.LatestSolveResult;
            return solve != null && solve.TryGetVoltage(terminal, board.ground, out float value) ? value : 0;
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
                    throw new TimeoutException($"{step}. Stage {board?.Stage}, manager {manager?.SatisfiedTargetCount}, {manager?.LastEvaluationMessage}");
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
