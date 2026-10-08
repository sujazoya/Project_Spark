
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectSpark.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class SparkTMPTypewriter : MonoBehaviour
    {
        [Serializable]
        public sealed class TextEvent : UnityEvent<string> { }

        [Header("Runtime Text")]
        [SerializeField]
        private bool autoTypeOnEnable = true;

        [Tooltip("Detect text changes made directly to TMP at runtime.")]
        [SerializeField]
        private bool detectExternalTextChanges = true;

        [Tooltip("Wait for rapid text updates to settle before typing.")]
        [SerializeField, Min(0f)]
        private float changeDebounce = 0.08f;

        [Header("Typing Speed")]
        [SerializeField, Min(0.001f)]
        private float secondsPerCharacter = 0.025f;

        [SerializeField, Min(0f)]
        private float startDelay;

        [SerializeField, Min(1f)]
        private float fastMultiplier = 2.5f;

        [SerializeField]
        private bool useUnscaledTime = true;

        [SerializeField, Range(0f, 1f)]
        private float spaceDelayMultiplier = 0.35f;

        [Header("Punctuation Pauses")]
        [SerializeField, Min(0f)]
        private float commaPause = 0.12f;

        [SerializeField, Min(0f)]
        private float sentencePause = 0.32f;

        [SerializeField, Min(0f)]
        private float colonPause = 0.16f;

        [Header("Typing Audio (Optional)")]
        [SerializeField]
        private AudioSource typingAudioSource;

        [SerializeField]
        private AudioClip typingClip;

        [SerializeField, Min(0f)]
        private float audioInterval = 0.035f;

        [SerializeField, Range(0.1f, 3f)]
        private float minimumPitch = 0.95f;

        [SerializeField, Range(0.1f, 3f)]
        private float maximumPitch = 1.05f;

        [SerializeField, Range(0f, 1f)]
        private float typingVolume = 0.5f;

        [Header("Events")]
        [SerializeField]
        private TextEvent onTypingStarted = new TextEvent();

        [SerializeField]
        private TextEvent onTypingCompleted = new TextEvent();

        [SerializeField]
        private UnityEvent onCharacterRevealed = new UnityEvent();

        private TMP_Text textComponent;
        private Coroutine typingCoroutine;

        private string lastObservedText = string.Empty;
        private string currentMessage = string.Empty;

        private float changeTimer;
        private float audioTimer;
        private float speedMultiplier = 1f;
        private float originalAudioPitch = 1f;

        private int visibleCharacters;
        private bool isTyping;
        private bool isInitialized;
        private bool internalTextChange;
        private bool pendingExternalChange;

        public bool IsTyping => isTyping;
        public string CurrentMessage => currentMessage;
        public TMP_Text TextComponent => textComponent;

        public event Action TypingStarted;
        public event Action TypingCompleted;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (isInitialized)
                return;

            textComponent = GetComponent<TMP_Text>();

            if (typingAudioSource != null)
                originalAudioPitch = typingAudioSource.pitch;

            isInitialized = true;
        }

        private void OnEnable()
        {
            Initialize();

            lastObservedText = textComponent.text;

            if (autoTypeOnEnable && !string.IsNullOrEmpty(lastObservedText))
            {
                StartTyping(lastObservedText, false);
            }
            else
            {
                currentMessage = lastObservedText;
                isTyping = false;
            }
        }

        private void LateUpdate()
        {
            if (!detectExternalTextChanges || !isInitialized)
                return;

            if (internalTextChange)
                return;

            string actualText = textComponent.text;

            if (actualText == lastObservedText)
                return;

            lastObservedText = actualText;
            pendingExternalChange = true;
            changeTimer = 0f;

            // External writers may set maxVisibleCharacters themselves.
            // Defer animation until their updates have settled.
        }

        private void Update()
        {
            if (!detectExternalTextChanges || !pendingExternalChange)
                return;

            changeTimer += useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            if (changeTimer < changeDebounce)
                return;

            pendingExternalChange = false;

            // Read the latest value, not an intermediate update.
            string latestText = textComponent.text;
            lastObservedText = latestText;

            if (latestText == currentMessage)
                return;

            StartTyping(latestText, false);
        }

        /// <summary>
        /// Recommended API for Spark AI or any other text producer.
        /// Each new message is typed once.
        /// </summary>
        public void SetText(string message)
        {
            Initialize();

            string newMessage = message ?? string.Empty;

            pendingExternalChange = false;
            lastObservedText = newMessage;

            StartTyping(newMessage, true);
        }

        /// <summary>
        /// Types the current TMP text without requiring a string argument.
        /// </summary>
        public void PlayExistingText()
        {
            Initialize();

            pendingExternalChange = false;
            lastObservedText = textComponent.text;

            StartTyping(lastObservedText, false);
        }

        public void Replay()
        {
            PlayExistingText();
        }

        private void StartTyping(string message, bool assignText)
        {
            StopTypingInternal();

            currentMessage = message ?? string.Empty;
            visibleCharacters = 0;
            audioTimer = 0f;
            speedMultiplier = 1f;

            internalTextChange = assignText;

            if (assignText)
                textComponent.text = currentMessage;

            textComponent.maxVisibleCharacters = int.MaxValue;
            textComponent.ForceMeshUpdate();

            int characterCount = textComponent.textInfo.characterCount;

            internalTextChange = false;

            lastObservedText = textComponent.text;
            textComponent.maxVisibleCharacters = 0;

            isTyping = true;

            onTypingStarted.Invoke(currentMessage);
            TypingStarted?.Invoke();

            if (characterCount == 0)
            {
                CompleteImmediately();
                return;
            }

            typingCoroutine = StartCoroutine(TypeRoutine());
        }

        private IEnumerator TypeRoutine()
        {
            if (startDelay > 0f)
                yield return WaitForDuration(startDelay);

            float elapsed = 0f;

            while (isTyping)
            {
                // If an external writer changes TMP, let the observer
                // handle the new message rather than continuing stale text.
                if (textComponent.text != currentMessage)
                    yield break;

                int count = textComponent.textInfo.characterCount;

                if (visibleCharacters >= count)
                    break;

                elapsed += GetDeltaTime();
                audioTimer += GetDeltaTime();

                float interval =
                    GetCharacterInterval(visibleCharacters)
                    / speedMultiplier;

                if (elapsed < interval)
                {
                    yield return null;
                    continue;
                }

                elapsed = 0f;

                char character = GetCharacter(visibleCharacters);

                visibleCharacters++;
                textComponent.maxVisibleCharacters = visibleCharacters;

                onCharacterRevealed.Invoke();

                if (!char.IsWhiteSpace(character))
                    PlayTypingSound();

                yield return null;
            }

            if (isTyping &&
                textComponent.text == currentMessage)
            {
                CompleteImmediately();
            }
        }

        private IEnumerator WaitForDuration(float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration && isTyping)
            {
                elapsed += GetDeltaTime();
                yield return null;
            }
        }

        private float GetDeltaTime()
        {
            return useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;
        }

        private float GetCharacterInterval(int index)
        {
            float interval = secondsPerCharacter;
            char c = GetCharacter(index);

            if (char.IsWhiteSpace(c))
                interval *= spaceDelayMultiplier;

            switch (c)
            {
                case ',':
                case ';':
                    interval += commaPause;
                    break;

                case '.':
                case '!':
                case '?':
                    interval += sentencePause;
                    break;

                case ':':
                    interval += colonPause;
                    break;
            }

            return Mathf.Max(0.001f, interval);
        }

        private char GetCharacter(int index)
        {
            TMP_TextInfo info = textComponent.textInfo;

            if (index < 0 || index >= info.characterCount)
                return '\0';

            return info.characterInfo[index].character;
        }

        private void PlayTypingSound()
        {
            if (typingAudioSource == null || typingClip == null)
                return;

            if (audioTimer < audioInterval)
                return;

            audioTimer = 0f;

            typingAudioSource.pitch = UnityEngine.Random.Range(
                Mathf.Min(minimumPitch, maximumPitch),
                Mathf.Max(minimumPitch, maximumPitch));

            typingAudioSource.PlayOneShot(typingClip, typingVolume);
        }

        public void Skip()
        {
            if (isTyping)
                CompleteImmediately();
        }

        public void ShowAll()
        {
            Skip();
        }

        public void FastForward()
        {
            if (isTyping)
                speedMultiplier = Mathf.Max(1f, fastMultiplier);
        }

        public void StopTyping()
        {
            StopTypingInternal();
            RestoreAudioPitch();
        }

        public void Clear()
        {
            pendingExternalChange = false;
            StopTypingInternal();
            RestoreAudioPitch();

            currentMessage = string.Empty;
            lastObservedText = string.Empty;
            visibleCharacters = 0;

            internalTextChange = true;
            textComponent.text = string.Empty;
            textComponent.maxVisibleCharacters = int.MaxValue;
            internalTextChange = false;
        }

        public void SetTypingSpeed(float seconds)
        {
            secondsPerCharacter = Mathf.Max(0.001f, seconds);
        }

        private void CompleteImmediately()
        {
            if (!isTyping)
                return;

            isTyping = false;

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            textComponent.maxVisibleCharacters = int.MaxValue;
            visibleCharacters = textComponent.textInfo.characterCount;

            RestoreAudioPitch();

            onTypingCompleted.Invoke(currentMessage);
            TypingCompleted?.Invoke();
        }

        private void StopTypingInternal()
        {
            isTyping = false;

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
        }

        private void RestoreAudioPitch()
        {
            if (typingAudioSource != null)
                typingAudioSource.pitch = originalAudioPitch;
        }

        private void OnDisable()
        {
            pendingExternalChange = false;
            StopTypingInternal();
            RestoreAudioPitch();
        }
    }
}
