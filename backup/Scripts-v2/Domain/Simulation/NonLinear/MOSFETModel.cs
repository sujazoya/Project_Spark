namespace ProjectSpark.Domain.Simulation.NonLinear
{
    public sealed class MOSFETModel
        : DeviceModel
    {
        public double Threshold =
            2.5;

        public override void Stamp(
            NonLinearContext context)
        {

        }
    }
}
