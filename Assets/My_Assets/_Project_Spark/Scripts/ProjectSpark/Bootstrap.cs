using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.Bootstrap
{
    public sealed class Bootstrap : MonoBehaviour
    {
        private const string MainMenuPath = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";

        private void Start()
        {
            int buildIndex = SceneUtility.GetBuildIndexByScenePath(MainMenuPath);
            if (buildIndex < 0)
            {
                Debug.LogError("The active build profile cannot load " + MainMenuPath + ".", this);
                return;
            }

            SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
        }
    }
}
