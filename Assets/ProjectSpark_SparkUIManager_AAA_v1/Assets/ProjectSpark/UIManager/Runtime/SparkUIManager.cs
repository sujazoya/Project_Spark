using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.UIManagement
{
    /// <summary>Registry, deterministic ordering, focus, exclusive groups, transitions, and Escape routing.</summary>
    [DefaultExecutionOrder(-400)]
    [DisallowMultipleComponent]
    public sealed class SparkUIManager : MonoBehaviour
    {
        [SerializeField] private List<SparkUIPanel> registeredPanels = new List<SparkUIPanel>();
        [SerializeField] private bool autoDiscoverChildren = true;
        [SerializeField] private bool escapeClosesTopPanel = true;
        [SerializeField] private int sortingBase = 100;
        [SerializeField] private bool verboseLogging;
        private readonly Dictionary<string, SparkUIPanel> lookup = new Dictionary<string, SparkUIPanel>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<SparkUIPanel, Coroutine> running = new Dictionary<SparkUIPanel, Coroutine>();
        private readonly List<SparkUIPanel> openOrder = new List<SparkUIPanel>();
        private readonly List<string> history = new List<string>();
        private SparkUIPanel focused;
        private float nextNativePoll;

        public static SparkUIManager Instance { get; private set; }
        public event Action Changed;
        public event Action<string> LogEvent;
        public IReadOnlyList<SparkUIPanel> Panels { get { return registeredPanels; } }
        public IReadOnlyList<string> History { get { return history; } }
        public string FocusedId { get { return focused ? focused.Id : "None"; } }
        public bool IsModalOpen { get { return registeredPanels.Any(p => p && p.IsVisible && (p.Layer == SparkUILayer.Modal || p.Layer == SparkUILayer.Fullscreen)); } }
        public bool IsGameplayInputBlocked { get { return SparkUIMonitorDashboard.IsConsoleVisible || registeredPanels.Any(p => p && p.IsVisible && (p.BlocksGameplayInput || p.Layer == SparkUILayer.Modal || p.Layer == SparkUILayer.Fullscreen)); } }
        public bool IsPointerOverUI { get { return EventSystem.current && EventSystem.current.IsPointerOverGameObject(); } }
        public int OpenCount { get { return registeredPanels.Count(p => p && p.IsVisible); } }
        public int TransitionCount { get { return running.Count; } }

        private void Awake()
        {
            if (Instance && Instance != this) { Debug.LogError("[SparkUI] Duplicate manager detected; disabling duplicate.", this); enabled = false; return; }
            Instance = this;
            Initialize();
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
        private void Update()
        {
            if(Time.unscaledTime >= nextNativePoll)
            {
                nextNativePoll=Time.unscaledTime+.18f;
                ObserveNativePanels();
            }
            if (!escapeClosesTopPanel) return;
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape)) Back();
#endif
        }
        [ContextMenu("Rebuild panel registry")]
        public void Initialize()
        {
            if (autoDiscoverChildren)
            {
                foreach (var p in GetComponentsInChildren<SparkUIPanel>(true))
                    if (p && !registeredPanels.Contains(p)) registeredPanels.Add(p);
            }
            lookup.Clear(); openOrder.Clear(); running.Clear(); history.Clear(); focused = null;
            registeredPanels.RemoveAll(p => !p);
            foreach (var p in registeredPanels)
            {
                if (string.IsNullOrWhiteSpace(p.Id)) { Debug.LogError("[SparkUI] Empty panel ID: " + p.name, p); continue; }
                if (lookup.ContainsKey(p.Id)) { Debug.LogError("[SparkUI] Duplicate panel ID: " + p.Id, p); continue; }
                lookup.Add(p.Id, p);
                p.gameObject.SetActive(true);
                p.Group.alpha = p.VisibleOnStart ? 1f : 0f;
                p.Group.interactable = p.VisibleOnStart;
                p.Group.blocksRaycasts = p.VisibleOnStart;
                p.SetState(p.VisibleOnStart ? SparkUIPanelState.Open : SparkUIPanelState.Closed);
                if (p.VisibleOnStart) openOrder.Add(p);
                else if (p.DeactivateWhenClosed) p.gameObject.SetActive(false);
            }
            Reorder(); Notify("Registry initialized: " + lookup.Count + " panels");
        }
        public bool Register(SparkUIPanel panel)
        {
            if (!panel || string.IsNullOrWhiteSpace(panel.Id) || lookup.ContainsKey(panel.Id)) return false;
            lookup.Add(panel.Id, panel); registeredPanels.Add(panel);
            panel.SetState(SparkUIPanelState.Closed); panel.gameObject.SetActive(false);
            Notify("Registered: " + panel.Id); return true;
        }
        public SparkUIPanel RegisterNative(string id, string label, SparkUILayer layer, string group,
            bool blockGameplay, Action<bool> apply, Func<bool> read)
        {
            if (Find(id) || apply == null || read == null) return null;
            var go=new GameObject(label+" UI registration");
            go.transform.SetParent(transform,false);
            var p=go.AddComponent<SparkUIPanel>();
            p.BindNative(id,label,layer,group,blockGameplay,apply,read);
            Register(p);
            apply(false);
            return p;
        }

        private void ObserveNativePanels()
        {
            foreach(var panel in registeredPanels.ToArray())
            {
                if(!panel || panel.NativeIsVisible==null) continue;
                bool visible=panel.NativeIsVisible();
                if(visible && !panel.IsVisible) Open(panel.Id);
                else if(!visible && panel.IsVisible) Close(panel.Id);
            }
        }

        public bool Unregister(string id)
        {
            SparkUIPanel p;
            if (!lookup.TryGetValue(id, out p)) return false;
            Cancel(p); lookup.Remove(id); registeredPanels.Remove(p); openOrder.Remove(p);
            if (focused == p) focused = null;
            Notify("Unregistered: " + id); return true;
        }
        public SparkUIPanel Find(string id) { SparkUIPanel p; return !string.IsNullOrWhiteSpace(id) && lookup.TryGetValue(id, out p) ? p : null; }
        public bool Open(string id)
        {
            var p = Find(id);
            if (!p) { Debug.LogWarning("[SparkUI] Panel not registered: " + id, this); return false; }
            if (p.IsVisible) { BringToFront(id); return true; }
            foreach (var other in registeredPanels.ToArray())
            {
                if (!other || other == p || !other.IsVisible) continue;
                bool sameGroup = !string.IsNullOrWhiteSpace(p.ExclusiveGroup) && string.Equals(p.ExclusiveGroup, other.ExclusiveGroup, StringComparison.OrdinalIgnoreCase);
                bool otherMain = p.Layer == SparkUILayer.Main && other.Layer == SparkUILayer.Main;
                if ((p.ClosePolicy == SparkUIClosePolicy.SameGroup && sameGroup) || (p.ClosePolicy == SparkUIClosePolicy.OtherMainPanels && (sameGroup || otherMain))) Close(other.Id);
            }
            Cancel(p); p.gameObject.SetActive(true);
            var g = p.Group; g.blocksRaycasts = true; g.interactable = true;
            p.SetState(SparkUIPanelState.Opening);
            openOrder.Remove(p); openOrder.Add(p); focused = p;
            history.Add(p.Id); if (history.Count > 150) history.RemoveAt(0);
            p.NativeVisibility?.Invoke(true);
            Reorder();
            if (p.Animate && p.FadeSeconds > .001f) running[p] = StartCoroutine(Fade(p, 1f, SparkUIPanelState.Open, false));
            else { g.alpha = 1f; p.SetState(SparkUIPanelState.Open); }
            Notify("Open: " + p.Id); return true;
        }
        public bool Close(string id)
        {
            var p = Find(id);
            if (!p || p.State == SparkUIPanelState.Closed || p.State == SparkUIPanelState.Closing) return false;
            Cancel(p); p.NativeVisibility?.Invoke(false); p.SetState(SparkUIPanelState.Closing);
            p.Group.blocksRaycasts = false; p.Group.interactable = false;
            if (focused == p) focused = null;
            openOrder.Remove(p); Reorder();
            if (p.Animate && p.FadeSeconds > .001f) running[p] = StartCoroutine(Fade(p, 0f, SparkUIPanelState.Closed, true));
            else CompleteClose(p);
            Notify("Close: " + p.Id); return true;
        }
        public bool Toggle(string id) { var p = Find(id); return p ? (p.IsVisible ? Close(id) : Open(id)) : false; }
        public bool Minimize(string id)
        {
            var p = Find(id); if (!p || !p.AllowMinimize || !p.IsVisible) return false;
            Cancel(p); p.NativeVisibility?.Invoke(false); p.Group.alpha = 0f; p.Group.blocksRaycasts = false; p.Group.interactable = false;
            p.SetState(SparkUIPanelState.Minimized); openOrder.Remove(p); if (focused == p) focused = null;
            Reorder(); Notify("Minimized: " + p.Id); return true;
        }
        public bool BringToFront(string id)
        {
            var p = Find(id); if (!p) return false;
            if (p.State == SparkUIPanelState.Minimized) return Open(id);
            if (!p.IsVisible) return Open(id);
            openOrder.Remove(p); openOrder.Add(p); focused = p; Reorder(); Notify("Focus: " + p.Id); return true;
        }
        public void CloseAll(bool includeHud = false)
        {
            foreach (var p in registeredPanels.ToArray())
                if (p && (includeHud || p.Layer != SparkUILayer.HUD && p.Layer != SparkUILayer.Background && p.Layer != SparkUILayer.System)) Close(p.Id);
        }
        public bool Back()
        {
            // Modal always gets priority; otherwise last opened/focused panel.
            var top = openOrder.Where(p => p && p.IsVisible && (p.Layer == SparkUILayer.Modal || p.Layer == SparkUILayer.Fullscreen)).LastOrDefault();
            if (!top) top = openOrder.LastOrDefault(p => p && p.IsVisible && p.Layer != SparkUILayer.HUD && p.Layer != SparkUILayer.Background);
            return top && Close(top.Id);
        }
        private void Reorder()
        {
            for (int i = 0; i < openOrder.Count; i++)
            {
                var p = openOrder[i]; if (p) p.SetSort(sortingBase + (int)p.Layer * 100 + p.Priority + i);
            }
            if (!focused || !focused.IsVisible) focused = openOrder.LastOrDefault(p => p && p.IsVisible);
        }
        private IEnumerator Fade(SparkUIPanel p, float target, SparkUIPanelState finalState, bool closing)
        {
            var g = p.Group; float from = g.alpha, t = 0f, duration = Mathf.Max(.001f, p.FadeSeconds);
            while (t < duration)
            {
                if (!p) yield break;
                t += Time.unscaledDeltaTime;
                g.alpha = Mathf.Lerp(from, target, Mathf.Clamp01(t / duration));
                yield return null;
            }
            g.alpha = target; running.Remove(p);
            if (closing) CompleteClose(p); else p.SetState(finalState);
            if (Changed != null) Changed();
        }
        private void CompleteClose(SparkUIPanel p)
        {
            if (!p) return;
            p.Group.alpha = 0f; p.SetState(SparkUIPanelState.Closed);
            if (p.DeactivateWhenClosed) p.gameObject.SetActive(false);
        }
        private void Cancel(SparkUIPanel p)
        {
            Coroutine c;
            if (running.TryGetValue(p, out c)) { if (c != null) StopCoroutine(c); running.Remove(p); }
        }
        private void Notify(string message)
        {
            if (verboseLogging) Debug.Log("[SparkUI] " + message, this);
            if (LogEvent != null) LogEvent(message);
            if (Changed != null) Changed();
        }
    }
}
