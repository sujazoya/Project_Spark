using UnityEngine;

namespace ProjectSpark.Gameplay.Devices
{
    public sealed class DeviceSocket
        : MonoBehaviour
    {
        [SerializeField]
        private string socketId;

        private DevicePart current;

        public bool Occupied =>
            current != null;

        public void Attach(
            DevicePart part)
        {
            current = part;
        }

        public void Detach()
        {
            current = null;
        }
    }
}
