using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;

namespace ProjectSpark.UI
{
    /// <summary>
    /// Binds the generated ProjectSpark shop panel to the existing offline economy.
    /// Does not modify shop prices/balances or create purchasable components directly.
    /// A purchased item is added to inventory via SparkEconomyService.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProjectSparkShopRuntime : MonoBehaviour
    {
        [SerializeField] private SparkEconomyService economy;
        [SerializeField] private bool startVisible = false;
        [SerializeField, Min(1)] private int maximumQuantity = 99;
        [SerializeField] private bool warnIfCatalogMissing = true;

        [Header("Purchase Confirmation")]
        [SerializeField] private bool requireConfirmation = true;
        [SerializeField, Min(1)] private int confirmAboveQuantity = 1;
        [SerializeField] private bool alwaysConfirmDiamonds = true;

        private GameObject confirmationPanel;
        private TMP_Text confirmationText;
        private bool pendingUseDiamonds;
        private int pendingQuantity;
        private string pendingItemId;

        private Transform content;
        private Transform details;
        private Transform header;
        private Transform categories;
        private GameObject cardTemplate;
        private readonly List<GameObject> runtimeCards = new List<GameObject>();
        private readonly Dictionary<string, GameObject> cardsById = new Dictionary<string, GameObject>();
        private TMP_Text coinText, diamondText, titleText, specsText, descriptionText, quantityText, buyText;
        private Image previewImage;
        private TMP_InputField searchInput;
        private Button diamondBuyButton;
        private TMP_Text diamondBuyText;
        private string searchText = "";
        private int sortMode; // 0: name, 1: coin price, 2: owned
        private readonly List<SparkEconomyCatalog.Item> sortedItems = new List<SparkEconomyCatalog.Item>();
        private string selectedId;
        private string categoryFilter = "ALL";
        private int quantity = 1;
        private bool initialized;
        private bool subscribed;
        private string lastMessage = "";

        private void Awake()
        {
            // The generated builder creates one Canvas at the root.
            Transform window = transform.Find("Backdrop/ShopWindow");
            if (window == null) { Debug.LogError("[Shop] Generated ShopWindow hierarchy not found.", this); return; }
            header = window.Find("Header");
            Transform body = window.Find("Body");
            categories = body != null ? body.Find("Categories") : null;
            details = body != null ? body.Find("SelectedItemDetails") : null;
            content = body != null ? body.Find("Catalog/ProductScroll/Viewport/Content") : null;
            if (header == null || details == null || content == null)
            { Debug.LogError("[Shop] Missing generated header/details/catalog. Rebuild shop prefab.", this); return; }

            coinText = TextAt(header, "CoinBalance");
            diamondText = TextAt(header, "DiamondBalance");
            titleText = TextAt(details, "SelectedTitle");
            specsText = TextAt(details, "Specs");
            descriptionText = TextAt(details, "Description");
            quantityText = TextAt(details, "Quantity");
            buyText = TextAt(details, "BuyButton/Text");
            Transform preview = details.Find("ItemPreview");
            if (preview != null)
            {
                // Image overlay created only at runtime, under the existing prefab hierarchy.
                GameObject iconGO = new GameObject("SelectedIcon_Runtime", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(preview, false);
                RectTransform rt = (RectTransform)iconGO.transform;
                rt.anchorMin = new Vector2(.08f, .08f); rt.anchorMax = new Vector2(.92f, .92f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                previewImage = iconGO.GetComponent<Image>();
                previewImage.preserveAspect = true;
                previewImage.raycastTarget = false;
            }
            Hook(details, "DecreaseQuantity", () => ChangeQuantity(-1));
            Hook(details, "IncreaseQuantity", () => ChangeQuantity(+1));
            Hook(details, "BuyButton", () => BuySelected(false));
            BuildDiamondButton();
            BuildSearchAndSort(body);
            Hook(header, "CloseButton", Hide);
            if (categories != null)
            {
                for (int i = 0; i < categories.childCount; i++)
                {
                    Transform child = categories.GetChild(i);
                    if (!child.name.StartsWith("Category_", StringComparison.Ordinal)) continue;
                    string filter = child.name.Substring("Category_".Length);
                    Button button = child.GetComponent<Button>();
                    if (button != null) button.onClick.AddListener(() => SetCategory(filter));
                }
            }
            // Existing generated cards are visual examples only. Use the first as a template.
            if (content.childCount > 0) cardTemplate = content.GetChild(0).gameObject;
            for (int i = 0; i < content.childCount; i++) content.GetChild(i).gameObject.SetActive(false);
            initialized = true;
        }

        private void Start()
        {
            BindEconomy();
            if (!initialized) return;
            BuildCards();
            Refresh();
            if (!startVisible) gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            BindEconomy();
            if (initialized) Refresh();
        }

        private void OnDisable() { UnbindEconomy(); }

        private void BindEconomy()
        {
            if (economy == null) economy = SparkEconomyService.Instance;
            if (!subscribed && economy != null)
            {
                economy.Changed += Refresh;
                subscribed = true;
            }
        }
        private void UnbindEconomy()
        {
            if (subscribed && economy != null) economy.Changed -= Refresh;
            subscribed = false;
        }

        private static TMP_Text TextAt(Transform parent, string path)
        {
            Transform t = parent != null ? parent.Find(path) : null;
            return t != null ? t.GetComponent<TMP_Text>() : null;
        }
        private static void Hook(Transform parent, string path, UnityEngine.Events.UnityAction action)
        {
            Transform t = parent.Find(path);
            if (t != null && t.TryGetComponent(out Button button)) button.onClick.AddListener(action);
        }

        private static string DetermineCategory(string displayName)
        {
            if (string.IsNullOrEmpty(displayName)) return "OTHER";
            string s = displayName.ToLowerInvariant();
            if (s.Contains("resistor") || s.Contains("ohm") || s.Contains(" Ω")) return "RESISTORS";
            if (s.Contains("capacitor") || s.Contains(" µf") || s.Contains(" nf")) return "CAPACITORS";
            if (s.Contains("led")) return "LEDs";
            if (s.Contains("diode")) return "DIODES";
            if (s.Contains("transistor")) return "TRANSISTORS";
            if (s.Contains("switch") || s.Contains("button")) return "SWITCHES";
            if (s.Contains("motor")) return "MOTORS";
            if (s.Contains("battery") || s.Contains("supply")) return "POWER";
            if (s.Contains("wire") || s.Contains("connector")) return "CONNECTORS";
            if (s.Contains("tool")) return "TOOLS";
            if (s.Contains(" ic") || s.Contains("chip")) return "ICs";
            return "OTHER";
        }

        public void BuildCards()
        {
            if (!initialized || cardTemplate == null) return;
            foreach (GameObject go in runtimeCards) if (go != null) Destroy(go);
            runtimeCards.Clear(); cardsById.Clear();
            if (economy == null || economy.Catalog == null)
            {
                if (warnIfCatalogMissing) Debug.LogWarning("[Shop] Assign a SparkEconomyCatalog to EconomyRoot.", this);
                return;
            }
            foreach (SparkEconomyCatalog.Item item in economy.Catalog.Items)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.id)) continue;
                if (cardsById.ContainsKey(item.id)) { Debug.LogWarning("[Shop] Duplicate item ID " + item.id, this); continue; }
                GameObject card = Instantiate(cardTemplate, content, false);
                card.name = "ShopItem_" + item.id;
                card.SetActive(true);
                runtimeCards.Add(card); cardsById.Add(item.id, card);
                Transform tr = card.transform;
                TMP_Text itemName = TextAt(tr, "ItemName");
                TMP_Text subtitle = TextAt(tr, "Subtitle");
                TMP_Text price = TextAt(tr, "Price");
                if (itemName != null) itemName.text = item.displayName;
                if (subtitle != null) subtitle.text = DetermineCategory(item.displayName);
                if (price != null) price.text = item.sellForCoins ? "● " + item.coinPrice : item.sellForDiamonds ? "◆ " + item.diamondPrice : "Unavailable";
                Transform iconHolder = tr.Find("IconPlaceholder");
                if (iconHolder != null && item.icon != null)
                {
                    Transform glyph = iconHolder.Find("Glyph");
                    if (glyph != null) glyph.gameObject.SetActive(false);
                    GameObject iconGO = new GameObject("Icon_Runtime", typeof(RectTransform), typeof(Image));
                    iconGO.transform.SetParent(iconHolder, false);
                    RectTransform iconRect = (RectTransform)iconGO.transform;
                    iconRect.anchorMin = new Vector2(.06f, .06f); iconRect.anchorMax = new Vector2(.94f, .94f);
                    iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
                    Image img = iconGO.GetComponent<Image>();
                    img.sprite = item.icon; img.preserveAspect = true; img.raycastTarget = false;
                }
                string id = item.id;
                Button purchase = tr.Find("CardBuyButton")?.GetComponent<Button>();
                if (purchase != null)
                {
                    purchase.onClick.RemoveAllListeners();
                    purchase.onClick.AddListener(() => { Select(id); BuyOneWithCoins(); });
                    purchase.interactable = item.sellForCoins;
                }
                // Click anywhere else on the card to inspect it.
                Button selectButton = card.GetComponent<Button>();
                if (selectButton == null) selectButton = card.AddComponent<Button>();
                selectButton.onClick.AddListener(() => Select(id));
            }
            Transform first = content.Find("ShopItem_" + selectedId);
            if (string.IsNullOrEmpty(selectedId) || first == null)
            {
                foreach (SparkEconomyCatalog.Item item in economy.Catalog.Items)
                    if (item != null && cardsById.ContainsKey(item.id)) { selectedId = item.id; break; }
            }
            FilterCards();
        }

