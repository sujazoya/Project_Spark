using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Tools
{
    public sealed class SparkSelectTool : SparkTool
    {
        [Header("Selection")]
        [SerializeField]
        private bool selectOnBegin = true;

        protected override SparkToolType GetToolType()
        {
            return SparkToolType.Select;
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
                return SparkResult.Invalid(
                    "No component selected.");

            if (!selectOnBegin)
                return SparkResult.Success();

            context.TargetHit.Target.SetSelected(true);

            return SparkResult.Success(
                "Component selected.");
        }
    }
}