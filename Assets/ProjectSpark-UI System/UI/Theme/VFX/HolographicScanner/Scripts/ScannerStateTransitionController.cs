using UnityEngine;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerStateTransitionController
        : MonoBehaviour
    {
        [SerializeField]
        private ScannerStateTransition transition;

        [SerializeField]
        private ScannerState currentState =
            ScannerState.Acquire;

        public ScannerState CurrentState =>
            currentState;

        public void TransitionTo(
            ScannerState newState)
        {
            if (currentState == newState)
                return;

            ScannerState previousState =
                currentState;

            currentState =
                newState;

            PlayTransition(
                previousState,
                newState);
        }

        public void SetStateImmediate(
            ScannerState state)
        {
            currentState =
                state;
        }

        private void PlayTransition(
            ScannerState previous,
            ScannerState next)
        {
            if (transition == null)
                return;

            /*
             * Forward transition:
             *
             * Acquire → Scan
             * Scan → Analyze
             * Analyze → Result
             *
             * Reverse:
             *
             * Result → Analyze
             * Analyze → Scan
             * Scan → Acquire
             */

            int previousIndex =
                (int)previous;

            int nextIndex =
                (int)next;

            if (nextIndex >= previousIndex)
                transition.PlayForward();
            else
                transition.PlayReverse();
        }
    }
}