// ============================================================================
// MultimeterController.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class MultimeterController : MonoBehaviour
    {
        public float MeasureBattery(float voltage)
        {
            return voltage;
        }

        public float MeasureResistance(
            ResistorController resistor)
        {
            if(resistor.IsBurnt)
                return Mathf.Infinity;

            return 220f;
        }

        public bool MeasureLED()
        {
            return true;
        }
    }
}