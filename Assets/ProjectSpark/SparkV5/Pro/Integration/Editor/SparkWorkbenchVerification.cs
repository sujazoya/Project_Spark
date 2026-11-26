using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    [InitializeOnLoad]
    internal static class SparkWorkbenchVerification
    {
        private const string Scene = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
        private const string Boot = "Assets/My_Assets/_Project_Spark/Scenes/Boot/Bootstrap.unity";
        private const string Report = "Logs/SparkWorkbenchVerification.txt";
        private const string FlightReport = "Logs/SparkFlightV13Repair/Verification.txt";
        private const string CableReportFolder = "Logs/SparkMasterAudit/NativeWireVisualRepair";
        private const string Request = "Logs/SparkWorkbench/run.request";
        private const string Flag = "Spark.Workbench.Verification";
        private const string Source = "Battery_negative";
        private const string Destination = "LED_Cathode";

        static SparkWorkbenchVerification()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += OnPlayMode;
        }

        [MenuItem("Project Spark/Spark V5 Pro/AAA Laboratory/Test Worktable Navigation and Wire Tool")]
        private static void Queue()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Request));
            File.WriteAllText(Request, "");
        }

[MenuItem("Project Spark/Spark V5 Pro/AAA Laboratory/Test Level 20 Flight and Native Wires")]
private static void QueueFlight()
{
    if (EditorApplication.isPlayingOrWillChangePlaymode)
    {
        EditorUtility.DisplayDialog(
            "Spark Level 20 Verification",
            "Stop Play Mode before running this test.\n\n" +
            "The verification tool will enter Play Mode " +
            "automatically and return to Edit Mode afterward.",
            "OK");

        return;
    }

    string requestPath = Path.GetFullPath(Request);

    Directory.CreateDirectory(
        Path.GetDirectoryName(requestPath));

    File.WriteAllText(requestPath, "flight");

    Debug.Log(
        "[Spark Verification] Level 20 flight test queued. " +
        "Starting Play Mode verification...");

    // Process the request on the next safe Editor callback.
    EditorApplication.delayCall += Poll;
}


        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(Request)) return;
            string request = File.ReadAllText(Request).Trim();
            bool testBoot = request == "boot";
            bool testCable = request == "cable" || request == "cable-before";
            bool testFlight = request == "flight" || testCable;
            File.Delete(Request);
            if (testFlight) Directory.CreateDirectory(Path.GetDirectoryName(FlightReport));
            if (testCable) Directory.CreateDirectory(CableReportFolder);
            SessionState.SetString(Flag + ".report", testCable
                ? Path.Combine(CableReportFolder, request + ".txt") : testFlight ? FlightReport : Report);
            File.WriteAllText(SessionState.GetString(Flag + ".report", Report),
                "Spark MainMenu verification " + DateTimeOffset.Now + Environment.NewLine);
            bool useOpenMainMenu = EditorSceneManager.GetActiveScene().path == Scene;
            var openScenes = EditorSceneManager.GetSceneManagerSetup();
            for (int i = 0; i < openScenes.Length; i++)
                if (!useOpenMainMenu && openScenes[i].isLoaded && EditorSceneManager.GetSceneByPath(openScenes[i].path).isDirty)
                {
                    Log("SKIPPED Open Unity scene has unsaved changes.");
                    return;
                }
            SessionState.SetString(Flag + ".previous", AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene) ?? "");
            SessionState.SetBool(Flag + ".boot", testBoot);
            SessionState.SetBool(Flag + ".flight", testFlight);
            SessionState.SetString(Flag + ".cable", testCable ? request : "");
            SessionState.SetBool(Flag, true);
            EditorSceneManager.playModeStartScene =
                useOpenMainMenu ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(testBoot ? Boot : Scene);
            EditorApplication.EnterPlaymode();
        }

        private static async void OnPlayMode(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Flag, false)) return;
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    SessionState.GetString(Flag + ".previous", ""));
                SessionState.EraseString(Flag + ".previous");
                SessionState.EraseBool(Flag + ".boot");
                SessionState.EraseBool(Flag + ".flight");
                SessionState.EraseString(Flag + ".cable");
                SessionState.EraseBool(Flag);
                Log("Returned to Edit Mode.");
                SessionState.EraseString(Flag + ".report");
            }
            if (state != PlayModeStateChange.EnteredPlayMode) return;

            bool background = Application.runInBackground;
            try
            {
                Application.runInBackground = true;
                bool startedAtBoot = SessionState.GetBool(Flag + ".boot", false);
                int originalScene = SceneManager.GetActiveScene().handle;
                if (startedAtBoot && SceneManager.GetActiveScene().path != Boot)
                    EditorSceneManager.LoadSceneInPlayMode(Boot, new LoadSceneParameters(LoadSceneMode.Single));
                await WaitFor(() =>
                {
                    var active = SceneManager.GetActiveScene();
                    return active.path == Scene && (!startedAtBoot || active.handle != originalScene);
                }, "Bootstrap to MainMenu");
                if (startedAtBoot) Log("PASS Bootstrap loaded the configured MainMenu scene.");
                if (SessionState.GetBool(Flag + ".flight", false))
                    await SparkFlightLevel20Verification.Verify(Log, SessionState.GetString(Flag + ".cable", ""));
                else
                    await Verify();
                Log("PASS Verification complete.");
            }
            catch (Exception error) { Log("FAIL " + error); }
            finally
            {
                Application.runInBackground = background;
                EditorApplication.ExitPlaymode();
            }
        }

        [MenuItem("Project Spark/Spark V5 Pro/AAA Laboratory/Test AAA Native Cable Visuals")]
        private static void QueueCableVisuals()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Request));
            File.WriteAllText(Request, "cable");
            EditorApplication.delayCall += Poll;
        }

        private static async Task Verify()
        {
            var directors = Object.FindObjectsByType<SparkNativeWireDirector>(FindObjectsSortMode.None);
            var manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
            Require(directors.Length == 1 && manager, $"Expected one native director and active manager, found {directors.Length} directors.");
            var director = directors[0];
            var executive = director.GetComponent<SparkProExecutive>();
            var port = director.GetComponent<SparkAINativeSignalWirePort>();
            var agent = director.GetComponent<NavMeshAgent>();
            Require(executive && port && agent, "Spark's native wiring components are absent.");
            var directorBindings = new SerializedObject(director);
            var workbench = directorBindings.FindProperty("workbench").objectReferenceValue as SparkWorkbenchNavigation;
            var builder = new SerializedObject(port).FindProperty("playerBuilder").objectReferenceValue as SignalWireBuilder;
            Require(workbench && builder && Object.FindObjectsByType<SparkAINativeSignalWirePort>(FindObjectsSortMode.None).Length == 1,
                "Expected one tabletop, one native wire port and its bound player builder.");
            var table = workbench.GetComponent<BoxCollider>().bounds;
            Log($"Table {table.size.x:F2} × {table.size.z:F2} at y={table.max.y:F3}. Spark starts at {agent.transform.position:F2}.");

            await WaitFor(() => executive.ActiveLevelManager == manager, "MainMenu Spark Live session binding");
            executive.SetSpeakAnswers(false);
            if (manager.LevelProgression) manager.LevelProgression.enabled = false;
            var managerSettings = new SerializedObject(manager);
            managerSettings.FindProperty("enforceProgressionAccess").boolValue = false;
            managerSettings.FindProperty("autoAdvanceOnCompletion").boolValue = false;
            managerSettings.ApplyModifiedPropertiesWithoutUndo();
            int level1 = manager.GetCampaignLevelIndices().Single(index => manager.GetLevel(index).LevelNumber == 1);
            int level2 = manager.GetCampaignLevelIndices().Single(index => manager.GetLevel(index).LevelNumber == 2);
            Require(manager.StartLevel(level1), "Level manager failed to start Level 1.");
            await WaitFor(() => SparkAILevelContext.Capture(manager).HasEvaluation && port.IsConfigured, "native layout and wire builder");

            var level = SparkAILevelContext.Capture(manager);
            var circuit = manager.CircuitSystem;
            var bindings = level.Layout.SceneBindings;
            bool sourceFound = bindings.TryGetTerminal(Source, out var a);
            bool destinationFound = bindings.TryGetTerminal(Destination, out var b);
            Require(sourceFound && destinationFound && a && b, "Level 1 return terminals are missing.");
            Require(!circuit.HasConnectionBetween(a, b), "Level 1 return terminals already connected.");
            Log($"Level 1 terminals {Source} {a.transform.position:F2}, {Destination} {b.transform.position:F2}.");

            Ask(executive, "draw wire from " + Source + " to " + Destination, "No circuit change yet");
            await Task.Delay(100);
            Require(!director.IsBusy && !builder.IsDrawing && !circuit.HasConnectionBetween(a, b),
                "Wire preview must not start movement or cable creation.");
            Log("PASS Preview waits for confirmation.");

            Ask(executive, "confirm wire", "walking to the source");
            Require(agent.isOnNavMesh && Mathf.Abs(agent.transform.position.y - table.max.y) < .2f,
                "Spark did not attach to the tabletop at its real elevation.");
            var start = agent.transform.position;
            await WaitFor(() => builder.IsDrawing || !director.IsBusy, "native source attachment");
            Require(builder.IsDrawing && !circuit.HasConnectionBetween(a, b), director.LastStatus);
            float traveled = Vector3.Distance(start, agent.transform.position);
            Require(traveled > .35f, "Spark's position did not move on the tabletop before source attachment.");
            Log($"PASS Spark reached the real source stance after moving {traveled:F2} units.");

            Ask(executive, "cancel wire", "cancelled");
            Require(!director.IsBusy && !builder.IsDrawing && !agent.hasPath && !circuit.HasConnectionBetween(a, b),
                "Cancel must release the builder and leave topology unchanged.");
            Log("PASS Cancel stopped movement and removed the in-progress cable.");

            Ask(executive, "draw wire from " + Source + " to " + Destination, "No circuit change yet");
            Ask(executive, "confirm wire", "walking to the source");
            var beforeDestination = agent.transform.position;
            await WaitFor(() => !director.IsBusy, "native route completion");
            Require(circuit.HasConnectionBetween(a, b), director.LastStatus);
            ulong wire = port.LastAIConnectionId;
            var registries = Object.FindObjectsByType<SparkWireRegistry>(FindObjectsSortMode.None);
            Require(wire != 0 && circuit.HasConnection(wire) && registries.Any(registry => registry.TryGetWire(wire, out _)),
                "Completed cable must exist in both native topology and the wire registry.");
            Require(agent.isOnNavMesh && !agent.hasPath && Vector3.Distance(beforeDestination, agent.transform.position) > .35f,
                "Spark did not walk to the destination stance.");
            Log($"PASS Spark walked to the destination and committed native visual/electrical wire {wire}.");
            Ask(executive, "undo last AI wire", "Last AI-native wire removed");
            Require(!circuit.HasConnectionBetween(a, b), "Undo left a circuit connection.");

            executive.SetExaminationMode(true);
            executive.Ask("draw wire from " + Source + " to " + Destination);
            Require(executive.LastAnswer.Contains("Independent examination") && !director.IsBusy &&
                !circuit.HasConnectionBetween(a, b), "Independent examination failed to block assistance.");
            executive.SetExaminationMode(false);
            Log("PASS Independent examination blocked wiring.");

            Ask(executive, "draw wire from " + Source + " to " + Destination, "No circuit change yet");
            var blocked = new GameObject("Verification obstruction");
            blocked.transform.SetParent(level.Layout.transform, true);
            blocked.transform.position = new Vector3(table.center.x, table.max.y + .3f, table.center.z);
            blocked.AddComponent<BoxCollider>().size = new Vector3(table.size.x + 1f, .6f, table.size.z + 1f);
            Physics.SyncTransforms();
            executive.Ask("confirm wire");
            Require(!director.IsBusy && !builder.IsDrawing && !circuit.HasConnectionBetween(a, b) &&
                !executive.LastAnswer.Contains("walking to the source"), "Clearance was bypassed by a blocked table.");
            Log("PASS Full tabletop obstruction blocked the action. " + executive.LastAnswer);
            blocked.SetActive(false);
            Object.Destroy(blocked);

            Ask(executive, "draw wire from " + Source + " to " + Destination, "No circuit change yet");
            Ask(executive, "confirm wire", "walking to the source");
            Require(manager.StartLevel(level2), "Level manager failed to switch to Level 2.");
            await WaitFor(() => !director.IsBusy, "level switch cancellation");
            Require(!builder.IsDrawing && !agent.hasPath, "Level switch left a wire gesture or walk active.");
            Log("PASS Switching levels cancelled the in-progress action.");
        }

        private static void Ask(SparkProExecutive executive, string question, string expected)
        {
            executive.Ask(question);
            Require(executive.LastAnswer.Contains(expected, StringComparison.OrdinalIgnoreCase), executive.LastAnswer);
        }

        private static async Task WaitFor(Func<bool> check, string stage)
        {
            float deadline = Time.realtimeSinceStartup + 45f;
            while (!check())
            {
                Require(Time.realtimeSinceStartup < deadline, "Timed out waiting for " + stage);
                await Task.Delay(20);
            }
        }

        private static void Require(bool condition, string failure)
        {
            if (!condition) throw new InvalidOperationException(failure);
        }

        private static void Log(string line) =>
            File.AppendAllText(SessionState.GetString(Flag + ".report", Report), line + Environment.NewLine);
    }
}
