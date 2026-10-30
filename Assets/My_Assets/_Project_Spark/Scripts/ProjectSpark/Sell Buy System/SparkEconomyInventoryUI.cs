using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    /// <summary>Inspector-driven selected-inventory-item panel; no runtime UI creation.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkEconomyInventoryUI : MonoBehaviour
    {
        [SerializeField] private SparkEconomyService economy;
        [SerializeField] private SparkPurchasedComponentSpawner spawner;
        [SerializeField] private string selectedItemId = "resistor_220";
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text ownedText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Image icon;
        [SerializeField] private Button placeButton;
        private SparkEconomyService subscribed;

        private void OnEnable() { Bind(); }
        private void Start() { Bind(); }
        private void OnDisable() { Unbind(); }

        private void Bind()
        {
            if (economy == null) economy = SparkEconomyService.Instance;
            if (economy != subscribed)
            {
                Unbind();
                subscribed = economy;
                if (subscribed != null) subscribed.Changed += Refresh;
            }
            Refresh();
        }

        private void Unbind()
        {
            if (subscribed != null) subscribed.Changed -= Refresh;
            subscribed = null;
        }

        public void SelectItem(string itemId)
        {
            selectedItemId = itemId;
            if (messageText != null) messageText.text = "";
            Refresh();
        }

        public void PlaceSelected()
        {
            if (spawner == null) { Message("Placement system unavailable."); return; }
            bool ok = spawner.TryPlaceOwnedItem(selectedItemId);
            Message(ok ? "Component moved to storage shelf." : "Cannot place: check stock and empty storage slot.");
            Refresh();
        }

        private void Message(string text)
        {
            if (messageText != null) messageText.text = text;
        }

        public void Refresh()
        {
            if (economy == null) return;
            SparkEconomyCatalog.Item entry = economy.Catalog != null ? economy.Catalog.Find(selectedItemId) : null;
            int count = economy.GetItemCount(selectedItemId);
            if (nameText != null) nameText.text = entry != null ? entry.displayName : "Unknown component";
            if (ownedText != null) ownedText.text = "Inventory: " + count;
            if (icon != null)
            {
                icon.sprite = entry != null ? entry.icon : null;
                icon.enabled = icon.sprite != null;
            }
            if (placeButton != null) placeButton.interactable = entry != null && entry.prefab != null && count > 0;
        }
    }
}
