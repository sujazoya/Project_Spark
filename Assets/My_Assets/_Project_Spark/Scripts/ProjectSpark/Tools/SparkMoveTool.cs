using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Tools
{
    public sealed class SparkMoveTool : SparkTool
    {
        protected override SparkToolType GetToolType()
        {
            return SparkToolType.Move;
        }

        private SparkTransformManipulable GetManipulator(
            SparkToolContext context)
        {
            if (context.TargetHit.Target == null)
                return null;

            return context.TargetHit.Target
                .GetComponentInChildren<SparkTransformManipulable>();
        }

        public override bool CanBegin(
            in SparkToolContext context,
            out string reason)
        {
            if (!base.CanBegin(context, out reason))
                return false;

            SparkTransformManipulable manipulator =
                GetManipulator(context);

            if (manipulator == null)
            {
                reason =
                    "Component has no SparkTransformManipulable.";

                return false;
            }

            SparkInteractionContext interactionContext =
                context.Gameplay.CreateInteractionContext(
                    context.TargetHit,
                    SparkInteractionType.Move);

            return manipulator.CanMove(
                interactionContext,
                out reason);
        }

        protected override SparkResult OnBegin(
            SparkToolContext context)
        {
            SparkTransformManipulable manipulator =
                GetManipulator(context);

            if (manipulator == null)
            {
                return SparkResult.Unavailable(
                    "Move controller is missing.");
            }

            SparkInteractionContext interactionContext =
                context.Gameplay.CreateInteractionContext(
                    context.TargetHit,
                    SparkInteractionType.Move);

            return manipulator.BeginMove(
                interactionContext);
        }

        protected override SparkResult OnUpdate(
            SparkToolContext context)
        {
            SparkTransformManipulable manipulator =
                GetManipulator(context);

            if (manipulator == null)
            {
                return SparkResult.Unavailable(
                    "Move controller is missing.");
            }

            SparkInteractionContext interactionContext =
                context.Gameplay.CreateInteractionContext(
                    context.TargetHit,
                    SparkInteractionType.Move);

            return manipulator.UpdateMove(
                interactionContext);
        }

        protected override SparkResult OnEnd(
            SparkToolContext context)
        {
            SparkTransformManipulable manipulator =
                GetManipulator(context);

            if (manipulator == null)
            {
                return SparkResult.Unavailable(
                    "Move controller is missing.");
            }

            SparkInteractionContext interactionContext =
                context.Gameplay.CreateInteractionContext(
                    context.TargetHit,
                    SparkInteractionType.Move);

            return manipulator.EndMove(
                interactionContext);
        }

        protected override SparkResult OnCancel(
            SparkToolContext context)
        {
            SparkTransformManipulable manipulator =
                GetManipulator(context);

            if (manipulator == null)
                return SparkResult.Cancelled();

            SparkInteractionContext interactionContext =
                context.Gameplay.CreateInteractionContext(
                    context.TargetHit,
                    SparkInteractionType.Move);

            return manipulator.CancelMove(
                interactionContext);
        }
    }
}