using System;
using UnityEngine;

namespace ProjectSpark.Core.Logging
{
    /// <summary>
    /// Central logger used throughout Project Spark.
    /// </summary>
    public sealed class GameLogger : ILogService
    {
        private const string Prefix = "[ProjectSpark]";

        public void Initialize()
        {
            Debug.Log($"{Prefix} Logger Initialized");
        }

        public void Shutdown()
        {
            Debug.Log($"{Prefix} Logger Shutdown");
        }

        public void Log(string message)
        {
            Debug.Log($"{Prefix} {message}");
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning($"{Prefix} {message}");
        }

        public void LogError(string message)
        {
            Debug.LogError($"{Prefix} {message}");
        }

        public void LogException(Exception exception)
        {
            Debug.LogException(exception);
        }

        public void Log(LogCategory category, string message)
        {
            Debug.Log($"{Prefix} [{category}] {message}");
        }

        public void Log(LogCategory category, object sender, string message)
        {
            string senderName = sender != null
                ? sender.GetType().Name
                : "Unknown";

            Debug.Log($"{Prefix} [{category}] [{senderName}] {message}");
        }
      
    }
}