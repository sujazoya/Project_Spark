using UnityEngine.SceneManagement;

namespace ProjectSpark.Core.Bootstrap
{
    public static class SceneLoader
    {
        public static void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public static void ReloadCurrent()
        {
            var scene = SceneManager.GetActiveScene();

            SceneManager.LoadScene(scene.name);
        }
    }
}
