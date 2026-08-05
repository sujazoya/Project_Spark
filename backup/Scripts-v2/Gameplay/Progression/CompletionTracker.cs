namespace ProjectSpark.Gameplay.Progression
{
    public sealed class CompletionTracker
    {
        public int LevelsCompleted
        {
            get;
            private set;
        }

        public void CompleteLevel()
        {
            LevelsCompleted++;
        }
    }
}
