using UnityEngine;

namespace ProjectSpark.Domain.Simulation
{
    [System.Serializable]
    public sealed class SimulationState
    {
        public bool IsRunning;

        public bool HasFault;

        public float Voltage;

        public float Current;

        public float Power;

        public float Temperature;

        public float DeltaTime;
    }
}
