using System;
using System.IO;
using System.Globalization;
using UnityEngine.EventSystems;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.DeterministicGrid;
using SparkLevel118Process = ProjectSpark.Gameplay.DeterministicGrid.SparkLevel118Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.DeterministicGrid
{
    [InitializeOnLoad]
    internal static class Level118Acceptance
    {
        private const string Request = "Logs/Level118Acceptance.request";
        private const string Report = "Logs/Level118Acceptance.txt";
        private const string Session = "ProjectSpark.Level118.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel118Process board;

        static Level118Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/118/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE DETERMINISTIC GRID / TSX26-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel118Builder.Prefab);
                Check(prefab && prefab.LevelDefinition && prefab.SceneBindings, "Serialized campaign prefab resolves.");
                var menu = AssetDatabase.LoadAssetAtPath<SceneAsset>(SparkLevel118Builder.Scene);
                Check(menu, "The campaign startup scene resolves.");
                var sourceScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (sourceScene.path != SparkLevel118Builder.Scene)
                {
                    Check(sourceScene.path == SparkLevel118Builder.PreviewScene && !sourceScene.isDirty &&
                        UnityEngine.SceneManagement.SceneManager.sceneCount == 1, "The saved Level 118 preview is ready to leave for campaign testing.");
                    EditorSceneManager.OpenScene(SparkLevel118Builder.Scene, OpenSceneMode.Single);
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
                        PlayerPrefs.SetString(SparkLevel118Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel118Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel118Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel118Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel118Process.CheckpointKey);
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
            Check(manager && loader && wire, "MainMenu campaign manager, layout loader and native Wire tool resolve.");
            if (manager.LevelProgression) manager.LevelProgression.enabled = false;
            var data = new SerializedObject(manager);
            data.FindProperty("autoAdvanceOnCompletion").boolValue = false;
            data.FindProperty("autoEvaluate").boolValue = true;
            data.ApplyModifiedPropertiesWithoutUndo();
            var levels = data.FindProperty("levels");
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel118Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).Single();
            Check(index == levels.arraySize - 1 && index >= 0 && manager.LevelProgression &&
                manager.LevelProgression.CanPlayLevel(index) && manager.StartLevel(index),
                "MainMenu starts The Deterministic Grid at the appended campaign index.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.JumperPresent &&
                !board.Powered && board.Inhibited && board.parts.All(part => !part.model.activeSelf) &&
                manager.SatisfiedTargetCount == 0, "Starts empty at 0/6 with W321 open and protected output inhibited.");
            Check(board.parts.Length == 236 && board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 640 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 28 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 116,
                "236 footprints, 320 distinct native route pairs, 116 barrels and 28 layers exist in the loaded prefab.");
            Check(board.jumper.from.name == "U31.TRUSTED_DETERMINISTIC_READY_OUT" &&
                board.jumper.to.name == "U32.TRUSTED_DETERMINISTIC_READY_IN" &&
                board.pathA != board.pathB && board.portPins.Distinct().Count() == 6 && board.primaryPin != board.backupPin,
                "W321, two clocks, six PHYs and both uplinks have distinct native terminals.");

            var poses = File.ReadLines("Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_118_Layout/TSX26/Data/ComponentPlacements.csv").Skip(1)
                .Select(row => row.Split(',')).ToArray();
            foreach (var (part, i) in board.parts.Select((part, i) => (part, i)))
            {
                float Axis(int field) => float.Parse(poses[i][field], CultureInfo.InvariantCulture);
                var original = new Vector3(Axis(3), Axis(4), Axis(5));
                if (part.label != poses[i][0] || (part.footprint.localPosition - original).sqrMagnitude > 1e-10f)
                    throw new InvalidOperationException("Source footprint moved: " + part.label);
            }
            var bounds = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(bounds.size.x - 10.13f) < .10f && Mathf.Abs(bounds.size.z - 7.1754f) < .09f &&
                Mathf.Abs(bounds.center.x - center.x) < .04f && Mathf.Abs(bounds.center.z - center.z) < .04f,
                $"All original placements align and the board fits {bounds.size.x:F2} × {bounds.size.z:F2} on {size.x:F2} × {size.z:F2}.");
            var toBoard = board.fixture.worldToLocalMatrix;
            var uplinks = SparkEquipmentWorkbenchFit.MeasureArt(board.assemblyArt.transform.Find("L118_UplinkSFP_CagePair"), toBoard).center;
            var fields = SparkEquipmentWorkbenchFit.MeasureArt(board.assemblyArt.transform.Find("L118_FieldRJ45_CageBank"), toBoard).center;
            Check(Mathf.Abs(uplinks.x - .015f) < .001f && Mathf.Abs(fields.x - .080f) < .001f &&
                Mathf.Abs(uplinks.z + .130f) < .001f && Mathf.Abs(fields.z + .130f) < .001f,
                "Both original cage assemblies align with the source board orientation.");
            Check(board.fixture.GetComponentsInChildren<TextMesh>(true)
                .Any(label => label.text == "TSX-26  /  THE DETERMINISTIC GRID"), "The board carries the TSX-26 silkscreen.");
            Check(!board.fixture.Find("Assembled visual reference (inactive)").gameObject.activeSelf,
                "The assembled reference remains inactive and cannot award placement evidence.");
            await Screenshot("Level118Opening");

            board.SelectPart(1);
            Check(!board.FitPart(), "Mismatched packages are rejected.");
            board.SelectPart(0);
            board.RotatePart();
            Check(!board.FitPart() && !board.Solder(), "Wrong orientation and unseated parts cannot be secured.");
            board.SelectPart(0);
            Click("Fit part");
            Check(board.parts[0].model.activeSelf && !board.parts[0].contact.IsClosed && !board.Solder(),
                "Precision contacts require inspection before soldering.");
            Click("Inspect contacts");
            Click("Secure part");
            await Wait(() => board.Installed == 1 && PlayerPrefs.HasKey(SparkLevel118Process.CheckpointKey), "saved first part");
            var firstBoard = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Campaign reloads a partially assembled board.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) && board != firstBoard &&
                board.GetComponentInChildren<Canvas>(), "part checkpoint reloaded");
            Click("Resume");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && !board.Powered && !board.Qualifies,
                "The checkpoint restores the actual assembly contact with bench power isolated.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                string label = board.parts[i].label;
                bool precision = i < 8 || label.StartsWith("Y", StringComparison.Ordinal) || label.StartsWith("J", StringComparison.Ordinal);
                if (!board.FitPart() || precision && !board.InspectContacts() || !board.Solder())
                    throw new InvalidOperationException("Assembly failed at " + label);
                if (i % 16 == 0) await Task.Yield();
            }
            await Stage(1);
            var first = board.routes[0];
            board.SetLayer(2);
            Check(!board.PlateVia() && !board.ConnectPads(), "Wrong-layer plating and wiring are rejected.");
            for (int layer = 1; layer <= SparkLevel118Process.LayerCount; layer++)
            {
                board.SetLayer(layer);
                var button = board.GetComponentsInChildren<Button>().Single(item => item.name == "Layer reference");
                button.onClick.Invoke();
                if (!board.referenceLayers.gameObject.activeSelf || board.referenceLayers.Cast<Transform>()
                    .Single(item => item.gameObject.activeSelf).name != $"L{layer:00}_Reference")
                    throw new InvalidOperationException("Layer reference mismatch: L" + layer);
            }
            Log("PASS All 28 inspection layers display the original matching reference.");
            board.SelectRoute(0);
            Check(!board.ConnectPads() && board.PlateVia(), "A route requiring a barrel remains blocked until plated.");
            board.PadView();
            var start = Camera.main.WorldToScreenPoint(first.from.transform.position);
            var end = Camera.main.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(end), "Native Wire tool connects the selected copper pair.");
            await Wait(() => board.Routed == 1 && board.Plated == 1, "first route and plated span");
            await Screenshot("Level118Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                board.SelectRoute(i);
                if (board.routes[i].barrel && !board.PlateVia() || !board.ConnectPads())
                    throw new InvalidOperationException("Copper failed at " + board.routes[i].id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 320 && board.Plated == 116 && !board.TogglePower(),
                "Completed copper cannot bridge or replace the independent W321 edge.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "Initial W321 edge is inserted in the native graph.");
            await Stage(2);
            Check(board.witnesses.Length == 11 && board.witnesses.All(pin => board.Voltage(pin) > 4.7f) &&
                !board.MeasureClocks() && !board.TrainPorts(), "All 11 circuit domains solve; unpowered commissioning is rejected.");
            Click("Bench power");
            Check(board.RailsReady && !board.LoadSchedule() && !board.StartProtected(),
                "Rails alone cannot load the schedule or authorize protected traffic.");
            await QualifyPaths();
            await Stage(3);
            Click("Select windows");
            Check(!board.LoadSchedule() && board.Failure == SparkLevel118Process.GridFailure.ScheduleLimits &&
                !board.RunSchedule(), "A shared window too short for a full command is rejected.");
            Click("Select windows");
            await RunQualified();
            await Stage(4);
            Check(board.Qualifies && board.Sent == 192 && board.Delivered == 192 && board.Suppressed == 192 &&
                board.PathACopies == 192 && board.PathBCopies == 192 && board.Misses == 0 && board.CrcErrors == 0 &&
                board.JitterUs <= 50 && board.WorstUs <= 500 && board.PeakQueue >= 14 && board.CongestionDrops > 0 &&
                board.BackgroundDelivered > 0 && Enumerable.Range(0, 4).All(field => board.DeliveredAt(field) == 48),
                "Deadline, jitter, CRC, exact-once delivery, both path copies and sustained queue overload are verified. " + board.Evidence);
            await Screenshot("Level118TrafficProof");

            await Task.Delay(650);
            var commissioned = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Campaign reloads the commissioned board for evidence freshness.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) && board != commissioned &&
                board.GetComponentInChildren<Canvas>(), "commissioned checkpoint reloaded");
            Click("Resume");
            await Stage(4);
            Check(!board.Powered && !board.Qualifies && !board.ClockReady && !board.GateReady && board.Inhibited &&
                board.Routed == 320 && board.Plated == 116, "Checkpoint retains native copper and objective history with expired qualification.");
            Check(board.TogglePower(), "Re-energize the restored bench.");
            await QualifyPaths();
            await RunQualified();

            int[] faultSites = { board.clockPart, board.pathPart, board.sramPart };
            for (int fault = 0; fault < 3; fault++)
            {
                Check(board.BeginFaultTrial() && !board.ProbeFault(), "Staged faults require fresh observed evidence.");
                if (fault == 0)
                {
                    Check(board.MeasureClocks(), "Primary clock fault measurement starts.");
                    await Wait(() => !board.Measuring, "faulted timebase measurement");
                    Check(board.Failure == SparkLevel118Process.GridFailure.PrimaryDrift &&
                        Math.Abs(board.PrimaryDriftPpm) > 30 && Math.Abs(board.BackupDriftPpm) < 5 &&
                        board.Voltage(board.primaryPin) > 4.7f && board.Voltage(board.trimPin) < .5f &&
                        !board.LoadSchedule() && board.ProbeFault(), "Y1 trim opens with clock power retained and an independently stable backup. " + board.Evidence);
                }
                if (fault == 1)
                {
                    await QualifyPaths();
                    Check(board.LoadSchedule() && board.RunSchedule(), "Run timed traffic with a seeded Path B contact fault.");
                    await Wait(() => !board.Running, "Path B intermittency run");
                    Check(board.Failure == SparkLevel118Process.GridFailure.PathBVia && board.PathBOutages > 20 &&
                        board.Failovers > 20 && board.CrcErrors > 0 && board.Delivered == 192 &&
                        board.Misses == 0 && !board.Qualifies && board.ProbeFault(),
                        "Path A sustains delivery while Path B loses continuity and its recovering copies can fail CRC. " + board.Evidence);
                    await Screenshot("Level118PathFault");
                }
                if (fault == 2)
                {
                    await QualifyPaths();
                    Check(!board.LoadSchedule() && board.Failure == SparkLevel118Process.GridFailure.GateMemory &&
                        !board.GateReady && !board.RunSchedule() && board.Voltage(board.sramPin) < .5f && board.ProbeFault(),
                        "U15 opened data line corrupts readback. Scheduled traffic is rejected. " + board.Evidence);
                }

                Check(!board.Repair() && board.TogglePower(), "Fault repair requires bench isolation.");
                board.SelectFootprint(0);
                for (int kit = 0; kit <= fault; kit++) board.SelectSpare();
                Check(!board.Repair(), "A correct spare on the wrong footprint is rejected.");
                if (fault == 1)
                    Check(!board.InspectPathVia() && board.InspectPathVia(),
                        "Path B repair requires inspecting the original training via at its entry layer.");
                var targetPart = board.parts[faultSites[fault]];
                var footprint = targetPart.footprint.GetComponent<SparkLevel118Footprint>();
                Check(footprint && targetPart.footprint.GetComponent<Collider>(), "Repair target resolves to its own board collider.");
                ExecuteEvents.Execute<IPointerClickHandler>(footprint.gameObject, new PointerEventData(EventSystem.current)
                    { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
                Check(board.Repair() && !board.Qualifies && board.TogglePower(),
                    "The repair transaction awaits fresh qualification before credit.");
                await QualifyPaths();
                await RunQualified();
                Check(board.Repaired == fault + 1 && board.Qualifies,
                    $"Repair {fault + 1} qualifies only after new clock, path, gate and traffic evidence.");
            }

            await Stage(5);
            await Wait(() => board.Fresh && !board.Inhibited, "U32 native permit before W321 opens");
            Check(board.StartProtected(), "An active U32 permit starts the protected burst.");
            await Wait(() => board.ProtectedBusy && board.ProtectedDelivered >= 8, "protected traffic in flight");
            Check(board.OpenW321(), "The native wire registry opens W321 during active delivery.");
            Check(!board.JumperPresent && !board.ProtectedBusy && board.Inhibited && board.UndeliveredAtTrip > 0 &&
                board.Voltage(board.monitorSupply) > 4.7f && board.Voltage(board.rail) > 4.7f &&
                board.Voltage(board.primaryPin) > 4.7f && board.Voltage(board.backupPin) > 4.7f &&
                board.Voltage(board.jumper.to) < .5f && manager.SatisfiedTargetCount == 5,
                $"W321 stops {board.UndeliveredAtTrip} pending commands while both clocks and the independent rail stay powered.");
            await Screenshot("Level118W321Open");
            Check(board.RestoreW321() && board.Inhibited && !board.ResetSafety() && !board.StartProtected(),
                "Reconnection does not clear U32 or recreate expired operating evidence.");
            await QualifyPaths();
            await RunQualified();
            await Wait(() => board.Fresh, "fresh measured W321 heartbeat");
            Check(board.Inhibited && board.ResetSafety() && !board.Inhibited && board.StartProtected(),
                "U32 requires deliberate reset after fresh evidence before starting the recovery burst.");
            await Stage(6);
            Check(manager.IsCompleted && board.ProtectedDelivered == 192 && board.Repaired == 3 && board.JumperPresent,
                "Campaign manager reached 6/6 after 192/192 fresh protected commands.");
            Click("Port map");
            Click("Angle view");
            await Screenshot("Level118Inspection");
        }

        private static async Task QualifyPaths()
        {
            Check(board.TrainPorts() && board.TrainedPorts == 6 && board.PathAReady && board.PathBReady &&
                board.MeasureClocks(), "Six native PHY returns and separate uplinks train.");
            await Wait(() => !board.Measuring, "four-timestamp measurements and holdover probe");
            Check(board.ClockReady && Math.Abs(board.PrimaryOffsetNs) < 900 &&
                Math.Abs(board.PrimaryDriftPpm) < 5 && Math.Abs(board.BackupDriftPpm) < 5 &&
                board.HoldoverNs < 570 && board.ExpiredHoldoverNs > 850,
                $"Separate clocks qualify; backup uncertainty {board.HoldoverNs:F0} ns at 180 ms and {board.ExpiredHoldoverNs:F0} ns at 620 ms.");
        }

        private static async Task RunQualified()
        {
            Check(board.LoadSchedule() && board.GateReady && board.RunSchedule(), "Readback CRC and guarded schedule authorize a congestion run.");
            await Wait(() => !board.Running, "completed deterministic congestion test");
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
                    string readings = $"Rails {board.RailsReady}, clocks {board.ClockReady}, ports {board.TrainedPorts}, gate {board.GateReady}, permit {!board.Inhibited}";
                    throw new TimeoutException($"{step}. Stage {board.Stage}, epoch {board.Epoch}, failure {board.Failure}. {readings}. {board.Evidence}");
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
