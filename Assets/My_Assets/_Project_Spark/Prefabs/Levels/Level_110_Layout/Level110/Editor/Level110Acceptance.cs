using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    [InitializeOnLoad]
    internal static class Level110Acceptance
    {
        private const string Request = "Logs/Level110Acceptance.request";
        private const string Report = "Logs/Level110Acceptance.txt";
        private const string Session = "ProjectSpark.Level110.Acceptance";
        private const string Save = "ProjectSpark.Level110.Board.v1";
        private static SparkLevel110Process board;
        private static LevelGamePlayManager manager;
        private static int checks;

        static Level110Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += PlayState;
        }

        [MenuItem("Tools/Project Spark/Levels/Run Level 110 Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode
                || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, $"VOLTAGE DOMINION / PWR18-EDU-1 / Unity {Application.unityVersion}\n");
            checks = 0;
            try
            {
                var asset = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel110Builder.PrefabPath);
                Check(asset && asset.LevelDefinition.LevelId == "level_110" && asset.SceneBindings, "Generated campaign prefab and bindings resolve");
                Check(SparkLevel110Process.ProtectionBench(), "Model protection bench observes UVP, OVP, OCP and thermal cutoff");
                SessionState.SetBool(Session, true);
                EditorApplication.EnterPlaymode();
            }
            catch (Exception error) { Log("STOP " + error); }
        }

        private static void Log(string line) => File.AppendAllText(Report, line + Environment.NewLine);
        private static void Check(bool passed, string label)
        {
            if (!passed) throw new InvalidOperationException(label);
            checks++;
            Log("PASS " + label);
        }

        private static async void PlayState(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Session, false)) return;
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.EraseBool(Session);
                Log("Returned to Edit Mode.");
                return;
            }
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            var filter = Debug.unityLogger.filterLogType;
            var runInBackground = Application.runInBackground;
            string[] keys = { Save, "ProjectSpark.LevelProgress" };
            bool[] existed = keys.Select(PlayerPrefs.HasKey).ToArray();
            string[] prior = keys.Select(key => PlayerPrefs.GetString(key)).ToArray();
            try
            {
                Application.runInBackground = true;
                Debug.unityLogger.filterLogType = LogType.Error;
                await Play();
                Log("COMPLETE. Live LevelGamePlayManager confirmed 6/6.");
            }
            catch (Exception error) { Log("STOP " + error); }
            finally
            {
                for (int i = 0; i < keys.Length; i++)
                    if (existed[i]) PlayerPrefs.SetString(keys[i], prior[i]); else PlayerPrefs.DeleteKey(keys[i]);
                PlayerPrefs.Save();
                Debug.unityLogger.filterLogType = filter;
                Application.runInBackground = runInBackground;
                EditorApplication.ExitPlaymode();
            }
        }

        private static async Task Wait(Func<bool> condition, string step, int timeout = 25)
        {
            var until = DateTime.UtcNow.AddSeconds(timeout);
            while (!condition())
            {
                if (DateTime.UtcNow > until)
                    throw new TimeoutException($"{step}. Stage {board?.Stage}, manager {manager?.SatisfiedTargetCount}, "
                        + $"routes {board?.Routed}, vias {board?.Plated}, powered {board?.Powered},"
                        + $" VCORE {board?.Model.Voltage:F4}, trip {board?.Model.Trip}, manager status {manager?.LastEvaluationMessage}.");
                await Task.Delay(45);
            }
        }

        private static async Task Stage(int stage)
        {
            await Wait(() => board.Stage == stage && manager.SatisfiedTargetCount == stage, $"Manager objective {stage}/6");
            Check(true, $"Manager confirms {stage}/6");
        }

        private static async Task Commission()
        {
            await Wait(() => board.Model.Voltage < .08, "Discharge before starting");
            Check(board.Power(), "Bench input applies through the verified graph");
            await Wait(() => board.Model.PowerGood, "Four-phase startup");
            Check(board.Calibrate(), "Independent VCORE and current probes calibrate");
        }

        private static async Task Capture(bool expectQualified)
        {
            Check(board.Capture(), "Load step capture starts");
            await Wait(() => !board.Testing, "Load step capture completion");
            Check(board.HasQualifiedRun == expectQualified, expectQualified ? "Measured transient, current sharing, ripple and efficiency pass"
                : "Seeded fault is detectable in the measured load step");
        }

        private static async Task Play()
        {
            manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
            int index = Array.FindIndex(manager.Levels, level => level && level.LevelId == "level_110");
            if (index < 0)
            {
                // An already-open dirty scene can predate the saved campaign registration.
                var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel110Builder.DefinitionPath);
                string guid = AssetDatabase.AssetPathToGUID(SparkLevel110Builder.DefinitionPath);
                Check(File.ReadAllText(SparkLevel110Builder.ScenePath).Contains("guid: " + guid),
                    "The saved MainMenu scene already contains Level 110");
                var campaign = new SerializedObject(manager);
                var levels = campaign.FindProperty("levels");
                index = levels.arraySize;
                levels.arraySize++;
                levels.GetArrayElementAtIndex(index).objectReferenceValue = definition;
                campaign.ApplyModifiedPropertiesWithoutUndo();
            }
            Check(index >= 0 && manager.StartLevel(index), "Start Level 110 through normal campaign manager");
            await Wait(() => (board = Object.FindFirstObjectByType<SparkLevel110Process>()) && board.Model.Tick > 0, "Runtime start");
            Check(board.Stage == 0 && board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.Powered,
                "Empty board starts with zero credited assembly, routing, vias and power");
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable");
            var surface = table.GetComponent<BoxCollider>();
            Vector3 tableSize = Vector3.Scale(surface.size, table.lossyScale);
            float width = board.board.TransformVector(Vector3.right * .310f).magnitude;
            float depth = board.board.TransformVector(Vector3.forward * .230f).magnitude;
            Check(width < tableSize.x && depth < tableSize.z && Mathf.Abs(Mathf.Max(width / tableSize.x, depth / tableSize.z) - .88f) < .01f,
                $"Board fits the live worktable at {width:F2} × {depth:F2} scene units within {tableSize.x:F2} × {tableSize.z:F2}");
            board.completedReference.SetActive(true);
            await Task.Yield();
            Check(board.Installed == 0 && manager.SatisfiedTargetCount == 0, "Assembled visual reference gives no objective credit");
            board.completedReference.SetActive(false);
            board.SelectInventory(1);
            Check(!board.Fit(), "Wrong shelf part rejected");
            board.SelectInventory(0);
            board.RotatePart();
            Check(!board.Fit(), "Wrong orientation rejected");
            board.SelectFootprint(0);
            Check(board.Fit() && !board.Solder() && board.Installed == 0, "Fitting alone gives no solder credit");
            board.ApplyFlux();
            Check(board.Solder(), "First component solder changes native contact state");
            board.SaveBoard();
            Check(manager.RestartLevel(), "Restart through campaign manager");
            var prior = board;
            await Wait(() => (board = Object.FindFirstObjectByType<SparkLevel110Process>()) && board != prior && board.Model.Tick > 0, "Fresh layout start");
            Check(board.ResumeBoard() && board.Installed == 1 && board.Stage == 0 && !board.Powered, "Saved assembly restores without live certificates");
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                for (int i = 1; i < board.parts.Length; i++)
                {
                    board.SelectFootprint(i);
                    board.SelectInventory(i);
                    if (!board.Fit()) throw new InvalidOperationException($"Fit failed for {board.parts[i].id}");
                    board.ApplyFlux();
                    if (!board.Solder()) throw new InvalidOperationException($"Solder failed for {board.parts[i].id}");
                }
            }
            Check(board.Installed == 130 && board.parts.All(part => part.contact.IsConducting), "130 source parts have real closed solder contacts");
            Check(board.MountCooling() && board.cooling.All(piece => piece.activeSelf), "Service cooling hardware mounts after assembly");
            await Stage(1);

            board.SelectRoute(0);
            Check(board.PlateVia(), "First route plates its native barrel before routing");
            board.FocusRoute();
            await Task.Yield();
            var wire = Object.FindFirstObjectByType<SignalWireBuilder>();
            var firstRoute = board.routes[0];
            Vector2 from = Camera.main.WorldToScreenPoint(firstRoute.from.transform.position);
            Vector2 to = Camera.main.WorldToScreenPoint(firstRoute.to.transform.position);
            Check(wire.FindStartTerminal(from) == firstRoute.from
                && wire.BeginWire(firstRoute.from, WirePolarity.Neutral) && wire.TryEndWire(to),
                "Native wire gesture resolves both projected pads and commits the segment");
            using (manager.ElectricalSolver.BeginConfigurationBatch())
            {
                for (int i = 1; i < board.routes.Length; i++)
                {
                    board.SelectRoute(i);
                    if (board.routes[i].via && !board.PlateVia()) throw new InvalidOperationException($"Via failed: {board.routes[i].viaId}");
                    if (!board.ConnectRoute()) throw new InvalidOperationException($"Route failed: {board.routes[i].id}");
                }
                board.SelectRoute(board.routes.Length);
                Check(board.ConnectRoute(), "Commissioning jumper closes through native wiring");
            }
            Check(board.Routed == 172 && board.Plated == 56, "All 172 graph wires and 56 actual barrel switches are present");
            bool solved = manager.ElectricalSolver.SolveNow();
            var result = manager.ElectricalSolver.LatestSolveResult;
            Log($"Native branch reading, solve={solved}: " + string.Join(", ", board.branches.Select(branch =>
                branch.name + "=" + (result != null && result.TryGetVoltage(branch.sense, board.ground, out var voltage)
                    ? voltage.ToString("F4") + " V" : "unknown"))));
            await Stage(2);
            await Commission();
            await Capture(true);
            await Stage(3);
            Check(board.CapturedTransient, "Oscilloscope retains measured sub-display-interval transient peaks and dips");
            for (int i = 0; i < 3; i++)
            {
                Check(board.InjectNextFault(), $"Fault F{i + 1} changes the model");
                Check(!board.Diagnose() && !board.Repair(), "Unmeasured fault cannot authorize repair");
                await Capture(false);
                if (i == 0)
                {
                    Check(!board.Diagnose(), "Powered ESR measurement rejected");
                    board.Power();
                    await Wait(() => board.Model.Voltage < .08, "Safe ESR measurement");
                }
                Check(board.Diagnose(), $"Instrument evidence confirms F{i + 1}");
                if (board.Powered)
                {
                    Check(!board.Repair(), "Energized rework rejected");
                    board.Power();
                    await Wait(() => board.Model.Voltage < .08, "Safe rework voltage");
                }
                board.SelectFootprint((board.faultParts[i] + 1) % board.parts.Length);
                Check(!board.Repair(), "Wrong footprint cannot clear the fault");
                board.SelectFootprint(board.faultParts[i]);
                Check(board.Repair(), "Correct replacement changes the electrical model");
                await Commission();
                await Capture(true);
            }
            await Stage(4);
            Check(board.TestProtection(), "Operator validates the four protection cases");
            await Capture(true);
            await Stage(5);
            await Wait(() => !board.Inhibited && board.workloadInterlock.IsConducting, "Protected workload armed");
            Check(board.BreakJumper() && board.Inhibited && !board.workloadInterlock.IsConducting, "W173 opens native edge and independently opens workload path");
            await Wait(() => board.Model.PowerGood && !board.HealthPresent, "Low-power diagnostic rail retained through W173 loss");
            Check(!board.ResetSafety(), "Reset rejected while W173 is open");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectRoute() && board.Inhibited && !board.ResetSafety(), "Reconnect alone cannot reset safety");
            await Capture(true);
            Check(board.ResetSafety() && board.workloadInterlock.IsConducting,
                "Fresh evidence permits a separate reset and closes the native interlock");
            await Capture(true);
            await Stage(6);
            Check(manager.IsCompleted, "Authoritative live manager completed Level 110");
            board.Home();
            await Task.Delay(550);
            ScreenCapture.CaptureScreenshot("Logs/Level110Complete.png");
            await Task.Delay(400);
            int previousIndex = Array.FindLastIndex(manager.Levels, level => level && level.LevelId == "level_104");
            if (previousIndex >= 0)
            {
                Check(manager.StartLevel(previousIndex), "Existing Level 104 still loads");
                await Wait(() => !board && Object.FindFirstObjectByType<SparkLevel104Process>(), "Level 110 teardown and Level 104 handoff");
                Check(manager.ActiveLevel.LevelId == "level_104", "Campaign regains previous level authority");
            }
        }
    }
}
