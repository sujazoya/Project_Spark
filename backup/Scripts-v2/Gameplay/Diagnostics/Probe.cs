using UnityEngine;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class Probe : MonoBehaviour
    {
        [SerializeField]
        private ProbeTarget target;

        public ProbeTarget Target => target;

        public bool IsConnected =>
            target != null;

        public void Connect(
            ProbeTarget value)
        {
            target = value;
        }

        public void Disconnect()
        {
            target = null;
        }
    }
}
