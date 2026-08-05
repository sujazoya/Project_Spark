using UnityEngine;
using ProjectSpark.Gameplay.Electronics;

namespace ProjectSpark.Gameplay.Inventory
{
    public sealed class InventoryFactory
    {
        private readonly ComponentFactory factory = new();

        public ElectronicComponent Create(
     ComponentDefinition definition,
     Vector3 position,
     Quaternion rotation)
        {
            return factory.Spawn(
                definition,
                position,
                rotation);
        }
    }
}