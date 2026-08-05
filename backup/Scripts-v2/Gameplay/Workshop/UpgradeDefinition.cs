using UnityEngine;

namespace ProjectSpark.Gameplay.Workshop
{
    [CreateAssetMenu(
        menuName="Project Spark/Workshop Upgrade")]
    public sealed class UpgradeDefinition
        : ScriptableObject
    {
        public string UpgradeId;

        public string DisplayName;

        public int Cost;

        public int RequiredLevel;
    }
}
