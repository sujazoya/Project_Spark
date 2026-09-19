using UnityEngine;
using UnityEngine.VFX;

namespace AAAUI.VFX
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VisualEffect))]
    public sealed class WorldVFX : MonoBehaviour
    {
        private static readonly int IntensityID = Shader.PropertyToID("Intensity");
        private static readonly int SpeedID = Shader.PropertyToID("Speed");
        private static readonly int ProgressID = Shader.PropertyToID("Progress");

        [Header("Playback")]
        [SerializeField]
        private bool playOnEnable;

        [SerializeField]
        private bool loop = true;

        [Header("Parameters")]
        [SerializeField, Min(0f)]
        private float intensity = 1f;

        [SerializeField, Min(0f)]
        private float speed = 1f;


        [SerializeField]
        private VisualEffect visualEffect;

        public bool IsPlaying { get; private set; }

        public bool IsPaused { get; private set; }

        public float Intensity => intensity;

        public float Speed => speed;

        private void Awake()
        {
            visualEffect = GetComponent<VisualEffect>();

            ApplyParameters();
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void OnDisable()
        {
            IsPlaying = false;
            IsPaused = false;
        }

        public void Play()
        {
            if (visualEffect == null)
                return;

            IsPaused = false;
            IsPlaying = true;

            visualEffect.Reinit();

            if (loop)
                visualEffect.Play();
            else
                visualEffect.Play();
        }

        public void Stop()
        {
            if (visualEffect == null)
                return;

            visualEffect.Stop();

            IsPlaying = false;
            IsPaused = false;
        }

        public void Restart()
        {
            Stop();
            Play();
        }

        public void Pause()
        {
            if (!IsPlaying || IsPaused)
                return;

            if (visualEffect == null)
                return;

            visualEffect.pause = true;
            IsPaused = true;
        }

        public void Resume()
        {
            if (!IsPlaying || !IsPaused)
                return;

            if (visualEffect == null)
                return;

            visualEffect.pause = false;
            IsPaused = false;
        }

        public void SetIntensity(float value)
        {
            intensity = Mathf.Max(0f, value);

            if (visualEffect != null)
                visualEffect.SetFloat(IntensityID, intensity);
        }

        public void SetSpeed(float value)
        {
            speed = Mathf.Max(0f, value);

            if (visualEffect != null)
                visualEffect.SetFloat(SpeedID, speed);
        }

        public void SetProgress(float value)
        {
            if (visualEffect == null)
                return;

            visualEffect.SetFloat(
                ProgressID,
                Mathf.Clamp01(value)
            );
        }

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }

        private void ApplyParameters()
        {
            visualEffect.SetFloat(IntensityID, intensity);
            visualEffect.SetFloat(SpeedID, speed);
        }
    }
}