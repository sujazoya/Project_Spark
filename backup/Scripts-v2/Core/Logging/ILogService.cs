using ProjectSpark.Core.Services;

namespace ProjectSpark.Core.Logging
{
    public interface ILogService : IService
    {
        void Log(
            LogCategory category,
            object sender,
           string message);    
            void Log(
            LogCategory category,
            string message);
            }
}