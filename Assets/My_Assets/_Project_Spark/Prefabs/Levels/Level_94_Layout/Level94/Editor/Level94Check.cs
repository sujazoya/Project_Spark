#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
internal static class Level94Check
{
    private const string Request = "Logs/SparkLevel94Check.request";
    private const string Report = "Logs/SparkLevel94Check.txt";
    private const string Session = "Spark.Level94.Check";
    private const string Layout = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_94_Layout/Level_94_Layout.prefab";
    private const string Victory = "ProjectSpark.Tutorial.LevelWon.level_94";

    static Level94Check()
    {
        EditorApplication.update += Poll;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Session, false)) return;
            SessionState.EraseBool(Session);
            Verify();
        };
    }

    [MenuItem("Tools/Project Spark/Level 94/Verify Board Foundry")]
    private static void Start()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
            EditorApplication.isUpdating || SceneManager.GetActiveScene().name != "MainMenu") return;
        File.WriteAllText(Report, "Level 94 / The Board Foundry" + Environment.NewLine);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Layout);
        var layout = prefab ? prefab.GetComponent<SparkLevelLayout>() : null;
        if (!layout || !layout.LevelDefinition || layout.LevelDefinition.LevelId != "level_94" ||
            !prefab.GetComponent<SparkLevel94Process>())
        {
            Record("FAIL Active prefab or Level 94 identity is missing.");
            return;
        }
        SessionState.SetBool(Session, true);
        EditorApplication.EnterPlaymode();
    }

    private static void Poll()
    {
        if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
            EditorApplication.isUpdating || SceneManager.GetActiveScene().name != "MainMenu") return;
        File.Delete(Request);
        Start();
    }

    private static void Record(string message) => File.AppendAllText(Report, message + Environment.NewLine);

    private static void Check(bool condition, string context)
    {
        if (!condition) throw new InvalidOperationException(context);
    }

    private static async void Verify()
    {
        var background = Application.runInBackground;
        var oldLogFilter = Debug.unityLogger.filterLogType;
        string oldCheckpoint = PlayerPrefs.GetString(SparkLevel94Process.CheckpointKey, null);
        int oldVictory = PlayerPrefs.GetInt(Victory, -1);
        SparkLevel94Process board = null;
        try
        {
            Application.runInBackground = true;
            Debug.unityLogger.filterLogType = LogType.Error;
            PlayerPrefs.DeleteKey(SparkLevel94Process.CheckpointKey);
            var manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
            var settings = new SerializedObject(manager);
            settings.FindProperty("enforceProgressionAccess").boolValue = false;
            settings.ApplyModifiedPropertiesWithoutUndo();
            if (manager.LevelProgression) manager.LevelProgression.enabled = false;
            int index = Enumerable.Range(0, manager.LevelCount).Single(i =>
                manager.GetLevel(i) && manager.GetLevel(i).LevelId == "level_94");
            Check(manager.StartLevel(index), "Level 94 did not open from MainMenu.");

            async Task Wait(Func<bool> complete, string context)
            {
                var deadline = DateTime.UtcNow.AddSeconds(22);
                while (!complete())
                {
                    Check(EditorApplication.isPlaying && DateTime.UtcNow < deadline,
                        $"{context} | {board?.detail?.text} | {board?.readings?.text} | {manager.LastEvaluationMessage}");
                    await Task.Delay(35);
                }
            }

            var wire = Object.FindFirstObjectByType<SignalWireBuilder>();
            var tool = Object.FindFirstObjectByType<SparkWireTool>(FindObjectsInactive.Include);
            var camera = Camera.main;
            Check(wire && tool && camera, "Native Wire tool and camera are required.");

            async Task FindBoard()
            {
                await Wait(() =>
                {
                    board = manager.LayoutLoader.ActiveLayout.GetComponent<SparkLevel94Process>();
                    return board && board.transform.Find("Board Foundry service console");
                }, "service console");
            }

            void Click(string name)
            {
                var button = board.GetComponentsInChildren<Button>().Single(item => item.name == name);
                Check(button.isActiveAndEnabled && button.interactable, "Player control unavailable: " + name);
                button.onClick.Invoke();
            }

            void Drag(SparkTerminal from, SparkTerminal to)
            {
                Vector2 start = camera.WorldToScreenPoint(from.transform.position);
                Vector2 end = camera.WorldToScreenPoint(to.transform.position);
                Check(tool.TryResolveTerminalTarget(start, out var hit) && hit.Collider.GetComponent<SparkTerminal>() == from,
                    "Native Wire tool could not select " + from.name);
                Check(wire.BeginWire(from, WirePolarity.Neutral), "Could not begin wire on " + from.name);
                wire.UpdateWireFromScreenPosition(end);
                Check(wire.TryEndWire(end), "Could not place wire on " + to.name);
            }

            Task At(int count) => Wait(() => manager.SatisfiedTargetCount == count, $"Expected {count}/6");

            await FindBoard();
            await At(0);
            Check(manager.ActiveTargetCount == 6 && board.parts.Length == 27 && board.parts.All(item => !item.activeSelf) &&
                board.routes.Length == 35 && board.routes.Count(item => item.via) == 6 && !board.supply.IsOutputActive,
                "Opening state is incomplete or credits construction.");
            Check(board.routes.Take(34).All(route => route.from && route.to && route.copper) &&
                board.routes[34].from.name == "J3.A" && board.routes[34].to.name == "J3.B", "PCB binding mismatch.");
            var substrate = board.arena.Find("L94_ART/L94_FR4BoardSubstrate").GetComponentsInChildren<Renderer>();
            var bounds = substrate[0].bounds;
            foreach (var renderer in substrate) bounds.Encapsulate(renderer.bounds);
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform
                .Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>().bounds;
            Check(bounds.size.x > table.size.x * .45f && bounds.size.x < table.size.x * .8f &&
                bounds.size.z > table.size.z * .60f && bounds.size.z < table.size.z * .91f,
                $"Board {bounds.size} does not fit the table {table.size}.");
            Record($"PASS Level 94 starts at 0/6. Board {bounds.size.x:F2} × {bounds.size.z:F2} fits the {table.size.x:F2} × {table.size.z:F2} table.");
            await Task.Delay(1300);
            foreach (float x in new[] { bounds.min.x, bounds.max.x })
            foreach (float z in new[] { bounds.min.z, bounds.max.z })
            {
                var corner = camera.WorldToViewportPoint(new Vector3(x, bounds.center.y, z));
                Check(corner.z > 0 && corner.x > .225f && corner.x < .98f && corner.y > .02f && corner.y < .98f,
                    $"Board overview is clipped or overlaps the console: {corner}.");
            }
            ScreenCapture.CaptureScreenshot("Logs/SparkLevel94Opening.png");
            await Task.Delay(180);

            for (int i = 0; i < 34; i++) board.InspectNet();
            await At(1);
            await Wait(() => board.GetComponentsInChildren<Button>().Any(item => item.name == "Browse stock"), "assembly controls");
            Click("Browse stock");
            Click("Fit part");
            Check(board.detail.text != null && !board.parts[0].activeSelf, "Wrong stock was installed.");
            Click("Match stock");
            Click("Fit part");
            Click("Solder");
            Check(!board.parts[0].activeSelf, "A misaligned placement was soldered.");
            for (int i = 0; i < 3; i++) board.RotatePart();
            board.FitPart();
            Check(!board.parts[0].activeSelf, "Fit credited an unsoldered part.");
            board.SolderPart();
            await Task.Delay(530);
            Check(PlayerPrefs.HasKey(SparkLevel94Process.CheckpointKey), "Partial assembly checkpoint was not saved.");
            Check(manager.RestartLevel(), "Could not reload Level 94.");
            await FindBoard();
            await Wait(() => board.GetComponentsInChildren<Button>().Any(item => item.name == "Resume checkpoint" && item.gameObject.activeInHierarchy),
                "Resume checkpoint button");
            Click("Resume checkpoint");
            await At(1);
            Check(board.parts[0].activeSelf && board.parts.Skip(1).All(item => !item.activeSelf) && !board.supply.IsOutputActive,
                "Checkpoint did not restore the partial build with power isolated.");
            Record("PASS Wrong stock, misalignment, and unsoldered placement are rejected. Resume restores the first soldered part.");

            for (int i = 1; i < 27; i++)
            {
                board.MatchPart();
                for (int turns = 1 + i % 3; turns < 4; turns++) board.RotatePart();
                board.FitPart();
                board.SolderPart();
            }
            await At(2);
            await Wait(() => board.GetComponentsInChildren<Button>().Any(item => item.name == "Wire tool"), "wire controls");
            Click("Wire tool");
            await Task.Delay(550);
            ScreenCapture.CaptureScreenshot("Logs/SparkLevel94Copper.png");

            board.BoardView();
            await Task.Delay(80);
            Drag(board.routes[0].from, board.routes[0].to);
            Check(manager.RestartLevel(), "Could not restart immediately after wiring.");
            await FindBoard();
            await Wait(() => board.GetComponentsInChildren<Button>().Any(item => item.name == "Resume checkpoint"), "copper checkpoint");
            Click("Resume checkpoint");
            await At(2);
            Check(manager.CircuitSystem.HasConnectionBetween(board.routes[0].from, board.routes[0].to) &&
                !board.supply.IsOutputActive, "Immediate restart lost a native wire from the queued checkpoint.");
            Record("PASS An immediate restart preserves W001 through the pending checkpoint.");

            for (int i = 1; i < 34; i++)
            {
                var route = board.routes[i];
                board.BoardView();
                await Task.Delay(65);
                if (route.via)
                {
                    board.BackCopper();
                    board.FitVia();
                    Check(!route.via.IsClosed, "A through-layer barrel was plated from the wrong face.");
                    Check(!tool.TryResolveTerminalTarget(camera.WorldToScreenPoint(route.from.transform.position), out _),
                        "A front endpoint was available while Back copper was selected.");
                    board.FrontCopper();
                    board.FitVia();
                    Drag(route.from, route.viaTop);
                    board.BackCopper();
                    Drag(route.viaBottom, route.to);
                }
                else Drag(route.from, route.to);
                await Task.Yield();
            }
            await At(3);
            Check(board.routes.Take(34).Where(item => item.via).All(item => item.via.IsClosed), "A required barrel remains open.");
            Record("PASS 34 native route gestures completed, including both segments of all six plated vias.");

            async Task Trusted() => await Wait(() => board.readings.text.Contains("W035 FEEDBACK   3."),
                "trusted solved W035 signal");
            Drag(board.routes[34].from, board.routes[34].to);
            board.TogglePower();
            await Trusted();
            board.TestBoard();
            await Task.Delay(120);
            await Wait(() => board.detail.text.Contains("fresh rail measurement"), "missing calibration rejection");
            board.Measure();
            board.TestBoard();
            await At(4);
            Record("PASS Commissioned through a fresh solved rail and load-current qualification.");

            var cases = new Action[] { board.InjectVia, board.InjectPullup, board.InjectBridge };
            for (int i = 0; i < cases.Length; i++)
            {
                cases[i]();
                board.Measure();
                board.Repair();
                await Wait(() => board.detail.text.Contains("Isolate the protected bench"), "live repair rejection");
                board.TogglePower();
                board.Repair();
                board.TogglePower();
                await Trusted();
                board.Measure();
                board.TestBoard();
                if (i < 2) await Wait(() => board.detail.text.Contains("Repair qualification passed"), "fresh repair qualification");
            }
            await At(5);
            Record("PASS Each of the three fault cases demanded diagnosis, isolation and an independent powered retest.");

            board.ArmJumper();
            board.UndoCopper();
            await Wait(() => board.readings.text.Contains("W035 FEEDBACK   invalid") &&
                board.readings.text.Contains("LOAD   INHIBITED") && board.readings.text.Contains("SHUNT   unavailable"),
                "physical W035 loss and load inhibit");
            Check(manager.SatisfiedTargetCount == 5 && !manager.IsCompleted, "Prior evidence was lost or a stale result completed the level.");
            Drag(board.routes[34].from, board.routes[34].to);
            await Trusted();
            board.Measure();
            board.ResetSafety();
            board.TestBoard();
            await At(6);
            await Wait(() => manager.IsCompleted, "campaign completion");
            Record("PASS 6/6 after physical W035 loss, native reconnection, fresh feedback measurement, reset and qualification.");
            await Task.Delay(450);
            ScreenCapture.CaptureScreenshot("Logs/SparkLevel94Complete.png");
            await Task.Delay(700);
            board.enabled = false;
        }
        catch (Exception error) { Record("FAIL " + error); if (board) board.enabled = false; }
        finally
        {
            if (string.IsNullOrEmpty(oldCheckpoint)) PlayerPrefs.DeleteKey(SparkLevel94Process.CheckpointKey);
            else PlayerPrefs.SetString(SparkLevel94Process.CheckpointKey, oldCheckpoint);
            if (oldVictory < 0) PlayerPrefs.DeleteKey(Victory);
            else PlayerPrefs.SetInt(Victory, oldVictory);
            PlayerPrefs.Save();
            Debug.unityLogger.filterLogType = oldLogFilter;
            Application.runInBackground = background;
            EditorApplication.ExitPlaymode();
        }
    }
}
#endif
