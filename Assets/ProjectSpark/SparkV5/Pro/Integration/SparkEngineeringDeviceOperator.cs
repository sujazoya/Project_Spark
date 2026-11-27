using System;
using System.Collections;
using System.Text;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Assisted, one-operation-at-a-time bridge to genuine native device controls.
    /// Does not auto-solve levels, bypass exams, create wiring or write solver/evaluator state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkEngineeringDeviceOperator : MonoBehaviour
    {
        [SerializeField, Range(0.2f, 8f)] private float stateConfirmationTimeout = 3f;
        [SerializeField] private bool requirePowerOffForNonPowerActions = true;
        private SparkProExecutive executive;
        private SparkEngineeringControlHub hub;
        private SparkNativeWireDirector director;
        private SparkNativeWireCinematic performer;
        private SparkNativeWireAutoEngineer legacyAuto;
        private SparkEngineeringDevicePort proposed;
        private bool proposedOn;
        private LevelGamePlayManager proposedLevel;
        private Transform proposedRoot;
        private Coroutine operation;
        private LevelGamePlayManager operationLevel;
        private Transform operationRoot;
        private string lastStatus = "Ready. Inspect devices; Assisted mode requires explicit approval.";
        public event Action StatusChanged;
        public string LastStatus => lastStatus;
        public bool IsBusy => operation != null;
        internal int OperationRevision { get; private set; }
        internal int VerifiedOperationRevision { get; private set; }
        public string Proposal => proposed ? proposed.DeviceId + " -> " + (proposedOn ? "ON/CLOSED" : "OFF/OPEN") : "None";

        private void Awake()
        {
            executive = GetComponent<SparkProExecutive>();
            hub = GetComponent<SparkEngineeringControlHub>();
            director = GetComponent<SparkNativeWireDirector>();
            performer = GetComponent<SparkNativeWireCinematic>();
            legacyAuto = GetComponent<SparkNativeWireAutoEngineer>();
        }
        private void OnEnable() { SceneManager.activeSceneChanged += SceneChanged; }
        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= SceneChanged;
            Invalidate("Device operator disabled; any dispatched native action must be checked manually.");
        }
        private void SceneChanged(Scene a, Scene b)
        { Invalidate("Scene changed; proposal cancelled. No further device action will be dispatched."); }
        private void Invalidate(string why)
        {
            if (operation != null)
            {
                StopCoroutine(operation);
                if (performer && performer.IsPerforming) performer.CancelPerformance();
            }
            operation = null;
            operationLevel = null;
            operationRoot = null;
            proposed = null;
            proposedLevel = null;
            proposedRoot = null;
            Publish(why);
        }
        private string Publish(string message)
        {
            lastStatus = message;
            Debug.Log("[Spark Device Operator v16] " + message, this);
            StatusChanged?.Invoke();
            return message;
        }
        private bool Context(out LevelGamePlayManager manager, out SparkAILevelContext context, out string error)
        {
            manager = executive ? executive.ActiveLevelManager : null;
            context = manager ? SparkAILevelContext.Capture(manager) : default;
            error = null;
            if (!executive || !hub || !manager || !context.Definition || !context.Layout ||
                !context.Layout.gameObject.activeInHierarchy || !context.Layout.SceneBindings || !manager.CircuitSystem)
                error = "Active authored level, control hub, bindings or electrical system is missing.";
            else if (executive.ExamMode)
                error = "Independent exam mode forbids Spark device operations.";
            else if (manager.IsFailed)
                error = "Official evaluator reports level failure.";
            else if (hub.Mode != SparkEngineeringControlHub.OperatingMode.Assisted)
                error = "Use Assisted mode and explicitly approve one device action. Manual mode is player-only; Auto mode follows reviewed plans.";
            else if (hub.IsExecuting || (director && director.IsBusy) || (legacyAuto && legacyAuto.IsExecuting))
                error = "Native wiring is busy; do not change device state during a cable operation.";
            return error == null;
        }
        private SparkEngineeringDevicePort FindPort(SparkAILevelContext context, string id, out string problem)
        {
            problem = null;
            SparkEngineeringDevicePort match = null;
            int n = 0;
            var ports = context.Layout.GetComponentsInChildren<SparkEngineeringDevicePort>(true);
            for (int i = 0; i < ports.Length; i++)
            {
                var port = ports[i];
                if (!port || !string.Equals(port.DeviceId, id, StringComparison.OrdinalIgnoreCase)) continue;
                if (!port.gameObject.activeInHierarchy) continue;
                n++; match = port;
            }
            if (n != 1)
            { problem = n == 0 ? "No active device port with ID '" + id + "' in this level." : "Ambiguous duplicate device ID '" + id + "'."; return null; }
            return match;
        }
        private bool AllVirtualPowerOff(SparkAILevelContext context)
        {
            var supplies = context.Layout.GetComponentsInChildren<SparkPowerSupply>(true);
            for (int i = 0; i < supplies.Length; i++)
                if (supplies[i] && supplies[i].IsOutputActive) return false;
            return true;
        }
        private bool WiringReadyForPowerOn(LevelGamePlayManager manager, SparkAILevelContext context, out string reason)
        {
            reason = null;
            var plans = Resources.LoadAll<SparkNativeWiringPlan>("SparkNativeWirePlans");
            SparkNativeWiringPlan selected = null;
            int matches = 0;
            string layoutName = context.Layout.name.Replace("(Clone)", "").Trim();
            for (int i = 0; i < plans.Length; i++)
            {
                var plan = plans[i];
                if (!plan || !plan.IsApproved || plan.LevelNumber != context.Definition.LevelNumber ||
                    !string.Equals(plan.LayoutName, layoutName, StringComparison.Ordinal)) continue;
                matches++; selected = plan;
            }
            if (matches != 1 || !selected.Validate(out reason))
            { reason = "Cannot energize without one valid, uniquely approved wiring plan for the active level."; return false; }
            if (selected.Wires.Count == 0)
            { reason = "Approved plan contains no reviewed electrical connections; cannot authorize power ON."; return false; }
            for (int i = 0; i < selected.Wires.Count; i++)
            {
                var wire = selected.Wires[i];
                SparkTerminal a, b;
                if (wire == null || !context.Layout.SceneBindings.TryGetTerminal(wire.fromId, out a) ||
                    !context.Layout.SceneBindings.TryGetTerminal(wire.toId, out b) || !a || !b ||
                    !a.gameObject.activeInHierarchy || !b.gameObject.activeInHierarchy ||
                    !a.transform.IsChildOf(context.Layout.transform) || !b.transform.IsChildOf(context.Layout.transform) ||
                    !manager.CircuitSystem.HasConnectionBetween(a, b))
                { reason = "Reviewed connection " + (i + 1) + " is missing or invalid. Power ON refused."; return false; }
            }
            return true;
        }
        private bool Preflight(SparkEngineeringDevicePort port, bool desired,
            LevelGamePlayManager manager, SparkAILevelContext context, out string why)
        {
            why = null;
            if (!port || !port.gameObject.activeInHierarchy || !port.transform.IsChildOf(context.Layout.transform))
            { why = "Device is inactive or not part of the active level."; return false; }
            if (!port.TryRead(out _, out why)) return false;
            if (!port.CanOperate(desired, this, out why)) return false;
            if (port.Kind == SparkEngineeringDevicePort.DeviceType.VirtualPowerSupply && desired)
            {
                if (!WiringReadyForPowerOn(manager, context, out why)) return false;
                if (!manager.HasCurrentElectricalEvaluation)
                { why = "Await a fresh electrical evaluation before energizing."; return false; }
                var graph = manager.ElectricalSolver.CurrentGraph;
                var supply = port.Supply;
                if (graph == null || !graph.TryGetNode(supply.PositiveTerminal, out int positive) ||
                    !graph.TryGetNode(supply.NegativeTerminal, out int negative) || positive == negative ||
                    manager.SourceShorted || manager.TargetShorted || manager.Overloaded)
                { why = "The authoritative solver does not verify separate, safe supply terminals."; return false; }
            }
            if (requirePowerOffForNonPowerActions &&
                port.Kind != SparkEngineeringDevicePort.DeviceType.VirtualPowerSupply &&
                !AllVirtualPowerOff(context))
            { why = "Virtual power must be off before operating this device."; return false; }
            return true;
        }
        public string Inspect()
        {
            if (!executive || !executive.ActiveLevelManager)
                return Publish("No active level to inspect.");
            var context = SparkAILevelContext.Capture(executive.ActiveLevelManager);
            if (!context.Layout) return Publish("No active authored layout.");
            var ports = context.Layout.GetComponentsInChildren<SparkEngineeringDevicePort>(true);
            var sb = new StringBuilder("PHYSICAL DEVICE PORTS (read-only; no device operation)\n");
            if (ports.Length == 0) sb.AppendLine("No device ports bound. In Unity select a device and add/bind a v16 port.");
            for (int i = 0; i < ports.Length; i++)
            {
                var p = ports[i];
                if (!p) continue;
                bool read = p.TryRead(out bool isOn, out string evidence);
                sb.Append(p.DeviceId).Append(" [").Append(p.Kind).Append("] ")
                  .Append(read ? (isOn ? "ON/CLOSED " : "OFF/OPEN ") : "UNVERIFIED ")
                  .Append(evidence).Append(" | native on=").Append(p.HasNativeAction(true))
                  .Append(" off=").Append(p.HasNativeAction(false))
                  .Append(" | physical control=").Append(p.PhysicalControl ? p.PhysicalControl.name : "unbound").AppendLine();
            }
            return Publish(sb.ToString());
        }
        public string Propose(string id, bool turnOn)
        {
            if (operation != null) return Publish("Another native device action is still awaiting verification.");
            proposed = null;
            proposedLevel = null;
            proposedRoot = null;
            if (!Context(out var manager, out var context, out string why)) return Publish(why);
            var port = FindPort(context, id, out why);
            if (!port) return Publish(why);
            if (!Preflight(port, turnOn, manager, context, out why)) return Publish("Proposal rejected: " + why);
            proposed = port;
            proposedOn = turnOn;
            proposedLevel = manager;
            proposedRoot = context.Layout.transform;
            return Publish("REVIEW ONLY: " + Proposal + ". No operation executed. Say 'spark approve device action' after verifying the actual hardware and schematic.");
        }
        public string Approve()
        {
            if (operation != null) return Publish("Device action is already running.");
            if (!proposed || !proposedRoot || !proposedLevel) return Publish("No pending approved device proposal.");
            if (!Context(out var manager, out var context, out string why)) return Publish(why);
            if (manager != proposedLevel || context.Layout.transform != proposedRoot)
            { Invalidate("Level changed; device proposal cancelled."); return lastStatus; }
            var port = FindPort(context, proposed.DeviceId, out why);
            if (port != proposed || !Preflight(port, proposedOn, manager, context, out why))
            { Invalidate("Device authorization expired: " + why); return lastStatus; }
            operationLevel = manager;
            operationRoot = context.Layout.transform;
            bool desired = proposedOn;
            OperationRevision++;
            operation = StartCoroutine(Execute(port, desired));
            return Publish("Executing ONE approved native device action. Awaiting actual device-state confirmation.");
        }
        private IEnumerator Execute(SparkEngineeringDevicePort port, bool desired)
        {
            // Deliberately start next frame so operation handle exists before any failure/completion.
            yield return null;
            if (!Context(out var manager, out var context, out string problem) ||
                manager != operationLevel || context.Layout.transform != operationRoot ||
                !Preflight(port, desired, manager, context, out problem))
            { Finish("CANCELLED before dispatch: " + problem); yield break; }
            if (!port.TryRead(out bool current, out problem))
            { Finish("Unable to verify existing device state: " + problem); yield break; }
            if (current == desired)
            { Finish("Already in requested state; no native command was dispatched. " + problem); yield break; }
            var solver = manager.ElectricalSolver;
            if (!solver) { Finish("The authoritative electrical solver is unavailable."); yield break; }
            int previousSolve = solver.CompletedSolveRevision;
            bool dispatched = false;
            string dispatch = null;
            bool StillAuthorized()
            {
                return Context(out var currentManager, out var currentLevel, out dispatch) &&
                    currentManager == operationLevel && currentLevel.Layout.transform == operationRoot &&
                    Preflight(port, desired, currentManager, currentLevel, out dispatch);
            }
            void Dispatch() => dispatched = StillAuthorized() && port.TryInvokeNative(desired, this, out dispatch);
            var control = port.PhysicalControl;
            if (control)
            {
                if (!performer || !director)
                { Finish("The device's physical approach is unavailable."); yield break; }
                yield return performer.OperateDevice(port.PhysicalLever, director.Workbench, operationRoot, StillAuthorized, Dispatch);
            }
            else Dispatch();
            if (!dispatched)
            { Finish("Native operation was rejected before confirmation. " + (dispatch ?? (control ? performer.LastError : null))); yield break; }
            float deadline = Time.realtimeSinceStartup + stateConfirmationTimeout;
            while (Time.realtimeSinceStartup < deadline)
            {
                if (port && port.Supply && desired && manager && manager.LayoutLoader &&
                    manager.LayoutLoader.ActiveLayout && manager.LayoutLoader.ActiveLayout.transform == operationRoot &&
                    manager.HasCurrentElectricalEvaluation && manager.LastEvaluatedSolveRevision > previousSolve &&
                    (manager.SourceShorted || manager.TargetShorted || manager.Overloaded))
                {
                    bool stopped = port.TryInvokeNative(false, this, out _) && !port.Supply.IsOutputActive;
                    Finish(stopped ? "Safety stopped the virtual supply after an electrical fault." :
                        "Electrical fault detected. Turn off the virtual supply with the player control.");
                    yield break;
                }
                if (!Context(out manager, out context, out problem) ||
                    manager != operationLevel || context.Layout.transform != operationRoot || !port)
                {
                    Finish("UNCERTAIN: Context changed after native action was dispatched. Inspect real device state manually.");
                    yield break;
                }
                bool freshSolve = solver && !solver.IsDirty && !solver.IsSolving &&
                    solver.LatestSolveResult != null && solver.CompletedSolveRevision > previousSolve;
                bool needsEvaluator = port.Supply && desired;
                if (port.TryRead(out current, out problem) && current == desired && freshSolve &&
                    (!needsEvaluator || manager.HasCurrentElectricalEvaluation))
                {
                    VerifiedOperationRevision = OperationRevision;
                    Finish("VERIFIED: Real device state changed to " + (desired ? "ON/CLOSED" : "OFF/OPEN") +
                        " (" + problem + "). Fresh native solver result confirmed." +
                        (control ? "" : " No physical control point is authored for this device."));
                    yield break;
                }
                yield return new WaitForSecondsRealtime(0.08f);
            }
            if (port && port.Supply && desired && port.Supply.IsOutputActive)
                port.TryInvokeNative(false, this, out _);
            Finish("UNVERIFIED after native action. Inspect the device and solver state.");
        }
        private void Finish(string outcome)
        {
            operation = null;
            operationLevel = null;
            operationRoot = null;
            proposed = null;
            proposedLevel = null;
            proposedRoot = null;
            Publish(outcome);
            if (executive) executive.PublishWirePerformanceNarration(outcome);
        }
        public string Cancel()
        {
            Invalidate("Proposal cancelled. If a native callback was already dispatched, verify the physical device state; no rollback is assumed.");
            return lastStatus;
        }
        public static bool TryHandleFrom(SparkProExecutive caller, string normalized, out string result)
        {
            result = null;
            if (!caller || string.IsNullOrWhiteSpace(normalized)) return false;
            string q = normalized.Trim().ToLowerInvariant();
            bool recognized = q == "spark inspect devices" || q == "spark device status" ||
                              q == "spark approve device action" || q == "spark cancel device action" ||
                              q.StartsWith("spark propose device ", StringComparison.Ordinal);
            if (!recognized) return false;
            var operatorComponent = caller.GetComponent<SparkEngineeringDeviceOperator>();
            if (!operatorComponent)
            { result = "Device Operator v16 is not bound. Run AAA Laboratory > 33 - Bind Device Operator v16."; return true; }
            if (q == "spark inspect devices") result = operatorComponent.Inspect();
            else if (q == "spark device status") result = "Device action=" + operatorComponent.Proposal +
                ", busy=" + operatorComponent.IsBusy + ". " + operatorComponent.LastStatus;
            else if (q == "spark approve device action") result = operatorComponent.Approve();
            else if (q == "spark cancel device action") result = operatorComponent.Cancel();
            else
            {
                string spec = normalized.Trim().Substring("spark propose device ".Length).Trim();
                bool turnOn;
                if (spec.EndsWith(" on", StringComparison.OrdinalIgnoreCase)) { turnOn = true; spec = spec.Substring(0, spec.Length - 3).Trim(); }
                else if (spec.EndsWith(" off", StringComparison.OrdinalIgnoreCase)) { turnOn = false; spec = spec.Substring(0, spec.Length - 4).Trim(); }
                else { result = "Use: spark propose device <exact device ID> on|off"; return true; }
                result = operatorComponent.Propose(spec, turnOn);
            }
            return true;
        }
    }
}
