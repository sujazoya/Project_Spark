using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectSpark.UI
{
    [DisallowMultipleComponent]
    public sealed class ProjectSparkTutorialHub : MonoBehaviour
    {
        [Serializable] public sealed class HubAction
        {
            public Button button;
            public UnityEvent onClick = new UnityEvent();
        }

        [Header("Drawer")]
        [SerializeField] private RectTransform drawer;
        [SerializeField] private Button drawerToggle;
        [SerializeField] private TMP_Text drawerArrow;
        [SerializeField] private Button closeButton;
        [SerializeField] private bool startOpen;
        [SerializeField, Min(.05f)] private float animationSeconds = .35f;
        [SerializeField, Min(0)] private float offscreenExtra = 4f;
        [SerializeField] private float openRightInset = 12f;

        [Header("Live Information")]
        [SerializeField] private TMP_Text goalText;
        [SerializeField] private TMP_Text stepText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TMP_Text issueText;
        [SerializeField] private TMP_Text recommendationText;
        [SerializeField] private TMP_Text voiceStateText;
        [SerializeField] private Toggle voiceToggle;

        [Header("Tutorial Actions – assign existing system methods in Inspector")]
        [SerializeField] private HubAction objectives = new HubAction();
        [SerializeField] private HubAction currentStep = new HubAction();
        [SerializeField] private HubAction hint = new HubAction();
        [SerializeField] private HubAction diagnose = new HubAction();
        [SerializeField] private HubAction troubleshooter = new HubAction();
        [SerializeField] private HubAction assistant = new HubAction();
        [SerializeField] private HubAction previousStep = new HubAction();
        [SerializeField] private HubAction nextStep = new HubAction();
        [SerializeField] private HubAction resetTutorial = new HubAction();
        [SerializeField] private HubAction componentsHelp = new HubAction();
        [SerializeField] private UnityEvent<bool> onVoiceChanged = new UnityEvent<bool>();

        private Coroutine motion;
        private bool open;
        private float initialY;

        private void Awake()
        {
            if (drawer == null) drawer = transform.Find("Drawer") as RectTransform;
            if (drawerToggle == null) drawerToggle = transform.Find("Drawer/DrawerToggle")?.GetComponent<Button>();
            if (drawerArrow == null) drawerArrow = transform.Find("Drawer/DrawerToggle/Arrow")?.GetComponent<TMP_Text>();
            if (drawer == null) { Debug.LogError("[TutorialHub] Drawer missing.", this); enabled = false; return; }
            initialY = drawer.anchoredPosition.y;
            if (drawerToggle != null) drawerToggle.onClick.AddListener(ToggleDrawer);
            if (closeButton != null) closeButton.onClick.AddListener(CloseDrawer);
            Bind(objectives); Bind(currentStep); Bind(hint); Bind(diagnose);
            Bind(troubleshooter); Bind(assistant); Bind(previousStep);
            Bind(nextStep); Bind(resetTutorial); Bind(componentsHelp);
            if (voiceToggle != null) voiceToggle.onValueChanged.AddListener(VoiceChanged);
            SetOpenImmediate(startOpen);
            VoiceChanged(voiceToggle != null && voiceToggle.isOn, false);
        }

        private static void Bind(HubAction action)
        {
            if (action != null && action.button != null)
                action.button.onClick.AddListener(action.onClick.Invoke);
        }

        private static void Unbind(HubAction action)
        {
            if (action != null && action.button != null)
                action.button.onClick.RemoveListener(action.onClick.Invoke);
        }

        private void OnDestroy()
        {
            if (drawerToggle != null) drawerToggle.onClick.RemoveListener(ToggleDrawer);
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseDrawer);
            Unbind(objectives); Unbind(currentStep); Unbind(hint); Unbind(diagnose);
            Unbind(troubleshooter); Unbind(assistant); Unbind(previousStep);
            Unbind(nextStep); Unbind(resetTutorial); Unbind(componentsHelp);
            if (voiceToggle != null) voiceToggle.onValueChanged.RemoveListener(VoiceChanged);
        }

        private void OnDisable()
        {
            if (motion != null) { StopCoroutine(motion); motion = null; }
        }

        // Drawer is right-anchored; positive X moves the panel beyond its parent's right edge.
        private float ClosedX => drawer.rect.width + offscreenExtra;
        private float OpenX => -openRightInset;

        public void ToggleDrawer() => SetOpen(!open);
        public void OpenDrawer() => SetOpen(true);
        public void CloseDrawer() => SetOpen(false);

        public void SetOpen(bool value)
        {
            if (drawer == null) return;
            open = value;
            UpdateArrow();
            if (motion != null) StopCoroutine(motion);
            if (!isActiveAndEnabled) { SetOpenImmediate(value); return; }
            motion = StartCoroutine(Animate(open ? OpenX : ClosedX));
        }

        private void SetOpenImmediate(bool value)
        {
            if (drawer == null) return;
            open = value;
            if (motion != null) { StopCoroutine(motion); motion = null; }
            drawer.anchoredPosition = new Vector2(value ? OpenX : ClosedX, initialY);
            UpdateArrow();
        }

        private IEnumerator Animate(float target)
        {
            float from = drawer.anchoredPosition.x;
            float elapsed = 0f;
            while (elapsed < animationSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / animationSeconds);
                t = t * t * (3f - 2f * t);
                drawer.anchoredPosition = new Vector2(Mathf.Lerp(from, target, t), initialY);
                yield return null;
            }
            drawer.anchoredPosition = new Vector2(target, initialY);
            motion = null;
        }

        private void UpdateArrow()
        {
            // Tab is on the LEFT edge of the right-side drawer.
            if (drawerArrow != null) drawerArrow.text = open ? ">" : "<";
        }

        private void VoiceChanged(bool enabled) => VoiceChanged(enabled, true);
        private void VoiceChanged(bool enabled, bool notify)
        {
            if (voiceStateText != null) voiceStateText.text = enabled ? "ON" : "OFF";
            if (notify) onVoiceChanged.Invoke(enabled);
        }

        // Public setters let your existing level/tutorial systems supply real data.
        public void SetGoal(string goal) { if (goalText != null) goalText.text = goal ?? ""; }
        public void SetStep(int current, int total)
        {
            if (stepText != null) stepText.text = total > 0 ? $"STEP {Mathf.Clamp(current, 0, total)} / {total}" : "STEP —";
            if (progressSlider != null) progressSlider.value = total > 0 ? Mathf.Clamp01((float)current / total) : 0f;
        }
        public void SetIssue(string issue) { if (issueText != null) issueText.text = string.IsNullOrWhiteSpace(issue) ? "No issues reported" : issue; }
        public void SetRecommendation(string recommendation) { if (recommendationText != null) recommendationText.text = recommendation ?? ""; }
        public void SetVoiceEnabled(bool value)
        {
            if (voiceToggle != null) voiceToggle.isOn = value;
            else VoiceChanged(value, false);
        }
    }
}
