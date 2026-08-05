using ProjectSpark.Domain.Simulation;

namespace ProjectSpark.Domain.Simulation.Solver
{
    public sealed class MatrixBuilder
    {
        public SimulationMatrix Build(
            CircuitGraph graph)
        {
            SimulationMatrix matrix =
                new(graph.Nodes.Count);

            // Components will stamp
            // themselves into matrix here.

            return matrix;
        }
    }
}
