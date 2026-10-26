using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.AI;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Presentation-only guided tutorial. Authoritative gameplay systems report facts
    /// through Signal(); the director never moves components, connects wires or solves circuits.
    /// Builds a responsive runtime uGUI overlay; no prefab required.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkTutorialDirector : MonoBehaviour
    {
        [Header("Lessons")]
        [SerializeField] private SparkTutorialDefinition defaultTutorial;
        [SerializeField] private bool autoStart;
        [Header("AI voice")]
        [SerializeField] private SparkAIVoiceController voice;
        [SerializeField] private bool voiceEnabled = true;
        [Header("Camera")]
        [SerializeField] private Camera sceneCamera;
        [Header("Presentation")]
        [SerializeField] private int overlaySortingOrder = 500;
        [SerializeField] private Color accent = new Color(0.03f, 0.9f, 0.95f, 1f);
        [SerializeField] private Color panelColor = new Color(0.035f, 0.07f, 0.12f, 0.95f);
        [SerializeField] private bool enableHotkeys = true;

        private Image[] focusBorders;
        private const float FocusBorderThickness = 3f;

        public event Action<SparkTutorialDefinition, int> StepChanged;
        public event Action<SparkTutorialDefinition> TutorialCompleted;
        public event Action<string> GameplaySignalReceived;
        public bool IsRunning => lesson != null && stepIndex >= 0 && stepIndex < lesson.steps.Count;
        public int CurrentStepIndex => stepIndex;
        public SparkTutorialStep CurrentStep => IsRunning ? lesson.steps[stepIndex] : null;

        private SparkTutorialDefinition lesson;
        private int stepIndex = -1;
        private float stepElapsed;
        private bool hintShown;
        private Canvas canvas;
        private CanvasGroup group;
        private RectTransform panel;
        private RectTransform frame;
        private Image frameImage;
        private Text heading, body, counter, tip;
        private Button nextButton, hintButton, skipButton;
        private Text nextLabel;
        private readonly Dictionary<string, SparkTutorialTarget> targets = new Dictionary<string, SparkTutorialTarget>();
        private bool uiBuilt;
        private float nextTargetSearchTime;
        private Font font;

        private void Awake()
        {
            if (sceneCamera == null) sceneCamera = Camera.main;
            if (voice == null) voice = FindFirstObjectByType<SparkAIVoiceController>();
            BuildUI();
            SetVisible(false);
        }

        private void Start()
        {
            if (autoStart && defaultTutorial != null) Begin(defaultTutorial);
        }

        private void Update()
        {
            if (!IsRunning) return;
            stepElapsed += Time.unscaledDeltaTime;
            if (!hintShown && CurrentStep.autoHintAfterSeconds > 0f &&
                stepElapsed >= CurrentStep.autoHintAfterSeconds) ShowHint();
            if (enableHotkeys && Input.GetKeyDown(KeyCode.F1)) ShowHint();
            if (enableHotkeys && Input.GetKeyDown(KeyCode.F2)) RepeatExplanation();
            AnimateAndPosition();
        }

        public void BeginDefault() { if (defaultTutorial != null) Begin(defaultTutorial); }
        public void Begin(SparkTutorialDefinition definition)
        {
            if (definition == null || definition.steps == null || definition.steps.Count == 0)
            {
                Debug.LogWarning("[Tutorial] Missing/empty definition.", this);
                return;
            }
            lesson = definition;
            stepIndex = 0;
            if (definition.resumeFromCheckpoint)
                stepIndex = Mathf.Clamp(PlayerPrefs.GetInt(SaveKey(definition), 0), 0, definition.steps.Count - 1);
            EnterStep();
        }

        public void Restart()
        {
            if (lesson == null) lesson = defaultTutorial;
            if (lesson == null) return;
            PlayerPrefs.DeleteKey(SaveKey(lesson));
            stepIndex = 0;
            EnterStep();
        }

        public void Close()
        {
            lesson = null;
            stepIndex = -1;
            voice?.StopVoice();
            SetVisible(false);
        }

        public void Signal(string signal)
        {
            if (string.IsNullOrWhiteSpace(signal)) return;
            GameplaySignalReceived?.Invoke(signal);
            if (!IsRunning) return;
            SparkTutorialStep step = CurrentStep;
            if (step.advance == SparkTutorialAdvance.ContinueButton) return;
            if (string.Equals(step.requiredSignal, signal, StringComparison.OrdinalIgnoreCase)) Next();
        }

        public void Continue()
        {
            if (!IsRunning) return;
            if (CurrentStep.advance == SparkTutorialAdvance.GameplaySignal) return;
            Next();
        }

        public void SkipStep()
        {
            if (IsRunning && CurrentStep.allowSkip) Next();
        }

       
private void Next()
{
    if (!IsRunning)
        return;

    stepIndex++;

    if (stepIndex >= lesson.steps.Count)
    {
        SparkTutorialDefinition finished = lesson;

        PlayerPrefs.SetInt(
            SaveKey(finished),
            finished.steps.Count);

        PlayerPrefs.Save();

        // Stop speaking when the lesson finishes.
        voice?.StopVoice();

        // End the active lesson without hiding the UI.
        lesson = null;
        stepIndex = -1;

        // Display completion information.
        heading.text = "TUTORIAL COMPLETED";

        body.text =
            "Excellent! You've completed this lesson.\n\n" +
            "You can now practice these skills in ProjectSpark.";

        counter.text = "LESSON COMPLETE";

        tip.text =
            "You can restart the tutorial anytime.";

        // Hide unnecessary buttons.
        hintButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);

        // Reuse Continue as a close button.
        nextButton.gameObject.SetActive(true);
        nextLabel.text = "CLOSE";

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(Close);

        SetVisible(true);

        // Notify other tutorial systems.
        TutorialCompleted?.Invoke(finished);

        Debug.Log(
            "[Tutorial] Completed: " +
            finished.displayName,
            this);

        return;
    }

    EnterStep();
}


       
private void EnterStep()
{
    // ========================================================
    // VALIDATE TUTORIAL STATE
    // ========================================================

    if (!IsRunning)
        return;

    SparkTutorialStep step = CurrentStep;

    if (step == null)
    {
        Debug.LogWarning(
            "[Tutorial] Cannot enter step: current step is NULL.",
            this);
        return;
    }

    // ========================================================
    // RESET STEP STATE
    // ========================================================

    stepElapsed = 0f;
    hintShown = false;
    nextTargetSearchTime = 0f;

    // ========================================================
    // RESTORE CONTINUE BUTTON
    // ========================================================

    // Important:
    // The completion screen changes Continue into Close.
    // Restore its original behavior for each tutorial step.
    nextButton.onClick.RemoveAllListeners();
    nextButton.onClick.AddListener(Continue);

    nextLabel.text = "CONTINUE";

    // ========================================================
    // UPDATE TUTORIAL CONTENT
    // ========================================================

    heading.text = step.title;
    body.text = step.explanation;

    counter.text =
        (stepIndex + 1) +
        " / " +
        lesson.steps.Count +
        "  ·  " +
        lesson.displayName;

    tip.text = "";

    // ========================================================
    // CONTROL BUTTON VISIBILITY
    // ========================================================

    bool needsSignal =
        step.advance ==
        SparkTutorialAdvance.GameplaySignal;

    // Gameplay-signal steps require the player
    // to perform the actual action.
    nextButton.gameObject.SetActive(!needsSignal);

    // Restore hint availability after completion.
    hintButton.gameObject.SetActive(true);

    // Skip depends on the current lesson step.
    skipButton.gameObject.SetActive(step.allowSkip);

    // ========================================================
    // GAMEPLAY INSTRUCTIONS
    // ========================================================

    if (needsSignal &&
        !string.IsNullOrWhiteSpace(step.requiredSignal))
    {
        tip.text =
            "Complete the highlighted action to continue.";
    }

    // ========================================================
    // SAVE CURRENT CHECKPOINT
    // ========================================================

    if (lesson.resumeFromCheckpoint)
    {
        PlayerPrefs.SetInt(
            SaveKey(lesson),
            stepIndex);

        PlayerPrefs.Save();
    }

    // ========================================================
    // SHOW TUTORIAL PANEL
    // ========================================================

    SetVisible(true);

    // ========================================================
    // AI VOICE NARRATION
    // ========================================================

    if (step.speak && voiceEnabled)
    {
        Speak(step.explanation);
    }

    // ========================================================
    // NOTIFY CINEMATIC GUIDE / GHOST DEMONSTRATOR
    // ========================================================

    StepChanged?.Invoke(lesson, stepIndex);

    Debug.Log(
        "[Tutorial] Entered step " +
        (stepIndex + 1) +
        "/" +
        lesson.steps.Count +
        " | ID=" +
        step.id +
        " | Title=" +
        step.title,
        this);
}

