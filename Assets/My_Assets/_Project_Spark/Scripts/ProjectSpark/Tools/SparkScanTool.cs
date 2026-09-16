using ProjectSpark.Gameplay;
using ProjectSpark.Scan;
using UnityEngine;

namespace ProjectSpark.Tools
{
    public sealed class SparkScanTool : SparkTool
    {
        [Header("Scan System")]
        [SerializeField]
        private SparkScanSystem scanSystem;

        public SparkScanReport LastReport
        {
            get;
            private set;
        }

        protected override SparkToolType GetToolType()
        {
            return SparkToolType.Scan;
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

            if (scanSystem == null)
            {
                reason =
                    "Scan system is not configured.";

                return false;
            }

            if (context.TargetHit.Target == null)
            {
                reason =
                    "No component selected.";

                return false;
            }

            reason = null;
            return true;
        }

        protected override SparkResult OnBegin(
            SparkToolContext context)
        {
            if (scanSystem == null)
            {
                return SparkResult.Unavailable(
                    "Scan system is not configured.");
            }

            if (context.TargetHit.Target == null)
            {
                return SparkResult.Invalid(
                    "No component selected.");
            }

            SparkResult result =
                scanSystem.TryScan(
                    context.TargetHit.Target,
                    out SparkScanReport report);

            if (result.Succeeded)
            {
                LastReport =
                    report;
            }

            return result;
        }
    }
}