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
    /// Responsibilities:
    /// - Left-drag rotates the holographic object.
    /// - Middle-drag pans the viewer.
    /// - Mouse wheel zooms the viewer.
    ///
    /// This class does not implement camera or object transformation logic.
    /// Those responsibilities remain in HolographicViewerCamera and
    /// HolographicObjectController.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HolographicViewerMouseInput : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HolographicObjectController objectController;
        [SerializeField] private HolographicViewerCamera viewerCamera;

        [Header("Input")]
        [SerializeField] private bool respectUI = true;
        [SerializeField] private bool rotateWithLeftMouse = true;
        [SerializeField] private bool panWithMiddleMouse = true;
        [SerializeField] private bool enableWheelZoom = true;

        [Header("Sensitivity")]
        [SerializeField, Min(0.0001f)]
        private float panSensitivity = 0.0025f;

        [Header("Options")]
        [SerializeField] private bool requireTargetForRotation = true;
        public bool controllScreenSpace = false;

#if ENABLE_INPUT_SYSTEM

        private bool rotating;
        private bool panning;

#endif

        private void Reset()
        {
            objectController =
                GetComponent<HolographicObjectController>();

            viewerCamera =
                GetComponent<HolographicViewerCamera>();
        }

        private void Awake()
        {
            if (objectController == null)
            {
                objectController =
                    GetComponent<HolographicObjectController>();
            }

            if (viewerCamera == null)
            {
                viewerCamera =
                    GetComponent<HolographicViewerCamera>();
            }
        }

#if ENABLE_INPUT_SYSTEM

        private void Update()
        {
            Mouse mouse = Mouse.current;

            if (mouse == null)
                return;
                if(controllScreenSpace)
                {
                    if (objectController != null)
                    {
                        HandleZoom(mouse);
                        HandleRotation(mouse);
                        HandlePan(mouse);
                    }
                }
                else
                {
                    
                }

            
        }

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

                if (delta.sqrMagnitude > 0f)
                {
                    objectController.RotateFromDrag(delta);
                }
            }

            if (rotating &&
                mouse.leftButton.wasReleasedThisFrame)
            {
                rotating = false;
                objectController.EndDrag();
            }
        }

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

                if (delta.sqrMagnitude > 0f)
                {
                    viewerCamera.Pan(
                        -delta * panSensitivity);
                }
            }

            if (panning &&
                mouse.middleButton.wasReleasedThisFrame)
            {
                panning = false;
            }
        }

        private bool HasValidRotationTarget()
        {
            if (objectController == null)
                return false;

            return objectController.gameObject != null;
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

#endif

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM

            if (rotating &&
                objectController != null)
            {
                objectController.EndDrag();
            }

            rotating = false;
            panning = false;

#endif
        }
        public void ToggleControlScreenSpace()
        {
            controllScreenSpace = !controllScreenSpace;
        }
    }

}