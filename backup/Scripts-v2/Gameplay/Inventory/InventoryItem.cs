using ProjectSpark.Gameplay.Electronics;

namespace ProjectSpark.Gameplay.Inventory
{
    public sealed class InventoryItem
    {
        public ComponentDefinition Definition;

        public int Remaining;

        public bool Infinite;
    }
}
