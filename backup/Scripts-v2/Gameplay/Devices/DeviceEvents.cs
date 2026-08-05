using System;

namespace ProjectSpark.Gameplay.Devices
{
    public static class DeviceEvents
    {
        public static event Action<DeviceInstance>
            DeviceSpawned;

        public static event Action<DeviceInstance>
            DeviceDestroyed;

        public static event Action<DeviceInstance>
            DeviceRepaired;

        public static void RaiseSpawned(
            DeviceInstance device)
        {
            DeviceSpawned?.Invoke(device);
        }

        public static void RaiseDestroyed(
            DeviceInstance device)
        {
            DeviceDestroyed?.Invoke(device);
        }

        public static void RaiseRepaired(
            DeviceInstance device)
        {
            DeviceRepaired?.Invoke(device);
        }
    }
}
