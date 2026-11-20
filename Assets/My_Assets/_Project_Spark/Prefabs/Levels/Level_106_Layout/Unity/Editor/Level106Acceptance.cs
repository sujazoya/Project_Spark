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
    internal static class Level106Acceptance
    {
        private const string Request = "Logs/Level106Acceptance.request";
        private const string Report = "Logs/Level106Acceptance.txt";
        private const string Session = "ProjectSpark.Level106.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel106Process board;

        static Level106Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/106/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "FIRST BOOT / SFX14-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel106Builder.Prefab);
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
                        PlayerPrefs.SetString(SparkLevel106Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel106Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel106Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel106Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel106Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel106Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index >= 0 && manager.StartLevel(index), "MainMenu loads First Boot.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board)
                && board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.Powered &&
                !board.JumperPresent && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Empty board starts at 0/6 with W141 open and power isolated.");
            Check(board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 280 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 10 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 42,
                "280 unique service contacts, ten layers and 42 native via barrels are serialized.");
            var pcb = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform
                .Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(pcb.size.x - 10.13f) < .08f && Mathf.Abs(pcb.size.z - 7.51f) < .08f &&
                Mathf.Abs(pcb.center.x - center.x) < .03f && Mathf.Abs(pcb.center.z - center.z) < .03f,
                $"SFX-14 fits {pcb.size.x:F2} × {pcb.size.z:F2} on the {size.x:F2} × {size.z:F2} tabletop.");
            await Screenshot("Level106Opening");

            board.SelectPart(1);
            Check(!board.FitPart(), "Wrong component is rejected.");
            board.SelectPart(0);
            board.RotatePart();
            Check(!board.FitPart() && !board.Solder(), "Rotated or unseated placements cannot be soldered.");
            board.SelectPart(0);
            Click("Fit part");
            Click("Solder");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && board.parts[0].model.activeSelf,
                "Player controls close a real solder contact.");
            await Wait(() => PlayerPrefs.HasKey(SparkLevel106Process.CheckpointKey), "saved assembly");
            var initial = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Restart First Boot for checkpoint verification.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board != initial && board.GetComponentInChildren<Canvas>(), "reloaded workbench");
            Click("Resume");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && !board.Powered,
                "Resume restores the physical contact and leaves power off.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                if (!board.FitPart() || !board.Solder()) throw new InvalidOperationException("Assembly part " + i);
                if (i % 18 == 0) await Task.Yield();
            }
            await Stage(1);
            var cpu = board.parts[0].model.GetComponentsInChildren<Renderer>()
                .Select(renderer => renderer.bounds).Aggregate((result, next) => { result.Encapsulate(next); return result; });
            Check(cpu.size.x > 1.18f && cpu.size.z > 1.18f && cpu.size.y < .26f &&
                Mathf.Abs(cpu.center.x - pcb.center.x) < .07f,
                "The authored CPU package is seated flat on its source footprint.");
            var first = board.routes[0];
            board.SetLayer(first.layer % 10 + 1);
            Check(!board.PlateVia() && !board.ConnectPads(), "Wrong-layer copper and vias are rejected.");
            board.SelectRoute(0);
            Check(!board.ConnectPads() && board.PlateVia(), "Via must be plated before adding copper.");
            board.PadView();
            var camera = Camera.main;
            var start = camera.WorldToScreenPoint(first.from.transform.position);
            var end = camera.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(end), "The native Wire gesture connects the exact highlighted pair.");
            await Screenshot("Level106Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                var route = board.routes[i];
                board.SelectRoute(i);
                if (route.barrel && !board.PlateVia()) throw new InvalidOperationException(route.viaId);
                if (!board.ConnectPads()) throw new InvalidOperationException(route.id);
                if (i % 14 == 0) await Task.Yield();
            }
            Check(board.Routed == 140 && board.Plated == 42 && !board.JumperPresent &&
                !board.TogglePower() && !board.StartPost(), "Copper alone cannot substitute for W141.");
            board.SelectRoute(board.routes.Length - 1);
            Check(board.RemoveSelectedRoute() && board.Routed == 139 && board.ConnectPads(),
                "Removing a route retracts its native continuity.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "Connect U14 to U15 through the native W141 edge.");
            await Stage(2);
            Check(board.witnesses.All(pin => board.Voltage(pin) > 11.5f), "All seven independently routed branches conduct.");
            Check(board.TogglePower() && board.Voltage(board.standby) > 11.9f && board.Voltage(board.core) < .1f,
                "Standby is separate from the disabled CPU rail.");
            await Boot();
            await Stage(4);
            Check(board.TrainedLanes == 4 && Mathf.Abs(board.Voltage(board.core) - 1f) < .06f &&
                Mathf.Abs(board.Voltage(board.ddr) - 1.2f) < .07f && board.Voltage(board.linkSense) > 2.7f,
                "Power/reset, four DDR lanes, firmware, PCIe and eMMC passed the first POST.");

            for (int fault = 0; fault < 3; fault++)
            {
                Check(board.BeginFaultTrial(), "Seed fault trial F" + (fault + 1) + ".");
                await Wait(() => !board.Busy && board.FaultWitnessed, "fault evidence " + (fault + 1));
                Check(!board.BootPassed, board.PostCode);
                if (fault == 0) Check(board.PostCode.StartsWith("E1") && board.Voltage(board.resetSense) > 2.7f,
                    "Solver sees the premature reset edge.");
                if (fault == 1) Check(board.Voltage(board.resetSense) > 2.7f && board.Voltage(board.flashSense) < .25f,
                    "Solder bridge corrupts the SPI contact while CPU reset remains released.");
                if (fault == 2) Check(board.Voltage(board.storageSense) < .25f && board.TrainedLanes == 4 &&
                    board.Voltage(board.linkSense) > 2.7f, "The eMMC open blocks BDS after memory and PCIe pass.");
                Check(!board.Repair() && board.TogglePower(), "Live repair is rejected and the bench can be isolated.");
                board.SelectFootprint((board.repairParts[fault] + 1) % board.parts.Length);
                Check(!board.Repair() && board.LocateFault() && board.Repair(), "Only the diagnosed footprint can be repaired.");
                Check(!board.BootPassed && board.TogglePower(), "The repair requires new POST evidence.");
                await Boot();
                Check(board.Repaired == fault + 1, "Fresh POST verifies the repair.");
            }
            await Stage(5);
            await Wait(() => board.Fresh && !board.Inhibited && board.Voltage(board.protectedFeed) > 11f, "W141 heartbeat");
            Click("Open W141");
            await Wait(() => board.Inhibited && board.Voltage(board.protectedFeed) < .25f, "safe inhibition");
            Check(!manager.CircuitSystem.HasConnectionBetween(board.jumper.from, board.jumper.to) &&
                board.Voltage(board.standby) > 11f && !board.ResetSafety() && manager.SatisfiedTargetCount == 5,
                "Physical W141 loss isolates the protected rail while standby survives.");
            Click("Restore W141");
            Check(board.JumperPresent && board.Inhibited && !board.ResetSafety(),
                "Reconnection alone cannot release the workload.");
            await Boot();
            await Wait(() => board.Fresh, "fresh receiver pulses");
            Check(board.Inhibited && board.ResetSafety(), "Fresh POST and U14 pulses allow explicit U15 reset.");
            await Stage(6);
            Check(manager.IsCompleted && !board.Inhibited && board.Voltage(board.protectedFeed) > 11f &&
                board.GetComponentsInChildren<SparkResistor>().All(resistor => !resistor.IsBurned),
                "Native protected output and campaign completion agree.");
            board.BoardView();
            await Screenshot("Level106Complete");
        }

        private static async Task Boot()
        {
            int stageDuringPost = board.Stage;
            Check(board.StartPost(), "Start fresh POST.");
            if (stageDuringPost == 2) await Stage(3);
            await Wait(() => !board.Busy, "POST handoff");
            Check(board.BootPassed, board.PostCode);
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
