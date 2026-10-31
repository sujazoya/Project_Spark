
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSpark.UI;

public static class ProjectSparkConfirmationPanelBuilder
{
    private static readonly Color Backdrop =
        new Color(0f, 0f, 0f, 0.78f);

    private static readonly Color Panel =
        new Color(0.063f, 0.141f, 0.227f);

    private static readonly Color Cyan =
        new Color(0.22f, 0.76f, 1f);

    private static readonly Color White =
        new Color(0.94f, 0.98f, 1f);

    private static readonly Color Muted =
        new Color(0.62f, 0.76f, 0.85f);

    [MenuItem(
        "Tools/Project Spark/Shop/Create Confirmation Panel")]
    public static void Create()
    {
        ProjectSparkShopRuntime shop =
            Selection.activeGameObject != null
            ? Selection.activeGameObject
                .GetComponentInParent<ProjectSparkShopRuntime>()
            : null;

        if (shop == null)
        {
            ProjectSparkShopRuntime[] shops =
                Object.FindObjectsByType<ProjectSparkShopRuntime>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            if (shops.Length == 1)
                shop = shops[0];
        }

        if (shop == null)
        {
            Debug.LogError(
                "[Shop Builder] Select the Shop Panel " +
                "GameObject before creating the modal.");
            return;
        }

        Transform window =
            shop.transform.Find("Backdrop/ShopWindow");

        if (window == null)
        {
            Debug.LogError(
                "[Shop Builder] ShopWindow not found.",
                shop);
            return;
        }

        Transform existing =
            window.Find("PurchaseConfirmation_Runtime");

        if (existing != null)
        {
            Selection.activeGameObject =
                existing.gameObject;

            Debug.LogWarning(
                "[Shop Builder] Confirmation panel already exists. " +
                "No changes were made.",
                existing);
            return;
        }

        // Full-window modal blocker.
        GameObject overlay = CreateUI(
            "PurchaseConfirmation_Runtime",
            window);

        RectTransform overlayRect =
            overlay.GetComponent<RectTransform>();

        Stretch(overlayRect);

        Image overlayImage =
            overlay.AddComponent<Image>();

        overlayImage.color = Backdrop;
        overlayImage.raycastTarget = true;

        // Centered confirmation card.
        GameObject card = CreateUI(
            "ConfirmationCard",
            overlay.transform);

        RectTransform cardRect =
            card.GetComponent<RectTransform>();

        cardRect.anchorMin =
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);

        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(620, 570);
        cardRect.anchoredPosition = Vector2.zero;

        Image cardImage = card.AddComponent<Image>();
        cardImage.color = Panel;

        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = Cyan;
        outline.effectDistance = new Vector2(2, -2);

        Shadow shadow = card.AddComponent<Shadow>();
        shadow.effectColor =
            new Color(0f, 0f, 0f, 0.75f);
        shadow.effectDistance = new Vector2(0, -8);

        // Header.
        Text(
            "HeaderTitle",
            card.transform,
            "CONFIRM PURCHASE",
            29,
            White,
            new Vector2(24, -16),
            new Vector2(570, 48),
            TextAlignmentOptions.Left);

        Text(
            "HeaderSubtitle",
            card.transform,
            "PROJECT SPARK COMPONENT STORE",
            14,
            Cyan,
            new Vector2(24, -59),
            new Vector2(550, 26),
            TextAlignmentOptions.Left);

        // Product preview.
        GameObject preview = CreateUI(
            "ProductPreview",
            card.transform);

        RectTransform previewRect =
            preview.GetComponent<RectTransform>();

        TopLeft(
            previewRect,
            new Vector2(30, -112),
            new Vector2(132, 132));

        preview.AddComponent<Image>().color =
            new Color(0.09f, 0.20f, 0.31f);

        GameObject productIcon = CreateUI(
            "ProductIcon",
            preview.transform);

        RectTransform iconRect =
            productIcon.GetComponent<RectTransform>();

