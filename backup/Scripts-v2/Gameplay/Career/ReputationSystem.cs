using UnityEngine;

namespace ProjectSpark.Gameplay.Career
{
    public sealed class ReputationSystem
        : MonoBehaviour
    {
        [SerializeField]
        private int reputation;

        public int Reputation => reputation;

        public void Add(int amount)
        {
            reputation += amount;

            CareerEvents.RaiseReputation(
                reputation);
        }
    }
}
