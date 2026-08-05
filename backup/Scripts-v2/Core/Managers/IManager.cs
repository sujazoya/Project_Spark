namespace ProjectSpark.Core.Managers
{
    /// <summary>
    /// Base contract for all managers.
    /// </summary>
    public interface IManager
    {
        bool IsInitialized { get; }

        void Initialize();

        void Shutdown();

        void Tick(float deltaTime);
    }
}
