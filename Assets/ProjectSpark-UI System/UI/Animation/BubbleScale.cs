
using UnityEngine;

namespace ProjectSpark
{
    /// <summary>
    /// Produces a soft, organic bubble-like scale animation.
    /// Designed for holographic UI, selection effects and scientific visualization.
    /// </summary>
    public sealed class BubbleScale : MonoBehaviour
    {
        [Header("Bubble Scale")]
        [SerializeField]
        private float scaleAmount = 0.12f;

        [SerializeField]
        private float speed = 1.5f;

        [Header("Organic Shape")]
        [SerializeField]
        private float verticalStretch = 0.035f;

        [SerializeField]
        private float horizontalSquash = 0.02f;

        [Header("Wobble")]
        [SerializeField]
        private float wobbleAmount = 0.015f;

        [SerializeField]
        private float wobbleSpeed = 1.2f;

        [Header("Animation")]
        [SerializeField]
        private bool playOnEnable = true;

        [SerializeField]
        private bool unscaledTime = true;

        private Vector3 baseScale;
        private float time;
        private bool playing;

        private void Awake()
        {
            baseScale = transform.localScale;
            playing = playOnEnable;
        }

        private void OnEnable()
        {
            if (baseScale == Vector3.zero)
            {
                baseScale = transform.localScale;
            }

            if (playOnEnable)
            {
                ResetAnimation();
                Play();
            }
        }

        private void Update()
        {
            if (!playing)
            {
                return;
            }

            float deltaTime = unscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            time += deltaTime;

            float mainWave =
                Mathf.Sin(time * speed * Mathf.PI * 2f);

            /*
             * Smooth bubble pulse.
             * Remapping the sine wave keeps the movement soft.
             */
            float pulse =
                mainWave * 0.5f + 0.5f;

            float smoothPulse =
                pulse * pulse * (3f - 2f * pulse);

            /*
             * Main expansion.
             */
            float expansion =
                Mathf.Lerp(
                    1f - scaleAmount,
                    1f + scaleAmount,
                    smoothPulse
                );

            /*
             * Slight vertical stretching during expansion.
             */
            float stretch =
                Mathf.Sin(time * speed * Mathf.PI * 2f);

            float yScale =
                expansion +
                stretch * verticalStretch;

            /*
             * Slight X/Z compression while Y stretches.
             */
            float squash =
                expansion -
                stretch * horizontalSquash;

            /*
             * Organic wobble.
             */
            float wobbleX =
                Mathf.Sin(time * wobbleSpeed * 2.13f) *
                wobbleAmount;

            float wobbleZ =
                Mathf.Cos(time * wobbleSpeed * 1.71f) *
                wobbleAmount;

            Vector3 scale = new Vector3(
                squash + wobbleX,
                yScale,
                squash + wobbleZ
            );

            transform.localScale =
                Vector3.Scale(
                    baseScale,
                    scale
                );
        }

        /// <summary>
        /// Starts the bubble animation.
        /// </summary>
        public void Play()
        {
            playing = true;
        }

        /// <summary>
        /// Stops the bubble animation and restores the original scale.
        /// </summary>
        public void Stop()
        {
            playing = false;
            transform.localScale = baseScale;
        }

        /// <summary>
        /// Restarts the bubble animation from the beginning.
        /// </summary>
        public void ResetAnimation()
        {
            time = 0f;
            transform.localScale = baseScale;
        }

        /// <summary>
        /// Sets the animation intensity.
        /// </summary>
        public void SetIntensity(float amount)
        {
            scaleAmount = Mathf.Max(0f, amount);
        }

        /// <summary>
        /// Sets the animation speed.
        /// </summary>
        public void SetSpeed(float value)
        {
            speed = Mathf.Max(0f, value);
        }
    }
}