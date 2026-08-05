namespace ProjectSpark.Domain.Simulation
{
    public sealed class CircuitValidator
    {
        public bool Validate(CircuitGraph graph)
        {
            return
                graph.Nodes.Count > 0 &&
                graph.Edges.Count > 0;
        }
    }
}
