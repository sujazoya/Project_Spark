
using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [CreateAssetMenu(
        fileName = "SparkLevelLayoutCatalog",
        menuName = "Project Spark/Level Layout Catalog")]
    public sealed class SparkLevelLayoutCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public SparkLevelDefinition level;
            public SparkLevelLayout prefab;
        }

        [SerializeField]
        private Entry[] entries =
            Array.Empty<Entry>();

        public SparkLevelLayout GetPrefab(
            SparkLevelDefinition level)
        {
            if (level == null || entries == null)
                return null;

            foreach (Entry entry in entries)
            {
                if (entry == null)
                    continue;

                if (entry.level == level)
                    return entry.prefab;
            }

            return null;
        }
    }
}
