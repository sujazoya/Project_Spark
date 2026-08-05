namespace ProjectSpark.Domain.Simulation.Stamping
{
    public sealed class GroundStamp
        : ICircuitStamp
    {
        public void Stamp(
            StampContext context)
        {
            context.Matrix.A[
                context.NegativeNode,
                context.NegativeNode] = 1;

            context.Matrix.B[
                context.NegativeNode] = 0;
        }
    }
}
