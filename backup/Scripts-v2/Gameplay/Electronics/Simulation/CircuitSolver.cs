namespace ProjectSpark.Gameplay.Electronics
{
    /// <summary>
    /// First version of the circuit solver.
    /// Future versions will support
    /// Kirchhoff's Laws,
    /// AC,
    /// Logic ICs,
    /// Microcontrollers,
    /// Signal tracing.
    /// </summary>
    public sealed class CircuitSolver
    {
        public void Solve(CircuitGraph graph)
        {
            foreach (var component in graph.Components)
            {
                component.ResetComponent();
            }

            foreach (var component in graph.Components)
            {
                component.Simulate();
            }
        }
    }
}
