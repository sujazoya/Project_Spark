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
    /// <summary>Idempotent, fully Editor-built dashboard. No gameplay scripts are changed.</summary>
    public static class SparkAITrainingDashboardBuilder
    {
        private static readonly Color Back = new Color(.025f, .073f, .12f, .985f);
        private static readonly Color Tile = new Color(.055f, .13f, .205f, 1f);
        private static readonly Color Accent = new Color(.28f, .9f, 1f, 1f);
        private static readonly Color Ink = new Color(.94f, .98f, 1f, 1f);
        private static readonly Color Muted = new Color(.68f, .82f, .89f, 1f);
        private static readonly Color ButtonColor = new Color(.08f, .30f, .45f, 1f);

        [MenuItem("Tools/Project Spark/AI/Create Training Dashboard")]
        public static void Create()
        {
            var teaching = Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            var level = Object.FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            var training = Object.FindFirstObjectByType<SparkAIInteractiveTrainingChallenges>(FindObjectsInactive.Include);
            var assistant = Object.FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            Canvas canvas = teaching != null ? teaching.GetComponentInParent<Canvas>() : null;
            if (canvas == null)
            {
                Debug.LogError("[Spark Training Dashboard] AAA Visual Teaching Canvas not found. Create that Canvas first.");
                return;
            }

            Transform existingRoot = canvas.transform.Find("SparkAITrainingDashboardRoot");
            if (existingRoot != null)
            {
                Selection.activeGameObject = existingRoot.gameObject;
                Debug.Log("[Spark Training Dashboard] Existing dashboard selected; no duplicate created.", existingRoot);
                return;
            }

            GameObject root = UI("SparkAITrainingDashboardRoot", canvas.transform);
            Undo.RegisterCreatedObjectUndo(root, "Create Spark AI Training Dashboard");
            Stretch(root.GetComponent<RectTransform>());
            Button opener = MakeButton(root.transform, "DashboardTab", "TRAINING STATS", -207, -195, 187, 40, true);

            GameObject card = UI("DashboardCard", root.transform);
            Position(card.GetComponent<RectTransform>(), -782, -115, 755, 720, true);
            Image cardImage = card.AddComponent<Image>();
            cardImage.color = Back;
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = Accent;
            outline.effectDistance = new Vector2(1f, -1f);

            MakeText(card.transform, "Header", "SPARK AI  /  TRAINING DASHBOARD", 20, Accent,
                22, -16, 658, 31);
            Button close = MakeButton(card.transform, "Close", "X", 693, -15, 36, 30);
            TMP_Text levelText = MakeText(card.transform, "ActiveLevel", "LOADING LEARNING RECORDS", 13,
                Muted, 23, -58, 704, 23);

            MakeBlock(card.transform, "TotalTile", 22, -96, 221, 95);
            MakeBlock(card.transform, "CompletedTile", 267, -96, 221, 95);
            MakeBlock(card.transform, "PercentTile", 512, -96, 221, 95);
            MakeText(card.transform, "TotalLabel", "OBSERVED OBJECTIVES", 12, Muted, 37, -107, 194, 22);
            MakeText(card.transform, "CompletedLabel", "RECORDED COMPLETE", 12, Muted, 282, -107, 194, 22);
            MakeText(card.transform, "PercentLabel", "COMPLETION RATE", 12, Muted, 527, -107, 194, 22);
            TMP_Text total = MakeText(card.transform, "TotalValue", "0", 33, Ink, 38, -132, 174, 46);
            TMP_Text completed = MakeText(card.transform, "CompletedValue", "0", 33, Accent, 282, -132, 174, 46);
            TMP_Text percent = MakeText(card.transform, "PercentValue", "—", 33, Ink, 527, -132, 174, 46);

            MakeText(card.transform, "SkillsHeader", "SKILL PRACTICE  /  RECORDED OBJECTIVES", 14,
                Accent, 23, -207, 688, 26);
            MakeBlock(card.transform, "SkillsBackground", 22, -239, 711, 151);
            TMP_Text foundation = MakeText(card.transform, "FoundationCount", "FOUNDATION 0 / 0", 13,
                Ink, 38, -248, 654, 20);
            MakeBlock(card.transform, "FoundationTrack", 39, -273, 665, 8,
                new Color(.12f, .25f, .35f, 1f));
            Image foundationFill = MakeFill(card.transform, "FoundationFill", 39, -273, 665, 8);

            TMP_Text applied = MakeText(card.transform, "AppliedCount", "APPLIED 0 / 0", 13,
                Ink, 38, -292, 654, 20);
            MakeBlock(card.transform, "AppliedTrack", 39, -317, 665, 8,
                new Color(.12f, .25f, .35f, 1f));
            Image appliedFill = MakeFill(card.transform, "AppliedFill", 39, -317, 665, 8);

            TMP_Text advanced = MakeText(card.transform, "AdvancedCount", "ADVANCED 0 / 0", 13,
                Ink, 38, -336, 654, 20);
            MakeBlock(card.transform, "AdvancedTrack", 39, -361, 665, 8,
                new Color(.12f, .25f, .35f, 1f));
            Image advancedFill = MakeFill(card.transform, "AdvancedFill", 39, -361, 665, 8);

            MakeText(card.transform, "PracticeHeader", "RECOMMENDED NEXT PRACTICE", 14,
                Accent, 23, -405, 688, 25);
            MakeBlock(card.transform, "PracticeBackground", 22, -435, 711, 91);
            TMP_Text recTitle = MakeText(card.transform, "PracticeTitle", "No recommendation yet", 18,
                Ink, 38, -442, 663, 30);
            TMP_Text recInfo = MakeText(card.transform, "PracticeDetails", "Start a level to get recommendations", 12,
                Muted, 38, -473, 666, 48);

            MakeText(card.transform, "HistoryHeader", "LEARNING HISTORY  /  FIRST OBSERVED", 14,
                Accent, 23, -542, 687, 23);

            GameObject scrollGo = UI("HistoryScroll", card.transform);
            Position(scrollGo.GetComponent<RectTransform>(), 22, -568, 711, 79);
            ScrollRect scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 23f;
            scroll.inertia = true;

            GameObject viewportGo = UI("Viewport", scrollGo.transform);
            Stretch(viewportGo.GetComponent<RectTransform>());
            Image viewportImage = viewportGo.AddComponent<Image>();
            viewportImage.color = Tile;
            viewportImage.raycastTarget = true;
            viewportGo.AddComponent<RectMask2D>();

            GameObject contentGo = UI("Content", viewportGo.transform);
            RectTransform contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 150f);

            TMP_Text history = MakeText(contentGo.transform, "HistoryText",
                "No recorded achievements observed yet.", 12, Ink, 12, -7, 680, 136);
            history.enableAutoSizing = false;
            history.overflowMode = TextOverflowModes.Overflow;
            scroll.viewport = viewportGo.GetComponent<RectTransform>();
            scroll.content = contentRect;

            Button refresh = MakeButton(card.transform, "Refresh", "REFRESH", 22, -662, 130, 34);
            Button openTraining = MakeButton(card.transform, "OpenTraining", "OPEN TRAINING", 162, -662, 241, 34);
            Button ask = MakeButton(card.transform, "AskSpark", "ASK SPARK", 412, -662, 321, 34);
            TMP_Text footer = MakeText(card.transform, "Footer",
                "Training records only; gameplay progression and rewards are unchanged.",
                10, Muted, 22, -701, 704, 18);

            SparkAITrainingDashboard controller = Undo.AddComponent<SparkAITrainingDashboard>(root);
            var serialized = new SerializedObject(controller);
            Set(serialized, "levelManager", level);
            Set(serialized, "training", training);
            Set(serialized, "assistant", assistant);
            Set(serialized, "card", card);
            Set(serialized, "levelText", levelText);
            Set(serialized, "totalText", total);
            Set(serialized, "completedText", completed);
            Set(serialized, "percentText", percent);
            Set(serialized, "foundationText", foundation);
            Set(serialized, "appliedText", applied);
            Set(serialized, "advancedText", advanced);
            Set(serialized, "foundationFill", foundationFill);
            Set(serialized, "appliedFill", appliedFill);
            Set(serialized, "advancedFill", advancedFill);
            Set(serialized, "recommendationTitle", recTitle);
            Set(serialized, "recommendationDetails", recInfo);
            Set(serialized, "historyText", history);
            Set(serialized, "historyContent", contentRect);
            Set(serialized, "historyScroll", scroll);
            Set(serialized, "footerText", footer);
            serialized.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(opener.onClick, controller.Toggle);
            UnityEventTools.AddPersistentListener(close.onClick, controller.Hide);
            UnityEventTools.AddPersistentListener(refresh.onClick, controller.RefreshDashboard);
            UnityEventTools.AddPersistentListener(openTraining.onClick, controller.OpenTraining);
            UnityEventTools.AddPersistentListener(ask.onClick, controller.AskSpark);

            card.SetActive(false);
            Selection.activeGameObject = root;
            EditorUtility.SetDirty(controller);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[Spark Training Dashboard] Built successfully. It observes current level objectives and existing verified training records. " +
                (training == null ? "Training controller missing; create training challenges for OPEN TRAINING." : "Training controller assigned."), root);
        }

        private static GameObject UI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Position(RectTransform rect, float x, float y,
            float width, float height, bool topRight = false)
        {
            rect.anchorMin = rect.anchorMax = topRight ? Vector2.one : new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void MakeBlock(Transform parent, string name,
            float x, float y, float w, float h, Color? color = null)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            Image img = go.AddComponent<Image>();
            img.color = color ?? Tile;
            img.raycastTarget = false;
        }

        private static TMP_Text MakeText(Transform parent, string name, string content,
            int size, Color color, float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.enableAutoSizing = true;
            text.fontSizeMin = 10;
            text.fontSizeMax = size;
            text.raycastTarget = false;
            text.richText = false;
            // Avoid overriding a user's font assets or applying a custom holographic shader.
            return text;
        }

        private static Image MakeFill(Transform parent, string name,
            float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            Image fill = go.AddComponent<Image>();
            fill.color = Accent;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;
            fill.raycastTarget = false;
            return fill;
        }

        private static Button MakeButton(Transform parent, string name,
            string label, float x, float y, float w, float h, bool topRight = false)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h, topRight);
            Image img = go.AddComponent<Image>();
            img.color = ButtonColor;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = img;
            TMP_Text text = MakeText(go.transform, "Label", label, 13, Ink, 0, 0, w, h);
            text.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private static void Set(SerializedObject serialized, string field, Object value)
        {
            SerializedProperty prop = serialized.FindProperty(field);
            if (prop == null)
                Debug.LogError("[Spark Training Dashboard] Missing serialized field: " + field);
            else
                prop.objectReferenceValue = value;
        }
    }
}
#endif
