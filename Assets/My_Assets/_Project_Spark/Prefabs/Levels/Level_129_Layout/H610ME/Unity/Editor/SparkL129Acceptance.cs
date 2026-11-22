using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.SiliconWorkshop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.Level129.Editor
{
    [InitializeOnLoad]
    internal static class SparkL129Acceptance
    {
        private const string Request = "Logs/Level129Acceptance.request";
        private const string Report = "Logs/Level129Acceptance.txt";
        private const string Session = "ProjectSpark.Level129.Acceptance";
        private static SparkL129Workshop board;
        private static LevelGamePlayManager manager;
        private static SparkLevelLayoutLoader loader;

        static SparkL129Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/129/Run Gameplay Acceptance")]
        private static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE SILICON WORKSHOP / H610-EDU-1" + Environment.NewLine);
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkL129PlayableBuilder.Prefab);
                Check(prefab && prefab.Validate(out _), "Serialized campaign prefab and bindings resolve.");
                var active = SceneManager.GetActiveScene();
                if (active.path != SparkL129PlayableBuilder.Scene)
                {
                    Check(!active.isDirty && SceneManager.sceneCount == 1, "The saved scene can be left for campaign testing.");
                    EditorSceneManager.OpenScene(SparkL129PlayableBuilder.Scene, OpenSceneMode.Single);
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
                    PlayerPrefs.SetString(SparkL129Workshop.CheckpointKey, SessionState.GetString(Session + ".SavedCheckpoint", ""));
                else PlayerPrefs.DeleteKey(SparkL129Workshop.CheckpointKey);
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
            SessionState.SetBool(Session + ".HadCheckpoint", PlayerPrefs.HasKey(SparkL129Workshop.CheckpointKey));
            SessionState.SetString(Session + ".SavedCheckpoint", PlayerPrefs.GetString(SparkL129Workshop.CheckpointKey));
            PlayerPrefs.DeleteKey(SparkL129Workshop.CheckpointKey);
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
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkL129PlayableBuilder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == definition).DefaultIfEmpty(-1).Single();
            Check(index >= 0 && index == levels.arraySize - 1 && manager.StartLevel(index),
                $"The standard MainMenu loader starts the appended campaign entry at index {index}.");
            await Bind();
            Check(board.Stage == 0 && board.Installed == 0 && manager.SatisfiedTargetCount == 0 &&
                board.cover.activeSelf && board.service.All(site => !site.model.activeSelf) &&
                board.supplies.All(source => !source.IsOutputActive),
                "Factory art is present. Service parts start empty, isolated and 0/6.");
            var body = board.substrate.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((first, next) => { first.Encapsulate(next); return first; });
            var table = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Single(node => node.name == "CentralTable").GetComponent<BoxCollider>();
            var tableSize = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(body.size.x - 9.423f) < .08f && Mathf.Abs(body.size.z - 7.98f) < .08f &&
                Mathf.Abs(body.center.x - center.x) < .05f && Mathf.Abs(body.center.z - center.z) < .05f,
                $"Original substrate is centered at {body.size.x:F2} × {body.size.z:F2} on the {tableSize.x:F2} × {tableSize.z:F2} game table.");
            await Screen("Level129Opening");

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
            Check(board.Seat() && board.Secure() && board.nvmeScrew.activeSelf, "M.2 drive secures with the retaining screw.");
            await Stage(1);
            Check(manager.RestartLevel(), "Restart reloads the native campaign prefab.");
            await Bind();
            Check(board.HasCheckpoint && board.Stage == 0 && board.Resume() && !board.Standby && board.Installed == 31 &&
                board.RailCount == 0 && !board.PostPassed, "Checkpoint restores service hardware and requires fresh rail and POST evidence.");

            PowerChecks();
            await Stage(2);
            Check(!board.RunInterface(), "Interface testing is blocked before POST.");
            await Post(SparkL129Workshop.PostSignal.Ready);
            await Stage(3);
            board.ChoosePort(3);
            board.ChooseMode(4);
            Check(!board.RunInterface(), "Wrong protocol on a powered interface is rejected.");
            Interfaces();
            await Stage(4);
            await Screen("Level129Interfaces");

            string[] references = { "PQ04", "DDR5_2", "M2_1" };
            SparkL129Workshop.PostSignal[] signals = { SparkL129Workshop.PostSignal.CPU,
                SparkL129Workshop.PostSignal.DRAM, SparkL129Workshop.PostSignal.BOOT };
            for (int fault = 0; fault < references.Length; fault++)
            {
                Check(board.InjectFault(), "Open the independent hardware ticket " + (fault + 1));
                await Post(signals[fault]);
                Check(!board.Diagnose((fault + 1) % 3) && board.Diagnose(fault) && board.Diagnostic == fault,
                    $"{references[fault]} diagnosed at {signals[fault]} with independent evidence. {board.Detail}");
                if (fault == 0) await Screen("Level129Fault");
                board.SetRepairSite(fault);
                board.ChooseSpare(fault);
                Check(!board.Repair() && board.Isolate() && board.supplies.All(source => !source.IsOutputActive),
                    "Powered replacement rejected. Native sources are off after isolation.");
                board.ChooseSpare((fault + 1) % 3);
                Check(!board.Repair(), "Wrong spare is rejected.");
                board.ChooseSpare(fault);
                board.SetRepairSite((fault + 1) % 3);
                Check(!board.Repair(), "Wrong physical repair site is rejected.");
                board.SetRepairSite(fault);
                Check(board.Repair() && !board.VerifyRepair(), "Physical repair requires fresh loaded rails, POST and interface qualification.");
                PowerChecks();
                await Post(SparkL129Workshop.PostSignal.Ready);
                Interfaces();
                Check(board.VerifyRepair() && board.Repaired == fault + 1, "Native reevaluation verified repair " + (fault + 1));
            }

            await Stage(5);
            Check(!board.SimulateFanLoss() && board.FanRpm >= 850 && board.BeginThermal(), "Protection challenge begins with real fan-load feedback.");
            await Wait(() => board.ThermalSamples >= 10, "loaded CPU and fan feedback");
            Check(board.Amps(board.boost) > .6f && board.SimulateFanLoss(), "Fan failure introduced while the native CPU load is active.");
            await Wait(() => board.Tripped, "three lost tach samples and protected shutdown");
            Check(board.ProtectionObserved && !board.Running && board.Standby && board.Amps(board.boost) < .001f &&
                board.Amps(board.rails[2]) < .001f && board.Volts(board.rails[0]) > 4.7f && board.FanRpm == 0,
                "Native CPU power removed under load, tach lost, standby retained and hardware guard latched.");
            await Screen("Level129Guard");
            Check(!board.RestoreFan() && !board.PowerOn() && !board.ResetGuard(),
                "Live fan repair, latched restart and premature reset are rejected.");
            Check(board.Isolate() && board.RestoreFan() && board.ResetGuard() && !board.BeginThermal(),
                "Fan restored with isolated power, manual reset required, stale pre-trip evidence cannot start the final run.");

            PowerChecks();
            await Post(SparkL129Workshop.PostSignal.Ready);
            Interfaces();
            Check(board.FreshReboot && board.BeginThermal(), "Fresh loaded rails, POST and eight interface passes authorize the last thermal run.");
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && board.FreshReboot && board.ProtectionObserved &&
                board.ThermalSamples == 24 && board.Peak < 62f && board.Temperature < board.Peak && board.FanRpm >= 850,
                "6/6 complete at the native manager. " + board.Detail);
            Click("Angle view");
            await Screen("Level129Qualified");
        }

        private static void PowerChecks()
        {
            Check(!board.PowerOn() && board.ApplyStandby() && !board.PowerOn(), "Main power is inhibited before loaded standby verification.");
            board.ChooseRail(0);
            Check(board.ProbeRail() && board.PowerOn(), "Loaded standby was measured before enabling main power.");
            for (int i = 1; i < board.rails.Length; i++)
            {
                board.ChooseRail(i);
                Check(board.ProbeRail(), board.Detail);
            }
            Check(board.RailCount == 5, "Five fresh native rail readings are in range.");
        }

        private static async Task Post(SparkL129Workshop.PostSignal expected)
        {
            Check(board.StartPost(), "Staged POST started.");
            await Wait(() => !board.IsBusy, "POST finished");
            Check(board.Post == expected && board.PostPassed == (expected == SparkL129Workshop.PostSignal.Ready), "POST ended at " + expected);
        }

        private static void Interfaces()
        {
            for (int i = 0; i < SparkL129Workshop.PortNames.Length; i++)
            {
                board.ChoosePort(i);
                board.ChooseMode(SparkL129Workshop.CorrectPortModes[i]);
                Check(board.RunInterface(), board.Detail);
            }
            Check(board.PortCount == 8, "All eight native interface loads and virtual fixtures passed.");
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

