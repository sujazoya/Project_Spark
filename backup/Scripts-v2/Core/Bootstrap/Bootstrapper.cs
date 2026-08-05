using UnityEngine;
using ProjectSpark.Core.Logging;
using ProjectSpark.Core.Managers;
using ProjectSpark.Core.Services;

namespace ProjectSpark.Core.Bootstrap
{
    /// <summary>
    /// Root entry point of Project Spark.
    /// This object should exist exactly once in the Bootstrap scene.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public sealed class Bootstrapper : MonoBehaviour
    {
        public static Bootstrapper Instance { get; private set; }

        private ProjectContext _context;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);

            Bootstrap();
        }

        private void Bootstrap()
        {
            _context = new ProjectContext();

            _context.Lifetime = GameLifetime.Bootstrapping;

            ApplicationInitializer.Configure();

            RegisterServices();

            RegisterManagers();

            InitializeManagers();

            _context.Lifetime = GameLifetime.Running;

            GameServices.Logger.Log(
            LogCategory.Bootstrap,
            this,
            "Project Spark Boot Complete");
        }

        private void Update()
        {
            if (_context == null)
                return;

            _context.Managers.TickAll(Time.deltaTime);
        }

        private void OnApplicationQuit()
        {
            _context.Managers.ShutdownAll();

            ServiceLocator.Clear();
        }

        private void RegisterServices()
        {
            ServiceLocator.Register<ILogService>(
                new GameLogger());
        }

        private void RegisterManagers()
        {
            _context.Managers.Register(
                new GameManager());
        }

        private void InitializeManagers()
        {
            _context.Managers.InitializeAll();
        }
    }
}
