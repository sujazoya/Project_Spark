// ============================================================================
// LevelProgression.cs
// ============================================================================

using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class LevelProgression : MonoBehaviour
    {
        [SerializeField]
        GameObject successPanel;

        [SerializeField]
        int nextScene;

        public void CompleteLevel()
        {
            successPanel.SetActive(true);

            Debug.Log("LEVEL COMPLETE");
        }

        public void NextLevel()
        {
            SceneManager.LoadScene(nextScene);
        }

        public void Restart()
        {
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex);
        }
    }
}