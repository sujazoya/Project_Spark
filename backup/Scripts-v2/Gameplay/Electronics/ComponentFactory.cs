using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class ComponentFactory
    {
        public ElectronicComponent Spawn(
            ComponentDefinition definition,
            Vector3 position,
            Quaternion rotation)
        {
            GameObject obj =
                Object.Instantiate(
                    definition.Prefab,
                    position,
                    rotation);

            return obj.GetComponent<ElectronicComponent>();
        }
    }
}