private void ConfigureFocusOutline(Image focusImage)
{
    if (focusImage == null)
        return;

    // Remove the filled rectangle completely.
    focusImage.color = Color.clear;

    // Remove old outlines to avoid duplicates.
    UnityEngine.UI.Outline[] oldOutlines =
        focusImage.GetComponents<UnityEngine.UI.Outline>();

    foreach (UnityEngine.UI.Outline old in oldOutlines)
    {
        if (Application.isPlaying)
            Destroy(old);
        else
            DestroyImmediate(old);
    }

    UnityEngine.UI.Outline outline =
        focusImage.gameObject.AddComponent<
            UnityEngine.UI.Outline>();

    outline.effectColor =
        new Color(0f, 1f, 1f, 0.95f);

    outline.effectDistance =
        new Vector2(3f, -3f);

    outline.useGraphicAlpha = false;

    // Never block mouse interaction with components.
    focusImage.raycastTarget = false;
}


        public void ShowHint()
        {
            if (!IsRunning || string.IsNullOrWhiteSpace(CurrentStep.hint)) return;
            hintShown = true;
            tip.text = "HINT: " + CurrentStep.hint;
            if (voiceEnabled) Speak(CurrentStep.hint);
        }

        public void RepeatExplanation()
        {
            if (IsRunning) Speak(CurrentStep.explanation);
        }

        private void Speak(string message)
        {
            if (voice != null && !string.IsNullOrWhiteSpace(message)) voice.SpeakText(message);
        }

        private static string SaveKey(SparkTutorialDefinition def) => "ProjectSpark.Tutorial." + def.tutorialId;

        private SparkTutorialTarget FindTarget(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            SparkTutorialTarget found;
            if (targets.TryGetValue(key, out found) && found != null && found.isActiveAndEnabled) return found;
            if (Time.unscaledTime < nextTargetSearchTime) return null;
            nextTargetSearchTime = Time.unscaledTime + 0.5f;
            targets.Clear();
            foreach (SparkTutorialTarget t in FindObjectsByType<SparkTutorialTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t != null && !string.IsNullOrWhiteSpace(t.key) && !targets.ContainsKey(t.key)) targets.Add(t.key, t);
            return targets.TryGetValue(key, out found) && found != null && found.isActiveAndEnabled ? found : null;
        }

       
