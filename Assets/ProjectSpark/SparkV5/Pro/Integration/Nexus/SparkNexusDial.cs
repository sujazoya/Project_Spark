using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.SparkV5.Pro
{
    // Based on the segmented-ring graphic in the supplied Nexus source kit.
    [RequireComponent(typeof(CanvasRenderer))]
    internal sealed class SparkNexusDial : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            float radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.47f;
            Vector2 center = rectTransform.rect.center;
            for (int ring = 0; ring < 3; ring++)
            {
                float size = radius * (1f - 0.17f * ring);
                for (int segment = 0; segment < 72; segment++)
                {
                    if (ring != 0 && segment % 12 > (ring == 1 ? 8 : 3)) continue;
                    var a = Direction(segment * 5f);
                    var b = Direction((segment + 1) * 5f);
                    Stroke(mesh, center + a * size, center + b * size, ring == 1 ? 1.7f : 0.8f);
                }
            }
            for (int i = 0; i < 48; i++)
            {
                Vector2 direction = Direction(i * 7.5f);
                Stroke(mesh, center + direction * radius * 0.93f, center + direction * radius * (i % 4 == 0 ? 1.05f : 1f), 1f);
            }
        }

        private static Vector2 Direction(float degrees) =>
            new(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad));

        private void Stroke(VertexHelper mesh, Vector2 a, Vector2 b, float width)
        {
            Vector2 direction = (b - a).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * width * 0.5f;
            int offset = mesh.currentVertCount;
            mesh.AddVert(a - normal, color, Vector2.zero);
            mesh.AddVert(a + normal, color, Vector2.zero);
            mesh.AddVert(b + normal, color, Vector2.zero);
            mesh.AddVert(b - normal, color, Vector2.zero);
            mesh.AddTriangle(offset, offset + 1, offset + 2);
            mesh.AddTriangle(offset, offset + 2, offset + 3);
        }
    }
}
