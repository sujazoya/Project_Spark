using System;
using System.IO;
using System.Globalization;
using UnityEngine.EventSystems;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.AxisForge;
using SparkLevel120Process = ProjectSpark.Gameplay.AxisForge.SparkLevel120Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.AxisForge
{
    [InitializeOnLoad]
    internal static class Level120Acceptance
    {
        private const string Request = "Logs/Level120Acceptance.request";
        private const string Report = "Logs/Level120Acceptance.txt";
        private const string Session = "ProjectSpark.Level120.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel120Process board;

        static Level120Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/120/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE AXIS FORGE / RAX28-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel120Builder.Prefab);
                Check(prefab && prefab.LevelDefinition && prefab.SceneBindings, "Serialized campaign prefab resolves.");
                var menu = AssetDatabase.LoadAssetAtPath<SceneAsset>(SparkLevel120Builder.Scene);
                Check(menu, "The campaign startup scene resolves.");
                var sourceScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (sourceScene.path != SparkLevel120Builder.Scene)
                {
                    Check(sourceScene.path == SparkLevel120Builder.PreviewScene && !sourceScene.isDirty &&
                        UnityEngine.SceneManagement.SceneManager.sceneCount == 1, "The saved Level 120 preview is ready to leave for campaign testing.");
                    EditorSceneManager.OpenScene(SparkLevel120Builder.Scene, OpenSceneMode.Single);
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
                        PlayerPrefs.SetString(SparkLevel120Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel120Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel120Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel120Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel120Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel120Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).Single();
            Check(index == levels.arraySize - 1 && index >= 0 && manager.LevelProgression &&
                manager.LevelProgression.CanPlayLevel(index) && manager.StartLevel(index),
                "MainMenu starts The Axis Forge at the appended campaign index.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.JumperPresent &&
                !board.Powered && board.Inhibited && board.parts.All(part => !part.model.activeSelf) &&
                manager.SatisfiedTargetCount == 0, "Starts empty at 0/6 with W353 open and all actuators inhibited.");
            Check(board.parts.Length == 264 && board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 704 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 32 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 132,
                "264 footprints, 352 distinct native route pairs, 132 barrels and 32 layers exist in the loaded prefab.");
            var axes = board.drivePins.Concat(board.encoderPins).Concat(board.thermalPins).ToArray();
            Check(board.jumper.from.name == "U47.TRUSTED_SIX_AXIS_READY_OUT" &&
                board.jumper.to.name == "U48.TRUSTED_SIX_AXIS_READY_IN" &&
                axes.Length == 18 && axes.Distinct().Count() == 18 && board.stoA != board.stoB,
                "W353, eighteen axis witness terminals and two STO paths have separate native bindings.");

            var poses = File.ReadLines("Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_120_Layout/RAX28/Data/ComponentPlacements.csv").Skip(1)
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
            Check(Mathf.Abs(bounds.size.x - 10.13f) < .10f && Mathf.Abs(bounds.size.z - 7.4027f) < .09f &&
                Mathf.Abs(bounds.center.x - center.x) < .04f && Mathf.Abs(bounds.center.z - center.z) < .04f,
                $"All original placements align and the board fits {bounds.size.x:F2} × {bounds.size.z:F2} on {size.x:F2} × {size.z:F2}.");
            var toBoard = board.fixture.worldToLocalMatrix;
            for (int axis = 0; axis < 6; axis++)
            {
                var sink = board.assemblyArt.transform.Find($"L120_Axis{(char)('A' + axis)}_Heatsink");
                var origin = SparkEquipmentWorkbenchFit.MeasureArt(sink, toBoard).center;
                if (Mathf.Abs(origin.x - .101f) > .001f || Mathf.Abs(origin.z - (.150f - axis * .060f)) > .001f)
                    throw new InvalidOperationException("Source heatsink moved: " + sink.name + " " + origin);
            }
            Log("PASS All six heatsinks align at the original right-side centers.");
            Check(board.fixture.GetComponentsInChildren<TextMesh>(true)
                .Any(label => label.text == "RAX-28  /  THE AXIS FORGE"), "The visible board label matches RAX-28.");
            Check(!board.fixture.Find("Assembled visual reference (inactive)").gameObject.activeSelf,
                "The assembled reference remains inactive and cannot award placement evidence.");
            await Screenshot("Level120Opening");

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
            await Wait(() => board.Installed == 1 && PlayerPrefs.HasKey(SparkLevel120Process.CheckpointKey), "saved first part");
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
            for (int layer = 1; layer <= SparkLevel120Process.LayerCount; layer++)
            {
                board.SetLayer(layer);
                var button = board.GetComponentsInChildren<Button>().Single(item => item.name == "Layer reference");
                button.onClick.Invoke();
                if (!board.referenceLayers.gameObject.activeSelf || board.referenceLayers.Cast<Transform>()
                    .Single(item => item.gameObject.activeSelf).name != $"L{layer:00}_Reference")
                    throw new InvalidOperationException("Layer reference mismatch: L" + layer);
            }
            Log("PASS All 32 inspection layers display the original matching reference.");
            board.SelectRoute(0);
            Check(!board.ConnectPads() && board.PlateVia(), "A route requiring a barrel remains blocked until plated.");
            board.PadView();
            var start = Camera.main.WorldToScreenPoint(first.from.transform.position);
            var end = Camera.main.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(end), "Native Wire tool connects the selected copper pair.");
            await Wait(() => board.Routed == 1 && board.Plated == 1, "first route and plated span");
            await Screenshot("Level120Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                board.SelectRoute(i);
                if (board.routes[i].barrel && !board.PlateVia() || !board.ConnectPads())
                    throw new InvalidOperationException("Copper failed at " + board.routes[i].id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 352 && board.Plated == 132 && !board.TogglePower(),
                "Completed copper cannot bridge or replace the independent W353 edge.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "Initial W353 edge is inserted in the native graph.");
            await Stage(2);
            Check(board.witnesses.Length == 16 && board.witnesses.All(pin => board.Voltage(pin) > 11.4f) &&
                !board.BeginPrecharge() && !board.HomeAxes(),
                "All 16 domain returns solve and unpowered commissioning is rejected.");

            await Commission();
            await Stage(3);
            Check(!board.RunTrajectory() && board.TestEnvelope() && board.WorkspaceProof && !board.TorqueOn,
                "A reachable target below the virtual floor is rejected without applying torque.");
            await RunPath();
            await Stage(4);
            Check(board.Qualifies && Enumerable.Range(0, 6).All(axis => board.Travel(axis) > 12f) &&
                board.CartesianSamples >= 200 && board.IkResidualMm < 1.2f && board.IkOrientationDeg < .25f &&
                board.ToolErrorMm < 28f && board.PeakError < 3.2f && board.MinimumBus > 10.6f &&
                board.PeakCurrent < 8.4f && board.BrakeEnergy > .012f && board.RegenEnergy > board.BrakeEnergy,
                "Six bounded joint motions and Cartesian FK/IK round-trip meet the training limits. " + board.Evidence);
            await Screenshot("Level120TrajectoryProof");

            await Task.Delay(650);
            var commissioned = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Campaign reloads the commissioned board for evidence freshness.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) && board != commissioned &&
                board.GetComponentInChildren<Canvas>(), "commissioned checkpoint reloaded");
            Click("Resume");
            await Stage(4);
            Check(!board.Powered && !board.Qualifies && !board.BothStoTested && !board.Homed && board.Inhibited &&
                board.Routed == 352 && board.Plated == 132, "Checkpoint retains copper and milestone history while expiring live authorization.");
            await CommissionAndProve();

            int[] sites = { board.gatePart, board.encoderPart, board.thermalPart };
            var expected = new[] { SparkLevel120Process.AxisFailure.PhaseB,
                SparkLevel120Process.AxisFailure.EncoderD, SparkLevel120Process.AxisFailure.ThermalF };
            for (int fault = 0; fault < expected.Length; fault++)
            {
                Check(board.BeginFaultTrial() && !board.ProbeFault(), "Fault credit requires fresh observed evidence.");
                await RunPath();
                Check(board.Failure == expected[fault] && !board.Qualifies && board.ProbeFault(),
                    $"Fault {fault + 1} produces an independent signature. {board.Evidence}");
                if (fault == 0)
                    Check(board.PhaseClips > 80 && board.PhaseImbalance > .03f && board.Voltage(board.phaseB) < .5f &&
                        board.drivePins.All(pin => board.Voltage(pin) > 11f),
                        "Only the axis C B-high contact is open while all six base drive rails remain present.");
                if (fault == 1)
                    Check(board.EncoderMisses > 5 && board.EncoderDisagreement > .75f && !board.TorqueOn &&
                        board.Voltage(board.resolverWitness) > 11f && board.Voltage(board.monitorSupply) > 11f,
                        "Encoder D staleness inhibits all joint torque with the independent position reference and diagnostics live.");
                if (fault == 2)
                {
                    Check(board.DerateSeconds > .7f && board.Voltage(board.drivePins[5]) > 11f,
                        "Axis F thermal feedback dropouts derate the actuator with its drive rail retained.");
                    await Screenshot("Level120ThermalFault");
                }
                Check(!board.Repair() && board.TogglePower(), "Repair requires an isolated shared DC link.");
                await Wait(() => board.Bus < .55f && board.Speed < 1.9f, "discharged link and stopped joints");
                board.SelectFootprint(0);
                for (int spare = 0; spare <= fault; spare++) board.SelectSpare();
                Check(!board.Repair(), "Wrong footprint is rejected even when the spare is correct.");
                if (fault == 1)
                    Check(!board.InspectEncoderVia() && board.InspectEncoderVia(),
                        "The encoder repair requires the original training span on its entry layer.");
                var targetPart = board.parts[sites[fault]];
                var footprint = targetPart.footprint.GetComponent<SparkLevel120Footprint>();
                Check(footprint && targetPart.footprint.GetComponent<Collider>(), "The repair target has its own board collider.");
                ExecuteEvents.Execute<IPointerClickHandler>(footprint.gameObject, new PointerEventData(EventSystem.current)
                    { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
                Check(board.Repair() && !board.Qualifies, "The replacement still needs fresh commissioning and a qualifying path.");
                await CommissionAndProve();
                Check(board.Repaired == fault + 1 && board.Qualifies,
                    $"Repair {fault + 1} earns credit after new six-axis proof. {board.Evidence}");
            }

            await Stage(5);
            await Wait(() => board.Fresh && !board.Inhibited, "U48 permit before W353 disconnect");
            Check(board.StartProtectedMotion(), "A solved U48 permit starts protected motion.");
            await Wait(() => board.TorqueOn && board.Speed > 18f, "moving protected joints");
            Check(board.OpenW353(), "The native registry opens W353 while the protected path is in motion.");
            Check(!board.JumperPresent && board.Tripped && !board.TorqueOn && board.TripSpeed > 10f &&
                board.Inhibited && board.Voltage(board.monitorSupply) > 11f && board.Voltage(board.mainRail) > 11f &&
                board.Voltage(board.jumper.to) < .5f && board.Bus > 10.5f && board.StoA && board.StoB &&
                board.drivePins.All(pin => board.Voltage(pin) > 11f) && manager.SatisfiedTargetCount == 5,
                $"W353 inhibited all joint torque at {board.TripSpeed:F1} deg/s with the shared link and independent diagnostic rails retained.");
            await Screenshot("Level120W353Open");
            Check(board.RestoreW353() && board.Inhibited && !board.ResetSafety() && !board.StartProtectedMotion(),
                "Reconnection alone cannot bypass U48.");
            await Wait(() => board.Speed < 4.8f, "joint settling after W353");
            Check(board.ProveSto(0) && board.ProveSto(1) && board.HomeAxes(), "W353 recovery starts fresh independent STO and homing.");
            await Wait(() => !board.Running && board.Homed, "fresh homing after W353");
            Check(board.TestEnvelope(), "Recovery includes the workspace boundary check.");
            await RunPath();
            await Wait(() => board.Fresh, "fresh post-repair U48 heartbeat");
            Check(board.Inhibited && board.ResetSafety() && !board.Inhibited && board.StartProtectedMotion(),
                "Explicit U48 reset releases the freshly qualified permit.");
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && board.JumperPresent &&
                board.ProtectedToolError is > 0 and < 28f && !board.TorqueOn,
                $"Campaign manager completed 6/6 after a fresh protected path at {board.ProtectedToolError:F1} mm peak tool error.");
            Click("Angle view");
            await Screenshot("Level120Inspection");
        }

        private static async Task Commission()
        {
            Check(board.TogglePower() && board.ChassisReady && !board.CloseContactor() && !board.HomeAxes() &&
                board.BeginPrecharge(), "Native rail voltage does not bypass precharge or commissioning.");
            await Wait(() => board.Bus >= 11.2f && board.CloseContactor(), "settled shared DC link");
            Check(board.ReadyAxes == 6 && !board.HomeAxes() && board.ProveSto(0) && !board.BothStoTested &&
                board.ProveSto(1) && board.BothStoTested && board.HomeAxes(),
                "Each STO path independently drops and restores before six-axis homing.");
            await Wait(() => !board.Running && board.Homed, "six measured home references");
        }

        private static async Task CommissionAndProve()
        {
            await Commission();
            Check(board.TestEnvelope(), "Forbidden target rejected with all joints stationary.");
            await RunPath();
        }

        private static async Task RunPath()
        {
            Check(board.RunTrajectory(), "Coordinated service path started.");
            await Wait(() => !board.Running, "completed service motion");
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
                    string readings = $"Bus {board.Bus:F3} V, STO A {board.StoA}, STO B {board.StoB}, joint speed {board.Speed:F1} deg/s";
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
