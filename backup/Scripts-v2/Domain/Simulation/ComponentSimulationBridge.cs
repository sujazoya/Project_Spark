using ProjectSpark.Gameplay.Electronics;

namespace ProjectSpark.Domain.Simulation
{
    public sealed class ComponentSimulationBridge
    {
        public void Apply(
            ElectronicComponent component,
            NodeState state)
        {
            component.State.IsPowered =
                state.Powered;

            component.State.Voltage =
                state.Voltage;
        }
    }
}
