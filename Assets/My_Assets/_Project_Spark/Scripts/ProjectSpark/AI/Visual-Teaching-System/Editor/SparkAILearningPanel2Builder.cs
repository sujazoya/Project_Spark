#if UNITY_EDITOR
using System.Collections.Generic;
using ProjectSpark.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectSpark.UI.Editor
{
    /// <summary>
    /// Non-destructive AILearningPanel 2.0 upgrade. Reuses the 34 existing
    /// action bindings, drawer buttons and canvas. Safe to run repeatedly.
    /// </summary>
    public static class SparkAILearningPanel2Builder
    {
        private static readonly Color Ink = Hex("EFFAFF");
        private static readonly Color Muted = Hex("9EBCCA");
        private static readonly Color Cyan = Hex("43D9F5");
        private static readonly Color Dark = Hex("091F30");
        private static readonly Color Tile = Hex("183B4E");
        private static readonly Color Mint = Hex("64E3B2");

        [MenuItem("Tools/Project Spark/AI/Upgrade Existing AI Learning Panel to 2.0")]
        public static void Upgrade()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[AILearningPanel 2.0] Exit Play Mode before editing UI.");
                return;
            }
            SparkAILearningPanel panel = Object.FindFirstObjectByType<SparkAILearningPanel>(
                FindObjectsInactive.Include);
            if (panel == null)
            {
                Debug.LogError("[AILearningPanel 2.0] Existing AILearningPanel not found. " +
                    "Create it with the original 3-mode builder first.");
                return;
            }
            Transform detail = panel.transform.Find("DetailedDrawer");
            Transform footer = detail != null ? detail.Find("Footer") : null;
            Transform scroll = detail != null ? detail.Find("ActionsScroll") : null;
            if (detail == null || footer == null || scroll == null)
            {
                Debug.LogError("[AILearningPanel 2.0] Existing generated " +
                    "DetailedDrawer/Footer/ActionsScroll structure is missing. No changes made.", panel);
                return;
            }

            // Preserve the prior layout in Undo, and update only two existing dimensions.
            RectTransform footerRect = footer.GetComponent<RectTransform>();
            RectTransform scrollRect = scroll.GetComponent<RectTransform>();
            if (footerRect == null || scrollRect == null) return;
            Undo.RecordObject(footerRect, "Expand AI Learning Footer");
            Undo.RecordObject(scrollRect, "Adjust AI Learning Scroll Region");
            footerRect.sizeDelta = new Vector2(footerRect.sizeDelta.x, 158f);
            Vector2 scrollMin = scrollRect.offsetMin;
            Vector2 scrollMax = scrollRect.offsetMax;
            scrollRect.offsetMin = new Vector2(scrollMin.x, 167f);
            scrollRect.offsetMax = new Vector2(scrollMax.x, -228f);

            Transform quick = detail.Find("QuickLaunch_2_0");
            if (quick == null)
            {
                GameObject go = UI("QuickLaunch_2_0", detail);
                Undo.RegisterCreatedObjectUndo(go, "Create AI Quick Launch");
                quick = go.transform;
                RectTransform rect = go.GetComponent<RectTransform>();
                TopLeft(rect, 16f, -151f, 408f, 66f);
                Image bg = go.AddComponent<Image>();
                bg.color = Dark;
                bg.raycastTarget = false;
            }
            else
            {
                Undo.RecordObject(quick.GetComponent<RectTransform>(), "Align Quick Launch");
                TopLeft(quick.GetComponent<RectTransform>(), 16f, -151f, 408f, 66f);
            }
            TextMeshProUGUI caption = GetOrMakeText(quick, "QuickCaption",
                "QUICK LAUNCH  /  YOUR MOST USED TOOLS", 10, Cyan,
                9f, -5f, 387f, 17f);
            caption.fontStyle = FontStyles.Bold;

            string[] shortcutIds = { "ask", "library", "training", "student" };
            string[] shortcutLabels = { "ASK SPARK", "LIBRARY", "TRAINING", "PROGRESS" };
            Button[] shortcutButtons = new Button[shortcutIds.Length];
            for (int i = 0; i < shortcutIds.Length; i++)
                shortcutButtons[i] = GetOrMakeButton(quick, "Quick_" + shortcutIds[i],
                    shortcutLabels[i], 9f + i * 100f, -25f, 94f, 33f);

            // In the footer, leave the original availability/refresh/feedback intact.
            TMP_Text level = GetOrMakeText(footer, "LevelStatus_2_0",
                "NO ACTIVE LEVEL", 12, Ink, 16f, -89f, 390f, 20f);
            TMP_Text objectives = GetOrMakeText(footer, "ObjectiveStatus_2_0",
                "OBJECTIVES  — / —", 10, Muted, 16f, -110f, 260f, 18f);
            TMP_Text lastTool = GetOrMakeText(footer, "LastTool_2_0",
                "LAST USED  /  NONE YET", 10, Cyan, 16f, -140f, 390f, 16f);

            Image track = GetOrMakeImage(footer, "ObjectiveTrack_2_0",
                16f, -129f, 406f, 5f, Hex("24485B"));
            track.raycastTarget = false;
            Image fill = GetOrMakeImage(footer, "ObjectiveFill_2_0",
                16f, -129f, 406f, 5f, Mint);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0;
            fill.raycastTarget = false;

            // Match the existing action bindings, without touching their listeners.
            SerializedObject panelSO = new SerializedObject(panel);
            SerializedProperty original = panelSO.FindProperty("actionBindings");
            if (original == null)
            {
                Debug.LogError("[AILearningPanel 2.0] Core actionBindings field missing.", panel);
                return;
            }
            var views = new List<SparkAILearningPanel2UX.ActionView>();
            for (int i = 0; i < original.arraySize; i++)
            {
                SerializedProperty entry = original.GetArrayElementAtIndex(i);
                var id = entry.FindPropertyRelative("id").stringValue;
                var rail = entry.FindPropertyRelative("iconsButton").objectReferenceValue as Button;
                var detailed = entry.FindPropertyRelative("detailedButton").objectReferenceValue as Button;
                if (string.IsNullOrWhiteSpace(id)) continue;
                views.Add(new SparkAILearningPanel2UX.ActionView
                {
                    id = id,
                    railButton = rail,
                    detailedButton = detailed,
                    railBackground = rail != null ? rail.GetComponent<Image>() : null,
                    detailedBackground = detailed != null ? detailed.GetComponent<Image>() : null
                });
            }

            SparkAILearningPanel2UX ux = panel.GetComponent<SparkAILearningPanel2UX>();
            if (ux == null) ux = Undo.AddComponent<SparkAILearningPanel2UX>(panel.gameObject);
            Undo.RecordObject(ux, "Update AI Learning Panel 2.0 References");
            SerializedObject so = new SerializedObject(ux);
            SetObject(so, "panel", panel);
            SetObject(so, "levelLabel", level);
            SetObject(so, "objectiveLabel", objectives);
            SetObject(so, "lastToolLabel", lastTool);
            SetObject(so, "objectiveFill", fill);
            SetButtons(so.FindProperty("shortcutButtons"), shortcutButtons);
            SetStrings(so.FindProperty("shortcutIds"), shortcutIds);
            SetViews(so.FindProperty("actionViews"), views);
            so.ApplyModifiedProperties();

            // Apply the familiar smooth three-state preset when the core script
            // supports it; old panels still receive the 2.0 UI safely.
            Undo.RecordObject(panel, "Polish AI Learning Drawer Motion");
            SerializedObject core = new SerializedObject(panel);
            SetBool(core, "animateTransitions", true);
            SetFloat(core, "animationSeconds", .34f);
            SetFloat(core, "slideDistanceMultiplier", .88f);
            SetFloat(core, "enterDelayFraction", .025f);
            SetBool(core, "subtleScaleAnimation", true);
            core.ApplyModifiedProperties();

            // Last used action indication is controlled by the UX component.
            // Improve default hover feel, retaining all 34 original buttons.
            for (int i = 0; i < views.Count; i++)
            {
                ImproveButton(views[i].railButton);
                ImproveButton(views[i].detailedButton);
            }
            for (int i = 0; i < shortcutButtons.Length; i++)
                ImproveButton(shortcutButtons[i]);

            EditorUtility.SetDirty(panel);
            EditorUtility.SetDirty(ux);
            if (panel.gameObject.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
            Selection.activeGameObject = panel.gameObject;
            Debug.Log("[AILearningPanel 2.0] Upgraded existing panel. " + views.Count +
                " existing actions retained; four shortcuts, live objective footer, " +
                "last-used highlights and smooth motion. No modules rebuilt.", panel);
        }

        private static void ImproveButton(Button button)
        {
            if (button == null) return;
            Undo.RecordObject(button, "Polish AI Button States");
            ColorBlock cb = button.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = Hex("CEF7FF");
            cb.pressedColor = Hex("7ACDD9");
            cb.selectedColor = Color.white;
            cb.fadeDuration = .115f;
            button.colors = cb;
        }

        private static Color Hex(string code)
        {
            return ColorUtility.TryParseHtmlString("#" + code, out Color c)
                ? c : Color.white;
        }

        private static GameObject UI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void TopLeft(RectTransform rect, float x, float y, float w, float h)
        {
            if (rect == null) return;
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
        }

        private static TextMeshProUGUI GetOrMakeText(Transform parent, string name,
            string value, int size, Color color, float x, float y, float w, float h)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : UI(name, parent);
            if (existing == null) Undo.RegisterCreatedObjectUndo(go, "Create AI Status Text");
            RectTransform rect = go.GetComponent<RectTransform>();
            Undo.RecordObject(rect, "Position AI Status Text");
            TopLeft(rect, x, y, w, h);
            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            if (label == null) label = Undo.AddComponent<TextMeshProUGUI>(go);
            Undo.RecordObject(label, "Style AI Status Text");
            label.text = value;
            label.fontSize = size;
            label.fontSizeMin = size - 2;
            label.fontSizeMax = size;
            label.enableAutoSizing = true;
            label.color = color;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = false;
            label.richText = false;
            return label;
        }

        private static Image GetOrMakeImage(Transform parent, string name,
            float x, float y, float w, float h, Color color)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : UI(name, parent);
            if (existing == null) Undo.RegisterCreatedObjectUndo(go, "Create AI Progress Bar");
            RectTransform rect = go.GetComponent<RectTransform>();
            Undo.RecordObject(rect, "Position AI Progress Bar");
            TopLeft(rect, x, y, w, h);
            Image image = go.GetComponent<Image>();
            if (image == null) image = Undo.AddComponent<Image>(go);
            Undo.RecordObject(image, "Style AI Progress Bar");
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Button GetOrMakeButton(Transform parent, string name,
            string title, float x, float y, float w, float h)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : UI(name, parent);
            if (existing == null) Undo.RegisterCreatedObjectUndo(go, "Create AI Quick Launch Button");
            RectTransform rect = go.GetComponent<RectTransform>();
            Undo.RecordObject(rect, "Align AI Quick Launch");
            TopLeft(rect, x, y, w, h);
            Image image = go.GetComponent<Image>();
            if (image == null) image = Undo.AddComponent<Image>(go);
            Undo.RecordObject(image, "Style AI Quick Launch");
            image.color = Tile;
            image.raycastTarget = true;
            Button button = go.GetComponent<Button>();
            if (button == null) button = Undo.AddComponent<Button>(go);
            button.targetGraphic = image;
            TextMeshProUGUI label = GetOrMakeText(go.transform, "Label", title,
                11, Ink, 0, 0, w, h);
            label.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private static void SetObject(SerializedObject so, string field, Object value)
        {
            SerializedProperty p = so.FindProperty(field);
            if (p != null) p.objectReferenceValue = value;
            else Debug.LogError("[AILearningPanel 2.0] Serialized field missing: " + field);
        }
        private static void SetFloat(SerializedObject so, string field, float value)
        {
            SerializedProperty p = so.FindProperty(field);
            if (p != null) p.floatValue = value;
        }
        private static void SetBool(SerializedObject so, string field, bool value)
        {
            SerializedProperty p = so.FindProperty(field);
            if (p != null) p.boolValue = value;
        }
        private static void SetButtons(SerializedProperty array, Button[] buttons)
        {
            if (array == null) return;
            array.arraySize = buttons.Length;
            for (int i = 0; i < buttons.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
        }
        private static void SetStrings(SerializedProperty array, string[] values)
        {
            if (array == null) return;
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                array.GetArrayElementAtIndex(i).stringValue = values[i];
        }
        private static void SetViews(SerializedProperty array,
            List<SparkAILearningPanel2UX.ActionView> values)
        {
            if (array == null) return;
            array.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
            {
                SerializedProperty e = array.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("id").stringValue = values[i].id;
                e.FindPropertyRelative("railButton").objectReferenceValue = values[i].railButton;
                e.FindPropertyRelative("detailedButton").objectReferenceValue = values[i].detailedButton;
                e.FindPropertyRelative("railBackground").objectReferenceValue = values[i].railBackground;
                e.FindPropertyRelative("detailedBackground").objectReferenceValue = values[i].detailedBackground;
            }
        }
    }
}
#endif
