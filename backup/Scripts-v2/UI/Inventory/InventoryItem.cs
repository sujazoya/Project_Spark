using ProjectSpark.Gameplay.Electronics;

namespace ProjectSpark.UI.Inventory
{
    [System.Serializable]
    public sealed class InventoryItem
    {
        public ComponentDefinition Definition;

        public bool Unlocked = true;

        public bool Favorite;
    }
}
