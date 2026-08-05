using UnityEngine;

namespace ProjectSpark.UI.Inventory
{
    public sealed class ComponentPalette
        : MonoBehaviour
    {
        [SerializeField]
        private InventoryManager inventory;

        private void OnEnable()
        {
            InventoryEvents.InventoryChanged +=
                Refresh;
        }

        private void OnDisable()
        {
            InventoryEvents.InventoryChanged -=
                Refresh;
        }

        public void Refresh()
        {

        }
    }
}
