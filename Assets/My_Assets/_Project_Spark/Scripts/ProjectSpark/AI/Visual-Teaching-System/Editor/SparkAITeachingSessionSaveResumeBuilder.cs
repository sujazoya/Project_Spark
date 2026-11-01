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
    /// Idempotent, additive one-click UI installer for the read-only study notebook.
    /// All generated UGUI objects explicitly contain a CanvasRenderer.
    /// </summary>
    public static class SparkAITeachingSessionSaveResumeBuilder
    {
        private static readonly Color Back = new Color(.025f, .073f, .12f, .985f);
        private static readonly Color Tile = new Color(.057f, .14f, .21f, 1f);
        private static readonly Color Inner = new Color(.042f, .108f, .17f, 1f);
        private static readonly Color Accent = new Color(.28f, .9f, 1f, 1f);
        private static readonly Color Ink = new Color(.94f, .98f, 1f, 1f);
        private static readonly Color Muted = new Color(.71f, .83f, .89f, 1f);
        private static readonly Color ButtonColor = new Color(.08f, .30f, .45f, 1f);
        private static readonly Color DangerColor = new Color(.42f, .14f, .20f, 1f);

        [MenuItem("Tools/Project Spark/AI/Create Teaching Session Save Resume")]
        public static void Create()
        {
            SparkVisualTeachingManager teaching =
                Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            if (teaching == null)
            {
                Debug.LogError("[Spark AI Notebook] Install the AAA Visual Teaching Canvas first.");
                return;
            }
            Canvas canvas = teaching.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[Spark AI Notebook] Visual Teaching Manager must be inside a Canvas.", teaching);
                return;
            }

            Transform existingRoot = canvas.transform.Find("SparkAIStudyNotebookRoot");
            if (existingRoot != null)
            {
                Selection.activeGameObject = existingRoot.gameObject;
                Debug.Log("[Spark AI Notebook] Existing panel selected; no duplicate created.", existingRoot);
                return;
            }

            // The AI Teaching Session Controller is optional. Search by its
            // exact name instead of referencing a type that may not be installed.
            MonoBehaviour session = FindOptionalSessionController();
            SparkAIUnifiedAssistant assistant =
                Object.FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            SparkVerifiedElectricalPublisher publisher =
                Object.FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            LevelGamePlayManager level =
                Object.FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);

            GameObject root = UI("SparkAIStudyNotebookRoot", canvas.transform);
            Undo.RegisterCreatedObjectUndo(root, "Create Spark AI Study Notebook");
            Stretch(root.GetComponent<RectTransform>());

            Button opener = MakeButton(root.transform, "OpenNotebook", "SAVE / RESUME", 18, -163, 198, 38);

            GameObject card = UI("StudyNotebookCard", root.transform);
            Center(card.GetComponent<RectTransform>(), 785, 728);
            Image background = card.AddComponent<Image>();
            background.color = Back;
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = Accent;
            outline.effectDistance = new Vector2(1f, -1f);

            MakeText(card.transform, "Header", "SPARK AI  /  TEACHING SESSION NOTEBOOK", 19,
                Accent, 20, -15, 710, 31);
            MakeText(card.transform, "Disclaimer", "HISTORICAL STUDY NOTES ONLY  •  SOLVER AND LEVEL STATE ARE NEVER RESTORED", 11,
                Muted, 21, -50, 721, 22);
            Button close = MakeButton(card.transform, "Close", "X", 736, -15, 29, 29);

            MakePanel(card.transform, "ContextBackground", 20, -81, 745, 113, Inner);
            TMP_Text title = MakeText(card.transform, "SnapshotTitle", "LIVE STUDY NOTEBOOK", 15,
                Accent, 32, -88, 711, 22);
            TMP_Text context = MakeText(card.transform, "RecordedContext", "No saved study context yet.", 12,
                Ink, 32, -115, 714, 72);

            MakeText(card.transform, "AnswerHeader", "LATEST EXPLANATION (ADVISORY)", 13,
                Accent, 21, -206, 727, 23);
            MakePanel(card.transform, "AnswerBackground", 20, -233, 745, 139, Tile);
            TMP_Text answer = MakeText(card.transform, "LastAnswer", "No Spark AI answer recorded.", 13,
                Ink, 33, -243, 716, 120);

            MakePanel(card.transform, "BookmarkBackground", 20, -381, 745, 63, Inner);
            TMP_Text bookmark = MakeText(card.transform, "LessonBookmark", "LAST OBSERVED STEP: NOT RECORDED", 12,
                Ink, 33, -390, 713, 47);

            MakeText(card.transform, "TimelineHeader", "OBSERVED SESSION EVENTS (UTC)", 13,
                Accent, 21, -457, 722, 21);
            MakePanel(card.transform, "TimelineBackground", 20, -484, 745, 103, Tile);
            TMP_Text timeline = MakeText(card.transform, "EventTimeline", "No recorded events.", 11,
                Muted, 33, -491, 712, 89);

            MakePanel(card.transform, "SavedHistoryBackground", 20, -598, 745, 45, Inner);
            TMP_Text saved = MakeText(card.transform, "SavedSessions", "No earlier study sessions.", 11,
                Muted, 31, -600, 720, 41);

            Button save = MakeButton(card.transform, "Save", "SAVE NOW", 20, -652, 110, 35);
            Button resume = MakeButton(card.transform, "ResumeNotes", "RESUME NOTES", 136, -652, 133, 35);
            Button live = MakeButton(card.transform, "LiveNotes", "LIVE", 275, -652, 80, 35);
            Button history = MakeButton(card.transform, "History", "HISTORY", 361, -652, 105, 35);
            Button openSession = MakeButton(card.transform, "OpenSession", "AI SESSION", 472, -652, 130, 35);
            Button delete = MakeButton(card.transform, "DeleteSaved", "DELETE NOTES", 608, -652, 157, 35, DangerColor);
            TMP_Text status = MakeText(card.transform, "Status", "Study notebook ready.", 11,
                Muted, 21, -696, 739, 25);

            SparkAITeachingSessionSaveResume controller =
                Undo.AddComponent<SparkAITeachingSessionSaveResume>(root);
            SerializedObject so = new SerializedObject(controller);
            Assign(so, "sessionController", session);
            Assign(so, "assistant", assistant);
            Assign(so, "teaching", teaching);
            Assign(so, "publisher", publisher);
            Assign(so, "levelManager", level);
            Assign(so, "card", card);
            Assign(so, "titleText", title);
            Assign(so, "contextText", context);
            Assign(so, "answerText", answer);
            Assign(so, "bookmarkText", bookmark);
            Assign(so, "timelineText", timeline);
            Assign(so, "savedSessionsText", saved);
            Assign(so, "statusText", status);
            Assign(so, "resumeButton", resume);
            Assign(so, "openSessionButton", openSession);
            so.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(opener.onClick, controller.Toggle);
            UnityEventTools.AddPersistentListener(close.onClick, controller.Hide);
            UnityEventTools.AddPersistentListener(save.onClick, controller.SaveNow);
            UnityEventTools.AddPersistentListener(resume.onClick, controller.ResumeNotes);
            UnityEventTools.AddPersistentListener(live.onClick, controller.LiveNotes);
            UnityEventTools.AddPersistentListener(history.onClick, controller.ShowHistory);
            UnityEventTools.AddPersistentListener(openSession.onClick, controller.OpenSession);
            UnityEventTools.AddPersistentListener(delete.onClick, controller.DeleteSavedNotes);

            card.SetActive(false);
            Selection.activeGameObject = root;
            EditorUtility.SetDirty(controller);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[Spark AI Notebook] Save/Resume installed. Historical notes only; existing tutorials untouched.", root);
        }

        private static MonoBehaviour FindOptionalSessionController()
        {
            MonoBehaviour[] components = Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (MonoBehaviour component in components)
            {
                if (component != null &&
                    component.GetType().FullName ==
                    "ProjectSpark.UI.VisualTeaching.SparkAITeachingSessionController")
                {
                    return component;
                }
            }

            return null;
        }

        private static GameObject UI(string name, Transform parent)
        {
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

        private static void Center(RectTransform rect, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(w, h);
        }

        private static void Position(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
        }

        private static void MakePanel(Transform parent, string name,
            float x, float y, float w, float h, Color color)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            Image img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
        }

        private static TMP_Text MakeText(Transform parent, string name, string value,
            int size, Color color, float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            TextMeshProUGUI txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = value;
            txt.fontSize = size;
            txt.color = color;
            txt.textWrappingMode = TextWrappingModes.Normal;
            txt.alignment = TextAlignmentOptions.TopLeft;
            txt.enableAutoSizing = true;
            txt.fontSizeMin = 10;
            txt.fontSizeMax = size;
            txt.raycastTarget = false;
            txt.richText = false;
            return txt;
        }

        private static Button MakeButton(Transform parent, string name, string title,
            float x, float y, float w, float h, Color? color = null)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            Image img = go.AddComponent<Image>();
            img.color = color ?? ButtonColor;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = img;
            TMP_Text label = MakeText(go.transform, "Label", title, 12, Ink, 0, 0, w, h);
            label.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private static void Assign(SerializedObject so, string fieldName, Object value)
        {
            SerializedProperty property = so.FindProperty(fieldName);
            if (property == null) Debug.LogError("[Spark AI Notebook] Missing serialized field: " + fieldName);
            else property.objectReferenceValue = value;
        }
    }
}
#endif
