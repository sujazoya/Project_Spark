using UnityEngine;

namespace ProjectSpark.Gameplay.Levels
{
    public sealed class LevelLoader
    {
        public void Load(LevelRuntime runtime)
        {
            Object.Instantiate(
                runtime.Definition.EnvironmentPrefab);

            Object.Instantiate(
                runtime.Definition.BoardPrefab);

            runtime.Loaded = true;

            LevelEvents.RaiseLoaded(
                runtime.Definition);
        }
    }
}
