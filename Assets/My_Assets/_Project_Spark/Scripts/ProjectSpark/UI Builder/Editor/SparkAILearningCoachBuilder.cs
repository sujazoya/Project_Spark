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
    /// Idempotent editor builder. All generated UI objects include CanvasRenderer.
    /// Existing teaching, training, solver and economy objects are untouched.
    /// </summary>
    public static class SparkAILearningCoachBuilder
    {
        private static readonly Color Back = new Color(.025f, .067f, .115f, .98f);
        private static readonly Color Tile = new Color(.065f, .145f, .215f, 1f);
        private static readonly Color Accent = new Color(.28f, .90f, 1f, 1f);
        private static readonly Color Ink = new Color(.94f, .98f, 1f, 1f);
        private static readonly Color Muted = new Color(.70f, .82f, .89f, 1f);
        private static readonly Color ButtonColor = new Color(.075f, .30f, .46f, 1f);

        [MenuItem("Tools/Project Spark/AI/Create Learning Coach")]
        public static void Create()
        {
            var teaching = Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            var level = Object.FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            var publisher = Object.FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            var training = Object.FindFirstObjectByType<SparkAIInteractiveTrainingChallenges>(FindObjectsInactive.Include);
            var assistant = Object.FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            var tutorial = Object.FindFirstObjectByType<SparkTutorialDirector>(FindObjectsInactive.Include);
            Canvas canvas = teaching != null ? teaching.GetComponentInParent<Canvas>() : null;

            if (canvas == null || level == null)
            {
                Debug.LogError("[Spark Learning Coach] Requires existing AAA Visual Teaching Canvas and LevelGamePlayManager.");
                return;
            }

            Transform existingRoot = canvas.transform.Find("SparkAILearningCoachRoot");
            if (existingRoot != null)
            {
                Selection.activeGameObject = existingRoot.gameObject;
                Debug.Log("[Spark Learning Coach] Existing coach selected; no duplicate created.", existingRoot);
                return;
            }

            GameObject root = UI("SparkAILearningCoachRoot", canvas.transform);
            Undo.RegisterCreatedObjectUndo(root, "Create Spark AI Learning Coach");
            Stretch(root.GetComponent<RectTransform>());

            Button openButton = MakeButton(root.transform, "OpenCoach", "AI LEARNING COACH", -221, -204, 206, 40, true);
            GameObject card = UI("LearningCoachCard", root.transform);
            Position(card.GetComponent<RectTransform>(), -814, -67, 800, 805, true);
            Image background = card.AddComponent<Image>();
            background.color = Back;
            var outline = card.AddComponent<Outline>();
            outline.effectColor = Accent;
            outline.effectDistance = new Vector2(1f, -1f);

            MakeText(card.transform, "Header", "SPARK AI  /  PERSONAL LEARNING COACH", 21, Accent, 21, -16, 680, 30);
            Button closeButton = MakeButton(card.transform, "Close", "X", 751, -16, 28, 29);
            TMP_Text header = MakeText(card.transform, "Level", "NO ACTIVE LEVEL", 13, Muted, 22, -55, 750, 22);

            MakeBlock(card.transform, "RecommendationBackground", 19, -86, 761, 95);
            TMP_Text recommendations = MakeText(card.transform, "Recommendation", "Start a level for a recommended practice plan.", 16,
                Ink, 34, -97, 736, 60);
            TMP_Text progress = MakeText(card.transform, "PlanProgress", "PRACTICE PLAN 0/0", 12,
                Muted, 35, -159, 710, 20);
            MakeBlock(card.transform, "ProgressTrack", 22, -186, 752, 7, new Color(.12f, .22f, .30f, 1f));
            GameObject fillGo = UI("ProgressFill", card.transform);
            Position(fillGo.GetComponent<RectTransform>(), 22, -186, 752, 7);
            Image fill = fillGo.AddComponent<Image>();
            fill.color = Accent;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;
            fill.raycastTarget = false;

            MakeText(card.transform, "RequirementsHeader", "CONFIGURED OBJECTIVE / STUDY", 13, Accent, 22, -211, 748, 22);
            MakeBlock(card.transform, "RequirementsBackground", 19, -237, 761, 130);
            TMP_Text objective = MakeText(card.transform, "Requirements", "Objective requirements", 14, Ink,
                32, -245, 737, 113);
            objective.overflowMode = TextOverflowModes.Ellipsis;

            MakeText(card.transform, "StepsHeader", "GUIDED PRACTICE STEPS", 13, Accent, 22, -380, 748, 22);
            MakeBlock(card.transform, "StepsBackground", 19, -408, 761, 109);
            TMP_Text study = MakeText(card.transform, "Steps", "Read, practice, verify.", 14, Ink,
                33, -416, 734, 91);
            study.overflowMode = TextOverflowModes.Ellipsis;

            MakeText(card.transform, "EvidenceHeader", "EVALUATOR EVIDENCE / PRACTICE STATUS", 13,
                Accent, 22, -530, 748, 22);
            MakeBlock(card.transform, "EvidenceBackground", 19, -557, 761, 65);
            TMP_Text evidence = MakeText(card.transform, "Evidence", "EVALUATOR: NOT REPORTED", 13,
                Ink, 33, -564, 731, 54);

            Button previousButton = MakeButton(card.transform, "Previous", "< PREV", 20, -634, 105, 39);
            Button nextButton = MakeButton(card.transform, "Next", "NEXT >", 130, -634, 105, 39);
            Button refreshButton = MakeButton(card.transform, "Refresh", "REFRESH", 240, -634, 118, 39);
            Button studiedButton = MakeButton(card.transform, "Studied", "STUDIED", 363, -634, 174, 39);
            Button hintButton = MakeButton(card.transform, "Hint", "NEXT HINT", 542, -634, 238, 39);

            Button startButton = MakeButton(card.transform, "Start", "START PRACTICE", 20, -679, 185, 40);
            Button verifyButton = MakeButton(card.transform, "Verify", "CHECK MASTERY", 211, -679, 185, 40);
            Button trainingButton = MakeButton(card.transform, "Training", "OPEN TRAINING", 402, -679, 184, 40);
            Button askButton = MakeButton(card.transform, "AskSpark", "ASK SPARK", 592, -679, 187, 40);

            Button tutorialButton = MakeButton(card.transform, "Tutorial", "OPEN TUTORIAL", 20, -724, 188, 36);
            Button cancelButton = MakeButton(card.transform, "Cancel", "CANCEL CHECK", 213, -724, 188, 36);
            TMP_Text status = MakeText(card.transform, "Status", "Read-only coaching; actual evaluator controls success.", 11,
                Muted, 20, -768, 760, 29);

            var coach = Undo.AddComponent<SparkAILearningCoach>(root);
            var serialized = new SerializedObject(coach);
            Set(serialized, "levelManager", level);
            Set(serialized, "publisher", publisher);
            Set(serialized, "training", training);
            Set(serialized, "assistant", assistant);
            Set(serialized, "tutorial", tutorial);
            Set(serialized, "card", card);
            Set(serialized, "headerText", header);
            Set(serialized, "progressText", progress);
            Set(serialized, "recommendationText", recommendations);
            Set(serialized, "objectiveText", objective);
            Set(serialized, "studyText", study);
            Set(serialized, "evidenceText", evidence);
            Set(serialized, "statusText", status);
            Set(serialized, "progressFill", fill);
            Set(serialized, "studiedButton", studiedButton);
            Set(serialized, "startButton", startButton);
            Set(serialized, "verifyButton", verifyButton);
            serialized.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(openButton.onClick, coach.Toggle);
            UnityEventTools.AddPersistentListener(closeButton.onClick, coach.Hide);
            UnityEventTools.AddPersistentListener(previousButton.onClick, coach.Previous);
            UnityEventTools.AddPersistentListener(nextButton.onClick, coach.Next);
            UnityEventTools.AddPersistentListener(refreshButton.onClick, coach.RefreshCoach);
            UnityEventTools.AddPersistentListener(studiedButton.onClick, coach.MarkStudied);
            UnityEventTools.AddPersistentListener(hintButton.onClick, coach.NextHint);
            UnityEventTools.AddPersistentListener(startButton.onClick, coach.StartPractice);
            UnityEventTools.AddPersistentListener(verifyButton.onClick, coach.CheckMastery);
            UnityEventTools.AddPersistentListener(trainingButton.onClick, coach.OpenTraining);
            UnityEventTools.AddPersistentListener(askButton.onClick, coach.AskSpark);
            UnityEventTools.AddPersistentListener(tutorialButton.onClick, coach.OpenTutorial);
            UnityEventTools.AddPersistentListener(cancelButton.onClick, coach.CancelPractice);

            card.SetActive(false);
            Selection.activeGameObject = root;
            EditorUtility.SetDirty(coach);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[Spark Learning Coach] Built. " + (publisher != null
                ? "Solver publisher connected." : "Publisher missing; check Require Fresh Solver Frame setting.") +
                (training != null ? " Training connected." : " Training panel missing; optional."), root);
        }

        private static GameObject UI(string name, Transform parent)
        {
            GameObject created = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            created.transform.SetParent(parent, false);
            return created;
        }
        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        private static void Position(RectTransform rect, float x, float y, float w, float h, bool topRight = false)
        {
            rect.anchorMin = rect.anchorMax = topRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
        }
        private static void MakeBlock(Transform parent, string name, float x, float y, float w, float h, Color? color = null)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            Image image = go.AddComponent<Image>();
            image.color = color ?? Tile;
            image.raycastTarget = false;
        }
        private static TMP_Text MakeText(Transform parent, string name, string value, int size, Color color,
            float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.fontSizeMax = size;
            text.fontSizeMin = 10;
            text.enableAutoSizing = true;
            text.color = color;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            text.richText = false;
            return text;
        }
        private static Button MakeButton(Transform parent, string name, string title,
            float x, float y, float w, float h, bool topRight = false)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h, topRight);
            Image image = go.AddComponent<Image>();
            image.color = ButtonColor;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            TMP_Text caption = MakeText(go.transform, "Label", title, 13, Ink, 0, 0, w, h);
            caption.alignment = TextAlignmentOptions.Center;
            return button;
        }
        private static void Set(SerializedObject data, string name, Object value)
        {
            SerializedProperty field = data.FindProperty(name);
            if (field == null) Debug.LogError("[Spark Learning Coach] Missing serialized field: " + name);
            else field.objectReferenceValue = value;
        }
    }
}
#endif
