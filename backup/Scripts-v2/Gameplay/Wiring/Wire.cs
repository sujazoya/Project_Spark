using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class Wire
    {
        public readonly List<WireNode>
            Nodes =
                new();

        public readonly List<WireConnection>
            Connections =
                new();

        public bool Powered;
    }
}
