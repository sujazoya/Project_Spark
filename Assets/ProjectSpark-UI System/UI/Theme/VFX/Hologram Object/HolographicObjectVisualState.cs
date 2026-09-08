using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicObjectVisualState : MonoBehaviour
    {
        private static readonly int HoverAmountID =
            Shader.PropertyToID("_HoverAmount");

        private static readonly int InspectionModeID =
            Shader.PropertyToID("_InspectionMode");

        [Header("Renderers")]
        [SerializeField] private Renderer[] renderers;

        [Header("Internal Parts")]
        [SerializeField] private GameObject internalParts;

        [Header("Wireframe")]
        [SerializeField] private GameObject wireframeOverlay;

        private MaterialPropertyBlock propertyBlock;
        [SerializeField] private HolographicInspectionHUD inspectionHUD;

        private static readonly int IsolationAmountID =
        Shader.PropertyToID("_IsolationAmount");
        [SerializeField]
        private HolographicSectionController sectionController;

        private static readonly int ComponentDimAmountID =
        Shader.PropertyToID("_ComponentDimAmount");
        [SerializeField] private float isolationAmount = 0f;
        [SerializeField] private float componentDimAmount = 0f;
        private HolographicExplodedView explodedView;

        public void SetIsolation(bool enabled)
        {
            isolationAmount =
                enabled ? 1f : 0f;

            Apply();
        }

        private float hoverAmount;
        private int inspectionMode;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();

            if (renderers == null || renderers.Length == 0)
            {
                renderers =
                    GetComponentsInChildren<Renderer>(
                        true
                    );
            }

            Apply();

            if (internalParts != null)
                internalParts.SetActive(false);

            if (wireframeOverlay != null)
                wireframeOverlay.SetActive(false);

                sectionController.ResetSection();
        }

        public void SetHover(float value)
        {
            hoverAmount = Mathf.Clamp01(value);
            Apply();
        }

       public void SetMode(int mode)
{
    inspectionMode =
        Mathf.Clamp(mode, 0, 4);

    HolographicInspectionMode currentMode =
        (HolographicInspectionMode)inspectionMode;

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

    Apply();

    if (inspectionHUD != null)
    {
        inspectionHUD.SetMode(
            currentMode
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

        public int GetMode()
        {
            return inspectionMode;
        }

            private void Apply()
        {
            if (renderers == null)
                return;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];

                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(
                    propertyBlock
                );

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

                renderer.SetPropertyBlock(
                    propertyBlock
                );
            }
        }
    }
}