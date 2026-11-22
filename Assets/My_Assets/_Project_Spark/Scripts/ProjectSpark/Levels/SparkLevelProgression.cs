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
                    !levelManager.Campaign && level && level.UnlockedByDefault;

                progress[i].completed = false;
                progress[i].completionCount = 0;
            }

            EnsureFirstAvailableLevel();
        }

        private void EnsureFirstAvailableLevel()
        {
            if (levelManager == null || progress == null)
                return;

            int first = levelManager.GetFirstCampaignIndex();
            if (!IsValidIndex(first))
                return;

            if (!levelManager.Campaign)
            {
                foreach (int index in levelManager.GetCampaignLevelIndices())
                    if (IsUnlocked(index))
                        return;
            }

            progress[first].unlocked = true;
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

            MarkCompleted(levelManager.GetLevelIndex(level));
        }

        private int GetUnlockableSuccessor(int index)
        {
            SparkLevelDefinition level = levelManager.GetLevel(index);
            if (!autoUnlockNextLevel || !level || !level.AutoUnlockNextLevel)
                return -1;

            int next = levelManager.GetAdjacentLevelIndex(index, 1);
            if (!levelManager.Campaign || next < 0)
                return next;

            SparkChapterDefinition current = levelManager.Campaign.FindChapter(level);
            SparkLevelDefinition successor = levelManager.GetLevel(next);
            SparkChapterDefinition following = levelManager.Campaign.FindChapter(successor);
            return following == current || levelManager.ChapterAccess.CanEnterLevel(successor) ? next : -1;
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

            if (levelManager && index == levelManager.GetFirstCampaignIndex())
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
            if (!IsValidIndex(index) || !levelManager || !levelManager.IsCampaignLevel(index))
                return false;

            bool firstCompletion = !progress[index].completed;
            progress[index].completed = true;
            progress[index].unlocked = true;
            progress[index].completionCount++;

            int next = GetUnlockableSuccessor(index);
            bool unlockNext = IsValidIndex(next) && !progress[next].unlocked;
            if (unlockNext)
                progress[next].unlocked = true;

            if (firstCompletion)
                LevelCompleted?.Invoke(index);
            if (unlockNext)
                LevelUnlocked?.Invoke(next);

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
            if (!IsValidIndex(index) || !levelManager || !levelManager.IsCampaignLevel(index))
                return false;
            if (IsUnlocked(index))
                return true;

            SparkChapterAccessController chapterAccess = levelManager.ChapterAccess;
            if (!chapterAccess)
                chapterAccess = GetComponent<SparkChapterAccessController>();
            return chapterAccess && chapterAccess.CanEnterLevel(levelManager.GetLevel(index));
        }

        public int GetFirstUnlockedLevel()
        {
            if (levelManager)
                foreach (int index in levelManager.GetCampaignLevelIndices())
                    if (CanPlayLevel(index))
                        return index;
            return -1;
        }

        public int GetFirstIncompleteLevel()
        {
            if (levelManager)
                foreach (int index in levelManager.GetCampaignLevelIndices())
                    if (CanPlayLevel(index) && !IsCompleted(index))
                        return index;
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
                    !levelManager.Campaign && level && level.UnlockedByDefault;

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
                Initialize();
            }


                for (int i = 0; i < count; i++)
                {
                    progress[i] =
                        data.levels[i];
                }

                RestoreCampaignAccess();
                EnsureFirstAvailableLevel();
                ProgressChanged?.Invoke();
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

            ProgressChanged?.Invoke();
        }


        // ============================================================
        // HELPERS
        // ============================================================

        private void RestoreCampaignAccess()
        {
            if (!levelManager || !levelManager.Campaign)
                return;

            // Old campaigns occasionally registered a definition more than once.
            for (int index = 0; index < progress.Length; index++)
            {
                int canonical = levelManager.GetLevelIndex(levelManager.GetLevel(index));
                if (canonical < 0 || canonical == index)
                    continue;

                progress[canonical].unlocked |= progress[index].unlocked || progress[index].completed;
                progress[canonical].completed |= progress[index].completed;
                progress[canonical].completionCount = Mathf.Max(
                    progress[canonical].completionCount, progress[index].completionCount);
            }

            if (!autoUnlockNextLevel)
                return;

            foreach (int index in levelManager.GetCampaignLevelIndices())
            {
                if (!IsCompleted(index))
                    continue;

                int successor = GetUnlockableSuccessor(index);
                if (IsValidIndex(successor))
                    progress[successor].unlocked = true;
            }
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