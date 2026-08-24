using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicObjectInteraction : MonoBehaviour
    {
        private static readonly int HoverAmountID =
            Shader.PropertyToID("_HoverAmount");

        [Header("References")]
        [SerializeField] private Camera viewerCamera;
        [SerializeField] private HolographicObjectController objectController;
        [SerializeField] private HolographicViewerCamera cameraController;

        [Header("Object")]
        [SerializeField] private Renderer[] renderers;

        [Header("Raycast")]
        [SerializeField] private LayerMask objectLayer;
        [SerializeField] private float rayDistance = 100f;

        [Header("Rotation")]
        [SerializeField] private float rotateSensitivity = 0.25f;

        [Header("Pan")]
        [SerializeField] private float panSensitivity = 0.003f;

        [Header("Double Click")]
        [SerializeField] private float doubleClickTime = 0.3f;

        private bool isHovering;
        private bool isRotating;
        private bool isPanning;

        private float lastClickTime = -10f;

#if ENABLE_INPUT_SYSTEM
        private Vector2 previousMousePosition;
#endif

        private void Awake()
        {
            if (renderers == null || renderers.Length == 0)
            {
                renderers =
                    GetComponentsInChildren<Renderer>(true);
            }

            SetHoverVisual(false);
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            UpdateMouse();
#endif
        }

#if ENABLE_INPUT_SYSTEM

        private void UpdateMouse()
        {
            if (Mouse.current == null ||
                viewerCamera == null)
                return;

            Vector2 mousePosition =
                Mouse.current.position.ReadValue();

            bool pointerOverInteractiveUI =
                IsPointerOverUI();

            bool hitObject = false;

            if (!pointerOverInteractiveUI)
            {
                hitObject = RaycastObject(mousePosition);
            }

            UpdateHover(hitObject);

            HandleRotation(
                mousePosition,
                hitObject,
                pointerOverInteractiveUI
            );

            HandlePan(
                mousePosition,
                pointerOverInteractiveUI
            );

            HandleZoom();

            HandleClick(
                hitObject,
                pointerOverInteractiveUI
            );
        }

        private bool RaycastObject(Vector2 mousePosition)
        {
            Ray ray =
                viewerCamera.ScreenPointToRay(mousePosition);

            return Physics.Raycast(
                ray,
                rayDistance,
                objectLayer,
                QueryTriggerInteraction.Ignore
            );
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
                return false;

            return EventSystem.current.IsPointerOverGameObject();
        }

        private void HandleRotation(
            Vector2 mousePosition,
            bool hitObject,
            bool pointerOverUI)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (hitObject && !pointerOverUI)
                {
                    isRotating = true;

                    previousMousePosition =
                        mousePosition;

                    objectController.BeginDrag();
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (isRotating)
                {
                    isRotating = false;
                    objectController.EndDrag();
                }
            }

            if (!isRotating)
                return;

            Vector2 delta =
                mousePosition -
                previousMousePosition;

            objectController.RotateFromDrag(
                delta * rotateSensitivity
            );

            previousMousePosition =
                mousePosition;
        }

        private void HandlePan(
            Vector2 mousePosition,
            bool pointerOverUI)
        {
            if (Mouse.current.middleButton.wasPressedThisFrame)
            {
                if (!pointerOverUI)
                {
                    isPanning = true;

                    previousMousePosition =
                        mousePosition;
                }
            }

            if (Mouse.current.middleButton.wasReleasedThisFrame)
            {
                isPanning = false;
            }

            if (!isPanning)
                return;

            Vector2 delta =
                mousePosition -
                previousMousePosition;

            cameraController.Pan(
                delta * panSensitivity
            );

            previousMousePosition =
                mousePosition;
        }

        private void HandleZoom()
        {
            float scroll =
                Mouse.current.scroll.ReadValue().y;

            if (Mathf.Abs(scroll) < 0.01f)
                return;

            cameraController.Zoom(scroll);
        }

        private void HandleClick(
            bool hitObject,
            bool pointerOverUI)
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return;

            if (pointerOverUI)
                return;

            if (!hitObject)
                return;

            float currentTime =
                Time.unscaledTime;

            float delta =
                currentTime -
                lastClickTime;

            if (delta <= doubleClickTime)
            {
                objectController.ResetView();
                cameraController.ResetView();

                lastClickTime = -10f;
                return;
            }

            lastClickTime = currentTime;
        }

#endif

        private void UpdateHover(bool value)
        {
            if (isHovering == value)
                return;

            isHovering = value;

            SetHoverVisual(value);
        }

        private void SetHoverVisual(bool value)
        {
            float amount = value ? 1f : 0f;

            if (renderers == null)
                return;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];

                if (renderer == null)
                    continue;

                MaterialPropertyBlock block =
                    new MaterialPropertyBlock();

                renderer.GetPropertyBlock(block);

                block.SetFloat(
                    HoverAmountID,
                    amount
                );

                renderer.SetPropertyBlock(block);
            }
        }
    }
}