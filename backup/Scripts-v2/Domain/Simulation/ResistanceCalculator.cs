using UnityEngine;

namespace ProjectSpark.Domain.Simulation
{
    public sealed class ResistanceCalculator
    {
        public float Calculate(
            float voltage,
            float current)
        {
            if (Mathf.Abs(current) < 0.0001f)
                return Mathf.Infinity;

            return voltage / current;
        }
    }
}
