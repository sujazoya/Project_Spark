using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    [DisallowMultipleComponent]
    public sealed class SparkVisualTeachingManager : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private SparkWorldAnnotationRenderer worldRenderer;
        [SerializeField] private SparkSchematicRenderer schematicRenderer;
        [SerializeField] private GameObject worldOverlay;
        [SerializeField] private CanvasGroup whiteboardGroup;
        [SerializeField] private GameObject launcher;

        [Header("User Interface")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text goalText;
        [SerializeField] private TMP_Text stepText;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Image progressFill;
        [SerializeField] private TMP_Text pauseButtonText;

        [Header("Lesson")]
        [SerializeField] private SparkVisualLesson initialLesson;
        [SerializeField] private bool openOnStart;
        [SerializeField] private bool playLessonOnStart;
        [SerializeField] private SparkTeachingView initialView = SparkTeachingView.Both;
        [SerializeField, Range(.25f, 3f)] private float playbackSpeed = 1f;
        [Header("Callbacks")]
        public UnityEvent onLessonFinished;
        public UnityEvent onVerifiedAction;

        public event Action<int, int> StepChanged;
        public SparkTeachingView View { get; private set; }
        public bool Paused { get; private set; }
        public bool PanelOpen { get; private set; }
        public int CurrentStepIndex => currentStepIndex;
        public int StepCount => steps != null ? steps.Length : 0;

        private SparkTeachingStep[] steps = Array.Empty<SparkTeachingStep>();
        private int currentStepIndex = -1;
        private bool verifiedCurrentStep;
        private Coroutine playback;
        private string goal;
        private string lessonTitle;

        private void Awake()
        {
            SetView(initialView);
            SetPanelOpen(openOnStart);
        }

        private void Start()
        {
            if (playLessonOnStart && currentStepIndex < 0)
            {
                if (initialLesson != null) StartLesson(initialLesson);
                else PlayDemoLesson();
            }
            else UpdateUI();
        }

        private void OnDisable()
        {
            if (playback != null) { StopCoroutine(playback); playback = null; }
        }

        public void SetPanelOpen(bool isOpen)
        {
            PanelOpen = isOpen;
            if (whiteboardGroup != null)
            {
                bool visible = isOpen && View != SparkTeachingView.WorldOnly;
                whiteboardGroup.alpha = visible ? 1f : 0f;
                whiteboardGroup.interactable = visible;
                whiteboardGroup.blocksRaycasts = visible;
            }
            if (worldOverlay != null)
                worldOverlay.SetActive(isOpen && View != SparkTeachingView.WhiteboardOnly);
            if (launcher != null) launcher.SetActive(!isOpen || View == SparkTeachingView.WorldOnly);
        }

        public void OpenPanel() => SetPanelOpen(true);
        // Reopens whiteboard even if the previous view was world-only.
        public void OpenTeachingBoard() { SetView(SparkTeachingView.Both); SetPanelOpen(true); }
        public void ClosePanel() => SetPanelOpen(false);
        public void TogglePanel() => SetPanelOpen(!PanelOpen);

        public void SetView(SparkTeachingView mode)
        {
            View = mode;
            SetPanelOpen(PanelOpen);
        }

        public void ViewBoth() => SetView(SparkTeachingView.Both);
        public void ViewWhiteboard() => SetView(SparkTeachingView.WhiteboardOnly);
        public void ViewWorld() => SetView(SparkTeachingView.WorldOnly);

        public void StartLesson(SparkVisualLesson lesson)
        {
            if (lesson == null || lesson.steps == null || lesson.steps.Length == 0)
            {
                SetStatus("No lesson steps assigned.");
                return;
            }
            initialLesson = lesson;
            lessonTitle = lesson.title;
            goal = lesson.goal;
            steps = lesson.steps;
            currentStepIndex = -1;
            LoadStep(0);
            OpenPanel();
        }

        public void NextStep()
        {
            if (steps == null || steps.Length == 0) return;
            if (currentStepIndex >= 0 && currentStepIndex < steps.Length)
            {
                string required = steps[currentStepIndex].requiredVerifiedActionId;
                if (!string.IsNullOrWhiteSpace(required) && !verifiedCurrentStep)
                {
                    SetStatus("Complete the highlighted circuit task first.");
                    return;
                }
            }
            if (currentStepIndex + 1 >= steps.Length)
            {
                SetStatus("Lesson complete.");
                onLessonFinished?.Invoke();
                return;
            }
            LoadStep(currentStepIndex + 1);
        }

        public void PreviousStep()
        {
            if (currentStepIndex > 0) LoadStep(currentStepIndex - 1);
        }

        public void ReplayStep()
        {
            if (currentStepIndex >= 0 && currentStepIndex < steps.Length)
                LoadStep(currentStepIndex);
        }

        public void ResetLesson()
        {
            if (steps != null && steps.Length > 0) LoadStep(0);
        }

        // Call ONLY after your trusted gameplay/diagnostic controller confirms
        // the action. Do not expose this method to raw unvalidated AI output.
        public bool ReportVerifiedAction(string actionId)
        {
            if (steps == null || currentStepIndex < 0 || currentStepIndex >= steps.Length) return false;
            SparkTeachingStep step = steps[currentStepIndex];
            if (string.IsNullOrWhiteSpace(step.requiredVerifiedActionId) ||
                !string.Equals(step.requiredVerifiedActionId, actionId, StringComparison.Ordinal)) return false;
            if (verifiedCurrentStep) return true;
            verifiedCurrentStep = true;
            SetStatus("Task verified. Great work!");
            onVerifiedAction?.Invoke();
            if (step.advanceWhenVerified) NextStep();
            return true;
        }

        public void Pause() { Paused = true; UpdateUI(); }
        public void Resume() { Paused = false; UpdateUI(); }
        public void TogglePause() { Paused = !Paused; UpdateUI(); }
        public void SetPlaybackSpeed(float speed) { playbackSpeed = Mathf.Clamp(speed, .25f, 3f); }

        public void ClearAnnotations()
        {
            StopPlayback();
            if (worldRenderer != null) worldRenderer.ClearAll();
        }

        public void ClearEverything()
        {
            ClearAnnotations();
            if (schematicRenderer != null) schematicRenderer.Clear();
            SetStatus("Teaching visuals cleared.");
        }

        public bool PresentPlanJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json.Length > 65536)
            { SetStatus("Invalid drawing plan size."); return false; }
            try { return PresentValidatedPlan(JsonUtility.FromJson<SparkVisualTeachingPlan>(json)); }
            catch (Exception ex)
            {
                Debug.LogWarning("[Spark Teaching] Could not parse teaching plan: " + ex.Message, this);
                SetStatus("Teaching plan could not be read.");
                return false;
            }
        }

        public bool PresentValidatedPlan(SparkVisualTeachingPlan plan)
        {
            if (!SparkTeachingValidator.Validate(plan, out string error))
            {
                Debug.LogWarning("[Spark Teaching] Rejected visual plan: " + error, this);
                SetStatus("Drawing request rejected: " + error);
                return false;
            }
            StopPlayback();
            if (worldRenderer != null) worldRenderer.ClearAll();
            if (schematicRenderer != null) schematicRenderer.Display(plan.schematic);
            lessonTitle = plan.title ?? "AI explanation";
            goal = plan.explanation ?? "";
            steps = Array.Empty<SparkTeachingStep>();
            currentStepIndex = -1;
            SetStatus("Illustrative explanation. Check actual circuit measurements in diagnostics.");
            UpdateUI();
            playback = StartCoroutine(PlayAnnotations(plan.annotations));
            ViewBoth();
            OpenPanel();
            return true;
        }

        private void LoadStep(int index)
        {
            if (steps == null || index < 0 || index >= steps.Length) return;
            StopPlayback();
            if (worldRenderer != null) worldRenderer.ClearAll();
            currentStepIndex = index;
            verifiedCurrentStep = false;
            SparkTeachingStep step = steps[index];
            if (schematicRenderer != null) schematicRenderer.Display(step.schematic);
            SetStatus(string.IsNullOrWhiteSpace(step.requiredVerifiedActionId)
                ? "Follow the guided explanation." : "Awaiting verified gameplay action.");
            UpdateUI();
            StepChanged?.Invoke(index + 1, steps.Length);
            playback = StartCoroutine(PlayAnnotations(step.annotations));
        }

        private void StopPlayback()
        {
            if (playback != null) { StopCoroutine(playback); playback = null; }
            Paused = false;
        }

        private IEnumerator PlayAnnotations(SparkVisualCommand[] commands)
        {
            if (commands == null) yield break;
            foreach (SparkVisualCommand c in commands)
            {
                if (!SparkTeachingValidator.Validate(c, out string reason))
                {
                    Debug.LogWarning("[Spark Teaching] Skipping invalid command: " + reason, this);
                    continue;
                }
                if (c.shape == SparkVisualShape.Clear)
                {
                    if (worldRenderer != null) worldRenderer.ClearAll();
                    continue;
                }
                if (c.shape == SparkVisualShape.Wait)
                {
                    yield return Delay(c.holdSeconds);
                    continue;
                }
                if (worldRenderer == null) continue;
                SparkWorldAnnotationRenderer.Annotation item = worldRenderer.Add(c);
                if (item == null) continue;
                if (c.drawSeconds <= 0f) worldRenderer.SetProgress(item, 1f);
                else
                {
                    float t = 0;
                    while (t < c.drawSeconds)
                    {
                        if (!Paused) t += Time.unscaledDeltaTime * playbackSpeed;
                        worldRenderer.SetProgress(item, Mathf.Clamp01(t / c.drawSeconds));
                        yield return null;
                    }
                    worldRenderer.SetProgress(item, 1f);
                }
                if (c.holdSeconds > 0f) yield return Delay(c.holdSeconds);
                if (!c.persistent) worldRenderer.Remove(item);
            }
            playback = null;
        }

        private IEnumerator Delay(float seconds)
        {
            float elapsed = 0;
            while (elapsed < seconds)
            {
                if (!Paused) elapsed += Time.unscaledDeltaTime * playbackSpeed;
                yield return null;
            }
        }

        private void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
        }

        private void UpdateUI()
        {
            if (titleText != null) titleText.text = string.IsNullOrEmpty(lessonTitle) ? "SPARK AI VISUAL TEACHER" : lessonTitle;
            if (goalText != null) goalText.text = string.IsNullOrEmpty(goal) ? "Circuit explanation" : goal;
            bool valid = steps != null && currentStepIndex >= 0 && currentStepIndex < steps.Length;
            if (stepText != null) stepText.text = valid ? $"STEP {currentStepIndex + 1} / {steps.Length}" : "AI EXPLANATION";
            if (instructionText != null) instructionText.text = valid ? steps[currentStepIndex].instruction : goal;
            if (hintText != null) hintText.text = valid ? steps[currentStepIndex].hint : "AI drawings are illustrative until verified by the circuit simulator.";
            if (progressFill != null) progressFill.fillAmount = valid ? (float)(currentStepIndex + 1) / steps.Length : 0f;
            if (pauseButtonText != null) pauseButtonText.text = Paused ? "RESUME" : "PAUSE";
        }

        public void PlayDemoLesson()
        {
            SparkSchematicData diagram = new SparkSchematicData {
                caption = "DEMO (illustrative) • 9V → 220Ω → LED",
                parts = new [] {
                    new SparkSchematicPart {id="battery", kind=SparkSchematicKind.Battery, label="9V", position=new Vector2(.16f,.65f)},
                    new SparkSchematicPart {id="resistor", kind=SparkSchematicKind.Resistor, label="220Ω", position=new Vector2(.49f,.65f)},
                    new SparkSchematicPart {id="led", kind=SparkSchematicKind.LED, label="LED", position=new Vector2(.79f,.65f)}
                },
                wires = new [] {
                    new SparkSchematicWire { points=new [] {new Vector2(.23f,.65f),new Vector2(.40f,.65f)} },
                    new SparkSchematicWire { points=new [] {new Vector2(.56f,.65f),new Vector2(.71f,.65f)} },
                    new SparkSchematicWire { points=new [] {new Vector2(.87f,.65f),new Vector2(.94f,.65f),new Vector2(.94f,.33f),new Vector2(.09f,.33f),new Vector2(.09f,.65f)} }
                }
            };
            var step1 = new SparkTeachingStep {
                title="Identify parts", instruction="Identify the battery, resistor and LED on the schematic.",
                hint="This is an example drawing, not a live measurement.", schematic=diagram,
                annotations=new [] { new SparkVisualCommand {shape=SparkVisualShape.Label,
                    from=new SparkVisualPoint {normalized=new Vector2(.5f,.84f)},
                    text="First identify the source and load.", persistent=true} }
            };
            var step2 = new SparkTeachingStep {
                title="Follow the route", instruction="Trace the series connection from source through resistor to LED.",
                hint="The resistor limits current; check actual polarity in the scene.", schematic=diagram,
                annotations=new [] {
                    new SparkVisualCommand {shape=SparkVisualShape.Arrow,
                        from=new SparkVisualPoint {normalized=new Vector2(.28f,.72f)},
                        to=new SparkVisualPoint {normalized=new Vector2(.50f,.72f)}, color=Color.yellow},
                    new SparkVisualCommand {shape=SparkVisualShape.Arrow,
                        from=new SparkVisualPoint {normalized=new Vector2(.51f,.72f)},
                        to=new SparkVisualPoint {normalized=new Vector2(.75f,.72f)}, color=Color.yellow}
                }
            };
            var step3 = new SparkTeachingStep {
                title="Inspect connections", instruction="Use the game's circuit diagnostics to verify wiring and LED polarity.",
                hint="A diagram is not proof that the real scene is correctly connected.", schematic=diagram,
                annotations=new [] {new SparkVisualCommand {shape=SparkVisualShape.Label,
                    from=new SparkVisualPoint {normalized=new Vector2(.5f,.84f)},
                    text="Verify before energizing the circuit.", color=Color.yellow}}
            };
            lessonTitle = "BASIC LED CIRCUIT — DEMO";
            goal = "Learn the circuit one step at a time";
            steps = new [] {step1, step2, step3};
            LoadStep(0);
            ViewBoth();
            OpenPanel();
        }
    }
}
