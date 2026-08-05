using System;

namespace ProjectSpark.Domain.Simulation.Solver
{
    public static class NumericalUtility
    {
        public static bool NearlyEqual(
            double a,
            double b,
            double tolerance = 1e-9)
        {
            return Math.Abs(a-b)
                <= tolerance;
        }
    }
}
