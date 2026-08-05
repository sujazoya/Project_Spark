using System;

namespace ProjectSpark.Gameplay.Levels
{
    [Serializable]
    public sealed class LevelStatistics
    {
        public float CompletionTime;

        public int Mistakes;

        public int HintsUsed;

        public int ComponentsPlaced;

        public bool Completed;
    }
}
