using System;
using System.IO;
using System.Globalization;
using UnityEngine.EventSystems;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.MotionCitadel;
using SparkLevel119Process = ProjectSpark.Gameplay.MotionCitadel.SparkLevel119Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.MotionCitadel
{
    [InitializeOnLoad]
    internal static class Level119Acceptance
    {
        private const string Request = "Logs/Level119Acceptance.request";
        private const string Report = "Logs/Level119Acceptance.txt";
        private const string Session = "ProjectSpark.Level119.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel119Process board;

        static Level119Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/119/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE MOTION CITADEL / MCX27-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel119Builder.Prefab);
                Check(prefab && prefab.LevelDefinition && prefab.SceneBindings, "Serialized campaign prefab resolves.");
                var menu = AssetDatabase.LoadAssetAtPath<SceneAsset>(SparkLevel119Builder.Scene);
                Check(menu, "The campaign startup scene resolves.");
                var sourceScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (sourceScene.path != SparkLevel119Builder.Scene)
                {
                    Check(sourceScene.path == SparkLevel119Builder.PreviewScene && !sourceScene.isDirty &&
                        UnityEngine.SceneManagement.SceneManager.sceneCount == 1, "The saved Level 119 preview is ready to leave for campaign testing.");
                    EditorSceneManager.OpenScene(SparkLevel119Builder.Scene, OpenSceneMode.Single);
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
                        PlayerPrefs.SetString(SparkLevel119Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel119Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel119Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel119Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel119Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel119Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).Single();
            Check(index == levels.arraySize - 1 && index >= 0 && manager.LevelProgression &&
                manager.LevelProgression.CanPlayLevel(index) && manager.StartLevel(index),
                "MainMenu starts The Motion Citadel at the appended campaign index.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.JumperPresent &&
                !board.Powered && board.Inhibited && board.parts.All(part => !part.model.activeSelf) &&
                manager.SatisfiedTargetCount == 0, "Starts empty at 0/6 with W337 open and PWM inhibited.");
            Check(board.parts.Length == 248 && board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 672 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 30 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 124,
                "248 footprints, 336 distinct native route pairs, 124 barrels and 30 layers exist in the loaded prefab.");
            Check(board.jumper.from.name == "U39.TRUSTED_MOTION_READY_OUT" &&
                board.jumper.to.name == "U40.TRUSTED_MOTION_READY_IN" && board.phaseU != board.phaseV &&
                board.phaseV != board.phaseW && board.phaseW != board.phaseU && board.stoA != board.stoB,
                "W337, three phase witnesses and two STO contacts bind to separate authoritative terminals.");

            var poses = File.ReadLines("Assets/ProjectSpark/Level119/MCX27/Data/ComponentPlacements.csv").Skip(1)
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
            Check(Mathf.Abs(bounds.size.x - 10.13f) < .10f && Mathf.Abs(bounds.size.z - 7.2936f) < .09f &&
                Mathf.Abs(bounds.center.x - center.x) < .04f && Mathf.Abs(bounds.center.z - center.z) < .04f,
                $"All original placements align and the board fits {bounds.size.x:F2} × {bounds.size.z:F2} on {size.x:F2} × {size.z:F2}.");
            var left = board.assemblyArt.transform.Find("L119_PowerStage_HeatsinkLeft");
            var right = board.assemblyArt.transform.Find("L119_PowerStage_HeatsinkRight");
            var toBoard = board.fixture.worldToLocalMatrix;
            var leftCenter = SparkEquipmentWorkbenchFit.MeasureArt(left, toBoard).center;
            var rightCenter = SparkEquipmentWorkbenchFit.MeasureArt(right, toBoard).center;
            Check(Mathf.Abs(leftCenter.x + .196f) < .001f && Mathf.Abs(rightCenter.x + .130f) < .001f &&
                Mathf.Abs(leftCenter.z + .132f) < .001f && Mathf.Abs(rightCenter.z + .132f) < .001f,
                $"Both source heat sinks align in board coordinates ({leftCenter.x:F3} m and {rightCenter.x:F3} m).");
            Check(!board.fixture.Find("Assembled visual reference (inactive)").gameObject.activeSelf,
                "The assembled reference remains inactive and cannot award placement evidence.");
            await Screenshot("Level119Opening");

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
            await Wait(() => board.Installed == 1 && PlayerPrefs.HasKey(SparkLevel119Process.CheckpointKey), "saved first part");
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
            for (int layer = 1; layer <= SparkLevel119Process.LayerCount; layer++)
            {
                board.SetLayer(layer);
                var button = board.GetComponentsInChildren<Button>().Single(item => item.name == "Layer reference");
                button.onClick.Invoke();
                if (!board.referenceLayers.gameObject.activeSelf || board.referenceLayers.Cast<Transform>()
                    .Single(item => item.gameObject.activeSelf).name != $"L{layer:00}_Reference")
                    throw new InvalidOperationException("Layer reference mismatch: L" + layer);
            }
            Log("PASS All 30 inspection layers display the original matching reference.");
            board.SelectRoute(0);
            Check(!board.ConnectPads() && board.PlateVia(), "A route requiring a barrel remains blocked until plated.");
            board.PadView();
            var start = Camera.main.WorldToScreenPoint(first.from.transform.position);
            var end = Camera.main.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(end), "Native Wire tool connects the selected copper pair.");
            await Wait(() => board.Routed == 1 && board.Plated == 1, "first route and plated span");
            await Screenshot("Level119Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                board.SelectRoute(i);
                if (board.routes[i].barrel && !board.PlateVia() || !board.ConnectPads())
                    throw new InvalidOperationException("Copper failed at " + board.routes[i].id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 336 && board.Plated == 124 && !board.TogglePower(),
                "Completed copper cannot bridge or replace the independent W337 edge.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "Initial W337 edge is inserted in the native graph.");
            await Stage(2);
            Check(board.witnesses.Length == 15 && board.witnesses.All(pin => board.Voltage(pin) > 11.4f) &&
                !board.BeginPrecharge() && !board.CloseContactor(), "All 15 domain returns solve and unpowered commissioning is rejected.");

            await PowerUp();
            await Stage(3);
            Click("Loop tuning");
            await RunProfile();
            await Stage(4);
            Check(board.Qualifies && board.Energy > .28f && board.RegenEnergy >= board.Energy &&
                board.PeakBus < 14.4f && board.CruiseRpm is > 420f and < 535f && board.CruiseTorque is > .08f and < .26f && board.PeakCurrent < 4.7f &&
                Mathf.Abs(board.RotorRpm) < 48f && board.PhaseSpread < .38f, "Healthy speed, phase balance, braking and current limits qualify. " + board.Evidence);
            await Screenshot("Level119ServoProof");

            await Task.Delay(650);
            var commissioned = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Campaign reloads the commissioned board for evidence freshness.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) && board != commissioned &&
                board.GetComponentInChildren<Canvas>(), "commissioned checkpoint reloaded");
            Click("Resume");
            await Stage(4);
            Check(!board.Powered && !board.Qualifies && !board.BothStoTested && board.Inhibited &&
                board.Routed == 336 && board.Plated == 124, "Checkpoint restores history and copper with expired operating evidence.");
            await PowerUp();
            await RunProfile();

            int[] faultRefs = { board.gatePart, board.encoderPart, board.brakePart };
            var expected = new[] { SparkLevel119Process.MotorFailure.PhaseV,
                SparkLevel119Process.MotorFailure.EncoderVia, SparkLevel119Process.MotorFailure.BrakeSense };
            for (int fault = 0; fault < expected.Length; fault++)
            {
                Check(board.BeginFaultTrial() && !board.ProbeFault(), "Fault evidence must be collected after staging.");
                await RunProfile();
                Check(board.Failure == expected[fault] && !board.Qualifies && board.ProbeFault(),
                    $"Fault {fault + 1} produces its own measured signature. {board.Evidence}");
                if (fault == 0)
                    Check(board.ReducedPhaseSteps > 500 && board.Voltage(board.phaseU) > 11f && board.Voltage(board.phaseV) < .5f &&
                        board.Voltage(board.phaseW) > 11f, "Only the modeled phase-V high-side path is open.");
                if (fault == 1)
                    Check(board.EncoderDropouts > 100 && board.EncoderError > 3f && board.Voltage(board.resolverWitness) > 11f,
                        "Encoder intermittency leaves independent resolver evidence live.");
                if (fault == 2)
                    Check(board.PeakBus >= 14.6f && board.Energy < .2f && !board.Pwm && board.Voltage(board.monitorSupply) > 11f,
                        "Brake under-read causes modeled overvoltage with PWM inhibited and diagnostics retained.");
                if (fault == 2) await Screenshot("Level119BrakeFault");
                Check(!board.Repair() && board.TogglePower(), "Repairs require a bench isolated from power.");
                await Wait(() => board.Bus < .55f && Mathf.Abs(board.RotorRpm) < 17f, "DC-link discharge and rotor standstill");
                board.SelectFootprint(0);
                for (int kit = 0; kit <= fault; kit++) board.SelectSpare();
                Check(!board.Repair(), "A valid repair kit at the wrong footprint is rejected.");
                if (fault == 1)
                {
                    Check(!board.InspectEncoderVia() && board.InspectEncoderVia(),
                        "Intermittent encoder contact requires its original training via entry layer.");
                }
                var targetPart = board.parts[faultRefs[fault]];
                var footprint = targetPart.footprint.GetComponent<SparkLevel119Footprint>();
                Check(footprint && targetPart.footprint.GetComponent<Collider>(), "Repair target has its own board collider.");
                ExecuteEvents.Execute<IPointerClickHandler>(footprint.gameObject, new PointerEventData(EventSystem.current)
                    { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
                Check(board.Repair() && !board.Qualifies, "Repair closes the fault transaction and awaits new measurements.");
                await PowerUp();
                await RunProfile();
                Check(board.Repaired == fault + 1 && board.Qualifies,
                    $"Repair {fault + 1} qualifies only after a fresh full profile. {board.Evidence}");
            }
            await Stage(5);
            await Wait(() => board.Fresh && !board.Inhibited, "native U40 torque permit before W337 interruption");
            Check(board.StartProtectedMotion(), "Protected motion starts only with active U40 permission.");
            await Wait(() => board.Pwm && board.RotorRpm > 180f, "live protected motion");
            Check(board.OpenW337(), "W337 opens through native wire removal during protected motion.");
            Check(board.Tripped && !board.JumperPresent && board.TripSpeedRpm > 150f && !board.Pwm && board.Torque == 0f &&
                board.Inhibited && board.Voltage(board.monitorSupply) > 11f && board.Voltage(board.mainRail) > 11f &&
                board.Voltage(board.jumper.to) < .5f && board.Bus > 10.5f && board.StoA && board.StoB,
                $"W337 cut PWM and torque at {board.TripSpeedRpm:F0} rpm with bus, diagnostics and independent STO preserved.");
            Check(manager.SatisfiedTargetCount == 5 && !board.ResetSafety(), "W337 interruption preserves 5/6 history and rejects an immediate reset.");
            await Screenshot("Level119W337Open");
            Check(board.RestoreW337() && board.Inhibited && !board.ResetSafety() && !board.StartProtectedMotion(),
                "Reconnection alone cannot authorize motion.");
            await Wait(() => Mathf.Abs(board.RotorRpm) < 17f, "rotor stops before W337 requalification");
            Check(board.ProveSto(0) && board.ProveSto(1) && board.AlignEncoder(), "W337 recovery collects fresh independent STO and alignment.");
            await RunProfile();
            await Wait(() => board.Fresh, "fresh W337 heartbeat after recovery evidence");
            Check(board.Inhibited && board.ResetSafety() && !board.Inhibited, "Explicit U40 reset releases the otherwise fresh inhibited permit.");
            Check(board.StartProtectedMotion(), "Protected requalification profile can now run.");
            await Stage(6);
            Check(manager.IsCompleted && board.Energy > .28f && !board.Pwm && board.Repaired == 3 && board.JumperPresent,
                "Native manager reached 6/6 after protected post-reset speed, braking and cooldown evidence.");
            Click("Angle view");
            await Screenshot("Level119Inspection");
        }

        private static async Task PowerUp()
        {
            Check(board.TogglePower() && board.ChassisReady && !board.CloseContactor() && !board.RunProfile() &&
                board.BeginPrecharge(), "Bench rails solve; precharge and STO verification cannot be bypassed.");
            await Wait(() => board.Bus >= 11.2f && board.CloseContactor(), "settled DC-link precharge");
            Check(!board.AlignEncoder() && board.ProveSto(0) && board.StoA && board.StoB && !board.BothStoTested &&
                board.ProveSto(1) && board.BothStoTested && board.AlignEncoder(),
                "Each STO channel independently drops and restores before encoder alignment.");
        }

        private static async Task RunProfile()
        {
            Check(board.RunProfile(), "Bounded service profile started.");
            await Wait(() => !board.Running, "completed servo and regenerative-brake profile");
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
                    string readings = $"Bus {board.Bus:F3} V, STO A {board.StoA}, STO B {board.StoB}, rotor {board.RotorRpm:F1} rpm";
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
