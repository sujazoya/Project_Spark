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

            if (Input.GetKeyDown(KeyCode.C))
            {
                progress = 0f;

                controller.StartTrace();
            }

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

            if (Input.GetKeyDown(KeyCode.X))
            {
                controller.StopTrace();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                progress = 1f;

                controller.CompleteTrace();
            }
        }
    }
}