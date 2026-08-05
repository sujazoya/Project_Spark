using UnityEngine;

namespace AAAUI
{
    [DisallowMultipleComponent]
    public sealed class HolographicVFX : MonoBehaviour
    {
        [Header("Intensity")]
        [SerializeField, Min(0f)]
        private float intensity = 1f;

        [SerializeField, Min(0f)]
        private float pulseSpeed = 2f;

        [Header("Flicker")]
        [SerializeField]
        private bool flicker = true;

        [SerializeField, Range(0f, 1f)]
        private float flickerAmount = 0.08f;

        [SerializeField, Min(0f)]
        private float flickerSpeed = 12f;

        [Header("Scan")]
        [SerializeField]
        private bool scan = true;

        [SerializeField, Min(0f)]
        private float scanSpeed = 1.5f;

        [Header("Runtime")]
        [SerializeField]
        private bool playOnEnable = true;

        private MaterialPropertyBlock propertyBlock;
        private Renderer targetRenderer;
        private float time;
        private bool playing;

        private static readonly int HologramIntensity =
            Shader.PropertyToID("_HologramIntensity");

        private static readonly int HologramPulse =
            Shader.PropertyToID("_HologramPulse");

        private static readonly int HologramFlicker =
            Shader.PropertyToID("_HologramFlicker");

        private static readonly int HologramScan =
            Shader.PropertyToID("_HologramScan");

        public float Intensity => intensity;

        private void Awake()
        {
            propertyBlock =
                new MaterialPropertyBlock();

            targetRenderer =
                GetComponent<Renderer>();
        }

        private void OnEnable()
        {
            playing = playOnEnable;
        }

        private void OnDisable()
        {
            playing = false;
        }

        private void Update()
        {
            if (!playing ||
                targetRenderer == null)
                return;

            time += Time.deltaTime;

            float pulse =
                0.5f +
                0.5f *
                Mathf.Sin(
                    time * pulseSpeed
                );

            float flickerValue = 1f;

            if (flicker)
            {
                float noise =
                    Mathf.PerlinNoise(
                        time * flickerSpeed,
                        0f
                    );

                flickerValue =
                    Mathf.Lerp(
                        1f - flickerAmount,
                        1f,
                        noise
                    );
            }

            float scanValue = 0f;

            if (scan)
            {
                scanValue =
                    Mathf.Repeat(
                        time * scanSpeed,
                        1f
                    );
            }

            targetRenderer.GetPropertyBlock(
                propertyBlock
            );

            propertyBlock.SetFloat(
                HologramIntensity,
                intensity
            );

            propertyBlock.SetFloat(
                HologramPulse,
                pulse
            );

            propertyBlock.SetFloat(
                HologramFlicker,
                flickerValue
            );

            propertyBlock.SetFloat(
                HologramScan,
                scanValue
            );

            targetRenderer.SetPropertyBlock(
                propertyBlock
            );
        }

        public void Play()
        {
            playing = true;
        }

        public void Stop()
        {
            playing = false;
        }

        public void SetIntensity(
            float value)
        {
            intensity =
                Mathf.Max(
                    0f,
                    value
                );
        }
    }
}