        iconRect.anchorMin = new Vector2(.1f, .1f);
        iconRect.anchorMax = new Vector2(.9f, .9f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        Image icon = productIcon.AddComponent<Image>();
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        Text(
            "ProductName",
            card.transform,
            "RESISTOR 220Ω",
            26,
            White,
            new Vector2(185, -115),
            new Vector2(400, 42),
            TextAlignmentOptions.Left);

        Text(
            "ProductCategory",
            card.transform,
            "RESISTORS",
            16,
            Cyan,
            new Vector2(185, -166),
            new Vector2(370, 30),
            TextAlignmentOptions.Left);

        Text(
            "QuantityText",
            card.transform,
            "Quantity: 5",
            19,
            Muted,
            new Vector2(185, -205),
            new Vector2(350, 34),
            TextAlignmentOptions.Left);

        // Price breakdown.
        GameObject pricePanel = CreateUI(
            "PriceBreakdown",
            card.transform);

        TopLeft(
            pricePanel.GetComponent<RectTransform>(),
            new Vector2(25, -270),
            new Vector2(570, 145));

        pricePanel.AddComponent<Image>().color =
            new Color(.025f, .080f, .14f);

        Text(
            "UnitPrice",
            pricePanel.transform,
            "Unit price: 10 coins",
            18,
            Muted,
            new Vector2(18, -13),
            new Vector2(530, 30),
            TextAlignmentOptions.Left);

        Text(
            "PriceQuantity",
            pricePanel.transform,
            "Quantity: 5",
            18,
            Muted,
            new Vector2(18, -51),
            new Vector2(530, 30),
            TextAlignmentOptions.Left);

        Text(
            "TotalPrice",
            pricePanel.transform,
            "TOTAL: 50 COINS",
            25,
            new Color(1f, .83f, .35f),
            new Vector2(18, -94),
            new Vector2(530, 38),
            TextAlignmentOptions.Left);

        Text(
            "InfoText",
            card.transform,
            "Purchased components are added to inventory.",
            16,
            Muted,
            new Vector2(30, -426),
            new Vector2(555, 36),
            TextAlignmentOptions.Center);

        // Actions.
        CreateButton(
            card.transform,
            "CancelButton",
            "CANCEL",
            new Vector2(30, -492),
            new Vector2(265, 55),
            new Color(.15f, .24f, .34f));

        CreateButton(
            card.transform,
            "ConfirmButton",
            "CONFIRM PURCHASE",
            new Vector2(325, -492),
            new Vector2(265, 55),
            new Color(.04f, .47f, .73f));

        // Runtime code activates this when required.
        overlay.SetActive(false);
        overlay.transform.SetAsLastSibling();

        Undo.RegisterCreatedObjectUndo(
            overlay,
            "Create Purchase Confirmation Panel");

        EditorUtility.SetDirty(shop);
        Selection.activeGameObject = overlay;

        Debug.Log(
            "[Shop Builder] Purchase confirmation created.",
            overlay);
    }

    private static GameObject CreateUI(
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

    private static void TopLeft(
        RectTransform rt,
        Vector2 position,
        Vector2 size)
    {
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    private static TextMeshProUGUI Text(
        string name,
        Transform parent,
        string value,
        float fontSize,
        Color color,
        Vector2 position,
        Vector2 size,
        TextAlignmentOptions alignment)
    {
        GameObject go = CreateUI(name, parent);

        TopLeft(
            go.GetComponent<RectTransform>(),
            position,
            size);

        TextMeshProUGUI text =
            go.AddComponent<TextMeshProUGUI>();

        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;

        return text;
    }

    private static Button CreateButton(
        Transform parent,
        string name,
        string label,
        Vector2 position,
        Vector2 size,
        Color color)
    {
        GameObject go = CreateUI(name, parent);

        TopLeft(
            go.GetComponent<RectTransform>(),
            position,
            size);

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
            size,
            TextAlignmentOptions.Center);

        return button;
    }
}
#endif
