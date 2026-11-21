using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.PacketCitadel;
using SparkLevel117Process = ProjectSpark.Gameplay.PacketCitadel.SparkLevel117Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.PacketCitadel
{
    [InitializeOnLoad]
    internal static class Level117Acceptance
    {
        private const string Request = "Logs/Level117Acceptance.request";
        private const string Report = "Logs/Level117Acceptance.txt";
        private const string Session = "ProjectSpark.Level117.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel117Process board;

        static Level117Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/117/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE PACKET CITADEL / NFX25-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel117Builder.Prefab);
                Check(prefab && prefab.LevelDefinition && prefab.SceneBindings, "Serialized campaign prefab resolves.");
                var menu = AssetDatabase.LoadAssetAtPath<SceneAsset>(SparkLevel117Builder.Scene);
                Check(menu, "The campaign startup scene resolves.");
                var sourceScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (sourceScene.path != SparkLevel117Builder.Scene)
                {
                    Check(sourceScene.path == SparkLevel117Builder.PreviewScene && !sourceScene.isDirty &&
                        UnityEngine.SceneManagement.SceneManager.sceneCount == 1, "The saved Level 117 preview is ready to leave for campaign testing.");
                    EditorSceneManager.OpenScene(SparkLevel117Builder.Scene, OpenSceneMode.Single);
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
                        PlayerPrefs.SetString(SparkLevel117Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel117Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel117Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel117Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel117Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel117Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index >= 0 && index == levels.arraySize - 1 && manager.LevelProgression &&
                manager.LevelProgression.CanPlayLevel(index) && manager.StartLevel(index),
                "MainMenu starts The Packet Citadel with existing campaign positions preserved.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.JumperPresent &&
                !board.Powered && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Starts empty at 0/6 with W305 open and bench power isolated.");
            Check(board.parts.Length == 220 && board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 608 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 26 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 108,
                "Serialized board contains 220 footprints, 304 route pairs, 26 layers and 108 native vias.");
            Check(board.jumper.from.name == "U35.QUALIFIED_NETWORK_READY_OUT" &&
                board.jumper.to.name == "U36.QUALIFIED_NETWORK_READY_IN",
                "W305 connects the fictional U35 network-ready output to independent U36.");
            var bounds = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(bounds.size.x - 10.13f) < .10f && Mathf.Abs(bounds.size.z - 7.047f) < .09f &&
                Mathf.Abs(bounds.center.x - center.x) < .04f && Mathf.Abs(bounds.center.z - center.z) < .04f,
                $"Board art fits {bounds.size.x:F2} × {bounds.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            Check(board.fixture.Find("NFX-25 placement legend").GetComponent<MeshFilter>().sharedMesh.vertexCount > 2000,
                "The original footprints have visible placement markings.");
            var sourceDepths = new[] { .112f, .042f, -.040f, -.111f };
            for (int port = 0; port < sourceDepths.Length; port++)
            {
                var cage = board.opticalArt.transform.Find($"L117_SFP_Cage{port + 1}");
                var lens = board.opticalArt.transform.Find($"L117_OpticalPort{port + 1}");
                var toBoard = board.fixture.worldToLocalMatrix;
                Bounds cageBounds = ProjectSpark.EditorTools.SparkEquipmentWorkbenchFit.MeasureArt(cage, toBoard);
                Bounds lensBounds = ProjectSpark.EditorTools.SparkEquipmentWorkbenchFit.MeasureArt(lens, toBoard);
                Check(Mathf.Abs(cageBounds.center.x - .193f) < .001f &&
                    Mathf.Abs(cageBounds.center.z - sourceDepths[port]) < .001f &&
                    Mathf.Abs(lensBounds.center.x - .2187f) < .001f &&
                    Mathf.Abs(lensBounds.center.z - sourceDepths[port]) < .001f,
                    $"Original P{port + 1} cage and lens are seated at the authored right-edge coordinates.");
            }
            await Screenshot("Level117Opening");
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
            await Wait(() => board.Installed == 1 && PlayerPrefs.HasKey(SparkLevel117Process.CheckpointKey), "saved first part");
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
                    ((i < 8 || board.parts[i].label.StartsWith("Y", StringComparison.Ordinal) || board.parts[i].label.StartsWith("J", StringComparison.Ordinal)) &&
                    !board.InspectContacts()) || !board.Solder())
                    throw new InvalidOperationException("Assembly failed at " + board.parts[i].label);
                if (i % 16 == 0) await Task.Yield();
            }
            await Stage(1);
            Check(board.Installed == 220 && !board.JumperPresent, "All original placements and BGA contacts complete.");
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
            await Screenshot("Level117Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                board.SelectRoute(i);
                if (board.routes[i].barrel && !board.PlateVia() || !board.ConnectPads())
                    throw new InvalidOperationException("Native connection failed at " + board.routes[i].id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 304 && board.Plated == 108 && !board.TogglePower(),
                "All copper still requires the independent W305 connection.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "W305 connects in the authoritative circuit.");
            await Stage(2);
            Check(board.witnesses.All(pin => board.Voltage(pin) > 4.7f) && !board.TrainLinks(),
                "All continuity groups solve and unpowered commissioning is blocked.");
            Click("Bench power");
            Check(board.RailsReady && !board.RunTraffic() && !board.StartProtectedBurst(),
                "Solved rails alone cannot qualify traffic or authorize a protected workload.");
            Check(board.TrainLinks() && board.TrainedLinks == 4 && board.ProbeOptics(),
                "Train four native optical paths and measure their individual transmit and receive levels.");
            await Stage(3);
            await MeasureClock(true);
            await RunPackets();
            await Stage(4);
            Check(board.Qualifies && board.Sent == 512 && board.Received == 512 && board.Duplicates == 0 &&
                board.Drops == 0 && board.CrcErrors == 0 && Enumerable.Range(0, 4).All(port => board.DeliveredAt(port) == 128),
                "All four ports delivered 128 frames with no observed integrity errors.");
            Check(board.PriorityMean < board.NormalMean && board.PeakQueue >= 2 && board.P95 < 6.4 &&
                Math.Abs(board.ClockOffsetUs) < 1.2 && board.PathDelayUs > 4.0,
                "Real queue service demonstrates class priority, bounded latency and independent four-timestamp lock.");
            await Screenshot("Level117PacketProof");

            await Task.Delay(700);
            var trainedBoard = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Reload the trained layout for safe checkpoint verification.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board != trainedBoard && board.GetComponentInChildren<Canvas>(), "network checkpoint reload");
            Click("Resume");
            await Stage(4);
            Check(board.Routed == 304 && board.Plated == 108 && !board.Qualifies && !board.ClockLocked && !board.Powered &&
                board.Inhibited, "Checkpoint restores wiring and history with expired link, PTP and packet evidence.");
            Check(board.TogglePower(), "Enable the resumed bench.");
            await Qualify();
            Check(board.Qualifies, "Fresh optical and timing evidence qualifies the restored board.");

            for (int fault = 0; fault < 3; fault++)
            {
                int prior = board.Epoch;
                Check(board.BeginFaultTrial() && board.Epoch > prior && !board.Qualifies && !board.LocateFault(),
                    $"Fault {fault + 1} invalidates earlier evidence.");
                Check(board.TrainLinks() && board.TrainedLinks == (fault == 0 ? 3 : 4),
                    "Measure the faulted optical training result.");
                await MeasureClock(fault != 1);
                await RunPackets();
                Check(!board.Qualifies && board.LocateFault() && !board.Repair(),
                    $"Fault {fault + 1} can be localized from current measurements with hot service blocked.");
                Check(fault switch
                {
                    0 => board.Failure == SparkLevel117Process.NetworkFailure.OpticalBias &&
                        board.Voltage(board.biasFeeds[2]) < .15f && board.TransmitDbm(2) < -20f && board.Drops > 0,
                    1 => board.Failure == SparkLevel117Process.NetworkFailure.ClockSkew &&
                        Math.Abs(board.ClockOffsetUs) > 4 && board.CrcErrors == 0 && board.Received == 512,
                    _ => board.Failure == SparkLevel117Process.NetworkFailure.LaneContinuity &&
                        board.MinimumPort4Eye < .52f && board.LaneOpenSamples > 0 && board.CrcErrors > 0
                }, $"Fault {fault + 1} has its own native and measured failure signature.");
                await Screenshot("Level117Fault" + (fault + 1));
                Check(board.TogglePower() && board.Voltage(board.moduleRail) < .15f,
                    "Isolate the real module rail before rework.");
                if (fault < 2)
                {
                    board.SelectFootprint(board.lanePart);
                    Check(!board.Repair(), "An unrelated part cannot repair this fault.");
                    board.SelectFootprint(fault == 0 ? board.biasPart : board.clockPart);
                    Check(!board.Repair() && board.SelectSpare() && board.Repair(), "The measured component needs its matched spare.");
                }
                else
                {
                    Check(!board.Repair() && board.InspectLaneVia(), "P4 repair requires inspection of the diagnosed native span.");
                    var via = board.routes[board.laneViaIndex];
                    board.SetLayer(via.exitLayer);
                    Check(!board.Repair(), "The incorrect entry layer is rejected.");
                    board.SetLayer(via.layer);
                    Check(board.Repair(), "Repair the original isolated P4 span.");
                }
                board.BoardView();
                Check(board.TogglePower(), "Re-enable the repaired optical bench.");
                await Qualify();
                Check(board.Repaired == fault + 1 && board.Qualifies && board.CrcErrors == 0,
                    $"Repair {fault + 1} passes fresh optical, timing and packet verification.");
            }
            await Stage(5);
            await Wait(() => board.Fresh && !board.Inhibited, "active protected network permit");
            int oldEpoch = board.Epoch;
            Check(board.StartProtectedBurst(), "Start an authorized protected traffic burst.");
            await Wait(() => board.ProtectedBusy && board.ProtectedDelivered > 0 && board.ProtectedDelivered < 512, "protected packets in flight");
            Check(board.OpenW305() && !board.JumperPresent && board.Inhibited && board.Powered &&
                board.Voltage(board.monitorSupply) > 3f && board.Voltage(board.moduleRail) > 3f &&
                board.Voltage(board.protectedFeed) < .15f && manager.SatisfiedTargetCount == 5 &&
                !board.ProtectedBusy && board.InterruptedFrames > 0 && !board.StartProtectedBurst(),
                "W305 stops the in-flight workload and retains real diagnostic and optical supplies.");
            await Screenshot("Level117W305Open");
            Check(!board.ResetSafety() && board.RestoreW305() && !board.ResetSafety() && !board.StartProtectedBurst(),
                "Physical reconnection alone cannot restore a stale authorization.");
            await Qualify();
            await Wait(() => board.Fresh, "fresh W305 voltage transitions");
            Check(board.Epoch > oldEpoch && board.Inhibited && board.ResetSafety(),
                "U36 reset requires the current independent link, PTP and packet evidence.");
            await Stage(6);
            Check(manager.IsCompleted && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f,
                "The campaign reaches 6/6 only after the native interlock releases protected traffic.");
            Check(board.StartProtectedBurst(), "The recovered U36 starts a new protected burst.");
            await Wait(() => !board.ProtectedBusy && board.ProtectedDelivered == 512, "protected burst completed");
            board.BoardView();
            await Screenshot("Level117Complete");
            Click("Layer view");
            for (int layer = 1; layer <= 26; layer++)
            {
                board.SetLayer(layer);
                Check(board.routes.Count(route => route.copper.enabled) > 0 &&
                    board.routes.All(route => route.copper.enabled == (route.layer == layer)) &&
                    board.Routed == 304 && board.Plated == 108,
                    $"Layer {layer:00} isolates its native overlay without changing the solved graph.");
            }
            await Screenshot("Level117Layers");
            Click("Angle view");
            Check(Vector3.Angle(Camera.main.transform.forward, Vector3.down) > 25f &&
                board.routes.All(route => !route.copper.enabled) && board.opticalArt.activeSelf,
                "The angled inspection shows the four original SFP cages and lenses.");
            await Screenshot("Level117Inspection");
        }

        private static async Task Qualify()
        {
            Check(board.TrainLinks() && board.TrainedLinks == 4, "Train all four original ports.");
            await MeasureClock(true);
            await RunPackets();
        }

        private static async Task MeasureClock(bool locked)
        {
            Check(board.SyncPtp(), "Start a fresh independent PTP timestamp capture.");
            await Wait(() => !board.Syncing, "PTP exchange result");
            Check(board.ClockLocked == locked, locked ? "PTP locked from measured timestamp exchanges." : "Y2 drift prevents disciplined lock.");
            Log(board.Evidence);
        }

        private static async Task RunPackets()
        {
            Check(board.RunTraffic(), "Start fresh priority, CRC and latency packet tests.");
            await Wait(() => !board.Testing, "packet acceptance measurement");
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
                    string rails = $"Primary {board.Voltage(board.standby):F3}, DIAG {board.Voltage(board.aux):F3}, CORE {board.Voltage(board.coreRail):F3}, MODULE {board.Voltage(board.moduleRail):F3}";
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
