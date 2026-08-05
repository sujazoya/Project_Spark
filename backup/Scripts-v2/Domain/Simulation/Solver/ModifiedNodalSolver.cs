using ProjectSpark.Domain.Simulation;

namespace ProjectSpark.Domain.Simulation.Solver
{
    public sealed class ModifiedNodalSolver
    {
        private readonly MatrixBuilder
            builder = new();

        private readonly GaussianSolver
            gaussian = new();

        public SolverResult Solve(
            CircuitGraph graph)
        {
            SimulationMatrix matrix =
                builder.Build(graph);

            return gaussian.Solve(matrix);
        }
    }
}
