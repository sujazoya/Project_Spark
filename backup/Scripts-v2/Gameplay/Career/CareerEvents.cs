using System;

namespace ProjectSpark.Gameplay.Career
{
    public static class CareerEvents
    {
        public static event Action<WorkOrder>
            WorkAccepted;

        public static event Action<WorkOrder>
            WorkCompleted;

        public static event Action<int>
            ReputationChanged;

        public static void RaiseAccepted(
            WorkOrder order)
        {
            WorkAccepted?.Invoke(order);
        }

        public static void RaiseCompleted(
            WorkOrder order)
        {
            WorkCompleted?.Invoke(order);
        }

        public static void RaiseReputation(
            int value)
        {
            ReputationChanged?.Invoke(value);
        }
    }
}
