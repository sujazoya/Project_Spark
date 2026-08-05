using System;

namespace ProjectSpark.Gameplay.Career
{
    [Serializable]
    public sealed class CustomerProfile
    {
        public string Id;

        public string Name;

        public int Reputation;

        public bool VIP;

        public string AvatarId;
    }
}
