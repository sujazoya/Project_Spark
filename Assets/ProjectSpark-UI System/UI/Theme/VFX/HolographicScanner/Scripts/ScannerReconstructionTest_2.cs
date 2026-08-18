using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerReconstructionTest_2
        : MonoBehaviour
    {
        [SerializeField]
        private ScannerReconstructionController_2 controller;

        [SerializeField, Min(0.01f)]
        private float speed = 0.25f;

        private float progress;

        private void Update()
        {
            if (controller == null)
                return;

            if (Input.GetKeyDown(
                    KeyCode.C))
            {
                progress = 0f;

                controller
                    .StartReconstruction();
            }

            if (Input.GetKey(
                    KeyCode.C))
            {
                progress +=
                    speed *
                    Time.deltaTime;

                progress =
                    Mathf.Clamp01(
                        progress);

                controller.SetProgress(
                    progress);
            }

            if (Input.GetKeyDown(
                    KeyCode.X))
            {
                progress = 0f;

                controller
                    .StopReconstruction();
            }

            if (Input.GetKeyDown(
                    KeyCode.Z))
            {
                progress = 1f;

                controller
                    .CompleteReconstruction();
            }
        }
    }
}