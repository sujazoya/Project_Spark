using System.Collections.Generic;

namespace ProjectSpark.Domain.Simulation
{
    public sealed class SimulationCache
    {
        private readonly Dictionary<int, float>
            voltageCache = new();

        public void SetVoltage(
            int node,
            float value)
        {
            voltageCache[node] = value;
        }

        public bool TryGetVoltage(
            int node,
            out float value)
        {
            return voltageCache.TryGetValue(
                node,
                out value);
        }

        public void Clear()
        {
            voltageCache.Clear();
        }
    }
}
