using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.PCBVirtualStudio
{
    [AddComponentMenu("Project Spark/PCB/Rounded Panel Graphic")]
    public sealed class SparkPCBRoundedPanelGraphic : MaskableGraphic
    {
        [SerializeField, Range(0f, 60f)] private float cornerRadius = 15f;
        [SerializeField, Range(0f, 10f)] private float borderWidth = 1.2f;
        [SerializeField] private Color borderColor = new Color(0.12f, 0.68f, 0.86f, 0.55f);
        [SerializeField, Range(2, 14)] private int cornerSegments = 6;

        public void SetStyle(Color fill, Color edge, float radius = 15f, float border = 1.2f)
        {
            color = fill;
            borderColor = edge;
            cornerRadius = radius;
            borderWidth = border;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            if (r.width < 1 || r.height < 1) return;
            float radius = Mathf.Min(cornerRadius, r.width / 2f, r.height / 2f);
            List<Vector2> outer = BuildPerimeter(r, radius, cornerSegments);
            Rect innerRect = new Rect(r.xMin + borderWidth, r.yMin + borderWidth,
                Mathf.Max(0, r.width - borderWidth * 2), Mathf.Max(0, r.height - borderWidth * 2));
            List<Vector2> inner = borderWidth > .001f
                ? BuildPerimeter(innerRect, Mathf.Max(0f, radius - borderWidth), cornerSegments)
                : outer;
            int count = outer.Count;
            UIVertex v = UIVertex.simpleVert;
            v.color = color;
            v.position = r.center;
            v.uv0 = new Vector2(.5f, .5f);
            vh.AddVert(v);
            for (int i = 0; i < count; i++)
            {
                v.position = inner[i];
                v.uv0 = new Vector2((inner[i].x-r.xMin)/r.width, (inner[i].y-r.yMin)/r.height);
                v.color = color;
                vh.AddVert(v);
            }
            for (int i = 0; i < count; i++)
                vh.AddTriangle(0, i + 1, ((i + 1) % count) + 1);

            if (borderWidth < .001f) return;
            int innerStart = 1;
            int outerStart = 1 + count;
            for (int i = 0; i < count; i++)
            {
                v.position = outer[i];
                v.uv0 = new Vector2((outer[i].x-r.xMin)/r.width, (outer[i].y-r.yMin)/r.height);
                v.color = borderColor;
                vh.AddVert(v);
            }
            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                vh.AddTriangle(outerStart + i, outerStart + j, innerStart + j);
                vh.AddTriangle(outerStart + i, innerStart + j, innerStart + i);
            }
        }

        private static List<Vector2> BuildPerimeter(Rect r, float radius, int segments)
        {
            var pts = new List<Vector2>(segments * 4 + 4);
            Vector2[] centers = {
                new Vector2(r.xMax-radius, r.yMax-radius),
                new Vector2(r.xMin+radius, r.yMax-radius),
                new Vector2(r.xMin+radius, r.yMin+radius),
                new Vector2(r.xMax-radius, r.yMin+radius)
            };
            for (int corner = 0; corner < 4; corner++)
            {
                float start = corner * 90f;
                for (int i = 0; i <= segments; i++)
                {
                    float ang = (start + i * (90f / segments)) * Mathf.Deg2Rad;
                    pts.Add(centers[corner] + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radius);
                }
            }
            return pts;
        }
    }
}
