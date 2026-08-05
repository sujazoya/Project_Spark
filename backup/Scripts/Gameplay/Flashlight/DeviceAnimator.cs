// ============================================================================
// DeviceAnimator.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    [RequireComponent(typeof(Animator))]
    public sealed class DeviceAnimator : MonoBehaviour
    {
        Animator animator;

        static readonly int OpenHash =
            Animator.StringToHash("Open");

        static readonly int CloseHash =
            Animator.StringToHash("Close");

        static readonly int TestHash =
            Animator.StringToHash("Test");

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void Open()
        {
            animator.SetTrigger(OpenHash);
        }

        public void Close()
        {
            animator.SetTrigger(CloseHash);
        }

        public void Test()
        {
            animator.SetTrigger(TestHash);
        }
    }
}