using UnityEngine;

namespace ProjectSpark.Domain.Tools
{
    public sealed class Probe : MonoBehaviour
    {
        [SerializeField]
        private Transform tip;

        public Vector3 Position => tip.position;
    }
}
