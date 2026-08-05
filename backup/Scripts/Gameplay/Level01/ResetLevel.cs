// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Level01/ResetLevel.cs

using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.Gameplay.Level01
{
    public sealed class ResetLevel : MonoBehaviour
    {
        public void ResetScene()
        {
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex);
        }
    }
}