using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Visual feedback for a physical hair-dryer control.
    ///
    /// Supports:
    /// - Indicator light intensity
    /// - Indicator color
    /// - Mechanical click pulse
    /// - Optional moving indicator transform
    ///
    /// Visual only.
    /// No electrical simulation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerSwitchIndicatorVisual : MonoBehaviour
    {
        private static readonly int EmissionColorID =
            Shader.PropertyToID("_EmissionColor");

        [Header("Indicator")]
        [SerializeField]
        private Renderer indicatorRenderer;

        [SerializeField]
        private Color offColor =
            new Color(0.03f, 0.03f, 0.03f);

        [SerializeField]
        private Color onColor =
            new Color(1f, 0.35f, 0.05f);

        [SerializeField]
        [Min(0f)]
        private float maximumEmission = 2f;

        [Header("Animation")]
        [SerializeField]
        [Min(0f)]
        private float fadeSpeed = 8f;

        [SerializeField]
        [Min(0f)]
        private float clickPulseDuration = 0.08f;

        [SerializeField]
        [Min(0f)]
        private float clickPulseScale = 0.04f;

        [Header("Runtime")]
        [SerializeField]
        private bool isOn;

        [SerializeField]
        [Range(0f, 1f)]
        private float intensity;

        private Material runtimeMaterial;
        private Vector3 originalScale;
        private float pulseTimer;

        public bool IsOn => isOn;

        public float Intensity => intensity;

        private void Awake()
        {
            if (indicatorRenderer != null)
            {
                runtimeMaterial =
                    indicatorRenderer.material;
            }

            originalScale = transform.localScale;

            ApplyVisual();
        }

        private void Update()
        {
            UpdateIntensity();
            UpdateClickPulse();
            ApplyVisual();
        }

        private void OnDestroy()
        {
            if (runtimeMaterial == null)
                return;

            if (Application.isPlaying)
                Destroy(runtimeMaterial);
            else
                DestroyImmediate(runtimeMaterial);
        }

        private void UpdateIntensity()
        {
            float target =
                isOn ? 1f : 0f;

            intensity =
                Mathf.MoveTowards(
                    intensity,
                    target,
                    fadeSpeed * Time.deltaTime);
        }

        private void UpdateClickPulse()
        {
            if (pulseTimer <= 0f)
            {
                transform.localScale =
                    originalScale;

                return;
            }

            pulseTimer -= Time.deltaTime;

            float normalized =
                Mathf.Clamp01(
                    pulseTimer /
                    Mathf.Max(0.001f, clickPulseDuration));

            float pulse =
                Mathf.Sin(
                    normalized * Mathf.PI);

            transform.localScale =
                originalScale *
                (1f + pulse * clickPulseScale);
        }

        private void ApplyVisual()
        {
            if (runtimeMaterial == null)
                return;

            if (!runtimeMaterial.HasProperty(EmissionColorID))
                return;

            Color color =
                Color.Lerp(
                    offColor,
                    onColor,
                    intensity);

            color *=
                Mathf.Lerp(
                    0f,
                    maximumEmission,
                    intensity);

            runtimeMaterial.SetColor(
                EmissionColorID,
                color);
        }

        public void SetOn()
        {
            SetState(true);
        }

        public void SetOff()
        {
            SetState(false);
        }

        public void SetState(bool state)
        {
            if (isOn != state)
                PlayClick();

            isOn = state;
        }

        public void PlayClick()
        {
            pulseTimer =
                clickPulseDuration;
        }

        public void SetColor(Color color)
        {
            onColor = color;
        }

        [ContextMenu("Test / ON")]
        private void TestOn()
        {
            SetOn();
        }

        [ContextMenu("Test / OFF")]
        private void TestOff()
        {
            SetOff();
        }

        [ContextMenu("Test / Click")]
        private void TestClick()
        {
            PlayClick();
        }
    }
}