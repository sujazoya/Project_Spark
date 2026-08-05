using System;

namespace ProjectSpark.Gameplay.Objectives
{
    public static class ObjectiveEvents
    {
        public static event Action<Objective>
            Started;

        public static event Action<Objective>
            Completed;

        public static event Action<Objective>
            Failed;

        public static void RaiseStarted(
            Objective objective)
        {
            Started?.Invoke(objective);
        }

        public static void RaiseCompleted(
            Objective objective)
        {
            Completed?.Invoke(objective);
        }

        public static void RaiseFailed(
            Objective objective)
        {
            Failed?.Invoke(objective);
        }
    }
}
