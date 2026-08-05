using System;

namespace ProjectSpark.Gameplay.Levels
{
    public static class LevelEvents
    {
        public static event Action<LevelDefinition>
            Loaded;

        public static event Action<LevelDefinition>
            Completed;

        public static event Action<LevelDefinition>
            Failed;

        public static void RaiseLoaded(
            LevelDefinition level)
            => Loaded?.Invoke(level);

        public static void RaiseCompleted(
            LevelDefinition level)
            => Completed?.Invoke(level);

        public static void RaiseFailed(
            LevelDefinition level)
            => Failed?.Invoke(level);
    }
}
