using UnityEngine;

namespace ProjectSpark.Domain.Simulation
{
    public sealed class SimulationScheduler
    {
        private float accumulator;

        public float TickRate = 60f;

        public bool ShouldTick(float deltaTime)
        {
            accumulator += deltaTime;

            float interval = 1f / TickRate;

            if (accumulator < interval)
                return false;

            accumulator -= interval;

            return true;
        }
    }
}
