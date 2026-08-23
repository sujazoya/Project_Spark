using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerDiagnosticPanelTest
        : MonoBehaviour
    {
        [SerializeField]
        private ScannerDiagnosticPanelController controller;

        [SerializeField]
        private ScannerComponentTarget target;

        [Header("Test Data")]
        [SerializeField]
        private ScannerDiagnosticData data =
            new ScannerDiagnosticData
            {
                componentId = "R1",
                componentName = "R1",
                componentType = "RESISTOR",

                primaryValueLabel = "RESISTANCE",
                primaryValue = "100 ",

                secondaryValueLabel = "VOLTAGE",
                secondaryValue = "4.98 V",

                tertiaryValueLabel = "CURRENT",
                tertiaryValue = "49.8 mA",

                quaternaryValueLabel = "POWER",
                quaternaryValue = "0.248 W",

                status = "NORMAL",
                fault = false,
                severity = 0f
            };

        private void Update()
        {
            if (controller == null ||
                target == null)
            {
                return;
            }

            if (Input.GetKeyDown(
                    KeyCode.Y))
            {
                controller.ShowComponent(
                    target,
                    data);
            }

            if (Input.GetKeyDown(
                    KeyCode.U))
            {
                controller.Hide();
            }
        }
    }
}