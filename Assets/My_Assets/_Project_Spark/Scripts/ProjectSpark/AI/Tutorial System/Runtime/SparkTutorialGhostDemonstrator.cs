using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Presentation-only cursor demonstrations. Does not inject input, move
    /// shelf objects, select components, or create circuit connections.
    /// Works with Phase 1 SparkTutorialDirector and SparkTutorialTarget.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkTutorialGhostDemonstrator : MonoBehaviour
    {
        public enum DemonstrationKind { Point, Click, Drag, Connect }

        [Serializable]
        public sealed class Demonstration
        {
            [Tooltip("Match SparkTutorialStep.id in the tutorial asset.")]
            public string stepId;
            public DemonstrationKind kind = DemonstrationKind.Drag;
            [Tooltip("SparkTutorialTarget.key for the start of the gesture.")]
            public string sourceKey;
            [Tooltip("SparkTutorialTarget.key for the destination. Not needed for Point/Click.")]
            public string destinationKey;
            [Min(0f)] public float startDelay = 0.6f;
            [Min(0.1f)] public float motionSeconds = 1.6f;
            [Min(0.1f)] public float endPauseSeconds = 0.7f;
            public bool replayUntilStepCompleted = true;
            [Min(0.1f)] public float replayDelaySeconds = 2f;
        }

        [Header("References")]
        [SerializeField] private SparkTutorialDirector director;
        [SerializeField] private Camera sceneCamera;

        [Header("Demonstrations (match tutorial step IDs)")]
        [SerializeField] private List<Demonstration> demonstrations = new List<Demonstration>();

        [Header("Presentation")]
        [SerializeField] private int sortingOrder = 520;
        [SerializeField] private Color cursorColor = new Color(0.04f, 0.94f, 1f, 0.95f);
        [SerializeField] private Color trailColor = new Color(0.04f, 0.94f, 1f, 0.5f);
        [SerializeField, Range(18f, 110f)] private float cursorSize = 44f;
        [SerializeField, Min(0f)] private float targetResolveTimeout = 8f;
        [SerializeField] private bool replayHotkeyF3 = true;
        [SerializeField] private bool logDemonstrations;

        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private RectTransform cursor;
        private RectTransform trail;
        private RectTransform halo;
        private Image cursorImage;
        private Image trailImage;
        private Image haloImage;
        private Texture2D generatedTexture;
        private Sprite generatedSprite;
        private Coroutine playRoutine;
        private SparkTutorialStep activeStep;
        private Demonstration activeDemo;
        private bool attempted;

        private void Awake()
        {
            if (director == null) director = GetComponent<SparkTutorialDirector>();
            if (director == null) director = FindFirstObjectByType<SparkTutorialDirector>();
            if (sceneCamera == null) sceneCamera = Camera.main;
            CreateUI();
            Hide();
        }

        private void OnEnable()
        {
            if (director == null) director = FindFirstObjectByType<SparkTutorialDirector>();
            if (director != null)
            {
                director.StepChanged += HandleStepChanged;
                director.TutorialCompleted += HandleTutorialCompleted;
                director.GameplaySignalReceived += HandleGameplaySignal;
            }
            if (director != null && director.IsRunning) PrepareStep();
        }

        private void OnDisable()
        {
            if (director != null)
            {
                director.StepChanged -= HandleStepChanged;
                director.TutorialCompleted -= HandleTutorialCompleted;
                director.GameplaySignalReceived -= HandleGameplaySignal;
            }
            StopDemo();
        }

        private void OnDestroy()
        {
            if (canvas != null) Destroy(canvas.gameObject);
            if (generatedSprite != null) Destroy(generatedSprite);
            if (generatedTexture != null) Destroy(generatedTexture);
        }

        private void Update()
        {
            if (director == null || !director.IsRunning)
            {
                if (playRoutine != null) StopDemo();
                activeStep = null;
                return;
            }
            if (director.CurrentStep != activeStep) PrepareStep();
            if (replayHotkeyF3 && Input.GetKeyDown(KeyCode.F3)) Replay();
        }

        private void HandleStepChanged(SparkTutorialDefinition lesson, int stepIndex) => PrepareStep();
        private void HandleTutorialCompleted(SparkTutorialDefinition lesson) => StopDemo();

        private void HandleGameplaySignal(string signal)
        {
            if (director == null || !director.IsRunning || activeStep == null) return;
            if (!string.IsNullOrWhiteSpace(activeStep.requiredSignal) &&
                string.Equals(activeStep.requiredSignal, signal, StringComparison.OrdinalIgnoreCase))
                StopDemo();
        }

        private void PrepareStep()
        {
            StopDemo();
            activeStep = director != null ? director.CurrentStep : null;
            activeDemo = null;
            attempted = false;
            if (activeStep == null) return;
            foreach (Demonstration demo in demonstrations)
            {
                if (demo != null && string.Equals(demo.stepId, activeStep.id, StringComparison.OrdinalIgnoreCase))
                {
                    activeDemo = demo;
                    break;
                }
            }
            if (activeDemo != null) Play();
        }

        [ContextMenu("Tutorial / Replay Ghost Demonstration")]
        public void Replay()
        {
            StopDemo();
            if (director != null && director.IsRunning && activeDemo != null) Play();
        }

        private void Play()
        {
            if (!isActiveAndEnabled || activeDemo == null) return;
            playRoutine = StartCoroutine(AnimateDemo(activeDemo, activeStep));
        }

        private IEnumerator AnimateDemo(Demonstration demo, SparkTutorialStep expectedStep)
        {
            if (demo.startDelay > 0f) yield return new WaitForSecondsRealtime(demo.startDelay);
            float resolving = 0f;
            while (IsStillCurrent(expectedStep) && !TryGetTargets(demo, out _, out _) &&
                   resolving < targetResolveTimeout)
            {
                resolving += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!IsStillCurrent(expectedStep) || !TryGetTargets(demo, out _, out _))
            {
                Hide();
                playRoutine = null;
                yield break;
            }

            attempted = true;
            do
            {
                if (!IsStillCurrent(expectedStep)) break;
                if (!TryGetTargets(demo, out Vector2 from, out Vector2 to)) break;
                Show();
                if (logDemonstrations) Debug.Log("[TutorialGhost] Demonstrating '" + demo.stepId + "'.", this);
                SetPointer(from, 0f);
                yield return new WaitForSecondsRealtime(0.35f);
                if (!IsStillCurrent(expectedStep)) break;
                if (demo.kind == DemonstrationKind.Click || demo.kind == DemonstrationKind.Point)
                {
                    SetPointer(from, 1f);
                    yield return new WaitForSecondsRealtime(demo.endPauseSeconds);
                }
                else
                {
                    float elapsed = 0f;
                    while (elapsed < demo.motionSeconds && IsStillCurrent(expectedStep))
                    {
                        elapsed += Time.unscaledDeltaTime;
                        if (!TryGetTargets(demo, out from, out to)) break;
                        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / demo.motionSeconds));
                        SetPointer(Vector2.Lerp(from, to, t), t);
                        SetTrail(from, Vector2.Lerp(from, to, t));
                        yield return null;
                    }
                    if (!IsStillCurrent(expectedStep)) break;
                    SetPointer(to, 1f);
                    yield return new WaitForSecondsRealtime(demo.endPauseSeconds);
                }
                Hide();
                if (!demo.replayUntilStepCompleted) break;
                yield return new WaitForSecondsRealtime(demo.replayDelaySeconds);
            }
            while (IsStillCurrent(expectedStep));
            Hide();
            playRoutine = null;
        }

        private bool IsStillCurrent(SparkTutorialStep step) =>
            director != null && director.IsRunning && director.CurrentStep == step;

        private bool TryGetTargets(Demonstration demo, out Vector2 from, out Vector2 to)
        {
            from = to = default;
            if (!TryResolve(demo.sourceKey, out from)) return false;
            if (demo.kind == DemonstrationKind.Point || demo.kind == DemonstrationKind.Click)
            {
                to = from;
                return true;
            }
            return TryResolve(demo.destinationKey, out to);
        }

        private bool TryResolve(string key, out Vector2 screen)
        {
            screen = default;
            if (string.IsNullOrWhiteSpace(key)) return false;
            SparkTutorialTarget[] targets = FindObjectsByType<SparkTutorialTarget>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (SparkTutorialTarget target in targets)
            {
                if (target == null || !target.isActiveAndEnabled ||
                    !string.Equals(target.key, key, StringComparison.OrdinalIgnoreCase)) continue;
                if (target.screenRect != null)
                {
                    Vector3[] corners = new Vector3[4];
                    target.screenRect.GetWorldCorners(corners);
                    Canvas parent = target.screenRect.GetComponentInParent<Canvas>();
                    Camera uiCamera = parent != null && parent.renderMode != RenderMode.ScreenSpaceOverlay
                        ? parent.worldCamera : null;
                    Vector2 a = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]);
                    Vector2 b = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[2]);
                    screen = (a + b) * 0.5f;
                    return true;
                }
                if (sceneCamera == null) sceneCamera = Camera.main;
                if (sceneCamera == null || target.WorldAnchor == null) return false;
                Vector3 point = sceneCamera.WorldToScreenPoint(target.WorldAnchor.position);
                if (point.z <= 0f) return false;
                screen = new Vector2(point.x, point.y);
                return true;
            }
            return false;
        }

        private void SetPointer(Vector2 screen, float pressed)
        {
            cursor.position = screen;
            cursor.localScale = Vector3.one * (1f - pressed * 0.12f);
            halo.position = screen;
            halo.localScale = Vector3.one * (1f + pressed * 0.55f);
            haloImage.color = new Color(cursorColor.r, cursorColor.g, cursorColor.b, 0.5f * (1f - pressed * 0.4f));
        }

        private void SetTrail(Vector2 start, Vector2 end)
        {
            Vector2 delta = end - start;
            trail.gameObject.SetActive(delta.sqrMagnitude > 9f);
            trail.position = (start + end) * 0.5f;
            trail.sizeDelta = new Vector2(delta.magnitude, 3f);
            trail.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        private void Show()
        {
            canvas.enabled = true;
            cursor.gameObject.SetActive(true);
            halo.gameObject.SetActive(true);
            trail.gameObject.SetActive(false);
        }

        private void Hide()
        {
            if (canvas != null) canvas.enabled = false;
            if (trail != null) trail.gameObject.SetActive(false);
        }

        private void StopDemo()
        {
            if (playRoutine != null) StopCoroutine(playRoutine);
            playRoutine = null;
            Hide();
        }

        private void CreateUI()
        {
            var go = new GameObject("Tutorial_Ghost_Demonstrator", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            canvasGroup = go.GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            generatedTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            generatedTexture.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                float d = Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(32, 32));
                float ring = Mathf.Clamp01((26 - d) * 2f) * Mathf.Clamp01((d - 20) * 2f);
                float dot = Mathf.Clamp01((5 - d) * 0.8f);
                pixels[y * 64 + x] = new Color(1, 1, 1, Mathf.Max(ring, dot));
            }
            generatedTexture.SetPixels(pixels);
            generatedTexture.Apply();
            generatedSprite = Sprite.Create(generatedTexture, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f));
            trail = CreateImage("Gesture_Path", trailColor, null, new Vector2(100, 3));
            halo = CreateImage("Click_Halo", cursorColor, generatedSprite, Vector2.one * cursorSize * 1.45f);
            cursor = CreateImage("Ghost_Cursor", cursorColor, generatedSprite, Vector2.one * cursorSize);
            cursorImage = cursor.GetComponent<Image>();
            trailImage = trail.GetComponent<Image>();
            haloImage = halo.GetComponent<Image>();
        }

        private RectTransform CreateImage(string name, Color color, Sprite sprite, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(canvas.transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }
    }
}
