using System;
using System.Collections.Generic;
using System.Globalization;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Formative local knowledge tests plus evaluator-backed practical verification.
    /// Does NOT write training achievements, game progress, solver values or rewards.
    /// A practical requires a PENDING baseline, a subsequent evaluator event, and
    /// optionally a fresh completed solver frame published after the attempt starts.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAILearningPathAssessment : MonoBehaviour
    {
        private const string ScorePrefix = "ProjectSpark.Assessments.Best.v1/";
        private const string PassedPrefix = "ProjectSpark.Assessments.Passed.v1/";

        [Header("Existing systems")]
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkVerifiedElectricalPublisher publisher;
        [SerializeField] private SparkAIUnifiedAssistant assistant;
        [Tooltip("Optional: attach the Guided Learning Paths component (as a MonoBehaviour).")]
        [SerializeField] private MonoBehaviour learningPaths;
        [Header("Editable exam banks")]
        [SerializeField] private SparkAIAssessmentBank[] banks = Array.Empty<SparkAIAssessmentBank>();
        [Header("Practical verification")]
        [SerializeField] private bool requireFreshSolverFrame = true;
        [SerializeField, Min(1f)] private float maxFrameAgeSeconds = 15f;
        [Header("Editor-generated UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text bankTitleText;
        [SerializeField] private TMP_Text bankDescriptionText;
        [SerializeField] private TMP_Text questionCountText;
        [SerializeField] private TMP_Text questionTitleText;
        [SerializeField] private TMP_Text questionPromptText;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text[] choiceLabels = new TMP_Text[4];
        [SerializeField] private Button[] choiceButtons = new Button[4];
        [SerializeField] private TMP_InputField numericInput;
        [SerializeField] private GameObject numericRoot;
        [SerializeField] private Image progressFill;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button beginPracticalButton;
        [SerializeField] private Button verifyButton;

        private sealed class Attempt
        {
            public bool[] answered;
            public bool[] correct;
            public int[] selectedChoice;
        }

        private Attempt[] attempts;
        private int bankIndex;
        private int questionIndex;
        private string message = "Choose an assessment bank and answer its questions.";
        private LevelGamePlayManager listeningLevel;
        private SparkVerifiedElectricalPublisher listeningPublisher;
        private int frameSerial;
        private int startFrameSerial;
        private float lastFrameTime = -9999f;
        private string observedCircuitId;
        private string practicalCircuitId;
        private string practicalLevelId;
        private string practicalTargetId;
        private int practicalBankIndex = -1;
        private int practicalQuestionIndex = -1;
        private bool practicalActive;

        private void Awake()
        {
            Resolve();
            MakeAttempts();
            if (panel != null) panel.SetActive(false);
        }

        private void OnEnable()
        {
            Resolve();
            if (attempts == null || attempts.Length != (banks == null ? 0 : banks.Length))
                MakeAttempts();
            Subscribe();
            Refresh();
        }

        private void OnDisable() { Unsubscribe(); }
        private void OnDestroy() { Unsubscribe(); }

        private void Resolve()
        {
            if (levelManager == null) levelManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            if (publisher == null) publisher = FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            if (assistant == null) assistant = FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
        }

        private void MakeAttempts()
        {
            int count = banks == null ? 0 : banks.Length;
            attempts = new Attempt[count];
            for (int b = 0; b < count; b++)
            {
                int n = banks[b] != null && banks[b].items != null ? banks[b].items.Length : 0;
                attempts[b] = new Attempt
                {
                    answered = new bool[n],
                    correct = new bool[n],
                    selectedChoice = new int[n]
                };
                for (int i = 0; i < n; i++) attempts[b].selectedChoice[i] = -1;
            }
        }

        private void Subscribe()
        {
            if (listeningLevel != levelManager)
            {
                if (listeningLevel != null) DetachLevel();
                listeningLevel = levelManager;
                if (listeningLevel != null)
                {
                    listeningLevel.LevelStarted += OnLevelReset;
                    listeningLevel.LevelReset += OnLevelReset;
                    listeningLevel.LevelFailed += OnLevelFailed;
                    listeningLevel.LevelEvaluationChanged += OnEvaluated;
                    listeningLevel.LevelCompleted += OnEvaluated;
                }
            }
            if (listeningPublisher != publisher)
            {
                if (listeningPublisher != null) listeningPublisher.FramePublished -= OnFrame;
                listeningPublisher = publisher;
                if (listeningPublisher != null) listeningPublisher.FramePublished += OnFrame;
            }
        }

        private void DetachLevel()
        {
            listeningLevel.LevelStarted -= OnLevelReset;
            listeningLevel.LevelReset -= OnLevelReset;
            listeningLevel.LevelFailed -= OnLevelFailed;
            listeningLevel.LevelEvaluationChanged -= OnEvaluated;
            listeningLevel.LevelCompleted -= OnEvaluated;
            listeningLevel = null;
        }

        private void Unsubscribe()
        {
            if (listeningLevel != null) DetachLevel();
            if (listeningPublisher != null) listeningPublisher.FramePublished -= OnFrame;
            listeningPublisher = null;
        }

        private void OnLevelReset(SparkLevelDefinition unused)
        {
            CancelPractical("Level started or reset. Begin a new practical attempt.");
            frameSerial = 0;
            observedCircuitId = null;
            lastFrameTime = -9999f;
        }

        private void OnLevelFailed(SparkLevelDefinition unused, string reason)
        {
            CancelPractical("Level failed. Restart and begin the practical again.");
        }

        private void OnFrame(SparkVerifiedElectricalFrame frame)
        {
            if (frame == null || frame.revision < 0 || frame.readings == null) return;
            string circuit = frame.circuitId ?? string.Empty;
            if (practicalActive && !string.IsNullOrEmpty(practicalCircuitId) &&
                !string.Equals(practicalCircuitId, circuit, StringComparison.Ordinal))
            {
                CancelPractical("Circuit identity changed. Practical attempt cancelled.");
                return;
            }
            observedCircuitId = circuit;
            frameSerial++;
            lastFrameTime = Time.unscaledTime;
            Refresh();
        }

        private void OnEvaluated(SparkLevelDefinition unused)
        {
            CheckPractical();
            Refresh();
        }

        private SparkAIAssessmentBank CurrentBank => banks != null && bankIndex >= 0 && bankIndex < banks.Length ? banks[bankIndex] : null;
        private SparkAssessmentItem CurrentItem
        {
            get
            {
                SparkAIAssessmentBank bank = CurrentBank;
                return bank != null && bank.items != null && questionIndex >= 0 && questionIndex < bank.items.Length
                    ? bank.items[questionIndex] : null;
            }
        }

        private Attempt CurrentAttempt => attempts != null && bankIndex >= 0 && bankIndex < attempts.Length ? attempts[bankIndex] : null;
        private static string Key(string prefix, string id)
        {
            string value = id ?? string.Empty;
            return prefix + value.Length + ":" + value;
        }

        public void Show() { Resolve(); Subscribe(); if (panel != null) panel.SetActive(true); Refresh(); }
        public void Hide() { if (panel != null) panel.SetActive(false); }
        public void Toggle() { if (panel != null && panel.activeSelf) Hide(); else Show(); }

        public void PreviousBank() { SelectBank(-1); }
        public void NextBank() { SelectBank(1); }
        private void SelectBank(int delta)
        {
            if (banks == null || banks.Length == 0) return;
            CancelPractical(null);
            bankIndex = (bankIndex + delta + banks.Length) % banks.Length;
            questionIndex = 0;
            if (numericInput != null) numericInput.text = string.Empty;
            message = "Assessment selected. Previous answers in this session are preserved.";
            Refresh();
        }
        public void PreviousQuestion() { MoveQuestion(-1); }
        public void NextQuestion() { MoveQuestion(1); }
        private void MoveQuestion(int delta)
        {
            var bank = CurrentBank;
            if (bank == null || bank.items == null || bank.items.Length == 0) return;
            // Navigation does not itself grade or cancel a pending practical.
            questionIndex = Mathf.Clamp(questionIndex + delta, 0, bank.items.Length - 1);
            if (numericInput != null) numericInput.text = string.Empty;
            message = "Select an answer, or begin a practical verification.";
            Refresh();
        }

        public void Choice0() { Choose(0); }
        public void Choice1() { Choose(1); }
        public void Choice2() { Choose(2); }
        public void Choice3() { Choose(3); }
        private void Choose(int value)
        {
            SparkAssessmentItem item = CurrentItem;
            Attempt a = CurrentAttempt;
            if (item == null || item.kind != SparkAssessmentItemKind.MultipleChoice || a == null ||
                a.answered[questionIndex] || item.choices == null || value >= item.choices.Length) return;
            a.selectedChoice[questionIndex] = value;
            message = "Choice selected. Press SUBMIT to grade it.";
            Refresh();
        }

        public void SubmitAnswer()
        {
            var item = CurrentItem;
            var a = CurrentAttempt;
            if (item == null || a == null || a.answered[questionIndex]) return;
            if (item.kind == SparkAssessmentItemKind.VerifiedPractical)
            {
                message = "Use BEGIN PRACTICAL. Success requires a new official evaluation.";
                Refresh(); return;
            }
            bool correct;
            if (item.kind == SparkAssessmentItemKind.MultipleChoice)
            {
                int choice = a.selectedChoice[questionIndex];
                if (choice < 0 || item.choices == null || choice >= item.choices.Length)
                {
                    message = "Choose one of the available answers first."; Refresh(); return;
                }
                correct = choice == item.correctChoice;
            }
            else
            {
                string entry = numericInput != null ? numericInput.text.Trim() : string.Empty;
                if (!float.TryParse(entry, NumberStyles.Float, CultureInfo.InvariantCulture, out float number) ||
                    float.IsNaN(number) || float.IsInfinity(number))
                {
                    message = "Enter a valid number using a decimal point (example: 12.5).";
                    Refresh(); return;
                }
                correct = Mathf.Abs(number - item.expectedNumber) <= Mathf.Max(0f, item.numericTolerance);
            }
            RecordAnswer(questionIndex, correct, correct ? "Correct. Review the explanation." : "Not correct. Review the explanation and continue.");
        }

        private void RecordAnswer(int index, bool correct, string nextMessage)
        {
            Attempt a = CurrentAttempt;
            if (a == null || index < 0 || index >= a.answered.Length || a.answered[index]) return;
            a.answered[index] = true;
            a.correct[index] = correct;
            message = nextMessage;
            FinalizeIfComplete();
            Refresh();
        }

        private void FinalizeIfComplete()
        {
            var bank = CurrentBank;
            var a = CurrentAttempt;
            if (bank == null || a == null || a.answered.Length == 0) return;
            int correct = 0;
            for (int i = 0; i < a.answered.Length; i++)
            {
                if (!a.answered[i]) return;
                if (a.correct[i]) correct++;
            }
            int score = Mathf.RoundToInt(correct * 100f / a.answered.Length);
            if (string.IsNullOrWhiteSpace(bank.bankId)) { message += " Bank ID missing: score was not saved."; return; }
            string bestKey = Key(ScorePrefix, bank.bankId);
            if (score > PlayerPrefs.GetInt(bestKey, -1)) PlayerPrefs.SetInt(bestKey, score);
            if (score >= bank.passingPercent)
                PlayerPrefs.SetInt(Key(PassedPrefix, bank.bankId), 1);
            PlayerPrefs.Save();
            message += score >= bank.passingPercent
                ? "  Assessment passed; local study record saved."
                : "  Assessment incomplete for passing score; retry from RESTART BANK.";
        }

        public void RestartBank()
        {
            CancelPractical(null);
            var b = CurrentBank;
            if (b == null || attempts == null) return;
            int count = b.items == null ? 0 : b.items.Length;
            attempts[bankIndex] = new Attempt
            {
                answered = new bool[count], correct = new bool[count], selectedChoice = new int[count]
            };
            for (int i = 0; i < count; i++) attempts[bankIndex].selectedChoice[i] = -1;
            questionIndex = 0;
            if (numericInput != null) numericInput.text = string.Empty;
            message = "Assessment restarted. Your previous best score remains saved.";
            Refresh();
        }

        private bool? EvaluatorResult(string id)
        {
            if (levelManager == null || levelManager.TargetResults == null) return null;
            foreach (var result in levelManager.TargetResults)
                if (string.Equals(result.targetId, id, StringComparison.Ordinal)) return result.satisfied;
            return null;
        }

        public void BeginPractical()
        {
            Resolve(); Subscribe();
            SparkAssessmentItem item = CurrentItem;
            Attempt a = CurrentAttempt;
            if (item == null || item.kind != SparkAssessmentItemKind.VerifiedPractical || a == null || a.answered[questionIndex]) return;
            CancelPractical(null);
            if (levelManager == null || levelManager.ActiveLevel == null || levelManager.ActiveLevel.Targets == null)
            {
                message = "Start a gameplay level with the required objective first."; Refresh(); return;
            }
            SparkLevelTarget found = null;
            foreach (SparkLevelTarget target in levelManager.ActiveLevel.Targets)
            {
                if (target == null || !target.IsConfigured() || string.IsNullOrWhiteSpace(target.TargetId) ||
                    target.Type != item.requiredTargetType) continue;
                bool? passed = EvaluatorResult(target.TargetId);
                if (passed.HasValue && !passed.Value) { found = target; break; }
            }
            if (found == null)
            {
                message = "No configured PENDING " + item.requiredTargetType + " evaluator target is available in this level. " +
                          "Choose a suitable level or reset one that has already passed.";
                Refresh(); return;
            }
            if (levelManager.IsFailed)
            {
                message = "Level has failed. Reset the level before starting a practical."; Refresh(); return;
            }
            practicalActive = true;
            practicalBankIndex = bankIndex;
            practicalQuestionIndex = questionIndex;
            practicalTargetId = found.TargetId;
            practicalLevelId = levelManager.ActiveLevel.LevelId;
            practicalCircuitId = observedCircuitId;
            startFrameSerial = frameSerial;
            message = "Practical started for evaluator target '" +
                      (string.IsNullOrWhiteSpace(found.DisplayName) ? found.TargetId : found.DisplayName) +
                      "'. Repair using normal controls; await a new solve and evaluation.";
            Refresh();
        }

        public void VerifyPractical()
        {
            if (!practicalActive)
                message = "Press BEGIN PRACTICAL on an unanswered practical question first.";
            else
                message = "Verification listens for a NEW evaluator update. This button cannot force a pass.";
            Refresh();
        }

        private void CheckPractical()
        {
            if (!practicalActive) return;
            if (levelManager == null || levelManager.ActiveLevel == null || levelManager.IsFailed ||
                !string.Equals(practicalLevelId, levelManager.ActiveLevel.LevelId, StringComparison.Ordinal))
            {
                CancelPractical("Level changed or failed. Practical cancelled."); return;
            }
            bool? official = EvaluatorResult(practicalTargetId);
            if (!official.HasValue || !official.Value)
            {
                message = "Evaluator still reports the practical objective as PENDING.";
                return;
            }
            if (requireFreshSolverFrame &&
                (frameSerial <= startFrameSerial || Time.unscaledTime - lastFrameTime > maxFrameAgeSeconds))
            {
                message = "Evaluator PASS observed, but fresh post-start solver evidence is missing or stale.";
                return;
            }
            int b = practicalBankIndex;
            int q = practicalQuestionIndex;
            if (b < 0 || q < 0 || attempts == null || b >= attempts.Length ||
                q >= attempts[b].answered.Length || attempts[b].answered[q])
            {
                CancelPractical("Practical state changed. No credit awarded."); return;
            }
            // Grant credit to the exact question originally started, even if the user navigated away.
            practicalActive = false;
            attempts[b].answered[q] = true;
            attempts[b].correct[q] = true;
            practicalBankIndex = practicalQuestionIndex = -1;
            practicalLevelId = practicalTargetId = practicalCircuitId = null;
            message = "Official evaluator confirmed a new PASS with required solver evidence. Practical credited.";
            int selectedBefore = bankIndex;
            bankIndex = b;
            FinalizeIfComplete();
            bankIndex = selectedBefore;
        }

        private void CancelPractical(string reason)
        {
            practicalActive = false;
            practicalBankIndex = practicalQuestionIndex = -1;
            practicalLevelId = practicalTargetId = practicalCircuitId = null;
            if (!string.IsNullOrEmpty(reason)) message = reason;
            Refresh();
        }

        public void ShowHint()
        {
            var item = CurrentItem;
            message = item == null ? "No question selected." :
                string.IsNullOrWhiteSpace(item.hint) ? "Read the question and inspect the existing level requirements." : item.hint;
            Refresh();
        }

        public void AskSpark()
        {
            Resolve();
            var item = CurrentItem;
            if (assistant == null || item == null)
            {
                message = "Spark AI Assistant or current question is unavailable."; Refresh(); return;
            }
            assistant.Ask("Teach the electrical concept behind this assessment question without giving me the exact answer: " +
                item.prompt + " Explain any example calculations as examples; use only verified game data for live measurements.");
        }

        public void OpenLearningPaths()
        {
            if (learningPaths != null)
                learningPaths.SendMessage("Show", SendMessageOptions.DontRequireReceiver);
            else
            {
                message = "Guided Learning Paths not linked; open the existing LEARNING PATHS panel.";
                Refresh();
            }
        }

        public void Refresh()
        {
            SparkAIAssessmentBank bank = CurrentBank;
            SparkAssessmentItem item = CurrentItem;
            Attempt a = CurrentAttempt;
            int total = a != null ? a.answered.Length : 0;
            int answered = 0, correct = 0;
            if (a != null)
                for (int i = 0; i < total; i++)
                {
                    if (a.answered[i]) answered++;
                    if (a.correct[i]) correct++;
                }
            if (bankTitleText != null) bankTitleText.text = bank != null ? bank.displayName : "NO ASSESSMENT BANKS";
            if (bankDescriptionText != null) bankDescriptionText.text = bank != null ? bank.description : "Run the Editor builder to create exam assets.";
            if (questionCountText != null) questionCountText.text = "BANK " + (bankIndex + 1) + "/" + (banks != null ? banks.Length : 0) +
                "    QUESTION " + (total > 0 ? questionIndex + 1 : 0) + "/" + total;
            if (questionTitleText != null) questionTitleText.text = item != null ? item.title : "NO QUESTION";
            if (questionPromptText != null) questionPromptText.text = item != null ? item.prompt : "Select a configured assessment.";
            bool isAnswered = a != null && item != null && a.answered[questionIndex];
            bool choice = item != null && item.kind == SparkAssessmentItemKind.MultipleChoice;
            bool numeric = item != null && item.kind == SparkAssessmentItemKind.Numeric;
            bool practical = item != null && item.kind == SparkAssessmentItemKind.VerifiedPractical;
            for (int i = 0; i < 4; i++)
            {
                bool exists = choice && item.choices != null && i < item.choices.Length && !string.IsNullOrEmpty(item.choices[i]);
                if (choiceButtons != null && i < choiceButtons.Length && choiceButtons[i] != null)
                {
                    choiceButtons[i].gameObject.SetActive(exists);
                    choiceButtons[i].interactable = exists && !isAnswered;
                }
                if (choiceLabels != null && i < choiceLabels.Length && choiceLabels[i] != null)
                    choiceLabels[i].text = exists ? (a != null && a.selectedChoice[questionIndex] == i ? "● " : "○ ") + item.choices[i] : string.Empty;
            }
            if (numericRoot != null) numericRoot.SetActive(numeric);
            if (numericInput != null) numericInput.interactable = numeric && !isAnswered;
            if (submitButton != null) submitButton.gameObject.SetActive(choice || numeric);
            if (submitButton != null) submitButton.interactable = !isAnswered;
            if (beginPracticalButton != null) beginPracticalButton.gameObject.SetActive(practical);
            if (beginPracticalButton != null) beginPracticalButton.interactable = !isAnswered;
            if (verifyButton != null) verifyButton.gameObject.SetActive(practical);
            if (resultText != null)
            {
                if (isAnswered)
                    resultText.text = (a.correct[questionIndex] ? "✓ CORRECT / VERIFIED\n" : "✕ NOT CORRECT\n") +
                        (item.explanation ?? string.Empty);
                else if (practical && practicalActive && practicalBankIndex == bankIndex && practicalQuestionIndex == questionIndex)
                    resultText.text = "PRACTICAL ACTIVE • Await a new completed solver frame and later official evaluator PASS.";
                else if (practical)
                    resultText.text = "Practical exam: begin only while the matching official objective is PENDING.";
                else resultText.text = "Choose an answer. Feedback appears after submission.";
            }
            if (scoreText != null)
            {
                int percent = total > 0 ? Mathf.RoundToInt(100f * correct / total) : 0;
                int best = bank != null ? PlayerPrefs.GetInt(Key(ScorePrefix, bank.bankId), -1) : -1;
                scoreText.text = "ANSWERED " + answered + "/" + total + "  •  SESSION " + percent +
                    "%  •  BEST " + (best < 0 ? "—" : best + "%") +
                    "  •  PASS ≥ " + (bank != null ? bank.passingPercent : 70) + "%";
            }
            if (progressFill != null) progressFill.fillAmount = total > 0 ? (float)answered / total : 0f;
            if (statusText != null) statusText.text = message;
        }
    }
}
