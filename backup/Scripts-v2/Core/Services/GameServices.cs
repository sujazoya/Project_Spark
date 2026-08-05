using ProjectSpark.Core.Logging;

namespace ProjectSpark.Core.Services
{
    /// <summary>
    /// Helper class for accessing core services.
    /// </summary>
    public static class GameServices
    {
        public static ILogService Logger =>
            ServiceLocator.Get<ILogService>();
    }
}
