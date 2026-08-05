using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WirePath
    {
        public readonly List<WireCorner>
            Corners = new();

        public float Length;
    }
}
