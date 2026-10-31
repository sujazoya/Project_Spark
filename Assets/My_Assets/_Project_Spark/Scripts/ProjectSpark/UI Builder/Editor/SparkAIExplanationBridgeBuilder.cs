#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.UI.VisualTeaching;

/// <summary>Safely adds a separate, opt-in AI explanation card to the existing adaptive guide.</summary>
public static class SparkAIExplanationBridgeBuilder
{
    private static readonly Color Deep = new Color(.025f, .075f, .135f, .985f);
    private static readonly Color CardBlue = new Color(.06f, .15f, .24f, 1f);
    private static readonly Color ButtonBlue = new Color(.075f, .39f, .61f, 1f);
    private static readonly Color Cyan = new Color(.18f, .82f, .99f, 1f);
    private static readonly Color White = new Color(.93f, .98f, 1f, 1f);
    private static readonly Color Muted = new Color(.65f, .79f, .87f, 1f);

    [MenuItem("Tools/Project Spark/AI/Add Natural Language Explanation Bridge")]
    public static void Build()
    {
        SparkAdaptiveExplanationEngine engine = null;
        if (Selection.activeGameObject != null)
            engine = Selection.activeGameObject.GetComponentInParent<SparkAdaptiveExplanationEngine>();
        if (engine == null)
            engine = Object.FindFirstObjectByType<SparkAdaptiveExplanationEngine>(FindObjectsInactive.Include);
        if (engine == null)
        {
            Debug.LogError("[Spark AI Explain] Install the Adaptive Explanation Engine first.");
            return;
        }
        SparkAdaptiveExplanationPanel adaptivePanel = engine.GetComponent<SparkAdaptiveExplanationPanel>();
        if (adaptivePanel == null)
        {
            Debug.LogError("[Spark AI Explain] Existing SparkAdaptiveExplanationPanel not found.", engine);
            return;
        }

        Transform parent = engine.transform;
        Transform existing = parent.Find("SparkAINaturalExplanationRoot");
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            Debug.LogWarning("[Spark AI Explain] Already installed; selected existing root.", existing);
            return;
        }

        GameObject root = UI("SparkAINaturalExplanationRoot", parent);
        Undo.RegisterCreatedObjectUndo(root, "Create Spark AI Explanation Bridge");
        Stretch(root.GetComponent<RectTransform>());
        // No fullscreen Image or raycast blocker. Gameplay remains clickable elsewhere.

        Button launcher = MakeButton(root.transform, "AskAILauncher", "ASK SPARK AI", 165f, -140f, 148f, 43f);
        GameObject card = UI("SparkAIExplanationCard", root.transform);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = cardRect.anchorMax = new Vector2(1f, .5f);
        cardRect.pivot = new Vector2(1f, .5f);
        cardRect.sizeDelta = new Vector2(530f, 720f);
        cardRect.anchoredPosition = new Vector2(-24f, 0f);
        card.AddComponent<Image>().color = Deep;
        Outline line = card.AddComponent<Outline>();
        line.effectColor = Cyan;
        line.effectDistance = new Vector2(1.5f, -1.5f);

        Text(card.transform, "Brand", "SPARK AI  /  VERIFIED EXPLANATION", 18f, Cyan, 17f, -12f, 426f, 36f);
        Button close = MakeButton(card.transform, "CloseButton", "X", 468, -12, 44, 35);
        Text(card.transform, "Intro", "AI explains measured facts, not imagined connections.", 13f, Muted, 18f, -60f, 492f, 26f);
        Text(card.transform, "QuestionLabel", "WHAT WOULD YOU LIKE EXPLAINED?", 13f, Cyan, 19f, -99f, 488f, 20f);
        TMP_InputField question = MakeInput(card.transform, "QuestionInput", 18, -122, 494, 48);

        Button ask = MakeButton(card.transform, "AskButton", "ASK AI", 18, -183, 148, 42);
        Button copy = MakeButton(card.transform, "CopyPromptButton", "COPY PROMPT", 179, -183, 151, 42);
        Button demo = MakeButton(card.transform, "PreviewButton", "PREVIEW", 344, -183, 168, 42);

        TMP_Text source = Text(card.transform, "SourceLabel", "SOURCE: NO REQUEST", 12f, Muted, 19, -236, 492, 22);
        Text(card.transform, "AnswerHeader", "AI EXPLANATION", 13, Cyan, 19, -265, 493, 22);
        Backdrop(card.transform, "AnswerBackground", 17, -289, 496, 95);
        TMP_Text answer = Text(card.transform, "Answer", "Select a verified component cue, then press ASK AI.", 17, White, 29, -296, 468, 82);
        Text(card.transform, "ReasonHeader", "WHY THIS READING MATTERS", 13, Cyan, 19, -392, 491, 22);
        Backdrop(card.transform, "ReasonBackground", 17, -417, 496, 84);
        TMP_Text reasoning = Text(card.transform, "Reasoning", "No explanation yet.", 16, White, 29, -423, 468, 72);
        Text(card.transform, "NextHeader", "NEXT CHECK  /  ADVISORY", 13, Cyan, 19, -508, 491, 22);
        Backdrop(card.transform, "NextBackground", 17, -533, 496, 77);
        TMP_Text next = Text(card.transform, "NextCheck", "Verify AI suggestions with the actual simulator.", 16, White, 29, -540, 468, 64);
        Button world = MakeButton(card.transform, "Show3DButton", "SHOW VERIFIED 3D", 18, -621, 238, 41);
        Button route = MakeButton(card.transform, "RouteBoardButton", "VERIFIED ROUTE", 274, -621, 238, 41);
        TMP_Text status = Text(card.transform, "Status", "Waiting for verified solver data.", 13, Muted, 19, -671, 491, 34);

