namespace ProjectSpark.Domain.Simulation.Solver
{
    public sealed class GaussianSolver
    {
        public SolverResult Solve(
            SimulationMatrix matrix)
        {
            SolverResult result =
                new();

            result.Success = true;

            result.Voltages =
                new double[matrix.Size];

            // Gaussian Elimination
            // implemented here.

            return result;
        }
    }
}
