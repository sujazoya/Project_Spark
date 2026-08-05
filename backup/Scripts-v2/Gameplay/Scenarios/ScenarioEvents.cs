using System;

namespace ProjectSpark.Gameplay.Scenarios
{
    public static class ScenarioEvents
    {
        public static event Action
            ScenarioStarted;

        public static event Action
            ScenarioCompleted;

        public static event Action
            ScenarioFailed;

        public static void RaiseStarted()
        {
            ScenarioStarted?.Invoke();
        }

        public static void RaiseCompleted()
        {
            ScenarioCompleted?.Invoke();
        }

        public static void RaiseFailed()
        {
            ScenarioFailed?.Invoke();
        }
    }
}
