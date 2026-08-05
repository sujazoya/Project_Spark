namespace ProjectSpark.Domain.Simulation.Stamping
{
    public sealed class ResistorStamp
        : ICircuitStamp
    {
        public void Stamp(
            StampContext context)
        {
            double g =
                1.0 / context.Value;

            context.Matrix.A[
                context.PositiveNode,
                context.PositiveNode] += g;

            context.Matrix.A[
                context.NegativeNode,
                context.NegativeNode] += g;

            context.Matrix.A[
                context.PositiveNode,
                context.NegativeNode] -= g;

            context.Matrix.A[
                context.NegativeNode,
                context.PositiveNode] -= g;
        }
    }
}
