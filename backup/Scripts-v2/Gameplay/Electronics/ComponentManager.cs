using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class ComponentManager
        : MonoBehaviour
    {
        [SerializeField]
        private ComponentDatabase database;

        private readonly ComponentRegistry registry =
            new();

        private readonly ComponentFactory factory =
            new();

        public ElectronicComponent Spawn(
            string componentId,
            Vector3 position,
            Quaternion rotation)
        {
            foreach (var def in database.Components)
            {
                if (def.Id != componentId)
                    continue;

                ElectronicComponent component =
                    factory.Spawn(
                        def,
                        position,
                        rotation);

                registry.Register(component);

                ComponentEvents.RaiseAdded(component);

                return component;
            }

            return null;
        }
    }
}
