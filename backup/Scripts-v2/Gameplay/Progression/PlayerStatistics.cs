using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Progression
{
    public sealed class PlayerStatistics
    {
        private readonly Dictionary<StatisticType,int>
            values = new();

        public int Get(
            StatisticType type)
        {
            values.TryGetValue(type,
                out int value);

            return value;
        }

        public void Add(
            StatisticType type,
            int amount)
        {
            values[type] =
                Get(type) + amount;
        }
    }
}
