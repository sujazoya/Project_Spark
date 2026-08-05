using System.Collections.Generic;

namespace ProjectSpark.Domain.Simulation
{
    public sealed class LoopDetector
    {
        public bool HasLoop(
            CircuitGraph graph)
        {
            HashSet<CircuitNode> visited =
                new();

            foreach (var node in graph.Nodes)
            {
                if (visited.Contains(node))
                    continue;

                if (Search(node, null, visited))
                    return true;
            }

            return false;
        }

        private bool Search(
            CircuitNode node,
            CircuitNode parent,
            HashSet<CircuitNode> visited)
        {
            visited.Add(node);

            foreach (var edge in node.Connections)
            {
                CircuitNode next =
                    edge.A == node
                        ? edge.B
                        : edge.A;

                if (next == parent)
                    continue;

                if (visited.Contains(next))
                    return true;

                if (Search(next,
                    node,
                    visited))
                    return true;
            }

            return false;
        }
    }
}
