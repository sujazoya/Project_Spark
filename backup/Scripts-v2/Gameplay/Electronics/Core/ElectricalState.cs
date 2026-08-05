namespace ProjectSpark.Gameplay.Electronics
{
    public struct ElectricalState
    {
        public float Voltage;
        public float Current;
        public float Resistance;
        public float Power;
        public float Temperature;

        public bool IsPowered;
        public bool IsShortCircuit;
        public bool IsOverloaded;

        public void Reset()
        {
            Voltage = 0f;
            Current = 0f;
            Resistance = 0f;
            Power = 0f;
            Temperature = 20f;

            IsPowered = false;
            IsShortCircuit = false;
            IsOverloaded = false;
        }
    }
}
