using TMPro;
using UnityEngine;
using ProjectSpark.Gameplay.Diagnostics;

namespace ProjectSpark.Domain.Tools
{
    public sealed class ToolHUD
        : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text valueText;

        public void UpdateDisplay(
            MeasurementResult result)
        {
            valueText.text =
                $"{result.Value:F2} {result.Value}";
        }
    }
}
