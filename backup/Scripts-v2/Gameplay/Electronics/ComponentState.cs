namespace ProjectSpark.Gameplay.Electronics
{
    [System.Serializable]
    public sealed class ComponentState
    {
        public bool IsPowered;

        public bool IsActive;

        public bool IsBroken;

        public float Voltage;

        public float Current;

        public float Temperature;
        public float Resistance { get; set; }
        public float Power => Voltage * Current;
    }
}
