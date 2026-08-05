using System;

namespace ProjectSpark.Gameplay.Placement
{
    public static class PlacementEvents
    {
        public static event Action PlacementStarted;
        public static event Action PlacementCancelled;
        public static event Action PlacementConfirmed;

        public static void RaiseStarted()
            => PlacementStarted?.Invoke();

        public static void RaiseCancelled()
            => PlacementCancelled?.Invoke();

        public static void RaiseConfirmed()
            => PlacementConfirmed?.Invoke();
    }
}
