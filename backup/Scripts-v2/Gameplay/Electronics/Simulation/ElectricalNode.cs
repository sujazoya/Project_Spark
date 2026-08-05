using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class ElectricalNode
    {
        public int Id { get; }

        public List<ConnectionPoint> Pins { get; } = new();

        public float Voltage;

        public bool Visited;

        public ElectricalNode(int id)
        {
            Id = id;
        }

        public void Add(ConnectionPoint pin)
        {
            if (!Pins.Contains(pin))
                Pins.Add(pin);
        }
    }
}
