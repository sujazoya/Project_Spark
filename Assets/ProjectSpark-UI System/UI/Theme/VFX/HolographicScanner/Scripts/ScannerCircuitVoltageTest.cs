using UnityEngine;
using AAAUI.VFX;
namespace ProjectSpark.Scanner
{
    public sealed class ScannerCircuitVoltageTest
        : MonoBehaviour
    {
        [SerializeField]
        private SignalPath_Manager pathManager;

        [SerializeField]
        private ScannerCircuitVoltageController controller;

        [SerializeField]
        private float testVoltage = 12f;

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

            // ---------------------------------------------------------
            // V = apply test voltage
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.V))
            {
                controller.SetVoltage(
                    paths[selectedPath],
                    testVoltage);
            }

            // ---------------------------------------------------------
            // B = clear selected path
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.B))
            {
                controller.ClearVoltage(
                    paths[selectedPath]);
            }

            // ---------------------------------------------------------
            // N = double test voltage
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.N))
            {
                testVoltage *= 2f;

                if (testVoltage > 24f)
                    testVoltage = 3f;

                controller.SetVoltage(
                    paths[selectedPath],
                    testVoltage);
            }

            // ---------------------------------------------------------
            // M = clear all
            // ---------------------------------------------------------

            if (Input.GetKeyDown(
                    KeyCode.M))
            {
                controller.ClearAllVoltage();
            }
        }
    }
}