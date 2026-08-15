using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerReconstructionVFXTest : MonoBehaviour
    {
        [SerializeField]
        private ScannerReconstructionVFXController controller;

        [SerializeField, Min(0.01f)]
        private float testSpeed = 0.4f;

        private float progress;

        private void Update()
        {
            if (controller == null)
                return;

            if (Input.GetKeyDown(KeyCode.V))
            {
                progress = 0f;
                controller.StartReconstruction();
            }

            if (Input.GetKey(KeyCode.V))
            {
                progress +=
                    testSpeed *
                    Time.deltaTime;

                progress =
                    Mathf.Clamp01(progress);

                controller.SetProgress(progress);
            }

            if (Input.GetKeyUp(KeyCode.V))
            {
                controller.StopReconstruction();
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                progress = 1f;
                controller.CompleteReconstruction();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                progress = 0f;
                controller.ResetVFX();
            }
        }
    }
}