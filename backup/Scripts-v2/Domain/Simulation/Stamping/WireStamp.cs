namespace ProjectSpark.Domain.Simulation.Stamping
{
    public sealed class WireStamp
        : ICircuitStamp
    {
        public void Stamp(
            StampContext context)
        {
            context.Value = 0.000001;

            new ResistorStamp()
                .Stamp(context);
        }
    }
}
