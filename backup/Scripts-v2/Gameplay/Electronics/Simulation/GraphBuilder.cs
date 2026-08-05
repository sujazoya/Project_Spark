namespace ProjectSpark.Gameplay.Electronics
{
    /// <summary>
    /// Converts placed components into a simulation graph.
    /// </summary>
    public sealed class GraphBuilder
    {
        public ElectricalGraph Build(CircuitGraph graph)
        {
            var electricalGraph = new ElectricalGraph();

            foreach (var component in graph.Components)
            {
                electricalGraph.Components.Add(component);
            }

            // TODO
            // Create nodes
            // Merge connected pins
            // Detect loops
            // Detect isolated networks

            return electricalGraph;
        }
    }
}
