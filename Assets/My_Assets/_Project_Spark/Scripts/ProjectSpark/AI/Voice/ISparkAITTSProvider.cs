namespace ProjectSpark.AI
{
    /// <summary>
    /// Provider interface for Project Spark AI speech synthesis.
    ///
    /// Implementations can use:
    /// - Local/offline TTS
    /// - Windows TTS
    /// - Unity audio clips
    /// - A future neural TTS system
    /// - Another external provider
    ///
    /// The AI teaching system does not need to know which provider
    /// is being used.
    /// </summary>
    public interface ISparkAITTSProvider
    {
        /// <summary>
        /// True when the provider is available and ready.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Starts speech synthesis for the supplied voice request.
        /// </summary>
        bool Speak(
            SparkAIVoiceRequest request);

        /// <summary>
        /// Stops the currently active speech.
        /// </summary>
        void Stop();

        /// <summary>
        /// Releases provider resources.
        /// </summary>
        void Shutdown();
    }
}