using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class ComponentRegistry
    {
        private readonly Dictionary<string, ElectronicComponent>
            _components = new();

        public void Register(ElectronicComponent component)
        {
            _components[component.Id] = component;
        }

        public void Unregister(ElectronicComponent component)
        {
            _components.Remove(component.Id);
        }

        public bool TryGet(
            string id,
            out ElectronicComponent component)
        {
            return _components.TryGetValue(id, out component);
        }

        public IEnumerable<ElectronicComponent> All
            => _components.Values;
    }
}
