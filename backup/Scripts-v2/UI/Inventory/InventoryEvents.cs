using System;

namespace ProjectSpark.UI.Inventory
{
    public static class InventoryEvents
    {
        public static event Action InventoryChanged;

        public static event Action<InventoryItem>
            Selected;

        public static void RaiseChanged()
        {
            InventoryChanged?.Invoke();
        }

        public static void RaiseSelected(
            InventoryItem item)
        {
            Selected?.Invoke(item);
        }
    }
}
