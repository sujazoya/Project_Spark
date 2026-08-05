using ProjectSpark.Core.Logging;
using ProjectSpark.Core.Services;

namespace ProjectSpark.Core.Managers
{
    /// <summary>
    /// Root manager responsible for game lifecycle.
    /// </summary>
    public sealed class GameManager : ManagerBase
    {
        protected override void OnInitialize()
        {
            GameServices.Logger.Log(
                LogCategory.Managers,
                this,
                "Game Manager Started");
        }

        protected override void OnShutdown()
        {
            GameServices.Logger.Log(
                LogCategory.Managers,
                this,
                "Game Manager Stopped");
        }

        public override void Tick(float deltaTime)
        {
            // Future:
            // Update Game State
            // Pause System
            // Global Timers
            // Analytics
        }
    }
}
