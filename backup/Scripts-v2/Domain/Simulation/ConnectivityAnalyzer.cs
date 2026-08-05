using System.Collections.Generic;

namespace ProjectSpark.Domain.Simulation
{
    public sealed class ConnectivityAnalyzer
    {
        public void FloodFill(
            CircuitNode node)
        {
            Queue<CircuitNode> queue =
                new();

            queue.Enqueue(node);

            while (queue.Count > 0)
            {
                CircuitNode current =
                    queue.Dequeue();

                current.Dirty = false;

                foreach (CircuitEdge edge
                    in current.Connections)
                {
                    CircuitNode next =
                        edge.A == current
                            ? edge.B
                            : edge.A;

                    if (next.Dirty)
                        queue.Enqueue(next);
                }
            }
        }
    }
}
