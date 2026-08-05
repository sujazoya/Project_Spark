using System.Collections.Generic;

namespace ProjectSpark.Domain.Simulation
{
    public sealed class DirtyRegionTracker
    {
        private readonly HashSet<CircuitNode>
            dirtyNodes = new();

        public void MarkDirty(CircuitNode node)
        {
            dirtyNodes.Add(node);
        }

        public IReadOnlyCollection<CircuitNode>
            DirtyNodes => dirtyNodes;

        public void Clear()
        {
            dirtyNodes.Clear();
        }
    }
}
