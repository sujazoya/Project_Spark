using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WirePathFinder
    {
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        public List<WireGridNode> FindPath(
            WireGrid grid,
            WireGridNode start,
            WireGridNode end)
        {
            Queue<WireGridNode> open = new();

            HashSet<WireGridNode> visited = new();

            open.Enqueue(start);

            visited.Add(start);

            while (open.Count > 0)
            {
                WireGridNode current =
                    open.Dequeue();

                if (current == end)
                    return BuildPath(end);

                foreach (Vector2Int dir in Directions)
                {
                    if (!grid.TryGetNode(
                        current.GridPosition + dir,
                        out WireGridNode next))
                        continue;

                    if (!next.Walkable)
                        continue;

                    if (visited.Contains(next))
                        continue;

                    next.Parent = current;

                    visited.Add(next);

                    open.Enqueue(next);
                }
            }

            return new();
        }

        private List<WireGridNode> BuildPath(
            WireGridNode end)
        {
            List<WireGridNode> path = new();

            WireGridNode current = end;

            while (current != null)
            {
                path.Add(current);

                current = current.Parent;
            }

            path.Reverse();

            return path;
        }
    }
}
