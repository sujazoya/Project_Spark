using UnityEngine;
using ProjectSpark.Circuit;

namespace AAAUI.VFX
{
    public enum WirePolarity
    {
        Positive,
        Negative
    }

    [DisallowMultipleComponent]
    public sealed class SignalWireBuilder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private BreadboardGrid breadboard;

        [SerializeField]
        private SignalPath_Manager pathManager;

        [Header("Input")]
        [SerializeField]
        private Camera targetCamera;

        [SerializeField]
        private LayerMask breadboardLayer;

        [Header("Snapping")]
        [SerializeField, Min(0.001f)]
        private float snapDistance = 0.08f;

        private SignalPath currentPath;

        private WirePolarity currentPolarity;

        private bool drawing;

        private Vector3 currentEnd;

        private int lastColumn = -1;
        private int lastRow = -1;

        public SignalPath CurrentPath =>
            currentPath;

        public bool IsDrawing =>
            drawing;

        private CircuitTerminal startTerminal;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
        }
        public void BeginWire(
    Vector3 startPosition,
    WirePolarity polarity,
    CircuitTerminal terminal)
        {
            startTerminal =
                terminal;

            BeginWire(
                startPosition,
                polarity
            );
        }

        // =========================================================
        // START
        // =========================================================

        public void BeginWire(
            Vector3 startPosition,
            WirePolarity polarity)
        {
            currentPolarity =
                polarity;

            if (breadboard == null)
            {
                Debug.LogError(
                    "SignalWireBuilder: Breadboard is NULL."
                );

                return;
            }

            if (pathManager == null)
            {
                Debug.LogError(
                    "SignalWireBuilder: Path Manager is NULL."
                );

                return;
            }

            if (!breadboard.TryGetNearestHole(
                    startPosition,
                    snapDistance,
                    out Vector3 hole,
                    out int column,
                    out int row))
            {
                Debug.LogWarning(
                    "SignalWireBuilder: No breadboard hole found."
                );

                return;
            }

            // Manager creates a NEW path + NEW mesh.
            // Previous wires remain untouched.
            pathManager.CreateNextWire(
                polarity
            );

            currentPath =
                pathManager.CurrentPath;

            if (currentPath == null)
            {
                Debug.LogError(
                    "SignalWireBuilder: Manager did not create a path."
                );

                return;
            }

            currentPath.AddPoint(
                hole
            );

            currentPath.SetPreviewPoint(
                hole
            );

            lastColumn =
                column;

            lastRow =
                row;

            currentEnd =
                hole;

            drawing =
                true;

            Rebuild();
        }

        // =========================================================
        // INPUT
        // =========================================================

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

            // Default input mode.
            // Your CircuitTerminal can call the
            // overload with explicit polarity.
            BeginWire(
                hit.point,
                WirePolarity.Positive
            );
        }

        // =========================================================
        // DRAGGING
        // =========================================================

        private void UpdateDragging()
        {
            if (currentPath == null)
                return;

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

            UpdateWire(
                worldPosition
            );
        }

        public void UpdateWire(
            Vector3 worldPosition)
        {
            if (!drawing ||
                currentPath == null)
            {
                return;
            }

            currentEnd =
                worldPosition;

            if (breadboard.TryGetNearestHole(
                    worldPosition,
                    snapDistance,
                    out Vector3 hole,
                    out int column,
                    out int row))
            {
                currentEnd =
                    hole;

                currentPath.SetPreviewPoint(
                    hole
                );

                // EXACTLY your existing point-add logic.
                if (column != lastColumn ||
                    row != lastRow)
                {
                    currentPath.AddPoint(
                        hole
                    );

                    lastColumn =
                        column;

                    lastRow =
                        row;
                }
            }
            else
            {
                currentPath.SetPreviewPoint(
                    worldPosition
                );
            }

            Rebuild();
        }

        // =========================================================
        // FINISH
        // =========================================================

        public void EndWire()
        {
            if (!drawing)
                return;

            if (!drawing)
                return;

            Debug.Log("[WIRE] RELEASE");

            CircuitTerminal endTerminal =
                FindTerminalUnderMouse();

            if (endTerminal == null)
            {
                Debug.Log(
                    "[WIRE] END TERMINAL = NULL"
                );
            }
            else
            {
                Debug.Log(
                    $"[WIRE] END = {endTerminal.name} | " +
                    $"Polarity = {endTerminal.Polarity}"
                );
            }           

            if (endTerminal != null &&
                startTerminal != null &&
                endTerminal != startTerminal)
            {
                startTerminal.ReceiveConnection(
                    endTerminal
                );
            }

            drawing = false;

            if (currentPath != null)
            {
                currentPath.ClearPreview();
            }

            Rebuild();

            if (pathManager != null)
            {
                pathManager.FinishCurrentPath();
            }

            currentPath = null;
            startTerminal = null;

            lastColumn = -1;
            lastRow = -1;
        }

        public void CancelWire()
        {
            if (!drawing)
                return;

            drawing =
                false;

            /*
             * We intentionally do NOT clear the path here.
             *
             * If you later want Cancel to remove ONLY
             * the currently created wire, we can add that
             * separately.
             */

            if (currentPath != null)
            {
                currentPath.ClearPreview();
            }

            Rebuild();

            pathManager.FinishCurrentPath();

            currentPath =
                null;

            lastColumn =
                -1;

            lastRow =
                -1;
        }

        // =========================================================
        // MESH
        // =========================================================

        private void Rebuild()
        {
            if (pathManager == null)
                return;

            SignalPathMesh mesh =
                pathManager.CurrentMesh;

            if (mesh == null)
                return;

            mesh.Rebuild();
        }
        private CircuitTerminal FindTerminalUnderMouse()
        {
            if (targetCamera == null)
                return null;

            Ray ray =
                targetCamera.ScreenPointToRay(
                    Input.mousePosition
                );

            RaycastHit[] hits =
                Physics.RaycastAll(
                    ray,
                    1000f
                );

            for (int i = 0; i < hits.Length; i++)
            {
                CircuitTerminal terminal =
                    hits[i].collider
                        .GetComponentInParent<CircuitTerminal>();

                if (terminal != null)
                {
                    Debug.Log(
                        $"[WIRE] TERMINAL HIT = {terminal.name}"
                    );

                    return terminal;
                }
            }

            return null;
        }
    }
}