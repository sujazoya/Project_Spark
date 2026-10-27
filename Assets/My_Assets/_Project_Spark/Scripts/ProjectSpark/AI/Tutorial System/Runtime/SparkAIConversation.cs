using System;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Phase 12: question/answer UI grounded in the active level evaluator.
    /// Read-only: never moves components, creates wires, or changes level results.
    /// Optional external AI integration uses events instead of guessing a brain API.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIConversation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private Camera sceneCamera;

        [Header("Controls")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F8;
        [SerializeField] private bool openOnStart = false;
        [SerializeField] private float inspectDistance = 25f;
        [SerializeField] private LayerMask inspectLayers = ~0;

        [Header("Optional AI Brain Adapter")]
        [Tooltip("When enabled, a listener must subscribe to ExternalQuestionRequested and call DeliverExternalAnswer. Otherwise local answers are used.")]
        [SerializeField] private bool routeToExternalBrain = false;
        [SerializeField, Min(4f)] private float externalAnswerTimeout = 15f;

        [Header("Appearance")]
        [SerializeField] private int sortingOrder = 720;

        public event Action<string, string> ExternalQuestionRequested;
        public event Action<string, string> AnswerReady;

        private Canvas canvas;
        private Text responseText;
        private Text statusText;
        private InputField questionInput;
        private string pendingQuestion;
        private float pendingUntil;

        private void Awake()
        {
            ResolveReferences();
            BuildUI();
            if (canvas != null) canvas.gameObject.SetActive(openOnStart);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey)) Toggle();

            if (pendingQuestion != null && Time.unscaledTime > pendingUntil)
            {
                string question = pendingQuestion;
                pendingQuestion = null;
                Publish(question, "An external AI response wasn't received. Here's the verified circuit information instead:\n\n" + LocalAnswer(question));
            }
        }

        private void ResolveReferences()
        {
            if (levelManager == null) levelManager = FindFirstObjectByType<LevelGamePlayManager>();
            if (sceneCamera == null) sceneCamera = Camera.main;
        }

        public void Toggle()
        {
            if (canvas == null) BuildUI();
            if (canvas == null) return;
            canvas.gameObject.SetActive(!canvas.gameObject.activeSelf);
        }

        public void Ask(string question)
        {
            if (string.IsNullOrWhiteSpace(question)) return;
            ResolveReferences();
            question = question.Trim();
            if (questionInput != null) questionInput.text = question;

            if (routeToExternalBrain && ExternalQuestionRequested != null)
            {
                pendingQuestion = question;
                pendingUntil = Time.unscaledTime + externalAnswerTimeout;
                if (statusText != null) statusText.text = "Thinking with circuit context…";
                ExternalQuestionRequested.Invoke(question, BuildVerifiedContext());
                return;
            }
            pendingQuestion = null;
            Publish(question, LocalAnswer(question));
        }

        /// <summary>Call from your actual AI brain adapter on the Unity main thread.</summary>
        public void DeliverExternalAnswer(string answer)
        {
            if (pendingQuestion == null) return;
            string question = pendingQuestion;
            pendingQuestion = null;
            if (string.IsNullOrWhiteSpace(answer)) answer = LocalAnswer(question);
            Publish(question, answer);
        }

        private void Publish(string question, string answer)
        {
            if (canvas == null) BuildUI();
            if (canvas != null) canvas.gameObject.SetActive(true);
            if (responseText != null) responseText.text = answer;
            if (statusText != null) statusText.text = "ASK SPARK  •  " + question;
            AnswerReady?.Invoke(question, answer);
        }

        public string BuildVerifiedContext()
        {
            ResolveReferences();
            var m = levelManager;
            if (m == null || m.ActiveLevel == null) return "No active level. Solver readings unavailable.";
            return "Level: " + m.ActiveLevel.LevelId +
                   "\nCompleted: " + m.IsCompleted +
                   "\nFailed: " + m.IsFailed +
                   "\nValid source: " + m.HasValidPowerSource +
                   "\nClosed return: " + m.ClosedReturn +
                   "\nSource short: " + m.SourceShorted +
                   "\nTarget short: " + m.TargetShorted +
                   "\nOverloaded: " + m.Overloaded +
                   "\nWrong connection: " + m.WrongConnection +
                   "\nObjectives: " + m.SatisfiedTargetCount + "/" + m.ActiveTargetCount +
                   "\nTarget voltage: " + m.TargetVoltage.ToString("F4") + " V" +
                   "\nTarget current: " + m.TargetCurrent.ToString("F6") + " A" +
                   "\nTarget power: " + m.TargetPower.ToString("F5") + " W" +
                   "\nNOTE: Target readings refer to evaluated level targets, not necessarily the resistor under the cursor.";
        }

        public string LocalAnswer(string question)
        {
            ResolveReferences();
            string q = question.ToLowerInvariant();
            var m = levelManager;

            if (ContainsAny(q, "resistor", "resistance", "ohm", "register"))
            {
                SparkResistor r = ResistorUnderCursor();
                if (r == null)
                    return "A resistor limits current and dissipates power as heat. Point at a resistor and ask again for its actual measurements.\n\n" + BriefCircuitState();
                return string.Format(
                    "{0}: {1:F1} Ω\nSolved voltage: {2:F3} V\nSolved current: {3:F6} A\nDissipated power: {4:F4} W\nPower rating: {5:F3} W\n\n{6}",
                    r.name, r.ResistanceOhms, r.Voltage, r.Current, r.PowerWatts, r.MaximumPowerWatts,
                    r.IsBurned ? "This resistor is marked as burned." :
                    r.IsConducting ? "The solver reports current through this resistor." :
                    "The solver does not currently report conduction through this resistor.");
            }
            if (ContainsAny(q, "voltage", "current", "amp", "power", "watt"))
            {
                if (m == null || m.ActiveLevel == null) return "Start a level to view live solver readings.";
                return string.Format("Evaluated level target readings:\nVoltage: {0:F3} V\nCurrent: {1:F6} A\nPower: {2:F4} W\n\nThese are target measurements, not necessarily measurements across any specific resistor. To measure a resistor, point at it and ask about the resistor.",
                    m.TargetVoltage, m.TargetCurrent, m.TargetPower);
            }
            if (ContainsAny(q, "led", "lamp", "bulb", "glow", "light"))
            {
                return "An LED needs correct polarity and an allowed current path. A visible glow alone doesn't prove the level is complete.\n\n" + BriefCircuitState();
            }
            if (ContainsAny(q, "wire", "connect", "terminal", "switch", "circuit", "why", "wrong", "help", "hint", "work", "complete"))
                return BriefCircuitState();
            return "I can explain the live circuit, why current is zero, resistor readings, the LED, wiring, or level progress.\n\n" + BriefCircuitState();
        }

        private string BriefCircuitState()
        {
            var m = levelManager;
            if (m == null || m.ActiveLevel == null) return "Start a level to get circuit-specific guidance.";
            if (m.IsCompleted) return "The level evaluator confirms completion. Objectives " + m.SatisfiedTargetCount + "/" + m.ActiveTargetCount + ".";
            if (m.IsFailed) return "The level has failed. " + (string.IsNullOrEmpty(m.LastFailureReason) ? "Review the wiring and component ratings." : m.LastFailureReason);
            string diagnosis;
            if (m.SourceShorted || m.TargetShorted) diagnosis = "A short circuit is detected. Look for a wire bypassing a component or joining supply terminals.";
            else if (m.Overloaded) diagnosis = "The evaluator reports an overload. Inspect resistor power ratings and source voltage.";
            else if (!m.HasValidPowerSource) diagnosis = "No valid active power source is detected. Check supply placement and terminal connections.";
            else if (!m.ClosedReturn) diagnosis = "The source is recognized, but the return path is open. Inspect the switch and wiring back to the source.";
            else if (m.WrongConnection) diagnosis = "The evaluator reports an invalid connection. Inspect the connected terminals.";
            else diagnosis = "The return path is detected, but some target conditions remain unsatisfied.";
            return diagnosis + "\nObjectives: " + m.SatisfiedTargetCount + "/" + m.ActiveTargetCount + ".";
        }

        private SparkResistor ResistorUnderCursor()
        {
            if (sceneCamera == null) return null;
            RaycastHit[] hits = Physics.RaycastAll(sceneCamera.ScreenPointToRay(Input.mousePosition), inspectDistance,
                inspectLayers, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                var resistor = hit.collider.GetComponentInParent<SparkResistor>();
                if (resistor != null) return resistor;
            }
            return null;
        }

        private static bool ContainsAny(string q, params string[] terms)
        {
            foreach (string term in terms) if (q.Contains(term)) return true;
            return false;
        }

        private void BuildUI()
        {
            if (canvas != null) return;
            var root = new GameObject("SparkAIConversation_UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var panel = Rect("AskSparkPanel", root.transform, new Vector2(540, 400), new Vector2(-25, 30),
                new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));
            var bg = panel.gameObject.AddComponent<Image>();
            bg.color = new Color(0.025f, 0.065f, 0.11f, 0.97f);

            statusText = TextElement("Status", panel, new Vector2(490, 45), new Vector2(0, 158), 18, Color.cyan);
            responseText = TextElement("Response", panel, new Vector2(490, 238), new Vector2(0, 13), 19, Color.white);
            responseText.alignment = TextAnchor.UpperLeft;
            responseText.text = "Ask about the circuit, a resistor, current, voltage or wiring.";

            var entry = Rect("QuestionInput", panel, new Vector2(390, 44), new Vector2(-48, -159));
            entry.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.18f, 0.23f, 1f);
            questionInput = entry.gameObject.AddComponent<InputField>();
            var inputText = TextElement("InputText", entry, new Vector2(365, 40), Vector2.zero, 18, Color.white);
            inputText.alignment = TextAnchor.MiddleLeft;
            questionInput.textComponent = inputText;
            var placeholder = TextElement("Placeholder", entry, new Vector2(365, 40), Vector2.zero, 17, Color.gray);
            placeholder.text = "Why is current zero?";
            questionInput.placeholder = placeholder;
            questionInput.lineType = InputField.LineType.SingleLine;
            questionInput.onEndEdit.AddListener(s => { if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Ask(s); });

            var sendRect = Rect("SendButton", panel, new Vector2(90, 44), new Vector2(200, -159));
            sendRect.gameObject.AddComponent<Image>().color = new Color(0.03f, 0.5f, 0.62f);
            var button = sendRect.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => Ask(questionInput.text));
            var sendLabel = TextElement("SendLabel", sendRect, new Vector2(85, 40), Vector2.zero, 18, Color.white);
            sendLabel.text = "ASK";
            sendLabel.alignment = TextAnchor.MiddleCenter;
            root.SetActive(false);
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 pos)
        {
            return Rect(name, parent, size, pos, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f));
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 pos,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.pivot = pivot;
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            return r;
        }

        private static Text TextElement(string name, Transform parent, Vector2 size, Vector2 pos, int fontSize, Color color)
        {
            var r = Rect(name, parent, size, pos);
            var t = r.gameObject.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.raycastTarget = false;
            return t;
        }

        [ContextMenu("Ask Spark / Test Circuit Question")]
        private void TestCircuitQuestion() => Ask("Why is my circuit not working?");
    }
}
