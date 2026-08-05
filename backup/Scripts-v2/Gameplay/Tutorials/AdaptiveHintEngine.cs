namespace ProjectSpark.Gameplay.Tutorials
{
    public sealed class AdaptiveHintEngine
    {
        public int CalculateHintLevel(
            int mistakes,
            float timeSpent)
        {
            if (mistakes >= 5)
                return 3;

            if (mistakes >= 3)
                return 2;

            if (timeSpent > 60)
                return 1;

            return 0;
        }
    }
}
