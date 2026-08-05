namespace ProjectSpark.UI.Gameplay
{
    public readonly struct UIComponentInspectionData
    {
        public readonly string ComponentId;

        public readonly string ComponentType;

        public readonly string Value;

        public readonly string Rating;

        public readonly float Voltage;

        public readonly float Current;

        public readonly float Power;

        public readonly float Temperature;

        public readonly string Status;

        public readonly string Fault;

        public UIComponentInspectionData(
            string componentId,
            string componentType,
            string value,
            string rating,
            float voltage,
            float current,
            float power,
            float temperature,
            string status,
            string fault)
        {
            ComponentId = componentId;
            ComponentType = componentType;
            Value = value;
            Rating = rating;
            Voltage = voltage;
            Current = current;
            Power = power;
            Temperature = temperature;
            Status = status;
            Fault = fault;
        }
    }
}