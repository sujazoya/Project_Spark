namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class NodeSolver
    {
        public void Solve(ElectricalGraph graph)
        {
            foreach (var node in graph.Nodes)
            {
                node.Visited = false;
            }

            // Future:
            // Breadth First Search
            // Connected Components
            // Network Islands
        }
    }
}
