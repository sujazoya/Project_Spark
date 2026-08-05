using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Devices
{
    public sealed class DeviceRegistry
    {
        private readonly Dictionary<string,
            DeviceDefinition> devices =
                new();

        public void Register(
            DeviceDefinition definition)
        {
            devices[definition.DeviceId] =
                definition;
        }

        public bool TryGet(
            string id,
            out DeviceDefinition definition)
        {
            return devices.TryGetValue(
                id,
                out definition);
        }
    }
}
