
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.UI;
using TMPro;
using ProjectSpark.UI;

public static class ProjectSparkPurchaseSuccessBuilder
{
    private static readonly Color Navy =
        new Color(.055f, .13f, .21f);

    private static readonly Color Dark =
        new Color(.025f, .07f, .12f);

    private static readonly Color Cyan =
        new Color(.18f, .73f, .96f);

    private static readonly Color White =
        new Color(.94f, .98f, 1f);

    private static readonly Color Muted =
        new Color(.62f, .76f, .85f);

    private static readonly Color Green =
        new Color(.19f, .84f, .64f);

    [MenuItem(
        "Tools/Project Spark/Shop/Create Purchase Success Panel")]
    public static void Create()
    {
        ProjectSparkShopRuntime shop = null;

        if (Selection.activeGameObject != null)
        {
            shop = Selection.activeGameObject
                .GetComponentInParent<ProjectSparkShopRuntime>();
        }

        if (shop == null)
        {
            var shops =
                Object.FindObjectsByType<ProjectSparkShopRuntime>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            if (shops.Length == 1)
                shop = shops[0];
        }

        if (shop == null)
        {
            Debug.LogError(
                "[Shop] Select the Shop Canvas containing " +
                "ProjectSparkShopRuntime.");
            return;
        }

        Transform window = shop.transform.Find(
            "Backdrop/ShopWindow");

        if (window == null)
        {
            Debug.LogError(
                "[Shop] Backdrop/ShopWindow not found.", shop);
            return;
        }

        if (window.Find("PurchaseSuccessPanel") != null)
        {
            Debug.LogWarning(
                "[Shop] PurchaseSuccessPanel already exists.",
                shop);
            return;
        }

        // ================================================
        // FULL WINDOW MODAL
        // ================================================

        GameObject root = UI(
            "PurchaseSuccessPanel", window);

        Undo.RegisterCreatedObjectUndo(
            root, "Create Purchase Success Panel");

        Stretch(root.GetComponent<RectTransform>());

        Image blocker = root.AddComponent<Image>();
        blocker.color = new Color(0, 0, 0, .80f);
        blocker.raycastTarget = true;

        // ================================================
        // SUCCESS CARD
        // ================================================

        GameObject card = UI(
            "SuccessCard", root.transform);

        RectTransform cardRect =
            card.GetComponent<RectTransform>();

        cardRect.anchorMin =
            cardRect.anchorMax = new Vector2(.5f, .5f);

        cardRect.pivot = new Vector2(.5f, .5f);
        cardRect.sizeDelta = new Vector2(620, 600);
        cardRect.anchoredPosition = Vector2.zero;

        card.AddComponent<Image>().color = Navy;

        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = Cyan;
        outline.effectDistance = new Vector2(2, -2);

        Shadow shadow = card.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, .7f);
        shadow.effectDistance = new Vector2(0, -8);

        // ================================================
        // SUCCESS ICON
        // ================================================

        Text(
            "SuccessIcon",
            card.transform,
            "✓",
            74,
            Green,
            new Vector2(225, -24),
            new Vector2(170, 85),
            TextAlignmentOptions.Center);

        Text(
            "SuccessTitle",
            card.transform,
            "PURCHASE SUCCESSFUL",
            30,
            White,
            new Vector2(20, -119),
            new Vector2(580, 48),
            TextAlignmentOptions.Center);

        Text(
            "Subtitle",
            card.transform,
            "COMPONENT ADDED TO INVENTORY",
            16,
            Cyan,
            new Vector2(20, -166),
            new Vector2(580, 32),
            TextAlignmentOptions.Center);

        // ================================================
        // PRODUCT PREVIEW
        // ================================================

        GameObject preview = UI(
            "ProductPreview", card.transform);

        Place(
            preview.GetComponent<RectTransform>(),
            237, -212, 146, 120);

        preview.AddComponent<Image>().color = Dark;

        GameObject iconObject = UI(
            "ProductIcon", preview.transform);

        RectTransform iconRect =
            iconObject.GetComponent<RectTransform>();

