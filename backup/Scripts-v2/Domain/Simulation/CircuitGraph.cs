using System.Collections.Generic;

namespace ProjectSpark.Domain.Simulation
{
    public sealed class CircuitGraph
    {
        public readonly List<CircuitNode>
            Nodes =
                new();

        public readonly List<CircuitEdge>
            Edges =
                new();

        public void Clear()
        {
            Nodes.Clear();
            Edges.Clear();
        }
    }
}
