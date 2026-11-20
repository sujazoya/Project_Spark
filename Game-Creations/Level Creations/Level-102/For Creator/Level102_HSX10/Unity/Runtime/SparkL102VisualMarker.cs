using UnityEngine;
namespace ProjectSpark.Level102
{
    public sealed class SparkL102VisualMarker : MonoBehaviour
    {
        [Tooltip("Art-only model or footprint reference, never an electrical net identifier.")]
        public string assetReference;
        [Tooltip("True only after your authoritative PCB placement service confirms installation.")]
        public bool installedVisualState;
    }
}
