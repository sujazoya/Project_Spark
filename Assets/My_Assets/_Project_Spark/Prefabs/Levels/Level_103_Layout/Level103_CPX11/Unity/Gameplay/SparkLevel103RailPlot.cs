using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay.PowerCore
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SparkLevel103RailPlot : MaskableGraphic
    {
        private float[] trace;
        private int count;

        internal void SetTrace(float[] values, int length)
        {
            if (ReferenceEquals(trace, values) && count == length) return;
            trace = values;
            count = length;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r = GetPixelAdjustedRect();
            Quad(mesh, new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin),
                new Vector2(r.xMax, r.yMax), new Vector2(r.xMin, r.yMax), new Color(.013f, .036f, .05f));
            var grid = new Color(.22f, .35f, .42f);
            for (int i = 0; i <= 4; i++)
            {
                float x = Mathf.Lerp(r.xMin, r.xMax, i / 4f), y = Mathf.Lerp(r.yMin, r.yMax, i / 4f);
                Line(mesh, new Vector2(x, r.yMin), new Vector2(x, r.yMax), .38f, grid);
                Line(mesh, new Vector2(r.xMin, y), new Vector2(r.xMax, y), .38f, grid);
            }
            if (count < 2) return;
            var ink = new Color(1f, .73f, .37f);
            Vector2 At(int i) => new Vector2(Mathf.Lerp(r.xMin, r.xMax, i / (float)(count - 1)),
                Mathf.Lerp(r.yMin, r.yMax, Mathf.InverseLerp(.68f, 1.24f, trace[i])));
            for (int i = 1; i < count; i++) Line(mesh, At(i - 1), At(i), .85f, ink);
        }

        private static void Line(VertexHelper mesh, Vector2 a, Vector2 b, float width, Color color)
        {
            Vector2 d = b - a;
            Vector2 normal = new Vector2(-d.y, d.x).normalized * width;
            Quad(mesh, a - normal, b - normal, b + normal, a + normal, color);
        }

        private static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
        {
            int i = mesh.currentVertCount;
            mesh.AddVert(a, color, Vector2.zero);
            mesh.AddVert(b, color, Vector2.zero);
            mesh.AddVert(c, color, Vector2.zero);
            mesh.AddVert(d, color, Vector2.zero);
            mesh.AddTriangle(i, i + 1, i + 2);
            mesh.AddTriangle(i, i + 2, i + 3);
        }
    }
}
