#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSpark.UI;

public static class ProjectSparkThreeModeToolsBuilder
{
    private static readonly Color Navy = new Color(.025f, .075f, .13f, .98f);
    private static readonly Color Cyan = new Color(.13f, .78f, .99f);
    private static readonly Color Tile = new Color(.055f, .19f, .31f, 1f);
    private static readonly Color White = new Color(.92f, .98f, 1f);
    private static readonly string[] Names = {
        "Select", "Inspect", "Move", "Rotate", "Wire", "Measure", "Scan", "Screen Control", "Reset Scene"
    };
    private static readonly string[] Glyphs = { "↖", "⌕", "✥", "↻", "⌁", "⌗", "◎", "▣", "⟳" };
    private static readonly string[] Shortcuts = { "1", "2", "3", "4", "5", "6", "7", "8", "R" };

    [MenuItem("Tools/Project Spark/Tools/Create Three-Mode Tools Panel")]
    public static void Create()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[Three-Mode Tools] Run builder in Edit Mode.");
            return;
        }
        Transform parent = FindCanvasParent();
        if (parent == null)
        {
            Debug.LogError("[Three-Mode Tools] Select a Canvas or Main_HUD inside a Canvas.");
            return;
        }
        if (parent.Find("SparkThreeModeTools") != null)
        {
            Debug.LogWarning("[Three-Mode Tools] Already exists. Select/remove the old panel before creating another.");
            Selection.activeGameObject = parent.Find("SparkThreeModeTools").gameObject;
            return;
        }

        GameObject root = UI("SparkThreeModeTools", parent);
        Undo.RegisterCreatedObjectUndo(root, "Create Project Spark Three-Mode Tools");
        RectTransform r = root.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(1f, .5f);
        r.pivot = new Vector2(1f, .5f);
        r.sizeDelta = new Vector2(94, 680);
        r.anchoredPosition = new Vector2(94, 0); // hidden; tab protrudes left
        Image bg = root.AddComponent<Image>();
        bg.color = Navy;
        AddOutline(root, Cyan, 2);

        GameObject header = UI("ExpandedHeader", root.transform);
        StretchX(header.GetComponent<RectTransform>(), 6, 6, -12, 74);
        Text("Brand", header.transform, "PROJECT SPARK", 22, White, 0, 0, 0, 36, TextAlignmentOptions.Center, true);
        Text("Caption", header.transform, "TOOLS  /  QUICK ACCESS", 12, Cyan, 0, -36, 0, 28, TextAlignmentOptions.Center, true);

        Button[] buttons = new Button[9];
        TMP_Text[] labels = new TMP_Text[9];
        TMP_Text[] keys = new TMP_Text[9];
        Image[] backgrounds = new Image[9];

        for (int i = 0; i < Names.Length; i++)
        {
            // Every row stretches to width of root; only the icon stays visible in compact mode.
            GameObject go = UI(Names[i].Replace(" ", "") + "Button", root.transform);
            RectTransform row = go.GetComponent<RectTransform>();
            StretchX(row, 9, 9, -85 - i * 58, 50);
            Image tile = go.AddComponent<Image>();
            tile.color = Tile;
            AddOutline(go, new Color(.07f, .43f, .60f), 1);
            Button button = go.AddComponent<Button>();
            button.targetGraphic = tile;
            buttons[i] = button;
            backgrounds[i] = tile;

            GameObject iconBg = UI("IconFrame", go.transform);
            RectTransform iconRect = iconBg.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, .5f);
            iconRect.anchorMax = new Vector2(0, .5f);
            iconRect.pivot = new Vector2(0, .5f);
            iconRect.sizeDelta = new Vector2(52, 44);
            iconRect.anchoredPosition = new Vector2(4, 0);
            Image iconPlate = iconBg.AddComponent<Image>();
            iconPlate.color = new Color(.015f, .11f, .20f, 1f);
            iconPlate.raycastTarget = false;
            Text("Glyph", iconBg.transform, Glyphs[i], 30, Cyan, 0, 0, 0, 0,
                TextAlignmentOptions.Center, true, true);

            labels[i] = Text("Label", go.transform, Names[i], 20, White,
                68, 0, 0, 0, TextAlignmentOptions.MidlineLeft, false, true, 54);
            keys[i] = Text("Shortcut", go.transform, Shortcuts[i], 14, Cyan,
                0, 0, 32, 0, TextAlignmentOptions.Center, false, true, 0, 8);
        }

        GameObject footer = UI("ExpandedFooter", root.transform);
        StretchX(footer.GetComponent<RectTransform>(), 12, 12, -618, 40);
        TMP_Text footerText = Text("CurrentTool", footer.transform, "ACTIVE: SELECT", 14, Cyan,
            0, 0, 0, 0, TextAlignmentOptions.Center, true, true);

        // Toggle is intentionally OUTSIDE the drawer's left edge.
        GameObject toggle = UI("ModeToggle", root.transform);
        RectTransform toggleRect = toggle.GetComponent<RectTransform>();
        toggleRect.anchorMin = toggleRect.anchorMax = new Vector2(0f, .5f);
        toggleRect.pivot = new Vector2(1f, .5f);
        toggleRect.sizeDelta = new Vector2(48, 84);
        toggleRect.anchoredPosition = Vector2.zero;
        Image tabImage = toggle.AddComponent<Image>();
        tabImage.color = new Color(.025f, .40f, .67f, 1f);
        AddOutline(toggle, Cyan, 2);
        Button tabButton = toggle.AddComponent<Button>();
        tabButton.targetGraphic = tabImage;
        TMP_Text arrow = Text("Arrow", toggle.transform, "<", 36, White, 0, 0, 0, 0,
            TextAlignmentOptions.Center, true, true);

        var controller = Undo.AddComponent<ProjectSparkThreeModeToolsPanel>(root);
        SerializedObject serialized = new SerializedObject(controller);
        Set(serialized, "drawer", r);
        Set(serialized, "modeButton", tabButton);
        Set(serialized, "modeArrow", arrow);
        Set(serialized, "expandedHeader", header);
        Set(serialized, "expandedFooter", footer);
        Set(serialized, "selectedToolText", footerText);
        SetArray(serialized, "toolButtons", buttons);
        SetArray(serialized, "toolNames", labels);
        SetArray(serialized, "shortcutLabels", keys);
        SetArray(serialized, "toolBackgrounds", backgrounds);
        serialized.FindProperty("startMode").enumValueIndex = 0;
        serialized.FindProperty("iconsWidth").floatValue = 94f;
        serialized.FindProperty("completeWidth").floatValue = 324f;
        serialized.FindProperty("animationSeconds").floatValue = .32f;
        serialized.FindProperty("rightInset").floatValue = 14f;
        serialized.ApplyModifiedProperties();

        // Display hidden state already in the Scene view.
        header.SetActive(false);
        footer.SetActive(false);
        foreach (var t in labels) t.gameObject.SetActive(false);
        foreach (var t in keys) t.gameObject.SetActive(false);
        Selection.activeGameObject = root;
        EditorUtility.SetDirty(controller);
        Debug.Log("[Project Spark] Three-mode tools created. Wire actions on SparkThreeModeTools component.", root);
    }

    private static Transform FindCanvasParent()
    {
        Transform selected = Selection.activeTransform;
        if (selected != null)
        {
            Canvas canvas = selected.GetComponentInParent<Canvas>();
            if (canvas != null) return selected is RectTransform ? selected : canvas.transform;
        }
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var canvas in canvases)
            if (canvas.name == "Main_HUD") return canvas.transform;
        foreach (var canvas in canvases)
            if (canvas.isRootCanvas) return canvas.transform;
        return null;
    }

    private static GameObject UI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    // Left and right insets from parent, top offset and height.
    private static void StretchX(RectTransform r, float left, float right, float top, float height)
    {
        r.anchorMin = new Vector2(0, 1);
        r.anchorMax = new Vector2(1, 1);
        r.pivot = new Vector2(.5f, 1);
        r.offsetMin = new Vector2(left, top - height);
        r.offsetMax = new Vector2(-right, top);
    }

    private static TMP_Text Text(string name, Transform parent, string value, float size, Color color,
        float left, float top, float width, float height, TextAlignmentOptions alignment,
        bool stretchHorizontal, bool stretchVertical = false, float right = 0, float topOrBottom = 0)
    {
        var go = UI(name, parent);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0, stretchVertical ? 0 : 1);
        r.anchorMax = new Vector2(stretchHorizontal ? 1 : (right > 0 ? 1 : 0), 1);
        r.pivot = new Vector2(0, 1);
        r.offsetMin = new Vector2(left, stretchVertical ? 0 : top - height);
        r.offsetMax = new Vector2(-right, stretchVertical ? 0 : top);
        if (!stretchHorizontal && right <= 0) { r.sizeDelta = new Vector2(width, height); r.anchoredPosition = new Vector2(left, top); }
        if (!stretchVertical && height <= 0) { r.anchorMin = new Vector2(r.anchorMin.x, 0); r.anchorMax = new Vector2(r.anchorMax.x, 1); r.offsetMin = new Vector2(left, 0); r.offsetMax = new Vector2(-right, 0); }
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = value;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 11;
        tmp.fontSizeMax = size;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static void AddOutline(GameObject go, Color color, int amount)
    {
        var outline = go.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(amount, -amount);
    }

    private static void Set(SerializedObject so, string property, Object reference)
    {
        var p = so.FindProperty(property);
        if (p != null) p.objectReferenceValue = reference;
    }
    private static void SetArray<T>(SerializedObject so, string property, T[] values) where T : Object
    {
        var p = so.FindProperty(property);
        if (p == null) return;
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }
}
#endif
