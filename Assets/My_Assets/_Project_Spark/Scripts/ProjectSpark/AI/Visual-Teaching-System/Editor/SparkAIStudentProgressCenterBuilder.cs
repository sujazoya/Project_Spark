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
    /// <summary>
    /// Idempotent, non-destructive editor-only builder. Existing module panels, launchers,
    /// saved data, teaching system, solver and gameplay scripts are not modified.
    /// </summary>
    public static class SparkAIStudentProgressCenterBuilder
    {
        private static readonly Color Back = new Color(.025f, .071f, .115f, .985f);
        private static readonly Color Tile = new Color(.060f, .137f, .210f, 1f);
        private static readonly Color TileAlt = new Color(.073f, .164f, .243f, 1f);
        private static readonly Color Accent = new Color(.29f, .91f, 1f, 1f);
        private static readonly Color Ink = new Color(.94f, .98f, 1f, 1f);
        private static readonly Color Muted = new Color(.70f, .82f, .88f, 1f);
        private static readonly Color Btn = new Color(.09f, .34f, .49f, 1f);

        [MenuItem("Tools/Project Spark/AI/Create Student Progress Center")]
        public static void Create()
        {
            SparkVisualTeachingManager teacher =
                Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            if (teacher == null)
            {
                Debug.LogError("[Spark Student Center] AAA Visual Teaching Manager not found. " +
                    "Create the Visual Teaching System first.");
                return;
            }
            Canvas canvas = teacher.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[Spark Student Center] The Visual Teaching Manager requires a parent Canvas.", teacher);
                return;
            }
            Transform existingRoot = canvas.transform.Find("SparkAIStudentProgressCenterRoot");
            if (existingRoot != null)
            {
                Selection.activeGameObject = existingRoot.gameObject;
                Debug.Log("[Spark Student Center] Existing hub selected. No duplicate created.", existingRoot);
                return;
            }

            LevelGamePlayManager level = Object.FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            SparkAIUnifiedAssistant assistant = Object.FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            SparkAIInteractiveTrainingChallenges training = Object.FindFirstObjectByType<SparkAIInteractiveTrainingChallenges>(FindObjectsInactive.Include);
            SparkAITrainingDashboard dashboard = Object.FindFirstObjectByType<SparkAITrainingDashboard>(FindObjectsInactive.Include);
            SparkAITrainingAnalytics analytics = Object.FindFirstObjectByType<SparkAITrainingAnalytics>(FindObjectsInactive.Include);
            SparkAILearningCoach coach = Object.FindFirstObjectByType<SparkAILearningCoach>(FindObjectsInactive.Include);
            SparkAIGuidedRepairMissions repairs = Object.FindFirstObjectByType<SparkAIGuidedRepairMissions>(FindObjectsInactive.Include);

            GameObject root = UI("SparkAIStudentProgressCenterRoot", canvas.transform);
            Undo.RegisterCreatedObjectUndo(root, "Create Spark AI Student Progress Center");
            Stretch(root.GetComponent<RectTransform>());
            // The hub launcher is intentionally placed top-left, away from existing right-side AI tabs.
            Button launcher = ButtonAt(root.transform, "OpenStudentProgress", "STUDENT CENTER", 18, -18, 194, 40);

            GameObject panel = UI("StudentProgressPanel", root.transform);
            Center(panel.GetComponent<RectTransform>(), 985, 680);
            Image background = panel.AddComponent<Image>();
            background.color = Back;
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = Accent;
            outline.effectDistance = new Vector2(1f, -1f);

            TextAt(panel.transform, "Title", "PROJECT SPARK  /  STUDENT PROGRESS CENTER", 21, Accent,
                24, -18, 780, 31);
            TextAt(panel.transform, "Subtitle", "TRAIN  •  ANALYZE  •  LEARN  •  REPAIR", 12, Muted,
                24, -51, 775, 21);
            Button close = ButtonAt(panel.transform, "Close", "X", 932, -18, 32, 31);
            TMP_Text activeLevel = TextAt(panel.transform, "ActiveLevel", "NO ACTIVE LEVEL", 13, Ink,
                24, -79, 935, 22);

            // The two summary cards deliberately show DIFFERENT sources of truth.
            Block(panel.transform, "EvaluatorTile", 22, -111, 457, 115, Tile);
            TextAt(panel.transform, "EvaluatorHeader", "OFFICIAL LEVEL EVALUATOR", 13, Accent, 38, -122, 425, 22);
            TMP_Text evaluator = TextAt(panel.transform, "EvaluatorValue", "EVALUATOR —", 19, Ink,
                38, -150, 425, 28);
            Block(panel.transform, "EvalTrack", 38, -194, 421, 9, new Color(.13f, .23f, .31f, 1f));
            Image evalFill = FillAt(panel.transform, "EvaluatorProgressFill", 38, -194, 421, 9);

            Block(panel.transform, "AchievementsTile", 503, -111, 459, 115, Tile);
            TextAt(panel.transform, "AchievementHeader", "VERIFIED TRAINING ACHIEVEMENTS", 13, Accent,
                519, -122, 426, 22);
            TMP_Text achievements = TextAt(panel.transform, "AchievementValue", "TRAINING RECORD —", 19, Ink,
                519, -150, 426, 28);
            Block(panel.transform, "AchievementTrack", 519, -194, 425, 9, new Color(.13f, .23f, .31f, 1f));
            Image achievementFill = FillAt(panel.transform, "AchievementProgressFill", 519, -194, 425, 9);

            Block(panel.transform, "FocusTile", 22, -240, 940, 46, TileAlt);
            TMP_Text focus = TextAt(panel.transform, "Priority", "NEXT STEP / SELECT PRACTICE", 13,
                Ink, 38, -251, 906, 28);

            TMP_Text trainingStatus;
            TMP_Text dashboardStatus;
            TMP_Text analyticsStatus;
            TMP_Text coachStatus;
            TMP_Text repairStatus;
            Button trainingOpen = ModuleCard(panel.transform, "Training", "01  INTERACTIVE TRAINING",
                "Guided objectives and evaluator checks", 22, -301, 457, 92,
                out trainingStatus);
            Button dashboardOpen = ModuleCard(panel.transform, "Dashboard", "02  TRAINING DASHBOARD",
                "Saved achievements and learning history", 503, -301, 459, 92,
                out dashboardStatus);
            Button analyticsOpen = ModuleCard(panel.transform, "Analytics", "03  TRAINING ANALYTICS",
                "Trends, repeated issues and reports", 22, -407, 457, 92,
                out analyticsStatus);
            Button coachOpen = ModuleCard(panel.transform, "Coach", "04  PERSONAL LEARNING COACH",
                "Recommended exercises and mastery checks", 503, -407, 459, 92,
                out coachStatus);
            Button repairsOpen = ModuleCard(panel.transform, "Repairs", "05  GUIDED REPAIR MISSIONS",
                "Verified fault guidance and repair evaluation", 22, -513, 940, 75,
                out repairStatus);

            Button refresh = ButtonAt(panel.transform, "Refresh", "REFRESH", 24, -608, 133, 38);
            Button ask = ButtonAt(panel.transform, "AskSpark", "ASK SPARK", 169, -608, 167, 38);
            Button export = ButtonAt(panel.transform, "ExportAnalytics", "EXPORT REPORT", 349, -608, 185, 38);
            TMP_Text footer = TextAt(panel.transform, "Footer", "No rewards, progression or electrical state changed.",
                11, Muted, 24, -654, 925, 20);

            SparkAIStudentProgressCenter controller = Undo.AddComponent<SparkAIStudentProgressCenter>(root);
            SerializedObject serialized = new SerializedObject(controller);
            Assign(serialized, "levelManager", level);
            Assign(serialized, "assistant", assistant);
            Assign(serialized, "training", training);
            Assign(serialized, "dashboard", dashboard);
            Assign(serialized, "analytics", analytics);
            Assign(serialized, "coach", coach);
            Assign(serialized, "repairs", repairs);
            Assign(serialized, "hubPanel", panel);
            Assign(serialized, "levelText", activeLevel);
            Assign(serialized, "evaluatorText", evaluator);
            Assign(serialized, "trainingText", achievements);
            Assign(serialized, "focusText", focus);
            Assign(serialized, "objectiveFill", evalFill);
            Assign(serialized, "trainingFill", achievementFill);
            Assign(serialized, "trainingStatus", trainingStatus);
            Assign(serialized, "dashboardStatus", dashboardStatus);
            Assign(serialized, "analyticsStatus", analyticsStatus);
            Assign(serialized, "coachStatus", coachStatus);
            Assign(serialized, "repairStatus", repairStatus);
            Assign(serialized, "footerText", footer);
            Assign(serialized, "trainingButton", trainingOpen);
            Assign(serialized, "dashboardButton", dashboardOpen);
            Assign(serialized, "analyticsButton", analyticsOpen);
            Assign(serialized, "coachButton", coachOpen);
            Assign(serialized, "repairsButton", repairsOpen);
            Assign(serialized, "exportButton", export);
            Assign(serialized, "askButton", ask);
            serialized.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(launcher.onClick, controller.Toggle);
            UnityEventTools.AddPersistentListener(close.onClick, controller.Hide);
            UnityEventTools.AddPersistentListener(refresh.onClick, controller.Refresh);
            UnityEventTools.AddPersistentListener(trainingOpen.onClick, controller.OpenTraining);
            UnityEventTools.AddPersistentListener(dashboardOpen.onClick, controller.OpenDashboard);
            UnityEventTools.AddPersistentListener(analyticsOpen.onClick, controller.OpenAnalytics);
            UnityEventTools.AddPersistentListener(coachOpen.onClick, controller.OpenCoach);
            UnityEventTools.AddPersistentListener(repairsOpen.onClick, controller.OpenRepairs);
            UnityEventTools.AddPersistentListener(ask.onClick, controller.AskSpark);
            UnityEventTools.AddPersistentListener(export.onClick, controller.ExportAnalytics);

            panel.SetActive(false);
            Selection.activeGameObject = root;
            EditorUtility.SetDirty(controller);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[Spark Student Center] Hub created. All five panels are optional; missing modules " +
                "show NOT INSTALLED until their own builders have been run. Existing launchers remain unchanged.", root);
        }

        private static GameObject UI(string name, Transform parent)
        {
            // All Graphic-containing objects, even text labels, have a CanvasRenderer.
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void Position(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
        }

        private static void Center(RectTransform rect, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(w, h);
        }

        private static void Block(Transform parent, string name, float x, float y, float w, float h,
            Color color)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            Image image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static Image FillAt(Transform parent, string name, float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            Image image = go.AddComponent<Image>();
            image.color = Accent;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
            image.fillAmount = 0f;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text TextAt(Transform parent, string name, string content, int fontSize,
            Color color, float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.color = color;
            text.fontSize = fontSize;
            text.enableAutoSizing = true;
            text.fontSizeMin = 10;
            text.fontSizeMax = fontSize;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;
            text.richText = false;
            return text;
        }

        private static Button ButtonAt(Transform parent, string name, string label,
            float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            Image image = go.AddComponent<Image>();
            image.color = Btn;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            TMP_Text caption = TextAt(go.transform, "Label", label, 13, Ink, 0, 0, w, h);
            caption.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private static Button ModuleCard(Transform parent, string name, string heading, string description,
            float x, float y, float w, float h, out TMP_Text status)
        {
            GameObject card = UI(name + "Card", parent);
            Position(card.GetComponent<RectTransform>(), x, y, w, h);
            Image image = card.AddComponent<Image>();
            image.color = Tile;
            image.raycastTarget = false;
            TextAt(card.transform, "Heading", heading, 15, Accent, 15, -8, w - 156, 25);
            TextAt(card.transform, "Description", description, 12, Ink, 15, -37, w - 160, 23);
            status = TextAt(card.transform, "ModuleStatus", "CHECKING MODULE", 11, Muted,
                15, -h + 22, w - 160, 18);
            return ButtonAt(card.transform, "Open", "OPEN", w - 115, -22, 97, 39);
        }

        private static void Assign(SerializedObject serialized, string field, Object value)
        {
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
                Debug.LogError("[Spark Student Center] Missing serialized field: " + field);
            else
                property.objectReferenceValue = value;
        }
    }
}
#endif
