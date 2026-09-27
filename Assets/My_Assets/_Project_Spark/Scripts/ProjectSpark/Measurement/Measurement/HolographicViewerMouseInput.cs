using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.HolographicViewer
{
    /// <summary>
    /// Handles mouse input for the holographic viewer.
    ///
    /// Gizmo rotation has priority over normal viewer rotation.
    /// The viewer is locked only while an actual gizmo rotation
    /// session is active.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HolographicViewerMouseInput : MonoBehaviour
    {
        [Header("Viewer References")]
        [SerializeField]
        private HolographicObjectController objectController;

        [SerializeField]
        private HolographicViewerCamera viewerCamera;

        [Header("Rotation Gizmo")]
        [SerializeField]
        private SparkRotationGizmo rotationGizmo;

        [SerializeField]
        private HolographicComponentManipulator manipulator;

        [Header("Screen Rotation")]
        [SerializeField]
        private bool controllScreenSpace = true;

        [SerializeField]
        private bool rotateWithLeftMouse = true;

        [SerializeField]
        private float rotationDeadZone = 2f;

        [Header("Pan")]
        [SerializeField]
        private bool panWithMiddleMouse = true;

        [SerializeField]
        private float panSensitivity = 1f;

        [Header("Zoom")]
        [SerializeField]
        private bool enableWheelZoom = true;

        [Header("UI")]
        [SerializeField]
        private bool respectUI = true;

        private bool rotating;
        private bool panning;
        private bool componentRotating;

        private Vector2 rotationStartPosition;

        public bool IsComponentRotating
        {
            get { return componentRotating; }
        }

        public bool IsScreenSpaceControlEnabled
        {
            get { return controllScreenSpace; }
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void Update()
        {
            ResolveReferences();

            HandleZoom();

            /*
             * Component/gizmo rotation always owns the mouse
             * while an actual manipulation is active.
             */
            if (componentRotating)
            {
                HandleComponentRotation();
                return;
            }

            /*
             * Viewer controls are disabled only while the
             * manipulator has locked them.
             */
            if (!controllScreenSpace)
            {
                StopScreenManipulation();
                UpdateGizmoHover();
                return;
            }

            HandleRotation();
            HandlePan();
            UpdateGizmoHover();
        }

        // ============================================================
        // ROTATION
        // ============================================================

        private void HandleRotation()
{
    if (!rotateWithLeftMouse)
    {
        return;
    }

#if ENABLE_INPUT_SYSTEM

    if (Mouse.current == null)
    {
        return;
    }

    Vector2 pointer =
        Mouse.current.position.ReadValue();

    // ------------------------------------------------------------
    // MOUSE DOWN
    // ------------------------------------------------------------

    if (Mouse.current.leftButton.wasPressedThisFrame)
    {
        if (respectUI && IsPointerOverUI())
        {
            return;
        }

        /*
         * IMPORTANT:
         * The rotation gizmo gets first ownership of the mouse.
         *
         * If this succeeds, camera rotation NEVER starts.
         */
        if (TryBeginComponentRotation(pointer))
        {
            return;
        }

        /*
         * Gizmo did not claim the mouse.
         * Normal viewer/camera rotation is allowed.
         */
        BeginViewerRotation(pointer);
    }

    // ------------------------------------------------------------
    // MOUSE HELD
    // ------------------------------------------------------------

    if (rotating &&
        Mouse.current.leftButton.isPressed)
    {
        UpdateViewerRotation(pointer);
    }

    // ------------------------------------------------------------
    // MOUSE RELEASE
    // ------------------------------------------------------------

    if (rotating &&
        Mouse.current.leftButton.wasReleasedThisFrame)
    {
        StopRotation();
    }

#endif
}

private bool TryBeginComponentRotation(Vector2 pointer)
{
    if (rotationGizmo == null)
        return false;

    if (manipulator == null)
        return false;

    if (!rotationGizmo.IsVisible)
        return false;

    Transform target = rotationGizmo.Target;

    if (target == null)
        return false;

    if (!rotationGizmo.TryGetAxis(
            pointer,
            out SparkRotationGizmo.GizmoAxis axis))
    {
        return false;
    }

    if (axis == SparkRotationGizmo.GizmoAxis.None)
        return false;

    if (!rotationGizmo.TryGetWorldAxis(
            axis,
            out Vector3 worldAxis))
    {
        return false;
    }

    if (!rotationGizmo.TryPress(pointer))
        return false;

    if (!manipulator.BeginRotate(
            target,
            worldAxis,
            pointer,
            out string reason))
    {
        rotationGizmo.Cancel();
        return false;
    }

    componentRotating = true;

    rotating = false;
    panning = false;

    controllScreenSpace = false;

    return true;
}

        // ============================================================
        // COMPONENT ROTATION
        // ============================================================

        private void HandleComponentRotation()
        {
#if ENABLE_INPUT_SYSTEM

            if (Mouse.current == null)
            {
                StopComponentRotation(true);
                return;
            }

            /*
             * If the gizmo/tool was hidden while the drag was active,
             * terminate the manipulation safely.
             */
            if (rotationGizmo == null ||
                !rotationGizmo.IsVisible)
            {
                StopComponentRotation(true);
                return;
            }

            Vector2 pointer =
                Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.isPressed)
            {
                if (manipulator != null &&
                    manipulator.IsManipulating)
                {
                    manipulator.UpdateManipulation(pointer);
                }

                return;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                StopComponentRotation(false);
            }

#endif
        }

        // ============================================================
        // EXTERNAL STOP
        // ============================================================

        /// <summary>
        /// Used by SparkRotateTool when the Rotate tool is disabled.
        /// </summary>
        public void CancelComponentRotation()
        {
            StopComponentRotation(true);
        }

        // ============================================================
        // STOP COMPONENT ROTATION
        // ============================================================

        private void StopComponentRotation(
            bool cancel)
        {
            if (!componentRotating)
            {
                return;
            }

            componentRotating =
                false;

            if (manipulator != null &&
                manipulator.IsManipulating)
            {
                if (cancel)
                {
                    manipulator.Cancel();
                }
                else
                {
                    manipulator.Commit();
                }
            }

            if (rotationGizmo != null)
            {
                rotationGizmo.Release();
            }

            controllScreenSpace =
                true;
        }

        // ============================================================
        // VIEWER ROTATION
        // ============================================================

        private void BeginViewerRotation(
            Vector2 pointer)
        {
            if (objectController == null)
            {
                return;
            }

            rotating =
                false;

            rotationStartPosition =
                pointer;

            objectController.BeginDrag();

            rotating =
                true;
        }

        private void UpdateViewerRotation(
            Vector2 pointer)
        {
            if (!rotating)
            {
                return;
            }

            Vector2 delta =
                pointer -
                rotationStartPosition;

            if (delta.sqrMagnitude <
                rotationDeadZone *
                rotationDeadZone)
            {
                return;
            }

            rotationStartPosition =
                pointer;

            objectController.RotateFromDrag(
                delta);
        }

        public void StopRotation()
        {
            if (!rotating)
            {
                return;
            }

            rotating =
                false;

            if (objectController != null)
            {
                objectController.EndDrag();
            }
        }

        // ============================================================
        // PAN
        // ============================================================

        private void HandlePan()
        {
            if (!panWithMiddleMouse)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM

            if (Mouse.current == null)
            {
                return;
            }

            if (Mouse.current.middleButton.wasPressedThisFrame)
            {
                if (IsPointerOverUI())
                {
                    return;
                }

                panning =
                    true;
            }

            if (panning &&
                Mouse.current.middleButton.isPressed)
            {
                Vector2 delta =
                    Mouse.current.delta.ReadValue();

                if (viewerCamera != null)
                {
                    viewerCamera.Pan(
                        delta *
                        panSensitivity);
                }
            }

            if (panning &&
                Mouse.current.middleButton.wasReleasedThisFrame)
            {
                panning =
                    false;
            }

#endif
        }

        // ============================================================
        // ZOOM
        // ============================================================

        private void HandleZoom()
        {
            if (!enableWheelZoom)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM

            if (Mouse.current == null)
            {
                return;
            }

            Vector2 scroll =
                Mouse.current.scroll.ReadValue();

            if (Mathf.Approximately(
                    scroll.y,
                    0f))
            {
                return;
            }

            if (viewerCamera != null)
            {
                viewerCamera.Zoom(
                    scroll.y);
            }

#endif
        }

        // ============================================================
        // GIZMO HOVER
        // ============================================================

        private void UpdateGizmoHover()
        {
            if (componentRotating)
            {
                return;
            }

            if (rotationGizmo == null ||
                !rotationGizmo.IsVisible)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM

            if (Mouse.current == null)
            {
                return;
            }

            Vector2 pointer =
                Mouse.current.position.ReadValue();

            rotationGizmo.UpdateHover(
                pointer);

#endif
        }

        // ============================================================
        // SCREEN CONTROL
        // ============================================================

        public void SetScreenSpaceControl(
            bool enabled)
        {
            controllScreenSpace =
                enabled;

            if (!enabled)
            {
                StopScreenManipulation();
            }
        }

        private void StopScreenManipulation()
        {
            StopRotation();

            panning =
                false;
        }

        public void StopActiveScreenManipulation()
        {
            StopScreenManipulation();
        }

        // ============================================================
        // UI
        // ============================================================

        private bool IsPointerOverUI()
        {
            if (!respectUI)
            {
                return false;
            }

            return EventSystem.current != null &&
                   EventSystem.current.IsPointerOverGameObject();
        }

        public void ToggleControlScreenSpace()
{
    SetScreenSpaceControl(
        !controllScreenSpace);
}


        // ============================================================
        // REFERENCES
        // ============================================================

        private void ResolveReferences()
        {
            if (objectController == null)
            {
                objectController =
                    GetComponentInParent<
                        HolographicObjectController>();
            }

            if (viewerCamera == null)
            {
                viewerCamera =
                    GetComponentInParent<
                        HolographicViewerCamera>();
            }

            if (rotationGizmo == null)
            {
                rotationGizmo =
                    GetComponentInParent<
                        SparkRotationGizmo>();
            }

            if (manipulator == null)
            {
                manipulator =
                    GetComponentInParent<
                        HolographicComponentManipulator>();
            }
        }

        // ============================================================
        // DISABLE
        // ============================================================

        private void OnDisable()
        {
            StopComponentRotation(true);
            StopScreenManipulation();
        }
    }
}