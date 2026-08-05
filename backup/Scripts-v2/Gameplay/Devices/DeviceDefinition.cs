using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Devices
{
    [CreateAssetMenu(
        menuName="Project Spark/Devices/Definition")]
    public sealed class DeviceDefinition
        : ScriptableObject
    {
        public string DeviceId;

        public string DisplayName;

        public GameObject Prefab;

        public List<string> RequiredParts =
            new();
    }
}
