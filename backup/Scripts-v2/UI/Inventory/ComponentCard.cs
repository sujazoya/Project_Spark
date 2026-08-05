using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.Inventory
{
    public sealed class ComponentCard
        : MonoBehaviour
    {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private TMP_Text title;

        private InventoryItem item;

        public void Bind(
            InventoryItem value)
        {
            item = value;

            icon.sprite =
                item.Definition.Icon;

            title.text =
                item.Definition.DisplayName;
        }

        public void Select()
        {
            InventoryEvents
                .RaiseSelected(item);
        }
    }
}
