using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicComponentVisual : MonoBehaviour
    {
        private static readonly int SelectedAmountID =
            Shader.PropertyToID("_SelectedAmount");

        private static readonly int ComponentHoverID =
            Shader.PropertyToID("_ComponentHover");

        private static readonly int IsolationAmountID =
            Shader.PropertyToID("_IsolationAmount");

        private static readonly int ComponentDimAmountID =
            Shader.PropertyToID("_ComponentDimAmount");

        [Header("Renderers")]
        [SerializeField] private Renderer[] renderers;

        private MaterialPropertyBlock propertyBlock;

        private float hoverAmount;
        private float selectedAmount;
        private float isolationAmount;
        private float dimAmount;

        private void Awake()
        {
            propertyBlock =
                new MaterialPropertyBlock();

            if (renderers == null ||
                renderers.Length == 0)
            {
                renderers =
                    GetComponentsInChildren<
                        Renderer>(true);
            }

            Apply();
        }

        public void SetHover(bool value)
        {
            hoverAmount =
                value ? 1f : 0f;

            Apply();
        }

        public void SetSelected(bool value)
        {
            selectedAmount =
                value ? 1f : 0f;

            Apply();
        }

        public void SetIsolation(
            bool enabled)
        {
            isolationAmount =
                enabled ? 1f : 0f;

            Apply();
        }

        public void SetDimAmount(
            float value)
        {
            dimAmount =
                Mathf.Clamp01(value);

            Apply();
        }

        private void Apply()
        {
            if (renderers == null)
                return;

            for (int i = 0;
                 i < renderers.Length;
                 i++)
            {
                Renderer renderer =
                    renderers[i];

                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(
                    propertyBlock
                );

                propertyBlock.SetFloat(
                    SelectedAmountID,
                    selectedAmount
                );

                propertyBlock.SetFloat(
                    ComponentHoverID,
                    hoverAmount
                );

                propertyBlock.SetFloat(
                    IsolationAmountID,
                    isolationAmount
                );

                propertyBlock.SetFloat(
                    ComponentDimAmountID,
                    dimAmount
                );

                renderer.SetPropertyBlock(
                    propertyBlock
                );
            }
        }
    }
}