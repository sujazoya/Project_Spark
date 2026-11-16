using UnityEngine;
using ProjectSpark.Gameplay;

namespace AAAUI.VFX
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SignalPathMesh))]
    public sealed class SignalWireElectricalGlow : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float voltageThreshold = 0.05f;
        [SerializeField, Min(0.001f)] private float fullGlowVoltage = 5f;
        [SerializeField, Min(0f)] private float maximumIntensity = 1.5f;
        [SerializeField, Min(0f)] private float flowSpeed = 1f;
        [SerializeField, Min(0.01f)] private float fadeSpeed = 6f;

                [Header("Current Animation")]
        [SerializeField, Min(0.000001f)]
        private float fullIntensityCurrentAmps = 0.02f;

        [SerializeField, Min(0f)]
        private float maximumCurrentIntensity = 1f;

        [SerializeField, Min(0f)]
        private float minimumCurrentFlowSpeed = 0.3f;

        [SerializeField, Min(0f)]
        private float maximumCurrentFlowSpeed = 2f;

        [SerializeField, Min(0f)]
        private float currentDeadZoneAmps = 0.000001f;

        private float targetCurrentIntensity;
        private float currentFlowSpeed;
        private float currentFlowDirection = 1f;

        private SignalPathMesh sourceMesh;
        private MeshFilter overlayFilter;
        private MeshRenderer overlayRenderer;
        private MaterialPropertyBlock properties;
        private float voltage;
        private float displayedIntensity;

        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int FlowSpeedId = Shader.PropertyToID("_FlowSpeed");
        private static readonly int PolarityId = Shader.PropertyToID("_Polarity");

        public void Initialize(SignalPathMesh mesh, Material overlayMaterial)
        {
            sourceMesh = mesh;
            if (sourceMesh == null || overlayMaterial == null) return;

            Transform existing = transform.Find("ElectricalGlow");
            GameObject child = existing != null ? existing.gameObject : new GameObject("ElectricalGlow");
            if (existing == null) child.transform.SetParent(transform, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;

            overlayFilter = child.GetComponent<MeshFilter>();
            if (overlayFilter == null) overlayFilter = child.AddComponent<MeshFilter>();
            overlayRenderer = child.GetComponent<MeshRenderer>();
            if (overlayRenderer == null) overlayRenderer = child.AddComponent<MeshRenderer>();
            overlayRenderer.sharedMaterial = overlayMaterial;
            overlayRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            overlayRenderer.receiveShadows = false;
            properties = new MaterialPropertyBlock();
            SyncMesh();
            Apply();
        }
        public void SetCurrent(float signedAmps)
        {
            if (float.IsNaN(signedAmps) ||
                float.IsInfinity(signedAmps))
            {
                signedAmps = 0f;
            }

            float magnitude = Mathf.Abs(signedAmps);

            if (magnitude <= currentDeadZoneAmps)
            {
                targetCurrentIntensity = 0f;
                currentFlowSpeed = 0f;
                return;
            }

            float normalized = Mathf.Clamp01(
                magnitude / fullIntensityCurrentAmps);

            targetCurrentIntensity =
                normalized * maximumCurrentIntensity;

            currentFlowSpeed = Mathf.Lerp(
                minimumCurrentFlowSpeed,
                maximumCurrentFlowSpeed,
                normalized);

            currentFlowDirection =
                signedAmps >= 0f ? 1f : -1f;
        }

        public void SetVoltage(float signedVoltage)
        {
            voltage = float.IsNaN(signedVoltage) || float.IsInfinity(signedVoltage)
                ? 0f : signedVoltage;
        }

        private void LateUpdate()
        {
            if (sourceMesh == null || overlayRenderer == null) return;
            SyncMesh();
            float target = Mathf.Abs(voltage) > voltageThreshold
                ? Mathf.Clamp01(Mathf.Abs(voltage) / fullGlowVoltage) * maximumIntensity
                : 0f;
            displayedIntensity = Mathf.MoveTowards(displayedIntensity, target, fadeSpeed * Time.deltaTime);
            Apply();
        }

        private void SyncMesh()
        {
            if (overlayFilter != null && sourceMesh != null && overlayFilter.sharedMesh != sourceMesh.GeneratedMesh)
                overlayFilter.sharedMesh = sourceMesh.GeneratedMesh;
        }

        private void Apply()
        {
            if (overlayRenderer == null) return;
            if (properties == null) properties = new MaterialPropertyBlock();
            overlayRenderer.GetPropertyBlock(properties);
            properties.SetFloat(IntensityId, displayedIntensity);
            properties.SetFloat(FlowSpeedId, displayedIntensity > 0.001f ? flowSpeed : 0f);
            properties.SetFloat(PolarityId, Mathf.Sign(voltage));
            overlayRenderer.SetPropertyBlock(properties);
        }
    }
}
