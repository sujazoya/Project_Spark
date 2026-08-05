using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay.Electronics;

namespace ProjectSpark.Gameplay.Inventory
{
    public sealed class InventoryManager : MonoBehaviour
    {
        [SerializeField]
        private ToolboxDefinition toolbox;

        private readonly List<InventoryItem> _items =
            new();

        public IReadOnlyList<InventoryItem> Items => _items;

        private void Awake()
        {
            BuildInventory();
        }

        private void BuildInventory()
        {
            _items.Clear();

            foreach(var item in toolbox.Items)
            {
                _items.Add(new InventoryItem
                {
                    Definition = item.Definition,
                    Remaining = item.Quantity,
                    Infinite = item.Infinite
                });
            }
        }

        public bool Consume(ComponentDefinition definition)
        {
            foreach(var item in _items)
            {
                if(item.Definition != definition)
                    continue;

                if(item.Infinite)
                    return true;

                if(item.Remaining <= 0)
                    return false;

                item.Remaining--;

                return true;
            }

            return false;
        }
    }
}
