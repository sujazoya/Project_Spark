namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class SimulationEngine
    {
        private readonly CircuitGraph _graph = new();

        private readonly CircuitSolver _solver = new();

        public CircuitGraph Graph => _graph;

        public void AddComponent(ElectronicComponent component)
        {
            _graph.Add(component);
        }

        public void RemoveComponent(ElectronicComponent component)
        {
            _graph.Remove(component);
        }

        public void Simulate()
        {
            _solver.Solve(_graph);
        }

        public void Reset()
        {
            _graph.Clear();
        }
    }
}
