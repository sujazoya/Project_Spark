using UnityEngine;
using ProjectSpark.Circuit;

namespace AAAUI.VFX
{
    [DisallowMultipleComponent]
    public sealed class SignalWireBuilder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private BreadboardGrid breadboard;

        [SerializeField]
        private SignalPath path;

        [SerializeField]
        private SignalPathMesh pathMesh;

        [Header("Input")]
        [SerializeField]
        private Camera targetCamera;

        [SerializeField]
        private LayerMask breadboardLayer;

        [Header("Snapping")]
        [SerializeField, Min(0.001f)]
        private float snapDistance = 0.08f;

        private bool drawing;

        private Vector3 currentEnd;

        private int lastColumn = -1;
        private int lastRow = -1;

        public bool IsDrawing => drawing;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        private void Update()
        {
            if (targetCamera == null)
                return;

            if (!drawing)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    TryBeginWire();
                }

                return;
            }

            UpdateDragging();

            if (Input.GetMouseButtonUp(0))
            {
                EndWire();
            }
        }

        private void TryBeginWire()
        {
            Ray ray =
                targetCamera.ScreenPointToRay(
                    Input.mousePosition
                );

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    1000f,
                    breadboardLayer))
            {
                return;
            }

            BeginWire(hit.point);
        }

        private void UpdateDragging()
        {
            Ray ray =
                targetCamera.ScreenPointToRay(
                    Input.mousePosition
                );

            Transform gridTransform =
                breadboard.GridOrigin;

            if (gridTransform == null)
                return;

            Plane plane =
                new Plane(
                    gridTransform.up,
                    gridTransform.position
                );

            if (!plane.Raycast(
                    ray,
                    out float distance))
            {
                return;
            }

            Vector3 worldPosition =
                ray.GetPoint(distance);

            UpdateWire(worldPosition);
        }

        public void BeginWire(Vector3 startPosition)
        {
            if (breadboard == null ||
                path == null)
                return;

            if (!breadboard.TryGetNearestHole(
                    startPosition,
                    snapDistance,
                    out Vector3 hole,
                    out int column,
                    out int row))
            {
                return;
            }

            path.Clear();

            path.AddPoint(hole);

            lastColumn = column;
            lastRow = row;

            currentEnd = hole;

            drawing = true;

            Rebuild();
        }

        public void UpdateWire(Vector3 worldPosition)
        {
            if (!drawing)
                return;

            currentEnd = worldPosition;

            if (breadboard.TryGetNearestHole(
                    worldPosition,
                    snapDistance,
                    out Vector3 hole,
                    out int column,
                    out int row))
            {
                currentEnd = hole;

                // Show the snapped endpoint.
                path.SetPreviewPoint(hole);

                // Add the hole only once.
                if (column != lastColumn ||
                    row != lastRow)
                {
                    path.AddPoint(hole);

                    lastColumn = column;
                    lastRow = row;
                }
            }
            else
            {
                // Free endpoint follows mouse.
                path.SetPreviewPoint(worldPosition);
            }

            Rebuild();
        }
        public void EndWire()
        {
            if (!drawing)
                return;

            drawing = false;

            path.ClearPreview();

            Rebuild();
        }

        public void CancelWire()
        {
            drawing = false;

            if (path != null)
                path.Clear();

            Rebuild();
        }

        private void Rebuild()
        {
            if (pathMesh != null)
                pathMesh.Rebuild();
        }
    }
}