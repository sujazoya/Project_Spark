using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerScanPlaneTest : MonoBehaviour
    {
        [SerializeField]
        private ScannerScanPlaneController controller;

        private void Update()
        {
            if (controller == null)
                return;

            if (Input.GetKeyDown(KeyCode.Space))
                controller.StartScan();

            if (Input.GetKeyUp(KeyCode.Space))
                controller.StopScan();
        }
    }
}