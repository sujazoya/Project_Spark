#if UNITY_EDITOR
using ProjectSpark.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching.Editor
{
    /// <summary>Idempotent, additive Editor builder for the guided repair overlay.</summary>
    public static class SparkAIGuidedRepairMissionsBuilder
    {
        private static readonly Color Back = new Color(.025f, .075f, .115f, .985f);
        private static readonly Color Tile = new Color(.067f, .145f, .22f, 1f);
        private static readonly Color Accent = new Color(.27f, .89f, 1f, 1f);
        private static readonly Color Ink = new Color(.94f, .98f, 1f, 1f);
        private static readonly Color Muted = new Color(.7f, .8f, .87f, 1f);

        [MenuItem("Tools/Project Spark/AI/Create Guided Repair Missions")]
        public static void Create()
        {
            SparkVisualTeachingManager teacher = Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            SparkVerifiedElectricalPublisher publisher = Object.FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            LevelGamePlayManager level = Object.FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            SparkWorldAnnotationRenderer renderer = Object.FindFirstObjectByType<SparkWorldAnnotationRenderer>(FindObjectsInactive.Include);
            if (teacher == null || publisher == null || level == null || renderer == null)
            {
                Debug.LogError("[Spark Guided Repair] Requires AAA Teaching Manager, " +
                    "Verified Electrical Publisher, LevelGamePlayManager and World Annotation Renderer.");
                return;
            }
            Canvas canvas = teacher.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[Spark Guided Repair] The teaching manager requires a parent Canvas.", teacher);
                return;
            }

            Transform existing = canvas.transform.Find("SparkAIGuidedRepairRoot");
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                Debug.Log("[Spark Guided Repair] Already installed. Existing panel selected; no duplicate created.", existing);
                return;
            }

            GameObject root = UI("SparkAIGuidedRepairRoot", canvas.transform);
            Undo.RegisterCreatedObjectUndo(root, "Create Spark Guided Repair Missions");
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Button launcher = ButtonAt(root.transform, "OpenRepairMissions", "AI REPAIR MISSIONS",
                -231, -92, 212, 41, true);
            GameObject card = UI("RepairMissionCard", root.transform);
            Position(card.GetComponent<RectTransform>(), -598f, -149f, 578f, 565f, true);
            Image back = card.AddComponent<Image>();
            back.color = Back;
            Outline border = card.AddComponent<Outline>();
            border.effectColor = Accent;
            border.effectDistance = new Vector2(1f, -1f);

            Text(card.transform, "Title", "SPARK AI / VERIFIED REPAIR MISSIONS", 15, Accent, 17, -12, 494, 25);
            Button close = ButtonAt(card.transform, "Close", "X", 518, -11, 42, 30);
            TMP_Text title = Text(card.transform, "FaultTitle", "AWAITING VERIFIED CIRCUIT FAULT", 20, Ink, 18, -50, 534, 31);
            Block(card.transform, "EvidenceTile", 16, -90, 546, 112);
            TMP_Text evidence = Text(card.transform, "Evidence", "Publish a verified solver frame.", 14, Muted,
                26, -100, 530, 92);
            TMP_Text step = Text(card.transform, "Step", "CHOOSE A REPAIR MISSION", 17, Accent,
                20, -215, 530, 30);
            Block(card.transform, "GuidanceTile", 16, -250, 546, 147);
            TMP_Text body = Text(card.transform, "Guidance", "Begin after verified evidence appears.", 16, Ink,
                27, -261, 527, 127);
            TMP_Text status = Text(card.transform, "VerificationStatus", "Read-only training mission", 13, Muted,
                19, -402, 538, 48);

            Button previous = ButtonAt(card.transform, "PreviousFault", "< FAULT", 17, -456, 86, 33);
            Button next = ButtonAt(card.transform, "NextFault", "FAULT >", 108, -456, 83, 33);
            Button begin = ButtonAt(card.transform, "Begin", "BEGIN", 196, -456, 79, 33);
            Button inspected = ButtonAt(card.transform, "Inspected", "INSPECTED", 280, -456, 112, 33);
            Button hint = ButtonAt(card.transform, "NextHint", "HINT", 397, -456, 78, 33);
            Button focus = ButtonAt(card.transform, "Highlight", "3D", 480, -456, 81, 33);
            Button verify = ButtonAt(card.transform, "VerifyRepair", "VERIFY REPAIR", 17, -503, 254, 42);
            Button cancel = ButtonAt(card.transform, "Cancel", "CANCEL / NEW MISSION", 281, -503, 280, 42);

            SparkAIGuidedRepairMissions controller = Undo.AddComponent<SparkAIGuidedRepairMissions>(root);
            SerializedObject so = new SerializedObject(controller);
            Assign(so, "publisher", publisher);
            Assign(so, "levelManager", level);
            Assign(so, "worldRenderer", renderer);
            Assign(so, "teacher", teacher);
            Assign(so, "panel", card);
            Assign(so, "titleText", title);
            Assign(so, "evidenceText", evidence);
            Assign(so, "stepText", step);
            Assign(so, "bodyText", body);
            Assign(so, "statusText", status);
            Assign(so, "beginButton", begin);
            Assign(so, "inspectedButton", inspected);
            Assign(so, "hintButton", hint);
            Assign(so, "verifyButton", verify);
            Assign(so, "highlightButton", focus);
            Assign(so, "cancelButton", cancel);
            Assign(so, "previousButton", previous);
            Assign(so, "nextButton", next);
            so.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(launcher.onClick, controller.Toggle);
            UnityEventTools.AddPersistentListener(close.onClick, controller.Hide);
            UnityEventTools.AddPersistentListener(previous.onClick, controller.PreviousFault);
            UnityEventTools.AddPersistentListener(next.onClick, controller.NextFault);
            UnityEventTools.AddPersistentListener(begin.onClick, controller.BeginMission);
            UnityEventTools.AddPersistentListener(inspected.onClick, controller.AcknowledgeInspection);
            UnityEventTools.AddPersistentListener(hint.onClick, controller.NextHint);
            UnityEventTools.AddPersistentListener(focus.onClick, controller.FocusFault);
            UnityEventTools.AddPersistentListener(verify.onClick, controller.VerifyRepair);
            UnityEventTools.AddPersistentListener(cancel.onClick, controller.CancelMission);

            card.SetActive(false);
            Selection.activeGameObject = root;
            EditorUtility.SetDirty(controller);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("[Spark Guided Repair] Installed. Publish a verified frame, open AI REPAIR MISSIONS, and press BEGIN.", root);
        }

        private static GameObject UI(string name, Transform parent)
        {
            // Explicit CanvasRenderer avoids MissingComponentException under masked UGUI canvases.
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Position(RectTransform r, float x, float y, float w, float h, bool right = false)
        {
            r.anchorMin = r.anchorMax = right ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = new Vector2(x, y);
            r.sizeDelta = new Vector2(w, h);
        }

        private static TMP_Text Text(Transform parent, string name, string caption, int fontSize,
            Color color, float x, float y, float w, float h)
        {
            var go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = caption;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.enableAutoSizing = true;
            text.fontSizeMin = 11;
            text.fontSizeMax = fontSize;
            text.raycastTarget = false;
            text.richText = false;
            return text;
        }

        private static void Block(Transform parent, string name, float x, float y, float w, float h)
        {
            var go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h);
            go.AddComponent<Image>().color = Tile;
            go.GetComponent<Image>().raycastTarget = false;
        }

        private static Button ButtonAt(Transform parent, string name, string caption,
            float x, float y, float w, float h, bool right = false)
        {
            var go = UI(name, parent);
            Position(go.GetComponent<RectTransform>(), x, y, w, h, right);
            var background = go.AddComponent<Image>();
            background.color = new Color(.09f, .31f, .47f, 1f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = background;
            TMP_Text text = Text(go.transform, "Label", caption, 13, Ink, 0, 0, w, h);
            text.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private static void Assign(SerializedObject so, string name, Object value)
        {
            var prop = so.FindProperty(name);
            if (prop == null) Debug.LogError("[Spark Guided Repair] Missing serialized field: " + name);
            else prop.objectReferenceValue = value;
        }
    }
}
#endif
