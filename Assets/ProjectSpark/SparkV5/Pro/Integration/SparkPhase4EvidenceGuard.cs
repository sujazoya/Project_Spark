using System;
using ProjectSpark.AI.Tutorial;

namespace ProjectSpark.SparkV5.Pro
{
    internal static class SparkPhase4EvidenceGuard
    {
        internal static bool CanPublishNumber(SparkCortex.Context context, double number, string unit) =>
            context.HasFreshSolve && !double.IsNaN(number) && !double.IsInfinity(number) &&
            !string.IsNullOrWhiteSpace(unit);

        internal static bool EvaluatorIsCurrent(SparkAILevelContext context) =>
            context.HasEvaluation && context.Manager.HasCurrentElectricalEvaluation;
    }
}
