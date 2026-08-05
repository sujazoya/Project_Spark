using System.Collections.Generic;

namespace ProjectSpark.UI.Inventory
{
    public sealed class RecentComponents
    {
        private readonly Queue<InventoryItem>
            history =
                new();

        public void Add(
            InventoryItem item)
        {
            history.Enqueue(item);

            while (history.Count > 20)
                history.Dequeue();
        }
    }
}