        SparkAIExplanationRequestBridge bridge = Undo.AddComponent<SparkAIExplanationRequestBridge>(root);
        SerializedObject so = new SerializedObject(bridge);
        Ref(so, "engine", engine);
        Ref(so, "adaptivePanel", adaptivePanel);
        Ref(so, "responseCard", card);
        Ref(so, "questionInput", question);
        Ref(so, "statusText", status);
        Ref(so, "sourceText", source);
        Ref(so, "answerText", answer);
        Ref(so, "reasoningText", reasoning);
        Ref(so, "nextCheckText", next);
        Ref(so, "showWorldButton", world);
        Ref(so, "showRouteButton", route);
        so.ApplyModifiedProperties();

        UnityEventTools.AddPersistentListener(launcher.onClick, bridge.Toggle);
        UnityEventTools.AddPersistentListener(close.onClick, bridge.Hide);
        UnityEventTools.AddPersistentListener(ask.onClick, bridge.RequestExplanation);
        UnityEventTools.AddPersistentListener(copy.onClick, bridge.CopyLastPrompt);
        UnityEventTools.AddPersistentListener(demo.onClick, bridge.PreviewVerifiedCue);
        UnityEventTools.AddPersistentListener(world.onClick, bridge.ShowVerifiedComponent);
        UnityEventTools.AddPersistentListener(route.onClick, bridge.ShowVerifiedRoute);


        card.SetActive(false);
        Selection.activeGameObject = root;
        EditorUtility.SetDirty(bridge);
        Debug.Log("[Spark AI Explain] Bridge panel created. Connect Prompt Ready to your existing AI request API.", root);
    }

    private static GameObject UI(string name, Transform parent)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        obj.transform.SetParent(parent, false);
        return obj;
    }
    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
    private static void Place(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }
    private static void Backdrop(Transform parent, string name, float x, float y, float w, float h)
    {
        GameObject obj = UI(name, parent);
        Place(obj.GetComponent<RectTransform>(), x, y, w, h);
        Image image = obj.AddComponent<Image>();
        image.color = CardBlue;
        image.raycastTarget = false;
    }
    private static TMP_Text Text(Transform parent, string name, string value, float size,
        Color color, float x, float y, float w, float h)
    {
        GameObject obj = UI(name, parent);
        Place(obj.GetComponent<RectTransform>(), x, y, w, h);
        TextMeshProUGUI label = obj.AddComponent<TextMeshProUGUI>();
        label.text = value;
        label.fontSize = size;
        label.fontSizeMin = 11;
        label.fontSizeMax = size;
        label.enableAutoSizing = true;
        label.color = color;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.richText = false;
        label.raycastTarget = false;
        return label;
    }
    private static Button MakeButton(Transform parent, string name, string title,
        float x, float y, float w, float h)
    {
        GameObject obj = UI(name, parent);
        Place(obj.GetComponent<RectTransform>(), x, y, w, h);
        Image img = obj.AddComponent<Image>();
        img.color = ButtonBlue;
        Button button = obj.AddComponent<Button>();
        button.targetGraphic = img;
        TMP_Text label = Text(obj.transform, "Text", title, 15, White, 0, 0, w, h);
        label.alignment = TextAlignmentOptions.Center;
        return button;
    }
    private static TMP_InputField MakeInput(Transform parent, string name,
        float x, float y, float w, float h)
    {
        GameObject container = UI(name, parent);
        Place(container.GetComponent<RectTransform>(), x, y, w, h);
        Image bg = container.AddComponent<Image>();
        bg.color = CardBlue;
        TMP_InputField input = container.AddComponent<TMP_InputField>();
        input.characterLimit = 240;
        input.lineType = TMP_InputField.LineType.SingleLine;

        GameObject viewport = UI("Viewport", container.transform);
        RectTransform viewportRt = viewport.GetComponent<RectTransform>();
        Stretch(viewportRt);
        viewportRt.offsetMin = new Vector2(12f, 3f);
        viewportRt.offsetMax = new Vector2(-12f, -3f);

        TextMeshProUGUI entry = (TextMeshProUGUI)Text(viewport.transform, "InputText",
            "", 16, White, 0, 0, w - 24, h - 6);
        entry.enableWordWrapping = false;
        TextMeshProUGUI placeholder = (TextMeshProUGUI)Text(viewport.transform, "Placeholder",
            "Why is this component behaving this way?", 15, Muted, 0, 0, w - 24, h - 6);
        placeholder.fontStyle = FontStyles.Italic;
        input.textViewport = viewportRt;
        input.textComponent = entry;
        input.placeholder = placeholder;
        return input;
    }
    private static void Ref(SerializedObject obj, string field, Object reference)
    {
        SerializedProperty property = obj.FindProperty(field);
        if (property == null) Debug.LogError("[Spark AI Explain] Missing field: " + field);
        else property.objectReferenceValue = reference;
    }
}
#endif
