namespace ProjectSpark.Domain.Simulation.Solver
{
    public sealed class SolverResult
    {
        public bool Success;

        public int Iterations;

        public double[] Voltages;

        public double SolveTime;
    }
}
