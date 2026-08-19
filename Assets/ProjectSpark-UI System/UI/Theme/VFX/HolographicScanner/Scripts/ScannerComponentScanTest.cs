using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerComponentScanTest : MonoBehaviour
    {
        [SerializeField]
        private ScannerComponentScanController controller;

        [SerializeField, Min(0.01f)]
        private float speed = 0.25f;

        private float progress;

        private void Update()
        {
            if (controller == null)
                return;

            if (Input.GetKeyDown(KeyCode.V))
            {
                progress = 0f;
            }

            if (Input.GetKey(KeyCode.V))
            {
                progress += speed * Time.deltaTime;
                progress = Mathf.Clamp01(progress);

                controller.SetProgress(progress);
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                progress = 0f;
                controller.ResetScan();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                progress = 1f;
                controller.CompleteScan();
            }
        }
    }
}