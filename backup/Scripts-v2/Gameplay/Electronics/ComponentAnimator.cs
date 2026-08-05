using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class ComponentAnimator : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private ElectronicComponent component;

        private static readonly int ActiveHash =
            Animator.StringToHash("Active");

        private void Update()
        {
            animator.SetBool(
                ActiveHash,
                component.State.IsActive);
        }
    }
}
