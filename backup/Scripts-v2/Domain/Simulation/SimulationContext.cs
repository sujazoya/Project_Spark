using System.Collections.Generic;

namespace ProjectSpark.Domain.Simulation
{
    public sealed class SimulationContext
    {
        public CircuitGraph Graph;

        public Dictionary<CircuitNode, NodeState> Nodes =
            new();

        public Dictionary<CircuitEdge, EdgeState> Edges =
            new();
    }
}
