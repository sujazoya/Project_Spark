using System;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Read-only, level-aware electrical coaching. Never assumes a lamp's visual
    /// state is equivalent to completion and never modifies the circuit.
    /// Works independently of the guided tutorial's current step.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkTutorialCircuitAwareness : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LevelGamePlayManager levelManager;

        [Header("Behavior")]
        [SerializeField] private bool showOnMeaningfulStateChange = true;
        [SerializeField] private bool showOnLevelStart = true;
        [SerializeField, Min(0.1f)] private float refreshSeconds = 0.4f;
        [SerializeField, Min(1f)] private float messageDurationSeconds = 7f;
        [SerializeField] private KeyCode recallKey = KeyCode.F4;
        [SerializeField] private bool logChanges;

        [Header("Panel (away from tutorial's bottom-left HUD)")]
        [SerializeField] private Vector2 panelSize = new Vector2(490f, 170f);
        [SerializeField] private Vector2 bottomRightOffset = new Vector2(24f, 195f);
        [SerializeField] private int sortingOrder = 540;

        private Canvas canvas;
        private Text heading;
        private Text body;
        private Text footer;
        private float nextRefresh;
        private float hideAt;
        private string lastSignature;
        private string lastDescription;
        private LevelGamePlayManager subscribedManager;
        private bool initialized;

        private void Awake()
        {
            ResolveReferences();
            BuildPanel();
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
            RefreshState(showOnLevelStart);
        }

        private void OnDisable()
        {
            Unsubscribe();
            if (canvas != null) canvas.gameObject.SetActive(false);
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
        }

        private void Subscribe()
        {
            if (subscribedManager == levelManager) return;
            Unsubscribe();
            if (levelManager == null) return;

            levelManager.LevelStarted += OnLevelStarted;
            levelManager.LevelReset += OnLevelStarted;
            levelManager.LevelCompleted += OnLevelEnded;
            levelManager.LevelFailed += OnLevelFailed;
            levelManager.LevelEvaluationChanged += OnLevelChanged;
            subscribedManager = levelManager;
        }

        private void Unsubscribe()
        {
            if (subscribedManager == null) return;
            subscribedManager.LevelStarted -= OnLevelStarted;
            subscribedManager.LevelReset -= OnLevelStarted;
            subscribedManager.LevelCompleted -= OnLevelEnded;
            subscribedManager.LevelFailed -= OnLevelFailed;
            subscribedManager.LevelEvaluationChanged -= OnLevelChanged;
            subscribedManager = null;
        }

        private void OnLevelStarted(SparkLevelDefinition level)
        {
            initialized = false;
            lastSignature = null;
            RefreshState(showOnLevelStart);
        }

        private void OnLevelEnded(SparkLevelDefinition level) => RefreshState(true);
        private void OnLevelFailed(SparkLevelDefinition level, string reason) => RefreshState(true);
        private void OnLevelChanged(SparkLevelDefinition level) => RefreshState(false);

        private void Update()
        {
            if (levelManager == null)
            {
                ResolveReferences();
                Subscribe();
            }

            if (Time.unscaledTime >= nextRefresh)
            {
                nextRefresh = Time.unscaledTime + refreshSeconds;
                RefreshState(false);
            }

            if (Input.GetKeyDown(recallKey))
                Show(lastDescription, true);

            if (canvas != null && canvas.gameObject.activeSelf &&
                Time.unscaledTime >= hideAt)
                canvas.gameObject.SetActive(false);
        }

        private void RefreshState(bool forceShow)
        {
            if (levelManager == null || levelManager.ActiveLevel == null) return;

            LevelGamePlayManager m = levelManager;
            string state = GetState(m);
            // Signature intentionally excludes noisy floating-point solver updates.
            string signature = m.ActiveLevel.LevelId + "|" + state + "|" +
                m.SatisfiedTargetCount + "/" + m.ActiveTargetCount;
            bool changed = !string.Equals(signature, lastSignature, StringComparison.Ordinal);
            lastSignature = signature;

            string description = BuildDescription(m, state);
            lastDescription = description;
            if (changed && logChanges)
                Debug.Log("[Spark AI Circuit] " + signature + " | " + description, this);

            bool shouldShow = forceShow ||
                (initialized && changed && showOnMeaningfulStateChange);
            initialized = true;
            if (shouldShow) Show(description, true);
            else if (canvas != null && canvas.gameObject.activeSelf)
                UpdateLabels(description);
        }

        private static string GetState(LevelGamePlayManager m)
        {
            if (m.IsCompleted) return "Completed";
            if (m.IsFailed) return "Failed";
            if (!m.IsPlaying) return "Waiting";
            if (m.SourceShorted || m.TargetShorted) return "ShortCircuit";
            if (m.Overloaded) return "Overload";
            if (!m.HasValidPowerSource) return "NoSource";
            if (!m.ClosedReturn) return "OpenCircuit";
            if (m.SatisfiedTargetCount < m.ActiveTargetCount) return "TargetsPending";
            return "AwaitingEvaluation";
        }

        private static string BuildDescription(LevelGamePlayManager m, string state)
        {
            string first;
            switch (state)
            {
                case "Completed":
                    first = "Level complete. The official objective evaluator has confirmed success.";
                    break;
                case "Failed":
                    first = string.IsNullOrWhiteSpace(m.LastFailureReason)
                        ? "The level evaluator reports a failure. Review the circuit."
                        : "Level failure: " + m.LastFailureReason;
                    break;
                case "ShortCircuit":
                    first = "A short-circuit condition is reported. Review the wiring before continuing.";
                    break;
                case "Overload":
                    first = "The evaluator reports an overload. Check the component ratings and electrical power.";
                    break;
                case "NoSource":
                    first = "No valid power source is recognized in the solved level state.";
                    break;
                case "OpenCircuit":
                    first = "The return path is still open. Check the switch and both circuit connections.";
                    break;
                case "TargetsPending":
                    first = "The circuit has a closed return path, but some level objectives are not satisfied.";
                    break;
                case "AwaitingEvaluation":
                    first = "The network appears connected. Waiting for the level objectives to confirm success.";
                    break;
                default:
                    first = "Waiting for the level and electrical solver to become ready.";
                    break;
            }

            string measurements = "Solved target: " +
                m.TargetVoltage.ToString("F3") + " V  |  " +
                m.TargetCurrent.ToString("F4") + " A  |  " +
                m.TargetPower.ToString("F3") + " W";
            return first + "\n" + measurements;
        }

        private void Show(string description, bool resetTimer)
        {
            if (string.IsNullOrEmpty(description)) return;
            if (canvas == null) BuildPanel();
            if (canvas == null) return;
            UpdateLabels(description);
            canvas.gameObject.SetActive(true);
            if (resetTimer) hideAt = Time.unscaledTime + messageDurationSeconds;
        }

        private void UpdateLabels(string description)
        {
            if (heading == null || body == null || footer == null || levelManager == null) return;
            heading.text = "SPARK AI  /  LIVE CIRCUIT";
            body.text = description;
            footer.text = "OBJECTIVES  " + levelManager.SatisfiedTargetCount + "/" +
                levelManager.ActiveTargetCount + "      F4  REOPEN";
        }

        private void BuildPanel()
        {
            if (canvas != null) return;
            GameObject root = new GameObject("Spark_AI_LiveCircuitHUD",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            root.GetComponent<GraphicRaycaster>().enabled = false;

            GameObject panel = new GameObject("CircuitNote", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-Mathf.Abs(bottomRightOffset.x), bottomRightOffset.y);
            rect.sizeDelta = panelSize;
            Image bg = panel.GetComponent<Image>();
            bg.color = new Color(0.02f, 0.06f, 0.09f, 0.94f);
            bg.raycastTarget = false;

            heading = MakeLabel(panel.transform, "Heading", new Vector2(0f, 60f),
                new Vector2(panelSize.x - 36f, 28f), 19, FontStyle.Bold,
                new Color(0.2f, 0.94f, 1f));
            body = MakeLabel(panel.transform, "Explanation", new Vector2(0f, 0f),
                new Vector2(panelSize.x - 36f, 87f), 18, FontStyle.Normal, Color.white);
            footer = MakeLabel(panel.transform, "Footer", new Vector2(0f, -65f),
                new Vector2(panelSize.x - 36f, 24f), 14, FontStyle.Bold,
                new Color(0.56f, 0.83f, 0.9f));
            root.SetActive(false);
        }

        private static Text MakeLabel(Transform parent, string label, Vector2 pos,
            Vector2 size, int fontSize, FontStyle style, Color color)
        {
            GameObject go = new GameObject(label, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            Text t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAnchor.MiddleLeft;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.raycastTarget = false;
            return t;
        }
    }
}
