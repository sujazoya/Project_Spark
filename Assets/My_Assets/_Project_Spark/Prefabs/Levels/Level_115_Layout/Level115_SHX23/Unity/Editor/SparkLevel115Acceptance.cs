using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.SecureEnclave;
using SparkLevel115Process = ProjectSpark.Gameplay.SecureEnclave.SparkLevel115Process;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools.SecureEnclave
{
    [InitializeOnLoad]
    internal static class Level115Acceptance
    {
        private const string Request = "Logs/Level115Acceptance.request";
        private const string Report = "Logs/Level115Acceptance.txt";
        private const string Session = "ProjectSpark.Level115.Acceptance";
        private static LevelGamePlayManager manager;
        private static SparkLevel115Process board;

        static Level115Acceptance()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += Play;
        }

        [MenuItem("Tools/Project Spark/Levels/115/Run Gameplay Acceptance")]
        public static void RequestRun() => File.WriteAllText(Request, "");

        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete(Request);
            File.WriteAllText(Report, "THE SECURE ENCLAVE / SHX23-EDU-1\n");
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<SparkLevelLayout>(SparkLevel115Builder.Prefab);
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
                        PlayerPrefs.SetString(SparkLevel115Process.CheckpointKey, SessionState.GetString(Session + ".checkpoint", ""));
                    else PlayerPrefs.DeleteKey(SparkLevel115Process.CheckpointKey);
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
            SessionState.SetBool(Session + ".hadCheckpoint", PlayerPrefs.HasKey(SparkLevel115Process.CheckpointKey));
            SessionState.SetString(Session + ".checkpoint", PlayerPrefs.GetString(SparkLevel115Process.CheckpointKey));
            SessionState.SetBool(Session + ".checkpointCaptured", true);
            PlayerPrefs.DeleteKey(SparkLevel115Process.CheckpointKey);
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
            Check(manager && loader && wire, "Campaign manager, layout loader and native Wire tool resolve.");
            if (manager.LevelProgression) manager.LevelProgression.enabled = false;
            var data = new SerializedObject(manager);
            data.FindProperty("autoAdvanceOnCompletion").boolValue = false;
            data.FindProperty("autoEvaluate").boolValue = true;
            data.ApplyModifiedPropertiesWithoutUndo();
            var levels = data.FindProperty("levels");
            var target = AssetDatabase.LoadAssetAtPath<SparkLevelDefinition>(SparkLevel115Builder.Definition);
            int index = Enumerable.Range(0, levels.arraySize)
                .Where(i => levels.GetArrayElementAtIndex(i).objectReferenceValue == target).DefaultIfEmpty(-1).First();
            Check(index >= 0 && index == levels.arraySize - 1 && manager.LevelProgression &&
                manager.LevelProgression.CanPlayLevel(index) && manager.StartLevel(index),
                "MainMenu starts The Secure Enclave with existing campaign positions preserved.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) &&
                board.GetComponentInChildren<Canvas>(), "workbench startup");
            Check(board.Installed == 0 && board.Routed == 0 && board.Plated == 0 && !board.JumperPresent &&
                !board.Powered && board.parts.All(part => !part.model.activeSelf) && manager.SatisfiedTargetCount == 0,
                "Starts empty at 0/6 with W273 open and bench power isolated.");
            Check(board.parts.Length == 196 && board.routes.SelectMany(route => new[] { route.from, route.to }).Distinct().Count() == 544 &&
                board.routes.Select(route => route.layer).Distinct().Count() == 22 &&
                board.routes.Count(route => route.barrel && route.layer != route.exitLayer) == 92,
                "Serialized board contains 196 footprints, 272 route pairs, 22 layers and 92 native vias.");
            Check(board.jumper.from.name == "U7.TRUST_READY_OUT" &&
                board.jumper.to.name == "U8.TRUST_READY_IN",
                "W273 connects the fictional U7 trust output to the independent U8 input.");
            var bounds = board.boardArt.GetComponentsInChildren<Renderer>().Select(renderer => renderer.bounds)
                .Aggregate((result, next) => { result.Encapsulate(next); return result; });
            var table = GameObject.Find("PS_PRO_GAME_ROOM").transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable").GetComponent<BoxCollider>();
            var size = Vector3.Scale(table.size, table.transform.lossyScale);
            var center = table.transform.TransformPoint(table.center);
            Check(Mathf.Abs(bounds.size.x - 10.13f) < .10f && Mathf.Abs(bounds.size.z - 7.236f) < .09f &&
                Mathf.Abs(bounds.center.x - center.x) < .04f && Mathf.Abs(bounds.center.z - center.z) < .04f,
                $"Board art fits {bounds.size.x:F2} × {bounds.size.z:F2} on the {size.x:F2} × {size.z:F2} table.");
            Check(board.fixture.Find("SHX-23 placement legend").GetComponent<MeshFilter>().sharedMesh.vertexCount > 2000,
                "The original footprints have visible placement markings.");
            await Screenshot("Level115Opening");
            board.SelectPart(1);
            Check(!board.FitPart(), "Mismatched parts are rejected.");
            board.SelectPart(0);
            board.RotatePart();
            Check(!board.FitPart() && !board.Solder(), "Wrong orientation cannot be secured.");
            board.SelectPart(0);
            Click("Fit part");
            Check(board.parts[0].model.activeSelf && !board.parts[0].contact.IsClosed && !board.Solder(),
                "Placement preview requires contact inspection before closing the contact.");
            Click("Inspect contacts");
            Click("Secure part");
            await Wait(() => board.Installed == 1 && PlayerPrefs.HasKey(SparkLevel115Process.CheckpointKey), "saved first part");
            var firstBoard = board;
            loader.UnloadLevel();
            Check(manager.StartLevel(index), "Campaign reloads the board for checkpoint verification.");
            await Wait(() => loader.ActiveLayout && loader.ActiveLayout.TryGetComponent(out board) && board != firstBoard &&
                board.GetComponentInChildren<Canvas>(), "reloaded board");
            Click("Resume");
            Check(board.Installed == 1 && board.parts[0].contact.IsClosed && !board.Powered && !board.Trusted,
                "Checkpoint restores the real assembly contact with power isolated.");
            for (int i = 1; i < board.parts.Length; i++)
            {
                if (!board.FitPart() ||
                    ((i < 8 || board.shieldParts.Contains(i) || board.parts[i].label.StartsWith("J", StringComparison.Ordinal)) &&
                    !board.InspectContacts()) || !board.Solder())
                    throw new InvalidOperationException("Assembly failed at " + board.parts[i].label);
                if (i % 16 == 0) await Task.Yield();
            }
            await Stage(1);
            Check(board.Installed == 196 && board.ShieldsMounted && !board.JumperPresent, "All original placements and four shield mounts complete.");
            var first = board.routes[0];
            board.SetLayer(2);
            Check(!board.PlateVia() && !board.ConnectPads(), "Wrong-layer plating and wiring are blocked.");
            board.SelectRoute(0);
            Check(!board.ConnectPads() && board.PlateVia(), "The first route needs its own plated span.");
            board.PadView();
            var start = Camera.main.WorldToScreenPoint(first.from.transform.position);
            var end = Camera.main.WorldToScreenPoint(first.to.transform.position);
            Check(wire.FindStartTerminal(start) == first.from && wire.BeginWire(first.from, WirePolarity.Neutral) &&
                wire.TryEndWire(end), "Native Wire tool connects the highlighted physical terminals.");
            await Wait(() => board.Routed == 1 && board.Plated == 1, "first route and via");
            await Screenshot("Level115Copper");
            board.BoardView();
            for (int i = 1; i < board.routes.Length; i++)
            {
                board.SelectRoute(i);
                if (board.routes[i].barrel && !board.PlateVia() || !board.ConnectPads())
                    throw new InvalidOperationException("Native connection failed at " + board.routes[i].id);
                if (i % 16 == 0) await Task.Yield();
            }
            Check(board.Routed == 272 && board.Plated == 92 && !board.TogglePower(),
                "All copper still requires the independent W273 connection.");
            board.SelectRoute(board.routes.Length);
            Check(board.ConnectPads(), "W273 connects in the authoritative circuit.");
            await Stage(2);
            Check(board.witnesses.All(pin => board.Voltage(pin) > 4.7f) && !board.ProbeCommissioning(),
                "All continuity groups solve and unpowered commissioning is blocked.");
            Click("Bench power");
            Check(board.RailsReady && board.Voltage(board.reset) < .15f && board.Voltage(board.clock) > 3f &&
                board.ProbeCommissioning(), "Independent solved rails, held reset, clock and tamper continuity pass.");
            await Stage(3);
            Check(!board.IssueChallenge() && !board.VerifyAttestation(), "A powered board alone cannot attest.");
            await BootFailure(1, SparkLevel115Process.BootFailure.Digest);
            await BootFailure(2, SparkLevel115Process.BootFailure.Signature);
            await BootFailure(3, SparkLevel115Process.BootFailure.Rollback);
            Check(board.SelectImage(0), "Return to the pinned-root version 3 study image.");
            Check(board.BeginBoot(), "Start the cold boot state machine.");
            await Wait(() => !board.Booting, "verified firmware");
            Check(board.BootAuthenticated && !board.Trusted && manager.SatisfiedTargetCount == 3,
                "Signature and version verification alone leave objective four pending.");
            await Screenshot("Level115Boot");
            string firstNonce = Attest();
            await Stage(4);
            Check(board.Trusted && !board.VerifyAttestation() && board.VersionFloor == 3,
                "A consumed nonce cannot be replayed and the monotonic version advances.");

            for (int fault = 0; fault < 3; fault++)
            {
                int prior = board.BootEpoch;
                Check(board.BeginFaultTrial() && !board.Trusted && !board.VerifyAttestation(),
                    $"Fault {fault + 1} invalidates cached boot evidence.");
                if (fault < 2)
                {
                    Check(board.BeginBoot(), "Start a diagnostic boot on the faulted hardware.");
                    await Wait(() => !board.Booting, "measured boot interruption");
                    Check(fault == 0 ? board.Failure == SparkLevel115Process.BootFailure.Supply && board.FailureReading < 1.69f :
                        board.Failure == SparkLevel115Process.BootFailure.Flash && board.FailureReading < 4.6f,
                        $"Fault {fault + 1} produces its own native voltage discontinuity.");
                }
                else
                {
                    await Wait(() => board.TamperLatched, "independent tamper latch");
                    Check(board.Voltage(board.meshSense) < .15f && board.Voltage(board.aux) > 3f && !board.BeginBoot(),
                        "The physical shield loop opens and rejects boot while diagnostics stay live.");
                }
                Check(board.LocateFault() && !board.Repair(), "Measured localization succeeds and hot rework is blocked.");
                await Screenshot("Level115Fault" + (fault + 1));
                Check(board.TogglePower(), "Bench isolated for repair.");
                if (fault == 0)
                {
                    board.SelectFootprint(board.flashPart);
                    Check(!board.Repair(), "Wrong target cannot repair the Q1 feed.");
                    board.SelectFootprint(board.railPart);
                    Check(board.Repair(), "Renew the selected Q1 secure-feed contact.");
                }
                else if (fault == 1)
                {
                    Check(!board.Repair() && board.InspectFlashSpan(), "Via repair requires inspection of the diagnosed span.");
                    var via = board.routes[board.flashViaIndex];
                    board.SetLayer(via.exitLayer);
                    Check(!board.Repair(), "Wrong-layer repair is rejected.");
                    board.SetLayer(via.layer);
                    Check(board.Repair() && via.barrel.IsClosed, "The original flash via contact is restored on its own span.");
                }
                else
                {
                    Check(!board.Repair() && board.ToggleService() && !board.ShieldsMounted && !board.TogglePower(),
                        "SH3 must be lifted and bench power remains blocked while it is removed.");
                    Check(board.Repair() && board.ToggleService() && board.ShieldsMounted && board.TamperLatched,
                        "Remounting the repaired seam retains the independent latch.");
                }
                board.BoardView();
                Check(board.TogglePower(), "Restore diagnostic power after mechanical repair.");
                if (fault == 2)
                    Check(!board.BeginBoot() && board.Voltage(board.meshSense) > 3f && board.ResetTamper(),
                        "Real loop continuity permits explicit reset and a fresh boot.");
                await ValidBoot();
                string nonce = Attest();
                Check(board.Repaired == fault + 1 && board.BootEpoch > prior && nonce != firstNonce,
                    $"Repair {fault + 1} requires its own fresh authenticated boot and quote.");
                firstNonce = nonce;
            }
            await Stage(5);
            await Wait(() => board.Fresh && !board.Inhibited, "prior trusted W273 permit");
            int oldBoot = board.BootEpoch;
            Check(board.OpenW273() && !board.JumperPresent && board.Inhibited && board.Powered &&
                board.Voltage(board.monitorSupply) > 3f && board.Voltage(board.protectedFeed) < .15f &&
                manager.SatisfiedTargetCount == 5, "Physical W273 open drops only the protected workload and preserves history.");
            await Screenshot("Level115W273Open");
            Check(!board.ResetSafety() && board.RestoreW273() && !board.ResetSafety() && !board.VerifyAttestation(),
                "Reconnection alone cannot clear the latch or reuse the old quote.");
            await ValidBoot();
            string finalNonce = Attest();
            await Wait(() => board.Fresh, "new W273 transitions");
            Check(board.BootEpoch > oldBoot && finalNonce != firstNonce && board.Inhibited && board.ResetSafety(),
                "The independent reset requires the newer boot and nonce-bound attestation.");
            await Stage(6);
            Check(manager.IsCompleted && !board.Inhibited && board.Voltage(board.protectedFeed) > 3f,
                "Live campaign authority grants 6/6 only after restored U8 authorization.");
            board.BoardView();
            await Screenshot("Level115Complete");
            Click("Layer view");
            for (int layer = 1; layer <= 22; layer++)
            {
                board.SetLayer(layer);
                Check(board.routes.Count(route => route.copper.enabled) > 0 &&
                    board.routes.All(route => route.copper.enabled == (route.layer == layer)) &&
                    board.ShieldsMounted && board.Routed == 272 && board.Plated == 92,
                    $"Layer {layer:00} changes presentation while the native board remains solved.");
            }
            await Screenshot("Level115Layers");
            Click("Angle view");
            Check(Vector3.Angle(Camera.main.transform.forward, Vector3.down) > 25f &&
                board.routes.All(route => !route.copper.enabled) && board.ShieldsMounted,
                "Inspection view displays the mounted original shield cans.");
            await Screenshot("Level115Inspection");
        }

        private static async Task BootFailure(int image, SparkLevel115Process.BootFailure failure)
        {
            Check(board.SelectImage(image) && board.BeginBoot(), $"Load study fixture {image} and start boot.");
            await Wait(() => !board.Booting, "rejected firmware fixture " + image);
            Check(board.Failure == failure && !board.BootAuthenticated && !board.IssueChallenge() && manager.SatisfiedTargetCount == 3,
                failure + " blocks authentication and objective advancement.");
            Log(board.Evidence);
        }

        private static async Task ValidBoot()
        {
            Check(board.BeginBoot(), "Start a new trusted-boot attempt.");
            await Wait(() => !board.Booting, "completed authentication");
            Check(board.BootAuthenticated && !board.Trusted, "Digest, pinned-root signature and rollback checks passed.");
            Log(board.Evidence);
        }

        private static string Attest()
        {
            Check(board.IssueChallenge(), "Issue a fresh independently verified nonce.");
            string nonce = board.ChallengeId;
            Check(!string.IsNullOrEmpty(nonce) && board.VerifyAttestation() && board.Trusted, "Verify the current signed device quote.");
            Log(board.Evidence);
            return nonce;
        }


        private static async Task Screenshot(string name)
        {
            await Task.Delay(140);
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
                    string rails = $"Bench {board.Voltage(board.standby):F3}, DIAG {board.Voltage(board.aux):F3}, SECURE {board.Voltage(board.secure):F3}, FLASH {board.Voltage(board.flash):F3}, MESH {board.Voltage(board.meshSense):F3}";
                    throw new TimeoutException($"{step}. Stage {board.Stage}, epoch {board.Epoch}, boot {board.BootEpoch}, failure {board.Failure}. {rails}. {board.Evidence}");
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
