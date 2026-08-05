using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireBuilder
    {
        public Wire Build(
            WirePin start,
            WirePin end)
        {
            Wire wire = new();

            WireNode a =
                new()
                {
                    Position =
                        start.WorldPosition
                };

            WireNode b =
                new()
                {
                    Position =
                        end.WorldPosition
                };

            WireConnection connection =
                new()
                {
                    A = a,
                    B = b
                };

            a.Connections.Add(connection);

            b.Connections.Add(connection);

            wire.Nodes.Add(a);

            wire.Nodes.Add(b);

            wire.Connections.Add(connection);

            return wire;
        }
    }
}
