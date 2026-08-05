using UnityEngine;

namespace ProjectSpark.Gameplay.Progression
{
    public sealed class StatisticsManager
        : MonoBehaviour
    {
        private readonly PlayerStatistics
            statistics =
                new();

        public void Add(
            StatisticType type,
            int amount)
        {
            statistics.Add(
                type,
                amount);

            StatisticsEvents.Raise(
                type,
                statistics.Get(type));
        }

        public int Get(
            StatisticType type)
        {
            return statistics.Get(type);
        }
    }
}
