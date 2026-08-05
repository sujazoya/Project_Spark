namespace ProjectSpark.Core.Logging
{
    /// <summary>
    /// Base logger interface.
    /// Every logging implementation must follow this contract.
    /// </summary>
    public interface ILogger
    {
        void Log(string message);

        void LogWarning(string message);

        void LogError(string message);

        void LogException(System.Exception exception);

        void Log(LogCategory category, string message);

        void Log(LogCategory category, object sender, string message);
    }
}
