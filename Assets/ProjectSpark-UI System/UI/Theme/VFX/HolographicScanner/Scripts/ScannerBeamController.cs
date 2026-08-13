using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerBeamController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform beamTransform;
        [SerializeField] private Renderer beamRenderer;

        [Header("Material Properties")]
        [SerializeField] private string intensityProperty = "_BeamIntensity";
        [SerializeField] private string alphaProperty = "_BeamAlpha";

        [Header("Runtime")]
        [SerializeField, Min(0f)] private float idleIntensity = 0.0f;
        [SerializeField, Min(0f)] private float activeIntensity = 2.5f;
        [SerializeField, Min(0f)] private float idleAlpha = 0.0f;
        [SerializeField, Min(0f)] private float activeAlpha = 0.16f;

        private MaterialPropertyBlock propertyBlock;

        private int intensityId;
        private int alphaId;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();

            intensityId = Shader.PropertyToID(intensityProperty);
            alphaId = Shader.PropertyToID(alphaProperty);

            SetBeam(0f);
        }

        public void SetBeam(float normalized)
        {
            normalized = Mathf.Clamp01(normalized);

            float intensity = Mathf.Lerp(
                idleIntensity,
                activeIntensity,
                normalized);

            float alpha = Mathf.Lerp(
                idleAlpha,
                activeAlpha,
                normalized);

            SetMaterialValues(intensity, alpha);

            if (beamTransform != null)
            {
                float scale = Mathf.Lerp(
                    0.85f,
                    1.0f,
                    normalized);

                beamTransform.localScale = new Vector3(
                    scale,
                    1f,
                    scale);
            }
        }

        public void Activate()
        {
            SetBeam(1f);
        }

        public void Deactivate()
        {
            SetBeam(0f);
        }

        private void SetMaterialValues(
            float intensity,
            float alpha)
        {
            if (beamRenderer == null)
                return;

            beamRenderer.GetPropertyBlock(propertyBlock);

            propertyBlock.SetFloat(
                intensityId,
                intensity);

            propertyBlock.SetFloat(
                alphaId,
                alpha);

            beamRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}