using ProjectSpark.Domain.Tools;
namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class MeasurementSession
    {
        public Probe PositiveProbe;

        public Probe NegativeProbe;

        public MeasurementType Type;

        public ToolState State;

        public bool Ready =>
            PositiveProbe != null &&
            NegativeProbe != null;
    }
}
