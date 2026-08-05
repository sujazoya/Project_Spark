using UnityEngine;

namespace ProjectSpark.Domain.Simulation
{
    [CreateAssetMenu(
        menuName = "Project Spark/Simulation/Settings")]
    public class SimulationSettings : ScriptableObject
    {
        [Header("Simulation")]

        public float TickRate = 60f;

        public bool AutoStart = true;

        [Header("Limits")]

        public float MaxVoltage = 240f;

        public float MaxCurrent = 20f;

        public float MaxTemperature = 150f;
    }
}
