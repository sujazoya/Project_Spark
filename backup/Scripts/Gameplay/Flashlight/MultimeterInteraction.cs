// ============================================================================
// MultimeterInteraction.cs
// ============================================================================

using TMPro;
using UnityEngine;
using ProjectSpark.Gameplay.Level01;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class MultimeterInteraction : MonoBehaviour
    {
        [SerializeField]
        TMP_Text display;

        [SerializeField]
        MultimeterController multimeter;

        [SerializeField]
        BatteryController battery;

        [SerializeField]
        ResistorController resistor;

        public void MeasureBattery()
        {
            display.text =
                multimeter.MeasureBattery(
                    battery.Voltage).ToString("0.00") + " V";
        }

        public void MeasureResistor()
        {
            float value =
                multimeter.MeasureResistance(
                    resistor);

            if (float.IsInfinity(value))
            {
                display.text = "OL";
                return;
            }

            display.text =
                value.ToString("0") + " Ω";
        }
    }
}