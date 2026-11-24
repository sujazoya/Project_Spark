namespace ProjectSpark.UI.VisualTeaching
{
    internal static class SparkPhase4TeachingContract
    {
        internal static bool CanCreditPractical(bool sameAttempt, bool sameLayout, bool independent, bool pendingAtStart,
            bool passedNow, int startingSolve, int evaluatedSolve, bool evaluatorCurrent, bool requiresFrame, bool frameFresh) =>
            sameAttempt && sameLayout && independent && pendingAtStart && passedNow && evaluatorCurrent &&
            startingSolve >= 0 && evaluatedSolve > startingSolve && (!requiresFrame || frameFresh);

        internal static int ReviewAfterDays(int consecutiveIndependentPasses, bool latestIndependentPass) =>
            !latestIndependentPass || consecutiveIndependentPasses <= 1 ? 1 : consecutiveIndependentPasses switch
            {
                2 => 3, 3 => 7, 4 => 14, _ => 30
            };
    }
}
