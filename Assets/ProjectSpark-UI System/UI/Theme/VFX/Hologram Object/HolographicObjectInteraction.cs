using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicComponentInteraction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera viewerCamera;
        [SerializeField] private HolographicComponentHUD componentHUD;

        [Header("Raycast")]
        [SerializeField] private LayerMask componentLayer;
        [SerializeField] private float rayDistance = 100f;

        private HolographicComponentVisual hoveredVisual;
        private HolographicComponentVisual selectedVisual;

        // FIX:
        // This field was missing.
        private HolographicComponentData selectedData;

        public HolographicComponentData SelectedData =>
            selectedData;

#if ENABLE_INPUT_SYSTEM

        private void Update()
        {
            if (Mouse.current == null)
                return;

            if (viewerCamera == null)
                return;

            UpdateHover();
            UpdateSelection();
        }

        private void UpdateHover()
        {
            if (IsPointerOverUI())
            {
                SetHovered(null);
                return;
            }

            Vector2 mousePosition =
                Mouse.current.position.ReadValue();

            if (!TryRaycastComponent(
                    mousePosition,
                    out HolographicComponentVisual visual,
                    out _))
            {
                SetHovered(null);
                return;
            }

            SetHovered(visual);
        }

        private void UpdateSelection()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return;

            if (IsPointerOverUI())
                return;

            Vector2 mousePosition =
                Mouse.current.position.ReadValue();

            if (TryRaycastComponent(
                    mousePosition,
                    out HolographicComponentVisual visual,
                    out HolographicComponentData data))
            {
                Select(visual, data);
            }
            else
            {
                ClearSelection();
            }
        }

        private bool TryRaycastComponent(
            Vector2 mousePosition,
            out HolographicComponentVisual visual,
            out HolographicComponentData data)
        {
            visual = null;
            data = null;

            Ray ray =
                viewerCamera.ScreenPointToRay(mousePosition);

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    rayDistance,
                    componentLayer,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            visual =
                hit.collider.GetComponentInParent<
                    HolographicComponentVisual>();

            data =
                hit.collider.GetComponentInParent<
                    HolographicComponentData>();

            return visual != null &&
                   data != null;
        }

        private void SetHovered(
            HolographicComponentVisual visual)
        {
            if (hoveredVisual == visual)
                return;

            if (hoveredVisual != null &&
                hoveredVisual != selectedVisual)
            {
                hoveredVisual.SetHover(false);
            }

            hoveredVisual = visual;

            if (hoveredVisual != null &&
                hoveredVisual != selectedVisual)
            {
                hoveredVisual.SetHover(true);
            }
        }

        private void Select(
            HolographicComponentVisual visual,
            HolographicComponentData data)
        {
            if (selectedVisual != null)
            {
                selectedVisual.SetSelected(false);
            }

            if (hoveredVisual != null &&
                hoveredVisual != visual)
            {
                hoveredVisual.SetHover(false);
            }

            selectedVisual = visual;
            selectedData = data;

            if (selectedVisual != null)
            {
                selectedVisual.SetSelected(true);
            }

            if (componentHUD != null)
            {
                componentHUD.Show(selectedData);
            }
        }

        private void ClearSelection()
        {
            if (selectedVisual != null)
            {
                selectedVisual.SetSelected(false);
            }

            selectedVisual = null;
            selectedData = null;

            if (componentHUD != null)
            {
                componentHUD.Clear();
            }
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
                return false;

            return EventSystem.current.IsPointerOverGameObject();
        }

#endif
    }
}