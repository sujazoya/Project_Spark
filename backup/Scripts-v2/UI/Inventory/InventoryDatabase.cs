using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.Inventory
{
    [CreateAssetMenu(
        menuName="Project Spark/Inventory/Database")]
    public sealed class InventoryDatabase
        : ScriptableObject
    {
        public List<InventoryItem> Items =
            new();
    }
}
