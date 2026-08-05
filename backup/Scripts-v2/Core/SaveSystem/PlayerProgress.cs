using System.Collections.Generic;

namespace ProjectSpark.Core.SaveSystem
{
    [System.Serializable]
    public sealed class PlayerProgress
    {
        public List<int> CompletedLevels =
            new();

        public int TotalStars;

        public int Coins;

        public int XP;
    }
}
