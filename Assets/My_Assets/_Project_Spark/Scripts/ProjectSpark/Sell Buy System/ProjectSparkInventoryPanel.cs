using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;

namespace ProjectSpark.UI
{
    [DisallowMultipleComponent]
    public sealed class ProjectSparkInventoryPanel : MonoBehaviour
    {
        [SerializeField] private SparkEconomyService economy;
        [SerializeField] private SparkPurchasedComponentSpawner spawner;
        [SerializeField] private bool startVisible = false;
        [SerializeField] private bool showUnowned = false;

        private TMP_Text coinBalance, diamondBalance, selectedName, selectedCount, status;
        private Image selectedIcon;
        private Button deliverButton;
        private TMP_InputField search;
        private Transform content;
        private GameObject cardTemplate;
        private string selectedId;
        private readonly List<GameObject> cards = new List<GameObject>();
        private SparkEconomyService subscribed;
        private bool ready;

        private void Awake()
        {
            Transform window = transform.Find("Backdrop/InventoryWindow");
            if (window == null) { Debug.LogError("[Inventory] Generated hierarchy missing.", this); return; }
            Transform header = window.Find("Header");
            Transform body = window.Find("Body");
            Transform details = body != null ? body.Find("Details") : null;
            content = body != null ? body.Find("ItemScroll/Viewport/Content") : null;
            if (header == null || details == null || content == null) { Debug.LogError("[Inventory] Panel references missing.", this); return; }
            coinBalance = FindText(header, "CoinBalance");
            diamondBalance = FindText(header, "DiamondBalance");
            selectedName = FindText(details, "SelectedName");
            selectedCount = FindText(details, "SelectedCount");
            status = FindText(details, "StatusText");
            selectedIcon = details.Find("SelectedIcon")?.GetComponent<Image>();
            deliverButton = details.Find("DeliverButton")?.GetComponent<Button>();
            search = body.Find("Search")?.GetComponent<TMP_InputField>();
            cardTemplate = content.Find("ItemCardTemplate")?.gameObject;
            if (cardTemplate == null) { Debug.LogError("[Inventory] ItemCardTemplate missing.", this); return; }
            cardTemplate.SetActive(false);
            if (deliverButton != null) deliverButton.onClick.AddListener(DeliverSelected);
            if (search != null) search.onValueChanged.AddListener(_ => Refresh());
            Button close = header.Find("CloseButton")?.GetComponent<Button>();
            if (close != null) close.onClick.AddListener(Hide);
            ready = true;
            if (!startVisible) gameObject.SetActive(false);
        }

        private void OnEnable() { ResolveAndBind(); Refresh(); }
        private void Start() { ResolveAndBind(); Refresh(); }
        private void OnDisable() { Unbind(); }
        private void OnDestroy() { Unbind(); }

        private void ResolveAndBind()
        {
            if (economy == null) economy = SparkEconomyService.Instance;
            if (spawner == null) spawner = FindFirstObjectByType<SparkPurchasedComponentSpawner>();
            if (economy == subscribed) return;
            Unbind();
            subscribed = economy;
            if (subscribed != null) subscribed.Changed += Refresh;
        }

        private void Unbind()
        {
            if (subscribed != null) subscribed.Changed -= Refresh;
            subscribed = null;
        }

        public void Show() { gameObject.SetActive(true); ResolveAndBind(); Refresh(); }
        public void Hide() { gameObject.SetActive(false); }
        public void Toggle() { if (gameObject.activeSelf) Hide(); else Show(); }

        public void Refresh()
        {
            if (!ready) return;
            if (economy == null) economy = SparkEconomyService.Instance;
            if (coinBalance != null) coinBalance.text = "COINS  " + (economy != null ? economy.Coins.ToString("N0") : "—");
            if (diamondBalance != null) diamondBalance.text = "DIAMONDS  " + (economy != null ? economy.Diamonds.ToString("N0") : "—");
            for (int i = 0; i < cards.Count; i++) if (cards[i] != null) Destroy(cards[i]);
            cards.Clear();
            if (economy == null || economy.Catalog == null) { SetStatus("Assign EconomyRoot and a catalog."); UpdateDetails(); return; }
            string term = search != null ? search.text.Trim() : "";
            bool selectedStillAvailable = false;
            SparkEconomyCatalog.Item[] entries = economy.Catalog.Items;
            if (entries == null) { UpdateDetails(); return; }
            foreach (SparkEconomyCatalog.Item entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.id)) continue;
                int owned = economy.GetItemCount(entry.id);
                if (!showUnowned && owned <= 0) continue;
                if (term.Length > 0 && (entry.displayName == null || entry.displayName.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0) && entry.id.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (entry.id == selectedId) selectedStillAvailable = true;
                GameObject card = Instantiate(cardTemplate, content);
                card.name = "Owned_" + entry.id;
                card.SetActive(true);
                cards.Add(card);
                TMP_Text name = FindText(card.transform, "Name");
                TMP_Text count = FindText(card.transform, "Count");
                Image icon = card.transform.Find("Icon")?.GetComponent<Image>();
                if (name != null) name.text = entry.displayName;
                if (count != null) count.text = "OWNED ×" + owned;
                if (icon != null) { icon.sprite = entry.icon; icon.enabled = entry.icon != null; }
                Button button = card.GetComponent<Button>();
                if (button != null) { string id = entry.id; button.onClick.AddListener(() => Select(id)); }
            }
            if (!selectedStillAvailable)
            {
                selectedId = null;
                foreach (SparkEconomyCatalog.Item entry in entries)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.id)) continue;
                    if (!showUnowned && economy.GetItemCount(entry.id) <= 0) continue;
                    if (term.Length > 0 && (entry.displayName == null || entry.displayName.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0) && entry.id.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    selectedId = entry.id;
                    break;
                }
            }
            UpdateDetails();
        }

        public void Select(string itemId) { selectedId = itemId; SetStatus(""); UpdateDetails(); }

        public void DeliverSelected()
        {
            ResolveAndBind();
            if (spawner == null) { SetStatus("Purchased component spawner missing."); return; }
            if (string.IsNullOrWhiteSpace(selectedId)) { SetStatus("Choose an owned component."); return; }
            bool placed = spawner.TryPlaceOwnedItem(selectedId);
            Refresh();
            SetStatus(placed ? "Delivered to shelf storage." : "Delivery failed. Check stock, prefab and free shelf slot.");            
            
        }

        private void UpdateDetails()
        {
            SparkEconomyCatalog.Item entry = economy != null && economy.Catalog != null ? economy.Catalog.Find(selectedId) : null;
            int count = entry != null ? economy.GetItemCount(selectedId) : 0;
            if (selectedName != null) selectedName.text = entry != null ? entry.displayName : "SELECT A COMPONENT";
            if (selectedCount != null) selectedCount.text = entry != null ? "Owned: " + count : "No items available";
            if (selectedIcon != null) { selectedIcon.sprite = entry != null ? entry.icon : null; selectedIcon.enabled = selectedIcon.sprite != null; }
            if (deliverButton != null) deliverButton.interactable = entry != null && entry.prefab != null && count > 0;
        }

        private void SetStatus(string value) { if (status != null) status.text = value; }
        private static TMP_Text FindText(Transform parent, string path) => parent.Find(path)?.GetComponent<TMP_Text>();
    }
}
