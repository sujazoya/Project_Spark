#if UNITY_EDITOR
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching.Editor
{
    /// <summary>One-click, additive editor builder. Existing training UI is never rebuilt.</summary>
    public static class SparkAITrainingAnalyticsBuilder
    {
        private static readonly Color Back = new Color(.025f, .072f, .115f, .985f);
        private static readonly Color Tile = new Color(.052f, .136f, .204f, 1f);
        private static readonly Color Accent = new Color(.28f, .9f, 1f, 1f);
        private static readonly Color White = new Color(.94f, .98f, 1f, 1f);
        private static readonly Color Muted = new Color(.72f, .83f, .9f, 1f);
        private static readonly Color ButtonColor = new Color(.075f, .30f, .45f, 1f);

        [MenuItem("Tools/Project Spark/AI/Create Training Analytics")]
        public static void Create()
        {
            var teaching = Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            var level = Object.FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            var dashboard = Object.FindFirstObjectByType<SparkAITrainingDashboard>(FindObjectsInactive.Include);
            var training = Object.FindFirstObjectByType<SparkAIInteractiveTrainingChallenges>(FindObjectsInactive.Include);
            var assistant = Object.FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            Canvas canvas = teaching != null ? teaching.GetComponentInParent<Canvas>() : null;
            if (canvas == null)
            {
                Debug.LogError("[Spark AI Analytics] Create the AAA Visual Teaching Canvas before building analytics.");
                return;
            }

            Transform existingRoot = canvas.transform.Find("SparkAITrainingAnalyticsRoot");
            if (existingRoot != null)
            {
                Selection.activeGameObject = existingRoot.gameObject;
                Debug.Log("[Spark AI Analytics] Existing panel selected. No duplicate created.", existingRoot);
                return;
            }

            GameObject root = UI("SparkAITrainingAnalyticsRoot", canvas.transform);
            Undo.RegisterCreatedObjectUndo(root, "Create Spark AI Training Analytics");
            Stretch(root.GetComponent<RectTransform>());

            Button opener = MakeButton(root.transform, "OpenAnalytics", "AI ANALYTICS",
                24, -195, 166, 39);

            GameObject panel = UI("AnalyticsCard", root.transform);
            Position(panel.GetComponent<RectTransform>(), 24, -75, 842, 799);
            Image background = panel.AddComponent<Image>();
            background.color = Back;
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = Accent;
            outline.effectDistance = new Vector2(1f, -1f);

            MakeText(panel.transform, "Header", "SPARK AI  /  TRAINING ANALYTICS", 21, Accent,
                21, -16, 730, 35);
            Button close = MakeButton(panel.transform, "Close", "X", 788, -17, 32, 29);
            TMP_Text levelLabel = MakeText(panel.transform, "ActiveLevel", "SAVED TRAINING REPORT",
                13, Muted, 22, -58, 790, 25);

            MakeBlock(panel.transform, "ObjectiveTile", 22, -95, 246, 87);
            MakeBlock(panel.transform, "CompleteTile", 296, -95, 246, 87);
            MakeBlock(panel.transform, "RemainingTile", 570, -95, 250, 87);
            MakeText(panel.transform, "ObjectiveLabel", "OBSERVED OBJECTIVES", 12, Muted, 37, -105, 220, 20);
            MakeText(panel.transform, "CompleteLabel", "RECORDED COMPLETE", 12, Muted, 311, -105, 220, 20);
            MakeText(panel.transform, "RemainingLabel", "NOT YET RECORDED", 12, Muted, 585, -105, 225, 20);
            TMP_Text total = MakeText(panel.transform, "Total", "0", 32, White, 38, -127, 190, 49);
            TMP_Text earned = MakeText(panel.transform, "Earned", "0", 32, Accent, 312, -127, 190, 49);
            TMP_Text remaining = MakeText(panel.transform, "Remaining", "0", 32, White, 586, -127, 190, 49);

            MakeText(panel.transform, "TrendHeader", "7-DAY ACHIEVEMENT OBSERVATIONS (UTC)",
                15, Accent, 22, -195, 780, 25);
            MakeBlock(panel.transform, "TrendBackground", 22, -226, 798, 145);
            TMP_Text trendSummary = MakeText(panel.transform, "TrendSummary", "No observations yet",
                12, Muted, 36, -230, 750, 22);

            Image[] bars = new Image[7];
            TMP_Text[] dayLabels = new TMP_Text[7];
            for (int i = 0; i < 7; i++)
            {
                float x = 45 + i * 108;
                MakeBlock(panel.transform, "BarTrack" + i, x, -262, 57, 68,
                    new Color(.1f, .22f, .32f, 1f));
                GameObject barObject = UI("DayBar" + i, panel.transform);
                Position(barObject.GetComponent<RectTransform>(), x, -262, 57, 68);
                Image bar = barObject.AddComponent<Image>();
                bar.color = Accent;
                bar.type = Image.Type.Filled;
                bar.fillMethod = Image.FillMethod.Vertical;
                bar.fillOrigin = (int)Image.OriginVertical.Bottom;
                bar.fillAmount = 0;
                bar.raycastTarget = false;
                bars[i] = bar;
                dayLabels[i] = MakeText(panel.transform, "DayLabel" + i, "--/--\n0", 11,
                    Muted, x - 10, -337, 77, 27);
                dayLabels[i].alignment = TextAlignmentOptions.Center;
            }

            MakeText(panel.transform, "TierHeader", "RECORDED SKILL COVERAGE", 13, Accent,
                23, -386, 770, 20);
            TMP_Text tierSummary = MakeText(panel.transform, "TierSummary",
                "FOUNDATION 0/0   APPLIED 0/0   ADVANCED 0/0", 12, White,
                22, -409, 794, 26);

            MakeText(panel.transform, "RecurringHeader", "REPEATED PENDING OBSERVATIONS", 14,
                Accent, 23, -446, 785, 26);
            MakeBlock(panel.transform, "RecurringBackground", 22, -478, 798, 92);
            TMP_Text recurring = MakeText(panel.transform, "RecurringText",
                "No repeated pending observations yet.", 13, White, 38, -487, 765, 76);
            recurring.overflowMode = TextOverflowModes.Ellipsis;

            MakeText(panel.transform, "RecommendationsHeader", "RECOMMENDED PRACTICE", 14,
                Accent, 22, -581, 785, 25);
            MakeBlock(panel.transform, "RecommendationBackground", 22, -611, 798, 91);
            TMP_Text recommendations = MakeText(panel.transform, "RecommendationsText",
                "Start a level to see evaluator-informed practice suggestions.", 13,
                White, 38, -619, 767, 77);
            recommendations.overflowMode = TextOverflowModes.Ellipsis;

            Button refresh = MakeButton(panel.transform, "Refresh", "REFRESH", 22, -713, 125, 38);
            Button openTraining = MakeButton(panel.transform, "Training", "OPEN TRAINING", 157, -713, 160, 38);
            Button ask = MakeButton(panel.transform, "AskSpark", "ASK SPARK", 327, -713, 145, 38);
            Button export = MakeButton(panel.transform, "Export", "EXPORT JSON + CSV", 482, -713, 338, 38);
            TMP_Text status = MakeText(panel.transform, "Status",
                "Training analytics reads saved records and evaluator history only.", 10,
                Muted, 24, -759, 796, 30);

            var analytics = Undo.AddComponent<SparkAITrainingAnalytics>(root);
            var serialized = new SerializedObject(analytics);
            Set(serialized, "levelManager", level);
            Set(serialized, "trainingDashboard", dashboard);
            Set(serialized, "training", training);
            Set(serialized, "assistant", assistant);
            Set(serialized, "card", panel);
            Set(serialized, "levelText", levelLabel);
            Set(serialized, "totalText", total);
            Set(serialized, "earnedText", earned);
            Set(serialized, "remainingText", remaining);
            Set(serialized, "tierSummaryText", tierSummary);
            Set(serialized, "trendSummaryText", trendSummary);
            Set(serialized, "issueText", recurring);
            Set(serialized, "recommendationsText", recommendations);
            Set(serialized, "statusText", status);
            SetArray(serialized, "trendBars", bars);
            SetArray(serialized, "trendLabels", dayLabels);
            serialized.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(opener.onClick, analytics.Toggle);
            UnityEventTools.AddPersistentListener(close.onClick, analytics.Hide);
            UnityEventTools.AddPersistentListener(refresh.onClick, analytics.RefreshAnalytics);
            UnityEventTools.AddPersistentListener(openTraining.onClick, analytics.OpenTraining);
            UnityEventTools.AddPersistentListener(ask.onClick, analytics.AskSpark);
            UnityEventTools.AddPersistentListener(export.onClick, analytics.ExportReport);

            panel.SetActive(false);
            Selection.activeGameObject = root;
            EditorUtility.SetDirty(analytics);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[Spark AI Analytics] Created read-only analytics. " +
                (dashboard == null
                    ? "Dashboard not found: create Training Dashboard to index previous training achievements."
                    : "Existing dashboard connected."), root);
        }

        private static GameObject UI(string name, Transform parent)
        {
            GameObject created = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            created.transform.SetParent(parent, false);
            return created;
        }

        private static void Position(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void MakeBlock(Transform parent, string name, float x, float y, float w, float h,
            Color? shade = null)
        {
            GameObject block = UI(name, parent);
            Position(block.GetComponent<RectTransform>(), x, y, w, h);
            Image image = block.AddComponent<Image>();
            image.color = shade ?? Tile;
            image.raycastTarget = false;
        }

        private static TMP_Text MakeText(Transform parent, string name, string value, int size,
            Color color, float x, float y, float w, float h)
        {
            GameObject child = UI(name, parent);
            Position(child.GetComponent<RectTransform>(), x, y, w, h);
            TextMeshProUGUI label = child.AddComponent<TextMeshProUGUI>();
            label.text = value;
            label.fontSize = size;
            label.color = color;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.alignment = TextAlignmentOptions.TopLeft;
            label.enableAutoSizing = true;
            label.fontSizeMin = 9;
            label.fontSizeMax = size;
            label.raycastTarget = false;
            label.richText = false;
            return label;
        }

        private static Button MakeButton(Transform parent, string name, string label,
            float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            Image image = go.AddComponent<Image>();
            image.color = ButtonColor;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            TMP_Text text = MakeText(go.transform, "Label", label, 13, White, 0, 0, w, h);
            text.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private static void Set(SerializedObject so, string propertyName, Object value)
        {
            SerializedProperty property = so.FindProperty(propertyName);
            if (property == null)
                Debug.LogError("[Spark AI Analytics] Missing serialized field: " + propertyName);
            else
                property.objectReferenceValue = value;
        }

        private static void SetArray<T>(SerializedObject so, string propertyName, T[] values) where T : Object
        {
            SerializedProperty property = so.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError("[Spark AI Analytics] Missing serialized array: " + propertyName);
                return;
            }
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
#endif
