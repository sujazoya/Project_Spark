using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay.SiliconWorkshop
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SparkL129Trace : MaskableGraphic
    {
        private const int Capacity = 24;
        private readonly Vector3[] record = new Vector3[Capacity];
        private int count;

        public void Clear()
        {
            count = 0;
            gameObject.SetActive(true);
            SetVerticesDirty();
        }

        public void Append(float temperature, int rpm, bool loaded)
        {
            if (count >= Capacity) return;
            record[count++] = new Vector3(Mathf.InverseLerp(25f, 62f, temperature), Mathf.Clamp01(rpm / 1500f), loaded ? 1f : 0f);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertices)
        {
            vertices.Clear();
            var rect = GetPixelAdjustedRect();
            Fill(vertices, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMax), new Color(.025f, .075f, .095f));
            float bottom = rect.yMin + 9f, top = rect.yMax - 29f;
            for (int i = 0; i <= 3; i++)
            {
                float y = Mathf.Lerp(bottom, top, i / 3f);
                Fill(vertices, new Vector2(8f, y), new Vector2(rect.xMax - 8f, y + 1f), new Color(.095f, .18f, .23f));
            }
            Vector2 Point(int index, float value) => new(Mathf.Lerp(8f, rect.xMax - 8f, index / (Capacity - 1f)), Mathf.Lerp(bottom, top, value));
            for (int i = 1; i < count; i++)
            {
                if (record[i].z > 0f)
                    Fill(vertices, Point(i - 1, 0f), Point(i, 1f), new Color(.54f, .24f, .08f, .17f));
                Line(vertices, Point(i - 1, record[i - 1].x), Point(i, record[i].x), new Color(1f, .71f, .32f));
                Line(vertices, Point(i - 1, record[i - 1].y), Point(i, record[i].y), new Color(.36f, .92f, .77f));
            }
        }

        private static void Fill(VertexHelper vertices, Vector2 a, Vector2 b, Color color)
        {
            Quad(vertices, a, new Vector2(b.x, a.y), b, new Vector2(a.x, b.y), color);
        }

        private static void Line(VertexHelper vertices, Vector2 a, Vector2 b, Color color)
        {
            var direction = (b - a).normalized;
            var normal = new Vector2(-direction.y, direction.x) * 1.1f;
            Quad(vertices, a - normal, b - normal, b + normal, a + normal, color);
        }

        private static void Quad(VertexHelper vertices, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
        {
            int first = vertices.currentVertCount;
            vertices.AddVert(a, color, Vector2.zero);
            vertices.AddVert(b, color, Vector2.zero);
            vertices.AddVert(c, color, Vector2.zero);
            vertices.AddVert(d, color, Vector2.zero);
            vertices.AddTriangle(first, first + 1, first + 2);
            vertices.AddTriangle(first, first + 2, first + 3);
        }
    }
}
