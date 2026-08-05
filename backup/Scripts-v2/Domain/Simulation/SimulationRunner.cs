using UnityEngine;

namespace ProjectSpark.Domain.Simulation
{
    public sealed class SimulationRunner
        : MonoBehaviour
    {
        [SerializeField]
        private SimulationEngine engine;

        private readonly SimulationScheduler
            scheduler = new();

        private void Update()
        {
            if (!scheduler.ShouldTick(
                Time.deltaTime))
                return;

            SimulationEvents
                .RaiseTickStarted();

            engine.Simulate(
                1f / scheduler.TickRate);

            SimulationEvents
                .RaiseTickFinished();
        }
    }
}
