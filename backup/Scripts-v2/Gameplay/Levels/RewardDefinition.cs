using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.Levels
{
    [Serializable]
    public class RewardDefinition
    {
        [Min(0)]
        public int Coins;

        [Min(0)]
        public int Experience;

        [Min(0)]
        public int KnowledgePoints;
    }
}
