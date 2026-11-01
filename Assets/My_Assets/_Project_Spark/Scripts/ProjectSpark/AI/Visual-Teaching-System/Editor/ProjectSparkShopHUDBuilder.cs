
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using ProjectSpark.UI;
using ProjectSpark.Gameplay;

public static class ProjectSparkShopHUDBuilder
{
    private static readonly Color Navy =
        new Color(.035f, .09f, .16f, .97f);

    private static readonly Color Panel =
        new Color(.055f, .15f, .24f, 1f);

    private static readonly Color Blue =
        new Color(.065f, .37f, .60f);

    private static readonly Color Cyan =
        new Color(.22f, .76f, .98f);

    private static readonly Color White =
        new Color(.94f, .98f, 1f);

    private static readonly Color Muted =
        new Color(.62f, .76f, .85f);

    private static readonly Color Gold =
        new Color(1f, .82f, .28f);

    [MenuItem(
        "Tools/Project Spark/Shop/Create HUD Shortcuts")]
    public static void Create()
    {
        GameObject mainHUD = FindMainHUD();

        if (mainHUD == null)
        {
            Debug.LogError(
                "[Shop HUD] Main_HUD not found.");
            return;
        }

        var shop =
            Object.FindFirstObjectByType
                <ProjectSparkShopRuntime>(
                    FindObjectsInactive.Include);

        var inventory =
            Object.FindFirstObjectByType
                <ProjectSparkInventoryPanel>(
                    FindObjectsInactive.Include);

        var economy =
            Object.FindFirstObjectByType
                <SparkEconomyService>(
                    FindObjectsInactive.Include);

        // =========================================
        // CREATE OR REUSE ROOT
        // =========================================

        Transform existing =
            mainHUD.transform.Find("ShopQuickAccess");

        GameObject root;

        if (existing != null)
        {
            root = existing.gameObject;

            // Preserve DrawerToggle and any
            // existing runtime components.
            for (int i = root.transform.childCount - 1;
                 i >= 0; i--)
            {
                Transform child =
                    root.transform.GetChild(i);

                if (child.name == "DrawerToggle")
                    continue;

                Undo.DestroyObjectImmediate(
                    child.gameObject);
            }
        }
        else
        {
            root = CreateUI(
                "ShopQuickAccess",
                mainHUD.transform);

            Undo.RegisterCreatedObjectUndo(
                root,
                "Create Vertical Shop HUD");
        }

        RectTransform rootRect =
            root.GetComponent<RectTransform>();

        if (rootRect == null)
        {
            Debug.LogError(
                "[Shop HUD] Root needs RectTransform.");
            return;
        }

        Undo.RecordObject(
            rootRect,
            "Configure Vertical Shop HUD");

        // =========================================
        // VERTICAL RIGHT-SIDE LAYOUT
        // =========================================

        rootRect.anchorMin =
            rootRect.anchorMax =
                new Vector2(1f, .5f);

        rootRect.pivot =
            new Vector2(1f, .5f);

        rootRect.sizeDelta =
            new Vector2(265f, 360f);

        // Open position.
        rootRect.anchoredPosition =
            new Vector2(-20f, 0f);

        Image background =
            root.GetComponent<Image>();

        if (background == null)
            background = Undo.AddComponent<Image>(
                root);

        background.color = Navy;

        Outline outline =
            root.GetComponent<Outline>();

        if (outline == null)
            outline = Undo.AddComponent<Outline>(
                root);

        outline.effectColor = Cyan;
        outline.effectDistance =
            new Vector2(1f, -1f);

        // =========================================
        // HEADER
        // =========================================

        TMP_Text title = CreateText(
            root.transform,
            "Title",
            "PROJECT SPARK",
            23f,
            White,
            15f, -18f, 235f, 35f);

        title.alignment =
            TextAlignmentOptions.Center;

        TMP_Text subtitle = CreateText(
            root.transform,
            "Subtitle",
            "QUICK ACCESS",
            13f,
            Cyan,
            15f, -55f, 235f, 25f);

        subtitle.alignment =
            TextAlignmentOptions.Center;

        CreatePanel(
            root.transform,
            "HeaderDivider",
            Cyan,
            18f, -91f, 229f, 2f);

        // =========================================
        // CURRENCY BACKGROUND
        // =========================================

        CreatePanel(
            root.transform,
            "CurrencyPanel",
            Panel,
            14f, -108f, 237f, 97f);

        // COINS
        CreateText(
            root.transform,
            "CoinIcon",
            "●",
            25f,
            Gold,
            27f, -119f, 30f, 30f);

        CreateText(
            root.transform,
            "CoinLabel",
            "COINS",
            15f,
            Muted,
            62f, -122f, 95f, 28f);

        TMP_Text coins = CreateText(
            root.transform,
            "CoinsText",
            "0",
            19f,
            Gold,
            148f, -121f, 87f, 30f);

        coins.alignment =
            TextAlignmentOptions.MidlineRight;

        // DIAMONDS
        CreateText(
            root.transform,
            "DiamondIcon",
            "◆",
            23f,
            Cyan,
            27f, -163f, 30f, 30f);

        CreateText(
            root.transform,
            "DiamondLabel",
            "DIAMONDS",
            15f,
            Muted,
            62f, -166f, 95f, 28f);

        TMP_Text diamonds = CreateText(
            root.transform,
            "DiamondsText",
            "0",
            19f,
            Cyan,
            148f, -165f, 87f, 30f);

        diamonds.alignment =
            TextAlignmentOptions.MidlineRight;

        // =========================================
        // VERTICAL BUTTONS
        // =========================================

        Button shopButton = CreateButton(
            root.transform,
            "ShopButton",
            "SHOP",
            18f, -224f, 229f, 48f);

        Button inventoryButton = CreateButton(
            root.transform,
            "InventoryButton",
            "INVENTORY",
            18f, -285f, 229f, 48f);

        // =========================================
        // CONNECT EXISTING RUNTIME COMPONENT
        // =========================================

        ProjectSparkShopHUD hud =
            root.GetComponent<ProjectSparkShopHUD>();

        if (hud == null)
        {
            hud = Undo.AddComponent
                <ProjectSparkShopHUD>(root);
        }

        SerializedObject so =
            new SerializedObject(hud);

        Assign(so, "shop", shop);
        Assign(so, "inventory", inventory);
        Assign(so, "economy", economy);

        Assign(so, "shopButton", shopButton);
        Assign(so, "inventoryButton", inventoryButton);

        Assign(so, "coinsText", coins);
        Assign(so, "diamondsText", diamonds);

        so.ApplyModifiedProperties();

        // =========================================
        // UPDATE EXISTING SLIDING DRAWER
        // =========================================

        ProjectSparkShopHUDDrawer drawer =
            root.GetComponent
                <ProjectSparkShopHUDDrawer>();

        if (drawer != null)
        {
            SerializedObject drawerSO =
                new SerializedObject(drawer);

            Assign(
                drawerSO,
                "drawer",
                rootRect);

            drawerSO.ApplyModifiedProperties();

            // Closed position for the existing
            // drawer animation script.
            Vector2 closed =
                rootRect.anchoredPosition;

            closed.x = rootRect.rect.width;

            rootRect.anchoredPosition = closed;
        }

        // =========================================
        // FINALIZE
        // =========================================

        EditorUtility.SetDirty(rootRect);
        EditorUtility.SetDirty(hud);
        EditorUtility.SetDirty(root);

        PrefabUtility
            .RecordPrefabInstancePropertyModifications(
                rootRect);

        Selection.activeGameObject = root;

        Debug.Log(
            "[Project Spark] Vertical Shop HUD ready!",
            root);

        if (shop == null ||
            inventory == null ||
            economy == null)
        {
            Debug.LogWarning(
                "[Shop HUD] Some runtime references " +
                "are missing. Assign them in Inspector.",
                root);
        }
    }

