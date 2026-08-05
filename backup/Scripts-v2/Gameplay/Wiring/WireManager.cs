using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireManager
        : MonoBehaviour
    {
        private readonly WireGraph
            graph =
                new();

        private readonly WireFactory
            factory =
                new();

        public IReadOnlyList<Wire>
            Wires => graph.Wires;

        public Wire CreateWire()
        {
            Wire wire =
                factory.Create();

            graph.Wires.Add(wire);

            return wire;
        }

        public void RemoveWire(
            Wire wire)
        {
            graph.Wires.Remove(wire);
        }
    }
}
