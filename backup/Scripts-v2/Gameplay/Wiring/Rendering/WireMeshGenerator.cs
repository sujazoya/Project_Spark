using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring.Rendering
{
    public sealed class WireMeshGenerator
    {
        public Mesh Generate(
            WireSpline spline)
        {
            WireMeshData data =
                new WireMeshData();

            // TODO
            // Generate cylindrical mesh
            // Extrude along spline
            // Create UVs
            // Generate caps

            return new WireMeshBuilder()
                .Build(data);
        }
    }
}
