namespace ProjectSpark.Domain.Simulation.Stamping
{
    public sealed class BatteryStamp
        : ICircuitStamp
    {
        public void Stamp(
            StampContext context)
        {
            context.Matrix.B[
                context.PositiveNode] +=
                context.Value;

            context.Matrix.B[
                context.NegativeNode] -=
                context.Value;
        }
    }
}
