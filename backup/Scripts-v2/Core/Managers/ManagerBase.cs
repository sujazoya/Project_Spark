using ProjectSpark.Core.Logging;
using ProjectSpark.Core.Services;
namespace ProjectSpark.Core.Managers
{
    /// <summary>
    /// Base class for every manager.
    /// </summary>
    public abstract class ManagerBase : IManager
    {
        public bool IsInitialized => State == ManagerState.Running;

        public ManagerState State { get; private set; } = ManagerState.None;

        public virtual void Initialize()
        {
            if (State != ManagerState.None &&
                State != ManagerState.Shutdown)
                return;

            State = ManagerState.Initializing;

            OnInitialize();

            State = ManagerState.Running;

            if (ServiceLocator.Exists<ILogService>())
            {
                GameServices.Logger.Log(
                    LogCategory.Managers,
                    this,
                    "Initialized");
            }
        }

        public virtual void Shutdown()
        {
            if (State != ManagerState.Running)
                return;

            State = ManagerState.ShuttingDown;

            OnShutdown();

            State = ManagerState.Shutdown;

            if (ServiceLocator.Exists<ILogService>())
            {
                GameServices.Logger.Log(
                    LogCategory.Managers,
                    this,
                    "Shutdown");
            }
        }

        public virtual void Tick(float deltaTime)
        {

        }

        protected abstract void OnInitialize();

        protected abstract void OnShutdown();
    }
}
