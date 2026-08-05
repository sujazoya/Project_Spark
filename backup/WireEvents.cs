// ============================================================================
// WireEvents.cs
// ============================================================================

using System;

namespace ProjectSpark.Gameplay.Wiring
{
    public static class WireEvents
    {
        public static Action<WireController> DragStarted;

        public static Action<WireController> DragEnded;

        public static Action<WireController> Connected;

        public static Action<WireController> Disconnected;

        public static Action CircuitCompleted;
    }
}