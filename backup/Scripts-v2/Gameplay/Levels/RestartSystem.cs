using UnityEngine.SceneManagement;

namespace ProjectSpark.Gameplay.Levels
{
    public sealed class RestartSystem
    {
        public void Restart()
        {
            SceneManager.LoadScene(
                SceneManager
                    .GetActiveScene()
                    .buildIndex);
        }
    }
}
