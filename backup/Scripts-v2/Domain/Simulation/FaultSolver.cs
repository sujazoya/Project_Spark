namespace ProjectSpark.Domain.Simulation
{
    public sealed class FaultSolver
        : ISimulationSolver
    {
        public void Execute(
            SimulationContext context)
        {
            foreach (CircuitEdge edge
                in context.Graph.Edges)
            {
                EdgeState state =
                    context.Edges[edge];

                state.Faulted =
                    state.Temperature > 120f;
            }
        }
    }
}
