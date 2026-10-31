#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.UI.VisualTeaching;

public static class SparkAdaptiveExplanationBuilder
{
    private static readonly Color Deep = new Color(.024f, .075f, .13f, .98f);
    private static readonly Color Panel = new Color(.06f, .15f, .24f, 1f);
    private static readonly Color Blue = new Color(.07f, .39f, .60f, 1f);
    private static readonly Color Cyan = new Color(.18f, .82f, .99f, 1f);
    private static readonly Color White = new Color(.93f, .98f, 1f, 1f);
    private static readonly Color Muted = new Color(.63f, .78f, .87f, 1f);

    [MenuItem("Tools/Project Spark/AI/Create Adaptive Explanation Panel")]
    public static void Create()
    {
        SparkVisualTeachingManager teacher = null;
        if (Selection.activeGameObject != null)
            teacher = Selection.activeGameObject.GetComponentInParent<SparkVisualTeachingManager>();
        if (teacher == null)
            teacher = Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
        if (teacher == null)
        {
            Debug.LogError("[Spark Adaptive] Create the AAA Visual Teaching system first.");
            return;
        }

        Canvas canvas = teacher.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Spark Adaptive] Teaching Canvas not found.", teacher);
            return;
        }
        if (canvas.transform.Find("SparkAdaptiveExplanationRoot") != null)
        {
            Selection.activeGameObject = canvas.transform.Find("SparkAdaptiveExplanationRoot").gameObject;
            Debug.LogWarning("[Spark Adaptive] Panel already exists. Selected existing panel.");
            return;
        }

        SparkVerifiedElectricalPublisher publisher =
            teacher.GetComponent<SparkVerifiedElectricalPublisher>();
        if (publisher == null)
            publisher = Object.FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
        if (publisher == null)
        {
            Debug.LogError("[Spark Adaptive] Solver Telemetry Publisher not found. Install the Solver Link first.", teacher);
            return;
        }
        SparkVisualTeachingLiveBridge bridge = teacher.GetComponent<SparkVisualTeachingLiveBridge>();
        if (bridge == null)
            bridge = Object.FindFirstObjectByType<SparkVisualTeachingLiveBridge>(FindObjectsInactive.Include);

        GameObject root = UI("SparkAdaptiveExplanationRoot", canvas.transform);
        Undo.RegisterCreatedObjectUndo(root, "Create Adaptive Explanation Panel");
        Stretch(root.GetComponent<RectTransform>());
        // No root Image: does not intercept gameplay input outside visible buttons.

        Button launcher = MakeButton(root.transform, "AdaptiveLauncher", "AI GUIDE", 18, -140, 136, 43);
        GameObject card = UI("AdaptiveExplanationCard", root.transform);
        Place(card.GetComponent<RectTransform>(), 18, -195, 466, 440);
        card.AddComponent<Image>().color = Deep;
        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = Cyan;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        GameObject severityGo = UI("SeverityStrip", card.transform);
        Place(severityGo.GetComponent<RectTransform>(), 0, 0, 7, 440);
        Image severity = severityGo.AddComponent<Image>();
        severity.color = Cyan; severity.raycastTarget = false;

        Text(card.transform, "Brand", "SPARK AI / ADAPTIVE EXPLANATION", 16, Cyan, 22, -14, 367, 30);
        Button close = MakeButton(card.transform, "CloseButton", "X", 408, -12, 42, 34);
        Image cardHeader = RectImage(card.transform, "HeaderBackground", Panel, 18, -59, 430, 48);
        cardHeader.transform.SetAsFirstSibling();
        TMP_Text title = Text(card.transform, "IssueTitle", "AWAITING VERIFIED SOLVER DATA", 19, White, 25, -67, 407, 32);
        TMP_Text component = Text(card.transform, "Component", "NO COMPONENT YET", 15, Cyan, 24, -124, 418, 25);
        RectImage(card.transform, "ExplanationBackground", Panel, 18, -159, 430, 112);
        TMP_Text explanation = Text(card.transform, "Explanation", "Publish a solver frame to begin.", 17, White, 30, -171, 403, 92);
        Text(card.transform, "RecommendedLabel", "RECOMMENDED NEXT ACTION", 13, Cyan, 22, -282, 419, 20);
        TMP_Text action = Text(card.transform, "RecommendedAction", "Awaiting verified measurements.", 15, Muted, 22, -308, 420, 43);
        TMP_Text source = Text(card.transform, "Source", "SOURCE: NO FRAME", 11, Muted, 22, -356, 350, 19);
        TMP_Text position = Text(card.transform, "CuePosition", "0 / 0", 13, Cyan, 385, -354, 60, 22);
        Button prev = MakeButton(card.transform, "PreviousButton", "PREV", 20, -386, 73, 40);
        Button next = MakeButton(card.transform, "NextButton", "NEXT", 99, -386, 73, 40);
        Button world = MakeButton(card.transform, "ShowInWorldButton", "SHOW 3D", 178, -386, 122, 40);
        Button path = MakeButton(card.transform, "ShowPathButton", "ROUTE BOARD", 306, -386, 139, 40);

        SparkAdaptiveExplanationEngine engine = Undo.AddComponent<SparkAdaptiveExplanationEngine>(root);
        SparkAdaptiveExplanationPanel panel = Undo.AddComponent<SparkAdaptiveExplanationPanel>(root);
        SerializedObject engineSO = new SerializedObject(engine);
        Ref(engineSO, "publisher", publisher);
        engineSO.ApplyModifiedProperties();

        SerializedObject panelSO = new SerializedObject(panel);
        Ref(panelSO, "engine", engine);
        Ref(panelSO, "teacher", teacher);
        Ref(panelSO, "liveBridge", bridge);
        Ref(panelSO, "card", card);
        Ref(panelSO, "headerText", title);
        Ref(panelSO, "componentText", component);
        Ref(panelSO, "explanationText", explanation);
        Ref(panelSO, "actionText", action);
        Ref(panelSO, "sourceText", source);
        Ref(panelSO, "positionText", position);
        Ref(panelSO, "severityStrip", severity);
        Ref(panelSO, "worldButton", world);
        Ref(panelSO, "pathButton", path);
        panelSO.ApplyModifiedProperties();

        UnityEventTools.AddPersistentListener(launcher.onClick, panel.ToggleCard);
        UnityEventTools.AddPersistentListener(close.onClick, panel.CloseCard);
        UnityEventTools.AddPersistentListener(prev.onClick, panel.PreviousCue);
        UnityEventTools.AddPersistentListener(next.onClick, panel.NextCue);
        UnityEventTools.AddPersistentListener(world.onClick, panel.ShowCurrentInWorld);
        UnityEventTools.AddPersistentListener(path.onClick, panel.ShowVerifiedRouteWhiteboard);
        card.SetActive(false);
        Selection.activeGameObject = root;
        EditorUtility.SetDirty(engine);
        EditorUtility.SetDirty(panel);
        Debug.Log("[Spark Adaptive] Installed. Launcher is always active; frame reception is automatic. " +
                  "Actual verified results must be published by your solver.", root);
    }

    private static GameObject UI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private static void Place(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }

    private static Image RectImage(Transform parent, string name, Color color,
        float x, float y, float w, float h)
    {
        GameObject go = UI(name, parent);
        Place(go.GetComponent<RectTransform>(), x, y, w, h);
        Image image = go.AddComponent<Image>();
        image.color = color; image.raycastTarget = false;
        return image;
    }

    private static TMP_Text Text(Transform parent, string name, string value,
        float size, Color color, float x, float y, float w, float h)
    {
        GameObject go = UI(name, parent);
        Place(go.GetComponent<RectTransform>(), x, y, w, h);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = value; tmp.fontSize = size; tmp.color = color;
        tmp.enableAutoSizing = true; tmp.fontSizeMin = 11; tmp.fontSizeMax = size;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static Button MakeButton(Transform parent, string name, string value,
        float x, float y, float w, float h)
    {
        GameObject go = UI(name, parent);
        Place(go.GetComponent<RectTransform>(), x, y, w, h);
        Image image = go.AddComponent<Image>();
        image.color = Blue;
        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        TMP_Text txt = Text(go.transform, "Text", value, 15, White, 0, 0, w, h);
        txt.alignment = TextAlignmentOptions.Center;
        return button;
    }

    private static void Ref(SerializedObject obj, string property, Object value)
    {
        SerializedProperty prop = obj.FindProperty(property);
        if (prop == null) Debug.LogError("[Spark Adaptive] Missing serialized property: " + property);
        else prop.objectReferenceValue = value;
    }
}
#endif
