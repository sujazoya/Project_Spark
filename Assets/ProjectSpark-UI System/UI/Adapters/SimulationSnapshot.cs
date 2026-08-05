namespace ProjectSpark.UI.Data
{
    public readonly struct SimulationSnapshot
    {
        public readonly bool IsRunning;

        public readonly bool HasFault;

        public readonly int ComponentCount;

        public readonly int WireCount;

        public readonly float SimulationTime;

        public SimulationSnapshot(
            bool isRunning,
            bool hasFault,
            int componentCount,
            int wireCount,
            float simulationTime)
        {
            IsRunning = isRunning;
            HasFault = hasFault;
            ComponentCount = componentCount;
            WireCount = wireCount;
            SimulationTime = simulationTime;
        }
    }
}