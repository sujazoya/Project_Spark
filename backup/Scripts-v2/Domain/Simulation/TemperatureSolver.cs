namespace ProjectSpark.Domain.Simulation
{
    public sealed class TemperatureSolver : ISimulationSolver
    {
        public void Execute(SimulationContext context)
        {
            Solve(context.Graph);
        }

        public void Solve(CircuitGraph graph)
        {
            foreach (CircuitEdge edge in graph.Edges)
            {
                // Basic power estimation
                float power =
                    edge.Current * edge.Current * edge.Resistance;

                // If your CircuitEdge doesn't store temperature yet,
                // this is currently just a placeholder.
                // TODO: Store temperature in an EdgeState or CircuitEdge.
            }
        }
    }
}