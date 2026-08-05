using UnityEngine;

namespace ProjectSpark.Circuit
{
    [DisallowMultipleComponent]
    public sealed class BreadboardGrid : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField, Min(1)]
        private int columns = 30;

        [SerializeField, Min(1)]
        private int rows = 10;

        [SerializeField, Min(0.001f)]
        private float spacingX = 0.1f;

        [SerializeField, Min(0.001f)]
        private float spacingZ = 0.1f;

        [Header("Grid Orientation")]
        [SerializeField]
        private Transform gridOrigin;
        public Transform GridOrigin => gridOrigin;

        [Header("Gizmos")]
        [SerializeField]
        private bool drawGizmos = true;

        [SerializeField]
        private bool drawLabels = false;

        [SerializeField, Min(0.001f)]
        private float gizmoRadius = 0.01f;

        public int Columns => columns;
        public int Rows => rows;

        public float SpacingX => spacingX;
        public float SpacingZ => spacingZ;

        public Vector3 GetHolePosition(int column, int row)
        {
            if (gridOrigin == null)
                return transform.position;

            column = Mathf.Clamp(
                column,
                0,
                columns - 1
            );

            row = Mathf.Clamp(
                row,
                0,
                rows - 1
            );

            Vector3 localPosition = new Vector3(
                column * spacingX,
                0f,
                row * spacingZ
            );

            return gridOrigin.TransformPoint(
                localPosition
            );
        }

        public bool TryGetNearestHole(
            Vector3 worldPosition,
            float maxDistance,
            out Vector3 holePosition,
            out int column,
            out int row)
        {
            holePosition = default;
            column = -1;
            row = -1;

            if (gridOrigin == null)
                return false;

            Vector3 localPosition =
                gridOrigin.InverseTransformPoint(
                    worldPosition
                );

            column = Mathf.RoundToInt(
                localPosition.x / spacingX
            );

            row = Mathf.RoundToInt(
                localPosition.z / spacingZ
            );

            if (column < 0 ||
                column >= columns ||
                row < 0 ||
                row >= rows)
            {
                return false;
            }

            holePosition =
                GetHolePosition(column, row);

            return Vector3.Distance(
                worldPosition,
                holePosition
            ) <= maxDistance;
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
                return;

            if (gridOrigin == null)
                return;

            if (columns <= 0 || rows <= 0)
                return;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    Vector3 position =
                        GetHolePosition(
                            column,
                            row
                        );

                    Gizmos.DrawWireSphere(
                        position,
                        gizmoRadius
                    );

                    if (drawLabels)
                    {
                        UnityEditor.Handles.Label(
                            position +
                            gridOrigin.up *
                            gizmoRadius * 2f,

                            $"{column},{row}"
                        );
                    }
                }
            }

            // X spacing reference
            if (columns > 1)
            {
                Vector3 a =
                    GetHolePosition(0, 0);

                Vector3 b =
                    GetHolePosition(1, 0);

                Gizmos.DrawLine(a, b);
            }

            // Z spacing reference
            if (rows > 1)
            {
                Vector3 a =
                    GetHolePosition(0, 0);

                Vector3 b =
                    GetHolePosition(0, 1);

                Gizmos.DrawLine(a, b);
            }

            // Draw orientation axes.
            Vector3 origin =
                gridOrigin.position;

            Gizmos.DrawLine(
                origin,
                origin + gridOrigin.right * spacingX
            );

            Gizmos.DrawLine(
                origin,
                origin + gridOrigin.forward * spacingZ
            );
        }

#endif
    }
}