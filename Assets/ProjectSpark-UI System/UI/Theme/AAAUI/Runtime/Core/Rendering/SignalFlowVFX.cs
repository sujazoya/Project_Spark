using UnityEngine;

namespace AAAUI
{
    [DisallowMultipleComponent]
    public sealed class SignalFlowVFX : MonoBehaviour
    {
        [Header("Flow")]
        [SerializeField]
        private bool playing = true;

        [SerializeField, Min(0f)]
        private float speed = 1f;

        [SerializeField, Min(0f)]
        private float intensity = 1f;

        [SerializeField]
        private bool reverse;

        [Header("Polarity")]
        [SerializeField]
        private Color positiveColor =
            new Color(1f, 0.03f, 0.03f, 0.40f);

        [SerializeField]
        private Color negativeColor =
            new Color(0.03f, 0.20f, 1f, 0.40f);

        [Header("Pulse")]
        [SerializeField, Min(0f)]
        private float pulseSpeed = 4f;

        [SerializeField, Range(0f, 1f)]
        private float pulseAmount = 0.25f;

        private MaterialPropertyBlock propertyBlock;
        private Renderer targetRenderer;

        private float time;

        private bool positivePolarity = true;

        [Header("Wire Materials")]
        [SerializeField]
        private Material positiveMaterial;

        [SerializeField]
        private Material negativeMaterial;

        private static readonly int FlowOffset =
            Shader.PropertyToID("_SignalFlowOffset");

        private static readonly int FlowIntensity =
            Shader.PropertyToID("_SignalFlowIntensity");

        private static readonly int FlowPulse =
            Shader.PropertyToID("_SignalFlowPulse");

        private static readonly int WireColor =
            Shader.PropertyToID("_WireColor");

        private void Awake()
        {
            propertyBlock =
                new MaterialPropertyBlock();

            targetRenderer =
                GetComponent<Renderer>();

            ApplyColor();
        }
        public void SetPolarity(bool positive)
        {
            if (targetRenderer == null)
                return;

            targetRenderer.sharedMaterial =
                positive
                    ? positiveMaterial
                    : negativeMaterial;
        }
      
        private void Update()
        {
            if (!playing ||
                targetRenderer == null)
                return;

            time += Time.deltaTime;

            float direction =
                reverse ? -1f : 1f;

            float offset =
                time *
                speed *
                direction;

            float pulse =
                0.5f +
                0.5f *
                Mathf.Sin(
                    time * pulseSpeed
                );

            pulse =
                Mathf.Lerp(
                    1f,
                    pulse,
                    pulseAmount
                );

            targetRenderer.GetPropertyBlock(
                propertyBlock
            );

            propertyBlock.SetFloat(
                FlowOffset,
                offset
            );

            propertyBlock.SetFloat(
                FlowIntensity,
                intensity
            );

            propertyBlock.SetFloat(
                FlowPulse,
                pulse
            );

            propertyBlock.SetColor(
                WireColor,
                positivePolarity
                    ? positiveColor
                    : negativeColor
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

        public void SetSpeed(float value)
        {
            speed =
                Mathf.Max(
                    0f,
                    value
                );
        }

        public void SetIntensity(float value)
        {
            intensity =
                Mathf.Max(
                    0f,
                    value
                );
        }

        public void SetDirection(bool backwards)
        {
            reverse = backwards;
        }

      

        private void ApplyColor()
        {
            if (targetRenderer == null)
                return;

            targetRenderer.GetPropertyBlock(
                propertyBlock
            );

            propertyBlock.SetColor(
                WireColor,
                positivePolarity
                    ? positiveColor
                    : negativeColor
            );

            targetRenderer.SetPropertyBlock(
                propertyBlock
            );
        }
    }
}