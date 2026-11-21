using System;
using UnityEngine;
namespace ProjectSpark.Level127
{
    // Idealized thermal RC model. Never issues gameplay or actuator commands.
    public sealed class SparkL127ThermalTeachingDemo : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float heatCapacityJPerK = 120f;
        [SerializeField, Min(0f)] private float conductanceWPerK = 2.4f;
        [SerializeField] private float sinkCelsius = 10f;
        public bool TryPredict(float initialC, float heatWatts, float seconds, bool backupLoop,
            out float finalC)
        {
            finalC = 0f;
            if (!Finite(initialC) || !Finite(heatWatts) || !Finite(seconds) ||
                !Finite(heatCapacityJPerK) || !Finite(conductanceWPerK) || !Finite(sinkCelsius) ||
                heatCapacityJPerK <= 0f || conductanceWPerK < 0f || seconds < 0f || heatWatts < 0f) return false;
            double g = conductanceWPerK * (backupLoop ? 0.65 : 1.0);
            double result;
            if (g < 1e-8) result = initialC + heatWatts * (double)seconds / heatCapacityJPerK;
            else { double eq = sinkCelsius + heatWatts / g;
                result = eq + (initialC - eq) * Math.Exp(-g * seconds / heatCapacityJPerK); }
            if (double.IsNaN(result) || double.IsInfinity(result) || Math.Abs(result) > float.MaxValue) return false;
            finalC = (float)result;
            return true;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
