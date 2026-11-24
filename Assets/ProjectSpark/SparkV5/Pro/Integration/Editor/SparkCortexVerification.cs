using System;
using System.IO;
using System.Threading.Tasks;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using ProjectSpark.UIManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    [InitializeOnLoad]
    internal sealed class SparkCortexVerification : EditorWindow
    {
        private const string Menu = "Project Spark/Spark V5 Pro/Cortex/";
        private const string Report = "Logs/SparkCortexVerification.txt";
        private const string Request = "Logs/SparkCortexVerification.request";
        private const string MainMenu = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Session = "SparkCortex.Verification";
        static SparkCortexVerification()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem(Menu + "Run Cortex Tests")]
        private static void Queue() => File.WriteAllText(Request, "");

        [MenuItem(Menu + "Inspect Current Intelligence Context")]
        private static void Monitor() => GetWindow<SparkCortexVerification>("Spark Cortex");

        [MenuItem(Menu + "Audit Cortex Architecture")]
        private static void Audit()
        {
            var catalog = SparkProKnowledgeCompiler.CompileCatalog(false);
            SparkCortexChecks.StaticChecks(catalog, message => Debug.Log(message));
            SparkProLiveBuilder.ValidateAll();
        }

        private SparkProExecutive monitored;
        private bool ownsCapture;

        private void OnDisable()
        {
            if (ownsCapture && monitored && monitored.Microphone) monitored.Microphone.Stop();
            ownsCapture = false;
        }
        private void OnInspectorUpdate() { if (EditorApplication.isPlaying) Repaint(); }
        private void OnGUI()
        {
            if (!EditorApplication.isPlaying) monitored = null;
            else if (!monitored) monitored = Object.FindFirstObjectByType<SparkProExecutive>();
            var executive = monitored;
            EditorGUILayout.LabelField("Spark Cortex", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(executive ? executive.CortexStatus : "Open MainMenu in Play Mode to inspect active tool evidence.", MessageType.Info);
            if (!executive || !executive.Microphone) return;
            executive.SemanticEnabled = EditorGUILayout.ToggleLeft("Use optional local semantic routing", executive.SemanticEnabled);
            if (executive.SemanticEnabled)
            {
                EditorGUILayout.LabelField("Semantic gateway", executive.SemanticStatus);
                bool consent = EditorGUILayout.ToggleLeft("Send unresolved question text to the configured cloud service for this play session",
                    executive.SemanticCloudConsent);
                if (!consent) executive.SemanticCloudConsent = false;
                else if (!executive.SemanticCloudConsent)
                    executive.SemanticCloudConsent = EditorUtility.DisplayDialog("Cloud routing consent",
                        "If the optional gateway is configured for cloud access, it can send the text of your unresolved question to that provider. Circuit data, account information and recordings are excluded.",
                        "Allow for this session", "Keep local");
            }
            var microphone = executive.Microphone;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Physical microphone", $"{Microphone.devices.Length} detected · {microphone.ProviderStatus}");
            EditorGUILayout.LabelField(microphone.Status);
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), Mathf.Clamp01(microphone.InputLevel * 4f), "Live input");
            using (new EditorGUI.DisabledScope(Microphone.devices.Length == 0 || ownsCapture ||
                microphone.State != SparkVoiceInputState.Off))
            {
                if (GUILayout.Button("Record a push-to-talk sample"))
                {
                    ownsCapture = true;
                    microphone.BeginPushToTalk();
                }
                if (GUILayout.Button("Start hands-free test"))
                {
                    ownsCapture = true;
                    microphone.StartHandsFree();
                }
            }
            using (new EditorGUI.DisabledScope(!ownsCapture))
            {
                if (GUILayout.Button("Finish push-to-talk sample")) microphone.EndPushToTalk();
                if (GUILayout.Button("Mute / stop test")) { microphone.Stop(); ownsCapture = false; }
            }
            if (ownsCapture && microphone.State == SparkVoiceInputState.Off) ownsCapture = false;
            if (microphone.LastRecognitionConfidence.HasValue)
                EditorGUILayout.LabelField("Last recognition confidence", $"{microphone.LastRecognitionConfidence:P0}");

        }

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(Request);
            File.WriteAllText(Report, "SPARK CORTEX / WINDOWS UNITY EDITOR\n");
            try
            {
                var catalog = SparkProKnowledgeCompiler.CompileCatalog(false);
                SparkProLabBuilder.Ensure();
                SparkProLiveBuilder.RegisterTeachingModels();
                SparkCortexChecks.StaticChecks(catalog, Log);
                SessionState.SetString(Session + ".start", EditorSceneManager.playModeStartScene
                    ? AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene) : "");
                EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenu);
                SessionState.SetBool(Session, true);
                EditorApplication.EnterPlaymode();
            }
            catch (Exception error) { Log("FAIL " + error); }
        }

        private static async void Play(PlayModeStateChange phase)
        {
            if (!SessionState.GetBool(Session, false)) return;
            if (phase == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.EraseBool(Session);
                EditorSceneManager.playModeStartScene =
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(SessionState.GetString(Session + ".start", ""));
                SessionState.EraseString(Session + ".start");
                Log("Returned to Edit Mode.");
            }
            if (phase != PlayModeStateChange.EnteredPlayMode) return;
            bool background = Application.runInBackground;
            var logLevel = Debug.unityLogger.filterLogType;
            try
            {
                Application.runInBackground = true;
                Debug.unityLogger.filterLogType = LogType.Error;
                await Wait(() => SparkUIManager.Instance && SparkUIManager.Instance.Find(SparkProLivePanel.PanelId), "managed Spark Live");
                var executive = Object.FindFirstObjectByType<SparkProExecutive>();
                var manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
                var bridge = Object.FindFirstObjectByType<SparkAIUnifiedAssistant>();
                if (!executive || !manager || !bridge) throw new InvalidOperationException("Native MainMenu dependencies are missing.");
                var academy = Object.FindFirstObjectByType<SparkProAcademy>();
                var academyData = new SerializedObject(academy);
                academyData.FindProperty("persistProfile").boolValue = false;
                academyData.ApplyModifiedPropertiesWithoutUndo();
                if (manager.LevelProgression) manager.LevelProgression.enabled = false;
                var fixture = new SerializedObject(manager);
                fixture.FindProperty("enforceProgressionAccess").boolValue = false;
                fixture.FindProperty("autoAdvanceOnCompletion").boolValue = false;
                fixture.ApplyModifiedPropertiesWithoutUndo();
                executive.SetSpeakAnswers(false);
                await SparkCortexChecks.Play(executive, manager, bridge, Log, Wait);
                if (!SparkUIManager.Instance.Open(SparkProLivePanel.PanelId)) throw new InvalidOperationException("Spark Live panel did not open.");
                await Wait(() => executive.Microphone.ProviderStatus == "Speech server ready", "owned Vosk service startup");
                Log("PASS Windows speech service ready through the existing Spark Live panel.");
                executive.Ask("show me a transistor in 3d");
                var projection = GameObject.Find("SparkTeachingProjection");
                if (!projection || projection.GetComponentInChildren<SparkTerminal>() || projection.GetComponentInChildren<Collider>())
                    throw new InvalidOperationException("Render-only teaching projection was not isolated.");
                Log("PASS existing render-only hologram has no circuit terminals or colliders.");
                executive.Hologram.Clear();
                executive.SetSpeakAnswers(true);
                executive.Ask("what level am I on?");
                await Wait(() => executive.PlaybackActive && executive.CharacterState == SparkState.Speaking, "Piper playback and character state");
                executive.Interrupt();
                if (executive.PlaybackActive) throw new InvalidOperationException("Interrupt left voice playback running.");
                Log("PASS Piper voice playback, character speaking state and interruption.");
                Log("COMPLETE");
            }
            catch (Exception error) { Log("FAIL " + error); }
            finally
            {
                Application.runInBackground = background;
                Debug.unityLogger.filterLogType = logLevel;
                EditorApplication.ExitPlaymode();
            }
        }

        private static async Task Wait(Func<bool> condition, string label)
        {
            var deadline = DateTime.UtcNow.AddSeconds(35);
            while (!condition())
            {
                if (DateTime.UtcNow > deadline) throw new TimeoutException(label);
                await Task.Delay(55);
            }
        }

        private static void Log(string message) => File.AppendAllText(Report, message + Environment.NewLine);
    }
}
