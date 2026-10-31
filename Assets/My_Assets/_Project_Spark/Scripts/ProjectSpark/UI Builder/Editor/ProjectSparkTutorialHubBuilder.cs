#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSpark.UI;

public static class ProjectSparkTutorialHubBuilder
{
    private static readonly Color Back = new Color(.025f, .075f, .135f, .98f);
    private static readonly Color Surface = new Color(.052f, .15f, .245f, 1f);
    private static readonly Color ButtonColor = new Color(.065f, .24f, .38f, 1f);
    private static readonly Color Cyan = new Color(.22f, .82f, 1f, 1f);
    private static readonly Color White = new Color(.94f, .98f, 1f, 1f);
    private static readonly Color Muted = new Color(.65f, .79f, .87f, 1f);
    private static readonly Color Red = new Color(.95f, .34f, .37f, 1f);
    private static readonly Color Yellow = new Color(1f, .79f, .3f, 1f);

    [MenuItem("Tools/Project Spark/Tutorial/Create Tutorial Hub Drawer")]
    public static void Create()
    {
        Canvas canvas = null;
        if (Selection.activeGameObject != null)
            canvas = Selection.activeGameObject.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
                if (c.name == "Main_HUD") { canvas = c; break; }
        }
        if (canvas == null)
        {
            Debug.LogError("[TutorialHub] Select Main_HUD (or any existing screen-space Canvas) before running the builder.");
            return;
        }
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            Debug.LogError("[TutorialHub] Use a screen-space Canvas for this right-side drawer.", canvas);
            return;
        }
        if (canvas.transform.Find("TutorialHubRoot") != null)
        {
            Debug.LogWarning("[TutorialHub] TutorialHubRoot already exists. No duplicates created.", canvas);
            return;
        }

        GameObject root = UI("TutorialHubRoot", canvas.transform);
        Undo.RegisterCreatedObjectUndo(root, "Create Project Spark Tutorial Hub");
        RectTransform rootRT = root.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = rootRT.offsetMax = Vector2.zero;
        // Root has no raycast graphic: gameplay remains clickable outside drawer.

        GameObject drawer = UI("Drawer", root.transform);
        RectTransform drawerRT = drawer.GetComponent<RectTransform>();
        drawerRT.anchorMin = drawerRT.anchorMax = new Vector2(1, .5f);
        drawerRT.pivot = new Vector2(1, .5f);
        drawerRT.sizeDelta = new Vector2(590, 812);
        drawerRT.anchoredPosition = new Vector2(594, 0);
        Image drawerImage = drawer.AddComponent<Image>();
        drawerImage.color = Back;
        Outline border = drawer.AddComponent<Outline>();
        border.effectColor = Cyan;
        border.effectDistance = new Vector2(2, -2);

        Text(drawer.transform, "Title", "TUTORIAL HUB", 29, White, 23, -16, 420, 42, TextAlignmentOptions.MidlineLeft);
        Text(drawer.transform, "Subtitle", "LEARN  •  BUILD  •  DIAGNOSE", 15, Cyan, 24, -57, 420, 26, TextAlignmentOptions.MidlineLeft);
        Button close = Button(drawer.transform, "CloseButton", "X", 532, -17, 39, 39, Red, 22);

        Panel(drawer.transform, "GoalCard", 16, -102, 558, 108, Surface);
        Text(drawer.transform, "GoalLabel", "CURRENT GOAL", 16, Cyan, 29, -110, 300, 28);
        TMP_Text goal = Text(drawer.transform, "GoalText", "Follow the current circuit objectives", 18, White, 29, -142, 522, 40);
        TMP_Text step = Text(drawer.transform, "StepText", "STEP —", 14, Muted, 380, -111, 164, 26, TextAlignmentOptions.MidlineRight);
        Slider slider = Progress(drawer.transform, 29, -192, 518, 6);

        Panel(drawer.transform, "IssueCard", 16, -225, 273, 100, new Color(.22f, .07f, .105f, .95f));
        Text(drawer.transform, "IssueLabel", "!   DETECTED ISSUE", 15, Red, 27, -235, 240, 25);
        TMP_Text issue = Text(drawer.transform, "IssueText", "No issues reported", 15, White, 27, -265, 245, 49);
        Panel(drawer.transform, "RecommendationCard", 302, -225, 272, 100, Surface);
        Text(drawer.transform, "RecommendationLabel", "RECOMMENDED ACTION", 14, Yellow, 313, -235, 248, 25);
        TMP_Text recommendation = Text(drawer.transform, "RecommendationText", "Open a hint when you need help", 15, White, 313, -265, 245, 49);

        // All tutorial-specific actions are intentionally UnityEvents: no guessed method names.
        Button objectives = Button(drawer.transform, "ObjectivesButton", "◎  OBJECTIVES", 16, -347, 273, 54, ButtonColor);
        Button current = Button(drawer.transform, "CurrentStepButton", "≡  CURRENT STEP", 302, -347, 272, 54, ButtonColor);
        Button hint = Button(drawer.transform, "HintButton", "✦  HINT", 16, -411, 273, 54, ButtonColor);
        Button diagnosis = Button(drawer.transform, "DiagnoseButton", "⌁  DIAGNOSE  [F5]", 302, -411, 272, 54, ButtonColor);
        Button troubleshoot = Button(drawer.transform, "TroubleshooterButton", "⚒  TROUBLESHOOT  [F6]", 16, -475, 273, 54, ButtonColor, 16);
        Button assistant = Button(drawer.transform, "AssistantButton", "✧  ASK SPARK  [F7]", 302, -475, 272, 54, ButtonColor);

        Panel(drawer.transform, "VoiceRow", 16, -539, 558, 48, Surface);
        Text(drawer.transform, "VoiceLabel", "◖))  VOICE HELP", 18, White, 29, -548, 255, 30);
        Toggle voice = VoiceToggle(drawer.transform, 445, -547, 36, 31);
        TMP_Text voiceState = Text(drawer.transform, "VoiceState", "OFF", 16, Cyan, 489, -548, 55, 30);

        Button previous = Button(drawer.transform, "PreviousButton", "‹  PREVIOUS STEP", 16, -602, 273, 50, ButtonColor);
        Button next = Button(drawer.transform, "NextButton", "NEXT STEP  ›", 302, -602, 272, 50, ButtonColor);
        Button reset = Button(drawer.transform, "ResetButton", "↻  RESET TUTORIAL", 16, -660, 273, 50, ButtonColor);
        Button components = Button(drawer.transform, "ComponentsHelpButton", "▣  COMPONENTS HELP", 302, -660, 272, 50, ButtonColor, 17);
        Button bottomClose = Button(drawer.transform, "BottomCloseButton", "✕   CLOSE PANEL", 16, -723, 558, 58, new Color(.32f, .085f, .13f, 1f));

        // Place tab ON LEFT EDGE. Since drawer is moved offscreen right, tab stays visible.
        GameObject tab = UI("DrawerToggle", drawer.transform);
        RectTransform tabRT = tab.GetComponent<RectTransform>();
        tabRT.anchorMin = tabRT.anchorMax = new Vector2(0, .5f);
        tabRT.pivot = new Vector2(1, .5f);
        tabRT.anchoredPosition = Vector2.zero;
        tabRT.sizeDelta = new Vector2(48, 96);
        Image tabImg = tab.AddComponent<Image>();
        tabImg.color = new Color(.06f, .4f, .64f);
        Button tabButton = tab.AddComponent<Button>();
        tabButton.targetGraphic = tabImg;
        TMP_Text arrow = Text(tab.transform, "Arrow", "<", 39, White, 0, 0, 48, 96, TextAlignmentOptions.Center);

        ProjectSparkTutorialHub hub = Undo.AddComponent<ProjectSparkTutorialHub>(root);
        SerializedObject so = new SerializedObject(hub);
        Set(so, "drawer", drawerRT);
        Set(so, "drawerToggle", tabButton);
        Set(so, "drawerArrow", arrow);
        Set(so, "closeButton", close);
        Set(so, "goalText", goal);
        Set(so, "stepText", step);
        Set(so, "progressSlider", slider);
        Set(so, "issueText", issue);
        Set(so, "recommendationText", recommendation);
        Set(so, "voiceStateText", voiceState);
        Set(so, "voiceToggle", voice);
        Set(so, "objectives.button", objectives);
        Set(so, "currentStep.button", current);
        Set(so, "hint.button", hint);
        Set(so, "diagnose.button", diagnosis);
        Set(so, "troubleshooter.button", troubleshoot);
        Set(so, "assistant.button", assistant);
        Set(so, "previousStep.button", previous);
        Set(so, "nextStep.button", next);
        Set(so, "resetTutorial.button", reset);
        Set(so, "componentsHelp.button", components);
        so.ApplyModifiedProperties();

        // Both close buttons run same runtime method; bound after root controller exists.
        UnityEditor.Events.UnityEventTools.AddPersistentListener(bottomClose.onClick, hub.CloseDrawer);
        EditorUtility.SetDirty(bottomClose);
        EditorUtility.SetDirty(hub);
        Selection.activeGameObject = root;
        Debug.Log("[Project Spark] Tutorial Hub generated. Assign tutorial-specific UnityEvents on TutorialHubRoot.", root);
    }

    private static GameObject UI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
    private static void Position(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }
    private static GameObject Panel(Transform parent, string name, float x, float y, float w, float h, Color color)
    {
        GameObject go = UI(name, parent);
        Position(go.GetComponent<RectTransform>(), x, y, w, h);
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return go;
    }
    private static TMP_Text Text(Transform parent, string name, string value, float size, Color color,
        float x, float y, float w, float h, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
    {
        GameObject go = UI(name, parent);
        Position(go.GetComponent<RectTransform>(), x, y, w, h);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = value;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 11;
        tmp.fontSizeMax = size;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;
        return tmp;
    }
    private static Button Button(Transform parent, string name, string title, float x, float y, float w, float h,
        Color color, float size = 18)
    {
        GameObject go = UI(name, parent);
        Position(go.GetComponent<RectTransform>(), x, y, w, h);
        Image img = go.AddComponent<Image>();
        img.color = color;
        Button button = go.AddComponent<Button>();
        button.targetGraphic = img;
        ColorBlock cb = button.colors;
        cb.highlightedColor = new Color(.68f, .95f, 1f);
        cb.pressedColor = new Color(.48f, .78f, .98f);
        button.colors = cb;
        Text(go.transform, "Text", title, size, White, 5, 0, w - 10, h, TextAlignmentOptions.Center);
        return button;
    }
    private static Slider Progress(Transform parent, float x, float y, float w, float h)
    {
        GameObject root = UI("ProgressBar", parent);
        Position(root.GetComponent<RectTransform>(), x, y, w, h);
        Image bg = root.AddComponent<Image>();
        bg.color = new Color(.13f, .31f, .39f);
        bg.raycastTarget = false;
        GameObject fillArea = UI("FillArea", root.transform);
        RectTransform area = fillArea.GetComponent<RectTransform>();
        area.anchorMin = Vector2.zero; area.anchorMax = Vector2.one;
        area.offsetMin = area.offsetMax = Vector2.zero;
        GameObject fill = UI("Fill", fillArea.transform);
        RectTransform fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Cyan; fillImage.raycastTarget = false;
        Slider slider = root.AddComponent<Slider>();
        slider.fillRect = fillRT;
        slider.interactable = false;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0;
        return slider;
    }
    private static Toggle VoiceToggle(Transform parent, float x, float y, float w, float h)
    {
        GameObject root = UI("VoiceToggle", parent);
        Position(root.GetComponent<RectTransform>(), x, y, w, h);
        Image bg = root.AddComponent<Image>();
        bg.color = new Color(.12f, .29f, .4f);
        Toggle toggle = root.AddComponent<Toggle>();
        toggle.targetGraphic = bg;
        GameObject mark = UI("Checkmark", root.transform);
        RectTransform r = mark.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(.2f, .2f); r.anchorMax = new Vector2(.8f, .8f);
        r.offsetMin = r.offsetMax = Vector2.zero;
        Image graphic = mark.AddComponent<Image>();
        graphic.color = Cyan;
        toggle.graphic = graphic;
        toggle.isOn = false;
        return toggle;
    }
    private static void Set(SerializedObject so, string path, Object value)
    {
        SerializedProperty p = so.FindProperty(path);
        if (p != null) p.objectReferenceValue = value;
        else Debug.LogWarning("[TutorialHub Builder] Serialized field missing: " + path);
    }
}
#endif
