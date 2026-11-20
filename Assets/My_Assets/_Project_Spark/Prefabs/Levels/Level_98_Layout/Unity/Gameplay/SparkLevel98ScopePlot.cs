using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SparkLevel98ScopePlot : MaskableGraphic
    {
        private readonly Vector2[] samples = new Vector2[120];
        private int count;

        public void Clear()
        {
            count = 0;
            SetVerticesDirty();
        }

        public void Add(float voltage, float current)
        {
            if (count == samples.Length) return;
            samples[count++] = new Vector2(voltage, current);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect bounds = GetPixelAdjustedRect();
            Color background = new(.012f, .035f, .047f);
            Quad(mesh, new Vector2(bounds.xMin, bounds.yMin), new Vector2(bounds.xMax, bounds.yMin),
                new Vector2(bounds.xMax, bounds.yMax), new Vector2(bounds.xMin, bounds.yMax), background);
            Color grid = new(.10f, .24f, .29f);
            for (int i = 1; i < 8; i++)
            {
                float x = bounds.xMin + i * bounds.width / 8f;
                Line(mesh, new Vector2(x, bounds.yMin), new Vector2(x, bounds.yMax), grid, .55f);
            }
            Line(mesh, new Vector2(bounds.xMin, bounds.center.y), new Vector2(bounds.xMax, bounds.center.y), grid, .55f);
            Vector2 Point(int i, bool voltage)
            {
                Vector2 sample = samples[i];
                float normalized = voltage ? Mathf.InverseLerp(1.2f, 2f, sample.x) : Mathf.InverseLerp(0, 3.4f, sample.y);
                return new Vector2(bounds.xMin + 2 + (bounds.width - 4) * i / (samples.Length - 1f),
                    bounds.yMin + bounds.height * ((voltage ? .53f : .04f) + normalized * .41f));
            }
            for (int i = 1; i < count; i++)
            {
                Line(mesh, Point(i - 1, true), Point(i, true), new Color(.35f, .94f, .86f), 1.45f);
                Line(mesh, Point(i - 1, false), Point(i, false), new Color(1f, .69f, .31f), 1.45f);
            }
        }

        private static void Line(VertexHelper mesh, Vector2 a, Vector2 b, Color color, float thickness)
        {
            Vector2 edge = b - a;
            Vector2 normal = new Vector2(-edge.y, edge.x).normalized * (thickness * .5f);
            Quad(mesh, a - normal, b - normal, b + normal, a + normal, color);
        }

        private static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(a, color, Vector2.zero);
            mesh.AddVert(b, color, Vector2.zero);
            mesh.AddVert(c, color, Vector2.zero);
            mesh.AddVert(d, color, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
            mesh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
