using System;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    internal static class SparkFlightLevel20Verification
    {
        private const string Preview = "draw real wire from SwitchOutput to R_LTOP_Input";

        internal static async Task Verify(Action<string> log)
        {
            var director = Object.FindObjectsByType<SparkNativeWireDirector>(FindObjectsSortMode.None).Single();
            var manager = Object.FindFirstObjectByType<LevelGamePlayManager>();
            var executive = director.GetComponent<SparkProExecutive>();
            var character = director.GetComponent<SparkProCharacter>();
            var flight = director.GetComponent<SparkV5FlightController>();
            var auto = director.GetComponent<SparkNativeWireAutoEngineer>();
            var performer = director.GetComponent<SparkNativeWireCinematic>();
            var agent = director.GetComponent<NavMeshAgent>();
            var port = director.GetComponent<SparkAINativeSignalWirePort>();
            var builder = new SerializedObject(port).FindProperty("playerBuilder").objectReferenceValue as SignalWireBuilder;
            Require(manager && executive && character && flight && auto && performer && agent && port && builder,
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
                .Single(candidate => candidate.LevelNumber == 20);
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

            Power(true);
            executive.Ask(Preview);
            Require(executive.LastAnswer.Contains("Turn off the virtual power", StringComparison.OrdinalIgnoreCase) &&
                !director.IsBusy && !builder.IsDrawing, "Energized supply did not block wiring. " + executive.LastAnswer);
            Power(false);
            executive.SetExaminationMode(true);
            executive.Ask(Preview);
            Require(executive.LastAnswer.Contains("Independent examination", StringComparison.OrdinalIgnoreCase) &&
                !director.IsBusy, "Exam mode did not block wiring.");
            executive.SetExaminationMode(false);
            log("PASS Virtual power and independent exam rejected wiring.");

            var obstruction = new GameObject("Flight verification obstacle");
            obstruction.transform.position = first.a.transform.position + Vector3.up * .6f;
            var blocker = obstruction.AddComponent<BoxCollider>();
            blocker.size = new Vector3(3f, 2.5f, 3f);
            Physics.SyncTransforms();
            Begin(executive, director);
            await Wait(() => !director.IsBusy, "blocked approach to fail", 45f);
            Require(!builder.IsDrawing && !circuit.HasConnectionBetween(first.a, first.b) &&
                flight.LastApproachReport.Contains(obstruction.name), "Blocked action did not stop at the actual collider. " + director.LastStatus);
            Require(!blocker.bounds.Contains(flight.transform.position + Vector3.up * .5f),
                "The walking approach crossed the obstacle before flight planning.");
            log("PASS Obstructed source produced no cable. " + flight.LastApproachReport);
            obstruction.SetActive(false);
            Object.Destroy(obstruction);

            int beforeFlight = flight.SuccessfulApproaches;
            Begin(executive, director);
            await Wait(() => flight.IsMoving || !director.IsBusy, "first autonomous flight");
            Require(flight.IsMoving && !builder.IsDrawing, director.LastStatus);
            executive.Ask("cancel wire");
            Require(!director.IsBusy && !flight.IsMoving && !builder.IsDrawing &&
                !circuit.HasConnectionBetween(first.a, first.b), "Cancellation left an active flight or native cable.");
            log("PASS Cancellation stopped flight and left the circuit unchanged.");

            int observedRoutes = 0;
            int gripSamples = 0;
            float largestGripGap = 0f;
            string previousReport = null;
            Begin(executive, director);
            await Wait(() =>
            {
                if (flight.SuccessfulApproaches > beforeFlight && previousReport != flight.LastApproachReport)
                {
                    previousReport = flight.LastApproachReport;
                    log(previousReport);
                    observedRoutes++;
                }
                var path = builder.CurrentPath;
                if (path && path.HasPreview && performer.TryGetHeldCableGrip(out var grip))
                {
                    largestGripGap = Mathf.Max(largestGripGap, Vector3.Distance(grip, path.PreviewPoint));
                    gripSamples++;
                }
                return !director.IsBusy;
            }, "the physical SwitchOutput to R_LTOP_Input wire", 85f);
            Require(flight.SuccessfulApproaches > beforeFlight && observedRoutes > 0 &&
                circuit.HasConnectionBetween(first.a, first.b), director.LastStatus);
            ulong wire = port.LastAIConnectionId;
            var registries = Object.FindObjectsByType<SparkWireRegistry>(FindObjectsSortMode.None);
            Require(wire != 0 && circuit.HasConnection(wire) && registries.Any(registry => registry.TryGetWire(wire, out _)),
                "The first flight-assisted cable must exist in the visual registry and the native graph.");
            Require(gripSamples > 4 && largestGripGap < .015f,
                $"Native cable left the actual hand grip in {gripSamples} samples. Maximum gap {largestGripGap:F4}.");
            log($"PASS Animated grasp followed by native cable preview, {gripSamples} samples and maximum gap {largestGripGap:F4} units.");
            log($"PASS Flight approached the authored terminals, the hands attached the player cable and graph verified wire {wire}.");
            executive.Ask("undo last AI wire");
            Require(!circuit.HasConnectionBetween(first.a, first.b), executive.LastAnswer);

            executive.Ask("preview approved wiring plan");
            log(executive.LastAnswer);
            executive.Ask("auto wire practice level");
            Require(executive.LastAnswer.Contains("10", StringComparison.Ordinal) &&
                executive.LastAnswer.Contains("performing", StringComparison.OrdinalIgnoreCase), executive.LastAnswer);
            int reported = 0;
            await Wait(() =>
            {
                int count = pairs.Count(pair => circuit.HasConnectionBetween(pair.a, pair.b));
                if (count != reported) { reported = count; log($"Reviewed plan: {count}/10 native links."); }
                return !auto.IsExecuting;
            }, "all ten reviewed plan wires", 360f);
            Require(reported == 10 && !director.IsBusy, auto.LastStatus + " " + director.LastStatus);
            Power(true);
            if (!powerSwitch.IsClosed) powerSwitch.ToggleState();
            await Wait(() => manager.SatisfiedTargetCount == 5 && manager.IsCompleted,
                "powered Level 20 evaluator at 5/5");
            log("PASS Reviewed 10-wire plan and native powered evaluator at 5/5.");

            Power(false);
            Require(manager.RestartLevel(), "Could not reload Level 20 before level-swap cancellation.");
            await Wait(() => SparkAILevelContext.Capture(manager).HasEvaluation &&
                SparkAILevelContext.Capture(manager).Layout != level.Layout, "fresh Level 20");
            var fresh = SparkAILevelContext.Capture(manager).Layout;
            await InstallBridgeResistors(fresh.transform, log);
            await Wait(() => fresh.SceneBindings.TryGetTerminal("R_LTOP_Input", out _), "reloaded resistor terminal");
            Begin(executive, director);
            await Wait(() => flight.IsMoving || !director.IsBusy, "active flight before changing levels");
            Require(flight.IsMoving && !builder.IsDrawing, director.LastStatus);
            Require(manager.StartLevel(Slot(21)), "Could not change to Level 21.");
            await Wait(() => !director.IsBusy, "level swap cancellation");
            Require(!flight.IsMoving && !builder.IsDrawing, "Level switch left active flight or a loose cable.");
            log("PASS Active level change cancelled the flight wiring action.");

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
            foreach (var socket in new[] { highA, highB })
                foreach (var collider in socket.GetComponentsInChildren<Collider>())
                    collider.isTrigger = true;
            Physics.SyncTransforms();
            if (!flight.IsFlying)
            {
                Require(flight.RequestTakeoff(out string setup), setup);
                await Wait(() => flight.IsFlying && !flight.IsMoving && flight.LastOperationSucceeded, "initial hover for walking fixture");
            }
            int flightsBeforeGroundTest = flight.SuccessfulApproaches;
            bool sawWalk = false, carriedCableWhileWalking = false;
            Begin(executive, director);
            await Wait(() =>
            {
                bool walking = !flight.IsFlying && agent.isActiveAndEnabled && agent.isOnNavMesh && agent.velocity.sqrMagnitude > .02f;
                sawWalk |= walking;
                carriedCableWhileWalking |= walking && builder.IsDrawing && performer.TryGetHeldCableGrip(out _);
                return !director.IsBusy;
            }, "forced walkable terminal heights", 100f);
            Require(sawWalk && carriedCableWhileWalking && flight.SuccessfulApproaches == flightsBeforeGroundTest &&
                manager.CircuitSystem.HasConnectionBetween(highA, highB), "Spark should land and walk when reachable. " + director.LastStatus);
            log("PASS Controlled reach-height fixture: Spark landed from hover, walked to both reachable sockets and carried the native cable while walking, with zero flight approaches.");
        }

        private static void Begin(SparkProExecutive executive, SparkNativeWireDirector director)
        {
            executive.Ask(Preview);
            Require(executive.LastAnswer.Contains("No circuit change yet", StringComparison.OrdinalIgnoreCase), executive.LastAnswer);
            executive.Ask("confirm wire");
            Require(director.IsBusy, executive.LastAnswer);
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
