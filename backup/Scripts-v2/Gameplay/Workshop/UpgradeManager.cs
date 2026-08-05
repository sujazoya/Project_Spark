using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Workshop
{
    public sealed class UpgradeManager
    {
        private readonly HashSet<string>
            unlocked = new();

        public bool Unlock(
            UpgradeDefinition upgrade)
        {
            return unlocked.Add(
                upgrade.UpgradeId);
        }

        public bool IsUnlocked(
            string id)
        {
            return unlocked.Contains(id);
        }
    }
}
