using UnityEngine;
using UnityEngine.Rendering;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerCircuitFlowTarget : MonoBehaviour
    {
        private static readonly int FlowActiveID =
            Shader.PropertyToID("_FlowActive");

        private static readonly int FlowDirectionID =
            Shader.PropertyToID("_FlowDirection");

        private static readonly int FlowIntensityID =
            Shader.PropertyToID("_FlowIntensity");

        private SignalPathMesh sourceMesh;
        private MeshRenderer sourceRenderer;

        private MeshFilter overlayFilter;
        private MeshRenderer overlayRenderer;

        private MaterialPropertyBlock propertyBlock;

        public SignalPathMesh SourceMesh =>
            sourceMesh;

        public bool IsActive =>
            overlayRenderer != null &&
            overlayRenderer.enabled;

        public void Initialize(
            SignalPathMesh mesh,
            Material flowMaterial)
        {
            sourceMesh = mesh;

            if (sourceMesh == null)
                return;

            if (flowMaterial == null)
                return;

            sourceRenderer =
                sourceMesh.MeshRenderer;

            if (sourceRenderer == null)
                return;

            propertyBlock =
                new MaterialPropertyBlock();

            CreateOverlay(flowMaterial);
        }

        private void CreateOverlay(
            Material flowMaterial)
        {
            GameObject overlay =
                new GameObject(
                    "__ScannerFlowOverlay");

            overlay.transform.SetParent(
                sourceRenderer.transform,
                false);

            overlay.transform.localPosition =
                Vector3.zero;

            overlay.transform.localRotation =
                Quaternion.identity;

            overlay.transform.localScale =
                Vector3.one;

            overlay.layer =
                sourceRenderer.gameObject.layer;

            overlayFilter =
                overlay.AddComponent<MeshFilter>();

            overlayFilter.sharedMesh =
                sourceMesh.GeneratedMesh;

            overlayRenderer =
                overlay.AddComponent<MeshRenderer>();

            overlayRenderer.sharedMaterial =
                flowMaterial;

            overlayRenderer.shadowCastingMode =
                ShadowCastingMode.Off;

            overlayRenderer.receiveShadows =
                false;

            overlayRenderer.motionVectorGenerationMode =
                MotionVectorGenerationMode.ForceNoMotion;

            overlayRenderer.enabled =
                false;
        }

        public void SetFlow(
            bool active,
            float direction,
            float intensity)
        {
            if (overlayRenderer == null)
                return;

            direction =
                Mathf.Clamp(
                    direction,
                    -1f,
                    1f);

            intensity =
                Mathf.Max(
                    0f,
                    intensity);

            overlayRenderer.GetPropertyBlock(
                propertyBlock);

            propertyBlock.SetFloat(
                FlowActiveID,
                active ? 1f : 0f);

            propertyBlock.SetFloat(
                FlowDirectionID,
                direction);

            propertyBlock.SetFloat(
                FlowIntensityID,
                intensity);

            overlayRenderer.SetPropertyBlock(
                propertyBlock);

            overlayRenderer.enabled =
                active && intensity > 0f;
        }

        public void StopFlow()
        {
            if (overlayRenderer == null)
                return;

            overlayRenderer.GetPropertyBlock(
                propertyBlock);

            propertyBlock.SetFloat(
                FlowActiveID,
                0f);

            overlayRenderer.SetPropertyBlock(
                propertyBlock);

            overlayRenderer.enabled =
                false;
        }
    }
}