using UnityEngine;
namespace ProjectSpark.Level97 {
    // Optional clickable hit target for existing world-space virtual repair UI.
    // Never grants gameplay completion or bypasses the electrical solver.
    public sealed class SparkLevel97BoardTarget : MonoBehaviour {
        public string componentId; public string padId; public string targetType;
        public Vector3 VfxContactPoint => transform.position;
    }
}
