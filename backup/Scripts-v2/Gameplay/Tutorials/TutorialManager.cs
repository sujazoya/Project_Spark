using UnityEngine;

namespace ProjectSpark.Gameplay.Tutorials
{
    public sealed class TutorialManager
        : MonoBehaviour
    {
        [SerializeField]
        private TutorialSequence sequence;

        private int index;

        public TutorialStep Current =>
            sequence.Steps[index];

        public void StartTutorial()
        {
            index = 0;

            TutorialEvents
                .RaiseStarted(Current);
        }

        public void NextStep()
        {
            TutorialEvents
                .RaiseCompleted(Current);

            index++;

            if (index >= sequence.Steps.Count)
            {
                TutorialEvents
                    .RaiseFinished();

                return;
            }

            TutorialEvents
                .RaiseStarted(Current);
        }
    }
}
