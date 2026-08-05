using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay.Inventory.UI
{
    public sealed class ToolboxSlotUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text quantity;

        private InventoryItem _item;

        public void Initialize(InventoryItem item)
        {
            _item = item;

            icon.sprite = item.Definition.Icon;

            Refresh();
        }

        public void Refresh()
        {
            quantity.text =
                _item.Infinite
                ? "∞"
                : _item.Remaining.ToString();
        }
    }
}
