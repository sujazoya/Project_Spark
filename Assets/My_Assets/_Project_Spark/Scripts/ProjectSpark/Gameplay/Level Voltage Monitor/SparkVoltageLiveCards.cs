using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Visual-only adapter. Observes the existing monitor's TMP text and displays
    /// the same live readings as reusable, colorful rows. Does not touch the solver.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVoltageLiveCards : MonoBehaviour
    {
        [SerializeField] private TMP_Text sourceReadings;
        [SerializeField] private ScrollRect readingsScroll;
        [SerializeField, Range(16, 256)] private int maximumRows = 180;
        [SerializeField] private float rowHeight = 40f;
        [SerializeField] private float rowSpacing = 5f;

        private RectTransform listRoot;
        private string lastText;
        private readonly List<Row> rows = new List<Row>();
        private static readonly Regex VoltageAtEnd = new Regex(
            @"(?<value>[+\-−]?\d+(?:[.,]\d+)?\s*(?:µV|uV|mV|kV|V))\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Color[] Colors = {
            Hex("#00AEDE"), Hex("#F39A22"), Hex("#23B85A"),
            Hex("#1779EF"), Hex("#D949A6"), Hex("#875DE7")
        };

        private class Row
        {
            public RectTransform rect;
            public Image background;
            public Image topHighlight;
            public Image edge;
            public TMP_Text label;
            public TMP_Text value;
        }

        public void Bind(TMP_Text source, ScrollRect scroll)
        {
            // Restore previous source if this component is deliberately rebound.
            if (sourceReadings != null && sourceReadings != source)
                sourceReadings.color = new Color(sourceReadings.color.r, sourceReadings.color.g, sourceReadings.color.b, 1f);
            sourceReadings = source;
            readingsScroll = scroll;
            lastText = null;
            if (Application.isPlaying) EnsureInitialized();
        }

        private void OnEnable() { lastText = null; if (Application.isPlaying) EnsureInitialized(); }

        private void LateUpdate()
        {
            if (!EnsureInitialized() || sourceReadings == null) return;
            string current = sourceReadings.text ?? string.Empty;
            if (current != lastText)
            {
                lastText = current;
                Render(current);
            }
            // The source is still updated by SparkLevelVoltageMonitor, but is not drawn.
            if (sourceReadings.color.a > 0.001f)
            {
                Color c = sourceReadings.color;
                c.a = 0f;
                sourceReadings.color = c;
            }
        }

        private bool EnsureInitialized()
        {
            if (sourceReadings == null || readingsScroll == null) return false;
            if (listRoot != null) return true;
            Transform old = readingsScroll.viewport != null
                ? readingsScroll.viewport.Find("PS_LiveCardList") : null;
            if (old != null) listRoot = old as RectTransform;
            else
            {
                GameObject root = new GameObject("PS_LiveCardList", typeof(RectTransform),
                    typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
                root.transform.SetParent(readingsScroll.viewport, false);
                listRoot = root.GetComponent<RectTransform>();
            }
            listRoot.anchorMin = new Vector2(0f, 1f);
            listRoot.anchorMax = new Vector2(1f, 1f);
            listRoot.pivot = new Vector2(.5f, 1f);
            listRoot.anchoredPosition = Vector2.zero;
            listRoot.sizeDelta = new Vector2(-14f, 0f);
            VerticalLayoutGroup layout = listRoot.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(5, 5, 8, 8);
            layout.spacing = rowSpacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            ContentSizeFitter fit = listRoot.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            readingsScroll.content = listRoot;
            readingsScroll.horizontal = false;
            readingsScroll.vertical = true;
            // Source stays active for the monitor's serialized reference and updates.
            if (sourceReadings != null) sourceReadings.raycastTarget = false;
            return true;
        }

        private void Render(string value)
        {
            string[] lines = value.Replace("\r", "").Split('\n');
            int used = 0;
            for (int i = 0; i < lines.Length && used < maximumRows; i++)
            {
                string raw = lines[i].Trim();
                if (string.IsNullOrEmpty(raw)) continue;
                Row row = GetRow(used);
                Match match = VoltageAtEnd.Match(StripTags(raw));
                if (match.Success)
                {
                    string plain = StripTags(raw);
                    string label = plain.Substring(0, match.Index).Trim(' ', '\t', ':', '|', '-', '—');
                    row.label.text = string.IsNullOrEmpty(label) ? "VOLTAGE" : label;
                    row.value.text = match.Groups["value"].Value;
                }
                else
                {
                    row.label.text = StripTags(raw);
                    row.value.text = "";
                }
                ApplyColor(row, used);
                row.rect.gameObject.SetActive(true);
                used++;
            }
            if (used == 0)
            {
                Row row = GetRow(0);
                row.label.text = "Waiting for live voltage data...";
                row.value.text = "";
                ApplyColor(row, 0);
                row.rect.gameObject.SetActive(true);
                used = 1;
            }
            for (int i = used; i < rows.Count; i++) rows[i].rect.gameObject.SetActive(false);
            if (listRoot != null) LayoutRebuilder.MarkLayoutForRebuild(listRoot);
        }

        private Row GetRow(int index)
        {
            if (index < rows.Count) return rows[index];
            var row = new Row();
            GameObject go = new GameObject("Voltage_Row_" + index.ToString("000"),
                typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(listRoot, false);
            row.rect = go.GetComponent<RectTransform>();
            row.background = go.GetComponent<Image>();
            row.background.raycastTarget = false;
            LayoutElement le = go.GetComponent<LayoutElement>();
            le.minHeight = rowHeight;
            le.preferredHeight = rowHeight;
            row.edge = Block("Edge", go.transform, Hex("#E7F8FF"), 4f, 0f, 0f, 0f);
            row.topHighlight = Block("Gloss", go.transform, new Color(1f, 1f, 1f, .24f), 4f, 4f, 3f, 0f);
            row.label = Text("Name", go.transform, TextAlignmentOptions.MidlineLeft, 15.5f);
            row.value = Text("Value", go.transform, TextAlignmentOptions.MidlineRight, 17.5f);
            // Reserve an explicit fixed right column for the numerical reading.
            RectTransform l = row.label.rectTransform;
            l.anchorMin = new Vector2(0f, 0f);
            l.anchorMax = new Vector2(1f, 1f);
            l.offsetMin = new Vector2(17f, 0f);
            l.offsetMax = new Vector2(-160f, 0f);
            RectTransform v = row.value.rectTransform;
            v.anchorMin = new Vector2(1f, 0f);
            v.anchorMax = new Vector2(1f, 1f);
            v.pivot = new Vector2(1f, .5f);
            v.anchoredPosition = new Vector2(-12f, 0f);
            v.sizeDelta = new Vector2(143f, 0f);
            rows.Add(row);
            return row;
        }

        private void ApplyColor(Row row, int index)
        {
            Color c = Colors[index % Colors.Length];
            row.background.color = Color.Lerp(c, Hex("#061324"), .32f);
            row.edge.color = Color.Lerp(c, Color.white, .45f);
        }

        private static Image Block(string name, Transform parent, Color color,
            float left, float top, float height, float right)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform r = go.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(.5f, 1f);
            r.offsetMin = new Vector2(left, -Mathf.Max(1f, height + top));
            r.offsetMax = new Vector2(-right, -top);
            if (name == "Edge")
            {
                r.anchorMin = Vector2.zero;
                r.anchorMax = new Vector2(0f, 1f);
                r.offsetMin = Vector2.zero;
                r.offsetMax = new Vector2(4f, 0f);
            }
            Image img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        private static TMP_Text Text(string name, Transform parent,
            TextAlignmentOptions alignment, float size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = size;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static string StripTags(string s) => Regex.Replace(s, "<[^>]+>", string.Empty);
        private static Color Hex(string hex) { ColorUtility.TryParseHtmlString(hex, out Color c); return c; }

        private void OnDisable()
        {
            // Give the original UI back its text in edit mode or if this adapter is disabled.
            if (sourceReadings != null)
            {
                Color c = sourceReadings.color;
                c.a = 1f;
                sourceReadings.color = c;
            }
        }
    }
}
