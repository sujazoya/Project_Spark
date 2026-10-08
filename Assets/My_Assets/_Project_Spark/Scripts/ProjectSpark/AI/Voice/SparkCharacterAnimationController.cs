using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Complete visual animation controller for the Project Spark AI character.
    ///
    /// Designed for:
    /// - Unity UI Image
    /// - Transparent sprite animation
    /// - Spark AI character
    /// - Piper TTS AudioSource
    ///
    /// Features:
    /// - Idle breathing
    /// - Automatic blinking
    /// - Audio-driven talking
    /// - Emotions
    /// - Looking left/right
    /// - Head tilt
    /// - Bounce
    /// - Smooth scale and rotation motion
    ///
    /// No Animator is required.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkCharacterAnimationController : MonoBehaviour
    {
        public enum SparkEmotion
        {
            Normal,
            Happy,
            Excited,
            Surprised,
            Thinking,
            Sad,
            Angry
        }

        public enum SparkLookDirection
        {
            Center,
            Left,
            Right
        }

        private enum AnimationMode
        {
            Idle,
            Talk,
            Emotion,
            Look,
            Bounce,
            Blink,
            Wink,
            HeadTilt
        }

        [Header("UI Character")]

        [SerializeField]
        private Image characterImage;

        [SerializeField]
        private RectTransform characterRect;

        [Header("Voice / Audio")]

        [Tooltip("AudioSource playing Spark's generated Piper voice.")]
        [SerializeField]
        private AudioSource voiceAudioSource;

        [Tooltip("Use the AudioSource waveform to drive talking animation.")]
        [SerializeField]
        private bool audioDrivenTalking = true;

        [Tooltip("How strongly audio amplitude affects talking.")]
        [SerializeField]
        [Range(0.1f, 5f)]
        private float audioSensitivity = 2.5f;

        [Tooltip("Smooths the detected voice amplitude.")]
        [SerializeField]
        [Range(1f, 30f)]
        private float audioSmoothing = 12f;

        [Header("Idle Animation")]

        [SerializeField]
        private Sprite[] idleFrames;

        [SerializeField]
        [Range(1f, 30f)]
        private float idleFrameRate = 8f;

        [SerializeField]
        private float idleScaleAmount = 0.015f;

        [SerializeField]
        private float idleRotationAmount = 0.8f;

        [SerializeField]
        private float idleBreathingSpeed = 1.2f;

        [Header("Blink")]

        [SerializeField]
        private Sprite[] blinkFrames;

        [SerializeField]
        private float minimumBlinkDelay = 2.5f;

        [SerializeField]
        private float maximumBlinkDelay = 6f;

        [SerializeField]
        private float blinkFrameRate = 14f;

        [Header("Talking")]

        [SerializeField]
        private Sprite[] talkFrames;

        [SerializeField]
        private float talkFrameRate = 14f;

        [SerializeField]
        private float talkFrameRateBoost = 12f;

        [SerializeField]
        private float minimumTalkAmplitude = 0.015f;

        [Header("Happy")]

        [SerializeField]
        private Sprite[] happyFrames;

        [SerializeField]
        private float happyFrameRate = 10f;

        [Header("Excited")]

        [SerializeField]
        private Sprite[] excitedFrames;

        [SerializeField]
        private float excitedFrameRate = 14f;

        [Header("Surprised")]

        [SerializeField]
        private Sprite[] surprisedFrames;

        [SerializeField]
        private float surprisedFrameRate = 10f;

        [Header("Thinking")]

        [SerializeField]
        private Sprite[] thinkingFrames;

        [SerializeField]
        private float thinkingFrameRate = 7f;

        [Header("Sad")]

        [SerializeField]
        private Sprite[] sadFrames;

        [SerializeField]
        private float sadFrameRate = 7f;

        [Header("Angry")]

        [SerializeField]
        private Sprite[] angryFrames;

        [SerializeField]
        private float angryFrameRate = 10f;

        [Header("Wink")]

        [SerializeField]
        private Sprite[] winkFrames;

        [SerializeField]
        private float winkFrameRate = 12f;

        [Header("Look Left")]

        [SerializeField]
        private Sprite[] lookLeftFrames;

        [SerializeField]
        private float lookLeftFrameRate = 10f;

        [Header("Look Right")]

        [SerializeField]
        private Sprite[] lookRightFrames;

        [SerializeField]
        private float lookRightFrameRate = 10f;

        [Header("Head Tilt")]

        [SerializeField]
        private Sprite[] headTiltFrames;

        [SerializeField]
        private float headTiltFrameRate = 10f;

        [Header("Bounce")]

        [SerializeField]
        private Sprite[] bounceFrames;

        [SerializeField]
        private float bounceFrameRate = 14f;

        [SerializeField]
        private float bounceScaleAmount = 0.035f;

        [SerializeField]
        private float bounceRotationAmount = 2f;

        [Header("Living Motion")]

        [SerializeField]
        private bool enableLivingMotion = true;

        [SerializeField]
        private float livingMotionSpeed = 1.4f;

        [SerializeField]
        private float livingMotionScale = 0.008f;

        [SerializeField]
        private float livingMotionRotation = 0.45f;

        [Header("Animation Behaviour")]

        [SerializeField]
        private bool returnToIdleAfterOneShot = true;

        [SerializeField]
        private bool automaticBlinking = true;

        [SerializeField]
        private bool talkingOverridesEmotion = true;

        private AnimationMode currentMode = AnimationMode.Idle;

        private SparkEmotion currentEmotion = SparkEmotion.Normal;

        private SparkLookDirection currentLookDirection =
            SparkLookDirection.Center;

        private Sprite[] currentFrames;

        private float currentFrameRate;

        private int currentFrame;

        private float frameTimer;

        private float blinkTimer;

        private float nextBlinkTime;

        private float audioAmplitude;

        private float targetAudioAmplitude;

        private float animationTime;

        private bool oneShotAnimation;

        private bool isTalking;

        private Vector3 originalScale;

        private Quaternion originalRotation;

        private float baseRotation;

        private float baseScale = 1f;

        private const int AudioSampleCount = 128;

        private readonly float[] audioSamples =
            new float[AudioSampleCount];

        private void Awake()
        {
            if (characterImage == null)
            {
                characterImage = GetComponent<Image>();
            }

            if (characterRect == null && characterImage != null)
            {
                characterRect = characterImage.rectTransform;
            }

            if (characterRect != null)
            {
                originalScale = characterRect.localScale;
                originalRotation = characterRect.localRotation;
            }

            ScheduleNextBlink();

            SetIdle();
        }

        private void OnEnable()
        {
            animationTime = 0f;
            frameTimer = 0f;
            currentFrame = 0;

            ScheduleNextBlink();
        }

        private void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;

            animationTime += deltaTime;

            UpdateVoiceAmplitude(deltaTime);

            UpdateTalkingState();

            UpdateBlink(deltaTime);

            UpdateAnimation(deltaTime);

            UpdateLivingMotion();

            ApplyTransformMotion();
        }

        // ---------------------------------------------------------
        // VOICE
        // ---------------------------------------------------------

        private void UpdateVoiceAmplitude(float deltaTime)
        {
            if (!audioDrivenTalking ||
                voiceAudioSource == null ||
                !voiceAudioSource.isPlaying)
            {
                targetAudioAmplitude = 0f;

                audioAmplitude = Mathf.MoveTowards(
                    audioAmplitude,
                    0f,
                    audioSmoothing * deltaTime);

                return;
            }

            voiceAudioSource.GetOutputData(
                audioSamples,
                0);

            float sum = 0f;

            for (int i = 0; i < audioSamples.Length; i++)
            {
                float sample = audioSamples[i];

                sum += sample * sample;
            }

            float rms = Mathf.Sqrt(
                sum / audioSamples.Length);

            targetAudioAmplitude =
                Mathf.Clamp01(
                    rms * audioSensitivity);

            audioAmplitude = Mathf.Lerp(
                audioAmplitude,
                targetAudioAmplitude,
                1f - Mathf.Exp(
                    -audioSmoothing * deltaTime));
        }

       private void UpdateTalkingState()
{
    if (voiceAudioSource == null)
    {
        isTalking = false;

        if (currentMode == AnimationMode.Talk)
        {
            SetIdle();
        }

        return;
    }

    if (!voiceAudioSource.isPlaying)
    {
        isTalking = false;

        if (currentMode == AnimationMode.Talk)
        {
            SetIdle();
        }

        return;
    }

    if (!audioDrivenTalking)
    {
        isTalking = true;

        if (currentMode != AnimationMode.Talk)
        {
            StartTalking();
        }

        return;
    }

    isTalking =
        audioAmplitude >= minimumTalkAmplitude;

    if (isTalking)
    {
        if (currentMode != AnimationMode.Talk)
        {
            StartTalking();
        }
    }
    else
    {
        if (currentMode == AnimationMode.Talk)
        {
            SetIdle();
        }
    }
}

        // ---------------------------------------------------------
        // BLINK
        // ---------------------------------------------------------

        private void UpdateBlink(float deltaTime)
        {
            if (!automaticBlinking)
            {
                return;
            }

            if (blinkFrames == null ||
                blinkFrames.Length == 0)
            {
                return;
            }

            blinkTimer += deltaTime;

            if (currentMode == AnimationMode.Blink)
            {
                return;
            }

            if (blinkTimer >= nextBlinkTime)
            {
                PlayBlink();
            }
        }

        private void ScheduleNextBlink()
        {
            blinkTimer = 0f;

            nextBlinkTime = UnityEngine.Random.Range(
                minimumBlinkDelay,
                maximumBlinkDelay);
        }

        private void PlayBlink()
        {
            if (blinkFrames == null ||
                blinkFrames.Length == 0)
            {
                return;
            }

            currentMode = AnimationMode.Blink;

            currentFrames = blinkFrames;

            currentFrameRate = blinkFrameRate;

            currentFrame = 0;

            frameTimer = 0f;

            oneShotAnimation = true;

            ScheduleNextBlink();
        }

        // ---------------------------------------------------------
        // MAIN ANIMATION
        // ---------------------------------------------------------
