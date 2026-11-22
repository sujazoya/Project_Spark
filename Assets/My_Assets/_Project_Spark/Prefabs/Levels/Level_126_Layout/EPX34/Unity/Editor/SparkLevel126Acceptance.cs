using System;
using System.IO;
using System.Globalization;
using UnityEngine.EventSystems;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.EclipseCore;
using SparkLevel126Process = ProjectSpark.Gameplay.EclipseCore.SparkLevel126Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.EclipseCore
{
    [InitializeOnLoad]
    internal static class Level126Acceptance
    {
        private const string Request = "Logs/Level126Acceptance.request";
        private const string Report = "Logs/Level126Acceptance.txt";
        private const string Session = "ProjectSpark.Level126.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel126Process board;

        static Level126Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/126/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE ECLIPSE CORE / EPX34-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel126Builder.Prefab);
                Check(prefab && prefab.LevelDefinition && prefab.SceneBindings, "Serialized campaign prefab resolves.");
                var menu = AssetDatabase.LoadAssetAtPath<SceneAsset>(SparkLevel126Builder.Scene);
                Check(menu, "The campaign startup scene resolves.");
                var sourceScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (sourceScene.path != SparkLevel126Builder.Scene)
                {
                    Check(sourceScene.path == SparkLevel126Builder.PreviewScene && !sourceScene.isDirty &&
                        UnityEngine.SceneManagement.SceneManager.sceneCount == 1, "The saved Level 126 preview is ready to leave for campaign testing.");
                    EditorSceneManager.OpenScene(SparkLevel126Builder.Scene, OpenSceneMode.Single);
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
                        PlayerPrefs.SetString(SparkLevel126Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel126Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel126Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel126Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel126Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel126Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).Single();
            Check(index == levels.arraySize - 1 && index >= 0 && manager.LevelProgression &&
                manager.LevelProgression.CanPlayLevel(index) && manager.StartLevel(index),
                "MainMenu starts The Eclipse Core at the appended campaign index.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.JumperPresent &&
                !board.Powered && board.Inhibited && board.parts.All(part => !part.model.activeSelf) &&
                manager.SatisfiedTargetCount == 0, "Starts empty at 0/6 with W449 open and the protected output inhibited.");
            Check(board.parts.Length == 360 && board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 896 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 44 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 180,
                "360 footprints, 448 distinct native route pairs, 180 barrels and 44 layers exist in the loaded prefab.");
            Check(board.jumper.from.name == "U6.TRUSTED_ENERGY_READY_OUT" &&
                board.jumper.to.name == "U3.TRUSTED_ENERGY_READY_IN" &&
                board.solarPins.Distinct().Count() == 2 && board.essentialPin != board.protectedFeed &&
                board.testAuxPin != board.protectedFeed && board.chargePin != board.dischargePin,
                "W449, solar inputs, battery converters, essential rail and the auxiliary gate use separate native terminals.");

            var poses = File.ReadLines("Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_126_Layout/EPX34/Data/ComponentPlacements.csv").Skip(1)
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
            Check(Mathf.Abs(bounds.size.x - 10.13f) < .10f && Mathf.Abs(bounds.size.z - 7.91406f) < .09f &&
                Mathf.Abs(bounds.center.x - center.x) < .04f && Mathf.Abs(bounds.center.z - center.z) < .04f,
                $"All original placements align and the board fits {bounds.size.x:F2} × {bounds.size.z:F2} on {size.x:F2} × {size.z:F2}.");
            var toBoard = board.fixture.worldToLocalMatrix;
            var structural = File.ReadLines(SparkLevel126Builder.Folder + "/EPX34/Data/ModelManifest.csv").Skip(1)
                .Select(row => row.Split(',')).Where(row => row[2] is "heatsink" or "busbar").ToArray();
            Check(structural.Length == 9, "Six heat spreaders and three busbars resolve to their authored model records.");
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
                .Any(label => label.text == "EPX-34  /  THE ECLIPSE CORE"), "The visible board label matches EPX-34.");
            Check(!board.fixture.Find("Assembled visual reference (inactive)").gameObject.activeSelf,
                "The assembled reference remains inactive and cannot award placement evidence.");
            await Screenshot("Level126Opening");

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
            await Wait(() => board.Installed == 1 && PlayerPrefs.HasKey(SparkLevel126Process.CheckpointKey), "saved first part");
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
            for (int layer = 1; layer <= SparkLevel126Process.LayerCount; layer++)
            {
                board.SetLayer(layer);
                var button = board.GetComponentsInChildren<Button>().Single(item => item.name == "Layer reference");
                button.onClick.Invoke();
                if (!board.referenceLayers.gameObject.activeSelf || board.referenceLayers.Cast<Transform>()
                    .Single(item => item.gameObject.activeSelf).name != $"L{layer:00}_Reference")
                    throw new InvalidOperationException("Layer reference mismatch: L" + layer);
            }
            Log("PASS All 44 inspection layers display the original matching reference.");
            board.SelectRoute(0);
            Check(!board.ConnectPads() && board.PlateVia(), "A route requiring a barrel remains blocked until plated.");
            board.PadView();
            var start = Camera.main.WorldToScreenPoint(first.from.transform.position);
            var end = Camera.main.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(end), "Native Wire tool connects the selected copper pair.");
            await Wait(() => board.Routed == 1 && board.Plated == 1, "first route and plated span");
            await Screenshot("Level126Copper");
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
            Check(board.Routed == 448 && board.Plated == 180 && !board.TogglePower(),
                "Completed copper cannot bridge or replace the independent W449 edge.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "Initial W449 edge is inserted in the native graph.");
            await Stage(2);
            Check(board.witnesses.Length == 8 && board.witnesses.All(pin => board.Voltage(pin) > 4.7f) && !board.Calibrate(),
                "All eight circuit domains solve. Unpowered calibration is rejected.");

            Check(board.TogglePower() && board.RailsReady && !board.Calibrate() && !board.RunEnergy(),
                "Powered rails do not bypass native load-step measurement or converter commissioning.");
            Commission();
            await Stage(3);
            Check(!board.RunEnergy() && board.CheckGuardRejection(),
                "Capacity, reserve, unserved load and bad-data guards each reject an invalid operating state.");
            await Replay();
            await Stage(4);
            Check(board.Qualifies && board.Accepted == 64 && board.ShedSamples == 36 && board.RecoverySamples == 22 &&
                board.EclipseEnd is > 91f and < 94f && board.FinalEnergy is > 154f and < 157f &&
                board.UnservedEssential < .8f && board.LedgerResidual < .05f && board.Inhibited,
                $"Eclipse resilience verifies reserve, load shedding, sunrise recharge and energy conservation. {board.Evidence}");
            await Screenshot("Level126EnergyProof");

            await Task.Delay(650);
            var commissioned = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Campaign reloads the commissioned board to check stale evidence.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) && board != commissioned &&
                board.GetComponentInChildren<Canvas>(), "commissioned checkpoint reloaded");
            Click("Resume");
            await Stage(4);
            Check(!board.Powered && !board.Qualifies && !board.Measured && !board.Calibrated && board.Inhibited &&
                board.Routed == 448 && board.Plated == 180, "Checkpoint retains copper and milestones and expires protected authorization.");
            await Requalify(true);

            int[] sites = { board.solarPart, board.sensePart, board.filterPart };
            var expected = new[] { SparkLevel126Process.EnergyFailure.SolarOvercurrent,
                SparkLevel126Process.EnergyFailure.SenseDrift, SparkLevel126Process.EnergyFailure.EssentialSag };
            for (int fault = 0; fault < expected.Length; fault++)
            {
                Check(board.BeginFaultTrial() && !board.ProbeFault(), "Fault diagnosis requires new observed evidence.");
                await Replay();
                Check(board.Failure == expected[fault] && !board.Qualifies && board.ProbeFault(),
                    $"Fault {fault + 1} has a distinct measured signature. {board.Evidence}");
                if (fault == 0)
                {
                    Check(board.SolarTrips == 64 && board.SolarLatched && board.PeakSolarFault > .39f &&
                        board.SolarCurrent(0) < .001f && board.SolarCurrent(1) is > .018f and < .022f &&
                        board.SenseHolds == 0 && board.SagHolds == 0 && board.Voltage(board.telemetryPin) > 4.7f,
                        "Q1 trips the native solar-A branch while solar B and telemetry stay live.");
                    await Screenshot("Level126SolarFault");
                    Check(board.TogglePower() && board.TogglePower() && board.SolarLatched &&
                        board.SolarCurrent(0) < .001f && !board.Calibrate(),
                        "Cycling power cannot clear the unresolved solar-A latch.");
                }
                if (fault == 1)
                    Check(board.SenseHolds == 64 && board.MeterError is > 45f and < 50f && board.SenseGain is > 1.46f and < 1.48f &&
                        board.LedgerResidual < .05f && board.SolarTrips == 0 && board.SagHolds == 0,
                        "R1 causes native signed-current calibration drift while the independent energy ledger remains conserved.");
                if (fault == 2)
                    Check(board.SagHolds == 64 && board.MinimumLoadedBus is > 3.75f and < 3.9f &&
                        board.PeakSag > .27f && board.UnservedEssential > 35f &&
                        board.Voltage(board.rail) > 4.7f && board.SolarTrips == 0 && board.SenseHolds == 0,
                        "C1 produces a native load-step dip and lost essential energy with the upstream converter still live.");

                Check(!board.Repair() && board.TogglePower(), "The selected repair is blocked until the bench is isolated.");
                board.SelectFootprint(0);
                for (int spare = 0; spare <= fault; spare++) board.SelectSpare();
                Check(!board.Repair(), "A correct spare on the wrong footprint is rejected.");
                var targetPart = board.parts[sites[fault]];
                var footprint = targetPart.footprint.GetComponent<SparkLevel126Footprint>();
                Check(footprint && targetPart.footprint.GetComponent<Collider>(), "Repair site has its own board collider.");
                ExecuteEvents.Execute<IPointerClickHandler>(footprint.gameObject, new PointerEventData(EventSystem.current)
                    { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
                bool repairedContact = board.Repair() && (fault switch
                {
                    0 => Mathf.Abs(board.solarLoads[0].Resistance - 250f) < .01f,
                    1 => Mathf.Abs(board.batterySense.Resistance - 100f) < .01f,
                    _ => Mathf.Abs(board.filterSeries.Resistance - .12f) < .001f
                });
                Check(repairedContact && !board.Qualifies, "Repair changed the exact native device and requires fresh trials.");
                await Requalify(true);
                Check(board.Repaired == fault + 1 && board.Qualifies && !board.SolarLatched,
                    $"Repair {fault + 1} earns credit only after new measurement and replay.");
            }

            await Stage(5);
            await Wait(() => board.Fresh && board.Inhibited, "U3 ready with auxiliary inhibited at idle");
            Click("Test U3 timer");
            await Wait(() => board.WatchdogObserved && board.WatchdogActive && board.Voltage(board.jumper.to) > 4.7f,
                "held-high feedback expires");
            Check(!board.Fresh && !board.StartProtected() && board.Inhibited,
                "A static high on W449 cannot satisfy the independent U3 watchdog.");
            await Wait(() => !board.WatchdogActive && board.Fresh, "live readiness transitions after watchdog check");
            Click("Protected trial");
            await Wait(() => board.ProtectedBusy && board.ProtectedDelivered is >= 7 and <= 42 && board.Inhibited &&
                board.SolarWatts < .5f && board.EssentialWatts > 55f, "native eclipse shedding with essential power retained");
            Log("PASS Both native solar branches are out and auxiliary power is shed while the essential load remains energized.");
            await Wait(() => board.ProtectedBusy && board.ProtectedSunriseSamples > 0 && !board.Inhibited &&
                board.ProtectedAuxWatts > 40f && board.SolarWatts > 170f, "native sunrise auxiliary load");
            Log("PASS Sunrise energizes the native protected auxiliary load before opening W449.");
            Check(board.OpenW449(), "The native circuit opens W449 while the sunrise load is active.");
            Check(!board.JumperPresent && !board.ProtectedBusy && board.Inhibited && board.UndeliveredAtTrip > 0 &&
                board.Voltage(board.monitorSupply) > 4.7f && board.Voltage(board.rail) > 4.7f &&
                board.Voltage(board.telemetryPin) > 4.7f && board.Voltage(board.dischargePin) > 4.7f &&
                board.EssentialWatts > 55f && board.ProtectedAuxWatts < .5f &&
                board.Voltage(board.jumper.to) < .5f && manager.SatisfiedTargetCount == 5 && !board.ResetSafety(),
                $"W449 inhibited auxiliary power with {board.UndeliveredAtTrip} samples pending. Essential load and telemetry stayed live. U3 refused reset while open.");
            await Screenshot("Level126W449Open");
            Check(board.RestoreW449() && board.Inhibited && !board.ResetSafety() && !board.StartProtected(),
                "Reconnection alone cannot bypass U3 or restore stale evidence.");
            await Requalify(false);
            await Wait(() => board.Fresh, "fresh post-repair U3 heartbeat");
            Check(board.Inhibited && !board.StartProtected() && board.ResetSafety() && board.Inhibited && board.StartProtected(),
                "An explicit U3 reset is required. Auxiliary stays inhibited until a new load sample is authorized.");
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && board.JumperPresent && board.ProtectedDelivered == 64 &&
                board.Inhibited && board.ProtectedAuxWatts < .5f && board.EssentialWatts > 55f && board.ProtectedSunriseSamples == 22,
                "Campaign completed 6/6 after 64/64 verified gate decisions, including 22 protected sunrise samples.");
            Click("Power map");
            Click("Angle view");
            await Screenshot("Level126Inspection");
        }

        private static void Commission()
        {
            bool measured = board.MeasureBench();
            Check(measured && board.Measured && board.SolarWatts > 170f && board.EssentialWatts > 55f &&
                board.LoadedBus > 4.7f && board.SenseGain is > .96f and < 1.04f,
                "Measured both solar feeds and the native essential load step. " + board.Evidence);
            Check(board.Calibrate() && board.Calibrated, "Redundant converters and both supervisors commission on native circuit evidence.");
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
            Check(board.RunEnergy(), "Deterministic 64-sample eclipse trial started.");
            await Wait(() => !board.Running, "finished eclipse trial");
        }

        private static async Task Screenshot(string name)
        {
            await Task.Delay(180);
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
                    string readings = $"Rails {board.RailsReady}, measured {board.Measured}, solar {board.SolarWatts:F0} W, essential {board.EssentialWatts:F0} W, samples {board.Accepted}/{board.Sent}";
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
