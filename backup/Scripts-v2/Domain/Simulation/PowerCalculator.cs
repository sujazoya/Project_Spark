namespace ProjectSpark.Domain.Simulation
{
    public sealed class PowerCalculator
    {
        public float Calculate(
            float voltage,
            float current)
        {
            return voltage * current;
        }
    }
}
