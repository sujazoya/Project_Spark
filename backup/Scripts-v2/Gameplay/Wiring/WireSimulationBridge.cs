using ProjectSpark.Domain.Simulation;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireSimulationBridge
    {
        public CircuitEdge CreateEdge(
            WireConnection connection)
        {
            return new CircuitEdge
            {
                Resistance = connection.Broken ? float.MaxValue : 0.01f,
                Current = 0f
            };
        }
    }
}