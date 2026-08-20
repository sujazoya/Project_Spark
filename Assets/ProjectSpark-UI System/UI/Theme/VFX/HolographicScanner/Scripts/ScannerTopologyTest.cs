using UnityEngine;

namespace ProjectSpark.Scanner
{
    /// <summary>
    /// Development test driver for Effect 10.
    ///
    /// Controls:
    /// T = start topology scan
    /// Hold T = animate topology reconstruction
    /// X = reset
    /// Z = complete immediately
    /// Right Arrow = step forward
    /// Left Arrow = step backward
    /// </summary>
    public sealed class ScannerTopologyTest : MonoBehaviour
    {
        [SerializeField]
        private ScannerTopologyController controller;

        [SerializeField, Min(0.01f)]
        private float speed = 0.35f;

        [SerializeField, Range(0f, 1f)]
        private float stepAmount = 0.1f;

        private float progress;

        private void Update()
        {
            if (controller == null)
                return;

            // ---------------------------------------------------------
            // T = start
            // ---------------------------------------------------------

            if (Input.GetKeyDown(KeyCode.T))
            {
                progress = 0f;

                controller.StartTopologyScan();
                controller.SetProgress(progress);
            }

            // ---------------------------------------------------------
            // Hold T = animate
            // ---------------------------------------------------------

            if (Input.GetKey(KeyCode.T))
            {
                progress += speed * Time.deltaTime;

                progress =
                    Mathf.Clamp01(progress);

                controller.SetProgress(progress);
            }

            // ---------------------------------------------------------
            // X = reset
            // ---------------------------------------------------------

            if (Input.GetKeyDown(KeyCode.X))
            {
                progress = 0f;

                controller.ResetTopology();
            }

            // ---------------------------------------------------------
            // Z = complete
            // ---------------------------------------------------------

            if (Input.GetKeyDown(KeyCode.Z))
            {
                progress = 1f;

                controller.StartTopologyScan();
                controller.CompleteTopology();
            }

            // ---------------------------------------------------------
            // Right Arrow = step forward
            // ---------------------------------------------------------

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                progress += stepAmount;

                progress =
                    Mathf.Clamp01(progress);

                controller.StartTopologyScan();
                controller.SetProgress(progress);
            }

            // ---------------------------------------------------------
            // Left Arrow = step backward
            // ---------------------------------------------------------

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                progress -= stepAmount;

                progress =
                    Mathf.Clamp01(progress);

                controller.StartTopologyScan();
                controller.SetProgress(progress);
            }
        }
    }
}