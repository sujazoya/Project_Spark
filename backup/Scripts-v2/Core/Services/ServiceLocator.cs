using System;
using System.Collections.Generic;

namespace ProjectSpark.Core.Services
{
    /// <summary>
    /// Central service registry.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IService> _services = new();

        public static void Register<T>(T service)
            where T : class, IService
        {
            var type = typeof(T);

            if (_services.ContainsKey(type))
                throw new InvalidOperationException($"Service already registered: {type.Name}");

            _services.Add(type, service);

            service.Initialize();
        }

        public static T Get<T>()
            where T : class, IService
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var service))
                return service as T;

            throw new InvalidOperationException($"Service not found: {type.Name}");
        }

        public static bool TryGet<T>(out T service)
            where T : class, IService
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = obj as T;
                return true;
            }

            service = null;
            return false;
        }

        public static bool Exists<T>()
            where T : class, IService
        {
            return _services.ContainsKey(typeof(T));
        }

        public static void Unregister<T>()
            where T : class, IService
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var service))
            {
                service.Shutdown();
                _services.Remove(type);
            }
        }

        public static void Clear()
        {
            foreach (var service in _services.Values)
                service.Shutdown();

            _services.Clear();
        }
    }
}
