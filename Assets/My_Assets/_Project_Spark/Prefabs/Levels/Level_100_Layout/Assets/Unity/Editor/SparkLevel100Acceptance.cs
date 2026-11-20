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
    internal static class Level100Acceptance
    {
        private const string Request = "Logs/Level100Acceptance.request";
        private const string Report = "Logs/Level100Acceptance.txt";
        private const string Session = "ProjectSpark.Level100.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel100Process board;

        static Level100Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/100/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "DIGITAL NEXUS / DNX8-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel100Builder.Prefab);
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
                        PlayerPrefs.SetString(SparkLevel100Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel100Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel100Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel100Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel100Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel100Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index >= 0 && manager.StartLevel(index), "MainMenu loads Digital Nexus.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board)
                && board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.Powered &&
                !board.JumperPresent && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Campaign starts at 0/6 with the board empty and W093 open.");
            Check(board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 184 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 4 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 20,
                "Serialized board has 184 route endpoints, four layers and 20 native via barrels.");
            Check(board.jumper.from.name == "U1.TRUSTED_COMM_HEALTH_OUT" &&
                board.jumper.to.name == "U9.TRUSTED_COMM_HEALTH_IN", "W093 maps to the intended U1 and U9 terminals.");
            var boardArt = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform
                .Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(boardArt.size.x - 10.13f) < .10f && Mathf.Abs(boardArt.size.z - 6.995f) < .09f &&
                Mathf.Abs(boardArt.center.x - center.x) < .04f && Mathf.Abs(boardArt.center.z - center.z) < .04f,
                $"Board art fits {boardArt.size.x:F2} × {boardArt.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            await Task.Delay(1100);
            await Screenshot("Level100Opening");
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
            await Wait(() => PlayerPrefs.HasKey(SparkLevel100Process.CheckpointKey), "saved assembly");
            var initial = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Reload Digital Nexus for checkpoint verification.");
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
            await Screenshot("Level100Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                var route = board.routes[i];
                board.SelectRoute(i);
                if (route.barrel && !board.PlateVia()) throw new InvalidOperationException(route.viaId);
                if (!board.ConnectPads()) throw new InvalidOperationException(route.id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 92 && board.Plated == 20 && !board.JumperPresent && !board.TogglePower(),
                "An assembled 92-route network still requires W093.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "W093 connected through the native circuit graph.");
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
            Check(board.Voltage(board.clockSense) > 2.7f && board.Voltage(board.resetSense) > 2.65f && board.Inhibited &&
                manager.SatisfiedTargetCount == 2, "Boot qualifies Y1 and reset without enabling the protected output or awarding a bus objective.");
            Check(board.RunDiagnostics(), "Start I²C and SPI commissioning.");
            await Stage(3);
            Check(board.DiagnosticsPassed == 2 && board.Traffic.Contains("I²C ACK 0x48 / 0x50 / 0x68") &&
                board.Traffic.Contains("SPI CS0 RAM D8 / D6 · CS1 ADC 96"),
                "Three I²C devices responded and two SPI targets completed separate exchanges.");
            await DiagnosticPass();
            await Stage(4);
            Check(board.Traffic.Contains("UART 115200 8N1 · TX A5 / RX 3C"), "UART verifies both decoded directions.");

            for (int fault = 0; fault < 3; fault++)
            {
                int oldEpoch = board.Epoch;
                Check(board.BeginFaultTrial() && board.RunDiagnostics(), "Capture newly injected fault " + (fault + 1) + ".");
                await Wait(() => board.FaultWitnessed, "measured communication fault");
                Check(board.Running && board.Epoch == oldEpoch, "Fault leaves the MCU running.");
                if (fault == 0)
                {
                    Check(board.I2cRise >= .44f && board.Voltage(board.ports[0].receiver) < 2.6f &&
                        board.Voltage(board.ports[1].receiver) > 2.6f, "R1 changes native SDA rise while SCL stays qualified.");
                }
                else if (fault == 1)
                {
                    Check(board.routes[board.spiRoute].viaId == "V11" && board.Plated == 19 &&
                        !board.routes[board.spiRoute].barrel.IsClosed && board.Voltage(board.ports[2].sender) > 2.6f &&
                        board.Voltage(board.ports[2].receiver) < .30f, "V11 opens only the real SPI clock path.");
                }
                else
                {
                    Check(board.UartBaud == 9600 && board.Traffic.Contains("UART frame error"), "Receiver sampling detects the 9600 baud mismatch.");
                }
                Log(board.Evidence);
                board.SelectFootprint(0);
                board.SelectRoute(0);
                Check(!board.Repair() && board.LocateFault(), "The failing transaction identifies the correct target.");
                if (fault < 2)
                {
                    Check(!board.Repair() && board.TogglePower() && board.Repair(), "Physical repair requires isolated bench power.");
                    await PowerUp();
                }
                else Check(board.Repair() && board.UartBaud == 115200 && board.Powered,
                    "U8 is repaired through live 115200 8N1 configuration.");
                await BootOnce();
                await DiagnosticPass();
                Check(board.Repaired == fault + 1 && board.Epoch > oldEpoch && board.I2cRise < .44f,
                    "Each repair requires a fresh boot and successful three-bus qualification.");
            }
            await Stage(5);

            await Wait(() => board.Fresh && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f, "energized protected output");
            int protectedEpoch = board.Epoch;
            int runningTimer = board.TimerTicks;
            Click("Open W093");
            await Wait(() => board.Inhibited && board.Voltage(board.protectedFeed) < .25f && board.TimerTicks > runningTimer,
                "protected output inhibited and application timer still advancing");
            Check(!manager.CircuitSystem.HasConnectionBetween(board.jumper.from, board.jumper.to) &&
                board.Running && board.Epoch == protectedEpoch && board.Voltage(board.vdd) > 3.05f &&
                !board.ResetSafety() && manager.SatisfiedTargetCount == 5,
                "W093 removes actual continuity and protected output while U1 stays powered and running.");
            Click("Restore W093");
            Check(board.JumperPresent && board.Inhibited && !board.ResetSafety(), "Reconnecting W093 alone cannot release the load.");
            await Wait(() => board.Fresh, "new W093 edge cadence");
            Check(board.ResetSafety() && board.Inhibited, "Explicit reset requires a newer boot and diagnostic epoch before enabling the output.");
            await BootOnce();
            Check(board.Epoch > protectedEpoch && board.Inhibited, "Fresh boot alone cannot authorize the protected output.");
            await DiagnosticPass();
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f,
                "Fresh protected output and the campaign manager both confirm all six objectives.");
            board.BoardView();
            await Task.Delay(140);
            await Screenshot("Level100Complete");
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
            Check(board.RunDiagnostics(), "Start multi-bus diagnostics.");
            await Wait(() => board.DiagnosticsPassed == 3 && !board.Diagnosing, "all three bus exchanges");
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
                        (board ? $"3.3 V rail {board.Voltage(board.vdd):F3} V, Y1 {board.Voltage(board.clockSense):F3} V, reset {board.Voltage(board.resetSense):F3} V, SPI SCK {board.Voltage(board.ports[2].receiver):F3} V." : "Board unavailable."));
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
