using System;

namespace ProjectSpark.Gameplay.Repair
{
    public static class RepairEvents
    {
        public static event Action<Fault>
            FaultFound;

        public static event Action<Fault>
            FaultRepaired;

        public static event Action
            DeviceFixed;

        public static void RaiseFound(
            Fault fault)
        {
            FaultFound?.Invoke(fault);
        }

        public static void RaiseRepaired(
            Fault fault)
        {
            FaultRepaired?.Invoke(fault);
        }

        public static void RaiseDeviceFixed()
        {
            DeviceFixed?.Invoke();
        }
    }
}
