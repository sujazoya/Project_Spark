using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;
using ProjectSpark.AI.Tutorial;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Read-only, evaluator-driven training challenges. Never edits a circuit, awards currency,
    /// or marks a game level complete. All calls/events occur on Unity's main thread.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIInteractiveTrainingChallenges : MonoBehaviour
    {
        private enum Phase { Idle, Study, Practice, AwaitVerification, Completed }
        private enum Tier { Foundation, Applied, Advanced }

        private sealed class Challenge
        {
            public string id;
            public string name;
            public SparkLevelTarget target;
            public Tier tier;
        }

        [Header("Existing project systems")]
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkVerifiedElectricalPublisher publisher;
        [SerializeField] private SparkAIUnifiedAssistant assistant;
        [SerializeField] private SparkWorldAnnotationRenderer worldRenderer;

        [Header("Editor-generated UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text tierText;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text guideText;
        [SerializeField] private TMP_Text evidenceText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Image progressFill;
        [SerializeField] private Button beginButton;
        [SerializeField] private Button studiedButton;
        [SerializeField] private Button hintButton;
        [SerializeField] private Button askButton;
        [SerializeField] private Button highlightButton;
        [SerializeField] private Button verifyButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;

        [Header("Verified completion rules")]
        [Tooltip("When enabled, requires a fresh completed solver frame as well as a later evaluator update.")]
        [SerializeField] private bool requireFreshSolverFrame = true;
        [SerializeField, Min(1f)] private float maxFrameAgeSeconds = 15f;
        [Tooltip("Stores TRAINING achievements only. Never changes the actual level evaluator or unlocks.")]
        [SerializeField] private bool saveVerifiedTrainingAchievements = true;

        private readonly List<Challenge> challenges = new List<Challenge>();
        private readonly List<SparkWorldAnnotationRenderer.Annotation> ownedCues =
            new List<SparkWorldAnnotationRenderer.Annotation>();
        private SparkVerifiedElectricalPublisher subscribedPublisher;
        private LevelGamePlayManager subscribedLevel;
        private SparkVerifiedElectricalFrame latestFrame;
        private int selected;
        private Phase phase;
        private int hintStage;
        private int frameSerial;
        private int evalSerial;
        private int startedFrameSerial;
        private int startedEvalSerial;
        private float lastFrameAt = -10000f;
        private string observedCircuitId;
        private string activeLevelId;
        private string startedLevelId;
        private string startedCircuitId;
        private string activeChallengeId;
        private string statusMessage;
        private bool levelFailed;
        private int lastRevision;

        public bool HasActiveChallenge => !string.IsNullOrEmpty(activeChallengeId);
        public int ChallengeCount => challenges.Count;
        public bool RequireFreshSolverFrame { get => requireFreshSolverFrame; set => requireFreshSolverFrame = value; }

        private void Awake()
        {
            Resolve();
            if (panel != null) panel.SetActive(false);
        }

        private void OnEnable()
        {
            Resolve();
            Subscribe();
            RebuildChallenges();
        }

        private void Start()
        {
            Resolve();
            Subscribe();
            RebuildChallenges();
        }

        private void OnDisable()
        {
            Unsubscribe();
            ClearOwnedCues();
        }

        private void OnDestroy() { Unsubscribe(); }

        private void Resolve()
        {
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            if (publisher == null)
                publisher = FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            if (assistant == null)
                assistant = FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            if (worldRenderer == null)
                worldRenderer = FindFirstObjectByType<SparkWorldAnnotationRenderer>(FindObjectsInactive.Include);
        }

        private void Subscribe()
        {
            if (subscribedPublisher != publisher)
            {
                if (subscribedPublisher != null)
                    subscribedPublisher.FramePublished -= OnFramePublished;
                subscribedPublisher = publisher;
                if (subscribedPublisher != null)
                    subscribedPublisher.FramePublished += OnFramePublished;
            }
            if (subscribedLevel != levelManager)
            {
                if (subscribedLevel != null) UnsubscribeLevel();
                subscribedLevel = levelManager;
                if (subscribedLevel != null)
                {
                    subscribedLevel.LevelStarted += OnLevelRebuilt;
                    subscribedLevel.LevelReset += OnLevelRebuilt;
                    subscribedLevel.LevelEvaluationChanged += OnEvaluated;
                    subscribedLevel.LevelCompleted += OnEvaluated;
                    subscribedLevel.LevelFailed += OnFailed;
                }
            }
        }

        private void UnsubscribeLevel()
        {
            if (subscribedLevel == null) return;
            subscribedLevel.LevelStarted -= OnLevelRebuilt;
            subscribedLevel.LevelReset -= OnLevelRebuilt;
            subscribedLevel.LevelEvaluationChanged -= OnEvaluated;
            subscribedLevel.LevelCompleted -= OnEvaluated;
            subscribedLevel.LevelFailed -= OnFailed;
            subscribedLevel = null;
        }

        private void Unsubscribe()
        {
            if (subscribedPublisher != null)
                subscribedPublisher.FramePublished -= OnFramePublished;
            subscribedPublisher = null;
            UnsubscribeLevel();
        }

        private void OnLevelRebuilt(SparkLevelDefinition level)
        {
            ResetAttempt("Level started or reset. Choose a fresh training challenge.");
            activeLevelId = null;
            RebuildChallenges();
        }

        private void OnFailed(SparkLevelDefinition level, string reason)
        {
            levelFailed = true;
            if (HasActiveChallenge && phase != Phase.Completed)
            {
                phase = Phase.Practice;
                statusMessage = "Level failed. Reset or restart the level before verification.";
            }
            Refresh();
        }

        private void OnEvaluated(SparkLevelDefinition level)
        {
            evalSerial++;
            if (IsActiveLevelDifferent())
            {
                ResetAttempt("Level identity changed. Previous challenge cancelled.");
                RebuildChallenges();
                return;
            }
            RebuildChallenges();
            VerifyIfReady();
            Refresh();
        }

        private void OnFramePublished(SparkVerifiedElectricalFrame frame) { SubmitVerifiedFrame(frame); }

        /// <summary>Alternative for a trusted solver that does not use the publisher. Main thread only.</summary>
        public bool SubmitVerifiedFrame(SparkVerifiedElectricalFrame frame)
        {
            if (!ValidFrame(frame)) return false;
            string id = frame.circuitId ?? string.Empty;
            if (observedCircuitId != null &&
                !string.Equals(observedCircuitId, id, StringComparison.Ordinal))
            {
                ResetAttempt("Circuit changed. Training verification was cancelled.");
                // A new circuit may reuse revision numbers.
                lastRevision = 0;
            }
            observedCircuitId = id;
            if (frame.revision > 0 && frame.revision <= lastRevision) return false;
            if (frame.revision > 0) lastRevision = frame.revision;
            latestFrame = frame.Copy();
            frameSerial++;
            lastFrameAt = Time.unscaledTime;
            VerifyIfReady();
            Refresh();
            return true;
        }

        private static bool ValidFrame(SparkVerifiedElectricalFrame frame)
        {
            if (frame == null || frame.revision < 0 ||
                frame.circuitId != null && frame.circuitId.Length > 80 ||
                frame.readings == null || frame.readings.Count > 32)
                return false;
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (SparkVerifiedElectricalReading reading in frame.readings)
            {
                if (reading == null || string.IsNullOrWhiteSpace(reading.anchorId) ||
                    reading.anchorId.Length > 80 || !ids.Add(reading.anchorId) ||
                    !Finite(reading.voltageVolts) || !Finite(reading.currentAmps) ||
                    !Finite(reading.resistanceOhms) || reading.resistanceOhms < 0f ||
                    !Enum.IsDefined(typeof(SparkElectricalCondition), reading.condition))
                    return false;
            }
            return true;
        }

        private static bool Finite(float n) { return !float.IsNaN(n) && !float.IsInfinity(n); }

        private bool IsActiveLevelDifferent()
        {
            string now = levelManager != null && levelManager.ActiveLevel != null
                ? levelManager.ActiveLevel.LevelId : null;
            return !string.Equals(activeLevelId, now, StringComparison.Ordinal);
        }

        private void ResetAttempt(string message)
        {
            phase = Phase.Idle;
            activeChallengeId = null;
            startedLevelId = null;
            startedCircuitId = null;
            startedEvalSerial = startedFrameSerial = 0;
            hintStage = 0;
            frameSerial = evalSerial = lastRevision = 0;
            latestFrame = null;
            observedCircuitId = null;
            lastFrameAt = -10000f;
            levelFailed = false;
            statusMessage = message;
            ClearOwnedCues();
            Refresh();
        }

        private void RebuildChallenges()
        {
            string oldSelection = selected >= 0 && selected < challenges.Count
                ? challenges[selected].id : null;
            challenges.Clear();
            if (levelManager == null || levelManager.ActiveLevel == null)
            {
                activeLevelId = null;
                selected = 0;
                Refresh();
                return;
            }
            activeLevelId = levelManager.ActiveLevel.LevelId;
            if (levelManager.ActiveLevel.Targets != null)
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (SparkLevelTarget target in levelManager.ActiveLevel.Targets)
                {
                    if (target == null || string.IsNullOrWhiteSpace(target.TargetId) ||
                        !seen.Add(target.TargetId) || challenges.Count >= 40) continue;
                    challenges.Add(new Challenge {
                        target = target,
                        id = target.TargetId,
                        name = string.IsNullOrWhiteSpace(target.DisplayName)
                            ? target.TargetId : target.DisplayName,
                        tier = DetermineTier(target)
                    });
                }
            }
            // Difficulty rises from Foundation to Applied to Advanced.
            // List.Sort is stable enough for tier grouping only if ties use original order,
            // so retain the original target order with an insertion-based tier pass.
            for (int i = 1; i < challenges.Count; i++)
            {
                Challenge candidate = challenges[i];
                int j = i - 1;
                while (j >= 0 && challenges[j].tier > candidate.tier)
                {
                    challenges[j + 1] = challenges[j];
                    j--;
                }
                challenges[j + 1] = candidate;
            }
            selected = 0;
            for (int i = 0; i < challenges.Count; i++)
            {
                if (challenges[i].id == oldSelection ||
                    oldSelection == null && challenges[i].id == activeChallengeId)
                {
                    selected = i;
                    break;
                }
            }
            Refresh();
        }

        private static Tier DetermineTier(SparkLevelTarget target)
        {
            switch (target.Type)
            {
                case SparkLevelTarget.TargetType.LEDOn:
                case SparkLevelTarget.TargetType.ComponentConducting:
                case SparkLevelTarget.TargetType.ComponentPowered:
                case SparkLevelTarget.TargetType.VoltagePresent:
                case SparkLevelTarget.TargetType.TerminalPowered:
                    return Tier.Foundation;
                case SparkLevelTarget.TargetType.CurrentRange:
                case SparkLevelTarget.TargetType.VoltageRange:
                case SparkLevelTarget.TargetType.PowerRange:
                    return Tier.Applied;
                default:
                    return Tier.Advanced;
            }
        }

        private Challenge Current()
        {
            if (!string.IsNullOrEmpty(activeChallengeId))
            {
                foreach (Challenge c in challenges)
                    if (c.id == activeChallengeId) return c;
            }
            return selected >= 0 && selected < challenges.Count ? challenges[selected] : null;
        }

        private bool? OfficialResult(string targetId)
        {
            if (levelManager == null || levelManager.TargetResults == null) return null;
            foreach (var result in levelManager.TargetResults)
                if (string.Equals(result.targetId, targetId, StringComparison.Ordinal))
                    return result.satisfied;
            return null;
        }

        public void BeginChallenge()
        {
            Resolve();
            Challenge c = Current();
            if (c == null || c.target == null || levelManager == null || levelManager.ActiveLevel == null)
            {
                statusMessage = "Start a level with configured objectives.";
                Refresh(); return;
            }
            if (levelManager.IsFailed || levelFailed)
            {
                statusMessage = "Restart the failed level before starting training.";
                Refresh(); return;
            }
            if (!c.target.IsConfigured())
            {
                statusMessage = "This objective is not configured. Ask the level designer to complete its setup.";
                Refresh(); return;
            }
            bool? initialResult = OfficialResult(c.id);
            if (!initialResult.HasValue)
            {
                statusMessage = "Waiting for the first official objective evaluation.";
                Refresh(); return;
            }
            if (initialResult.Value)
            {
                statusMessage = "This target is already PASS. Choose a pending objective, or reset the level to practise it.";
                Refresh(); return;
            }
            activeChallengeId = c.id;
            startedLevelId = activeLevelId;
            startedCircuitId = observedCircuitId;
            startedFrameSerial = frameSerial;
            startedEvalSerial = evalSerial;
            phase = Phase.Study;
            hintStage = 0;
            statusMessage = "Study the actual objective requirements, then press STUDIED.";
            ClearOwnedCues();
            Refresh();
        }

        public void AcknowledgeStudy()
        {
            if (phase != Phase.Study) return;
            phase = Phase.Practice;
            statusMessage = "Use the normal circuit controls. Ask Spark or request a progressive hint.";
            Refresh();
        }

        public void NextHint()
        {
            if (!HasActiveChallenge || phase == Phase.Completed) return;
            hintStage = Mathf.Clamp(hintStage + 1, 1, 3);
            Refresh();
        }

        public void AskSpark()
        {
            Challenge c = Current();
            Resolve();
            if (c == null || assistant == null)
            {
                statusMessage = "SparkAIUnifiedAssistant is not available.";
                Refresh(); return;
            }
            // The assistant builds its own verified world context and never receives fake measurements.
            assistant.Ask("Explain how to investigate the current level objective '" +
                Safe(c.name, 70) + "'. Use only verified circuit results and configured requirements. " +
                "What should I check next?");
            statusMessage = "Opened an Ask Spark question for this objective.";
            Refresh();
        }

        public void VerifyChallenge()
        {
            if (!HasActiveChallenge || (phase != Phase.Practice && phase != Phase.AwaitVerification))
            {
                statusMessage = "Begin a challenge and mark it STUDIED before verifying.";
                Refresh(); return;
            }
            phase = Phase.AwaitVerification;
            statusMessage = "Awaiting a new official evaluation" +
                (requireFreshSolverFrame ? " and a completed solver frame" : "") + ".";
            VerifyIfReady();
            Refresh();
        }

        private void VerifyIfReady()
        {
            if (phase != Phase.AwaitVerification || !HasActiveChallenge) return;
            if (levelManager == null || levelManager.ActiveLevel == null ||
                levelManager.IsFailed || levelFailed ||
                !string.Equals(startedLevelId, levelManager.ActiveLevel.LevelId, StringComparison.Ordinal))
            {
                statusMessage = "Level unavailable, failed, or changed; verification blocked.";
                return;
            }
            if (evalSerial <= startedEvalSerial)
            {
                statusMessage = "Waiting for an evaluator update AFTER challenge start.";
                return;
            }
            if (requireFreshSolverFrame)
            {
                if (latestFrame == null || frameSerial <= startedFrameSerial ||
                    Time.unscaledTime - lastFrameAt > maxFrameAgeSeconds ||
                    startedCircuitId != null && !string.Equals(startedCircuitId, observedCircuitId, StringComparison.Ordinal))
                {
                    statusMessage = "Waiting for a fresh solver frame for the same circuit.";
                    return;
                }
            }
            bool? passed = OfficialResult(activeChallengeId);
            if (!passed.HasValue)
            {
                statusMessage = "Evaluator has not reported this objective. Cannot claim success.";
                return;
            }
            if (!passed.Value)
            {
                statusMessage = "Evaluator still reports PENDING. Practice and verify again.";
                return;
            }
            // A satisfied target is an evaluator claim, not a derived current/voltage estimate.
            phase = Phase.Completed;
            statusMessage = "TRAINING VERIFIED: evaluator marked this objective PASSED.";
            if (saveVerifiedTrainingAchievements)
            {
                PlayerPrefs.SetInt(ProgressKey(activeLevelId, activeChallengeId), 1);
                PlayerPrefs.Save();
            }
            ClearOwnedCues();
        }

        private static string ProgressKey(string level, string target)
        {
            // Length-delimited encoding avoids collisions (a|bc vs ab|c).
            string a = level ?? string.Empty;
            string b = target ?? string.Empty;
            return "ProjectSpark.Training.v1/" + a.Length + ":" + a + "/" + b.Length + ":" + b;
        }

        private bool IsRecorded(Challenge challenge)
        {
            return saveVerifiedTrainingAchievements &&
                PlayerPrefs.GetInt(ProgressKey(activeLevelId, challenge.id), 0) == 1;
        }

        public void CancelChallenge()
        {
            activeChallengeId = null;
            startedCircuitId = startedLevelId = null;
            phase = Phase.Idle;
            hintStage = 0;
            statusMessage = "Training attempt closed. Circuit and level are unchanged.";
            ClearOwnedCues();
            Refresh();
        }

        public void PreviousChallenge()
        {
            if (HasActiveChallenge || challenges.Count == 0) return;
            selected = (selected + challenges.Count - 1) % challenges.Count;
            statusMessage = null;
            Refresh();
        }

        public void NextChallenge()
        {
            if (challenges.Count == 0) return;
            if (HasActiveChallenge && phase != Phase.Completed) return;
            if (HasActiveChallenge)
            {
                activeChallengeId = null;
                phase = Phase.Idle;
                ClearOwnedCues();
            }
            selected = (selected + 1) % challenges.Count;
            // Prefer a challenge without a recorded achievement.
            for (int i = 0; i < challenges.Count; i++)
            {
                int index = (selected + i) % challenges.Count;
                if (!IsRecorded(challenges[index])) { selected = index; break; }
            }
            statusMessage = "Choose BEGIN to start the next practice objective.";
            Refresh();
        }

        public void FocusComponent()
        {
            Challenge c = Current();
            if (c == null || c.target == null || string.IsNullOrWhiteSpace(c.target.TargetComponentId))
            {
                statusMessage = "No objective-bound component ID available. Nothing to highlight.";
                Refresh(); return;
            }
            string id = c.target.TargetComponentId;
            // Only highlight an EXACT registered anchor. Never infer target binding from names.
            if (!SparkTeachingAnchor.TryFind(id, out _))
            {
                statusMessage = "No matching SparkTeachingAnchor for component ID: " + Safe(id, 70);
                Refresh(); return;
            }
            Resolve();
            ClearOwnedCues();
            if (worldRenderer == null)
            {
                statusMessage = "World annotation renderer unavailable.";
                Refresh(); return;
            }
            var point = new SparkVisualPoint { space = SparkPointSpace.WorldAnchor, anchorId = id };
            var cmd = new SparkVisualCommand {
                shape = SparkVisualShape.Ring,
                from = point, to = point,
                normalizedSize = new Vector2(.11f, .14f),
                thickness = 4f,
                color = new Color(.28f, .9f, 1f, 1f),
                drawSeconds = .35f,
                persistent = true
            };
            if (!SparkTeachingValidator.Validate(cmd, out string reason))
            {
                statusMessage = "Highlight rejected: " + reason;
                Refresh(); return;
            }
            var annotation = worldRenderer.Add(cmd);
            if (annotation != null)
            {
                ownedCues.Add(annotation);
                worldRenderer.SetProgress(annotation, 1f);
                statusMessage = "Highlighted the configured target anchor: " + Safe(id, 70);
            }
            Refresh();
        }

        private void ClearOwnedCues()
        {
            if (worldRenderer != null)
                foreach (var cue in ownedCues)
                    if (cue != null) worldRenderer.Remove(cue);
            ownedCues.Clear();
        }

        private static string Safe(string input, int max)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            input = input.Replace('\n', ' ').Replace('\r', ' ').Trim();
            return input.Length <= max ? input : input.Substring(0, max) + "...";
        }

        private static string Requirements(SparkLevelTarget target, bool detailed)
        {
            if (target == null) return "No configured objective found.";
            var sb = new StringBuilder();
            if (!target.IsConfigured()) sb.AppendLine("WARNING: Objective configuration incomplete.");
            sb.AppendLine("Type: " + target.Type);
            if (!string.IsNullOrWhiteSpace(target.Description))
                sb.AppendLine(Safe(target.Description, 230));
            if (!detailed) return sb.ToString().Trim();
            switch (target.Type)
            {
                case SparkLevelTarget.TargetType.CurrentRange:
                    sb.AppendLine("Min current: " + (target.MinimumCurrent * 1000f).ToString("F3") + " mA");
                    if (target.UseMaximumCurrent)
                        sb.AppendLine("Max current: " + (target.MaximumCurrent * 1000f).ToString("F3") + " mA");
                    break;
                case SparkLevelTarget.TargetType.VoltageRange:
                    sb.AppendLine("Min voltage: " + target.MinimumVoltage.ToString("F3") + " V");
                    if (target.UseMaximumVoltage)
                        sb.AppendLine("Max voltage: " + target.MaximumVoltage.ToString("F3") + " V");
                    break;
                case SparkLevelTarget.TargetType.PowerRange:
                    sb.AppendLine("Min power: " + (target.MinimumPower * 1000f).ToString("F3") + " mW");
                    if (target.UseMaximumPower)
                        sb.AppendLine("Max power: " + (target.MaximumPower * 1000f).ToString("F3") + " mW");
                    break;
                default:
                    sb.AppendLine("Follow the official target type and its configured conditions.");
                    break;
            }
            if (target.RequireElectricalEnabled) sb.AppendLine("Electrical enabled required.");
            if (target.RequireConduction) sb.AppendLine("Conduction required.");
            if (target.Inverted) sb.AppendLine("Inverted evaluation; read the official result.");
            if (!string.IsNullOrWhiteSpace(target.TargetComponentId))
                sb.AppendLine("Bound component ID: " + Safe(target.TargetComponentId, 80));
            else sb.AppendLine("No component binding ID configured.");
            return sb.ToString().TrimEnd();
        }

        private void Refresh()
        {
            Challenge c = Current();
            if (titleText != null)
                titleText.text = c == null ? "NO ACTIVE TRAINING OBJECTIVES" : Safe(c.name, 100);
            if (tierText != null)
                tierText.text = c == null ? "AWAITING LEVEL" :
                    (c.tier == Tier.Foundation ? "01 / FOUNDATION" :
                    c.tier == Tier.Applied ? "02 / APPLIED ELECTRICS" : "03 / ADVANCED") +
                    "  |  " + (selected + 1) + "/" + challenges.Count;
            if (objectiveText != null)
                objectiveText.text = c == null
                    ? "Start a level with configured objectives to begin." :
                    Requirements(c.target, true);
            if (evidenceText != null)
            {
                bool? pass = c == null ? null : OfficialResult(c.id);
                evidenceText.text = c == null ? "Evaluator status: UNAVAILABLE" :
                    "OFFICIAL EVALUATOR: " + (pass.HasValue ? (pass.Value ? "PASS" : "PENDING") : "NOT REPORTED") +
                    "\nTraining record: " + (IsRecorded(c) ? "VERIFIED IN AN EARLIER ATTEMPT" :
                    phase == Phase.Completed ? "VERIFIED THIS ATTEMPT" : "NOT YET VERIFIED");
            }
            if (guideText != null)
            {
                if (c == null) guideText.text = "No level objective is available.";
                else if (phase == Phase.Study)
                    guideText.text = "STEP 1 · STUDY\nRead the configured requirements. " +
                        "Use FOCUS only when the target ID matches a registered teaching anchor.";
                else if (phase == Phase.Practice || phase == Phase.AwaitVerification)
                    guideText.text = "STEP 2 · PRACTICE / VERIFY\n" +
                        (hintStage <= 0 ? "Use the existing workbench, wiring, switch and placement tools. " +
                            "Only the evaluator can confirm objective completion." :
                         hintStage == 1 ? "HINT 1: Review this target's goal and bound component (if any)." :
                         hintStage == 2 ? "HINT 2:\n" + Requirements(c.target, true) :
                         "HINT 3: Compare the exact objective-bound component with the official result. " +
                         "A nearby resistor or glowing LED may not be the evaluated target.");
                else if (phase == Phase.Completed)
                    guideText.text = "COMPLETE · OFFICIALLY VERIFIED\nPractice achievement recorded. " +
                        "Use NEXT for another challenge. Game progression was not modified.";
                else guideText.text = "CHOOSE TRAINING\nBegin a guided practice challenge. " +
                    "Progression tiers use your real level target types, not invented circuit states.";
            }
            if (progressText != null || progressFill != null)
            {
                int completed = 0;
                foreach (Challenge challenge in challenges) if (IsRecorded(challenge)) completed++;
                if (progressText != null)
                    progressText.text = "TRAINING RECORD  " + completed + "/" + challenges.Count +
                        "   |   LEVEL " + (levelManager != null && levelManager.ActiveLevel != null
                        ? Safe(levelManager.ActiveLevel.DisplayName, 36) : "NONE");
                if (progressFill != null)
                    progressFill.fillAmount = challenges.Count > 0 ? (float)completed / challenges.Count : 0f;
            }
            if (statusText != null)
                statusText.text = !string.IsNullOrWhiteSpace(statusMessage) ? statusMessage :
                    "Read-only challenge · Official evaluator controls completion";
            if (beginButton != null) beginButton.interactable = c != null && !HasActiveChallenge;
            if (studiedButton != null) studiedButton.interactable = phase == Phase.Study;
            if (hintButton != null) hintButton.interactable = HasActiveChallenge && phase != Phase.Completed;
            if (askButton != null) askButton.interactable = c != null && assistant != null;
            if (highlightButton != null) highlightButton.interactable = c != null && c.target != null &&
                !string.IsNullOrWhiteSpace(c.target.TargetComponentId);
            if (verifyButton != null) verifyButton.interactable = phase == Phase.Practice || phase == Phase.AwaitVerification;
            if (cancelButton != null) cancelButton.interactable = HasActiveChallenge;
            if (previousButton != null) previousButton.interactable = !HasActiveChallenge && challenges.Count > 1;
            if (nextButton != null) nextButton.interactable = challenges.Count > 1 && (!HasActiveChallenge || phase == Phase.Completed);
        }

        public void Show() { if (panel != null) panel.SetActive(true); Refresh(); }
        public void Hide() { if (panel != null) panel.SetActive(false); ClearOwnedCues(); }
        public void Toggle() { if (panel != null && panel.activeSelf) Hide(); else Show(); }
    }
}
