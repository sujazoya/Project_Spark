using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerStateTransitionTest
        : MonoBehaviour
    {
        [SerializeField]
        private ScannerStateTransitionController controller;

        private void Update()
        {
            if (controller == null)
                return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                controller.TransitionTo(
                    ScannerState.Acquire);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                controller.TransitionTo(
                    ScannerState.Scan);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                controller.TransitionTo(
                    ScannerState.Analyze);
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                controller.TransitionTo(
                    ScannerState.Result);
            }
        }
    }
}