using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Inventory
{
    [CreateAssetMenu(
        menuName="Project Spark/Inventory/Toolbox")]
    public class ToolboxDefinition : ScriptableObject
    {
        public List<ToolboxItem> Items = new();
    }
}
