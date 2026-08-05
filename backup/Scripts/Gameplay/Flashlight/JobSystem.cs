// ============================================================================
// JobSystem.cs
// ============================================================================

using UnityEngine;
using ProjectSpark.Gameplay.UI;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class JobSystem : MonoBehaviour
    {
        [SerializeField]
        CustomerDialogue dialogue;

        [SerializeField]
        RewardSystem rewards;

        [SerializeField]
       SuccessPanel success;

        bool accepted;

        public void AcceptJob()
        {
            accepted = true;
        }

        public void CompleteJob()
        {
            if (!accepted)
                return;

            rewards.Reward();

            success.Show();
        }
    }
}