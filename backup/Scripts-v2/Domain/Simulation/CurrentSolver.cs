namespace ProjectSpark.Domain.Simulation
{
    public sealed class CurrentSolver : ISimulationSolver
    {
        public void Execute(SimulationContext context)
        {
            Solve(context.Graph);
        }

        public void Solve(CircuitGraph graph)
        {
            foreach (CircuitEdge edge in graph.Edges)
            {
                if (edge.Resistance <= 0f)
                {
                    edge.Current = 0f;
                    continue;
                }

                edge.Current =
                    (edge.A.Voltage - edge.B.Voltage) /
                    edge.Resistance;
            }
        }
    }
}