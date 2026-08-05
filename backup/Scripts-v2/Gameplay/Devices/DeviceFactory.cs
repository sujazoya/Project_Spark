using UnityEngine;

namespace ProjectSpark.Gameplay.Devices
{
    public sealed class DeviceFactory
    {
        public DeviceInstance Create(
            DeviceDefinition definition)
        {
            GameObject obj =
                Object.Instantiate(
                    definition.Prefab);

            DeviceInstance instance =
                obj.GetComponent<DeviceInstance>();

            return instance;
        }
    }
}
