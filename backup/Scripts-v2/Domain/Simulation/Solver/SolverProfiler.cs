using System.Diagnostics;

namespace ProjectSpark.Domain.Simulation.Solver
{
    public sealed class SolverProfiler
    {
        private readonly Stopwatch
            stopwatch = new();

        public void Begin()
        {
            stopwatch.Restart();
        }

        public double End()
        {
            stopwatch.Stop();

            return stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
