using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerComponentProjectionTest
        : MonoBehaviour
    {
        [SerializeField]
        private ScannerComponentTarget target;

        [SerializeField, Min(0.01f)]
        private float projectionSpeed = 0.5f;

        private float projectionProgress;

        private void Update()
        {
            if (target == null)
                return;

            // -------------------------------------------------------------
            // P = start / animate projection
            // -------------------------------------------------------------

            if (Input.GetKeyDown(KeyCode.P))
            {
                projectionProgress = 0f;

                target.SetIdentified(true);
                target.SetProjectionVisible(true);
                target.SetProjectionProgress(0f);
            }

            if (Input.GetKey(KeyCode.P))
            {
                projectionProgress +=
                    projectionSpeed *
                    Time.deltaTime;

                projectionProgress =
                    Mathf.Clamp01(
                        projectionProgress);

                target.SetProjectionProgress(
                    projectionProgress);
            }

            // -------------------------------------------------------------
            // O = disable projection
            // -------------------------------------------------------------

            if (Input.GetKeyDown(KeyCode.O))
            {
                projectionProgress = 0f;

                target.SetProjectionProgress(0f);
                target.SetProjectionVisible(false);
            }

            // -------------------------------------------------------------
            // I = complete projection
            // -------------------------------------------------------------

            if (Input.GetKeyDown(KeyCode.I))
            {
                projectionProgress = 1f;

                target.SetProjectionVisible(true);
                target.SetProjectionProgress(1f);
            }
        }
    }
}