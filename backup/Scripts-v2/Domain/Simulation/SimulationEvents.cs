using System;

namespace ProjectSpark.Domain.Simulation
{
    public static class SimulationEvents
    {
        public static event Action TickStarted;

        public static event Action TickFinished;

        public static void RaiseTickStarted()
        {
            TickStarted?.Invoke();
        }

        public static void RaiseTickFinished()
        {
            TickFinished?.Invoke();
        }
    }
}
