using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.ExpansionNexus;
using SparkLevel105Process = ProjectSpark.Gameplay.ExpansionNexus.SparkLevel105Process;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.ExpansionNexus
{
    [InitializeOnLoad]
    internal static class Level105Acceptance
    {
        private const string Request = "Logs/Level105Acceptance.request";
        private const string Report = "Logs/Level105Acceptance.txt";
        private const string Session = "ProjectSpark.Level105.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel105Process board;

        static Level105Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/105/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "EXPANSION NEXUS / ENX13-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel105Builder.Prefab);
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
                        PlayerPrefs.SetString(SparkLevel105Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel105Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel105Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel105Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel105Process.CheckpointKey);
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
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel105Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index == levels.arraySize - 1 && manager.LevelProgression && manager.LevelProgression.CanPlayLevel(index) &&
                manager.StartLevel(index), "MainMenu launches Expansion Nexus with normal access checks and its own progression position.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board)
                && board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && board.Card == SparkLevel105Process.CardPosition.Detached && !board.Powered &&
                !board.JumperPresent && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Campaign starts at 0/6 with the board empty and W133 open.");
            Check(board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 264 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 8 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 38,
                "Serialized board has 264 route endpoints, eight layers and 38 native via barrels.");
            Check(board.jumper.from.name == "U12.TRUSTED_EXPANSION_READY_OUT" &&
                board.jumper.to.name == "U13.TRUSTED_EXPANSION_READY_IN", "W133 maps to the intended U12 and U13 terminals.");
            var boardArt = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform
                .Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(boardArt.size.x - 10.13f) < .10f && Mathf.Abs(boardArt.size.z - 7.129f) < .09f &&
                Mathf.Abs(boardArt.center.x - center.x) < .04f && Mathf.Abs(boardArt.center.z - center.z) < .04f,
                $"Board art fits {boardArt.size.x:F2} × {boardArt.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            var looseCard = board.cardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((bounds, next) => { bounds.Encapsulate(next); return bounds; });
            Check(board.parts.Count(part => part.onCard) == 6 && !looseCard.Intersects(boardArt) &&
                looseCard.max.x < center.x + size.x * .5f && looseCard.min.z > center.z - size.z * .5f &&
                looseCard.max.z < center.z + size.z * .5f, "The separate ECX-4 tray and all six card placements fit the same table.");
            var mainLegend = board.fixture.Find("ENX-13 placement legend").GetComponent<MeshFilter>().sharedMesh;
            var cardLegend = board.card.Find("ECX-4 placement legend").GetComponent<MeshFilter>().sharedMesh;
            Check(mainLegend.vertexCount >= 94 * 12 && cardLegend.vertexCount >= 6 * 12 && !board.cardSurface.enabled,
                "Both boards show authored footprint guides and preserve unobstructed assembly selection.");
            await Task.Delay(1100);
            await Screenshot("Level105Opening");
            board.SelectPart(1);
            Check(!board.FitPart(), "Wrong component is rejected.");
            board.SelectPart(0);
            board.RotatePart();
            Check(!board.FitPart() && !board.Solder(), "Wrong orientation and incomplete placement cannot be soldered.");
            board.SelectPart(0);
            Click("Fit part");
            Check(board.parts[0].model.activeSelf && !board.parts[0].contact.IsClosed, "Fit preview leaves the solder contact open.");
            Check(!board.Solder(), "BGA solder is blocked before alignment inspection.");
            Click("Inspect BGA");
            Click("Solder");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && board.parts[0].model.activeSelf,
                "Assembly controls close a real contact.");
            await Wait(() => PlayerPrefs.HasKey(SparkLevel105Process.CheckpointKey), "saved assembly");
            var initial = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Reload Expansion Nexus for checkpoint verification.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board != initial && board.GetComponentInChildren<Canvas>(), "reloaded board");
            Click("Resume");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && !board.Powered && !board.FullLink,
                "Resume restores the solder contact with power isolated and training cleared.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                if (!board.FitPart() || (i == 1 && !board.InspectBga()) || !board.Solder()) throw new InvalidOperationException("Assembly part " + i);
                if (i % 14 == 0) await Task.Yield();
            }
            await Stage(1);
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
            await Screenshot("Level105Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                var route = board.routes[i];
                board.SelectRoute(i);
                if (route.barrel && !board.PlateVia()) throw new InvalidOperationException(route.viaId);
                if (!board.ConnectPads()) throw new InvalidOperationException(route.id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 132 && board.Plated == 38 && !board.JumperPresent && !board.TogglePower(),
                "An assembled 132-route network still requires W133.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "W133 connected through the native circuit graph.");
            await Stage(2);
            Check(board.witnesses.All(pin => board.Voltage(pin) > 11.5f), "All seven continuity domains conduct.");
            await Wait(() => board.cardSurface.enabled, "the ECX-4 inspection target");
            Vector3 previousView = Camera.main.transform.position;
            board.cardArt.GetComponent<SparkLevel105CardInspector>().OnPointerClick(new PointerEventData(EventSystem.current)
                { button = PointerEventData.InputButton.Left });
            Check(Vector3.Distance(Camera.main.transform.position, previousView) > .1f,
                "The parked ECX-4 has a physical target that opens the slot inspection view.");
            Check(!board.TogglePower() && !board.SeatCard() && !board.Train(), "Detached ECX-4 cannot train or skip ground-first alignment.");
            Click("Align card");
            Check(board.Card == SparkLevel105Process.CardPosition.Grounded && board.groundContact.IsConducting &&
                board.presenceContact.IsConducting && !board.powerContact.IsConducting &&
                board.slotContacts.All(contact => !contact.IsConducting), "Card ground and presence engage before power and signal contacts.");
            Click("Bench power");
            await Wait(() => board.Voltage(board.presence) > 3, "ground-first presence measurement");
            Check(!board.CardPowered && !board.ClockReady && !board.Train() && !board.RemoveCard() && !board.SeatCard(),
                "Partially engaged presence cannot train. Hot mechanical actions are blocked.");
            Click("Bench power");
            Click("Seat ECX-4");
            Check(board.Card == SparkLevel105Process.CardPosition.Seated &&
                board.slotContacts.All(contact => contact.IsConducting), "Mechanical seating closes the individual sideband, clock and lane contacts.");
            await Task.Delay(180);
            await Screenshot("Level105Seated");
            await PowerUp();
            Check(!board.ReadConfiguration() && !board.RunTraffic(), "Clock and reset alone cannot enumerate or transfer.");
            await TrainHealthy();
            await Stage(3);
            Check(Enumerable.Range(0, 4).All(i => board.TxMargin(i) >= 42f && board.RxMargin(i) >= 42f),
                "Both polarities of four bidirectional lane pairs qualify at ×4 and 5.0 GT/s.");
            Check(!board.AssignResources() && !board.RunTraffic(), "Resource assignment and traffic require protocol enumeration in order.");
            await EnumerateAndTransfer();
            await Stage(4);
            Check(board.Qualifies && board.Frames == 128 && board.PayloadBytes == 4096 && board.Errors == 0,
                "The enumerated endpoint verifies 128 sequence-tagged frames and 4,096 bytes in both directions.");

            for (int fault = 0; fault < 3; fault++)
            {
                int oldEpoch = board.Epoch;
                Click("Fault trial");
                Check(!board.Qualifies && !board.Enumerated && !board.LocateFault(), "Fault injection invalidates prior evidence and blocks unmeasured repairs.");
                Check(board.Train(), "Start a fresh link diagnostic.");
                await Wait(() => !board.Training, "fault training result");
                if (fault == 0)
                {
                    Check(!board.coupling.IsConducting && board.Link == SparkLevel105Process.LinkState.L0 && board.Width == 2 &&
                        board.TxMargin(2) == 0 && board.RxMargin(2) >= 42 && !board.ReadConfiguration(),
                        "C22 opens the native lane 2 TX path. ×2 fallback cannot pass ×4 enumeration.");
                }
                else if (fault == 1)
                {
                    Check(!board.clockBuffer.IsConducting && !board.ClockReady && !board.ResetReleased &&
                        board.Link == SparkLevel105Process.LinkState.Fault && board.CardPowered,
                        "The missing negative REFCLK state holds reset and blocks Polling with the endpoint still powered.");
                }
                else
                {
                    Check(board.FullLink, "The intermittent slot fault permits initial training.");
                    await EnumerateAndTransfer();
                    Check(board.Link == SparkLevel105Process.LinkState.Recovery && board.FailingLane == 3 &&
                        board.FaultContactOpens > 0 && board.Errors > 0 && !board.Qualifies,
                        "The native lane 3 contact opens under traffic and produces actual lane-specific CRC errors.");
                }
                Click("Locate fault");
                Check(board.FaultWitnessed && !board.Repair(), "Measured diagnosis locates the exact part and blocks hot rework.");
                await Task.Delay(170);
                await Screenshot("Level105Fault" + (fault + 1));
                Click("Bench power");
                if (fault == 2)
                {
                    Check(!board.Repair(), "The intermittent contact cannot be repaired without physical reseating.");
                    Click("Remove card");
                    Click("Align card");
                    Click("Seat ECX-4");
                }
                else
                {
                    board.SelectFootprint(0);
                    Check(!board.Repair(), "A wrong component target cannot repair the measured fault.");
                    board.SelectFootprint(fault == 0 ? board.couplingPart : board.clockPart);
                }
                Click("Apply rework");
                await PowerUp();
                await TrainHealthy();
                await EnumerateAndTransfer();
                await Wait(() => board.Repaired == fault + 1, "reworked fault verified");
                Check(board.Qualifies && board.Epoch > oldEpoch && board.Errors == 0, "New clock, link, enumeration and clean data verify this repair.");
            }
            await Stage(5);
            await Wait(() => board.Fresh && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f, "qualified U13 permit");
            int before = board.Epoch;
            Click("Open W133");
            Check(!manager.CircuitSystem.HasConnectionBetween(board.jumper.from, board.jumper.to) && board.CardPowered &&
                board.ClockReady && board.Voltage(board.monitorSupply) > 3f && board.Inhibited &&
                board.Voltage(board.protectedFeed) < .25f && !board.Qualifies && !board.ResetSafety(),
                "W133 physically opens and the independent U13 permit drops while REFCLK and ECX-4 remain powered.");
            Click("Restore W133");
            Check(board.Inhibited && !board.ResetSafety() && !board.RunTraffic(),
                "Restoring the wire cannot reuse stale link, enumeration or traffic evidence.");
            await TrainHealthy();
            await EnumerateAndTransfer();
            await Wait(() => board.Fresh, "new W133 edge cadence");
            Check(board.Epoch > before && board.Inhibited && manager.SatisfiedTargetCount == 5,
                "Fresh post-reconnect evidence alone retains the latched inhibition.");
            Click("Reset safety");
            await Stage(6);
            Check(manager.IsCompleted && board.Qualifies && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f,
                "Fresh ×4 link, enumeration, traffic and an explicit U13 reset restore the physical workload permit.");
            Click("Board overview");
            await Task.Delay(200);
            var plot = board.GetComponentInChildren<SparkLevel105TrainingPlot>();
            Check(plot && plot.TryGetComponent<CanvasRenderer>(out var renderer) && !renderer.cull, "The live four-lane timing plot renders.");
            await Screenshot("Level105Complete");
            board.SetLayer(1);
            Click("Layer view");
            await Task.Delay(180);
            await Screenshot("Level105LayerView");
            for (int layer = 1; layer <= 8; layer++)
            {
                Check(board.routes.Any(route => route.copper.enabled) &&
                    board.routes.All(route => route.copper.enabled == (route.layer == layer)) &&
                    board.Powered && manager.IsCompleted, $"Topology overlays isolate L{layer:00} during live diagnostics.");
                if (layer < 8) Click("Next layer");
            }
        }

        private static async Task PowerUp()
        {
            Check(board.TogglePower(), "Enable the current-limited bench.");
            await Wait(() => board.CardPowered && board.ClockReady && board.ResetReleased, "qualified auxiliary power, differential REFCLK and reset");
        }

        private static async Task TrainHealthy()
        {
            Click("Train link");
            await Wait(() => board.FullLink && !board.Training, "Gen2-style ×4 L0");
        }

        private static async Task EnumerateAndTransfer()
        {
            Click("Read config");
            Click("Assign resources");
            Check(board.Enumerated, "The resource window is assigned on the current link.");
            int previous = board.Captures;
            Click("Run traffic");
            await Wait(() => board.Captures > previous && !board.Testing, "bounded traffic transfer");
            Log(board.Evidence);
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
                    string rails = $"Bench {board.Voltage(board.standby):F3} V, AUX {board.Voltage(board.aux):F3} V, ECX-4 {board.Voltage(board.endpointPower):F3} V";
                    string margins = string.Join(", ", Enumerable.Range(0, 4).Select(i => $"{board.TxMargin(i):F0}/{board.RxMargin(i):F0}"));
                    throw new TimeoutException($"{step}. Stage {board.Stage}, epoch {board.Epoch}, {board.Link} ×{board.Width}. " +
                        $"{rails}. CLK P/N {board.Voltage(board.clockP):F3}/{board.Voltage(board.clockN):F3} V. " +
                        $"TX/RX margins {margins}. {board.Evidence}");
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
