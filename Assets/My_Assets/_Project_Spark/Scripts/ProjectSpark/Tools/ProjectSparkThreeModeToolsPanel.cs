using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectSpark.UI
{
    [DisallowMultipleComponent]
    public sealed class ProjectSparkThreeModeToolsPanel : MonoBehaviour
    {
        public enum DisplayMode { Hidden = 0, Icons = 1, Complete = 2 }

        [Header("Generated UI")]
        [SerializeField] private RectTransform drawer;
        [SerializeField] private Button modeButton;
        [SerializeField] private TMP_Text modeArrow;
        [SerializeField] private GameObject expandedHeader;
        [SerializeField] private GameObject expandedFooter;
        [SerializeField] private Button[] toolButtons = new Button[9];
        [SerializeField] private TMP_Text[] toolNames = new TMP_Text[9];
        [SerializeField] private TMP_Text[] shortcutLabels = new TMP_Text[9];
        [SerializeField] private Image[] toolBackgrounds = new Image[9];
        [SerializeField] private TMP_Text selectedToolText;

        [Header("Three Modes")]
        [SerializeField] private DisplayMode startMode = DisplayMode.Hidden;
        [SerializeField, Min(48f)] private float iconsWidth = 94f;
        [SerializeField, Min(180f)] private float completeWidth = 324f;
        [SerializeField, Min(0.05f)] private float animationSeconds = 0.32f;
        [SerializeField, Min(0f)] private float rightInset = 14f;
        [SerializeField] private bool autoCollapseToIconsOnToolClick = false;

        [Header("Optional Keyboard Shortcuts")]
        [SerializeField] private bool enableKeyboardShortcuts = false;
        [SerializeField] private bool ignoreShortcutsWhenTyping = true;

        [Header("Tool Selection")]
        [SerializeField, Range(0, 8)] private int initialSelectedTool = 0;
        [SerializeField] private Color normalColor = new Color(.055f, .19f, .31f, 1f);
        [SerializeField] private Color selectedColor = new Color(.045f, .48f, .66f, 1f);

        [Header("Tool Actions — Assign Existing Gameplay Methods")]
        [SerializeField] private UnityEvent onSelect = new UnityEvent();
        [SerializeField] private UnityEvent onInspect = new UnityEvent();
        [SerializeField] private UnityEvent onMove = new UnityEvent();
        [SerializeField] private UnityEvent onRotate = new UnityEvent();
        [SerializeField] private UnityEvent onWire = new UnityEvent();
        [SerializeField] private UnityEvent onMeasure = new UnityEvent();
        [SerializeField] private UnityEvent onScan = new UnityEvent();
        [SerializeField] private UnityEvent onScreenControl = new UnityEvent();
        [SerializeField] private UnityEvent onResetScene = new UnityEvent();

        public DisplayMode CurrentMode { get; private set; }
        public int SelectedTool { get; private set; }
        public event Action<DisplayMode> ModeChanged;
        public event Action<int> ToolSelected;

        private Coroutine motion;
        private int direction = 1;
        private bool initialized;
        private readonly UnityAction[] clickActions = new UnityAction[9];

        private static readonly string[] Names = {
            "Select", "Inspect", "Move", "Rotate", "Wire",
            "Measure", "Scan", "Screen Control", "Reset Scene"
        };

        private void Awake()
        {
            if (drawer == null) drawer = transform as RectTransform;
            if (modeButton != null) modeButton.onClick.AddListener(NextMode);
            for (int i = 0; i < 9; i++)
            {
                int index = i;
                clickActions[i] = () => ActivateTool(index);
                if (toolButtons != null && i < toolButtons.Length && toolButtons[i] != null)
                    toolButtons[i].onClick.AddListener(clickActions[i]);
            }
            SelectedTool = Mathf.Clamp(initialSelectedTool, 0, 8);
        }

        private void Start()
        {
            Canvas.ForceUpdateCanvases();
            initialized = true;
            SetModeImmediate(startMode);
            UpdateSelectedVisuals();
        }

        private void OnDisable()
        {
            if (motion != null) { StopCoroutine(motion); motion = null; }
        }

        private void OnDestroy()
        {
            if (modeButton != null) modeButton.onClick.RemoveListener(NextMode);
            for (int i = 0; i < 9; i++)
                if (clickActions[i] != null && toolButtons != null && i < toolButtons.Length && toolButtons[i] != null)
                    toolButtons[i].onClick.RemoveListener(clickActions[i]);
        }

        private void Update()
        {
            if (!enableKeyboardShortcuts || !initialized) return;
            if (ignoreShortcutsWhenTyping && UnityEngine.EventSystems.EventSystem.current != null)
            {
                var selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
                if (selected != null && (selected.GetComponent<TMP_InputField>() != null ||
                                         selected.GetComponent<InputField>() != null)) return;
            }
           /* for (int i = 0; i < 8; i++)
            {
                if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
                { ActivateTool(i); return; }
            }
            if (Input.GetKeyDown(KeyCode.R)) ActivateTool(8);*/
        }

        // Single tab cycles: Hidden -> Icons -> Complete -> Icons -> Hidden.
        public void NextMode()
        {
            int next = (int)CurrentMode + direction;
            if (next >= 2) { next = 2; direction = -1; }
            else if (next <= 0) { next = 0; direction = 1; }
            ChangeMode((DisplayMode)next);
        }

        public void ShowIcons() => ChangeMode(DisplayMode.Icons);
        public void ShowComplete() => ChangeMode(DisplayMode.Complete);
        public void HideDrawer() => ChangeMode(DisplayMode.Hidden);
        public void CycleMode() => NextMode();

        public void ChangeMode(DisplayMode mode)
        {
            if (drawer == null) return;
            mode = (DisplayMode)Mathf.Clamp((int)mode, 0, 2);
            CurrentMode = mode;
            if (mode == DisplayMode.Hidden) direction = 1;
            else if (mode == DisplayMode.Complete) direction = -1;

            if (motion != null) { StopCoroutine(motion); motion = null; }
            // Keep labels visible throughout expansion, hiding them only at the end of collapse.
            if (mode == DisplayMode.Complete) SetExpandedElements(true);
            UpdateArrow();
            ModeChanged?.Invoke(mode);
            if (!isActiveAndEnabled || !initialized) SetModeImmediate(mode);
            else motion = StartCoroutine(AnimateTo(mode));
        }

        public void SetModeImmediate(DisplayMode mode)
        {
            if (drawer == null) return;
            if (motion != null) { StopCoroutine(motion); motion = null; }
            CurrentMode = mode;
            if (mode == DisplayMode.Hidden) direction = 1;
            if (mode == DisplayMode.Complete) direction = -1;
            SetDrawerWidth(mode == DisplayMode.Complete ? completeWidth : iconsWidth);
            SetDrawerX(TargetX(mode));
            SetExpandedElements(mode == DisplayMode.Complete);
            UpdateArrow();
        }

        private float TargetX(DisplayMode mode) =>
            mode == DisplayMode.Hidden ? iconsWidth : -rightInset;

        private IEnumerator AnimateTo(DisplayMode target)
        {
            float fromWidth = drawer.rect.width;
            float toWidth = target == DisplayMode.Complete ? completeWidth : iconsWidth;
            float fromX = drawer.anchoredPosition.x;
            float toX = TargetX(target);
            float elapsed = 0f;
            while (elapsed < animationSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / animationSeconds);
                t = t * t * (3f - 2f * t);
                SetDrawerWidth(Mathf.Lerp(fromWidth, toWidth, t));
                SetDrawerX(Mathf.Lerp(fromX, toX, t));
                yield return null;
            }
            SetDrawerWidth(toWidth);
            SetDrawerX(toX);
            SetExpandedElements(target == DisplayMode.Complete);
            motion = null;
        }

        private void SetDrawerWidth(float value)
        {
            var rect = drawer.sizeDelta;
            rect.x = value;
            drawer.sizeDelta = rect;
        }

        private void SetDrawerX(float value)
        {
            var point = drawer.anchoredPosition;
            point.x = value;
            drawer.anchoredPosition = point;
        }

        private void SetExpandedElements(bool expanded)
        {
            if (expandedHeader != null) expandedHeader.SetActive(expanded);
            if (expandedFooter != null) expandedFooter.SetActive(expanded);
            for (int i = 0; i < 9; i++)
            {
                if (toolNames != null && i < toolNames.Length && toolNames[i] != null)
                    toolNames[i].gameObject.SetActive(expanded);
                if (shortcutLabels != null && i < shortcutLabels.Length && shortcutLabels[i] != null)
                    shortcutLabels[i].gameObject.SetActive(expanded);
            }
        }

        private void UpdateArrow()
        {
            if (modeArrow != null) modeArrow.text = direction > 0 ? "<" : ">";
        }

        public void ActivateTool(int index)
        {
            if (index < 0 || index > 8) return;
            SelectedTool = index;
            UpdateSelectedVisuals();
            ToolSelected?.Invoke(index);
            switch (index)
            {
                case 0: onSelect?.Invoke(); break;
                case 1: onInspect?.Invoke(); break;
                case 2: onMove?.Invoke(); break;
                case 3: onRotate?.Invoke(); break;
                case 4: onWire?.Invoke(); break;
                case 5: onMeasure?.Invoke(); break;
                case 6: onScan?.Invoke(); break;
                case 7: onScreenControl?.Invoke(); break;
                case 8: onResetScene?.Invoke(); break;
            }
            if (autoCollapseToIconsOnToolClick && CurrentMode == DisplayMode.Complete)
                ShowIcons();
        }

        public void SelectToolVisualOnly(int index)
        {
            if (index < 0 || index > 8) return;
            SelectedTool = index;
            UpdateSelectedVisuals();
        }

        private void UpdateSelectedVisuals()
        {
            for (int i = 0; i < 9; i++)
                if (toolBackgrounds != null && i < toolBackgrounds.Length && toolBackgrounds[i] != null)
                    toolBackgrounds[i].color = i == SelectedTool ? selectedColor : normalColor;
            if (selectedToolText != null) selectedToolText.text = "ACTIVE: " + Names[SelectedTool].ToUpperInvariant();
        }
    }
}
