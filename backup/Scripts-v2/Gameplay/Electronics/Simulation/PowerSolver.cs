namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class PowerSolver
    {
        public void Solve(ElectricalGraph graph)
        {
            foreach (var component in graph.Components)
            {
                // Power is calculated automatically from Voltage * Current.
            }
        }
    }
}