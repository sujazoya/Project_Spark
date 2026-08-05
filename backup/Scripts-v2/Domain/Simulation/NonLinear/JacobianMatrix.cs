namespace ProjectSpark.Domain.Simulation.NonLinear
{
    public sealed class JacobianMatrix
    {
        public double[,] Values;

        public JacobianMatrix(
            int size)
        {
            Values =
                new double[size,size];
        }
    }
}
