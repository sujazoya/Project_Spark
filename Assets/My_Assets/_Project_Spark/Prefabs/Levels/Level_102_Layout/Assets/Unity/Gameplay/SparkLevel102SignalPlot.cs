using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay.SignalPhantom
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SparkLevel102SignalPlot : MaskableGraphic
    {
        private SparkLevel102Channel.Capture capture;
        private bool eye;

        internal void SetCapture(SparkLevel102Channel.Capture value, bool showEye)
        {
            if (ReferenceEquals(capture, value) && eye == showEye) return;
            capture = value;
            eye = showEye;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertices)
        {
            vertices.Clear();
            Rect rect = GetPixelAdjustedRect();
            Quad(vertices, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin),
                new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMin, rect.yMax), new Color(.016f, .043f, .063f));
            var baseline = new Color(.23f, .37f, .48f, .95f);
            for (int i = 0; i <= 4; i++)
            {
                float x = Mathf.Lerp(rect.xMin, rect.xMax, i / 4f);
                float y = Mathf.Lerp(rect.yMin, rect.yMax, i / 4f);
                Line(vertices, new Vector2(x, rect.yMin), new Vector2(x, rect.yMax), .45f, baseline);
                Line(vertices, new Vector2(rect.xMin, y), new Vector2(rect.xMax, y), .45f, baseline);
            }
            if (capture == null) return;
            if (eye)
            {
                var ink = new Color(.42f, .78f, 1f, .26f);
                for (int row = 0; row < SparkLevel102Channel.EyeTraces; row++)
                    Trace(vertices, rect, capture.eye, row * SparkLevel102Channel.Phases, SparkLevel102Channel.Phases, .49f, ink);
            }
            else
            {
                Trace(vertices, rect, capture.tdr, 0, capture.tdr.Length, .35f, new Color(.46f, .88f, 1f, 1f));
            }
        }

        private static void Trace(VertexHelper target, Rect rect, float[] source, int start, int count, float range, Color ink)
        {
            Vector2 prev = new Vector2(rect.xMin, rect.center.y + source[start] * rect.height * .45f / range);
            for (int i = 1; i < count; i++)
            {
                Vector2 point = new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, i / (float)(count - 1)),
                    rect.center.y + Mathf.Clamp(source[start + i] / range, -1, 1) * rect.height * .45f);
                Line(target, prev, point, .70f, ink);
                prev = point;
            }
        }

        private static void Line(VertexHelper vertices, Vector2 a, Vector2 b, float width, Color color)
        {
            Vector2 d = b - a;
            Vector2 normal = new Vector2(-d.y, d.x).normalized * width;
            Quad(vertices, a - normal, b - normal, b + normal, a + normal, color);
        }

        private static void Quad(VertexHelper vertices, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
        {
            int start = vertices.currentVertCount;
            vertices.AddVert(a, color, Vector2.zero);
            vertices.AddVert(b, color, Vector2.zero);
            vertices.AddVert(c, color, Vector2.zero);
            vertices.AddVert(d, color, Vector2.zero);
            vertices.AddTriangle(start, start + 1, start + 2);
            vertices.AddTriangle(start, start + 2, start + 3);
        }
    }
}
