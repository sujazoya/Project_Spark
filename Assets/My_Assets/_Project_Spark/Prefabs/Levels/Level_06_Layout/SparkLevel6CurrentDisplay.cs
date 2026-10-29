
using TMPro;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel6CurrentDisplay
        : MonoBehaviour
    {
        [Header("Circuit LEDs")]
        [SerializeField]
        private SparkLED branch1LED;

        [SerializeField]
        private SparkLED branch2LED;

        [Header("Existing TextMeshPro Displays")]
        [SerializeField]
        private TMP_Text branch1CurrentText;

        [SerializeField]
        private TMP_Text branch2CurrentText;

        [SerializeField]
        private TMP_Text totalCurrentText;

        [SerializeField]
        private TMP_Text circuitStatusText;

        [Header("Safe Current Range (mA)")]
        [SerializeField]
        private float minimumBranchCurrentMA = 6f;

        [SerializeField]
        private float maximumBranchCurrentMA = 15f;

        private void Update()
        {
            RefreshMeasurements();
        }

        private void RefreshMeasurements()
        {
            float branch1 = ReadCurrentMA(branch1LED);
            float branch2 = ReadCurrentMA(branch2LED);

            // Valid for this level's two-branch circuit
            // when these are the only current paths.
            float total = branch1 + branch2;

            if (branch1CurrentText != null)
            {
                branch1CurrentText.text =
                    branch1.ToString("F2") + " mA";
            }

            if (branch2CurrentText != null)
            {
                branch2CurrentText.text =
                    branch2.ToString("F2") + " mA";
            }

            if (totalCurrentText != null)
            {
                totalCurrentText.text =
                    total.ToString("F2") + " mA";
            }

            bool branch1Safe =
                IsCurrentSafe(branch1);

            bool branch2Safe =
                IsCurrentSafe(branch2);

            if (circuitStatusText != null)
            {
                if (branch1Safe && branch2Safe)
                {
                    circuitStatusText.text =
                        "BOTH BRANCHES SAFE";
                }
                else if (branch1 > maximumBranchCurrentMA ||
                         branch2 > maximumBranchCurrentMA)
                {
                    circuitStatusText.text =
                        "WARNING: EXCESS CURRENT";
                }
                else
                {
                    circuitStatusText.text =
                        "CIRCUIT NOT YET SAFE";
                }
            }
        }

        private float ReadCurrentMA(SparkLED led)
        {
            if (led == null ||
                !led.ElectricalEnabled)
            {
                return 0f;
            }

            float current =
                led.ElectricalState.Current;

            if (float.IsNaN(current) ||
                float.IsInfinity(current))
            {
                return 0f;
            }

            return Mathf.Abs(current) * 1000f;
        }

        private bool IsCurrentSafe(float currentMA)
        {
            return
                currentMA >= minimumBranchCurrentMA &&
                currentMA <= maximumBranchCurrentMA;
        }
    }
}
