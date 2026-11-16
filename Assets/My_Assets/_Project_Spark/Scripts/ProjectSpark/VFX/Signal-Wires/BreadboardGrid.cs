using UnityEngine;

namespace ProjectSpark.Circuit
{
    public sealed class BreadboardGrid : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private Transform gridOrigin;
        [SerializeField, Min(0.001f)] private float spacing = 0.1f;
        [SerializeField, Min(1)] private int columns = 64;
        [SerializeField, Min(1)] private int rows = 32;

        [Header("Scene View Hole Gizmos")]
        [SerializeField] private bool showHoleGizmos = true;
        [SerializeField] private bool drawOnlyWhenSelected = true;
        [SerializeField] private Color holeColor = new Color(0.1f, 0.85f, 1f, 0.85f);
        [SerializeField, Min(0.001f)] private float holeRadius = 0.012f;
        [SerializeField] private bool drawAsWireSpheres = true;
        [SerializeField] private bool showGridBounds = true;
        [SerializeField] private Color boundsColor = new Color(0.1f, 0.85f, 1f, 0.35f);

        public Transform GridOrigin => gridOrigin != null ? gridOrigin : transform;

        public bool TryGetNearestHole(Vector3 worldPosition, float maxDistance, out Vector3 hole, out int column, out int row)
        {
            Vector3 local = GridOrigin.InverseTransformPoint(worldPosition);
            column = Mathf.RoundToInt(local.x / spacing);
            row = Mathf.RoundToInt(local.z / spacing);
            if (column < 0 || column >= columns || row < 0 || row >= rows)
            {
                hole = default;
                return false;
            }

            hole = GridOrigin.TransformPoint(new Vector3(column * spacing, 0f, row * spacing));
            return (hole - worldPosition).sqrMagnitude <= maxDistance * maxDistance;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawOnlyWhenSelected)
                DrawHoleGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            if (drawOnlyWhenSelected)
                DrawHoleGizmos();
        }

        private void DrawHoleGizmos()
        {
            if (!showHoleGizmos || spacing <= 0f || columns < 1 || rows < 1)
                return;

            Transform origin = GridOrigin;
            if (origin == null)
                return;

            Gizmos.color = holeColor;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    // Identical transform/coordinates to TryGetNearestHole().
                    Vector3 worldHole = origin.TransformPoint(
                        new Vector3(column * spacing, 0f, row * spacing));
                    if (drawAsWireSpheres)
                        Gizmos.DrawWireSphere(worldHole, holeRadius);
                    else
                        Gizmos.DrawSphere(worldHole, holeRadius);
                }
            }

            if (showGridBounds)
            {
                Gizmos.color = boundsColor;
                Vector3 bottomLeft = origin.TransformPoint(Vector3.zero);
                Vector3 bottomRight = origin.TransformPoint(new Vector3((columns - 1) * spacing, 0f, 0f));
                Vector3 topLeft = origin.TransformPoint(new Vector3(0f, 0f, (rows - 1) * spacing));
                Vector3 topRight = origin.TransformPoint(new Vector3((columns - 1) * spacing, 0f, (rows - 1) * spacing));
                Gizmos.DrawLine(bottomLeft, bottomRight);
                Gizmos.DrawLine(bottomRight, topRight);
                Gizmos.DrawLine(topRight, topLeft);
                Gizmos.DrawLine(topLeft, bottomLeft);
            }
        }
#endif
    }
}
