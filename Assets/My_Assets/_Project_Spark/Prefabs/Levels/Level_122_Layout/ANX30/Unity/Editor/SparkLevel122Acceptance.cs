using System;
using System.IO;
using System.Globalization;
using UnityEngine.EventSystems;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.AutonomySentinel;
using SparkLevel122Process = ProjectSpark.Gameplay.AutonomySentinel.SparkLevel122Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.AutonomySentinel
{
    [InitializeOnLoad]
    internal static class Level122Acceptance
    {
        private const string Request = "Logs/Level122Acceptance.request";
        private const string Report = "Logs/Level122Acceptance.txt";
        private const string Session = "ProjectSpark.Level122.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel122Process board;

        static Level122Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/122/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE AUTONOMY SENTINEL / ANX30-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel122Builder.Prefab);
                Check(prefab && prefab.LevelDefinition && prefab.SceneBindings, "Serialized campaign prefab resolves.");
                var menu = AssetDatabase.LoadAssetAtPath<SceneAsset>(SparkLevel122Builder.Scene);
                Check(menu, "The campaign startup scene resolves.");
                var sourceScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (sourceScene.path != SparkLevel122Builder.Scene)
                {
                    Check(sourceScene.path == SparkLevel122Builder.PreviewScene && !sourceScene.isDirty &&
                        UnityEngine.SceneManagement.SceneManager.sceneCount == 1, "The saved Level 122 preview is ready to leave for campaign testing.");
                    EditorSceneManager.OpenScene(SparkLevel122Builder.Scene, OpenSceneMode.Single);
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
                        PlayerPrefs.SetString(SparkLevel122Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel122Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel122Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel122Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel122Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel122Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).Single();
            Check(index == levels.arraySize - 1 && index >= 0 && manager.LevelProgression &&
                manager.LevelProgression.CanPlayLevel(index) && manager.StartLevel(index),
                "MainMenu starts The Autonomy Sentinel at the appended campaign index.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.JumperPresent &&
                !board.Powered && board.Inhibited && board.parts.All(part => !part.model.activeSelf) &&
                manager.SatisfiedTargetCount == 0, "Starts empty at 0/6 with W385 open and the protected output inhibited.");
            Check(board.parts.Length == 296 && board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 768 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 36 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 148,
                "296 footprints, 384 distinct native route pairs, 148 barrels and 36 layers exist in the loaded prefab.");
            Check(board.jumper.from.name == "U5.TRUSTED_LOCALIZATION_READY_OUT" &&
                board.jumper.to.name == "U4.TRUSTED_LOCALIZATION_READY_IN" &&
                board.sensorPins.Distinct().Count() == 6 && !board.sensorPins.Contains(board.referencePin),
                "W385, dual GNSS, dual radar and redundant IMU use separate native terminals.");

            var poses = File.ReadLines("Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_122_Layout/ANX30/Data/ComponentPlacements.csv").Skip(1)
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
            Check(Mathf.Abs(bounds.size.x - 10.13f) < .10f && Mathf.Abs(bounds.size.z - 7.5975f) < .09f &&
                Mathf.Abs(bounds.center.x - center.x) < .04f && Mathf.Abs(bounds.center.z - center.z) < .04f,
                $"All original placements align and the board fits {bounds.size.x:F2} × {bounds.size.z:F2} on {size.x:F2} × {size.z:F2}.");
            var toBoard = board.fixture.worldToLocalMatrix;
            var structural = File.ReadLines(SparkLevel122Builder.Folder + "/ANX30/Data/ModelManifest.csv").Skip(1)
                .Select(row => row.Split(',')).Where(row => row[2] is "heatsink" or "antenna" or "shield").ToArray();
            Check(structural.Length == 14, "Four heat sinks, six antennas and four shields resolve to their authored model records.");
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
            Log("PASS All fourteen structural parts align at their original source positions.");
            Check(board.fixture.GetComponentsInChildren<TextMesh>(true)
                .Any(label => label.text == "ANX-30  /  THE AUTONOMY SENTINEL"), "The visible board label matches ANX-30.");
            Check(!board.fixture.Find("Assembled visual reference (inactive)").gameObject.activeSelf,
                "The assembled reference remains inactive and cannot award placement evidence.");
            await Screenshot("Level122Opening");

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
            await Wait(() => board.Installed == 1 && PlayerPrefs.HasKey(SparkLevel122Process.CheckpointKey), "saved first part");
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
            for (int layer = 1; layer <= SparkLevel122Process.LayerCount; layer++)
            {
                board.SetLayer(layer);
                var button = board.GetComponentsInChildren<Button>().Single(item => item.name == "Layer reference");
                button.onClick.Invoke();
                if (!board.referenceLayers.gameObject.activeSelf || board.referenceLayers.Cast<Transform>()
                    .Single(item => item.gameObject.activeSelf).name != $"L{layer:00}_Reference")
                    throw new InvalidOperationException("Layer reference mismatch: L" + layer);
            }
            Log("PASS All 36 inspection layers display the original matching reference.");
            board.SelectRoute(0);
            Check(!board.ConnectPads() && board.PlateVia(), "A route requiring a barrel remains blocked until plated.");
            board.PadView();
            var start = Camera.main.WorldToScreenPoint(first.from.transform.position);
            var end = Camera.main.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(end), "Native Wire tool connects the selected copper pair.");
            await Wait(() => board.Routed == 1 && board.Plated == 1, "first route and plated span");
            await Screenshot("Level122Copper");
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
            Check(board.Routed == 384 && board.Plated == 148 && !board.TogglePower(),
                "Completed copper cannot bridge or replace the independent W385 edge.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "Initial W385 edge is inserted in the native graph.");
            await Stage(2);
            Check(board.witnesses.Length == 8 && board.witnesses.All(pin => board.Voltage(pin) > 4.7f) && !board.Calibrate(),
                "All eight circuit domains solve. Unpowered calibration is rejected.");

            Check(board.TogglePower() && board.RailsReady && !board.Calibrate() && !board.RunNavigation(),
                "Powered rails do not bypass PLL measurement or sensor commissioning.");
            Commission();
            await Stage(3);
            Check(!board.RunNavigation() && board.CheckGuardRejection(),
                "Spoofed GNSS, a stale command and disagreeing IMUs each receive an independent hold.");
            await Replay();
            await Stage(4);
            Check(board.Qualifies && board.Accepted == 64 && board.SafeDecisions == 64 && board.BrakeHolds >= 8 &&
                board.BoundaryHolds >= 5 && board.AgedCommands >= 4 && board.Indoor == 32 &&
                board.PeakError < .22f && board.MaxUncertainty < .48f,
                "Navigation qualifies indoor/outdoor localization, obstacle, geofence and command freshness. " + board.Evidence);
            await Screenshot("Level122NavigationProof");

            await Task.Delay(650);
            var commissioned = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Campaign reloads the commissioned board to check stale evidence.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) && board != commissioned &&
                board.GetComponentInChildren<Canvas>(), "commissioned checkpoint reloaded");
            Click("Resume");
            await Stage(4);
            Check(!board.Powered && !board.Qualifies && !board.ClockReady && !board.Calibrated && board.Inhibited &&
                board.Routed == 384 && board.Plated == 148, "Checkpoint retains copper and milestones and expires protected authorization.");
            await Requalify(true);

            int[] sites = { board.pllPart, board.gnssPart, board.imuPart };
            SparkSwitch[] contacts = { board.pllContact, board.rfContact, board.imuContact };
            var expected = new[] { SparkLevel122Process.SensorFailure.RadarPll,
                SparkLevel122Process.SensorFailure.GnssAFeed, SparkLevel122Process.SensorFailure.ImuReference };
            for (int fault = 0; fault < expected.Length; fault++)
            {
                Check(board.BeginFaultTrial() && !board.ProbeFault(), "Fault diagnosis requires new observed evidence.");
                if (fault == 0)
                    Check(!board.MeasureClock() && board.MeasuredPhase > 120f && board.Voltage(board.clockPin) > 4.7f,
                        "Radar PLL failure exceeds the jitter limit while the U17 timebase stays powered.");
                await Replay();
                Check(!contacts[fault].IsConducting && board.Failure == expected[fault] && !board.Qualifies && board.ProbeFault(),
                    $"Fault {fault + 1} opens its own contact and produces a distinct measured signature. {board.Evidence}");
                if (fault == 0)
                {
                    Check(board.BadRadar == 64 && board.MissingA == 0 && board.Diverged == 0 &&
                        board.Accepted == 0 && board.PeakPhase > 120f, "All 64 radar trials lose ranging integrity while GNSS and IMU stay valid.");
                    await Screenshot("Level122RadarFault");
                }
                if (fault == 1)
                    Check(board.MissingA == 12 && board.Accepted == 52 && board.BadRadar == 0 && board.Diverged == 0 &&
                        board.Voltage(board.sensorPins[1]) > 4.7f, "GNSS A intermittency affects outdoors while independent GNSS B and indoor radar stay live.");
                if (fault == 2)
                    Check(board.Diverged == 64 && board.PeakImuDelta > .30f && board.Voltage(board.referencePin) < .5f &&
                        board.Voltage(board.rail) > 4.7f && board.BadRadar == 0 && board.MissingA == 0,
                        "The native U11 reference contact opens while both IMUs stay powered and 64 estimates disagree.");

                Check(!board.Repair() && board.TogglePower(), "The selected repair is blocked until the bench is isolated.");
                board.SelectFootprint(0);
                for (int spare = 0; spare <= fault; spare++) board.SelectSpare();
                Check(!board.Repair(), "A correct spare on the wrong footprint is rejected.");
                if (fault == 1)
                    Check(!board.InspectGnssVia() && board.InspectGnssVia(),
                        "GNSS A repair requires inspection of the correct via entry layer.");
                var targetPart = board.parts[sites[fault]];
                var footprint = targetPart.footprint.GetComponent<SparkLevel122Footprint>();
                Check(footprint && targetPart.footprint.GetComponent<Collider>(), "Repair site has its own board collider.");
                ExecuteEvents.Execute<IPointerClickHandler>(footprint.gameObject, new PointerEventData(EventSystem.current)
                    { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
                Check(board.Repair() && contacts[fault].IsConducting && !board.Qualifies, "A correct replacement closes that exact native contact and requires fresh trials.");
                await Requalify(true);
                Check(board.Repaired == fault + 1 && board.Qualifies,
                    $"Repair {fault + 1} earns credit only after new measurement and replay.");
            }

            await Stage(5);
            await Wait(() => board.Fresh && !board.Inhibited, "U4 permit before W385 disconnect");
            Check(board.StartProtected(), "A solved U4 permit starts protected motion.");
            await Wait(() => board.ProtectedBusy && board.ProtectedDelivered >= 6 && !board.Inhibited, "qualified frames in flight");
            Check(board.OpenW385(), "The native circuit opens W385 during active protected acquisition.");
            Check(!board.JumperPresent && !board.ProtectedBusy && board.Inhibited && board.UndeliveredAtTrip > 0 &&
                board.Voltage(board.monitorSupply) > 4.7f && board.Voltage(board.rail) > 4.7f &&
                board.Voltage(board.clockPin) > 4.7f && board.sensorPins.All(pin => board.Voltage(pin) > 4.7f) &&
                board.Voltage(board.jumper.to) < .5f && manager.SatisfiedTargetCount == 5,
                $"W385 inhibited {board.UndeliveredAtTrip} pending sets while independent diagnostics and sensors stayed powered.");
            await Screenshot("Level122W385Open");
            Check(board.RestoreW385() && board.Inhibited && !board.ResetSafety() && !board.StartProtected(),
                "Reconnection alone cannot bypass U4 or restore stale evidence.");
            await Requalify(false);
            await Wait(() => board.Fresh, "fresh post-repair U4 heartbeat");
            Check(board.Inhibited && board.ResetSafety() && !board.Inhibited && board.StartProtected(),
                "Explicit U4 reset releases the freshly qualified protected output.");
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && board.JumperPresent && board.ProtectedDelivered == 64,
                "Campaign manager completed 6/6 after 64/64 verified native motion-gate decisions.");
            Click("Sensor map");
            Click("Angle view");
            await Screenshot("Level122Inspection");
        }

        private static void Commission()
        {
            bool clock = board.MeasureClock();
            Check(clock && board.ClockReady && board.MeasuredPhase <= 62,
                $"Clock contact measurement · reference {board.Voltage(board.clockPin):F3} V · PLL {board.Voltage(board.pllPin):F3} V · {board.MeasuredPhase:F1} ns.");
            bool calibration = board.Calibrate();
            Check(calibration && board.Calibrated && board.SensorCount == 6,
                $"Sensor commissioning · six live links {board.SensorCount}/6 · reference {board.Voltage(board.referencePin):F3} V.");
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
            Check(board.RunNavigation(), "Deterministic 64-sample navigation trial started.");
            await Wait(() => !board.Running, "finished navigation trial");
        }

        private static async Task Screenshot(string name)
        {
            await Task.Delay(148);
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
