// ============================================================================
// Level01SaveData.cs
// ============================================================================

using System;

namespace ProjectSpark.Gameplay.Flashlight
{
    [Serializable]
    public class Level01SaveData
    {
        public bool Opened;

        public bool BatteryRemoved;

        public bool PCBInspected;

        public bool ResistorRemoved;

        public bool ReplacementInstalled;

        public bool Soldered;

        public bool BatteryInserted;

        public bool Tested;

        public bool Completed;
    }
}