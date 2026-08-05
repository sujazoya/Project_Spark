using System;

namespace ProjectSpark.Gameplay.Progression
{
    public static class StatisticsEvents
    {
        public static event Action<
            StatisticType,int>
            StatisticChanged;

        public static void Raise(
            StatisticType type,
            int value)
        {
            StatisticChanged?.Invoke(
                type,
                value);
        }
    }
}
