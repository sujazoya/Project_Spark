using System.Collections.Generic;

namespace ProjectSpark.Domain.Simulation
{
    public sealed class SimulationPipeline
    {
        private readonly List<ISimulationSolver>
            _solvers =
                new();

        public SimulationPipeline()
        {
            _solvers.Add(
                new VoltageSolver());

            _solvers.Add(
                new CurrentSolver());

            _solvers.Add(
                new TemperatureSolver());

            _solvers.Add(
                new FaultSolver());
        }

        public void Execute(
            SimulationContext context)
        {
            foreach (var solver
                in _solvers)
            {
                solver.Execute(context);
            }
        }
    }
}
