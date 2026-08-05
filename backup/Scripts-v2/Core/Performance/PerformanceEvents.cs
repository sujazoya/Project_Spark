using System;

namespace ProjectSpark.Core.Performance
{
    public static class PerformanceEvents
    {
        public static event Action<float>
            FPSChanged;

        public static event Action<long>
            MemoryChanged;

        public static void RaiseFPS(
            float fps)
        {
            FPSChanged?.Invoke(fps);
        }

        public static void RaiseMemory(
            long memory)
        {
            MemoryChanged?.Invoke(memory);
        }
    }
}
