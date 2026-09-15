using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicComponentInteraction : MonoBehaviour
    {
        [SerializeField]
        private HolographicComponentVisual[] components;
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

            [SerializeField]
        private float nonSelectedDim = 0.18f;
        [SerializeField]
private HolographicMeasurementController measurement;

        [SerializeField]
private HolographicComponentCallout callout;

[SerializeField]
private HolographicSectionController sectionController;


#if ENABLE_INPUT_SYSTEM

        private void Awake()
        {
            if (components == null ||
                components.Length == 0)
            {
                components =
                    FindComponents();
            }
        }
        private HolographicComponentVisual[]
    FindComponents()
        {
            return FindObjectsByType<
                HolographicComponentVisual>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );
        }
        private void HandleSectionInput()
        {
            if (!Mouse.current.leftButton.isPressed)
                return;
        }
        private void ApplyIsolation()
        {
            if (components == null)
                return;

            for (int i = 0;
                i < components.Length;
                i++)
            {
                HolographicComponentVisual component =
                    components[i];

                if (component == null)
                    continue;

                bool isSelected =
                    component == selectedVisual;

                component.SetIsolation(true);

                component.SetDimAmount(
                    isSelected
                        ? 0f
                        : nonSelectedDim
                );
            }
        }

        private void Update()
        {
                    if (measurement != null &&
            measurement.IsActive)
        {
            SetHovered(null);
            return;
        }
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
                        if (measurement != null &&
                measurement.IsActive)
            {
                return;
            }
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
                selectedVisual.SetIsolation(false);
                selectedVisual.SetDimAmount(0f);
            }

            selectedVisual = visual;
            selectedData = data;

            if (selectedVisual != null)
            {
                selectedVisual.SetSelected(true);

                selectedVisual.SetIsolation(true);
                selectedVisual.SetDimAmount(0f);
            }

            if (componentHUD != null)
            {
                componentHUD.Show(selectedData);
            }
            ApplyIsolation();
            if (callout != null)
            {
                callout.Show(
                    selectedData,
                    selectedData.CalloutAnchor
                );
            }
        }

        private void ClearSelection()
        {
            if (components != null)
            {
                for (int i = 0;
                    i < components.Length;
                    i++)
                {
                    if (components[i] == null)
                        continue;

                    components[i].SetIsolation(false);
                    components[i].SetDimAmount(0f);
                    components[i].SetSelected(false);
                }
            }

            selectedVisual = null;
            selectedData = null;

            if (componentHUD != null)
            {
                componentHUD.Clear();
            }
            if (callout != null)
{
            callout.Hide();
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