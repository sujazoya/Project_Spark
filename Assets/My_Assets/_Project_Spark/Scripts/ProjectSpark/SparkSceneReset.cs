
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Provides scene reset functionality for Project Spark.
    ///
    /// Reloading the active scene restores all scene objects
    /// to their serialized scene state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSceneReset : MonoBehaviour
    {
        /// <summary>
        /// Resets the currently active scene.
        /// </summary>
        public void ResetScene()
        {
            Scene activeScene =
                SceneManager.GetActiveScene();

            SceneManager.LoadScene(
                activeScene.buildIndex);
        }

        /// <summary>
        /// Resets the currently active scene by name.
        /// </summary>
        public void ResetSceneByName()
        {
            Scene activeScene =
                SceneManager.GetActiveScene();

            SceneManager.LoadScene(
                activeScene.name);
        }

        /// <summary>
        /// Can be connected directly to a Unity UI Button.
        /// </summary>
        public void ResetFromButton()
        {
            ResetScene();
        }
    }
}
