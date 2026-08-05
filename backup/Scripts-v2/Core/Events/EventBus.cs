using System;
using System.Collections.Generic;

namespace ProjectSpark.Core.Events
{
    /// <summary>
    /// Global event dispatcher.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Subscribers = new();

        public static void Subscribe<T>(Action<T> callback)
            where T : IEvent
        {
            var type = typeof(T);

            if (!Subscribers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                Subscribers.Add(type, list);
            }

            if (!list.Contains(callback))
                list.Add(callback);
        }

        public static void Unsubscribe<T>(Action<T> callback)
            where T : IEvent
        {
            var type = typeof(T);

            if (Subscribers.TryGetValue(type, out var list))
                list.Remove(callback);
        }

        public static void Publish<T>(T gameEvent)
            where T : IEvent
        {
            var type = typeof(T);

            if (!Subscribers.TryGetValue(type, out var list))
                return;

            // Copy to avoid modification during iteration
            var listeners = list.ToArray();

            foreach (var listener in listeners)
            {
                ((Action<T>)listener)?.Invoke(gameEvent);
            }
        }

        public static void Clear()
        {
            Subscribers.Clear();
        }
    }
}
