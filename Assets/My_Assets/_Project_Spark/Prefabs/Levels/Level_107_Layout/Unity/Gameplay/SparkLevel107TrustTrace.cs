using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SparkLevel107TrustTrace : MaskableGraphic
    {
        private readonly float[] samples = new float[72];
        private int next, count;
        private float due;

        public void Sample(float time, float voltage)
        {
            if (time < due) return;
            due = time + .028f;
            samples[next] = Mathf.Clamp01(voltage / 3.3f);
            next = (next + 1) % samples.Length;
            count = Mathf.Min(count + 1, samples.Length);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r = rectTransform.rect;
            var grid = new Color32(39, 70, 84, 255);
            var signal = new Color32(74, 229, 197, 255);
            void Line(Vector2 a, Vector2 b, Color32 color, float width)
            {
                Vector2 n = new Vector2(b.y - a.y, a.x - b.x).normalized * width;
                int i = mesh.currentVertCount;
                mesh.AddVert(a - n, color, Vector2.zero);
                mesh.AddVert(a + n, color, Vector2.zero);
                mesh.AddVert(b + n, color, Vector2.zero);
                mesh.AddVert(b - n, color, Vector2.zero);
                mesh.AddTriangle(i, i + 1, i + 2);
                mesh.AddTriangle(i, i + 2, i + 3);
            }
            for (int y = 0; y < 3; y++)
            {
                float height = r.yMin + (y + .5f) * r.height / 3f;
                Line(new Vector2(r.xMin, height), new Vector2(r.xMax, height), grid, .45f);
            }
            for (int i = 1; i < count; i++)
            {
                int before = (next + samples.Length - count + i - 1) % samples.Length;
                float x = r.xMin + (i - 1) * r.width / (samples.Length - 1);
                float x2 = r.xMin + i * r.width / (samples.Length - 1);
                float y = r.yMin + r.height * (.14f + samples[before] * .72f);
                float y2 = r.yMin + r.height * (.14f + samples[(before + 1) % samples.Length] * .72f);
                Line(new Vector2(x, y), new Vector2(x2, y), signal, .80f);
                if (Mathf.Abs(y2 - y) > .1f) Line(new Vector2(x2, y), new Vector2(x2, y2), signal, .80f);
            }
        }
    }
}
