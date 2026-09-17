using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.HolographicViewer
{
    /// <summary>
    /// Handles component hover and selection for the Project Spark holographic
    /// viewer.
    ///
    /// Responsibilities:
    /// - Acquire the viewer camera.
    /// - Raycast interactive component geometry.
    /// - Manage hover state.
    /// - Manage selection state.
    /// - Drive inspection presentation.
    ///
    /// This class does not own object rotation, camera movement, measurement
    /// solving, or electrical state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HolographicComponentInteraction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera viewerCamera;
        [SerializeField] private HolographicComponentHUD componentHUD;
        [SerializeField] private HolographicMeasurementController measurement;
        [SerializeField] private HolographicComponentCallout callout;
        [SerializeField] private HolographicSectionController sectionController;

        [Header("Components")]
        [Tooltip(
            "Interactive holographic components. " +
            "When empty, children are automatically collected.")]
        [SerializeField]
        private HolographicComponentVisual[] components;

        [Header("Raycast")]
        [Tooltip(
            "Layer mask used for component interaction. " +
            "For initial testing, use Everything.")]
        [SerializeField]
        private LayerMask componentLayer = ~0;

        [SerializeField, Min(0.1f)]
        private float rayDistance = 100f;

        [SerializeField]
        private QueryTriggerInteraction triggerInteraction =
            QueryTriggerInteraction.Collide;

        [Header("Selection")]
        [SerializeField, Range(0f, 1f)]
        private float nonSelectedDim = 0.18f;

        [Header("Input")]
        [Tooltip(
            "Disable this during initial testing if a fullscreen UI is " +
            "blocking world interaction.")]
        [SerializeField]
        private bool blockWorldInteractionOverUI = false;

        private HolographicComponentVisual hoveredVisual;
        private HolographicComponentVisual selectedVisual;
        private HolographicComponentData selectedData;

        public HolographicComponentData SelectedData =>
            selectedData;

        public HolographicComponentVisual HoveredVisual =>
            hoveredVisual;

        public HolographicComponentVisual SelectedVisual =>
            selectedVisual;

#if ENABLE_INPUT_SYSTEM

        private void Reset()
        {
            viewerCamera = Camera.main;

            components =
                GetComponentsInChildren<
                    HolographicComponentVisual>(
                    true);
        }

        private void Awake()
        {
            ResolveDependencies();

            if (components == null ||
                components.Length == 0)
            {
                components =
                    GetComponentsInChildren<
                        HolographicComponentVisual>(
                        true);
            }

            if (componentLayer.value == 0)
            {
                Debug.LogWarning(
                    $"{nameof(HolographicComponentInteraction)} on " +
                    $"'{name}' has an empty component layer mask. " +
                    "Temporarily using Everything.",
                    this);

                componentLayer = ~0;
            }

            if (viewerCamera == null)
            {
                Debug.LogError(
                    $"{nameof(HolographicComponentInteraction)} on " +
                    $"'{name}' has no viewer camera.",
                    this);
            }

            if (components == null ||
                components.Length == 0)
            {
                Debug.LogWarning(
                    $"{nameof(HolographicComponentInteraction)} on " +
                    $"'{name}' found no " +
                    $"{nameof(HolographicComponentVisual)} components.",
                    this);
            }
        }

        private void OnEnable()
        {
            ResolveDependencies();
        }

        private void Update()
        {
            if (Mouse.current == null)
                return;

            if (viewerCamera == null)
            {
                ResolveDependencies();

                if (viewerCamera == null)
                    return;
            }

            if (measurement != null &&
                measurement.IsActive)
            {
                SetHovered(null);
                return;
            }

            UpdateHover();
            UpdateSelection();
        }

        private void UpdateHover()
        {
            if (blockWorldInteractionOverUI &&
                IsPointerOverUI())
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

            if (blockWorldInteractionOverUI &&
                IsPointerOverUI())
            {
                return;
            }

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

            if (viewerCamera == null)
                return false;

            Ray ray =
                viewerCamera.ScreenPointToRay(mousePosition);

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    rayDistance,
                    componentLayer,
                    triggerInteraction))
            {
                return false;
            }

            visual =
                hit.collider.GetComponentInParent<
                    HolographicComponentVisual>();

            if (visual == null)
            {
                return false;
            }

            data =
                visual.GetComponentInParent<
                    HolographicComponentData>();

            return true;
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
                if (selectedData != null)
                    componentHUD.Show(selectedData);
                else
                    componentHUD.Clear();
            }

            ApplyIsolation();

            if (callout != null &&
                selectedData != null)
            {
                callout.Show(
                    selectedData,
                    selectedData.CalloutAnchor);
            }
        }

        public void ClearSelection()
        {
            if (components != null)
            {
                for (int i = 0;
                     i < components.Length;
                     i++)
                {
                    HolographicComponentVisual component =
                        components[i];

                    if (component == null)
                        continue;

                    component.SetIsolation(false);
                    component.SetDimAmount(0f);
                    component.SetSelected(false);
                    component.SetHover(false);
                }
            }

            selectedVisual = null;
            selectedData = null;
            hoveredVisual = null;

            if (componentHUD != null)
                componentHUD.Clear();

            if (callout != null)
                callout.Hide();
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
                        : nonSelectedDim);
            }
        }

        private void ResolveDependencies()
        {
            if (viewerCamera == null)
                viewerCamera = Camera.main;

            if (viewerCamera == null)
            {
                viewerCamera =
                    GetComponentInParent<Camera>();
            }
        }

        private static bool IsPointerOverUI()
        {
            EventSystem eventSystem =
                EventSystem.current;

            if (eventSystem == null)
                return false;

            return eventSystem.IsPointerOverGameObject();
        }

#endif
    }
}