using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireGraph
    {
        public readonly List<Wire>
            Wires =
                new();

        public void Clear()
        {
            Wires.Clear();
        }
    }
}
