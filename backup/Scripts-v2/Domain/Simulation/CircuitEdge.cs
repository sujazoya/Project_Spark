namespace ProjectSpark.Domain.Simulation
{
    public sealed class CircuitEdge
    {
        public CircuitNode A;

        public CircuitNode B;

        public float Resistance;

        public float Current;
        public bool IsBroken;
        public bool IsEnabled = true;
    }
}
