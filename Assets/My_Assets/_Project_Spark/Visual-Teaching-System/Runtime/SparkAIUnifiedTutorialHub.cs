using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Additive, read-only navigation shell around existing Project Spark teaching modules.
    /// Does not create lessons, grade objectives, edit circuits, or manage training data.
    /// The builder configures all scene references and Unity UI events.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIUnifiedTutorialHub : MonoBehaviour
    {
        [Header("Existing systems")]
        [SerializeField] private SparkVisualTeachingManager teaching;
        [SerializeField] private SparkAIAnimatedAnswerPlayer animatedAnswers;
        [SerializeField] private SparkAIComponentVisualLessonPlayer componentLessons;
        [SerializeField] private SparkAIStudentProgressCenter studentProgress;
        [SerializeField] private SparkAIUnifiedAssistant assistant;
        [SerializeField] private LevelGamePlayManager levelManager;

        [Header("Editor-generated UI")]
        [SerializeField] private GameObject hubCard;
        [SerializeField] private TMP_Text levelStatus;
        [SerializeField] private TMP_Text lessonStatus;
        [SerializeField] private TMP_Text feedback;
        [SerializeField] private TMP_Text whiteboardAvailability;
        [SerializeField] private TMP_Text animatedAvailability;
        [SerializeField] private TMP_Text componentAvailability;
        [SerializeField] private TMP_Text studentAvailability;
        [SerializeField] private Button whiteboardButton;
        [SerializeField] private Button animatedButton;
        [SerializeField] private Button componentButton;
        [SerializeField] private Button studentButton;
        [SerializeField] private Button worldButton;
        [SerializeField] private Button bothButton;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button askButton;

        [Header("Options")]
        [Tooltip("Hide other visual panels when opening a new module. Sessions are preserved, but the other players may stop their own playback when hidden.")]
        [SerializeField] private bool closeOtherTeachingPanels = true;
        [SerializeField, Min(0.5f)] private float refreshSeconds = 1.2f;

        private LevelGamePlayManager subscribedLevel;
        private float nextRefresh;

        private void Awake()
        {
            Resolve();
            if (hubCard != null) hubCard.SetActive(false);
        }

        private void OnEnable()
        {
            Resolve();
            Subscribe();
            Refresh();
        }

        private void OnDisable() => Unsubscribe();
        private void OnDestroy() => Unsubscribe();

        private void Update()
        {
            if (hubCard == null || !hubCard.activeSelf || Time.unscaledTime < nextRefresh)
                return;

            nextRefresh = Time.unscaledTime + Mathf.Max(0.5f, refreshSeconds);
            Refresh();
        }

        private void Resolve()
        {
            if (teaching == null)
                teaching = FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            if (animatedAnswers == null)
                animatedAnswers = FindFirstObjectByType<SparkAIAnimatedAnswerPlayer>(FindObjectsInactive.Include);
            if (componentLessons == null)
                componentLessons = FindFirstObjectByType<SparkAIComponentVisualLessonPlayer>(FindObjectsInactive.Include);
            if (studentProgress == null)
                studentProgress = FindFirstObjectByType<SparkAIStudentProgressCenter>(FindObjectsInactive.Include);
            if (assistant == null)
                assistant = FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
        }

        private void Subscribe()
        {
            if (subscribedLevel == levelManager) return;
            Unsubscribe();
            if (levelManager == null) return;
            subscribedLevel = levelManager;
            subscribedLevel.LevelStarted += OnLevelChanged;
            subscribedLevel.LevelReset += OnLevelChanged;
            subscribedLevel.LevelEvaluationChanged += OnLevelChanged;
            subscribedLevel.LevelCompleted += OnLevelChanged;
            subscribedLevel.LevelFailed += OnLevelFailed;
        }

        private void Unsubscribe()
        {
            if (subscribedLevel == null) return;
            subscribedLevel.LevelStarted -= OnLevelChanged;
            subscribedLevel.LevelReset -= OnLevelChanged;
            subscribedLevel.LevelEvaluationChanged -= OnLevelChanged;
            subscribedLevel.LevelCompleted -= OnLevelChanged;
            subscribedLevel.LevelFailed -= OnLevelFailed;
            subscribedLevel = null;
        }

        private void OnLevelChanged(SparkLevelDefinition _) => Refresh();
        private void OnLevelFailed(SparkLevelDefinition _, string __) => Refresh();

        private static void Set(TMP_Text target, string value)
        {
            if (target != null) target.text = value;
        }

        private static bool Available(MonoBehaviour target)
        {
            return target != null && target.enabled && target.gameObject.activeInHierarchy;
        }

        private static void Mark(Button button, TMP_Text label, bool available)
        {
            if (button != null) button.interactable = available;
            Set(label, available ? "READY" : "NOT INSTALLED / INACTIVE");
        }

        [ContextMenu("Spark AI Tutorial Hub / Refresh")]
        public void Refresh()
        {
            Resolve();
            Subscribe();
            bool canTeach = Available(teaching);
            Mark(whiteboardButton, whiteboardAvailability, canTeach);
            Mark(animatedButton, animatedAvailability, Available(animatedAnswers));
            Mark(componentButton, componentAvailability, Available(componentLessons));
            Mark(studentButton, studentAvailability, Available(studentProgress));

            if (worldButton != null) worldButton.interactable = canTeach;
            if (bothButton != null) bothButton.interactable = canTeach;
            if (previousButton != null) previousButton.interactable = canTeach && teaching.StepCount > 0;
            if (nextButton != null) nextButton.interactable = canTeach && teaching.StepCount > 0;
            if (replayButton != null) replayButton.interactable = canTeach && teaching.StepCount > 0;
            if (askButton != null) askButton.interactable = Available(assistant);

            if (levelManager == null || levelManager.ActiveLevel == null)
            {
                Set(levelStatus, "NO ACTIVE LEVEL  /  START A LEVEL TO VIEW LIVE PROGRESS");
            }
            else
            {
                Set(levelStatus, "LEVEL  /  " + levelManager.ActiveLevel.DisplayName +
                    "     EVALUATOR  " + levelManager.SatisfiedTargetCount + "/" +
                    levelManager.ActiveTargetCount + "     STATE  " + levelManager.RuntimeState);
            }

            if (canTeach)
            {
                Set(lessonStatus, teaching.StepCount > 0
                    ? "VISUAL LESSON  /  STEP " + (teaching.CurrentStepIndex + 1) +
                      " OF " + teaching.StepCount + "     VIEW  " + teaching.View
                    : "VISUAL LESSON  /  NONE LOADED     VIEW  " + teaching.View);
            }
            else Set(lessonStatus, "VISUAL TEACHING MANAGER UNAVAILABLE");
        }

        [ContextMenu("Spark AI Tutorial Hub / Show")]
        public void Show()
        {
            Resolve();
            if (hubCard == null)
            {
                Debug.LogWarning("[Spark AI Tutorial Hub] HubCard reference missing.", this);
                return;
            }
            hubCard.SetActive(true);
            Refresh();
            nextRefresh = Time.unscaledTime + Mathf.Max(0.5f, refreshSeconds);
        }

        public void Hide()
        {
            if (hubCard != null) hubCard.SetActive(false);
        }

        public void Toggle()
        {
            if (hubCard != null && hubCard.activeSelf) Hide();
            else Show();
        }

        private bool Require(MonoBehaviour module, string name)
        {
            Resolve();
            if (Available(module)) return true;
            string message = name + " is missing or inactive. Create/enable its existing panel first.";
            Set(feedback, message);
            Debug.LogWarning("[Spark AI Tutorial Hub] " + message, this);
            return false;
        }

        private void CloseOthers(MonoBehaviour destination)
        {
            if (!closeOtherTeachingPanels) return;
            if (Available(animatedAnswers) && animatedAnswers != destination) animatedAnswers.Hide();
            if (Available(componentLessons) && componentLessons != destination) componentLessons.Hide();
            if (Available(studentProgress) && studentProgress != destination) studentProgress.Hide();
            if (Available(teaching) && teaching != destination) teaching.ClosePanel();
        }

        public void OpenWhiteboard()
        {
            Resolve();
            if (!Require(teaching, "Visual Teaching Manager")) return;
            CloseOthers(teaching);
            Hide();
            teaching.ViewWhiteboard();
            teaching.OpenPanel();
        }

        public void OpenBothViews()
        {
            Resolve();
            if (!Require(teaching, "Visual Teaching Manager")) return;
            CloseOthers(teaching);
            Hide();
            teaching.OpenTeachingBoard();
        }

        public void OpenWorldView()
        {
            Resolve();
            if (!Require(teaching, "Visual Teaching Manager")) return;
            CloseOthers(teaching);
            Hide();
            teaching.ViewWorld();
            teaching.OpenPanel();
        }

        public void OpenAnimatedAnswers()
        {
            Resolve();
            if (!Require(animatedAnswers, "Animated Answer Player")) return;
            CloseOthers(animatedAnswers);
            Hide();
            animatedAnswers.Show();
        }

        public void OpenComponentLessons()
        {
            Resolve();
            if (!Require(componentLessons, "Component Visual Lessons")) return;
            CloseOthers(componentLessons);
            Hide();
            componentLessons.Show();
        }

        public void OpenStudentProgress()
        {
            Resolve();
            if (!Require(studentProgress, "Student Progress Center")) return;
            CloseOthers(studentProgress);
            Hide();
            studentProgress.Show();
        }

        public void PreviousTeachingStep()
        {
            Resolve();
            if (!Require(teaching, "Visual Teaching Manager")) return;
            if (teaching.StepCount == 0) return;
            teaching.PreviousStep();
            OpenBothViews();
        }

        public void NextTeachingStep()
        {
            Resolve();
            if (!Require(teaching, "Visual Teaching Manager")) return;
            if (teaching.StepCount == 0) return;
            teaching.NextStep();
            OpenBothViews();
        }

        public void ReplayTeachingStep()
        {
            Resolve();
            if (!Require(teaching, "Visual Teaching Manager")) return;
            if (teaching.StepCount == 0) return;
            teaching.ReplayStep();
            OpenBothViews();
        }

        public void OpenAskSpark()
        {
            Resolve();
            if (!Require(assistant, "SparkAIUnifiedAssistant")) return;
            CloseOthers(assistant);
            Hide();
            assistant.Show();
        }
    }
}
