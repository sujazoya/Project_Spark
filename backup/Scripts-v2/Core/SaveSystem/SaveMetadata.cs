using System;

namespace ProjectSpark.Core.SaveSystem
{
    [Serializable]
    public class SaveMetadata
    {
        public string PlayerName;

        public int WorkshopLevel;

        public float PlayTime;

        public string LastPlayed;

        public int SaveVersion;
    }
}
