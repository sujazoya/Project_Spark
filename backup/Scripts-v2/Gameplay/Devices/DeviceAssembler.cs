using UnityEngine;

namespace ProjectSpark.Gameplay.Devices
{
    public sealed class DeviceAssembler
    {
        public bool Validate(
            DeviceInstance device)
        {
            return
                device != null &&
                device.Definition != null;
        }
    }
}
