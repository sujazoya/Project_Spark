using ProjectSpark.Domain.Tools;
using TMPro;
using UnityEngine;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class InstrumentDisplay
        : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text value;

        [SerializeField]
        private TMP_Text unit;

        public void Display(MeasurementResult result)
        {
            value.text = result.Passed
                ? result.Value.ToString("0.000")
                : "---";

            switch (result.Type)
            {
                case MeasurementType.Voltage:
                    unit.text = "V";
                    break;

                case MeasurementType.Current:
                    unit.text = "A";
                    break;

                case MeasurementType.Resistance:
                    unit.text = "Ω";
                    break;

                case MeasurementType.Continuity:
                    unit.text = "";
                    break;

                default:
                    unit.text = "";
                    break;
            }
        }
    }
}