        private void Select(string id)
        {
            selectedId = id;
            quantity = 1;
            lastMessage = "";
            Refresh();
        }
        private void SetCategory(string filter) { categoryFilter = filter; FilterCards(); }
        private void OnSearchChanged(string value)
        {
            searchText = value ?? "";
            FilterCards();
        }
        private void CycleSort()
        {
            sortMode = (sortMode + 1) % 3;
            FilterCards();
        }
        private void FilterCards()
        {
            if (economy == null || economy.Catalog == null) return;
            sortedItems.Clear();
            foreach (SparkEconomyCatalog.Item item in economy.Catalog.Items)
            {
                if (item == null || !cardsById.TryGetValue(item.id, out GameObject card) || card == null) continue;
                bool categoryOk = categoryFilter == "ALL" || DetermineCategory(item.displayName) == categoryFilter;
                bool searchOk = string.IsNullOrWhiteSpace(searchText) ||
                    (!string.IsNullOrEmpty(item.displayName) && item.displayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(item.id) && item.id.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                bool visible = categoryOk && searchOk;
                card.SetActive(visible);
                if (visible) sortedItems.Add(item);
            }
            sortedItems.Sort((a, b) =>
            {
                int c = sortMode == 1 ? a.coinPrice.CompareTo(b.coinPrice) :
                    sortMode == 2 ? economy.GetItemCount(b.id).CompareTo(economy.GetItemCount(a.id)) : 0;
                return c != 0 ? c : string.Compare(a.displayName, b.displayName, StringComparison.OrdinalIgnoreCase);
            });
            for (int i = 0; i < sortedItems.Count; i++)
                if (cardsById.TryGetValue(sortedItems[i].id, out GameObject card))
                  card.transform.SetAsLastSibling();
            UpdateSortLabel();
        }
        private void ChangeQuantity(int delta)
        {
            quantity = Mathf.Clamp(quantity + delta, 1, Mathf.Max(1, maximumQuantity));
            Refresh();
        }
            private void BuyOneWithCoins()
        {
            RequestPurchase(false, 1);
        }

        private void BuySelected(bool useDiamonds)
        {
            RequestPurchase(useDiamonds, quantity);
        }

      
private void PurchaseQuantity(
    bool useDiamonds,
    int count)
{
    if (economy == null)
        economy = SparkEconomyService.Instance;

    if (economy == null || economy.Catalog == null)
    {
        lastMessage = "Economy unavailable.";
        Refresh();
        return;
    }

    SparkEconomyCatalog.Item item =
        economy.Catalog.Find(selectedId);

    if (item == null)
    {
        lastMessage = "Please select a component.";
        Refresh();
        return;
    }

    count = Mathf.Clamp(
        count,
        1,
        Mathf.Max(1, maximumQuantity));

    bool success = economy.TryPurchaseQuantity(
        selectedId,
        useDiamonds,
        count,
        out string message);

    lastMessage = success
        ? "SUCCESS! " + message
        : message;

    Refresh();
}

private void UpdateCardOwnedCounts()
{
    if (economy == null)
        return;

    foreach (var pair in cardsById)
    {
        string itemId = pair.Key;
        GameObject card = pair.Value;

        if (card == null)
            continue;

        Transform subtitleTransform =
            card.transform.Find("Subtitle");

        if (subtitleTransform == null)
            continue;

        TMP_Text subtitle =
            subtitleTransform.GetComponent<TMP_Text>();

        if (subtitle == null)
            continue;

        SparkEconomyCatalog.Item item =
            economy.Catalog != null
                ? economy.Catalog.Find(itemId)
                : null;

        if (item == null)
            continue;

        int owned = economy.GetItemCount(itemId);

        subtitle.text =
            DetermineCategory(item.displayName) +
            "\nOWNED: " + owned;
    }
}


        private void BuildDiamondButton()
        {
            Transform original = details.Find("BuyButton");
            if (original == null) return;
            // Reuse the exact prefab styling, no additional external graphics needed.
            GameObject clone = Instantiate(original.gameObject, details, false);
            clone.name = "BuyDiamondsButton_Runtime";
            RectTransform coinRect = original as RectTransform;
            RectTransform diamondRect = clone.GetComponent<RectTransform>();
            if (coinRect != null)
            {
                coinRect.anchorMin = new Vector2(.045f, .008f);
                coinRect.anchorMax = new Vector2(.49f, .082f);
                coinRect.offsetMin = coinRect.offsetMax = Vector2.zero;
            }
            diamondRect.anchorMin = new Vector2(.51f, .008f);
            diamondRect.anchorMax = new Vector2(.955f, .082f);
            diamondRect.offsetMin = diamondRect.offsetMax = Vector2.zero;
            diamondBuyButton = clone.GetComponent<Button>();
            diamondBuyText = TextAt(clone.transform, "Text");
            if (diamondBuyButton != null)
            {
                diamondBuyButton.onClick.RemoveAllListeners();
                diamondBuyButton.onClick.AddListener(() => BuySelected(true));
            }
            if (clone.TryGetComponent(out Image image))
                image.color = new Color(.09f, .38f, .82f, 1f);
        }

        private void BuildSearchAndSort(Transform body)
        {
            Transform toolbar = body.Find("Catalog/Toolbar");
            if (toolbar == null) return;
            Transform oldPlaceholder = toolbar.Find("SearchPlaceholder");
            if (oldPlaceholder == null) return;
            GameObject fieldGO = new GameObject("SearchInput_Runtime", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            fieldGO.transform.SetParent(toolbar, false);
            RectTransform rt = (RectTransform)fieldGO.transform;
            rt.anchorMin = new Vector2(.015f, .05f);
            rt.anchorMax = new Vector2(.505f, .95f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            fieldGO.GetComponent<Image>().color = new Color(.035f,.105f,.19f,.95f);
            searchInput = fieldGO.GetComponent<TMP_InputField>();
            GameObject textGO = new GameObject("InputText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(fieldGO.transform, false);
            RectTransform textRT = (RectTransform)textGO.transform;
            textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(12, 0); textRT.offsetMax = new Vector2(-12, 0);
            TextMeshProUGUI entry = textGO.GetComponent<TextMeshProUGUI>();
            entry.fontSize = 21; entry.color = Color.white;
            entry.alignment = TextAlignmentOptions.MidlineLeft;
            entry.enableWordWrapping = false;
            GameObject hintGO = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
            hintGO.transform.SetParent(fieldGO.transform, false);
            RectTransform hintRT = (RectTransform)hintGO.transform;
            hintRT.anchorMin = Vector2.zero; hintRT.anchorMax = Vector2.one;
            hintRT.offsetMin = new Vector2(12,0); hintRT.offsetMax = new Vector2(-12,0);
            TextMeshProUGUI hint = hintGO.GetComponent<TextMeshProUGUI>();
            hint.text = "Search components...";
            hint.fontSize = 21; hint.color = new Color(.57f,.73f,.85f,1f);
            hint.alignment = TextAlignmentOptions.MidlineLeft;
            searchInput.textViewport = rt;
            searchInput.textComponent = entry;
            searchInput.placeholder = hint;
            searchInput.onValueChanged.AddListener(OnSearchChanged);
            oldPlaceholder.gameObject.SetActive(false);
            Transform sort = toolbar.Find("SortButton");
            if (sort != null && sort.TryGetComponent(out Button button))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(CycleSort);
            }
        }

        private void UpdateSortLabel()
        {
            Transform sort = transform.Find("Backdrop/ShopWindow/Body/Catalog/Toolbar/SortButton");
            TMP_Text label = TextAt(sort, "Text");
            if (label != null) label.text = sortMode == 0 ? "SORT: NAME" : sortMode == 1 ? "SORT: PRICE" : "SORT: OWNED";
        }

        public void Refresh()
        {
            if (!initialized) return;
            if (economy == null) economy = SparkEconomyService.Instance;
            if (coinText != null) coinText.text = "●  " + (economy != null ? economy.Coins.ToString("N0") : "—");
            if (diamondText != null) diamondText.text = "◆  " + (economy != null ? economy.Diamonds.ToString("N0") : "—");
            SparkEconomyCatalog.Item item = economy != null && economy.Catalog != null ? economy.Catalog.Find(selectedId) : null;
            if (titleText != null) titleText.text = item != null ? item.displayName : "Select a component";
            if (quantityText != null) quantityText.text = quantity.ToString();
            if (buyText != null) buyText.text = item == null ? "UNAVAILABLE" : item.sellForCoins ? "● " + ((long)item.coinPrice * quantity).ToString("N0") : "COINS N/A";
            if (diamondBuyText != null) diamondBuyText.text = item == null ? "UNAVAILABLE" : item.sellForDiamonds ? "◆ " + ((long)item.diamondPrice * quantity).ToString("N0") : "GEMS N/A";
            if (diamondBuyButton != null) diamondBuyButton.interactable = item != null && item.sellForDiamonds && economy != null && (long)economy.Diamonds >= (long)item.diamondPrice * quantity;
            if (specsText != null) specsText.text = item != null ? "Type: " + DetermineCategory(item.displayName) + "\n\nPrice: " + (item.sellForCoins ? item.coinPrice + " coins" : "—") + "\n\nDiamonds: " + (item.sellForDiamonds ? item.diamondPrice.ToString() : "Not offered") + "\n\nOwned: " + economy.GetItemCount(selectedId) : "";
            if (descriptionText != null) descriptionText.text = string.IsNullOrEmpty(lastMessage) ? "Purchased components go to your inventory." : lastMessage;
            if (previewImage != null) { previewImage.sprite = item != null ? item.icon : null; previewImage.enabled = previewImage.sprite != null; }
            Transform buy = details != null ? details.Find("BuyButton") : null;
            if (buy != null && buy.TryGetComponent(out Button button)) button.interactable = item != null && item.sellForCoins && economy != null && (long)economy.Coins >= (long)item.coinPrice * quantity;

            UpdateCardOwnedCounts();
            RefreshSelectedCardHighlight();
        }
        
private void RefreshSelectedCardHighlight()
{
    foreach (var pair in cardsById)
    {
        GameObject card = pair.Value;

        if (card == null)
            continue;

        Image background = card.GetComponent<Image>();

        if (background == null)
            continue;

        bool selected = pair.Key == selectedId;

        background.color = selected
            ? new Color(0.12f, 0.30f, 0.48f, 1f)
            : new Color(0.06f, 0.13f, 0.22f, 1f);
    }
}

private void RequestPurchase(bool useDiamonds, int count)
{
    if (economy == null || economy.Catalog == null)
        return;

    SparkEconomyCatalog.Item item =
        economy.Catalog.Find(selectedId);

    if (item == null)
        return;

    bool needsConfirmation =
        requireConfirmation &&
        (count >= confirmAboveQuantity ||
         (useDiamonds && alwaysConfirmDiamonds));

    if (!needsConfirmation)
    {
        PurchaseQuantity(useDiamonds, count);
        return;
    }

    pendingItemId = selectedId;
    pendingUseDiamonds = useDiamonds;
    pendingQuantity = count;

    ShowPurchaseConfirmation(item, useDiamonds, count);
}

private void ConfirmPurchase()
{
    if (string.IsNullOrEmpty(pendingItemId))
        return;

    // Preserve the originally confirmed product.
    selectedId = pendingItemId;

    bool useDiamonds = pendingUseDiamonds;
    int count = pendingQuantity;

    ClearPendingPurchase();

    PurchaseQuantity(useDiamonds, count);
}

private void CancelPurchase()
{
    ClearPendingPurchase();
}

        private void ClearPendingPurchase()
        {
            pendingItemId = null;
            pendingQuantity = 0;

                    if (confirmationPanel != null)
                        confirmationPanel.SetActive(false);
        }

        private void ShowPurchaseConfirmation(
            SparkEconomyCatalog.Item item,
            bool useDiamonds,
            int count)
        {
            if (confirmationPanel == null)
            {
                Transform window =
                    transform.Find("Backdrop/ShopWindow");

                if (window == null)
                    return;

                confirmationPanel = new GameObject(
                    "PurchaseConfirmation_Runtime",
                    typeof(RectTransform),
                    typeof(Image));

                confirmationPanel.transform.SetParent(
                    window, false);

                RectTransform panelRect =
                    confirmationPanel.GetComponent<RectTransform>();

                panelRect.anchorMin = new Vector2(.25f, .3f);
                panelRect.anchorMax = new Vector2(.75f, .7f);
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;

                Image background =
                    confirmationPanel.GetComponent<Image>();

                background.color =
                    new Color(.025f, .075f, .14f, .98f);

                confirmationText =
                    CreateConfirmationText(
                        confirmationPanel.transform,
                        "ConfirmationText",
                        new Vector2(.08f, .3f),
                        new Vector2(.92f, .9f));

                CreateConfirmationButton(
                    confirmationPanel.transform,
                    "ConfirmButton",
                    "CONFIRM",
                    new Vector2(.08f, .08f),
                    new Vector2(.47f, .27f),
                    ConfirmPurchase);

                CreateConfirmationButton(
                    confirmationPanel.transform,
                    "CancelButton",
                    "CANCEL",
                    new Vector2(.53f, .08f),
                    new Vector2(.92f, .27f),
                    CancelPurchase);
            }

            long total = (long)(
                useDiamonds ? item.diamondPrice : item.coinPrice
            ) * count;

            confirmationText.text =
                "CONFIRM PURCHASE\n\n" +
                item.displayName + "\n" +
                "Quantity: " + count + "\n" +
                "Total: " + total.ToString("N0") +
                (useDiamonds ? " Diamonds" : " Coins");

            confirmationPanel.SetActive(true);
            confirmationPanel.transform.SetAsLastSibling();
        }

        private TMP_Text CreateConfirmationText(
            Transform parent,
            string objectName,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject go = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(TextMeshProUGUI));

            go.transform.SetParent(parent, false);

            RectTransform rect =
                go.GetComponent<RectTransform>();

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TextMeshProUGUI label =
                go.GetComponent<TextMeshProUGUI>();

            label.fontSize = 24;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;

            return label;
        }

        private void CreateConfirmationButton(
            Transform parent,
            string objectName,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            UnityEngine.Events.UnityAction action)
        {
            GameObject go = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));

            go.transform.SetParent(parent, false);

            RectTransform rect =
                go.GetComponent<RectTransform>();

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            go.GetComponent<Image>().color =
                new Color(.08f, .35f, .68f, 1f);

            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(action);

            TMP_Text text = CreateConfirmationText(
                go.transform,
                "Text",
                Vector2.zero,
                Vector2.one);

            text.text = label;
            text.fontSize = 20;
        }



        public void Show() { gameObject.SetActive(true); BindEconomy(); Refresh(); }
        public void Hide() {
            CancelPurchase();
             gameObject.SetActive(false); }
        public void Toggle() { if (gameObject.activeSelf) Hide(); else Show(); }
    }
}
