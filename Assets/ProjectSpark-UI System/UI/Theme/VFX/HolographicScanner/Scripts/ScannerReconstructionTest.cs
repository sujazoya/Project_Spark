using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerReconstructionTest : MonoBehaviour
    {
        [SerializeField]
        private ScannerReconstructionController controller;

        private void Update()
        {
            if (controller == null)
                return;

            if (Input.GetKeyDown(KeyCode.R))
                controller.StartReconstruction();

            if (Input.GetKeyDown(KeyCode.T))
                controller.CompleteReconstruction();

            if (Input.GetKeyDown(KeyCode.Y))
                controller.ResetReconstruction();
        }
    }
}