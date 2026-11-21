using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.DataPhoenix;
using SparkLevel116Process = ProjectSpark.Gameplay.DataPhoenix.SparkLevel116Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.DataPhoenix
{
    [InitializeOnLoad]
    internal static class Level116Acceptance
    {
        private const string Request = "Logs/Level116Acceptance.request";
        private const string Report = "Logs/Level116Acceptance.txt";
        private const string Session = "ProjectSpark.Level116.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel116Process board;

        static Level116Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/116/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE DATA PHOENIX / NVX24-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel116Builder.Prefab);
                Check(prefab && prefab.LevelDefinition && prefab.SceneBindings, "Serialized campaign prefab resolves.");
                var menu = AssetDatabase.LoadAssetAtPath<SceneAsset>(SparkLevel116Builder.Scene);
                Check(menu, "The campaign startup scene resolves.");
                var sourceScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (sourceScene.path != SparkLevel116Builder.Scene)
                {
                    Check(sourceScene.path == SparkLevel116Builder.PreviewScene && !sourceScene.isDirty &&
                        UnityEngine.SceneManagement.SceneManager.sceneCount == 1, "The saved Level 116 preview is ready to leave for campaign testing.");
                    EditorSceneManager.OpenScene(SparkLevel116Builder.Scene, OpenSceneMode.Single);
                }
                SessionState.SetString(Session + ".startScene", AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
                EditorSceneManager.playModeStartScene = null;
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
                string previous = SessionState.GetString(Session + ".startScene", "");
                EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(previous) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);
                SessionState.EraseString(Session + ".startScene");
                if (SessionState.GetBool(Session + ".checkpointCaptured", false))
                {
                    if (SessionState.GetBool(Session + ".hadCheckpoint", false))
                        PlayerPrefs.SetString(SparkLevel116Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel116Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel116Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel116Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel116Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel116Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index >= 0 && index == levels.arraySize - 1 && manager.LevelProgression &&
                manager.LevelProgression.CanPlayLevel(index) && manager.StartLevel(index),
                "MainMenu starts The Data Phoenix with existing campaign positions preserved.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.JumperPresent &&
                !board.Powered && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Starts empty at 0/6 with W289 open and bench power isolated.");
            Check(board.parts.Length == 208 && board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 576 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 24 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 100,
                "Serialized board contains 208 footprints, 288 route pairs, 24 layers and 100 native vias.");
            Check(board.jumper.from.name == "U31.TRUSTED_STORAGE_READY_OUT" &&
                board.jumper.to.name == "U30.TRUSTED_STORAGE_READY_IN",
                "W289 connects the fictional U31 storage-ready output to independent U30.");
            var bounds = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(bounds.size.x - 10.13f) < .10f && Mathf.Abs(bounds.size.z - 7.137f) < .09f &&
                Mathf.Abs(bounds.center.x - center.x) < .04f && Mathf.Abs(bounds.center.z - center.z) < .04f,
                $"Board art fits {bounds.size.x:F2} × {bounds.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            Check(board.fixture.Find("NVX-24 placement legend").GetComponent<MeshFilter>().sharedMesh.vertexCount > 2000,
                "The original footprints have visible placement markings.");
            await Screenshot("Level116Opening");
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
            await Wait(() => board.Installed == 1 && PlayerPrefs.HasKey(SparkLevel116Process.CheckpointKey), "saved first part");
            var firstBoard = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Campaign reloads the board for checkpoint verification.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) && board != firstBoard &&
                board.GetComponentInChildren<Canvas>(), "reloaded board");
            Click("Resume");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && !board.Powered && !board.Qualifies,
                "Checkpoint restores the real assembly contact with power isolated.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                if (!board.FitPart() ||
                    ((i < 8 || board.parts[i].label.StartsWith("NAND", StringComparison.Ordinal) || board.parts[i].label.StartsWith("J", StringComparison.Ordinal)) &&
                    !board.InspectContacts()) || !board.Solder())
                    throw new InvalidOperationException("Assembly failed at " + board.parts[i].label);
                if (i % 16 == 0) await Task.Yield();
            }
            await Stage(1);
            Check(board.Installed == 208 && !board.JumperPresent, "All original placements and BGA contacts complete.");
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
            await Screenshot("Level116Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                board.SelectRoute(i);
                if (board.routes[i].barrel && !board.PlateVia() || !board.ConnectPads())
                    throw new InvalidOperationException("Native connection failed at " + board.routes[i].id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 288 && board.Plated == 100 && !board.TogglePower(),
                "All copper still requires the independent W289 connection.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "W289 connects in the authoritative circuit.");
            await Stage(2);
            Check(board.witnesses.All(pin => board.Voltage(pin) > 4.7f) && !board.InitializeNand(),
                "All continuity groups solve and unpowered commissioning is blocked.");
            Click("Bench power");
            Check(board.RailsReady && !board.OpenQueue() && !board.ProtectedWrite(),
                "Solved rails alone cannot authorize the queue or a protected host write.");
            BringUp(8);
            await Stage(3);
            Check(board.HoldSeconds > .22f, "Commissioned reserve has enough loaded hold-up to complete a write.");
            await RunTest();
            await Stage(4);
            Check(board.Qualifies && board.VerifiedBits == 1024 && board.CorrectedPages == 8 && board.RejectedDoubleBit,
                "ECC corrected all eight single-bit samples, rejected the double-bit error and verified 1024 bits.");
            Check(board.CutPrimaryVolts < .15f && board.CutAcknowledgements == 1 &&
                board.EarlierCheckpointRecovered && board.RecoveredPages == 32,
                "Primary feed opened. The early candidate was discarded and the acknowledged reserve commit recovered.");
            Click("Channel map");
            await Screenshot("Level116Durability");

            int previousAcks = board.Acknowledgements;
            await Task.Delay(700);
            var qualifiedBoard = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Reload the qualified layout for durable checkpoint verification.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board != qualifiedBoard && board.GetComponentInChildren<Canvas>(), "storage checkpoint reload");
            Click("Resume");
            await Stage(4);
            Check(board.Acknowledgements == previousAcks && board.RecoveredPages == 32 && !board.Qualifies &&
                board.Inhibited && !board.Powered, "Checkpoint restores flash and contacts with authorization cleared.");
            Check(board.TogglePower(), "Enable bench after restore.");
            BringUp(8);
            await RunTest();
            Check(board.Qualifies, "A fresh replay qualifies the restored board.");

            for (int fault = 0; fault < 3; fault++)
            {
                int prior = board.Epoch;
                Check(board.BeginFaultTrial() && board.Epoch > prior && !board.Qualifies && !board.LocateFault(),
                    $"Fault {fault + 1} invalidates the previous storage evidence.");
                BringUp(fault == 0 ? 7 : 8);
                await RunTest();
                Check(!board.Qualifies && board.LocateFault() && !board.Repair(),
                    $"Fault {fault + 1} is localized from a finite failed run and blocks energized repair.");
                Check(fault switch
                {
                    0 => board.Failure == SparkLevel116Process.StorageFailure.ChannelTimeout &&
                        board.CommandTimeouts == 4 && board.Voltage(board.channelFeeds[3]) < .15f,
                    1 => board.Failure == SparkLevel116Process.StorageFailure.HoldUp && board.HoldSeconds is > .07f and < .188f &&
                        board.CutAcknowledgements == 0 && board.RecoveredPages == 32,
                    _ => board.Failure == SparkLevel116Process.StorageFailure.Replay &&
                        board.CutAcknowledgements == 1 && board.RetiredMetadataBlocks == 0
                }, $"Fault {fault + 1} produced its distinct measured outcome.");
                await Screenshot("Level116Fault" + (fault + 1));
                Check(board.TogglePower() && board.Voltage(board.nandRail) < .15f, "Isolate real NAND power before rework.");
                if (fault == 0)
                {
                    Check(!board.Repair() && board.InspectChannelVia(), "NAND4 repair requires inspecting its original via span.");
                    var via = board.routes[board.channelViaIndex];
                    board.SetLayer(via.exitLayer);
                    Check(!board.Repair(), "The incorrect via layer is rejected.");
                    board.SetLayer(via.layer);
                    Check(board.Repair(), "The isolated CE via contact is repaired.");
                }
                else if (fault == 1)
                {
                    Check(!board.Repair() && board.SelectSpare() && board.Repair(),
                        "Only the correctly rated replacement restores PLP3.");
                }
                else
                {
                    board.SelectFootprint(board.capacitorPart);
                    Check(!board.Repair(), "Wrong physical target cannot remap NAND8.");
                    board.SelectFootprint(board.metadataPart);
                    Check(board.Repair() && board.RetiredMetadataBlocks == 1 && board.RecoveredPages == 32,
                        "Metadata remap retires one failed block and reconstructs the map from acknowledged physical pages.");
                }
                board.BoardView();
                Check(board.TogglePower(), "Re-enable the repaired bench.");
                BringUp(8);
                await RunTest();
                Check(board.Repaired == fault + 1 && board.Qualifies && board.CutAcknowledgements == 1,
                    $"Repair {fault + 1} passes fresh ECC, reserve and replay verification.");
            }
            await Stage(5);
            await Wait(() => board.Fresh && !board.Inhibited, "active write permit");
            int oldEpoch = board.Epoch;
            int before = board.Acknowledgements;
            Check(board.ProtectedWrite() && board.ProtectedBusy, "Start an authorized host write with a durable commit still pending.");
            Check(board.OpenW289() && !board.JumperPresent && board.Inhibited && board.Powered &&
                board.Voltage(board.monitorSupply) > 3f && board.Voltage(board.protectedFeed) < .15f &&
                manager.SatisfiedTargetCount == 5 && board.AckSafeAtInterruption &&
                !board.ProtectedBusy && before == board.Acknowledgements && !board.ProtectedWrite(),
                "W289 opens during live work. Pending data receives no completion and previously acknowledged data survives.");
            await Screenshot("Level116W289Open");
            Check(!board.ResetSafety() && board.RestoreW289() && !board.ResetSafety() && !board.ProtectedWrite(),
                "Reconnection alone cannot restore aged write authorization.");
            BringUp(8);
            await RunTest();
            await Wait(() => board.Fresh, "measured new W289 transitions");
            Check(board.Epoch > oldEpoch && board.Inhibited && board.ResetSafety(),
                "U30 reset requires current ECC, reserve, replay and new heartbeat.");
            await Stage(6);
            Check(manager.IsCompleted && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f,
                "The campaign reaches 6/6 only after the protected-write interlock is independently released.");
            int finalAck = board.Acknowledgements;
            Check(board.ProtectedWrite(), "The recovered supervisor accepts a new protected host write.");
            await Wait(() => !board.ProtectedBusy && board.Acknowledgements == finalAck + 1, "protected write completed durably");
            board.BoardView();
            await Screenshot("Level116Complete");
            Click("Layer view");
            for (int layer = 1; layer <= 24; layer++)
            {
                board.SetLayer(layer);
                Check(board.routes.Count(route => route.copper.enabled) > 0 &&
                    board.routes.All(route => route.copper.enabled == (route.layer == layer)) &&
                    board.Routed == 288 && board.Plated == 100,
                    $"Layer {layer:00} isolates its native overlay without changing the solved graph.");
            }
            await Screenshot("Level116Layers");
            Click("Angle view");
            Check(Vector3.Angle(Camera.main.transform.forward, Vector3.down) > 25f &&
                board.routes.All(route => !route.copper.enabled) && board.heatSpreader.activeSelf,
                "The angled inspection frames all installed originals and the controller heatsink.");
            await Screenshot("Level116Inspection");
        }

        private static void BringUp(int channels)
        {
            Check(board.InitializeNand() && board.ReadyChannels == channels, $"Initialize {channels}/8 native NAND channel witnesses.");
            Check(board.ProbeReserve() && board.OpenQueue(), "Measure loaded PLP and open the bounded command queue.");
        }

        private static async Task RunTest()
        {
            Check(board.RunStorage(), "Start fresh finite data, ECC and power-loss testing.");
            await Wait(() => !board.Testing, "storage test result");
            Log(board.Evidence);
        }


        private static async Task Screenshot(string name)
        {
            await Task.Delay(140);
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
                    string rails = $"Primary {board.Voltage(board.standby):F3}, DIAG {board.Voltage(board.aux):F3}, CTRL {board.Voltage(board.controllerRail):F3}, NAND {board.Voltage(board.nandRail):F3}";
                    throw new TimeoutException($"{step}. Stage {board.Stage}, epoch {board.Epoch}, failure {board.Failure}. {rails}. {board.Evidence}");
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
