namespace ProjectSpark.Domain.Simulation.Stamping
{
    public sealed class LEDStamp
        : ICircuitStamp
    {
        public void Stamp(
            StampContext context)
        {
            double resistance =
                context.Value > 2.0
                ? 10
                : 1000000;

            context.Value =
                resistance;

            new ResistorStamp()
                .Stamp(context);
        }
    }
}
