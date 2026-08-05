using System;
using ProjectSpark.Gameplay.Electronics;
using UnityEngine;

namespace ProjectSpark.Gameplay.Inventory
{
    [Serializable]
    public class ToolboxItem
    {
        public ComponentDefinition Definition;

        [Min(1)]
        public int Quantity = 1;

        public bool Infinite;
    }
}
