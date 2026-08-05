using System.Collections.Generic;

namespace ProjectSpark.Core.Managers
{
    /// <summary>
    /// Stores and updates all managers.
    /// </summary>
    public sealed class ManagerRegistry
    {
        private readonly List<IManager> _managers = new();

        public void Register(IManager manager)
        {
            if (_managers.Contains(manager))
                return;

            _managers.Add(manager);
        }

        public void InitializeAll()
        {
            foreach (var manager in _managers)
                manager.Initialize();
        }

        public void TickAll(float deltaTime)
        {
            foreach (var manager in _managers)
                manager.Tick(deltaTime);
        }

        public void ShutdownAll()
        {
            for (int i = _managers.Count - 1; i >= 0; i--)
                _managers[i].Shutdown();
        }
    }
}
