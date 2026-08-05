using System;

namespace ProjectSpark.Gameplay.Career
{
    [Serializable]
    public sealed class WorkOrder
    {
        public string Id;

        public CustomerProfile Customer;

        public DeviceProfile Device;

        public string Complaint;

        public int Reward;

        public int XP;

        public WorkOrderStatus Status;

        public float TimeLimit;
    }
}
