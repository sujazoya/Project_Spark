using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay.Objectives;

namespace ProjectSpark.Gameplay.Levels
{
    [CreateAssetMenu(
        menuName="Project Spark/Levels/Level")]
    public sealed class LevelDefinition
        : ScriptableObject
    {
        public string LevelId;

        public string LevelName;

        public string Description;

        public LevelDifficulty Difficulty;

        public GameObject EnvironmentPrefab;

        public GameObject BoardPrefab;

        public List<Objective> Objectives;

        public LevelReward Reward;
    }
}
