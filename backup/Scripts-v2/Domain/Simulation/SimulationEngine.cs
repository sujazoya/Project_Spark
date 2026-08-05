using UnityEngine;

namespace ProjectSpark.Domain.Simulation
{
    public sealed class SimulationEngine
        : MonoBehaviour
    {
        private readonly GraphBuilder
            builder =
                new();

        private readonly VoltageSolver
            voltage =
                new();

        private readonly CurrentSolver
            current =
                new();

        private CircuitGraph graph;

        public SimulationStatistics
            Statistics { get; }
                = new();

        private int tick;

        public void Simulate(
            float deltaTime)
        {
            graph =
                builder.Build();

            voltage.Solve(graph);

            current.Solve(graph);

            tick++;
        }
    }
}
