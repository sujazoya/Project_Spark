using UnityEngine;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerSignalPulseTest
        : MonoBehaviour
    {
        [SerializeField]
        private SignalPath_Manager pathManager;

        [SerializeField]
        private ScannerSignalPulseController controller;

        [SerializeField, Min(0.01f)]
        private float speed = 0.35f;

        [SerializeField]
        private float direction = 1f;

        private int selectedPath;
        private float position;

        private void Update()
        {
            if (pathManager == null ||
                controller == null)
            {
                return;
            }

            SignalPath[] paths =
                pathManager.Paths;

            if (paths == null ||
                paths.Length == 0)
            {
                return;
            }

            selectedPath =
                Mathf.Clamp(
                    selectedPath,
                    0,
                    paths.Length - 1);

            // ---------------------------------------------------------
            // Select previous path
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.LeftArrow))
            {
                selectedPath--;

                if (selectedPath < 0)
                {
                    selectedPath =
                        paths.Length - 1;
                }
            }

            // ---------------------------------------------------------
            // Select next path
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.RightArrow))
            {
                selectedPath++;

                if (selectedPath >= paths.Length)
                    selectedPath = 0;
            }

            // ---------------------------------------------------------
            // Start pulse
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.P))
            {
                position = 0f;

                controller.SendPulse(
                    paths[selectedPath],
                    position,
                    direction);
            }

            // ---------------------------------------------------------
            // Hold P = animate pulse
            // ---------------------------------------------------------

            if (Input.GetKey(KeyCode.P))
            {
                position +=
                    speed *
                    direction *
                    Time.deltaTime;

                if (position > 1f)
                    position = 0f;

                if (position < 0f)
                    position = 1f;

                controller.SendPulse(
                    paths[selectedPath],
                    position,
                    direction);
            }

            // ---------------------------------------------------------
            // O = stop selected pulse
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.O))
            {
                controller.StopPulse(
                    paths[selectedPath]);
            }

            // ---------------------------------------------------------
            // X = stop all
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.X))
            {
                controller.StopAllPulses();
            }

            // ---------------------------------------------------------
            // D = reverse direction
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.D))
            {
                direction *= -1f;
            }
        }
    }
}