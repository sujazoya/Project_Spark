using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WirePin : MonoBehaviour
    {
        [SerializeField]
        private string pinId;

        [SerializeField]
        private bool output;

        [SerializeField]
        private bool input;

        public string PinId => pinId;

        public bool IsOutput => output;

        public bool IsInput => input;

        public Vector3 WorldPosition
            => transform.position;
    }
}
