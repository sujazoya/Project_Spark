using System;

namespace ProjectSpark.Domain.Simulation.NonLinear
{
    public sealed class ConvergenceChecker
    {
        public bool HasConverged(
            double[] previous,
            double[] current,
            double tolerance)
        {
            for(int i=0;i<current.Length;i++)
            {
                if(Math.Abs(
                    previous[i]-
                    current[i])
                    > tolerance)
                    return false;
            }

            return true;
        }
    }
}
