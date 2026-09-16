using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    /// <summary>
    /// Controls the visual shader state of a holographic object.
    ///
    /// Handles:
    /// - Hover amount
    /// - Inspection mode
    /// - Isolation
    /// - Component dimming
    /// - Internal-part visibility
    /// - Wireframe visibility
    /// - Exploded-view state
    /// - Section mode
    /// </summary>
    public sealed class HolographicObjectVisualState : MonoBehaviour
    {
        private static readonly int HoverAmountID =
            Shader.PropertyToID("_HoverAmount");

        private static readonly int InspectionModeID =
            Shader.PropertyToID("_InspectionMode");

        private static readonly int IsolationAmountID =
            Shader.PropertyToID("_IsolationAmount");

        private static readonly int ComponentDimAmountID =
            Shader.PropertyToID("_ComponentDimAmount");

        [Header("Renderers")]
        [SerializeField]
        private Renderer[] renderers;

        [Header("Internal Parts")]
        [SerializeField]
        private GameObject internalParts;

        [Header("Wireframe")]
        [SerializeField]
        private GameObject wireframeOverlay;

        [Header("Inspection")]
        [SerializeField]
        private HolographicInspectionHUD inspectionHUD;

        [SerializeField]
        private HolographicSectionController sectionController;

        [SerializeField]
        private HolographicExplodedView explodedView;

        [Header("Visual State")]
        [SerializeField]
        [Range(0f, 1f)]
        private float isolationAmount;

        [SerializeField]
        [Range(0f, 1f)]
        private float componentDimAmount;

        private MaterialPropertyBlock propertyBlock;

        private float hoverAmount;
        private int inspectionMode;

        private void Awake()
        {
            EnsurePropertyBlock();
            CacheRenderers();

            InitializeVisualObjects();
            InitializeSection();

            Apply();
        }

        /// <summary>
        /// Ensures the shared MaterialPropertyBlock exists.
        /// </summary>
        private void EnsurePropertyBlock()
        {
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }
        }

        /// <summary>
        /// Finds child renderers automatically when none are assigned.
        /// </summary>
        private void CacheRenderers()
        {
            if (renderers != null && renderers.Length > 0)
            {
                return;
            }

            renderers = GetComponentsInChildren<Renderer>(true);
        }

        /// <summary>
        /// Initializes visual-only child objects.
        /// </summary>
        private void InitializeVisualObjects()
        {
            if (internalParts != null)
            {
                internalParts.SetActive(false);
            }

            if (wireframeOverlay != null)
            {
                wireframeOverlay.SetActive(false);
            }
        }

        /// <summary>
        /// Resets the section system when available.
        /// </summary>
        private void InitializeSection()
        {
            if (sectionController != null)
            {
                sectionController.ResetSection();
            }
        }

        /// <summary>
        /// Sets the hover shader amount.
        /// </summary>
        public void SetHover(float value)
        {
            hoverAmount = Mathf.Clamp01(value);
            Apply();
        }

        /// <summary>
        /// Enables or disables object isolation.
        /// </summary>
        public void SetIsolation(bool enabled)
        {
            isolationAmount = enabled ? 1f : 0f;
            Apply();
        }

        /// <summary>
        /// Sets the component dimming amount.
        /// </summary>
        public void SetComponentDimAmount(float value)
        {
            componentDimAmount = Mathf.Clamp01(value);
            Apply();
        }

        /// <summary>
        /// Sets the holographic inspection mode.
        /// </summary>
        public void SetMode(int mode)
        {
            inspectionMode = Mathf.Clamp(mode, 0, 4);

            HolographicInspectionMode currentMode =
                (HolographicInspectionMode)inspectionMode;

            UpdateModeVisuals(currentMode);
            Apply();

            if (inspectionHUD != null)
            {
                inspectionHUD.SetMode(currentMode);
            }
        }

        /// <summary>
        /// Updates child visual objects according to the inspection mode.
        /// </summary>
        private void UpdateModeVisuals(
            HolographicInspectionMode currentMode)
        {
            if (internalParts != null)
            {
                internalParts.SetActive(
                    currentMode ==
                    HolographicInspectionMode.Internal
                );
            }

            if (wireframeOverlay != null)
            {
                wireframeOverlay.SetActive(
                    currentMode ==
                    HolographicInspectionMode.Wireframe
                );
            }

            if (explodedView != null)
            {
                explodedView.SetExploded(
                    currentMode ==
                    HolographicInspectionMode.Exploded
                );
            }

            if (sectionController != null)
            {
                sectionController.SetEnabled(
                    currentMode ==
                    HolographicInspectionMode.Section
                );
            }
        }

        /// <summary>
        /// Returns the current inspection mode as an integer.
        /// </summary>
        public int GetMode()
        {
            return inspectionMode;
        }

        /// <summary>
        /// Applies the current visual state to all renderers.
        /// </summary>
        private void Apply()
        {
            EnsurePropertyBlock();

            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];

                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(propertyBlock);

                propertyBlock.SetFloat(
                    HoverAmountID,
                    hoverAmount
                );

                propertyBlock.SetFloat(
                    InspectionModeID,
                    inspectionMode
                );

                propertyBlock.SetFloat(
                    IsolationAmountID,
                    isolationAmount
                );

                propertyBlock.SetFloat(
                    ComponentDimAmountID,
                    componentDimAmount
                );

                renderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
}