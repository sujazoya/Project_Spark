using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.HolographicViewer
{
    /// <summary>
    /// Desktop mouse input bridge for the Project Spark holographic viewer.
    ///
    /// Viewer controls:
    /// - Left mouse drag   -> viewer/object rotation
    /// - Middle mouse drag -> viewer pan
    /// - Mouse wheel       -> viewer zoom
    ///
    /// Screen rotation/pan can be locked while an engineering component is
    /// being moved or rotated.
    ///
    /// This component does not implement transformation logic. It delegates
    /// rotation to HolographicObjectController and pan/zoom to
    /// HolographicViewerCamera.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HolographicViewerMouseInput : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private HolographicObjectController objectController;

        [SerializeField]
        private HolographicViewerCamera viewerCamera;

        [Header("Screen Control")]
        [Tooltip(
            "When enabled, screen rotation and pan are allowed. " +
            "Disable this while manipulating an engineering object.")]
        [SerializeField]
        private bool controllScreenSpace = true;

        [Header("Input")]
        [SerializeField]
        private bool respectUI = true;

        [Header("Rotation")]
        [SerializeField]
        private bool rotateWithLeftMouse = true;

        [SerializeField, Min(0f)]
        private float rotationDeadZone = 0.01f;

        [Header("Pan")]
        [SerializeField]
        private bool panWithMiddleMouse = true;

        [SerializeField, Min(0.0001f)]
        private float panSensitivity = 0.0025f;

        [Header("Zoom")]
        [SerializeField]
        private bool enableWheelZoom = true;

        [Header("Options")]
        [SerializeField]
        private bool requireTargetForRotation = true;

#if ENABLE_INPUT_SYSTEM

        private bool rotating;
        private bool panning;

#endif

        /// <summary>
        /// True when screen-level rotation and pan are allowed.
        /// </summary>
        public bool IsScreenSpaceControlEnabled =>
            controllScreenSpace;

        private void Reset()
        {
            objectController =
                GetComponent<HolographicObjectController>();

            viewerCamera =
                GetComponent<HolographicViewerCamera>();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
        }

#if ENABLE_INPUT_SYSTEM

        private void Update()
        {
            Mouse mouse = Mouse.current;

            if (mouse == null)
                return;

            HandleZoom(mouse);

            if (!controllScreenSpace)
            {
                StopActiveScreenManipulation();
                return;
            }

            HandleRotation(mouse);
            HandlePan(mouse);
        }

#endif

        // ============================================================
        // ZOOM
        // ============================================================

#if ENABLE_INPUT_SYSTEM

        private void HandleZoom(Mouse mouse)
        {
            if (!enableWheelZoom)
                return;

            if (viewerCamera == null)
                return;

            if (IsBlockedByUI())
                return;

            float wheel =
                mouse.scroll.ReadValue().y;

            if (Mathf.Abs(wheel) <= Mathf.Epsilon)
                return;

            viewerCamera.Zoom(wheel);
        }

#endif

        // ============================================================
        // SCREEN ROTATION
        // ============================================================

#if ENABLE_INPUT_SYSTEM

        private void HandleRotation(Mouse mouse)
        {
            if (!rotateWithLeftMouse)
                return;

            if (objectController == null)
                return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (IsBlockedByUI())
                    return;

                if (requireTargetForRotation &&
                    !HasValidRotationTarget())
                {
                    return;
                }

                rotating = true;

                objectController.BeginDrag();
            }

            if (rotating &&
                mouse.leftButton.isPressed)
            {
                Vector2 delta =
                    mouse.delta.ReadValue();

                if (delta.sqrMagnitude >
                    rotationDeadZone *
                    rotationDeadZone)
                {
                    objectController.RotateFromDrag(
                        delta);
                }
            }

            if (rotating &&
                mouse.leftButton.wasReleasedThisFrame)
            {
                StopRotation();
            }
        }

        private void StopRotation()
        {
            if (!rotating)
                return;

            rotating = false;

            if (objectController != null)
            {
                objectController.EndDrag();
            }
        }

#endif

        // ============================================================
        // SCREEN PAN
        // ============================================================

#if ENABLE_INPUT_SYSTEM

        private void HandlePan(Mouse mouse)
        {
            if (!panWithMiddleMouse)
                return;

            if (viewerCamera == null)
                return;

            if (mouse.middleButton.wasPressedThisFrame)
            {
                if (IsBlockedByUI())
                    return;

                panning = true;
            }

            if (panning &&
                mouse.middleButton.isPressed)
            {
                Vector2 delta =
                    mouse.delta.ReadValue();

                if (delta.sqrMagnitude >
                    0f)
                {
                    viewerCamera.Pan(
                        -delta *
                        panSensitivity);
                }
            }

            if (panning &&
                mouse.middleButton.wasReleasedThisFrame)
            {
                panning = false;
            }
        }

#endif

        // ============================================================
        // CONTROL LOCK
        // ============================================================

        /// <summary>
        /// Enables or disables screen rotation and pan.
        ///
        /// This should be set to false while an engineering component is being
        /// moved or rotated, so the viewer cannot consume the same mouse input.
        ///
        /// Mouse-wheel zoom remains independently controlled by
        /// enableWheelZoom.
        /// </summary>
        public void SetScreenSpaceControl(
            bool enabled)
        {
            if (controllScreenSpace == enabled)
                return;

            controllScreenSpace = enabled;

            if (!enabled)
            {
                StopActiveScreenManipulation();
            }
        }

        /// <summary>
        /// Toggles screen rotation/pan control.
        /// </summary>
        public void ToggleControlScreenSpace()
        {
            SetScreenSpaceControl(
                !controllScreenSpace);
        }

        /// <summary>
        /// Immediately stops any active screen rotation or pan interaction.
        /// </summary>
        public void StopActiveScreenManipulation()
        {
#if ENABLE_INPUT_SYSTEM

            StopRotation();

            panning = false;

#endif
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        private bool HasValidRotationTarget()
        {
            return objectController != null &&
                   objectController.gameObject != null;
        }

        private bool IsBlockedByUI()
        {
            if (!respectUI)
                return false;

            EventSystem eventSystem =
                EventSystem.current;

            if (eventSystem == null)
                return false;

            return eventSystem.IsPointerOverGameObject();
        }

        // ============================================================
        // REFERENCES
        // ============================================================

        private void ResolveReferences()
        {
            if (objectController == null)
            {
                objectController =
                    GetComponent<
                        HolographicObjectController>();
            }

            if (viewerCamera == null)
            {
                viewerCamera =
                    GetComponent<
                        HolographicViewerCamera>();
            }

            if (viewerCamera == null)
            {
                viewerCamera =
                    FindFirstObjectByType<
                        HolographicViewerCamera>();
            }
        }

        // ============================================================
        // CLEANUP
        // ============================================================

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM

            StopActiveScreenManipulation();

#endif
        }
    }
}