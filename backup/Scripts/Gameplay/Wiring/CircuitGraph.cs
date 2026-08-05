// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/CircuitGraph.cs

using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class CircuitGraph
    {
        private readonly Dictionary<WireConnector, List<WireConnector>> graph = new();

        public void Clear()
        {
            graph.Clear();
        }

        public void AddConnection(WireConnector a, WireConnector b)
        {
            if (a == null || b == null)
                return;

            if (!graph.ContainsKey(a))
                graph.Add(a, new List<WireConnector>());

            if (!graph.ContainsKey(b))
                graph.Add(b, new List<WireConnector>());

            if (!graph[a].Contains(b))
                graph[a].Add(b);

            if (!graph[b].Contains(a))
                graph[b].Add(a);
        }

        public bool HasPath(WireConnector start, WireConnector end)
        {
            if (start == null || end == null)
                return false;

            HashSet<WireConnector> visited = new();

            Queue<WireConnector> queue = new();

            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                WireConnector current = queue.Dequeue();

                if (current == end)
                    return true;

                if (!visited.Add(current))
                    continue;

                if (!graph.TryGetValue(current, out var list))
                    continue;

                foreach (var c in list)
                    queue.Enqueue(c);
            }

            return false;
        }
    }
}