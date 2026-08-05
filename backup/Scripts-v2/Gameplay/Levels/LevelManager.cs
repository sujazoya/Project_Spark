using UnityEngine;

namespace ProjectSpark.Gameplay.Levels
{
    public sealed class LevelManager
        : MonoBehaviour
    {
        [SerializeField]
        private LevelDefinition currentLevel;

        private readonly LevelLoader
            loader =
                new();

        private LevelRuntime runtime;

        private void Start()
        {
            runtime =
                new LevelRuntime
                {
                    Definition = currentLevel
                };

            loader.Load(runtime);
        }

        public void Complete()
        {
            runtime.Statistics.Completed = true;

            LevelEvents.RaiseCompleted(
                runtime.Definition);
        }
    }
}
