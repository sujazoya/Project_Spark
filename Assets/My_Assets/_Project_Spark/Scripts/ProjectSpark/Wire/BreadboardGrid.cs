using UnityEngine;

namespace ProjectSpark.Circuit
{
    public sealed class BreadboardGrid : MonoBehaviour
    {
        [SerializeField] private Transform gridOrigin;
        [SerializeField, Min(0.001f)] private float spacing = 0.1f;
        [SerializeField, Min(1)] private int columns = 64;
        [SerializeField, Min(1)] private int rows = 32;
        public Transform GridOrigin => gridOrigin != null ? gridOrigin : transform;

        public bool TryGetNearestHole(Vector3 worldPosition, float maxDistance, out Vector3 hole, out int column, out int row)
        {
            Vector3 local = GridOrigin.InverseTransformPoint(worldPosition);
            column = Mathf.RoundToInt(local.x / spacing);
            row = Mathf.RoundToInt(local.z / spacing);
            if (column < 0 || column >= columns || row < 0 || row >= rows) { hole = default; return false; }
            hole = GridOrigin.TransformPoint(new Vector3(column * spacing, 0f, row * spacing));
            return (hole - worldPosition).sqrMagnitude <= maxDistance * maxDistance;
        }
    }
}