    // =========================================
    // FIND MAIN HUD
    // =========================================

    private static GameObject FindMainHUD()
    {
        Transform selected =
            Selection.activeTransform;

        while (selected != null)
        {
            if (selected.name == "Main_HUD")
                return selected.gameObject;

            selected = selected.parent;
        }

        Scene scene =
            SceneManager.GetActiveScene();

        foreach (GameObject sceneRoot
                 in scene.GetRootGameObjects())
        {
            Transform[] transforms =
                sceneRoot.GetComponentsInChildren
                    <Transform>(true);

            foreach (Transform t in transforms)
            {
                if (t.name == "Main_HUD")
                    return t.gameObject;
            }
        }

        return null;
    }

    // =========================================
    // HELPERS
    // =========================================

    private static GameObject CreateUI(
        string name,
        Transform parent)
    {
        GameObject go =
            new GameObject(
                name,
                typeof(RectTransform));

        go.transform.SetParent(parent, false);

        return go;
    }

    private static void Place(
        RectTransform rect,
        float x, float y,
        float width, float height)
    {
        rect.anchorMin =
            new Vector2(0f, 1f);

        rect.anchorMax =
            new Vector2(0f, 1f);

        rect.pivot =
            new Vector2(0f, 1f);

        rect.anchoredPosition =
            new Vector2(x, y);

        rect.sizeDelta =
            new Vector2(width, height);
    }

    private static GameObject CreatePanel(
        Transform parent,
        string name,
        Color color,
        float x, float y,
        float width, float height)
    {
        GameObject go =
            CreateUI(name, parent);

        Place(
            go.GetComponent<RectTransform>(),
            x, y, width, height);

        Image image =
            go.AddComponent<Image>();

        image.color = color;
        image.raycastTarget = false;

        return go;
    }

    private static TMP_Text CreateText(
        Transform parent,
        string name,
        string value,
        float fontSize,
        Color color,
        float x, float y,
        float width, float height)
    {
        GameObject go =
            CreateUI(name, parent);

        Place(
            go.GetComponent<RectTransform>(),
            x, y, width, height);

        TextMeshProUGUI text =
            go.AddComponent<TextMeshProUGUI>();

        text.text = value;
        text.fontSize = fontSize;
        text.color = color;

        text.alignment =
            TextAlignmentOptions.MidlineLeft;

        text.enableAutoSizing = true;
        text.fontSizeMin = 10f;
        text.fontSizeMax = fontSize;
        text.raycastTarget = false;

        return text;
    }

    private static Button CreateButton(
        Transform parent,
        string name,
        string label,
        float x, float y,
        float width, float height)
    {
        GameObject go =
            CreateUI(name, parent);

        Place(
            go.GetComponent<RectTransform>(),
            x, y, width, height);

        Image image =
            go.AddComponent<Image>();

        image.color = Blue;

        Button button =
            go.AddComponent<Button>();

        button.targetGraphic = image;

        ColorBlock colors =
            button.colors;

        colors.normalColor = Color.white;

        colors.highlightedColor =
            new Color(.70f, .90f, 1f);

        colors.pressedColor =
            new Color(.40f, .75f, 1f);

        button.colors = colors;

        TMP_Text text = CreateText(
            go.transform,
            "Text",
            label,
            19f,
            White,
            0f, 0f, width, height);

        text.alignment =
            TextAlignmentOptions.Center;

        return button;
    }

    private static void Assign(
        SerializedObject so,
        string field,
        Object value)
    {
        SerializedProperty property =
            so.FindProperty(field);

        if (property != null)
            property.objectReferenceValue = value;
    }
}

#endif
