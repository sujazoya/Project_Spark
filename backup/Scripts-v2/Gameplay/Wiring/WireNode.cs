using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireNode
    {
        public Vector3 Position;

        public readonly List<WireConnection>
            Connections =
                new();
    }
}
