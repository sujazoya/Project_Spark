
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;

namespace ProjectSpark.UI
{
    [DisallowMultipleComponent]
    public sealed class ProjectSparkShopHUD : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private ProjectSparkShopRuntime shop;
        [SerializeField] private ProjectSparkInventoryPanel inventory;
        [Header("Inventory Notification")]
        [SerializeField] private GameObject inventoryBadge;
        [SerializeField] private TMP_Text inventoryBadgeText;

        [Header("Economy")]
        [SerializeField] private SparkEconomyService economy;

        [Header("Buttons")]
        [SerializeField] private Button shopButton;
        [SerializeField] private Button inventoryButton;

        [Header("Balances")]
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text diamondsText;
        [Header("Drawer")]
        [SerializeField] private ProjectSparkShopHUDDrawer drawer;

        private SparkEconomyService subscribedEconomy;

        private void Awake()
        {
            if (shopButton != null)
                shopButton.onClick.AddListener(OpenShop);

            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(OpenInventory);
                if (drawer == null)
                drawer = GetComponent<ProjectSparkShopHUDDrawer>();
        }
private void OnEnable()
{
    BindEconomy();

    // Economy initialization may not have
    // completed yet.
    if (economy != null)
        Refresh();
}

        private void Start()
        {
            BindEconomy();
            Refresh();
        }

        private void OnDisable()
        {
            UnbindEconomy();
        }

        private void OnDestroy()
        {
            UnbindEconomy();

            if (shopButton != null)
                shopButton.onClick.RemoveListener(OpenShop);

            if (inventoryButton != null)
                inventoryButton.onClick.RemoveListener(OpenInventory);
        }

        private void BindEconomy()
        {
            if (economy == null)
                economy = SparkEconomyService.Instance;

            if (economy == subscribedEconomy)
                return;

            UnbindEconomy();

            subscribedEconomy = economy;

            if (subscribedEconomy != null)
                subscribedEconomy.Changed += Refresh;
        }

        private void UnbindEconomy()
        {
            if (subscribedEconomy != null)
                subscribedEconomy.Changed -= Refresh;

            subscribedEconomy = null;
        }

        public void OpenShop()
        {
            if (shop == null)
                return;

            if (inventory != null)
                inventory.Hide();

            shop.Show();
            if (drawer != null)
            drawer.Close();
        }

        public void OpenInventory()
        {
            if (inventory == null)
                return;

            if (shop != null)
                shop.Hide();

            inventory.Show();
        }

        public void Refresh()
        {
            if (economy == null)
                economy = SparkEconomyService.Instance;

            if (coinsText != null)
                coinsText.text = economy != null
                    ? economy.Coins.ToString("N0")
                    : "—";

            if (diamondsText != null)
                diamondsText.text = economy != null
                    ? economy.Diamonds.ToString("N0")
                    : "—";

                    RefreshInventoryBadge();
        }
        
private void RefreshInventoryBadge()
{
    if (inventoryBadge == null)
        return;

    if (economy == null)
        economy = SparkEconomyService.Instance;

    if (economy == null || economy.Catalog == null ||
        economy.Catalog.Items == null)
    {
        inventoryBadge.SetActive(false);
        return;
    }

    long totalOwned = 0;

    foreach (SparkEconomyCatalog.Item item
             in economy.Catalog.Items)
    {
        if (item == null ||
            string.IsNullOrWhiteSpace(item.id))
            continue;

        int count = economy.GetItemCount(item.id);

        if (count > 0)
            totalOwned += count;
    }

    inventoryBadge.SetActive(totalOwned > 0);

    if (inventoryBadgeText != null)
    {
        inventoryBadgeText.text =
            totalOwned > 99
                ? "99+"
                : totalOwned.ToString();
    }
}

    }
}
