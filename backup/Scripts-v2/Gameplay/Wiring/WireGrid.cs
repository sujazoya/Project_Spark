using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireGrid
    {
        private readonly Dictionary<Vector2Int, WireGridNode>
            _nodes = new();

        public void AddNode(WireGridNode node)
        {
            _nodes[node.GridPosition] = node;
        }

        public bool TryGetNode(
            Vector2Int position,
            out WireGridNode node)
        {
            return _nodes.TryGetValue(position, out node);
        }

        public IEnumerable<WireGridNode> Nodes => _nodes.Values;
    }
}
