namespace ProjectSpark.Domain.Simulation
{
    public sealed class VoltageSolver : ISimulationSolver
    {
        public void Execute(SimulationContext context)
        {
            Solve(context.Graph);
        }

        public void Solve(CircuitGraph graph)
        {
            foreach (CircuitNode node in graph.Nodes)
            {
                // TODO:
                // Implement voltage calculation.
                // Temporary placeholder.
                node.Voltage = 0f;
            }
        }
    }
}