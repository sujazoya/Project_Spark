using System;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Read-only diagnosis of the AUTHORITATIVE level evaluation. Never edits
    /// circuit topology or declares victory. F5 opens/closes the diagnosis.
    /// An optional marker identifies an unsatisfied objective's scene binding,
    /// not a proven defective wire or component.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAICircuitDiagnosis : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private Camera sceneCamera;

        [Header("Behavior")]
        [SerializeField] private KeyCode diagnoseKey = KeyCode.F5;
        [SerializeField] private bool openWhenLevelFails = false;
        [SerializeField] private bool closeOnLevelChange = true;
        [SerializeField, Min(0.1f)] private float refreshSeconds = 0.35f;
        [SerializeField] private bool logDiagnosis;

        [Header("Presentation")]
        [SerializeField] private Vector2 panelSize = new Vector2(560f, 240f);
        [SerializeField] private Vector2 rightCenterInset = new Vector2(24f, 0f);
        [SerializeField] private int sortingOrder = 565;
        [SerializeField] private bool markUnsatisfiedObjective = true;
        [SerializeField] private Color markerColor = new Color(0.98f, 0.66f, 0.16f, 1f);

        private LevelGamePlayManager subscribed;
        private Canvas canvas;
        private Text heading;
        private Text message;
        private Text details;
        private RectTransform marker;
        private Image[] markerEdges;
        private Transform markedTarget;
        private float nextRefresh;
        private string lastText;

        private void Awake()
        {
            ResolveReferences();
            BuildUI();
        }

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
        }

        private void Start()
        {
            ResolveReferences();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            Close();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            if (canvas != null) Destroy(canvas.gameObject);
        }

        private void ResolveReferences()
        {
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>();
            if (sceneCamera == null) sceneCamera = Camera.main;
        }

        private void Subscribe()
        {
            if (subscribed == levelManager) return;
            Unsubscribe();
            if (levelManager == null) return;
            subscribed = levelManager;
            subscribed.LevelStarted += OnLevelChanged;
            subscribed.LevelReset += OnLevelChanged;
            subscribed.LevelEvaluationChanged += OnEvaluationChanged;
            subscribed.LevelCompleted += OnLevelCompleted;
            subscribed.LevelFailed += OnLevelFailed;
        }

        private void Unsubscribe()
        {
            if (subscribed == null) return;
            subscribed.LevelStarted -= OnLevelChanged;
            subscribed.LevelReset -= OnLevelChanged;
            subscribed.LevelEvaluationChanged -= OnEvaluationChanged;
            subscribed.LevelCompleted -= OnLevelCompleted;
            subscribed.LevelFailed -= OnLevelFailed;
            subscribed = null;
        }

        private void OnLevelChanged(SparkLevelDefinition _)
        {
            markedTarget = null;
            if (closeOnLevelChange) Close();
            else if (IsOpen) Refresh();
        }

        private void OnEvaluationChanged(SparkLevelDefinition _)
        {
            if (IsOpen) Refresh();
        }

        private void OnLevelCompleted(SparkLevelDefinition _)
        {
            if (IsOpen) Refresh();
        }

        private void OnLevelFailed(SparkLevelDefinition _, string reason)
        {
            if (openWhenLevelFails) Open();
            else if (IsOpen) Refresh();
        }

        private void Update()
        {
            if (levelManager == null)
            {
                ResolveReferences();
                Subscribe();
            }
            if (Input.GetKeyDown(diagnoseKey))
            {
                if (IsOpen) Close(); else Open();
            }
            if (!IsOpen) return;
            if (Time.unscaledTime >= nextRefresh)
            {
                nextRefresh = Time.unscaledTime + refreshSeconds;
                Refresh();
            }
            PositionMarker();
        }

        public bool IsOpen => canvas != null && canvas.gameObject.activeSelf;

        [ContextMenu("Diagnosis / Open")]
        public void Open()
        {
            if (canvas == null) BuildUI();
            if (canvas == null) return;
            canvas.gameObject.SetActive(true);
            Refresh();
        }

        [ContextMenu("Diagnosis / Close")]
        public void Close()
        {
            if (canvas != null) canvas.gameObject.SetActive(false);
            markedTarget = null;
        }

        [ContextMenu("Diagnosis / Refresh")]
        public void Refresh()
        {
            if (levelManager == null || heading == null) return;
            LevelGamePlayManager m = levelManager;
            markedTarget = null;
            string main;
            string next;

            if (m.ActiveLevel == null)
            {
                main = "No active level is loaded.";
                next = "Start a level before inspecting its circuit.";
            }
            else if (m.IsCompleted)
            {
                main = "The official level evaluator confirmed success.";
                next = "All required objectives are complete. No repair is needed.";
            }
            else if (m.IsFailed)
            {
                main = "The level evaluator reports a failed configuration.";
                next = string.IsNullOrWhiteSpace(m.LastFailureReason)
                    ? "Inspect the circuit and review the failure condition."
                    : "Reported reason: " + m.LastFailureReason;
            }
            else if (m.SourceShorted || m.TargetShorted)
            {
                main = "A short-circuit condition was detected.";
                next = "Inspect the power and target connections. Avoid a direct bypass across the source or load.";
            }
            else if (m.Overloaded)
            {
                main = "The level evaluator detected an overload.";
                next = "Check component power ratings and the solver's voltage, current, and power readings.";
            }
            else if (m.WrongConnection)
            {
                main = "The evaluator detected an invalid connection.";
                next = "Review terminal polarity and the required wiring path. Only a rejected connection attempt is a wiring mistake.";
            }
            else if (!m.HasValidPowerSource)
            {
                main = "The solver does not recognize a valid power source for this level.";
                next = "Inspect the battery or power supply and its terminal connections.";
            }
            else if (!m.ClosedReturn)
            {
                main = "The circuit does not have a complete return path.";
                next = "Check both ends of the circuit and the switch. A wire touching a terminal visually may not be electrically registered.";
            }
            else if (m.ActiveTargetCount > 0 && m.SatisfiedTargetCount < m.ActiveTargetCount)
            {
                main = "A return path exists, but some required objectives are not satisfied.";
                next = "Check the objective list and required operating values.";
            }
            else
            {
                main = "No confirmed fault is available in the latest level state.";
                next = "Use the workbench readings and wait for the next authoritative evaluation.";
            }

            string pendingName = null;
            var results = m.TargetResults;
            if (m.ActiveLevel != null && results != null)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    var result = results[i];
                    if (result.satisfied) continue;
                    pendingName = string.IsNullOrWhiteSpace(result.displayName)
                        ? result.targetId : result.displayName;
                    if (markUnsatisfiedObjective && m.SceneBindings != null &&
                        !string.IsNullOrWhiteSpace(result.targetId) &&
                        m.SceneBindings.TryGetObject(result.targetId, out GameObject obj) && obj != null)
                    {
                        markedTarget = obj.transform;
                    }
                    break;
                }
            }

            heading.text = "SPARK AI  /  CIRCUIT DIAGNOSIS";
            message.text = main + "\n" + next;
            details.text = "TARGETS " + m.SatisfiedTargetCount + "/" + m.ActiveTargetCount +
                "   |   V " + m.TargetVoltage.ToString("F3") + " V" +
                "   I " + m.TargetCurrent.ToString("F4") + " A" +
                "   P " + m.TargetPower.ToString("F3") + " W" +
                (string.IsNullOrEmpty(pendingName) ? "" : "\nNEXT UNSATISFIED OBJECTIVE: " + pendingName) +
                "\nF5  CLOSE";
            string signature = main + " | " + next + " | " + pendingName;
            if (logDiagnosis && signature != lastText)
                Debug.Log("[SparkAI Diagnosis] " + signature, this);
            lastText = signature;
        }

        private void BuildUI()
        {
            if (canvas != null) return;
            var root = new GameObject("SparkAI_CircuitDiagnosis_UI", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            root.GetComponent<GraphicRaycaster>().enabled = false;
            var cg = root.GetComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            var panelObject = new GameObject("DiagnosisPanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(root.transform, false);
            RectTransform panel = panelObject.GetComponent<RectTransform>();
            panel.anchorMin = panel.anchorMax = new Vector2(1f, 0.5f);
            panel.pivot = new Vector2(1f, 0.5f);
            panel.anchoredPosition = new Vector2(-Mathf.Abs(rightCenterInset.x), rightCenterInset.y);
            panel.sizeDelta = panelSize;
            var bg = panelObject.GetComponent<Image>();
            bg.color = new Color(0.025f, 0.055f, 0.085f, 0.95f);
            bg.raycastTarget = false;
            heading = MakeText(panel, "Title", new Vector2(0f, 87f),
                new Vector2(panelSize.x - 36f, 30f), 21, FontStyle.Bold,
                new Color(0.25f, 0.92f, 1f));
            message = MakeText(panel, "Explanation", new Vector2(0f, 12f),
                new Vector2(panelSize.x - 36f, 104f), 19, FontStyle.Normal, Color.white);
            details = MakeText(panel, "Readings", new Vector2(0f, -83f),
                new Vector2(panelSize.x - 36f, 71f), 15, FontStyle.Normal,
                new Color(0.75f, 0.87f, 0.93f));

            var markerObject = new GameObject("ObjectiveMarker", typeof(RectTransform));
            markerObject.transform.SetParent(root.transform, false);
            marker = markerObject.GetComponent<RectTransform>();
            marker.anchorMin = marker.anchorMax = new Vector2(0.5f, 0.5f);
            marker.sizeDelta = new Vector2(100f, 76f);
            markerEdges = new Image[4];
            for (int i = 0; i < 4; i++)
            {
                var edge = new GameObject("Edge_" + i, typeof(RectTransform), typeof(Image));
                edge.transform.SetParent(marker, false);
                Image img = edge.GetComponent<Image>();
                img.raycastTarget = false;
                img.color = markerColor;
                markerEdges[i] = img;
            }
            root.SetActive(false);
        }

        private void PositionMarker()
        {
            if (marker == null) return;
            if (!markUnsatisfiedObjective || markedTarget == null || sceneCamera == null)
            {
                marker.gameObject.SetActive(false);
                return;
            }
            Vector3 projected = sceneCamera.WorldToScreenPoint(markedTarget.position);
            bool visible = projected.z > 0f && projected.x >= 0 && projected.x <= Screen.width &&
                projected.y >= 0 && projected.y <= Screen.height;
            marker.gameObject.SetActive(visible);
            if (!visible) return;
            RectTransform root = marker.parent as RectTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(root,
                new Vector2(projected.x, projected.y), null, out Vector2 point))
                marker.anchoredPosition = point;
            float pulse = 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 3f);
            for (int i = 0; i < 4; i++) markerEdges[i].color = new Color(
                markerColor.r, markerColor.g, markerColor.b, markerColor.a * pulse);
            float halfW = marker.rect.width / 2f, halfH = marker.rect.height / 2f;
            SetEdge(markerEdges[0].rectTransform, new Vector2(0, halfH), new Vector2(100, 3));
            SetEdge(markerEdges[1].rectTransform, new Vector2(0, -halfH), new Vector2(100, 3));
            SetEdge(markerEdges[2].rectTransform, new Vector2(-halfW, 0), new Vector2(3, 76));
            SetEdge(markerEdges[3].rectTransform, new Vector2(halfW, 0), new Vector2(3, 76));
        }

        private static void SetEdge(RectTransform rect, Vector2 pos, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }

        private static Text MakeText(Transform parent, string name, Vector2 position,
            Vector2 size, int sizePt, FontStyle style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var label = go.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = sizePt;
            label.fontStyle = style;
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }
    }
}
