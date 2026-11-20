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
    internal static class Level99Acceptance
    {
        private const string Request = "Logs/Level99Acceptance.request";
        private const string Report = "Logs/Level99Acceptance.txt";
        private const string Session = "ProjectSpark.Level99.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel99Process board;

        static Level99Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/99/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "SILICON BRAIN / SBC7-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel99Builder.Prefab);
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
                        PlayerPrefs.SetString(SparkLevel99Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel99Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel99Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel99Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel99Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel99Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index >= 0 && manager.StartLevel(index), "MainMenu loads Silicon Brain.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board)
                && board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.Powered &&
                !board.JumperPresent && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Campaign starts at 0/6 with the board empty and W083 open.");
            Check(board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 164 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 4 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 18,
                "Serialized board has 164 route endpoints, four layers and 18 native via barrels.");
            Check(board.jumper.from.name == "U1.APPLICATION_HEARTBEAT_OUT" &&
                board.jumper.to.name == "U8.TRUSTED_HEARTBEAT_IN", "W083 maps to the intended U1 and U8 terminals.");
            var boardArt = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform
                .Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(boardArt.size.x - 10.40f) < .10f && Mathf.Abs(boardArt.size.z - 6.93f) < .09f &&
                Mathf.Abs(boardArt.center.x - center.x) < .04f && Mathf.Abs(boardArt.center.z - center.z) < .04f,
                $"Board art fits {boardArt.size.x:F2} × {boardArt.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            await Task.Delay(1100);
            await Screenshot("Level99Opening");
            board.SelectPart(1);
            Check(!board.FitPart(), "Wrong component is rejected.");
            board.SelectPart(0);
            board.RotatePart();
            Check(!board.FitPart() && !board.Solder(), "Wrong orientation and incomplete placement cannot be soldered.");
            board.SelectPart(0);
            Click("Fit part");
            Click("Solder");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && board.parts[0].model.activeSelf,
                "Assembly controls close a real contact.");
            await Wait(() => PlayerPrefs.HasKey(SparkLevel99Process.CheckpointKey), "saved assembly");
            var initial = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Reload Silicon Brain for checkpoint verification.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board != initial && board.GetComponentInChildren<Canvas>(), "reloaded board");
            Click("Resume");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && !board.Powered && !board.ProgramVerified,
                "Resume restores the solder contact with power isolated and blank firmware.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                if (!board.FitPart() || !board.Solder()) throw new InvalidOperationException("Assembly part " + i);
                if (i % 14 == 0) await Task.Yield();
            }
            await Stage(1);
            var first = board.routes[0];
            board.SetLayer(first.layer % 4 + 1);
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
            await Screenshot("Level99Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                var route = board.routes[i];
                board.SelectRoute(i);
                if (route.barrel && !board.PlateVia()) throw new InvalidOperationException(route.viaId);
                if (!board.ConnectPads()) throw new InvalidOperationException(route.id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 82 && board.Plated == 18 && !board.JumperPresent && !board.TogglePower(),
                "An assembled 82-route network still requires W083.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "W083 connected through the native circuit graph.");
            await Stage(2);
            Check(board.witnesses.All(pin => board.Voltage(pin) > 4.7f), "All six training continuity branches conduct.");
            await PowerUp();
            Check(!board.StartBoot(), "Blank internal firmware cannot boot.");
            board.CycleImage();
            Check(!board.Program(), "SWD rejects an incompatible image.");
            board.CycleImage();
            Check(!board.Program(), "SWD rejects a corrupt digest.");
            board.CycleImage();
            Check(board.Program(), "SWD accepts the approved curated image.");
            await Wait(() => !board.Programming && board.ProgramVerified, "firmware write and readback");
            Check(board.Evidence.StartsWith("DIAGNOSTIC 1.0"), "SWD readback names the verified image and digest.");
            await BootOnce();
            await Stage(3);
            Check(board.Voltage(board.clockSense) > 2.7f && board.Voltage(board.resetSense) > 2.65f,
                "Native clock envelope and reset are solved before the virtual 48 MHz application starts.");
            await DiagnosticPass();
            await Stage(4);

            for (int fault = 0; fault < 3; fault++)
            {
                int oldEpoch = board.Epoch;
                Check(board.BeginFaultTrial(), "Begin circuit fault " + (fault + 1) + ".");
                if (fault == 1)
                {
                    Check(board.Running && board.routes[board.spiRoute].viaId == "V11" &&
                        !board.routes[board.spiRoute].barrel.IsClosed && board.Plated == 17 && board.RunDiagnostics(),
                        "V11 opens actual external data while the same U1 epoch continues.");
                    await Wait(() => board.FaultWitnessed, "external SPI fault witness");
                    Check(board.Running && board.Epoch == oldEpoch && board.Voltage(board.spiSense) < .25f,
                        "U2 SPI fails independently of the booted internal image.");
                }
                else
                {
                    Check(board.StartBoot(), "Run the faulted boot.");
                    await Wait(() => board.FaultWitnessed, "clock or reset fault witness");
                    Check(!board.Running && board.Epoch == oldEpoch, "Clock or reset fault blocks completion of a new boot.");
                }
                Log(board.Evidence);
                Check(!board.Repair() && board.TogglePower(), "Live repair is blocked and the bench can be safely isolated.");
                board.SelectFootprint(0);
                board.SelectRoute(0);
                Check(!board.Repair() && board.LocateFault() && board.Repair(), "Only the identified component or barrel can be repaired.");
                await PowerUp();
                await BootOnce();
                await DiagnosticPass();
                Check(board.Repaired == fault + 1 && board.Epoch > oldEpoch, "Fresh boot and all five diagnostics qualify the repair.");
            }
            await Stage(5);

            await Wait(() => board.Fresh && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f, "energized protected output");
            int protectedEpoch = board.Epoch;
            int runningTimer = board.TimerTicks;
            Click("Open W083");
            await Wait(() => board.Inhibited && board.Voltage(board.protectedFeed) < .25f && board.TimerTicks > runningTimer,
                "protected output inhibited and application timer still advancing");
            Check(!manager.CircuitSystem.HasConnectionBetween(board.jumper.from, board.jumper.to) &&
                board.Running && board.Epoch == protectedEpoch && board.Voltage(board.vdd) > 3.05f &&
                !board.ResetSafety() && manager.SatisfiedTargetCount == 5,
                "W083 removes actual continuity and protected output while U1 stays powered and running.");
            Click("Restore W083");
            Check(board.JumperPresent && board.Inhibited && !board.ResetSafety(), "Reconnecting W083 alone cannot release the load.");
            await Wait(() => board.Fresh, "new W083 edge cadence");
            Check(board.ResetSafety() && board.Inhibited, "Explicit reset requires a newer boot and diagnostic epoch before enabling the output.");
            await BootOnce();
            Check(board.Epoch > protectedEpoch && board.Inhibited, "Fresh boot alone cannot authorize the protected output.");
            await DiagnosticPass();
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f,
                "Fresh protected output and the campaign manager both confirm all six objectives.");
            board.BoardView();
            await Screenshot("Level99Complete");
        }

        private static async Task PowerUp()
        {
            Check(board.TogglePower(), "Enable the isolated 5 V bench.");
            Check(!board.regulator.IsConducting, "Bench startup begins with the regulator gate open.");
            await Wait(() => board.regulator.IsConducting && board.Voltage(board.vdd) is > 3.05f and < 3.50f,
                "3.3 V regulator startup");
            await Task.Yield();
        }

        private static async Task BootOnce()
        {
            Check(board.StartBoot(), "Start an application boot.");
            await Wait(() => board.Running, "new application epoch");
        }

        private static async Task DiagnosticPass()
        {
            Check(board.RunDiagnostics(), "Start peripheral diagnostics.");
            await Wait(() => board.DiagnosticsPassed == 5 && !board.Diagnosing, "all peripheral transactions");
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
                    throw new TimeoutException($"{step}. Stage {board?.Stage}, phase {board?.Evidence}, boot epoch {board?.Epoch}, diagnostics {board?.DiagnosticsPassed}, fresh {board?.Fresh}, manager {manager?.SatisfiedTargetCount}, {manager?.LastEvaluationMessage}. " +
                        (board ? $"3.3 V rail {board.Voltage(board.vdd):F3} V, Y1 {board.Voltage(board.clockSense):F3} V, reset {board.Voltage(board.resetSense):F3} V, SPI {board.Voltage(board.spiSense):F3} V." : "Board unavailable."));
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
