using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Career
{
    public sealed class WorkshopInventory
    {
        private readonly Dictionary<string,int>
            stock = new();

        public void AddPart(
            string id,
            int quantity)
        {
            if (!stock.ContainsKey(id))
                stock[id] = 0;

            stock[id] += quantity;
        }

        public bool ConsumePart(string id)
        {
            if (!stock.ContainsKey(id))
                return false;

            if (stock[id] <= 0)
                return false;

            stock[id]--;

            return true;
        }
    }
}
