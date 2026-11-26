using System;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;
using ProjectSpark.UIManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    internal static class SparkFlightLevel20Verification
    {
        private const string Preview = "draw real wire from SwitchOutput to R_LTOP_Input";

        internal static async Task Verify(Action<string> log, string cableCase = "")
        {
            var director = Object.FindObjectsByType<SparkNativeWireDirector>(FindObjectsSortMode.None).Single();
            var manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
            var executive = director.GetComponent<SparkProExecutive>();
            var character = director.GetComponent<SparkProCharacter>();
            var flight = director.GetComponent<SparkV5FlightController>();
            var auto = director.GetComponent<SparkNativeWireAutoEngineer>();
            var hub = director.GetComponent<SparkEngineeringControlHub>();
            var device = director.GetComponent<SparkEngineeringDeviceOperator>();
            var performer = director.GetComponent<SparkNativeWireCinematic>();
            var agent = director.GetComponent<NavMeshAgent>();
            var port = director.GetComponent<SparkAINativeSignalWirePort>();
            var builder = new SerializedObject(port).FindProperty("playerBuilder").objectReferenceValue as SignalWireBuilder;
            Require(manager && executive && character && flight && auto && hub && device && performer && agent && port && builder,
                "Real MainMenu components must be bound.");
            await Wait(() => executive.ActiveLevelManager == manager, "Spark Live level binding");
            executive.SetSpeakAnswers(false);
            if (manager.LevelProgression) manager.LevelProgression.enabled = false;
            var settings = new SerializedObject(manager);
            settings.FindProperty("enforceProgressionAccess").boolValue = false;
            settings.FindProperty("autoAdvanceOnCompletion").boolValue = false;
            settings.ApplyModifiedPropertiesWithoutUndo();
            int Slot(int number) => manager.GetCampaignLevelIndices().Single(index => manager.GetLevel(index).LevelNumber == number);
            Require(manager.StartLevel(Slot(20)), "Native Level 20 did not start.");
            await Wait(() => SparkAILevelContext.Capture(manager).HasEvaluation &&
                manager.ActiveLevel.LevelNumber == 20 && port.IsConfigured, "Level 20 with native evaluator");
            var level = SparkAILevelContext.Capture(manager);
            var circuit = manager.CircuitSystem;
            var bindings = level.Layout.SceneBindings;
            var supply = level.Layout.GetComponentsInChildren<SparkPowerSupply>().Single();
            var powerSwitch = level.Layout.GetComponentsInChildren<SparkSwitch>().Single();
            await InstallBridgeResistors(level.Layout.transform, log);
            var plan = Resources.LoadAll<SparkNativeWiringPlan>("SparkNativeWirePlans")
                .Single(candidate => candidate.LevelNumber == 20 && candidate.IsApproved);
            Require(plan.IsApproved && plan.Wires.Count == 10, "Level 20's reviewed ten-wire plan is required.");
            await Wait(() => plan.Wires.All(step => bindings.TryGetTerminal(step.fromId, out _) &&
                bindings.TryGetTerminal(step.toId, out _)), "all reviewed terminal bindings after shelf placement");
            var pairs = plan.Wires.Select(step =>
            {
                bool from = bindings.TryGetTerminal(step.fromId, out var a);
                bool to = bindings.TryGetTerminal(step.toId, out var b);
                Require(from && to && a && b, $"Authored terminals missing for {step.fromId} to {step.toId}.");
                return (a, b);
            }).ToArray();
            var first = pairs[0];
            var playerProfile = (builder.DrawingMode, builder.CableRouting, builder.TerminalInputStyle, builder.ExternalInputControl);
            bool diagnosticBaseline = cableCase == "cable-before";
            using var capture = cableCase.Length == 0 ? null : new SparkNativeCableVisualCapture(
                cableCase, builder, performer, flight, director, first.a, first.b, log);
            Require(!circuit.HasConnectionBetween(first.a, first.b), "The reported connection already exists.");
            var home = flight.transform.position;
            var shoulder = character.GetAnchor("Shoulder_R");
            log($"Live Level 20 start {home:F3}, shoulder {shoulder.position:F3}, " +
                $"SwitchOutput {first.a.transform.position:F3}, R_LTOP_Input {first.b.transform.position:F3}.");
            log($"V13.2 legacy volume required root world Y >= {home.y - .65f:F3}. " +
                $"Highest upright candidate Y was {first.a.transform.position.y + .25f - (shoulder.position.y - home.y):F3}.");

            // Exercise the original power owner through its normal API.
            void Power(bool on)
            {
                var interaction = new SparkInteractionContext(executive, supply, SparkInteractionType.Toggle,
                    SparkToolType.Select, supply.transform.position, Vector3.up, default, supply.gameObject);
                var result = supply.SetOutput(on, interaction);
                Require(result.Succeeded && supply.IsOutputActive == on, "Native virtual power control rejected the test.");
            }

            Power(false);
            Require(flight.RequestTakeoff(out string takeoff), takeoff);
            await Wait(() => flight.IsFlying && !flight.IsMoving && flight.LastOperationSucceeded, "manual takeoff");
            Require(flight.transform.position.y > home.y + .25f, "Manual flight did not climb and hover.");
            Require(flight.RequestLanding(out string landing), landing);
            await Wait(() => !flight.IsFlying, "manual landing");
            Require(Vector3.Distance(home, flight.transform.position) < .06f, "Manual landing missed the original home.");
            log("PASS Manual takeoff, hover and landing.");

            executive.Ask("spark manual mode");
            executive.Ask(Preview);
            Require(!director.IsBusy && !builder.IsDrawing && !circuit.HasConnectionBetween(first.a, first.b),
                "Manual mode must leave the player in control.");
            executive.Ask("spark assisted mode");
            executive.Ask("spark propose device PowerSupply_01 on");
            Require(executive.LastAnswer.Contains("missing", StringComparison.OrdinalIgnoreCase) && !supply.IsOutputActive,
                "The device operator allowed an incomplete circuit to energize. " + executive.LastAnswer);
            Power(true);
            executive.Ask("spark suggest next wire");
            executive.Ask("spark approve suggested wire");
            Require(executive.LastAnswer.Contains("virtual power output OFF", StringComparison.OrdinalIgnoreCase) &&
                !director.IsBusy && !builder.IsDrawing, "Energized supply did not block wiring. " + executive.LastAnswer);
            Power(false);
            executive.SetExaminationMode(true);
            executive.Ask("spark suggest next wire");
            Require(executive.LastAnswer.Contains("Independent exam", StringComparison.OrdinalIgnoreCase) &&
                !director.IsBusy, "Exam mode did not block wiring.");
            var examinationReport = director.GetComponent<SparkEngineeringMissionAdvisor>().Inspect();
            Require(examinationReport.Contains("Reviewed instructions are hidden") &&
                !examinationReport.Contains("SwitchOutput -> R_LTOP_Input"), "The Advisor panel exposed practice steps in an exam.");
            executive.SetExaminationMode(false);
            executive.Ask("spark inspect mission");
            Require(executive.LastAnswer.Contains("Graph verification") && !circuit.HasConnectionBetween(first.a, first.b),
                "Mission Advisor did not inspect the authored guidance without changing the circuit.");
            log("PASS Manual ownership, incomplete-circuit guard, Mission Advisor readback, power and exam restrictions.");

            var obstruction = new GameObject("Flight verification obstacle");
            obstruction.transform.position = first.a.transform.position + Vector3.up * .6f;
            var blocker = obstruction.AddComponent<BoxCollider>();
            blocker.size = new Vector3(3f, 2.5f, 3f);
            Physics.SyncTransforms();
            await Begin(executive, director, hub);
            await Wait(() => !director.IsBusy, "blocked approach to fail", 45f);
            Require(!builder.IsDrawing && !circuit.HasConnectionBetween(first.a, first.b) &&
                director.LastStatus.Contains(obstruction.name), "Blocked action did not stop at the actual collider. " + director.LastStatus);
            Require(!blocker.bounds.Contains(flight.transform.position + Vector3.up * .5f),
                "The walking approach crossed the obstacle before flight planning.");
            log("PASS Obstructed source produced no cable. " + director.LastStatus);
            obstruction.SetActive(false);
            Object.Destroy(obstruction);

            int beforeFlight = flight.SuccessfulApproaches;
            await Begin(executive, director, hub);
            await Wait(() => builder.IsDrawing || !director.IsBusy, "active native preview before cancellation");
            Require(builder.IsDrawing && !flight.IsFlying, director.LastStatus);
            var cancelledPath = builder.CurrentPath;
            executive.Ask("cancel wire");
            Require(!director.IsBusy && !flight.IsMoving && !builder.IsDrawing &&
                !circuit.HasConnectionBetween(first.a, first.b), "Cancellation left an active flight or native cable.");
            await Wait(() => !cancelledPath, "cancelled cable was destroyed");
            Require(playerProfile == (builder.DrawingMode, builder.CableRouting, builder.TerminalInputStyle, builder.ExternalInputControl),
                "The cancelled wire failed to restore the player profile.");
            log("PASS Cancellation removed the native preview and left the circuit unchanged.");

            bool walked = false, flew = false, sawRoute = false;
            await Begin(executive, director, hub);
            await Wait(() =>
            {
                walked |= agent.isActiveAndEnabled && agent.isOnNavMesh && agent.velocity.sqrMagnitude > .02f;
                flew |= flight.IsFlying || flight.IsMoving;
                sawRoute |= builder.IsDrawing && builder.DragRoutePointCount > 0;
                return !director.IsBusy;
            }, "the grounded SwitchOutput to R_LTOP_Input wire", 65f);
            Require(!flew && flight.SuccessfulApproaches == beforeFlight && (sawRoute || diagnosticBaseline) &&
                circuit.HasConnectionBetween(first.a, first.b), director.LastStatus);
            ulong wire = port.LastAIConnectionId;
            var registries = Object.FindObjectsByType<SparkWireRegistry>(FindObjectsSortMode.None);
            Require(wire != 0 && circuit.HasConnection(wire) && registries.Any(registry => registry.TryGetWire(wire, out _)),
                "The first player-drawn cable must exist in the visual registry and the native graph.");
            var record = registries.Select(registry => registry.TryGetWire(wire, out var item) ? item.Path : null)
                .Single(path => path);
            var points = record.Points;
            Require(points.Length >= (diagnosticBaseline ? 2 : 3) && Vector3.Distance(points[0], first.a.transform.position) < .001f &&
                Vector3.Distance(points[^1], first.b.transform.position) < .001f,
                "Native drag route must have exact endpoints and sampled interior points.");
            float referenceSpan = Vector3.Distance(points[0], points[^1]);
            float totalLength = Enumerable.Range(1, points.Length - 1).Sum(i => Vector3.Distance(points[i - 1], points[i]));
            Require(totalLength < referenceSpan * 1.8f, "The native freehand cable developed an oversized route.");
            Require(playerProfile == (builder.DrawingMode, builder.CableRouting, builder.TerminalInputStyle, builder.ExternalInputControl),
                "Native attachment failed to restore the player profile.");
            log($"PASS Authored Level 20 use of player draw, wire {wire}, walked during action {walked}, zero flight, {points.Length} points, length {totalLength:F3} for a {referenceSpan:F3} terminal span.");
            if (capture != null) await capture.IsolateRenderer(record, "authored-attached");
            executive.Ask("undo last AI wire");
            Require(!circuit.HasConnectionBetween(first.a, first.b), executive.LastAnswer);
            if (capture != null)
                await capture.ReplayHistoricalRoute(port, first.a, first.b, circuit, diagnosticBaseline);

            executive.Ask("spark auto mode");
            executive.Ask("preview approved wiring plan");
            log(executive.LastAnswer);
            executive.Ask("auto wire practice level");
            Require(executive.LastAnswer.Contains("10", StringComparison.Ordinal) &&
                executive.LastAnswer.Contains("AUTO started", StringComparison.OrdinalIgnoreCase), executive.LastAnswer);
            executive.Ask("spark pause automation");
            await Wait(() => hub.Phase == SparkEngineeringControlHub.ExecutionPhase.Paused, "Auto pause");
            Require(!director.IsBusy && !builder.IsDrawing && !auto.IsExecuting, "Auto pause left an active wire owner.");
            executive.Ask("spark resume automation");
            int reported = 0;
            await Wait(() =>
            {
                int count = pairs.Count(pair => circuit.HasConnectionBetween(pair.a, pair.b));
                if (count != reported) { reported = count; log($"Reviewed plan: {count}/10 native links."); }
                return !hub.IsExecuting;
            }, "all ten reviewed plan wires", 360f);
            Require(reported == 10 && !director.IsBusy && !auto.IsExecuting, hub.LastMessage + " " + director.LastStatus);
            Require(!builder.IsDrawing && !Object.FindObjectsByType<SignalPath>(FindObjectsSortMode.None).Any(path => path.HasPreview),
                "A native preview survived the reviewed plan.");
            foreach (var pair in pairs)
            {
                var visual = Object.FindObjectsByType<SparkWireRegistry>(FindObjectsSortMode.None)
                    .SelectMany(registry => registry.Wires)
                    .Single(item => item != null && item.Path &&
                        (item.TerminalA == pair.a && item.TerminalB == pair.b ||
                         item.TerminalA == pair.b && item.TerminalB == pair.a));
                var nativeMesh = visual.Path.GetComponent<SignalPathMesh>();
                var route = nativeMesh.GetEffectiveRoutePoints();
                var metric = SparkNativeCableVisualCapture.Measure(route);
                Require(nativeMesh && nativeMesh.GeneratedMesh && !visual.Path.HasPreview && metric.invalid == 0 &&
                    metric.length < Vector3.Distance(route[0], route[^1]) * 1.8f,
                    $"Malformed native wire {pair.a.name} to {pair.b.name}.");
            }
            log("PASS v14 alone ran the reviewed plan and honored pause and resume.");
            executive.Ask("spark assisted mode");
            if (powerSwitch.IsClosed) await Operate(executive, device, "Power_Switch", false);
            await Operate(executive, device, "Power_Switch", true);
            Require(powerSwitch.IsClosed, "The native switch did not close.");
            await Wait(() => manager.HasCurrentElectricalEvaluation, "fresh native evaluator before energizing");
            await Operate(executive, device, "PowerSupply_01", true);
            await Wait(() => manager.SatisfiedTargetCount == 5 && manager.IsCompleted,
                "powered Level 20 evaluator at 5/5");
            log("PASS Reviewed 10-wire plan and native powered evaluator at 5/5.");

            await Operate(executive, device, "PowerSupply_01", false);
            Require(!supply.IsOutputActive && supply.State == SparkPowerSupplyState.Standby, "The native supply did not return to Standby.");
            log("PASS v16 authored switch and power operated through Spark Live with fresh electrical readback.");
            Require(manager.RestartLevel(), "Could not reload Level 20 before level-swap cancellation.");
            await Wait(() => SparkAILevelContext.Capture(manager).HasEvaluation &&
                SparkAILevelContext.Capture(manager).Layout != level.Layout, "fresh Level 20");
            var fresh = SparkAILevelContext.Capture(manager).Layout;
            await InstallBridgeResistors(fresh.transform, log);
            await Wait(() => fresh.SceneBindings.TryGetTerminal("R_LTOP_Input", out _), "reloaded resistor terminal");
            await Begin(executive, director, hub);
            await Wait(() => builder.IsDrawing || !director.IsBusy, "active native preview before changing levels");
            Require(builder.IsDrawing && !flight.IsFlying, director.LastStatus);
            Require(manager.StartLevel(Slot(21)), "Could not change to Level 21.");
            await Wait(() => !director.IsBusy, "level swap cancellation");
            Require(!flight.IsMoving && !builder.IsDrawing, "Level switch left active flight or a loose cable.");
            log("PASS Active level change cancelled the grounded wiring action.");

            Require(manager.StartLevel(Slot(20)), "Could not load Level 20 for the ground-first fixture.");
            await Wait(() => SparkAILevelContext.Capture(manager).HasEvaluation && manager.ActiveLevel.LevelNumber == 20,
                "ground-first layout");
            var walkingLayout = SparkAILevelContext.Capture(manager).Layout;
            await InstallBridgeResistors(walkingLayout.transform, log);
            await Wait(() => walkingLayout.SceneBindings.TryGetTerminal("R_LTOP_Input", out _), "ground-first terminal bindings");
            var tabletop = Object.FindFirstObjectByType<SparkWorkbenchNavigation>().GetComponent<BoxCollider>().bounds;
            walkingLayout.SceneBindings.TryGetTerminal("SwitchOutput", out var highA);
            walkingLayout.SceneBindings.TryGetTerminal("R_LTOP_Input", out var highB);
            // Isolated Play Mode ergonomics fixture. The authored Level 20 geometry
            // has already passed the native plan without changing its terminals.
            highA.transform.position = tabletop.center + new Vector3(3.8f, tabletop.extents.y + .96f, 2.7f);
            highB.transform.position = tabletop.center + new Vector3(6.2f, tabletop.extents.y + .96f, 2.7f);
            capture?.Track(highA, highB);
            foreach (var socket in new[] { highA, highB })
                foreach (var collider in socket.GetComponentsInChildren<Collider>())
                    collider.isTrigger = true;
            Physics.SyncTransforms();
            int flightsBeforeGroundTest = flight.SuccessfulApproaches;
            bool sawWalk = false, carriedCableWhileWalking = false;
            int gripSamples = 0;
            float largestGripGap = 0f;
            await Begin(executive, director, hub);
            await Wait(() =>
            {
                bool walking = !flight.IsFlying && agent.isActiveAndEnabled && agent.isOnNavMesh && agent.velocity.sqrMagnitude > .02f;
                sawWalk |= walking;
                if (builder.CurrentPath && builder.CurrentPath.HasPreview && performer.TryGetHeldCableGrip(out var grip))
                {
                    gripSamples++;
                    largestGripGap = Mathf.Max(largestGripGap, Vector3.Distance(grip, builder.CurrentPath.PreviewPoint));
                    carriedCableWhileWalking |= walking;
                }
                return !director.IsBusy;
            }, "forced walkable terminal heights", 100f);
            Require(sawWalk && carriedCableWhileWalking && flight.SuccessfulApproaches == flightsBeforeGroundTest &&
                gripSamples > 4 && largestGripGap < .015f &&
                manager.CircuitSystem.HasConnectionBetween(highA, highB), "Spark should carry the cable on foot between reachable sockets. " + director.LastStatus);
            log($"PASS Controlled reach-height fixture: Spark carried and inserted native cable on foot, zero flight approaches, {gripSamples} hand samples and maximum gap {largestGripGap:F4}.");

            var panels = new[] { "SparkEngineeringV14_Overlay", "SparkMissionAdvisorV15_Overlay", "SPARK_DeviceOperatorV16_Canvas" };
            int eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Count(item => item.isActiveAndEnabled);
            log($"Runtime UI has {eventSystems} active EventSystem(s) and {panels.Count(name => GameObject.Find(name))}/{panels.Length} engineering canvases.");
            Require(eventSystems == 1 && panels.All(name => GameObject.Find(name)), "Engineering canvases or shared player input are missing or duplicated.");
            Require(SparkUIManager.Instance && SparkUIManager.Instance.Open(SparkProLivePanel.PanelId), "Spark Live could not open through the shared UI manager.");
            executive.SetSpeakAnswers(true);
            executive.Ask("what level am I on?");
            Require(executive.LastAnswer.Contains("Level 20", StringComparison.OrdinalIgnoreCase), "Spark Live did not return the authoritative level text.");
            await Wait(() => executive.PlaybackActive && executive.CharacterState == SparkState.Speaking,
                "actual Spark Live voice playback", 25f);
            executive.Interrupt();
            Require(!executive.PlaybackActive, "Interrupted speech left an active player.");
            executive.SetSpeakAnswers(false);
            SparkUIManager.Instance.Close(SparkProLivePanel.PanelId);
            log("PASS MainMenu engineering canvases, one active EventSystem, Spark Live text, voice playback and interruption.");

            foreach (var chapter in manager.Campaign.Chapters.Where(item => item))
            {
                int index = chapter.Levels.Where(item => item).Select(manager.GetLevelIndex).First(candidate => candidate >= 0);
                var definition = manager.GetLevel(index);
                Require(manager.StartLevel(index), "Cannot load chapter " + chapter.ChapterNumber);
                await Wait(() => SparkAILevelContext.Capture(manager).HasEvaluation &&
                    manager.ActiveLevel == definition && manager.TargetResults.Count > 0, "chapter evaluator");
                log($"PASS Chapter {chapter.ChapterNumber} Level {definition.LevelNumber} loaded with {manager.TargetResults.Count} evaluator rows.");
            }
        }

        private static async Task Begin(SparkProExecutive executive, SparkNativeWireDirector director, SparkEngineeringControlHub hub)
        {
            executive.Ask("spark suggest next wire");
            Require(executive.LastAnswer.Contains("SUGGESTION ONLY", StringComparison.OrdinalIgnoreCase) &&
                !director.IsBusy && !hub.IsExecuting, executive.LastAnswer);
            executive.Ask("spark approve suggested wire");
            Require(hub.IsExecuting, executive.LastAnswer);
            await Wait(() => director.IsBusy || !hub.IsExecuting, "v14 handoff to native Wire Tool");
            Require(director.IsBusy, hub.LastMessage);
        }

        private static async Task Operate(SparkProExecutive executive, SparkEngineeringDeviceOperator device, string id, bool on)
        {
            executive.Ask($"spark propose device {id} {(on ? "on" : "off")}");
            Require(executive.LastAnswer.Contains("REVIEW ONLY", StringComparison.Ordinal), executive.LastAnswer);
            executive.Ask("spark approve device action");
            Require(device.IsBusy, executive.LastAnswer);
            await Wait(() => !device.IsBusy, "native device operation " + id, 100f);
            Require(device.LastStatus.StartsWith("VERIFIED", StringComparison.Ordinal), device.LastStatus);
        }

        private static async Task InstallBridgeResistors(Transform layout, Action<string> log)
        {
            var shelf = Object.FindFirstObjectByType<SparkObjectShelf>();
            Require(shelf, "Level 20 requires the native resistor shelf.");
            var slots = layout.GetComponentsInChildren<SparkShelfSlot>()
                .Where(slot => slot.Kind == SparkShelfSlotKind.Workstation && slot.SlotId.StartsWith("Register_Slot_", StringComparison.Ordinal))
                .ToArray();
            Require(slots.Length == 4, $"Expected four authored resistor slots, found {slots.Length}.");
            foreach (var slot in slots)
            {
                int ohms = slot.SlotId.EndsWith("_TOP", StringComparison.Ordinal) ? 330 : 470;
                bool Matches(SparkShelfItem item)
                {
                    var resistor = item ? item.GetComponentInChildren<SparkResistor>() : null;
                    return resistor && Mathf.Approximately(resistor.ResistanceOhms, ohms);
                }
                if (Matches(slot.InstalledItem)) continue;
                if (slot.Occupant)
                {
                    var occupant = slot.Occupant;
                    Require(shelf.TryReturnToShelf(occupant), $"Could not return {occupant.name} through the shelf.");
                    await Wait(() => !occupant.IsMoving && !slot.Occupant, "resistor returned to storage");
                }
                var item = shelf.RegisteredItems.FirstOrDefault(candidate => candidate && !candidate.IsAtWorkbench && Matches(candidate));
                if (!item)
                {
                    string path = $"Assets/My_Assets/_Project_Spark/Prefabs/Electronic Components/Registers/Resistor_{ohms} Ω.prefab";
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    Require(prefab, $"Missing authorized {ohms} ohm inventory prefab.");
                    var placed = Object.Instantiate(prefab, layout);
                    item = placed.GetComponent<SparkShelfItem>();
                    shelf.RegisterLevelItems(layout);
                }
                Require(item && shelf.MoveToSlot(item, slot), $"Native shelf refused {ohms} ohm in {slot.SlotId}.");
                await Wait(() => !item.IsMoving && slot.InstalledItem == item, "authored resistor slot binding");
            }
            log("PASS Four authored bridge workstations populated via the native shelf with 330/470 ohm parts.");
        }

        private static async Task Wait(Func<bool> check, string description, float seconds = 35f)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!check())
            {
                Require(Time.realtimeSinceStartup < deadline, "Timed out waiting for " + description);
                await Task.Delay(35);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
