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
using Unity.Profiling;
using Object = UnityEngine.Object;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    internal static class SparkFlightLevel20Verification
    {
        private const string Preview = "draw real wire from SwitchOutput to R_LTOP_Input";

        internal static async Task Verify(Action<string> log, string cableCase = "")
        {
            bool v19 = cableCase is "v19" or "v201-level20";
            VerifyCableGeometry(log);
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
            if (cableCase == "v191-board100")
            {
                const string key = SparkLevel100Process.CheckpointKey;
                bool hadCheckpoint = PlayerPrefs.HasKey(key);
                string checkpoint = PlayerPrefs.GetString(key, "");
                SparkLevel100Process board = null;
                try
                {
                    Require(manager.StartLevel(Slot(100)), "Native Level 100 did not load.");
                    await Wait(() => manager.ActiveLevel.LevelNumber == 100 &&
                        manager.LayoutLoader.ActiveLayout.GetComponentInChildren<SparkLevel100Process>(true),
                        "bound Level 100 board");
                    board = manager.LayoutLoader.ActiveLayout.GetComponentInChildren<SparkLevel100Process>(true);
                    Require(board.Stage == 0 && !board.Powered && board.parts.Length > 1 &&
                        board.parts[0].contact && !board.parts[0].contact.IsClosed,
                        "The test needs an empty, isolated native board.");
                    int installed = board.Installed;
                    board.SelectFootprint(0);
                    board.SelectPart(1);
                    Require(!board.FitPart() && board.Installed == installed, "A wrong component was accepted.");
                    board.SelectPart(0);
                    Require(board.FitPart() && board.Solder(), "The native board rejected its authored part.");
                    Require(board.parts[0].contact.IsClosed && board.Installed == installed + 1,
                        "The native board did not register the soldered component.");
                    Require(!manager.IsCompleted, "A single board operation unexpectedly completed the level.");
                    log("PASS Level 100 board rejected the wrong part and registered the correctly fitted and soldered part.");
                }
                finally
                {
                    // Flush this fixture's board checkpoint before restoring the player's previous value.
                    if (board) board.enabled = false;
                    if (hadCheckpoint) PlayerPrefs.SetString(key, checkpoint);
                    else PlayerPrefs.DeleteKey(key);
                    PlayerPrefs.Save();
                }
                return;
            }
            if (cableCase is "v19-starter" or "v19-level48" or "v20-level48" or "v20-level48-wire10" or "v201-level1" or "v201-level48")
            {
                int mission = cableCase is "v19-starter" or "v201-level1" ? 1 : 48;
                Require(manager.StartLevel(Slot(mission)), $"Native Level {mission} did not start.");
                await Wait(() => executive.ActiveLevelManager == manager &&
                    SparkAILevelContext.Capture(manager).HasEvaluation && port.IsConfigured, "mission native evaluator");
                var starter = SparkAILevelContext.Capture(manager);
                var prefab = manager.LayoutLoader.ActiveSourcePrefab;
                Require(SparkUniversalConstructionEditor.TryReadMissionWires(starter.Definition, prefab,
                    out var wires, out string source, out string error), error);
                bool wire10Only = cableCase == "v20-level48-wire10";
                if (wire10Only)
                {
                    Require(wires.Length >= 10 && wires[9].fromId == "DRIVER_MOTOR_MINUS" &&
                        wires[9].toId == "PRINT_MOTOR_MINUS", "The wire-10 evidence changed.");
                    wires = new[] { wires[9] };
                    log("Isolated native wire-10 test from a fresh Level 48. The preceding nine wires are outside this fixture.");
                }
                var trial = ScriptableObject.CreateInstance<SparkNativeWiringPlan>();
                try
                {
                    trial.LoadDraft(new SparkNativeWiringPlan.RecordedDraft
                    { levelNumber = mission, layoutName = prefab.name, wires = wires });
                    Require(trial.TryApprove("In-memory verification fixture", source, out error), error);
                    Require(trial.ReviewAuthoredRevision(starter.Definition, prefab, out error), error);
                    var cache = typeof(SparkEngineeringControlHub).GetField("loadedPlans",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    Require(cache != null, "The approved-plan cache is missing.");
                    cache.SetValue(hub, new[] { trial });
                    var registration = starter.Layout.GetComponentInChildren<SparkLevel48Registration>(true);
                    if (registration)
                        await Wait(() => registration.statusText &&
                            registration.statusText.text != "Build the registration circuit.", "Level 48 native controller initialization");
                    var starterSupply = registration ? registration.source :
                        starter.Layout.GetComponentsInChildren<SparkPowerSupply>(true).Single();
                    var interaction = new SparkInteractionContext(executive, starterSupply, SparkInteractionType.Toggle,
                        SparkToolType.Select, starterSupply.transform.position, Vector3.up, default, starterSupply.gameObject);
                    if (starterSupply.IsOutputActive)
                    {
                        if (registration) registration.MainPower();
                        else Require(starterSupply.SetOutput(false, interaction).Succeeded, "The starter supply did not turn off.");
                    }
                    var activeSupplies = starter.Layout.GetComponentsInChildren<SparkPowerSupply>(true)
                        .Where(supply => supply.IsOutputActive).Select(supply => supply.ObjectId).ToArray();
                    Require(activeSupplies.Length == 0, "Mission supplies still active after isolation: " +
                        string.Join(", ", activeSupplies));
                    executive.Ask("spark auto mode");
                    executive.Ask("hey Spark draw current level");
                    Require(hub.IsExecuting, executive.LastAnswer);
                    if (cableCase == "v201-level48")
                    {
                        var seen = new System.Collections.Generic.HashSet<(int, SparkNativeWireCinematic.CableStage, bool)>();
                        int verified = 0;
                        var originalArms = character.GetComponent<SparkTelescopicArms>();
                        await Wait(() =>
                        {
                            while (verified < hub.CompletedSteps)
                            {
                                var previous = wires[verified++];
                                bool found = starter.Layout.SceneBindings.TryResolveLivePair(starter.Layout, previous.fromId,
                                    previous.toId, out var from, out var to, out _) &&
                                    manager.CircuitSystem.HasConnectionBetween(from, to);
                                Require(found, $"Wire {verified} had no authoritative topology evidence.");
                                log($"Wire {verified} {previous.fromId} → {previous.toId}: native registration verified. {director.LastStatus}");
                            }
                            int index = hub.CompletedSteps;
                            if (index < wires.Length && performer.IsPerforming && seen.Add((index, performer.Stage, flight.IsFlying)))
                            {
                                var step = wires[index];
                                bool cable = performer.TryMeasureHeldCable(out _, out float rest, out float route, out float extension);
                                var target = starter.Layout.SceneBindings.TryGetTerminal(step.toId, out var destination)
                                    ? destination.transform.position.ToString("F3") : "unresolved";
                                log($"Wire {index + 1} {step.fromId} → {step.toId}: {performer.Stage}; " +
                                    $"{(flight.IsFlying ? "flight" : agent.isActiveAndEnabled && agent.isOnNavMesh ? "ground" : "stationary")}; " +
                                    $"root {character.transform.position:F3}, facing {character.transform.rotation.eulerAngles:F1}, " +
                                    $"shoulder {character.GetAnchor("Shoulder_R").position:F3}, elbow {character.GetAnchor("Elbow_R").position:F3}, " +
                                    $"wrist {character.GetAnchor("Wrist_R").position:F3}, destination {target}, reach {originalArms.MaximumReach:F3} m" +
                                    (cable ? $"; cable {route:F3}/{rest:F3} m, extension {extension:F3}." : "."));
                            }
                            return !hub.IsExecuting;
                        }, "complete Level 48 sequence", 540f);
                        log("Final motion: " + flight.LastApproachReport);
                        Require(verified == wires.Length, $"Level 48 stopped after {verified}/{wires.Length}: {hub.LastMessage} {director.LastStatus}");
                    }
                    else if (mission == 48 && !wire10Only)
                    {
                        var step = wires[3];
                        Require(step.toId == "SENSOR_GND" &&
                            starter.Layout.SceneBindings.TryResolveLivePair(starter.Layout, step.fromId, step.toId,
                                out _, out _, out _), "The Level 48 regression must use the authored fourth wire.");
                        var sampledStages = new System.Collections.Generic.HashSet<SparkNativeWireCinematic.CableStage>();
                        var originalArms = character.GetComponent<SparkTelescopicArms>();
                        await Wait(() =>
                        {
                            if (hub.CurrentAction.StartsWith("4/", StringComparison.Ordinal) && performer.IsPerforming &&
                                sampledStages.Add(performer.Stage))
                            {
                                performer.TryMeasureNativeMotion(out var motion);
                                if (performer.TryMeasureHeldCable(out var grip, out float rest, out float route, out _))
                                    log($"Wire 4 {performer.Stage}: root {character.transform.position:F3}, " +
                                        $"NavMesh {agent.isOnNavMesh}, flight {flight.IsFlying}, " +
                                        $"shoulder {character.GetAnchor("Shoulder_R").position:F3}, " +
                                        $"elbow {character.GetAnchor("Elbow_R").position:F3}, wrist {character.GetAnchor("Wrist_R").position:F3}, " +
                                        $"index {character.GetAnchor("FingerTip0_R").position:F3}, cable end {grip:F3}, " +
                                        $"extension ratio {motion.ExtensionRatio:F3}, maximum reach {originalArms.MaximumReach:F3} m, " +
                                        $"cable {route:F3}/{rest:F3} m, cable contacts {motion.CableContacts}.");
                            }
                            return !hub.IsExecuting || hub.CompletedSteps >= 4;
                        }, "previously failing Level 48 wire 4", 180f);
                        Require(starter.Layout.SceneBindings.TryResolveLivePair(starter.Layout, step.fromId, step.toId,
                            out var sourcePin, out var sensorGround, out error) &&
                            manager.CircuitSystem.HasConnectionBetween(sourcePin, sensorGround),
                            $"Wire 4 regression at {step.toId}: {hub.LastMessage} {director.LastStatus}");
                        log($"PASS Targeted Level 48 wire 4. Native circuit registered {step.fromId} → {step.toId} at {sensorGround.transform.position:F3}.");
                    }
                    if (cableCase == "v20-level48")
                    {
                        Require(wires.Length >= 10, "Level 48 needs an authored wire 10.");
                        var step = wires[9];
                        Require(step.fromId == "DRIVER_MOTOR_MINUS" || step.toId == "DRIVER_MOTOR_MINUS",
                            "Wire 10 no longer references the recorded connector.");
                        await Wait(() => !hub.IsExecuting || hub.CompletedSteps >= 10, "the Level 48 wire 10", 280f);
                        log(hub.CompletedSteps < 9 ? "Earlier native failure before wire 10: " + director.LastStatus :
                            "Wire 10 native status: " + director.LastStatus);
                        log("Wire 10 flight diagnostics: " + flight.LastApproachReport);
                        Require(starter.Layout.SceneBindings.TryResolveLivePair(starter.Layout, step.fromId, step.toId,
                            out var sourcePin, out var destinationPin, out error) &&
                            manager.CircuitSystem.HasConnectionBetween(sourcePin, destinationPin),
                            $"Level 48 wire 10 {step.fromId} → {step.toId}: {hub.LastMessage} {director.LastStatus}");
                        log($"PASS Level 48 wire 10 {step.fromId} → {step.toId} registered in SparkCircuitSystem.");
                    }
                    await Wait(() => !hub.IsExecuting, $"Level {mission}'s physical cables", mission == 1 ? 100f : 420f);
                    if (wire10Only) log("Wire 10 native status: " + director.LastStatus + "\nFlight geometry: " + flight.LastApproachReport);
                    foreach (var step in wires)
                    {
                        Require(starter.Layout.SceneBindings.TryResolveLivePair(starter.Layout, step.fromId, step.toId,
                            out var a, out var b, out error), error);
                        Require(manager.CircuitSystem.HasConnectionBetween(a, b), hub.LastMessage + " " + director.LastStatus);
                    }
                    log($"PASS Level {mission} exact command registered {wires.Length}/{wires.Length} native wires.");
                    if (wire10Only) return;
                    if (mission == 1)
                    {
                        Require(starterSupply.SetOutput(true, interaction).Succeeded, "The starter supply did not turn on.");
                        await Wait(() => manager.IsFailed || manager.SatisfiedTargetCount == 1 && manager.IsCompleted,
                            "starter's official objective");
                        Require(manager.SatisfiedTargetCount == 1 && manager.IsCompleted,
                            $"Starter result {manager.SatisfiedTargetCount}/1, failure {manager.LastFailureReason}.");
                        log($"PASS Starter native wiring and evaluator 1/1. Review revision {trial.ReviewedRevision}.");
                    }
                    else
                    {
                        Require(registration, "The Level 48 registration controller is missing.");
                        await VerifyRegistrationMission(registration, manager, log);
                        log($"PASS Level 48 native wiring and evaluator 6/6. Review revision {trial.ReviewedRevision}.");
                    }
                }
                finally
                {
                    hub.Reload();
                    Object.Destroy(trial);
                }
                return;
            }
            Require(manager.StartLevel(Slot(20)), "Native Level 20 did not start.");
            await Wait(() => SparkAILevelContext.Capture(manager).HasEvaluation &&
                manager.ActiveLevel.LevelNumber == 20 && port.IsConfigured, "Level 20 with native evaluator");
            var level = SparkAILevelContext.Capture(manager);
            var circuit = manager.CircuitSystem;
            var bindings = level.Layout.SceneBindings;
            var supply = level.Layout.GetComponentsInChildren<SparkPowerSupply>().Single();
            var powerSwitch = level.Layout.GetComponentsInChildren<SparkSwitch>().Single();
            if (cableCase == "switch")
            {
                Require(!supply.IsOutputActive, "The test requires de-energized hardware.");
                executive.Ask("spark assisted mode");
                await VerifyStandingSwitch(executive, device, flight, agent, powerSwitch, log);
                return;
            }
            Require(bindings.ResolveLiveTerminal(level.Layout, "SwitchOutput", out _) ==
                SparkLevelSceneBindings.TerminalResolution.Resolved, "The fixed SwitchOutput binding must resolve.");
            Require(bindings.ResolveLiveTerminal(level.Layout, "R_LTOP_Input", out _) ==
                SparkLevelSceneBindings.TerminalResolution.RUNTIME_SLOT_UNPOPULATED,
                "The left-top resistor must require installation in its authored workstation.");
            Require(bindings.ResolveLiveTerminal(level.Layout, "not_a_level_20_terminal", out _) ==
                SparkLevelSceneBindings.TerminalResolution.ID_NOT_DECLARED, "Unknown IDs must fail closed.");
            executive.Ask("spark assisted mode");
            if (v19)
            {
                executive.Ask("hey Spark draw current level");
                Require(executive.LastAnswer.Contains("RUNTIME_SLOT_UNPOPULATED", StringComparison.Ordinal) &&
                    !director.IsBusy, executive.LastAnswer);
            }
            executive.Ask("Spark, inspect the next connection.");
            Require(executive.LastAnswer.Contains("RUNTIME_SLOT_UNPOPULATED: R_LTOP_Input", StringComparison.Ordinal),
                "Advisor must explain the missing workstation assignment before approval.");
            log("PASS Fixed terminal resolution, unknown ID rejection and explicit unpopulated workstation diagnosis.");
            await InstallBridgeResistors(level.Layout.transform, log);
            var plan = Resources.LoadAll<SparkNativeWiringPlan>("SparkNativeWirePlans")
                .Single(candidate => candidate.LevelNumber == 20 && candidate.IsApproved);
            Require(plan.IsApproved && plan.Wires.Count == 10, "Level 20's reviewed ten-wire plan is required.");
            await Wait(() => plan.Wires.All(step => bindings.TryGetTerminal(step.fromId, out _) &&
                bindings.TryGetTerminal(step.toId, out _)), "all reviewed terminal bindings after shelf placement");
            if (v19)
            {
                Require(plan.ValidateStoredAuthoredRevision(out string reason), reason);
                var tampered = Object.Instantiate(plan);
                try
                {
                    var state = new SerializedObject(tampered);
                    state.FindProperty("reviewedRevision").stringValue = "deliberate invalidation";
                    state.ApplyModifiedPropertiesWithoutUndo();
                    Require(!tampered.MatchesReviewedLayout(manager.ActiveLevel, manager.LayoutLoader.ActiveSourcePrefab, out _),
                        "An edited revision retained authorization.");
                }
                finally { Object.Destroy(tampered); }
                executive.Ask("spark draw this level");
                Require(hub.PendingStep == "SwitchOutput -> R_LTOP_Input" && !director.IsBusy, executive.LastAnswer);
                executive.Ask("spark stop drawing");
                Require(!hub.IsExecuting && hub.PendingStep == "None", "Universal stop left the Assisted proposal active.");
                log("PASS v19 runtime dependency, Assisted suggestion, revision invalidation and pending stop.");
            }
            var pairs = plan.Wires.Select(step =>
            {
                Require(bindings.TryResolveLivePair(level.Layout, step.fromId, step.toId, out var a, out var b, out string error),
                    $"Authored terminals invalid for {step.fromId} to {step.toId}. {error}");
                return (a, b);
            }).ToArray();
            var first = pairs[0];
            bool phase2 = cableCase == "phase2";
            var playerProfile = (builder.DrawingMode, builder.CableRouting, builder.TerminalInputStyle, builder.ExternalInputControl);
            bool diagnosticBaseline = cableCase == "cable-before";
            using var capture = cableCase.Length == 0 || v19 ? null : new SparkNativeCableVisualCapture(
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
            if (v19)
            {
                executive.Ask("hey Spark draw current level");
                Require(hub.Mode == SparkEngineeringControlHub.OperatingMode.Manual && !hub.IsExecuting &&
                    executive.LastAnswer.Contains("Manual mode", StringComparison.Ordinal), executive.LastAnswer);
                executive.Ask("spark drawing status");
                Require(executive.LastAnswer.Contains("Level 20", StringComparison.Ordinal), executive.LastAnswer);
            }
            executive.Ask(Preview);
            Require(!director.IsBusy && !builder.IsDrawing && !circuit.HasConnectionBetween(first.a, first.b),
                "Manual mode must leave the player in control.");
            executive.Ask("spark assisted mode");
            executive.Ask("spark propose device PowerSupply_01 on");
            Require(executive.LastAnswer.Contains("missing", StringComparison.OrdinalIgnoreCase) && !supply.IsOutputActive,
                "The device operator allowed an incomplete circuit to energize. " + executive.LastAnswer);
            Power(true);
            executive.Ask("Spark, suggest the next wire.");
            executive.Ask("Spark, show me where the source terminal is.");
            Require(executive.LastAnswer.Contains("reviewed source", StringComparison.OrdinalIgnoreCase) &&
                !director.IsBusy, executive.LastAnswer);
            executive.Ask("Spark, connect the approved wire.");
            Require(executive.LastAnswer.Contains("virtual power output OFF", StringComparison.OrdinalIgnoreCase) &&
                !director.IsBusy && !builder.IsDrawing, "Energized supply did not block wiring. " + executive.LastAnswer);
            Power(false);
            executive.SetExaminationMode(true);
            if (v19)
            {
                executive.Ask("spark auto draw");
                Require(!hub.IsExecuting && executive.LastAnswer.Contains("Independent exam", StringComparison.Ordinal),
                    executive.LastAnswer);
            }
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

            if (phase2)
            {
                async Task InterruptPreview(string name, Action change, Action restore, string reason)
                {
                    await Begin(executive, director, hub);
                    await Wait(() => builder.IsDrawing || !director.IsBusy, "Phase 2 preview for " + name);
                    Require(builder.IsDrawing, director.LastStatus);
                    var preview = builder.CurrentPath;
                    try
                    {
                        change();
                        Physics.SyncTransforms();
                        await Wait(() => !director.IsBusy && !hub.IsExecuting && !preview, "Phase 2 interruption " + name);
                        Require(!builder.IsDrawing && !port.IsWiring && !circuit.HasConnectionBetween(first.a, first.b) &&
                            playerProfile == (builder.DrawingMode, builder.CableRouting, builder.TerminalInputStyle, builder.ExternalInputControl),
                            "Interruption failed to restore the player tool or left an unapproved link.");
                        if (reason != null) Require(director.LastStatus.Contains(reason), director.LastStatus);
                    }
                    finally { restore(); Physics.SyncTransforms(); }
                    log("PASS Live preview interruption for " + name + ". " + director.LastStatus);
                }
                Vector3 sourcePosition = first.a.transform.position;
                Vector3 endPosition = first.b.transform.position;
                await InterruptPreview("source motion", () => first.a.transform.position += Vector3.up * .03f,
                    () => first.a.transform.position = sourcePosition, "source terminal moved");
                await InterruptPreview("destination motion", () => first.b.transform.position += Vector3.up * .03f,
                    () => first.b.transform.position = endPosition, "destination terminal moved");
                await InterruptPreview("destination inactive", () => first.b.gameObject.SetActive(false),
                    () => first.b.gameObject.SetActive(true), "destination terminal disappeared");
                Vector3 robotPosition = default;
                await InterruptPreview("unplanned Spark displacement", () =>
                {
                    robotPosition = flight.transform.position;
                    flight.transform.position += Vector3.forward * .30f;
                }, () => flight.transform.position = robotPosition, "position changed");
                await InterruptPreview("mode returned to Manual", () => executive.Ask("spark manual mode"),
                    () => executive.Ask("spark assisted mode"), null);
                await InterruptPreview("independent exam began", () => executive.SetExaminationMode(true),
                    () => executive.SetExaminationMode(false), null);
                await InterruptPreview("virtual supply energized", () => Power(true), () => Power(false), null);
                var performerSettings = new SerializedObject(performer);
                var length = performerSettings.FindProperty("cableRestLength");
                float usual = length.floatValue;
                length.floatValue = .15f;
                performerSettings.ApplyModifiedPropertiesWithoutUndo();
                try
                {
                    await Begin(executive, director, hub);
                    await Wait(() => !director.IsBusy && !hub.IsExecuting, "short lead rejection");
                    Require(!builder.IsDrawing && !port.IsWiring && !circuit.HasConnectionBetween(first.a, first.b) &&
                        (director.LastStatus.Contains("too short", StringComparison.OrdinalIgnoreCase) ||
                        director.LastStatus.Contains("shorter", StringComparison.OrdinalIgnoreCase)), director.LastStatus);
                }
                finally { length.floatValue = usual; performerSettings.ApplyModifiedPropertiesWithoutUndo(); }
                log("PASS Short physical lead rejected and native tool released.");

                var unboundContact = first.b.gameObject.AddComponent<SparkNativeConnectorContact>();
                try
                {
                    await Begin(executive, director, hub);
                    await Wait(() => !director.IsBusy && !hub.IsExecuting, "unverified alignment rejection");
                    Require(!builder.IsDrawing && !port.IsWiring && !circuit.HasConnectionBetween(first.a, first.b) &&
                        director.LastStatus.Contains("destination contact frame", StringComparison.OrdinalIgnoreCase),
                        director.LastStatus);
                }
                finally { Object.Destroy(unboundContact); }
                await Wait(() => !unboundContact, "test contact component removal");
                log("PASS A connector with an unbound mating frame was rejected before attachment.");
            }

            bool walked = false, flew = false, sawRoute = false;
            using (var movie = phase2 ? new SparkNativeCableVisualCapture.MotionRecording("native-wire") : null)
            {
                await Begin(executive, director, hub);
                await Wait(() =>
                {
                    walked |= agent.isActiveAndEnabled && agent.isOnNavMesh && agent.velocity.sqrMagnitude > .02f;
                    flew |= flight.IsFlying || flight.IsMoving;
                    sawRoute |= builder.IsDrawing && performer.IsPhysicalCable && performer.TryGetHeldCableGrip(out _);
                    return !director.IsBusy;
                }, "the grounded SwitchOutput to R_LTOP_Input wire", 65f);
                if (movie != null) await Task.Delay(160);
            }
            Require(!flew && flight.SuccessfulApproaches == beforeFlight, "The authored grounded fixture unexpectedly used flight.");
            Require(circuit.HasConnectionBetween(first.a, first.b), director.LastStatus);
            Require(sawRoute || diagnosticBaseline, "No physical native cable was sampled at a measured hand grip.");
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
            Require(totalLength < referenceSpan * 1.8f,
                $"The native cable developed an oversized route: {totalLength:F3} over a {referenceSpan:F3} span.");
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
            executive.Ask(v19 ? "hey Spark draw current level" : "auto wire practice level");
            Require(executive.LastAnswer.Contains("10", StringComparison.Ordinal) &&
                executive.LastAnswer.Contains("AUTO started", StringComparison.OrdinalIgnoreCase), executive.LastAnswer);
            executive.Ask(v19 ? "spark pause drawing" : "spark pause automation");
            await Wait(() => hub.Phase == SparkEngineeringControlHub.ExecutionPhase.Paused, "Auto pause");
            Require(!director.IsBusy && !builder.IsDrawing && !auto.IsExecuting, "Auto pause left an active wire owner.");
            executive.Ask(v19 ? "spark resume drawing" : "spark resume automation");
            int reported = 0;
            int carrySamples = 0, profilerSamples = 0;
            float carryGap = 0f;
            double mainThreadTotal = 0, gcTotal = 0;
            using var mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
            using var gcAllocated = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
            await Wait(() =>
            {
                int count = pairs.Count(pair => circuit.HasConnectionBetween(pair.a, pair.b));
                if (count != reported) { reported = count; log($"Reviewed plan: {count}/10 native links."); }
                if (performer.Stage == SparkNativeWireCinematic.CableStage.Carrying && builder.CurrentPath &&
                    builder.CurrentPath.HasPreview && performer.TryGetHeldCableGrip(out var heldGrip))
                {
                    if (agent.isActiveAndEnabled && agent.isOnNavMesh && agent.velocity.sqrMagnitude > .02f)
                    {
                        carrySamples++;
                        carryGap = Mathf.Max(carryGap, Vector3.Distance(heldGrip, builder.CurrentPath.PreviewPoint));
                    }
                    if (mainThread.Valid && gcAllocated.Valid && mainThread.Count > 0 && gcAllocated.Count > 0)
                    {
                        profilerSamples++;
                        mainThreadTotal += mainThread.LastValue;
                        gcTotal += gcAllocated.LastValue;
                    }
                }
                return !hub.IsExecuting;
            }, "all ten reviewed plan wires", 360f);
            Require(reported == 10 && !director.IsBusy && !auto.IsExecuting, hub.LastMessage + " " + director.LastStatus);
            log($"Authored plan walking carry samples: {carrySamples}; maximum grip/preview gap {carryGap:F4}.");
            if (profilerSamples > 0)
                log($"Editor carry counters: {profilerSamples} polls, mean main thread {mainThreadTotal / profilerSamples / 1000000d:F2} ms, " +
                    $"mean total frame GC allocation {gcTotal / profilerSamples:F0} bytes.");
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
            log(v19 ? "PASS Exact v19 command ran the reviewed plan and honored pause and resume."
                : "PASS v14 alone ran the reviewed plan and honored pause and resume.");
            executive.Ask("spark assisted mode");
            using (var movie = phase2 ? new SparkNativeCableVisualCapture.MotionRecording("standing-switch") : null)
                await VerifyStandingSwitch(executive, device, flight, agent, powerSwitch, log, phase2);
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
            var arms = director.GetComponent<SparkTelescopicArms>();
            Require(arms, "Original telescopic arms are required for the controlled walking fixture.");
            float originalReach = arms.MaximumReach;
            // The sockets are 2.4 units apart. Force two stances rather than
            // expecting the production planner to walk with both in reach.
            Require(arms.SetMaximumReach(1.15f), "Could not configure the controlled reach limit.");
            try
            {
                await Begin(executive, director, hub);
                await Wait(() =>
                {
                    bool walking = !flight.IsFlying && agent.isActiveAndEnabled && agent.isOnNavMesh && agent.velocity.sqrMagnitude > .02f;
                    sawWalk |= walking;
                    if (performer.Stage == SparkNativeWireCinematic.CableStage.Carrying &&
                        builder.CurrentPath && builder.CurrentPath.HasPreview && performer.TryGetHeldCableGrip(out var grip))
                    {
                        gripSamples++;
                        largestGripGap = Mathf.Max(largestGripGap, Vector3.Distance(grip, builder.CurrentPath.PreviewPoint));
                        carriedCableWhileWalking |= walking;
                    }
                    return !director.IsBusy;
                }, "forced walkable terminal heights", 100f);
            }
            finally { arms.SetMaximumReach(originalReach); }
            Require(sawWalk && carriedCableWhileWalking && flight.SuccessfulApproaches == flightsBeforeGroundTest &&
                gripSamples > 4 && largestGripGap < .015f &&
                manager.CircuitSystem.HasConnectionBetween(highA, highB),
                $"Walking={sawWalk}, carrying while walking={carriedCableWhileWalking}, samples={gripSamples}, gap={largestGripGap:F4}. " + director.LastStatus);
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
                if (v19 && definition.LevelNumber != plan.LevelNumber)
                {
                    executive.Ask("spark auto mode");
                    executive.Ask("hey spark wire current level");
                    Require(!hub.IsExecuting && !director.IsBusy &&
                        executive.LastAnswer.Contains($"Level {definition.LevelNumber}", StringComparison.Ordinal) &&
                        executive.LastAnswer.Contains("Construction blocked", StringComparison.Ordinal), executive.LastAnswer);
                    log($"PASS Level {definition.LevelNumber} authority and missing-plan rejection.");
                }
            }
        }

        private static async Task VerifyRegistrationMission(SparkLevel48Registration mission,
            LevelGamePlayManager manager, Action<string> log)
        {
            async Task Gate(int number)
            {
                await Wait(() => manager.IsFailed || manager.SatisfiedTargetCount >= number,
                    $"Level 48 official objective {number}/6", 50f);
                Require(!manager.IsFailed, manager.LastFailureReason);
                log($"PASS Level 48 official evaluator {number}/6.");
            }

            mission.MainPower();
            await Gate(1);
            await Wait(() => mission.calibrateButton && mission.calibrateButton.interactable, "registration calibrate is available");
            mission.Calibrate();
            await Gate(2);
            var modeLabel = mission.manualButton.GetComponentInChildren<UnityEngine.UI.Text>();
            if (modeLabel.text.Contains("MANUAL", StringComparison.Ordinal)) mission.ToggleAuto();
            mission.Speed30();
            await Wait(() => mission.startButton && mission.startButton.interactable, "calibrated machine can start");
            mission.StartWeb();
            await Gate(3);
            mission.Speed60();
            await Gate(4);
            mission.TestGlare();
            await Gate(5);
            mission.ToggleCover();
            await Wait(() => mission.resetButton && mission.resetButton.interactable ||
                mission.statusText && mission.statusText.text.Contains("SAFE STOP", StringComparison.Ordinal),
                "missing registration mark stops the machine", 20f);
            mission.ToggleCover();
            await Wait(() => mission.resetButton && mission.resetButton.interactable, "safe reset becomes available");
            mission.ResetFault();
            await Wait(() => mission.startButton && mission.startButton.interactable, "recovered machine can start");
            mission.StartWeb();
            await Gate(6);
            Require(manager.IsCompleted, "The official evaluator did not report Level 48 completed.");
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

        private static void VerifyCableGeometry(Action<string> log)
        {
            Vector3 source = Vector3.zero;
            Vector3 end = Vector3.right * 1.16f;
            var valid = new[] { source, Vector3.right * .58f + Vector3.down * .12f, end };
            Require(SparkAINativeSignalWirePort.TryValidateCableGeometry(valid, source, 1.30f, .025f,
                out float route, out float segment, out string reason), reason);
            float rise = Mathf.Sqrt(5.27f * 5.27f - .58f * .58f);
            var historicalLength = new[] { source, new Vector3(.58f, rise, 0f), end };
            Require(!SparkAINativeSignalWirePort.TryValidateCableGeometry(historicalLength, source, 12f, .025f,
                out float badRoute, out _, out _) && Mathf.Abs(badRoute - 10.54f) < .001f,
                "A 10.54-unit visual route escaped the native cable geometry guard.");
            Require(!SparkAINativeSignalWirePort.TryValidateCableGeometry(valid, source, .8f, .025f,
                out _, out _, out _), "An overstretched lead was accepted.");
            Require(!SparkAINativeSignalWirePort.TryValidateCableGeometry(valid, Vector3.up * .02f, 1.30f, .025f,
                out _, out _, out _), "A stale source position was accepted.");
            Require(!SparkAINativeSignalWirePort.TryValidateCableGeometry(
                new[] { source, Vector3.right, Vector3.right * .6f, end }, source, 2f, .025f,
                out _, out _, out _), "A backward cable segment was accepted.");
            log($"PASS Cable geometry contract. Normal {route:F3} over 1.160, peak segment {segment:F3}. " +
                $"Rejected synthetic {badRoute:F3} route, short lead, stale source and backward segment.");
        }

        private static async Task VerifyStandingSwitch(SparkProExecutive executive, SparkEngineeringDeviceOperator device,
            SparkV5FlightController flight, NavMeshAgent agent, SparkSwitch powerSwitch, Action<string> log, bool closeup = false)
        {
            var lever = powerSwitch.GetComponent<SwitchLeverController>();
            var arm = executive.GetComponent<SparkTelescopicArms>();
            Vector3 point = default;
            Require(lever && arm && lever.TryGetPressPoint(out point), "The physical switch press bindings are missing.");
            async Task VerifyPress(bool on)
            {
                bool previous = powerSwitch.IsClosed;
                Require(previous != on, "Expected an actual switch state change.");
                bool grounded = false;
                float gap = float.PositiveInfinity;
                Vector3 feet = default;
                void OnChanged()
                {
                    if (powerSwitch.IsClosed == previous || !float.IsPositiveInfinity(gap)) return;
                    grounded = !flight.IsFlying && agent.isOnNavMesh && !agent.hasPath &&
                        agent.velocity.sqrMagnitude < .0004f;
                    feet = flight.transform.position;
                    if (arm.TryGetGripPosition(SparkTelescopicArms.ManipulatorSide.Right, out var index))
                    {
                        gap = Vector3.Distance(index, lever.LeverTransform.TransformPoint(point));
                        if (closeup) SparkNativeCableVisualCapture.CaptureCloseup(on ? "switch-on" : "switch-off", flight.transform, index);
                    }
                }
                powerSwitch.VisualStateChanged += OnChanged;
                try { await Operate(executive, device, "Power_Switch", on); }
                finally { powerSwitch.VisualStateChanged -= OnChanged; }
                Require(grounded && powerSwitch.IsClosed == on && gap <= Mathf.Min(.005f, arm.ContactTolerance) && arm.IsRightArmResting &&
                    Vector3.Distance(feet, flight.transform.position) < .02f,
                    $"Switch press {on} failed standing, contact or release checks (grounded {grounded}, gap {gap:F4}, " +
                    $"arm resting {arm.IsRightArmResting}, drift {Vector3.Distance(feet, flight.transform.position):F4}).");
                log($"PASS Standing switch {(on ? "ON" : "OFF")}, root {feet:F3}, index gap {gap:F4} m, original arm retracted.");
            }
            if (!powerSwitch.IsClosed) await VerifyPress(true);
            await VerifyPress(false);
            await VerifyPress(true);
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

        internal static async Task InstallBridgeResistors(Transform layout, Action<string> log)
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
