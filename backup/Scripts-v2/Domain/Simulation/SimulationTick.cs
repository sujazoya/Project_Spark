namespace ProjectSpark.Domain.Simulation
{
    public readonly struct SimulationTick
    {
        public readonly float DeltaTime;

        public readonly int Tick;

        public SimulationTick(
            int tick,
            float deltaTime)
        {
            Tick = tick;
            DeltaTime = deltaTime;
        }
    }
}
