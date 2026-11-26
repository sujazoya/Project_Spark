using System;
using System.Collections;
using System.Collections.Generic;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Executes ONLY explicitly approved operations through the real SparkCircuitSystem.
    /// Visualizes wires in the world with a LineRenderer; all scoring stays with the game.
    /// This is a practical action engine, NOT a universal autonomous level solver.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLabAutonomyAgent : MonoBehaviour
    {
        [Header("Game binding")]
        [SerializeField] private SparkProExecutive executive;
        [SerializeField] private SparkProCharacter character;
        [Header("Permissions")]
        [SerializeField] private bool allowConfirmedManualWires = true;
        [SerializeField] private bool allowReviewedPracticePlanExecution = true;
        [SerializeField] private bool requirePowerOffForEdits = true;
        [Header("Visual wire")]
        [SerializeField] private bool showAgentWireOverlay = true;
        [SerializeField, Range(0.004f, 0.04f)] private float wireWidth = 0.013f;
        [SerializeField] private Material wireMaterial;

        private SparkTerminal previewA, previewB;
        private string previewFromId, previewToId;
        private GameObject previewObject;
        private Material ownedLineMaterial;
        private LineRenderer previewLine;
        private LevelGamePlayManager previewManager;
        private Transform previewLayout;
        private Coroutine execution;
        private SparkLabActionPlan runningPlan;
        private int runningIndex;
        private readonly List<CreatedWire> ownWires = new List<CreatedWire>(16);

        private sealed class CreatedWire
        {
            public SparkCircuitConnection Connection;
            public LevelGamePlayManager Manager;
            public SparkTerminal A, B;
            public GameObject Visual;
            public LineRenderer Line;
        }

        public string LastStatus { get; private set; } = "No action requested.";
        public event Action<string> StatusChanged;
        public bool IsExecuting => execution != null;

        private void Awake()
        {
            if (!executive) executive = GetComponent<SparkProExecutive>();
            if (!character) character = GetComponent<SparkProCharacter>() ?? GetComponentInParent<SparkProCharacter>();
        }

        private void OnEnable() { SceneManager.activeSceneChanged += OnSceneChanged; }
        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
            StopPlan();
            ClearPreview();
        }

        private void OnDestroy()
        {
            if (ownedLineMaterial) Destroy(ownedLineMaterial);
            ownedLineMaterial = null;
        }

        private void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            StopPlan();
            ClearPreview();
            ownWires.Clear();
            Report("Level changed. Pending agent actions were cancelled.");
        }

        public static bool TryHandleFrom(SparkProExecutive source, string question, out string answer)
        {
            answer = null;
            SparkLabActionCommand.Request request = SparkLabActionCommand.Parse(question);
            if (request.Type == SparkLabActionCommand.Kind.None) return false;
            if (!source) { answer = "Spark's executive is unavailable."; return true; }
            SparkLabAutonomyAgent agent = source.GetComponent<SparkLabAutonomyAgent>();
            answer = agent ? agent.Handle(request) :
                "Spark Action Agent is not bound. Run AAA Laboratory > 9 - Bind Action Agent outside Play Mode.";
            return true;
        }

        public string Handle(SparkLabActionCommand.Request request)
        {
            if (!executive) executive = GetComponent<SparkProExecutive>();
            if (GetComponent<SparkEngineeringControlHub>() && request.Type is
                SparkLabActionCommand.Kind.PreviewWire or SparkLabActionCommand.Kind.ConfirmWire or
                SparkLabActionCommand.Kind.RunApprovedPlan or SparkLabActionCommand.Kind.UndoLastWire)
                return "Use the Engineering Control Hub for approved native wiring.";
            if (request.Type == SparkLabActionCommand.Kind.Help)
                return "Action commands: 'draw wire from TerminalID to TerminalID' (preview only), " +
                    "'confirm wire', 'cancel wire', 'undo last AI wire', 'preview level solution', " +
                    "'auto solve practice level', 'stop auto solve', and 'agent status'. " +
                    "Each connection is checked; exam and certification bypasses are disabled.";
            if (request.Type == SparkLabActionCommand.Kind.Status) return LastStatus +
                (execution != null && runningPlan ? " Plan step " + (runningIndex + 1) +
                 "/" + runningPlan.steps.Count + "." : "");
            if (request.Type == SparkLabActionCommand.Kind.StopPlan)
            {
                StopPlan(); ClearPreview();
                return Report("AI action plan stopped. Already-created wires remain until explicitly undone.");
            }
            if (request.Type == SparkLabActionCommand.Kind.CancelWire)
            {
                ClearPreview(); return Report("Pending wire preview cancelled. No connection was created.");
            }
            if (request.Type == SparkLabActionCommand.Kind.UndoLastWire) return UndoLastWire();
            if (!TryGetLevel(out LevelGamePlayManager manager, out SparkAILevelContext level, out string failure))
                return Report(failure);
            if (executive.ExamMode) return Report("Independent examination is active. AI wiring and auto-solving are not permitted.");
            if (character && character.WarningActive)
                return Report("A safety warning is active. AI laboratory actions are temporarily unavailable.");
            if (execution != null) return Report("An approved practice plan is already running. Say 'agent status' or 'stop auto solve'.");

            switch (request.Type)
            {
                case SparkLabActionCommand.Kind.ShowNextObjective:
                    return ShowNextObjective();
                case SparkLabActionCommand.Kind.PreviewWire:
                    return PreviewWire(manager, level, request.From, request.To);
                case SparkLabActionCommand.Kind.ConfirmWire:
                    return ConfirmWire(manager, level);
                case SparkLabActionCommand.Kind.PreviewPlan:
                    return DescribePlan(level);
                case SparkLabActionCommand.Kind.RunApprovedPlan:
                    return RunPlan(manager, level);
                default:
                    return Report("Command not supported.");
            }
        }

        private bool TryGetLevel(out LevelGamePlayManager manager, out SparkAILevelContext level, out string error)
        {
            manager = executive ? executive.ActiveLevelManager : null;
            level = manager ? SparkAILevelContext.Capture(manager) : default;
            error = "";
            if (!manager || !level.Definition || !level.Layout || !level.Layout.gameObject.activeInHierarchy)
            {
                error = "Open a playable level with an active authored laboratory layout.";
                return false;
            }
            if (!manager.CircuitSystem || !level.Layout.SceneBindings)
            {
                error = "The current mission's circuit system or scene terminal bindings are missing.";
                return false;
            }
            return true;
        }

        private bool CanEdit(LevelGamePlayManager manager, SparkAILevelContext level, out string reason)
        {
            reason = "";
            if (!manager || !level.Layout || !manager.CircuitSystem)
            { reason = "No writable circuit system is available."; return false; }
            if (executive && executive.ExamMode)
            { reason = "Independent exam mode forbids autonomous actions."; return false; }
            if (character && character.WarningActive)
            { reason = "Safety warning active."; return false; }
            if (!requirePowerOffForEdits) return true;
            SparkPowerSupply[] supplies = level.Layout.GetComponentsInChildren<SparkPowerSupply>(true);
            for (int i = 0; i < supplies.Length; i++)
                if (supplies[i] && supplies[i].IsOutputActive)
                { reason = "Power supply is active. Switch off the VIRTUAL supply before allowing AI rewiring."; return false; }
            return true;
        }

        private bool ResolvePair(SparkAILevelContext level, string idA, string idB,
            out SparkTerminal a, out SparkTerminal b, out string reason)
        {
            a = b = null;
            reason = "";
            if (string.IsNullOrWhiteSpace(idA) || string.IsNullOrWhiteSpace(idB) ||
                !level.Layout.SceneBindings.TryGetTerminal(idA, out a) ||
                !level.Layout.SceneBindings.TryGetTerminal(idB, out b) || !a || !b)
            { reason = "Both exact, authored terminal IDs must exist in this level's SceneBindings."; return false; }
            if (a == b || !a.gameObject.activeInHierarchy || !b.gameObject.activeInHierarchy ||
                !a.transform.IsChildOf(level.Layout.transform) || !b.transform.IsChildOf(level.Layout.transform))
            { reason = "Terminal endpoints are identical, inactive, or outside the current workbench."; return false; }
            // A component's own A/B terminals describe the component, not a cable between them.
            if (a.Owner && b.Owner && a.Owner == b.Owner)
            { reason = "Refusing a direct wire across one component's own terminals; that could bypass/short the part."; return false; }
            return true;
        }

        private string ShowNextObjective()
        {
            SparkLabTeachingDirector inspector = GetComponent<SparkLabTeachingDirector>();
            if (!inspector) return Report("3D laboratory inspector is not attached to Spark.");
            if (inspector.TryShowCurrent(SparkLabFocusIntent.Kind.NextConnection))
                return Report("Displayed the next authored objective's actual terminals in 3D. " +
                    "These are inspection endpoints, NOT a command to short them together. " + inspector.LastInspection);
            return Report("3D target could not be verified: " + inspector.LastInspection);
        }

        private string PreviewWire(LevelGamePlayManager manager, SparkAILevelContext level, string from, string to)
        {
            if (!allowConfirmedManualWires) return Report("Manual AI wiring is disabled in this laboratory.");
            if (!ResolvePair(level, from, to, out SparkTerminal a, out SparkTerminal b, out string reason))
                return Report("Wire preview refused: " + reason);
            if (manager.CircuitSystem.HasConnectionBetween(a, b))
                return Report("Those terminals already have a circuit connection; no duplicate wire was drawn.");
            ClearPreview();
            previewA = a; previewB = b; previewFromId = from; previewToId = to;
            previewManager = manager; previewLayout = level.Layout.transform;
            previewLine = CreateLine("Spark_AI_WIRE_PREVIEW_ONLY", level.Layout.transform, true, out previewObject);
            UpdateLine(previewLine, a.transform.position, b.transform.position);
            if (character) character.PointAt(a.transform);
            return Report("Previewing a non-electrical wire from " + a.name + " to " + b.name +
                ". This is NOT yet connected. Check the authored schematic, isolate virtual power, then say 'confirm wire'.");
        }

        private string ConfirmWire(LevelGamePlayManager manager, SparkAILevelContext level)
        {
            if (!allowConfirmedManualWires) return Report("Manual AI wiring is disabled in this laboratory.");
            if (!previewA || !previewB || previewManager != manager ||
                previewLayout != level.Layout.transform)
            { ClearPreview(); return Report("No valid wire preview for this level. Preview a pair again."); }
            if (!CanEdit(manager, level, out string reason)) return Report(reason);
            SparkTerminal a = previewA, b = previewB;
            if (!ResolvePair(level, previewFromId, previewToId,
                out SparkTerminal resolvedA, out SparkTerminal resolvedB, out reason) ||
                resolvedA != a || resolvedB != b)
            { ClearPreview(); return Report("The terminal binding has changed: " + reason); }
            if (!TryMakeRealWire(manager, level, a, b, out reason)) return Report(reason);
            ClearPreview();
            return Report("Actual SparkCircuitSystem connection created between " + a.name + " and " + b.name +
                ". Use the game evaluator and new solver result to verify the objective. 'Undo last AI wire' reverses this action.");
        }

        private bool TryMakeRealWire(LevelGamePlayManager manager, SparkAILevelContext level,
            SparkTerminal a, SparkTerminal b, out string message)
        {
            message = "";
            if (!CanEdit(manager, level, out message)) return false;
            if (manager.CircuitSystem.HasConnectionBetween(a, b))
            { message = "Connection already exists; no duplicate was added."; return false; }
            if (!manager.CircuitSystem.TryCreateConnection(a, b, SparkConnectionKind.Wire,
                    SparkConnectionDirection.Bidirectional, out SparkCircuitConnection connection))
            { message = "CircuitSystem rejected this connection. No wire was created."; return false; }
            var created = new CreatedWire { Connection = connection, Manager = manager, A = a, B = b };
            if (showAgentWireOverlay)
            {
                created.Line = CreateLine("Spark_AI_WIRE_ACTUAL_CONNECTION", level.Layout.transform,
                    false, out created.Visual);
                UpdateLine(created.Line, a.transform.position, b.transform.position);
            }
            ownWires.Add(created);
            if (character) character.PointAt(b.transform);
            return true;
        }

        private string UndoLastWire()
        {
            if (execution != null) return Report("Stop auto solve before undoing an AI wire.");
            if (ownWires.Count == 0) return Report("There are no AI-created wires to undo.");
            int last = ownWires.Count - 1;
            CreatedWire wire = ownWires[last];
            if (!wire.Manager || !wire.Manager.CircuitSystem)
            { ownWires.RemoveAt(last); return Report("Original level is gone; the AI wire record has expired."); }
            if (executive && executive.ExamMode) return Report("Cannot modify circuits during independent examinations.");
            SparkAILevelContext wireLevel = SparkAILevelContext.Capture(wire.Manager);
            if (!CanEdit(wire.Manager, wireLevel, out string editReason))
                return Report("Cannot undo while live wiring is unsafe: " + editReason);
            if (!wire.Manager.CircuitSystem.TryRemoveConnection(wire.Connection.Id, out _))
                return Report("Circuit system refused to remove that AI wire. No other wiring was touched.");
            if (wire.Visual) Destroy(wire.Visual);
            ownWires.RemoveAt(last);
            return Report("Undid the last AI-created wire and removed its overlay.");
        }

        private SparkLabActionPlan FindPlan(SparkAILevelContext level)
        {
            SparkLabActionPlan[] plans = Resources.LoadAll<SparkLabActionPlan>("SparkAgentPlans");
            SparkLabActionPlan match = null;
            for (int i = 0; i < plans.Length; i++)
            {
                if (!plans[i] || !plans[i].IsApproved || plans[i].levelNumber != level.Definition.LevelNumber) continue;
                if (match) return null; // Ambiguous plans fail closed.
                match = plans[i];
            }
            return match;
        }

        private string DescribePlan(SparkAILevelContext level)
        {
            SparkLabActionPlan plan = FindPlan(level);
            if (!plan) return Report("No unique, reviewed assisted-practice wiring plan exists for Level " +
                level.Definition.LevelNumber + ". Auto-solving is disabled until the actual schematic is reviewed.");
            var summary = new System.Text.StringBuilder();
            summary.Append("Approved practice plan for Level ").Append(plan.levelNumber)
                .Append("; reviewer: ").Append(plan.Reviewer).Append("; ")
                .Append(plan.steps.Count).Append(" actions. ");
            for (int i = 0; i < plan.steps.Count && i < 10; i++)
            {
                var step = plan.steps[i];
                summary.Append(i + 1).Append(": ").Append(step.operation).Append(" ")
                    .Append(step.operation == SparkLabActionPlan.Operation.ConnectWire
                        ? step.fromTerminalId + " -> " + step.toTerminalId : step.componentId).Append("; ");
            }
            return Report(summary.ToString());
        }

        private bool PreflightPlan(SparkLabActionPlan plan, SparkAILevelContext level, out string reason)
        {
            reason = "";
            if (!plan.IsApproved) { reason = "Plan approval expired after source edits."; return false; }
            for (int i = 0; i < plan.steps.Count; i++)
            {
                var step = plan.steps[i];
                if (step == null)
                { reason = "Step " + (i + 1) + " is empty."; return false; }
                if (step.operation == SparkLabActionPlan.Operation.ConnectWire)
                {
                    if (!ResolvePair(level, step.fromTerminalId, step.toTerminalId,
                        out SparkTerminal a, out SparkTerminal b, out reason))
                    { reason = "Step " + (i + 1) + ": " + reason; return false; }
                }
                else if (step.operation == SparkLabActionPlan.Operation.CloseSwitch ||
                         step.operation == SparkLabActionPlan.Operation.OpenSwitch)
                {
                    if (string.IsNullOrWhiteSpace(step.componentId) ||
                        !level.Layout.SceneBindings.TryGetComponent(step.componentId, out var component) ||
                        !(component is SparkSwitch) || !component.transform.IsChildOf(level.Layout.transform))
                    { reason = "Step " + (i + 1) + ": switch binding invalid."; return false; }
                }
                else { reason = "Step " + (i + 1) + " is an unsupported operation."; return false; }
            }
            return true;
        }

        private string RunPlan(LevelGamePlayManager manager, SparkAILevelContext level)
        {
            if (!allowReviewedPracticePlanExecution) return Report("Automatic assisted practice is disabled.");
            SparkLabActionPlan plan = FindPlan(level);
            if (!plan) return DescribePlan(level);
            if (!CanEdit(manager, level, out string reason)) return Report(reason);
            if (manager.IsCompleted) return Report("The mission evaluator already reports completion. No action was taken.");
            if (plan.steps.Count == 0) return Report("The approved plan has no actions.");
            if (!PreflightPlan(plan, level, out reason))
                return Report("Plan preflight refused: " + reason);
            ClearPreview();
            runningPlan = plan; runningIndex = 0;
            execution = StartCoroutine(ExecutePlan(plan, manager, level.Layout.transform));
            return Report("Started reviewed assisted-practice plan: " + plan.steps.Count +
                " actions. Spark will use real circuit APIs, check each result and stop on a fault. " +
                "The evaluator—not Spark—decides whether the level is complete.");
        }

        private IEnumerator ExecutePlan(SparkLabActionPlan plan, LevelGamePlayManager manager, Transform layoutRoot)
        {
            for (int i = 0; i < plan.steps.Count; i++)
            {
                runningIndex = i;
                if (!TryGetLevel(out LevelGamePlayManager live, out SparkAILevelContext level,
                    out string reason) || live != manager || level.Layout.transform != layoutRoot ||
                    !CanEdit(manager, level, out reason))
                { Report("Auto-plan stopped: " + reason); execution = null; runningPlan = null; yield break; }
                var step = plan.steps[i];
                if (step == null)
                { Report("Auto-plan stopped at empty step " + (i + 1)); execution = null; runningPlan = null; yield break; }
                if (!plan.IsApproved)
                { Report("Plan source changed during execution; stopped without further actions.");
                  execution = null; runningPlan = null; yield break; }
                bool performed = false;
                bool addedWire = false;
                SparkSwitch changedSwitch = null;
                bool oldSwitchOpen = false;
                if (step.operation == SparkLabActionPlan.Operation.ConnectWire)
                {
                    if (!ResolvePair(level, step.fromTerminalId, step.toTerminalId,
                        out SparkTerminal a, out SparkTerminal b, out reason))
                    { Report("Step " + (i+1) + " invalid: " + reason); execution = null; runningPlan = null; yield break; }
                    if (!manager.CircuitSystem.HasConnectionBetween(a, b))
                    {
                        if (!TryMakeRealWire(manager, level, a, b, out reason))
                        { Report("Step " + (i+1) + " failed: " + reason); execution = null; runningPlan = null; yield break; }
                        performed = true;
                        addedWire = true;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(step.componentId) ||
                        !level.Layout.SceneBindings.TryGetComponent(step.componentId, out var component) ||
                        !(component is SparkSwitch control) || !component.transform.IsChildOf(layoutRoot))
                    { Report("Plan switch step " + (i + 1) + " references no valid active switch."); execution = null; runningPlan = null; yield break; }
                    oldSwitchOpen = control.IsOpen;
                    changedSwitch = control;
                    control.SetState(step.operation == SparkLabActionPlan.Operation.CloseSwitch
                        ? SparkSwitchState.Closed : SparkSwitchState.Open);
                    performed = true;
                }
                if (performed) Report("Executed practice plan step " + (i + 1) + "/" + plan.steps.Count + ".");
                // Let the authoritative electrical/evaluator updates run before the next operation.
                float expiry = Time.unscaledTime + 3f;
                while (Time.unscaledTime < expiry && manager && manager.ElectricalSolver &&
                    (manager.ElectricalSolver.IsDirty || manager.ElectricalSolver.IsSolving))
                    yield return null;
                yield return new WaitForSecondsRealtime(0.15f);
                if (!manager || !manager.CircuitSystem)
                { Report("Plan stopped: circuit was unloaded."); execution = null; runningPlan = null; yield break; }
                if (!manager.ElectricalSolver || manager.ElectricalSolver.IsDirty ||
                    manager.ElectricalSolver.IsSolving ||
                    manager.ElectricalSolver.LatestSolveResult == null ||
                    !manager.HasCurrentElectricalEvaluation)
                {
                    Report("Plan stopped at step " + (i + 1) +
                        ": no fresh solver AND evaluator evidence. No further actions were taken.");
                    execution = null; runningPlan = null; yield break;
                }
                if (manager.HasCurrentElectricalEvaluation &&
                    (manager.SourceShorted || manager.TargetShorted || manager.Overloaded))
                {
                    bool reverted = false;
                    if (addedWire && ownWires.Count > 0)
                    {
                        int last = ownWires.Count - 1;
                        var own = ownWires[last];
                        if (own.Manager == manager && manager.CircuitSystem.TryRemoveConnection(own.Connection.Id, out _))
                        {
                            if (own.Visual) Destroy(own.Visual);
                            ownWires.RemoveAt(last);
                            reverted = true;
                        }
                    }
                    if (changedSwitch)
                    {
                        changedSwitch.SetState(oldSwitchOpen ? SparkSwitchState.Open : SparkSwitchState.Closed);
                        reverted = true;
                    }
                    Report("Plan STOPPED: evaluator reports a short or overload after step " +
                        (i + 1) + (reverted ? ". Spark reverted the last action." :
                        ". Action could not be safely reverted; inspect the circuit before proceeding."));
                    execution = null; runningPlan = null; yield break;
                }
            }
            bool completed = manager && manager.HasCurrentElectricalEvaluation && manager.IsCompleted;
            Report(completed
                ? "The game evaluator reports this assisted practice level completed. No exam score or certificate was granted by Spark."
                : "Approved actions finished, but the evaluator has NOT confirmed level completion. " +
                  "Check the remaining objectives; Spark will not invent a pass.");
            execution = null; runningPlan = null;
        }

        private void StopPlan()
        {
            if (execution != null) StopCoroutine(execution);
            execution = null; runningPlan = null; runningIndex = 0;
        }

        private void ClearPreview()
        {
            previewA = previewB = null;
            previewFromId = previewToId = null;
            previewManager = null; previewLayout = null;
            if (previewObject) Destroy(previewObject);
            previewObject = null; previewLine = null;
        }

        private LineRenderer CreateLine(string label, Transform parent, bool preview, out GameObject node)
        {
            node = new GameObject(label);
            node.transform.SetParent(parent, false);
            node.layer = 2; // IgnoreRaycast: does not intercept component selection.
            LineRenderer line = node.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 20;
            line.startWidth = line.endWidth = wireWidth;
            line.numCapVertices = 6;
            line.numCornerVertices = 5;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            if (wireMaterial) line.sharedMaterial = wireMaterial;
            else
            {
                Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
                if (shader)
                {
                    if (!ownedLineMaterial) ownedLineMaterial = new Material(shader) { name = "Spark Agent Line Runtime" };
                    line.sharedMaterial = ownedLineMaterial;
                }
            }
            Color tint = preview ? new Color(1f, 0.72f, 0.22f, 0.78f) :
                new Color(0.14f, 0.85f, 0.97f, 0.92f);
            line.startColor = tint; line.endColor = tint;
            return line;
        }

        private static void UpdateLine(LineRenderer line, Vector3 a, Vector3 b)
        {
            if (!line) return;
            for (int i = 0; i < line.positionCount; i++)
            {
                float t = (float)i / (line.positionCount - 1);
                Vector3 pos = Vector3.Lerp(a, b, t);
                pos.y -= 0.12f * Mathf.Sin(t * Mathf.PI);
                line.SetPosition(i, pos);
            }
        }

        private void LateUpdate()
        {
            if (previewLine && previewA && previewB)
                UpdateLine(previewLine, previewA.transform.position, previewB.transform.position);
            for (int i = 0; i < ownWires.Count; i++)
            {
                CreatedWire wire = ownWires[i];
                if (wire.Line && wire.A && wire.B)
                    UpdateLine(wire.Line, wire.A.transform.position, wire.B.transform.position);
            }
        }

        private string Report(string message)
        {
            LastStatus = message;
            Debug.Log("[SPARK Agent V8] " + message, this);
            StatusChanged?.Invoke(message);
            return message;
        }
    }
}
