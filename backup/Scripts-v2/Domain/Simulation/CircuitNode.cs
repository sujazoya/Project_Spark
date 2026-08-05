using System.Collections.Generic;

namespace ProjectSpark.Domain.Simulation
{
    public sealed class CircuitNode
    {
        public int Id;

        public readonly List<CircuitEdge>
            Connections =
                new();

        public float Voltage;

        public bool Dirty;
    }
}
