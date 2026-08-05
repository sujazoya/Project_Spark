namespace ProjectSpark.Domain.Simulation
{
    public interface ISimulationSolver
    {
        void Execute(
            SimulationContext context);
    }
}
