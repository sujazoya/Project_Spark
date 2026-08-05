using UnityEngine;

namespace ProjectSpark.UI.Inventory
{
    public sealed class InventoryManager
        : MonoBehaviour
    {
        [SerializeField]
        private InventoryDatabase database;

        public InventoryDatabase Database
            => database;

        public void Select(
            InventoryItem item)
        {
            InventoryEvents
                .RaiseSelected(item);
        }
    }
}
