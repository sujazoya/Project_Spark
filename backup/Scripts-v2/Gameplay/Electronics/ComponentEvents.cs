using System;

namespace ProjectSpark.Gameplay.Electronics
{
    public static class ComponentEvents
    {
        public static event Action<ElectronicComponent> Added;
        public static event Action<ElectronicComponent> Removed;
        public static event Action<ElectronicComponent> Changed;

        public static void RaiseAdded(ElectronicComponent c)
            => Added?.Invoke(c);

        public static void RaiseRemoved(ElectronicComponent c)
            => Removed?.Invoke(c);

        public static void RaiseChanged(ElectronicComponent c)
            => Changed?.Invoke(c);
    }
}