private void AnimateAndPosition()
{
    if (group == null || frame == null)
        return;

    // ========================================================
    // ANIMATE TUTORIAL PANEL
    // ========================================================

    float fade =
        Mathf.Clamp01(stepElapsed * 4f);

    group.alpha = fade;

    float breath =
        1f +
        0.025f *
        Mathf.Sin(Time.unscaledTime * 3f);

    frame.localScale =
        Vector3.one * breath;

    // ========================================================
    // VALIDATE CURRENT STEP
    // ========================================================

    SparkTutorialStep step = CurrentStep;

    if (step == null)
    {
        frame.gameObject.SetActive(false);
        return;
    }

    // ========================================================
    // FIND TUTORIAL TARGET
    // ========================================================

    SparkTutorialTarget locator =
        FindTarget(step.locatorKey);

    RectTransform screen =
        locator != null && locator.screenRect != null
            ? locator.screenRect
            : step.screenTarget;

    Transform world =
        locator != null
            ? locator.WorldAnchor
            : step.worldTarget;

    Vector2 screenPosition = Vector2.zero;

    bool valid = false;

    // ========================================================
    // SCREEN-SPACE TARGET
    // ========================================================

    if (step.focus ==
        SparkTutorialFocus.ScreenRect &&
        screen != null)
    {
        Vector3[] corners =
            new Vector3[4];

        screen.GetWorldCorners(corners);

        Camera uiCamera = null;

        Canvas canvas =
            screen.GetComponentInParent<Canvas>();

        if (canvas != null &&
            canvas.renderMode !=
            RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        Vector2 a =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera, corners[0]);

        Vector2 b =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera, corners[2]);

        screenPosition =
            (a + b) * 0.5f;

        frame.sizeDelta =
            new Vector2(
                Mathf.Abs(b.x - a.x) + 26f,
                Mathf.Abs(b.y - a.y) + 26f);

        valid = true;
    }

    // ========================================================
    // WORLD-SPACE TARGET
    // ========================================================

    else if (step.focus ==
             SparkTutorialFocus.WorldTransform &&
             world != null &&
             sceneCamera != null)
    {
        Vector3 projected =
            sceneCamera.WorldToScreenPoint(
                world.position + step.worldOffset);

        screenPosition =
            new Vector2(
                projected.x,
                projected.y);

        frame.sizeDelta =
            new Vector2(135f, 90f);

        valid = projected.z > 0f;
    }

    // ========================================================
    // SHOW / HIDE FOCUS
    // ========================================================

    frame.gameObject.SetActive(valid);

    if (!valid)
        return;

    // ========================================================
    // MOVE FOCUS FRAME
    // ========================================================

    // Convert screen coordinates to the local
    // coordinates of the frame's UI parent.
    RectTransform parent =
        frame.parent as RectTransform;

    if (parent != null)
    {
        Canvas canvas =
            frame.GetComponentInParent<Canvas>();

        Camera canvasCamera = null;

        if (canvas != null &&
            canvas.renderMode !=
            RenderMode.ScreenSpaceOverlay)
        {
            canvasCamera = canvas.worldCamera;
        }

        if (RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                parent,
                screenPosition,
                canvasCamera,
                out Vector2 localPosition))
        {
            frame.anchoredPosition =
                localPosition;
        }
    }
    else
    {
        frame.position =
            screenPosition;
    }

    // ========================================================
    // UPDATE TRANSPARENT CYAN OUTLINE
    // ========================================================

    UpdateFocusBorders();
}


        private void SetVisible(bool visible)
        {
            if (canvas != null) canvas.gameObject.SetActive(visible);
        }

        private void BuildUI()
        {
            if (uiBuilt) return;
            uiBuilt = true;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject root = new GameObject("Spark_AI_Tutorial_Overlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = overlaySortingOrder;
            root.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            root.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);
            group = root.GetComponent<CanvasGroup>();

            frame = Rect("Focus_Pulse", root.transform, new Vector2(135f, 90f));
            frameImage = frame.gameObject.AddComponent<Image>();
            frameImage.color = new Color(accent.r, accent.g, accent.b, 0.17f);
            frameImage.raycastTarget = false;
            var outline = frame.gameObject.AddComponent<Outline>();
            outline.effectColor = accent;
            outline.effectDistance = new Vector2(3f, 3f);

            panel = Rect("Teacher_Panel", root.transform, new Vector2(880f, 250f));
            panel.anchorMin = new Vector2(0.5f, 0f);
            panel.anchorMax = new Vector2(0.5f, 0f);
            panel.pivot = new Vector2(0.5f, 0f);
            panel.anchoredPosition = new Vector2(0f, 30f);
            Image bg = panel.gameObject.AddComponent<Image>();
            bg.color = panelColor;
            bg.raycastTarget = false;
            Outline border = panel.gameObject.AddComponent<Outline>();
            border.effectColor = accent;
            border.effectDistance = new Vector2(2, 2);

            heading = Label("Title", panel, new Vector2(810, 34), new Vector2(0, 192), 27, FontStyle.Bold, TextAnchor.MiddleLeft);
            body = Label("Explanation", panel, new Vector2(810, 95), new Vector2(0, 95), 22, FontStyle.Normal, TextAnchor.UpperLeft);
            tip = Label("Hint", panel, new Vector2(810, 40), new Vector2(0, 48), 17, FontStyle.Italic, TextAnchor.MiddleLeft);
            counter = Label("Progress", panel, new Vector2(420, 30), new Vector2(-192, 12), 16, FontStyle.Normal, TextAnchor.MiddleLeft);

            nextButton = Button("Continue", panel, new Vector2(150, 38), new Vector2(318, 15), Continue, out nextLabel);
            Text dummy;
            hintButton = Button("Hint", panel, new Vector2(96, 38), new Vector2(126, 15), ShowHint, out dummy);
            skipButton = Button("Skip", panel, new Vector2(85, 38), new Vector2(224, 15), SkipStep, out dummy);
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            return rt;
        }

        private Text Label(string name, RectTransform parent, Vector2 size, Vector2 pos, int fontSize, FontStyle style, TextAnchor align)
        {
            RectTransform rt = Rect(name, parent, size);
            rt.anchoredPosition = pos;
            Text t = rt.gameObject.AddComponent<Text>();
            t.font = font;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = Color.white;
            t.alignment = align;
            t.raycastTarget = false;
            return t;
        }

        private Button Button(string name, RectTransform parent, Vector2 size, Vector2 pos, UnityEngine.Events.UnityAction callback, out Text caption)
        {
            RectTransform rt = Rect(name, parent, size);
            rt.anchoredPosition = pos;
            Image image = rt.gameObject.AddComponent<Image>();
            image.color = new Color(0.025f, 0.25f, 0.32f, 0.98f);
            Button button = rt.gameObject.AddComponent<Button>();
            button.onClick.AddListener(callback);
            caption = Label("Label", rt, size, Vector2.zero, 16, FontStyle.Bold, TextAnchor.MiddleCenter);
            caption.text = name.ToUpperInvariant();
            return button;
        }

        
