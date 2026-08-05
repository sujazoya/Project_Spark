using UnityEngine;

namespace ProjectSpark.Gameplay.Devices
{
    public sealed class DeviceInstance
        : MonoBehaviour
    {
        [SerializeField]
        private DeviceDefinition definition;

        [SerializeField]
        private DeviceState state;

        public DeviceDefinition Definition =>
            definition;

        public DeviceState State =>
            state;

        public void SetState(
            DeviceState value)
        {
            state = value;
        }
    }
}
