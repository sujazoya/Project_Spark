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

namespace ProjectSpark.EditorTools
{
    [InitializeOnLoad]
    internal static class Level98Acceptance
    {
        private const string Request = "Logs/Level98Acceptance.request";
        private const string Report = "Logs/Level98Acceptance.txt";
        private const string Session = "ProjectSpark.Level98.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel98Process board;

        static Level98Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/106/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "TRANSIENT SHIELD / PIS6-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel98Builder.Prefab);
                Check(prefab && prefab.LevelDefinition && prefab.SceneBindings, "Serialized campaign prefab resolves.");
                Check(prefab.GetComponent<SparkLevel98Process>().profile != null, "Serialized transient profile resolves.");
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
                if (SessionState.GetBool(Session + ".checkpointCaptured", false))
                {
                    if (SessionState.GetBool(Session + ".hadCheckpoint", false))
                        PlayerPrefs.SetString(SparkLevel98Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel98Process.CheckpointKey);
                    PlayerPrefs.Save();
                    SessionState.EraseBool(Session + ".checkpointCaptured");
                    SessionState.EraseBool(Session + ".hadCheckpoint");
                    SessionState.EraseString(Session + ".checkpoint");
                }
                Log("Returned to Edit Mode.");
            }
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            var background = Application.runInBackground;
            var filter = Debug.unityLogger.filterLogType;
            Application.runInBackground = true;
            Debug.unityLogger.filterLogType = LogType.Error;
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel98Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel98Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel98Process.CheckpointKey);
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
            Check(manager && loader && wire, "Campaign and native Wire tool resolve.");
            if (manager.LevelProgression) manager.LevelProgression.enabled = false;
            var data = new SerializedObject(manager);
            data.FindProperty("enforceProgressionAccess").boolValue = false;
            data.FindProperty("autoAdvanceOnCompletion").boolValue = false;
            data.FindProperty("autoEvaluate").boolValue = true;
            data.ApplyModifiedPropertiesWithoutUndo();
            var levels = data.FindProperty("levels");
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel98Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index >= 0 && manager.StartLevel(index), "MainMenu loads Transient Shield.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board)
                && board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.Powered &&
                !board.JumperPresent && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Empty board starts at 0/6 with W075 open and power isolated.");
            Check(board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 148 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 4 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 16,
                "148 unique route endpoints, four layers and 16 native via barrels are serialized.");
            var boardArt = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform
                .Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(boardArt.size.x - 10.69f) < .10f && Mathf.Abs(boardArt.size.z - 6.75f) < .09f &&
                Mathf.Abs(boardArt.center.x - center.x) < .03f && Mathf.Abs(boardArt.center.z - center.z) < .03f,
                $"Board art fits {boardArt.size.x:F2} × {boardArt.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            await Screenshot("Level98Opening");
            board.SelectPart(1);
            Check(!board.FitPart(), "Wrong component is rejected.");
            board.SelectPart(0);
            board.RotatePart();
            Check(!board.FitPart() && !board.Solder(), "Wrong orientation and unsoldered placement are rejected.");
            board.SelectPart(0);
            Click("Fit part");
            Click("Solder");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && board.parts[0].model.activeSelf,
                "Player controls close a real assembly contact.");
            await Wait(() => PlayerPrefs.HasKey(SparkLevel98Process.CheckpointKey), "saved assembly");
            var initial = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Restart Transient Shield to verify the checkpoint.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board != initial && board.GetComponentInChildren<Canvas>(), "reloaded board");
            Click("Resume");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && !board.Powered,
                "Resume restores the assembly contact with power isolated.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                if (!board.FitPart() || !board.Solder()) throw new InvalidOperationException("Assembly part " + i);
                if (i % 14 == 0) await Task.Yield();
            }
            await Stage(1);
            var first = board.routes[0];
            board.SetLayer(first.layer % 4 + 1);
            Check(!board.PlateVia() && !board.ConnectPads(), "Wrong-layer copper and vias are rejected.");
            board.SelectRoute(0);
            Check(!board.ConnectPads() && board.PlateVia(), "Route cannot bypass its open via.");
            board.PadView();
            var camera = Camera.main;
            var start = camera.WorldToScreenPoint(first.from.transform.position);
            var end = camera.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(end), "Native Wire gesture connects the selected pads.");
            await Screenshot("Level98Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                var route = board.routes[i];
                board.SelectRoute(i);
                if (route.barrel && !board.PlateVia()) throw new InvalidOperationException(route.viaId);
                if (!board.ConnectPads()) throw new InvalidOperationException(route.id);
                if (i % 14 == 0) await Task.Yield();
            }
            Check(board.Routed == 74 && board.Plated == 16 && !board.JumperPresent && !board.TogglePower(), "Copper and vias cannot substitute for W075.");
            board.SelectRoute(board.senseRoute);
            Check(board.RemoveSelectedRoute() && board.Routed == 73 && board.ConnectPads(), "Removing a trace retracts native continuity.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "Native W075 link connects the U8 and U9 contacts.");
            await Stage(2);
            Check(board.witnesses.All(pin => board.Voltage(pin) > 11.5f), "Five construction branches conduct.");
            Check(board.TogglePower() && board.Voltage(board.standby) > 11.9f && board.Voltage(board.mainRail) < .2f,
                "12 V input and regulated rail sequence are distinct.");
            await Calibrate();
            await Stage(3);
            await Capture();
            await Stage(4);

            for (int fault = 0; fault < 3; fault++)
            {
                Check(board.BeginFaultTrial(), "Seed circuit fault " + (fault + 1) + ".");
                if (fault == 0) Check(board.bulkEsr.Resistance > .3f, "CP2 ESR increased on the real capacitor branch.");
                if (fault == 1) Check(board.routes[board.groundRoute].barrel.IsOpen && board.Plated == 15,
                    "V16 electrical barrel opened, reducing plated continuity.");
                if (fault == 2) Check(!manager.CircuitSystem.HasConnectionBetween(board.routes[board.senseRoute].from, board.sense),
                    "Fault removes the actual W074 Kelvin edge.");
                await Wait(() => board.Fresh && !board.Inhibited, "load permit during diagnosis");
                await Capture();
                Check(board.FaultWitnessed && !board.Repair() && board.TogglePower(),
                    "Native trace exposed the fault and live repair was rejected.");
                board.SelectFootprint(0);
                board.SelectRoute(0);
                Check(!board.Repair() && board.LocateFault() && board.Repair(), "Only the diagnosed target can be repaired.");
                Check(board.TogglePower(), "Return bench power after isolation and repair.");
                await Calibrate();
                await Capture();
                Check(board.Repaired == fault + 1, "Fresh captured profile qualifies the repair.");
            }
            await Stage(5);
            await Wait(() => board.Fresh && !board.Inhibited && board.LoadCurrent > .4f, "U9 protected load baseline");
            Click("Open W075");
            await Wait(() => board.Inhibited && board.LoadCurrent < .025f, "native W075 work inhibition");
            Check(!manager.CircuitSystem.HasConnectionBetween(board.jumper.from, board.jumper.to) &&
                board.Voltage(board.standby) > 11f && board.Voltage(board.mainRail) > 4.8f &&
                !board.RunProfiles() && !board.ResetSafety() && manager.SatisfiedTargetCount == 5,
                "W075 loss turns off actual load current while the 12 V and 5 V diagnostics survive.");
            Click("Restore W075");
            Check(board.JumperPresent && board.Inhibited && !board.ResetSafety(), "Jumper alone keeps the safety latch set.");
            await Calibrate();
            Check(board.Inhibited && !board.RunProfiles() && board.ResetSafety(), "New calibration and explicit reset are required.");
            await Wait(() => !board.Inhibited, "rearmed U9 load permit");
            await Capture();
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && !board.Inhibited &&
                board.GetComponentsInChildren<SparkResistor>().All(resistor => !resistor.IsBurned),
                "Native supervisor output and 6/6 campaign completion agree.");
            board.BoardView();
            await Screenshot("Level98Complete");
        }

        private static async Task Calibrate()
        {
            await Wait(() => board.Fresh && board.SenseValid &&
                (board.Inhibited || board.LoadCurrent is >= .40f and <= .61f), "fresh PIS-6 calibration readings");
            Check(board.Calibrate(), $"Calibrate {board.LoadVoltage:F3} V and {board.LoadCurrent:F3} A.");
        }

        private static async Task Capture()
        {
            Check(board.RunProfiles(), "Start a native load-step capture.");
            await Wait(() => !board.Busy, "capture completed");
            Log(board.Evidence.Replace("\n", " | "));
        }

        private static async Task Screenshot(string name)
        {
            string path = Path.GetFullPath("Logs/" + name + ".png");
            File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            await Wait(() => File.Exists(path) && new FileInfo(path).Length > 5000, name + " capture");
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
                    throw new TimeoutException($"{step}. Stage {board?.Stage}, VLOAD {board?.LoadVoltage:F3}, ILOAD {board?.LoadCurrent:F3}, sense {board?.SenseVoltage:F3}, fresh {board?.Fresh}, capture {board?.Evidence}, manager {manager?.SatisfiedTargetCount}, {manager?.LastEvaluationMessage}");
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
