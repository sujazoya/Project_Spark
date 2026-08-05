namespace ProjectSpark.Gameplay.Levels
{
    public sealed class LevelRuntime
    {
        public LevelDefinition Definition;

        public LevelStatistics Statistics = new();

        public bool Loaded;

        public bool Completed;
    }
}