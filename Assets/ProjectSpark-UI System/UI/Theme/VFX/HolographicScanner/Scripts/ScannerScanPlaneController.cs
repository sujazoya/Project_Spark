using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerScanPlaneController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Transform scanPlane;

        [Header("Shader")]
        [SerializeField] private string scanPositionProperty = "_ScanPosition";
        [SerializeField] private string scanIntensityProperty = "_ScanIntensity";
        [SerializeField] private string scanAlphaProperty = "_ScanAlpha";

        [Header("Movement")]
        [SerializeField, Min(0.01f)] private float scanSpeed = 0.6f;

        [Header("Intensity")]
        [SerializeField] private float inactiveIntensity = 0f;
        [SerializeField] private float activeIntensity = 5f;

        [Header("Alpha")]
        [SerializeField] private float inactiveAlpha = 0f;
        [SerializeField] private float activeAlpha = 0.35f;

        private MaterialPropertyBlock propertyBlock;

        private int scanPositionId;
        private int scanIntensityId;
        private int scanAlphaId;

        private float position;
        private bool scanning;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();

            scanPositionId =
                Shader.PropertyToID(scanPositionProperty);

            scanIntensityId =
                Shader.PropertyToID(scanIntensityProperty);

            scanAlphaId =
                Shader.PropertyToID(scanAlphaProperty);

            SetScanPosition(0f);

            SetActiveState(false);
        }

        private void Update()
        {
            if (!scanning)
                return;

            position += scanSpeed * Time.deltaTime;

            if (position > 1f)
                position = 0f;

            SetScanPosition(position);
        }

        public void StartScan()
        {
            position = 0f;
            scanning = true;

            SetActiveState(true);
            SetScanPosition(position);
        }

        public void StopScan()
        {
            scanning = false;
            SetActiveState(false);
        }

        public void SetProgress(float normalized)
        {
            position = Mathf.Clamp01(normalized);
            SetScanPosition(position);
        }

        private void SetScanPosition(float value)
        {
            if (targetRenderer == null)
                return;

            targetRenderer.GetPropertyBlock(propertyBlock);

            propertyBlock.SetFloat(
                scanPositionId,
                Mathf.Clamp01(value));

            targetRenderer.SetPropertyBlock(propertyBlock);
        }

        private void SetActiveState(bool active)
        {
            if (targetRenderer == null)
                return;

            targetRenderer.GetPropertyBlock(propertyBlock);

            propertyBlock.SetFloat(
                scanIntensityId,
                active
                    ? activeIntensity
                    : inactiveIntensity);

            propertyBlock.SetFloat(
                scanAlphaId,
                active
                    ? activeAlpha
                    : inactiveAlpha);

            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}