using System;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectSpark.Gameplay
{
    /// <summary>Rewards confirmed first clear. Never watches transient target progress.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevelRewardBridge : MonoBehaviour
    {
        [Serializable]
        public sealed class Reward
        {
            public string levelId = "Level_06";
            [Min(0)] public int coins = 150;
            [Min(0)] public int diamonds = 3;
        }

        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkEconomyService economy;
        [SerializeField] private Reward[] rewards = { new Reward() };
        [SerializeField] private UnityEvent onFirstReward;
        private LevelGamePlayManager subscribed;

        private void Awake() { Resolve(); }
        private void OnEnable() { Resolve(); Subscribe(); }
        private void Start() { Resolve(); Subscribe(); }
        private void OnDisable() { Unsubscribe(); }
        private void OnDestroy() { Unsubscribe(); }

        private void Resolve()
        {
            if (levelManager == null) levelManager = GetComponent<LevelGamePlayManager>();
            if (levelManager == null) levelManager = FindFirstObjectByType<LevelGamePlayManager>();
            if (economy == null) economy = SparkEconomyService.Instance;
        }

        private void Subscribe()
        {
            if (subscribed == levelManager && subscribed != null) return;
            Unsubscribe();
            if (levelManager == null) return;
            subscribed = levelManager;
            subscribed.LevelCompleted += OnLevelCompleted;
        }

        private void Unsubscribe()
        {
            if (subscribed != null) subscribed.LevelCompleted -= OnLevelCompleted;
            subscribed = null;
        }

        private void OnLevelCompleted(SparkLevelDefinition completed)
        {
            if (completed == null || levelManager == null || !levelManager.IsCompleted) return;
            if (economy == null) economy = SparkEconomyService.Instance;
            if (economy == null || rewards == null) return;
            foreach (Reward reward in rewards)
            {
                if (reward == null || !string.Equals(completed.LevelId, reward.levelId, StringComparison.Ordinal)) continue;
                if (economy.TryAwardLevelCompletion(completed.LevelId, reward.coins, reward.diamonds))
                {
                    Debug.Log($"[Economy] First clear: {completed.LevelId} +{reward.coins} coins +{reward.diamonds} diamonds", this);
                    onFirstReward?.Invoke();
                }
                return;
            }
        }
    }
}
