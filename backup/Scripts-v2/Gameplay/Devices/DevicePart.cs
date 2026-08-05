using UnityEngine;

namespace ProjectSpark.Gameplay.Devices
{
    public class DevicePart : MonoBehaviour
    {
        [SerializeField]
        private string partId;

        [SerializeField]
        private bool removable = true;

        public string PartId => partId;

        public bool Removable => removable;
    }
}
