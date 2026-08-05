using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class CircuitGraph
    {
        private readonly List<ElectronicComponent> _components = new();

        public IReadOnlyList<ElectronicComponent> Components => _components;

        public void Add(ElectronicComponent component)
        {
            if (!_components.Contains(component))
                _components.Add(component);
        }

        public void Remove(ElectronicComponent component)
        {
            _components.Remove(component);
        }

        public void Clear()
        {
            _components.Clear();
        }
    }
}
