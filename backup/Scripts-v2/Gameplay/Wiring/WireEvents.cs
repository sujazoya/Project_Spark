using System;

namespace ProjectSpark.Gameplay.Wiring
{
    public static class WireEvents
    {
        public static event Action<Wire>
            WireCreated;

        public static event Action<Wire>
            WireDeleted;

        public static event Action<Wire>
            WireUpdated;

        public static void RaiseCreated(
            Wire wire)
        {
            WireCreated?.Invoke(wire);
        }

        public static void RaiseDeleted(
            Wire wire)
        {
            WireDeleted?.Invoke(wire);
        }

        public static void RaiseUpdated(
            Wire wire)
        {
            WireUpdated?.Invoke(wire);
        }
    }
}
