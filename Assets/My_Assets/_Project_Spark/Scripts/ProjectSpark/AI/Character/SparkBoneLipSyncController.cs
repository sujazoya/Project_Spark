
using UnityEngine;

namespace ProjectSpark.AI.Character
{
    /// <summary>
    /// Audio-amplitude-driven lip animation for a bone-based character
    /// with separate upper and lower lip bones and no jaw bone.
    ///
    /// Uses an existing AudioSource. Does not create or play audio.
    /// Bone references and audio samples are cached to avoid recurring
    /// managed allocations in the animation loop.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkBoneLipSyncController : MonoBehaviour
    {
        [Header("Voice Source")]
        [SerializeField]
        private AudioSource voiceSource;

        [Header("Lip Bones")]
        [SerializeField]
        private Transform upperLipBone;

        [SerializeField]
        private Transform lowerLipBone;

        [Header("Upper Lip Rotation")]
        [Tooltip("Local rotation axis relative to the upper lip's rest pose.")]
        [SerializeField]
        private Vector3 upperLipLocalAxis = Vector3.right;

        [Tooltip("Signed angle in degrees at full mouth opening.")]
        [SerializeField, Range(-45f, 45f)]
        private float upperLipOpenAngle = -8f;

        [Header("Lower Lip Rotation")]
        [Tooltip("Local rotation axis relative to the lower lip's rest pose.")]
        [SerializeField]
        private Vector3 lowerLipLocalAxis = Vector3.right;

        [Tooltip("Signed angle in degrees at full mouth opening.")]
        [SerializeField, Range(-60f, 60f)]
        private float lowerLipOpenAngle = 20f;

        [Header("Audio Analysis")]
        [Tooltip("Number of audio samples used for RMS analysis.")]
        [SerializeField, Range(64, 1024)]
        private int sampleCount = 256;

        [Tooltip("Audio RMS below this value is treated as silence.")]
        [SerializeField, Min(0f)]
        private float silenceThreshold = 0.008f;

        [Tooltip("Multiplier applied to the signal above the threshold.")]
        [SerializeField, Min(0.1f)]
        private float audioGain = 12f;

        [Tooltip("Higher values reduce small mouth movements.")]
        [SerializeField, Range(0.5f, 3f)]
        private float responsePower = 1.2f;

        [Header("Smoothing")]
        [SerializeField, Min(0.01f)]
        private float openingSmoothTime = 0.06f;

        [SerializeField, Min(0.01f)]
        private float closingSmoothTime = 0.045f;

        [Header("Behaviour")]
        [SerializeField]
        private bool animateWhilePlaying = true;

        [SerializeField]
        private bool animateInUnscaledTime = false;

        [Header("Diagnostics")]
        [SerializeField]
        private bool showDebugValues;

        private const float SpeakingThreshold = 0.035f;

        private float[] audioSamples;
        private Quaternion upperLipRestRotation;
        private Quaternion lowerLipRestRotation;

        private float mouthOpen;
        private float mouthVelocity;
        private float currentRms;
        private bool initialized;

        /// <summary>True when the audio signal produces meaningful mouth movement.</summary>
        public bool IsSpeaking { get; private set; }

        /// <summary>Normalized mouth opening, from zero to one.</summary>
        public float MouthOpen => mouthOpen;

        /// <summary>Measured audio RMS for optional diagnostics.</summary>
        public float AudioRms => currentRms;

        /// <summary>The AudioSource currently monitored by this controller.</summary>
        public AudioSource VoiceSource => voiceSource;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            int count = Mathf.Clamp(sampleCount, 64, 1024);

            if (audioSamples == null || audioSamples.Length != count)
            {
                audioSamples = new float[count];
            }

            if (upperLipBone != null)
            {
                upperLipRestRotation = upperLipBone.localRotation;
            }

            if (lowerLipBone != null)
            {
                lowerLipRestRotation = lowerLipBone.localRotation;
            }

            mouthOpen = 0f;
            mouthVelocity = 0f;
            currentRms = 0f;
            IsSpeaking = false;
            initialized = true;
        }

        /// <summary>
        /// Assign an existing voice AudioSource at runtime.
        /// </summary>
        public void SetVoiceSource(AudioSource source)
        {
            voiceSource = source;
        }

