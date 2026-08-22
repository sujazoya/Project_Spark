using UnityEngine;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerFaultEnergyTest
        : MonoBehaviour
    {
        [SerializeField]
        private SignalPath_Manager pathManager;

        [SerializeField]
        private ScannerFaultEnergyController controller;

        [SerializeField, Range(0f, 1f)]
        private float faultPosition = 0.5f;

        [SerializeField, Range(0f, 1f)]
        private float severity = 1f;

        [SerializeField, Range(0f, 1f)]
        private float energy = 1f;

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

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                selectedPath--;

                if (selectedPath < 0)
                    selectedPath =
                        paths.Length - 1;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                selectedPath++;

                if (selectedPath >= paths.Length)
                    selectedPath = 0;
            }

            // F = activate
            if (Input.GetKeyDown(KeyCode.F))
            {
                controller.SetFaultEnergy(
                    paths[selectedPath],
                    true,
                    faultPosition,
                    severity,
                    energy);
            }

            // G = clear selected
            if (Input.GetKeyDown(KeyCode.G))
            {
                controller.ClearFaultEnergy(
                    paths[selectedPath]);
            }

            // H = clear all
            if (Input.GetKeyDown(KeyCode.H))
            {
                controller.ClearAll();
            }

            // Up = stronger
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                energy += 0.1f;
                energy = Mathf.Clamp01(energy);

                controller.SetFaultEnergy(
                    paths[selectedPath],
                    true,
                    faultPosition,
                    severity,
                    energy);
            }

            // Down = weaker
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                energy -= 0.1f;
                energy = Mathf.Clamp01(energy);

                controller.SetFaultEnergy(
                    paths[selectedPath],
                    true,
                    faultPosition,
                    severity,
                    energy);
            }
        }
    }
}