
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSpark.UI;

public static class ProjectSparkInventoryBadgeBuilder
{
    [MenuItem(
        "Tools/Project Spark/Shop/Add Inventory Badge")]
    public static void Create()
    {
        ProjectSparkShopHUD hud =
            Object.FindFirstObjectByType<ProjectSparkShopHUD>(
                FindObjectsInactive.Include);

        if (hud == null)
        {
            Debug.LogError(
                "[Inventory Badge] Shop HUD not found.");
            return;
        }

        Transform button = hud.transform.Find(
            "InventoryButton");

        if (button == null)
        {
            Debug.LogError(
                "[Inventory Badge] InventoryButton missing.",
                hud);
            return;
        }

        Transform existing = button.Find("InventoryBadge");

        if (existing != null)
        {
            Debug.LogWarning(
                "[Inventory Badge] Already created.", hud);
            return;
        }

        GameObject badge = new GameObject(
            "InventoryBadge",
            typeof(RectTransform),
            typeof(Image));

        badge.transform.SetParent(button, false);

        Undo.RegisterCreatedObjectUndo(
            badge, "Create Inventory Badge");

        RectTransform rect =
            badge.GetComponent<RectTransform>();

        rect.anchorMin =
            rect.anchorMax = new Vector2(1f, .5f);

        rect.pivot = new Vector2(1f, .5f);
        rect.sizeDelta = new Vector2(44f, 30f);
        rect.anchoredPosition =
            new Vector2(-8f, 0f);

        Image image = badge.GetComponent<Image>();
        image.color = new Color(.88f, .15f, .22f);
        image.raycastTarget = false;

        GameObject labelObject = new GameObject(
            "Count",
            typeof(RectTransform),
            typeof(TextMeshProUGUI));

        labelObject.transform.SetParent(
            badge.transform, false);

        RectTransform labelRect =
            labelObject.GetComponent<RectTransform>();

        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label =
            labelObject.GetComponent<TextMeshProUGUI>();

        label.text = "0";
        label.fontSize = 18;
        label.color = Color.white;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;

        SerializedObject so = new SerializedObject(hud);

        so.FindProperty("inventoryBadge")
            .objectReferenceValue = badge;

        so.FindProperty("inventoryBadgeText")
            .objectReferenceValue = label;

        so.ApplyModifiedProperties();

        badge.SetActive(false);

        EditorUtility.SetDirty(hud);

        Selection.activeGameObject = badge;

        Debug.Log(
            "[Project Spark] Inventory badge created.",
            badge);
    }
}

#endif
