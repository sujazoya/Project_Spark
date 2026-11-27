using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    internal enum NexusStage
    {
        Idle, Resolving, Validating, AwaitingApproval, PreparingMode, AwaitingModeAck, Revalidating,
        Submitted, Accepted, Running, Verifying, Succeeded, Failed, Cancelled, TimedOut, Answered, Unknown
    }

    internal sealed class SparkNexusWorkflow
    {
        private readonly SparkProExecutive executive;
        private readonly SparkNexusContext context;
        private readonly SparkEngineeringControlHub hub;
        private readonly SparkEngineeringDeviceOperator devices;
        private NexusCommand command;
        private string phrase;
        private string parameter;
        private NexusSnapshot initial;
        private SparkNativeWiringPlan plan;
        private string reviewedRevision;
        private string consentEvidence;
        private int modeRevision;
        private int operationRevision;
        private float deadline;

        public Guid RequestId { get; private set; }
        public NexusStage Stage { get; private set; }
        public string Detail { get; private set; } = "";
        public string LastCommandId { get; private set; }
        public bool Busy => Stage != NexusStage.Idle && (int)Stage < (int)NexusStage.Succeeded;
        public event Action Changed;

        public SparkNexusWorkflow(SparkProExecutive executive, SparkNexusContext context)
        {
            this.executive = executive; this.context = context;
            hub = executive.GetComponent<SparkEngineeringControlHub>();
            devices = executive.GetComponent<SparkEngineeringDeviceOperator>();
        }

        private void Set(NexusStage stage, string detail)
        {
            Stage = stage; Detail = detail ?? ""; Changed?.Invoke();
        }

        public string Blocker(NexusCommand entry, NexusSnapshot snapshot)
        {
            if (!Available(entry.Capability)) return "Native handler unavailable in this session.";
            if (executive.ExamMode && entry.Category != "Session") return "Independent examination is active.";
            if (entry.Mode != NexusMode.Retain && !hub) return "Engineering mode owner unavailable.";
            if (entry.Id.StartsWith("work.") && snapshot?.Level == null) return "Load a campaign level.";
            if (entry.Id is "flight.selected" or "work.selected" && !(executive.ActiveSelectionController && executive.ActiveSelectionController.SelectedObject))
                return "Select an object on the workbench.";
            if (entry.Parameter == NexusParameter.Device && (snapshot == null || !snapshot.Targets.Any(target => target.Component is SparkEngineeringDevicePort)))
                return "No active device port is bound.";
            if (entry.Parameter == NexusParameter.Terminal && (snapshot == null || !snapshot.Targets.Any(target => target.Component is ProjectSpark.Gameplay.SparkTerminal)))
                return "No active terminal binding is available.";
            if (entry.Evidence == NexusEvidence.Wire && (!hub || hub.PendingStep == "None")) return "Suggest a wire first.";
            if (entry.Evidence == NexusEvidence.Device && (!devices || devices.Proposal == "None")) return "Propose a device action first.";
            if (entry.Evidence is NexusEvidence.Plan or NexusEvidence.Suggestion && snapshot?.Layout == null)
                return "Load a bound layout.";
            if (entry.Evidence == NexusEvidence.Resume && (!hub || !hub.IsPaused))
                return "No paused construction.";
            if (entry.Evidence == NexusEvidence.Pause && (!hub || !hub.IsExecuting))
                return "No construction in progress.";
            return "";
        }

        private bool Available(NexusCapability capability) => capability switch
        {
            NexusCapability.Engineering => hub && hub.isActiveAndEnabled,
            NexusCapability.Devices => devices && devices.isActiveAndEnabled,
            NexusCapability.NativeRecorder => executive.TryGetComponent<SparkNativeWireAutoEngineer>(out var recorder) && recorder.isActiveAndEnabled,
            NexusCapability.NativeWire => executive.TryGetComponent<SparkNativeWireDirector>(out var wire) && wire.isActiveAndEnabled,
            NexusCapability.Lab => executive.TryGetComponent<SparkLabAutonomyAgent>(out var agent) && agent.isActiveAndEnabled,
            NexusCapability.Demo => executive.TryGetComponent<SparkLabDemonstrationDirector>(out var demo) && demo.isActiveAndEnabled,
            NexusCapability.Mission => executive.TryGetComponent<SparkEngineeringMissionAdvisor>(out var mission) && mission.isActiveAndEnabled,
            NexusCapability.Flight => executive.TryGetComponent<SparkV5FlightController>(out var flight) && flight.isActiveAndEnabled,
            NexusCapability.Arms => executive.TryGetComponent<SparkTelescopicArms>(out var arms) && arms.isActiveAndEnabled,
            NexusCapability.Teaching => executive.Hologram,
            _ => executive && executive.isActiveAndEnabled
        };

        public void Start(NexusCommand entry, string value)
        {
            if (Busy) return;
            context.Tick();
            command = entry; parameter = value?.Trim() ?? ""; initial = context.Snapshot;
            plan = null; reviewedRevision = null; consentEvidence = null; RequestId = Guid.NewGuid();
            LastCommandId = entry?.Id;
            Set(NexusStage.Resolving, entry?.Title ?? "Ask Spark");
            if (!Validate(out string reason)) { Set(NexusStage.Failed, reason); return; }
            Set(NexusStage.Validating, phrase);
            if (entry?.Approval is NexusApproval.Confirm or NexusApproval.NativeProposal)
            {
                consentEvidence = entry.Evidence == NexusEvidence.Device ? devices.Proposal
                    : entry.Evidence == NexusEvidence.Wire ? hub.PendingStep
                    : plan ? plan.name + " · " + reviewedRevision : null;
                Set(NexusStage.AwaitingApproval, phrase + (string.IsNullOrEmpty(consentEvidence) ? "" : "\n" + consentEvidence));
                return;
            }
            Prepare();
        }

        public void Confirm()
        {
            if (Stage == NexusStage.AwaitingApproval) Prepare();
        }

        private bool Validate(out string reason)
        {
            var current = context.Snapshot;
            reason = command == null ? "" : Blocker(command, current);
            if (reason.Length > 0) return false;
            if (current == null || initial == null || current.Epoch != initial.Epoch || current.Layout != initial.Layout)
            { reason = "Level changed. Select a command for the current workbench."; return false; }

            if (command == null || command.Parameter == NexusParameter.Text)
            {
                if (parameter.Length == 0 || parameter.Length > 900 || parameter.Any(char.IsControl))
                { reason = "Enter a single line of up to 900 characters."; return false; }
            }
            else if (command.Parameter == NexusParameter.Level)
            {
                if (!int.TryParse(parameter, NumberStyles.None, CultureInfo.InvariantCulture, out int number) || number < 1)
                { reason = "Enter a positive level number."; return false; }
                if (command.Id == "teach.draw" && (!current.Level || current.Level.LevelNumber != number))
                { reason = "Load that level before opening its diagram."; return false; }
            }
            else if (command.Parameter == NexusParameter.Metres &&
                (!float.TryParse(parameter, NumberStyles.Float, CultureInfo.InvariantCulture, out float distance) ||
                 float.IsInfinity(distance) || float.IsNaN(distance) || distance <= 0))
            { reason = "Enter a positive distance in metres."; return false; }
            else if (command.Parameter is NexusParameter.Device or NexusParameter.Terminal)
            {
                var match = current.Targets.Where(target => target.NativeId == parameter && target.Component &&
                    target.Component.gameObject.activeInHierarchy && (command.Parameter == NexusParameter.Device
                        ? target.Component is SparkEngineeringDevicePort
                        : target.Component is ProjectSpark.Gameplay.SparkTerminal)).ToArray();
                if (match.Length != 1 || !initial.Targets.Any(target => target.Component == match[0].Component && target.NativeId == parameter))
                { reason = "The exact native target is absent, ambiguous, or replaced."; return false; }
            }

            if (command?.Evidence is NexusEvidence.Plan or NexusEvidence.Suggestion)
            {
                var candidate = hub.GetNexusConstructionPlan(out reason);
                if (!candidate) return false;
                if (plan && (candidate != plan || candidate.ReviewedRevision != reviewedRevision))
                { reason = "Reviewed plan changed. Reopen the command."; return false; }
                plan = candidate; reviewedRevision = plan.ReviewedRevision;
                if (current.Topology != initial.Topology)
                { reason = "Circuit changed before submission. Review the current connections."; return false; }
            }

            if (command?.Evidence == NexusEvidence.Wire && consentEvidence != null &&
                (hub.PendingStep != consentEvidence || current.Topology != initial.Topology))
            { reason = "The proposed wire or circuit changed. Review the new proposal."; return false; }
            if (command?.Evidence == NexusEvidence.Device && consentEvidence != null &&
                (devices.Proposal != consentEvidence || current.Topology != initial.Topology))
            { reason = "The device proposal or circuit changed. Review the new proposal."; return false; }

            phrase = command?.Format(parameter) ?? parameter;
            return true;
        }

        private void Prepare()
        {
            context.Tick();
            if (!Validate(out string reason)) { Set(NexusStage.Failed, reason); return; }
            bool control = command?.Evidence is NexusEvidence.Pause or NexusEvidence.Resume or NexusEvidence.CancelPlan or NexusEvidence.CancelDevice;
            if (!control && ((hub && hub.IsExecuting) || (devices && devices.IsBusy)))
            { Set(NexusStage.Failed, "Finish or cancel the current native operation."); return; }

            if (command != null && command.Mode != NexusMode.Retain && command.Evidence != NexusEvidence.Mode)
            {
                var target = RequiredMode(command.Mode);
                if (hub.Mode != target)
                {
                    if (command.Approval == NexusApproval.NativeProposal)
                    { Set(NexusStage.Failed, "Native proposal mode changed. Make a new proposal."); return; }
                    Set(NexusStage.PreparingMode, "Requesting " + target);
                    modeRevision = hub.ModeAcknowledgement;
                    deadline = Time.realtimeSinceStartup + 4f;
                    Set(NexusStage.AwaitingModeAck, "Waiting for the engineering owner");
                    if (!executive.TrySubmitNexus("spark " + target.ToString().ToLowerInvariant() + " mode"))
                        Set(NexusStage.Failed, "Spark Live cannot accept the mode request.");
                    return;
                }
            }
            Submit();
        }

        public void Tick()
        {
            if (Stage == NexusStage.AwaitingModeAck)
            {
                if (context.Snapshot?.Epoch != initial.Epoch)
                { Set(NexusStage.Cancelled, "Level changed before dispatch."); return; }
                if (hub.ModeAcknowledgement != modeRevision)
                {
                    if (hub.ModeAcknowledgement != modeRevision + 1 || hub.Mode != RequiredMode(command.Mode))
                    { Set(NexusStage.Failed, "The engineering owner acknowledged a different mode."); return; }
                    Submit();
                }
                else if (Time.realtimeSinceStartup >= deadline)
                    Set(NexusStage.TimedOut, "No native mode acknowledgement. The command was not sent.");
                return;
            }
            if (Stage != NexusStage.Running) return;
            if (context.Snapshot?.Epoch != initial.Epoch)
            { Set(NexusStage.Unknown, "Level changed after submission. Check native state."); return; }

            if (command.Evidence == NexusEvidence.Device)
            {
                if (devices.OperationRevision != operationRevision)
                { Set(NexusStage.Unknown, "The native device owner moved to another request."); return; }
                if (devices.IsBusy) { TimeoutIfRequired(); return; }
                Set(NexusStage.Verifying, devices.LastStatus);
                Set(devices.VerifiedOperationRevision == operationRevision ? NexusStage.Succeeded : NexusStage.Unknown, devices.LastStatus);
                return;
            }
            if (hub.WiringOperationRevision != operationRevision)
            { Set(NexusStage.Unknown, "The engineering owner moved to another wiring request."); return; }
            if (command.Evidence == NexusEvidence.Pause)
            {
                if (hub.Phase == SparkEngineeringControlHub.ExecutionPhase.Paused)
                    Set(NexusStage.Succeeded, "Native wiring is paused between connections.");
                else if (!hub.IsExecuting) Set(NexusStage.Unknown, hub.LastMessage);
                else TimeoutIfRequired();
                return;
            }
            if (hub.IsExecuting) { TimeoutIfRequired(); return; }
            Set(NexusStage.Verifying, hub.LastMessage);
            var manager = executive.ActiveLevelManager;
            switch (hub.Phase)
            {
                case SparkEngineeringControlHub.ExecutionPhase.Completed:
                    var evidence = manager && manager.HasCurrentElectricalEvaluation
                        ? $" · {manager.SatisfiedTargetCount}/{manager.ActiveTargetCount} objectives verified by evaluator"
                        : " · Objective evidence unavailable";
                    Set(NexusStage.Succeeded, hub.LastMessage + evidence);
                    break;
                case SparkEngineeringControlHub.ExecutionPhase.Cancelled: Set(NexusStage.Cancelled, hub.LastMessage); break;
                case SparkEngineeringControlHub.ExecutionPhase.Failed: Set(NexusStage.Failed, hub.LastMessage); break;
                default: Set(NexusStage.Unknown, hub.LastMessage); break;
            }
        }

        private void TimeoutIfRequired()
        {
            if (Time.realtimeSinceStartup >= deadline)
                Set(NexusStage.TimedOut, "Native completion is still unconfirmed. Inspect status before sending another action.");
        }

        private void Submit()
        {
            Set(NexusStage.Revalidating, phrase);
            context.Tick();
            if (!Validate(out string reason)) { Set(NexusStage.Failed, reason); return; }
            if (command != null && command.Mode != NexusMode.Retain && command.Evidence != NexusEvidence.Mode &&
                hub.Mode != RequiredMode(command.Mode))
            { Set(NexusStage.Failed, "Native mode changed before submission."); return; }

            int previousMode = hub ? hub.ModeAcknowledgement : 0;
            int previousDevice = devices ? devices.OperationRevision : 0;
            int previousWiring = hub ? hub.WiringOperationRevision : 0;
            Set(NexusStage.Submitted, phrase);
            string reply = null;
            void Receipt(string answer) => reply = answer;
            executive.AnswerReady += Receipt;
            bool accepted;
            try { accepted = executive.TrySubmitNexus(phrase); }
            finally { executive.AnswerReady -= Receipt; }
            if (!accepted) { Set(NexusStage.Failed, "Spark Live could not accept the request."); return; }
            Set(NexusStage.Accepted, reply ?? phrase);
            deadline = Time.realtimeSinceStartup + 180f;
            switch (command?.Evidence)
            {
                case NexusEvidence.Mode:
                    Set(hub.ModeAcknowledgement != previousMode && hub.Mode == RequiredMode(command.Mode)
                        ? NexusStage.Succeeded : NexusStage.Failed, reply);
                    break;
                case NexusEvidence.Plan:
                case NexusEvidence.Wire:
                    operationRevision = hub.WiringOperationRevision;
                    Set(hub.IsExecuting && operationRevision == previousWiring + 1 ? NexusStage.Running : NexusStage.Failed, reply);
                    break;
                case NexusEvidence.Device:
                    operationRevision = devices.OperationRevision;
                    Set(devices.IsBusy && operationRevision == previousDevice + 1 ? NexusStage.Running : NexusStage.Failed, reply);
                    break;
                case NexusEvidence.Suggestion:
                    Set(hub.PendingStep != "None" ? NexusStage.Succeeded : NexusStage.Unknown, reply);
                    break;
                case NexusEvidence.Proposal:
                    Set(devices.Proposal != "None" ? NexusStage.Succeeded : NexusStage.Failed, reply);
                    break;
                case NexusEvidence.Pause:
                    operationRevision = previousWiring;
                    Set(hub.IsPaused && hub.IsExecuting ? NexusStage.Running : NexusStage.Failed, reply);
                    break;
                case NexusEvidence.Resume:
                    operationRevision = previousWiring;
                    Set(hub.IsExecuting && !hub.IsPaused ? NexusStage.Running : NexusStage.Failed, reply);
                    break;
                case NexusEvidence.CancelPlan:
                    Set(!hub.IsExecuting && hub.Phase == SparkEngineeringControlHub.ExecutionPhase.Cancelled
                        ? NexusStage.Cancelled : NexusStage.Unknown, reply);
                    break;
                case NexusEvidence.CancelDevice:
                    Set(!devices.IsBusy && devices.Proposal == "None" ? NexusStage.Cancelled : NexusStage.Unknown, reply);
                    break;
                default:
                    // The existing transcript carries answers, including asynchronous and teaching responses.
                    bool answered = reply != null && (command == null || command.Evidence == NexusEvidence.Reply);
                    Set(answered ? NexusStage.Answered : NexusStage.Unknown,
                        reply ?? "Submitted to Spark Live. Follow the native transcript for a response.");
                    break;
            }
        }

        public void Cancel()
        {
            if (Stage is NexusStage.AwaitingApproval or NexusStage.AwaitingModeAck)
            { Set(NexusStage.Cancelled, "Cancelled before command dispatch."); return; }
            if ((hub && hub.IsExecuting) || (devices && devices.IsBusy))
            {
                bool device = devices && devices.IsBusy;
                if (!executive.TrySubmitNexus(device ? "spark cancel device action" : "spark stop drawing"))
                { Set(NexusStage.Unknown, "Spark Live did not accept cancellation."); return; }
                bool stopped = device ? !devices.IsBusy && devices.Proposal == "None"
                    : !hub.IsExecuting && hub.Phase == SparkEngineeringControlHub.ExecutionPhase.Cancelled;
                Set(stopped ? NexusStage.Cancelled : NexusStage.Unknown, "Native owner: " + (device ? devices.LastStatus : hub.LastMessage));
            }
        }

        public void TogglePause()
        {
            if (!hub || !hub.IsExecuting || !executive.TrySubmitNexus(hub.IsPaused ? "spark resume drawing" : "spark pause drawing")) return;
            if (Stage != NexusStage.Running) return;
            Set(NexusStage.Running, hub.LastMessage);
        }

        private static SparkEngineeringControlHub.OperatingMode RequiredMode(NexusMode mode) =>
            (SparkEngineeringControlHub.OperatingMode)((int)mode - 1);
    }
}
