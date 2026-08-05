using UnityEngine;

namespace ProjectSpark.Gameplay.Tutorials
{
    public sealed class TutorialTrigger
        : MonoBehaviour
    {
        [SerializeField]
        private TutorialManager manager;

        private bool triggered;

        private void OnTriggerEnter(
            Collider other)
        {
            if (triggered)
                return;

            triggered = true;

            manager.StartTutorial();
        }
    }
}
