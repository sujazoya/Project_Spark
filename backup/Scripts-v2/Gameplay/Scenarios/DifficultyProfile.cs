using UnityEngine;

namespace ProjectSpark.Gameplay.Scenarios
{
    [CreateAssetMenu(
        menuName="Project Spark/Difficulty Profile")]
    public class DifficultyProfile
        : ScriptableObject
    {
        public int DifficultyLevel;

        public float HintDelay;

        public float TimeMultiplier;

        public float RewardMultiplier;
    }
}
