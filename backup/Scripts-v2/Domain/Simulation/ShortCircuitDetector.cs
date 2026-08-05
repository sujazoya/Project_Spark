namespace ProjectSpark.Domain.Simulation
{
    public sealed class ShortCircuitDetector
    {
        public bool Detect(CircuitGraph graph)
        {
            foreach (CircuitEdge edge in graph.Edges)
            {
                if (edge == null)
                    continue;

                if (edge.Resistance < 0.01f &&
                    edge.Current > 20f)
                {
                    return true;
                }
            }

            return false;
        }
    }
}