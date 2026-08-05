using UnityEngine;

namespace ProjectSpark.Gameplay.Progression
{
    [CreateAssetMenu(
        menuName="Project Spark/Achievement")]
    public sealed class AchievementDefinition
        : ScriptableObject
    {
        public string Id;

        public string Title;

        public string Description;

        public StatisticType TargetStatistic;

        public int TargetValue;
    }
}
