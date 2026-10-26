using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Optional presentation companion for SparkTutorialDirector.
    /// Highlights lesson targets, shows a pulsing instruction pointer and can
    /// smoothly aim a camera at a world target without moving the player.
    /// Does not manipulate gameplay or advance tutorial steps.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkTutorialCinematicGuide : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SparkTutorialDirector director;
        [SerializeField] private Camera sceneCamera;

        [Header("Focus Pointer")]
        [SerializeField] private bool showPointer = true;
        [SerializeField] private Color pointerColor = new Color(0.02f, 0.9f, 1f, 0.95f);
        [SerializeField, Range(16f, 120f)] private float pointerSize = 54f;
        [SerializeField] private Vector2 pointerOffset = new Vector2(0f, 65f);
        [SerializeField] private float pulseSpeed = 3.5f;
        [SerializeField] private int overlaySortingOrder = 501;

        [Header("Optional Camera Guidance")]
        [Tooltip("OFF by default because rotating a player-controlled camera can conflict with mouse look.")]
        [SerializeField] private bool guideCamera = false;
        [SerializeField, Range(0.1f, 10f)] private float cameraTurnSpeed = 2.5f;
        [SerializeField] private bool restoreCameraOnStepChange = false;

        private Canvas canvas;
        private RectTransform pointer;
        private CanvasGroup pointerGroup;
        private Image pointerImage;
        private Transform targetWorld;
        private RectTransform targetUI;
        private Vector3 targetOffset;
        private bool wantsFocus;
        private Quaternion originalCameraRotation;
        private bool savedRotation;
        private float nextResolveTime;
        private string currentKey;
        private SparkTutorialStep cachedStep;

        private void Awake()
        {
            if (director == null) director = GetComponent<SparkTutorialDirector>();
            if (director == null) director = FindFirstObjectByType<SparkTutorialDirector>();
            if (sceneCamera == null) sceneCamera = Camera.main;
            CreateOverlay();
        }

        private void OnEnable()
        {
            if (director == null) director = FindFirstObjectByType<SparkTutorialDirector>();
            if (director != null) director.StepChanged += OnStepChanged;
            RefreshTarget();
        }

        private void OnDisable()
        {
            if (director != null) director.StepChanged -= OnStepChanged;
            if (restoreCameraOnStepChange) RestoreCamera();
            if (canvas != null) canvas.enabled = false;
        }

        private void OnDestroy()
        {
            if (canvas != null) Destroy(canvas.gameObject);
        }

        private void OnStepChanged(SparkTutorialDefinition tutorial, int index)
        {
            if (restoreCameraOnStepChange) RestoreCamera();
            cachedStep = null;
            RefreshTarget();
        }

        private void Update()
        {
            if (director == null || !director.IsRunning)
            {
                if (canvas != null) canvas.enabled = false;
                return;
            }

            if (canvas != null) canvas.enabled = true;
            if (sceneCamera == null) sceneCamera = Camera.main;
            if (director.CurrentStep != cachedStep ||
                (wantsFocus && targetWorld == null && targetUI == null && Time.unscaledTime >= nextResolveTime))
            {
                RefreshTarget();
            }

            bool visible = TryGetScreenTarget(out Vector2 screenPosition);
            if (pointer != null)
            {
                pointer.gameObject.SetActive(showPointer && visible);
                if (showPointer && visible)
                {
                    pointer.position = screenPosition + pointerOffset;
                    float t = Time.unscaledTime * pulseSpeed;
                    pointer.localScale = Vector3.one * (1f + 0.12f * Mathf.Sin(t));
                    pointerGroup.alpha = 0.7f + 0.3f * Mathf.Sin(t) * Mathf.Sin(t);
                }
            }

            if (guideCamera && targetWorld != null && sceneCamera != null)
            {
                Vector3 toTarget = targetWorld.position + targetOffset - sceneCamera.transform.position;
                if (toTarget.sqrMagnitude > 0.0001f)
                {
                    if (!savedRotation)
                    {
                        originalCameraRotation = sceneCamera.transform.rotation;
                        savedRotation = true;
                    }
                    Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                    sceneCamera.transform.rotation = Quaternion.Slerp(
                        sceneCamera.transform.rotation, desired,
                        1f - Mathf.Exp(-cameraTurnSpeed * Time.unscaledDeltaTime));
                }
            }
        }

        private void RefreshTarget()
        {
            targetWorld = null;
            targetUI = null;
            wantsFocus = false;
            currentKey = null;
            nextResolveTime = Time.unscaledTime + 0.35f;

            if (director == null || !director.IsRunning) return;
            cachedStep = director.CurrentStep;
            SparkTutorialStep step = cachedStep;
            currentKey = step.locatorKey;
            targetOffset = step.worldOffset;
            wantsFocus = step.focus != SparkTutorialFocus.None;
            if (!wantsFocus) return;

            if (step.focus == SparkTutorialFocus.WorldTransform) targetWorld = step.worldTarget;
            if (step.focus == SparkTutorialFocus.ScreenRect) targetUI = step.screenTarget;

            if (string.IsNullOrWhiteSpace(currentKey)) return;
            // Runtime layouts are instantiated after scene load, so look up
            // active targets again until the correct level object appears.
            SparkTutorialTarget[] candidates = FindObjectsByType<SparkTutorialTarget>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (SparkTutorialTarget candidate in candidates)
            {
                if (candidate == null || !candidate.isActiveAndEnabled ||
                    !string.Equals(candidate.key, currentKey, StringComparison.OrdinalIgnoreCase)) continue;
                if (step.focus == SparkTutorialFocus.WorldTransform) targetWorld = candidate.WorldAnchor;
                else if (step.focus == SparkTutorialFocus.ScreenRect) targetUI = candidate.screenRect;
                break;
            }
        }

        private bool TryGetScreenTarget(out Vector2 position)
        {
            position = default;
            if (!wantsFocus) return false;
            if (targetUI != null)
            {
                Vector3[] corners = new Vector3[4];
                targetUI.GetWorldCorners(corners);
                Canvas parentCanvas = targetUI.GetComponentInParent<Canvas>();
                Camera uiCamera = parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? parentCanvas.worldCamera : null;
                Vector2 a = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]);
                Vector2 b = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[2]);
                position = (a + b) * 0.5f;
                return true;
            }
            if (targetWorld == null || sceneCamera == null) return false;
            Vector3 screen = sceneCamera.WorldToScreenPoint(targetWorld.position + targetOffset);
            if (screen.z <= 0f) return false;
            position = new Vector2(screen.x, screen.y);
            return true;
        }

        private void RestoreCamera()
        {
            if (savedRotation && sceneCamera != null) sceneCamera.transform.rotation = originalCameraRotation;
            savedRotation = false;
        }

        private void CreateOverlay()
        {
            GameObject root = new GameObject("Tutorial_Cinematic_Guide", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = overlaySortingOrder;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            GameObject arrow = new GameObject("Animated_Target_Pointer", typeof(RectTransform),
                typeof(CanvasGroup), typeof(Image));
            arrow.transform.SetParent(root.transform, false);
            pointer = arrow.GetComponent<RectTransform>();
            pointer.sizeDelta = new Vector2(pointerSize, pointerSize);
            pointerGroup = arrow.GetComponent<CanvasGroup>();
            pointerGroup.blocksRaycasts = false;
            pointerGroup.interactable = false;
            pointerImage = arrow.GetComponent<Image>();
            pointerImage.raycastTarget = false;
            pointerImage.color = pointerColor;
            pointerImage.sprite = CreateRingSprite();
            arrow.SetActive(false);
        }

        // Procedurally generated AA-ish ring texture: no external sprites needed.
        private Sprite CreateRingSprite()
        {
            const int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = "SparkTutorial_Ring_Runtime";
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f - size * 0.5f) / (size * 0.5f);
                float dy = (y + 0.5f - size * 0.5f) / (size * 0.5f);
                float radius = Mathf.Sqrt(dx * dx + dy * dy);
                float outer = Mathf.Clamp01((0.96f - radius) * 45f);
                float inner = Mathf.Clamp01((radius - 0.77f) * 45f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, outer * inner);
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
