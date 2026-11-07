using TMPro;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Slides the existing voltage-monitor panel off the right edge while leaving
    /// its toggle tab visible. Pure presentation: never touches the solver.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVoltageDrawer : MonoBehaviour
    {
        [Header("Generated UI references")]
        [SerializeField] private RectTransform drawerPanel;
        [SerializeField] private TMP_Text arrowText;
        [SerializeField] private TMP_Text tabText;

        [Header("Animation")]
        [SerializeField] private bool startOpen = false;
        [SerializeField, Min(0.05f)] private float animationDuration = 0.34f;
        [SerializeField, Min(0f)] private float additionalHideDistance = 0f;
        [SerializeField] private bool useUnscaledTime = true;

        private Vector2 openPosition;
        private float progress;
        private float fromProgress;
        private float targetProgress;
        private float elapsed;
        private bool initialized;

        public bool IsOpen => targetProgress >= 0.5f;

        private void Awake() => Initialize();

        private void OnEnable()
        {
            Initialize();
            progress = startOpen ? 1f : 0f;
            fromProgress = targetProgress = progress;
            elapsed = animationDuration;
            ApplyPosition();
            UpdateTab();
        }

        private void Initialize()
        {
            if (initialized || drawerPanel == null) return;
            openPosition = drawerPanel.anchoredPosition;
            initialized = true;
        }

        private void Update()
        {
            if (!initialized || drawerPanel == null) return;
            if (Mathf.Approximately(progress, targetProgress)) return;

            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.05f, animationDuration));
            // Cubic ease-out: fast start, smooth finish.
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            progress = Mathf.Lerp(fromProgress, targetProgress, eased);
            if (t >= 1f) progress = targetProgress;
            ApplyPosition();
        }

        private void ApplyPosition()
        {
            // At closed, panel starts just beyond the right screen edge.
            // The toggle tab, placed to the panel's left, remains visible.
            float travel = drawerPanel.rect.width +
                Mathf.Abs(openPosition.x) + additionalHideDistance;
            drawerPanel.anchoredPosition = openPosition +
                new Vector2((1f - progress) * travel, 0f);
        }

        private void UpdateTab()
        {
            // When closed, arrow points left to indicate opening.
            if (arrowText != null) arrowText.text = IsOpen ? "❯" : "❮";
            if (tabText != null) tabText.text = IsOpen ? "HIDE" : "VOLTS";
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open() => AnimateTo(1f);
        public void Close() => AnimateTo(0f);

        private void AnimateTo(float target)
        {
            Initialize();
            if (!initialized) return;
            fromProgress = progress;
            targetProgress = target;
            elapsed = 0f;
            UpdateTab();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (initialized) ApplyPosition();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            animationDuration = Mathf.Max(0.05f, animationDuration);
            additionalHideDistance = Mathf.Max(0f, additionalHideDistance);
        }
#endif
    }
}
