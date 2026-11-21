using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.EmcGauntlet;
using SparkLevel112Process = ProjectSpark.Gameplay.EmcGauntlet.SparkLevel112Process;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.EmcGauntlet
{
    [InitializeOnLoad]
    internal static class Level112Acceptance
    {
        private const string Request = "Logs/Level112Acceptance.request";
        private const string Report = "Logs/Level112Acceptance.txt";
        private const string Session = "ProjectSpark.Level112.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel112Process board;

        static Level112Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/112/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE EMC GAUNTLET / EMC20-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel112Builder.Prefab);
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
                if (SessionState.GetBool(Session + ".checkpointCaptured", false))
                {
                    if (SessionState.GetBool(Session + ".hadCheckpoint", false))
                        PlayerPrefs.SetString(SparkLevel112Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel112Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel112Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel112Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel112Process.CheckpointKey);
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
            data.FindProperty("autoAdvanceOnCompletion").boolValue = false;
            data.FindProperty("autoEvaluate").boolValue = true;
            data.ApplyModifiedPropertiesWithoutUndo();
            var levels = data.FindProperty("levels");
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel112Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index == levels.arraySize - 1 && manager.LevelProgression && manager.LevelProgression.CanPlayLevel(index) &&
                manager.StartLevel(index), "MainMenu loads The EMC Gauntlet with normal access and existing campaign positions preserved.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board)
                && board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.Powered &&
                !board.JumperPresent && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Campaign starts at 0/6 with the board empty and W209 open.");
            Check(board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 416 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 16 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 68,
                "Serialized board has 416 route endpoints, 16 layers and 68 native via barrels.");
            Check(board.jumper.from.name == "U21.EMC_QUALIFIED_OUT" &&
                board.jumper.to.name == "U9.EMC_QUALIFIED_IN", "W209 maps to the intended U21 and U9 terminals.");
            var boardArt = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform
                .Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(boardArt.size.x - 10.13f) < .10f && Mathf.Abs(boardArt.size.z - 7.035f) < .09f &&
                Mathf.Abs(boardArt.center.x - center.x) < .04f && Mathf.Abs(boardArt.center.z - center.z) < .04f,
                $"Board art fits {boardArt.size.x:F2} × {boardArt.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            var legend = board.fixture.Find("EMC-20 placement legend").GetComponent<MeshFilter>();
            Check(legend.sharedMesh.vertexCount >= 152 * 12, "Authored footprint markings are visible at the original placements.");
            await Task.Delay(1100);
            await Screenshot("Level112Opening");
            board.SelectPart(1);
            Check(!board.FitPart(), "Wrong component is rejected.");
            board.SelectPart(0);
            board.RotatePart();
            Check(!board.FitPart() && !board.Solder(), "Wrong orientation and incomplete placement cannot be soldered.");
            board.SelectPart(0);
            Click("Fit part");
            Check(board.parts[0].model.activeSelf && !board.parts[0].contact.IsClosed, "Fit preview leaves the solder contact open.");
            Check(!board.Solder(), "Precision solder is blocked before contact inspection.");
            Click("Inspect contacts");
            Click("Solder");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && board.parts[0].model.activeSelf,
                "Assembly controls close a real contact.");
            await Wait(() => PlayerPrefs.HasKey(SparkLevel112Process.CheckpointKey), "saved assembly");
            var initial = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Reload The EMC Gauntlet for checkpoint verification.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board != initial && board.GetComponentInChildren<Canvas>(), "reloaded board");
            Click("Resume");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && !board.Powered && !board.BaselineReady,
                "Resume restores the solder contact with power isolated and training cleared.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                if (!board.FitPart() || ((i < 24 || board.parts[i].label.StartsWith("J", StringComparison.Ordinal)) && !board.InspectContacts()) || !board.Solder()) throw new InvalidOperationException("Assembly part " + i);
                if (i % 14 == 0) await Task.Yield();
            }
            Check(board.Installed == 152 && !board.ShieldsMounted && manager.SatisfiedTargetCount == 0,
                "The component count cannot pass assembly without the three original shield cans.");
            Click("Shield can 1");
            Click("Shield can 2");
            Check(!board.ShieldsMounted && !board.shieldBond.IsClosed, "Incomplete shield assembly keeps the native bond open.");
            Click("Shield can 3");
            Check(board.ShieldsMounted && board.shieldBond.IsClosed && board.shields.All(item => item.activeSelf),
                "All three removable shielding assemblies seat and close the nominal shield bond.");
            var shieldBounds = board.shields.Select(item => item.GetComponent<BoxCollider>().bounds).ToArray();
            Check(!shieldBounds[0].Intersects(shieldBounds[1]) && !shieldBounds[0].Intersects(shieldBounds[2]) &&
                !shieldBounds[1].Intersects(shieldBounds[2]) && board.shieldSurfaces.Length >= 3,
                "All three shield cans occupy distinct, clickable mounts.");
            await Stage(1);
            Check(board.ShieldsMounted && board.shieldBond.IsClosed && board.shieldSurfaces.All(surface => !surface.enabled),
                "The routing view exposes pads beneath the lids with all three native mounts intact.");
            var first = board.routes[0];
            board.SetLayer(first.layer % 8 + 1);
            Check(!board.PlateVia() && !board.ConnectPads(), "Wrong-layer copper and via actions are blocked.");
            board.SelectRoute(0);
            Check(!board.ConnectPads() && board.PlateVia(), "An open barrel blocks its required copper.");
            board.PadView();
            var camera = Camera.main;
            var start = camera.WorldToScreenPoint(first.from.transform.position);
            var end = camera.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(end), "Native Wire tool connects the exact pair of highlighted pads.");
            await Wait(() => board.Routed == 1 && board.Plated == 1, "first route and via registered");
            await Task.Delay(120);
            await Screenshot("Level112Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                var route = board.routes[i];
                board.SelectRoute(i);
                if (route.barrel && !board.PlateVia()) throw new InvalidOperationException(route.viaId);
                if (!board.ConnectPads()) throw new InvalidOperationException(route.id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 208 && board.Plated == 68 && !board.JumperPresent && !board.TogglePower(),
                "An assembled 208-route network still requires W209.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "W209 connected through the native circuit graph.");
            await Stage(2);
            Check(board.witnesses.All(pin => board.Voltage(pin) > 4.7f), "All seven native training continuity groups conduct.");
            Check(!board.CalibrateFixture() && !board.CaptureBaseline() && !board.RunSuppression(), "Unpowered measurements are rejected.");
            await PowerUp();
            Check(!board.ToggleShield(0) && !board.CaptureBaseline() && !board.RunSuppression(),
                "Energized shield service and uncalibrated measurements are blocked.");
            Check(board.CalibrateFixture(), "Calibrate native differential clock and fixture states.");
            Check(board.Calibrated && board.Voltage(board.fixtureSense) is > 1.59f and < 1.71f && !board.RunSuppression(),
                "Calibrated reference gain alone cannot replace a measured idle baseline.");
            Click("Capture baseline");
            await Stage(3);
            await Suppression();
            await Stage(4);
            Check(board.Qualifies && board.Errors == 0 && board.Packets == 512 && board.PayloadBytes == 8192 &&
                board.MeasuredBond < .060f && board.InsertionLoss >= 14f && board.MinimumEye >= 320f,
                "Suppression, bond, return and eye pass with 512 finite received packets.");
            float original24 = board.BandConducted(4);
            Click("Spread spectrum");
            Check(board.SpreadSpectrum && !board.Qualifies && !board.RunSuppression(), "Native U6 changes invalidate the previous detector epoch.");
            await Requalify();
            Check(board.Qualifies && board.BandConducted(4) < original24 - 1f,
                "Spread spectrum reduces the observed 24 MHz band under the same load.");
            Click("Spread spectrum");
            await Requalify();

            for (int fault = 0; fault < 3; fault++)
            {
                int oldEpoch = board.Epoch;
                Click("Fault trial");
                Check(!board.Qualifies && !board.LocateFault(), "Fault injection invalidates cached evidence and speculative diagnosis.");
                await Requalify();
                Check(!board.Qualifies, "The actual fault blocks current suppression qualification.");
                if (fault == 0)
                {
                    Check(!board.shieldBond.IsClosed && board.MeasuredBond > .25f && board.PeakRadiated > 41f && board.Errors == 0,
                        "A real seam opening increases bond impedance and radiated proxy while packet data still pass.");
                }
                else if (fault == 1)
                {
                    Check(board.MeasuredCurrent > .65f && board.MeasuredChoke < 1.4f && board.Voltage(board.chokeSense) > 2.85f &&
                        board.InsertionLoss < 14 && board.PeakConducted > 49f && board.Errors == 0,
                        "L1 saturates at solved workload current and recovers on idle, reducing loaded insertion loss and failing conducted emissions.");
                }
                else
                {
                    Check(!board.routes[board.returnRoute].barrel.IsConducting && board.Plated == 67 && board.Routed == 207 &&
                        board.ReturnOpen && board.MinimumEye < 320f && board.Errors > 0 && board.RailsReady,
                        "V44 opens the real reference path, collapses the sampled eye and damages counted packets with the bench powered.");
                }
                Click("Locate fault");
                Check(board.FaultWitnessed && !board.Repair(), "Measured diagnosis localizes the fault and blocks energized repairs.");
                await Task.Delay(180);
                await Screenshot("Level112Fault" + (fault + 1));
                Click("Bench power");
                if (fault == 0)
                {
                    board.SelectFootprint(0);
                    Check(!board.Repair(), "The wrong component cannot rework the diagnosed seam.");
                    board.InspectShield(0);
                    Check(!board.Repair(), "The mounted shield must be removed for seam service.");
                    Click("Shield can 1");
                    Check(!board.shields[0].activeSelf && !board.TogglePower(), "The unmounted shield blocks bench power.");
                    Click("Apply rework");
                    Click("Shield can 1");
                    Check(board.shields[0].activeSelf && board.shieldBond.IsClosed, "Remounting the rebuilt seam closes the native bond.");
                }
                else if (fault == 1)
                {
                    board.SelectFootprint(0);
                    Check(!board.Repair(), "The wrong footprint cannot replace L1.");
                    board.SelectFootprint(board.chokePart);
                    Click("Apply rework");
                }
                else
                {
                    board.SetLayer(7);
                    Check(!board.Repair() && !board.PlateVia(), "Wrong-layer or generic via actions cannot repair V44.");
                    board.SetLayer(6);
                    Click("Apply rework");
                }
                await PowerUp();
                await Requalify();
                await Wait(() => board.Repaired == fault + 1, "fresh repair evidence");
                Check(board.Qualifies && board.Epoch > oldEpoch && board.Errors == 0 && board.Packets == 512,
                    "Fresh baseline, suppression, eye and packet comparisons verify the repair.");
            }
            await Stage(5);
            await Wait(() => board.Fresh && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f, "U9 independent permit");
            int previousEpoch = board.Epoch;
            Click("Open W209");
            Check(!manager.CircuitSystem.HasConnectionBetween(board.jumper.from, board.jumper.to) && board.RailsReady && board.Inhibited &&
                board.Voltage(board.monitorSupply) > 3f && board.Voltage(board.protectedFeed) < .25f && !board.Qualifies && !board.ResetSafety(),
                "W209 opens the native edge, drops the protected relay and preserves the powered measurement bench.");
            Click("Restore W209");
            Check(board.Inhibited && !board.ResetSafety() && !board.RunSuppression(), "Physical reconnection cannot reuse EMC evidence or reset the latch.");
            await Requalify();
            await Wait(() => board.Fresh, "fresh pre-reset heartbeat");
            Check(board.Epoch > previousEpoch && board.Inhibited && manager.SatisfiedTargetCount == 5,
                "Fresh pre-reset evidence keeps the independent U9 latch inhibited.");
            int resetEpoch = board.Epoch;
            Click("Reset safety");
            Check(!board.Calibrated && !board.Qualifies && !board.RunSuppression(), "Explicit reset requires another complete qualification.");
            await Requalify();
            await Stage(6);
            Check(manager.IsCompleted && board.Repaired == 3 && board.Epoch > resetEpoch && !board.Inhibited &&
                board.Voltage(board.protectedFeed) > 3f && board.Qualifies, "Post-reset spectra and packets restore the actual U9 permit.");
            Click("Board overview");
            Click("Conducted");
            await Task.Delay(180);
            var plot = board.GetComponentInChildren<SparkLevel112TrainingPlot>();
            Check(plot && plot.TryGetComponent<CanvasRenderer>(out var renderer) && !renderer.cull, "The live conducted spectrum renders.");
            await Screenshot("Level112Complete");
            Click("Near-field");
            await Task.Delay(150);
            await Screenshot("Level112NearField");
            board.SetLayer(6);
            Click("Layer view");
            await Task.Delay(150);
            await Screenshot("Level112LayerView");
            board.SetLayer(1);
            for (int layer = 1; layer <= 16; layer++)
            {
                Check(board.routes.Any(route => route.copper.enabled) &&
                    board.routes.All(route => route.copper.enabled == (route.layer == layer)) &&
                    board.Powered && manager.IsCompleted, $"Live routed copper isolates layer {layer:00} without changing readiness.");
                if (layer < 16) Click("Next layer");
            }
        }

        private static async Task PowerUp()
        {
            Check(board.TogglePower(), "Enable protected bench and auxiliary power.");
            await Wait(() => board.RailsReady, "protected rail and auxiliary supply");
        }

        private static async Task Requalify()
        {
            Check(board.CalibrateFixture() && board.CaptureBaseline(), "Record fresh clock, fixture gain and idle spectrum.");
            await Suppression();
        }

        private static async Task Suppression()
        {
            int capture = board.Captures;
            Check(board.RunSuppression(), "Start loaded spectrum and independent packet comparison.");
            await Wait(() => board.Captures > capture && !board.Testing, "loaded EMC and packet evidence");
            Log(board.Evidence);
            Log($"Bond {board.MeasuredBond * 1000f:F1} mΩ, load {board.MeasuredCurrent:F2} A, L1 {board.MeasuredChoke:F2} V, eye {board.MinimumEye:F0} mV.");
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
                {
                    if (!board) throw new TimeoutException(step + ". Board unavailable.");
                    string rails = $"Bench {board.Voltage(board.standby):F3}, AUX {board.Voltage(board.aux):F3}, fixture {board.Voltage(board.fixtureSense):F3}";
                    throw new TimeoutException($"{step}. Stage {board.Stage}, epoch {board.Epoch}. {rails}. " +
                        $"Bond {board.MeasuredBond * 1000f:F1} mΩ, eye {board.MinimumEye:F0} mV. {board.Evidence}");
                }
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