private void EnsureFocusBorders()
{
    if (frame == null)
        return;

    if (focusBorders != null &&
        focusBorders.Length == 4 &&
        focusBorders[0] != null)
    {
        return;
    }

    // Remove the cyan filled background.
    UnityEngine.UI.Image background =
        frame.GetComponent<UnityEngine.UI.Image>();

    if (background != null)
    {
        background.color = Color.clear;
        background.raycastTarget = false;
    }

    focusBorders = new UnityEngine.UI.Image[4];

    string[] borderNames =
    {
        "TopBorder",
        "BottomBorder",
        "LeftBorder",
        "RightBorder"
    };

    for (int i = 0; i < 4; i++)
    {
        GameObject borderObject = new GameObject(
            borderNames[i],
            typeof(RectTransform),
            typeof(UnityEngine.UI.Image));

        borderObject.transform.SetParent(frame, false);

        RectTransform rect =
            borderObject.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        UnityEngine.UI.Image image =
            borderObject.GetComponent<UnityEngine.UI.Image>();

        image.color = Color.cyan;
        image.raycastTarget = false;

        focusBorders[i] = image;
    }
}

private void UpdateFocusBorders()
{
    if (frame == null)
        return;

    EnsureFocusBorders();

    float width = frame.rect.width;
    float height = frame.rect.height;
    float thickness = 3f;

    RectTransform top =
        focusBorders[0].rectTransform;

    RectTransform bottom =
        focusBorders[1].rectTransform;

    RectTransform left =
        focusBorders[2].rectTransform;

    RectTransform right =
        focusBorders[3].rectTransform;

    top.sizeDelta = new Vector2(width, thickness);
    top.anchoredPosition =
        new Vector2(0f, height * 0.5f);

    bottom.sizeDelta = new Vector2(width, thickness);
    bottom.anchoredPosition =
        new Vector2(0f, -height * 0.5f);

    left.sizeDelta = new Vector2(thickness, height);
    left.anchoredPosition =
        new Vector2(-width * 0.5f, 0f);

    right.sizeDelta = new Vector2(thickness, height);
    right.anchoredPosition =
        new Vector2(width * 0.5f, 0f);

    float pulse =
        0.7f +
        0.3f * Mathf.Sin(Time.unscaledTime * 3f);

    Color color = new Color(0f, 1f, 1f, pulse);

    foreach (UnityEngine.UI.Image image in focusBorders)
    {
        if (image != null)
            image.color = color;
    }
}

    }
}
