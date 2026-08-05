using ProjectSpark.Domain.Simulation.Solver;

namespace ProjectSpark.Domain.Simulation.Stamping
{
    public sealed class StampContext
    {
        public SimulationMatrix Matrix;

        public int PositiveNode;

        public int NegativeNode;

        public double Value;
    }
}
