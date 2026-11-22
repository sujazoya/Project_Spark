using System;
using System.IO;
using System.Globalization;
using UnityEngine.EventSystems;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.ThermalAegis;
using SparkLevel127Process = ProjectSpark.Gameplay.ThermalAegis.SparkLevel127Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.ThermalAegis
{
    [InitializeOnLoad]
    internal static class Level127Acceptance
    {
        private const string Request = "Logs/Level127Acceptance.request";
        private const string Report = "Logs/Level127Acceptance.txt";
        private const string Session = "ProjectSpark.Level127.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel127Process board;

        static Level127Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/127/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE THERMAL AEGIS / TCX35-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel127Builder.Prefab);
                Check(prefab && prefab.LevelDefinition && prefab.SceneBindings, "Serialized campaign prefab resolves.");
                var menu = AssetDatabase.LoadAssetAtPath<SceneAsset>(SparkLevel127Builder.Scene);
                Check(menu, "The campaign startup scene resolves.");
                var sourceScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (sourceScene.path != SparkLevel127Builder.Scene)
                {
                    Check(sourceScene.path == SparkLevel127Builder.PreviewScene && !sourceScene.isDirty &&
                        UnityEngine.SceneManagement.SceneManager.sceneCount == 1, "The saved Level 127 preview is ready to leave for campaign testing.");
                    EditorSceneManager.OpenScene(SparkLevel127Builder.Scene, OpenSceneMode.Single);
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
                        PlayerPrefs.SetString(SparkLevel127Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel127Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel127Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel127Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel127Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel127Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).Single();
            Check(index == levels.arraySize - 1 && index >= 0 && manager.LevelProgression &&
                manager.LevelProgression.CanPlayLevel(index) && manager.StartLevel(index),
                "MainMenu starts The Thermal Aegis at the appended campaign index.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.JumperPresent &&
                !board.Powered && board.Inhibited && board.parts.All(part => !part.model.activeSelf) &&
                manager.SatisfiedTargetCount == 0, "Starts empty at 0/6 with W465 open and the protected output inhibited.");
            Check(board.parts.Length == 376 && board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 928 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 46 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 188,
                "376 footprints, 464 distinct native route pairs, 188 barrels and 46 layers exist in the loaded prefab.");
            Check(board.jumper.from.name == "U6.TRUSTED_THERMAL_READY_OUT" &&
                board.jumper.to.name == "U3.TRUSTED_THERMAL_READY_IN" &&
                board.pumpPins.Distinct().Count() == 2 && board.radiatorPins.Distinct().Count() == 2 &&
                board.sensorPins.Distinct().Count() == 12 && board.heaterSupply != board.protectedFeed,
                "W465, pump loops, all twelve sensors, survival heat and the radiator gate use separate native terminals.");

            var poses = File.ReadLines("Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_127_Layout/TCX35/Data/ComponentPlacements.csv").Skip(1)
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
            Check(Mathf.Abs(bounds.size.x - 10.13f) < .10f && Mathf.Abs(bounds.size.z - 7.98121f) < .09f &&
                Mathf.Abs(bounds.center.x - center.x) < .04f && Mathf.Abs(bounds.center.z - center.z) < .04f,
                $"All original placements align and the board fits {bounds.size.x:F2} × {bounds.size.z:F2} on {size.x:F2} × {size.z:F2}.");
            var toBoard = board.fixture.worldToLocalMatrix;
            var structural = File.ReadLines(SparkLevel127Builder.Folder + "/TCX35/Data/ModelManifest.csv").Skip(1)
                .Select(row => row.Split(',')).Where(row => row[2] is "heatsink" or "heatpipe" or "shield").ToArray();
            Check(structural.Length == 10, "Six heat spreaders, three heat pipes and one shield resolve to their authored model records.");
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
            Log("PASS All ten structural parts align at their original source positions.");
            Check(board.fixture.GetComponentsInChildren<TextMesh>(true)
                .Any(label => label.text == "TCX-35  /  THE THERMAL AEGIS"), "The visible board label matches TCX-35.");
            Check(!board.fixture.Find("Assembled visual reference (inactive)").gameObject.activeSelf,
                "The assembled reference remains inactive and cannot award placement evidence.");
            Check(board.jumperVisual && !board.jumperVisual.activeSelf && board.parts.All(part => part.label != "W465"),
                "W465 starts open as a separate structural item outside the 376-part assembly count.");
            await Screenshot("Level127Opening");

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
            await Wait(() => board.Installed == 1 && PlayerPrefs.HasKey(SparkLevel127Process.CheckpointKey), "saved first part");
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
            for (int layer = 1; layer <= SparkLevel127Process.LayerCount; layer++)
            {
                board.SetLayer(layer);
                var button = board.GetComponentsInChildren<Button>().Single(item => item.name == "Layer reference");
                button.onClick.Invoke();
                if (!board.referenceLayers.gameObject.activeSelf || board.referenceLayers.Cast<Transform>()
                    .Single(item => item.gameObject.activeSelf).name != $"L{layer:00}_Reference")
                    throw new InvalidOperationException("Layer reference mismatch: L" + layer);
            }
            Log("PASS All 46 inspection layers display the original matching reference.");
            board.SelectRoute(0);
            Check(!board.ConnectPads() && board.PlateVia(), "A route requiring a barrel remains blocked until plated.");
            board.PadView();
            var start = Camera.main.WorldToScreenPoint(first.from.transform.position);
            var end = Camera.main.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(end), "Native Wire tool connects the selected copper pair.");
            await Wait(() => board.Routed == 1 && board.Plated == 1, "first route and plated span");
            await Screenshot("Level127Copper");
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
            Check(board.Routed == 464 && board.Plated == 188 && !board.TogglePower(),
                "Completed copper cannot bridge or replace the independent W465 edge.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads() && board.jumperVisual.activeSelf, "Initial W465 edge is inserted in the native graph and shows the original jumper.");
            await Stage(2);
            Check(board.witnesses.Length == 8 && board.witnesses.All(pin => board.Voltage(pin) > 4.7f) && !board.Calibrate(),
                "All eight circuit domains solve. Unpowered calibration is rejected.");

            Check(board.TogglePower() && board.RailsReady && !board.Calibrate() && !board.RunThermal(),
                "Powered rails do not bypass native pump measurement or sensor commissioning.");
            Commission();
            await Stage(3);
            Check(!board.RunThermal() && board.CheckGuardRejection(),
                "Missing sensors, reference disagreement, lost flow and bad data each inhibit radiator requests.");
            await Replay();
            await Stage(4);
            Check(board.Qualifies && board.Accepted == 64 && board.HeaterSamples >= 3 && board.BackupSamples == 16 &&
                board.RestoredSamples == 16 && board.PeakTemperature is > 45f and < 50f && board.MinimumTemperature >= 15f &&
                board.FinalTemperature < board.PeakTemperature - 2f && board.BalanceError < .05f && board.Inhibited,
                $"Thermal endurance verifies cold start, heat pulse, loop-B takeover and recovery. {board.Evidence}");
            float healthyPeak = board.PeakTemperature;
            await Screenshot("Level127ThermalProof");

            await Task.Delay(650);
            var commissioned = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Campaign reloads the commissioned board to check stale evidence.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) && board != commissioned &&
                board.GetComponentInChildren<Canvas>(), "commissioned checkpoint reloaded");
            Click("Resume");
            await Stage(4);
            Check(!board.Powered && !board.Qualifies && !board.Measured && !board.Calibrated && board.Inhibited &&
                board.Routed == 464 && board.Plated == 188, "Checkpoint retains copper and milestones and expires protected authorization.");
            await Requalify(true);

            int[] sites = { board.pumpPart, board.sensePart, board.outletPart };
            var expected = new[] { SparkLevel127Process.ThermalFailure.PumpOpen,
                SparkLevel127Process.ThermalFailure.SensorReference, SparkLevel127Process.ThermalFailure.StaleOutlet };
            for (int fault = 0; fault < expected.Length; fault++)
            {
                Check(board.BeginFaultTrial() && !board.ProbeFault(), "Fault diagnosis requires new observed evidence.");
                await Replay();
                Check(board.Failure == expected[fault] && !board.Qualifies && board.ProbeFault(),
                    $"Fault {fault + 1} has a distinct measured signature. {board.Evidence}");
                if (fault == 0)
                {
                    Check(!board.pumpAContact.IsConducting && board.PumpLoss == 48 && board.PumpCurrent(0) < .001f &&
                        board.PumpCurrent(1) is > .036f and < .044f && board.PeakTemperature > healthyPeak + 1.2f &&
                        board.DriftSamples == 0 && board.StaleSamples == 0,
                        "Q1 opens the native pump A driver. Pump B takes over and the simulated core warms further.");
                    await Screenshot("Level127PumpFault");
                }
                if (fault == 1)
                    Check(board.DriftSamples == 64 && board.MaxDisagreement is > 10f and < 12f &&
                        board.ReferenceGain is > 1.46f and < 1.48f && board.PumpLoss == 0 && board.StaleSamples == 0,
                        "R1 changes the native temperature reference while both pump paths remain intact.");
                if (fault == 2)
                    Check(!board.outletContact.IsConducting && board.StaleSamples == 26 && board.PeakSensorAge == 4f &&
                        board.Voltage(board.sensorPins[0]) < .5f && board.sensorPins.Skip(1).All(pin => board.Voltage(pin) > 4.7f) &&
                        board.Inhibited && board.DriftSamples == 0 && board.PumpLoss == 0,
                        "TH1 intermittency ages out outlet data while the eleven independent channels stay live.");

                Check(!board.Repair() && board.TogglePower(), "The selected repair is blocked until the bench is isolated.");
                board.SelectFootprint(0);
                for (int spare = 0; spare <= fault; spare++) board.SelectSpare();
                Check(!board.Repair(), "A correct spare on the wrong footprint is rejected.");
                var targetPart = board.parts[sites[fault]];
                var footprint = targetPart.footprint.GetComponent<SparkLevel127Footprint>();
                Check(footprint && targetPart.footprint.GetComponent<Collider>(), "Repair site has its own board collider.");
                ExecuteEvents.Execute<IPointerClickHandler>(footprint.gameObject, new PointerEventData(EventSystem.current)
                    { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
                bool repairedContact = board.Repair() && (fault switch
                {
                    0 => board.pumpAContact.IsConducting,
                    1 => Mathf.Abs(board.referenceLoad.Resistance - 100f) < .01f,
                    _ => board.outletContact.IsConducting
                });
                Check(repairedContact && !board.Qualifies, "Repair changed the exact native device and requires fresh trials.");
                await Requalify(true);
                Check(board.Repaired == fault + 1 && board.Qualifies,
                    $"Repair {fault + 1} earns credit only after new measurement and replay.");
            }

            await Stage(5);
            await Wait(() => board.Fresh && board.Inhibited, "U3 ready with both radiators inhibited at idle");
            Click("Test U3 timer");
            await Wait(() => board.WatchdogObserved && board.WatchdogActive && board.Voltage(board.jumper.to) > 4.7f,
                "held-high feedback expires");
            Check(!board.Fresh && !board.StartProtected() && board.Inhibited,
                "A static high on W465 cannot satisfy the independent U3 watchdog.");
            await Wait(() => !board.WatchdogActive && board.Fresh, "live readiness transitions after watchdog check");
            Click("Protected trial");
            await Wait(() => board.ProtectedBusy && board.ProtectedHeatSamples >= 3, "actual protected cold-start heater load");
            Log("PASS Native survival heater power warms the cold-start model.");
            await Wait(() => board.ProtectedBusy && board.ProtectedBackupGates > 0 && !board.Inhibited &&
                board.RadiatorCurrent > .021f && board.PumpCurrent(1) > .035f &&
                board.radiatorPins.All(pin => board.Voltage(pin) > 4.7f), "live backup flow and both radiator outputs");
            Log("PASS Both native radiator outputs are energized during loop B takeover before opening W465.");
            Check(board.OpenW465(), "The native circuit opens W465 during active radiator operation.");
            Check(!board.JumperPresent && !board.jumperVisual.activeSelf && !board.ProtectedBusy && board.Inhibited &&
                board.UndeliveredAtTrip > 0 && board.RadiatorCurrent < .001f && board.radiatorPins.All(pin => board.Voltage(pin) < .5f) &&
                board.Voltage(board.monitorSupply) > 4.7f && board.Voltage(board.telemetryPin) > 4.7f &&
                board.Voltage(board.heaterSupply) > 4.7f && board.Voltage(board.heaterBackup) > 4.7f &&
                board.PumpCurrent(1) > .035f && board.sensorPins.All(pin => board.Voltage(pin) > 4.7f) &&
                board.Voltage(board.jumper.to) < .5f && manager.SatisfiedTargetCount == 5 && !board.ResetSafety(),
                $"W465 cut both radiators with {board.UndeliveredAtTrip} samples pending. Backup cooling, survival rail and sensing stayed live.");
            await Screenshot("Level127W465Open");
            Check(board.RestoreW465() && board.jumperVisual.activeSelf && board.Inhibited && !board.ResetSafety() && !board.StartProtected(),
                "Reconnection alone cannot bypass U3 or restore stale evidence.");
            await Requalify(false);
            await Wait(() => board.Fresh, "fresh post-repair U3 heartbeat");
            Check(board.Inhibited && !board.StartProtected() && board.ResetSafety() && board.Inhibited && board.StartProtected(),
                "An explicit U3 reset is required. Both radiators stay inhibited until a fresh command is authorized.");
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && board.JumperPresent && board.ProtectedDelivered == 64 &&
                board.Inhibited && board.RadiatorCurrent < .001f && board.ProtectedBackupGates == 16 && board.ProtectedHeatSamples >= 3 &&
                board.Voltage(board.heaterSupply) > 4.7f,
                "Campaign completed 6/6 after 64/64 verified gate decisions, including 16 protected backup-loop samples.");
            Click("Thermal map");
            Click("Angle view");
            await Screenshot("Level127Inspection");
        }

        private static void Commission()
        {
            bool measured = board.MeasureBench();
            Check(measured && board.Measured && board.SensorCount == 12 &&
                board.MeasuredPumpA is > .045f and < .055f && board.MeasuredPumpB is > .036f and < .044f &&
                board.ReferenceGain is > .96f and < 1.04f,
                "Measured both coolant-loop currents and all twelve sensors. " + board.Evidence);
            Check(board.Calibrate() && board.Calibrated, "Both supervisors and survival heat commission on native circuit evidence.");
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
            Check(board.RunThermal(), "Deterministic 64-sample endurance trial started.");
            await Wait(() => !board.Running, "finished endurance trial");
        }

        private static async Task Screenshot(string name)
        {
            await Task.Delay(188);
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
                    string readings = $"Rails {board.RailsReady}, measured {board.Measured}, sensors {board.SensorCount}/12, pump A {board.PumpCurrent(0):F3} A, samples {board.Accepted}/{board.Sent}";
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