        /// <summary>
        /// Enable or disable audio-driven lip animation.
        /// Disabling animation returns the lips to their cached rest pose.
        /// </summary>
        public void SetLipSyncEnabled(bool enabled)
        {
            animateWhilePlaying = enabled;

            if (!enabled)
            {
                ResetMouth();
            }
        }

        /// <summary>
        /// Recapture the current lip rotations as their neutral rest pose.
        /// Call only when the lips are in the desired neutral position.
        /// </summary>
        public void CaptureRestPose()
        {
            if (upperLipBone != null)
            {
                upperLipRestRotation = upperLipBone.localRotation;
            }

            if (lowerLipBone != null)
            {
                lowerLipRestRotation = lowerLipBone.localRotation;
            }

            mouthOpen = 0f;
            mouthVelocity = 0f;
        }

        private void LateUpdate()
        {
            if (!initialized)
            {
                return;
            }

            if (!animateWhilePlaying ||
                voiceSource == null ||
                !voiceSource.isPlaying)
            {
                currentRms = 0f;
                IsSpeaking = false;
                AnimateToward(0f);
                return;
            }

            AnalyzeAudio();
            AnimateToward(CalculateMouthTarget());
        }

        private void AnalyzeAudio()
        {
            if (audioSamples == null || audioSamples.Length == 0)
            {
                currentRms = 0f;
                return;
            }

            voiceSource.GetOutputData(audioSamples, 0);

            float sumSquares = 0f;

            for (int i = 0; i < audioSamples.Length; i++)
            {
                float sample = audioSamples[i];
                sumSquares += sample * sample;
            }

            currentRms = Mathf.Sqrt(sumSquares / audioSamples.Length);
        }

        private float CalculateMouthTarget()
        {
            float signal = Mathf.Max(0f, currentRms - silenceThreshold);

            float normalized = Mathf.Clamp01(signal * audioGain);

            float target = Mathf.Pow(normalized, responsePower);

            IsSpeaking = target >= SpeakingThreshold;

            return target;
        }

        private void AnimateToward(float target)
        {
            float deltaTime = animateInUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            float smoothTime = target > mouthOpen
                ? openingSmoothTime
                : closingSmoothTime;

            if (deltaTime > 0f)
            {
                mouthOpen = Mathf.SmoothDamp(
                    mouthOpen,
                    target,
                    ref mouthVelocity,
                    smoothTime,
                    Mathf.Infinity,
                    deltaTime);
            }

            mouthOpen = Mathf.Clamp01(mouthOpen);

            ApplyLipRotations();
        }

        private void ApplyLipRotations()
        {
            if (upperLipBone != null)
            {
                upperLipBone.localRotation =
                    upperLipRestRotation *
                    Quaternion.AngleAxis(
                        upperLipOpenAngle * mouthOpen,
                        GetSafeAxis(upperLipLocalAxis));
            }

            if (lowerLipBone != null)
            {
                lowerLipBone.localRotation =
                    lowerLipRestRotation *
                    Quaternion.AngleAxis(
                        lowerLipOpenAngle * mouthOpen,
                        GetSafeAxis(lowerLipLocalAxis));
            }
        }

        private static Vector3 GetSafeAxis(Vector3 axis)
        {
            if (axis.sqrMagnitude < 0.0001f)
            {
                return Vector3.right;
            }

            return axis.normalized;
        }

        private void ResetMouth()
        {
            mouthOpen = 0f;
            mouthVelocity = 0f;
            currentRms = 0f;
            IsSpeaking = false;

            ApplyLipRotations();
        }

        private void OnDisable()
        {
            if (!initialized)
            {
                return;
            }

            ResetMouth();
        }

        private void OnValidate()
        {
            sampleCount = Mathf.Clamp(sampleCount, 64, 1024);
            silenceThreshold = Mathf.Max(0f, silenceThreshold);
            audioGain = Mathf.Max(0.1f, audioGain);
            responsePower = Mathf.Clamp(responsePower, 0.5f, 3f);
            openingSmoothTime = Mathf.Max(0.01f, openingSmoothTime);
            closingSmoothTime = Mathf.Max(0.01f, closingSmoothTime);
        }

        private void OnGUI()
        {
            if (!showDebugValues || !Application.isPlaying)
            {
                return;
            }

            GUI.Label(
                new Rect(12f, 12f, 340f, 22f),
                $"Spark Lip Sync | RMS: {currentRms:F4} | Mouth: {mouthOpen:F2} | Speaking: {IsSpeaking}");
        }
    }
}
