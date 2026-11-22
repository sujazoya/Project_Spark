using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.RecoveryLab;
using ProjectSpark.Measurement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.Level130.Editor
{
    [InitializeOnLoad]
    internal static class SparkL130Acceptance
    {
        private const string Request = "Logs/Level130Acceptance.request";
        private const string Report = "Logs/Level130Acceptance.txt";
        private const string Session = "ProjectSpark.Level130.Acceptance";
        private static SparkL130Workshop board;
        private static LevelGamePlayManager manager;
        private static SparkLevelLayoutLoader loader;

        static SparkL130Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/130/Run Gameplay Acceptance")]
        private static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE RECOVERY LAB / H610-RECOVERY-1" + Environment.NewLine);
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkL130PlayableBuilder.Prefab);
                Check(prefab && prefab.Validate(out _), "Serialized campaign prefab and bindings resolve.");
                var active = SceneManager.GetActiveScene();
                if (active.path != SparkL130PlayableBuilder.Scene)
                {
                    Check(!active.isDirty && SceneManager.sceneCount == 1, "The saved scene can be left for campaign testing.");
                    EditorSceneManager.OpenScene(SparkL130PlayableBuilder.Scene, OpenSceneMode.Single);
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
                    PlayerPrefs.SetString(SparkL130Workshop.CheckpointKey, SessionState.GetString(Session + ".SavedCheckpoint", ""));
                else PlayerPrefs.DeleteKey(SparkL130Workshop.CheckpointKey);
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
            SessionState.SetBool(Session + ".HadCheckpoint", PlayerPrefs.HasKey(SparkL130Workshop.CheckpointKey));
            SessionState.SetString(Session + ".SavedCheckpoint", PlayerPrefs.GetString(SparkL130Workshop.CheckpointKey));
            PlayerPrefs.DeleteKey(SparkL130Workshop.CheckpointKey);
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
            var definition = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkL130PlayableBuilder.Definition);
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
            await Screen("Level130Opening");

            Check(!board.RecordIntake(), "Intake rejects missing inspection and inventory.");
            for (int site = 0; site < board.diagnostics.Length; site++)
            {
                board.ChooseDiagnostic(site);
                Check(board.InspectLandmark(), "Original landmark recorded: " + board.diagnostics[site].name);
            }
            Click("Service tray");
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
            Check(board.RecordIntake() && board.IntakeCount == 10, "Ten original observations and five compatible service parts sign intake.");
            await Stage(1);
            Check(manager.RestartLevel(), "Restart reloads the native campaign prefab.");
            await Bind();
            Check(board.HasCheckpoint && board.Stage == 0 && board.Resume() && !board.Standby && board.Installed == 31 &&
                board.RailCount == 0 && !board.PostPassed, "Checkpoint restores service hardware and requires fresh rail and POST evidence.");

            var measurement = Object.FindFirstObjectByType<SparkMeasurementSystem>();
            Check(measurement, "The campaign measurement system is present.");
            int readingEvents = 0;
            void CountReading(SparkMeasurementReading reading) { if (reading.Valid) readingEvents++; }
            measurement.ReadingProduced += CountReading;
            try
            {
                PowerRequest();
                board.ChooseRail(1);
                Check(board.ProbeRail(), board.Detail);
                board.ChooseRail(2);
                Check(!board.ProbeRail() && board.Volts(board.rails[2]) < .1f && board.Volts(board.rails[3]) < .1f,
                    "Native EPS socket and protected VCORE are dead although upstream EPS is energized.");
                await Post(SparkL130Workshop.PostSignal.CPU);
                board.ChooseDiagnostic(0);
                Check(!board.AnalyzeSelected(), "The standby visual anchor cannot claim an EPS fault.");
                board.ChooseDiagnostic(1);
                Check(board.AnalyzeSelected() && board.HasDiagnosis, "EPS fault captured with measured upstream and downstream readings. " + board.Detail);
                await Screen("Level130EPSFault");
                Fix(0);
                PowerChecks();
                Check(readingEvents >= 18 && board.VerifyRepair(), "EPS recovery uses fresh native measurement events and restored loaded CPU power. " + board.Detail);
                await Stage(2);

                Check(board.OpenCase() && board.Volts(board.romPower) > 3.2f && board.Amps(board.romData) < .001f,
                    "ROM fault changes the native data-ready branch while the flash power branch stays live.");
                await Post(SparkL130Workshop.PostSignal.Firmware);
                board.ChooseDiagnostic(2);
                Check(board.AnalyzeSelected(), "ROM's two corrupt reads are captured before memory training. " + board.Detail);
                await Screen("Level130RomFault");
                Fix(1);
                PowerChecks();
                Check(!board.VerifyRepair(), "Firmware repair cannot qualify before independent cold initialization.");
                await Post(SparkL130Workshop.PostSignal.Ready);
                Check(board.VerifyRepair(), "The fictional ROM now produces the approved repeat CRC and a fresh POST. " + board.Detail);
                await Stage(3);

                Check(!board.ProbeDevice() && !board.SignDevices() && board.Isolate(),
                    "ROM's recovery boot cannot be reused as the next objective's fresh device audit.");
                PowerChecks();
                await Post(SparkL130Workshop.PostSignal.Ready);
                Devices();
                Check(board.SignDevices(), "Both DDR5 channels, both PCIe links and the NVMe test sign a new boot.");
                await Stage(4);

                Check(board.OpenCase(), "Open the cooling feedback case from the clean device baseline.");
                await Post(SparkL130Workshop.PostSignal.Ready);
                Devices();
                Check(!board.BeginThermal() && board.MotorRpm >= 900 && board.TachRpm == 0,
                    "The fan motor is powered while feedback is open. The trial requires a measured diagnosis.");
                board.ChooseDiagnostic(3);
                Check(board.AnalyzeSelected() && board.BeginThermal(), "Fan motor versus native tach discrepancy measured. " + board.Detail);
                await Wait(() => board.Tripped, "loaded missing-tach guard");
                Check(board.ProtectionObserved && board.Standby && !board.Running && board.Amps(board.boost) < .001f &&
                    board.Amps(board.rails[3]) < .001f && board.Volts(board.rails[0]) > 4.7f,
                    "The independent missing-tach guard removed the CPU load while retaining standby.");
                await Screen("Level130TachGuard");
                Check(!board.ResetProtection() && !board.PowerOn(), "The latched bench rejects restart and premature reset.");
                Fix(2);
                Check(!board.fanOriginal.activeSelf && board.fanReplacement.activeSelf && board.ResetProtection(),
                    "The exact kit tach spare is present and an isolated acknowledgement clears the latch.");
                PowerChecks();
                await Post(SparkL130Workshop.PostSignal.Ready);
                Devices();
                Check(!board.VerifyRepair() && board.BeginThermal(), "Fan replacement requires a new full loaded thermal qualification.");
                await Wait(() => !board.IsBusy, "thermal retest");
                Check(board.ThermalPassed && board.VerifyRepair(), "All samples pass with independently restored motor and tach. " + board.Detail);
                await Stage(5);
                int repairedBoot = board.Boot;
                Check(!board.SignAudit() && board.Isolate(), "Final release rejects all evidence from the fan repair boot.");

                PowerChecks();
                await Post(SparkL130Workshop.PostSignal.Ready);
                Devices();
                Check(board.Boot > repairedBoot && !board.SignAudit() && board.BeginThermal(),
                    "Final audit requires fresh thermal evidence even after power, POST and device checks.");
                await Wait(() => !board.IsBusy, "current boot audit thermal completion");
                Check(board.ThermalPassed && board.SignAudit(), "The final audit accepts power, POST, devices and thermal evidence from one fresh boot. " + board.Detail);
                await Stage(6);
                Check(manager.IsCompleted && board.Repaired == 3 && board.ThermalSamples == board.ExpectedThermalSamples &&
                    board.Temperature < board.Peak && board.TachRpm >= 900,
                    "6/6 at the native manager. " + board.Detail);
                foreach (string id in new[] { "F1", "F2", "F3" })
                {
                    var entries = board.Journal.Where(entry => entry.CaseId == id).ToArray();
                    Check(entries.Length >= 2 && !entries.First().Passed && entries.Last().Passed &&
                        entries.First().Revision < entries.Last().Revision && entries.First().Boot < entries.Last().Boot &&
                        entries.First().Seconds <= entries.Last().Seconds,
                        id + " journal retains ordered before/after evidence tied to the repair, topology and boot.");
                }
                Click("Angle view");
                await Screen("Level130Qualified");
            }
            finally { measurement.ReadingProduced -= CountReading; }
        }

        private static void Fix(int fault)
        {
            board.ChooseRepair(fault);
            Check(!board.InspectRepair() && !board.Repair() && board.Isolate() &&
                board.supplies.All(source => !source.IsOutputActive), "Powered inspection and repair are rejected; motherboard supplies are off after isolation.");
            Check(!board.Repair() && board.InspectRepair(), "The correct component requires a separate isolated inspection.");
            board.ChooseRepair((fault + 1) % 3);
            Check(!board.Repair(), "The wrong recovery action is rejected.");
            board.ChooseRepair(fault);
            Check(board.Repair() && !board.VerifyRepair(), "Matched recovery action requires later powered retest.");
        }

        private static void PowerRequest()
        {
            Check(!board.PowerOn() && board.ApplyStandby() && !board.PowerOn(), "Main power waits for a valid loaded standby reading.");
            board.ChooseRail(0);
            Check(board.ProbeRail() && board.PowerOn(), "Native measurement accepted standby before main request.");
        }

        private static void PowerChecks()
        {
            PowerRequest();
            for (int i = 1; i < board.rails.Length; i++)
            {
                board.ChooseRail(i);
                Check(board.ProbeRail(), board.Detail);
            }
            Check(board.RailCount == 6, "Six current-boot loaded measurements are in range.");
        }

        private static void Devices()
        {
            for (int device = 0; device < SparkL130Workshop.PortNames.Length; device++)
            {
                board.ChoosePort(device);
                Check(board.ProbeDevice(), board.Detail);
            }
        }

        private static async Task Post(SparkL130Workshop.PostSignal expected)
        {
            Check(board.StartPost(), "Staged POST started.");
            await Wait(() => !board.IsBusy, "POST finished");
            Check(board.Post == expected && board.PostPassed == (expected == SparkL130Workshop.PostSignal.Ready), "POST ended at " + expected);
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

