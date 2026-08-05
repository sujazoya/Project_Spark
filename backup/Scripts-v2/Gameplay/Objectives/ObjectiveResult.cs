namespace ProjectSpark.Gameplay.Objectives
{
    public readonly struct ObjectiveResult
    {
        public bool Completed { get; }

        public float Progress { get; }

        public ObjectiveResult(bool completed, float progress)
        {
            Completed = completed;
            Progress = progress;
        }
    }
}