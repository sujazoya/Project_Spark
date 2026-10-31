#if UNITY_EDITOR
using ProjectSpark.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching.Editor
{
    /// <summary>Safe, idempotent Editor-only panel builder for component lesson player.</summary>
    public static class SparkAIComponentVisualLessonsBuilder
    {
        private static readonly Color Background = new Color(.025f, .07f, .12f, .98f);
        private static readonly Color Surface = new Color(.07f, .15f, .23f, 1f);
        private static readonly Color Accent = new Color(.27f, .83f, .97f, 1f);
        private static readonly Color ButtonColor = new Color(.09f, .35f, .53f, 1f);
        private static readonly Color White = new Color(.94f, .97f, 1f, 1f);
        private static readonly Color Muted = new Color(.65f, .78f, .86f, 1f);

        [MenuItem("Tools/Project Spark/AI/Create Component Visual Lessons")]
        public static void Create()
        {
            SparkVisualTeachingManager teacher = Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            SparkVerifiedElectricalPublisher publisher = Object.FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            SparkWorldAnnotationRenderer renderer = Object.FindFirstObjectByType<SparkWorldAnnotationRenderer>(FindObjectsInactive.Include);
            LevelGamePlayManager manager = Object.FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            if (teacher == null || publisher == null || renderer == null)
            {
                Debug.LogError("[Spark Component Lessons] Requires existing AAA teaching manager, " +
                    "world annotation renderer, and verified solver publisher. Install the previous packages first.");
                return;
            }
            Canvas canvas = teacher.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[Spark Component Lessons] Visual Teaching Manager has no parent Canvas.", teacher);
                return;
            }

            Transform old = canvas.transform.Find("SparkAIComponentVisualLessonsRoot");
            if (old != null)
            {
                Selection.activeGameObject = old.gameObject;
                Debug.Log("[Spark Component Lessons] Existing panel selected; no duplicate created.", old);
                return;
            }

            GameObject root = UI("SparkAIComponentVisualLessonsRoot", canvas.transform);
            Undo.RegisterCreatedObjectUndo(root, "Create Spark Component Visual Lessons");
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Button launcher = MakeButton(root.transform, "OpenComponentLessons", "COMPONENT LESSONS", 24, -88, 192, 44);
            GameObject card = UI("ComponentLessonsCard", root.transform);
            Position(card.GetComponent<RectTransform>(), 24, -144, 574, 578);
            Image backdrop = card.AddComponent<Image>();
            backdrop.color = Background;
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = Accent;
            outline.effectDistance = new Vector2(1f, -1f);

            Text(card.transform, "Brand", "SPARK AI  /  VERIFIED COMPONENT LESSONS", 15, Accent,
                17, -12, 475, 30);
            Button close = MakeButton(card.transform, "Close", "X", 516, -11, 40, 32);
            TMP_Text headline = Text(card.transform, "FaultHeadline", "AWAITING VERIFIED COMPONENT CONDITIONS",
                20, White, 18, -53, 533, 37);
            TMP_Text counter = Text(card.transform, "StepCounter", "0 / 0", 13, Accent,
                20, -99, 514, 22);
            Block(card.transform, "EvidenceBackground", Surface, 16, -129, 542, 105);
            TMP_Text evidence = Text(card.transform, "VerifiedEvidence", "Waiting for completed solver measurements.",
                14, Muted, 27, -140, 521, 84);
            TMP_Text stepTitle = Text(card.transform, "LessonStepTitle", "LESSON READY", 18, Accent,
                20, -250, 521, 32);
            Block(card.transform, "StepBackground", Surface, 16, -284, 542, 162);
            TMP_Text stepBody = Text(card.transform, "LessonText", "Verified teaching steps will appear here.",
                16, White, 27, -295, 521, 138);
            TMP_Text status = Text(card.transform, "LessonStatus", "READY · PLAYER-TRIGGERED", 12, Muted,
                18, -450, 538, 35);

            Button previousCase = MakeButton(card.transform, "PreviousCase", "◀ FAULT", 17, -493, 89, 34);
            Button nextCase = MakeButton(card.transform, "NextCase", "FAULT ▶", 111, -493, 89, 34);
            Button previousStep = MakeButton(card.transform, "PreviousStep", "PREV", 205, -493, 65, 34);
            Button nextStep = MakeButton(card.transform, "NextStep", "NEXT", 275, -493, 65, 34);
            Button play = MakeButton(card.transform, "Play", "PLAY", 345, -493, 65, 34);
            Button pause = MakeButton(card.transform, "Pause", "PAUSE", 415, -493, 67, 34);
            Button show3D = MakeButton(card.transform, "Show3D", "3D", 487, -493, 70, 34);
            Button board = MakeButton(card.transform, "ShowBoard", "SHOW BOARD (REPLACES LESSON)", 17, -536, 541, 32);

            SparkAIComponentVisualLessonPlayer player = Undo.AddComponent<SparkAIComponentVisualLessonPlayer>(root);
            SerializedObject so = new SerializedObject(player);
            Ref(so, "publisher", publisher);
            Ref(so, "levelManager", manager);
            Ref(so, "teacher", teacher);
            Ref(so, "worldRenderer", renderer);
            Ref(so, "card", card);
            Ref(so, "headlineText", headline);
            Ref(so, "evidenceText", evidence);
            Ref(so, "stepCounterText", counter);
            Ref(so, "stepTitleText", stepTitle);
            Ref(so, "stepBodyText", stepBody);
            Ref(so, "statusText", status);
            Ref(so, "previousCaseButton", previousCase);
            Ref(so, "nextCaseButton", nextCase);
            Ref(so, "previousStepButton", previousStep);
            Ref(so, "nextStepButton", nextStep);
            Ref(so, "playButton", play);
            Ref(so, "pauseButton", pause);
            Ref(so, "worldButton", show3D);
            Ref(so, "boardButton", board);
            so.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(launcher.onClick, player.Toggle);
            UnityEventTools.AddPersistentListener(close.onClick, player.Hide);
            UnityEventTools.AddPersistentListener(previousCase.onClick, player.PreviousCase);
            UnityEventTools.AddPersistentListener(nextCase.onClick, player.NextCase);
            UnityEventTools.AddPersistentListener(previousStep.onClick, player.PreviousStep);
            UnityEventTools.AddPersistentListener(nextStep.onClick, player.NextStep);
            UnityEventTools.AddPersistentListener(play.onClick, player.Play);
            UnityEventTools.AddPersistentListener(pause.onClick, player.PauseOrResume);
            UnityEventTools.AddPersistentListener(show3D.onClick, player.Show3D);
            UnityEventTools.AddPersistentListener(board.onClick, player.ShowBoard);

            card.SetActive(false);
            Selection.activeGameObject = root;
            EditorUtility.SetDirty(player);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[Spark Component Lessons] Created player. Publish verified frame, " +
                "open COMPONENT LESSONS, select a condition, press PLAY.", root);
        }

        private static GameObject UI(string name, Transform parent)
        {
            // Explicit CanvasRenderer prevents the missing-CanvasRenderer errors
            // previously observed on Project Spark's UGUI visual teaching content.
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Position(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
        }

        private static TMP_Text Text(Transform parent, string name, string text, int size, Color color,
            float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.color = color;
            label.fontSize = size;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.enableAutoSizing = true;
            label.fontSizeMax = size;
            label.fontSizeMin = 10;
            label.alignment = TextAlignmentOptions.TopLeft;
            label.raycastTarget = false;
            label.richText = false;
            return label;
        }

        private static Image Block(Transform parent, string name, Color color,
            float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            Image image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Button MakeButton(Transform parent, string name, string caption,
            float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            Image image = go.AddComponent<Image>();
            image.color = ButtonColor;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            TMP_Text label = Text(go.transform, "Caption", caption, 13, White, 0f, 0f, w, h);
            label.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private static void Ref(SerializedObject so, string name, Object target)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property == null) Debug.LogError("[Spark Component Lessons] Missing field: " + name);
            else property.objectReferenceValue = target;
        }
    }
}
#endif