        iconRect.anchorMin = new Vector2(.08f, .08f);
        iconRect.anchorMax = new Vector2(.92f, .92f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        Image productIcon = iconObject.AddComponent<Image>();
        productIcon.preserveAspect = true;
        productIcon.raycastTarget = false;

        // ================================================
        // PRODUCT INFORMATION
        // ================================================

        TMP_Text productName = Text(
            "ProductName",
            card.transform,
            "Resistor 220Ω",
            26,
            White,
            new Vector2(24, -342),
            new Vector2(572, 43),
            TextAlignmentOptions.Center);

        TMP_Text quantity = Text(
            "QuantityText",
            card.transform,
            "Quantity purchased: 5",
            19,
            Muted,
            new Vector2(24, -384),
            new Vector2(572, 34),
            TextAlignmentOptions.Center);

        // ================================================
        // REMAINING BALANCE
        // ================================================

        GameObject balancePanel = UI(
            "BalancePanel", card.transform);

        Place(
            balancePanel.GetComponent<RectTransform>(),
            30, -432, 560, 65);

        balancePanel.AddComponent<Image>().color = Dark;

        TMP_Text balance = Text(
            "RemainingBalance",
            balancePanel.transform,
            "Remaining: 200 Coins",
            22,
            new Color(1f, .83f, .36f),
            Vector2.zero,
            new Vector2(560, 65),
            TextAlignmentOptions.Center);

        // ================================================
        // ACTION BUTTONS
        // ================================================

        Button continueButton = CreateButton(
            card.transform,
            "ContinueButton",
            "CONTINUE SHOPPING",
            new Vector2(30, -519),
            new Vector2(265, 56),
            new Color(.15f, .25f, .36f));

        Button inventoryButton = CreateButton(
            card.transform,
            "InventoryButton",
            "VIEW INVENTORY",
            new Vector2(325, -519),
            new Vector2(265, 56),
            new Color(.035f, .48f, .74f));

        // ================================================
        // CONNECT BUTTON EVENTS
        // ================================================

        UnityEventTools.AddPersistentListener(
            continueButton.onClick,
            shop.ClosePurchaseSuccess);

        UnityEventTools.AddPersistentListener(
            inventoryButton.onClick,
            shop.SuccessOpenInventory);

        EditorUtility.SetDirty(continueButton);
        EditorUtility.SetDirty(inventoryButton);

        // ================================================
        // CONNECT SHOP RUNTIME FIELDS
        // ================================================

        SerializedObject serialized =
            new SerializedObject(shop);

        Assign(serialized, "purchaseSuccessPanel", root);
        Assign(serialized, "successProductName", productName);
        Assign(serialized, "successQuantity", quantity);
        Assign(serialized, "successBalance", balance);
        Assign(serialized, "successProductIcon", productIcon);

        serialized.ApplyModifiedProperties();

        PrefabUtility.RecordPrefabInstancePropertyModifications(
            shop);

        // ================================================
        // FINALIZE
        // ================================================

        root.transform.SetAsLastSibling();
        root.SetActive(false);

        EditorUtility.SetDirty(shop);

        Selection.activeGameObject = root;

        Debug.Log(
            "[Project Spark] Purchase Success Panel created!",
            root);
    }

    // ====================================================
    // HELPERS
    // ====================================================

    private static GameObject UI(
        string name,
        Transform parent)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform));

        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void Place(
        RectTransform rt,
        float x, float y,
        float width, float height)
    {
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(width, height);
    }

    private static TMP_Text Text(
        string name,
        Transform parent,
        string value,
        float size,
        Color color,
        Vector2 position,
        Vector2 dimensions,
        TextAlignmentOptions alignment)
    {
        GameObject go = UI(name, parent);

        Place(
            go.GetComponent<RectTransform>(),
            position.x,
            position.y,
            dimensions.x,
            dimensions.y);

        TextMeshProUGUI text =
            go.AddComponent<TextMeshProUGUI>();

        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.enableAutoSizing = true;
        text.fontSizeMin = 12;
        text.fontSizeMax = size;

        return text;
    }

    private static Button CreateButton(
        Transform parent,
        string name,
        string label,
        Vector2 position,
        Vector2 dimensions,
        Color color)
    {
        GameObject go = UI(name, parent);

        Place(
            go.GetComponent<RectTransform>(),
            position.x,
            position.y,
            dimensions.x,
            dimensions.y);

        Image image = go.AddComponent<Image>();
        image.color = color;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;

        Text(
            "Text",
            go.transform,
            label,
            19,
            Color.white,
            Vector2.zero,
            dimensions,
            TextAlignmentOptions.Center);

        return button;
    }

    private static void Assign(
        SerializedObject obj,
        string propertyName,
        Object value)
    {
        SerializedProperty property =
            obj.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogWarning(
                "[Shop Builder] Missing runtime field: " +
                propertyName);
            return;
        }

        property.objectReferenceValue = value;
    }
}
#endif
