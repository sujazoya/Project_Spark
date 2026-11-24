using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.RedundancyCitadel;
using SparkLevel113Process = ProjectSpark.Gameplay.RedundancyCitadel.SparkLevel113Process;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.RedundancyCitadel
{
    [InitializeOnLoad]
    internal static class Level113Acceptance
    {
        private const string Request = "Logs/Level113Acceptance.request";
        private const string Report = "Logs/Level113Acceptance.txt";
        private const string Session = "ProjectSpark.Level113.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel113Process board;

        static Level113Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/113/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || File.Exists("Logs/Level113Builder.request") || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE REDUNDANCY CITADEL / RDX21-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel113Builder.Prefab);
                Check(prefab && prefab.LevelDefinition && prefab.SceneBindings, "Serialized campaign prefab resolves.");
                SessionState.SetString(Session + ".start", EditorSceneManager.playModeStartScene
                    ? AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene) : "");
                EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(SparkLevel113Builder.Scene);
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
                EditorSceneManager.playModeStartScene =
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(SessionState.GetString(Session + ".start", ""));
                SessionState.EraseString(Session + ".start");
                if (SessionState.GetBool(Session + ".checkpointCaptured", false))
                {
                    if (SessionState.GetBool(Session + ".hadCheckpoint", false))
                        PlayerPrefs.SetString(SparkLevel113Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel113Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel113Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel113Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel113Process.CheckpointKey);
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
            Check(manager && loader && wire, "MainMenu resolves the live level manager, layout loader and Wire tool.");
            var data = new SerializedObject(manager);
            var levels = data.FindProperty("levels");
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel113Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize).Single(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target);
            int[] order = manager.GetCampaignLevelIndices().ToArray();
            int position = Array.IndexOf(order, index);
            string Id(int slot) => ((SparkLevelDefinition)levels.GetArrayElementAtIndex(slot).objectReferenceValue).LevelId;
            Check(position > 0 && position < order.Length - 1 && Id(order[position - 1]) == "level_112" &&
                Id(order[position + 1]) == "level_114", "Campaign orders Level 113 between Levels 112 and 114.");
            Check(index == levels.arraySize - 1, "Legacy progression slots retain their previous indices.");
            if (manager.LevelProgression) manager.LevelProgression.enabled = false;
            data.FindProperty("enforceProgressionAccess").boolValue = false;
            data.FindProperty("autoAdvanceOnCompletion").boolValue = false;
            data.FindProperty("autoEvaluate").boolValue = true;
            data.ApplyModifiedPropertiesWithoutUndo();
            Check(manager.StartLevel(index), "The campaign starts Level 113 through the native loader.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.JumperPresent &&
                !board.Powered && manager.SatisfiedTargetCount == 0, "Empty board, isolated bench and open W233 at 0/6.");
            Check(board.parts.Length == 166 && board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 464 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 18 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 76,
                "166 original footprints, 232 native copper pairs, 76 native via barrels and 18 layers resolve.");
            Check(board.jumper.from.name == "U15.TRUSTED_FAILOVER_READY_OUT" &&
                board.jumper.to.name == "U6.TRUSTED_FAILOVER_READY_IN", "W233 is the separate U15-to-U6 training edge.");
            var bounds = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(bounds.size.x - 10.13f) < .10f && Mathf.Abs(bounds.size.z - 7.20f) < .10f &&
                Vector2.Distance(new Vector2(bounds.center.x, bounds.center.z), new Vector2(center.x, center.z)) < .05f &&
                bounds.size.x < size.x && bounds.size.z < size.z,
                $"RDX-21 occupies {bounds.size.x:F2} × {bounds.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            await Screenshot("Level113Opening");

            board.SelectPart(1);
            Check(!board.FitPart(), "Wrong part is rejected.");
            board.SelectPart(0);
            board.RotatePart();
            Check(!board.FitPart(), "Wrong orientation is rejected.");
            board.SelectPart(0);
            Click("Fit part");
            Check(board.parts[0].model.activeSelf && !board.Solder(), "Precision packages require contact inspection.");
            Click("Inspect contacts");
            Click("Secure part");
            await Wait(() => PlayerPrefs.HasKey(SparkLevel113Process.CheckpointKey), "first placement checkpoint");
            var previousBoard = board;
            Check(manager.RestartLevel(), "The native manager can restart the loaded level.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board != previousBoard && board.GetComponentInChildren<Canvas>(), "fresh workbench after restart");
            Check(board.Installed == 0 && board.Resume() && board.Installed == 1 && board.parts[0].contact.IsClosed &&
                !board.Powered && !board.JumperPresent, "Resume restores the saved footprint with power isolated and W233 open.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                board.SelectFootprint(i);
                board.SelectPart(i);
                if (!board.FitPart()) throw new InvalidOperationException($"Could not fit {board.parts[i].label}");
                board.InspectContacts();
                if (!board.Solder()) throw new InvalidOperationException($"Could not secure {board.parts[i].label}");
            }
            await Stage(1);
            board.SelectRoute(0);
            board.SetLayer(2);
            Check(!board.PlateVia() && !board.ConnectPads(), "Wrong-layer work is rejected.");
            board.SelectRoute(0);
            Check(!board.ConnectPads() && board.PlateVia(), "A route with an open barrel cannot be closed.");
            var first = board.routes[0];
            board.PadView();
            var start = Camera.main.WorldToScreenPoint(first.from.transform.position);
            var end = Camera.main.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(end), "The native Wire tool connects the highlighted terminal objects.");
            await Wait(() => board.Routed == 1 && board.Plated == 1, "first native Wire route");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                board.SelectRoute(i);
                if (board.routes[i].barrel && !board.PlateVia() || !board.ConnectPads())
                    throw new InvalidOperationException("Could not build " + board.routes[i].id);
                if (i % 36 == 0) await Task.Yield();
            }
            Check(board.Routed == 232 && board.Plated == 76 && !board.JumperPresent &&
                manager.SatisfiedTargetCount == 1, "Complete copper alone cannot pass the W233 continuity check.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "Native circuit creates W233.");
            await Stage(2);
            bool powered = board.TogglePower();
            bool commissioned = powered && board.CommissionRails();
            string Reading(SparkTerminal pin) => $"{pin.name} {board.Voltage(pin):F3} V";
            Log("RAILS " + string.Join(", ", new[] { board.input, board.reserveInput, board.bus, board.rail5,
                board.rail33, board.monitorSupply }.Select(Reading)) +
                $"; diodes {board.primaryIsolation.ElectricalState.Current:F4}/{board.reserveIsolation.ElectricalState.Current:F4} A" +
                $"; Q switches {board.primaryPath.IsClosed}/{board.reservePath.IsClosed}; supply {board.source.State}/{board.reserve.State}");
            Check(commissioned, "Both inputs, reverse isolation and regulated rails are measured.");
            await Stage(3);
            await Qualify();
            Check(board.AcceptQualification(), "Signs the loaded transfer, ECC and watchdog evidence.");
            await Stage(4);

            for (int i = 0; i < 3; i++)
            {
                Check(board.BeginFaultTrial() && board.ActiveFault == i && board.LocateFault(), $"F{i + 1} produces the expected diagnostic signature.");
                Check(!board.Repair(), $"F{i + 1} rejects live rework.");
                Check(board.TogglePower(), "Isolates both input paths for repair.");
                Check(!board.Repair(), "Rework rejects an unselected fault site.");
                Check(board.SelectFaultSite(), "Locates the diagnosed board position.");
                if (i == 1)
                {
                    var ecc = board.routes[board.eccRoute];
                    board.SetLayer(ecc.exitLayer);
                    Check(!board.Repair(), "ECC rework rejects the wrong layer.");
                    board.SetLayer(ecc.layer);
                }
                Check(board.Repair() && board.TogglePower() && !board.AcceptQualification(), "Repair requires new evidence.");
                await Qualify();
                Check(board.AcceptQualification() && board.Repairs == i + 1, $"F{i + 1} qualifies after a fresh load and diagnosis run.");
            }
            await Stage(5);
            Check(!board.OpenW233() && board.ArmSafety() && board.OpenW233(), "Requires an armed, live W233 removal.");
            Check(board.Latched && board.Inhibited && !board.JumperPresent && board.Powered &&
                board.Voltage(board.monitorSupply) > 3.15f && board.Voltage(board.protectedFeed) < .25f,
                "Native circuit shows protected load inhibited while U6 diagnostics remain powered.");
            Check(!board.RestoreW233() && board.ObserveInhibit() && board.RestoreW233(), "Requires observing the independent latch before restoring W233.");
            Check(board.Latched && board.Inhibited && !board.ResetSafety(), "Reconnection alone cannot clear U6 or reuse stale permits.");
            await Qualify();
            Check(board.Latched && board.Inhibited && !board.AcceptQualification(),
                "Fresh evidence cannot bypass the explicit U6 reset.");
            Check(board.ResetSafety() && !board.Latched && !board.Inhibited && board.AcceptQualification(),
                "U6 resets only with a fresh permit and restores the native protected load.");
            await Stage(6);
            Check(manager.IsCompleted && manager.SatisfiedTargetCount == 6, "Native evaluator completed Level 113 with 6/6 objectives.");
            Check(!board.TogglePower(), "Completed inspection preserves the qualified power state.");
            board.AngleView();
            await Task.Delay(750);
            await Screenshot("Level113Complete");
        }

        private static async Task Qualify()
        {
            Check(board.CommissionRails() && board.StartTransfer(), "Loads the bus and starts timed reserve takeover.");
            await Wait(() => !board.Transferring, "loaded transfer");
            Check(board.TransferMilliseconds >= 79f && board.MinimumTransferVoltage > 10.5f,
                $"Simulated transfer {board.TransferMilliseconds:F0} ms, modeled hold-up minimum {board.MinimumTransferVoltage:F2} V.");
            Check(board.InjectEcc(1) && !board.ResolveEcc(true) && board.ResolveEcc(false), "Single-bit syndrome requires verified correction.");
            Check(board.InjectEcc(2) && !board.ResolveEcc(false) && board.ResolveEcc(true), "Double-bit syndrome requires quarantine.");
            Check(board.AuditTiming(), "Independent clock comparison and withheld-heartbeat watchdog pass.");
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
                    string rails = $"Bus {board.Voltage(board.bus):F3}, 5 V {board.Voltage(board.rail5):F3}, 3.3 V {board.Voltage(board.rail33):F3}";
                    throw new TimeoutException($"{step}. Stage {board.Stage}. {rails}.");
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

