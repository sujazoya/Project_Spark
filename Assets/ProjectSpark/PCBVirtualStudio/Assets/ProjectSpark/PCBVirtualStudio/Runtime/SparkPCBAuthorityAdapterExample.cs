using UnityEngine;

namespace ProjectSpark.PCBVirtualStudio
{
    // EXAMPLE INTEGRATION TEMPLATE: replace these methods with actual Project Spark solver
    // queries and mutations after examining the existing project APIs. Disabled by default.
    public sealed class SparkPCBAuthorityAdapterExample : MonoBehaviour, ISparkPCBRepairAuthority
    {
        public bool TryApplyRepair(SparkVirtualPCBTarget target, SparkVirtualTool tool, out string message)
        {
            message = "BLOCKED: attach the existing Project Spark repair authority and validate topology. " +
                      "This template cannot commit real copper or component changes.";
            return false;
        }

        public bool TryReadProbe(SparkVirtualPCBTarget target, SparkVirtualTool tool, out string reading)
        {
            reading = "BLOCKED: bind actual voltage, waveform, continuity or temperature results " +
                      "from the existing solver. No simulated numbers are presented as live readings.";
            return false;
        }
    }
}
