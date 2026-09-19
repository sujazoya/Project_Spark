using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// UI/gameplay-facing level selection controller.
    ///
    /// Responsibilities:
    /// - Select a level
    /// - Navigate previous/next levels
    /// - Respect progression locks
    /// - Start selected level
    /// - Continue from progression
    /// - Retry current level
    ///
    /// Does NOT own level gameplay or progression data.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevelSelector : MonoBehaviour
    {
        // ============================================================
        // EVENTS
        // ============================================================

        public event Action<int> SelectionChanged;
        public event Action<int> LevelSelectionRejected;
        public event Action<int> LevelStarted;

        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("References")]
        [SerializeField]
        private LevelGamePlayManager levelManager;

        [SerializeField]
        private SparkLevelProgression progression;

        [Header("Selection")]
        [SerializeField, Min(0)]
        private int selectedLevelIndex;

        [SerializeField]
        private bool autoSelectCurrentLevel = true;

        [SerializeField]
        private bool autoSelectFirstAvailable = true;

        // ============================================================
        // RUNTIME
        // ============================================================

        private bool initialized;

        // ============================================================
        // PUBLIC PROPERTIES
        // ============================================================

        public LevelGamePlayManager LevelManager => levelManager;

        public SparkLevelProgression Progression => progression;

        public int SelectedLevelIndex => selectedLevelIndex;

        public int LevelCount =>
            levelManager != null
                ? levelManager.LevelCount
                : 0;

        public bool HasSelection =>
            selectedLevelIndex >= 0 &&
            selectedLevelIndex < LevelCount;

        public SparkLevelDefinition SelectedLevel =>
            HasSelection
                ? levelManager.GetLevel(selectedLevelIndex)
                : null;

        public bool IsSelectedLevelUnlocked =>
            HasSelection &&
            IsLevelUnlocked(selectedLevelIndex);

        public bool IsSelectedLevelCompleted =>
            HasSelection &&
            IsLevelCompleted(selectedLevelIndex);

        public bool CanStartSelectedLevel =>
            HasSelection &&
            IsSelectedLevelUnlocked;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            ResolveReferences();
            InitializeSelection();
        }

        private void OnEnable()
        {
            if (levelManager != null)
                levelManager.LevelStarted += OnLevelStarted;

            if (progression != null)
                progression.ProgressChanged += OnProgressChanged;
        }

        private void OnDisable()
        {
            if (levelManager != null)
                levelManager.LevelStarted -= OnLevelStarted;

            if (progression != null)
                progression.ProgressChanged -= OnProgressChanged;
        }

        private void OnValidate()
        {
            if (selectedLevelIndex < 0)
                selectedLevelIndex = 0;
        }

        // ============================================================
        // INITIALIZATION
        // ============================================================

        private void ResolveReferences()
        {
            if (levelManager == null)
                levelManager = GetComponent<LevelGamePlayManager>();

            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>();

            if (progression == null)
                progression = GetComponent<SparkLevelProgression>();

            if (progression == null)
                progression = FindFirstObjectByType<SparkLevelProgression>();
        }

        private void InitializeSelection()
        {
            if (levelManager == null)
                return;

            if (LevelCount <= 0)
            {
                selectedLevelIndex = 0;
                initialized = true;
                return;
            }

            if (autoSelectCurrentLevel)
            {
                int current = levelManager.ActiveLevelIndex;

                if (IsValidIndex(current))
                {
                    selectedLevelIndex = current;
                    initialized = true;
                    return;
                }
            }

            if (autoSelectFirstAvailable)
            {
                int available = GetFirstAvailableLevel();

                if (available >= 0)
                {
                    selectedLevelIndex = available;
                    initialized = true;
                    return;
                }
            }

            selectedLevelIndex = Mathf.Clamp(
                selectedLevelIndex,
                0,
                LevelCount - 1);

            initialized = true;
        }

        // ============================================================
        // SELECTION
        // ============================================================

        public bool SelectLevel(int index)
        {
            if (!IsValidIndex(index))
                return false;

            if (selectedLevelIndex == index)
                return true;

            selectedLevelIndex = index;

            SelectionChanged?.Invoke(selectedLevelIndex);

            return true;
        }

        public bool SelectLevel(string levelId)
        {
            if (string.IsNullOrWhiteSpace(levelId))
                return false;

            if (levelManager == null)
                return false;

            for (int i = 0; i < LevelCount; i++)
            {
                SparkLevelDefinition level =
                    levelManager.GetLevel(i);

                if (level == null)
                    continue;

                if (!string.Equals(
                        level.LevelId,
                        levelId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return SelectLevel(i);
            }

            return false;
        }

        public bool SelectNext()
        {
            if (LevelCount <= 0)
                return false;

            int next = selectedLevelIndex + 1;

            if (next >= LevelCount)
                return false;

            return SelectLevel(next);
        }

        public bool SelectPrevious()
        {
            if (LevelCount <= 0)
                return false;

            int previous = selectedLevelIndex - 1;

            if (previous < 0)
                return false;

            return SelectLevel(previous);
        }

        public bool SelectFirst()
        {
            if (LevelCount <= 0)
                return false;

            return SelectLevel(0);
        }

        public bool SelectLast()
        {
            if (LevelCount <= 0)
                return false;

            return SelectLevel(LevelCount - 1);
        }

        // ============================================================
        // AVAILABLE LEVEL SELECTION
        // ============================================================

        public bool SelectFirstAvailable()
        {
            int index = GetFirstAvailableLevel();

            if (index < 0)
                return false;

            return SelectLevel(index);
        }

        public bool SelectFirstIncomplete()
        {
            int index = GetFirstIncompleteLevel();

            if (index < 0)
                return false;

            return SelectLevel(index);
        }

        public bool SelectNextUnlocked()
        {
            if (LevelCount <= 0)
                return false;

            for (int i = selectedLevelIndex + 1; i < LevelCount; i++)
            {
                if (!IsLevelUnlocked(i))
                    continue;

                return SelectLevel(i);
            }

            return false;
        }

        // ============================================================
        // START / CONTINUE
        // ============================================================

        public bool StartSelectedLevel()
        {
            if (!HasSelection)
                return false;

            if (!IsLevelUnlocked(selectedLevelIndex))
            {
                LevelSelectionRejected?.Invoke(selectedLevelIndex);

                return false;
            }

            if (levelManager == null)
                return false;

            bool started =
                levelManager.StartLevel(selectedLevelIndex);

            if (started)
                LevelStarted?.Invoke(selectedLevelIndex);

            return started;
        }

        /// <summary>
        /// Starts the currently selected level if possible.
        /// Otherwise selects the first available level.
        /// </summary>
        public bool Continue()
        {
            if (HasSelection &&
                IsLevelUnlocked(selectedLevelIndex))
            {
                return StartSelectedLevel();
            }

            int index = GetContinueLevel();

            if (index < 0)
                return false;

            if (!SelectLevel(index))
                return false;

            return StartSelectedLevel();
        }

        /// <summary>
        /// Continue from the first unlocked incomplete level.
        /// Falls back to the first unlocked level.
        /// </summary>
        public bool ContinueFromProgression()
        {
            int index = GetFirstIncompleteLevel();

            if (index < 0)
                index = GetFirstAvailableLevel();

            if (index < 0)
                return false;

            if (!SelectLevel(index))
                return false;

            return StartSelectedLevel();
        }

        // ============================================================
        // RETRY
        // ============================================================

        public bool Retry()
        {
            if (levelManager == null)
                return false;

            int current = levelManager.ActiveLevelIndex;

            if (!IsValidIndex(current))
                return false;

            if (!IsLevelUnlocked(current))
                return false;

            selectedLevelIndex = current;

            return levelManager.RestartLevel();
        }

        public bool RetrySelectedLevel()
        {
            if (!HasSelection)
                return false;

            if (!IsLevelUnlocked(selectedLevelIndex))
                return false;

            if (levelManager == null)
                return false;

            if (levelManager.ActiveLevelIndex != selectedLevelIndex)
            {
                return StartSelectedLevel();
            }

            return levelManager.RestartLevel();
        }

        // ============================================================
        // PROGRESSION QUERIES
        // ============================================================

        public bool IsLevelUnlocked(int index)
        {
            if (!IsValidIndex(index))
                return false;

            if (progression == null)
            {
                SparkLevelDefinition level =
                    levelManager.GetLevel(index);

                return level != null &&
                       level.UnlockedByDefault;
            }

            return progression.IsUnlocked(index);
        }

        public bool IsLevelCompleted(int index)
        {
            if (!IsValidIndex(index))
                return false;

            if (progression == null)
                return false;

            return progression.IsCompleted(index);
        }

        public int GetCompletionCount(int index)
        {
            if (!IsValidIndex(index))
                return 0;

            if (progression == null)
                return 0;

            return progression.GetCompletionCount(index);
        }

        // ============================================================
        // FIND LEVELS
        // ============================================================

        public int GetFirstAvailableLevel()
        {
            if (LevelCount <= 0)
                return -1;

            for (int i = 0; i < LevelCount; i++)
            {
                if (IsLevelUnlocked(i))
                    return i;
            }

            return -1;
        }

        public int GetFirstIncompleteLevel()
        {
            if (LevelCount <= 0)
                return -1;

            for (int i = 0; i < LevelCount; i++)
            {
                if (!IsLevelUnlocked(i))
                    continue;

                if (!IsLevelCompleted(i))
                    return i;
            }

            return -1;
        }

        public int GetContinueLevel()
        {
            if (LevelCount <= 0)
                return -1;

            // Prefer the current selection.
            if (HasSelection &&
                IsLevelUnlocked(selectedLevelIndex))
            {
                return selectedLevelIndex;
            }

            // Then first incomplete level.
            int incomplete = GetFirstIncompleteLevel();

            if (incomplete >= 0)
                return incomplete;

            // Finally first unlocked level.
            return GetFirstAvailableLevel();
        }

        // ============================================================
        // CURRENT LEVEL INFORMATION
        // ============================================================

        public string GetSelectedLevelId()
        {
            SparkLevelDefinition level = SelectedLevel;

            return level != null
                ? level.LevelId
                : string.Empty;
        }

        public string GetSelectedLevelName()
        {
            SparkLevelDefinition level = SelectedLevel;

            return level != null
                ? level.DisplayName
                : string.Empty;
        }

        public string GetSelectedLevelDescription()
        {
            SparkLevelDefinition level = SelectedLevel;

            return level != null
                ? level.Description
                : string.Empty;
        }

        public string GetSelectionStatus()
        {
            if (!HasSelection)
                return "NO LEVEL";

            if (!IsLevelUnlocked(selectedLevelIndex))
                return "LOCKED";

            if (IsLevelCompleted(selectedLevelIndex))
                return "COMPLETED";

            return "AVAILABLE";
        }

        // ============================================================
        // PROGRESSION CALLBACK
        // ============================================================

        private void OnProgressChanged()
        {
            if (!initialized)
                return;

            // Selection itself does not change automatically.
            // UI can refresh by reading the public properties.
            SelectionChanged?.Invoke(selectedLevelIndex);
        }

      private void OnLevelStarted(
    SparkLevelDefinition level)
    {
        if (level == null)
            return;

        int index = levelManager != null
            ? levelManager.ActiveLevelIndex
            : -1;

        if (!IsValidIndex(index))
            return;

        selectedLevelIndex = index;

        SelectionChanged?.Invoke(
            selectedLevelIndex);
    }

        // ============================================================
        // VALIDATION
        // ============================================================

        private bool IsValidIndex(int index)
        {
            return levelManager != null &&
                   index >= 0 &&
                   index < levelManager.LevelCount;
        }
    }
}