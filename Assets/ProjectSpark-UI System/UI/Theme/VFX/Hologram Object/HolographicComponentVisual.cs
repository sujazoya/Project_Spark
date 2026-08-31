using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicComponentVisual : MonoBehaviour
    {
        private static readonly int SelectedAmountID =
            Shader.PropertyToID("_SelectedAmount");

        private static readonly int ComponentHoverID =
            Shader.PropertyToID("_ComponentHover");

        [Header("Renderers")]
        [SerializeField] private Renderer[] renderers;

        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();

            if (renderers == null || renderers.Length == 0)
            {
                renderers =
                    GetComponentsInChildren<Renderer>(true);
            }

            SetHover(false);
            SetSelected(false);
        }

        public void SetHover(bool value)
        {
            float amount = value ? 1f : 0f;

            ApplyValue(
                ComponentHoverID,
                amount
            );
        }

        public void SetSelected(bool value)
        {
            float amount = value ? 1f : 0f;

            ApplyValue(
                SelectedAmountID,
                amount
            );
        }

        private void ApplyValue(
            int propertyID,
            float value)
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
                    propertyID,
                    value
                );

                renderer.SetPropertyBlock(
                    propertyBlock
                );
            }
        }
    }
}