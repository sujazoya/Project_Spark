using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Handles level unlock/completion progression.
    ///
    /// This component does not evaluate circuits and does not solve
    /// electrical systems. It only tracks gameplay progression.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevelProgression : MonoBehaviour
    {
        [Serializable]
        private struct LevelProgressData
        {
            public bool unlocked;
            public bool completed;
            public int completionCount;
        }

        [Header("Manager")]
        [SerializeField] private LevelGamePlayManager levelManager;

        [Header("Progression")]
        [SerializeField] private bool initializeOnAwake = true;

        [SerializeField] private bool autoUnlockNextLevel = true;

        [Header("Persistence")]
        [SerializeField] private bool saveProgress = true;

        [SerializeField] private string saveKey =
            "ProjectSpark.LevelProgress";

        [Header("Debug")]
        [SerializeField] private bool debugLogging;

        [SerializeField] private LevelProgressData[] progress;

        public LevelGamePlayManager LevelManager =>
            levelManager;

        public int LevelCount =>
            progress != null ? progress.Length : 0;

        public event Action<int> LevelUnlocked;

        public event Action<int> LevelCompleted;

        public event Action ProgressReset;
        /// <summary>
/// Raised whenever level progression changes.
/// </summary>
public event Action ProgressChanged;

        private void Awake()
        {
            ResolveManager();

            if (!initializeOnAwake)
                return;

            Initialize();

            if (saveProgress)
                Load();
        }

        private void OnEnable()
        {
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

        // ============================================================
        // INITIALIZATION
        // ============================================================

        public void Initialize()
        {
            if (levelManager == null)
                return;

            int count =
                levelManager.LevelCount;

            if (count <= 0)
            {
                progress =
                    Array.Empty<LevelProgressData>();

                return;
            }

            LevelProgressData[] old =
                progress;

            progress =
                new LevelProgressData[count];

            for (int i = 0; i < count; i++)
            {
                bool preserve =
                    old != null &&
                    i < old.Length;

                if (preserve)
                {
                    progress[i] = old[i];
                    continue;
                }

                SparkLevelDefinition level =
                    levelManager.Levels[i];

                progress[i].unlocked =
                    level != null &&
                    level.UnlockedByDefault;

                progress[i].completed = false;
                progress[i].completionCount = 0;
            }

            EnsureFirstAvailableLevel();
        }

        private void EnsureFirstAvailableLevel()
        {
            if (progress == null ||
                progress.Length == 0)
            {
                return;
            }

            bool anyUnlocked = false;

            for (int i = 0;
                 i < progress.Length;
                 i++)
            {
                if (progress[i].unlocked)
                {
                    anyUnlocked = true;
                    break;
                }
            }

            if (anyUnlocked)
                return;

            progress[0].unlocked = true;
        }

        // ============================================================
        // EVENTS
        // ============================================================

        private void Subscribe()
        {
            if (levelManager == null)
                return;

            levelManager.LevelCompleted -=
                OnLevelCompleted;

            levelManager.LevelCompleted +=
                OnLevelCompleted;
        }

        private void Unsubscribe()
        {
            if (levelManager == null)
                return;

            levelManager.LevelCompleted -=
                OnLevelCompleted;
        }

        private void OnLevelCompleted(
            SparkLevelDefinition level)
        {
            if (levelManager == null ||
                level == null)
            {
                return;
            }

            int index =
                FindLevelIndex(level);

            if (index < 0)
                return;

            MarkCompleted(index);

            if (autoUnlockNextLevel &&
                level.AutoUnlockNextLevel)
            {
                UnlockLevel(index + 1);
            }
        }

        // ============================================================
        // UNLOCKING
        // ============================================================

        public bool IsUnlocked(int index)
        {
            if (!IsValidIndex(index))
                return false;

            return progress[index].unlocked;
        }

        public bool UnlockLevel(int index)
        {
            if (!IsValidIndex(index))
                return false;

            if (progress[index].unlocked)
                return false;

            progress[index].unlocked = true;

            LevelUnlocked?.Invoke(index);

            Log(
                $"Level unlocked: {index}");

            SaveIfEnabled();

            return true;
        }

        public bool LockLevel(int index)
        {
            if (!IsValidIndex(index))
                return false;

            if (index == 0)
                return false;

            progress[index].unlocked = false;

            SaveIfEnabled();

            return true;
        }

        // ============================================================
        // COMPLETION
        // ============================================================

        public bool IsCompleted(int index)
        {
            if (!IsValidIndex(index))
                return false;

            return progress[index].completed;
        }

        public bool MarkCompleted(int index)
        {
            if (!IsValidIndex(index))
                return false;

            bool firstCompletion =
                !progress[index].completed;

            progress[index].completed = true;

            progress[index].unlocked = true;

            progress[index].completionCount++;

            if (firstCompletion)
            {
                LevelCompleted?.Invoke(index);

                Log(
                    $"Level completed: {index}");
            }

            SaveIfEnabled();

            return true;
        }

        public int GetCompletionCount(int index)
        {
            if (!IsValidIndex(index))
                return 0;

            return progress[index].completionCount;
        }

        // ============================================================
        // LEVEL ACCESS
        // ============================================================

        public bool CanPlayLevel(int index)
        {
            return IsUnlocked(index);
        }

        public int GetFirstUnlockedLevel()
        {
            if (progress == null)
                return -1;

            for (int i = 0;
                 i < progress.Length;
                 i++)
            {
                if (progress[i].unlocked)
                    return i;
            }

            return -1;
        }

        public int GetFirstIncompleteLevel()
        {
            if (progress == null)
                return -1;

            for (int i = 0;
                 i < progress.Length;
                 i++)
            {
                if (progress[i].unlocked &&
                    !progress[i].completed)
                {
                    return i;
                }
            }

            return -1;
        }

        // ============================================================
        // START LEVEL THROUGH PROGRESSION
        // ============================================================

        public bool StartLevel(int index)
        {
            if (levelManager == null)
                return false;

            if (!CanPlayLevel(index))
            {
                Log(
                    $"Cannot start locked level: {index}");

                return false;
            }

            return levelManager.StartLevel(index);
        }

        // ============================================================
        // RESET PROGRESSION
        // ============================================================

        public void ResetProgress()
        {
            if (levelManager == null)
                return;

            int count =
                levelManager.LevelCount;

            progress =
                new LevelProgressData[count];

            for (int i = 0; i < count; i++)
            {
                SparkLevelDefinition level =
                    levelManager.Levels[i];

                progress[i].unlocked =
                    level != null &&
                    level.UnlockedByDefault;

                progress[i].completed = false;
                progress[i].completionCount = 0;
            }

            EnsureFirstAvailableLevel();

            SaveIfEnabled();

            ProgressReset?.Invoke();

            Log("Progression reset.");
        }

        // ============================================================
        // SAVE / LOAD
        // ============================================================

        public void Save()
        {
            if (progress == null)
                return;

            ProgressSaveData data =
                new ProgressSaveData
                {
                    version = 1,
                    levels = progress
                };

            string json =
                JsonUtility.ToJson(data);

            PlayerPrefs.SetString(
                saveKey,
                json);

            PlayerPrefs.Save();

            Log("Progress saved.");
        }

        public void Load()
        {
            if (!PlayerPrefs.HasKey(saveKey))
            {
                Log("No saved progression found.");
                return;
            }

            string json =
                PlayerPrefs.GetString(saveKey);

            if (string.IsNullOrEmpty(json))
                return;

            try
            {
                ProgressSaveData data =
                    JsonUtility.FromJson<
                        ProgressSaveData>(json);

                if (data.levels == null)
                {
                    Log(
                        "Saved progression contains no levels.");

                    return;
                }

                int currentCount =
                    levelManager != null
                        ? levelManager.LevelCount
                        : 0;

                int count =
                    Mathf.Min(
                        currentCount,
                        data.levels.Length);

                if (progress == null ||
                    progress.Length != currentCount)
                {
                    progress =
                        new LevelProgressData[
                            currentCount];
                }

                for (int i = 0; i < count; i++)
                {
                    progress[i] =
                        data.levels[i];
                }

                EnsureFirstAvailableLevel();

                Log("Progress loaded.");
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[SparkLevelProgression] " +
                    $"Failed to load progress: " +
                    $"{exception.Message}",
                    this);
            }
        }

        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(saveKey);

            Log("Progress save deleted.");
        }

        private void SaveIfEnabled()
        {
            if (saveProgress)
                Save();
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private int FindLevelIndex(
            SparkLevelDefinition level)
        {
            if (levelManager == null ||
                level == null ||
                levelManager.Levels == null)
            {
                return -1;
            }

            for (int i = 0;
                 i < levelManager.Levels.Length;
                 i++)
            {
                if (levelManager.Levels[i] == level)
                    return i;
            }

            return -1;
        }

        private bool IsValidIndex(int index)
        {
            return progress != null &&
                   index >= 0 &&
                   index < progress.Length;
        }

        private void ResolveManager()
        {
            if (levelManager != null)
                return;

            levelManager =
                GetComponent<LevelGamePlayManager>();

            if (levelManager != null)
                return;

            levelManager =
                FindFirstObjectByType<
                    LevelGamePlayManager>();
        }

        private void Log(string message)
        {
            if (!debugLogging)
                return;

            Debug.Log(
                $"[SparkLevelProgression] {message}",
                this);
        }

        // ============================================================
        // SAVE DATA
        // ============================================================

        [Serializable]
        private sealed class ProgressSaveData
        {
            public int version;

            public LevelProgressData[] levels;
        }
    }
}