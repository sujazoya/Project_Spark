
using UnityEngine;

namespace ProjectSpark.UI
{
    [DisallowMultipleComponent]
    public class SparkUIAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private static readonly int PlayInHash =
            Animator.StringToHash("PlayIn");

        private static readonly int PlayOutHash =
            Animator.StringToHash("PlayOut");

        public bool IsVisible { get; private set; }

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        public void PlayIn()
        {
            if (animator == null)
            {
                Debug.LogWarning(
                    "SparkUIAnimationController: Animator is missing.",
                    this);

                return;
            }

            animator.ResetTrigger(PlayOutHash);
            animator.SetTrigger(PlayInHash);

            IsVisible = true;

            Debug.Log("Spark UI → Play IN");
        }

        public void PlayOut()
        {
            if (animator == null)
            {
                Debug.LogWarning(
                    "SparkUIAnimationController: Animator is missing.",
                    this);

                return;
            }

            animator.ResetTrigger(PlayInHash);
            animator.SetTrigger(PlayOutHash);

            IsVisible = false;

            Debug.Log("Spark UI → Play OUT");
        }

        public void Toggle()
        {
            if (IsVisible)
                PlayOut();
            else
                PlayIn();
        }
    }
}
