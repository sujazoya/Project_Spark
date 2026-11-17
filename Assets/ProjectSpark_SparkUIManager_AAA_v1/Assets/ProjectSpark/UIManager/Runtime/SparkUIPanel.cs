using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UIManagement
{
    public enum SparkUILayer { Background = 0, HUD = 10, Main = 20, Tool = 30, Overlay = 40, Popup = 50, Modal = 60, Fullscreen = 70, System = 80 }
    public enum SparkUIPanelState { Closed, Opening, Open, Minimized, Closing }
    public enum SparkUIClosePolicy { None, SameGroup, OtherMainPanels }

    /// <summary>Attach to the EXISTING top-level panel. Does not replace any feature scripts.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkUIPanel : MonoBehaviour
    {
        [Header("Registration")]
        [SerializeField] private string panelId = "Panel";
        [SerializeField] private string displayName = "Panel";
        [SerializeField] private SparkUILayer layer = SparkUILayer.Main;
        [SerializeField] private int priority;
        [SerializeField] private string exclusiveGroup = "";
        [SerializeField] private SparkUIClosePolicy closePolicy = SparkUIClosePolicy.SameGroup;
        [Header("Behavior")]
        [SerializeField] private bool visibleOnStart;
        [SerializeField] private bool blocksGameplayInput = true;
        [SerializeField] private bool locksFocus;
        [SerializeField] private bool allowMinimize = true;
        [SerializeField] private bool animate = true;
        [SerializeField, Range(0f, 1f)] private float fadeSeconds = .18f;
        [SerializeField] private bool manageCanvasSorting = true;
        [SerializeField] private bool deactivateWhenClosed = true;
        [Header("Optional existing components")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Canvas ownCanvas;

        internal Action<bool> NativeVisibility { get; private set; }
        internal Func<bool> NativeIsVisible { get; private set; }

        internal void BindNative(string id, string label, SparkUILayer panelLayer, string group,
            bool blockGameplay, Action<bool> apply, Func<bool> read)
        {
            panelId=id;
            displayName=label;
            layer=panelLayer;
            exclusiveGroup=group;
            blocksGameplayInput=blockGameplay;
            manageCanvasSorting=false;
            animate=false;
            NativeVisibility=apply;
            NativeIsVisible=read;
        }

        public string Id { get { return panelId; } }
        public string DisplayName { get { return string.IsNullOrWhiteSpace(displayName) ? panelId : displayName; } }
        public SparkUILayer Layer { get { return layer; } }
        public int Priority { get { return priority; } }
        public string ExclusiveGroup { get { return exclusiveGroup; } }
        public SparkUIClosePolicy ClosePolicy { get { return closePolicy; } }
        public bool VisibleOnStart { get { return visibleOnStart; } }
        public bool BlocksGameplayInput { get { return blocksGameplayInput; } }
        public bool LocksFocus { get { return locksFocus; } }
        public bool AllowMinimize { get { return allowMinimize; } }
        public bool AlwaysVisible { get; internal set; }
        public bool Animate { get { return animate; } }
        public float FadeSeconds { get { return fadeSeconds; } }
        public bool DeactivateWhenClosed { get { return deactivateWhenClosed; } }
        public SparkUIPanelState State { get; internal set; } = SparkUIPanelState.Closed;
        public bool IsVisible { get { return State == SparkUIPanelState.Open || State == SparkUIPanelState.Opening; } }
        public event Action<SparkUIPanel, SparkUIPanelState> StateChanged;
        internal CanvasGroup Group
        {
            get
            {
                if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
                if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
                return canvasGroup;
            }
        }
        internal void SetState(SparkUIPanelState state)
        {
            State = state;
            if (StateChanged != null) StateChanged(this, state);
        }
        internal void SetSort(int sortOrder)
        {
            if (!manageCanvasSorting) return;
            if (!ownCanvas) ownCanvas = GetComponent<Canvas>();
            if (!ownCanvas) ownCanvas = gameObject.AddComponent<Canvas>();
            ownCanvas.overrideSorting = true;
            ownCanvas.sortingOrder = Mathf.Clamp(sortOrder, -32768, 32767);
            if (!GetComponent<GraphicRaycaster>()) gameObject.AddComponent<GraphicRaycaster>();
        }
        internal void SetIdEditor(string id) { panelId = id; displayName = id; }
        private void OnValidate()
        {
            fadeSeconds = Mathf.Max(0f, fadeSeconds);
            priority = Mathf.Clamp(priority, -99, 99);
        }
    }
}
