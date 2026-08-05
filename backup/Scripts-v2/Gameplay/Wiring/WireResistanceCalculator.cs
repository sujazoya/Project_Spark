namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireResistanceCalculator
    {
        public float Calculate(
            float length)
        {
            return length * 0.005f;
        }
    }
}
