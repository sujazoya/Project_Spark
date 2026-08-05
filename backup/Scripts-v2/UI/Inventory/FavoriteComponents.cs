namespace ProjectSpark.UI.Inventory
{
    public sealed class FavoriteComponents
    {
        public void Toggle(
            InventoryItem item)
        {
            item.Favorite =
                !item.Favorite;

            InventoryEvents
                .RaiseChanged();
        }
    }
}
