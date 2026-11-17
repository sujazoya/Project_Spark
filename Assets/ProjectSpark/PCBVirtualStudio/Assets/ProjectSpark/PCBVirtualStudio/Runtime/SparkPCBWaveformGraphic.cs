using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.PCBVirtualStudio
{
    // A procedural multi-channel UGUI display. This is VISUAL DEMO telemetry until an authority is wired.
    [AddComponentMenu("Project Spark/PCB/Procedural Signal Graph")]
    public sealed class SparkPCBWaveformGraphic : MaskableGraphic
    {
        [SerializeField] private float frequency = 4f;
        [SerializeField, Range(.05f, .95f)] private float duty = .42f;
        [SerializeField] private bool animate = true;
        [SerializeField] private Color gridColor = new Color(.17f, .40f, .51f, .28f);
        [SerializeField] private Color traceA = new Color(.10f, .90f, .98f, 1f);
        [SerializeField] private Color traceB = new Color(1f, .69f, .26f, 1f);
        [SerializeField] private Color traceC = new Color(.76f, .43f, 1f, 1f);
        private float phase;
        private float nextRefresh;

        public void SetDemoSignal(float hz, float dutyFraction)
        {
            frequency = Mathf.Clamp(hz / 64f, 1f, 9f);
            duty = Mathf.Clamp(dutyFraction, .05f, .95f);
            SetVerticesDirty();
        }

        private void Update()
        {
            if (!animate || Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + .05f;
            phase += Time.unscaledDeltaTime * .15f;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            for (int x = 0; x <= 12; x++)
                DrawLine(vh, new Vector2(r.xMin + r.width * x / 12f, r.yMin),
                    new Vector2(r.xMin + r.width * x / 12f, r.yMax), 1f, gridColor);
            for (int y = 0; y <= 6; y++)
                DrawLine(vh, new Vector2(r.xMin, r.yMin + r.height * y / 6f),
                    new Vector2(r.xMax, r.yMin + r.height * y / 6f), 1f, gridColor);

            int segments = 140;
            Vector2 a = Vector2.zero;
            for (int ch = 0; ch < 3; ch++)
            {
                Color c = ch == 0 ? traceA : ch == 1 ? traceB : traceC;
                for (int i = 0; i <= segments; i++)
                {
                    float t = i / (float)segments;
                    float cyc = Mathf.Repeat((t + phase) * frequency, 1f);
                    float y;
                    if (ch == 0) y = cyc < duty ? .81f : .63f;
                    else if (ch == 1) y = .40f + .13f * Mathf.Sin(t * 28f + phase * 10f);
                    else y = .20f + .06f * Mathf.Sin(t * 56f + phase * 8f) + .025f * Mathf.Sin(t * 151f);
                    Vector2 b = new Vector2(r.xMin + t * r.width, r.yMin + y * r.height);
                    if (i > 0) DrawLine(vh, a, b, ch == 0 ? 2.7f : 2f, c);
                    a = b;
                }
            }
        }

        private static void DrawLine(VertexHelper vh, Vector2 a, Vector2 b, float thickness, Color color)
        {
            Vector2 d = (b - a).normalized;
            Vector2 n = new Vector2(-d.y, d.x) * thickness * .5f;
            int index = vh.currentVertCount;
            UIVertex v = UIVertex.simpleVert;
            v.color = color;
            v.position = a - n; vh.AddVert(v);
            v.position = a + n; vh.AddVert(v);
            v.position = b + n; vh.AddVert(v);
            v.position = b - n; vh.AddVert(v);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
