using System;

namespace ProjectSpark.Gameplay.Levels
{
    [Serializable]
    public sealed class LevelReward
    {
        public int Coins;

        public int Experience;

        public int Stars;

        public bool UnlockNextLevel;
    }
}
