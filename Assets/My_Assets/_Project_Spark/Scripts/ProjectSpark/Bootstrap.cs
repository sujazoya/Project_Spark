using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.Bootstrap
{
    public sealed class Bootstrap : MonoBehaviour
    {
        private const string MainMenu = "Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu";

        private void Start()
        {
            if (!Application.CanStreamedLevelBeLoaded(MainMenu))
            {
                Debug.LogError("MainMenu must be included in the build scenes.", this);
                return;
            }

            SceneManager.LoadSceneAsync(MainMenu, LoadSceneMode.Single);
        }
    }
}
