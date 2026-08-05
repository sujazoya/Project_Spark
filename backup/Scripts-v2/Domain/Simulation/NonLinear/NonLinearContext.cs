using ProjectSpark.Domain.Simulation.Solver;

namespace ProjectSpark.Domain.Simulation.NonLinear
{
    public sealed class NonLinearContext
    {
        public SimulationMatrix Matrix;

        public double[] Voltages;

        public double TimeStep;

        public int Iteration;
    }
}
