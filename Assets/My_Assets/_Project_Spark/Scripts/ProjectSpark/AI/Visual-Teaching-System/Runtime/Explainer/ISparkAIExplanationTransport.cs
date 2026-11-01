using System;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Contract for any EXISTING Spark AI / remote model / local model adapter.
    /// Its completion callback MUST be invoked on Unity's main thread.
    /// Return false when a request could not be dispatched.
    /// The result must be JSON with matching requestId, answer, reasoning, nextCheck.
    /// </summary>
    public interface ISparkAIExplanationTransport
    {
        bool TrySendExplanation(
            string requestId,
            string prompt,
            Action<string, string> onJsonReply,
            Action<string, string> onError);
    }
}
