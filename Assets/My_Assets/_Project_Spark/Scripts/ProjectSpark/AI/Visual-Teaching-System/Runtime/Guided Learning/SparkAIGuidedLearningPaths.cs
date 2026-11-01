using System;
using System.Collections.Generic;
using System.Text;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Guided learning paths built from authored SparkVisualLesson assets and the
    /// EXISTING training system's evaluator-verified PlayerPrefs achievement keys.
    /// Never writes a training achievement, gameplay result, circuit measurement,
    /// level unlock, or currency. Lesson study is a user acknowledgement only.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIGuidedLearningPaths : MonoBehaviour
    {
        private const string StagePrefix = "ProjectSpark.GuidedPaths.v1/";
        private const string SelectionKey = "ProjectSpark.GuidedPaths.Selected.v1";
        [Header("Existing systems")]
        [SerializeField] private SparkVisualTeachingManager teaching;
        [SerializeField] private SparkAIInteractiveTrainingChallenges training;
        [SerializeField] private SparkAILessonLibrary library;
        [SerializeField] private SparkAIUnifiedAssistant assistant;
        [SerializeField] private LevelGamePlayManager levelManager;
        [Header("Editable course assets")]
        [SerializeField] private SparkGuidedLearningPathAsset[] courses = Array.Empty<SparkGuidedLearningPathAsset>();
        [Header("Editor generated UI")]
        [SerializeField] private GameObject card;
        [SerializeField] private TMP_Text courseTitleText;
        [SerializeField] private TMP_Text courseDescriptionText;
        [SerializeField] private TMP_Text stepCountText;
        [SerializeField] private TMP_Text stepTitleText;
        [SerializeField] private TMP_Text stepDescriptionText;
        [SerializeField] private TMP_Text stepListText;
        [SerializeField] private TMP_Text evidenceText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Image progressFill;
        [SerializeField] private Button startButton;
        [SerializeField] private Button studiedButton;
        [SerializeField] private Button verifyButton;

        private int selectedCourse;
        private int selectedStage;
        private LevelGamePlayManager subscribedLevel;
        private string openedLessonCourse;
        private string openedLessonStage;
        private string boundPracticeCourse;
        private string boundPracticeStage;
        private string boundLevelId;
        private string boundTargetId;
        private string boundTargetTitle;
        private string message = "Choose a course and start its first lesson.";

        private void Awake()
        {
            Resolve();
            if (card != null) card.SetActive(false);
            selectedCourse = Mathf.Max(0, PlayerPrefs.GetInt(SelectionKey, 0));
            selectedCourse = Mathf.Clamp(selectedCourse, 0, Mathf.Max(0, CourseCount - 1));
        }

        private void OnEnable()
        {
            Resolve();
            Subscribe();
            Refresh();
        }

        private void OnDisable() { Unsubscribe(); }
        private void OnDestroy() { Unsubscribe(); }

        private void Resolve()
        {
            if (teaching == null) teaching = FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            if (training == null) training = FindFirstObjectByType<SparkAIInteractiveTrainingChallenges>(FindObjectsInactive.Include);
            if (library == null) library = FindFirstObjectByType<SparkAILessonLibrary>(FindObjectsInactive.Include);
            if (assistant == null) assistant = FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            if (levelManager == null) levelManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
        }

        private void Subscribe()
        {
            if (subscribedLevel == levelManager) return;
            Unsubscribe();
            subscribedLevel = levelManager;
            if (subscribedLevel == null) return;
            subscribedLevel.LevelStarted += OnLevelRebuilt;
            subscribedLevel.LevelReset += OnLevelRebuilt;
            subscribedLevel.LevelEvaluationChanged += OnEvaluated;
            subscribedLevel.LevelCompleted += OnEvaluated;
        }

        private void Unsubscribe()
        {
            if (subscribedLevel == null) return;
            subscribedLevel.LevelStarted -= OnLevelRebuilt;
            subscribedLevel.LevelReset -= OnLevelRebuilt;
            subscribedLevel.LevelEvaluationChanged -= OnEvaluated;
            subscribedLevel.LevelCompleted -= OnEvaluated;
            subscribedLevel = null;
        }

        private void OnLevelRebuilt(SparkLevelDefinition unused)
        {
            ClearPracticeBinding();
            message = "Level started or reset. Start practice again for its current objectives.";
            Refresh();
        }

        private void OnEvaluated(SparkLevelDefinition unused) { Refresh(); }

        private int CourseCount => courses == null ? 0 : courses.Length;
        private SparkGuidedLearningPathAsset CurrentCourse()
        {
            return selectedCourse >= 0 && selectedCourse < CourseCount ? courses[selectedCourse] : null;
        }
        private SparkLearningPathStage CurrentStage()
        {
            var course = CurrentCourse();
            return course != null && course.stages != null && selectedStage >= 0 &&
                   selectedStage < course.stages.Length ? course.stages[selectedStage] : null;
        }

        private static string SavedStageKey(string pathId, string stageId)
        {
            string a = pathId ?? string.Empty, b = stageId ?? string.Empty;
            return StagePrefix + a.Length + ":" + a + "/" + b.Length + ":" + b;
        }

        private static string TrainingAchievementKey(string levelId, string targetId)
        {
            // EXACT same format as SparkAIInteractiveTrainingChallenges.ProgressKey.
            string a = levelId ?? string.Empty, b = targetId ?? string.Empty;
            return "ProjectSpark.Training.v1/" + a.Length + ":" + a + "/" + b.Length + ":" + b;
        }

        private static bool ValidCourse(SparkGuidedLearningPathAsset course)
        {
            return course != null && !string.IsNullOrWhiteSpace(course.pathId) &&
                   course.stages != null && course.stages.Length > 0;
        }

        private static bool ValidStep(SparkLearningPathStage stage)
        {
            return stage != null && !string.IsNullOrWhiteSpace(stage.id);
        }

        private static bool Completed(SparkGuidedLearningPathAsset course, SparkLearningPathStage stage)
        {
            return ValidCourse(course) && ValidStep(stage) &&
                   PlayerPrefs.GetInt(SavedStageKey(course.pathId, stage.id), 0) == 1;
        }

        private static void SaveCompleted(SparkGuidedLearningPathAsset course, SparkLearningPathStage stage)
        {
            PlayerPrefs.SetInt(SavedStageKey(course.pathId, stage.id), 1);
            PlayerPrefs.Save();
        }

        private static bool Unlocked(SparkGuidedLearningPathAsset course, int index)
        {
            if (!ValidCourse(course) || index < 0 || index >= course.stages.Length) return false;
            for (int i = 0; i < index; i++)
                if (!Completed(course, course.stages[i])) return false;
            return true;
        }

        public void Show() { Resolve(); if (card != null) card.SetActive(true); Refresh(); }
        public void Hide() { if (card != null) card.SetActive(false); }
        public void Toggle() { if (card != null && card.activeSelf) Hide(); else Show(); }

        public void PreviousCourse() { SwitchCourse(-1); }
        public void NextCourse() { SwitchCourse(1); }
        private void SwitchCourse(int delta)
        {
            if (CourseCount == 0) { Refresh(); return; }
            selectedCourse = (selectedCourse + delta + CourseCount) % CourseCount;
            selectedStage = 0;
            ForgetTransientSelection();
            PlayerPrefs.SetInt(SelectionKey, selectedCourse);
            PlayerPrefs.Save();
            message = "Course selected. Existing study records are preserved.";
            Refresh();
        }
        public void PreviousStage() { MoveStage(-1); }
        public void NextStage() { MoveStage(1); }
        private void MoveStage(int delta)
        {
            var course = CurrentCourse();
            if (!ValidCourse(course)) return;
            selectedStage = Mathf.Clamp(selectedStage + delta, 0, course.stages.Length - 1);
            ForgetTransientSelection();
            Refresh();
        }

        private void ForgetTransientSelection()
        {
            openedLessonCourse = openedLessonStage = null;
            ClearPracticeBinding();
        }

        private void ClearPracticeBinding()
        {
            boundPracticeCourse = boundPracticeStage = null;
            boundLevelId = boundTargetId = boundTargetTitle = null;
        }

        public void StartCurrentStep()
        {
            Resolve();
            var course = CurrentCourse();
            var stage = CurrentStage();
            if (!ValidCourse(course) || !ValidStep(stage))
            {
                message = "No course stage configured."; Refresh(); return;
            }
            if (!Unlocked(course, selectedStage))
            {
                message = "This stage is locked. Finish the earlier course stages first.";
                Refresh(); return;
            }
            if (stage.kind == SparkLearningPathStageKind.Lesson)
            {
                if (teaching == null || stage.lesson == null || stage.lesson.steps == null || stage.lesson.steps.Length == 0)
                {
                    message = "Visual Teaching Manager or authored lesson is missing. Check the course asset.";
                    Refresh(); return;
                }
                teaching.StartLesson(stage.lesson);
                openedLessonCourse = course.pathId;
                openedLessonStage = stage.id;
                message = "Lesson opened. Study its steps, then press MARK STUDIED. This is not a verified gameplay award.";
            }
            else
            {
                BeginVerifiedPractice(course, stage);
            }
            Refresh();
        }

        private bool? ObjectivePassed(string targetId)
        {
            if (levelManager == null || levelManager.TargetResults == null) return null;
            foreach (var result in levelManager.TargetResults)
                if (string.Equals(result.targetId, targetId, StringComparison.Ordinal)) return result.satisfied;
            return null;
        }

        private void BeginVerifiedPractice(SparkGuidedLearningPathAsset course, SparkLearningPathStage stage)
        {
            ClearPracticeBinding();
            if (levelManager == null || levelManager.ActiveLevel == null || levelManager.ActiveLevel.Targets == null)
            {
                message = "Start a gameplay level with a matching configured objective."; return;
            }
            SparkLevelTarget chosen = null;
            // Prefer an unfinished matching target with an authoritative evaluator result.
            foreach (SparkLevelTarget target in levelManager.ActiveLevel.Targets)
            {
                if (target == null || !target.IsConfigured() || target.Type != stage.requiredTargetType ||
                    string.IsNullOrWhiteSpace(target.TargetId)) continue;
                bool? passed = ObjectivePassed(target.TargetId);
                if (passed.HasValue && !passed.Value) { chosen = target; break; }
                if (chosen == null) chosen = target;
            }
            if (chosen == null)
            {
                message = "This level has no configured " + stage.requiredTargetType + " objective. Choose a suitable level.";
                return;
            }
            boundPracticeCourse = course.pathId;
            boundPracticeStage = stage.id;
            boundLevelId = levelManager.ActiveLevel.LevelId;
            boundTargetId = chosen.TargetId;
            boundTargetTitle = string.IsNullOrWhiteSpace(chosen.DisplayName) ? chosen.TargetId : chosen.DisplayName;
            if (training != null) training.Show();
            message = "Training opened. Select objective '" + boundTargetTitle + "' in AI TRAINING, finish its verified challenge, then return and click CHECK VERIFIED.";
        }

        public void MarkStudied()
        {
            var course = CurrentCourse();
            var stage = CurrentStage();
            if (!ValidCourse(course) || !ValidStep(stage) || stage.kind != SparkLearningPathStageKind.Lesson ||
                !Unlocked(course, selectedStage)) { message = "Select an unlocked lesson stage."; Refresh(); return; }
            if (!string.Equals(openedLessonCourse, course.pathId, StringComparison.Ordinal) ||
                !string.Equals(openedLessonStage, stage.id, StringComparison.Ordinal))
            {
                message = "Open this lesson from the path before marking it studied.";
                Refresh(); return;
            }
            SaveCompleted(course, stage);
            message = "Study acknowledged. This is an educational checkpoint, not solver/evaluator verification.";
            Refresh();
        }

        public void CheckVerified()
        {
            var course = CurrentCourse();
            var stage = CurrentStage();
            if (!ValidCourse(course) || !ValidStep(stage) || stage.kind != SparkLearningPathStageKind.VerifiedPractice ||
                !Unlocked(course, selectedStage)) { message = "Choose an unlocked practice stage."; Refresh(); return; }
            if (levelManager == null || levelManager.ActiveLevel == null ||
                boundLevelId == null || !string.Equals(levelManager.ActiveLevel.LevelId, boundLevelId, StringComparison.Ordinal) ||
                !string.Equals(boundPracticeCourse, course.pathId, StringComparison.Ordinal) ||
                !string.Equals(boundPracticeStage, stage.id, StringComparison.Ordinal))
            {
                message = "Press START PRACTICE in the current level before checking verified training.";
                Refresh(); return;
            }
            if (PlayerPrefs.GetInt(TrainingAchievementKey(boundLevelId, boundTargetId), 0) != 1)
            {
                bool? passed = ObjectivePassed(boundTargetId);
                message = "No saved verified training achievement for '" + boundTargetTitle + "'. " +
                    (passed.HasValue && passed.Value ? "The evaluator currently shows PASS, but that alone is not a training award. " : "") +
                    "Finish its AI TRAINING challenge with Save Verified Training Achievements enabled.";
                Refresh(); return;
            }
            // The trusted training module writes this key only after its own
            // fresh-evaluation requirements. We never synthesize the key here.
            SaveCompleted(course, stage);
            message = "Course practice credited from the saved evaluator-verified training achievement. " +
                "This does not imply the objective is passing in the current circuit.";
            Refresh();
        }

        public void OpenTraining()
        {
            Resolve();
            if (training != null) training.Show();
            else { message = "AI Training panel not found."; Refresh(); }
        }
        public void OpenLibrary()
        {
            Resolve();
            if (library != null) library.Show();
            else { message = "Lesson Library panel not found."; Refresh(); }
        }
        public void AskSpark()
        {
            Resolve();
            var stage = CurrentStage();
            if (assistant == null || stage == null) { message = "Ask Spark Assistant unavailable."; Refresh(); return; }
            assistant.Ask("Teach the concept '" + stage.title + "' using only the current level's verified facts. " +
                "Distinguish an educational example from solver measurements. What should I study or verify next?");
        }

        public void Refresh()
        {
            var course = CurrentCourse();
            var stage = CurrentStage();
            bool valid = ValidCourse(course);
            bool validStage = ValidStep(stage);
            int done = 0, total = valid ? course.stages.Length : 0;
            if (valid) foreach (var item in course.stages) if (Completed(course, item)) done++;
            if (courseTitleText != null) courseTitleText.text = valid ? course.title : "NO LEARNING PATHS";
            if (courseDescriptionText != null) courseDescriptionText.text = valid ? course.description : "Create paths using the Project Spark Editor menu.";
            if (stepCountText != null) stepCountText.text = valid ?
                "COURSE " + (selectedCourse + 1) + "/" + CourseCount + "     STEP " + (selectedStage + 1) + "/" + total +
                "     COMPLETED " + done + "/" + total : "NO COURSE";
            if (progressFill != null) progressFill.fillAmount = total > 0 ? (float)done / total : 0f;
            if (stepTitleText != null) stepTitleText.text = validStage ? stage.title : "SELECT A STAGE";
            bool unlocked = validStage && Unlocked(course, selectedStage);
            bool isPractice = validStage && stage.kind == SparkLearningPathStageKind.VerifiedPractice;
            if (stepDescriptionText != null) stepDescriptionText.text = validStage ?
                stage.description + "\n\n" + (Completed(course, stage) ? "COMPLETE" : unlocked ?
                    (isPractice ? "PRACTICE: EVALUATOR-VERIFIED ACHIEVEMENT REQUIRED" : "LESSON: MARK STUDIED AFTER REVIEW") : "LOCKED: FINISH PREVIOUS STEPS") : "";
            if (startButton != null) startButton.interactable = validStage && unlocked;
            if (studiedButton != null) studiedButton.interactable = validStage && unlocked && !isPractice && !Completed(course, stage);
            if (verifyButton != null) verifyButton.interactable = validStage && unlocked && isPractice && !Completed(course, stage);
            var list = new StringBuilder();
            if (valid)
            {
                for (int i = 0; i < course.stages.Length && i < 20; i++)
                {
                    var s = course.stages[i];
                    if (!ValidStep(s)) continue;
                    list.Append(i == selectedStage ? "> " : "  ");
                    list.Append(Completed(course, s) ? "[DONE] " : Unlocked(course, i) ? "[READY] " : "[LOCKED] ");
                    list.AppendLine(s.title);
                }
            }
            if (stepListText != null) stepListText.text = list.ToString();
            if (evidenceText != null)
            {
                string level = levelManager != null && levelManager.ActiveLevel != null ? levelManager.ActiveLevel.LevelId : "none";
                string official = boundTargetId == null ? "No practice target bound" :
                    (ObjectivePassed(boundTargetId).HasValue ?
                        (ObjectivePassed(boundTargetId).Value ? "PASS" : "PENDING") : "NOT REPORTED");
                evidenceText.text = "ACTIVE LEVEL: " + level + "   |   PRACTICE: " + official +
                    "\nBOUND TARGET: " + (boundTargetTitle ?? "none") +
                    "   |   TRAINING RECORD: " + (boundTargetId != null && PlayerPrefs.GetInt(TrainingAchievementKey(boundLevelId, boundTargetId), 0) == 1 ? "VERIFIED" : "NOT FOUND");
            }
            if (statusText != null) statusText.text = message;
        }
    }
}
