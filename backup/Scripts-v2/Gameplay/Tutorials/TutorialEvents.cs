using System;

namespace ProjectSpark.Gameplay.Tutorials
{
    public static class TutorialEvents
    {
        public static event Action<TutorialStep>
            StepStarted;

        public static event Action<TutorialStep>
            StepCompleted;

        public static event Action
            TutorialFinished;

        public static void RaiseStarted(
            TutorialStep step)
        {
            StepStarted?.Invoke(step);
        }

        public static void RaiseCompleted(
            TutorialStep step)
        {
            StepCompleted?.Invoke(step);
        }

        public static void RaiseFinished()
        {
            TutorialFinished?.Invoke();
        }
    }
}
