using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Connects SparkAIVoiceController to an external/local TTS provider.
    ///
    /// Unity cannot serialize interface references directly, so the provider
    /// is assigned as a MonoBehaviour and validated at runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAITTSBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SparkAIVoiceController voiceController;

        [SerializeField]
        private MonoBehaviour providerComponent;

        private ISparkAITTSProvider provider;
        private bool initialized;

        public bool IsInitialized => initialized;

        public bool IsProviderAvailable =>
            provider != null &&
            provider.IsAvailable;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            if (!initialized)
                return;

            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();

            if (provider != null)
                provider.Shutdown();

            provider = null;
            initialized = false;
        }

        private void Initialize()
        {
            initialized = false;
            provider = null;

            if (voiceController == null)
            {
                Debug.LogError(
                    "[AI TTS BRIDGE] Voice Controller is not assigned.",
                    this);

                return;
            }

            if (providerComponent == null)
            {
                Debug.LogError(
                    "[AI TTS BRIDGE] Provider Component is not assigned.",
                    this);

                return;
            }

            provider =
                providerComponent as ISparkAITTSProvider;

            if (provider == null)
            {
                Debug.LogError(
                    "[AI TTS BRIDGE] Provider component does not implement ISparkAITTSProvider. " +
                    $"Component: {providerComponent.GetType().FullName}",
                    providerComponent);

                return;
            }

            initialized = true;

            Debug.Log(
                $"[AI TTS BRIDGE] Initialized provider: " +
                $"{providerComponent.GetType().Name}",
                this);
        }

        private void Subscribe()
        {
            if (voiceController == null)
                return;

            voiceController.VoiceRequestCreated -= HandleVoiceRequestCreated;
            voiceController.VoiceStopped -= HandleVoiceStopped;

            voiceController.VoiceRequestCreated +=
                HandleVoiceRequestCreated;

            voiceController.VoiceStopped +=
                HandleVoiceStopped;
        }

        private void Unsubscribe()
        {
            if (voiceController == null)
                return;

            voiceController.VoiceRequestCreated -=
                HandleVoiceRequestCreated;

            voiceController.VoiceStopped -=
                HandleVoiceStopped;
        }

        private void HandleVoiceRequestCreated(
            SparkAIVoiceRequest request)
        {
            if (!initialized)
                return;

            if (provider == null)
                return;

            if (!request.IsValid)
                return;

            bool accepted =
                provider.Speak(request);

            if (!accepted)
            {
                Debug.LogWarning(
                    "[AI TTS BRIDGE] TTS provider rejected the voice request.",
                    this);
            }
        }

        private void HandleVoiceStopped()
        {
            if (!initialized)
                return;

            if (provider == null)
                return;

            provider.Stop();
        }

        public bool Speak(SparkAIVoiceRequest request)
        {
            if (!initialized)
                return false;

            if (provider == null)
                return false;

            if (!request.IsValid)
                return false;

            return provider.Speak(request);
        }

        public void Stop()
        {
            if (provider == null)
                return;

            provider.Stop();
        }
    }
}