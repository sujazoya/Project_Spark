using UnityEngine;
using UnityEngine.Rendering;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerCircuitVoltageTarget
        : MonoBehaviour
    {
        private static readonly int VoltageNormalizedID =
            Shader.PropertyToID("_VoltageNormalized");

        private static readonly int VoltageActiveID =
            Shader.PropertyToID("_VoltageActive");

        private SignalPathMesh sourceMesh;

        private MeshFilter overlayFilter;
        private MeshRenderer overlayRenderer;

        private MaterialPropertyBlock propertyBlock;

        public SignalPathMesh SourceMesh =>
            sourceMesh;

        public void Initialize(
            SignalPathMesh mesh,
            Material voltageMaterial)
        {
            sourceMesh = mesh;

            if (sourceMesh == null)
                return;

            if (voltageMaterial == null)
                return;

            MeshRenderer sourceRenderer =
                sourceMesh.MeshRenderer;

            if (sourceRenderer == null)
                return;

            propertyBlock =
                new MaterialPropertyBlock();

            GameObject overlay =
                new GameObject(
                    "__ScannerVoltageOverlay");

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
                voltageMaterial;

            overlayRenderer.shadowCastingMode =
                ShadowCastingMode.Off;

            overlayRenderer.receiveShadows =
                false;

            overlayRenderer.motionVectorGenerationMode =
                MotionVectorGenerationMode.ForceNoMotion;

            overlayRenderer.enabled =
                false;
        }

        public void SetVoltage(
            float normalizedVoltage)
        {
            if (overlayRenderer == null)
                return;

            normalizedVoltage =
                Mathf.Clamp01(
                    normalizedVoltage);

            overlayRenderer.GetPropertyBlock(
                propertyBlock);

            propertyBlock.SetFloat(
                VoltageNormalizedID,
                normalizedVoltage);

            propertyBlock.SetFloat(
                VoltageActiveID,
                normalizedVoltage > 0.001f
                    ? 1f
                    : 0f);

            overlayRenderer.SetPropertyBlock(
                propertyBlock);

            overlayRenderer.enabled =
                normalizedVoltage > 0.001f;
        }

        public void ClearVoltage()
        {
            if (overlayRenderer == null)
                return;

            overlayRenderer.GetPropertyBlock(
                propertyBlock);

            propertyBlock.SetFloat(
                VoltageNormalizedID,
                0f);

            propertyBlock.SetFloat(
                VoltageActiveID,
                0f);

            overlayRenderer.SetPropertyBlock(
                propertyBlock);

            overlayRenderer.enabled =
                false;
        }
    }
}