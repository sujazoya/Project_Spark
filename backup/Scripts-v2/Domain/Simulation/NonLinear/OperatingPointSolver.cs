namespace ProjectSpark.Domain.Simulation.NonLinear
{
    public sealed class OperatingPointSolver
    {
        private readonly NewtonSolver
            solver =
                new();

        public bool Solve(
            NonLinearContext context)
        {
            return solver.Solve(
                context);
        }
    }
}
