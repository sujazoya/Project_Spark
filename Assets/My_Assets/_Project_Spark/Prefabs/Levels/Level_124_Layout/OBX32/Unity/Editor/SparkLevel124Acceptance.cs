using System;
using System.IO;
using System.Globalization;
using UnityEngine.EventSystems;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.OrbitalBastion;
using SparkLevel124Process = ProjectSpark.Gameplay.OrbitalBastion.SparkLevel124Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.OrbitalBastion
{
    [InitializeOnLoad]
    internal static class Level124Acceptance
    {
        private const string Request = "Logs/Level124Acceptance.request";
        private const string Report = "Logs/Level124Acceptance.txt";
        private const string Session = "ProjectSpark.Level124.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel124Process board;

        static Level124Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/124/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE ORBITAL BASTION / OBX32-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel124Builder.Prefab);
                Check(prefab && prefab.LevelDefinition && prefab.SceneBindings, "Serialized campaign prefab resolves.");
                var menu = AssetDatabase.LoadAssetAtPath<SceneAsset>(SparkLevel124Builder.Scene);
                Check(menu, "The campaign startup scene resolves.");
                var sourceScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (sourceScene.path != SparkLevel124Builder.Scene)
                {
                    Check(sourceScene.path == SparkLevel124Builder.PreviewScene && !sourceScene.isDirty &&
                        UnityEngine.SceneManagement.SceneManager.sceneCount == 1, "The saved Level 124 preview is ready to leave for campaign testing.");
                    EditorSceneManager.OpenScene(SparkLevel124Builder.Scene, OpenSceneMode.Single);
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
                        PlayerPrefs.SetString(SparkLevel124Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel124Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel124Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel124Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel124Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel124Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).Single();
            Check(index == levels.arraySize - 1 && index >= 0 && manager.LevelProgression &&
                manager.LevelProgression.CanPlayLevel(index) && manager.StartLevel(index),
                "MainMenu starts The Orbital Bastion at the appended campaign index.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.JumperPresent &&
                !board.Powered && board.Inhibited && board.parts.All(part => !part.model.activeSelf) &&
                manager.SatisfiedTargetCount == 0, "Starts empty at 0/6 with W417 open and the protected output inhibited.");
            Check(board.parts.Length == 328 && board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 832 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 40 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 164,
                "328 footprints, 416 distinct native route pairs, 164 barrels and 40 layers exist in the loaded prefab.");
            Check(board.jumper.from.name == "U5.TRUSTED_ATTITUDE_READY_OUT" &&
                board.jumper.to.name == "U3.TRUSTED_ATTITUDE_READY_IN" &&
                board.sensorPins.Distinct().Count() == 5 && board.wheelPins.Length == 4 &&
                board.gatePins.Distinct().Count() == 4 && !board.wheelPins.Intersect(board.gatePins).Any() &&
                !board.gatePins.Contains(board.unloadPin),
                "W417, five sensor channels, four wheel shunts, their gates and the momentum dump use separate native terminals.");

            var poses = File.ReadLines("Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_124_Layout/OBX32/Data/ComponentPlacements.csv").Skip(1)
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
            Check(Mathf.Abs(bounds.size.x - 10.13f) < .10f && Mathf.Abs(bounds.size.z - 7.76633f) < .09f &&
                Mathf.Abs(bounds.center.x - center.x) < .04f && Mathf.Abs(bounds.center.z - center.z) < .04f,
                $"All original placements align and the board fits {bounds.size.x:F2} × {bounds.size.z:F2} on {size.x:F2} × {size.z:F2}.");
            var toBoard = board.fixture.worldToLocalMatrix;
            var structural = File.ReadLines(SparkLevel124Builder.Folder + "/OBX32/Data/ModelManifest.csv").Skip(1)
                .Select(row => row.Split(',')).Where(row => row[2] is "heatsink" or "shield").ToArray();
            Check(structural.Length == 11, "Eight heat sinks and three shields resolve to their authored model records.");
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
            Log("PASS All eleven structural parts align at their original source positions.");
            Check(board.fixture.GetComponentsInChildren<TextMesh>(true)
                .Any(label => label.text == "OBX-32  /  THE ORBITAL BASTION"), "The visible board label matches OBX-32.");
            Check(!board.fixture.Find("Assembled visual reference (inactive)").gameObject.activeSelf,
                "The assembled reference remains inactive and cannot award placement evidence.");
            await Screenshot("Level124Opening");

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
            await Wait(() => board.Installed == 1 && PlayerPrefs.HasKey(SparkLevel124Process.CheckpointKey), "saved first part");
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
            for (int layer = 1; layer <= SparkLevel124Process.LayerCount; layer++)
            {
                board.SetLayer(layer);
                var button = board.GetComponentsInChildren<Button>().Single(item => item.name == "Layer reference");
                button.onClick.Invoke();
                if (!board.referenceLayers.gameObject.activeSelf || board.referenceLayers.Cast<Transform>()
                    .Single(item => item.gameObject.activeSelf).name != $"L{layer:00}_Reference")
                    throw new InvalidOperationException("Layer reference mismatch: L" + layer);
            }
            Log("PASS All 40 inspection layers display the original matching reference.");
            board.SelectRoute(0);
            Check(!board.ConnectPads() && board.PlateVia(), "A route requiring a barrel remains blocked until plated.");
            board.PadView();
            var start = Camera.main.WorldToScreenPoint(first.from.transform.position);
            var end = Camera.main.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(end), "Native Wire tool connects the selected copper pair.");
            await Wait(() => board.Routed == 1 && board.Plated == 1, "first route and plated span");
            await Screenshot("Level124Copper");
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
            Check(board.Routed == 416 && board.Plated == 164 && !board.TogglePower(),
                "Completed copper cannot bridge or replace the independent W417 edge.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "Initial W417 edge is inserted in the native graph.");
            await Stage(2);
            Check(board.witnesses.Length == 6 && board.witnesses.All(pin => board.Voltage(pin) > 4.7f) && !board.Calibrate(),
                "All six circuit domains solve. Unpowered calibration is rejected.");

            Check(board.TogglePower() && board.RailsReady && !board.Calibrate() && !board.RunOrbit(),
                "Powered rails do not bypass dual-clock measurement or sensor commissioning.");
            Commission();
            await Stage(3);
            Check(!board.RunOrbit() && board.CheckGuardRejection(),
                "Stale tracking, excess current and uncorrectable ECC each independently block the gate request.");
            await Replay();
            await Stage(4);
            Check(board.Qualifies && board.Accepted == 64 && board.CorrectedWords == 2 &&
                board.SafeHolds == 6 && board.PointingError < 3f && board.EclipseError < 9f &&
                board.DesaturationSamples == 16 && board.StoredBefore > .72f && board.StoredAfter < .36f &&
                board.EnabledGates == 0 && board.Voltage(board.unloadPin) < .5f,
                $"Attitude trial qualifies eclipse pointing, wheel unloading and ECC safe mode. {board.Evidence}");
            await Screenshot("Level124AttitudeProof");

            await Task.Delay(650);
            var commissioned = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Campaign reloads the commissioned board to check stale evidence.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) && board != commissioned &&
                board.GetComponentInChildren<Canvas>(), "commissioned checkpoint reloaded");
            Click("Resume");
            await Stage(4);
            Check(!board.Powered && !board.Qualifies && !board.ClockReady && !board.Calibrated && board.Inhibited &&
                board.Routed == 416 && board.Plated == 164, "Checkpoint retains copper and milestones and expires protected authorization.");
            await Requalify(true);

            int[] sites = { board.wheelPart, board.trackerPart, board.tachPart };
            var expected = new[] { SparkLevel124Process.SensorFailure.WheelOvercurrent,
                SparkLevel124Process.SensorFailure.TrackerClock, SparkLevel124Process.SensorFailure.Tachometer };
            for (int fault = 0; fault < expected.Length; fault++)
            {
                Check(board.BeginFaultTrial() && !board.ProbeFault(), "Fault diagnosis requires new observed evidence.");
                await Replay();
                Check(board.Failure == expected[fault] && !board.Qualifies && board.ProbeFault(),
                    $"Fault {fault + 1} has a distinct measured signature. {board.Evidence}");
                if (fault == 0)
                {
                    Check(board.OvercurrentHolds == 64 && board.CurrentLatched && board.StaleB == 0 && board.TachHolds == 0 &&
                        board.WheelTestCurrent(2) > .39f && board.WheelTestCurrent(1) is > .045f and < .055f &&
                        board.Inhibited && board.EnabledGates == 0 && board.Voltage(board.telemetryPin) > 4.7f,
                        "The native RW3 shunt trips U3 while independent telemetry and neighboring wheel loads stay live.");
                    await Screenshot("Level124CurrentFault");
                    Check(board.TogglePower() && board.TogglePower() && board.CurrentLatched &&
                        board.WheelTestCurrent(2) > .39f && !board.Calibrate(),
                        "Cycling power with the same shunt cannot clear the native current latch.");
                }
                if (fault == 1)
                    Check(!board.trackerContact.IsConducting && board.StaleB == 26 && board.Accepted == 38 &&
                        board.OvercurrentHolds == 0 && board.TachHolds == 0 && board.Voltage(board.sensorPins[0]) > 4.7f &&
                        board.Voltage(board.sensorPins[1]) < .5f,
                        "The native tracker B via loses 26 frames while tracker A and the gyro stay live.");
                if (fault == 2)
                    Check(!board.tachContact.IsConducting && board.TachHolds == 64 && board.TachDifference > .35f &&
                        board.Voltage(board.tachReference) is > 3.65f and < 3.90f && board.StaleB == 0 &&
                        board.OvercurrentHolds == 0 && board.Voltage(board.rail) > 4.7f,
                        "The native tachometer reference drifts while independent wheel current and tracker timing remain healthy.");

                Check(!board.Repair() && board.TogglePower(), "The selected repair is blocked until the bench is isolated.");
                board.SelectFootprint(0);
                for (int spare = 0; spare <= fault; spare++) board.SelectSpare();
                Check(!board.Repair(), "A correct spare on the wrong footprint is rejected.");
                if (fault == 1)
                    Check(!board.InspectTrackerVia() && board.InspectTrackerVia(),
                        "Tracker via repair requires its correct entry layer.");
                var targetPart = board.parts[sites[fault]];
                var footprint = targetPart.footprint.GetComponent<SparkLevel124Footprint>();
                Check(footprint && targetPart.footprint.GetComponent<Collider>(), "Repair site has its own board collider.");
                ExecuteEvents.Execute<IPointerClickHandler>(footprint.gameObject, new PointerEventData(EventSystem.current)
                    { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
                bool repairedContact = board.Repair() && (fault switch
                {
                    0 => Mathf.Abs(board.wheelLoads[2].Resistance - 100f) < .01f,
                    1 => board.trackerContact.IsConducting,
                    _ => board.tachContact.IsConducting
                });
                Check(repairedContact && !board.Qualifies, "Repair changed the exact native device and requires fresh trials.");
                await Requalify(true);
                Check(board.Repaired == fault + 1 && board.Qualifies && !board.CurrentLatched,
                    $"Repair {fault + 1} earns credit only after new measurement and replay.");
            }

            await Stage(5);
            await Wait(() => board.Fresh && board.EnabledGates == 0, "U3 ready with gates inhibited at idle");
            Click("Test U3 timer");
            await Wait(() => board.WatchdogObserved && board.WatchdogActive && board.Voltage(board.jumper.to) > 4.7f,
                "held-high feedback expires");
            Check(!board.Fresh && !board.StartProtected() && board.EnabledGates == 0,
                "A static high on W417 cannot satisfy the independent U3 watchdog.");
            await Wait(() => !board.WatchdogActive && board.Fresh, "live readiness transitions after watchdog check");
            Click("Protected trial");
            await Wait(() => board.ProtectedBusy && board.EnabledGates == 4 && board.Voltage(board.unloadPin) > 4.7f,
                "live wheel and momentum-dump gates");
            Log("PASS All four wheel gates and the independently solved unload gate are energized before opening W417.");
            Check(board.OpenW417(), "The native circuit opens W417 during a protected attitude trial.");
            Check(!board.JumperPresent && !board.ProtectedBusy && board.Inhibited && board.EnabledGates == 0 && board.UndeliveredAtTrip > 0 &&
                board.Voltage(board.monitorSupply) > 4.7f && board.Voltage(board.rail) > 4.7f && board.Voltage(board.driveRail) > 4.7f &&
                board.Voltage(board.clockPin) > 4.7f && board.sensorPins.All(pin => board.Voltage(pin) > 4.7f) &&
                board.Voltage(board.telemetryPin) > 4.7f && board.Voltage(board.unloadPin) < .5f &&
                board.gatePins.All(pin => board.Voltage(pin) < .5f) &&
                board.Voltage(board.jumper.to) < .5f && manager.SatisfiedTargetCount == 5,
                $"W417 disabled all four wheel outputs and the unload gate with {board.UndeliveredAtTrip} trials pending while wheel supply, compute and telemetry stayed live.");
            await Screenshot("Level124W417Open");
            Check(board.RestoreW417() && board.Inhibited && !board.ResetSafety() && !board.StartProtected(),
                "Reconnection alone cannot bypass U3 or restore stale evidence.");
            await Requalify(false);
            await Wait(() => board.Fresh, "fresh post-repair U3 heartbeat");
            Check(board.Inhibited && !board.StartProtected() && board.ResetSafety() && board.Inhibited && board.StartProtected(),
                "An explicit U3 reset is required. Gates remain safe until the next authorized attitude sample.");
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && board.JumperPresent && board.ProtectedDelivered == 64 &&
                board.Inhibited && board.EnabledGates == 0 && board.gatePins.All(pin => board.Voltage(pin) < .5f) &&
                board.Voltage(board.unloadPin) < .5f && board.ProtectedUnloads == 16 && board.ProtectedSafeHolds == 6,
                "Campaign completed 6/6 after 64/64 verified gate decisions, including 16 native unload samples and six ECC holds.");
            Click("Wheel map");
            Click("Angle view");
            await Screenshot("Level124Inspection");
        }

        private static void Commission()
        {
            bool clock = board.MeasureClock();
            Check(clock && board.ClockReady && board.MeasuredSkew <= 140,
                $"Timing · clock A {board.Voltage(board.clockPin):F3} V · clock B {board.Voltage(board.secondaryClock):F3} V · {board.MeasuredSkew:F1} µs.");
            bool calibration = board.Calibrate();
            Check(calibration && board.Calibrated && board.SensorCount == 5 && board.WheelCount == 4 &&
                Enumerable.Range(0, 4).All(i => board.WheelTestCurrent(i) is > .045f and < .055f),
                $"Five live sensor paths · four measured wheel shunts · tachometer reference {board.Voltage(board.tachReference):F3} V.");
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
            Check(board.RunOrbit(), "Deterministic 64-sample attitude trial started.");
            await Wait(() => !board.Running, "finished attitude trial");
        }

        private static async Task Screenshot(string name)
        {
            await Task.Delay(164);
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
