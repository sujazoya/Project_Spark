using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    [DisallowMultipleComponent]
    public sealed class SparkSchematicRenderer : MonoBehaviour, IScrollHandler, IDragHandler
    {
        [Header("Builder assigns these")]
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform diagramContent;
        [SerializeField] private SparkTeachingVectorGraphic lines;
        [SerializeField] private TMP_Text captionText;
        [Header("Interaction")]
        [SerializeField, Range(.5f, 4f)] private float zoom = 1f;
        [SerializeField, Range(1f, 50f)] private float zoomSpeed = 15f;
        [SerializeField] private bool allowPan = true;
        private SparkSchematicData data;
        private readonly List<SparkVisualStroke> graphics = new List<SparkVisualStroke>();
        private readonly List<GameObject> labels = new List<GameObject>();
        private Vector2 lastSize;

        private void Awake()
        {
            if (viewport == null) viewport = transform as RectTransform;
            if (diagramContent != null) diagramContent.localScale = Vector3.one * zoom;
        }

        public void Display(SparkSchematicData schematic)
        {
            data = schematic;
            Rebuild();
        }

        public void Clear()
        {
            data = null;
            Rebuild();
        }

        public void ZoomIn() => SetZoom(zoom + .2f);
        public void ZoomOut() => SetZoom(zoom - .2f);
        public void ResetView()
        {
            SetZoom(1f);
            if (diagramContent != null) diagramContent.anchoredPosition = Vector2.zero;
        }

        public void SetZoom(float value)
        {
            zoom = Mathf.Clamp(value, .65f, 2.5f);
            if (diagramContent != null) diagramContent.localScale = Vector3.one * zoom;
        }

        public void OnScroll(PointerEventData eventData)
        {
            SetZoom(zoom + Mathf.Sign(eventData.scrollDelta.y) * zoomSpeed * .01f);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!allowPan || diagramContent == null) return;
            float canvasScale = 1f;
            Canvas c = GetComponentInParent<Canvas>();
            if (c != null) canvasScale = Mathf.Max(.01f, c.scaleFactor);
            diagramContent.anchoredPosition += eventData.delta / canvasScale;
        }

        private void LateUpdate()
        {
            if (diagramContent != null && (diagramContent.rect.size - lastSize).sqrMagnitude > .01f)
                Rebuild();
        }

        public void Rebuild()
        {
            for (int i = labels.Count - 1; i >= 0; i--)
                if (labels[i] != null) Destroy(labels[i]);
            labels.Clear();
            graphics.Clear();
            if (lines == null || diagramContent == null) return;
            Vector2 size = diagramContent.rect.size;
            lastSize = size;
            if (data == null)
            {
                lines.Clear();
                if (captionText != null) captionText.text = "No verified schematic loaded";
                return;
            }
            if (captionText != null) captionText.text = data.caption;
            if (data.wires != null)
                foreach (SparkSchematicWire wire in data.wires)
                {
                    if (wire == null || wire.points == null || wire.points.Length < 2) continue;
                    Vector2[] pts = new Vector2[wire.points.Length];
                    for (int i = 0; i < pts.Length; i++) pts[i] = Local(wire.points[i], size);
                    Add(pts, wire.energized ? new Color(.98f, .72f, .23f, 1f) : wire.color, 3f);
                }
            if (data.parts != null)
                foreach (SparkSchematicPart part in data.parts)
                    if (part != null) DrawPart(part, size);
            lines.SetStrokes(graphics);
        }

        private static Vector2 Local(Vector2 normalized, Vector2 size) =>
            new Vector2((normalized.x - .5f) * size.x, (normalized.y - .5f) * size.y);

        private void Add(Vector2[] points, Color color, float width = 3f, bool closed = false)
        {
            Color glow = color;
            glow.a *= .14f;
            graphics.Add(new SparkVisualStroke { points = points, color = glow, width = width + 6f, closed = closed });
            graphics.Add(new SparkVisualStroke { points = points, color = color, width = width, closed = closed });
        }

        private void DrawPart(SparkSchematicPart part, Vector2 size)
        {
            Vector2 p = Local(part.position, size);
            Color c = part.color;
            const float w = 32f;
            switch (part.kind)
            {
                case SparkSchematicKind.Resistor:
                    Add(new [] { p + new Vector2(-w,0), p + new Vector2(-23,0), p + new Vector2(-17,11),
                        p + new Vector2(-9,-11), p + new Vector2(-1,11), p + new Vector2(7,-11),
                        p + new Vector2(15,11), p + new Vector2(23,0), p + new Vector2(w,0) }, c);
                    break;
                case SparkSchematicKind.Battery:
                    Add(new [] {p + new Vector2(-12,-22),p + new Vector2(-12,22)}, c, 4f);
                    Add(new [] {p + new Vector2(12,-12),p + new Vector2(12,12)}, c, 4f);
                    Add(new [] {p + new Vector2(-w,0),p + new Vector2(-12,0)}, c);
                    Add(new [] {p + new Vector2(12,0),p + new Vector2(w,0)}, c);
                    break;
                case SparkSchematicKind.Capacitor:
                    Add(new [] {p + new Vector2(-7,-18),p + new Vector2(-7,18)}, c, 4);
                    Add(new [] {p + new Vector2(7,-18),p + new Vector2(7,18)}, c, 4);
                    Add(new [] {p + new Vector2(-w,0),p + new Vector2(-7,0)}, c);
                    Add(new [] {p + new Vector2(7,0),p + new Vector2(w,0)}, c);
                    break;
                case SparkSchematicKind.LED:
                case SparkSchematicKind.Diode:
                    Add(new [] {p + new Vector2(-w,0),p + new Vector2(-14,0)}, c);
                    Add(new [] {p + new Vector2(14,0),p + new Vector2(w,0)}, c);
                    Add(new [] {p + new Vector2(-14,-13),p + new Vector2(-14,13),p + new Vector2(13,0)}, c, 3, true);
                    Add(new [] {p + new Vector2(14,-15),p + new Vector2(14,15)}, c, 4);
                    if (part.kind == SparkSchematicKind.LED)
                    {
                        Add(new [] {p + new Vector2(6,19),p + new Vector2(20,33)}, c, 2);
                        Add(new [] {p + new Vector2(17,19),p + new Vector2(31,33)}, c, 2);
                    }
                    break;
                case SparkSchematicKind.Switch:
                    Add(new [] {p + new Vector2(-w,0),p + new Vector2(-14,0)}, c);
                    Add(new [] {p + new Vector2(14,0),p + new Vector2(w,0)}, c);
                    Add(new [] {p + new Vector2(-14,0),p + new Vector2(12,17)}, c);
                    break;
                case SparkSchematicKind.Ground:
                    Add(new [] {p + new Vector2(0,18), p + new Vector2(0,0)}, c);
                    Add(new [] {p + new Vector2(-20,0),p + new Vector2(20,0)}, c);
                    Add(new [] {p + new Vector2(-13,-7),p + new Vector2(13,-7)}, c);
                    Add(new [] {p + new Vector2(-6,-14),p + new Vector2(6,-14)}, c);
                    break;
                case SparkSchematicKind.Meter:
                    // Compact LCD body with two sensing terminals, A and B.
                    Add(new [] {p + new Vector2(-34,-23),p + new Vector2(34,-23),
                        p + new Vector2(34,23),p + new Vector2(-34,23)}, c, 3, true);
                    Add(new [] {p + new Vector2(-26,-14),p + new Vector2(26,-14),
                        p + new Vector2(26,14),p + new Vector2(-26,14)}, c, 1.7f, true);
                    MakeLabel("LCD", p + new Vector2(0,0), c, 56, 13);
                    MakeLabel("A+", p + new Vector2(-48,10), c, 34, 12);
                    MakeLabel("B-", p + new Vector2(48,-10), c, 34, 12);
                    break;
                case SparkSchematicKind.Generic:
                    // Junction dots are small rather than large obstructive rings.
                    Vector2[] dot = new Vector2[20];
                    for (int i = 0; i < dot.Length; i++)
                    {
                        float a = i * Mathf.PI * 2f / dot.Length;
                        dot[i] = p + new Vector2(Mathf.Cos(a)*5f, Mathf.Sin(a)*5f);
                    }
                    Add(dot, c, 3f, true);
                    break;
                default:
                    Vector2[] ring = new Vector2[32];
                    for (int i = 0; i < ring.Length; i++)
                    {
                        float a = i * Mathf.PI * 2 / ring.Length;
                        ring[i] = p + new Vector2(Mathf.Cos(a) * 22, Mathf.Sin(a) * 22);
                    }
                    Add(ring, c, 3, true);
                    break;
            }
            if (!string.IsNullOrWhiteSpace(part.label))
            {
                float labelOffset = part.kind == SparkSchematicKind.Meter ? -47f : -40f;
                MakeLabel(part.label, p + new Vector2(0,labelOffset), c, 116, 14);
            }
        }

        private void MakeLabel(string content, Vector2 at, Color color, float width, int fontSize)
        {
            if (string.IsNullOrWhiteSpace(content)) return;
            GameObject go = new GameObject("Symbol Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(diagramContent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(.5f,.5f);
            rt.sizeDelta = new Vector2(width, 25f);
            rt.anchoredPosition = at;
            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.enableAutoSizing = true;
            text.fontSizeMin = 10;
            text.fontSizeMax = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;
            text.raycastTarget = false;
            labels.Add(go);
        }

    }
}
