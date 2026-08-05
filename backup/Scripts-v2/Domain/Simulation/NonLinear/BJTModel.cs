namespace ProjectSpark.Domain.Simulation.NonLinear
{
    public sealed class BJTModel
        : DeviceModel
    {
        public double Beta = 100;

        public override void Stamp(
            NonLinearContext context)
        {

        }
    }
}
