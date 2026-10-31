using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    [Serializable]
    public sealed class SparkVisualStroke
    {
        public Vector2[] points;
        public Color color = Color.cyan;
        public float width = 4f;
        public float progress = 1f;
        public bool arrow;
        public bool closed;
    }

    // One efficient UI Graphic for all lines, rings, arrows, wires and symbols.
    // Uses Unity's CanvasRenderer (no textures, shaders or LineRenderer required).
    [RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasRenderer))]
public sealed class SparkTeachingVectorGraphic : MaskableGraphic
    {
        private readonly List<SparkVisualStroke> strokes = new List<SparkVisualStroke>();
        public override Texture mainTexture => Texture2D.whiteTexture;

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
        }

        public void SetStrokes(List<SparkVisualStroke> source)
        {
            strokes.Clear();
            if (source != null) strokes.AddRange(source);
            SetVerticesDirty();
        }

        public void Clear()
        {
            strokes.Clear();
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            foreach (SparkVisualStroke s in strokes)
            {
                if (s == null || s.points == null || s.points.Length < 2) continue;
                float fraction = Mathf.Clamp01(s.progress);
                int segmentCount = s.points.Length - 1 + (s.closed ? 1 : 0);
                float drawSegments = segmentCount * fraction;
                int complete = Mathf.FloorToInt(drawSegments);
                for (int i = 0; i < segmentCount; i++)
                {
                    float f = i < complete ? 1f : Mathf.Clamp01(drawSegments - i);
                    if (f <= .0001f) break;
                    Vector2 a = s.points[i % s.points.Length];
                    Vector2 b = Vector2.Lerp(a, s.points[(i + 1) % s.points.Length], f);
                    AddSegment(vh, a, b, Mathf.Max(1, s.width), s.color);
                    if (f < 1f) break;
                }
                if (s.arrow && fraction >= .999f)
                {
                    Vector2 end = s.points[s.points.Length - 1];
                    Vector2 d = end - s.points[s.points.Length - 2];
                    if (d.sqrMagnitude > .01f)
                    {
                        d.Normalize();
                        Vector2 back = -d * Mathf.Max(14, s.width * 3.5f);
                        float r = .48f;
                        Vector2 left = new Vector2(back.x * r - back.y * (1-r), back.y * r + back.x * (1-r));
                        Vector2 right = new Vector2(back.x * r + back.y * (1-r), back.y * r - back.x * (1-r));
                        AddSegment(vh, end, end + left, s.width, s.color);
                        AddSegment(vh, end, end + right, s.width, s.color);
                    }
                }
            }
        }

        private static void AddSegment(VertexHelper vh, Vector2 a, Vector2 b, float width, Color color)
        {
            Vector2 delta = b - a;
            if (delta.sqrMagnitude < .0001f) return;
            Vector2 n = new Vector2(-delta.y, delta.x).normalized * (width * .5f);
            int index = vh.currentVertCount;
            Add(vh, a - n, color);
            Add(vh, a + n, color);
            Add(vh, b + n, color);
            Add(vh, b - n, color);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }

        private static void Add(VertexHelper vh, Vector2 point, Color color)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = point;
            vertex.color = color;
            vertex.uv0 = Vector2.zero;
            vh.AddVert(vertex);
        }
    }
}
