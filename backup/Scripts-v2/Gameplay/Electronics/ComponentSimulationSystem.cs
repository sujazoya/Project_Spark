using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class ComponentSimulationSystem
        : MonoBehaviour
    {
        [SerializeField]
        private ComponentManager componentManager;

        private void Update()
        {
            // Later replace this with your SimulationEngine tick.

            // Example:
            // foreach (ElectronicComponent component in componentManager.All)
            // {
            //     component.Simulate(Time.deltaTime);
            // }
        }
    }
}
