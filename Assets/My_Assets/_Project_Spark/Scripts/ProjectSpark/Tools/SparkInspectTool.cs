using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Tools
{
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
            if (!base.CanBegin(context, out reason))
                return false;

            if (context.TargetHit.Target == null)
            {
                reason = "No component selected.";
                return false;
            }

            reason = null;
            return true;
        }

        protected override SparkResult OnBegin(
            SparkToolContext context)
        {
            if (context.TargetHit.Target == null)
            {
                return SparkResult.Invalid(
                    "No component selected.");
            }

            SparkInteractionContext interactionContext =
                context.Gameplay.CreateInteractionContext(
                    context.TargetHit,
                    SparkInteractionType.Inspect);

            return context.TargetHit.Target.Inspect(
                interactionContext);
        }
    }
}