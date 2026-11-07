#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Gameplay;

namespace ProjectSpark.EditorTools
{
    /// <summary>Builds an animated <> side drawer around the existing voltage monitor.</summary>
    public static class SparkVoltageDrawerEditor
    {
        [MenuItem("Tools/Project Spark/Monitoring/Create Animated Voltage Drawer")]
        public static void CreateDrawer()
        {
            // Reuse the current monitor builder. It intentionally refuses duplicate monitors.
            SparkLevelVoltageMonitor monitor = Object.FindFirstObjectByType<SparkLevelVoltageMonitor>();
            if (monitor == null)
            {
                SparkLevelVoltageMonitorEditor.Create();
                monitor = Object.FindFirstObjectByType<SparkLevelVoltageMonitor>();
            }
            if (monitor == null)
            {
                Debug.LogError("[Spark] Unable to create/find the Live Voltage Monitor.");
                return;
            }

            RectTransform panel = monitor.transform.Find("VoltageMonitor_Panel") as RectTransform;
            if (panel == null)
            {
                EditorUtility.DisplayDialog("Project Spark",
                    "The monitor Canvas has no child called VoltageMonitor_Panel. " +
                    "Build the monitor using its Editor tool first.", "OK");
                return;
            }

            SparkVoltageDrawer drawer = monitor.GetComponent<SparkVoltageDrawer>();
            if (drawer != null)
            {
                Selection.activeGameObject = drawer.gameObject;
                EditorGUIUtility.PingObject(drawer);
                Debug.Log("[Spark] Animated voltage drawer already installed.", drawer);
                return;
            }

            Undo.AddComponent<SparkVoltageDrawer>(monitor.gameObject);
            drawer = monitor.GetComponent<SparkVoltageDrawer>();
            RectTransform handle = CreateTab(panel);
            TMP_Text arrow = CreateText("Arrow", handle, "❮", 26f,
                new Vector2(0f, -13f), new Vector2(48f, 37f));
            arrow.alignment = TextAlignmentOptions.Center;
            TMP_Text title = CreateText("Label", handle, "VOLTS", 12f,
                new Vector2(0f, -53f), new Vector2(48f, 32f));
            title.alignment = TextAlignmentOptions.Center;
            title.fontStyle = FontStyles.Bold;

            Button toggle = handle.gameObject.AddComponent<Button>();
            toggle.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = toggle.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = Hex("#B6EDFF");
            cb.pressedColor = Hex("#6DCBEC");
            cb.selectedColor = Color.white;
            toggle.colors = cb;

            SerializedObject so = new SerializedObject(drawer);
            so.FindProperty("drawerPanel").objectReferenceValue = panel;
            so.FindProperty("arrowText").objectReferenceValue = arrow;
            so.FindProperty("tabText").objectReferenceValue = title;
            so.FindProperty("startOpen").boolValue = false;
            so.FindProperty("animationDuration").floatValue = 0.34f;
            so.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(toggle.onClick, drawer.Toggle);
            EditorUtility.SetDirty(toggle);
            EditorUtility.SetDirty(drawer);

            Selection.activeGameObject = monitor.gameObject;
            EditorGUIUtility.PingObject(monitor.gameObject);
            Debug.Log("[Spark] Animated <> drawer created. Save scene and Play. Click VOLTS tab to open/close.", monitor);
        }

        [MenuItem("Tools/Project Spark/Monitoring/Add Drawer To Existing Monitor")]
        private static void UpgradeExisting() => CreateDrawer();

        private static RectTransform CreateTab(RectTransform panel)
        {
            GameObject go = new GameObject("VoltageDrawer_ToggleTab",
                typeof(RectTransform), typeof(Image), typeof(Outline));
            Undo.RegisterCreatedObjectUndo(go, "Create voltage drawer tab");
            go.transform.SetParent(panel, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(48f, 96f);
            // Left of the 650px panel, so tab is still in view when closed.
            rect.anchoredPosition = new Vector2(-1f, -24f);
            Image image = go.GetComponent<Image>();
            image.color = Hex("#19465E");
            image.raycastTarget = true;
            Outline outline = go.GetComponent<Outline>();
            outline.effectColor = Hex("#39C4EE");
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            return rect;
        }

        private static TMP_Text CreateText(string name, Transform parent, string text,
            float size, Vector2 pos, Vector2 dimensions)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = dimensions;
            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = Hex("#E8F5FA");
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out Color c);
            return c;
        }
    }
}
#endif
