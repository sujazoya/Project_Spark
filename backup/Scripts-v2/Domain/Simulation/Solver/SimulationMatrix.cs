namespace ProjectSpark.Domain.Simulation.Solver
{
    public sealed class SimulationMatrix
    {
        public double[,] A;

        public double[] B;

        public int Size;

        public SimulationMatrix(int size)
        {
            Size = size;

            A = new double[size,size];

            B = new double[size];
        }

        public void Clear()
        {
            System.Array.Clear(A,0,A.Length);
            System.Array.Clear(B,0,B.Length);
        }
    }
}
