using System.Collections.Generic;

namespace ProjectSpark.Domain.Simulation
{
    public interface ICircuitComponent
    {
        string Id { get; }

        IReadOnlyList<CircuitNode> Nodes { get; }

        bool Enabled { get; }

        void Simulate();
    }
}
