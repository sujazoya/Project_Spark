using UnityEngine;

namespace ProjectSpark.UI.Inventory
{
    public sealed class ComponentSearch
        : MonoBehaviour
    {
        public string SearchText
        {
            get;
            private set;
        }

        public void UpdateSearch(
            string text)
        {
            SearchText =
                text.ToLower();

            InventoryEvents
                .RaiseChanged();
        }
    }
}
