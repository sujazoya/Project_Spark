using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicSectionController : MonoBehaviour
    {
        private static readonly int SectionEnabledID =
            Shader.PropertyToID("_SectionEnabled");

        private static readonly int SectionPositionID =
            Shader.PropertyToID("_SectionPosition");

        [Header("Renderers")]
        [SerializeField] private Renderer[] renderers;

        [Header("Section")]
        [SerializeField] private float minimum = -1.5f;
        [SerializeField] private float maximum = 1.5f;

        [SerializeField] private float position;

        private MaterialPropertyBlock propertyBlock;

        private bool enabledState;

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

        public void SetEnabled(bool value)
        {
            enabledState = value;
            Apply();
        }

        public void SetPosition(float value)
        {
            position =
                Mathf.Clamp(
                    value,
                    minimum,
                    maximum
                );

            Apply();
        }

        public void Move(float delta)
        {
            SetPosition(
                position + delta
            );
        }

        public void ResetSection()
        {
            position =
                Mathf.Lerp(
                    minimum,
                    maximum,
                    0.5f
                );

            Apply();
        }

        public float GetNormalizedPosition()
        {
            if (Mathf.Approximately(
                    minimum,
                    maximum))
            {
                return 0f;
            }

            return Mathf.InverseLerp(
                minimum,
                maximum,
                position
            );
        }

        public float GetPosition()
        {
            return position;
        }

        private void Apply()
        {
            if (renderers == null)
                return;

            float enabled =
                enabledState ? 1f : 0f;

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
                    SectionEnabledID,
                    enabled
                );

                propertyBlock.SetFloat(
                    SectionPositionID,
                    position
                );

                renderer.SetPropertyBlock(
                    propertyBlock
                );
            }
        }
    }
}