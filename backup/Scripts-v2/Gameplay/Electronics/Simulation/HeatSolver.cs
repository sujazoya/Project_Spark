namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class HeatSolver
    {
        public void Solve(ElectricalGraph graph)
        {
            foreach (var component in graph.Components)
            {
                component.State.Temperature +=
                    component.State.Power * 0.01f;
            }
        }
    }
}
