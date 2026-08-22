using UnityEngine;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerFaultLocalizationTest
        : MonoBehaviour
    {
        [SerializeField]
        private SignalPath_Manager pathManager;

        [SerializeField]
        private ScannerFaultLocalizationController controller;

        [SerializeField, Range(0f, 1f)]
        private float testPosition = 0.5f;

        [SerializeField, Range(0f, 1f)]
        private float testSeverity = 1f;

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
                    KeyCode.LeftArrow))
            {
                selectedPath--;

                if (selectedPath < 0)
                    selectedPath =
                        paths.Length - 1;
            }

            if (Input.GetKeyDown(
                    KeyCode.RightArrow))
            {
                selectedPath++;

                if (selectedPath >= paths.Length)
                    selectedPath = 0;
            }

            // F = fault
            if (Input.GetKeyDown(KeyCode.F))
            {
                controller.ShowPathFault(
                    paths[selectedPath],
                    testPosition,
                    testSeverity);
            }

            // G = clear selected
            if (Input.GetKeyDown(KeyCode.G))
            {
                controller.ClearPathFault(
                    paths[selectedPath]);
            }

            // H = clear all
            if (Input.GetKeyDown(KeyCode.H))
            {
                controller.ClearAllFaults();
            }
        }
    }
}