#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using ProjectSpark.UIManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.SparkV5.Pro
{
    public static class SparkPhase4PlayerChecks
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void VerifyStandalone()
        {
#if !UNITY_EDITOR
            string[] arguments = Environment.GetCommandLineArgs();
            int flag = Array.IndexOf(arguments, "-spark-phase4-verify");
            if (flag > 0 && flag + 1 < arguments.Length) Run(arguments[flag + 1]);
#endif
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
