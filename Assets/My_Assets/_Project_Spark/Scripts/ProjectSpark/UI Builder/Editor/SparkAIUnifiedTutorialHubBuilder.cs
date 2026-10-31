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
    /// Idempotent one-click hub UI builder. Does not rebuild or alter existing modules.
    /// Every generated RectTransform also has a CanvasRenderer.
    /// </summary>
    public static class SparkAIUnifiedTutorialHubBuilder
    {
        private static readonly Color Back = new Color(.022f, .067f, .113f, .985f);
        private static readonly Color Tile = new Color(.059f, .142f, .212f, 1f);
        private static readonly Color Dark = new Color(.035f, .105f, .16f, 1f);
        private static readonly Color Accent = new Color(.27f, .88f, .99f, 1f);
        private static readonly Color Ink = new Color(.95f, .985f, 1f, 1f);
        private static readonly Color Muted = new Color(.68f, .82f, .89f, 1f);
        private static readonly Color Btn = new Color(.092f, .34f, .49f, 1f);

        [MenuItem("Tools/Project Spark/AI/Create Unified Tutorial Hub")]
        public static void Create()
        {
            SparkVisualTeachingManager teaching =
                Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            if (teaching == null)
            {
                Debug.LogError("[Spark AI Unified Hub] Create the AAA Visual Teaching System first.");
                return;
            }

            Canvas canvas = teaching.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[Spark AI Unified Hub] Teaching Manager must be under a Canvas.", teaching);
                return;
            }

            Transform existingRoot = canvas.transform.Find("SparkAIUnifiedTutorialHubRoot");
            if (existingRoot != null)
            {
                Selection.activeGameObject = existingRoot.gameObject;
                Debug.Log("[Spark AI Unified Hub] Existing hub selected. No duplicate created.", existingRoot);
                return;
            }

            SparkAIAnimatedAnswerPlayer animated =
                Object.FindFirstObjectByType<SparkAIAnimatedAnswerPlayer>(FindObjectsInactive.Include);
            SparkAIComponentVisualLessonPlayer component =
                Object.FindFirstObjectByType<SparkAIComponentVisualLessonPlayer>(FindObjectsInactive.Include);
            SparkAIStudentProgressCenter student =
                Object.FindFirstObjectByType<SparkAIStudentProgressCenter>(FindObjectsInactive.Include);
            SparkAIUnifiedAssistant assistant =
                Object.FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            LevelGamePlayManager level =
                Object.FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);

            GameObject root = UI("SparkAIUnifiedTutorialHubRoot", canvas.transform);
            Undo.RegisterCreatedObjectUndo(root, "Create Spark AI Unified Tutorial Hub");
            Stretch(root.GetComponent<RectTransform>());

            // Top-left launcher below Student Center tab. Can be repositioned in Inspector.
            Button opener = MakeButton(root.transform, "OpenUnifiedHub", "AI TUTORIAL HUB", 18, -70, 198, 39);

            GameObject card = UI("UnifiedTutorialHubCard", root.transform);
            Center(card.GetComponent<RectTransform>(), 886, 630);
            Image background = card.AddComponent<Image>();
            background.color = Back;
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = Accent;
            outline.effectDistance = new Vector2(1f, -1f);

            MakeText(card.transform, "Title", "PROJECT SPARK  /  UNIFIED AI TUTORIAL HUB", 22, Accent, 22, -18, 777, 32);
            MakeText(card.transform, "Subtitle", "TEACH  •  EXPLAIN  •  EXPLORE  •  TRACK", 12, Muted, 22, -52, 790, 22);
            Button close = MakeButton(card.transform, "Close", "X", 833, -17, 32, 30);

            Block(card.transform, "LevelPanel", 21, -82, 844, 82, Dark);
            TMP_Text levelText = MakeText(card.transform, "LevelStatus", "NO ACTIVE LEVEL", 13, Ink, 35, -94, 812, 24);
            TMP_Text lessonText = MakeText(card.transform, "LessonStatus", "VISUAL LESSON  /  NONE LOADED", 12, Muted, 35, -124, 812, 23);

            MakeText(card.transform, "ModuleHeading", "SELECT TEACHING MODE", 13, Accent, 23, -181, 800, 22);

            TMP_Text whiteboardAvailability;
            TMP_Text animatedAvailability;
            TMP_Text componentAvailability;
            TMP_Text studentAvailability;

            Button whiteboard = Module(card.transform, "Whiteboard", "01  CIRCUIT WHITEBOARD",
                "Schematic steps and visual annotations", 22, -214, 409, 89, out whiteboardAvailability);
            Button animatedButton = Module(card.transform, "AnimatedAI", "02  ANIMATED AI ANSWERS",
                "Play verified AI explanation steps", 455, -214, 409, 89, out animatedAvailability);
            Button componentButton = Module(card.transform, "ComponentLessons", "03  COMPONENT LESSONS",
                "Resistor, LED, switch and fault topics", 22, -314, 409, 89, out componentAvailability);
            Button studentButton = Module(card.transform, "StudentProgress", "04  STUDENT PROGRESS",
                "Training, analytics, coach and repairs", 455, -314, 409, 89, out studentAvailability);

            MakeText(card.transform, "ControlsHeading", "VISUAL TEACHING CONTROLS", 13, Accent, 23, -421, 810, 23);
            Button world = MakeButton(card.transform, "View3D", "3D VIEW", 22, -458, 146, 40);
            Button both = MakeButton(card.transform, "ViewBoth", "BOTH VIEWS", 175, -458, 147, 40);
            Button prev = MakeButton(card.transform, "PreviousStep", "< PREVIOUS", 329, -458, 123, 40);
            Button next = MakeButton(card.transform, "NextStep", "NEXT >", 459, -458, 117, 40);
            Button replay = MakeButton(card.transform, "ReplayStep", "REPLAY", 583, -458, 117, 40);
            Button ask = MakeButton(card.transform, "AskSpark", "ASK SPARK", 707, -458, 157, 40);

            Block(card.transform, "FooterPanel", 22, -515, 842, 86, Tile);
            TMP_Text feedback = MakeText(card.transform, "Feedback", "Select a mode. Existing lessons and gameplay remain in their current state.",
                13, Ink, 34, -529, 818, 49);
            MakeText(card.transform, "FooterNote", "No new circuit states, AI providers or training scores are generated by this hub.",
                11, Muted, 34, -579, 816, 17);

            SparkAIUnifiedTutorialHub controller = Undo.AddComponent<SparkAIUnifiedTutorialHub>(root);
            SerializedObject so = new SerializedObject(controller);
            Assign(so, "teaching", teaching);
            Assign(so, "animatedAnswers", animated);
            Assign(so, "componentLessons", component);
            Assign(so, "studentProgress", student);
            Assign(so, "assistant", assistant);
            Assign(so, "levelManager", level);
            Assign(so, "hubCard", card);
            Assign(so, "levelStatus", levelText);
            Assign(so, "lessonStatus", lessonText);
            Assign(so, "feedback", feedback);
            Assign(so, "whiteboardAvailability", whiteboardAvailability);
            Assign(so, "animatedAvailability", animatedAvailability);
            Assign(so, "componentAvailability", componentAvailability);
            Assign(so, "studentAvailability", studentAvailability);
            Assign(so, "whiteboardButton", whiteboard);
            Assign(so, "animatedButton", animatedButton);
            Assign(so, "componentButton", componentButton);
            Assign(so, "studentButton", studentButton);
            Assign(so, "worldButton", world);
            Assign(so, "bothButton", both);
            Assign(so, "previousButton", prev);
            Assign(so, "nextButton", next);
            Assign(so, "replayButton", replay);
            Assign(so, "askButton", ask);
            so.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(opener.onClick, controller.Toggle);
            UnityEventTools.AddPersistentListener(close.onClick, controller.Hide);
            UnityEventTools.AddPersistentListener(whiteboard.onClick, controller.OpenWhiteboard);
            UnityEventTools.AddPersistentListener(animatedButton.onClick, controller.OpenAnimatedAnswers);
            UnityEventTools.AddPersistentListener(componentButton.onClick, controller.OpenComponentLessons);
            UnityEventTools.AddPersistentListener(studentButton.onClick, controller.OpenStudentProgress);
            UnityEventTools.AddPersistentListener(world.onClick, controller.OpenWorldView);
            UnityEventTools.AddPersistentListener(both.onClick, controller.OpenBothViews);
            UnityEventTools.AddPersistentListener(prev.onClick, controller.PreviousTeachingStep);
            UnityEventTools.AddPersistentListener(next.onClick, controller.NextTeachingStep);
            UnityEventTools.AddPersistentListener(replay.onClick, controller.ReplayTeachingStep);
            UnityEventTools.AddPersistentListener(ask.onClick, controller.OpenAskSpark);

            card.SetActive(false);
            Selection.activeGameObject = root;
            EditorUtility.SetDirty(controller);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[Spark AI Unified Hub] Created. Click AI TUTORIAL HUB in Play Mode. " +
                "Missing modules will be marked unavailable until their builders are run.", root);
        }

        private static GameObject UI(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Center(RectTransform rect, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void TopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void Block(Transform parent, string name, float x, float y,
            float width, float height, Color color)
        {
            GameObject obj = UI(name, parent);
            TopLeft(obj.GetComponent<RectTransform>(), x, y, width, height);
            Image image = obj.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static TMP_Text MakeText(Transform parent, string name, string value, int fontSize,
            Color color, float x, float y, float width, float height)
        {
            GameObject obj = UI(name, parent);
            TopLeft(obj.GetComponent<RectTransform>(), x, y, width, height);
            TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.enableAutoSizing = true;
            text.fontSizeMin = 10;
            text.fontSizeMax = fontSize;
            text.raycastTarget = false;
            text.richText = false;
            return text;
        }

        private static Button MakeButton(Transform parent, string name, string label,
            float x, float y, float width, float height)
        {
            GameObject obj = UI(name, parent);
            TopLeft(obj.GetComponent<RectTransform>(), x, y, width, height);
            Image background = obj.AddComponent<Image>();
            background.color = Btn;
            Button button = obj.AddComponent<Button>();
            button.targetGraphic = background;
            TMP_Text text = MakeText(obj.transform, "Label", label, 13, Ink, 0, 0, width, height);
            text.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private static Button Module(Transform parent, string name, string label, string subtitle,
            float x, float y, float width, float height, out TMP_Text status)
        {
            GameObject obj = UI(name, parent);
            TopLeft(obj.GetComponent<RectTransform>(), x, y, width, height);
            Image background = obj.AddComponent<Image>();
            background.color = Tile;
            Button button = obj.AddComponent<Button>();
            button.targetGraphic = background;
            MakeText(obj.transform, "ModuleTitle", label, 16, Ink, 15, -10, width - 30, 24);
            MakeText(obj.transform, "ModuleSubtitle", subtitle, 12, Muted, 15, -40, width - 30, 20);
            status = MakeText(obj.transform, "Availability", "CHECKING", 11, Accent, 15, -65, width - 30, 17);
            return button;
        }

        private static void Assign(SerializedObject so, string field, Object value)
        {
            SerializedProperty property = so.FindProperty(field);
            if (property == null)
                Debug.LogError("[Spark AI Unified Hub] Missing serialized field: " + field);
            else property.objectReferenceValue = value;
        }
    }
}
#endif
