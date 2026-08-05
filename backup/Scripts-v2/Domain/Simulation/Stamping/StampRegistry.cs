using System.Collections.Generic;

namespace ProjectSpark.Domain.Simulation.Stamping
{
    public sealed class StampRegistry
    {
        private readonly List<ICircuitStamp>
            stamps = new();

        public IReadOnlyList<ICircuitStamp>
            Stamps => stamps;

        public void Register(
            ICircuitStamp stamp)
        {
            stamps.Add(stamp);
        }

        public void Clear()
        {
            stamps.Clear();
        }
    }
}
