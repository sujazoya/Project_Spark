#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.UI.VisualTeaching;

/// <summary>
/// Additive editor-only UI builder. Never changes the user's existing assistant,
/// solver, adaptive panel, telemetry, or teaching manager scripts.
/// </summary>
public static class SparkAIAnimatedAnswerBuilder
{
    private static readonly Color Background = new Color(.023f, .07f, .125f, .98f);
    private static readonly Color Surface = new Color(.056f, .155f, .245f, 1f);
    private static readonly Color ButtonColor = new Color(.09f, .36f, .55f, 1f);
    private static readonly Color TextWhite = new Color(.92f, .97f, 1f, 1f);
    private static readonly Color Cyan = new Color(.27f, .85f, .98f, 1f);
    private static readonly Color Muted = new Color(.66f, .77f, .85f, 1f);

    [MenuItem("Tools/Project Spark/AI/Create Animated Answer Player")]
    public static void Create()
    {
        SparkAIExplanationRequestBridge bridge = Object.FindFirstObjectByType<SparkAIExplanationRequestBridge>(FindObjectsInactive.Include);
        SparkAdaptiveExplanationEngine engine = Object.FindFirstObjectByType<SparkAdaptiveExplanationEngine>(FindObjectsInactive.Include);
        SparkAIUnifiedAssistant assistant = Object.FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
        SparkVisualTeachingManager teacher = Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
        SparkAdaptiveExplanationPanel adaptive = Object.FindFirstObjectByType<SparkAdaptiveExplanationPanel>(FindObjectsInactive.Include);
        SparkWorldAnnotationRenderer renderer = Object.FindFirstObjectByType<SparkWorldAnnotationRenderer>(FindObjectsInactive.Include);

        if (bridge == null || engine == null || assistant == null || teacher == null || adaptive == null || renderer == null)
        {
            Debug.LogError("[Spark Animated AI] Missing dependency. Required: SparkAIUnifiedAssistant, " +
                "SparkAIExplanationRequestBridge, SparkAdaptiveExplanationEngine, " +
                "SparkAdaptiveExplanationPanel, SparkVisualTeachingManager, SparkWorldAnnotationRenderer. " +
                "Install the previous packages first.");
            return;
        }

        Canvas canvas = teacher.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Spark Animated AI] Visual Teaching Canvas missing.", teacher);
            return;
        }

        Transform existing = canvas.transform.Find("SparkAIAnimatedAnswerRoot");
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            Debug.Log("[Spark Animated AI] Existing player selected. No duplicate was created.");
            return;
        }

        GameObject root = UI("SparkAIAnimatedAnswerRoot", canvas.transform);
        Undo.RegisterCreatedObjectUndo(root, "Create Spark AI Animated Answer Player");
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        // Root has no Image/Graphic and does not intercept unrelated UI clicks.

        Button launcher = MakeButton(root.transform, "OpenAnimatedAnswer", "ANIMATE AI", 24, -85, 158, 42, true);
        GameObject card = UI("AnimatedAnswerCard", root.transform);
        Place(card.GetComponent<RectTransform>(), 24, -135, 540, 560, true);
        Image backdrop = card.AddComponent<Image>();
        backdrop.color = Background;
        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = Cyan;
        outline.effectDistance = new Vector2(1f, -1f);

        Text(card.transform, "Brand", "SPARK AI  /  ANIMATED VERIFIED EXPLANATION", 16, Cyan,
            16, -12, 449, 27);
        Button close = MakeButton(card.transform, "Close", "X", 482, -10, 40, 34, false);
        TMP_Text title = Text(card.transform, "StepTitle", "WAITING FOR VERIFIED AI ANSWER", 19, TextWhite,
            17, -55, 505, 36);
        TMP_Text counter = Text(card.transform, "StepCounter", "STEP 0 / 0", 13, Cyan,
            18, -95, 504, 22);
        Block(card.transform, "StepBackground", Surface, 15, -128, 510, 135);
        TMP_Text body = Text(card.transform, "StepBody", "Ask AI on a verified adaptive cue.",
            16, TextWhite, 26, -140, 485, 113);
        Text(card.transform, "AIAnswerTitle", "ASSISTANT ANSWER  ·  ADVISORY TEXT", 13, Cyan,
            18, -274, 480, 26);
        Block(card.transform, "AnswerBackground", Surface, 15, -305, 510, 139);
        TMP_Text answer = Text(card.transform, "AIAnswer", "Waiting for a question through the Natural Explanation panel.",
            14, TextWhite, 27, -312, 483, 123);
        answer.enableAutoSizing = true;
        answer.fontSizeMin = 10;
        answer.fontSizeMax = 14;
        TMP_Text status = Text(card.transform, "Status", "SOURCE: WAITING FOR SOLVER FRAME", 12, Muted,
            16, -448, 510, 36);

        Button previous = MakeButton(card.transform, "Previous", "PREV", 15, -496, 73, 43, false);
        Button next = MakeButton(card.transform, "Next", "NEXT", 93, -496, 73, 43, false);
        Button play = MakeButton(card.transform, "Play", "PLAY", 171, -496, 70, 43, false);
        Button pause = MakeButton(card.transform, "Pause", "PAUSE", 246, -496, 75, 43, false);
        Button show3D = MakeButton(card.transform, "Show3D", "SHOW 3D", 326, -496, 96, 43, false);
        Button route = MakeButton(card.transform, "RouteBoard", "ROUTE", 427, -496, 98, 43, false);

        SparkAIAnimatedAnswerPlayer player = Undo.AddComponent<SparkAIAnimatedAnswerPlayer>(root);
        SerializedObject so = new SerializedObject(player);
        Ref(so, "assistant", assistant);
        Ref(so, "requestBridge", bridge);
        Ref(so, "engine", engine);
        Ref(so, "adaptivePanel", adaptive);
        Ref(so, "teacher", teacher);
        Ref(so, "worldRenderer", renderer);
        Ref(so, "card", card);
        Ref(so, "titleText", title);
        Ref(so, "stepText", counter);
        Ref(so, "bodyText", body);
        Ref(so, "aiAnswerText", answer);
        Ref(so, "statusText", status);
        Ref(so, "previousButton", previous);
        Ref(so, "nextButton", next);
        Ref(so, "playButton", play);
        Ref(so, "pauseButton", pause);
        Ref(so, "worldButton", show3D);
        Ref(so, "routeButton", route);
        so.ApplyModifiedProperties();

        UnityEventTools.AddPersistentListener(launcher.onClick, player.Toggle);
        UnityEventTools.AddPersistentListener(close.onClick, player.Hide);
        UnityEventTools.AddPersistentListener(previous.onClick, player.PreviousStep);
        UnityEventTools.AddPersistentListener(next.onClick, player.NextStep);
        UnityEventTools.AddPersistentListener(play.onClick, player.Play);
        UnityEventTools.AddPersistentListener(pause.onClick, player.PauseOrResume);
        UnityEventTools.AddPersistentListener(show3D.onClick, player.Show3D);
        UnityEventTools.AddPersistentListener(route.onClick, player.ShowRouteBoard);

        card.SetActive(false);
        Selection.activeGameObject = root;
        PrefabUtility.RecordPrefabInstancePropertyModifications(player);
        EditorUtility.SetDirty(player);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
        Debug.Log("[Spark Animated AI] Created read-only animated answer player. " +
            "Publish verified frame > ASK AI in Natural Explanation > ANIMATE AI > PLAY.", root);
    }

    private static GameObject UI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Place(RectTransform rect, float x, float y, float w, float h, bool right)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(right ? 1 : 0, 1);
        rect.pivot = new Vector2(right ? 1 : 0, 1);
        rect.anchoredPosition = new Vector2(right ? -x : x, y);
        rect.sizeDelta = new Vector2(w, h);
    }

    private static Image Block(Transform parent, string name, Color color,
        float x, float y, float w, float h)
    {
        GameObject go = UI(name, parent);
        Place(go.GetComponent<RectTransform>(), x, y, w, h, false);
        Image image = go.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text Text(Transform parent, string name, string value,
        float fontSize, Color color, float x, float y, float w, float h)
    {
        GameObject go = UI(name, parent);
        Place(go.GetComponent<RectTransform>(), x, y, w, h, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = value;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 11;
        tmp.fontSizeMax = fontSize;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.raycastTarget = false;
        tmp.richText = false;
        return tmp;
    }

    private static Button MakeButton(Transform parent, string name, string label,
        float x, float y, float w, float h, bool right)
    {
        GameObject go = UI(name, parent);
        Place(go.GetComponent<RectTransform>(), x, y, w, h, right);
        Image image = go.AddComponent<Image>();
        image.color = ButtonColor;
        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        TMP_Text text = Text(go.transform, "Caption", label, 14, TextWhite, 0, 0, w, h);
        text.alignment = TextAlignmentOptions.Center;
        return button;
    }

    private static void Ref(SerializedObject so, string key, Object value)
    {
        SerializedProperty p = so.FindProperty(key);
        if (p != null) p.objectReferenceValue = value;
        else Debug.LogError("[Spark Animated AI] Missing property " + key);
    }
}
#endif
