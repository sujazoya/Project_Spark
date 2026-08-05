using UnityEngine;

namespace ProjectSpark.Gameplay.Inventory.UI
{
    public sealed class ToolboxUI : MonoBehaviour
    {
        [SerializeField]
        private InventoryManager inventory;

        [SerializeField]
        private ToolboxSlotUI slotPrefab;

        [SerializeField]
        private Transform container;

        private void Start()
        {
            foreach(var item in inventory.Items)
            {
                var slot = Instantiate(
                    slotPrefab,
                    container);

                slot.Initialize(item);
            }
        }
    }
}
