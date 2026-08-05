using System;

namespace ProjectSpark.Core.SaveSystem
{
    [Serializable]
    public class SaveSlot
    {
        public int Slot;

        public SaveMetadata Metadata;

        public SaveGame Save;
    }
}
