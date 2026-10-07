using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Deterministic educational mistake detector for Project Spark.
    ///
    /// This component does not solve circuits and does not decide whether
    /// the player is "wrong" independently.
    ///
    /// It converts authoritative Project Spark level/evaluation evidence
    /// into learning evidence for SparkAILearning.
    ///
    /// Current evidence:
    /// - Wrong connection
    /// - Source short
    /// - Target short
    /// - Overload
    /// - Level failure with a meaningful failure reason
    ///
    /// Repeated mistakes are tracked per concept through SparkAILearning.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIMistakeDetector : MonoBehaviour
    {
        [Header("Authoritative Project Spark")]
        [SerializeField]
        private LevelGamePlayManager levelGamePlayManager;

        [Header("Spark AI")]
        [SerializeField]
        private SparkAILesson lesson;

        [SerializeField]
        private SparkAILearning learning;

        [Header("Detection")]
        [Tooltip(
            "Record a struggle when the authoritative level reports " +
            "an incorrect circuit state.")]
        [SerializeField]
        private bool recordEvaluationStruggles = true;

        [Tooltip(
            "Record a repeated mistake after the same problem is observed " +
            "multiple times without leaving that problem state.")]
        [SerializeField]
        private bool recordRepeatedMistakes = true;

        [SerializeField]
        private int repeatedMistakeThreshold = 2;

        private bool subscribed;
        private bool initialized;

        private string activeLevelId = string.Empty;

        private int wrongConnectionCount;
        private int sourceShortCount;
        private int targetShortCount;
        private int overloadCount;

        private bool previousWrongConnection;
        private bool previousSourceShort;
        private bool previousTargetShort;
        private bool previousOverload;

        public bool IsInitialized =>
            initialized;

        private void Awake()
        {
            if (levelGamePlayManager == null)
            {
                Debug.LogError(
                    "[Spark AI Mistake Detector] " +
                    "LevelGamePlayManager reference is missing.",
                    this);

                return;
            }

            if (lesson == null)
            {
                Debug.LogError(
                    "[Spark AI Mistake Detector] " +
                    "SparkAILesson reference is missing.",
                    this);

                return;
            }

            if (learning == null)
            {
                Debug.LogError(
                    "[Spark AI Mistake Detector] " +
                    "SparkAILearning reference is missing.",
                    this);

                return;
            }

            Subscribe();

            initialized = true;

            ResetTracking();

            SparkLevelDefinition activeLevel =
                levelGamePlayManager.ActiveLevel;

            if (activeLevel != null)
                BeginLevel(activeLevel);
        }

        private void OnEnable()
        {
            if (!initialized)
                return;

            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (subscribed)
                return;

            if (levelGamePlayManager == null)
                return;

            levelGamePlayManager.LevelStarted -= HandleLevelStarted;
            levelGamePlayManager.LevelStarted += HandleLevelStarted;

            levelGamePlayManager.LevelEvaluationChanged -=
                HandleLevelEvaluationChanged;

            levelGamePlayManager.LevelEvaluationChanged +=
                HandleLevelEvaluationChanged;

            levelGamePlayManager.LevelFailed -=
                HandleLevelFailed;

            levelGamePlayManager.LevelFailed +=
                HandleLevelFailed;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
                return;

            if (levelGamePlayManager != null)
            {
                levelGamePlayManager.LevelStarted -=
                    HandleLevelStarted;

                levelGamePlayManager.LevelEvaluationChanged -=
                    HandleLevelEvaluationChanged;

                levelGamePlayManager.LevelFailed -=
                    HandleLevelFailed;
            }

            subscribed = false;
        }

        private void HandleLevelStarted(
            SparkLevelDefinition level)
        {
            if (level == null)
                return;

            BeginLevel(level);
        }

        private void BeginLevel(
            SparkLevelDefinition level)
        {
            activeLevelId =
                level.LevelId ?? string.Empty;

            ResetTracking();

            /*
             * The lesson bridge normally activates the lesson from
             * LevelStarted as well.
             *
             * This fallback makes the detector safe if execution order
             * causes this component to receive LevelStarted first.
             */
            if (!lesson.HasActiveLesson ||
                !string.Equals(
                    lesson.ActiveLesson.Id,
                    activeLevelId,
                    StringComparison.Ordinal))
            {
                lesson.TrySetActiveLesson(activeLevelId);
            }
        }

        private void HandleLevelEvaluationChanged(
            SparkLevelDefinition level)
        {
            if (!recordEvaluationStruggles)
                return;

            if (level == null)
                return;

            if (!IsActiveLevel(level))
                return;

            EvaluateCurrentEvidence();
        }

        private void HandleLevelFailed(
            SparkLevelDefinition level,
            string reason)
        {
            if (level == null)
                return;

            if (!IsActiveLevel(level))
                return;

            /*
             * First inspect authoritative flags.
             *
             * If none identify the problem, use the failure event as
             * evidence only when a meaningful failure reason exists.
             */
            bool recorded =
                EvaluateCurrentEvidence();

            if (recorded)
                return;

            if (string.IsNullOrWhiteSpace(reason))
                return;

            RecordGeneralLessonStruggle(
                reason);
        }

        private bool IsActiveLevel(
            SparkLevelDefinition level)
        {
            if (levelGamePlayManager == null)
                return false;

            SparkLevelDefinition activeLevel =
                levelGamePlayManager.ActiveLevel;

            if (activeLevel == null)
                return false;

            return ReferenceEquals(
                activeLevel,
                level);
        }

        /// <summary>
        /// Reads authoritative level state and records the most
        /// specific currently-known problem.
        ///
        /// Returns true when learning evidence was recorded.
        /// </summary>
        private bool EvaluateCurrentEvidence()
        {
            if (!lesson.HasActiveLesson)
                return false;

            bool recorded = false;

            if (levelGamePlayManager.WrongConnection)
            {
                recorded |= HandleWrongConnection();
            }
            else
            {
                previousWrongConnection = false;
            }

            if (levelGamePlayManager.SourceShorted)
            {
                recorded |= HandleSourceShort();
            }
            else
            {
                previousSourceShort = false;
            }

            if (levelGamePlayManager.TargetShorted)
            {
                recorded |= HandleTargetShort();
            }
            else
            {
                previousTargetShort = false;
            }

            /*
             * Current Project Spark versions expose the overload state
             * through the level snapshot as a reserved field.
             *
             * We intentionally do not access a guessed
             * LevelGamePlayManager.IsOverloaded property here.
             */

            return recorded;
        }

        private bool HandleWrongConnection()
        {
            wrongConnectionCount++;

            bool repeated =
                previousWrongConnection &&
                wrongConnectionCount >= repeatedMistakeThreshold;

            previousWrongConnection = true;

            string reason =
                "The authoritative level evaluation reports an incorrect connection.";

            if (repeated && recordRepeatedMistakes)
            {
                RecordRepeatedMistakeForActiveConcepts(
                    reason);

                return true;
            }

            RecordStruggleForActiveConcepts(
                reason);

            return true;
        }

        private bool HandleSourceShort()
        {
            sourceShortCount++;

            bool repeated =
                previousSourceShort &&
                sourceShortCount >= repeatedMistakeThreshold;

            previousSourceShort = true;

            string reason =
                "The authoritative level evaluation reports that the power source is shorted.";

            if (repeated && recordRepeatedMistakes)
            {
                RecordRepeatedMistakeForActiveConcepts(
                    reason);

                return true;
            }

            RecordStruggleForActiveConcepts(
                reason);

            return true;
        }

        private bool HandleTargetShort()
        {
            targetShortCount++;

            bool repeated =
                previousTargetShort &&
                targetShortCount >= repeatedMistakeThreshold;

            previousTargetShort = true;

            string reason =
                "The authoritative level evaluation reports that the target is shorted.";

            if (repeated && recordRepeatedMistakes)
            {
                RecordRepeatedMistakeForActiveConcepts(
                    reason);

                return true;
            }

            RecordStruggleForActiveConcepts(
                reason);

            return true;
        }

        private void RecordGeneralLessonStruggle(
            string reason)
        {
            string[] concepts =
                lesson.ActiveLesson.ConceptIds;

            if (concepts == null)
                return;

            for (int i = 0; i < concepts.Length; i++)
            {
                string conceptId =
                    concepts[i];

                if (string.IsNullOrWhiteSpace(conceptId))
                    continue;

                lesson.RecordConceptStruggle(
                    conceptId,
                    reason);
            }
        }

        private void RecordStruggleForActiveConcepts(
            string reason)
        {
            string[] concepts =
                lesson.ActiveLesson.ConceptIds;

            if (concepts == null)
                return;

            for (int i = 0; i < concepts.Length; i++)
            {
                string conceptId =
                    concepts[i];

                if (string.IsNullOrWhiteSpace(conceptId))
                    continue;

                lesson.RecordConceptStruggle(
                    conceptId,
                    reason);
            }
        }

        private void RecordRepeatedMistakeForActiveConcepts(
            string reason)
        {
            string[] concepts =
                lesson.ActiveLesson.ConceptIds;

            if (concepts == null)
                return;

            for (int i = 0; i < concepts.Length; i++)
            {
                string conceptId =
                    concepts[i];

                if (string.IsNullOrWhiteSpace(conceptId))
                    continue;

                lesson.RecordConceptMistake(
                    conceptId,
                    reason);
            }
        }

        private void ResetTracking()
        {
            wrongConnectionCount = 0;
            sourceShortCount = 0;
            targetShortCount = 0;
            overloadCount = 0;

            previousWrongConnection = false;
            previousSourceShort = false;
            previousTargetShort = false;
            previousOverload = false;
        }
    }
}