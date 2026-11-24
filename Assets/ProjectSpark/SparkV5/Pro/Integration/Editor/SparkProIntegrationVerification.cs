using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using ProjectSpark.UIManagement;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    [InitializeOnLoad]
    internal static class SparkProIntegrationVerification
    {
        private const string MainMenu = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string RunFlag = "SparkPro.PlayVerification";
        private const string PreviousScene = "SparkPro.PlayVerification.StartScene";
        private const string Report = "Logs/SparkV5PlayMode.txt";

        static SparkProIntegrationVerification()
        {
            EditorApplication.playModeStateChanged += ObservePlayState;
        }

        [MenuItem("Project Spark/Spark V5 Pro/Create AI Integration Test Scene")]
        private static void CreateIntegrationScene()
        {
            const string directory = SparkProKnowledgeCompiler.Root + "/IntegrationTest";
            const string path = directory + "/MainMenu.unity";
            if (!AssetDatabase.IsValidFolder(directory))
                AssetDatabase.CreateFolder(SparkProKnowledgeCompiler.Root, "IntegrationTest");
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(path) && !AssetDatabase.CopyAsset(MainMenu, path))
                throw new InvalidOperationException("Could not create a copy of the saved MainMenu scene.");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            Debug.Log("[Spark Pro] Standalone integration scene copy created under " + directory +
                ". The verification command exercises the current saved production scene.");
        }

        [MenuItem("Project Spark/Spark V5 Pro/Run AI Integration Tests")]
        internal static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            Directory.CreateDirectory("Logs");
            File.WriteAllText(Report, "Spark V5 Pro integration verification\n");
            try
            {
                SparkProLiveBuilder.BuildLivePanel();
                SparkProLiveBuilder.RegisterTeachingModels();
                var catalog = SparkProKnowledgeCompiler.CompileCatalog(false);
                SparkProLiveBuilder.ValidateCharacter();
                SparkProIntegrationSetup.Test();
                Require(catalog.TryLevel(120, out var level) && level.inCampaign && level.chapterNumber > 0 &&
                    level.objectives.Length == 6, "Level 120 authored objectives and chapter");
                Require(catalog.TryLevel(89, out var archived) && !string.IsNullOrWhiteSpace(archived.sourceGuid),
                    "Authored Level 89 has its original asset identity");
                Record($"PASS native character LOD imports, prefab controls, lecture/practical contracts, {catalog.Count} authored level records.");

                SessionState.SetString(PreviousScene, AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
                SessionState.SetBool(RunFlag, true);
                EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenu);
                EditorApplication.EnterPlaymode();
            }
            catch (Exception error)
            {
                Record("FAIL " + error);
                throw;
            }
        }

        private static void ObservePlayState(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(RunFlag, false))
                return;
            if (state == PlayModeStateChange.EnteredPlayMode)
                VerifyPlay();
            if (state != PlayModeStateChange.EnteredEditMode)
                return;
            EditorSceneManager.playModeStartScene =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(SessionState.GetString(PreviousScene, ""));
            SessionState.EraseString(PreviousScene);
            SessionState.EraseBool(RunFlag);
        }

        private static async void VerifyPlay()
        {
            bool oldBackground = Application.runInBackground;
            try
            {
                Application.runInBackground = true;
                await Until(() => SparkUIManager.Instance && SparkUIManager.Instance.Find(SparkProLivePanel.PanelId),
                    "managed Spark Live panel registration");
                var manager = SparkUIManager.Instance;
                var executive = Object.FindFirstObjectByType<SparkProExecutive>();
                var levels = Object.FindFirstObjectByType<LevelGamePlayManager>();
                var assistant = Object.FindFirstObjectByType<SparkAIUnifiedAssistant>();
                var academy = Object.FindFirstObjectByType<SparkProAcademy>();
                Require(executive && levels && assistant && academy && executive.Microphone, "Production scene references");
                typeof(SparkProAcademy).GetField("persistProfile", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(academy, false);
                executive.SetSpeakAnswers(false);
                Require(executive.Microphone.State == SparkVoiceInputState.Off, "Microphone starts disabled");
                Require(manager.Open(SparkProLivePanel.PanelId), "Spark Live opens through the UI manager");
                var panel = Object.FindFirstObjectByType<SparkProLivePanel>();
                Require(panel && panel.Visible && manager.IsModalOpen, "Visible Live modal");
                Require(SparkProLivePanel.Install(executive, manager) == null, "Existing panel is reused");
                await Until(() => executive.Microphone.ProviderStatus.Contains("ready"), "local Vosk health");
                Require(levels.StartLevel(levels.GetFirstCampaignIndex()), "Load a real level through the game manager");
                await Until(() => SparkAILevelContext.Capture(levels).HasEvaluation, "committed live level evaluation");
                var composer = panel.GetComponentInChildren<TMP_InputField>();
                composer.text = "Explain Level 120";
                await Rendered(panel, "Send");
                Click(panel, "Send");
                Require(executive.LastAnswer.Contains("The Axis Forge") &&
                    executive.History.Count >= 2 && executive.History[0].Speaker == "You",
                    "Authored mission question through real UI");
                Require(executive.Microphone.State == SparkVoiceInputState.Off, "Text does not activate recording");
                Require(!assistant.PanelVisible, "Live submission keeps the legacy panel closed");
                var statusLabel = (TMP_Text)typeof(SparkProLivePanel).GetField("connection", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(panel);
                await Until(() => statusLabel.text == "Speech server ready", "visible speech status");
                await Capture("Logs/SparkV5Live.png");
                Record("PASS actual MainMenu modal, provider health, UI text submission and shared conversation history.");
                var endpoint = typeof(SparkProVoiceInput).GetField("endpoint", BindingFlags.Instance | BindingFlags.NonPublic);
                var originalEndpoint = endpoint.GetValue(executive.Microphone);
                executive.Microphone.SetHealthMonitoring(false);
                endpoint.SetValue(executive.Microphone, "http://127.0.0.1:1/transcribe");
                executive.Microphone.SetHealthMonitoring(true);
                await Until(() => executive.Microphone.ProviderStatus.Contains("unavailable"), "failed provider is displayed");
                executive.Ask("what level am I on?");
                Require(executive.LastAnswer.Contains("Level "), "Text fallback with unavailable provider");
                executive.Microphone.SetHealthMonitoring(false);
                endpoint.SetValue(executive.Microphone, originalEndpoint);
                executive.Microphone.SetHealthMonitoring(true);
                await Until(() => executive.Microphone.ProviderStatus.Contains("ready"), "provider health recovered");
                Record("PASS loopback provider disconnection, typed fallback and provider recovery.");

                executive.Ask("Hey Spark, explain the current mission");
                Require(executive.LastAnswer.Contains("chapter 1") && executive.LastAnswer.Contains("objectives"),
                    "Current chapter and evaluator-backed objective status");
                executive.Ask("practice ohm");
                Require(academy.IsPracticing, "Start an Academy exercise");
                string draft = executive.LastAnswer;
                executive.Ask("continue our lesson");
                Require(executive.LastAnswer == draft, "Resume same practice question");
                executive.Ask("practice hint");
                string firstHint = executive.LastAnswer;
                executive.Ask("practice hint");
                Require(executive.LastAnswer != firstHint, "Progressive Academy hints");
                executive.SetExaminationMode(true);
                executive.Ask("practice hint");
                Require(!academy.IsPracticing && executive.LastAnswer.Contains("Independent examination"),
                    "Executive assessment guard");
                Require(assistant.LocalAnswer("draw this level").Contains("Independent assessment"),
                    "Legacy local path shares the assessment guard");
                executive.SetExaminationMode(false);
                Record("PASS live chapter and objective evidence, Academy practice/resume and independent assessment restrictions.");

                executive.Ask("show me a transistor in 3d");
                Require(!panel.Visible && executive.Hologram.CurrentId == "transistor", "3D model request");
                var visual = GameObject.Find("SparkTeachingProjection");
                Require(visual && visual.GetComponentsInChildren<MeshRenderer>().Length > 0, "Projection includes mesh renderers");
                Require(visual.GetComponentsInChildren<Collider>().Length == 0 &&
                    visual.GetComponentsInChildren<SparkTerminal>().Length == 0, "Projection is isolated from gameplay");
                executive.Hologram.Zoom(1.25f);
                executive.Hologram.HighlightNextPart();
                executive.Hologram.ResetView();
                executive.Hologram.Clear();
                Require(string.IsNullOrEmpty(executive.Hologram.CurrentId), "Projection closes");

                manager.Open(SparkProLivePanel.PanelId);
                executive.SetSpeakAnswers(true);
                Require(executive.SpeechOutputAvailable, "Configured Windows Piper provider");
                executive.Ask("what level am I on?");
                await Until(() => executive.PlaybackActive, "real Piper playback", 20);
                Require(executive.Microphone.State == SparkVoiceInputState.Off, "Playback does not activate microphone");
                await Until(() => executive.CharacterState == SparkState.Speaking, "character follows actual audio playback");
                await Until(() => executive.SampleVoice(new float[256]) > .001f, "real audible output");
                executive.Ask("Wait, stop speaking");
                Require(!executive.PlaybackActive, "Interrupt stops audio");
                manager.Close(SparkProLivePanel.PanelId);
                Require(!panel.Visible && executive.Microphone.State == SparkVoiceInputState.Off, "Close stops capture and queued speech");
                Record("PASS visual-only models, Windows Piper playback, character speaking state, and playback interruption.");
                Record("PASS requested Unity integration checks completed. Physical microphone, Android devices, and full-duplex audio were outside this run.");
            }
            catch (Exception error) { Record("FAIL " + error); }
            finally
            {
                Application.runInBackground = oldBackground;
                EditorApplication.ExitPlaymode();
            }
        }

        private static async Task Rendered(SparkProLivePanel panel, string name)
        {
            await Until(() =>
            {
                var button = panel.GetComponentsInChildren<Button>().SingleOrDefault(item => item.name == name);
                return button && button.targetGraphic.depth >= 0;
            }, "rendered UI button " + name);
        }

        private static void Click(SparkProLivePanel panel, string name)
        {
            var button = panel.GetComponentsInChildren<Button>().Single(item => item.name == name);
            var rect = (RectTransform)button.transform;
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center))
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Require(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == button, "Pointer can reach " + name);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        private static async Task Until(Func<bool> condition, string action, float seconds = 7)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline)
                await Task.Delay(100);
            Require(condition(), "Timed out checking " + action);
        }

        private static async Task Capture(string path)
        {
            if (File.Exists(path)) File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            await Until(() => File.Exists(path), "Play Mode screenshot");
        }

        private static void Require(bool passed, string message)
        {
            if (!passed) throw new InvalidOperationException(message);
        }

        private static void Record(string message) => File.AppendAllText(Report, message + "\n");
    }
}

