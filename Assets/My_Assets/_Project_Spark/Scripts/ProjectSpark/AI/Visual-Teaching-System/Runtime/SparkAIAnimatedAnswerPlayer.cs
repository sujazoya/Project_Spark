using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.AI.Tutorial;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Optional read-only, verified-fact animation for answers returned by the EXISTING
    /// SparkAIUnifiedAssistant via the existing Natural Explanation request bridge.
    /// AI prose is displayed ONLY; it cannot specify anchors, measurements or paths.
    /// This component never calls PresentValidatedPlan except through the user's
    /// explicit ROUTE BOARD button on the existing adaptive panel.
    /// All callbacks and Unity UI operations require the Unity main thread.
    /// </summary>
    [DefaultExecutionOrder(-500)] // Capture PromptReady before a synchronous local answer can return.
    [DisallowMultipleComponent]
    public sealed class SparkAIAnimatedAnswerPlayer : MonoBehaviour
    {
        [Serializable]
        private sealed class AssistantReply
        {
            public string requestId;
            public string answer;
            public string reasoning;
            public string nextCheck;
        }

        private sealed class Step
        {
            public string title;
            public string body;
            public string anchorId;
            public bool verifiedFault;
            public bool isRoute;
        }

        [Header("Existing systems (Editor builder assigns these)")]
        [SerializeField] private SparkAIUnifiedAssistant assistant;
        [SerializeField] private SparkAIExplanationRequestBridge requestBridge;
        [SerializeField] private SparkAdaptiveExplanationEngine engine;
        [SerializeField] private SparkAdaptiveExplanationPanel adaptivePanel;
        [SerializeField] private SparkVisualTeachingManager teacher;
        [SerializeField] private SparkWorldAnnotationRenderer worldRenderer;

        [Header("Generated panel")]
        [SerializeField] private GameObject card;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text stepText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private TMP_Text aiAnswerText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button worldButton;
        [SerializeField] private Button routeButton;

        [Header("Playback")]
        [SerializeField] private bool openWhenAnswerArrives = false;
        [SerializeField, Range(2f, 20f)] private float secondsPerStep = 6f;
        [SerializeField, Range(.1f, 1f)] private float drawSeconds = .4f;
        [SerializeField] private bool logEvents;

        private SparkAIUnifiedAssistant boundAssistant;
        private SparkAIExplanationRequestBridge boundBridge;
        private readonly List<Step> steps = new List<Step>();
        private readonly List<SparkWorldAnnotationRenderer.Annotation> owned =
            new List<SparkWorldAnnotationRenderer.Annotation>();
        private SparkVerifiedElectricalFrame requestSnapshot;
        private string requestCueKey;
        private string pendingPrompt;
        private string pendingId;
        private string readyPrompt;
        private string receivedAnswer;
        private int stepIndex = -1;
        private bool worldVisible;
        private bool paused;
        private bool rejectedAsStale;
        private Coroutine playback;
        private Coroutine drawing;

        private void Awake()
        {
            Resolve();
            if (card != null) card.SetActive(false);
        }

        private void OnEnable()
        {
            Resolve();
            Subscribe();
            RefreshButtons();
        }

        private void Start() { Resolve(); Subscribe(); }

        private void OnDisable()
        {
            Unsubscribe();
            StopPlayback();
            ClearOwnAnnotations();
        }

        private void OnDestroy() { Unsubscribe(); }

        private void Resolve()
        {
            if (assistant == null)
                assistant = FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            if (requestBridge == null)
                requestBridge = FindFirstObjectByType<SparkAIExplanationRequestBridge>(FindObjectsInactive.Include);
            if (engine == null)
                engine = FindFirstObjectByType<SparkAdaptiveExplanationEngine>(FindObjectsInactive.Include);
            if (adaptivePanel == null)
                adaptivePanel = FindFirstObjectByType<SparkAdaptiveExplanationPanel>(FindObjectsInactive.Include);
            if (teacher == null)
                teacher = FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            if (worldRenderer == null)
                worldRenderer = FindFirstObjectByType<SparkWorldAnnotationRenderer>(FindObjectsInactive.Include);
        }

        private void Subscribe()
        {
            if (boundBridge != requestBridge)
            {
                if (boundBridge != null) boundBridge.PromptReady.RemoveListener(OnPromptReady);
                boundBridge = requestBridge;
                if (boundBridge != null) boundBridge.PromptReady.AddListener(OnPromptReady);
            }
            if (boundAssistant != assistant)
            {
                if (boundAssistant != null) boundAssistant.AnswerReady -= OnAnswerReady;
                boundAssistant = assistant;
                if (boundAssistant != null) boundAssistant.AnswerReady += OnAnswerReady;
            }
        }

        private void Unsubscribe()
        {
            if (boundBridge != null) boundBridge.PromptReady.RemoveListener(OnPromptReady);
            if (boundAssistant != null) boundAssistant.AnswerReady -= OnAnswerReady;
            boundBridge = null;
            boundAssistant = null;
        }

        // Only this existing, verified explanation pipeline can arm a playback.
        private void OnPromptReady(string prompt)
        {
            StopPlayback();
            ClearOwnAnnotations();
            steps.Clear();
            stepIndex = -1;
            readyPrompt = null;
            receivedAnswer = null;
            rejectedAsStale = false;
            pendingPrompt = null;
            pendingId = null;
            requestSnapshot = null;
            requestCueKey = null;
            worldVisible = false;

            SparkAdaptiveCue cue = engine != null ? engine.SelectedCue : null;
            SparkVerifiedElectricalFrame frame = engine != null ? engine.CurrentFrame : null;
            if (requestBridge == null || !requestBridge.HasPendingRequest ||
                !string.Equals(requestBridge.LastPrompt, prompt, StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(prompt) || frame == null || cue == null)
            {
                Set(statusText, "No valid verified explanation request to animate.");
                RefreshButtons();
                return;
            }

            pendingPrompt = prompt;
            pendingId = requestBridge.PendingRequestId;
            requestSnapshot = frame.Copy();
            requestCueKey = CueKey(cue);
            Set(statusText, "Waiting for Spark AI's answer to this verified request.");
            RefreshButtons();
        }

        private void OnAnswerReady(string originalQuestion, string reply)
        {
            if (pendingPrompt == null ||
                !string.Equals(originalQuestion, pendingPrompt, StringComparison.Ordinal))
                return; // Do not animate an unrelated Ask Spark chat reply.

            // An expired or cancelled Natural Explanation request must never
            // start a new visual lesson from a late assistant callback.
            if (requestBridge == null || !requestBridge.HasPendingRequest ||
                !string.Equals(requestBridge.PendingRequestId, pendingId, StringComparison.Ordinal))
            {
                Invalidate("The explanation request is no longer active.");
                return;
            }

            if (!SameVerifiedFacts())
            {
                Invalidate("The circuit or selected component changed before AI replied.");
                return;
            }

            string display = reply;
            if (!string.IsNullOrWhiteSpace(reply) && reply.TrimStart().StartsWith("{", StringComparison.Ordinal))
            {
                try
                {
                    AssistantReply parsed = JsonUtility.FromJson<AssistantReply>(reply);
                    if (parsed != null && !string.IsNullOrWhiteSpace(parsed.requestId) &&
                        !string.Equals(parsed.requestId, pendingId, StringComparison.Ordinal))
                    {
                        Invalidate("AI reply had a different request ID.");
                        return;
                    }
                    if (parsed != null && !string.IsNullOrWhiteSpace(parsed.answer))
                        display = parsed.answer;
                }
                catch (Exception) { /* Plain text is advisory only. */ }
            }

            if (string.IsNullOrWhiteSpace(display))
            {
                Invalidate("AI returned no explanation.");
                return;
            }

            receivedAnswer = Trim(display, 650);
            readyPrompt = pendingPrompt;
            pendingPrompt = null;
            BuildVerifiedSteps();
            if (steps.Count == 0) { Invalidate("No verified facts available for a visual explanation."); return; }
            stepIndex = 0;
            Set(aiAnswerText, "AI ANSWER (ADVISORY, NOT MEASUREMENT)\n" + receivedAnswer);
            if (openWhenAnswerArrives) Show();
            ShowStep();
            if (logEvents) Debug.Log("[Spark Animated AI] Prepared " + steps.Count + " verified visual steps.", this);
        }

        private void BuildVerifiedSteps()
        {
            steps.Clear();
            if (requestSnapshot == null || engine == null || engine.SelectedCue == null) return;
            SparkAdaptiveCue cue = engine.SelectedCue;
            string anchor = cue.hasWorldTarget ? cue.anchorId : null;
            steps.Add(new Step {
                title = "01  SPARK AI ANSWER",
                body = "The assistant's explanation is shown below. Visual annotations in later steps use verified simulator data only.",
                anchorId = anchor
            });

            SparkVerifiedElectricalReading reading = null;
            if (requestSnapshot.readings != null)
                foreach (SparkVerifiedElectricalReading item in requestSnapshot.readings)
                    if (item != null && string.Equals(item.anchorId, cue.anchorId, StringComparison.Ordinal))
                    { reading = item; break; }

            if (reading != null)
            {
                string details = "Voltage: " + Format(reading.voltageVolts, "F3") + " V\n" +
                    "Current: " + Format(reading.currentAmps * 1000f, "F3") + " mA\n" +
                    "Resistance: " + Format(reading.resistanceOhms, "F2") + " ohms";
                steps.Add(new Step { title = "02  VERIFIED MEASUREMENTS", body = details, anchorId = anchor });
            }
            else
                steps.Add(new Step { title = "02  VERIFIED MEASUREMENTS", body =
                    "No component reading was supplied for this cue. Do not infer voltage or current.", anchorId = anchor });

            bool fault = requestSnapshot.hasVerifiedFault &&
                string.Equals(requestSnapshot.faultAnchorId, cue.anchorId, StringComparison.Ordinal);
            steps.Add(new Step {
                title = "03  SOLVER DIAGNOSIS",
                body = cue.title + "\n" + Trim(cue.explanation, 260) +
                    (fault ? "\nVerified fault: " + Trim(requestSnapshot.verifiedFaultExplanation, 200) : ""),
                anchorId = anchor, verifiedFault = fault
            });

            if (requestSnapshot.hasVerifiedRoute && requestSnapshot.routeEnergized &&
                requestSnapshot.orderedRouteAnchors != null &&
                requestSnapshot.orderedRouteAnchors.Length >= 2)
            {
                steps.Add(new Step { title = "04  VERIFIED CURRENT ROUTE",
                    body = "Solver-provided conventional-current route order:\n" +
                        Trim(string.Join(" → ", requestSnapshot.orderedRouteAnchors), 320) +
                        "\nThis is NOT a complete electrical schematic.", isRoute = true });
            }
            else
            {
                steps.Add(new Step { title = "04  CURRENT ROUTE",
                    body = "No energized current route was verified in this frame. No route arrows will be drawn." });
            }
            steps.Add(new Step { title = "05  NEXT INVESTIGATION",
                body = "Suggested diagnostic check (not a completed action):\n" + Trim(cue.nextAction, 300), anchorId = anchor });
        }

        private static string Format(float v, string format) =>
            v.ToString(format, CultureInfo.InvariantCulture);
        private static string Trim(string s, int max) =>
            string.IsNullOrEmpty(s) ? "" : (s.Length > max ? s.Substring(0, max) : s);

        private static string CueKey(SparkAdaptiveCue cue) => cue == null ? null :
            (cue.anchorId ?? "") + "|" + (cue.title ?? "") + "|" +
            (cue.explanation ?? "") + "|" + (cue.nextAction ?? "");

        // Compare the relevant verified values, not frame identity or revision.
        // Identical solver refreshes do not cancel visual explanations.
        private bool SameVerifiedFacts()
        {
            if (requestSnapshot == null || engine == null || engine.CurrentFrame == null ||
                engine.SelectedCue == null || !string.Equals(requestCueKey, CueKey(engine.SelectedCue), StringComparison.Ordinal))
                return false;
            SparkVerifiedElectricalFrame now = engine.CurrentFrame;
            if (!string.Equals(now.circuitId, requestSnapshot.circuitId, StringComparison.Ordinal) ||
                now.hasVerifiedFault != requestSnapshot.hasVerifiedFault ||
                now.hasVerifiedRoute != requestSnapshot.hasVerifiedRoute ||
                now.routeEnergized != requestSnapshot.routeEnergized ||
                !string.Equals(now.faultAnchorId, requestSnapshot.faultAnchorId, StringComparison.Ordinal) ||
                !string.Equals(now.verifiedFaultExplanation, requestSnapshot.verifiedFaultExplanation, StringComparison.Ordinal))
                return false;
            string[] a = now.orderedRouteAnchors ?? Array.Empty<string>();
            string[] b = requestSnapshot.orderedRouteAnchors ?? Array.Empty<string>();
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (!string.Equals(a[i], b[i], StringComparison.Ordinal)) return false;
            return SameSelectedReading(now, requestSnapshot, engine.SelectedCue.anchorId);
        }

        private static bool SameSelectedReading(SparkVerifiedElectricalFrame a,
            SparkVerifiedElectricalFrame b, string anchor)
        {
            SparkVerifiedElectricalReading x = FindReading(a, anchor);
            SparkVerifiedElectricalReading y = FindReading(b, anchor);
            if (x == null || y == null) return x == null && y == null;
            return x.voltageVolts.Equals(y.voltageVolts) &&
                x.currentAmps.Equals(y.currentAmps) &&
                x.resistanceOhms.Equals(y.resistanceOhms) &&
                x.condition == y.condition &&
                string.Equals(x.explanation, y.explanation, StringComparison.Ordinal);
        }

        private static SparkVerifiedElectricalReading FindReading(SparkVerifiedElectricalFrame frame, string id)
        {
            if (frame?.readings == null) return null;
            foreach (SparkVerifiedElectricalReading r in frame.readings)
                if (r != null && string.Equals(r.anchorId, id, StringComparison.Ordinal)) return r;
            return null;
        }

        private void Update()
        {
            if ((readyPrompt == null && pendingPrompt == null) || rejectedAsStale) return;
            if (pendingPrompt != null && (requestBridge == null ||
                !requestBridge.HasPendingRequest ||
                !string.Equals(requestBridge.PendingRequestId, pendingId, StringComparison.Ordinal)))
            {
                Invalidate("The explanation request expired or was cancelled.");
                return;
            }
            if (!SameVerifiedFacts()) Invalidate("Circuit readings or selection changed. Ask AI again.");
            else RefreshButtons();
        }

        private void Invalidate(string reason)
        {
            rejectedAsStale = true;
            pendingPrompt = null;
            readyPrompt = null;
            StopPlayback();
            ClearOwnAnnotations();
            steps.Clear();
            stepIndex = -1;
            Set(statusText, "STALE — " + reason);
            RefreshButtons();
        }

        public void Show()
        {
            Resolve();
            if (card != null) card.SetActive(true);
            RefreshButtons();
        }
        public void Hide()
        {
            StopPlayback();
            ClearOwnAnnotations();
            worldVisible = false;
            if (card != null) card.SetActive(false);
        }
        public void Toggle() { if (card != null && card.activeSelf) Hide(); else Show(); }

        public void NextStep()
        {
            if (!CanPlay()) return;
            StopPlayback();
            if (stepIndex < steps.Count - 1) stepIndex++;
            ShowStep();
        }
        public void PreviousStep()
        {
            if (!CanPlay()) return;
            StopPlayback();
            if (stepIndex > 0) stepIndex--;
            ShowStep();
        }
        public void Play()
        {
            if (!CanPlay()) return;
            StopPlayback();
            paused = false;
            Enable3D();
            playback = StartCoroutine(Playback());
        }
        public void PauseOrResume()
        {
            if (playback == null) { Play(); return; }
            paused = !paused;
            Set(statusText, paused ? "PAUSED" : "PLAYING VERIFIED VISUAL STEPS");
        }
        public void Show3D()
        {
            if (!CanPlay()) return;
            Enable3D();
            ShowStep();
        }
        private void Enable3D()
        {
            if (teacher == null || worldRenderer == null) return;
            // Opt-in action. Does not reset lesson steps or whiteboard content.
            worldVisible = true;
            teacher.ViewBoth();
            teacher.OpenPanel();
        }
        public void ShowRouteBoard()
        {
            if (!CanPlay() || adaptivePanel == null || engine == null ||
                !engine.HasVerifiedCurrentPath) return;
            StopPlayback();
            ClearOwnAnnotations();
            // This EXPLICIT button intentionally reuses the existing route viewer.
            // It replaces the teacher's currently displayed lesson as documented.
            adaptivePanel.ShowVerifiedRouteWhiteboard();
            Set(statusText, "Opened solver-verified route. Previous whiteboard lesson was replaced.");
        }

        private IEnumerator Playback()
        {
            while (CanPlay())
            {
                ShowStep();
                float elapsed = 0f;
                while (elapsed < secondsPerStep)
                {
                    if (!CanPlay()) { playback = null; yield break; }
                    if (!paused) elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
                if (stepIndex >= steps.Count - 1) break;
                stepIndex++;
            }
            playback = null;
            paused = false;
            Set(statusText, "Playback complete. Verified data remains unchanged.");
            RefreshButtons();
        }

        private void StopPlayback()
        {
            if (playback != null) { StopCoroutine(playback); playback = null; }
            if (drawing != null) { StopCoroutine(drawing); drawing = null; }
            paused = false;
        }

        private bool CanPlay() => !rejectedAsStale && readyPrompt != null &&
            steps.Count > 0 && stepIndex >= 0 && SameVerifiedFacts();

        private void ShowStep()
        {
            if (!CanPlay()) { RefreshButtons(); return; }
            Step s = steps[stepIndex];
            Set(titleText, s.title);
            Set(stepText, "STEP " + (stepIndex + 1) + " / " + steps.Count);
            Set(bodyText, s.body);
            Set(statusText, "SOURCE: VERIFIED SOLVER DATA  |  AI ANSWER: ADVISORY");
            if (drawing != null) { StopCoroutine(drawing); drawing = null; }
            ClearOwnAnnotations();
            if (worldVisible && worldRenderer != null)
                drawing = StartCoroutine(DrawStep(s));
            RefreshButtons();
        }

        private IEnumerator DrawStep(Step s)
        {
            Color cyan = new Color(.2f, .85f, 1f, 1f);
            Color danger = new Color(1f, .32f, .25f, 1f);
            if (s.isRoute && requestSnapshot.hasVerifiedRoute && requestSnapshot.routeEnergized)
            {
                string[] ids = requestSnapshot.orderedRouteAnchors;
                if (ids != null && ids.Length >= 2 && ids.Length <= 24)
                {
                    bool allPresent = true;
                    foreach (string id in ids)
                        if (!SparkTeachingAnchor.TryFind(id, out _)) { allPresent = false; break; }
                    if (allPresent)
                    {
                        for (int i = 0; i + 1 < ids.Length; i++)
                            AddOwn(new SparkVisualCommand {
                                shape = SparkVisualShape.Arrow,
                                from = Anchor(ids[i]), to = Anchor(ids[i + 1]),
                                color = new Color(1f, .8f, .25f, 1f), thickness = 5f
                            });
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(s.anchorId) &&
                SparkTeachingAnchor.TryFind(s.anchorId, out _))
            {
                AddOwn(new SparkVisualCommand {
                    shape = SparkVisualShape.Ring, from = Anchor(s.anchorId),
                    color = s.verifiedFault ? danger : cyan, thickness = 5f,
                    normalizedSize = new Vector2(.065f, .1f)
                });
                string label = s.verifiedFault ? "VERIFIED FAULT" : "VERIFIED COMPONENT";
                AddOwn(new SparkVisualCommand {
                    shape = SparkVisualShape.Label,
                    from = new SparkVisualPoint {
                        space = SparkPointSpace.WorldAnchor, anchorId = s.anchorId,
                        pixelOffset = new Vector2(0f, 72f)
                    },
                    color = Color.white, fontSize = 18, text = label
                });
            }

            float t = 0f;
            while (t < drawSeconds && owned.Count > 0)
            {
                if (!paused) t += Time.unscaledDeltaTime;
                float v = Mathf.Clamp01(t / Mathf.Max(.05f, drawSeconds));
                foreach (var annotation in owned) worldRenderer.SetProgress(annotation, v);
                yield return null;
            }
            foreach (var annotation in owned) worldRenderer.SetProgress(annotation, 1f);
            drawing = null;
        }

        private static SparkVisualPoint Anchor(string id) =>
            new SparkVisualPoint { space = SparkPointSpace.WorldAnchor, anchorId = id };
        private void AddOwn(SparkVisualCommand command)
        {
            SparkWorldAnnotationRenderer.Annotation annotation = worldRenderer.Add(command);
            if (annotation != null) owned.Add(annotation);
        }
        private void ClearOwnAnnotations()
        {
            if (worldRenderer != null)
                foreach (var annotation in owned) worldRenderer.Remove(annotation);
            owned.Clear();
        }
        private void RefreshButtons()
        {
            bool ready = CanPlay();
            if (previousButton != null) previousButton.interactable = ready && stepIndex > 0;
            if (nextButton != null) nextButton.interactable = ready && stepIndex < steps.Count - 1;
            if (playButton != null) playButton.interactable = ready;
            if (pauseButton != null) pauseButton.interactable = ready && playback != null;
            if (worldButton != null) worldButton.interactable = ready && teacher != null && worldRenderer != null;
            if (routeButton != null) routeButton.interactable = ready && adaptivePanel != null &&
                engine != null && engine.HasVerifiedCurrentPath;
        }
        private static void Set(TMP_Text label, string text)
        {
            if (label != null) { label.richText = false; label.text = text ?? ""; }
        }
    }
}
