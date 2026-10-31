
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ProjectSpark.UI;

public static class ProjectSparkShopHUDDrawerBuilder
{
    [MenuItem(
        "Tools/Project Spark/Shop/Add Animated HUD Drawer")]
    public static void Create()
    {
        // ============================================
        // FIND SHOP QUICK ACCESS GAMEOBJECT
        // ============================================

        GameObject hudObject = null;

        // First use selected GameObject.
        if (Selection.activeGameObject != null)
        {
            Transform selected =
                Selection.activeGameObject.transform;

            while (selected != null)
            {
                if (selected.name == "ShopQuickAccess")
                {
                    hudObject = selected.gameObject;
                    break;
                }

                selected = selected.parent;
            }
        }

        // Otherwise search all scene objects.
        if (hudObject == null)
        {
            GameObject[] objects =
                Object.FindObjectsByType<GameObject>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            foreach (GameObject obj in objects)
            {
                if (obj.name == "ShopQuickAccess")
                {
                    hudObject = obj;
                    break;
                }
            }
        }

        if (hudObject == null)
        {
            Debug.LogError(
                "[Shop Drawer] ShopQuickAccess not found.");
            return;
        }

        RectTransform drawer =
            hudObject.GetComponent<RectTransform>();

        if (drawer == null)
        {
            Debug.LogError(
                "[Shop Drawer] ShopQuickAccess needs " +
                "a RectTransform.",
                hudObject);
            return;
        }

        // ============================================
        // VALIDATE EXISTING TOGGLE
        // ============================================

        if (drawer.Find("DrawerToggle") != null)
        {
            Debug.LogWarning(
                "[Shop Drawer] DrawerToggle already exists.",
                hudObject);
            return;
        }

        // ============================================
        // ENSURE TOP RIGHT ANCHOR
        // ============================================

        Undo.RecordObject(
            drawer,
            "Configure Shop Drawer Position");

        drawer.anchorMin = new Vector2(1f, 1f);
        drawer.anchorMax = new Vector2(1f, 1f);
        drawer.pivot = new Vector2(1f, 1f);

        // Preserve existing size.
        if (drawer.sizeDelta.x <= 0f)
            drawer.sizeDelta =
                new Vector2(440f, 140f);

        // ============================================
        // CREATE LEFT EDGE TAB
        // ============================================

        GameObject tab = new GameObject(
            "DrawerToggle",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));

        tab.transform.SetParent(drawer, false);

        Undo.RegisterCreatedObjectUndo(
            tab,
            "Create Shop Drawer Toggle");

        RectTransform tabRect =
            tab.GetComponent<RectTransform>();

        tabRect.anchorMin = new Vector2(0f, 1f);
        tabRect.anchorMax = new Vector2(0f, 1f);
        tabRect.pivot = new Vector2(1f, 1f);

        tabRect.sizeDelta =
            new Vector2(48f, 64f);

        tabRect.anchoredPosition =
            new Vector2(0f, -37f);

        Image image =
            tab.GetComponent<Image>();

        image.color =
            new Color(.055f, .42f, .68f, 1f);

        image.raycastTarget = true;

        Button button =
            tab.GetComponent<Button>();

        button.targetGraphic = image;

        ColorBlock colors = button.colors;

        colors.normalColor = Color.white;

        colors.highlightedColor =
            new Color(.70f, .94f, 1f);

        colors.pressedColor =
            new Color(.37f, .72f, .95f);

        colors.selectedColor = Color.white;

        button.colors = colors;

        // ============================================
        // CREATE ARROW
        // ============================================

        GameObject labelObject =
            new GameObject(
                "Arrow",
                typeof(RectTransform),
                typeof(TextMeshProUGUI));

        labelObject.transform.SetParent(
            tab.transform,
            false);

        RectTransform labelRect =
            labelObject.GetComponent<RectTransform>();

        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;

        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label =
            labelObject.GetComponent<TextMeshProUGUI>();

        label.text = "<";
        label.fontSize = 38f;
        label.color = Color.white;

        label.alignment =
            TextAlignmentOptions.Center;

        label.raycastTarget = false;

        // ============================================
        // ATTACH OR REUSE DRAWER CONTROLLER
        // ============================================

        ProjectSparkShopHUDDrawer controller =
            hudObject.GetComponent
                <ProjectSparkShopHUDDrawer>();

        if (controller == null)
        {
            controller =
                Undo.AddComponent
                    <ProjectSparkShopHUDDrawer>(
                        hudObject);
        }

        // ============================================
        // AUTO ASSIGN REFERENCES
        // ============================================

        SerializedObject serialized =
            new SerializedObject(controller);

        serialized.FindProperty("drawer")
            .objectReferenceValue = drawer;

        serialized.FindProperty("toggleButton")
            .objectReferenceValue = button;

        serialized.FindProperty("toggleLabel")
            .objectReferenceValue = label;

        serialized.FindProperty("startOpen")
            .boolValue = false;

        serialized.FindProperty("duration")
            .floatValue = 0.35f;

        serialized.FindProperty("openRightOffset")
            .floatValue = 20f;

        serialized.ApplyModifiedProperties();

        // ============================================
        // MOVE DRAWER OUTSIDE SCREEN
        // ============================================

        // Pivot is on the right edge.
        // Closed position places the panel
        // immediately outside the screen,
        // leaving the left tab visible.
        Vector2 position =
            drawer.anchoredPosition;

        position.x = drawer.rect.width;
        drawer.anchoredPosition = position;

        // ============================================
        // FINALIZE
        // ============================================

        EditorUtility.SetDirty(drawer);
        EditorUtility.SetDirty(controller);

        PrefabUtility
            .RecordPrefabInstancePropertyModifications(
                controller);

        Selection.activeGameObject = hudObject;

        Debug.Log(
            "[Project Spark] Animated Shop HUD Drawer created!",
            hudObject);
    }
}

#endif
