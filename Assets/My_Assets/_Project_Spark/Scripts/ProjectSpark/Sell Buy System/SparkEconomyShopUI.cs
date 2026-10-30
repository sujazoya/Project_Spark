using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    /// <summary>Wire existing UI buttons to these public methods in the Inspector.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkEconomyShopUI : MonoBehaviour
    {
        [SerializeField] private SparkEconomyService economy;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text diamondsText;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text pricesText;
        [SerializeField] private TMP_Text ownedText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Image itemIcon;
        [SerializeField] private string selectedItemId = "resistor_220";

        private void OnEnable()
        {
            if (economy == null) economy = SparkEconomyService.Instance;
            if (economy != null) economy.Changed += Refresh;
            Refresh();
        }
        private void OnDisable() { if (economy != null) economy.Changed -= Refresh; }

        public void SelectItem(string itemId)
        {
            selectedItemId = itemId;
            if (statusText != null) statusText.text = "";
            Refresh();
        }

        public void PurchaseWithCoins() { Purchase(false); }
        public void PurchaseWithDiamonds() { Purchase(true); }

        private void Purchase(bool withDiamonds)
        {
            if (economy == null) economy = SparkEconomyService.Instance;
            if (economy == null) { SetStatus("Economy unavailable."); return; }
            bool success = economy.TryPurchase(selectedItemId, withDiamonds, out string message);
            SetStatus(message);
            if (success) Refresh();
        }

        private void SetStatus(string value) { if (statusText != null) statusText.text = value; }

        public void Refresh()
        {
            if (economy == null) return;
            if (coinsText != null) coinsText.text = economy.Coins.ToString("N0");
            if (diamondsText != null) diamondsText.text = economy.Diamonds.ToString("N0");
            var item = economy.Catalog != null ? economy.Catalog.Find(selectedItemId) : null;
            if (itemNameText != null) itemNameText.text = item != null ? item.displayName : "Select a component";
            if (pricesText != null)
                pricesText.text = item == null ? "" :
                    (item.sellForCoins ? item.coinPrice + " coins" : "") +
                    (item.sellForCoins && item.sellForDiamonds ? "  /  " : "") +
                    (item.sellForDiamonds ? item.diamondPrice + " diamonds" : "");
            if (ownedText != null) ownedText.text = "Owned: " + economy.GetItemCount(selectedItemId);
            if (itemIcon != null) { itemIcon.sprite = item != null ? item.icon : null; itemIcon.enabled = itemIcon.sprite != null; }
        }
    }
}
