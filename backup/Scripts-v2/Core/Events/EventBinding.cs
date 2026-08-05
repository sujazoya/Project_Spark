using System;

namespace ProjectSpark.Core.Events
{
    public sealed class EventBinding<T> where T : IEvent
    {
        public Action<T> Action { get; }

        public EventBinding(Action<T> action)
        {
            Action = action;
        }
    }
}
