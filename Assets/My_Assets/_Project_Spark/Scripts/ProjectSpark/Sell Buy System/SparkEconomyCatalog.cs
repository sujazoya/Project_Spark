using System;
using UnityEngine;

public enum SparkShopCategory
{
    Resistors,
    Capacitors,
    LEDs,
    Diodes,
    Transistors,
    Switches,
    Motors,
    Power,
    Connectors,
    Tools,
    ICs,
    Other
}

namespace ProjectSpark.Gameplay
{
    [CreateAssetMenu(fileName = "SparkEconomyCatalog", menuName = "Project Spark/Economy/Catalog")]
    public sealed class SparkEconomyCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Item
        {
            public string id;
            public string displayName;
            public Sprite icon;
            public GameObject prefab;
            [Min(0)] public int coinPrice;
            [Min(0)] public int diamondPrice;
            public bool sellForCoins = true;
            public bool sellForDiamonds;
        }

        [Header("Shop Classification")]
        public SparkShopCategory category = SparkShopCategory.Other;

        [SerializeField] private Item[] items = Array.Empty<Item>();
        public Item[] Items => items;

        public Item Find(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            foreach (Item item in items)
                if (item != null && item.id == id) return item;
            return null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (Item item in items)
            {
                if (item == null) continue;
                if (string.IsNullOrWhiteSpace(item.id) || !seen.Add(item.id))
                    Debug.LogWarning("[Spark Economy] Missing/duplicate catalog item ID on " + name, this);
            }
        }
#endif
    }
}
