using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Per-object Show / Hide controller.
    ///
    /// Each object owns:
    /// - Its visibility target
    /// - Its own UI panel
    /// - Show button
    /// - Hide button
    /// - Close button
    ///
    /// Panel animation:
    /// - Fade
    /// - Scale
    ///
    /// No Animator is required.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkObjectVisibilityController : MonoBehaviour
    {
        // ============================================================
        // VISIBILITY TARGET
        // ============================================================

        [Header("Visibility Target")]

        [Tooltip(
            "GameObject that will be shown or hidden.")]
        [SerializeField]
        private GameObject visibilityTarget;

        // ============================================================
        // PANEL
        // ============================================================

        [Header("Own Panel")]

        [Tooltip(
            "This object's own Show / Hide panel.")]
        [SerializeField]
        private GameObject visibilityPanel;

        [Tooltip(
            "CanvasGroup used for panel fading. " +
            "If empty, it will be searched automatically.")]
        [SerializeField]
        private CanvasGroup panelCanvasGroup;

        // ============================================================
        // BUTTONS
        // ============================================================

        [Header("Panel Buttons")]

        [SerializeField]
        private Button showButton;

        [SerializeField]
        private Button hideButton;

        [SerializeField]
        private Button closeButton;

        // ============================================================
        // PANEL ANIMATION
        // ============================================================

        [Header("Panel Animation")]

        [SerializeField]
        private bool animatePanel = true;

        [SerializeField]
        [Min(0.01f)]
        private float openDuration = 0.18f;

        [SerializeField]
        [Min(0.01f)]
        private float closeDuration = 0.14f;

        [SerializeField]
        [Range(0f, 1f)]
        private float hiddenAlpha = 0f;

        [SerializeField]
        [Range(0.01f, 1f)]
        private float hiddenScale = 0.92f;

        [SerializeField]
        private AnimationCurve openCurve =
            AnimationCurve.EaseInOut(
                0f,
                0f,
                1f,
                1f);

        [SerializeField]
        private AnimationCurve closeCurve =
            AnimationCurve.EaseInOut(
                0f,
                0f,
                1f,
                1f);

        // ============================================================
        // SETTINGS
        // ============================================================

        [Header("Settings")]

        [SerializeField]
        private bool visibleOnStart = true;

        [SerializeField]
        private bool disableCollidersWhenHidden = true;

        // ============================================================
        // RUNTIME
        // ============================================================

        [Header("Runtime")]

        [SerializeField]
        private bool isVisible = true;

        private Coroutine panelAnimationCoroutine;

        private RectTransform panelRectTransform;

        private Vector3 visibleScale;

        // ============================================================
        // PUBLIC
        // ============================================================

        public bool IsVisible =>
            isVisible;

        public bool IsHidden =>
            !isVisible;

        public GameObject VisibilityTarget =>
            GetVisibilityTarget();

        public GameObject VisibilityPanel =>
            visibilityPanel;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            if (visibilityTarget == null)
            {
                visibilityTarget = gameObject;
            }

            SetupPanel();

            RegisterButtonListeners();

            PreparePanelClosed();
        }

        private void Start()
        {
            SetVisibleImmediate(
                visibleOnStart);
        }

        private void OnDestroy()
        {
            UnregisterButtonListeners();

            StopPanelAnimation();
        }

        // ============================================================
        // PANEL SETUP
        // ============================================================

        private void SetupPanel()
        {
            if (visibilityPanel == null)
                return;

            panelRectTransform =
                visibilityPanel.GetComponent<
                    RectTransform>();

            if (panelRectTransform != null)
            {
                visibleScale =
                    panelRectTransform.localScale;
            }

            if (panelCanvasGroup == null)
            {
                panelCanvasGroup =
                    visibilityPanel.GetComponent<
                        CanvasGroup>();
            }

            if (panelCanvasGroup == null)
            {
                panelCanvasGroup =
                    visibilityPanel.AddComponent<
                        CanvasGroup>();
            }
        }

        // ============================================================
        // BUTTON REGISTRATION
        // ============================================================

        private void RegisterButtonListeners()
        {
            if (showButton != null)
            {
                showButton.onClick.AddListener(
                    Show);
            }

            if (hideButton != null)
            {
                hideButton.onClick.AddListener(
                    Hide);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(
                    ClosePanel);
            }
        }

        private void UnregisterButtonListeners()
        {
            if (showButton != null)
            {
                showButton.onClick.RemoveListener(
                    Show);
            }

            if (hideButton != null)
            {
                hideButton.onClick.RemoveListener(
                    Hide);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(
                    ClosePanel);
            }
        }

        // ============================================================
        // OPEN PANEL
        // ============================================================

        public void OpenPanel()
        {
            if (visibilityPanel == null)
                return;

            StopPanelAnimation();

            visibilityPanel.SetActive(true);

            RefreshPanel();

            if (!animatePanel)
            {
                SetPanelVisibleImmediate();
                return;
            }

            panelAnimationCoroutine =
                StartCoroutine(
                    AnimatePanel(
                        true));
        }

        // ============================================================
        // CLOSE PANEL
        // ============================================================

        public void ClosePanel()
        {
            if (visibilityPanel == null)
                return;

            StopPanelAnimation();

            if (!animatePanel)
            {
                visibilityPanel.SetActive(false);
                return;
            }

            panelAnimationCoroutine =
                StartCoroutine(
                    AnimatePanel(
                        false));
        }

        // ============================================================
        // SHOW OBJECT
        // ============================================================

        public void Show()
        {
            SetVisible(true);

            RefreshPanel();
        }

        // ============================================================
        // HIDE OBJECT
        // ============================================================

        public void Hide()
        {
            SetVisible(false);

            RefreshPanel();
        }

        // ============================================================
        // TOGGLE OBJECT
        // ============================================================

        public void Toggle()
        {
            SetVisible(!isVisible);

            RefreshPanel();
        }

        // ============================================================
        // SET VISIBILITY
        // ============================================================

        public void SetVisible(bool visible)
        {
            GameObject target =
                GetVisibilityTarget();

            if (target == null)
                return;

            isVisible = visible;

            target.SetActive(visible);

            if (disableCollidersWhenHidden)
            {
                SetCollidersEnabled(
                    target,
                    visible);
            }
        }

        // ============================================================
        // IMMEDIATE VISIBILITY
        // ============================================================

        private void SetVisibleImmediate(
            bool visible)
        {
            GameObject target =
                GetVisibilityTarget();

            if (target == null)
                return;

            isVisible = visible;

            target.SetActive(visible);

            if (disableCollidersWhenHidden)
            {
                SetCollidersEnabled(
                    target,
                    visible);
            }
        }

        // ============================================================
        // GET TARGET
        // ============================================================

        private GameObject GetVisibilityTarget()
        {
            if (visibilityTarget != null)
                return visibilityTarget;

            return gameObject;
        }

        // ============================================================
        // COLLIDERS
        // ============================================================

        private void SetCollidersEnabled(
            GameObject target,
            bool enabled)
        {
            Collider[] colliders =
                target.GetComponentsInChildren<
                    Collider>(
                        true);

            for (int i = 0;
                 i < colliders.Length;
                 i++)
            {
                Collider collider =
                    colliders[i];

                if (collider == null)
                    continue;

                collider.enabled =
                    enabled;
            }
        }

        // ============================================================
        // PANEL ANIMATION
        // ============================================================

        private IEnumerator AnimatePanel(
            bool opening)
        {
            if (visibilityPanel == null)
                yield break;

            if (panelCanvasGroup == null)
            {
                SetupPanel();
            }

            if (panelCanvasGroup == null)
                yield break;

            if (panelRectTransform == null)
            {
                panelRectTransform =
                    visibilityPanel.GetComponent<
                        RectTransform>();
            }

            if (panelRectTransform != null &&
                visibleScale == Vector3.zero)
            {
                visibleScale =
                    panelRectTransform.localScale;
            }

            float duration =
                opening
                    ? openDuration
                    : closeDuration;

            float startAlpha =
                opening
                    ? hiddenAlpha
                    : 1f;

            float endAlpha =
                opening
                    ? 1f
                    : hiddenAlpha;

            Vector3 startScale =
                opening
                    ? visibleScale * hiddenScale
                    : visibleScale;

            Vector3 endScale =
                opening
                    ? visibleScale
                    : visibleScale * hiddenScale;

            panelCanvasGroup.alpha =
                startAlpha;

            if (panelRectTransform != null)
            {
                panelRectTransform.localScale =
                    startScale;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float normalized =
                    Mathf.Clamp01(
                        elapsed / duration);

                float curveValue =
                    opening
                        ? openCurve.Evaluate(
                            normalized)
                        : closeCurve.Evaluate(
                            normalized);

                panelCanvasGroup.alpha =
                    Mathf.Lerp(
                        startAlpha,
                        endAlpha,
                        curveValue);

                if (panelRectTransform != null)
                {
                    panelRectTransform.localScale =
                        Vector3.Lerp(
                            startScale,
                            endScale,
                            curveValue);
                }

                yield return null;
            }

            panelCanvasGroup.alpha =
                endAlpha;

            if (panelRectTransform != null)
            {
                panelRectTransform.localScale =
                    endScale;
            }

            if (!opening)
            {
                visibilityPanel.SetActive(false);
            }

            panelAnimationCoroutine =
                null;
        }

        // ============================================================
        // PANEL INITIAL STATE
        // ============================================================

        private void PreparePanelClosed()
        {
            if (visibilityPanel == null)
                return;

            if (panelCanvasGroup == null)
                SetupPanel();

            visibilityPanel.SetActive(false);

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha =
                    hiddenAlpha;
            }

            if (panelRectTransform != null)
            {
                panelRectTransform.localScale =
                    visibleScale * hiddenScale;
            }
        }

        private void SetPanelVisibleImmediate()
        {
            if (visibilityPanel == null)
                return;

            visibilityPanel.SetActive(true);

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 1f;
            }

            if (panelRectTransform != null)
            {
                panelRectTransform.localScale =
                    visibleScale;
            }
        }

        // ============================================================
        // STOP ANIMATION
        // ============================================================

        private void StopPanelAnimation()
        {
            if (panelAnimationCoroutine == null)
                return;

            StopCoroutine(
                panelAnimationCoroutine);

            panelAnimationCoroutine =
                null;
        }

        // ============================================================
        // PANEL BUTTON STATE
        // ============================================================

        private void RefreshPanel()
        {
            if (showButton != null)
            {
                showButton.interactable =
                    !isVisible;
            }

            if (hideButton != null)
            {
                hideButton.interactable =
                    isVisible;
            }
        }

        // ============================================================
        // CONTEXT MENU
        // ============================================================

        [ContextMenu("Show Object")]
        private void TestShow()
        {
            Show();
        }

        [ContextMenu("Hide Object")]
        private void TestHide()
        {
            Hide();
        }

        [ContextMenu("Toggle Object")]
        private void TestToggle()
        {
            Toggle();
        }

        [ContextMenu("Open Panel")]
        private void TestOpenPanel()
        {
            OpenPanel();
        }

        [ContextMenu("Close Panel")]
        private void TestClosePanel()
        {
            ClosePanel();
        }
    }
}
