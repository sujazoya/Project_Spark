using UnityEngine;

namespace ProjectSpark.Presentation.Electronics
{
    public sealed class ComponentAnimator
        : MonoBehaviour
    {
        [SerializeField]
        Animator animator;

        public void Powered(bool powered)
        {
            animator.SetBool(
                "Powered",
                powered);
        }

        public void Broken(bool broken)
        {
            animator.SetBool(
                "Broken",
                broken);
        }
    }
}
