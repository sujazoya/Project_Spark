using System;
using System.IO;
using System.Globalization;
using UnityEngine.EventSystems;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.SignalHorizon;
using SparkLevel125Process = ProjectSpark.Gameplay.SignalHorizon.SparkLevel125Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.SignalHorizon
{
    [InitializeOnLoad]
    internal static class Level125Acceptance
    {
        private const string Request = "Logs/Level125Acceptance.request";
        private const string Report = "Logs/Level125Acceptance.txt";
        private const string Session = "ProjectSpark.Level125.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel125Process board;

        static Level125Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/125/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE SIGNAL HORIZON / DSX33-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel125Builder.Prefab);
                Check(prefab && prefab.LevelDefinition && prefab.SceneBindings, "Serialized campaign prefab resolves.");
                var menu = AssetDatabase.LoadAssetAtPath<SceneAsset>(SparkLevel125Builder.Scene);
                Check(menu, "The campaign startup scene resolves.");
                var sourceScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (sourceScene.path != SparkLevel125Builder.Scene)
                {
                    Check(sourceScene.path == SparkLevel125Builder.PreviewScene && !sourceScene.isDirty &&
                        UnityEngine.SceneManagement.SceneManager.sceneCount == 1, "The saved Level 125 preview is ready to leave for campaign testing.");
                    EditorSceneManager.OpenScene(SparkLevel125Builder.Scene, OpenSceneMode.Single);
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
                        PlayerPrefs.SetString(SparkLevel125Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel125Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel125Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel125Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel125Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel125Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).Single();
            Check(index == levels.arraySize - 1 && index >= 0 && manager.LevelProgression &&
                manager.LevelProgression.CanPlayLevel(index) && manager.StartLevel(index),
                "MainMenu starts The Signal Horizon at the appended campaign index.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.JumperPresent &&
                !board.Powered && board.Inhibited && board.parts.All(part => !part.model.activeSelf) &&
                manager.SatisfiedTargetCount == 0, "Starts empty at 0/6 with W433 open and the protected output inhibited.");
            Check(board.parts.Length == 344 && board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 864 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 42 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 172,
                "344 footprints, 432 distinct native route pairs, 172 barrels and 42 layers exist in the loaded prefab.");
            Check(board.jumper.from.name == "U5.TRUSTED_TX_READY_OUT" &&
                board.jumper.to.name == "U3.TRUSTED_TX_READY_IN" &&
                board.sensorPins.Distinct().Count() == 2 && board.transmitterPins.Length == 2 &&
                board.gatePins.Distinct().Count() == 2 && !board.transmitterPins.Intersect(board.gatePins).Any(),
                "W433, two receivers, two test loads and the protected transmit gates use separate native terminals.");

            var poses = File.ReadLines("Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_125_Layout/DSX33/Data/ComponentPlacements.csv").Skip(1)
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
            Check(Mathf.Abs(bounds.size.x - 10.13f) < .10f && Mathf.Abs(bounds.size.z - 7.84258f) < .09f &&
                Mathf.Abs(bounds.center.x - center.x) < .04f && Mathf.Abs(bounds.center.z - center.z) < .04f,
                $"All original placements align and the board fits {bounds.size.x:F2} × {bounds.size.z:F2} on {size.x:F2} × {size.z:F2}.");
            var toBoard = board.fixture.worldToLocalMatrix;
            var structural = File.ReadLines(SparkLevel125Builder.Folder + "/DSX33/Data/ModelManifest.csv").Skip(1)
                .Select(row => row.Split(',')).Where(row => row[2] is "heatsink" or "shield").ToArray();
            Check(structural.Length == 9, "Six heat spreaders and three shields resolve to their authored model records.");
            foreach (var record in structural)
            {
                var item = board.assemblyArt.transform.Find(record[1]);
                float Axis(int field) => float.Parse(record[field], CultureInfo.InvariantCulture);
                var original = new Vector3(Axis(7), Axis(8), Axis(9));
                var centerInBoard = SparkEquipmentWorkbenchFit.MeasureArt(item, toBoard).center;
                if (!item.gameObject.activeSelf || (item.localPosition - original).sqrMagnitude > 1e-10f ||
                    Mathf.Abs(centerInBoard.x - original.x) > .001f || Mathf.Abs(centerInBoard.z - original.z) > .001f)
                    throw new InvalidOperationException("Source structural part moved: " + item.name + " " + centerInBoard);
            }
            Log("PASS All nine structural parts align at their original source positions.");
            Check(board.fixture.GetComponentsInChildren<TextMesh>(true)
                .Any(label => label.text == "DSX-33  /  THE SIGNAL HORIZON"), "The visible board label matches DSX-33.");
            Check(!board.fixture.Find("Assembled visual reference (inactive)").gameObject.activeSelf,
                "The assembled reference remains inactive and cannot award placement evidence.");
            await Screenshot("Level125Opening");

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
            await Wait(() => board.Installed == 1 && PlayerPrefs.HasKey(SparkLevel125Process.CheckpointKey), "saved first part");
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
                bool precision = i < 24 || label.StartsWith("Y", StringComparison.Ordinal) || label.StartsWith("J", StringComparison.Ordinal);
                if (!board.FitPart() || precision && !board.InspectContacts() || !board.Solder())
                    throw new InvalidOperationException("Assembly failed at " + label);
                if (i % 16 == 0) await Task.Yield();
            }
            await Stage(1);
            var first = board.routes[0];
            board.SetLayer(2);
            Check(!board.PlateVia() && !board.ConnectPads(), "Wrong-layer plating and wiring are rejected.");
            for (int layer = 1; layer <= SparkLevel125Process.LayerCount; layer++)
            {
                board.SetLayer(layer);
                var button = board.GetComponentsInChildren<Button>().Single(item => item.name == "Layer reference");
                button.onClick.Invoke();
                if (!board.referenceLayers.gameObject.activeSelf || board.referenceLayers.Cast<Transform>()
                    .Single(item => item.gameObject.activeSelf).name != $"L{layer:00}_Reference")
                    throw new InvalidOperationException("Layer reference mismatch: L" + layer);
            }
            Log("PASS All 42 inspection layers display the original matching reference.");
            board.SelectRoute(0);
            Check(!board.ConnectPads() && board.PlateVia(), "A route requiring a barrel remains blocked until plated.");
            board.PadView();
            var start = Camera.main.WorldToScreenPoint(first.from.transform.position);
            var end = Camera.main.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(end), "Native Wire tool connects the selected copper pair.");
            await Wait(() => board.Routed == 1 && board.Plated == 1, "first route and plated span");
            await Screenshot("Level125Copper");
            board.BoardView();
            for (int startIndex = 1; startIndex < board.routes.Length; startIndex += 32)
            {
                using (manager.ElectricalSolver.BeginConfigurationBatch())
                {
                    int endIndex = Math.Min(startIndex + 32, board.routes.Length);
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        board.SelectRoute(i);
                        if (board.routes[i].barrel && !board.PlateVia() || !board.ConnectPads())
                            throw new InvalidOperationException("Copper failed at " + board.routes[i].id);
                    }
                }
                await Task.Yield();
            }
            Check(board.Routed == 432 && board.Plated == 172 && !board.TogglePower(),
                "Completed copper cannot bridge or replace the independent W433 edge.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "Initial W433 edge is inserted in the native graph.");
            await Stage(2);
            Check(board.witnesses.Length == 8 && board.witnesses.All(pin => board.Voltage(pin) > 4.7f) && !board.Calibrate(),
                "All eight circuit domains solve. Unpowered calibration is rejected.");

            Check(board.TogglePower() && board.RailsReady && !board.Calibrate() && !board.RunLink(),
                "Powered rails do not bypass dual-clock measurement or sensor commissioning.");
            Commission();
            await Stage(3);
            Check(!board.RunLink() && board.CheckGuardRejection(),
                "Duplicate sequences, expired timestamps and uncorrectable payloads each fail the decoder.");
            await Replay();
            await Stage(4);
            Check(board.Qualifies && board.Accepted == 64 && board.Received == 64 && board.Corrected == 10 &&
                board.LowMargin == 16 && board.BackupFrames == 17 && board.Switchovers == 2 &&
                board.MinimumMargin is > 4.4f and < 4.7f && board.ActiveTxGate == -1,
                $"Virtual link verifies margin, bounded corrections and failover while transmission stays inhibited. {board.Evidence}");
            await Screenshot("Level125LinkProof");

            await Task.Delay(650);
            var commissioned = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Campaign reloads the commissioned board to check stale evidence.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) && board != commissioned &&
                board.GetComponentInChildren<Canvas>(), "commissioned checkpoint reloaded");
            Click("Resume");
            await Stage(4);
            Check(!board.Powered && !board.Qualifies && !board.ClockReady && !board.Calibrated && board.Inhibited &&
                board.Routed == 432 && board.Plated == 172, "Checkpoint retains copper and milestones and expires protected authorization.");
            await Requalify(true);

            int[] sites = { board.clockPart, board.filterPart, board.transmitterPart };
            var expected = new[] { SparkLevel125Process.LinkFailure.ClockDrift,
                SparkLevel125Process.LinkFailure.FilterNoise, SparkLevel125Process.LinkFailure.OutputCurrent };
            for (int fault = 0; fault < expected.Length; fault++)
            {
                Check(board.BeginFaultTrial() && !board.ProbeFault(), "Fault diagnosis requires new observed evidence.");
                if (fault == 0)
                    Check(!board.MeasureClock() && board.MeasuredSkew > 260f && board.Voltage(board.clockPin) > 4.7f &&
                        board.Voltage(board.trimPin) < .5f, "Y1 stays powered while the native trim contact opens and measured timing fails.");
                await Replay();
                Check(board.Failure == expected[fault] && !board.Qualifies && board.ProbeFault(),
                    $"Fault {fault + 1} has a distinct measured signature. {board.Evidence}");
                if (fault == 0)
                    Check(board.StaleClock == 64 && board.Received == 0 && board.FilterRejects == 0 &&
                        board.CurrentHolds == 0 && board.sensorPins.All(pin => board.Voltage(pin) > 4.7f),
                        "Clock drift rejects all 64 frames while both receivers stay powered.");
                if (fault == 1)
                {
                    Check(board.FilterRejects == 64 && board.Received == 48 && board.BackupFrames == 0 &&
                        board.Voltage(board.filterPin) is > 2.85f and < 3.0f && board.StaleClock == 0 &&
                        board.CurrentHolds == 0 && board.Voltage(board.sensorPins[0]) > 4.7f && board.Voltage(board.sensorPins[1]) > 4.7f,
                        "The native C1 series loss leaves RX A usable and prevents RX B from covering the 16-frame handover.");
                    await Screenshot("Level125FilterFault");
                }
                if (fault == 2)
                {
                    Check(board.CurrentHolds == 64 && board.Received == 64 && board.CurrentLatched && board.StaleClock == 0 &&
                        board.FilterRejects == 0 && board.TransmitterCurrent(0) > .39f && board.TransmitterCurrent(1) is > .045f and < .055f &&
                        board.Inhibited && board.ActiveTxGate == -1 && board.Voltage(board.telemetryPin) > 4.7f,
                        "The native Q1 shunt trips U3 while reception and independent telemetry stay live.");
                    Check(board.TogglePower() && board.TogglePower() && board.CurrentLatched &&
                        board.TransmitterCurrent(0) > .39f && !board.Calibrate(),
                        "Cycling power with the same Q1 condition cannot clear the native current latch.");
                }

                Check(!board.Repair() && board.TogglePower(), "The selected repair is blocked until the bench is isolated.");
                board.SelectFootprint(0);
                for (int spare = 0; spare <= fault; spare++) board.SelectSpare();
                Check(!board.Repair(), "A correct spare on the wrong footprint is rejected.");
                var targetPart = board.parts[sites[fault]];
                var footprint = targetPart.footprint.GetComponent<SparkLevel125Footprint>();
                Check(footprint && targetPart.footprint.GetComponent<Collider>(), "Repair site has its own board collider.");
                ExecuteEvents.Execute<IPointerClickHandler>(footprint.gameObject, new PointerEventData(EventSystem.current)
                    { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
                bool repairedContact = board.Repair() && (fault switch
                {
                    0 => board.clockContact.IsConducting,
                    1 => Mathf.Abs(board.filterSeries.Resistance - .12f) < .001f,
                    _ => Mathf.Abs(board.transmitterLoads[0].Resistance - 100f) < .01f
                });
                Check(repairedContact && !board.Qualifies, "Repair changed the exact native device and requires fresh trials.");
                await Requalify(true);
                Check(board.Repaired == fault + 1 && board.Qualifies && !board.CurrentLatched,
                    $"Repair {fault + 1} earns credit only after new measurement and replay.");
            }

            await Stage(5);
            await Wait(() => board.Fresh && board.ActiveTxGate == -1, "U3 ready with transmit inhibited at idle");
            Click("Test U3 timer");
            await Wait(() => board.WatchdogObserved && board.WatchdogActive && board.Voltage(board.jumper.to) > 4.7f,
                "held-high feedback expires");
            Check(!board.Fresh && !board.StartProtected() && board.ActiveTxGate == -1,
                "A static high on W433 cannot satisfy the independent U3 watchdog.");
            await Wait(() => !board.WatchdogActive && board.Fresh, "live readiness transitions after watchdog check");
            Click("Protected downlink");
            await Wait(() => board.ProtectedBusy && board.ActiveTxGate == 0, "native primary transmit gate");
            await Wait(() => board.ProtectedBusy && board.ActiveTxGate == 1 && board.Voltage(board.gatePins[1]) > 4.7f,
                "native backup transmit gate after the modeled handover");
            Log("PASS Live native TX A handed over to live TX B before opening W433.");
            Check(board.OpenW433(), "The native circuit opens W433 during protected backup transmit.");
            Check(!board.JumperPresent && !board.ProtectedBusy && board.Inhibited && board.ActiveTxGate == -1 && board.UndeliveredAtTrip > 0 &&
                board.Voltage(board.monitorSupply) > 4.7f && board.Voltage(board.rail) > 4.7f && board.Voltage(board.driveRail) > 4.7f &&
                board.Voltage(board.clockPin) > 4.7f && board.sensorPins.All(pin => board.Voltage(pin) > 4.7f) &&
                board.Voltage(board.telemetryPin) > 4.7f && board.gatePins.All(pin => board.Voltage(pin) < .5f) &&
                board.Voltage(board.jumper.to) < .5f && manager.SatisfiedTargetCount == 5 && !board.ResetSafety(),
                $"W433 inhibited both TX paths with {board.UndeliveredAtTrip} frames pending. RX and diagnostics stayed live. U3 refused reset while open.");
            await Screenshot("Level125W433Open");
            Check(board.RestoreW433() && board.Inhibited && !board.ResetSafety() && !board.StartProtected(),
                "Reconnection alone cannot bypass U3 or restore stale evidence.");
            await Requalify(false);
            await Wait(() => board.Fresh, "fresh post-repair U3 heartbeat");
            Check(board.Inhibited && !board.StartProtected() && board.ResetSafety() && board.Inhibited && board.StartProtected(),
                "An explicit U3 reset is required. Transmit stays inhibited until a fresh burst is authorized.");
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && board.JumperPresent && board.ProtectedDelivered == 64 &&
                board.Inhibited && board.ActiveTxGate == -1 && board.gatePins.All(pin => board.Voltage(pin) < .5f) &&
                board.ProtectedBackupFrames == 17,
                "Campaign completed 6/6 after 64/64 verified gate decisions, including 17 native backup bursts.");
            Click("Channel map");
            Click("Angle view");
            await Screenshot("Level125Inspection");
        }

        private static void Commission()
        {
            bool clock = board.MeasureClock();
            Check(clock && board.ClockReady && board.MeasuredSkew <= 125,
                $"Timing · clock A {board.Voltage(board.clockPin):F3} V · clock B {board.Voltage(board.secondaryClock):F3} V · {board.MeasuredSkew:F1} ns.");
            bool calibration = board.Calibrate();
            Check(calibration && board.Calibrated && board.SensorCount == 2 && board.TransmitterCount == 2 &&
                Enumerable.Range(0, 2).All(i => board.TransmitterCurrent(i) is > .045f and < .055f),
                $"Two live receive stages, two measured transmit shunts and C1 filter at {board.Voltage(board.filterPin):F3} V. {board.Evidence}");
        }

        private static async Task Requalify(bool powerUp)
        {
            if (powerUp) Check(board.TogglePower(), "Bench re-enabled.");
            Commission();
            Check(board.CheckGuardRejection(), "Guard challenges pass with the new commissioning evidence.");
            await Replay();
        }

        private static async Task Replay()
        {
            Check(board.RunLink(), "Deterministic 64-frame link trial started.");
            await Wait(() => !board.Running, "finished link trial");
        }

        private static async Task Screenshot(string name)
        {
            await Task.Delay(172);
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
                    string readings = $"Rails {board.RailsReady}, clock {board.ClockReady}, sensor links {board.SensorCount}, frames {board.Accepted}/{board.Sent}";
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
