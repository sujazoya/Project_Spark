using UnityEngine;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerCircuitFlowTest
        : MonoBehaviour
    {
        [SerializeField]
        private SignalPath_Manager pathManager;

        [SerializeField]
        private ScannerCircuitFlowController controller;

        [SerializeField, Min(0.01f)]
        private float intensity = 1.5f;

        [SerializeField]
        private float direction = 1f;

        private int selectedPath;

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

            if (Input.GetKeyDown(
                    KeyCode.RightArrow))
            {
                selectedPath++;

                if (selectedPath >= paths.Length)
                    selectedPath = 0;
            }

            if (Input.GetKeyDown(
                    KeyCode.LeftArrow))
            {
                selectedPath--;

                if (selectedPath < 0)
                    selectedPath =
                        paths.Length - 1;
            }

            if (Input.GetKeyDown(
                    KeyCode.F))
            {
                SignalPath path =
                    paths[selectedPath];

                controller.SetFlow(
                    path,
                    true,
                    direction,
                    intensity);
            }

            if (Input.GetKeyDown(
                    KeyCode.G))
            {
                SignalPath path =
                    paths[selectedPath];

                controller.StopFlow(
                    path);
            }

            if (Input.GetKeyDown(
                    KeyCode.H))
            {
                controller.StopAllFlow();
            }
        }
    }
}