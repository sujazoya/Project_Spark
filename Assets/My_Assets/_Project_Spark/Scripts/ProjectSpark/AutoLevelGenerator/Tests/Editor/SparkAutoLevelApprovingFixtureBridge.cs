#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator.Tests
{
    /// <summary>TEST FIXTURE ONLY. NEVER use as the gameplay bridge in production.</summary>
    public sealed class SparkAutoLevelApprovingFixtureBridge : SparkAutoLevelAuthorityBridge
    {
        public override bool TryCommitPart(SparkAutoPart def, SparkAutoPlacementTarget target, out string rejection)
        { rejection = null; return true; }
        public override bool TryCommitRoute(SparkAutoRoute def, out string rejection)
        { rejection = null; return true; }
        public override bool TryCommitVia(SparkAutoVia def, out string rejection)
        { rejection = null; return true; }
        public override bool TryConfirmObjective(SparkAutoObjective def, out string rejection)
        { rejection = null; return true; }
        public override bool TryConfirmSafetyReset(SparkAutoSafety def, out string rejection)
        { rejection = null; return true; }
    }
}
#endif
