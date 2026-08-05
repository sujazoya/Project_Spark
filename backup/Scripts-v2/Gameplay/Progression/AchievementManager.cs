using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Progression
{
    public sealed class AchievementManager
        : MonoBehaviour
    {
        [SerializeField]
        private List<AchievementDefinition>
            achievements = new();

        private readonly HashSet<string>
            unlocked = new();

        public bool IsUnlocked(
            string id)
        {
            return unlocked.Contains(id);
        }

        public void Unlock(
            string id)
        {
            unlocked.Add(id);
        }
    }
}
