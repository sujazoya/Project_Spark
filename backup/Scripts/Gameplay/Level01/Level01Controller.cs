// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Level01/Level01Controller.cs

using UnityEngine;
using ProjectSpark.Gameplay.Wiring;

namespace ProjectSpark.Gameplay.Level01
{
    public sealed class Level01Controller : MonoBehaviour
    {
        [SerializeField]
        private CircuitValidator validator;

        [SerializeField]
        private GameObject levelCompleteUI;

        private bool completed;

        private void Update()
        {
            if (completed)
                return;

            if (!validator.Validate())
                return;

            completed = true;

            levelCompleteUI.SetActive(true);

            Debug.Log("LEVEL COMPLETE");
        }
    }
}