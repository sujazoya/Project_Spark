namespace ProjectSpark.UI.Data
{
    public sealed class SimulationViewModel
    {
        public bool IsRunning
        {
            get;
        }

        public bool HasFault
        {
            get;
        }

        public float Voltage
        {
            get;
        }

        public float Current
        {
            get;
        }

        public float Power
        {
            get;
        }

        public int FaultCount
        {
            get;
        }

        public string StatusText
        {
            get;
        }

        public SimulationViewModel(
            bool isRunning,
            bool hasFault,
            float voltage,
            float current,
            float power,
            int faultCount,
            string statusText)
        {
            IsRunning =
                isRunning;

            HasFault =
                hasFault;

            Voltage =
                voltage;

            Current =
                current;

            Power =
                power;

            FaultCount =
                faultCount;

            StatusText =
                statusText;
        }
    }
}