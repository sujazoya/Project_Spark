using UnityEngine;
using UnityEngine.VFX;

namespace AAAUI.VFX
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VisualEffect))]
    public sealed class SignalVFX : MonoBehaviour
    {
        private static readonly int StartPositionID =
            Shader.PropertyToID("StartPosition");

        private static readonly int EndPositionID =
            Shader.PropertyToID("EndPosition");

        private static readonly int SpeedID =
            Shader.PropertyToID("Speed");

        private static readonly int IntensityID =
            Shader.PropertyToID("Intensity");

        private static readonly int ProgressID =
            Shader.PropertyToID("Progress");
        private static readonly int PulsePositionID =
    Shader.PropertyToID("_PulsePosition");

        [Header("Path")]
        [SerializeField]
        private Transform startPoint;

        [SerializeField]
        private Transform endPoint;

        [Header("Signal")]
        [SerializeField, Min(0f)]
        private float speed = 2f;

        [SerializeField, Min(0f)]
        private float intensity = 1f;

        private VisualEffect visualEffect;

        private float progress;

        public bool IsPlaying { get; private set; }

        public float Progress => progress;

        private void Awake()
        {
            visualEffect = GetComponent<VisualEffect>();

            ApplyPath();
            ApplyParameters();
        }

        private void OnEnable()
        {
            ApplyPath();
            ApplyParameters();
        }

        private void Update()
        {
            if (!IsPlaying)
                return;

            if (startPoint == null || endPoint == null)
                return;

            float distance = Vector3.Distance(
                startPoint.position,
                endPoint.position
            );

            if (distance <= 0.001f)
            {
                progress = 1f;
                ApplyProgress();
                return;
            }

            progress += (speed / distance) * Time.deltaTime;

            if (progress >= 1f)
            {
                progress = 1f;
                ApplyProgress();

                IsPlaying = false;
                return;
            }

            ApplyProgress();
        }

        public void Play()
        {
            if (visualEffect == null)
                return;

            ApplyPath();
            ApplyParameters();

            progress = 0f;

            ApplyProgress();

            visualEffect.Reinit();
            visualEffect.Play();

            IsPlaying = true;
        }

        public void Stop()
        {
            if (visualEffect == null)
                return;

            visualEffect.Stop();

            progress = 0f;
            IsPlaying = false;

            ApplyProgress();
        }

        public void Restart()
        {
            Stop();
            Play();
        }

        public void SetStartPoint(Transform point)
        {
            startPoint = point;
            ApplyPath();
        }

        public void SetEndPoint(Transform point)
        {
            endPoint = point;
            ApplyPath();
        }

        public void SetSpeed(float value)
        {
            speed = Mathf.Max(0f, value);

            if (visualEffect != null)
                visualEffect.SetFloat(SpeedID, speed);
        }

        public void SetIntensity(float value)
        {
            intensity = Mathf.Max(0f, value);

            if (visualEffect != null)
                visualEffect.SetFloat(IntensityID, intensity);
        }

        public void SetProgress(float value)
        {
            progress = Mathf.Clamp01(value);
            ApplyProgress();
        }

        private void ApplyPath()
        {
            if (visualEffect == null)
                return;

            if (startPoint != null)
            {
                visualEffect.SetVector3(
                    StartPositionID,
                    startPoint.position
                );
            }

            if (endPoint != null)
            {
                visualEffect.SetVector3(
                    EndPositionID,
                    endPoint.position
                );
            }
        }

        private void ApplyParameters()
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
        }

        private void ApplyProgress()
        {
            if (visualEffect == null)
                return;

            visualEffect.SetFloat(
                ProgressID,
                progress
            );
        }
    }
}