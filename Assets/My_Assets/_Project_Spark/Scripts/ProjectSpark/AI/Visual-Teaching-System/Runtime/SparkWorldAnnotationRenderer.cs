using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    [DisallowMultipleComponent]
    public sealed class SparkWorldAnnotationRenderer : MonoBehaviour
    {
        [SerializeField] private RectTransform drawArea;
        [SerializeField] private SparkTeachingVectorGraphic vectorGraphic;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private Canvas canvas;
        [SerializeField, Range(8, 80)] private int maxVisibleAnnotations = 40;

        public sealed class Annotation
        {
            internal SparkVisualCommand command;
            internal TextMeshProUGUI label;
            internal Image badge;
            internal float progress;
            internal LineRenderer worldLine;
        }

        private readonly List<Annotation> items = new List<Annotation>();
        private readonly List<SparkVisualStroke> strokes = new List<SparkVisualStroke>();
        private Material worldMaterial;
        public int WorldGeometryCount => items.FindAll(item => item.worldLine && item.worldLine.enabled).Count;

        private void Awake()
        {
            if (drawArea == null) drawArea = transform as RectTransform;
            if (vectorGraphic == null) vectorGraphic = GetComponent<SparkTeachingVectorGraphic>();
            if (canvas == null) canvas = GetComponentInParent<Canvas>();
            if (sceneCamera == null) sceneCamera = Camera.main;
        }

        public Annotation Add(SparkVisualCommand command)
        {
            if (command == null || items.Count >= maxVisibleAnnotations) return null;
            Annotation item = new Annotation { command = command, progress = 0f };
            if (command.worldGeometry && command.shape == SparkVisualShape.Ring)
            {
                if (!worldMaterial)
                {
                    var shader = Shader.Find("Sprites/Default");
                    if (shader) worldMaterial = new Material(shader);
                }
                if (worldMaterial)
                {
                    var node = new GameObject("Spark 3D teaching ring", typeof(LineRenderer));
                    node.transform.SetParent(transform, false);
                    item.worldLine = node.GetComponent<LineRenderer>();
                    item.worldLine.sharedMaterial = worldMaterial;
                    item.worldLine.startColor = item.worldLine.endColor = command.color;
                    item.worldLine.widthMultiplier = .035f;
                    item.worldLine.positionCount = 33;
                    item.worldLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    item.worldLine.receiveShadows = false;
                }
            }
            if (command.shape == SparkVisualShape.Label)
            {
                GameObject labelGo = new GameObject("AI Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelGo.transform.SetParent(drawArea, false);
                RectTransform rect = labelGo.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
                rect.sizeDelta = new Vector2(360f, 110f);
                item.label = labelGo.GetComponent<TextMeshProUGUI>();
                item.label.text = command.text;
                item.label.fontSize = command.fontSize;
                item.label.enableWordWrapping = true;
                item.label.alignment = TextAlignmentOptions.Center;
                item.label.raycastTarget = false;
                if (command.from.liveTarget)
                {
                    var badgeObject = new GameObject("Workbench teaching callout", typeof(RectTransform), typeof(Image));
                    item.badge = badgeObject.GetComponent<Image>();
                    item.badge.raycastTarget = false;
                    RectTransform badge = item.badge.rectTransform;
                    badge.SetParent(drawArea, false);
                    badge.anchorMin = badge.anchorMax = badge.pivot = new Vector2(.5f, .5f);
                    badge.sizeDelta = new Vector2(320, 62);
                    rect.SetParent(badge, false);
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = new Vector2(12, 5);
                    rect.offsetMax = new Vector2(-12, -5);
                    item.label.fontStyle = FontStyles.Bold;
                    item.label.enableAutoSizing = true;
                    item.label.fontSizeMin = 13;
                    item.label.fontSizeMax = command.fontSize;
                    item.label.overflowMode = TextOverflowModes.Ellipsis;
                }
            }
            items.Add(item);
            Redraw();
            return item;
        }

        public void SetProgress(Annotation annotation, float progress)
        {
            if (annotation == null) return;
            annotation.progress = Mathf.Clamp01(progress);
            Redraw();
        }

        public void Remove(Annotation annotation)
        {
            if (annotation == null) return;
            items.Remove(annotation);
            if (annotation.badge) Destroy(annotation.badge.gameObject);
            else if (annotation.label) Destroy(annotation.label.gameObject);
            if (annotation.worldLine) Destroy(annotation.worldLine.gameObject);
            Redraw();
        }

        public void ClearAll()
        {
            foreach (var item in items)
            {
                if (item.badge) Destroy(item.badge.gameObject);
                else if (item.label) Destroy(item.label.gameObject);
                if (item.worldLine) Destroy(item.worldLine.gameObject);
            }
            items.Clear();
            strokes.Clear();
            if (vectorGraphic != null) vectorGraphic.Clear();
        }

        private void LateUpdate()
        {
            // Keep labels/arrows locked to world objects as they or the camera move.
            if (items.Count > 0) Redraw();
        }

        private void OnDestroy()
        {
            if (worldMaterial) Destroy(worldMaterial);
        }

        private void Redraw()
        {
            if (drawArea == null || vectorGraphic == null) return;
            strokes.Clear();
            foreach (Annotation a in items)
            {
                SparkVisualCommand c = a.command;
                if (c == null) continue;
                if (a.worldLine)
                {
                    bool valid = TryWorldPosition(c.from, out var anchor);
                    a.worldLine.enabled = valid && a.progress > 0;
                    if (a.worldLine.enabled)
                        for (int i = 0; i < a.worldLine.positionCount; i++)
                        {
                            float angle = 2 * Mathf.PI * i * a.progress / (a.worldLine.positionCount - 1);
                            a.worldLine.SetPosition(i, anchor + new Vector3(.32f * Mathf.Cos(angle), .12f, .32f * Mathf.Sin(angle)));
                        }
                    continue;
                }
                bool good = TryPoint(c.from, out Vector2 start);
                bool endGood = TryPoint(c.to, out Vector2 end);
                if (a.label != null)
                {
                    if (a.badge)
                    {
                        a.badge.gameObject.SetActive(good);
                        if (!good) continue;
                        var rect = a.badge.rectTransform;
                        Vector2 half = rect.sizeDelta * .5f;
                        Rect area = drawArea.rect;
                        start.x = Mathf.Clamp(start.x, area.xMin + half.x + 12, area.xMax - half.x - 12);
                        start.y = Mathf.Clamp(start.y, area.yMin + half.y + 12, area.yMax - half.y - 12);
                        rect.anchoredPosition = start;
                        a.badge.color = new Color(.02f, .09f, .14f, .96f * a.progress);
                        a.label.color = new Color(.80f, .96f, 1, a.progress);
                        if (endGood) Stroke(new[] { end, start - new Vector2(0, half.y) }, c, a.progress, false, false);
                    }
                    else
                    {
                        a.label.gameObject.SetActive(good);
                        if (!good) continue;
                        a.label.rectTransform.anchoredPosition = start;
                        Color labelColor = c.color;
                        labelColor.a *= a.progress;
                        a.label.color = labelColor;
                    }
                    continue;
                }
                if (!good || !endGood) continue; // Never point at a guessed location.
                if (c.shape == SparkVisualShape.Line || c.shape == SparkVisualShape.Arrow)
                    Stroke(new [] { start, end }, c, a.progress, false, c.shape == SparkVisualShape.Arrow);
                else if (c.shape == SparkVisualShape.Rectangle)
                {
                    Vector2 half = Vector2.Scale(c.normalizedSize, drawArea.rect.size) * .5f;
                    Stroke(new [] {
                        start + new Vector2(-half.x, -half.y),
                        start + new Vector2(-half.x, half.y),
                        start + new Vector2(half.x, half.y),
                        start + new Vector2(half.x, -half.y)
                    }, c, a.progress, true, false);
                }
                else if (c.shape == SparkVisualShape.Ring)
                {
                    Vector2 half = Vector2.Scale(c.normalizedSize, drawArea.rect.size) * .5f;
                    Vector2[] points = new Vector2[48];
                    for (int i = 0; i < points.Length; i++)
                    {
                        float theta = i * Mathf.PI * 2f / points.Length;
                        points[i] = start + new Vector2(Mathf.Cos(theta) * half.x, Mathf.Sin(theta) * half.y);
                    }
                    Stroke(points, c, a.progress, true, false);
                }
            }
            vectorGraphic.SetStrokes(strokes);
        }

        private void Stroke(Vector2[] points, SparkVisualCommand c, float progress, bool closed, bool arrow)
        {
            Color glow = c.color;
            glow.a *= .17f;
            strokes.Add(new SparkVisualStroke {
                points = points, color = glow, width = c.thickness + 8f,
                progress = progress, closed = closed, arrow = arrow
            });
            strokes.Add(new SparkVisualStroke {
                points = points, color = c.color, width = c.thickness,
                progress = progress, closed = closed, arrow = arrow
            });
        }

        private bool TryPoint(SparkVisualPoint point, out Vector2 local)
        {
            local = Vector2.zero;
            if (point == null || drawArea == null) return false;
            if (point.space == SparkPointSpace.NormalizedScreen)
            {
                local = new Vector2(drawArea.rect.xMin + drawArea.rect.width * point.normalized.x,
                                    drawArea.rect.yMin + drawArea.rect.height * point.normalized.y) + point.pixelOffset;
                return true;
            }
            if (!TryWorldPosition(point, out Vector3 position)) return false;
            if (sceneCamera == null) sceneCamera = Camera.main;
            if (sceneCamera == null) return false;
            Vector3 view = sceneCamera.WorldToViewportPoint(position);
            if (view.z <= 0f || view.x < 0f || view.x > 1f || view.y < 0f || view.y > 1f) return false;
            Vector2 screen = sceneCamera.WorldToScreenPoint(position);
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? (canvas.worldCamera != null ? canvas.worldCamera : sceneCamera) : null;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(drawArea, screen, uiCamera, out local))
                return false;
            local += point.pixelOffset;
            return true;
        }

        private static bool TryWorldPosition(SparkVisualPoint point, out Vector3 position)
        {
            position = default;
            if (point == null) return false;
            if (point.liveTarget && point.liveTarget.gameObject.activeInHierarchy)
            {
                position = point.liveTarget.position;
                return true;
            }
            if (point.liveTarget != null || point.anchorId == "loaded-target") return false;
            if (!SparkTeachingAnchor.TryFind(point.anchorId, out var anchor)) return false;
            position = anchor.WorldPosition;
            return true;
        }
    }
}
