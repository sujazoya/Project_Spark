using UnityEngine;

namespace ProjectSpark.Gameplay.Career
{
    public sealed class EconomyManager
        : MonoBehaviour
    {
        public int Coins { get; private set; }

        public int XP { get; private set; }

        public void AddReward(
            int coins,
            int xp)
        {
            Coins += coins;
            XP += xp;
        }

        public bool SpendCoins(int amount)
        {
            if (Coins < amount)
                return false;

            Coins -= amount;
            return true;
        }
    }
}
