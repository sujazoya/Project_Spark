using System.Collections.Generic;

namespace ProjectSpark.Domain.Simulation.Solver
{
    public sealed class SparseMatrix
    {
        private readonly Dictionary<(int,int),double>
            values = new();

        public void Add(
            int row,
            int column,
            double value)
        {
            values[(row,column)] = value;
        }

        public bool TryGet(
            int row,
            int column,
            out double value)
        {
            return values.TryGetValue(
                (row,column),
                out value);
        }
    }
}
