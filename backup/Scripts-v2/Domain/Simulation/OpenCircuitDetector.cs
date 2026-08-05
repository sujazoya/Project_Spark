namespace ProjectSpark.Domain.Simulation
{
    public sealed class OpenCircuitDetector
    {
        public bool Detect(CircuitGraph graph)
        {
            foreach (CircuitEdge edge in graph.Edges)
            {
                if (edge == null)
                    return true;
            }

            return false;
        }
    }
}
