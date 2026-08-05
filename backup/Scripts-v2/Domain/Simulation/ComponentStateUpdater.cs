using ProjectSpark.Gameplay.Electronics;

namespace ProjectSpark.Domain.Simulation
{
    public sealed class ComponentStateUpdater
    {
        public void Update(
            ElectronicComponent component,
            float voltage,
            float current)
        {
            component.State.Voltage =
                voltage;

            component.State.Current =
                current;

            component.State.IsPowered =
                voltage > 0.1f;
        }
    }
}
