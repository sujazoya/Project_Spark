using UnityEngine;
using UnityEngine.VFX;

namespace AAAUI
{
    [DisallowMultipleComponent]
    public sealed class SignalFlowVFX : MonoBehaviour
    {
        [Header("VFX Graph")]
        [SerializeField]
        private VisualEffect visualEffect;

        [Header("Flow")]
        [SerializeField]
        private bool playing = true;

        [SerializeField, Min(0f)]
        private float speed = 1f;

        [SerializeField, Min(0f)]
        private float intensity = 1f;

        [SerializeField]
        private bool reverse;

        [Header("Pulse")]
        [SerializeField, Min(0f)]
        private float pulseSpeed = 4f;

        [SerializeField, Range(0f, 1f)]
        private float pulseAmount = 0.25f;

        private float time;

        private static readonly int SpeedID =
            Shader.PropertyToID("SignalSpeed");

        private static readonly int IntensityID =
            Shader.PropertyToID("SignalIntensity");

        private static readonly int DirectionID =
            Shader.PropertyToID("SignalDirection");

        private static readonly int PulseID =
            Shader.PropertyToID("SignalPulse");

        private void Awake()
        {
            if (visualEffect == null)
            {
                visualEffect =
                    GetComponent<VisualEffect>();
            }

            Apply();
        }

        private void Update()
        {
            if (!playing ||
                visualEffect == null)
                return;

            time += Time.deltaTime;

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

            visualEffect.SetFloat(
                SpeedID,
                speed
            );

            visualEffect.SetFloat(
                IntensityID,
                intensity
            );

            visualEffect.SetFloat(
                DirectionID,
                reverse ? -1f : 1f
            );

            visualEffect.SetFloat(
                PulseID,
                pulse
            );
        }

        private void Apply()
        {
            if (visualEffect == null)
                return;

            visualEffect.SetFloat(
                SpeedID,
                speed
            );

            visualEffect.SetFloat(
                IntensityID,
                intensity
            );

            visualEffect.SetFloat(
                DirectionID,
                reverse ? -1f : 1f
            );

            visualEffect.SetFloat(
                PulseID,
                1f
            );

            if (playing)
                visualEffect.Play();
            else
                visualEffect.Stop();
        }

        public void Play()
        {
            playing = true;

            if (visualEffect != null)
                visualEffect.Play();
        }

        public void Stop()
        {
            playing = false;

            if (visualEffect != null)
                visualEffect.Stop();
        }

        public void SetSpeed(float value)
        {
            speed = Mathf.Max(0f, value);

            if (visualEffect != null)
            {
                visualEffect.SetFloat(
                    SpeedID,
                    speed
                );
            }
        }

        public void SetIntensity(float value)
        {
            intensity = Mathf.Max(0f, value);

            if (visualEffect != null)
            {
                visualEffect.SetFloat(
                    IntensityID,
                    intensity
                );
            }
        }

        public void SetDirection(bool backwards)
        {
            reverse = backwards;

            if (visualEffect != null)
            {
                visualEffect.SetFloat(
                    DirectionID,
                    reverse ? -1f : 1f
                );
            }
        }
    }
}