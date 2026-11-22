using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.AorusWorkshop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.Level128.Editor
{
    [InitializeOnLoad]
    internal static class SparkL128Acceptance
    {
        private const string Request = "Logs/Level128Acceptance.request";
        private const string Report = "Logs/Level128Acceptance.txt";
        private const string Session = "ProjectSpark.Level128.Acceptance";
        private static SparkL128Workshop board;
        private static LevelGamePlayManager manager;
        private static SparkLevelLayoutLoader loader;

        static SparkL128Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/128/Run Gameplay Acceptance")]
        private static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE AORUS MASTER / X870E-EDU-1" + Environment.NewLine);
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkL128PlayableBuilder.Prefab);
                Check(prefab && prefab.Validate(out _), "Serialized campaign prefab and bindings resolve.");
                var active = SceneManager.GetActiveScene();
                if (active.path != SparkL128PlayableBuilder.Scene)
                {
                    Check(!active.isDirty && SceneManager.sceneCount == 1, "The saved scene can be left for campaign testing.");
                    EditorSceneManager.OpenScene(SparkL128PlayableBuilder.Scene, OpenSceneMode.Single);
                }
                SessionState.SetString(Session + ".StartScene", AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
                EditorSceneManager.playModeStartScene = null;
                SessionState.SetBool(Session, true);
                EditorApplication.EnterPlaymode();
            }
            catch (Exception ex) { Log("FAIL " + ex); }
        }

        private static async void Play(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Session, false)) return;
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.EraseBool(Session);
                string previous = SessionState.GetString(Session + ".StartScene", "");
                EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(previous) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);
                SessionState.EraseString(Session + ".StartScene");
                if (SessionState.GetBool(Session + ".HadCheckpoint", false))
                    PlayerPrefs.SetString(SparkL128Workshop.CheckpointKey, SessionState.GetString(Session + ".SavedCheckpoint", ""));
                else PlayerPrefs.DeleteKey(SparkL128Workshop.CheckpointKey);
                PlayerPrefs.Save();
                SessionState.EraseBool(Session + ".HadCheckpoint");
                SessionState.EraseString(Session + ".SavedCheckpoint");
                Log("Returned to Edit Mode.");
            }
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            var background = Application.runInBackground;
            var filter = Debug.unityLogger.filterLogType;
            Application.runInBackground = true;
            Debug.unityLogger.filterLogType = LogType.Error;
            SessionState.SetBool(Session + ".HadCheckpoint", PlayerPrefs.HasKey(SparkL128Workshop.CheckpointKey));
            SessionState.SetString(Session + ".SavedCheckpoint", PlayerPrefs.GetString(SparkL128Workshop.CheckpointKey));
            PlayerPrefs.DeleteKey(SparkL128Workshop.CheckpointKey);
            try
            {
                await Run();
                Log("COMPLETE. Live LevelGamePlayManager evaluated all six objectives.");
            }
            catch (Exception ex) { Log("FAIL " + ex); }
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
            loader = Object.FindFirstObjectByType<SparkLevelLayoutLoader>();
            var selection = Object.FindFirstObjectByType<SparkSelectionController>();
            Check(manager && loader && selection && manager.ElectricalSolver && manager.CircuitSystem, "MainMenu provides native layout, selection and electrical services.");
            if (manager.LevelProgression) manager.LevelProgression.enabled = false;
            var serialized = new SerializedObject(manager);
            var levels = serialized.FindProperty("levels");
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkL128PlayableBuilder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == definition).DefaultIfEmpty(-1).Single();
            Check(index >= 0 && index == levels.arraySize - 1 && manager.StartLevel(index),
                $"The standard MainMenu loader starts the appended campaign entry at index {index}.");
            await Bind();
            Check(board.Stage == 0 && board.Installed == 0 && manager.SatisfiedTargetCount == 0 &&
                board.retention.activeSelf && board.socketLever.activeSelf && board.service.All(site => !site.model.activeSelf) &&
                board.supplies.All(source => !source.IsOutputActive),
                "Factory art is present. Service parts start empty, isolated and 0/6.");
            var body = board.substrate.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((first, next) => { first.Encapsulate(next); return first; });
            var table = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Single(node => node.name == "CentralTable").GetComponent<BoxCollider>();
            var tableSize = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(body.size.x - 9.9746f) < .08f && Mathf.Abs(body.size.z - 7.98f) < .08f &&
                Mathf.Abs(body.center.x - center.x) < .05f && Mathf.Abs(body.center.z - center.z) < .05f,
                $"Original substrate is centered at {body.size.x:F2} × {body.size.z:F2} on the {tableSize.x:F2} × {tableSize.z:F2} game table.");
            await Screen("Level128Opening");

            Physics.SyncTransforms();
            var screenPoint = selection.SelectionCamera.WorldToScreenPoint(board.service[0].anchor.position);
            bool nativeSelection = selection.TrySelect(screenPoint, out string selectionReason);
            var hits = Physics.RaycastAll(selection.SelectionCamera.ScreenPointToRay(screenPoint), 1000f).OrderBy(hit => hit.distance)
                .Take(6).Select(hit => FullPath(hit.collider.transform) + $" [{hit.distance:F2}]");
            Check(nativeSelection && selection.SelectedObject == board.service[0].selectable,
                "The native pointer ray selects the original CPU footprint. " +
                (selection.SelectedObject ? selection.SelectedObject.name : selectionReason) + ". Ray hits: " + string.Join(", ", hits));
            Check(!board.PowerOn() && !board.Seat(), "Power and CPU seating are blocked with the socket guard closed.");
            Click("Open guard");
            Click("Rotate 90°");
            Check(!board.Seat() && !board.service[0].model.activeSelf, "A misaligned CPU key cannot seat.");
            board.RotatePart();
            board.RotatePart();
            board.RotatePart();
            Check(board.Seat() && board.service[0].model.activeSelf && board.Installed == 0, "Seating previews the CPU and requires separate confirmation.");
            Check(board.Secure(), "CPU retention frame secures the aligned processor.");

            board.ChooseSite(1);
            board.ChoosePart(1);
            Check(board.Seat() && !board.Secure() && !board.Tighten(3), "Cooler requires a correctly ordered cross-clamp.");
            foreach (int corner in new[] { 0, 3, 1, 2 }) Check(board.Tighten(corner), "Tightened cooler corner " + (corner + 1));
            Check(board.Secure(), "Cooler locks only after all four mounts.");
            board.ChooseSite(2);
            board.ChoosePart(4);
            Check(!board.Seat(), "DDR4 distractor is rejected by the DDR5 footprint.");
            board.ChoosePart(2);
            Check(board.Seat() && !board.Secure() && board.LatchDimm() && board.Secure(), "First DDR5 channel is keyed, seated and latched.");
            board.ChooseSite(3);
            Check(board.Seat() && board.LatchDimm() && board.Secure(), "Second independent DDR5 channel is latched.");
            board.ChooseSite(4);
            board.ChoosePart(3);
            Check(!board.Seat() && board.LiftDriveArmor(), "The original M.2 A spreader blocks seating until lifted.");
            Check(board.Seat() && board.Secure() && board.driveArmor[0].activeSelf && board.DriveMask == 1,
                "The original M.2 A drive anchor fits and closes its heat spreader after securing.");
            await Stage(1);
            Check(manager.RestartLevel(), "Restart reloads the native campaign prefab.");
            await Bind();
            Check(board.HasCheckpoint && board.Stage == 0 && board.Resume() && !board.Standby && board.Installed == 31 &&
                board.RailCount == 0 && !board.PostPassed, "Checkpoint restores service hardware and requires fresh rail and POST evidence.");

            PowerChecks();
            await Stage(2);
            Check(!board.RunInterface(), "Interface testing is blocked before POST.");
            await Post(SparkL128Workshop.PostSignal.Ready);
            await Stage(3);
            board.ChoosePort(0);
            board.ChooseMode(1);
            Check(!board.RunInterface() && board.GpuWidth == 16, "The native upper lane bank rejects an ×8 prediction with only M.2 A.");
            board.ChoosePort(4);
            Check(!board.RunInterface() && !board.LiftDriveArmor() && !board.SwapLaneDrive(),
                "Empty M.2 D cannot qualify, and heated board service is rejected.");
            Links();
            Check(board.DriveMask == 1 && board.LaneScenarios == 1 && board.GpuWidth == 16 && board.Stage == 3,
                "A-only baseline verified GPU ×16 without awarding the full lane matrix.");
            Check(board.Isolate(), "Fully isolated for chipset storage installation.");
            FitExpansion(4);
            PowerChecks();
            await Post(SparkL128Workshop.PostSignal.Ready);
            Links();
            Check(board.DriveMask == 9 && board.LaneScenarios == 3 && board.GpuWidth == 16 && board.Stage == 3,
                "Chipset M.2 D adds a verified drive without reducing the native GPU lane bank.");
            await Screen("Level128ChipsetX16");
            Check(board.Isolate(), "Fully isolated before adding CPU-connected M.2 B and C.");
            FitExpansion(2);
            FitExpansion(3);
            PowerChecks();
            await Post(SparkL128Workshop.PostSignal.Ready);
            Links();
            await Stage(4);
            Check(board.DriveMask == 15 && board.LaneScenarios == 7 && board.GpuWidth == 8 && board.PortCount == 5,
                "All three configurations, four M.2 fixtures and native GPU ×8 qualify the lane sharing objective.");
            await Screen("Level128SharedX8");

            string[] references = { "VRM_Q03", "DDR5_A2", "M2B_CPU" };
            SparkL128Workshop.PostSignal[] signals = { SparkL128Workshop.PostSignal.CPU,
                SparkL128Workshop.PostSignal.DRAM, SparkL128Workshop.PostSignal.BOOT };
            for (int fault = 0; fault < references.Length; fault++)
            {
                Check(board.InjectFault(), "Open the independent hardware ticket " + (fault + 1));
                await Post(signals[fault]);
                Check(!board.Diagnose((fault + 1) % 3) && board.Diagnose(fault) && board.Diagnostic == fault,
                    $"{references[fault]} diagnosed at {signals[fault]} with independent evidence. {board.Detail}");
                board.SetRepairSite(fault);
                board.ChooseSpare(fault);
                Check(!board.InspectRepair() && !board.Repair() && board.Isolate() && board.supplies.All(source => !source.IsOutputActive),
                    "Powered inspection and replacement rejected. All motherboard sources are off after isolation.");
                Check(!board.Repair(), "Replacement is blocked until the physical site is inspected.");
                board.SetRepairSite((fault + 1) % 3);
                Check(board.InspectRepair() && !board.Repair(), "Wrong physical repair site is rejected even after inspection.");
                board.SetRepairSite(fault);
                Check(board.InspectRepair() && (fault == 1 || fault == 0 && !board.vrmArmor.activeSelf ||
                    fault == 2 && !board.driveArmor[1].activeSelf), "Inspection opens the correct factory cover where one exists.");
                if (fault == 0) await Screen("Level128VRMInspection");
                board.ChooseSpare((fault + 1) % 3);
                Check(!board.Repair(), "Wrong spare is rejected.");
                board.ChooseSpare(fault);
                Check(board.Repair() && !board.VerifyRepair() && board.vrmArmor.activeSelf && board.driveArmor[1].activeSelf,
                    "Replacement refits the original covers and requires fresh rails, POST and link readings.");
                PowerChecks();
                await Post(SparkL128Workshop.PostSignal.Ready);
                Links();
                Check(board.VerifyRepair() && board.Repaired == fault + 1, "Fresh native evidence verified repair " + (fault + 1));
            }

            await Stage(5);
            Check(!board.SimulateFanLoss() && board.FanRpm >= 850 && board.BeginThermal(), "Thermal challenge starts only with native fan feedback.");
            await Wait(() => board.ThermalSamples >= 10, "loaded core and fan feedback");
            Check(board.Amps(board.boost) > .6f && board.SimulateFanLoss(), "Fan failure induced during genuine solved CPU load.");
            await Wait(() => board.Tripped, "independent fan-loss shutdown");
            Check(board.ProtectionObserved && !board.Running && board.Standby && board.Amps(board.boost) < .001f &&
                board.Amps(board.rails[2]) < .001f && board.Volts(board.rails[0]) > 4.7f &&
                board.Amps(board.rails[6]) < .001f && board.FanRpm == 0, "Guard drops VCORE, boost and power-good while standby remains live.");
            await Screen("Level128Guard");
            Check(!board.RestoreFan() && !board.PowerOn() && !board.ResetGuard(), "Live fan service and premature reset are rejected.");
            Check(board.Isolate() && board.RestoreFan() && board.ResetGuard() && !board.BeginThermal(),
                "Cooling is restored only isolated and the reset rejects stale pre-trip evidence.");

            PowerChecks();
            await Post(SparkL128Workshop.PostSignal.Ready);
            Links();
            Links();
            Check(board.ColdBoots == 1 && !board.BeginThermal() && board.Isolate(),
                "Repeating link checks in one powered session yields only one cold boot.");
            PowerChecks();
            await Post(SparkL128Workshop.PostSignal.Ready);
            Links();
            Check(board.ColdBoots == 2 && board.FreshReboot && board.BeginThermal(),
                "Two separately isolated, measured boot cycles authorize the final thermal soak.");
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && board.FreshReboot && board.ProtectionObserved &&
                board.ThermalSamples == 24 && board.Peak < 62f && board.Temperature < board.Peak && board.FanRpm >= 850,
                "6/6 complete at the native manager. " + board.Detail);
            Click("Angle view");
            await Screen("Level128Qualified");
        }

        private static void FitExpansion(int port)
        {
            board.ChoosePort(port);
            Check(!board.SwapLaneDrive() && board.LiftDriveArmor() && board.SwapLaneDrive() && board.driveArmor[port - 1].activeSelf,
                $"{SparkL128Workshop.PortNames[port]} installed at the original isolated footprint.");
        }

        private static void PowerChecks()
        {
            Check(!board.PowerOn() && board.ApplyStandby() && !board.PowerOn(), "Main feed stays inhibited until loaded standby verification.");
            board.ChooseRail(0);
            Check(board.ProbeRail() && board.PowerOn(), "Loaded standby qualified before enabling both main feeds.");
            board.ChooseRail(1);
            Check(board.ProbeRail(), board.Detail);
            Check(!board.EnableRegulators() && board.Amps(board.rails[6]) < .001f && board.Amps(board.rails[2]) < .001f,
                "One measured EPS feed cannot raise power-good or VCORE.");
            board.ChooseRail(5);
            Check(board.ProbeRail(), board.Detail);
            Check(board.EnableRegulators(), "Two measured EPS feeds enable native power-good and both regulated outputs.");
            foreach (int rail in new[] { 2, 3, 4, 6 })
            {
                board.ChooseRail(rail);
                Check(board.ProbeRail(), board.Detail);
            }
            Check(board.RailCount == 7, "Seven fresh native loaded test-point readings qualified.");
        }

        private static void Links()
        {
            board.ChoosePort(0);
            board.ChooseMode((board.DriveMask & 6) != 0 ? 1 : 0);
            Check(board.RunInterface(), board.Detail);
            for (int i = 1; i < SparkL128Workshop.PortNames.Length; i++)
            {
                if ((board.DriveMask & (1 << (i - 1))) == 0) continue;
                board.ChoosePort(i);
                Check(board.RunInterface(), board.Detail);
            }
        }

        private static async Task Post(SparkL128Workshop.PostSignal expected)
        {
            Check(board.StartPost(), "Staged POST started.");
            await Wait(() => !board.IsBusy, "POST finished");
            Check(board.Post == expected && board.PostPassed == (expected == SparkL128Workshop.PostSignal.Ready), "POST ended at " + expected);
        }

        private static async Task Bind()
        {
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board.GetComponentInChildren<Canvas>(), "runtime workbench and panel");
        }

        private static async Task Stage(int stage)
        {
            await Wait(() => board.Stage == stage && manager.SatisfiedTargetCount == stage, $"level manager objective {stage}/6");
            Log($"PASS Manager objective {stage}/6.");
        }

        private static void Click(string name)
        {
            var button = board.GetComponentsInChildren<Button>().Single(item => item.name == name);
            Check(button.interactable, name + " is available.");
            button.onClick.Invoke();
        }

        private static async Task Screen(string name)
        {
            await Task.Delay(140);
            string file = Path.GetFullPath("Logs/" + name + ".png");
            File.Delete(file);
            ScreenCapture.CaptureScreenshot(file);
            await Wait(() => File.Exists(file) && new FileInfo(file).Length > 5000, "capture " + name);
        }

        private static async Task Wait(Func<bool> done, string step)
        {
            var until = DateTime.UtcNow.AddSeconds(22);
            while (!done())
            {
                if (DateTime.UtcNow > until) throw new TimeoutException(step + ". " + (board ?
                    $"Stage {board.Stage}; manager {manager.SatisfiedTargetCount}; {board.Message}; {board.Detail}" : "Board unavailable."));
                await Task.Delay(35);
            }
        }

        private static void Check(bool condition, string detail)
        {
            if (!condition) throw new InvalidOperationException(detail + (board ? " / " + board.Message + " / " + board.Detail : ""));
            Log("PASS " + detail);
        }

        private static string FullPath(Transform node) => node.parent ? FullPath(node.parent) + "/" + node.name : node.name;

        private static void Log(string line) => File.AppendAllText(Report, line + Environment.NewLine);
    }
}

