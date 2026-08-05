using System;

namespace ProjectSpark.Core.SaveSystem
{
    [Serializable]
    public class SaveGame
    {
        public int Version =
            SaveVersion.Current;

        public long CreatedTime;

        public long ModifiedTime;

        public int Coins;

        public int XP;

        public int Reputation;

        public int CurrentScenario;

        public string WorkshopStateId;

        public string InventoryId;
    }
}
