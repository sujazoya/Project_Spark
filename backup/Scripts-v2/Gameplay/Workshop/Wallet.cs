using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Workshop
{
    public sealed class Wallet
    {
        private readonly Dictionary<Currency,int>
            balances = new();

        public int Get(Currency currency)
        {
            balances.TryGetValue(
                currency,
                out int value);

            return value;
        }

        public void Add(
            Currency currency,
            int amount)
        {
            balances[currency] =
                Get(currency) + amount;
        }

        public bool Spend(
            Currency currency,
            int amount)
        {
            if(Get(currency) < amount)
                return false;

            balances[currency] -= amount;

            return true;
        }
    }
}
