
using UnityEngine;
using ProjectSpark.AI.Tutorial;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkTutorialStarter : MonoBehaviour
    {
        [SerializeField]
        private SparkTutorialDirector tutorialDirector;

        [ContextMenu("Tutorial / Start")]
        public void StartTutorial()
        {
            if (tutorialDirector == null)
            {
                tutorialDirector =
                    FindFirstObjectByType<SparkTutorialDirector>();
            }

            if (tutorialDirector == null)
            {
                Debug.LogWarning(
                    "[Tutorial] Director not found.");
                return;
            }

            tutorialDirector.BeginDefault();

            Debug.Log("[Tutorial] Start requested.");
        }

        [ContextMenu("Tutorial / Restart")]
        public void RestartTutorial()
        {
            if (tutorialDirector != null)
                tutorialDirector.Restart();
        }
    }
}
