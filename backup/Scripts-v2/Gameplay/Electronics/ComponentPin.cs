using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class ComponentPin : MonoBehaviour
    {
        [SerializeField]
        private string pinName;

        [SerializeField]
        private bool input;

        [SerializeField]
        private bool output;

        public string PinName => pinName;

        public bool IsInput => input;

        public bool IsOutput => output;
    }
}
