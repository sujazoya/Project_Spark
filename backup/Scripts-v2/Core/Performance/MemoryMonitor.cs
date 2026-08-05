using UnityEngine;

namespace ProjectSpark.Core.Performance
{
    public sealed class MemoryMonitor
        : MonoBehaviour
    {
        private void Update()
        {
            long memory =
                System.GC.GetTotalMemory(false);

            PerformanceEvents
                .RaiseMemory(memory);
        }
    }
}
