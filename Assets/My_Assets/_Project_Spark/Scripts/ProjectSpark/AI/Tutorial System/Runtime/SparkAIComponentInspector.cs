using System;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Read-only component explainer. Invoke Inspect(Component) from an explicit
    /// inspection action (e.g. an info button); NOT every gameplay click.
    /// Does not change selection, wiring, circuit topology, or tutorial progress.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIComponentInspector : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private bool closeOnEscape = true;
        [SerializeField] private KeyCode closeKey = KeyCode.Escape;
        [SerializeField] private int sortingOrder = 575;
        [SerializeField] private Vector2 panelSize = new Vector2(540f, 250f);
        [SerializeField] private Vector2 topRightInset = new Vector2(24f, 100f);
        [SerializeField] private bool logInspections;

        private Canvas canvas;
        private Text title;
        private Text body;
        private Text footer;
        private Component inspected;
        private SparkResistor inspectedResistor;
        private string previousDetails;
        private float nextRefresh;

        public bool IsOpen => canvas != null && canvas.gameObject.activeSelf;

        private void Awake() { BuildUI(); }

        private void Update()
        {
            if (!IsOpen) return;
            if (closeOnEscape && Input.GetKeyDown(closeKey)) { Close(); return; }
            if (inspected == null) { Close(); return; }
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.2f;
            if (inspectedResistor != null) UpdateResistorReading();
        }

        /// <summary>Call ONLY when the player requests information.</summary>
        public void Inspect(Component component)
        {
            if (component == null) return;
            if (canvas == null) BuildUI();
            inspected = component;
            inspectedResistor = component.GetComponentInParent<SparkResistor>();
            if (inspectedResistor == null)
                inspectedResistor = component.GetComponentInChildren<SparkResistor>(true);

            SparkTerminal terminal = component.GetComponentInParent<SparkTerminal>();
            SparkShelfItem shelfItem = component.GetComponentInParent<SparkShelfItem>();
            string objectName = inspectedResistor != null ? inspectedResistor.name : component.name;
            if (inspectedResistor != null)
            {
                title.text = "RESISTOR  ·  " + objectName;
                body.text = "A resistor limits current and dissipates power as heat. It only carries current when the circuit provides a complete conducting path.";
                UpdateResistorReading();
            }
            else if (terminal != null)
            {
                title.text = "ELECTRICAL TERMINAL  ·  " + terminal.name;
                body.text = "A terminal is a connection point. Choose a compatible second terminal to attempt a wire connection. Selecting a terminal alone is not an error.";
                footer.text = "Connections: " + terminal.ConnectionCount + " / " + terminal.MaxConnections;
            }
            else
            {
                string typeName = component.GetType().Name;
                string lower = (objectName + " " + typeName).ToLowerInvariant();
                if (lower.Contains("switch"))
                {
                    title.text = "SWITCH  ·  " + objectName;
                    body.text = "A switch changes whether its intended conduction path is open or closed. A closed switch does not guarantee current: the rest of the circuit must also be complete.";
                }
                else if (lower.Contains("battery") || lower.Contains("supply") || lower.Contains("power"))
                {
                    title.text = "POWER SOURCE  ·  " + objectName;
                    body.text = "A power source establishes electrical potential. Actual current depends on the connected circuit and its electrical state.";
                }
                else if (lower.Contains("led") || lower.Contains("diode"))
                {
                    title.text = "LED / DIODE  ·  " + objectName;
                    body.text = "An LED is a polarity-sensitive diode. Its anode and cathode must be connected correctly, and current must remain within its safe operating limits.";
                }
                else
                {
                    title.text = "COMPONENT  ·  " + objectName;
                    body.text = "This is part of your workbench. Inspect its terminals and the current level objective to learn how it participates in the circuit.";
                }
                footer.text = shelfItem == null ? "Read-only inspection" :
                    (shelfItem.IsAtWorkbench ? "Position: Workstation" : "Position: Storage") + "  ·  Read-only inspection";
            }
            canvas.gameObject.SetActive(true);
            if (logInspections) Debug.Log("[SparkAI Inspector] " + title.text, component);
        }

        private void UpdateResistorReading()
        {
            if (inspectedResistor == null) return;
            SparkResistor r = inspectedResistor;
            string details = "R = " + r.ResistanceOhms.ToString("0.###") + " Ω" +
                             "    V = " + r.Voltage.ToString("0.###") + " V" +
                             "    I = " + r.Current.ToString("0.######") + " A" +
                             "    P = " + r.PowerWatts.ToString("0.###") + " W" +
                             "    Rating = " + r.MaximumPowerWatts.ToString("0.###") + " W";
            if (details == previousDetails) return;
            previousDetails = details;
            string state = r.IsBurned ? "FAILED / OPEN CIRCUIT" :
                r.IsConducting ? "CONDUCTING" : "NOT CONDUCTING";
            footer.text = details + "\nSolver state: " + state +
                (r.IsOverPower ? "  ·  OVER POWER RATING" : "");
        }

        public void Close()
        {
            if (canvas != null) canvas.gameObject.SetActive(false);
            inspected = null;
            inspectedResistor = null;
            previousDetails = null;
        }

        [ContextMenu("Debug / Inspect First Resistor")]
        private void InspectFirstResistor()
        {
            SparkResistor r = FindFirstObjectByType<SparkResistor>();
            if (r != null) Inspect(r);
            else Debug.LogWarning("[SparkAI Inspector] No active SparkResistor found.", this);
        }

        private void OnDestroy() { if (canvas != null) Destroy(canvas.gameObject); }

        private void BuildUI()
        {
            if (canvas != null) return;
            var root = new GameObject("SparkAI_ComponentInspector_UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            root.GetComponent<GraphicRaycaster>().enabled = false;
            var group = root.GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            var panelGo = new GameObject("InspectorPanel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(root.transform, false);
            var panel = panelGo.GetComponent<RectTransform>();
            panel.anchorMin = panel.anchorMax = new Vector2(1, 1);
            panel.pivot = new Vector2(1, 1);
            panel.sizeDelta = panelSize;
            panel.anchoredPosition = new Vector2(-topRightInset.x, -topRightInset.y);
            var bg = panelGo.GetComponent<Image>();
            bg.color = new Color(0.018f, 0.04f, 0.07f, 0.95f);
            bg.raycastTarget = false;

            title = MakeText("Title", panel, 23, FontStyle.Bold, new Color(0.35f, 0.91f, 1f), new Vector2(0, -12), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(500, 38));
            body = MakeText("Explanation", panel, 20, FontStyle.Normal, Color.white, new Vector2(0, -70), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(500, 112));
            footer = MakeText("Measurements", panel, 16, FontStyle.Normal, new Color(0.65f, 0.92f, 0.93f), new Vector2(0, 15), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(500, 75));
            root.SetActive(false);
        }

        private static Text MakeText(string name, RectTransform parent, int size, FontStyle style, Color color,
            Vector2 pos, Vector2 anchor, Vector2 pivot, Vector2 dimensions)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = dimensions;
            Text t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAnchor.UpperLeft;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }
    }
}
