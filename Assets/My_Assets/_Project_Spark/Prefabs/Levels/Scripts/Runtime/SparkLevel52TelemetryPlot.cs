using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel52TelemetryPlot : MaskableGraphic
    {
        public SparkLevel52Balance controller;
        private float refreshAt;

        private void Update()
        {
            if (Time.unscaledTime < refreshAt) return;
            refreshAt = Time.unscaledTime + .067f;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (controller == null) return;
            Rect area = rectTransform.rect;
            var grid = new Color32(46, 81, 104, 255);
            var waveform = new Color32(64, 234, 232, 255);
            Vector2 start = new(area.xMin + 12, area.center.y);
            float span = area.width * .65f;
            Line(mesh, start, start + Vector2.right * span, 1, grid);
            int samples = controller.SampleCount;
            for (int i = 1; i < samples; i += 2)
            {
                Vector2 Point(int sample) => start + new Vector2(span * sample / 239f,
                    Mathf.Clamp(controller.WaveSample(sample) / 2.6f, -1, 1) * (area.height * .40f));
                Line(mesh, Point(i-1), Point(Mathf.Min(i+1, samples-1)), 1.65f, waveform);
            }
            Vector2 center = new(area.xMin + area.width * .83f, area.center.y);
            float radius = area.height * .39f;
            for (int i = 0; i < 32; i++)
            {
                Vector2 Point(int step)
                {
                    float a = step * 2 * Mathf.PI / 32;
                    return center + radius * new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                }
                Line(mesh, Point(i), Point(i+1), 1, grid);
            }
            Line(mesh, center - Vector2.right * radius, center + Vector2.right * radius, 1, grid);
            Line(mesh, center - Vector2.up * radius, center + Vector2.up * radius, 1, grid);
            Vector2 Scaled(Vector2 v,float fullScale) => center + Vector2.ClampMagnitude(v/fullScale,1)*radius;
            Line(mesh, center, Scaled(controller.BaselineVector,2.4f), 2.5f, new Color32(251,105,123,255));
            Line(mesh, center, Scaled(controller.CorrectionA,1.24f), 2.5f, waveform);
            Line(mesh, center, Scaled(controller.CorrectionB,1.24f), 2.5f, new Color32(255,193,69,255));
            Line(mesh, center, Scaled(controller.MeasuredVector,2.4f), 3, new Color32(102,255,148,255));
        }

        private static void Line(VertexHelper mesh,Vector2 a,Vector2 b,float width,Color32 color)
        {
            if ((a-b).sqrMagnitude < .001f) return;
            Vector2 normal = Vector2.Perpendicular((b-a).normalized)*width*.5f;
            int i = mesh.currentVertCount;
            mesh.AddVert(a+normal,color,Vector2.zero);
            mesh.AddVert(b+normal,color,Vector2.zero);
            mesh.AddVert(b-normal,color,Vector2.zero);
            mesh.AddVert(a-normal,color,Vector2.zero);
            mesh.AddTriangle(i,i+1,i+2);
            mesh.AddTriangle(i,i+2,i+3);
        }
    }
}
