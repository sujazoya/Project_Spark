// ============================================================================
// RewardSystem.cs
// ============================================================================

using TMPro;
using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class RewardSystem : MonoBehaviour
    {
        [SerializeField] TMP_Text coins;
        [SerializeField] TMP_Text xp;
        [SerializeField] TMP_Text reputation;

        public void Reward()
        {
            coins.text = "+100";

            xp.text = "+50 XP";

            reputation.text = "+5 REP";
        }
    }
}