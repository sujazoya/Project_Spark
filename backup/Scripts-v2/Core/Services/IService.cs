namespace ProjectSpark.Core.Services
{
    /// <summary>
    /// Base interface for all game services.
    /// </summary>
    public interface IService
    {
        void Initialize();

        void Shutdown();
    }
}
