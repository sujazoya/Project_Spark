using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class ElectricalGraph
    {
        public List<ElectricalNode> Nodes { get; } = new();

        public List<ElectronicComponent> Components { get; } = new();

        public void Clear()
        {
            Nodes.Clear();
            Components.Clear();
        }
    }
}
