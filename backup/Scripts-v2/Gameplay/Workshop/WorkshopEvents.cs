using System;

namespace ProjectSpark.Gameplay.Workshop
{
    public static class WorkshopEvents
    {
        public static event Action
            WorkshopLevelUp;

        public static event Action<string>
            UpgradeUnlocked;

        public static void RaiseLevelUp()
        {
            WorkshopLevelUp?.Invoke();
        }

        public static void RaiseUpgrade(
            string id)
        {
            UpgradeUnlocked?.Invoke(id);
        }
    }
}
