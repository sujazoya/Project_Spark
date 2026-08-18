using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerCircuitTraceTest : MonoBehaviour
    {
        [SerializeField]
        private ScannerCircuitTraceController controller;

        [SerializeField, Min(0.01f)]
        private float speed = 0.4f;

        private float progress;

        private void Update()
        {
            if (controller == null)
                return;

            // C = Start trace from 0
            if (Input.GetKeyDown(KeyCode.C))
            {
                progress = 0f;

                controller.StartTrace();
            }

            // Hold C = animate 0 -> 1
            if (Input.GetKey(KeyCode.C))
            {
                progress +=
                    speed *
                    Time.deltaTime;

                progress =
                    Mathf.Clamp01(progress);

                controller.SetProgress(
                    progress);
            }

            // X = stop/reset
            if (Input.GetKeyDown(KeyCode.X))
            {
                progress = 0f;

                controller.StopTrace();
            }

            // Z = complete immediately
            if (Input.GetKeyDown(KeyCode.Z))
            {
                progress = 1f;

                controller.CompleteTrace();
            }
        }
    }
}