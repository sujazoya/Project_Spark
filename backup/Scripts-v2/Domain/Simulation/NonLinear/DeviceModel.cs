namespace ProjectSpark.Domain.Simulation.NonLinear
{
    public abstract class DeviceModel
    {
        public abstract void Stamp(
            NonLinearContext context);
    }
}
