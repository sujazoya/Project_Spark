using System;

namespace ProjectSpark.Gameplay.Career
{
    [Serializable]
    public sealed class DeviceProfile
    {
        public string DeviceId;

        public string DisplayName;

        public string Manufacturer;

        public int Difficulty;

        public int EstimatedMinutes;
    }
}
