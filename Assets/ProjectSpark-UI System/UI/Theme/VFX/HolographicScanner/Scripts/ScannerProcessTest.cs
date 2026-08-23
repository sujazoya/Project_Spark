using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerProcessTest
        : MonoBehaviour
    {
        [SerializeField]
        private ScannerProcessController controller;

        [SerializeField, Min(0.01f)]
        private float scanSpeed = 0.35f;

        private float scanProgress;

        private void Update()
        {
            if (controller == null)
                return;

            // ---------------------------------------------------------
            // A = begin acquire
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.A))
            {
                controller.StartScanProcess();
            }

            // ---------------------------------------------------------
            // S = enter scan
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.S))
            {
                scanProgress = 0f;

                controller.BeginScan();
            }

            // ---------------------------------------------------------
            // Hold S = scan progress
            // ---------------------------------------------------------

            if (Input.GetKey(KeyCode.S))
            {
                scanProgress +=
                    scanSpeed *
                    Time.deltaTime;

                scanProgress =
                    Mathf.Clamp01(
                        scanProgress);

                controller.SetScanProgress(
                    scanProgress);
            }

            // ---------------------------------------------------------
            // D = complete scan / analyze
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.D))
            {
                controller.CompleteScan();
            }

            // ---------------------------------------------------------
            // F = simulate analysis result: FAULT
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.F))
            {
                controller.CompleteAnalysis(
                    true);
            }

            // ---------------------------------------------------------
            // N = simulate analysis result: NORMAL
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.N))
            {
                controller.CompleteAnalysis(
                    false);
            }

            // ---------------------------------------------------------
            // R = result
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.R))
            {
                controller.ShowResult();
            }

            // ---------------------------------------------------------
            // X = reset
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.X))
            {
                controller.ResetVisualSystems();
            }
        }
    }
}