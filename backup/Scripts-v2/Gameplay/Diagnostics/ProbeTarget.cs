using UnityEngine;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class ProbeTarget : MonoBehaviour
    {
        [SerializeField]
        private int nodeId;

        public int NodeId => nodeId;
    }
}
