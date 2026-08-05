using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Devices
{
    public sealed class DeviceManager
        : MonoBehaviour
    {
        [SerializeField]
        private List<DeviceDefinition>
            definitions =
                new();

        private readonly DeviceRegistry
            registry =
                new();

        private readonly DeviceFactory
            factory =
                new();

        private void Awake()
        {
            foreach (var device
                in definitions)
            {
                registry.Register(device);
            }
        }

        public DeviceInstance Spawn(
            string id)
        {
            if (!registry.TryGet(
                id,
                out var definition))
                return null;

            DeviceInstance instance =
                factory.Create(definition);

            DeviceEvents
                .RaiseSpawned(instance);

            return instance;
        }
    }
}
