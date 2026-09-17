using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Tools
{
    [DisallowMultipleComponent]
    public sealed class SparkInspectTool : SparkTool
    {
        protected override SparkToolType GetToolType()
        {
            return SparkToolType.Inspect;
        }

        public override bool CanBegin(
            in SparkToolContext context,
            out string reason)
        {
            if (!base.CanBegin(
                    context,
                    out reason))
            {
                return false;
            }

            if (!context.HasTarget)
            {
                reason =
                    "No object under pointer.";

                return false;
            }

            SparkElectronicObject target =
                context.TargetHit.Target;

            if (target == null)
            {
                reason =
                    "Inspection target is missing.";

                return false;
            }

            ISparkInteractable interactable =
                target as ISparkInteractable;

            if (interactable == null)
            {
                reason =
                    "Target does not support inspection.";

                return false;
            }

            if (!interactable.CanInteract(
                    context.InteractionContext,
                    out reason))
            {
                return false;
            }

            reason = null;
            return true;
        }

        protected override SparkResult OnBegin(
            SparkToolContext context)
        {
            if (!context.HasTarget)
            {
                return SparkResult.Invalid(
                    "No inspection target.");
            }

            SparkElectronicObject target =
                context.TargetHit.Target;

            if (target == null)
            {
                return SparkResult.Invalid(
                    "Inspection target is missing.");
            }

            ISparkInteractable interactable =
                target as ISparkInteractable;

            if (interactable == null)
            {
                return SparkResult.Invalid(
                    "Target does not support inspection.");
            }

            return interactable.BeginInteraction(
                context.InteractionContext);
        }

        protected override SparkResult OnUpdate(
            SparkToolContext context)
        {
            if (Session == null)
            {
                return SparkResult.Unavailable(
                    "Inspection session is not active.");
            }

            SparkElectronicObject target =
                Session.Target;

            if (target == null)
            {
                return SparkResult.Invalid(
                    "Inspection target no longer exists.");
            }

            ISparkInteractable interactable =
                target as ISparkInteractable;

            if (interactable == null)
            {
                return SparkResult.Invalid(
                    "Target does not support inspection.");
            }

            return interactable.UpdateInteraction(
                context.InteractionContext);
        }

        protected override SparkResult OnEnd(
            SparkToolContext context)
        {
            if (Session == null)
            {
                return SparkResult.Unavailable(
                    "Inspection session is not active.");
            }

            SparkElectronicObject target =
                Session.Target;

            if (target == null)
            {
                return SparkResult.Invalid(
                    "Inspection target no longer exists.");
            }

            ISparkInteractable interactable =
                target as ISparkInteractable;

            if (interactable == null)
            {
                return SparkResult.Invalid(
                    "Target does not support inspection.");
            }

            return interactable.EndInteraction(
                context.InteractionContext);
        }

        protected override SparkResult OnCancel(
            SparkToolContext context)
        {
            if (Session == null)
            {
                return SparkResult.Cancelled(
                    "Inspection cancelled.");
            }

            SparkElectronicObject target =
                Session.Target;

            if (target == null)
            {
                return SparkResult.Cancelled(
                    "Inspection target no longer exists.");
            }

            ISparkInteractable interactable =
                target as ISparkInteractable;

            if (interactable == null)
            {
                return SparkResult.Cancelled(
                    "Target does not support inspection.");
            }

            return interactable.CancelInteraction(
                context.InteractionContext);
        }
    }
}