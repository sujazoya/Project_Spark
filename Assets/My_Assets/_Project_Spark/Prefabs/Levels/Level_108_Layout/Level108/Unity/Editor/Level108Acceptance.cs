using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Test = ProjectSpark.Gameplay.SparkLevel108Process.Test;

namespace ProjectSpark.EditorTools
{
    [InitializeOnLoad]
    internal static class Level108Acceptance
    {
        private const string Request = "Logs/Level108Acceptance.request";
        private const string Report = "Logs/Level108Acceptance.txt";
        private const string Session = "ProjectSpark.Level108.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel108Process board;

        static Level108Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/108/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "GHOST IN THE CIRCUIT / DFX16-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel108Builder.Prefab);
                Check(prefab && prefab.LevelDefinition && prefab.SceneBindings, "Serialized campaign prefab resolves.");
                SessionState.SetBool(Session, true);
                EditorApplication.EnterPlaymode();
            }
            catch (Exception error) { Log("FAIL " + error); }
        }

        private static async void Play(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Session, false)) return;
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.EraseBool(Session);
                Log("Returned to Edit Mode.");
            }
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            var background = Application.runInBackground;
            var filter = Debug.unityLogger.filterLogType;
            Application.runInBackground = true;
            Debug.unityLogger.filterLogType = LogType.Error;
            try
            {
                await Run();
                Log("COMPLETE. Live LevelGamePlayManager evaluated all six objectives.");
            }
            catch (Exception error) { Log("FAIL " + error); }
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
            var loader = Object.FindFirstObjectByType<SparkLevelLayoutLoader>();
            var wire = Object.FindFirstObjectByType<SignalWireBuilder>();
            Check(manager && loader && wire, "MainMenu gameplay and Wire tool resolve.");
            if (manager.LevelProgression) manager.LevelProgression.enabled = false;
            var data = new SerializedObject(manager);
            data.FindProperty("enforceProgressionAccess").boolValue = false;
            data.FindProperty("autoAdvanceOnCompletion").boolValue = false;
            data.FindProperty("autoEvaluate").boolValue = true;
            data.ApplyModifiedPropertiesWithoutUndo();
            var levels = data.FindProperty("levels");
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel108Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index >= 0 && manager.StartLevel(index), "Campaign loads Level 108.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board.GetComponentInChildren<Canvas>(), "workbench controls");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.Powered &&
                !manager.CircuitSystem.HasConnectionBetween(board.jumper.from, board.jumper.to) &&
                board.parts.All(part => !part.model.activeSelf), "Empty start with W157 and bench power open.");
            var art = board.modelSpace.Find("L108_BoardArtRoot").GetComponentsInChildren<Renderer>();
            Bounds fixture = art[0].bounds;
            foreach (var renderer in art) fixture.Encapsulate(renderer.bounds);
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var tableSize = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(fixture.size.x - 10.13f) < .08f && Mathf.Abs(fixture.size.z - 7.51f) < .08f &&
                  Mathf.Abs(fixture.center.x - center.x) < .03f && Mathf.Abs(fixture.center.z - center.z) < .03f,
                $"Board {fixture.size.x:F2} × {fixture.size.z:F2} centered on {tableSize.x:F2} × {tableSize.z:F2} tabletop.");
            board.SelectPart(1);
            Check(!board.FitPart(), "Wrong component rejected.");
            board.SelectPart(0);
            Click("Fit part");
            Click("Solder");
            Check(board.Installed == 1 && board.parts[0].model.activeSelf, "Assembly buttons install a native contact and show the part.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                if (!board.FitPart() || !board.Solder()) throw new InvalidOperationException("Part assembly " + i);
                if (i % 15 == 0) await Task.Yield();
            }
            await Stage(1);

            var first = board.routes[0];
            Check(!board.ConnectPads() && board.PlateVia(), "An open via blocks the route until plated.");
            board.PadView();
            var camera = Camera.main;
            var a = camera.WorldToScreenPoint(first.from.transform.position);
            var b = camera.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(a) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(b), "World-space Wire tool connects the highlighted copper pads.");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                var route = board.routes[i];
                board.SelectRoute(i);
                if (route.via && !board.PlateVia()) throw new InvalidOperationException(route.viaId);
                if (!board.ConnectPads()) throw new InvalidOperationException(route.id);
                if (i % 15 == 0) await Task.Yield();
            }
            board.SelectRoute(board.routes.Length);
            Check(board.Routed == 156 && board.Plated == 50 && !board.TogglePower(), "All routes and vias alone cannot start without W157.");
            Check(board.ConnectPads(), "W157 connects through the native circuit graph.");
            await Stage(2);
            board.BoardView();
            Check(board.TogglePower(), "Bench disconnect closes.");
            await Capture(Test.Standby);
            await Stage(3);
            Check(!board.ConfirmDiagnosis(), "Standby symptom alone cannot authorize a repair.");
            await Capture(Test.Isolate);
            await Repair(0);

            await Capture(Test.ColdBoot);
            Check(!board.ConfirmDiagnosis(), "Cold boot alone cannot prove the warm DDR4 fault.");
            await Capture(Test.WarmMemory);
            await Repair(1);

            await Capture(Test.ColdBoot);
            await Capture(Test.PCIeClock);
            Check(board.ConfirmDiagnosis() && board.IsSupported(2), "PCIe clock contact confirmed with cold/stress evidence.");
            await Stage(4);
            await Repair(2, true);

            await Qualification();
            await Stage(5);
            await Wait(() => board.Fresh && !board.Inhibited, "native heartbeat and work output");
            Check(Voltage(board.protectedFeed) > 11f, "Solved protected operation is enabled before W157 interruption.");
            Click("Open W157");
            await Wait(() => board.Inhibited && Voltage(board.protectedFeed) < .5f, "independent native interlock");
            Check(!manager.CircuitSystem.HasConnectionBetween(board.jumper.from, board.jumper.to) && !board.ResetSafety(),
                "Open W157 removes the graph edge, de-energizes the load, and blocks reset.");
            Click("Restore W157");
            await Wait(() => board.Fresh, "new W157 pulse transitions");
            Check(board.Inhibited && !board.ResetSafety(), "Restored W157 requires fresh diagnostic qualification and explicit reset.");
            await Qualification();
            Check(board.Inhibited && board.ResetSafety(), "New cold, warm and PCIe evidence permits explicit reset.");
            await Stage(6);
            Check(manager.IsCompleted && Voltage(board.protectedFeed) > 11f, "Native supervisor output and campaign completion agree.");
            Check(board.GetComponentsInChildren<SparkResistor>().All(resistor => !resistor.IsBurned),
                "Diagnostic bench loads remain intact through all fault and recovery tests.");
            board.BoardView();
            string screenshot = Path.GetFullPath("Logs/Level108Complete.png");
            File.Delete(screenshot);
            ScreenCapture.CaptureScreenshot(screenshot);
            await Wait(() => File.Exists(screenshot), "completion screenshot");
        }

        private static float Voltage(SparkTerminal terminal)
        {
            var solve = manager.ElectricalSolver.LatestSolveResult;
            return solve != null && solve.TryGetVoltage(terminal, board.ground, out float value) ? value : 0;
        }

        private static async Task Repair(int fault, bool alreadyConfirmed = false)
        {
            Check((alreadyConfirmed || board.ConfirmDiagnosis()) && board.IsSupported(fault), $"F{fault + 1} requires and has comparative evidence.");
            Check(!board.Repair(), $"Energized rework rejected for F{fault + 1}.");
            Check(board.TogglePower(), "Isolate for component work.");
            board.SelectFootprint((board.repairParts[fault] + 1) % board.parts.Length);
            Check(!board.Repair(), "Wrong repair footprint rejected.");
            board.LocateRepair();
            Check(board.Repair() && board.IsRepaired(fault), $"Correct repair clears F{fault + 1}.");
            Check(board.TogglePower(), "Reapply bench power.");
            await Task.Yield();
        }

        private static async Task Qualification()
        {
            await Capture(Test.ColdBoot);
            await Capture(Test.WarmMemory);
            await Capture(Test.PCIeClock);
            Check(board.Qualified, "Cold POST, warm DDR4 and stressed PCIe all qualify.");
        }

        private static async Task Capture(Test test)
        {
            Check(board.Run(test), "Start " + test);
            await Wait(() => !board.Testing, "finish " + test);
            Log(board.LastCapture);
        }

        private static void Click(string name)
        {
            var button = board.GetComponentsInChildren<Button>().Single(item => item.name == name);
            Check(button.interactable, name + " is available.");
            button.onClick.Invoke();
        }

        private static async Task Stage(int value)
        {
            await Wait(() => board.Stage == value && manager.SatisfiedTargetCount == value, $"{value}/6 objectives");
            Log($"PASS Manager objective {value}/6.");
        }

        private static async Task Wait(Func<bool> ready, string step)
        {
            var until = DateTime.UtcNow.AddSeconds(24);
            while (!ready())
            {
                if (DateTime.UtcNow > until)
                    throw new TimeoutException($"{step}. Stage {board?.Stage}, manager {manager?.SatisfiedTargetCount}, {manager?.LastEvaluationMessage}");
                await Task.Delay(35);
            }
        }

        private static void Check(bool success, string label)
        {
            if (!success) throw new InvalidOperationException(label);
            Log("PASS " + label);
        }

        private static void Log(string label) => File.AppendAllText(Report, label + Environment.NewLine);
    }
}
