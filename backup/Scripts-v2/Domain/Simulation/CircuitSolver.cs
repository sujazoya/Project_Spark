namespace ProjectSpark.Domain.Simulation
{
    public sealed class CircuitSolver
    {
        private readonly GraphBuilder _builder =
            new();

        public SimulationResult Solve()
        {
            CircuitGraph graph =
                _builder.Build();

            // TODO
            // Kirchhoff Solver
            // Voltage Solver
            // Current Solver
            // Fault Detection

            return new SimulationResult(
                true,
                false,
                "Simulation Complete");
        }
    }
}
