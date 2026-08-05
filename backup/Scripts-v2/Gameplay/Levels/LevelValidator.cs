namespace ProjectSpark.Gameplay.Levels
{
    public sealed class LevelValidator
    {
        public bool Validate(LevelRuntime runtime)
        {
            // Future:
            // Check objectives
            // Check board state
            // Check simulation

            return runtime.Completed;
        }
    }
}
