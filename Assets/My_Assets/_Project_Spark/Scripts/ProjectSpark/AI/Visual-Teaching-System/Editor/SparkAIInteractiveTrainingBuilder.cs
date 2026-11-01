#if UNITY_EDITOR
using ProjectSpark.Gameplay;
using ProjectSpark.AI.Tutorial;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching.Editor
{
    /// <summary>
    /// One-click additive and idempotent UI builder. Never touches existing game logic.
    /// Uses explicit CanvasRenderer components on every generated UI object.
    /// </summary>
    public static class SparkAIInteractiveTrainingBuilder
    {
        private static readonly Color Back = new Color(.025f, .073f, .12f, .985f);
        private static readonly Color Tile = new Color(.065f, .14f, .21f, 1f);
        private static readonly Color Accent = new Color(.28f, .9f, 1f, 1f);
        private static readonly Color Ink = new Color(.94f, .98f, 1f, 1f);
        private static readonly Color Muted = new Color(.71f, .83f, .89f, 1f);
        private static readonly Color ButtonColor = new Color(.08f, .30f, .45f, 1f);

        [MenuItem("Tools/Project Spark/AI/Create Interactive Training Challenges")]
        public static void Create()
        {
            var teaching = Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            var level = Object.FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            var publisher = Object.FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            var assistant = Object.FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            var renderer = Object.FindFirstObjectByType<SparkWorldAnnotationRenderer>(FindObjectsInactive.Include);
            Canvas canvas = teaching != null ? teaching.GetComponentInParent<Canvas>() : null;
            if (canvas == null || level == null)
            {
                Debug.LogError("[Spark AI Training] Requires the AAA Visual Teaching Canvas and LevelGamePlayManager.");
                return;
            }
            Transform existingRoot = canvas.transform.Find("SparkAIInteractiveTrainingRoot");
            if (existingRoot != null)
            {
                Selection.activeGameObject = existingRoot.gameObject;
                Debug.Log("[Spark AI Training] Existing training panel selected; duplicate not created.", existingRoot);
                return;
            }

            GameObject root = UI("SparkAIInteractiveTrainingRoot", canvas.transform);
            Undo.RegisterCreatedObjectUndo(root, "Create Spark AI Training Challenges");
            Stretch(root.GetComponent<RectTransform>());
            Button opener = MakeButton(root.transform, "OpenTraining", "AI TRAINING", -223, -143, 208, 41, true);

            GameObject card = UI("TrainingCard", root.transform);
            Position(card.GetComponent<RectTransform>(), -645, -112, 627, 675, true);
            Image background = card.AddComponent<Image>();
            background.color = Back;
            var outline = card.AddComponent<Outline>();
            outline.effectColor = Accent;
            outline.effectDistance = new Vector2(1f, -1f);

            MakeText(card.transform, "Header", "SPARK AI  /  INTERACTIVE TRAINING", 16, Accent, 20, -14, 528, 28);
            Button close = MakeButton(card.transform, "Close", "X", 570, -12, 38, 29);
            TMP_Text title = MakeText(card.transform, "ChallengeTitle", "START A LEVEL TO TRAIN", 21, Ink, 20, -53, 585, 35);
            TMP_Text tier = MakeText(card.transform, "Difficulty", "01 / FOUNDATION", 13, Accent, 20, -93, 585, 22);

            MakeBlock(card.transform, "ObjectiveBackground", 17, -123, 593, 170);
            TMP_Text objective = MakeText(card.transform, "Objective", "Configured objective information", 15, Ink, 29, -133, 568, 150);
            MakeText(card.transform, "GuideHeader", "TRAINING STEPS", 13, Accent, 21, -302, 560, 22);
            MakeBlock(card.transform, "GuideBackground", 17, -330, 593, 103);
            TMP_Text guide = MakeText(card.transform, "Guidance", "Practice using normal circuit controls.", 15, Ink, 29, -342, 567, 85);

            MakeBlock(card.transform, "EvidenceBackground", 17, -445, 593, 50);
            TMP_Text evidence = MakeText(card.transform, "EvaluatorEvidence", "OFFICIAL EVALUATOR: NOT REPORTED", 13,
                Muted, 28, -449, 565, 44);
            TMP_Text progress = MakeText(card.transform, "TrainingProgress", "TRAINING RECORD 0/0", 13, Ink,
                20, -504, 583, 20);
            MakeBlock(card.transform, "ProgressTrack", 21, -528, 584, 8, new Color(.13f, .20f, .29f, 1f));
            GameObject fillGo = UI("ProgressFill", card.transform);
            var fillRect = fillGo.GetComponent<RectTransform>();
            Position(fillRect, 21, -528, 584, 8);
            var fill = fillGo.AddComponent<Image>();
            fill.color = Accent;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;
            fill.raycastTarget = false;

            Button previous = MakeButton(card.transform, "Previous", "< PREV", 19, -548, 90, 36);
            Button next = MakeButton(card.transform, "Next", "NEXT >", 113, -548, 90, 36);
            Button begin = MakeButton(card.transform, "Begin", "BEGIN", 207, -548, 89, 36);
            Button studied = MakeButton(card.transform, "Studied", "STUDIED", 300, -548, 99, 36);
            Button hint = MakeButton(card.transform, "Hint", "HINT", 403, -548, 89, 36);
            Button focus = MakeButton(card.transform, "Focus", "FOCUS 3D", 496, -548, 110, 36);
            Button ask = MakeButton(card.transform, "AskSpark", "ASK SPARK", 20, -592, 145, 41);
            Button verify = MakeButton(card.transform, "Verify", "VERIFY OBJECTIVE", 172, -592, 207, 41);
            Button cancel = MakeButton(card.transform, "Cancel", "CANCEL", 386, -592, 221, 41);
            TMP_Text status = MakeText(card.transform, "TrainingStatus", "Read-only training", 12,
                Muted, 20, -643, 585, 24);

            var controller = Undo.AddComponent<SparkAIInteractiveTrainingChallenges>(root);
            var so = new SerializedObject(controller);
            Set(so, "levelManager", level);
            Set(so, "publisher", publisher);
            Set(so, "assistant", assistant);
            Set(so, "worldRenderer", renderer);
            Set(so, "panel", card);
            Set(so, "titleText", title);
            Set(so, "tierText", tier);
            Set(so, "objectiveText", objective);
            Set(so, "guideText", guide);
            Set(so, "evidenceText", evidence);
            Set(so, "progressText", progress);
            Set(so, "statusText", status);
            Set(so, "progressFill", fill);
            Set(so, "beginButton", begin);
            Set(so, "studiedButton", studied);
            Set(so, "hintButton", hint);
            Set(so, "askButton", ask);
            Set(so, "highlightButton", focus);
            Set(so, "verifyButton", verify);
            Set(so, "cancelButton", cancel);
            Set(so, "previousButton", previous);
            Set(so, "nextButton", next);
            so.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(opener.onClick, controller.Toggle);
            UnityEventTools.AddPersistentListener(close.onClick, controller.Hide);
            UnityEventTools.AddPersistentListener(previous.onClick, controller.PreviousChallenge);
            UnityEventTools.AddPersistentListener(next.onClick, controller.NextChallenge);
            UnityEventTools.AddPersistentListener(begin.onClick, controller.BeginChallenge);
            UnityEventTools.AddPersistentListener(studied.onClick, controller.AcknowledgeStudy);
            UnityEventTools.AddPersistentListener(hint.onClick, controller.NextHint);
            UnityEventTools.AddPersistentListener(focus.onClick, controller.FocusComponent);
            UnityEventTools.AddPersistentListener(ask.onClick, controller.AskSpark);
            UnityEventTools.AddPersistentListener(verify.onClick, controller.VerifyChallenge);
            UnityEventTools.AddPersistentListener(cancel.onClick, controller.CancelChallenge);

            card.SetActive(false);
            Selection.activeGameObject = root;
            EditorUtility.SetDirty(controller);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[Spark AI Training] Created. Set up the existing solver publisher, open AI TRAINING, and select BEGIN. " +
                (publisher == null ? "Publisher not found; either connect it or disable Require Fresh Solver Frame." :
                    "Publisher assigned."), root);
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
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void Position(RectTransform rect, float x, float y, float width, float height,
            bool topRight = false)
        {
            rect.anchorMin = rect.anchorMax = topRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void MakeBlock(Transform parent, string name, float x, float y, float w, float h,
            Color? color = null)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            var image = go.AddComponent<Image>();
            image.color = color ?? Tile;
            image.raycastTarget = false;
        }

        private static TMP_Text MakeText(Transform parent, string name, string value, int size,
            Color color, float x, float y, float w, float h)
        {
            GameObject go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.enableAutoSizing = true;
            text.fontSizeMin = 10;
            text.fontSizeMax = size;
            text.raycastTarget = false;
            text.richText = false;
            return text;
        }

        private static Button MakeButton(Transform parent, string name, string label,
            float x, float y, float w, float h, bool topRight = false)
        {
            var go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h, topRight);
            var image = go.AddComponent<Image>();
            image.color = ButtonColor;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            TMP_Text text = MakeText(go.transform, "Label", label, 13, Ink, 0, 0, w, h);
            text.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private static void Set(SerializedObject serialized, string field, Object value)
        {
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null) Debug.LogError("[Spark AI Training] Serialized field missing: " + field);
            else property.objectReferenceValue = value;
        }
    }
}
#endif