private void UpdateAnimation(float deltaTime)
{
    if (currentMode == AnimationMode.Idle)
    {
        return;
    }

    if (currentFrames == null ||
        currentFrames.Length == 0)
    {
        return;
    }

    float rate = currentFrameRate;

    if (currentMode == AnimationMode.Talk)
    {
        rate += audioAmplitude * talkFrameRateBoost;
    }

    if (rate <= 0f)
    {
        return;
    }

    frameTimer += deltaTime;

    float frameDuration = 1f / rate;

    if (frameTimer < frameDuration)
    {
        return;
    }

    frameTimer -= frameDuration;

    currentFrame++;

    if (currentFrame >= currentFrames.Length)
    {
        if (oneShotAnimation)
        {
            FinishOneShotAnimation();
            return;
        }

        currentFrame = 0;
    }

    ApplyCurrentFrame();
}
        private void ApplyCurrentFrame()
        {
            if (characterImage == null)
            {
                return;
            }

            if (currentFrames == null ||
                currentFrames.Length == 0)
            {
                return;
            }

            if (currentFrame < 0 ||
                currentFrame >= currentFrames.Length)
            {
                currentFrame = 0;
            }

            Sprite sprite = currentFrames[currentFrame];

            if (sprite != null)
            {
                characterImage.sprite = sprite;
            }
        }

        private void FinishOneShotAnimation()
        {
            if (!returnToIdleAfterOneShot)
            {
                currentFrame =
                    currentFrames.Length - 1;

                ApplyCurrentFrame();

                return;
            }

            SetIdle();
        }

        // ---------------------------------------------------------
        // LIVING MOTION
        // ---------------------------------------------------------

    private void UpdateLivingMotion()
{
    if (!enableLivingMotion)
    {
        baseScale = 1f;
        baseRotation = 0f;
        return;
    }

    float time = animationTime;

    // Very subtle breathing.
    float breathing =
        Mathf.Sin(time * 1.35f);

    // Tiny vertical life movement.
    float floating =
        Mathf.Sin(time * 0.75f) * 0.5f;

    baseScale =
        1f +
        breathing * 0.004f;

    baseRotation =
        floating * 0.18f;
}

        private void ApplyTransformMotion()
        {
            if (characterRect == null)
            {
                return;
            }

            float scale = baseScale;

            float rotation = baseRotation;

            if (currentMode == AnimationMode.Bounce)
            {
                float bounce =
                    Mathf.Sin(
                        animationTime * 12f);

                scale +=
                    Mathf.Abs(bounce) *
                    bounceScaleAmount;

                rotation +=
                    bounce *
                    bounceRotationAmount;
            }

            characterRect.localScale =
                originalScale * scale;

            characterRect.localRotation =
                originalRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    rotation);
        }

        // ---------------------------------------------------------
        // IDLE
        // ---------------------------------------------------------

            public void SetIdle()
        {
            currentMode = AnimationMode.Idle;

            currentFrames = null;

            currentFrameRate = 0f;

            currentFrame = 0;

            frameTimer = 0f;

            oneShotAnimation = false;

            ApplyIdleSprite();
        }
        private void ApplyIdleSprite()
        {
            if (characterImage == null)
            {
                return;
            }

            if (idleFrames == null ||
                idleFrames.Length == 0)
            {
                return;
            }

            // IMPORTANT:
            // Always use the first idle frame.
            // Never cycle idle mouth/expression frames.
            Sprite idleSprite = idleFrames[0];

            if (idleSprite != null)
            {
                characterImage.sprite = idleSprite;
            }
        }

        // ---------------------------------------------------------
        // TALK
        // ---------------------------------------------------------

        public void StartTalking()
        {
            isTalking = true;

            if (talkFrames == null ||
                talkFrames.Length == 0)
            {
                return;
            }

            currentMode = AnimationMode.Talk;

            currentFrames = talkFrames;

            currentFrameRate = talkFrameRate;

            currentFrame = 0;

            frameTimer = 0f;

            oneShotAnimation = false;

            ApplyCurrentFrame();
        }

        public void StopTalking()
        {
            isTalking = false;

            SetIdle();
        }

        // ---------------------------------------------------------
        // EMOTIONS
        // ---------------------------------------------------------

        public void SetEmotion(
            SparkEmotion emotion)
        {
            currentEmotion = emotion;

            switch (emotion)
            {
                case SparkEmotion.Normal:
                    SetIdle();
                    break;

                case SparkEmotion.Happy:
                    PlayLoop(
                        happyFrames,
                        happyFrameRate);
                    break;

                case SparkEmotion.Excited:
                    PlayLoop(
                        excitedFrames,
                        excitedFrameRate);
                    break;

                case SparkEmotion.Surprised:
                    PlayLoop(
                        surprisedFrames,
                        surprisedFrameRate);
                    break;

                case SparkEmotion.Thinking:
                    PlayLoop(
                        thinkingFrames,
                        thinkingFrameRate);
                    break;

                case SparkEmotion.Sad:
                    PlayLoop(
                        sadFrames,
                        sadFrameRate);
                    break;

                case SparkEmotion.Angry:
                    PlayLoop(
                        angryFrames,
                        angryFrameRate);
                    break;
            }
        }

        private void PlayLoop(
            Sprite[] frames,
            float frameRate)
        {
            if (frames == null ||
                frames.Length == 0)
            {
                SetIdle();
                return;
            }

            currentMode = AnimationMode.Emotion;

            currentFrames = frames;

            currentFrameRate = frameRate;

            currentFrame = 0;

            frameTimer = 0f;

            oneShotAnimation = false;

            ApplyCurrentFrame();
        }

        // ---------------------------------------------------------
        // LOOK
        // ---------------------------------------------------------

        public void LookLeft()
        {
            currentLookDirection =
                SparkLookDirection.Left;

            PlayOneShot(
                lookLeftFrames,
                lookLeftFrameRate,
                AnimationMode.Look);
        }

        public void LookRight()
        {
            currentLookDirection =
                SparkLookDirection.Right;

            PlayOneShot(
                lookRightFrames,
                lookRightFrameRate,
                AnimationMode.Look);
        }

        public void LookCenter()
        {
            currentLookDirection =
                SparkLookDirection.Center;

            SetIdle();
        }

        public void SetLookDirection(
            SparkLookDirection direction)
        {
            switch (direction)
            {
                case SparkLookDirection.Left:
                    LookLeft();
                    break;

                case SparkLookDirection.Right:
                    LookRight();
                    break;

                default:
                    LookCenter();
                    break;
            }
        }

        // ---------------------------------------------------------
        // WINK
        // ---------------------------------------------------------

        public void Wink()
        {
            PlayOneShot(
                winkFrames,
                winkFrameRate,
                AnimationMode.Wink);
        }

        // ---------------------------------------------------------
        // HEAD TILT
        // ---------------------------------------------------------

        public void HeadTilt()
        {
            PlayOneShot(
                headTiltFrames,
                headTiltFrameRate,
                AnimationMode.HeadTilt);
        }

        // ---------------------------------------------------------
        // BOUNCE
        // ---------------------------------------------------------

        public void Bounce()
        {
            PlayOneShot(
                bounceFrames,
                bounceFrameRate,
                AnimationMode.Bounce);
        }

        // ---------------------------------------------------------
        // GENERIC ONE-SHOT
        // ---------------------------------------------------------

        private void PlayOneShot(
            Sprite[] frames,
            float frameRate,
            AnimationMode mode)
        {
            if (frames == null ||
                frames.Length == 0)
            {
                return;
            }

            currentMode = mode;

            currentFrames = frames;

            currentFrameRate = frameRate;

            currentFrame = 0;

            frameTimer = 0f;

            oneShotAnimation = true;

            ApplyCurrentFrame();
        }

        // ---------------------------------------------------------
        // AI-FRIENDLY COMMANDS
        // ---------------------------------------------------------

        /// <summary>
        /// Makes Spark show a thinking expression.
        /// </summary>
        public void Think()
        {
            SetEmotion(
                SparkEmotion.Thinking);
        }

        /// <summary>
        /// Makes Spark appear happy.
        /// </summary>
        public void Smile()
        {
            SetEmotion(
                SparkEmotion.Happy);
        }

        /// <summary>
        /// Makes Spark appear excited.
        /// </summary>
        public void Excited()
        {
            SetEmotion(
                SparkEmotion.Excited);
        }

        /// <summary>
        /// Makes Spark react with surprise.
        /// </summary>
        public void Surprised()
        {
            SetEmotion(
                SparkEmotion.Surprised);
        }

        /// <summary>
        /// Makes Spark appear sad.
        /// </summary>
        public void Sad()
        {
            SetEmotion(
                SparkEmotion.Sad);
        }

        /// <summary>
        /// Makes Spark appear angry.
        /// </summary>
        public void Angry()
        {
            SetEmotion(
                SparkEmotion.Angry);
        }

        /// <summary>
        /// Makes Spark return to normal idle behaviour.
        /// </summary>
        public void Normal()
        {
            currentEmotion =
                SparkEmotion.Normal;

            SetIdle();
        }

        /// <summary>
        /// Plays a short visual reaction.
        /// </summary>
        public void React()
        {
            Bounce();
        }

        // ---------------------------------------------------------
        // EXTERNAL AUDIO CONTROL
        // ---------------------------------------------------------

        /// <summary>
        /// Assigns the voice AudioSource at runtime.
        /// Useful when the Spark voice system creates or changes
        /// the AudioSource dynamically.
        /// </summary>
        public void SetVoiceAudioSource(
            AudioSource source)
        {
            voiceAudioSource = source;
        }

        /// <summary>
        /// Returns the current normalized voice amplitude.
        /// </summary>
        public float GetVoiceAmplitude()
        {
            return audioAmplitude;
        }

        /// <summary>
        /// Returns true while Spark's voice is playing.
        /// </summary>
        public bool IsTalking()
        {
            if (voiceAudioSource == null)
            {
                return false;
            }

            return voiceAudioSource.isPlaying;
        }

        // ---------------------------------------------------------
        // SETTINGS
        // ---------------------------------------------------------

        public void SetAutomaticBlinking(
            bool enabled)
        {
            automaticBlinking = enabled;

            if (enabled)
            {
                ScheduleNextBlink();
            }
        }

        public void SetLivingMotion(
            bool enabled)
        {
            enableLivingMotion = enabled;

            if (!enabled &&
                characterRect != null)
            {
                characterRect.localScale =
                    originalScale;

                characterRect.localRotation =
                    originalRotation;
            }
        }

        // ---------------------------------------------------------
        // DEBUG / VALIDATION
        // ---------------------------------------------------------

        private void OnValidate()
        {
            minimumBlinkDelay =
                Mathf.Max(
                    0.1f,
                    minimumBlinkDelay);

            maximumBlinkDelay =
                Mathf.Max(
                    minimumBlinkDelay,
                    maximumBlinkDelay);

            idleFrameRate =
                Mathf.Max(
                    0.1f,
                    idleFrameRate);

            talkFrameRate =
                Mathf.Max(
                    0.1f,
                    talkFrameRate);

            blinkFrameRate =
                Mathf.Max(
                    0.1f,
                    blinkFrameRate);
        }
    }
}