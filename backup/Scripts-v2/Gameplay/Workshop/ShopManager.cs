using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Workshop
{
    public sealed class ShopManager
        : MonoBehaviour
    {
        [SerializeField]
        private List<ShopItem> items =
            new();

        public IReadOnlyList<ShopItem>
            Items => items;
    }
}
