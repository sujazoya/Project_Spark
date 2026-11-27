#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using ProjectSpark.UIManagement;
using Unity.Profiling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.SparkV5.Pro
{
    public static class SparkPhase4PlayerChecks
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void VerifyStandalone()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var activity = ProjectSpark.AI.SparkAndroidRecognition.Activity();
                using var intent = activity?.Call<AndroidJavaObject>("getIntent");
                if (intent?.Call<bool>("getBooleanExtra", "spark.phase2.verify", false) == true)
                    RunLevel20(Path.Combine(Application.persistentDataPath, "SparkAndroidLevel20.txt"));
            }
            catch (AndroidJavaException error)
            {
                Debug.LogWarning("Spark Android development verification intent unavailable: " + error.Message);
            }
#elif !UNITY_EDITOR
            string[] arguments = Environment.GetCommandLineArgs();
            int flag = Array.IndexOf(arguments, "-spark-phase4-verify");
            if (flag > 0 && flag + 1 < arguments.Length) Run(arguments[flag + 1]);
            else
            {
                flag = Array.IndexOf(arguments, "-spark-phase2-verify");
                if (flag > 0 && flag + 1 < arguments.Length) RunLevel20(arguments[flag + 1]);
            }
#endif
        }

        private static async void RunLevel20(string reportPath)
        {
            void Report(string line) => File.AppendAllText(reportPath, line + Environment.NewLine);
            int exit = 1;
            try
            {
                Application.runInBackground = true;
                Report($"Phase 2 development-player verification on {Application.platform}. Started in " +
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + ".");
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Bootstrap")
                    throw new InvalidOperationException("The development player did not start at Bootstrap.");
                SparkProExecutive executive = null;
                LevelGamePlayManager manager = null;
                await Wait(() =>
                {
                    if (!executive) executive = Object.FindFirstObjectByType<SparkProExecutive>();
                    if (!manager) manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
                    return executive && manager && executive.ActiveLevelManager == manager;
                }, "Bootstrap to MainMenu");
                executive.SetSpeakAnswers(false);
                Report("PASS Bootstrap initialized MainMenu and the native campaign manager.");
                // Opt-in development-player fixture. Keep campaign access and
                // automatic advancement from affecting this isolated replay.
                if (manager.LevelProgression) manager.LevelProgression.enabled = false;
                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                typeof(LevelGamePlayManager).GetField("enforceProgressionAccess", flags).SetValue(manager, false);
                typeof(LevelGamePlayManager).GetField("autoAdvanceOnCompletion", flags).SetValue(manager, false);
                int level = manager.GetCampaignLevelIndices().Single(index => manager.GetLevel(index).LevelNumber == 20);
                if (!manager.StartLevel(level)) throw new InvalidOperationException("Native Level 20 did not start.");
                var plan = Resources.LoadAll<SparkNativeWiringPlan>("SparkNativeWirePlans")
                    .Single(candidate => candidate.LevelNumber == 20 && candidate.IsApproved);
                await Wait(() =>
                {
                    var context = SparkAILevelContext.Capture(manager);
                    return context.HasEvaluation && manager.ActiveLevel.LevelNumber == 20 && context.Layout &&
                        plan.Wires.All(w => context.Layout.SceneBindings.TryGetTerminal(w.fromId, out _) &&
                            context.Layout.SceneBindings.TryGetTerminal(w.toId, out _));
                }, "Level 20 evaluation and authored resistor slots");
                var layout = manager.LayoutLoader.ActiveLayout;
                var bindings = layout.SceneBindings;
                var hub = executive.GetComponent<SparkEngineeringControlHub>();
                var device = executive.GetComponent<SparkEngineeringDeviceOperator>();
                var supply = layout.GetComponentInChildren<SparkPowerSupply>();
                if (!hub || !device || !supply || supply.IsOutputActive)
                    throw new InvalidOperationException("Wiring controls are missing or the bench is energized.");
                using var cpu = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread");
                using var gc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
                using var memory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
                executive.Ask("spark auto mode");
                executive.Ask("auto wire practice level");
                if (!hub.IsExecuting) throw new InvalidOperationException(hub.LastMessage);
                int samples = 0;
                double cpuSum = 0, gcSum = 0;
                long peakMemory = 0;
                float started = Time.realtimeSinceStartup;
                while (hub.IsExecuting)
                {
                    if (Time.realtimeSinceStartup - started > 300f) throw new TimeoutException("Level 20 native wiring.");
                    samples++;
                    if (cpu.Valid) cpuSum += cpu.LastValue;
                    if (gc.Valid) gcSum += gc.LastValue;
                    if (memory.Valid) peakMemory = Math.Max(peakMemory, memory.LastValue);
                    await Task.Delay(70);
                }
                int wires = plan.Wires.Count(w => bindings.TryResolveLivePair(layout, w.fromId, w.toId, out var a, out var b, out _) &&
                    manager.CircuitSystem.HasConnectionBetween(a, b));
                Report($"Matched {wires}/{plan.Wires.Count} authored native wires in {Time.realtimeSinceStartup - started:F2}s. {hub.LastMessage}");
                Report($"Samples {samples}, mean main thread {(cpu.Valid ? cpuSum / Math.Max(1, samples) / 1e6 : -1):F2} ms, " +
                    $"mean GC bytes {(gc.Valid ? gcSum / Math.Max(1, samples) : -1):F0}, peak total used memory bytes {(memory.Valid ? peakMemory : -1)}.");
                if (wires != 10) throw new InvalidOperationException("The development player did not complete all native wires.");
                executive.Ask("spark assisted mode");
                async Task Operate(string id, bool on)
                {
                    executive.Ask($"spark propose device {id} {(on ? "on" : "off")}");
                    executive.Ask("spark approve device action");
                    if (!device.IsBusy) throw new InvalidOperationException(device.LastStatus);
                    await Wait(() => !device.IsBusy, id);
                    if (!device.LastStatus.StartsWith("VERIFIED", StringComparison.Ordinal))
                        throw new InvalidOperationException(device.LastStatus);
                }
                var toggle = layout.GetComponentInChildren<SparkSwitch>();
                if (toggle.IsClosed) await Operate("Power_Switch", false);
                await Operate("Power_Switch", true);
                await Wait(() => manager.HasCurrentElectricalEvaluation, "fresh evaluator for power ON");
                await Operate("PowerSupply_01", true);
                await Wait(() => manager.SatisfiedTargetCount == 5 && manager.IsCompleted, "Level 20 powered 5/5");
                await Operate("PowerSupply_01", false);
                Report($"PASS 10/10 native wires, standing switch, and 5/5 powered objectives on {Application.platform}.");
                exit = 0;
            }
            catch (Exception error) { Report("FAIL " + error); }
            finally { Application.Quit(exit); }
        }

        private static async void Run(string reportPath)
        {
            void Report(string line) => File.AppendAllText(reportPath, line + Environment.NewLine);
            try
            {
                Application.runInBackground = true;
                Report("SPARK PHASE 4 / DEVELOPMENT STANDALONE / " + Application.platform);
                SparkProExecutive executive = null;
                LevelGamePlayManager manager = null;
                await Wait(() =>
                {
                    if (!executive) executive = Object.FindFirstObjectByType<SparkProExecutive>();
                    if (!manager) manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
                    return executive && manager && SparkUIManager.Instance && SparkUIManager.Instance.Find(SparkProLivePanel.PanelId);
                }, "Bootstrap to MainMenu");
                int first = manager.GetFirstCampaignIndex();
                if (first < 0 || !manager.StartLevel(first)) throw new InvalidOperationException("Cannot start first mission.");
                await Wait(() => SparkAILevelContext.Capture(manager).HasEvaluation, "first level evaluation");
                Report("PASS Bootstrap loaded MainMenu and native Level " + manager.ActiveLevel.LevelNumber + ".");
                await UseLive(executive, manager, Report, Wait);
                var previous = manager.LayoutLoader.ActiveLayout;
                if (!manager.RestartLevel()) throw new InvalidOperationException("Restart failed.");
                await Wait(() => manager.LayoutLoader.ActiveLayout != previous &&
                    SparkAILevelContext.Capture(manager).HasEvaluation, "new native layout after restart");
                Report("PASS native restart transitions to a fresh loaded layout.");
                VerifySave();
                Report("PASS isolated learner save/load round trip.");
                Report("COMPLETE standalone smoke checks.");
            }
            catch (Exception error) { Report("FAIL " + error); }
            finally { Application.Quit(); }
        }

        public static async Task UseLive(SparkProExecutive executive, LevelGamePlayManager manager,
            Action<string> report, Func<Func<bool>, string, Task> wait)
        {
            if (!SparkUIManager.Instance.Open(SparkProLivePanel.PanelId))
                throw new InvalidOperationException("Spark Live cannot open.");
            executive.SetSpeakAnswers(false);
            executive.Ask("what level am I on?");
            if (!executive.LastAnswer.Contains("Level ", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Text route failed.");
            await wait(() => executive.Microphone.ProviderStatus == "Speech server ready", "Vosk local health");
            report("PASS Spark Live text and local Vosk readiness.");
            executive.Ask("show me a transistor in 3d");
            var projector = GameObject.Find("SparkTeachingProjection");
            if (!projector || projector.GetComponentInChildren<SparkTerminal>() || projector.GetComponentInChildren<Collider>())
                throw new InvalidOperationException("Teaching projection leaks gameplay objects.");
            executive.Hologram.Clear();
            SparkUIManager.Instance.Open(SparkProLivePanel.PanelId);
            executive.SetSpeakAnswers(true);
            executive.Ask("what level am I on?");
            await wait(() => executive.PlaybackActive && executive.CharacterState == SparkState.Speaking, "Piper playback and speaking state");
            executive.Interrupt();
            if (executive.PlaybackActive) throw new InvalidOperationException("Interrupt left voice playback running.");
            report("PASS visual projection isolation, Piper playback, character speaking and interruption.");
        }

        private static void VerifySave()
        {
            string id = "phase4-player-check-" + Guid.NewGuid().ToString("N");
            string save = Path.Combine(Application.persistentDataPath, "SparkAcademy-" + id + ".json");
            GameObject first = null, second = null;
            try
            {
                SparkProAcademy Create(out GameObject owner)
                {
                    owner = new GameObject("Phase 4 isolated save fixture");
                    owner.SetActive(false);
                    var academy = owner.AddComponent<SparkProAcademy>();
                    typeof(SparkProAcademy).GetField("profileId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(academy, id);
                    owner.SetActive(true);
                    return academy;
                }
                var writer = Create(out first);
                string question = writer.StartPractice("ohm");
                var restored = Create(out second);
                if (!restored.IsPracticing || restored.ResumePractice() != question)
                    throw new InvalidOperationException("Practice reload did not recover the saved question.");
            }
            finally
            {
                if (first) Object.Destroy(first);
                if (second) Object.Destroy(second);
                foreach (string path in new[] { save, save + ".tmp", save + ".bak" })
                    if (File.Exists(path)) File.Delete(path);
            }
        }

        private static async Task Wait(Func<bool> ready, string label)
        {
            var deadline = DateTime.UtcNow.AddSeconds(45);
            while (!ready())
            {
                if (DateTime.UtcNow >= deadline) throw new TimeoutException(label);
                await Task.Delay(80);
            }
        }
    }
}
#endif
