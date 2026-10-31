using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectSpark.UI
{
    [DisallowMultipleComponent]
    public sealed class ProjectSparkDualModeToolsPanel : MonoBehaviour
    {
        public enum Tool { Select, Inspect, Move, Rotate, Wire, Measure, Scan, ScreenControl, ResetScene }
        [Serializable] public sealed class ToolAction
        {
            public Tool tool;
            public Button button;
            public Image background;
            public GameObject labelRoot;
            public GameObject shortcutRoot;
            public UnityEvent onActivated = new UnityEvent();
        }

        [Header("Drawer")]
        [SerializeField] private RectTransform drawer;
        [SerializeField] private Button toggleButton;
        [SerializeField] private TMP_Text toggleText;
        [SerializeField] private GameObject expandedHeader;
        [SerializeField] private bool startExpanded;
        [SerializeField] private bool collapseAfterSelection;
        [SerializeField, Min(.05f)] private float animationSeconds = .3f;
        [SerializeField, Min(70f)] private float compactWidth = 86f;
        [SerializeField, Min(150f)] private float expandedWidth = 310f;
        [SerializeField] private bool enableKeyboardShortcuts;
        [SerializeField] private ToolAction[] actions = Array.Empty<ToolAction>();

        [Header("Selection")]
        [SerializeField] private Tool initialTool = Tool.Select;
        [SerializeField] private Color idleColor = new Color(.045f, .19f, .30f, 1f);
        [SerializeField] private Color selectedColor = new Color(.035f, .48f, .68f, 1f);
        [SerializeField] private UnityEvent onResetSceneRequested = new UnityEvent();

        private Coroutine widthRoutine;
        private bool expanded;
        private Tool activeTool;
        private readonly System.Collections.Generic.List<UnityAction> handlers = new System.Collections.Generic.List<UnityAction>();
        public bool IsExpanded => expanded;
        public Tool ActiveTool => activeTool;

        private void Awake()
        {
            if (drawer == null) drawer = transform as RectTransform;
            if (toggleButton != null) toggleButton.onClick.AddListener(Toggle);
            if (actions != null)
            {
                for (int i = 0; i < actions.Length; i++)
                {
                    ToolAction action = actions[i];
                    if (action == null || action.button == null) { handlers.Add(null); continue; }
                    Tool selected = action.tool;
                    UnityAction callback = () => Activate(selected);
                    action.button.onClick.AddListener(callback);
                    handlers.Add(callback);
                }
            }
            activeTool = initialTool;
            expanded = startExpanded;
            ApplyWidth(expanded ? expandedWidth : compactWidth);
            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (toggleButton != null) toggleButton.onClick.RemoveListener(Toggle);
            if (actions == null) return;
            for (int i = 0; i < actions.Length && i < handlers.Count; i++)
                if (actions[i] != null && actions[i].button != null && handlers[i] != null)
                    actions[i].button.onClick.RemoveListener(handlers[i]);
        }

        private void OnDisable()
        {
            if (widthRoutine == null) return;
            StopCoroutine(widthRoutine);
            widthRoutine = null;
            ApplyWidth(expanded ? expandedWidth : compactWidth);
        }

        private void Update()
        {
            /*if (!enableKeyboardShortcuts) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) Activate(Tool.Select);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) Activate(Tool.Inspect);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) Activate(Tool.Move);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) Activate(Tool.Rotate);
            else if (Input.GetKeyDown(KeyCode.Alpha5)) Activate(Tool.Wire);
            else if (Input.GetKeyDown(KeyCode.Alpha6)) Activate(Tool.Measure);
            else if (Input.GetKeyDown(KeyCode.Alpha7)) Activate(Tool.Scan);
            else if (Input.GetKeyDown(KeyCode.Alpha8)) Activate(Tool.ScreenControl);
            else if (Input.GetKeyDown(KeyCode.R)) Activate(Tool.ResetScene);*/
        }

        public void Toggle() => SetExpanded(!expanded);
        public void Expand() => SetExpanded(true);
        public void Collapse() => SetExpanded(false);

        public void SetExpanded(bool value)
        {
            expanded = value;
            if (widthRoutine != null) StopCoroutine(widthRoutine);
            if (!isActiveAndEnabled) { ApplyWidth(expanded ? expandedWidth : compactWidth); UpdateVisuals(); return; }
            widthRoutine = StartCoroutine(AnimateWidth(expanded ? expandedWidth : compactWidth));
            UpdateVisuals();
        }

        private IEnumerator AnimateWidth(float target)
        {
            float initial = drawer != null ? drawer.sizeDelta.x : target;
            float time = 0f;
            while (time < animationSeconds)
            {
                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / animationSeconds);
                t = t * t * (3f - 2f * t);
                ApplyWidth(Mathf.Lerp(initial, target, t));
                yield return null;
            }
            ApplyWidth(target);
            widthRoutine = null;
        }

        private void ApplyWidth(float value)
        {
            if (drawer == null) return;
            Vector2 size = drawer.sizeDelta;
            size.x = value;
            drawer.sizeDelta = size;
        }

        private void UpdateVisuals()
        {
            if (toggleText != null) toggleText.text = expanded ? ">" : "<";
            if (expandedHeader != null) expandedHeader.SetActive(expanded);
            if (actions == null) return;
            foreach (ToolAction action in actions)
            {
                if (action == null) continue;
                if (action.labelRoot != null) action.labelRoot.SetActive(expanded);
                if (action.shortcutRoot != null) action.shortcutRoot.SetActive(expanded);
                if (action.background != null) action.background.color = action.tool == activeTool ? selectedColor : idleColor;
            }
        }

        public void Activate(Tool tool)
        {
            if (tool != Tool.ResetScene) activeTool = tool;
            if (actions != null)
                foreach (ToolAction action in actions)
                    if (action != null && action.tool == tool) { action.onActivated?.Invoke(); break; }
            if (tool == Tool.ResetScene) onResetSceneRequested?.Invoke();
            UpdateVisuals();
            if (collapseAfterSelection) Collapse();
        }

        public void SelectTool() => Activate(Tool.Select);
        public void InspectTool() => Activate(Tool.Inspect);
        public void MoveTool() => Activate(Tool.Move);
        public void RotateTool() => Activate(Tool.Rotate);
        public void WireTool() => Activate(Tool.Wire);
        public void MeasureTool() => Activate(Tool.Measure);
        public void ScanTool() => Activate(Tool.Scan);
        public void ScreenControlTool() => Activate(Tool.ScreenControl);
        public void RequestResetScene() => Activate(Tool.ResetScene);
    }
}
