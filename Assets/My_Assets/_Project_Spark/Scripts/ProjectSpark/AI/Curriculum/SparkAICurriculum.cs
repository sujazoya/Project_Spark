using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Runtime access to the Project Spark learning curriculum.
    ///
    /// Responsibilities:
    /// - Provides access to curriculum levels.
    /// - Finds curriculum levels by ID.
    /// - Finds lessons inside curriculum levels.
    /// - Tracks the currently selected curriculum level.
    ///
    /// This class does NOT:
    /// - Evaluate electrical circuits.
    /// - Complete Project Spark levels.
    /// - Modify the circuit.
    /// - Decide whether the player has passed a level.
    /// - Replace LevelGamePlayManager.
    ///
    /// Authoritative gameplay progression remains in the normal
    /// Project Spark level system.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAICurriculum : MonoBehaviour
    {
        [Header("Curriculum")]
        [SerializeField] private SparkAICurriculumDefinition curriculum;

        [Header("Runtime")]
        [SerializeField] private bool autoSelectFirstLevel = true;

        private readonly Dictionary<string, SparkAICurriculumLevel> levelsById =
            new Dictionary<string, SparkAICurriculumLevel>(
                StringComparer.OrdinalIgnoreCase);

        private string activeLevelId = string.Empty;
        private bool initialized;

        /// <summary>
        /// Assigned curriculum asset.
        /// </summary>
        public SparkAICurriculumDefinition Curriculum => curriculum;

        /// <summary>
        /// Whether the curriculum was successfully initialized.
        /// </summary>
        public bool IsInitialized => initialized;

        /// <summary>
        /// Currently selected curriculum level.
        /// </summary>
        public SparkAICurriculumLevel ActiveLevel
        {
            get
            {
                if (string.IsNullOrEmpty(activeLevelId))
                    return default;

                if (levelsById.TryGetValue(activeLevelId, out SparkAICurriculumLevel level))
                    return level;

                return default;
            }
        }

        /// <summary>
        /// ID of the currently selected curriculum level.
        /// </summary>
        public string ActiveLevelId => activeLevelId;

        /// <summary>
        /// Number of valid curriculum levels.
        /// </summary>
        public int LevelCount => levelsById.Count;

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// Builds the runtime lookup table.
        /// </summary>
        public bool Initialize()
        {
            levelsById.Clear();
            activeLevelId = string.Empty;
            initialized = false;

            if (curriculum == null)
            {
                Debug.LogWarning(
                    "[AI CURRICULUM] No curriculum definition assigned.",
                    this);

                return false;
            }

            IReadOnlyList<SparkAICurriculumLevel> levels =
                curriculum.Levels;

            if (levels == null || levels.Count == 0)
            {
                Debug.LogWarning(
                    "[AI CURRICULUM] Curriculum contains no levels.",
                    this);

                return false;
            }

            for (int i = 0; i < levels.Count; i++)
            {
                SparkAICurriculumLevel level = levels[i];

                if (!level.IsValid)
                    continue;

                if (string.IsNullOrWhiteSpace(level.LevelId))
                {
                    Debug.LogWarning(
                        $"[AI CURRICULUM] Level at index {i} has no LevelId.",
                        this);

                    continue;
                }

                if (levelsById.ContainsKey(level.LevelId))
                {
                    Debug.LogWarning(
                        $"[AI CURRICULUM] Duplicate LevelId '{level.LevelId}'.",
                        this);

                    continue;
                }

                levelsById.Add(level.LevelId, level);
            }

            initialized = levelsById.Count > 0;

            if (!initialized)
            {
                Debug.LogWarning(
                    "[AI CURRICULUM] No valid curriculum levels were found.",
                    this);

                return false;
            }

            if (autoSelectFirstLevel)
                SelectFirstLevel();

            return true;
        }

        /// <summary>
        /// Selects the first valid curriculum level.
        /// </summary>
        public bool SelectFirstLevel()
        {
            if (!initialized)
                return false;

            foreach (KeyValuePair<string, SparkAICurriculumLevel> pair in levelsById)
            {
                activeLevelId = pair.Key;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Selects a curriculum level by its LevelId.
        /// </summary>
        public bool SelectLevel(string levelId)
        {
            if (!initialized)
                return false;

            if (string.IsNullOrWhiteSpace(levelId))
                return false;

            if (!levelsById.ContainsKey(levelId))
                return false;

            activeLevelId = levelId;
            return true;
        }

        /// <summary>
        /// Finds a curriculum level without changing the active level.
        /// </summary>
        public bool TryGetLevel(
            string levelId,
            out SparkAICurriculumLevel level)
        {
            level = default;

            if (!initialized)
                return false;

            if (string.IsNullOrWhiteSpace(levelId))
                return false;

            return levelsById.TryGetValue(levelId, out level);
        }

        /// <summary>
        /// Finds a lesson inside a curriculum level.
        /// </summary>
        public bool TryGetLesson(
            string levelId,
            string lessonId,
            out SparkAILessonDefinition lesson)
        {
            lesson = default;

            if (!TryGetLevel(levelId, out SparkAICurriculumLevel level))
                return false;

            return level.TryGetLesson(lessonId, out lesson);
        }

        /// <summary>
        /// Finds a lesson inside the currently active curriculum level.
        /// </summary>
        public bool TryGetActiveLesson(
            string lessonId,
            out SparkAILessonDefinition lesson)
        {
            lesson = default;

            if (!HasActiveLevel)
                return false;

            return ActiveLevel.TryGetLesson(lessonId, out lesson);
        }

        /// <summary>
        /// Whether a valid active curriculum level exists.
        /// </summary>
        public bool HasActiveLevel =>
            initialized &&
            !string.IsNullOrEmpty(activeLevelId) &&
            levelsById.ContainsKey(activeLevelId);

        /// <summary>
        /// Returns all curriculum levels.
        /// </summary>
        public IReadOnlyCollection<SparkAICurriculumLevel> GetAllLevels()
        {
            return levelsById.Values;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (curriculum == null)
                return;

            ValidateCurriculum();
        }

        private void ValidateCurriculum()
        {
            IReadOnlyList<SparkAICurriculumLevel> levels =
                curriculum.Levels;

            if (levels == null)
                return;

            HashSet<string> ids =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < levels.Count; i++)
            {
                SparkAICurriculumLevel level = levels[i];

                if (!level.IsValid)
                    continue;

                if (string.IsNullOrWhiteSpace(level.LevelId))
                    continue;

                if (!ids.Add(level.LevelId))
                {
                    Debug.LogWarning(
                        $"[AI CURRICULUM] Duplicate LevelId '{level.LevelId}'.",
                        this);
                }
            }
        }
#endif
    }
}