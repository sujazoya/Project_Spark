using System;
using System.IO;
using System.Threading.Tasks;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using ProjectSpark.UIManagement;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build.Player;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    [InitializeOnLoad]
    internal static class SparkPhase4Verification
    {
        private const string Request = "Logs/SparkPhase4/run.request";
        private const string BuildRequest = "Logs/SparkPhase4/windows.request";
        private const string AndroidRequest = "Logs/SparkPhase4/android-compile.request";
        private const string Flag = "Spark.Phase4.Play";
        private const string Level113Checkpoint = ProjectSpark.Gameplay.RedundancyCitadel.SparkLevel113Process.CheckpointKey;
        private const string Report = "Logs/SparkSuperAIFinalVerification.txt";
        private const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";

        static SparkPhase4Verification()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Project Spark/Spark V5 Pro/Phase 4/Run Contract Checks")]
        private static void Queue() { Directory.CreateDirectory("Logs/SparkPhase4"); File.WriteAllText(Request, ""); }

        [MenuItem("Project Spark/Spark V5 Pro/Phase 4/Build Windows Validation Player")]
        private static void QueueBuild() { Directory.CreateDirectory("Logs/SparkPhase4"); File.WriteAllText(BuildRequest, ""); }

        [MenuItem("Project Spark/Spark V5 Pro/Phase 4/Compile Android Player Scripts")]
        private static void QueueAndroid() { Directory.CreateDirectory("Logs/SparkPhase4"); File.WriteAllText(AndroidRequest, ""); }

        private static void Poll()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (File.Exists(Request))
            {
                File.Delete(Request);
                File.WriteAllText(Report, "SPARK SUPER AI PHASE 4 / UNITY VERIFICATION" + Environment.NewLine);
                try
                {
                    var catalog = SparkProKnowledgeCompiler.CompileCatalog(false);
                    SparkPhase4TeachingExport.Run(catalog, Log);
                    SparkProLiveBuilder.RegisterTeachingModels();
                    SparkCortexChecks.StaticChecks(catalog, Log);
                    SparkPhase4RuntimeChecks.Contracts(Log);
                    SparkProLiveBuilder.ValidateCharacter();
                    SessionState.SetString(Flag + ".scene", EditorSceneManager.playModeStartScene ?
                        AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene) : "");
                    SessionState.SetBool(Flag, true);
                    EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(Scene);
                    EditorApplication.EnterPlaymode();
                }
                catch (Exception error) { Log("FAIL " + error); }
            }
            else if (File.Exists(AndroidRequest))
            {
                File.Delete(AndroidRequest);
                EditorApplication.delayCall += CompileAndroid;
            }
            else if (File.Exists(BuildRequest))
            {
                File.Delete(BuildRequest);
                EditorApplication.delayCall += BuildWindows;
            }
        }

        private static async void Play(PlayModeStateChange change)
        {
            if (!SessionState.GetBool(Flag, false)) return;
            if (change == PlayModeStateChange.EnteredEditMode)
            {
                if (SessionState.GetBool(Flag + ".checkpointSaved", false))
                {
                    if (SessionState.GetBool(Flag + ".checkpointExisted", false))
                        PlayerPrefs.SetString(Level113Checkpoint, SessionState.GetString(Flag + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(Level113Checkpoint);
                    PlayerPrefs.Save();
                    SessionState.EraseBool(Flag + ".checkpointSaved");
                    SessionState.EraseBool(Flag + ".checkpointExisted");
                    SessionState.EraseString(Flag + ".checkpoint");
                }
                SessionState.EraseBool(Flag);
                EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(SessionState.GetString(Flag + ".scene", ""));
                SessionState.EraseString(Flag + ".scene");
                Log("Returned to Edit Mode.");
            }
            if (change != PlayModeStateChange.EnteredPlayMode) return;
            bool background = Application.runInBackground;
            var filter = Debug.unityLogger.filterLogType;
            try
            {
                SessionState.SetBool(Flag + ".checkpointExisted", PlayerPrefs.HasKey(Level113Checkpoint));
                SessionState.SetString(Flag + ".checkpoint", PlayerPrefs.GetString(Level113Checkpoint, ""));
                SessionState.SetBool(Flag + ".checkpointSaved", true);
                Application.runInBackground = true;
                Debug.unityLogger.filterLogType = LogType.Error;
                await Wait(() => SparkUIManager.Instance && SparkUIManager.Instance.Find(SparkProLivePanel.PanelId), "Spark Live initialization");
                var executive = Object.FindFirstObjectByType<SparkProExecutive>();
                var manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
                var legacy = Object.FindFirstObjectByType<SparkAIUnifiedAssistant>();
                if (!executive || !manager || !legacy) throw new InvalidOperationException("MainMenu has missing AI dependencies.");
                var academy = new SerializedObject(Object.FindFirstObjectByType<SparkProAcademy>());
                academy.FindProperty("persistProfile").boolValue = false;
                academy.ApplyModifiedPropertiesWithoutUndo();
                if (manager.LevelProgression) manager.LevelProgression.enabled = false;
                var fixture = new SerializedObject(manager);
                fixture.FindProperty("enforceProgressionAccess").boolValue = false;
                fixture.FindProperty("autoAdvanceOnCompletion").boolValue = false;
                fixture.ApplyModifiedPropertiesWithoutUndo();
                executive.SetSpeakAnswers(false);
                using var main = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 8);
                using var allocations = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 8);
                await SparkPhase4RuntimeChecks.Campaign(manager, executive, Log, Wait);
                Log($"Profiler counters after chapter smoke: main thread {(main.Valid ? (main.LastValue / 1000000d).ToString("F2") + " ms" : "unavailable")}, " +
                    $"GC allocation {(allocations.Valid ? allocations.LastValue + " bytes" : "unavailable")}. Single Editor sample.");
                await SparkPhase4RuntimeChecks.Semantic(manager, executive, Log, Wait);
                await SparkCortexChecks.Play(executive, manager, legacy, Log, Wait);
                Log("Physical microphone devices detected by Unity: " + Microphone.devices.Length + ". Capture requires a spoken test.");
                await SparkPhase4PlayerChecks.UseLive(executive, manager, Log, Wait);
                Log("COMPLETE Editor integration checks.");
            }
            catch (Exception error) { Log("FAIL " + error); }
            finally
            {
                Application.runInBackground = background;
                Debug.unityLogger.filterLogType = filter;
                EditorApplication.ExitPlaymode();
            }
        }

        private static void CompileAndroid()
        {
            const string log = "Logs/SparkPhase4/AndroidCompile.txt";
            try
            {
                var settings = new ScriptCompilationSettings
                {
                    target = BuildTarget.Android, group = BuildTargetGroup.Android,
                    options = ScriptCompilationOptions.DevelopmentBuild
                };
                string output = Path.GetFullPath("Logs/SparkPhase4/AndroidScripts");
                Directory.CreateDirectory(output);
                var result = PlayerBuildInterface.CompilePlayerScripts(settings, output);
                bool exists = File.Exists(Path.Combine(output, "Assembly-CSharp.dll"));
                string summary = $"Android target scripts: {(exists ? "PASS" : "FAIL")}. " +
                    $"Architecture setting: {PlayerSettings.Android.targetArchitectures}.";
                File.WriteAllText(log, summary + Environment.NewLine);
                Log(summary);
            }
            catch (Exception error) { File.WriteAllText(log, error.ToString()); }
        }

        private static void BuildWindows()
        {
            string output = "Logs/SparkPhase4/WindowsPlayer/ProjectSpark.exe";
            Log($"Starting Windows development build at {DateTime.UtcNow:O}.");
            string speech = Environment.GetEnvironmentVariable("SPARK_WINDOWS_SPEECH_PACKAGE");
            try
            {
                var scenes = Array.FindAll(EditorBuildSettings.scenes, scene => scene.enabled);
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                if (string.IsNullOrWhiteSpace(speech) && Directory.Exists("Logs/SparkSpeechPackageValidated"))
                    Environment.SetEnvironmentVariable("SPARK_WINDOWS_SPEECH_PACKAGE", Path.GetFullPath("Logs/SparkSpeechPackageValidated"));
                var build = BuildPipeline.BuildPlayer(Array.ConvertAll(scenes, scene => scene.path), output,
                    BuildTarget.StandaloneWindows64, BuildOptions.Development | BuildOptions.CompressWithLz4);
                Log($"Windows build {build.summary.result}: {build.summary.totalErrors} errors, {build.summary.totalWarnings} warnings, " +
                    $"{build.summary.totalSize} bytes, {build.summary.totalTime}. Output {output}.");
            }
            catch (Exception error) { Log("FAIL Windows build " + error); }
            finally { Environment.SetEnvironmentVariable("SPARK_WINDOWS_SPEECH_PACKAGE", speech); }
        }

        private static async Task Wait(Func<bool> ready, string label)
        {
            var deadline = DateTime.UtcNow.AddSeconds(32);
            while (!ready())
            {
                if (DateTime.UtcNow >= deadline) throw new TimeoutException(label);
                await Task.Delay(55);
            }
        }

        private static void Log(string message) => File.AppendAllText(Report, message + Environment.NewLine);
    }
}
